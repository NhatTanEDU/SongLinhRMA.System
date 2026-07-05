using System;
using System.Diagnostics;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using RMA.Server.Services;

namespace RMA.Server.Middlewares
{
    public class ApiMetricsMiddleware
    {
        private readonly RequestDelegate _next;

        public ApiMetricsMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, MetricsCollectorService metricsCollector)
        {
            var path = context.Request.Path.Value ?? string.Empty;
            
            // Only intercept api requests and ignore metrics dashboard to prevent recursion
            if (!path.StartsWith("/api/", StringComparison.OrdinalIgnoreCase) || 
                path.Contains("/api/admin/metrics/dashboard", StringComparison.OrdinalIgnoreCase))
            {
                await _next(context);
                return;
            }

            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                await _next(context);
            }
            finally
            {
                stopwatch.Stop();
                var elapsedMs = stopwatch.ElapsedMilliseconds;

                // Group endpoint by pattern (e.g. collapse ids to prevent infinite endpoint lists)
                var routeEndpoint = NormalizePath(path, context.Request.Method);

                metricsCollector.IncrementApiCall(routeEndpoint, elapsedMs);

                // Track user action
                if (context.User.Identity?.IsAuthenticated == true)
                {
                    var username = context.User.Identity.Name ?? "Unknown";
                    var role = "Sales";
                    if (context.User.IsInRole("Admin"))
                    {
                        role = "Admin";
                    }
                    else if (context.User.IsInRole("Tech"))
                    {
                        role = "Tech";
                    }
                    
                    metricsCollector.IncrementUserAction(username, role);

                    // Track feature usage based on endpoint patterns
                    var feature = GetFeatureName(routeEndpoint);
                    if (!string.IsNullOrEmpty(feature))
                    {
                        metricsCollector.IncrementFeatureUsage(feature);
                    }
                }
            }
        }

        private string NormalizePath(string path, string method)
        {
            // E.g. GET /api/salesorders/12345 -> GET /api/salesorders/{id}
            var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < segments.Length; i++)
            {
                // Simple heuristic: if segment is longer than 15 chars or contains a digit/dash, replace with {id}
                if (segments[i].Length > 15 || segments[i].Any(char.IsDigit) || (i > 1 && Guid.TryParse(segments[i], out _)))
                {
                    segments[i] = "{id}";
                }
            }
            return $"{method} /{string.Join("/", segments)}";
        }

        private string GetFeatureName(string normalizedPath)
        {
            if (normalizedPath.Contains("salesorders", StringComparison.OrdinalIgnoreCase))
            {
                if (normalizedPath.Contains("PUT", StringComparison.OrdinalIgnoreCase)) return "Cập nhật Đơn hàng";
                if (normalizedPath.Contains("POST", StringComparison.OrdinalIgnoreCase)) return "Lên Đơn hàng mới";
                if (normalizedPath.Contains("DELETE", StringComparison.OrdinalIgnoreCase)) return "Hủy Đơn hàng";
                return "Xem Đơn hàng";
            }
            if (normalizedPath.Contains("devices", StringComparison.OrdinalIgnoreCase))
            {
                return "Quản lý thiết bị / Kích hoạt";
            }
            if (normalizedPath.Contains("rmatickets", StringComparison.OrdinalIgnoreCase))
            {
                return "Xử lý bảo hành (RMA)";
            }
            if (normalizedPath.Contains("customers", StringComparison.OrdinalIgnoreCase))
            {
                return "Quản lý Khách hàng";
            }
            if (normalizedPath.Contains("referencedata", StringComparison.OrdinalIgnoreCase) || normalizedPath.Contains("models", StringComparison.OrdinalIgnoreCase))
            {
                return "Xem Danh mục & Tồn kho";
            }
            return "Khác";
        }
    }
}
