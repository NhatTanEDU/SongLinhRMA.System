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
    private readonly FirestoreRepository<StatusHistory> _statusHistoryRepo;
    private readonly FirestoreRepository<StatusMaster> _statusMasterRepo;
    private readonly FirestoreRepository<Location> _locationRepo;
    private readonly FirestoreDb _firestoreDb;

    public DevicesController(
        FirestoreRepository<Device> deviceRepo,
        FirestoreRepository<Customer> customerRepo,
        FirestoreRepository<Model> modelRepo,
        FirestoreRepository<SalesOrder> orderRepo,
        FirestoreRepository<RmaTicket> ticketRepo,
        FirestoreRepository<StatusHistory> statusHistoryRepo,
        FirestoreRepository<StatusMaster> statusMasterRepo,
        FirestoreRepository<Location> locationRepo,
        FirestoreDb firestoreDb)
    {
        _deviceRepo = deviceRepo;
        _customerRepo = customerRepo;
        _modelRepo = modelRepo;
        _orderRepo = orderRepo;
        _ticketRepo = ticketRepo;
        _statusHistoryRepo = statusHistoryRepo;
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

        if (device == null) return NotFound("Không tìm thấy thiết bị có S/N này.");

        var c = await _customerRepo.GetByIdAsync(device.CustomerId);
        var m = await _modelRepo.GetByIdAsync(device.ModelId);

        string? orderCode = device.OrderCode;
        if (string.IsNullOrEmpty(orderCode) && !string.IsNullOrEmpty(device.OrderId))
        {
            var order = await _orderRepo.GetByIdAsync(device.OrderId);
            orderCode = order?.OrderCode;
        }

        return new DeviceDto
        {
            Id = device.Id,
            SerialNumber = device.SerialNumber,
            CustomerId = device.CustomerId,
            CustomerName = c?.Name ?? string.Empty,
            ModelId = device.ModelId,
            ModelName = m?.ModelName ?? string.Empty,
            Brand = m?.Brand,
            PurchaseDate = device.PurchaseDate,
            WarrantyExpiry = device.WarrantyExpiry,
            OrderId = device.OrderId,
            OrderCode = orderCode
        };
    }

    [HttpPost]
    public async Task<ActionResult<string>> Create(DeviceDto dto)
    {
        var upperSn = dto.SerialNumber.ToUpper();
        var existing = await _deviceRepo.GetByIdAsync(upperSn);
        if (existing != null)
        {
            return BadRequest("Số S/N này đã tồn tại trong hệ thống.");
        }

        var device = new Device
        {
            Id = upperSn,
            SerialNumber = dto.SerialNumber,
            CustomerId = dto.CustomerId,
            ModelId = dto.ModelId,
            PurchaseDate = dto.PurchaseDate,
            WarrantyExpiry = dto.WarrantyExpiry,
            OrderId = dto.OrderId,
            OrderCode = dto.OrderCode
        };

        var id = await _deviceRepo.AddAsync(device);
        return Ok(id);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, DeviceDto dto)
    {
        var d = await _deviceRepo.GetByIdAsync(id);
        if (d == null) return NotFound();

        d.SerialNumber = dto.SerialNumber;
        d.CustomerId = dto.CustomerId;
        d.ModelId = dto.ModelId;
        d.PurchaseDate = dto.PurchaseDate;
        d.WarrantyExpiry = dto.WarrantyExpiry;
        d.OrderId = dto.OrderId;
        d.OrderCode = dto.OrderCode;

        await _deviceRepo.UpdateAsync(id, d);
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
                list = await _deviceRepo.GetByFieldAsync("SerialNumber", upperSn);
            }
            device = list.FirstOrDefault();
        }

        if (device == null)
        {
            return NotFound($"Không tìm thấy thiết bị có S/N: {serialNumber}");
        }

        // 2. Parallel fetch
        var customerTask = !string.IsNullOrEmpty(device.CustomerId) 
            ? _customerRepo.GetByIdAsync(device.CustomerId) 
            : Task.FromResult<Customer?>(null);
            
        var modelTask = !string.IsNullOrEmpty(device.ModelId) 
            ? _modelRepo.GetByIdAsync(device.ModelId) 
            : Task.FromResult<Model?>(null);
        
        Task<SalesOrder?> orderTask = !string.IsNullOrEmpty(device.OrderId)
            ? _orderRepo.GetByIdAsync(device.OrderId)
            : Task.FromResult<SalesOrder?>(null);

        var ticketsTask = _ticketRepo.GetByFieldAsync("DeviceId", device.Id);
        
        await Task.WhenAll(customerTask, modelTask, orderTask, ticketsTask);
        
        var customer = customerTask.Result;
        var model = modelTask.Result;
        var order = orderTask.Result;
        var tickets = ticketsTask.Result;

        // 3. Fetch Locations, Statuses, and histories
        var rmaHistory = new List<RmaTicketLifecycleDto>();
        var rmaTicketsList = new List<RmaTicketTimelineDto>();

        if (tickets.Any())
        {
            var locationsTask = _locationRepo.GetAllAsync();
            var statusesTask = _statusMasterRepo.GetAllAsync();
            await Task.WhenAll(locationsTask, statusesTask);
            
            var locationMap = locationsTask.Result.ToDictionary(l => l.Id, l => l.Name);
            var statuses = statusesTask.Result.ToDictionary(s => s.Id, s => s.StatusName);
            string GetStatusName(string? statusId) => statusId != null && statuses.TryGetValue(statusId, out var name) ? name : statusId ?? "";

            var ticketIds = tickets.Select(t => t.Id).ToList();
            var histories = new List<StatusHistory>();

            // Fetch via direct Firestore collection query (batch of 30)
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

            var historiesByTicket = histories
                .GroupBy(h => h.RmaTicketId)
                .ToDictionary(g => g.Key, g => g.OrderBy(h => h.UpdateTime).ToList());

            foreach (var t in tickets)
            {
                var statusName = GetStatusName(t.StatusId);
                var ticketHistories = historiesByTicket.TryGetValue(t.Id, out var histList) ? histList : new List<StatusHistory>();

                // Build RmaTicketLifecycleDto (for feature branch GlobalSearchDialog)
                var ticketDtoV2 = new RmaTicketLifecycleDto
                {
                    TicketId = t.Id,
                    ProblemDescription = t.ProblemDescription,
                    ServiceMode = t.ServiceMode,
                    ReceivedDate = t.ReceivedDate,
                    SentDate = t.SentDate,
                    Status = TicketStatusHelper.ParseFromDbString(statusName),
                    Steps = ticketHistories.Select(h => new RmaStepDto
                    {
                        Status = TicketStatusHelper.ParseFromDbString(GetStatusName(h.StatusId)),
                        LocationName = (h.LocationId != null && locationMap.ContainsKey(h.LocationId)) ? locationMap[h.LocationId] : "Nội bộ",
                        Note = h.Note,
                        ChangedAt = h.UpdateTime
                    }).OrderBy(s => s.ChangedAt).ToList()
                };
                var latestStep = ticketDtoV2.Steps.OrderByDescending(s => s.ChangedAt).FirstOrDefault();
                ticketDtoV2.CurrentLocationName = latestStep?.LocationName ?? "Nội bộ";
                rmaHistory.Add(ticketDtoV2);

                // Build RmaTicketTimelineDto (for main branch GlobalSearch)
                var ticketDtoV1 = new RmaTicketTimelineDto
                {
                    TicketId = t.Id,
                    ReceivedDate = t.ReceivedDate,
                    ServiceMode = t.ServiceMode,
                    ProblemDescription = t.ProblemDescription,
                    StatusName = statusName,
                    StatusHistories = ticketHistories.Select(h => new StatusHistoryTimelineDto
                    {
                        UpdateTime = h.UpdateTime,
                        LocationName = h.LocationId != null && locationMap.ContainsKey(h.LocationId) ? locationMap[h.LocationId] : "Nội bộ",
                        Note = h.Note
                    }).ToList()
                };
                rmaTicketsList.Add(ticketDtoV1);
            }
        }

        // Sort by received date descending
        rmaHistory = rmaHistory.OrderByDescending(h => h.ReceivedDate).ToList();
        rmaTicketsList = rmaTicketsList.OrderByDescending(t => t.ReceivedDate).ToList();

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
            OrderId = device.OrderId,
            OrderCode = order?.OrderCode ?? device.OrderCode
        };

        var lifecycleDto = new DeviceLifecycleDto
        {
            DeviceInfo = deviceDto,
            
            // For Main
            SalesInfo = new DeviceLifecycleSalesInfoDto
            {
                OrderCode = order?.OrderCode ?? device.OrderCode,
                DeliveryDate = order?.DeliveryDate,
                CustomerName = customer?.Name
            },
            RmaTickets = rmaTicketsList,

            // For Feature
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
