using System.Net.Http.Json;
using RMA.Shared.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RMA.Client.Services;

public class ReferenceDataService
{
    private readonly HttpClient _http;

    public ReferenceDataService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<VendorDto>> GetVendorsAsync()
    {
        return await _http.GetFromJsonAsync<List<VendorDto>>("api/referencedata/vendors") ?? new List<VendorDto>();
    }

    public async Task<VendorDto?> CreateVendorAsync(VendorDto vendor)
    {
        var response = await _http.PostAsJsonAsync("api/referencedata/vendors", vendor);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<VendorDto>();
        }
        return null;
    }

    public async Task<bool> UpdateVendorAsync(string id, VendorDto vendor)
    {
        var response = await _http.PutAsJsonAsync($"api/referencedata/vendors/{id}", vendor);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteVendorAsync(string id)
    {
        var response = await _http.DeleteAsync($"api/referencedata/vendors/{id}");
        return response.IsSuccessStatusCode;
    }

    public async Task<List<ModelDto>> GetModelsAsync()
    {
        return await _http.GetFromJsonAsync<List<ModelDto>>("api/referencedata/models") ?? new List<ModelDto>();
    }

    public async Task<List<CategoryDto>> GetCategoriesAsync()
    {
        return await _http.GetFromJsonAsync<List<CategoryDto>>("api/referencedata/categories") ?? new List<CategoryDto>();
    }

    public async Task<ModelDto?> CreateModelAsync(ModelDto model)
    {
        var response = await _http.PostAsJsonAsync("api/referencedata/models", model);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<ModelDto>();
        }
        return null;
    }
}
