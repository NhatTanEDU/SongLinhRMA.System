using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RMA.Server.Entities;
using RMA.Server.Services;
using RMA.Shared.DTOs;

namespace RMA.Server.Controllers
{
    [Route("api/admin/metrics")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Bearer,Local", Roles = "Admin")]
    public class MetricsController : ControllerBase
    {
        private readonly MetricsCollectorService _metricsCollector;
        private readonly FirestoreRepository<Device> _deviceRepo;
        private readonly FirestoreRepository<Model> _modelRepo;

        public MetricsController(
            MetricsCollectorService metricsCollector,
            FirestoreRepository<Device> deviceRepo,
            FirestoreRepository<Model> modelRepo)
        {
            _metricsCollector = metricsCollector;
            _deviceRepo = deviceRepo;
            _modelRepo = modelRepo;
        }

        [HttpGet("dashboard")]
        public async Task<ActionResult<SystemMetricsDashboardDto>> GetDashboard()
        {
            try
            {
                var snapshot = _metricsCollector.GetCurrentMetricsSnapshot();

                // 1. Calculate Firebase Costs
                const int freeReads = 50000;
                const int freeWrites = 20000;
                const int freeDeletes = 20000;

                const double readRate = 0.06 / 100000.0;
                const double writeRate = 0.18 / 100000.0;
                const double deleteRate = 0.02 / 100000.0;

                double overReadCost = Math.Max(0, snapshot.FirebaseUsage.Reads - freeReads) * readRate;
                double overWriteCost = Math.Max(0, snapshot.FirebaseUsage.Writes - freeWrites) * writeRate;
                double overDeleteCost = Math.Max(0, snapshot.FirebaseUsage.Deletes - freeDeletes) * deleteRate;

                snapshot.FirebaseUsage.EstimatedCostUsd = Math.Round(overReadCost + overWriteCost + overDeleteCost, 4);

                // For display purposes, assume 0.05 GB storage and 12.5 MB bandwidth usage
                snapshot.FirebaseUsage.StorageGb = 0.05;
                snapshot.FirebaseUsage.BandwidthMb = 12.5;

                // 2. Load Device Stats
                try
                {
                    var devices = await _deviceRepo.GetAllAsync();
                    var models = await _modelRepo.GetAllAsync();
                    var modelsDict = models.ToDictionary(m => m.Id, m => m);
                    
                    snapshot.DeviceStats = devices
                        .GroupBy(d => modelsDict.ContainsKey(d.ModelId) ? modelsDict[d.ModelId].ModelName.Split(' ').FirstOrDefault() ?? "Khác" : "Chưa rõ")
                        .Select(g => new DeviceUsageStatsDto { BrandName = g.Key, DeviceCount = g.Count() })
                        .OrderByDescending(d => d.DeviceCount)
                        .ToList();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading device stats for metrics: {ex.Message}");
                }

                // 3. Generate Alerts
                var alerts = new List<string>();

                double readPercent = (snapshot.FirebaseUsage.Reads / (double)freeReads) * 100;
                if (readPercent >= 80)
                {
                    alerts.Add($"⚠️ Cảnh báo: Số lượt Đọc Firestore hôm nay đạt {readPercent:F1}% hạn mức miễn phí!");
                }

                double writePercent = (snapshot.FirebaseUsage.Writes / (double)freeWrites) * 100;
                if (writePercent >= 80)
                {
                    alerts.Add($"⚠️ Cảnh báo: Số lượt Ghi Firestore hôm nay đạt {writePercent:F1}% hạn mức miễn phí!");
                }

                // Detect slow endpoints (latency > 2000ms)
                var slowApis = snapshot.TopApis.Where(a => a.AvgResponseTimeMs > 2000).ToList();
                foreach (var api in slowApis)
                {
                    alerts.Add($"⚠️ Cảnh báo hiệu năng: API {api.Endpoint} phản hồi chậm trung bình {api.AvgResponseTimeMs:F0}ms!");
                }

                // Detect high activity users (actions > 500)
                var activeUsers = snapshot.UserActivities.Where(u => u.ActionCount > 500).ToList();
                foreach (var user in activeUsers)
                {
                    alerts.Add($"⚠️ Cảnh báo an ninh: Tài khoản {user.Username} ({user.Department}) có tần suất thao tác cao bất thường ({user.ActionCount} lần)!");
                }

                snapshot.SystemAlerts = alerts;

                return Ok(snapshot);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
