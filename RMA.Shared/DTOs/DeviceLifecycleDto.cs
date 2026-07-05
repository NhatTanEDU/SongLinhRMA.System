using System;
using System.Collections.Generic;
using RMA.Shared.Enums;

namespace RMA.Shared.DTOs
{
    public class DeviceLifecycleDto
    {
        public DeviceDto DeviceInfo { get; set; } = new();
        public string? OriginalOrderCode { get; set; }
        public DateTime? OriginalOrderDeliveryDate { get; set; }
        public List<RmaTicketLifecycleDto> RmaHistory { get; set; } = new();
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
