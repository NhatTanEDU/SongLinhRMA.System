using System;
using System.Collections.Generic;
using System.Linq;

namespace RMA.Shared.DTOs
{
    public class RmaTicketDto
    {
        public string Id { get; set; } = string.Empty;
        public string DeviceId { get; set; } = string.Empty;
        public string DeviceSerialNumber { get; set; } = string.Empty;
        public string DeviceModelName { get; set; } = string.Empty;
        public DateTime? DeviceWarrantyExpiry { get; set; }
        
        public string CustomerId { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string? CustomerPhone { get; set; }
        public string? CustomerContactPerson { get; set; }
        public string? CustomerAvatarUrl { get; set; }
        
        public string StatusId { get; set; } = string.Empty;
        public string StatusName { get; set; } = string.Empty;
        public string? StatusColorCode { get; set; }
        public string? WarningColor { get; set; }
        
        public string? VendorId { get; set; }
        public string? VendorName { get; set; }
        public string? VendorWarrantyLink { get; set; }
        
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

        public void PopulateChecklistsFromStaffNote()
        {
            if (Checklists != null && Checklists.Any())
                return;

            Checklists = new List<ChecklistDto>();

            if (string.IsNullOrEmpty(StaffNote))
                return;

            // Try to find [Phụ kiện: ...] in StaffNote
            var startTag = "[Phụ kiện:";
            var startIndex = StaffNote.IndexOf(startTag, StringComparison.OrdinalIgnoreCase);
            if (startIndex == -1)
                return;

            var contentStart = startIndex + startTag.Length;
            var endIndex = StaffNote.IndexOf("]", contentStart);
            if (endIndex == -1)
                return;

            var accsString = StaffNote.Substring(contentStart, endIndex - contentStart).Trim();
            if (string.IsNullOrEmpty(accsString) || accsString.Equals("Không có", StringComparison.OrdinalIgnoreCase))
                return;

            var accList = accsString.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            
            // Default list of accessories to show in checklist (with checked status based on parsed string)
            var defaultAccs = new[] { "Adapter/Sạc", "Dây nguồn", "Pin ngoài", "Chuột", "Bàn phím", "Bao da/Túi", "Bút cảm ứng" };
            
            foreach (var def in defaultAccs)
            {
                var isChecked = accList.Any(a => a.Contains(def, StringComparison.OrdinalIgnoreCase) || def.Contains(a, StringComparison.OrdinalIgnoreCase));
                Checklists.Add(new ChecklistDto { ItemName = def, IsChecked = isChecked });
            }

            // Add any custom accessories that are in the parsed list but not in the default list
            foreach (var acc in accList)
            {
                if (!defaultAccs.Any(def => def.Contains(acc, StringComparison.OrdinalIgnoreCase) || acc.Contains(def, StringComparison.OrdinalIgnoreCase)))
                {
                    Checklists.Add(new ChecklistDto { ItemName = acc, IsChecked = true });
                }
            }
        }
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
        public List<AttachmentDto> Attachments { get; set; } = new();
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
        public string? Base64Data { get; set; }
        public string? FileType { get; set; }
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

    public enum TicketType
    {
        BaoHanh,
        SuaChua
    }

    public class HandoverItemDto
    {
        public int STT { get; set; }
        public string DeviceName { get; set; } = string.Empty;
        public string SerialNumber { get; set; } = string.Empty;
        public int Quantity { get; set; } = 1;
        public string Unit { get; set; } = "Cái";
    }

    public class HandoverPdfRequest
    {
        public string CustomerName { get; set; } = string.Empty;
        public TicketType TicketType { get; set; }
        public List<HandoverItemDto> Items { get; set; } = new();
    }
}
