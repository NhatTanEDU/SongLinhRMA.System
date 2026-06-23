using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RMA.Server.Entities;
using RMA.Server.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace RMA.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Bearer,Local", Roles = "Admin")]
    public class BackupController : ControllerBase
    {
        private readonly FirestoreDb _firestoreDb;
        private readonly FirestoreRepository<UserAccount> _userRepo;
        private readonly FirestoreRepository<Device> _deviceRepo;
        private readonly FirestoreRepository<RmaTicket> _ticketRepo;
        private readonly FirestoreRepository<StatusHistory> _historyRepo;
        private readonly FirestoreRepository<Attachment> _attachmentRepo;
        private readonly FirestoreRepository<Customer> _customerRepo;
        private readonly FirestoreRepository<Category> _categoryRepo;
        private readonly FirestoreRepository<Model> _modelRepo;
        private readonly FirestoreRepository<Vendor> _vendorRepo;
        private readonly FirestoreRepository<StatusMaster> _statusRepo;
        private readonly FirestoreRepository<Location> _locationRepo;
        private readonly FirestoreRepository<SalesOrder> _salesOrderRepo;
        private readonly FirestoreRepository<SystemSetting> _settingRepo;
        private readonly FirestoreRepository<AuditLog> _auditLogRepo;

        public BackupController(
            FirestoreDb firestoreDb,
            FirestoreRepository<UserAccount> userRepo,
            FirestoreRepository<Device> deviceRepo,
            FirestoreRepository<RmaTicket> ticketRepo,
            FirestoreRepository<StatusHistory> historyRepo,
            FirestoreRepository<Attachment> attachmentRepo,
            FirestoreRepository<Customer> customerRepo,
            FirestoreRepository<Category> categoryRepo,
            FirestoreRepository<Model> modelRepo,
            FirestoreRepository<Vendor> vendorRepo,
            FirestoreRepository<StatusMaster> statusRepo,
            FirestoreRepository<Location> locationRepo,
            FirestoreRepository<SalesOrder> salesOrderRepo,
            FirestoreRepository<SystemSetting> settingRepo,
            FirestoreRepository<AuditLog> auditLogRepo)
        {
            _firestoreDb = firestoreDb;
            _userRepo = userRepo;
            _deviceRepo = deviceRepo;
            _ticketRepo = ticketRepo;
            _historyRepo = historyRepo;
            _attachmentRepo = attachmentRepo;
            _customerRepo = customerRepo;
            _categoryRepo = categoryRepo;
            _modelRepo = modelRepo;
            _vendorRepo = vendorRepo;
            _statusRepo = statusRepo;
            _locationRepo = locationRepo;
            _salesOrderRepo = salesOrderRepo;
            _settingRepo = settingRepo;
            _auditLogRepo = auditLogRepo;
        }

        [HttpGet("export")]
        public async Task<IActionResult> Export()
        {
            try
            {
                // Fetch component checklists dynamically from collection "component_checklists"
                var checklistList = new List<Dictionary<string, object>>();
                try
                {
                    var snapshot = await _firestoreDb.Collection("component_checklists").GetSnapshotAsync();
                    foreach (var doc in snapshot.Documents)
                    {
                        if (doc.Exists)
                        {
                            var data = doc.ToDictionary();
                            data["id"] = doc.Id;
                            
                            // Convert Timestamp fields or others to serialize cleanly
                            var sanitized = SanitizeDictionary(data);
                            checklistList.Add(sanitized);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error fetching component_checklists: {ex.Message}");
                }

                var package = new BackupDataPackage
                {
                    Users = await _userRepo.GetAllAsync(),
                    Devices = await _deviceRepo.GetAllAsync(),
                    RmaTickets = await _ticketRepo.GetAllAsync(),
                    StatusHistories = await _historyRepo.GetAllAsync(),
                    ComponentChecklists = checklistList,
                    Attachments = await _attachmentRepo.GetAllAsync(),
                    Customers = await _customerRepo.GetAllAsync(),
                    Categories = await _categoryRepo.GetAllAsync(),
                    Models = await _modelRepo.GetAllAsync(),
                    Vendors = await _vendorRepo.GetAllAsync(),
                    StatusMasters = await _statusRepo.GetAllAsync(),
                    Locations = await _locationRepo.GetAllAsync(),
                    SalesOrders = await _salesOrderRepo.GetAllAsync(),
                    SystemSettings = await _settingRepo.GetAllAsync(),
                    AuditLogs = await _auditLogRepo.GetAllAsync()
                };

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                byte[] jsonBytes = JsonSerializer.SerializeToUtf8Bytes(package, options);
                return File(jsonBytes, "application/json", $"RMA_Backup_{DateTime.UtcNow:yyyyMMdd_HHmmss}.json");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Export failed: {ex.Message}" });
            }
        }

        [HttpPost("import")]
        public async Task<IActionResult> Import(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { message = "No file uploaded or file is empty." });
            }

            try
            {
                using var stream = file.OpenReadStream();
                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };
                
                var package = await JsonSerializer.DeserializeAsync<BackupDataPackage>(stream, options);
                if (package == null)
                {
                    return BadRequest(new { message = "Invalid JSON structure." });
                }

                var batchWriter = new FirestoreBatchWriter(_firestoreDb);

                // Import each collection
                if (package.Users != null)
                {
                    foreach (var entity in package.Users)
                    {
                        var docRef = _firestoreDb.Collection("users").Document(entity.Id);
                        await batchWriter.SetAsync(docRef, entity);
                    }
                }
                if (package.Devices != null)
                {
                    foreach (var entity in package.Devices)
                    {
                        var docRef = _firestoreDb.Collection("devices").Document(entity.Id);
                        await batchWriter.SetAsync(docRef, entity);
                    }
                }
                if (package.RmaTickets != null)
                {
                    foreach (var entity in package.RmaTickets)
                    {
                        var docRef = _firestoreDb.Collection("rma_tickets").Document(entity.Id);
                        await batchWriter.SetAsync(docRef, entity);
                    }
                }
                if (package.StatusHistories != null)
                {
                    foreach (var entity in package.StatusHistories)
                    {
                        var docRef = _firestoreDb.Collection("status_histories").Document(entity.Id);
                        await batchWriter.SetAsync(docRef, entity);
                    }
                }
                if (package.ComponentChecklists != null)
                {
                    foreach (var dict in package.ComponentChecklists)
                    {
                        if (dict.TryGetValue("id", out var idObj) && idObj is string docId)
                        {
                            var docRef = _firestoreDb.Collection("component_checklists").Document(docId);
                            var writeData = new Dictionary<string, object>(dict);
                            writeData.Remove("id");
                            await batchWriter.SetAsync(docRef, writeData);
                        }
                    }
                }
                if (package.Attachments != null)
                {
                    foreach (var entity in package.Attachments)
                    {
                        var docRef = _firestoreDb.Collection("attachments").Document(entity.Id);
                        await batchWriter.SetAsync(docRef, entity);
                    }
                }
                if (package.Customers != null)
                {
                    foreach (var entity in package.Customers)
                    {
                        var docRef = _firestoreDb.Collection("customers").Document(entity.Id);
                        await batchWriter.SetAsync(docRef, entity);
                    }
                }
                if (package.Categories != null)
                {
                    foreach (var entity in package.Categories)
                    {
                        var docRef = _firestoreDb.Collection("categories").Document(entity.Id);
                        await batchWriter.SetAsync(docRef, entity);
                    }
                }
                if (package.Models != null)
                {
                    foreach (var entity in package.Models)
                    {
                        var docRef = _firestoreDb.Collection("models").Document(entity.Id);
                        await batchWriter.SetAsync(docRef, entity);
                    }
                }
                if (package.Vendors != null)
                {
                    foreach (var entity in package.Vendors)
                    {
                        var docRef = _firestoreDb.Collection("vendors").Document(entity.Id);
                        await batchWriter.SetAsync(docRef, entity);
                    }
                }
                if (package.StatusMasters != null)
                {
                    foreach (var entity in package.StatusMasters)
                    {
                        var docRef = _firestoreDb.Collection("status_masters").Document(entity.Id);
                        await batchWriter.SetAsync(docRef, entity);
                    }
                }
                if (package.Locations != null)
                {
                    foreach (var entity in package.Locations)
                    {
                        var docRef = _firestoreDb.Collection("locations").Document(entity.Id);
                        await batchWriter.SetAsync(docRef, entity);
                    }
                }
                if (package.SalesOrders != null)
                {
                    foreach (var entity in package.SalesOrders)
                    {
                        var docRef = _firestoreDb.Collection("sales_orders").Document(entity.Id);
                        await batchWriter.SetAsync(docRef, entity);
                    }
                }
                if (package.SystemSettings != null)
                {
                    foreach (var entity in package.SystemSettings)
                    {
                        var docRef = _firestoreDb.Collection("system_settings").Document(entity.Id);
                        await batchWriter.SetAsync(docRef, entity);
                    }
                }
                if (package.AuditLogs != null)
                {
                    foreach (var entity in package.AuditLogs)
                    {
                        var docRef = _firestoreDb.Collection("audit_logs").Document(entity.Id);
                        await batchWriter.SetAsync(docRef, entity);
                    }
                }

                // Final commit
                await batchWriter.CommitAsync();

                return Ok(new { message = "Khôi phục dữ liệu thành công." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Import failed: {ex.Message}" });
            }
        }

        private Dictionary<string, object> SanitizeDictionary(Dictionary<string, object> dict)
        {
            var result = new Dictionary<string, object>();
            foreach (var kvp in dict)
            {
                if (kvp.Value is Timestamp timestamp)
                {
                    result[kvp.Key] = timestamp.ToDateTime();
                }
                else if (kvp.Value is Dictionary<string, object> nestedDict)
                {
                    result[kvp.Key] = SanitizeDictionary(nestedDict);
                }
                else
                {
                    result[kvp.Key] = kvp.Value;
                }
            }
            return result;
        }
    }

    public class BackupDataPackage
    {
        public List<UserAccount>? Users { get; set; }
        public List<Device>? Devices { get; set; }
        public List<RmaTicket>? RmaTickets { get; set; }
        public List<StatusHistory>? StatusHistories { get; set; }
        public List<Dictionary<string, object>>? ComponentChecklists { get; set; }
        public List<Attachment>? Attachments { get; set; }
        public List<Customer>? Customers { get; set; }
        public List<Category>? Categories { get; set; }
        public List<Model>? Models { get; set; }
        public List<Vendor>? Vendors { get; set; }
        public List<StatusMaster>? StatusMasters { get; set; }
        public List<Location>? Locations { get; set; }
        public List<SalesOrder>? SalesOrders { get; set; }
        public List<SystemSetting>? SystemSettings { get; set; }
        public List<AuditLog>? AuditLogs { get; set; }
    }

    public class FirestoreBatchWriter
    {
        private readonly FirestoreDb _db;
        private WriteBatch _batch;
        private int _opCount = 0;

        public FirestoreBatchWriter(FirestoreDb db)
        {
            _db = db;
            _batch = _db.StartBatch();
        }

        public async Task SetAsync(DocumentReference docRef, object entity)
        {
            _batch.Set(docRef, entity, SetOptions.Overwrite);
            _opCount++;
            if (_opCount >= 400)
            {
                await CommitAsync();
            }
        }

        public async Task CommitAsync()
        {
            if (_opCount > 0)
            {
                await _batch.CommitAsync();
                _batch = _db.StartBatch();
                _opCount = 0;
            }
        }
    }
}
