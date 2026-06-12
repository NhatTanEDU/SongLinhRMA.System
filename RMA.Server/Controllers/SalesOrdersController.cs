using Microsoft.AspNetCore.Mvc;
using RMA.Server.Entities;
using RMA.Server.Services;
using RMA.Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RMA.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SalesOrdersController : ControllerBase
    {
        private readonly FirestoreRepository<SalesOrder> _orderRepo;
        private readonly FirestoreRepository<Model> _modelRepo;
        private readonly FirestoreRepository<Device> _deviceRepo;
        private readonly FirestoreRepository<Customer> _customerRepo;

        public SalesOrdersController(
            FirestoreRepository<SalesOrder> orderRepo,
            FirestoreRepository<Model> modelRepo,
            FirestoreRepository<Device> deviceRepo,
            FirestoreRepository<Customer> customerRepo)
        {
            _orderRepo = orderRepo;
            _modelRepo = modelRepo;
            _deviceRepo = deviceRepo;
            _customerRepo = customerRepo;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<SalesOrderDto>>> Get()
        {
            var orders = await _orderRepo.GetAllAsync();
            var customers = (await _customerRepo.GetAllAsync()).ToDictionary(c => c.Id, c => c.Name);
            var models = (await _modelRepo.GetAllAsync()).ToDictionary(m => m.Id, m => m);

            var dtos = orders.Select(o => new SalesOrderDto
            {
                Id = o.Id,
                OrderCode = o.OrderCode,
                CustomerId = o.CustomerId,
                CustomerName = customers.ContainsKey(o.CustomerId) ? customers[o.CustomerId] : string.Empty,
                OrderDate = o.OrderDate,
                DeliveryDate = o.DeliveryDate,
                Status = o.Status,
                Details = o.Details.Select(d => new OrderDetailDto
                {
                    ModelId = d.ModelId,
                    ModelName = models.ContainsKey(d.ModelId) ? models[d.ModelId].ModelName : string.Empty,
                    Quantity = d.Quantity,
                    WarrantyMonths = d.WarrantyMonths,
                    Note = d.Note,
                    IsSerialRequired = models.ContainsKey(d.ModelId) && models[d.ModelId].IsSerialRequired
                }).ToList()
            }).OrderByDescending(o => o.OrderDate).ToList();

            return Ok(dtos);
        }

        [HttpPost]
        public async Task<ActionResult<SalesOrderDto>> Post([FromBody] SalesOrderCreateDto dto)
        {
            var entity = new SalesOrder
            {
                OrderCode = $"SO-{DateTime.UtcNow:yyyyMMddHHmmss}",
                CustomerId = dto.CustomerId,
                OrderDate = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc),
                Status = "Pending"
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
                        WarrantyMonths = model.WarrantyMonths,
                        Note = detail.Note
                    });
                }
            }

            var newId = await _orderRepo.AddAsync(entity);
            entity.Id = newId;

            return Ok(entity);
        }

        [HttpPost("confirm-delivery")]
        public async Task<IActionResult> ConfirmDelivery([FromBody] ConfirmDeliveryDto dto)
        {
            var order = await _orderRepo.GetByIdAsync(dto.OrderId);
            if (order == null) return NotFound("Order not found");
            if (order.Status == "Delivered") return BadRequest("Order already delivered");

            var models = (await _modelRepo.GetAllAsync()).ToDictionary(m => m.Id, m => m);

            // Validation
            foreach (var detail in order.Details)
            {
                var model = models[detail.ModelId];
                if (model.IsSerialRequired)
                {
                    if (!dto.SerialNumbersByModel.ContainsKey(detail.ModelId) || 
                        dto.SerialNumbersByModel[detail.ModelId].Count != detail.Quantity)
                    {
                        return BadRequest($"Model {model.ModelName} requires exactly {detail.Quantity} serial numbers.");
                    }
                }
            }

            // Processing
            var purchaseDate = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);

            foreach (var detail in order.Details)
            {
                var model = models[detail.ModelId];
                var serials = new List<string>();

                if (model.IsSerialRequired)
                {
                    serials = dto.SerialNumbersByModel[detail.ModelId];
                }
                else
                {
                    for (int i = 0; i < detail.Quantity; i++)
                    {
                        serials.Add($"SYS-CABLE-{order.Id}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}");
                    }
                }

                foreach (var sn in serials)
                {
                    var device = new Device
                    {
                        SerialNumber = sn,
                        CustomerId = order.CustomerId,
                        ModelId = detail.ModelId,
                        PurchaseDate = purchaseDate,
                        WarrantyExpiry = purchaseDate.AddMonths(detail.WarrantyMonths),
                        OrderId = order.Id
                    };
                    await _deviceRepo.AddAsync(device);
                }

                // Deduct stock
                model.StockQuantity -= detail.Quantity;
                if (model.StockQuantity < 0) model.StockQuantity = 0;
                await _modelRepo.UpdateAsync(model.Id, model);
            }

            order.Status = "Delivered";
            order.DeliveryDate = purchaseDate;
            await _orderRepo.UpdateAsync(order.Id, order);

            return Ok();
        }
    }
}
