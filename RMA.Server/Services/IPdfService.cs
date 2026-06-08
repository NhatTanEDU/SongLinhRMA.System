using RMA.Shared.DTOs;

namespace RMA.Server.Services;

public interface IPdfService
{
    byte[] GenerateRmaReceiptPdf(RmaTicketDto ticket);
}
