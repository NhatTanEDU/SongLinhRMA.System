using System;

namespace RMA.Shared.DTOs
{
    public class SystemUserDto
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty; // Admin, Sales, Tech, Manager
        public string Status { get; set; } = "Active"; // Active, Inactive
        public string? Password { get; set; }
    }

    public class SystemSettingDto
    {
        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }

    public class AuditLogDto
    {
        public string Id { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string User { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string Details { get; set; } = string.Empty;
        public string OldValue { get; set; } = string.Empty;
        public string NewValue { get; set; } = string.Empty;
    }

    public class BypassDeliveryDto
    {
        public string OrderId { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
    }

    public class DispatchTicketDto
    {
        public string TicketId { get; set; } = string.Empty;
        public string TechnicianUsername { get; set; } = string.Empty;
        public string Note { get; set; } = string.Empty;
    }
}
