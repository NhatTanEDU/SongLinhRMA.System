using System;
using System.Collections.Generic;

namespace RMA.Shared.DTOs
{
    public class DeviceLifecycleDto
    {
        public DeviceLifecycleInfoDto DeviceInfo { get; set; } = new();
        public DeviceLifecycleSalesInfoDto SalesInfo { get; set; } = new();
        public List<RmaTicketTimelineDto> RmaTickets { get; set; } = new();
    }

    public class DeviceLifecycleInfoDto
    {
        public string SerialNumber { get; set; } = string.Empty;
        public string ModelName { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public DateTime? WarrantyExpiry { get; set; }
    }

    public class DeviceLifecycleSalesInfoDto
    {
        public string? OrderCode { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public string? CustomerName { get; set; }
    }

    public class RmaTicketTimelineDto
    {
        public string TicketId { get; set; } = string.Empty;
        public DateTime ReceivedDate { get; set; }
        public string? ServiceMode { get; set; }
        public string ProblemDescription { get; set; } = string.Empty;
        public string StatusName { get; set; } = string.Empty;
        public List<StatusHistoryTimelineDto> StatusHistories { get; set; } = new();
    }

    public class StatusHistoryTimelineDto
    {
        public DateTime UpdateTime { get; set; }
        public string? LocationName { get; set; }
        public string? Note { get; set; }
    }
}
