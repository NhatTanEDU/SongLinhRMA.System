using RMA.Shared.DTOs;

namespace RMA.Server.Services;

public interface IPdfService
{
    byte[] GenerateRmaReceiptPdf(RmaTicketDto ticket);
    byte[] GenerateHandoverPdf(RmaTicketDto ticket, TicketType ticketType, List<HandoverItemDto> items);
}
