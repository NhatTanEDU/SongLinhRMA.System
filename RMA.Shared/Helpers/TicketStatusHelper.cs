using System;
using RMA.Shared.Enums;

namespace RMA.Shared.Helpers
{
    public static class TicketStatusHelper
    {
        public static string GetDisplayName(TicketStatus status)
        {
            return status switch
            {
                TicketStatus.Pending => "Chờ tiếp nhận",
                TicketStatus.Processing => "Đang xử lý",
                TicketStatus.Repaired => "Đã sửa xong",
                TicketStatus.SentToVendor => "Đã gửi hãng bảo hành",
                TicketStatus.Completed => "Đã hoàn thành (Trả khách)",
                TicketStatus.Cancelled => "Đã hủy",
                _ => "Không xác định"
            };
        }

        public static string GetColorCode(TicketStatus status)
        {
            return status switch
            {
                TicketStatus.Pending => "#FF9800",       // Orange
                TicketStatus.Processing => "#2196F3",    // Blue
                TicketStatus.Repaired => "#4CAF50",      // Green
                TicketStatus.SentToVendor => "#9C27B0",  // Purple
                TicketStatus.Completed => "#00G853",     // Bright Green (or #00c853)
                TicketStatus.Cancelled => "#F44336",     // Red
                _ => "#9E9E9E"                           // Grey
            };
        }

        public static string GetColorClass(TicketStatus status)
        {
            return status switch
            {
                TicketStatus.Pending => "warning",
                TicketStatus.Processing => "info",
                TicketStatus.Repaired => "success",
                TicketStatus.SentToVendor => "secondary",
                TicketStatus.Completed => "success",
                TicketStatus.Cancelled => "error",
                _ => "default"
            };
        }

        public static string GetIcon(TicketStatus status)
        {
            // Note: MudBlazor Icons are strings representing SVG paths. We will map them in the client,
            // but we can return names or keywords here for general usage.
            return status switch
            {
                TicketStatus.Pending => "HourglassEmpty",
                TicketStatus.Processing => "Build",
                TicketStatus.Repaired => "CheckCircleOutline",
                TicketStatus.SentToVendor => "Business",
                TicketStatus.Completed => "CheckCircle",
                TicketStatus.Cancelled => "Cancel",
                _ => "HelpOutline"
            };
        }

        public static TicketStatus ParseFromDbString(string? statusStr)
        {
            if (string.IsNullOrWhiteSpace(statusStr))
            {
                return TicketStatus.Pending;
            }

            var lower = statusStr.Trim().ToLower();

            // Check standard IDs/Names
            if (lower == "status-1" || lower == "new" || lower.Contains("pending") || lower.Contains("tiếp nhận"))
            {
                return TicketStatus.Pending;
            }
            if (lower == "status-2" || lower == "in progress" || lower.Contains("processing") || lower.Contains("sửa chữa") || lower.Contains("xử lý"))
            {
                return TicketStatus.Processing;
            }
            if (lower == "status-3" || lower == "waiting for parts" || lower.Contains("parts") || lower.Contains("vendor") || lower.Contains("gửi hãng") || lower.Contains("hãng"))
            {
                return TicketStatus.SentToVendor;
            }
            if (lower == "status-4" || lower == "repaired" || lower.Contains("sửa xong") || lower.Contains("hoàn thành sửa"))
            {
                return TicketStatus.Repaired;
            }
            if (lower == "status-5" || lower == "closed" || lower.Contains("completed") || lower.Contains("trả khách") || lower.Contains("hoàn thành"))
            {
                return TicketStatus.Completed;
            }
            if (lower.Contains("cancel") || lower.Contains("hủy"))
            {
                return TicketStatus.Cancelled;
            }

            return TicketStatus.Pending;
        }
    }
}
