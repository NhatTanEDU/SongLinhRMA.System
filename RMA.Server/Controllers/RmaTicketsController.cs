using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
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
    private readonly FirestoreRepository<Attachment> _attachmentRepo;
    private readonly FirestoreRepository<StatusHistory> _statusHistoryRepo;
    private readonly FirestoreRepository<Location> _locationRepo;
    private readonly IPdfService _pdfService;
    private readonly IMemoryCache _cache;

    public RmaTicketsController(
        FirestoreRepository<RmaTicket> ticketRepo,
        FirestoreRepository<Device> deviceRepo,
        FirestoreRepository<Customer> customerRepo,
        FirestoreRepository<StatusMaster> statusRepo,
        FirestoreRepository<Vendor> vendorRepo,
        FirestoreRepository<Model> modelRepo,
        FirestoreRepository<Attachment> attachmentRepo,
        FirestoreRepository<StatusHistory> statusHistoryRepo,
        FirestoreRepository<Location> locationRepo,
        IPdfService pdfService,
        IMemoryCache cache)
    {
        _ticketRepo = ticketRepo;
        _deviceRepo = deviceRepo;
        _customerRepo = customerRepo;
        _statusRepo = statusRepo;
        _vendorRepo = vendorRepo;
        _modelRepo = modelRepo;
        _attachmentRepo = attachmentRepo;
        _statusHistoryRepo = statusHistoryRepo;
        _locationRepo = locationRepo;
        _pdfService = pdfService;
        _cache = cache;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<RmaTicketDto>>> Get()
    {
        var tickets = await _ticketRepo.GetAllAsync();
        
        var devices = await _cache.GetOrCreateAsync("devices_dict", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
            return (await _deviceRepo.GetAllAsync()).ToDictionary(d => d.Id, d => d);
        }) ?? new Dictionary<string, Device>();

        var customers = await _cache.GetOrCreateAsync("customers_dict", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
            return (await _customerRepo.GetAllAsync()).ToDictionary(c => c.Id, c => c);
        }) ?? new Dictionary<string, Customer>();

        var statuses = await _cache.GetOrCreateAsync("statuses_dict", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
            return (await _statusRepo.GetAllAsync()).ToDictionary(s => s.Id, s => s);
        }) ?? new Dictionary<string, StatusMaster>();

        var vendors = await _cache.GetOrCreateAsync("vendors_dict", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
            return (await _vendorRepo.GetAllAsync()).ToDictionary(v => v.Id, v => v);
        }) ?? new Dictionary<string, Vendor>();

        var models = await _cache.GetOrCreateAsync("models_dict", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
            return (await _modelRepo.GetAllAsync()).ToDictionary(m => m.Id, m => m);
        }) ?? new Dictionary<string, Model>();

        var attachmentsGroup = await _cache.GetOrCreateAsync("attachments_group", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30);
            return (await _attachmentRepo.GetAllAsync()).GroupBy(a => a.RmaTicketId).ToDictionary(g => g.Key, g => g.ToList());
        }) ?? new Dictionary<string, List<Attachment>>();

        var statusHistoriesGroup = await _cache.GetOrCreateAsync("histories_group", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30);
            return (await _statusHistoryRepo.GetAllAsync()).GroupBy(sh => sh.RmaTicketId).ToDictionary(g => g.Key, g => g.ToList());
        }) ?? new Dictionary<string, List<StatusHistory>>();

        var locations = await _cache.GetOrCreateAsync("locations_dict", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
            return (await _locationRepo.GetAllAsync()).ToDictionary(l => l.Id, l => l);
        }) ?? new Dictionary<string, Location>();

        var dtos = tickets.Select(t =>
        {
            var device = devices.TryGetValue(t.DeviceId, out var d) ? d : null;
            var customer = customers.TryGetValue(t.CustomerId, out var c) ? c : null;
            var status = statuses.TryGetValue(t.StatusId, out var s) ? s : null;
            var vendor = t.VendorId != null && vendors.TryGetValue(t.VendorId, out var v) ? v : null;
            var model = device != null && models.TryGetValue(device.ModelId, out var m) ? m : null;

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
            dto.PopulateChecklistsFromStaffNote();

            if (attachmentsGroup.TryGetValue(t.Id, out var atts))
            {
                dto.Attachments = atts.Select(a => new AttachmentDto
                {
                    Id = a.Id,
                    FileUrl = a.FileUrl,
                    FileName = System.IO.Path.GetFileName(a.FileUrl) ?? "Attachment",
                    UploadedAt = a.UploadedAt
                }).ToList();
            }

            if (statusHistoriesGroup.TryGetValue(t.Id, out var shs))
            {
                dto.StatusHistories = shs.Select(sh =>
                {
                    var locName = sh.LocationId != null && locations.TryGetValue(sh.LocationId, out var loc) ? loc.Name : "Nội bộ";
                    var stName = sh.StatusId != null && statuses.TryGetValue(sh.StatusId, out var st) ? st.StatusName : "Cập nhật";
                    return new StatusHistoryDto
                    {
                        Id = sh.Id,
                        StatusName = stName,
                        LocationName = locName,
                        Note = sh.Note,
                        CreatedAt = sh.UpdateTime
                    };
                }).OrderByDescending(h => h.CreatedAt).ToList();
            }

            return dto;
        }).ToList();

        return Ok(dtos);
    }

    [HttpGet("paged")]
    public async Task<ActionResult<IEnumerable<RmaTicketDto>>> GetPaged([FromQuery] TicketPagedRequestDto request)
    {
        int pageNumber = request.PageNumber > 0 ? request.PageNumber : 1;
        int pageSize = request.PageSize > 0 ? request.PageSize : 10;

        List<RmaTicket> tickets;

        if (request.Month.HasValue || !string.IsNullOrEmpty(request.WarningColor))
        {
            var allTickets = await _cache.GetOrCreateAsync("all_tickets_list", async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(5);
                return await _ticketRepo.GetAllAsync();
            }) ?? new List<RmaTicket>();

            tickets = allTickets;

            if (request.Month.HasValue)
            {
                tickets = tickets.Where(t => t.ReceivedDate.Month == request.Month.Value || (t.SentDate.HasValue && t.SentDate.Value.Month == request.Month.Value)).ToList();
            }

            if (!string.IsNullOrEmpty(request.WarningColor))
            {
                tickets = tickets.Where(t => string.Equals(t.WarningColor, request.WarningColor, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            tickets = tickets.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
        }
        else
        {
            tickets = await _ticketRepo.GetPagedAsync(pageSize, (pageNumber - 1) * pageSize);
        }

        var devices = await _cache.GetOrCreateAsync("devices_dict", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
            return (await _deviceRepo.GetAllAsync()).ToDictionary(d => d.Id, d => d);
        }) ?? new Dictionary<string, Device>();

        var customers = await _cache.GetOrCreateAsync("customers_dict", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
            return (await _customerRepo.GetAllAsync()).ToDictionary(c => c.Id, c => c);
        }) ?? new Dictionary<string, Customer>();

        var statuses = await _cache.GetOrCreateAsync("statuses_dict", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
            return (await _statusRepo.GetAllAsync()).ToDictionary(s => s.Id, s => s);
        }) ?? new Dictionary<string, StatusMaster>();

        var vendors = await _cache.GetOrCreateAsync("vendors_dict", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
            return (await _vendorRepo.GetAllAsync()).ToDictionary(v => v.Id, v => v);
        }) ?? new Dictionary<string, Vendor>();

        var models = await _cache.GetOrCreateAsync("models_dict", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
            return (await _modelRepo.GetAllAsync()).ToDictionary(m => m.Id, m => m);
        }) ?? new Dictionary<string, Model>();

        var attachmentsGroup = await _cache.GetOrCreateAsync("attachments_group", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30);
            return (await _attachmentRepo.GetAllAsync()).GroupBy(a => a.RmaTicketId).ToDictionary(g => g.Key, g => g.ToList());
        }) ?? new Dictionary<string, List<Attachment>>();

        var statusHistoriesGroup = await _cache.GetOrCreateAsync("histories_group", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30);
            return (await _statusHistoryRepo.GetAllAsync()).GroupBy(sh => sh.RmaTicketId).ToDictionary(g => g.Key, g => g.ToList());
        }) ?? new Dictionary<string, List<StatusHistory>>();

        var locations = await _cache.GetOrCreateAsync("locations_dict", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
            return (await _locationRepo.GetAllAsync()).ToDictionary(l => l.Id, l => l);
        }) ?? new Dictionary<string, Location>();

        var dtos = tickets.Select(t =>
        {
            var device = devices.TryGetValue(t.DeviceId, out var d) ? d : null;
            var customer = customers.TryGetValue(t.CustomerId, out var c) ? c : null;
            var status = statuses.TryGetValue(t.StatusId, out var s) ? s : null;
            var vendor = t.VendorId != null && vendors.TryGetValue(t.VendorId, out var v) ? v : null;
            var model = device != null && models.TryGetValue(device.ModelId, out var m) ? m : null;

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
            dto.PopulateChecklistsFromStaffNote();

            if (attachmentsGroup.TryGetValue(t.Id, out var atts))
            {
                dto.Attachments = atts.Select(a => new AttachmentDto
                {
                    Id = a.Id,
                    FileUrl = a.FileUrl,
                    FileName = System.IO.Path.GetFileName(a.FileUrl) ?? "Attachment",
                    UploadedAt = a.UploadedAt
                }).ToList();
            }

            if (statusHistoriesGroup.TryGetValue(t.Id, out var shs))
            {
                dto.StatusHistories = shs.Select(sh =>
                {
                    var locName = sh.LocationId != null && locations.TryGetValue(sh.LocationId, out var loc) ? loc.Name : "Nội bộ";
                    var stName = sh.StatusId != null && statuses.TryGetValue(sh.StatusId, out var st) ? st.StatusName : "Cập nhật";
                    return new StatusHistoryDto
                    {
                        Id = sh.Id,
                        StatusName = stName,
                        LocationName = locName,
                        Note = sh.Note,
                        CreatedAt = sh.UpdateTime
                    };
                }).OrderByDescending(h => h.CreatedAt).ToList();
            }

            return dto;
        }).ToList();

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

        var attachments = await _attachmentRepo.GetByFieldAsync("RmaTicketId", id);
        var statusHistories = await _statusHistoryRepo.GetByFieldAsync("RmaTicketId", id);
        var locations = (await _locationRepo.GetAllAsync()).ToDictionary(l => l.Id, l => l);
        var statuses = (await _statusRepo.GetAllAsync()).ToDictionary(s => s.Id, s => s);

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
        dto.PopulateChecklistsFromStaffNote();

        dto.Attachments = attachments.Select(a => new AttachmentDto
        {
            Id = a.Id,
            FileUrl = a.FileUrl,
            FileName = System.IO.Path.GetFileName(a.FileUrl) ?? "Attachment",
            UploadedAt = a.UploadedAt
        }).ToList();

        dto.StatusHistories = statusHistories.Select(sh =>
        {
            var locName = sh.LocationId != null && locations.TryGetValue(sh.LocationId, out var loc) ? loc.Name : "Nội bộ";
            var stName = sh.StatusId != null && statuses.TryGetValue(sh.StatusId, out var st) ? st.StatusName : "Cập nhật";
            return new StatusHistoryDto
            {
                Id = sh.Id,
                StatusName = stName,
                LocationName = locName,
                Note = sh.Note,
                CreatedAt = sh.UpdateTime
            };
        }).OrderByDescending(h => h.CreatedAt).ToList();

        return dto;
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

        // Process attachments
        await ProcessAttachmentsAsync(newId, dto.Attachments);

        // Add initial status history
        string? locName = ExtractLocationFromStaffNote(dto.StaffNote);
        string locId = string.Empty;
        if (!string.IsNullOrEmpty(locName))
        {
            locId = await ResolveLocationIdAsync(locName);
        }
        var firstHistory = new StatusHistory
        {
            RmaTicketId = newId,
            StatusId = dto.StatusId,
            LocationId = string.IsNullOrEmpty(locId) ? null : locId,
            UpdateTime = DateTime.UtcNow,
            Note = "Tiếp nhận phiếu mới"
        };
        await _statusHistoryRepo.AddAsync(firstHistory);

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

        var oldStatusId = entity.StatusId;
        var oldStaffNote = entity.StaffNote;

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

        // Process attachments
        await ProcessAttachmentsAsync(id, dto.Attachments);

        // Add status history if changed
        string? newLocName = ExtractLocationFromStaffNote(dto.StaffNote);
        string? oldLocName = ExtractLocationFromStaffNote(oldStaffNote);

        if (oldStatusId != dto.StatusId || newLocName != oldLocName)
        {
            string locId = string.Empty;
            if (!string.IsNullOrEmpty(newLocName))
            {
                locId = await ResolveLocationIdAsync(newLocName);
            }

            var history = new StatusHistory
            {
                RmaTicketId = id,
                StatusId = dto.StatusId,
                LocationId = string.IsNullOrEmpty(locId) ? null : locId,
                UpdateTime = DateTime.UtcNow,
                Note = oldStatusId != dto.StatusId ? "Thay đổi trạng thái" : "Cập nhật vị trí"
            };
            await _statusHistoryRepo.AddAsync(history);
        }

        return NoContent();
    }

    private async Task ProcessAttachmentsAsync(string ticketId, List<AttachmentDto> attachments)
    {
        // 1. Get existing attachments for this ticket
        var existing = await _attachmentRepo.GetByFieldAsync("RmaTicketId", ticketId);

        // 2. Identify attachments to delete
        var currentIds = attachments.Where(a => !string.IsNullOrEmpty(a.Id)).Select(a => a.Id).ToHashSet();
        foreach (var ext in existing)
        {
            if (!currentIds.Contains(ext.Id))
            {
                // Delete file locally
                if (!string.IsNullOrEmpty(ext.FileUrl))
                {
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", ext.FileUrl.TrimStart('/'));
                    if (System.IO.File.Exists(filePath))
                    {
                        try { System.IO.File.Delete(filePath); } catch { /* Ignore */ }
                    }
                }
                // Delete from DB
                await _attachmentRepo.DeleteAsync(ext.Id);
            }
        }

        // 3. Save new attachments
        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }

        foreach (var att in attachments)
        {
            if (!string.IsNullOrEmpty(att.Base64Data))
            {
                try
                {
                    var fileBytes = Convert.FromBase64String(att.Base64Data);
                    var fileName = $"{Guid.NewGuid()}_{att.FileName}";
                    var filePath = Path.Combine(uploadsFolder, fileName);

                    await System.IO.File.WriteAllBytesAsync(filePath, fileBytes);

                    var entity = new Attachment
                    {
                        RmaTicketId = ticketId,
                        FileUrl = $"/uploads/{fileName}",
                        FileType = att.FileType ?? "CONDITION_PHOTO",
                        UploadedAt = DateTime.UtcNow
                    };
                    await _attachmentRepo.AddAsync(entity);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error saving attachment: {ex.Message}");
                }
            }
        }
    }

    private string? ExtractLocationFromStaffNote(string? staffNote)
    {
        if (string.IsNullOrEmpty(staffNote))
            return null;

        var startTag = "[Vị trí:";
        var startIndex = staffNote.IndexOf(startTag, StringComparison.OrdinalIgnoreCase);
        if (startIndex == -1)
            return null;

        var contentStart = startIndex + startTag.Length;
        var endIndex = staffNote.IndexOf("]", contentStart);
        if (endIndex == -1)
            return null;

        return staffNote.Substring(contentStart, endIndex - contentStart).Trim();
    }

    private async Task<string> ResolveLocationIdAsync(string locationName)
    {
        if (string.IsNullOrWhiteSpace(locationName))
            return string.Empty;

        var locations = await _locationRepo.GetAllAsync();
        var loc = locations.FirstOrDefault(l => l.Name.Equals(locationName, StringComparison.OrdinalIgnoreCase));
        if (loc != null)
        {
            return loc.Id;
        }

        // Create new location
        var newLoc = new Location { Name = locationName };
        var newId = await _locationRepo.AddAsync(newLoc);
        return newId;
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
