using System;

namespace RMA.Shared.DTOs
{
    public class SalesOrderQueryDto
    {
        public string? SearchTerm { get; set; }
        public string? CustomerId { get; set; }
        public string? Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? SalesPersonId { get; set; }
        public string? ModelId { get; set; }
    }
}
