using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Google.Cloud.Firestore;
using RMA.Server.Services;
using RMA.Shared.DTOs;

namespace RMA.Server.Services
{
    public class MetricsFlushBackgroundService : BackgroundService
    {
        private readonly MetricsCollectorService _metricsCollector;
        private readonly FirestoreDb _firestoreDb;
        private readonly TimeSpan _flushInterval = TimeSpan.FromMinutes(5);
        private bool _isHydrated = false;

        public MetricsFlushBackgroundService(MetricsCollectorService metricsCollector, FirestoreDb firestoreDb)
        {
            _metricsCollector = metricsCollector;
            _firestoreDb = firestoreDb;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // 1. Startup Hydration: Fetch today's accumulated values on start
            await HydrateTodayMetrics();

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(_flushInterval, stoppingToken);
                    await FlushMetricsToFirestore();
                }
                catch (TaskCanceledException)
                {
                    // Shutdown requested
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in MetricsFlushBackgroundService: {ex.Message}");
                }
            }

            // Flush one last time on shutdown
            try
            {
                await FlushMetricsToFirestore();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error flushing metrics during shutdown: {ex.Message}");
            }
        }

        private async Task HydrateTodayMetrics()
        {
            if (_isHydrated) return;
            try
            {
                var todayDocId = DateTime.UtcNow.ToString("yyyy-MM-dd");
                var docRef = _firestoreDb.Collection("usage_metrics").Document(todayDocId);
                var snapshot = await docRef.GetSnapshotAsync();
                
                if (snapshot.Exists)
                {
                    var reads = snapshot.GetValue<int>("Reads");
                    var writes = snapshot.GetValue<int>("Writes");
                    var deletes = snapshot.GetValue<int>("Deletes");
                    
                    _metricsCollector.HydrateFirestoreCounts(reads, writes, deletes);
                }
                _isHydrated = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error hydrating metrics on startup: {ex.Message}");
            }
        }

        private async Task FlushMetricsToFirestore()
        {
            var snapshot = _metricsCollector.GetCurrentMetricsSnapshot();
            var todayDocId = DateTime.UtcNow.ToString("yyyy-MM-dd");
            var docRef = _firestoreDb.Collection("usage_metrics").Document(todayDocId);

            // Prepare dictionary to save in Firestore usage_metrics collection
            var data = new Dictionary<string, object>
            {
                { "Reads", snapshot.FirebaseUsage.Reads },
                { "Writes", snapshot.FirebaseUsage.Writes },
                { "Deletes", snapshot.FirebaseUsage.Deletes },
                { "LastUpdated", DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc) }
            };

            // Save details as arrays of maps/dictionaries
            var topApisData = new List<Dictionary<string, object>>();
            foreach (var api in snapshot.TopApis)
            {
                topApisData.Add(new Dictionary<string, object>
                {
                    { "Endpoint", api.Endpoint },
                    { "CallCount", api.CallCount },
                    { "AvgResponseTimeMs", api.AvgResponseTimeMs }
                });
            }
            data.Add("TopApis", topApisData);

            var collectionsData = new List<Dictionary<string, object>>();
            foreach (var col in snapshot.FirestoreCollections)
            {
                collectionsData.Add(new Dictionary<string, object>
                {
                    { "CollectionName", col.CollectionName },
                    { "Reads", col.Reads },
                    { "Writes", col.Writes }
                });
            }
            data.Add("FirestoreCollections", collectionsData);

            var featuresData = new List<Dictionary<string, object>>();
            foreach (var feat in snapshot.FeaturesUsage)
            {
                featuresData.Add(new Dictionary<string, object>
                {
                    { "FeatureName", feat.FeatureName },
                    { "UsageCount", feat.UsageCount }
                });
            }
            data.Add("FeaturesUsage", featuresData);

            var userActivitiesData = new List<Dictionary<string, object>>();
            foreach (var user in snapshot.UserActivities)
            {
                userActivitiesData.Add(new Dictionary<string, object>
                {
                    { "Username", user.Username },
                    { "Department", user.Department },
                    { "ActionCount", user.ActionCount }
                });
            }
            data.Add("UserActivities", userActivitiesData);

            // Set document, merge to prevent overwriting other fields if any
            await docRef.SetAsync(data, SetOptions.MergeAll);
        }
    }
}
