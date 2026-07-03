using System;
using System.Collections.Generic;

namespace RMA.Shared.DTOs
{
    public class SalesOrderSearchResultDto
    {
        public string OrderId { get; set; } = string.Empty;
        public string OrderCode { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerId { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string SalesNote { get; set; } = string.Empty;
        public string CustomerAvatarUrl { get; set; } = string.Empty;

        // Details of products in the order
        public List<OrderDetailDto> Details { get; set; } = new();

        // List of all Serial Numbers associated with this order
        public List<string> AssociatedSerials { get; set; } = new();
    }
}
