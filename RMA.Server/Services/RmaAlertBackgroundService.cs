using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
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

    public RmaAlertBackgroundService(
        IFcmService fcmService,
        IConfiguration config,
        ILogger<RmaAlertBackgroundService> logger,
        IServiceScopeFactory scopeFactory)
    {
        _fcmService = fcmService;
        _config = config;
        _logger = logger;
        _scopeFactory = scopeFactory;
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
                        else if (ticket.SentDate.HasValue)
                        {
                            // 3. Tính toán số ngày chênh lệch dựa trên giờ UTC
                            double diffDays = (DateTime.UtcNow - ticket.SentDate.Value).TotalDays;

                            string newWarningColor = "Green";
                            bool shouldSetUrgent = false;

                            if (diffDays >= 14)
                            {
                                newWarningColor = "Red";
                                shouldSetUrgent = true;
                            }
                            else if (diffDays >= 10)
                            {
                                newWarningColor = "Yellow";
                            }

                            // 4. Tối ưu hóa ghi: Chỉ cập nhật nếu WarningColor hoặc IsUrgent thay đổi
                            bool isColorChanged = ticket.WarningColor != newWarningColor;
                            bool isUrgentChanged = ticket.IsUrgent != shouldSetUrgent;

                            if (isColorChanged || isUrgentChanged)
                            {
                                ticket.WarningColor = newWarningColor;
                                
                                // Cập nhật mức độ ưu tiên sang Khẩn (IsUrgent = true) nếu đạt mốc >= 14 ngày
                                if (shouldSetUrgent)
                                {
                                    ticket.IsUrgent = true;
                                }

                                await ticketRepo.UpdateAsync(ticket.Id, ticket);
                                _logger.LogInformation("💾 [RmaAlert] Đã cập nhật cảnh báo cho Phiếu #{Id}: WarningColor={Color}, IsUrgent={Urgent}", 
                                    ticket.Id, newWarningColor, ticket.IsUrgent);

                                // 5. Nếu chuyển sang màu Đỏ (>= 14 ngày) thì bắn thông báo FCM
                                if (newWarningColor == "Red" && isColorChanged)
                                {
                                    string customerName = customers.TryGetValue(ticket.CustomerId, out var cust) ? cust.Name : "Khách hàng không xác định";
                                    
                                    // Sử dụng UTC+7 (giờ Việt Nam) cho hiển thị tin nhắn FCM
                                    DateTime localSentDate = ticket.SentDate.Value.AddHours(7);
                                    string reason = $"Gửi hãng quá 14 ngày (Từ ngày {localSentDate:dd/MM/yyyy HH:mm})";

                                    _logger.LogWarning("🚨 [RmaAlert] Phiếu RMA #{Id} đã quá hạn 14 ngày! Tiến hành gửi push notification qua FCM...", ticket.Id);
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
