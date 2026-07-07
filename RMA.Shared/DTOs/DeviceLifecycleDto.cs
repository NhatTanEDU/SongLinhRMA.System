using System;
using System.Collections.Generic;
using RMA.Shared.Enums;

namespace RMA.Shared.DTOs
{
    public class DeviceLifecycleDto
    {
        // Device Info (unified to DeviceDto for compatibility with both search layouts)
        public DeviceDto DeviceInfo { get; set; } = new();

        // For GlobalSearch.razor (main branch)
        public DeviceLifecycleSalesInfoDto SalesInfo { get; set; } = new();
        public List<RmaTicketTimelineDto> RmaTickets { get; set; } = new();

        // For GlobalSearchDialog.razor (feature branch)
        public string? OriginalOrderCode { get; set; }
        public DateTime? OriginalOrderDeliveryDate { get; set; }
        public List<RmaTicketLifecycleDto> RmaHistory { get; set; } = new();
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

    public class RmaTicketLifecycleDto
    {
        public string TicketId { get; set; } = string.Empty;
        public string ProblemDescription { get; set; } = string.Empty;
        public string? ServiceMode { get; set; }
        public DateTime ReceivedDate { get; set; }
        public DateTime? SentDate { get; set; }
        public TicketStatus Status { get; set; }
        public string? CurrentLocationName { get; set; }
        public List<RmaStepDto> Steps { get; set; } = new();
    }

    public class RmaStepDto
    {
        public TicketStatus Status { get; set; }
        public string LocationName { get; set; } = string.Empty;
        public string? Note { get; set; }
        public DateTime ChangedAt { get; set; }
    }
}
