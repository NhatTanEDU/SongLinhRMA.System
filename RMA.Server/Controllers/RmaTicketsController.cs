using Microsoft.AspNetCore.Mvc;
using RMA.Server.Entities;
using RMA.Server.Services;
using RMA.Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RMA.Server.Controllers;

[Route("api/[controller]")]
[ApiController]
public class RmaTicketsController : ControllerBase
{
    private readonly FirestoreRepository<RmaTicket> _ticketRepo;
    private readonly FirestoreRepository<Device> _deviceRepo;
    private readonly FirestoreRepository<Customer> _customerRepo;
    private readonly FirestoreRepository<StatusMaster> _statusRepo;
    private readonly FirestoreRepository<Vendor> _vendorRepo;
    private readonly FirestoreRepository<Model> _modelRepo;
    private readonly IPdfService _pdfService;

    public RmaTicketsController(
        FirestoreRepository<RmaTicket> ticketRepo,
        FirestoreRepository<Device> deviceRepo,
        FirestoreRepository<Customer> customerRepo,
        FirestoreRepository<StatusMaster> statusRepo,
        FirestoreRepository<Vendor> vendorRepo,
        FirestoreRepository<Model> modelRepo,
        IPdfService pdfService)
    {
        _ticketRepo = ticketRepo;
        _deviceRepo = deviceRepo;
        _customerRepo = customerRepo;
        _statusRepo = statusRepo;
        _vendorRepo = vendorRepo;
        _modelRepo = modelRepo;
        _pdfService = pdfService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<RmaTicketDto>>> Get()
    {
        var tickets = await _ticketRepo.GetAllAsync();
        
        var devices = (await _deviceRepo.GetAllAsync()).ToDictionary(d => d.Id, d => d);
        var customers = (await _customerRepo.GetAllAsync()).ToDictionary(c => c.Id, c => c);
        var statuses = (await _statusRepo.GetAllAsync()).ToDictionary(s => s.Id, s => s);
        var vendors = (await _vendorRepo.GetAllAsync()).ToDictionary(v => v.Id, v => v);
        var models = (await _modelRepo.GetAllAsync()).ToDictionary(m => m.Id, m => m);

        var dtos = tickets.Select(t =>
        {
            var device = devices.TryGetValue(t.DeviceId, out var d) ? d : null;
            var customer = customers.TryGetValue(t.CustomerId, out var c) ? c : null;
            var status = statuses.TryGetValue(t.StatusId, out var s) ? s : null;
            var vendor = t.VendorId != null && vendors.TryGetValue(t.VendorId, out var v) ? v : null;
            var model = device != null && models.TryGetValue(device.ModelId, out var m) ? m : null;

            return new RmaTicketDto
            {
                Id = t.Id,
                DeviceId = t.DeviceId,
                DeviceSerialNumber = device?.SerialNumber ?? string.Empty,
                DeviceModelName = model?.ModelName ?? string.Empty,
                
                CustomerId = t.CustomerId,
                CustomerName = customer?.Name ?? string.Empty,
                CustomerPhone = customer?.Phone,
                CustomerContactPerson = customer?.ContactPerson,
                CustomerAvatarUrl = customer?.AvatarUrl,
                
                StatusId = t.StatusId,
                StatusName = status?.StatusName ?? string.Empty,
                StatusColorCode = status?.ColorCode,
                WarningColor = t.WarningColor,
                
                VendorId = t.VendorId,
                VendorName = vendor?.Name,
                
                ProblemDescription = t.ProblemDescription,
                ServiceMode = t.ServiceMode,
                ReceivedDate = t.ReceivedDate,
                SentDate = t.SentDate,
                IsUrgent = t.IsUrgent,
                StaffNote = t.StaffNote,
                EndUserName = t.EndUserName
            };
        });

        return Ok(dtos);
    }

    [HttpGet("statuses")]
    public async Task<ActionResult<IEnumerable<StatusMasterDto>>> GetStatuses()
    {
        var statuses = await _statusRepo.GetAllAsync();
        if (!statuses.Any())
        {
            var defaults = new List<StatusMaster>
            {
                new() { StatusName = "New", ColorCode = "Blue" },
                new() { StatusName = "In Progress", ColorCode = "Orange" },
                new() { StatusName = "Waiting for Parts", ColorCode = "Red" },
                new() { StatusName = "Repaired", ColorCode = "Green" },
                new() { StatusName = "Closed", ColorCode = "Gray" }
            };
            foreach (var s in defaults)
            {
                s.Id = await _statusRepo.AddAsync(s);
            }
            statuses = defaults;
        }

        return Ok(statuses.Select(s => new StatusMasterDto { Id = s.Id, StatusName = s.StatusName, ColorCode = s.ColorCode }));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<RmaTicketDto>> Get(string id)
    {
        var t = await _ticketRepo.GetByIdAsync(id);
        if (t == null) return NotFound();

        var device = await _deviceRepo.GetByIdAsync(t.DeviceId);
        var customer = await _customerRepo.GetByIdAsync(t.CustomerId);
        var status = await _statusRepo.GetByIdAsync(t.StatusId);
        var vendor = t.VendorId != null ? await _vendorRepo.GetByIdAsync(t.VendorId) : null;
        var model = device != null ? await _modelRepo.GetByIdAsync(device.ModelId) : null;

        return new RmaTicketDto
        {
            Id = t.Id,
            DeviceId = t.DeviceId,
            DeviceSerialNumber = device?.SerialNumber ?? string.Empty,
            DeviceModelName = model?.ModelName ?? string.Empty,
            
            CustomerId = t.CustomerId,
            CustomerName = customer?.Name ?? string.Empty,
            CustomerPhone = customer?.Phone,
            CustomerContactPerson = customer?.ContactPerson,
            CustomerAvatarUrl = customer?.AvatarUrl,
            
            StatusId = t.StatusId,
            StatusName = status?.StatusName ?? string.Empty,
            StatusColorCode = status?.ColorCode,
            WarningColor = t.WarningColor,
            
            VendorId = t.VendorId,
            VendorName = vendor?.Name,
            
            ProblemDescription = t.ProblemDescription,
            ServiceMode = t.ServiceMode,
            ReceivedDate = t.ReceivedDate,
            SentDate = t.SentDate,
            IsUrgent = t.IsUrgent,
            StaffNote = t.StaffNote,
            EndUserName = t.EndUserName
        };
    }

    [HttpPost]
    public async Task<ActionResult<RmaTicketDto>> Post([FromBody] RmaTicketCreateDto dto)
    {
        var entity = new RmaTicket
        {
            DeviceId = dto.DeviceId,
            CustomerId = dto.CustomerId,
            StatusId = dto.StatusId,
            VendorId = dto.VendorId,
            ProblemDescription = dto.ProblemDescription,
            ServiceMode = dto.ServiceMode,
            ReceivedDate = DateTime.UtcNow,
            IsUrgent = dto.IsUrgent,
            StaffNote = dto.StaffNote,
            EndUserName = dto.EndUserName
        };
        var newId = await _ticketRepo.AddAsync(entity);
        entity.Id = newId;

        var createdDto = await Get(newId);
        if (createdDto.Result is NotFoundResult)
        {
            return StatusCode(500, "Error creating and retrieving ticket reference mappings.");
        }
        return CreatedAtAction(nameof(Get), new { id = newId }, createdDto.Value);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Put(string id, [FromBody] RmaTicketCreateDto dto)
    {
        var entity = await _ticketRepo.GetByIdAsync(id);
        if (entity == null) return NotFound();

        entity.DeviceId = dto.DeviceId;
        entity.CustomerId = dto.CustomerId;
        entity.StatusId = dto.StatusId;
        entity.VendorId = dto.VendorId;
        entity.ProblemDescription = dto.ProblemDescription;
        entity.ServiceMode = dto.ServiceMode;
        entity.IsUrgent = dto.IsUrgent;
        entity.StaffNote = dto.StaffNote;
        entity.EndUserName = dto.EndUserName;

        await _ticketRepo.UpdateAsync(id, entity);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        await _ticketRepo.DeleteAsync(id);
        return NoContent();
    }

    [HttpGet("{id}/pdf")]
    public async Task<IActionResult> GetPdf(string id)
    {
        var t = await _ticketRepo.GetByIdAsync(id);
        if (t == null) return NotFound();

        var device = await _deviceRepo.GetByIdAsync(t.DeviceId);
        var customer = await _customerRepo.GetByIdAsync(t.CustomerId);
        var status = await _statusRepo.GetByIdAsync(t.StatusId);
        var vendor = t.VendorId != null ? await _vendorRepo.GetByIdAsync(t.VendorId) : null;
        var model = device != null ? await _modelRepo.GetByIdAsync(device.ModelId) : null;

        var dto = new RmaTicketDto
        {
            Id = t.Id,
            DeviceId = t.DeviceId,
            DeviceSerialNumber = device?.SerialNumber ?? string.Empty,
            DeviceModelName = model?.ModelName ?? string.Empty,
            
            CustomerId = t.CustomerId,
            CustomerName = customer?.Name ?? string.Empty,
            CustomerPhone = customer?.Phone,
            CustomerContactPerson = customer?.ContactPerson,
            CustomerAvatarUrl = customer?.AvatarUrl,
            
            StatusId = t.StatusId,
            StatusName = status?.StatusName ?? string.Empty,
            StatusColorCode = status?.ColorCode,
            WarningColor = t.WarningColor,
            
            VendorId = t.VendorId,
            VendorName = vendor?.Name,
            
            ProblemDescription = t.ProblemDescription,
            ServiceMode = t.ServiceMode,
            ReceivedDate = t.ReceivedDate,
            SentDate = t.SentDate,
            IsUrgent = t.IsUrgent,
            StaffNote = t.StaffNote,
            EndUserName = t.EndUserName
        };

        var pdfBytes = _pdfService.GenerateRmaReceiptPdf(dto);
        return File(pdfBytes, "application/pdf", $"RmaReceipt_{id}.pdf");
    }

    [HttpGet("dashboard-summary")]
    public async Task<ActionResult<DashboardSummaryDto>> GetDashboardSummary()
    {
        var tickets = await _ticketRepo.GetAllAsync();
        var statuses = (await _statusRepo.GetAllAsync()).ToDictionary(s => s.Id, s => s);
        var vendors = (await _vendorRepo.GetAllAsync()).ToDictionary(v => v.Id, v => v);

        var activeStatusIds = statuses.Values
            .Where(s => !s.StatusName.Equals("Closed", StringComparison.OrdinalIgnoreCase))
            .Select(s => s.Id)
            .ToHashSet();

        var activeTickets = tickets.Where(t => activeStatusIds.Contains(t.StatusId)).ToList();

        var summary = new DashboardSummaryDto
        {
            TotalOpenTickets = activeTickets.Count,
            UrgentTickets = activeTickets.Count(t => t.IsUrgent),
            GreenAlertTickets = activeTickets.Count(t => t.WarningColor == "Green"),
            YellowAlertTickets = activeTickets.Count(t => t.WarningColor == "Yellow"),
            RedAlertTickets = activeTickets.Count(t => t.WarningColor == "Red"),
            
            TopVendors = activeTickets
                .GroupBy(t => t.VendorId ?? "internal")
                .Select(g => new VendorTicketCountDto
                {
                    VendorName = g.Key == "internal" ? "Nội bộ" : (vendors.TryGetValue(g.Key, out var v) ? v.Name : "Khác"),
                    TicketCount = g.Count()
                })
                .OrderByDescending(v => v.TicketCount)
                .Take(5)
                .ToList()
        };

        return Ok(summary);
    }
}
