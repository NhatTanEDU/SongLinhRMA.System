using System;

namespace RMA.Shared.DTOs
{
    public class DeviceDto
    {
        public string Id { get; set; } = string.Empty;
        public string SerialNumber { get; set; } = string.Empty;
        public string CustomerId { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string ModelId { get; set; } = string.Empty;
        public string ModelName { get; set; } = string.Empty;
        public string? Brand { get; set; }
        public DateTime? PurchaseDate { get; set; }
        public DateTime? WarrantyExpiry { get; set; }
        public string? OrderId { get; set; }
        public string? OrderCode { get; set; }
    }

    public class DeviceCreateDto
    {
        public string SerialNumber { get; set; } = string.Empty;
        public string CustomerId { get; set; } = string.Empty;
        public string ModelId { get; set; } = string.Empty;
        public DateTime? PurchaseDate { get; set; }
        public DateTime? WarrantyExpiry { get; set; }
    }
}
