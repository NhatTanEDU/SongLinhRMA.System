using System;
using System.Collections.Generic;

namespace RMA.Shared.DTOs
{
    public class FirebaseUsageDto
    {
        public int Reads { get; set; }
        public int Writes { get; set; }
        public int Deletes { get; set; }
        public double StorageGb { get; set; }
        public double BandwidthMb { get; set; }
        public double EstimatedCostUsd { get; set; }
    }

    public class ApiMetricDto
    {
        public string Endpoint { get; set; } = string.Empty;
        public int CallCount { get; set; }
        public double AvgResponseTimeMs { get; set; }
    }

    public class CollectionMetricDto
    {
        public string CollectionName { get; set; } = string.Empty;
        public int Reads { get; set; }
        public int Writes { get; set; }
    }

    public class FeatureUsageDto
    {
        public string FeatureName { get; set; } = string.Empty;
        public int UsageCount { get; set; }
    }

    public class UserActivityDto
    {
        public string Username { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public int ActionCount { get; set; }
    }

    public class DeviceUsageStatsDto
    {
        public string BrandName { get; set; } = string.Empty;
        public int DeviceCount { get; set; }
    }

    public class SystemMetricsDashboardDto
    {
        public FirebaseUsageDto FirebaseUsage { get; set; } = new();
        public List<ApiMetricDto> TopApis { get; set; } = new();
        public List<CollectionMetricDto> FirestoreCollections { get; set; } = new();
        public List<FeatureUsageDto> FeaturesUsage { get; set; } = new();
        public List<UserActivityDto> UserActivities { get; set; } = new();
        public List<DeviceUsageStatsDto> DeviceStats { get; set; } = new();
        public List<string> SystemAlerts { get; set; } = new();
    }
}
