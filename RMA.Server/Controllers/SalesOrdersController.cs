using Microsoft.AspNetCore.Mvc;
using RMA.Server.Entities;
using RMA.Server.Services;
using RMA.Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Cloud.Firestore;
using Microsoft.AspNetCore.SignalR;
using RMA.Server.Hubs;

using Microsoft.AspNetCore.Authorization;

namespace RMA.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SalesOrdersController : ControllerBase
    {
        private readonly FirestoreRepository<SalesOrder> _orderRepo;
        private readonly FirestoreRepository<Model> _modelRepo;
        private readonly FirestoreRepository<Device> _deviceRepo;
        private readonly FirestoreRepository<Customer> _customerRepo;
        private readonly FirestoreDb _firestoreDb;
        private readonly IHubContext<SalesHub> _hubContext;

        public SalesOrdersController(
            FirestoreRepository<SalesOrder> orderRepo,
            FirestoreRepository<Model> modelRepo,
            FirestoreRepository<Device> deviceRepo,
            FirestoreRepository<Customer> customerRepo,
            FirestoreDb firestoreDb,
            IHubContext<SalesHub> hubContext)
        {
            _orderRepo = orderRepo;
            _modelRepo = modelRepo;
            _deviceRepo = deviceRepo;
            _customerRepo = customerRepo;
            _firestoreDb = firestoreDb;
            _hubContext = hubContext;
        }

        [HttpPost]
        public async Task<ActionResult<SalesOrderDto>> Post([FromBody] SalesOrderCreateDto dto)
        {
            try
            {
                 var allOrders = await _orderRepo.GetAllAsync();
                 int nextIndex = allOrders.Count + 1;
                 var entity = new SalesOrder
                 {
                     OrderCode = $"SO-{nextIndex:D4}",
                     CustomerId = dto.CustomerId,
                     OrderDate = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc),
                     Status = "Pending",
                     SalesNote = dto.SalesNote
                 };
 
                 foreach (var detail in dto.Details)
                 {
                     var model = await _modelRepo.GetByIdAsync(detail.ModelId);
                     if (model != null)
                     {
                         entity.Details.Add(new OrderDetail
                         {
                             ModelId = detail.ModelId,
                             Quantity = detail.Quantity,
                             WarrantyMonths = (detail.WarrantyMonths.HasValue && detail.WarrantyMonths.Value > 0) ? detail.WarrantyMonths.Value : model.WarrantyMonths,
                             DeviceSpecs = detail.DeviceSpecs ?? string.Empty,
                             Note = detail.Note
                         });
                     }
                 }

                var newId = await _orderRepo.AddAsync(entity);
                entity.Id = newId;

                await _hubContext.Clients.All.SendAsync("OrderStateChanged");
                return Ok(entity);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<SalesOrderDto>>> Get([FromQuery] SalesOrderQueryDto query)
        {
            try
            {
                Response.Headers.Append("Cache-Control", "no-cache, no-store, must-revalidate");
                var orders = await _orderRepo.GetAllAsync();
                var customers = (await _customerRepo.GetAllAsync()).ToDictionary(c => c.Id, c => c);
                var models = await GetAndFixModelsAsync();

                // Apply CustomerId filter
                if (!string.IsNullOrEmpty(query.CustomerId))
                {
                    orders = orders.Where(o => o.CustomerId == query.CustomerId).ToList();
                }

                // Apply Status filter
                if (!string.IsNullOrEmpty(query.Status))
                {
                    orders = orders.Where(o => o.Status.Equals(query.Status, StringComparison.OrdinalIgnoreCase)).ToList();
                }

                // Apply Date filters
                if (query.StartDate.HasValue)
                {
                    var start = DateTime.SpecifyKind(query.StartDate.Value.Date, DateTimeKind.Utc);
                    orders = orders.Where(o => o.OrderDate >= start).ToList();
                }
                if (query.EndDate.HasValue)
                {
                    var end = DateTime.SpecifyKind(query.EndDate.Value.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc);
                    orders = orders.Where(o => o.OrderDate <= end).ToList();
                }

                // Apply SearchTerm filter
                if (!string.IsNullOrEmpty(query.SearchTerm))
                {
                    var matchingOrderIds = new HashSet<string>();
                    var devices = await _deviceRepo.GetAllAsync();
                    foreach (var dev in devices)
                    {
                        if (!string.IsNullOrEmpty(dev.SerialNumber) && 
                            dev.SerialNumber.Contains(query.SearchTerm, StringComparison.OrdinalIgnoreCase) && 
                            !string.IsNullOrEmpty(dev.OrderId))
                        {
                            matchingOrderIds.Add(dev.OrderId);
                        }
                    }

                    orders = orders.Where(o => 
                        (!string.IsNullOrEmpty(o.OrderCode) && o.OrderCode.Contains(query.SearchTerm, StringComparison.OrdinalIgnoreCase)) ||
                        (customers.ContainsKey(o.CustomerId) && customers[o.CustomerId].Name.Contains(query.SearchTerm, StringComparison.OrdinalIgnoreCase)) ||
                        (!string.IsNullOrEmpty(o.SalesNote) && o.SalesNote.Contains(query.SearchTerm, StringComparison.OrdinalIgnoreCase)) ||
                        (!string.IsNullOrEmpty(o.Note) && o.Note.Contains(query.SearchTerm, StringComparison.OrdinalIgnoreCase)) ||
                        matchingOrderIds.Contains(o.Id) ||
                        o.Details.Any(d => 
                            (models.ContainsKey(d.ModelId) && models[d.ModelId].ModelName.Contains(query.SearchTerm, StringComparison.OrdinalIgnoreCase)) ||
                            (!string.IsNullOrEmpty(d.Note) && d.Note.Contains(query.SearchTerm, StringComparison.OrdinalIgnoreCase))
                        )
                    ).ToList();
                }

                var dtos = orders.Select(o => new SalesOrderDto
                {
                    Id = o.Id,
                    OrderCode = o.OrderCode,
                    CustomerId = o.CustomerId,
                    CustomerName = customers.ContainsKey(o.CustomerId) ? customers[o.CustomerId].Name : string.Empty,
                    CustomerAvatarUrl = customers.ContainsKey(o.CustomerId) ? (customers[o.CustomerId].AvatarUrl ?? string.Empty) : string.Empty,
                    OrderDate = o.OrderDate,
                    DeliveryDate = o.DeliveryDate,
                    Status = o.Status,
                    SalesNote = o.SalesNote,
                    Note = o.Note,
                    LastUpdated = o.LastUpdated,
                    UpdatedBy = o.UpdatedBy,
                    Details = o.Details.Select(d => new OrderDetailDto
                    {
                        ModelId = d.ModelId,
                        ModelName = models.ContainsKey(d.ModelId) ? models[d.ModelId].ModelName : string.Empty,
                        Quantity = d.Quantity,
                        WarrantyMonths = d.WarrantyMonths,
                        DeviceSpecs = d.DeviceSpecs ?? string.Empty,
                        Note = d.Note,
                        IsSerialRequired = models.ContainsKey(d.ModelId) && models[d.ModelId].IsSerialRequired
                    }).ToList()
                }).OrderByDescending(o => o.OrderDate).ToList();
 
                return Ok(dtos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("pending")]
        public async Task<ActionResult<IEnumerable<SalesOrderDto>>> GetPending()
        {
            try
            {
                Response.Headers.Append("Cache-Control", "no-cache, no-store, must-revalidate");
                var orders = await _orderRepo.GetByFieldAsync("Status", "Pending");
                var customers = (await _customerRepo.GetAllAsync()).ToDictionary(c => c.Id, c => c);
                var models = await GetAndFixModelsAsync();
 
                var dtos = orders.Select(o => new SalesOrderDto
                {
                    Id = o.Id,
                    OrderCode = o.OrderCode,
                    CustomerId = o.CustomerId,
                    CustomerName = customers.ContainsKey(o.CustomerId) ? customers[o.CustomerId].Name : string.Empty,
                    CustomerAvatarUrl = customers.ContainsKey(o.CustomerId) ? (customers[o.CustomerId].AvatarUrl ?? string.Empty) : string.Empty,
                    OrderDate = o.OrderDate,
                    DeliveryDate = o.DeliveryDate,
                    Status = o.Status,
                    SalesNote = o.SalesNote,
                    Note = o.Note,
                    LastUpdated = o.LastUpdated,
                    UpdatedBy = o.UpdatedBy,
                    Details = o.Details.Select(d => new OrderDetailDto
                    {
                        ModelId = d.ModelId,
                        ModelName = models.ContainsKey(d.ModelId) ? models[d.ModelId].ModelName : string.Empty,
                        Quantity = d.Quantity,
                        WarrantyMonths = d.WarrantyMonths,
                        DeviceSpecs = d.DeviceSpecs ?? string.Empty,
                        Note = d.Note,
                        IsSerialRequired = models.ContainsKey(d.ModelId) && models[d.ModelId].IsSerialRequired
                    }).ToList()
                }).OrderByDescending(o => o.OrderDate).ToList();
 
                return Ok(dtos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(string id, [FromBody] SalesOrderCreateDto dto)
        {
            try
            {
                var order = await _orderRepo.GetByIdAsync(id);
                if (order == null) return NotFound("Order not found");
                if (order.Status != "Pending") return BadRequest("Only pending orders can be modified.");

                order.CustomerId = dto.CustomerId;
                order.SalesNote = dto.SalesNote;
                order.Details.Clear();

                foreach (var detail in dto.Details)
                {
                    var model = await _modelRepo.GetByIdAsync(detail.ModelId);
                    if (model != null)
                    {
                        order.Details.Add(new OrderDetail
                        {
                            ModelId = detail.ModelId,
                            Quantity = detail.Quantity,
                            WarrantyMonths = (detail.WarrantyMonths.HasValue && detail.WarrantyMonths.Value > 0) ? detail.WarrantyMonths.Value : model.WarrantyMonths,
                            DeviceSpecs = detail.DeviceSpecs ?? string.Empty,
                            Note = detail.Note
                        });
                    }
                }

                await _orderRepo.UpdateAsync(id, order);
                await _hubContext.Clients.All.SendAsync("OrderStateChanged");
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("{id}/business-info")]
        public async Task<IActionResult> UpdateBusinessInfo(string id, [FromBody] SalesOrderBusinessInfoDto dto)
        {
            if (dto == null) return BadRequest("Yêu cầu không hợp lệ.");

            try
            {
                var order = await _orderRepo.GetByIdAsync(id);
                if (order == null) return NotFound("Order not found");

                order.OrderCode = dto.OrderCode;
                order.Note = dto.Note;
                order.LastUpdated = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
                order.UpdatedBy = User.Identity?.Name ?? "Unknown";

                await _orderRepo.UpdateAsync(id, order);
                await _hubContext.Clients.All.SendAsync("OrderStateChanged");
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("{id}/status")]
        [Authorize(Roles = "Admin,Tech")]
        public async Task<IActionResult> UpdateStatus(string id, [FromBody] string status)
        {
            try
            {
                var order = await _orderRepo.GetByIdAsync(id);
                if (order == null) return NotFound("Order not found");

                order.Status = status;
                await _orderRepo.UpdateAsync(id, order);
                await _hubContext.Clients.All.SendAsync("OrderStateChanged");
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("confirm-delivery")]
        [Authorize(Roles = "Admin,Tech")]
        public async Task<IActionResult> ConfirmDelivery([FromBody] ConfirmDeliveryDto dto)
        {
            if (dto == null || string.IsNullOrEmpty(dto.OrderId))
            {
                return BadRequest("Yêu cầu không hợp lệ.");
            }

            try
            {
                var order = await _orderRepo.GetByIdAsync(dto.OrderId);
                if (order == null)
                {
                    return BadRequest("Đơn hàng không tồn tại.");
                }
                if (order.Status != "Pending" && order.Status != "Delivering")
                {
                    return BadRequest("Đơn hàng đã được xác nhận giao thành công trước đó, vui lòng tải lại trang!");
                }

                var models = await GetAndFixModelsAsync();

                WriteBatch batch = _firestoreDb.StartBatch();

                var purchaseDate = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);

                foreach (var detail in order.Details)
                {
                    if (!models.ContainsKey(detail.ModelId)) continue;
                    
                    var model = models[detail.ModelId];
                    var serials = new List<string>();

                    if (model.IsSerialRequired)
                    {
                        if (dto.SerialNumbersByModel == null || 
                            !dto.SerialNumbersByModel.ContainsKey(detail.ModelId) || 
                            dto.SerialNumbersByModel[detail.ModelId] == null ||
                            dto.SerialNumbersByModel[detail.ModelId].Count != detail.Quantity)
                        {
                            return BadRequest($"Model {model.ModelName} requires exactly {detail.Quantity} serial numbers.");
                        }
                        
                        var clientSerials = dto.SerialNumbersByModel[detail.ModelId];
                        for (int i = 0; i < clientSerials.Count; i++)
                        {
                            var sn = clientSerials[i];
                            if (string.IsNullOrWhiteSpace(sn))
                            {
                                sn = $"MISSING-SN-{order.Id}-{Guid.NewGuid().ToString().Substring(0, 8)}";
                            }
                            serials.Add(sn);
                        }
                    }
                    else
                    {
                        for (int i = 0; i < detail.Quantity; i++)
                        {
                            serials.Add($"SYS-{model.Id}-{order.Id}-{i}");
                        }
                    }

                    foreach (var sn in serials)
                    {
                        var device = new Device
                        {
                            Id = Guid.NewGuid().ToString(),
                            SerialNumber = sn,
                            CustomerId = order.CustomerId,
                            ModelId = detail.ModelId,
                            PurchaseDate = purchaseDate,
                            WarrantyExpiry = purchaseDate.AddMonths(detail.WarrantyMonths),
                            OrderId = order.Id
                        };
                        
                        var deviceDocRef = _firestoreDb.Collection("devices").Document(device.Id);
                        batch.Set(deviceDocRef, device);
                    }

                    model.StockQuantity -= detail.Quantity;
                    if (model.StockQuantity < 0) model.StockQuantity = 0;

                    var modelDocRef = _firestoreDb.Collection("models").Document(model.Id);
                    batch.Set(modelDocRef, model, SetOptions.Overwrite);
                }

                order.Status = "Delivered";
                order.DeliveryDate = purchaseDate;
                var orderDocRef = _firestoreDb.Collection("sales_orders").Document(order.Id);
                batch.Set(orderDocRef, order, SetOptions.Overwrite);

                await batch.CommitAsync();

                await _hubContext.Clients.All.SendAsync("OrderStateChanged");
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                var order = await _orderRepo.GetByIdAsync(id);
                if (order == null) return NotFound("Order not found");
                if (order.Status != "Pending") return BadRequest("Only pending orders can be deleted.");

                await _orderRepo.DeleteAsync(id);
                await _hubContext.Clients.All.SendAsync("OrderStateChanged");
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        private async Task<Dictionary<string, Model>> GetAndFixModelsAsync()
        {
            var modelsList = await _modelRepo.GetAllAsync();
            bool updated = false;
            foreach (var m in modelsList)
            {
                var lowerName = m.ModelName.ToLower();
                if ((lowerName.Contains("access point") || lowerName.Contains("laptop") || lowerName.Contains("ups") || lowerName.Contains("switch") || lowerName.Contains("dell") || lowerName.Contains("macbook") || lowerName.Contains("asus")) && !m.IsSerialRequired)
                {
                    m.IsSerialRequired = true;
                    await _modelRepo.UpdateAsync(m.Id, m);
                    updated = true;
                }
            }
            if (updated)
            {
                modelsList = await _modelRepo.GetAllAsync();
            }
            return modelsList.ToDictionary(m => m.Id, m => m);
        }
    }
}
