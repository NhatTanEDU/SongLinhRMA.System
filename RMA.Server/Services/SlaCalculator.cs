using System;

namespace RMA.Server.Services
{
    public static class SlaCalculator
    {
        public static (string WarningColor, bool ShouldSetUrgent) Calculate(
            DateTime receivedDate, 
            DateTime? sentDate, 
            string? statusId,
            DateTime utcNow, 
            double yellowDays = 10, 
            double redDays = 14)
        {
            var status = RMA.Shared.Helpers.TicketStatusHelper.ParseFromDbString(statusId);

            if (status == RMA.Shared.Enums.TicketStatus.Closed || status == RMA.Shared.Enums.TicketStatus.Completed)
            {
                return ("Green", false);
            }

            if (status == RMA.Shared.Enums.TicketStatus.WaitingVendor && sentDate.HasValue)
            {
                double elapsedDays = (utcNow.Date - sentDate.Value.Date).TotalDays;
                if (elapsedDays >= redDays) return ("Red", true);
                if (elapsedDays >= yellowDays) return ("Yellow", false);
                return ("Green", false);
            }

            double totalElapsed = (utcNow.Date - receivedDate.Date).TotalDays;
            
            if (totalElapsed > redDays)
            {
                return ("Red", true);
            }
            else if (totalElapsed >= yellowDays)
            {
                return ("Yellow", false);
            }

            return ("Green", false);
        }
    }
}
