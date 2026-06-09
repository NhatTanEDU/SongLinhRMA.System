namespace RMA.Shared.DTOs
{
    public class TicketPagedRequestDto
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? WarningColor { get; set; }
        public int? Month { get; set; }
    }
}
