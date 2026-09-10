using System.Text;
using System.Text.Json;

namespace EJLive.Core.Services;

public sealed class UnifiedOperationalReportingService
{
    public OperationalWindowReportResult ExportWindowReport(
        string outputFolder,
        string windowName,
        int lookbackHours,
        UnifiedServerAnalyticsSnapshot snapshot,
        DateTime generatedAtLocal,
        DateTime generatedAtUtc)
    {
        var safeWindowName = string.IsNullOrWhiteSpace(windowName) ? "window" : windowName.Trim().ToLowerInvariant();
        var safeHours = Math.Max(1, lookbackHours);

        Directory.CreateDirectory(outputFolder);
        var stamp = generatedAtLocal.ToString("yyyyMMdd-HHmmss");
        var jsonPath = Path.Combine(outputFolder, $"ops-{safeWindowName}-{stamp}.json");
        var csvPath = Path.Combine(outputFolder, $"ops-{safeWindowName}-{stamp}.csv");

        var payload = new
        {
            generatedAtUtc,
            window = safeWindowName,
            lookbackHours = safeHours,
            snapshot
        };

        File.WriteAllText(jsonPath, JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = true }));
        WriteWindowCsv(csvPath, snapshot, safeWindowName, safeHours);

        return new OperationalWindowReportResult(safeWindowName, safeHours, jsonPath, csvPath);
    }

    public OperationalBundleReportResult ExportBundleReport(
        string outputFolder,
        IEnumerable<OperationalWindowSnapshot> snapshots,
        DateTime generatedAtLocal,
        DateTime generatedAtUtc)
    {
        var windows = (snapshots ?? Array.Empty<OperationalWindowSnapshot>()).ToArray();
        if (windows.Length == 0)
            throw new InvalidOperationException("At least one window snapshot is required to build the bundle report.");

        Directory.CreateDirectory(outputFolder);
        var stamp = generatedAtLocal.ToString("yyyyMMdd-HHmmss");
        var jsonPath = Path.Combine(outputFolder, $"ops-bundle-{stamp}.json");
        var summaryCsvPath = Path.Combine(outputFolder, $"ops-bundle-summary-{stamp}.csv");
        var atmCsvPath = Path.Combine(outputFolder, $"ops-bundle-atm-{stamp}.csv");

        var payload = new
        {
            generatedAtUtc,
            windows = windows.Select(item => new
            {
                window = item.WindowName,
                lookbackHours = item.LookbackHours,
                snapshot = item.Snapshot
            })
        };

        File.WriteAllText(jsonPath, JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = true }));
        WriteBundleSummaryCsv(summaryCsvPath, windows);
        WriteBundleAtmCsv(atmCsvPath, windows);

        return new OperationalBundleReportResult(jsonPath, summaryCsvPath, atmCsvPath);
    }

    public string ExportFleetHealthReport(
        string outputFolder,
        UnifiedServerAnalyticsSnapshot snapshot,
        DateTime generatedAtLocal)
    {
        Directory.CreateDirectory(outputFolder);
        var path = Path.Combine(outputFolder, $"fleet-health-{generatedAtLocal:yyyyMMdd-HHmmss}.csv");
        using var writer = new StreamWriter(path, false, Encoding.UTF8);
        writer.WriteLine("atm_id,atm_type,connection,health,sync_open,sync_failed,pending_delivery,command_failures,telemetry_warnings,telemetry_errors,last_telemetry,last_heartbeat,heartbeat_age_minutes");

        foreach (var row in snapshot.AtmRows.OrderBy(item => item.ATM_ID, StringComparer.OrdinalIgnoreCase))
        {
            writer.WriteLine(string.Join(",",
                Csv(row.ATM_ID),
                Csv(row.ATM_Type),
                Csv(row.ConnectionStatus.ToString()),
                Csv(row.HealthScore),
                Csv(row.SyncOpen),
                Csv(row.SyncFailed),
                Csv(row.PendingDeliveries),
                Csv(row.CommandFailures),
                Csv(row.TelemetryWarnings),
                Csv(row.TelemetryErrors),
                Csv(row.LastTelemetryAtUtc?.ToString("O") ?? string.Empty),
                Csv(row.LastHeartbeatUtc?.ToString("O") ?? string.Empty),
                Csv(row.MinutesSinceHeartbeat == int.MaxValue ? string.Empty : row.MinutesSinceHeartbeat)));
        }

        return path;
    }

    private static void WriteWindowCsv(string path, UnifiedServerAnalyticsSnapshot snapshot, string windowName, int lookbackHours)
    {
        using var writer = new StreamWriter(path, false, Encoding.UTF8);
        writer.WriteLine("window,lookback_hours,atm_id,atm_type,connection,health,sync_open,sync_failed,sync_completed,pending_delivery,command_failures,telemetry_warnings,telemetry_errors,last_telemetry,last_heartbeat,heartbeat_age_minutes");
        foreach (var row in snapshot.AtmRows.OrderBy(item => item.ATM_ID, StringComparer.OrdinalIgnoreCase))
        {
            writer.WriteLine(string.Join(",",
                Csv(windowName),
                Csv(lookbackHours),
                Csv(row.ATM_ID),
                Csv(row.ATM_Type),
                Csv(row.ConnectionStatus.ToString()),
                Csv(row.HealthScore),
                Csv(row.SyncOpen),
                Csv(row.SyncFailed),
                Csv(row.SyncCompleted),
                Csv(row.PendingDeliveries),
                Csv(row.CommandFailures),
                Csv(row.TelemetryWarnings),
                Csv(row.TelemetryErrors),
                Csv(row.LastTelemetryAtUtc?.ToString("O") ?? string.Empty),
                Csv(row.LastHeartbeatUtc?.ToString("O") ?? string.Empty),
                Csv(row.MinutesSinceHeartbeat == int.MaxValue ? string.Empty : row.MinutesSinceHeartbeat)));
        }
    }

    private static void WriteBundleSummaryCsv(string path, IReadOnlyList<OperationalWindowSnapshot> snapshots)
    {
        using var writer = new StreamWriter(path, false, Encoding.UTF8);
        writer.WriteLine("window,lookback_hours,fleet_total,fleet_connected,fleet_offline,sync_open,sync_failed,pending_delivery,failed_delivery,command_failures,telemetry_events,telemetry_warnings,telemetry_errors,last_command_utc,last_telemetry_utc");

        foreach (var item in snapshots)
        {
            var snapshot = item.Snapshot;
            writer.WriteLine(string.Join(",",
                Csv(item.WindowName),
                Csv(item.LookbackHours),
                Csv(snapshot.Fleet.Total),
                Csv(snapshot.Fleet.Connected),
                Csv(snapshot.Fleet.Offline),
                Csv(snapshot.Sync.OpenItems),
                Csv(snapshot.Sync.Failed),
                Csv(snapshot.PendingDeliveries),
                Csv(snapshot.FailedDeliveries),
                Csv(snapshot.CommandFailures),
                Csv(snapshot.TelemetryEvents),
                Csv(snapshot.TelemetryWarnings),
                Csv(snapshot.TelemetryErrors),
                Csv(snapshot.LastCommandAtUtc?.ToString("O") ?? string.Empty),
                Csv(snapshot.LastTelemetryAtUtc?.ToString("O") ?? string.Empty)));
        }
    }

    private static void WriteBundleAtmCsv(string path, IReadOnlyList<OperationalWindowSnapshot> snapshots)
    {
        using var writer = new StreamWriter(path, false, Encoding.UTF8);
        writer.WriteLine("window,lookback_hours,atm_id,atm_type,connection,health,sync_open,sync_failed,pending_delivery,command_failures,telemetry_warnings,telemetry_errors,last_telemetry,last_heartbeat,heartbeat_age_minutes");

        foreach (var item in snapshots)
        {
            foreach (var row in item.Snapshot.AtmRows.OrderBy(entry => entry.ATM_ID, StringComparer.OrdinalIgnoreCase))
            {
                writer.WriteLine(string.Join(",",
                    Csv(item.WindowName),
                    Csv(item.LookbackHours),
                    Csv(row.ATM_ID),
                    Csv(row.ATM_Type),
                    Csv(row.ConnectionStatus.ToString()),
                    Csv(row.HealthScore),
                    Csv(row.SyncOpen),
                    Csv(row.SyncFailed),
                    Csv(row.PendingDeliveries),
                    Csv(row.CommandFailures),
                    Csv(row.TelemetryWarnings),
                    Csv(row.TelemetryErrors),
                    Csv(row.LastTelemetryAtUtc?.ToString("O") ?? string.Empty),
                    Csv(row.LastHeartbeatUtc?.ToString("O") ?? string.Empty),
                    Csv(row.MinutesSinceHeartbeat == int.MaxValue ? string.Empty : row.MinutesSinceHeartbeat)));
            }
        }
    }

    private static string Csv(object? value)
    {
        var text = Convert.ToString(value) ?? string.Empty;
        return "\"" + text.Replace("\"", "\"\"", StringComparison.Ordinal) + "\"";
    }
}

public sealed record OperationalWindowSnapshot(
    string WindowName,
    int LookbackHours,
    UnifiedServerAnalyticsSnapshot Snapshot);

public sealed record OperationalWindowReportResult(
    string WindowName,
    int LookbackHours,
    string JsonPath,
    string CsvPath);

public sealed record OperationalBundleReportResult(
    string JsonPath,
    string SummaryCsvPath,
    string AtmCsvPath);
