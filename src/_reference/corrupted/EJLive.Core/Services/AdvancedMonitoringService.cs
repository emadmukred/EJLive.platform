// AdvancedMonitoringService.cs
// Processed by Code Intelligence Scanner — Smart Merge & Restructure

using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System;

using AlertSeverity = EJLive.Core.Models.AlertSeverity;
using EJLive.Core.Engine;
using EJLive.Core.Models;
using HealthStatus = EJLive.Core.Models.HealthStatus;
using MetricType = EJLive.Core.Models.MetricType;

namespace EJLive.Core.Services
{
    public partial class AdvancedMonitoringService : IDisposable
    {
        private readonly Dictionary<string, PerformanceMetric> _performanceMetrics;


        private readonly Dictionary<string, HealthCheck> _healthChecks;


        private readonly List<SystemAlert> _alerts;


        private readonly Timer _monitoringTimer;


        private readonly Timer _healthCheckTimer;


        private bool _disposed;


        public AdvancedMonitoringService()
        {
            _performanceMetrics = new Dictionary<string, PerformanceMetric>();
            _healthChecks = new Dictionary<string, HealthCheck>();
            _alerts = new List<SystemAlert>();

            // مؤقت المراقبة كل 10 ثواني
            _monitoringTimer = new Timer(MonitoringCallback, null, TimeSpan.Zero, TimeSpan.FromSeconds(10));

            // مؤقت فحص الصحة كل 60 ثانية
            _healthCheckTimer = new Timer(HealthCheckCallback, null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(60));

            InitializeDefaultMetrics();
        }


    private readonly object _lock = new object();


