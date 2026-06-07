using System;
using System.Collections.Generic;

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
        public string? CustomerContactPerson { get; set; }
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
        public string? EndUserName { get; set; }

        public List<AttachmentDto> Attachments { get; set; } = new();
        public List<ChecklistDto> Checklists { get; set; } = new();
        public List<StatusHistoryDto> StatusHistories { get; set; } = new();
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
        public string? EndUserName { get; set; }
    }

    public class RmaTicketUpdateStatusDto
    {
        public string StatusId { get; set; } = string.Empty;
        public string LocationId { get; set; } = string.Empty;
        public string? Note { get; set; }
    }

    public class AttachmentDto
    {
        public string Id { get; set; } = string.Empty;
        public string FileUrl { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; }
    }

    public class ChecklistDto
    {
        public string Id { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public bool IsChecked { get; set; }
    }

    public class StatusHistoryDto
    {
        public string Id { get; set; } = string.Empty;
        public string StatusName { get; set; } = string.Empty;
        public string? StatusColorCode { get; set; }
        public string LocationName { get; set; } = string.Empty;
        public string? Note { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
