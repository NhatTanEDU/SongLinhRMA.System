namespace RMA.Server.Services;

/// <summary>
/// Interface gửi cảnh báo qua Firebase Cloud Messaging (FCM).
/// </summary>
public interface IFcmService
{
    /// <summary>Firebase đã khởi tạo thành công và sẵn sàng gửi thông báo.</summary>
    bool IsReady { get; }

    /// <summary>
    /// Gửi Push Notification cảnh báo phiếu RMA quá hạn.
    /// </summary>
    /// <param name="ticketId">ID phiếu RMA</param>
    /// <param name="customerName">Tên khách hàng</param>
    /// <param name="reason">Lý do cảnh báo (ví dụ: "Chưa gửi đi hãng" hoặc "Gửi hãng quá 14 ngày")</param>
    Task SendAlertAsync(string ticketId, string customerName, string reason);
}