    private void InitializeDefaultMetrics()
    {
        AddMetric("CPU_Usage", "CPU Usage", "%", MetricType.Percentage);
        AddMetric("Memory_Usage", "Memory Usage", "MB", MetricType.Number);
        AddMetric("Disk_Usage", "Disk Usage", "%", MetricType.Percentage);
        AddMetric("Network_In", "Network In", "KB/s", MetricType.Number);
        AddMetric("Network_Out", "Network Out", "KB/s", MetricType.Number);
        AddMetric("Active_Connections", "Active Connections", "", MetricType.Number);
        AddMetric("Database_Size", "Database Size", "MB", MetricType.Number);
        AddMetric("Queue_Size", "Queue Size", "", MetricType.Number);
        AddMetric("Response_Time", "Response Time", "ms", MetricType.Number);
        AddMetric("Error_Rate", "Error Rate", "%", MetricType.Percentage);
        AddMetric("Sync_Latency", "Sync Latency", "ms", MetricType.Number);
        AddMetric("Uptime", "System Uptime", "hours", MetricType.Number);
        AddMetric("Thread_Count", "Thread Count", "", MetricType.Number);
        AddMetric("Handles_Count", "Handles Count", "", MetricType.Number);
    }


private void AddMetric(string key, string displayName, string unit, MetricType type)
{
    lock (_lock)
    {
        _performanceMetrics[key] = new PerformanceMetric
        {
            Key = key,
            DisplayName = displayName,
            Unit = unit,
            Type = type,
            LastUpdated = DateTime.UtcNow
        };
}
}


private void MonitoringCallback(object? state)
{
    try
    {
        UpdateSystemMetrics();
        CheckThresholds();
    }
catch (Exception ex)
{
    RaiseAlert(new SystemAlert
    {
        AlertId = Guid.NewGuid().ToString(),
        Severity = AlertSeverity.Error,
        Category = "Monitoring",
        Title = "Monitoring Error",
        Message = $"Error in monitoring callback: {ex.Message}",
        Source = "AdvancedMonitoringService",
        CreatedAt = DateTime.UtcNow
    });
}
}


private void UpdateSystemMetrics()
{
    var process = Process.GetCurrentProcess();

    // CPU Usage
    var cpuUsage = GetCpuUsage();
    UpdateMetricValue("CPU_Usage", cpuUsage);

    // Memory Usage
    var memoryMB = process.WorkingSet64 / (1024.0 * 1024.0);
    UpdateMetricValue("Memory_Usage", memoryMB);

    // Disk Usage
    var diskUsage = GetDiskUsage();
    UpdateMetricValue("Disk_Usage", diskUsage);

    // Thread Count
    UpdateMetricValue("Thread_Count", process.Threads.Count);

    // Handles Count
    UpdateMetricValue("Handles_Count", process.HandleCount);

    // Uptime
    var uptime = (DateTime.Now - Process.GetCurrentProcess().StartTime).TotalHours;
    UpdateMetricValue("Uptime", uptime);
}


private double GetCpuUsage()
{
    try
    {
        var process = Process.GetCurrentProcess();
        var totalProcessorTime = process.TotalProcessorTime;
        var startTime = process.StartTime;
        var elapsedTime = DateTime.Now - startTime;

        if (elapsedTime.TotalMilliseconds > 0)
        {
            return (totalProcessorTime.TotalMilliseconds / (Environment.ProcessorCount * elapsedTime.TotalMilliseconds)) * 100.0;
        }
}
catch { }
return 0.0;
}


private double GetDiskUsage()
{
    try
    {
        var drive = new DriveInfo(Path.GetPathRoot(Environment.CurrentDirectory) ?? "C:\\");
        if (drive.TotalSize > 0)
        {
            var used = drive.TotalSize - drive.AvailableFreeSpace;
            return ((double)used / drive.TotalSize) * 100.0;
        }
}
catch { }
return 0.0;
}


private void UpdateMetricValue(string key, double value)
{
    lock (_lock)
    {
        if (_performanceMetrics.TryGetValue(key, out var metric))
        {
            metric.CurrentValue = value;
            metric.LastUpdated = DateTime.UtcNow;

            // Update statistics
            metric.Values.Add(value);
            if (metric.Values.Count > 100) // Keep last 100 values
            metric.Values.RemoveAt(0);

            metric.MinValue = metric.Values.Min();
            metric.MaxValue = metric.Values.Max();
            metric.AverageValue = metric.Values.Average();

            OnMetricUpdated?.Invoke(key, metric);
        }
}
}


private void CheckThresholds()
{
    lock (_lock)
    {
        foreach (var metric in _performanceMetrics.Values)
        {
            switch (metric.Key)
            {
                case "CPU_Usage":
                CheckMetricThreshold(metric, 80, 95, "High CPU Usage");
                break;
                case "Memory_Usage":
                CheckMetricThreshold(metric, 500, 750, "High Memory Usage");
                break;
                case "Disk_Usage":
                CheckMetricThreshold(metric, 80, 95, "High Disk Usage");
                break;
                case "Error_Rate":
                CheckMetricThreshold(metric, 5, 10, "High Error Rate");
                break;
                case "Response_Time":
                CheckMetricThreshold(metric, 5000, 10000, "High Response Time");
                break;
            }
    }
}
}


private void CheckMetricThreshold(PerformanceMetric metric, double warningThreshold, double errorThreshold, string alertTitle)
{
    if (metric.CurrentValue > errorThreshold)
    {
        RaiseAlert(new SystemAlert
        {
            AlertId = Guid.NewGuid().ToString(),
            Severity = AlertSeverity.Error,
            Category = "Performance",
            Title = alertTitle,
            Message = $"{metric.DisplayName} is critically high: {metric.CurrentValue:F2}{metric.Unit} (Threshold: {errorThreshold}{metric.Unit})",
            Source = "ThresholdMonitor",
            CreatedAt = DateTime.UtcNow
        });
}
else if (metric.CurrentValue > warningThreshold)
{
    RaiseAlert(new SystemAlert
    {
        AlertId = Guid.NewGuid().ToString(),
        Severity = AlertSeverity.Warning,
        Category = "Performance",
        Title = alertTitle,
        Message = $"{metric.DisplayName} is elevated: {metric.CurrentValue:F2}{metric.Unit} (Threshold: {warningThreshold}{metric.Unit})",
        Source = "ThresholdMonitor",
        CreatedAt = DateTime.UtcNow
    });
}
}


private void HealthCheckCallback(object? state)
{
    try
    {
        PerformHealthChecks();
    }
catch (Exception ex)
{
    Debug.WriteLine($"Health check error: {ex.Message}");
}
}


private void PerformHealthChecks()
{
    // Check database connectivity
    CheckDatabaseHealth();

    // Check network connectivity
    CheckNetworkHealth();

    // Check file system
    CheckFileSystemHealth();

    // Check service dependencies
    CheckServiceDependenciesHealth();
}


private void CheckDatabaseHealth()
{
    var healthCheck = new HealthCheck;
    {
        CheckId = "DB",
        Name = "Database Connectivity",
        CheckType = "Infrastructure",
        CheckedAt = DateTime.UtcNow
    };

try
{
    // Simulated database check - in production, would actually query the database
    var dbPath = Path.Combine(AppConstants.DefaultDatabasePath, "ejlive.db");
    if (File.Exists(dbPath))
    {
        var dbInfo = new FileInfo(dbPath);
        healthCheck.Status = HealthStatus.Healthy;
        healthCheck.Message = $"Database accessible. Size: {dbInfo.Length / 1024}KB";
        healthCheck.ResponseTimeMs = 50; // Simulated response time
    }
else
{
    healthCheck.Status = HealthStatus.Warning;
    healthCheck.Message = "Database file not found but will be created on first use";
}
}
catch (Exception ex)
{
    healthCheck.Status = HealthStatus.Unhealthy;
    healthCheck.Message = $"Database check failed: {ex.Message}";
}

UpdateHealthCheck(healthCheck);
}


private void CheckNetworkHealth()
{
    var healthCheck = new HealthCheck;
    {
        CheckId = "NET",
        Name = "Network Connectivity",
        CheckType = "Infrastructure",
        CheckedAt = DateTime.UtcNow
    };

try
{
    var networkInterfaces = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces();
    var activeConnection = networkInterfaces.Any(ni =>;
    ni.OperationalStatus == System.Net.NetworkInformation.OperationalStatus.Up &&
    (ni.NetworkInterfaceType == System.Net.NetworkInformation.NetworkInterfaceType.Ethernet ||
    ni.NetworkInterfaceType == System.Net.NetworkInformation.NetworkInterfaceType.Wireless80211));

    if (activeConnection)
    {
        healthCheck.Status = HealthStatus.Healthy;
        healthCheck.Message = "Network is operational";
        healthCheck.ResponseTimeMs = 10;
    }
else
{
    healthCheck.Status = HealthStatus.Warning;
    healthCheck.Message = "No active network connection detected";
}
}
catch (Exception ex)
{
    healthCheck.Status = HealthStatus.Unhealthy;
    healthCheck.Message = $"Network check failed: {ex.Message}";
}

UpdateHealthCheck(healthCheck);
}


private void CheckFileSystemHealth()
{
    var healthCheck = new HealthCheck;
    {
        CheckId = "FS",
        Name = "File System",
        CheckType = "Infrastructure",
        CheckedAt = DateTime.UtcNow
    };

try
{
    var tempPath = Path.GetTempPath();
    var testFile = Path.Combine(tempPath, $"ejlive_health_{Guid.NewGuid():N}.tmp");
    File.WriteAllText(testFile, "health_check");
    File.Delete(testFile);

    healthCheck.Status = HealthStatus.Healthy;
    healthCheck.Message = "File system is writable and operational";
    healthCheck.ResponseTimeMs = 15;
}
catch (Exception ex)
{
    healthCheck.Status = HealthStatus.Degraded;
    healthCheck.Message = $"File system check failed: {ex.Message}";
}

UpdateHealthCheck(healthCheck);
}


private void CheckServiceDependenciesHealth()
{
    var healthCheck = new HealthCheck;
    {
        CheckId = "SVC",
        Name = "Service Dependencies",
        CheckType = "Service",
        CheckedAt = DateTime.UtcNow
    };

try
{
    // Check essential Windows services
    var services = new[] { "W3SVC", "MSSQL$SQLEXPRESS", "EventLog" };
var healthyCount = 0;

foreach (var serviceName in services)
{
    try
    {
        var process = new Process;
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "sc.exe",
                Arguments = $"query {serviceName}",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
    };
process.Start();
var output = process.StandardOutput.ReadToEnd();
process.WaitForExit();

if (output.Contains("RUNNING"))
healthyCount++;
}
catch
{
    // Service not found or not running - this is fine
}
}

if (healthyCount >= 1) // At least one service is running
{
    healthCheck.Status = HealthStatus.Healthy;
    healthCheck.Message = $"{healthyCount}/{services.Length} essential services running";
}
else
{
    healthCheck.Status = HealthStatus.Warning;
    healthCheck.Message = "No essential services detected (this may be normal for standalone setup)";
}
}
catch (Exception ex)
{
    healthCheck.Status = HealthStatus.Degraded;
    healthCheck.Message = $"Service dependency check failed: {ex.Message}";
}

UpdateHealthCheck(healthCheck);
}


private void UpdateHealthCheck(HealthCheck healthCheck)
{
    lock (_lock)
    {
        _healthChecks[healthCheck.CheckId] = healthCheck;
        OnHealthCheckCompleted?.Invoke(healthCheck.CheckId, healthCheck);

        if (healthCheck.Status == HealthStatus.Unhealthy)
        {
            RaiseAlert(new SystemAlert
            {
                AlertId = Guid.NewGuid().ToString(),
                Severity = AlertSeverity.Error,
                Category = "Health Check",
                Title = $"Health Check Failed: {healthCheck.Name}",
                Message = healthCheck.Message,
                Source = "HealthCheckService",
                CreatedAt = DateTime.UtcNow
            });
    }
}
}


private void RaiseAlert(SystemAlert alert)
{
    lock (_lock)
    {
        _alerts.Add(alert);
        if (_alerts.Count > 1000)
        _alerts.RemoveRange(0, _alerts.Count - 1000);
    }

OnAlertRaised?.Invoke(alert);
}


public PerformanceSnapshot GetPerformanceSnapshot()
{
    lock (_lock)
    {
        return new PerformanceSnapshot
        {
            Timestamp = DateTime.UtcNow,
            Metrics = _performanceMetrics.Values
            .ToDictionary(m => m.Key, m => m.Clone()),
            HealthChecks = _healthChecks.Values
            .ToDictionary(h => h.CheckId, h => h.Clone()),
            ActiveAlerts = _alerts
            .Where(a => !a.IsResolved)
            .OrderByDescending(a => a.CreatedAt)
            .Take(50)
            .ToList()
        };
}
}


public List<SystemAlert> GetActiveAlerts(int maxCount = 50)
{
    lock (_lock)
    {
        return _alerts
        .Where(a => !a.IsResolved)
        .OrderByDescending(a => a.CreatedAt)
        .Take(maxCount)
        .ToList();
    }
}


public void ResolveAlert(string alertId)
{
    lock (_lock)
    {
        var alert = _alerts.FirstOrDefault(a => a.AlertId == alertId);
        if (alert != null)
        {
            alert.IsResolved = true;
            alert.ResolvedAt = DateTime.UtcNow;
        }
}
}


public double GetSystemHealthScore()
{
    lock (_lock)
    {
        var score = 100.0;
        var failedChecks = _healthChecks.Values.Count(h => h.Status == HealthStatus.Unhealthy);
        var degradedChecks = _healthChecks.Values.Count(h => h.Status == HealthStatus.Degraded);
        var warningChecks = _healthChecks.Values.Count(h => h.Status == HealthStatus.Warning);

        score -= failedChecks * 20;
        score -= degradedChecks * 10;
        score -= warningChecks * 5;

        // Consider performance metrics
        foreach (var metric in _performanceMetrics.Values)
        {
            if (metric.Key == "CPU_Usage" && metric.CurrentValue > 90)
            score -= 15;
            else if (metric.Key == "CPU_Usage" && metric.CurrentValue > 75)
            score -= 5;
            else if (metric.Key == "Memory_Usage" && metric.CurrentValue > 600)
            score -= 10;
            else if (metric.Key == "Error_Rate" && metric.CurrentValue > 10)
            score -= 20;
            else if (metric.Key == "Error_Rate" && metric.CurrentValue > 5)
            score -= 10;
        }

    return Math.Max(0, Math.Min(100, score));
}
}


public void Dispose()
{
    if (!_disposed)
    {
        _monitoringTimer?.Dispose();
        _healthCheckTimer?.Dispose();
        _disposed = true;
    }
}


public event Action<SystemAlert>? OnAlertRaised;


public event Action<string, PerformanceMetric>? OnMetricUpdated;


public event Action<string, HealthCheck>? OnHealthCheckCompleted;

}
// Class: AdvancedMonitoringService (from 1 sources)
public partial class AdvancedMonitoringService : IDisposable
{
    // --- Constants & Fields ---
    private readonly Dictionary<string, PerformanceMetric> _performanceMetrics;

