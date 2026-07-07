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
                TicketStatus.New => "Mới tiếp nhận",
                TicketStatus.Diagnosing => "Đang chẩn đoán",
                TicketStatus.WaitingApproval => "Chờ khách duyệt báo giá",
                TicketStatus.WaitingParts => "Chờ linh kiện thay thế",
                TicketStatus.Repairing => "Đang sửa chữa",
                TicketStatus.WaitingVendor => "Đang gửi hãng bảo hành",
                TicketStatus.Completed => "Đã xử lý xong",
                TicketStatus.Closed => "Đã đóng ca & Trả khách",
                _ => "Không xác định"
            };
        }

        public static string GetColorCode(TicketStatus status)
        {
            return status switch
            {
                TicketStatus.New => "#2196F3",             // Info/Blue
                TicketStatus.Diagnosing => "#FF9800",      // Warning/Orange
                TicketStatus.WaitingApproval => "#FFC107",  // Warning/Amber
                TicketStatus.WaitingParts => "#F44336",     // Error/Red
                TicketStatus.Repairing => "#0072BC",        // Primary/Solicom Blue
                TicketStatus.WaitingVendor => "#9C27B0",    // Secondary/Purple
                TicketStatus.Completed => "#4CAF50",        // Success/Green
                TicketStatus.Closed => "#424242",           // Dark Grey
                _ => "#9E9E9E"                              // Grey
            };
        }

        public static string GetColorClass(TicketStatus status)
        {
            return status switch
            {
                TicketStatus.New => "info",
                TicketStatus.Diagnosing => "warning",
                TicketStatus.WaitingApproval => "warning",
                TicketStatus.WaitingParts => "error",
                TicketStatus.Repairing => "primary",
                TicketStatus.WaitingVendor => "secondary",
                TicketStatus.Completed => "success",
                TicketStatus.Closed => "dark",
                _ => "default"
            };
        }

        public static string GetIcon(TicketStatus status)
        {
            return status switch
            {
                TicketStatus.New => "FiberNew",
                TicketStatus.Diagnosing => "Search",
                TicketStatus.WaitingApproval => "RateReview",
                TicketStatus.WaitingParts => "HourglassEmpty",
                TicketStatus.Repairing => "Handyman",
                TicketStatus.WaitingVendor => "LocalShipping",
                TicketStatus.Completed => "CheckCircle",
                TicketStatus.Closed => "Archive",
                _ => "HelpOutline"
            };
        }

        public static TicketStatus ParseFromDbString(string? statusStr)
        {
            if (string.IsNullOrWhiteSpace(statusStr))
            {
                return TicketStatus.New;
            }

            var lower = statusStr.Trim().ToLower();

            // Check standard IDs/Names
            if (lower == "status-1" || lower == "new" || lower.Contains("mới tiếp nhận") || lower.Contains("mới"))
            {
                return TicketStatus.New;
            }
            if (lower == "status-2" || lower == "in progress" || lower.Contains("chẩn đoán") || lower.Contains("tiến trình") || lower.Contains("đang xử lý"))
            {
                return TicketStatus.Diagnosing;
            }
            if (lower.Contains("báo giá") || lower.Contains("duyệt") || lower == "waitingapproval")
            {
                return TicketStatus.WaitingApproval;
            }
            if (lower == "status-3" || lower == "waiting for parts" || lower.Contains("parts") || lower.Contains("linh kiện") || lower.Contains("chờ linh kiện"))
            {
                return TicketStatus.WaitingParts;
            }
            if (lower == "status-4" || lower.Contains("repairing") || lower.Contains("đang sửa") || lower.Contains("sửa chữa"))
            {
                return TicketStatus.Repairing;
            }
            if (lower.Contains("gửi hãng") || lower.Contains("hãng") || lower == "waitingvendor")
            {
                return TicketStatus.WaitingVendor;
            }
            if (lower == "repaired" || lower.Contains("đã sửa xong") || lower.Contains("xử lý xong") || lower == "completed")
            {
                return TicketStatus.Completed;
            }
            if (lower == "status-5" || lower == "closed" || lower.Contains("đóng") || lower.Contains("trả khách"))
            {
                return TicketStatus.Closed;
            }

            return TicketStatus.New;
        }
    }
}
