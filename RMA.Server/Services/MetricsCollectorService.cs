using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using RMA.Shared.DTOs;

namespace RMA.Server.Services
{
    public class MetricsCollectorService
    {
        public static MetricsCollectorService? Instance { get; private set; }

        public MetricsCollectorService()
        {
            Instance = this;
        }

        private readonly ConcurrentDictionary<string, int> _apiCallCounts = new();
        private readonly ConcurrentDictionary<string, ConcurrentQueue<long>> _apiResponseTimes = new();
        
        // CollectionName -> (Reads, Writes, Deletes)
        private readonly ConcurrentDictionary<string, FirestoreCounter> _collectionMetrics = new();
        
        private readonly ConcurrentDictionary<string, int> _featureUsage = new();
        private readonly ConcurrentDictionary<string, UserActionCounter> _userActivities = new();

        private int _firestoreTotalReads = 0;
        private int _firestoreTotalWrites = 0;
        private int _firestoreTotalDeletes = 0;

        private class FirestoreCounter
        {
            public int Reads;
            public int Writes;
            public int Deletes;
        }

        private class UserActionCounter
        {
            public string Username = string.Empty;
            public string Department = string.Empty;
            public int ActionCount;
        }

        public void IncrementApiCall(string endpoint, long elapsedMs)
        {
            _apiCallCounts.AddOrUpdate(endpoint, 1, (_, count) => count + 1);
            
            var queue = _apiResponseTimes.GetOrAdd(endpoint, _ => new ConcurrentQueue<long>());
            queue.Enqueue(elapsedMs);
            
            // Keep queue size manageable (e.g., last 100 response times for averaging)
            while (queue.Count > 100)
            {
                queue.TryDequeue(out _);
            }
        }

        public void IncrementFirestoreOp(string collection, string operation, int count = 1)
        {
            var counter = _collectionMetrics.GetOrAdd(collection, _ => new FirestoreCounter());
            
            if (operation.Equals("Read", StringComparison.OrdinalIgnoreCase))
            {
                System.Threading.Interlocked.Add(ref counter.Reads, count);
                System.Threading.Interlocked.Add(ref _firestoreTotalReads, count);
            }
            else if (operation.Equals("Write", StringComparison.OrdinalIgnoreCase))
            {
                System.Threading.Interlocked.Add(ref counter.Writes, count);
                System.Threading.Interlocked.Add(ref _firestoreTotalWrites, count);
            }
            else if (operation.Equals("Delete", StringComparison.OrdinalIgnoreCase))
            {
                System.Threading.Interlocked.Add(ref counter.Deletes, count);
                System.Threading.Interlocked.Add(ref _firestoreTotalDeletes, count);
            }
        }

        public void IncrementFeatureUsage(string feature)
        {
            _featureUsage.AddOrUpdate(feature, 1, (_, count) => count + 1);
        }

        public void IncrementUserAction(string username, string department = "Sales")
        {
            if (string.IsNullOrEmpty(username)) return;
            
            _userActivities.AddOrUpdate(username, 
                new UserActionCounter { Username = username, Department = department, ActionCount = 1 }, 
                (_, counter) => {
                    counter.ActionCount++;
                    return counter;
                });
        }

        public (int Reads, int Writes, int Deletes) GetTotalFirestoreOps()
        {
            return (_firestoreTotalReads, _firestoreTotalWrites, _firestoreTotalDeletes);
        }

        public SystemMetricsDashboardDto GetCurrentMetricsSnapshot()
        {
            var snapshot = new SystemMetricsDashboardDto();
            
            // Firebase usage
            snapshot.FirebaseUsage.Reads = _firestoreTotalReads;
            snapshot.FirebaseUsage.Writes = _firestoreTotalWrites;
            snapshot.FirebaseUsage.Deletes = _firestoreTotalDeletes;
            
            // Calculate Top APIs
            snapshot.TopApis = _apiCallCounts.Select(kvp => new ApiMetricDto
            {
                Endpoint = kvp.Key,
                CallCount = kvp.Value,
                AvgResponseTimeMs = _apiResponseTimes.TryGetValue(kvp.Key, out var q) && q.Any() ? q.Average() : 0
            }).OrderByDescending(a => a.CallCount).Take(10).ToList();

            // Firestore collections
            snapshot.FirestoreCollections = _collectionMetrics.Select(kvp => new CollectionMetricDto
            {
                CollectionName = kvp.Key,
                Reads = kvp.Value.Reads,
                Writes = kvp.Value.Writes
            }).ToList();

            // Features usage
            snapshot.FeaturesUsage = _featureUsage.Select(kvp => new FeatureUsageDto
            {
                FeatureName = kvp.Key,
                UsageCount = kvp.Value
            }).OrderByDescending(f => f.UsageCount).ToList();

            // User activities
            snapshot.UserActivities = _userActivities.Values.Select(v => new UserActivityDto
            {
                Username = v.Username,
                Department = v.Department,
                ActionCount = v.ActionCount
            }).OrderByDescending(u => u.ActionCount).ToList();

            return snapshot;
        }

        public void Reset()
        {
            _apiCallCounts.Clear();
            _apiResponseTimes.Clear();
            _collectionMetrics.Clear();
            _featureUsage.Clear();
            _userActivities.Clear();
            System.Threading.Interlocked.Exchange(ref _firestoreTotalReads, 0);
            System.Threading.Interlocked.Exchange(ref _firestoreTotalWrites, 0);
            System.Threading.Interlocked.Exchange(ref _firestoreTotalDeletes, 0);
        }

        // Methods to hydrate/set counts from persisted DB (for background worker recovery)
        public void HydrateFirestoreCounts(int reads, int writes, int deletes)
        {
            System.Threading.Interlocked.Exchange(ref _firestoreTotalReads, reads);
            System.Threading.Interlocked.Exchange(ref _firestoreTotalWrites, writes);
            System.Threading.Interlocked.Exchange(ref _firestoreTotalDeletes, deletes);
        }
    }
}
