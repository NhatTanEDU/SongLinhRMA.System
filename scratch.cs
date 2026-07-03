[HttpPut("{id}/admin-override")]
[Authorize(Roles = "Admin")]
public async Task<IActionResult> AdminOverride(string id, [FromBody] AdminOverrideDto dto)
{
    if (id != dto.OrderId) return BadRequest("Order ID mismatch.");
    try
    {
        var order = await _orderRepo.GetByIdAsync(id);
        if (order == null) return NotFound("Order not found");

        var batch = _firestoreDb.StartBatch();

        // 1. Revert old stock if order was delivered
        if (order.Status == "Delivered" || order.Status == "Completed")
        {
            foreach (var detail in order.Details)
            {
                var modelDoc = await _firestoreDb.Collection("models").Document(detail.ModelId).GetSnapshotAsync();
                if (modelDoc.Exists)
                {
                    var model = modelDoc.ConvertTo<RMA.Server.Entities.Model>();
                    model.StockQuantity += detail.Quantity;
                    batch.Set(modelDoc.Reference, model, SetOptions.Overwrite);
                }
            }
            
            // Delete old devices
            var oldDevicesSnapshot = await _firestoreDb.Collection("devices").WhereEqualTo("OrderId", id).GetSnapshotAsync();
            foreach (var doc in oldDevicesSnapshot.Documents)
            {
                batch.Delete(doc.Reference);
            }
        }

        // 2. Update order properties
        order.CustomerId = dto.CustomerId;
        order.OrderCode = dto.OrderCode;
        order.DeliveryDate = dto.DeliveryDate;
        order.Details = dto.Details.Select(d => new OrderDetail
        {
            ModelId = d.ModelId,
            Quantity = d.Quantity,
            WarrantyMonths = d.WarrantyMonths ?? 0,
            DeviceSpecs = d.DeviceSpecs,
            Note = d.Note
        }).ToList();

        // 3. Create new devices and deduct new stock if order was delivered
        if (order.Status == "Delivered" || order.Status == "Completed")
        {
            var purchaseDate = order.DeliveryDate ?? DateTime.UtcNow;

            foreach (var detail in order.Details)
            {
                var modelDoc = await _firestoreDb.Collection("models").Document(detail.ModelId).GetSnapshotAsync();
                if (modelDoc.Exists)
                {
                    var model = modelDoc.ConvertTo<RMA.Server.Entities.Model>();
                    model.StockQuantity -= detail.Quantity;
                    if (model.StockQuantity < 0) model.StockQuantity = 0;
                    batch.Set(modelDoc.Reference, model, SetOptions.Overwrite);
                }

                var serials = new List<string>();
                if (dto.SerialNumbersByModel.TryGetValue(detail.ModelId, out var clientSerials))
                {
                    for (int i = 0; i < clientSerials.Count; i++)
                    {
                        var sn = clientSerials[i];
                        if (string.IsNullOrWhiteSpace(sn)) sn = "KHÔNG CÓ S/N";
                        serials.Add(sn);
                    }
                }
                else
                {
                    for (int i = 0; i < detail.Quantity; i++)
                    {
                        serials.Add("KHÔNG CÓ S/N");
                    }
                }

                foreach (var sn in serials)
                {
                    var normalizedSn = string.IsNullOrWhiteSpace(sn) || sn.Equals("KHÔNG CÓ S/N", StringComparison.OrdinalIgnoreCase) ? "" : sn.Trim().ToUpper();
                    var deviceId = string.IsNullOrEmpty(normalizedSn) ? $"NOSERIAL-{Guid.NewGuid()}" : normalizedSn;

                    // Note: If new S/N conflicts with another order's device, we might overwrite or fail. 
                    // In a batch, we just set. If we want to check for conflicts:
                    if (!deviceId.StartsWith("NOSERIAL-"))
                    {
                        var existing = await _deviceRepo.GetByIdAsync(deviceId);
                        if (existing != null && existing.OrderId != order.Id)
                        {
                            var customer = await _customerRepo.GetByIdAsync(existing.CustomerId);
                            var customerName = customer?.Name ?? "khách hàng khác";
                            return BadRequest($"Không thể ghi đè. Số S/N '{sn}' đã tồn tại và thuộc sở hữu của {customerName}!");
                        }
                    }

                    var device = new Device
                    {
                        Id = deviceId,
                        SerialNumber = sn,
                        CustomerId = order.CustomerId,
                        ModelId = detail.ModelId,
                        PurchaseDate = purchaseDate,
                        WarrantyExpiry = purchaseDate.AddMonths(detail.WarrantyMonths),
                        OrderId = order.Id,
                        OrderCode = order.OrderCode
                    };
                    
                    batch.Set(_firestoreDb.Collection("devices").Document(device.Id), device);
                }
            }
        }

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
