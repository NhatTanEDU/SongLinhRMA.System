using Microsoft.AspNetCore.Mvc;
using RMA.Server.Entities;
using RMA.Server.Services;
using RMA.Shared.DTOs;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;

namespace RMA.Server.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class DevicesController : ControllerBase
{
    private readonly FirestoreRepository<Device> _deviceRepo;
    private readonly FirestoreRepository<Customer> _customerRepo;
    private readonly FirestoreRepository<Model> _modelRepo;

    public DevicesController(
        FirestoreRepository<Device> deviceRepo,
        FirestoreRepository<Customer> customerRepo,
        FirestoreRepository<Model> modelRepo)
    {
        _deviceRepo = deviceRepo;
        _customerRepo = customerRepo;
        _modelRepo = modelRepo;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<DeviceDto>>> Get()
    {
        var devices = await _deviceRepo.GetAllAsync();
        
        // Optimize reads by fetching reference data in memory
        var customers = (await _customerRepo.GetAllAsync()).ToDictionary(c => c.Id, c => c.Name);
        var models = (await _modelRepo.GetAllAsync()).ToDictionary(m => m.Id, m => m);

        var dtos = devices.Select(d => new DeviceDto
        {
            Id = d.Id,
            SerialNumber = d.SerialNumber,
            CustomerId = d.CustomerId,
            CustomerName = customers.ContainsKey(d.CustomerId) ? customers[d.CustomerId] : string.Empty,
            ModelId = d.ModelId,
            ModelName = models.ContainsKey(d.ModelId) ? models[d.ModelId].ModelName : string.Empty,
            Brand = models.ContainsKey(d.ModelId) ? models[d.ModelId].Brand : string.Empty,
            PurchaseDate = d.PurchaseDate,
            WarrantyExpiry = d.WarrantyExpiry,
            OrderId = d.OrderId
        });

        return Ok(dtos);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<DeviceDto>> Get(string id)
    {
        var d = await _deviceRepo.GetByIdAsync(id);
        if (d == null) return NotFound();

        var c = await _customerRepo.GetByIdAsync(d.CustomerId);
        var m = await _modelRepo.GetByIdAsync(d.ModelId);

        return new DeviceDto
        {
            Id = d.Id,
            SerialNumber = d.SerialNumber,
            CustomerId = d.CustomerId,
            CustomerName = c?.Name ?? string.Empty,
            ModelId = d.ModelId,
            ModelName = m?.ModelName ?? string.Empty,
            Brand = m?.Brand,
            PurchaseDate = d.PurchaseDate,
            WarrantyExpiry = d.WarrantyExpiry,
            OrderId = d.OrderId
        };
    }

    [HttpGet("by-order/{orderId}")]
    public async Task<ActionResult<IEnumerable<DeviceDto>>> GetByOrder(string orderId)
    {
        var devices = await _deviceRepo.GetByFieldAsync("OrderId", orderId);
        var customers = (await _customerRepo.GetAllAsync()).ToDictionary(c => c.Id, c => c.Name);
        var models = (await _modelRepo.GetAllAsync()).ToDictionary(m => m.Id, m => m);

        var dtos = devices.Select(d => new DeviceDto
        {
            Id = d.Id,
            SerialNumber = d.SerialNumber,
            CustomerId = d.CustomerId,
            CustomerName = customers.ContainsKey(d.CustomerId) ? customers[d.CustomerId] : string.Empty,
            ModelId = d.ModelId,
            ModelName = models.ContainsKey(d.ModelId) ? models[d.ModelId].ModelName : string.Empty,
            Brand = models.ContainsKey(d.ModelId) ? models[d.ModelId].Brand : string.Empty,
            PurchaseDate = d.PurchaseDate,
            WarrantyExpiry = d.WarrantyExpiry,
            OrderId = d.OrderId
        });

        return Ok(dtos);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Tech")]
    public async Task<ActionResult<DeviceDto>> Post([FromBody] DeviceCreateDto dto)
    {
        var entity = new Device
        {
            SerialNumber = dto.SerialNumber,
            CustomerId = dto.CustomerId,
            ModelId = dto.ModelId,
            PurchaseDate = dto.PurchaseDate.HasValue ? DateTime.SpecifyKind(dto.PurchaseDate.Value, DateTimeKind.Utc) : null,
            WarrantyExpiry = dto.WarrantyExpiry.HasValue ? DateTime.SpecifyKind(dto.WarrantyExpiry.Value, DateTimeKind.Utc) : null
        };
        var newId = await _deviceRepo.AddAsync(entity);
        entity.Id = newId;

        return CreatedAtAction(nameof(Get), new { id = newId }, await Get(newId));
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Tech")]
    public async Task<IActionResult> Put(string id, [FromBody] DeviceCreateDto dto)
    {
        var entity = await _deviceRepo.GetByIdAsync(id);
        if (entity == null) return NotFound();

        entity.SerialNumber = dto.SerialNumber;
        entity.CustomerId = dto.CustomerId;
        entity.ModelId = dto.ModelId;
        entity.PurchaseDate = dto.PurchaseDate.HasValue ? DateTime.SpecifyKind(dto.PurchaseDate.Value, DateTimeKind.Utc) : null;
        entity.WarrantyExpiry = dto.WarrantyExpiry.HasValue ? DateTime.SpecifyKind(dto.WarrantyExpiry.Value, DateTimeKind.Utc) : null;

        await _deviceRepo.UpdateAsync(id, entity);
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Tech")]
    public async Task<IActionResult> Delete(string id)
    {
        await _deviceRepo.DeleteAsync(id);
        return NoContent();
    }
}