    private readonly Dictionary<string, HealthCheck> _healthChecks;

    private readonly List<SystemAlert> _alerts;

    private readonly Timer _monitoringTimer;

    private readonly Timer _healthCheckTimer;

    private bool _disposed;


    // --- Constructors ---
    public AdvancedMonitoringService()
    {
        _performanceMetrics = new Dictionary<string, PerformanceMetric>();
        _healthChecks = new Dictionary<string, HealthCheck>();
        _alerts = new List<SystemAlert>();

        // مؤقت المراقبة كل 10 ثواني
        _monitoringTimer = new Timer(MonitoringCallback, null, TimeSpan.Zero, TimeSpan.FromSeconds(10));

        // مؤقت فحص الصحة كل 60 ثانية
        _healthCheckTimer = new Timer(HealthCheckCallback, null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(60));

        InitializeDefaultMetrics();
    }


// --- Methods ---
private readonly object _lock = new object();

private void InitializeDefaultMetrics()
{
    AddMetric("CPU_Usage", "CPU Usage", "%", MetricType.Percentage);
    AddMetric("Memory_Usage", "Memory Usage", "MB", MetricType.Number);
    AddMetric("Disk_Usage", "Disk Usage", "%", MetricType.Percentage);
    AddMetric("Network_In", "Network In", "KB/s", MetricType.Number);
    AddMetric("Network_Out", "Network Out", "KB/s", MetricType.Number);
    AddMetric("Active_Connections", "Active Connections", "", MetricType.Number);
    AddMetric("Database_Size", "Database Size", "MB", MetricType.Number);
    AddMetric("Queue_Size", "Queue Size", "", MetricType.Number);
    AddMetric("Response_Time", "Response Time", "ms", MetricType.Number);
    AddMetric("Error_Rate", "Error Rate", "%", MetricType.Percentage);
    AddMetric("Sync_Latency", "Sync Latency", "ms", MetricType.Number);
    AddMetric("Uptime", "System Uptime", "hours", MetricType.Number);
    AddMetric("Thread_Count", "Thread Count", "", MetricType.Number);
    AddMetric("Handles_Count", "Handles Count", "", MetricType.Number);
}

private void AddMetric(string key, string displayName, string unit, MetricType type)
{
    lock (_lock)
    {
        _performanceMetrics[key] = new PerformanceMetric
        {
            Key = key,
            DisplayName = displayName,
            Unit = unit,
            Type = type,
            LastUpdated = DateTime.UtcNow
        };
}
}

private void MonitoringCallback(object? state)
{
    try
    {
        UpdateSystemMetrics();
        CheckThresholds();
    }
catch (Exception ex)
{
    RaiseAlert(new SystemAlert
    {
        AlertId = Guid.NewGuid().ToString(),
        Severity = AlertSeverity.Error,
        Category = "Monitoring",
        Title = "Monitoring Error",
        Message = $"Error in monitoring callback: {ex.Message}",
        Source = "AdvancedMonitoringService",
        CreatedAt = DateTime.UtcNow
    });
}
}

private void UpdateSystemMetrics()
{
    var process = Process.GetCurrentProcess();

    // CPU Usage
    var cpuUsage = GetCpuUsage();
    UpdateMetricValue("CPU_Usage", cpuUsage);

    // Memory Usage
    var memoryMB = process.WorkingSet64 / (1024.0 * 1024.0);
    UpdateMetricValue("Memory_Usage", memoryMB);

    // Disk Usage
    var diskUsage = GetDiskUsage();
    UpdateMetricValue("Disk_Usage", diskUsage);

    // Thread Count
    UpdateMetricValue("Thread_Count", process.Threads.Count);

    // Handles Count
    UpdateMetricValue("Handles_Count", process.HandleCount);

    // Uptime
    var uptime = (DateTime.Now - Process.GetCurrentProcess().StartTime).TotalHours;
    UpdateMetricValue("Uptime", uptime);
}

private double GetCpuUsage()
{
    try
    {
        var process = Process.GetCurrentProcess();
        var totalProcessorTime = process.TotalProcessorTime;
        var startTime = process.StartTime;
        var elapsedTime = DateTime.Now - startTime;

        if (elapsedTime.TotalMilliseconds > 0)
        {
            return (totalProcessorTime.TotalMilliseconds / (Environment.ProcessorCount * elapsedTime.TotalMilliseconds)) * 100.0;
        }
}
catch { }
return 0.0;
}

private double GetDiskUsage()
{
    try
    {
        var drive = new DriveInfo(Path.GetPathRoot(Environment.CurrentDirectory) ?? "C:\\");
        if (drive.TotalSize > 0)
        {
            var used = drive.TotalSize - drive.AvailableFreeSpace;
            return ((double)used / drive.TotalSize) * 100.0;
        }
}
catch { }
return 0.0;
}

private void UpdateMetricValue(string key, double value)
{
    lock (_lock)
    {
        if (_performanceMetrics.TryGetValue(key, out var metric))
        {
            metric.CurrentValue = value;
            metric.LastUpdated = DateTime.UtcNow;

            // Update statistics
            metric.Values.Add(value);
            if (metric.Values.Count > 100) // Keep last 100 values
            metric.Values.RemoveAt(0);

            metric.MinValue = metric.Values.Min();
            metric.MaxValue = metric.Values.Max();
            metric.AverageValue = metric.Values.Average();

            OnMetricUpdated?.Invoke(key, metric);
        }
}
}

private void CheckThresholds()
{
    lock (_lock)
    {
        foreach (var metric in _performanceMetrics.Values)
        {
            switch (metric.Key)
            {
                case "CPU_Usage":
                CheckMetricThreshold(metric, 80, 95, "High CPU Usage");
                break;
                case "Memory_Usage":
                CheckMetricThreshold(metric, 500, 750, "High Memory Usage");
                break;
                case "Disk_Usage":
                CheckMetricThreshold(metric, 80, 95, "High Disk Usage");
                break;
                case "Error_Rate":
                CheckMetricThreshold(metric, 5, 10, "High Error Rate");
                break;
                case "Response_Time":
                CheckMetricThreshold(metric, 5000, 10000, "High Response Time");
                break;
            }
    }
}
}

private void CheckMetricThreshold(PerformanceMetric metric, double warningThreshold, double errorThreshold, string alertTitle)
{
    if (metric.CurrentValue > errorThreshold)
    {
        RaiseAlert(new SystemAlert
        {
            AlertId = Guid.NewGuid().ToString(),
            Severity = AlertSeverity.Error,
            Category = "Performance",
            Title = alertTitle,
            Message = $"{metric.DisplayName} is critically high: {metric.CurrentValue:F2}{metric.Unit} (Threshold: {errorThreshold}{metric.Unit})",
            Source = "ThresholdMonitor",
            CreatedAt = DateTime.UtcNow
        });
}
else if (metric.CurrentValue > warningThreshold)
{
    RaiseAlert(new SystemAlert
    {
        AlertId = Guid.NewGuid().ToString(),
        Severity = AlertSeverity.Warning,
        Category = "Performance",
        Title = alertTitle,
        Message = $"{metric.DisplayName} is elevated: {metric.CurrentValue:F2}{metric.Unit} (Threshold: {warningThreshold}{metric.Unit})",
        Source = "ThresholdMonitor",
        CreatedAt = DateTime.UtcNow
    });
}
}

private void HealthCheckCallback(object? state)
{
    try
    {
        PerformHealthChecks();
    }
catch (Exception ex)
{
    Debug.WriteLine($"Health check error: {ex.Message}");
}
}

private void PerformHealthChecks()
{
    // Check database connectivity
    CheckDatabaseHealth();

    // Check network connectivity
    CheckNetworkHealth();

    // Check file system
    CheckFileSystemHealth();

    // Check service dependencies
    CheckServiceDependenciesHealth();
}

private void CheckDatabaseHealth()
{
    var healthCheck = new HealthCheck;
    {
        CheckId = "DB",
        Name = "Database Connectivity",
        CheckType = "Infrastructure",
        CheckedAt = DateTime.UtcNow
    };

try
{
    // Simulated database check - in production, would actually query the database
    var dbPath = Path.Combine(AppConstants.DefaultDatabasePath, "ejlive.db");
    if (File.Exists(dbPath))
    {
        var dbInfo = new FileInfo(dbPath);
        healthCheck.Status = HealthStatus.Healthy;
        healthCheck.Message = $"Database accessible. Size: {dbInfo.Length / 1024}KB";
        healthCheck.ResponseTimeMs = 50; // Simulated response time
    }
else
{
    healthCheck.Status = HealthStatus.Warning;
    healthCheck.Message = "Database file not found but will be created on first use";
}
}
catch (Exception ex)
{
    healthCheck.Status = HealthStatus.Unhealthy;
    healthCheck.Message = $"Database check failed: {ex.Message}";
}

UpdateHealthCheck(healthCheck);
}

private void CheckNetworkHealth()
{
    var healthCheck = new HealthCheck;
    {
        CheckId = "NET",
        Name = "Network Connectivity",
        CheckType = "Infrastructure",
        CheckedAt = DateTime.UtcNow
    };

try
{
    var networkInterfaces = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces();
    var activeConnection = networkInterfaces.Any(ni =>;
    ni.OperationalStatus == System.Net.NetworkInformation.OperationalStatus.Up &&
    (ni.NetworkInterfaceType == System.Net.NetworkInformation.NetworkInterfaceType.Ethernet ||
    ni.NetworkInterfaceType == System.Net.NetworkInformation.NetworkInterfaceType.Wireless80211));

    if (activeConnection)
    {
        healthCheck.Status = HealthStatus.Healthy;
        healthCheck.Message = "Network is operational";
        healthCheck.ResponseTimeMs = 10;
    }
else
{
    healthCheck.Status = HealthStatus.Warning;
    healthCheck.Message = "No active network connection detected";
}
}
catch (Exception ex)
{
    healthCheck.Status = HealthStatus.Unhealthy;
    healthCheck.Message = $"Network check failed: {ex.Message}";
}

UpdateHealthCheck(healthCheck);
}

private void CheckFileSystemHealth()
{
    var healthCheck = new HealthCheck;
    {
        CheckId = "FS",
        Name = "File System",
        CheckType = "Infrastructure",
        CheckedAt = DateTime.UtcNow
    };

try
{
    var tempPath = Path.GetTempPath();
    var testFile = Path.Combine(tempPath, $"ejlive_health_{Guid.NewGuid():N}.tmp");
    File.WriteAllText(testFile, "health_check");
    File.Delete(testFile);

    healthCheck.Status = HealthStatus.Healthy;
    healthCheck.Message = "File system is writable and operational";
    healthCheck.ResponseTimeMs = 15;
}
catch (Exception ex)
{
    healthCheck.Status = HealthStatus.Degraded;
    healthCheck.Message = $"File system check failed: {ex.Message}";
}

UpdateHealthCheck(healthCheck);
}

private void CheckServiceDependenciesHealth()
{
    var healthCheck = new HealthCheck;
    {
        CheckId = "SVC",
        Name = "Service Dependencies",
        CheckType = "Service",
        CheckedAt = DateTime.UtcNow
    };

try
{
    // Check essential Windows services
    var services = new[] { "W3SVC", "MSSQL$SQLEXPRESS", "EventLog" };
var healthyCount = 0;

foreach (var serviceName in services)
{
    try
    {
        var process = new Process;
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "sc.exe",
                Arguments = $"query {serviceName}",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
    };
process.Start();
var output = process.StandardOutput.ReadToEnd();
process.WaitForExit();

if (output.Contains("RUNNING"))
healthyCount++;
}
catch
{
    // Service not found or not running - this is fine
}
}

if (healthyCount >= 1) // At least one service is running
{
    healthCheck.Status = HealthStatus.Healthy;
    healthCheck.Message = $"{healthyCount}/{services.Length} essential services running";
}
else
{
    healthCheck.Status = HealthStatus.Warning;
    healthCheck.Message = "No essential services detected (this may be normal for standalone setup)";
}
}
catch (Exception ex)
{
    healthCheck.Status = HealthStatus.Degraded;
    healthCheck.Message = $"Service dependency check failed: {ex.Message}";
}

UpdateHealthCheck(healthCheck);
}

