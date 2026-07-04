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
    private readonly FirestoreRepository<SalesOrder> _orderRepo;
    private readonly FirestoreRepository<RmaTicket> _ticketRepo;
    private readonly FirestoreRepository<StatusHistory> _statusHistoryRepo;
    private readonly FirestoreRepository<StatusMaster> _statusMasterRepo;
    private readonly FirestoreRepository<Location> _locationRepo;

    public DevicesController(
        FirestoreRepository<Device> deviceRepo,
        FirestoreRepository<Customer> customerRepo,
        FirestoreRepository<Model> modelRepo,
        FirestoreRepository<SalesOrder> orderRepo,
        FirestoreRepository<RmaTicket> ticketRepo,
        FirestoreRepository<StatusHistory> statusHistoryRepo,
        FirestoreRepository<StatusMaster> statusMasterRepo,
        FirestoreRepository<Location> locationRepo)
    {
        _deviceRepo = deviceRepo;
        _customerRepo = customerRepo;
        _modelRepo = modelRepo;
        _orderRepo = orderRepo;
        _ticketRepo = ticketRepo;
        _statusHistoryRepo = statusHistoryRepo;
        _statusMasterRepo = statusMasterRepo;
        _locationRepo = locationRepo;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<DeviceDto>>> Get()
    {
        var devices = await _deviceRepo.GetAllAsync();
        
        // Optimize reads by fetching reference data in memory
        var customers = (await _customerRepo.GetAllAsync()).ToDictionary(c => c.Id, c => c.Name);
        var models = (await _modelRepo.GetAllAsync()).ToDictionary(m => m.Id, m => m);
        var orders = (await _orderRepo.GetAllAsync()).ToDictionary(o => o.Id, o => o.OrderCode);

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
            OrderId = d.OrderId,
            OrderCode = (d.OrderId != null && orders.ContainsKey(d.OrderId)) ? orders[d.OrderId] : d.OrderCode
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

        string? orderCode = d.OrderCode;
        if (string.IsNullOrEmpty(orderCode) && !string.IsNullOrEmpty(d.OrderId))
        {
            var order = await _orderRepo.GetByIdAsync(d.OrderId);
            orderCode = order?.OrderCode;
        }

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
            OrderId = d.OrderId,
            OrderCode = orderCode
        };
    }

    [HttpGet("by-order/{orderId}")]
    public async Task<ActionResult<IEnumerable<DeviceDto>>> GetByOrder(string orderId)
    {
        var devices = await _deviceRepo.GetByFieldAsync("OrderId", orderId);
        var customers = (await _customerRepo.GetAllAsync()).ToDictionary(c => c.Id, c => c.Name);
        var models = (await _modelRepo.GetAllAsync()).ToDictionary(m => m.Id, m => m);
        var order = await _orderRepo.GetByIdAsync(orderId);
        var orderCode = order?.OrderCode;

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
            OrderId = d.OrderId,
            OrderCode = d.OrderCode ?? orderCode
        });

        return Ok(dtos);
    }

    [HttpGet("by-serial/{serialNumber}")]
    public async Task<ActionResult<DeviceDto>> GetBySerialNumber(string serialNumber)
    {
        // v2: multi-case fallback for legacy data compatibility
        if (string.IsNullOrWhiteSpace(serialNumber)) return BadRequest("S/N không được để trống.");
        
        var normalizedSn = serialNumber.Trim().ToUpper();
        
        // Step 1 (Priority): Fast direct lookup by Document ID (O(1) complexity)
        var d = await _deviceRepo.GetByIdAsync(normalizedSn);
        
        // Step 2 (Fallback): Query by SerialNumber field - try original, UPPER and lower case
        // because old data may have been saved without normalization
        if (d == null)
        {
            var snOriginal = serialNumber.Trim();
            var snUpper = snOriginal.ToUpper();
            var snLower = snOriginal.ToLower();

            // Try UPPER first (current convention)
            var devicesUpper = await _deviceRepo.GetByFieldAsync("SerialNumber", snUpper);
            d = devicesUpper.FirstOrDefault();

            // Try original casing
            if (d == null && snOriginal != snUpper)
            {
                var devicesOrig = await _deviceRepo.GetByFieldAsync("SerialNumber", snOriginal);
                d = devicesOrig.FirstOrDefault();
            }

            // Try lower casing as last resort
            if (d == null && snLower != snOriginal && snLower != snUpper)
            {
                var devicesLower = await _deviceRepo.GetByFieldAsync("SerialNumber", snLower);
                d = devicesLower.FirstOrDefault();
            }
        }

        if (d == null) return NotFound("Không tìm thấy S/N trong hệ thống.");

        var c = await _customerRepo.GetByIdAsync(d.CustomerId);
        var m = await _modelRepo.GetByIdAsync(d.ModelId);

        string? orderCode = d.OrderCode;
        if (string.IsNullOrEmpty(orderCode) && !string.IsNullOrEmpty(d.OrderId))
        {
            var order = await _orderRepo.GetByIdAsync(d.OrderId);
            orderCode = order?.OrderCode;
        }

        return Ok(new DeviceDto
        {
            Id = d.Id,
            SerialNumber = d.SerialNumber,
            CustomerId = d.CustomerId,
            CustomerName = c?.Name ?? string.Empty,
            ModelId = d.ModelId,
            ModelName = m?.ModelName ?? string.Empty,
            Brand = m?.Brand ?? string.Empty,
            PurchaseDate = d.PurchaseDate,
            WarrantyExpiry = d.WarrantyExpiry,
            OrderId = d.OrderId,
            OrderCode = orderCode
        });
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Tech")]
    public async Task<ActionResult<DeviceDto>> Post([FromBody] DeviceCreateDto dto)
    {
        var normalizedSn = string.IsNullOrWhiteSpace(dto.SerialNumber) ? "" : dto.SerialNumber.Trim().ToUpper();
        var deviceId = string.IsNullOrEmpty(normalizedSn) || normalizedSn == "KHÔNG CÓ S/N" ? $"NOSERIAL-{Guid.NewGuid()}" : normalizedSn;

        if (!deviceId.StartsWith("NOSERIAL-"))
        {
            var existing = await _deviceRepo.GetByIdAsync(deviceId);
            if (existing != null)
            {
                var customer = await _customerRepo.GetByIdAsync(existing.CustomerId);
                var customerName = customer?.Name ?? "khách hàng khác";
                return BadRequest($"Mã S/N '{dto.SerialNumber}' đã tồn tại trên hệ thống thuộc sở hữu của khách hàng {customerName}!");
            }
        }

        var entity = new Device
        {
            Id = deviceId,
            SerialNumber = dto.SerialNumber,
            CustomerId = dto.CustomerId,
            ModelId = dto.ModelId,
            PurchaseDate = dto.PurchaseDate.HasValue ? DateTime.SpecifyKind(dto.PurchaseDate.Value, DateTimeKind.Utc) : null,
            WarrantyExpiry = dto.WarrantyExpiry.HasValue ? DateTime.SpecifyKind(dto.WarrantyExpiry.Value, DateTimeKind.Utc) : null
        };
        
        await _deviceRepo.UpdateAsync(deviceId, entity);

        return CreatedAtAction(nameof(Get), new { id = deviceId }, await Get(deviceId));
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

    [HttpGet("{serialNumber}/lifecycle")]
    public async Task<ActionResult<DeviceLifecycleDto>> GetLifecycle(string serialNumber)
    {
        // 1. Try get device by ID (SerialNumber uppercase)
        var deviceId = serialNumber.ToUpper();
        var device = await _deviceRepo.GetByIdAsync(deviceId);
        
        if (device == null)
        {
            // Fallback to query
            var devices = await _deviceRepo.GetByFieldAsync("SerialNumber", serialNumber);
            device = devices.FirstOrDefault();
            if (device == null)
            {
                return NotFound("Device not found.");
            }
        }

        // 2. Parallel fetch
        var customerTask = !string.IsNullOrEmpty(device.CustomerId) 
            ? _customerRepo.GetByIdAsync(device.CustomerId) 
            : Task.FromResult<Customer?>(null);
            
        var modelTask = !string.IsNullOrEmpty(device.ModelId) 
            ? _modelRepo.GetByIdAsync(device.ModelId) 
            : Task.FromResult<Model?>(null);
        
        Task<SalesOrder?> orderTask = Task.FromResult<SalesOrder?>(null);
        if (!string.IsNullOrEmpty(device.OrderId))
        {
            orderTask = _orderRepo.GetByIdAsync(device.OrderId);
        }

        var rmaTicketsTask = _ticketRepo.GetByFieldAsync("DeviceId", device.Id);
        
        await Task.WhenAll(customerTask, modelTask, orderTask, rmaTicketsTask);
        
        var customer = customerTask.Result;
        var model = modelTask.Result;
        var order = orderTask.Result;
        var rmaTickets = rmaTicketsTask.Result;

        // 3. Status Histories batch fetch
        var ticketIds = rmaTickets.Select(t => t.Id).ToList<object>();
        var allHistories = new List<StatusHistory>();
        
        if (ticketIds.Any())
        {
            var historiesTask = _statusHistoryRepo.GetByFieldInAsync("RmaTicketId", ticketIds);
            var statusMasterTask = _statusMasterRepo.GetAllAsync();
            var locationTask = _locationRepo.GetAllAsync();
            
            await Task.WhenAll(historiesTask, statusMasterTask, locationTask);
            
            allHistories = historiesTask.Result;
            var statusDict = statusMasterTask.Result.ToDictionary(s => s.Id, s => s.StatusName);
            var locationDict = locationTask.Result.ToDictionary(l => l.Id, l => l.Name);
            
            // Map histories and tickets
            var dto = new DeviceLifecycleDto
            {
                DeviceInfo = new DeviceLifecycleInfoDto
                {
                    SerialNumber = device.SerialNumber,
                    ModelName = model?.ModelName ?? string.Empty,
                    Brand = model?.Brand ?? string.Empty,
                    WarrantyExpiry = device.WarrantyExpiry
                },
                SalesInfo = new DeviceLifecycleSalesInfoDto
                {
                    OrderCode = order?.OrderCode ?? device.OrderCode,
                    DeliveryDate = order?.DeliveryDate,
                    CustomerName = customer?.Name
                },
                RmaTickets = rmaTickets.OrderByDescending(t => t.ReceivedDate).Select(t => new RmaTicketTimelineDto
                {
                    TicketId = t.Id,
                    ReceivedDate = t.ReceivedDate,
                    ServiceMode = t.ServiceMode,
                    ProblemDescription = t.ProblemDescription,
                    StatusName = t.StatusId != null && statusDict.ContainsKey(t.StatusId) ? statusDict[t.StatusId] : t.StatusId ?? string.Empty,
                    StatusHistories = allHistories.Where(h => h.RmaTicketId == t.Id)
                                        .OrderBy(h => h.UpdateTime)
                                        .Select(h => new StatusHistoryTimelineDto
                                        {
                                            UpdateTime = h.UpdateTime,
                                            LocationName = h.LocationId != null && locationDict.ContainsKey(h.LocationId) ? locationDict[h.LocationId] : string.Empty,
                                            Note = h.Note
                                        }).ToList()
                }).ToList()
            };
            
            return Ok(dto);
        }
        else
        {
             // No tickets
             var dto = new DeviceLifecycleDto
            {
                DeviceInfo = new DeviceLifecycleInfoDto
                {
                    SerialNumber = device.SerialNumber,
                    ModelName = model?.ModelName ?? string.Empty,
                    Brand = model?.Brand ?? string.Empty,
                    WarrantyExpiry = device.WarrantyExpiry
                },
                SalesInfo = new DeviceLifecycleSalesInfoDto
                {
                    OrderCode = order?.OrderCode ?? device.OrderCode,
                    DeliveryDate = order?.DeliveryDate,
                    CustomerName = customer?.Name
                },
                RmaTickets = new List<RmaTicketTimelineDto>()
            };
            
            return Ok(dto);
        }
    }
}
