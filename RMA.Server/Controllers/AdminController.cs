using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Memory;
using Google.Cloud.Firestore;
using Microsoft.AspNetCore.SignalR;
using RMA.Server.Entities;
using RMA.Server.Services;
using RMA.Shared.DTOs;
using RMA.Server.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;

namespace RMA.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Requires authentication by default. Custom route restrictions are checked per action or roles.
    public class AdminController : ControllerBase
    {
        private readonly FirestoreRepository<UserAccount> _userRepo;
        private readonly FirestoreRepository<SystemSetting> _settingRepo;
        private readonly FirestoreRepository<AuditLog> _auditLogRepo;
        private readonly FirestoreRepository<SalesOrder> _orderRepo;
        private readonly FirestoreRepository<Model> _modelRepo;
        private readonly FirestoreRepository<Device> _deviceRepo;
        private readonly FirestoreRepository<RmaTicket> _ticketRepo;
        private readonly FirestoreRepository<Location> _locationRepo;
        private readonly FirestoreRepository<StatusHistory> _statusHistoryRepo;
        private readonly FirestoreDb _firestoreDb;
        private readonly IMemoryCache _cache;
        private readonly IHubContext<SalesHub> _hubContext;

        public AdminController(
            FirestoreRepository<UserAccount> userRepo,
            FirestoreRepository<SystemSetting> settingRepo,
            FirestoreRepository<AuditLog> auditLogRepo,
            FirestoreRepository<SalesOrder> orderRepo,
            FirestoreRepository<Model> modelRepo,
            FirestoreRepository<Device> deviceRepo,
            FirestoreRepository<RmaTicket> ticketRepo,
            FirestoreRepository<Location> locationRepo,
            FirestoreRepository<StatusHistory> statusHistoryRepo,
            FirestoreDb firestoreDb,
            IMemoryCache cache,
            IHubContext<SalesHub> hubContext)
        {
            _userRepo = userRepo;
            _settingRepo = settingRepo;
            _auditLogRepo = auditLogRepo;
            _orderRepo = orderRepo;
            _modelRepo = modelRepo;
            _deviceRepo = deviceRepo;
            _ticketRepo = ticketRepo;
            _locationRepo = locationRepo;
            _statusHistoryRepo = statusHistoryRepo;
            _firestoreDb = firestoreDb;
            _cache = cache;
            _hubContext = hubContext;
        }

        // User & Role Management endpoints moved to UsersController.cs

        #region System Settings

        [HttpGet("settings")]
        public async Task<ActionResult<IEnumerable<SystemSettingDto>>> GetSettings()
        {
            try
            {
                var settings = await _settingRepo.GetAllAsync();
                
                // Fallback / seed initial settings if not populated
                var yellowSetting = settings.FirstOrDefault(s => s.Key == "SlaYellowDays");
                var redSetting = settings.FirstOrDefault(s => s.Key == "SlaRedDays");

                var result = new List<SystemSettingDto>();

                if (yellowSetting == null)
                {
                    var newYellow = new SystemSetting { Key = "SlaYellowDays", Value = "10" };
                    newYellow.Id = await _settingRepo.AddAsync(newYellow);
                    result.Add(new SystemSettingDto { Key = "SlaYellowDays", Value = "10" });
                }
                else
                {
                    result.Add(new SystemSettingDto { Key = yellowSetting.Key, Value = yellowSetting.Value });
                }

                if (redSetting == null)
                {
                    var newRed = new SystemSetting { Key = "SlaRedDays", Value = "14" };
                    newRed.Id = await _settingRepo.AddAsync(newRed);
                    result.Add(new SystemSettingDto { Key = "SlaRedDays", Value = "14" });
                }
                else
                {
                    result.Add(new SystemSettingDto { Key = redSetting.Key, Value = redSetting.Value });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("settings")]
        public async Task<IActionResult> SaveSettings([FromBody] List<SystemSettingDto> settings)
        {
            try
            {
                var dbSettings = await _settingRepo.GetAllAsync();
                var operatorName = User.Identity?.Name ?? "Unknown Admin";

                var oldVal = JsonSerializer.Serialize(dbSettings.Select(s => new SystemSettingDto { Key = s.Key, Value = s.Value }).ToList());

                foreach (var dto in settings)
                {
                    var existing = dbSettings.FirstOrDefault(s => s.Key == dto.Key);
                    if (existing != null)
                    {
                        existing.Value = dto.Value;
                        await _settingRepo.UpdateAsync(existing.Id, existing);
                    }
                    else
                    {
                        var newSet = new SystemSetting { Key = dto.Key, Value = dto.Value };
                        await _settingRepo.AddAsync(newSet);
                    }

                    // Invalidate Server Memory Cache immediately for this key
                    _cache.Remove($"setting_{dto.Key}");
                }

                // Also clear the cached settings list just in case
                _cache.Remove("sla_settings_cached");

                var newVal = JsonSerializer.Serialize(settings);

                await LogAdminActionAsync(
                    action: "UPDATE_SETTINGS",
                    details: "Đã cập nhật các cấu hình hệ thống (SLA warning thresholds).",
                    oldValue: oldVal,
                    newValue: newVal,
                    user: operatorName
                );

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        #endregion

        #region Audit Logs

        [HttpGet("logs")]
        public async Task<ActionResult<IEnumerable<AuditLogDto>>> GetLogs()
        {
            try
            {
                var logs = await _auditLogRepo.GetAllAsync();
                var dtos = logs.Select(l => new AuditLogDto
                {
                    Id = l.Id,
                    Action = l.Action,
                    User = l.User,
                    Timestamp = l.Timestamp,
                    Details = l.Details,
                    OldValue = l.OldValue,
                    NewValue = l.NewValue
                }).OrderByDescending(l => l.Timestamp).ToList();

                return Ok(dtos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("firestore-metrics")]
        public ActionResult GetFirestoreMetrics()
        {
            return Ok(new
            {
                TotalReads = FirestoreMetrics.TotalReads,
                TotalWrites = FirestoreMetrics.TotalWrites,
                DailyReadLimit = 50000,
                DailyWriteLimit = 20000
            });
        }

        #endregion

        #region Sales Order Emergency Bypass

        [HttpPost("salesorders/bypass")]
        public async Task<IActionResult> BypassSalesOrder([FromBody] BypassDeliveryDto dto)
        {
            if (dto == null || string.IsNullOrEmpty(dto.OrderId))
            {
                return BadRequest("Yêu cầu Bypass không hợp lệ.");
            }

            try
            {
                var order = await _orderRepo.GetByIdAsync(dto.OrderId);
                if (order == null) return NotFound("Không tìm thấy đơn hàng");
                if (order.Status == "Delivered") return BadRequest("Đơn hàng này đã được xác nhận giao từ trước.");

                var operatorName = User.Identity?.Name ?? "Unknown Admin";
                var oldOrderJson = JsonSerializer.Serialize(order);

                // Load all models to check serial requirement and decrement stocks
                var models = (await _modelRepo.GetAllAsync()).ToDictionary(m => m.Id, m => m);

                WriteBatch batch = _firestoreDb.StartBatch();
                var bypassDate = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);

                foreach (var detail in order.Details)
                {
                    if (!models.TryGetValue(detail.ModelId, out var model)) continue;

                    // Generate bypass serial numbers since we bypass manual verification
                    for (int i = 0; i < detail.Quantity; i++)
                    {
                        var sn = $"BYPASS-SO-{order.OrderCode}-{model.Brand ?? "SL"}-{i + 1}";
                        var device = new Device
                        {
                            Id = Guid.NewGuid().ToString(),
                            SerialNumber = sn,
                            CustomerId = order.CustomerId,
                            ModelId = detail.ModelId,
                            PurchaseDate = bypassDate,
                            WarrantyExpiry = bypassDate.AddMonths(detail.WarrantyMonths),
                            OrderId = order.Id
                        };

                        var deviceDocRef = _firestoreDb.Collection("devices").Document(device.Id);
                        batch.Set(deviceDocRef, device);
                    }

                    // Decrement stock
                    model.StockQuantity -= detail.Quantity;
                    if (model.StockQuantity < 0) model.StockQuantity = 0;

                    var modelDocRef = _firestoreDb.Collection("models").Document(model.Id);
                    batch.Set(modelDocRef, model, SetOptions.Overwrite);
                }

                // Update order to delivered
                order.Status = "Delivered";
                order.DeliveryDate = bypassDate;
                order.SalesNote = string.IsNullOrWhiteSpace(order.SalesNote)
                    ? $"[BYPASSED] {dto.Reason}"
                    : $"{order.SalesNote} | [BYPASSED] {dto.Reason}";

                var orderDocRef = _firestoreDb.Collection("sales_orders").Document(order.Id);
                batch.Set(orderDocRef, order, SetOptions.Overwrite);

                await batch.CommitAsync();

                var newOrderJson = JsonSerializer.Serialize(order);

                // Write Audit Log
                await LogAdminActionAsync(
                    action: "BYPASS_DELIVERY",
                    details: $"Admin cưỡng chế xác nhận giao hàng cho đơn '{order.OrderCode}'. Lý do: '{dto.Reason}'. Tự động kích hoạt S/N Bypass.",
                    oldValue: oldOrderJson,
                    newValue: newOrderJson,
                    user: operatorName
                );

                // Notify UI Clients
                await _hubContext.Clients.All.SendAsync("OrderStateChanged");

                return Ok(new { message = $"Đơn hàng {order.OrderCode} đã được cưỡng chế xác nhận thành công!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        #endregion

        #region RMA Ticket Technical Dispatch

        [HttpPost("rmatickets/dispatch")]
        public async Task<IActionResult> DispatchRmaTicket([FromBody] DispatchTicketDto dto)
        {
            if (dto == null || string.IsNullOrEmpty(dto.TicketId) || string.IsNullOrEmpty(dto.TechnicianUsername))
            {
                return BadRequest("Yêu cầu điều phối không hợp lệ.");
            }

            try
            {
                var ticket = await _ticketRepo.GetByIdAsync(dto.TicketId);
                if (ticket == null) return NotFound("Không tìm thấy phiếu bảo hành");

                var operatorName = User.Identity?.Name ?? "Unknown Admin";
                var oldTicketJson = JsonSerializer.Serialize(ticket);

                // Dispatch assignment means:
                // 1. Assign/append to technician info in staff note or location log
                // 2. Add StatusHistory for dispatching
                ticket.StaffNote = string.IsNullOrWhiteSpace(ticket.StaffNote)
                    ? $"[Phân công kỹ thuật: {dto.TechnicianUsername}] {dto.Note}"
                    : $"{ticket.StaffNote}\n[Phân công kỹ thuật: {dto.TechnicianUsername}] {dto.Note}";

                await _ticketRepo.UpdateAsync(ticket.Id, ticket);

                var newTicketJson = JsonSerializer.Serialize(ticket);

                // Create StatusHistory for dispatching
                var history = new StatusHistory
                {
                    RmaTicketId = ticket.Id,
                    StatusId = ticket.StatusId,
                    LocationId = null,
                    UpdateTime = DateTime.UtcNow,
                    Note = $"Admin điều phối phiếu sửa chữa cho kỹ thuật viên: '{dto.TechnicianUsername}'"
                };
                await _statusHistoryRepo.AddAsync(history);

                // Audit log
                await LogAdminActionAsync(
                    action: "DISPATCH_TICKET",
                    details: $"Điều phối phiếu bảo hành '{ticket.Id}' cho KT: '{dto.TechnicianUsername}'. Ghi chú: '{dto.Note}'",
                    oldValue: oldTicketJson,
                    newValue: newTicketJson,
                    user: operatorName
                );

                return Ok(new { message = $"Phân công phiếu thành công cho kỹ thuật viên '{dto.TechnicianUsername}'!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        #endregion

        #region Helper Method

        private async Task LogAdminActionAsync(string action, string details, string oldValue, string newValue, string user)
        {
            var auditLog = new AuditLog
            {
                Action = action,
                User = user,
                Timestamp = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc),
                Details = details,
                OldValue = oldValue,
                NewValue = newValue
            };

            await _auditLogRepo.AddAsync(auditLog);
        }

        #endregion
    }
}
