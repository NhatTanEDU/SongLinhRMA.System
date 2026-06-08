using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;

namespace RMA.Server.Services;

/// <summary>
/// Gửi Push Notification qua Firebase Cloud Messaging (FCM).
/// Đăng ký dạng Singleton vì FirebaseApp chỉ cần khởi tạo một lần.
/// </summary>
public class FcmService : IFcmService
{
    private readonly string _topic;
    private readonly ILogger<FcmService> _logger;

    /// <summary>True khi Firebase đã khởi tạo thành công.</summary>
    public bool IsReady { get; private set; }

    public FcmService(IConfiguration config, ILogger<FcmService> logger)
    {
        _logger = logger;
        _topic = config["Firebase:AlertTopic"] ?? "rma-alerts";

        var keyPath = config["Firebase:ServiceAccountKeyPath"] ?? "serviceAccountKey.json";

        // Resolve đường dẫn tương đối so với thư mục chạy app
        if (!Path.IsPathRooted(keyPath))
        {
            keyPath = Path.Combine(AppContext.BaseDirectory, keyPath);
        }

        if (!File.Exists(keyPath))
        {
            _logger.LogWarning(
                "⚠️ [Firebase] Không tìm thấy file '{KeyPath}'. " +
                "FCM sẽ KHÔNG hoạt động cho đến khi bạn đặt file serviceAccountKey.json vào đúng vị trí. " +
                "Tải file từ: Firebase Console → Project Settings → Service Accounts → Generate new private key",
                keyPath);
            IsReady = false;
            return;
        }

        try
        {
            // FirebaseApp chỉ được tạo một lần trong vòng đời ứng dụng
            if (FirebaseApp.DefaultInstance == null)
            {
                FirebaseApp.Create(new AppOptions
                {
                    Credential = GoogleCredential.FromFile(keyPath)
                });
                _logger.LogInformation("✅ [Firebase] Kết nối thành công với Firebase Project.");
            }

            IsReady = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ [Firebase] Lỗi khởi tạo FirebaseApp: {Message}", ex.Message);
            IsReady = false;
        }
    }

    public async Task SendAlertAsync(string ticketId, string customerName, string reason)
    {
        // Firebase chưa được cấu hình — warning đã hiển thị 1 lần ở constructor, không lặp lại
        if (!IsReady) return;

        var message = new Message
        {
            Topic = _topic,
            Notification = new Notification
            {
                Title = $"⚠️ Cảnh báo RMA #{ticketId}",
                Body  = $"KH: {customerName} — {reason}. Vui lòng xử lý gấp!"
            },
            Data = new Dictionary<string, string>
            {
                { "ticketId",      ticketId },
                { "customerName",  customerName },
                { "reason",        reason },
                { "timestamp",     DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString() }
            },
            Android = new AndroidConfig
            {
                Priority = Priority.High,
                Notification = new AndroidNotification
                {
                    ChannelId = "rma_alerts",
                    Color     = "#FF0000"
                }
            }
        };

        try
        {
            string messageId = await FirebaseMessaging.DefaultInstance.SendAsync(message);
            _logger.LogInformation(
                "✅ [FCM] Đã gửi cảnh báo Phiếu #{TicketId} | Topic: {Topic} | MessageId: {MessageId}",
                ticketId, _topic, messageId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "❌ [FCM] Lỗi gửi cảnh báo Phiếu #{TicketId}: {Message}",
                ticketId, ex.Message);
        }
    }
}
