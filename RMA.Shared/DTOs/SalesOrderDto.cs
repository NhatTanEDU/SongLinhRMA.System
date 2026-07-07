using System;
using System.Collections.Generic;

namespace RMA.Shared.DTOs
{
    public class SalesOrderDto
    {
        public string Id { get; set; } = string.Empty;
        public string OrderCode { get; set; } = string.Empty;
        public string CustomerId { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public string Status { get; set; } = "Pending";
        public string SalesNote { get; set; } = string.Empty;
        public string Note { get; set; } = string.Empty;
        public DateTime? LastUpdated { get; set; }
        public string UpdatedBy { get; set; } = string.Empty;
        public string CustomerAvatarUrl { get; set; } = string.Empty;
        public List<OrderDetailDto> Details { get; set; } = new List<OrderDetailDto>();
    }

    public class OrderDetailDto
    {
        public string ModelId { get; set; } = string.Empty;
        public string ModelName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public int WarrantyMonths { get; set; }
        public string DeviceSpecs { get; set; } = string.Empty;
        public string Note { get; set; } = string.Empty;
        public bool IsSerialRequired { get; set; }
        public List<string> SerialNumbers { get; set; } = new List<string>();
    }

    public class SalesOrderCreateDto
    {
        public string OrderCode { get; set; } = string.Empty;
        public string CustomerId { get; set; } = string.Empty;
        public string SalesNote { get; set; } = string.Empty;
        public List<OrderDetailCreateDto> Details { get; set; } = new List<OrderDetailCreateDto>();
    }

    public class OrderDetailCreateDto
    {
        public string ModelId { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public int? WarrantyMonths { get; set; }
        public string DeviceSpecs { get; set; } = string.Empty;
        public string Note { get; set; } = string.Empty;
    }

    public class ConfirmDeliveryDto
    {
        public string OrderId { get; set; } = string.Empty;
        public Dictionary<string, List<string>> SerialNumbersByModel { get; set; } = new Dictionary<string, List<string>>();
    }

    public class UpdateSalesOrderInfoDto
    {
        public string OrderId { get; set; } = string.Empty;
        public string? OrderCode { get; set; }
        public string? SalesNote { get; set; }
    }

    public class SalesOrderBusinessInfoDto
    {
        public string OrderCode { get; set; } = string.Empty;
        public string Note { get; set; } = string.Empty;
    }
}
