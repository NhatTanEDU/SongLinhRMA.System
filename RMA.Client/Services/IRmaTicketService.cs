using RMA.Shared.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RMA.Client.Services;

public interface IRmaTicketService
{
    Task<List<RmaTicketDto>> GetRmaTicketsAsync();
    Task<RmaTicketDto?> GetRmaTicketAsync(string id);
    Task<RmaTicketDto?> CreateRmaTicketAsync(RmaTicketDto ticket);
    Task<bool> UpdateRmaTicketAsync(string id, RmaTicketDto ticket);
    Task<bool> DeleteRmaTicketAsync(string id);
    Task<List<StatusMasterDto>> GetStatusesAsync();
    Task<List<VendorDto>> GetVendorsAsync();
    Task<List<ModelDto>> GetModelsAsync();
}
