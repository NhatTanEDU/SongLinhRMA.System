namespace RMA.Shared.Enums
{
    public enum TicketStatus
    {
        Pending,       // Chờ tiếp nhận
        Processing,    // Đang xử lý
        Repaired,      // Đã sửa xong
        SentToVendor,  // Đã gửi hãng
        Completed,     // Đã hoàn thành (Trả khách)
        Cancelled      // Đã hủy
    }
}