private void UpdateHealthCheck(HealthCheck healthCheck)
{
    lock (_lock)
    {
        _healthChecks[healthCheck.CheckId] = healthCheck;
        OnHealthCheckCompleted?.Invoke(healthCheck.CheckId, healthCheck);

        if (healthCheck.Status == HealthStatus.Unhealthy)
        {
            RaiseAlert(new SystemAlert
            {
                AlertId = Guid.NewGuid().ToString(),
                Severity = AlertSeverity.Error,
                Category = "Health Check",
                Title = $"Health Check Failed: {healthCheck.Name}",
                Message = healthCheck.Message,
                Source = "HealthCheckService",
                CreatedAt = DateTime.UtcNow
            });
    }
}
}

private void RaiseAlert(SystemAlert alert)
{
    lock (_lock)
    {
        _alerts.Add(alert);
        if (_alerts.Count > 1000)
        _alerts.RemoveRange(0, _alerts.Count - 1000);
    }

OnAlertRaised?.Invoke(alert);
}

public PerformanceSnapshot GetPerformanceSnapshot()
{
    lock (_lock)
    {
        return new PerformanceSnapshot
        {
            Timestamp = DateTime.UtcNow,
            Metrics = _performanceMetrics.Values
            .ToDictionary(m => m.Key, m => m.Clone()),
            HealthChecks = _healthChecks.Values
            .ToDictionary(h => h.CheckId, h => h.Clone()),
            ActiveAlerts = _alerts
            .Where(a => !a.IsResolved)
            .OrderByDescending(a => a.CreatedAt)
            .Take(50)
            .ToList()
        };
}
}

