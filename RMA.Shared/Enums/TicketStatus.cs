namespace RMA.Shared.Enums
{
    public enum TicketStatus
    {
        New = 0,               // Mới tiếp nhận
        Diagnosing = 1,        // Đang chẩn đoán
        WaitingApproval = 2,   // Chờ khách duyệt báo giá
        WaitingParts = 3,      // Chờ linh kiện thay thế
        Repairing = 4,         // Đang sửa chữa
        WaitingVendor = 5,     // Đang gửi hãng bảo hành
        Completed = 6,         // Đã xử lý xong
        Closed = 7             // Đã đóng ca & Trả khách
    }
}
