using System;
using Microsoft.AspNetCore.Mvc;
using RMA.Server.Entities;
using RMA.Server.Services;
using RMA.Shared.DTOs;
using RMA.Shared.Enums;
using RMA.Shared.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Cloud.Firestore;

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
    private readonly FirestoreRepository<StatusMaster> _statusMasterRepo;
    private readonly FirestoreRepository<Location> _locationRepo;
    private readonly FirestoreDb _firestoreDb;

    public DevicesController(
        FirestoreRepository<Device> deviceRepo,
        FirestoreRepository<Customer> customerRepo,
        FirestoreRepository<Model> modelRepo,
        FirestoreRepository<SalesOrder> orderRepo,
        FirestoreRepository<RmaTicket> ticketRepo,
        FirestoreRepository<StatusMaster> statusMasterRepo,
        FirestoreRepository<Location> locationRepo,
        FirestoreDb firestoreDb)
    {
        _deviceRepo = deviceRepo;
        _customerRepo = customerRepo;
        _modelRepo = modelRepo;
        _orderRepo = orderRepo;
        _ticketRepo = ticketRepo;
        _statusMasterRepo = statusMasterRepo;
        _locationRepo = locationRepo;
        _firestoreDb = firestoreDb;
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

    [HttpGet("{serialNumber}/lifecycle")]
    public async Task<ActionResult<DeviceLifecycleDto>> GetLifecycle(string serialNumber)
    {
        if (string.IsNullOrWhiteSpace(serialNumber))
        {
            return BadRequest("Số S/N không hợp lệ.");
        }

        // 1. Tìm Device theo S/N
        var upperSn = serialNumber.ToUpper();
        Device? device = await _deviceRepo.GetByIdAsync(upperSn);
        if (device == null)
        {
            // Fallback query by SerialNumber field
            var list = await _deviceRepo.GetByFieldAsync("SerialNumber", serialNumber);
            if (!list.Any())
            {
                // Try upper case fallback too
                list = await _deviceRepo.GetByFieldAsync("SerialNumber", upperSn);
            }
            device = list.FirstOrDefault();
        }

        if (device == null)
        {
            return NotFound($"Không tìm thấy thiết bị có S/N: {serialNumber}");
        }

        // 2. Query parallelly (Task.WhenAll): Model, Customer, SalesOrder, RmaTickets
        var modelTask = _modelRepo.GetByIdAsync(device.ModelId);
        var customerTask = _customerRepo.GetByIdAsync(device.CustomerId);
        
        Task<SalesOrder?> orderTask = !string.IsNullOrEmpty(device.OrderId)
            ? _orderRepo.GetByIdAsync(device.OrderId)
            : Task.FromResult<SalesOrder?>(null);

        Task<List<RmaTicket>> ticketsTask = _ticketRepo.GetByFieldAsync("DeviceId", device.Id);

        await Task.WhenAll(modelTask, customerTask, orderTask, ticketsTask);

        var model = modelTask.Result;
        var customer = customerTask.Result;
        var order = orderTask.Result;
        var tickets = ticketsTask.Result;

        // 3. Lấy tất cả status histories của các tickets thu được
        var rmaHistory = new List<RmaTicketLifecycleDto>();
        if (tickets.Any())
        {
            // Lấy tất cả Locations và Statuses
            var locationsTask = _locationRepo.GetAllAsync();
            var statusesTask = _statusMasterRepo.GetAllAsync();
            await Task.WhenAll(locationsTask, statusesTask);
            
            var locationMap = locationsTask.Result.ToDictionary(l => l.Id, l => l);
            var statuses = statusesTask.Result.ToDictionary(s => s.Id, s => s.StatusName);
            string GetStatusName(string? statusId) => statusId != null && statuses.TryGetValue(statusId, out var name) ? name : statusId ?? "";

            // Fetch status histories for all tickets in parallel or single query using FirestoreDb directly (WhereIn)
            var ticketIds = tickets.Select(t => t.Id).ToList();
            var histories = new List<StatusHistory>();

            // Firestore WhereIn supports up to 30 items
            const int batchSize = 30;
            for (int i = 0; i < ticketIds.Count; i += batchSize)
            {
                var batchIds = ticketIds.Skip(i).Take(batchSize).ToList();
                var querySnapshot = await _firestoreDb.Collection("status_histories")
                    .WhereIn("RmaTicketId", batchIds)
                    .GetSnapshotAsync();
                
                foreach (var doc in querySnapshot.Documents)
                {
                    if (doc.Exists)
                    {
                        histories.Add(doc.ConvertTo<StatusHistory>());
                    }
                }
            }

            // Map and group histories by ticket
            var historiesByTicket = histories
                .GroupBy(h => h.RmaTicketId)
                .ToDictionary(g => g.Key, g => g.OrderBy(h => h.UpdateTime).ToList());

            foreach (var t in tickets)
            {
                var ticketDto = new RmaTicketLifecycleDto
                {
                    TicketId = t.Id,
                    ProblemDescription = t.ProblemDescription,
                    ServiceMode = t.ServiceMode,
                    ReceivedDate = t.ReceivedDate,
                    SentDate = t.SentDate,
                    Status = TicketStatusHelper.ParseFromDbString(GetStatusName(t.StatusId))
                };

                if (historiesByTicket.TryGetValue(t.Id, out var ticketHistories))
                {
                    ticketDto.Steps = ticketHistories.Select(h => new RmaStepDto
                    {
                        Status = TicketStatusHelper.ParseFromDbString(GetStatusName(h.StatusId)),
                        LocationName = (h.LocationId != null && locationMap.ContainsKey(h.LocationId)) ? locationMap[h.LocationId].Name : "Nội bộ",
                        Note = h.Note,
                        ChangedAt = h.UpdateTime
                    }).OrderBy(s => s.ChangedAt).ToList();

                    var latestStep = ticketDto.Steps.OrderByDescending(s => s.ChangedAt).FirstOrDefault();
                    ticketDto.CurrentLocationName = latestStep?.LocationName ?? "Nội bộ";
                }
                else
                {
                    ticketDto.CurrentLocationName = "Nội bộ";
                }

                rmaHistory.Add(ticketDto);
            }
        }

        // Sort RMA history by received date descending
        rmaHistory = rmaHistory.OrderByDescending(h => h.ReceivedDate).ToList();

        var deviceDto = new DeviceDto
        {
            Id = device.Id,
            SerialNumber = device.SerialNumber,
            CustomerId = device.CustomerId,
            CustomerName = customer?.Name ?? string.Empty,
            ModelId = device.ModelId,
            ModelName = model?.ModelName ?? string.Empty,
            Brand = model?.Brand ?? string.Empty,
            PurchaseDate = device.PurchaseDate,
            WarrantyExpiry = device.WarrantyExpiry,
            OrderId = device.OrderId
        };

        var lifecycleDto = new DeviceLifecycleDto
        {
            DeviceInfo = deviceDto,
            OriginalOrderCode = order?.OrderCode,
            OriginalOrderDeliveryDate = order?.DeliveryDate,
            RmaHistory = rmaHistory
        };

        return Ok(lifecycleDto);
    }

    [HttpGet("{serialNumber}/summary")]
    public async Task<ActionResult<DeviceSummaryDto>> GetSummary(string serialNumber)
    {
        if (string.IsNullOrWhiteSpace(serialNumber))
        {
            return BadRequest("Số S/N không hợp lệ.");
        }

        var upperSn = serialNumber.ToUpper();
        Device? device = await _deviceRepo.GetByIdAsync(upperSn);
        if (device == null)
        {
            var list = await _deviceRepo.GetByFieldAsync("SerialNumber", serialNumber);
            if (!list.Any())
            {
                list = await _deviceRepo.GetByFieldAsync("SerialNumber", upperSn);
            }
            device = list.FirstOrDefault();
        }

        if (device == null)
        {
            return NotFound($"Không tìm thấy thiết bị có S/N: {serialNumber}");
        }

        var modelTask = _modelRepo.GetByIdAsync(device.ModelId);
        var customerTask = _customerRepo.GetByIdAsync(device.CustomerId);
        
        Task<SalesOrder?> orderTask = !string.IsNullOrEmpty(device.OrderId)
            ? _orderRepo.GetByIdAsync(device.OrderId)
            : Task.FromResult<SalesOrder?>(null);

        Task<List<RmaTicket>> ticketsTask = _ticketRepo.GetByFieldAsync("DeviceId", device.Id);
        Task<List<StatusMaster>> statusesTask = _statusMasterRepo.GetAllAsync();

        await Task.WhenAll(modelTask, customerTask, orderTask, ticketsTask, statusesTask);

        var model = modelTask.Result;
        var customer = customerTask.Result;
        var order = orderTask.Result;
        var tickets = ticketsTask.Result;
        var statuses = statusesTask.Result.ToDictionary(s => s.Id, s => s.StatusName);

        string GetStatusName(string? statusId) => statusId != null && statuses.TryGetValue(statusId, out var name) ? name : statusId ?? "";

        RmaTicket? activeTicket = null;
        if (tickets.Any())
        {
            activeTicket = tickets.FirstOrDefault(t => 
                TicketStatusHelper.ParseFromDbString(GetStatusName(t.StatusId)) != TicketStatus.Completed && 
                TicketStatusHelper.ParseFromDbString(GetStatusName(t.StatusId)) != TicketStatus.Closed);
        }

        var latestTicket = tickets.OrderByDescending(t => t.ReceivedDate).FirstOrDefault();
        string currentLocationName = "Nội bộ";
        TicketStatus currentStatus = TicketStatus.New;
        OpenTicketSummaryDto? activeTicketDto = null;

        var locations = await _locationRepo.GetAllAsync();
        var locationMap = locations.ToDictionary(l => l.Id, l => l.Name);

        if (latestTicket != null)
        {
            currentStatus = TicketStatusHelper.ParseFromDbString(GetStatusName(latestTicket.StatusId));

            var historySnapshot = await _firestoreDb.Collection("status_histories")
                .WhereEqualTo("RmaTicketId", latestTicket.Id)
                .GetSnapshotAsync();
            
            var history = historySnapshot.Documents
                .Where(d => d.Exists)
                .Select(d => d.ConvertTo<StatusHistory>())
                .OrderByDescending(h => h.UpdateTime)
                .FirstOrDefault();

            if (history != null)
            {
                if (history.LocationId != null && locationMap.TryGetValue(history.LocationId, out var locName))
                {
                    currentLocationName = locName;
                }
            }
        }

        if (activeTicket != null)
        {
            activeTicketDto = new OpenTicketSummaryDto
            {
                TicketId = activeTicket.Id,
                TicketCode = activeTicket.Id,
                Status = TicketStatusHelper.ParseFromDbString(GetStatusName(activeTicket.StatusId)),
                SentDate = activeTicket.SentDate,
                ReceivedDate = activeTicket.ReceivedDate
            };

            if (activeTicket.Id == latestTicket?.Id && currentLocationName != "Nội bộ")
            {
                activeTicketDto.LocationName = currentLocationName;
            }
            else
            {
                var actHistSnapshot = await _firestoreDb.Collection("status_histories")
                    .WhereEqualTo("RmaTicketId", activeTicket.Id)
                    .GetSnapshotAsync();
                
                var actHistory = actHistSnapshot.Documents
                    .Where(d => d.Exists)
                    .Select(d => d.ConvertTo<StatusHistory>())
                    .OrderByDescending(h => h.UpdateTime)
                    .FirstOrDefault();

                if (actHistory != null)
                {
                    if (actHistory.LocationId != null && locationMap.TryGetValue(actHistory.LocationId, out var locName))
                    {
                        activeTicketDto.LocationName = locName;
                    }
                    else
                    {
                        activeTicketDto.LocationName = "Nội bộ";
                    }
                }
                else
                {
                    activeTicketDto.LocationName = "Nội bộ";
                }
            }
        }

        var summaryDto = new DeviceSummaryDto
        {
            DeviceId = device.Id,
            SerialNumber = device.SerialNumber,
            ModelName = model?.ModelName ?? string.Empty,
            Brand = model?.Brand ?? string.Empty,
            PurchaseDate = device.PurchaseDate,
            WarrantyExpiry = device.WarrantyExpiry,
            CustomerId = device.CustomerId,
            CustomerName = customer?.Name ?? string.Empty,
            OrderId = device.OrderId,
            OrderCode = order?.OrderCode,
            DeliveryDate = order?.DeliveryDate,
            CurrentLocationName = currentLocationName,
            CurrentStatus = currentStatus,
            ActiveTicket = activeTicketDto,
            TicketHistory = tickets.OrderByDescending(t => t.ReceivedDate).Select(t => new OpenTicketSummaryDto
            {
                TicketId = t.Id,
                TicketCode = t.Id,
                Status = TicketStatusHelper.ParseFromDbString(GetStatusName(t.StatusId)),
                SentDate = t.SentDate,
                ReceivedDate = t.ReceivedDate,
                LocationName = "" // Location is not deeply fetched for all histories to save reads, except latest/active which is done above.
            }).ToList()
        };

        return Ok(summaryDto);
    }
}
