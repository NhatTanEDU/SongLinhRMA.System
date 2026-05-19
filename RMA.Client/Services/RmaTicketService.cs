using System.Net.Http.Json;
using RMA.Shared.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RMA.Client.Services;

public class RmaTicketService : IRmaTicketService
{
    private readonly HttpClient _http;

    public RmaTicketService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<RmaTicketDto>> GetRmaTicketsAsync()
    {
        return await _http.GetFromJsonAsync<List<RmaTicketDto>>("api/rmatickets") ?? new List<RmaTicketDto>();
    }

    public async Task<RmaTicketDto?> GetRmaTicketAsync(string id)
    {
        return await _http.GetFromJsonAsync<RmaTicketDto>($"api/rmatickets/{id}");
    }

    public async Task<RmaTicketDto?> CreateRmaTicketAsync(RmaTicketDto ticket)
    {
        var createDto = new RmaTicketCreateDto
        {
            DeviceId = ticket.DeviceId,
            CustomerId = ticket.CustomerId,
            StatusId = ticket.StatusId,
            VendorId = ticket.VendorId,
            ProblemDescription = ticket.ProblemDescription,
            ServiceMode = ticket.ServiceMode,
            IsUrgent = ticket.IsUrgent,
            StaffNote = ticket.StaffNote
        };

        var response = await _http.PostAsJsonAsync("api/rmatickets", createDto);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<RmaTicketDto>();
        }
        return null;
    }

    public async Task<bool> UpdateRmaTicketAsync(string id, RmaTicketDto ticket)
    {
        var createDto = new RmaTicketCreateDto
        {
            DeviceId = ticket.DeviceId,
            CustomerId = ticket.CustomerId,
            StatusId = ticket.StatusId,
            VendorId = ticket.VendorId,
            ProblemDescription = ticket.ProblemDescription,
            ServiceMode = ticket.ServiceMode,
            IsUrgent = ticket.IsUrgent,
            StaffNote = ticket.StaffNote
        };

        var response = await _http.PutAsJsonAsync($"api/rmatickets/{id}", createDto);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteRmaTicketAsync(string id)
    {
        var response = await _http.DeleteAsync($"api/rmatickets/{id}");
        return response.IsSuccessStatusCode;
    }
    
    public async Task<List<StatusMasterDto>> GetStatusesAsync()
    {
        return await _http.GetFromJsonAsync<List<StatusMasterDto>>("api/rmatickets/statuses") ?? new List<StatusMasterDto>();
    }

    public async Task<List<VendorDto>> GetVendorsAsync()
    {
        return await _http.GetFromJsonAsync<List<VendorDto>>("api/referencedata/vendors") ?? new List<VendorDto>();
    }

    public async Task<List<ModelDto>> GetModelsAsync()
    {
        return await _http.GetFromJsonAsync<List<ModelDto>>("api/referencedata/models") ?? new List<ModelDto>();
    }
}