public List<SystemAlert> GetActiveAlerts(int maxCount = 50)
{
    lock (_lock)
    {
        return _alerts
        .Where(a => !a.IsResolved)
        .OrderByDescending(a => a.CreatedAt)
        .Take(maxCount)
        .ToList();
    }
}

public void ResolveAlert(string alertId)
{
    lock (_lock)
    {
        var alert = _alerts.FirstOrDefault(a => a.AlertId == alertId);
        if (alert != null)
        {
            alert.IsResolved = true;
            alert.ResolvedAt = DateTime.UtcNow;
        }
}
}

public double GetSystemHealthScore()
{
    lock (_lock)
    {
        var score = 100.0;
        var failedChecks = _healthChecks.Values.Count(h => h.Status == HealthStatus.Unhealthy);
        var degradedChecks = _healthChecks.Values.Count(h => h.Status == HealthStatus.Degraded);
        var warningChecks = _healthChecks.Values.Count(h => h.Status == HealthStatus.Warning);

        score -= failedChecks * 20;
        score -= degradedChecks * 10;
        score -= warningChecks * 5;

        // Consider performance metrics
        foreach (var metric in _performanceMetrics.Values)
        {
            if (metric.Key == "CPU_Usage" && metric.CurrentValue > 90)
            score -= 15;
            else if (metric.Key == "CPU_Usage" && metric.CurrentValue > 75)
            score -= 5;
            else if (metric.Key == "Memory_Usage" && metric.CurrentValue > 600)
            score -= 10;
            else if (metric.Key == "Error_Rate" && metric.CurrentValue > 10)
            score -= 20;
            else if (metric.Key == "Error_Rate" && metric.CurrentValue > 5)
            score -= 10;
        }

    return Math.Max(0, Math.Min(100, score));
}
}

public void Dispose()
{
    if (!_disposed)
    {
        _monitoringTimer?.Dispose();
        _healthCheckTimer?.Dispose();
        _disposed = true;
    }
}


// --- Events ---
public event Action<SystemAlert>? OnAlertRaised;

public event Action<string, PerformanceMetric>? OnMetricUpdated;

public event Action<string, HealthCheck>? OnHealthCheckCompleted;

}
/// <summary>
/// خدمة المراقبة المتقدمة - Advanced Monitoring Service
/// توفر مراقبة شاملة لأداء النظام والصحة العامة مع تنبيهات ذكية
/// </summary>
public class AdvancedMonitoringService : IDisposable
{
    private readonly Dictionary<string, PerformanceMetric> _performanceMetrics;
    private readonly Dictionary<string, HealthCheck> _healthChecks;
    private readonly List<SystemAlert> _alerts;
    private readonly Timer _monitoringTimer;
    private readonly Timer _healthCheckTimer;
    private readonly object _lock = new object();
    private bool _disposed;

    public event Action<SystemAlert>? OnAlertRaised;
    public event Action<string, PerformanceMetric>? OnMetricUpdated;
    public event Action<string, HealthCheck>? OnHealthCheckCompleted;

