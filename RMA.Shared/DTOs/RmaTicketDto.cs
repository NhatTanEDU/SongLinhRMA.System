using System;

namespace RMA.Shared.DTOs
{
    public class RmaTicketDto
    {
        public string Id { get; set; } = string.Empty;
        public string DeviceId { get; set; } = string.Empty;
        public string DeviceSerialNumber { get; set; } = string.Empty;
        public string DeviceModelName { get; set; } = string.Empty;
        
        public string CustomerId { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string? CustomerPhone { get; set; }
        public string? CustomerAvatarUrl { get; set; }
        
        public string StatusId { get; set; } = string.Empty;
        public string StatusName { get; set; } = string.Empty;
        public string? StatusColorCode { get; set; }
        
        public string? VendorId { get; set; }
        public string? VendorName { get; set; }
        
        public string ProblemDescription { get; set; } = string.Empty;
        public string? ServiceMode { get; set; }
        public DateTime ReceivedDate { get; set; }
        public DateTime? SentDate { get; set; }
        public bool IsUrgent { get; set; }
        public string? StaffNote { get; set; }
    }

    public class RmaTicketCreateDto
    {
        public string DeviceId { get; set; } = string.Empty;
        public string CustomerId { get; set; } = string.Empty;
        public string StatusId { get; set; } = string.Empty;
        public string? VendorId { get; set; }
        public string ProblemDescription { get; set; } = string.Empty;
        public string? ServiceMode { get; set; }
        public bool IsUrgent { get; set; }
        public string? StaffNote { get; set; }
    }

    public class RmaTicketUpdateStatusDto
    {
        public string StatusId { get; set; } = string.Empty;
        public string LocationId { get; set; } = string.Empty;
        public string? Note { get; set; }
    }
}
