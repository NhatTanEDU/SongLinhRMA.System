using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Caching.Memory;
using RMA.Server.Entities;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RMA.Server.Services;

/// <summary>
/// Background Service chạy ngầm để quét cảnh báo các phiếu bảo hành gửi hãng quá hạn.
/// Sử dụng IServiceScopeFactory để tạo scope lấy Scoped FirestoreRepository một cách an toàn.
/// </summary>
public class RmaAlertBackgroundService : BackgroundService
{
    private readonly IFcmService _fcmService;
    private readonly IConfiguration _config;
    private readonly ILogger<RmaAlertBackgroundService> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IMemoryCache _cache;

    public RmaAlertBackgroundService(
        IFcmService fcmService,
        IConfiguration config,
        ILogger<RmaAlertBackgroundService> logger,
        IServiceScopeFactory scopeFactory,
        IMemoryCache cache)
    {
        _fcmService = fcmService;
        _config = config;
        _logger = logger;
        _scopeFactory = scopeFactory;
        _cache = cache;
    }

    private async Task<(double YellowDays, double RedDays)> GetSlaThresholdsAsync(IServiceProvider serviceProvider)
    {
        if (_cache.TryGetValue("sla_settings_cached", out (double yellow, double red) thresholds))
        {
            return thresholds;
        }

        double yellowDays = 10;
        double redDays = 14;

        try
        {
            var settingRepo = serviceProvider.GetRequiredService<FirestoreRepository<SystemSetting>>();
            var settings = await settingRepo.GetAllAsync();
            var yellow = settings.FirstOrDefault(s => s.Key == "SlaYellowDays");
            var red = settings.FirstOrDefault(s => s.Key == "SlaRedDays");

            if (yellow != null && double.TryParse(yellow.Value, out var yVal))
            {
                yellowDays = yVal;
            }
            if (red != null && double.TryParse(red.Value, out var rVal))
            {
                redDays = rVal;
            }

            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(10));
            _cache.Set("sla_settings_cached", (yellowDays, redDays), cacheEntryOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError("Lỗi khi tải cấu hình SLA từ database: {Msg}", ex.Message);
        }

        return (yellowDays, redDays);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("🕒 [RmaAlert] Background Service đã khởi động.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogInformation("🕒 [RmaAlert] Bắt đầu quét Firestore tìm các phiếu quá hạn...");

                using (var scope = _scopeFactory.CreateScope())
                {
                    var ticketRepo = scope.ServiceProvider.GetRequiredService<FirestoreRepository<RmaTicket>>();
                    var statusRepo = scope.ServiceProvider.GetRequiredService<FirestoreRepository<StatusMaster>>();
                    var customerRepo = scope.ServiceProvider.GetRequiredService<FirestoreRepository<Customer>>();

                    // 1. Lấy tất cả danh sách từ Firestore
                    var tickets = await ticketRepo.GetAllAsync();
                    var statuses = (await statusRepo.GetAllAsync()).ToDictionary(s => s.Id, s => s);
                    var customers = (await customerRepo.GetAllAsync()).ToDictionary(c => c.Id, c => c);

                    // 2. Lấy cấu hình SLA động từ Firestore (có caching)
                    var (yellowDays, redDays) = await GetSlaThresholdsAsync(scope.ServiceProvider);

                    foreach (var ticket in tickets)
                    {
                        bool isClosed = false;
                        if (statuses.TryGetValue(ticket.StatusId, out var status))
                        {
                            isClosed = status.StatusName.Equals("Closed", StringComparison.OrdinalIgnoreCase);
                        }

                        if (isClosed)
                        {
                            // Nếu phiếu đã Closed nhưng vẫn còn cảnh báo bị kẹt, hãy dọn dẹp
                            if (!string.IsNullOrEmpty(ticket.WarningColor) || ticket.IsUrgent)
                            {
                                ticket.WarningColor = null;
                                ticket.IsUrgent = false;
 
                                await ticketRepo.UpdateAsync(ticket.Id, ticket);
                                _logger.LogInformation("🧹 [RmaAlert] Đã làm sạch dữ liệu cảnh báo bị kẹt của phiếu Đóng #{Id}", ticket.Id);
                            }
                        }
                        else
                        {
                            // 3. Tính toán cảnh báo và mức độ khẩn qua SlaCalculator dùng cấu hình động
                            var (newWarningColor, shouldSetUrgent) = SlaCalculator.Calculate(
                                ticket.ReceivedDate, 
                                ticket.SentDate, 
                                ticket.StatusId, 
                                DateTime.UtcNow, 
                                yellowDays, 
                                redDays);
 
                            // 4. Tối ưu hóa ghi: Chỉ cập nhật nếu WarningColor hoặc IsUrgent thay đổi
                            bool isColorChanged = ticket.WarningColor != newWarningColor;
                            bool isUrgentChanged = ticket.IsUrgent != shouldSetUrgent;
 
                            if (isColorChanged || isUrgentChanged)
                            {
                                ticket.WarningColor = newWarningColor;
                                
                                // Cập nhật mức độ ưu tiên sang Khẩn (IsUrgent = true) nếu đạt mốc đỏ
                                if (shouldSetUrgent)
                                {
                                    ticket.IsUrgent = true;
                                }
 
                                await ticketRepo.UpdateAsync(ticket.Id, ticket);
                                _logger.LogInformation("💾 [RmaAlert] Đã cập nhật cảnh báo cho Phiếu #{Id}: WarningColor={Color}, IsUrgent={Urgent} (SLA: Yellow={Y}d, Red={R}d)", 
                                    ticket.Id, newWarningColor, ticket.IsUrgent, yellowDays, redDays);
 
                                // 5. Nếu chuyển sang màu Đỏ thì bắn thông báo FCM
                                if (newWarningColor == "Red" && isColorChanged)
                                {
                                    string customerName = customers.TryGetValue(ticket.CustomerId, out var cust) ? cust.Name : "Khách hàng không xác định";
                                    
                                    // Sử dụng UTC+7 (giờ Việt Nam) cho hiển thị tin nhắn FCM
                                    DateTime localRefDate = (ticket.SentDate ?? ticket.ReceivedDate).AddHours(7);
                                    string reason = $"Quá hạn {redDays} ngày (Từ ngày {localRefDate:dd/MM/yyyy HH:mm})";
 
                                    _logger.LogWarning("🚨 [RmaAlert] Phiếu RMA #{Id} đã quá hạn {RedDays} ngày! Tiến hành gửi push notification qua FCM...", ticket.Id, redDays);
                                    await _fcmService.SendAlertAsync(ticket.Id, customerName, reason);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ [RmaAlert] Gặp lỗi trong quá trình quét cảnh báo.");
            }

            // Đọc cấu hình thời gian chạy định kỳ (Hỗ trợ cấu hình giây cho testing)
            int intervalSeconds = _config.GetValue<int>("Firebase:CheckIntervalSeconds", 0);
            if (intervalSeconds <= 0)
            {
                int intervalMinutes = _config.GetValue<int>("Firebase:CheckIntervalMinutes", 60);
                intervalSeconds = intervalMinutes * 60;
            }

            _logger.LogInformation("🕒 [RmaAlert] Chờ {Seconds} giây trước khi quét tiếp...", intervalSeconds);
            await Task.Delay(TimeSpan.FromSeconds(intervalSeconds), stoppingToken);
        }
    }
}