    public AdvancedMonitoringService()
    {
        _performanceMetrics = new Dictionary<string, PerformanceMetric>();
        _healthChecks = new Dictionary<string, HealthCheck>();
        _alerts = new List<SystemAlert>();

        // مؤقت المراقبة كل 10 ثواني
        _monitoringTimer = new Timer(MonitoringCallback, null, TimeSpan.Zero, TimeSpan.FromSeconds(10));

        // مؤقت فحص الصحة كل 60 ثانية
        _healthCheckTimer = new Timer(HealthCheckCallback, null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(60));

        InitializeDefaultMetrics();
    }

private void InitializeDefaultMetrics()
{
    AddMetric("CPU_Usage", "CPU Usage", "%", MetricType.Percentage);
    AddMetric("Memory_Usage", "Memory Usage", "MB", MetricType.Number);
    AddMetric("Disk_Usage", "Disk Usage", "%", MetricType.Percentage);
    AddMetric("Network_In", "Network In", "KB/s", MetricType.Number);
    AddMetric("Network_Out", "Network Out", "KB/s", MetricType.Number);
    AddMetric("Active_Connections", "Active Connections", "", MetricType.Number);
    AddMetric("Database_Size", "Database Size", "MB", MetricType.Number);
    AddMetric("Queue_Size", "Queue Size", "", MetricType.Number);
    AddMetric("Response_Time", "Response Time", "ms", MetricType.Number);
    AddMetric("Error_Rate", "Error Rate", "%", MetricType.Percentage);
    AddMetric("Sync_Latency", "Sync Latency", "ms", MetricType.Number);
    AddMetric("Uptime", "System Uptime", "hours", MetricType.Number);
    AddMetric("Thread_Count", "Thread Count", "", MetricType.Number);
    AddMetric("Handles_Count", "Handles Count", "", MetricType.Number);
}

private void AddMetric(string key, string displayName, string unit, MetricType type)
{
    lock (_lock)
    {
        _performanceMetrics[key] = new PerformanceMetric
        {
            Key = key,
            DisplayName = displayName,
            Unit = unit,
            Type = type,
            LastUpdated = DateTime.UtcNow
        };
}
}

private void MonitoringCallback(object? state)
{
    try
    {
        UpdateSystemMetrics();
        CheckThresholds();
    }
catch (Exception ex)
{
    RaiseAlert(new SystemAlert
    {
        AlertId = Guid.NewGuid().ToString(),
        Severity = AlertSeverity.Error,
        Category = "Monitoring",
        Title = "Monitoring Error",
        Message = $"Error in monitoring callback: {ex.Message}",
        Source = "AdvancedMonitoringService",
        CreatedAt = DateTime.UtcNow
    });
}
}

private void UpdateSystemMetrics()
{
    var process = Process.GetCurrentProcess();

    // CPU Usage
    var cpuUsage = GetCpuUsage();
    UpdateMetricValue("CPU_Usage", cpuUsage);

    // Memory Usage
    var memoryMB = process.WorkingSet64 / (1024.0 * 1024.0);
    UpdateMetricValue("Memory_Usage", memoryMB);

    // Disk Usage
    var diskUsage = GetDiskUsage();
    UpdateMetricValue("Disk_Usage", diskUsage);

    // Thread Count
    UpdateMetricValue("Thread_Count", process.Threads.Count);

    // Handles Count
    UpdateMetricValue("Handles_Count", process.HandleCount);

    // Uptime
    var uptime = (DateTime.Now - Process.GetCurrentProcess().StartTime).TotalHours;
    UpdateMetricValue("Uptime", uptime);
}

private double GetCpuUsage()
{
    try
    {
        var process = Process.GetCurrentProcess();
        var totalProcessorTime = process.TotalProcessorTime;
        var startTime = process.StartTime;
        var elapsedTime = DateTime.Now - startTime;

        if (elapsedTime.TotalMilliseconds > 0)
        {
            return (totalProcessorTime.TotalMilliseconds / (Environment.ProcessorCount * elapsedTime.TotalMilliseconds)) * 100.0;
        }
}
catch { }
return 0.0;
}

private double GetDiskUsage()
{
    try
    {
        var drive = new DriveInfo(Path.GetPathRoot(Environment.CurrentDirectory) ?? "C:\\");
        if (drive.TotalSize > 0)
        {
            var used = drive.TotalSize - drive.AvailableFreeSpace;
            return ((double)used / drive.TotalSize) * 100.0;
        }
}
catch { }
return 0.0;
}

private void UpdateMetricValue(string key, double value)
{
    lock (_lock)
    {
        if (_performanceMetrics.TryGetValue(key, out var metric))
        {
            metric.CurrentValue = value;
            metric.LastUpdated = DateTime.UtcNow;

            // Update statistics
            metric.Values.Add(value);
            if (metric.Values.Count > 100) // Keep last 100 values
            metric.Values.RemoveAt(0);

            metric.MinValue = metric.Values.Min();
            metric.MaxValue = metric.Values.Max();
            metric.AverageValue = metric.Values.Average();

            OnMetricUpdated?.Invoke(key, metric);
        }
}
}

private void CheckThresholds()
{
    lock (_lock)
    {
        foreach (var metric in _performanceMetrics.Values)
        {
            switch (metric.Key)
            {
                case "CPU_Usage":
                CheckMetricThreshold(metric, 80, 95, "High CPU Usage");
                break;
                case "Memory_Usage":
                CheckMetricThreshold(metric, 500, 750, "High Memory Usage");
                break;
                case "Disk_Usage":
                CheckMetricThreshold(metric, 80, 95, "High Disk Usage");
                break;
                case "Error_Rate":
                CheckMetricThreshold(metric, 5, 10, "High Error Rate");
                break;
                case "Response_Time":
                CheckMetricThreshold(metric, 5000, 10000, "High Response Time");
                break;
            }
    }
}
}

private void CheckMetricThreshold(PerformanceMetric metric, double warningThreshold, double errorThreshold, string alertTitle)
{
    if (metric.CurrentValue > errorThreshold)
    {
        RaiseAlert(new SystemAlert
        {
            AlertId = Guid.NewGuid().ToString(),
            Severity = AlertSeverity.Error,
            Category = "Performance",
            Title = alertTitle,
            Message = $"{metric.DisplayName} is critically high: {metric.CurrentValue:F2}{metric.Unit} (Threshold: {errorThreshold}{metric.Unit})",
            Source = "ThresholdMonitor",
            CreatedAt = DateTime.UtcNow
        });
}
else if (metric.CurrentValue > warningThreshold)
{
    RaiseAlert(new SystemAlert
    {
        AlertId = Guid.NewGuid().ToString(),
        Severity = AlertSeverity.Warning,
        Category = "Performance",
        Title = alertTitle,
        Message = $"{metric.DisplayName} is elevated: {metric.CurrentValue:F2}{metric.Unit} (Threshold: {warningThreshold}{metric.Unit})",
        Source = "ThresholdMonitor",
        CreatedAt = DateTime.UtcNow
    });
}
}

private void HealthCheckCallback(object? state)
{
    try
    {
        PerformHealthChecks();
    }
catch (Exception ex)
{
    Debug.WriteLine($"Health check error: {ex.Message}");
}
}

private void PerformHealthChecks()
{
    // Check database connectivity
    CheckDatabaseHealth();

    // Check network connectivity
    CheckNetworkHealth();

    // Check file system
    CheckFileSystemHealth();

    // Check service dependencies
    CheckServiceDependenciesHealth();
}

private void CheckDatabaseHealth()
{
    var healthCheck = new HealthCheck;
    {
        CheckId = "DB",
        Name = "Database Connectivity",
        CheckType = "Infrastructure",
        CheckedAt = DateTime.UtcNow
    };

try
{
    // Simulated database check - in production, would actually query the database
    var dbPath = Path.Combine(AppConstants.DefaultDatabasePath, "ejlive.db");
    if (File.Exists(dbPath))
    {
        var dbInfo = new FileInfo(dbPath);
        healthCheck.Status = HealthStatus.Healthy;
        healthCheck.Message = $"Database accessible. Size: {dbInfo.Length / 1024}KB";
        healthCheck.ResponseTimeMs = 50; // Simulated response time
    }
else
{
    healthCheck.Status = HealthStatus.Warning;
    healthCheck.Message = "Database file not found but will be created on first use";
}
}
catch (Exception ex)
{
    healthCheck.Status = HealthStatus.Unhealthy;
    healthCheck.Message = $"Database check failed: {ex.Message}";
}

UpdateHealthCheck(healthCheck);
}

private void CheckNetworkHealth()
{
    var healthCheck = new HealthCheck;
    {
        CheckId = "NET",
        Name = "Network Connectivity",
        CheckType = "Infrastructure",
        CheckedAt = DateTime.UtcNow
    };

try
{
    var networkInterfaces = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces();
    var activeConnection = networkInterfaces.Any(ni =>;
    ni.OperationalStatus == System.Net.NetworkInformation.OperationalStatus.Up &&
    (ni.NetworkInterfaceType == System.Net.NetworkInformation.NetworkInterfaceType.Ethernet ||
    ni.NetworkInterfaceType == System.Net.NetworkInformation.NetworkInterfaceType.Wireless80211));

    if (activeConnection)
    {
        healthCheck.Status = HealthStatus.Healthy;
        healthCheck.Message = "Network is operational";
        healthCheck.ResponseTimeMs = 10;
    }
else
{
    healthCheck.Status = HealthStatus.Warning;
    healthCheck.Message = "No active network connection detected";
}
}
catch (Exception ex)
{
    healthCheck.Status = HealthStatus.Unhealthy;
    healthCheck.Message = $"Network check failed: {ex.Message}";
}

UpdateHealthCheck(healthCheck);
}

private void CheckFileSystemHealth()
{
    var healthCheck = new HealthCheck;
    {
        CheckId = "FS",
        Name = "File System",
        CheckType = "Infrastructure",
        CheckedAt = DateTime.UtcNow
    };

try
{
    var tempPath = Path.GetTempPath();
    var testFile = Path.Combine(tempPath, $"ejlive_health_{Guid.NewGuid():N}.tmp");
    File.WriteAllText(testFile, "health_check");
    File.Delete(testFile);

    healthCheck.Status = HealthStatus.Healthy;
    healthCheck.Message = "File system is writable and operational";
    healthCheck.ResponseTimeMs = 15;
}
catch (Exception ex)
{
    healthCheck.Status = HealthStatus.Degraded;
    healthCheck.Message = $"File system check failed: {ex.Message}";
}

UpdateHealthCheck(healthCheck);
}

private void CheckServiceDependenciesHealth()
{
    var healthCheck = new HealthCheck;
    {
        CheckId = "SVC",
        Name = "Service Dependencies",
        CheckType = "Service",
        CheckedAt = DateTime.UtcNow
    };

try
{
    // Check essential Windows services
    var services = new[] { "W3SVC", "MSSQL$SQLEXPRESS", "EventLog" };
var healthyCount = 0;

foreach (var serviceName in services)
{
    try
    {
        var process = new Process;
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "sc.exe",
                Arguments = $"query {serviceName}",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
    };
process.Start();
var output = process.StandardOutput.ReadToEnd();
process.WaitForExit();

if (output.Contains("RUNNING"))
healthyCount++;
}
catch
{
    // Service not found or not running - this is fine
}
}

if (healthyCount >= 1) // At least one service is running
{
    healthCheck.Status = HealthStatus.Healthy;
    healthCheck.Message = $"{healthyCount}/{services.Length} essential services running";
}
else
{
    healthCheck.Status = HealthStatus.Warning;
    healthCheck.Message = "No essential services detected (this may be normal for standalone setup)";
}
}
catch (Exception ex)
{
    healthCheck.Status = HealthStatus.Degraded;
    healthCheck.Message = $"Service dependency check failed: {ex.Message}";
}

UpdateHealthCheck(healthCheck);
}

