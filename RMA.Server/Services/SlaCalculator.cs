using System;

namespace RMA.Server.Services
{
    public static class SlaCalculator
    {
        public static (string WarningColor, bool ShouldSetUrgent) Calculate(DateTime? sentDate, DateTime utcNow, double yellowDays = 10, double redDays = 14)
        {
            if (!sentDate.HasValue)
            {
                return ("Green", false);
            }

            double diffDays = (utcNow - sentDate.Value).TotalDays;

            if (diffDays >= redDays)
            {
                return ("Red", true);
            }
            else if (diffDays >= yellowDays)
            {
                return ("Yellow", false);
            }

            return ("Green", false);
        }
    }
}
