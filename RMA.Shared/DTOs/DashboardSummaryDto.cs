using System.Collections.Generic;

namespace RMA.Shared.DTOs
{
    public class DashboardSummaryDto
    {
        public int TotalOpenTickets { get; set; }
        public int UrgentTickets { get; set; }
        
        // SLA Warning Colors Count
        public int GreenAlertTickets { get; set; }
        public int YellowAlertTickets { get; set; }
        public int RedAlertTickets { get; set; }
        
        // Top 5 Vendors
        public List<VendorTicketCountDto> TopVendors { get; set; } = new();
    }

    public class VendorTicketCountDto
    {
        public string VendorName { get; set; } = string.Empty;
        public int TicketCount { get; set; }
    }
}