private void UpdateHealthCheck(HealthCheck healthCheck)
{
    lock (_lock)
    {
        _healthChecks[healthCheck.CheckId] = healthCheck;
        OnHealthCheckCompleted?.Invoke(healthCheck.CheckId, healthCheck);

        if (healthCheck.Status == HealthStatus.Unhealthy)
        {
            RaiseAlert(new SystemAlert
            {
                AlertId = Guid.NewGuid().ToString(),
                Severity = AlertSeverity.Error,
                Category = "Health Check",
                Title = $"Health Check Failed: {healthCheck.Name}",
                Message = healthCheck.Message,
                Source = "HealthCheckService",
                CreatedAt = DateTime.UtcNow
            });
    }
}
}

private void RaiseAlert(SystemAlert alert)
{
    lock (_lock)
    {
        _alerts.Add(alert);
        if (_alerts.Count > 1000)
        _alerts.RemoveRange(0, _alerts.Count - 1000);
    }

OnAlertRaised?.Invoke(alert);
}

// Public Methods
public PerformanceSnapshot GetPerformanceSnapshot()
{
    lock (_lock)
    {
        return new PerformanceSnapshot
        {
            Timestamp = DateTime.UtcNow,
            Metrics = _performanceMetrics.Values
            .ToDictionary(m => m.Key, m => m.Clone()),
            HealthChecks = _healthChecks.Values
            .ToDictionary(h => h.CheckId, h => h.Clone()),
            ActiveAlerts = _alerts
            .Where(a => !a.IsResolved)
            .OrderByDescending(a => a.CreatedAt)
            .Take(50)
            .ToList()
        };
}
}

