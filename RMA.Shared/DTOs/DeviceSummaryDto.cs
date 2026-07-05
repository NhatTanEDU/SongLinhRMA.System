using System;
using RMA.Shared.Enums;

namespace RMA.Shared.DTOs
{
    public class DeviceSummaryDto
    {
        // 1. Thiết bị & Dòng máy
        public string DeviceId { get; set; } = string.Empty;
        public string SerialNumber { get; set; } = string.Empty;
        public string ModelName { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public DateTime? PurchaseDate { get; set; }
        public DateTime? WarrantyExpiry { get; set; }

        // 2. Khách hàng sở hữu
        public string CustomerId { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;

        // 3. Đơn hàng bán gốc
        public string? OrderId { get; set; }
        public string? OrderCode { get; set; }
        public DateTime? DeliveryDate { get; set; }

        // 4. Logistics & Vị trí hiện tại
        public string CurrentLocationName { get; set; } = string.Empty;
        public TicketStatus CurrentStatus { get; set; }

        // 5. Thông tin Ticket đang mở (nếu có)
        public OpenTicketSummaryDto? ActiveTicket { get; set; }

        // 6. Lịch sử các Ticket (bao gồm cả đóng và mở)
        public List<OpenTicketSummaryDto> TicketHistory { get; set; } = new();
    }

    public class OpenTicketSummaryDto
    {
        public string TicketId { get; set; } = string.Empty;
        public string TicketCode { get; set; } = string.Empty;
        public TicketStatus Status { get; set; }
        public string LocationName { get; set; } = string.Empty;
        public DateTime? SentDate { get; set; }
        public DateTime? ReceivedDate { get; set; }
    }
}