public List<SystemAlert> GetActiveAlerts(int maxCount = 50)
{
    lock (_lock)
    {
        return _alerts
        .Where(a => !a.IsResolved)
        .OrderByDescending(a => a.CreatedAt)
        .Take(maxCount)
        .ToList();
    }
}

public void ResolveAlert(string alertId)
{
    lock (_lock)
    {
        var alert = _alerts.FirstOrDefault(a => a.AlertId == alertId);
        if (alert != null)
        {
            alert.IsResolved = true;
            alert.ResolvedAt = DateTime.UtcNow;
        }
}
}

public double GetSystemHealthScore()
{
    lock (_lock)
    {
        var score = 100.0;
        var failedChecks = _healthChecks.Values.Count(h => h.Status == HealthStatus.Unhealthy);
        var degradedChecks = _healthChecks.Values.Count(h => h.Status == HealthStatus.Degraded);
        var warningChecks = _healthChecks.Values.Count(h => h.Status == HealthStatus.Warning);

        score -= failedChecks * 20;
        score -= degradedChecks * 10;
        score -= warningChecks * 5;

        // Consider performance metrics
        foreach (var metric in _performanceMetrics.Values)
        {
            if (metric.Key == "CPU_Usage" && metric.CurrentValue > 90)
            score -= 15;
            else if (metric.Key == "CPU_Usage" && metric.CurrentValue > 75)
            score -= 5;
            else if (metric.Key == "Memory_Usage" && metric.CurrentValue > 600)
            score -= 10;
            else if (metric.Key == "Error_Rate" && metric.CurrentValue > 10)
            score -= 20;
            else if (metric.Key == "Error_Rate" && metric.CurrentValue > 5)
            score -= 10;
        }

    return Math.Max(0, Math.Min(100, score));
}
}

public void Dispose()
{
    if (!_disposed)
    {
        _monitoringTimer?.Dispose();
        _healthCheckTimer?.Dispose();
        _disposed = true;
    }
}
}
public partial public public class AdvancedMonitoringService : IDisposable
{
    private readonly Dictionary<string, PerformanceMetric> _performanceMetrics;
    private readonly Dictionary<string, HealthCheck> _healthChecks;
    private readonly List<SystemAlert> _alerts;
    private readonly Timer _monitoringTimer;
    private readonly Timer _healthCheckTimer;
    private readonly object _lock = new object();
    private bool _disposed;
    public AdvancedMonitoringService()
    {
        private void InitializeDefaultMetrics()
        {
            private void AddMetric(string key, string displayName, string unit, MetricType type)
            {
                private void MonitoringCallback(object? state)
                {
                    private void UpdateSystemMetrics()
                    {
                        private double GetCpuUsage()
                        {
                            private double GetDiskUsage()
                            {
                                private void UpdateMetricValue(string key, double value)
                                {
                                    private void CheckThresholds()
                                    {
                                        private void CheckMetricThreshold(PerformanceMetric metric, double warningThreshold, double errorThreshold, string alertTitle)
                                        {
                                            private void HealthCheckCallback(object? state)
                                            {
                                                private void PerformHealthChecks()
                                                {
                                                    private void CheckDatabaseHealth()
                                                    {
                                                        private void CheckNetworkHealth()
                                                        {
                                                            private void CheckFileSystemHealth()
                                                            {
                                                                private void CheckServiceDependenciesHealth()
                                                                {
                                                                    private void UpdateHealthCheck(HealthCheck healthCheck)
                                                                    {
                                                                        private void RaiseAlert(SystemAlert alert)
                                                                        {
                                                                            public PerformanceSnapshot GetPerformanceSnapshot()
                                                                            {
                                                                                public List<SystemAlert> GetActiveAlerts(int maxCount = 50)
                                                                                {
                                                                                    public void ResolveAlert(string alertId)
                                                                                    {
                                                                                        public double GetSystemHealthScore()
                                                                                        {
                                                                                            public void Dispose()
                                                                                            {
                                                                                            }

                                                                                    }
                                                                                public partial public class AdvancedMonitoringService : IDisposable
                                                                                {
                                                                                    private readonly Dictionary<string, PerformanceMetric> _performanceMetrics;
                                                                                    private readonly Dictionary<string, HealthCheck> _healthChecks;
                                                                                    private readonly List<SystemAlert> _alerts;
                                                                                    private readonly Timer _monitoringTimer;
                                                                                    private readonly Timer _healthCheckTimer;
                                                                                    private readonly object _lock = new object();
                                                                                    private bool _disposed;
                                                                                    public AdvancedMonitoringService()
                                                                                    {
                                                                                        private void InitializeDefaultMetrics()
                                                                                        {
                                                                                            private void AddMetric(string key, string displayName, string unit, MetricType type)
                                                                                            {
                                                                                                private void MonitoringCallback(object? state)
                                                                                                {
                                                                                                    private void UpdateSystemMetrics()
                                                                                                    {
                                                                                                        private double GetCpuUsage()
                                                                                                        {
                                                                                                            private double GetDiskUsage()
                                                                                                            {
                                                                                                                private void UpdateMetricValue(string key, double value)
                                                                                                                {
                                                                                                                    private void CheckThresholds()
                                                                                                                    {
                                                                                                                        private void CheckMetricThreshold(PerformanceMetric metric, double warningThreshold, double errorThreshold, string alertTitle)
                                                                                                                        {
                                                                                                                            private void HealthCheckCallback(object? state)
                                                                                                                            {
                                                                                                                                private void PerformHealthChecks()
                                                                                                                                {
                                                                                                                                    private void CheckDatabaseHealth()
                                                                                                                                    {
                                                                                                                                        private void CheckNetworkHealth()
                                                                                                                                        {
                                                                                                                                            private void CheckFileSystemHealth()
                                                                                                                                            {
                                                                                                                                                private void CheckServiceDependenciesHealth()
                                                                                                                                                {
                                                                                                                                                    private void UpdateHealthCheck(HealthCheck healthCheck)
                                                                                                                                                    {
                                                                                                                                                        private void RaiseAlert(SystemAlert alert)
                                                                                                                                                        {
                                                                                                                                                            public PerformanceSnapshot GetPerformanceSnapshot()
                                                                                                                                                            {
                                                                                                                                                                public List<SystemAlert> GetActiveAlerts(int maxCount = 50)
                                                                                                                                                                {
                                                                                                                                                                    public void ResolveAlert(string alertId)
                                                                                                                                                                    {
                                                                                                                                                                        public double GetSystemHealthScore()
                                                                                                                                                                        {
                                                                                                                                                                            public void Dispose()
                                                                                                                                                                            {
                                                                                                                                                                            }

                                                                                                                                                                    }
                                                                                                                                                            }