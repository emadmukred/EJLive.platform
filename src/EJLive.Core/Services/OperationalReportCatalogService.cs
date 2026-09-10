namespace EJLive.Core.Services;

public sealed class OperationalReportCatalogService
{
    public IReadOnlyList<OperationalReportFileDescriptor> GetLatestReportFiles(string reportsFolder, int maxCount = 100)
    {
        if (string.IsNullOrWhiteSpace(reportsFolder) || !Directory.Exists(reportsFolder))
            return Array.Empty<OperationalReportFileDescriptor>();

        var take = Math.Clamp(maxCount, 1, 2000);
        return Directory.EnumerateFiles(reportsFolder, "*", SearchOption.TopDirectoryOnly)
            .Select(path => new FileInfo(path))
            .Where(file => file.Exists)
            .OrderByDescending(file => file.LastWriteTimeUtc)
            .Take(take)
            .Select(file => new OperationalReportFileDescriptor(
                file.Name,
                file.FullName,
                ResolveCategory(file.Name),
                file.LastWriteTime,
                file.Length))
            .ToArray();
    }

    public OperationalBundleSummaryPreview LoadLatestBundleSummary(string reportsFolder)
    {
        if (string.IsNullOrWhiteSpace(reportsFolder) || !Directory.Exists(reportsFolder))
            return new OperationalBundleSummaryPreview(string.Empty, Array.Empty<OperationalBundleWindowSummary>());

        var latest = Directory.EnumerateFiles(reportsFolder, "ops-bundle-summary-*.csv", SearchOption.TopDirectoryOnly)
            .Select(path => new FileInfo(path))
            .Where(file => file.Exists)
            .OrderByDescending(file => file.LastWriteTimeUtc)
            .FirstOrDefault();
        if (latest is null)
            return new OperationalBundleSummaryPreview(string.Empty, Array.Empty<OperationalBundleWindowSummary>());

        var rows = LoadBundleSummaryFromFile(latest.FullName);
        return new OperationalBundleSummaryPreview(latest.FullName, rows);
    }

    public IReadOnlyList<OperationalBundleWindowSummary> LoadBundleSummaryFromFile(string csvPath)
    {
        if (string.IsNullOrWhiteSpace(csvPath) || !File.Exists(csvPath))
            return Array.Empty<OperationalBundleWindowSummary>();

        var lines = File.ReadAllLines(csvPath);
        if (lines.Length < 2)
            return Array.Empty<OperationalBundleWindowSummary>();

        var header = ParseCsvLine(lines[0]);
        var windowIndex = IndexOfHeader(header, "window");
        var hoursIndex = IndexOfHeader(header, "lookback_hours");
        var fleetTotalIndex = IndexOfHeader(header, "fleet_total");
        var fleetConnectedIndex = IndexOfHeader(header, "fleet_connected");
        var fleetOfflineIndex = IndexOfHeader(header, "fleet_offline");
        var syncOpenIndex = IndexOfHeader(header, "sync_open");
        var syncFailedIndex = IndexOfHeader(header, "sync_failed");
        var pendingDeliveryIndex = IndexOfHeader(header, "pending_delivery");
        var commandFailuresIndex = IndexOfHeader(header, "command_failures");
        var telemetryWarningsIndex = IndexOfHeader(header, "telemetry_warnings");
        var telemetryErrorsIndex = IndexOfHeader(header, "telemetry_errors");

        if (windowIndex < 0 || hoursIndex < 0)
            return Array.Empty<OperationalBundleWindowSummary>();

        var rows = new List<OperationalBundleWindowSummary>(Math.Max(0, lines.Length - 1));
        foreach (var rawLine in lines.Skip(1))
        {
            if (string.IsNullOrWhiteSpace(rawLine))
                continue;

            var values = ParseCsvLine(rawLine);
            var row = new OperationalBundleWindowSummary(
                Window: ReadString(values, windowIndex),
                LookbackHours: ReadInt(values, hoursIndex),
                FleetTotal: ReadInt(values, fleetTotalIndex),
                FleetConnected: ReadInt(values, fleetConnectedIndex),
                FleetOffline: ReadInt(values, fleetOfflineIndex),
                SyncOpen: ReadInt(values, syncOpenIndex),
                SyncFailed: ReadInt(values, syncFailedIndex),
                PendingDelivery: ReadInt(values, pendingDeliveryIndex),
                CommandFailures: ReadInt(values, commandFailuresIndex),
                TelemetryWarnings: ReadInt(values, telemetryWarningsIndex),
                TelemetryErrors: ReadInt(values, telemetryErrorsIndex));
            if (!string.IsNullOrWhiteSpace(row.Window))
                rows.Add(row);
        }

        return rows;
    }

    private static string ResolveCategory(string fileName)
    {
        var name = (fileName ?? string.Empty).Trim();
        if (name.StartsWith("ops-bundle-summary-", StringComparison.OrdinalIgnoreCase))
            return "ops bundle summary";
        if (name.StartsWith("ops-bundle-atm-", StringComparison.OrdinalIgnoreCase))
            return "ops bundle atm";
        if (name.StartsWith("ops-bundle-", StringComparison.OrdinalIgnoreCase) &&
            name.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            return "ops bundle json";
        if (name.StartsWith("ops-", StringComparison.OrdinalIgnoreCase) &&
            name.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            return "ops window json";
        if (name.StartsWith("ops-", StringComparison.OrdinalIgnoreCase) &&
            name.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            return "ops window csv";
        if (name.StartsWith("fleet-health-", StringComparison.OrdinalIgnoreCase))
            return "fleet health";
        if (name.StartsWith("command-audit-", StringComparison.OrdinalIgnoreCase))
            return "command audit";
        if (name.StartsWith("monitoring-dashboard-", StringComparison.OrdinalIgnoreCase))
            return "monitoring snapshot";
        if (name.StartsWith("archive-cleanup-", StringComparison.OrdinalIgnoreCase))
            return "archive cleanup";
        return "other";
    }

    private static int IndexOfHeader(IReadOnlyList<string> header, string key)
    {
        for (var index = 0; index < header.Count; index++)
        {
            if (string.Equals(header[index], key, StringComparison.OrdinalIgnoreCase))
                return index;
        }

        return -1;
    }

    private static string ReadString(IReadOnlyList<string> values, int index)
    {
        if (index < 0 || index >= values.Count)
            return string.Empty;
        return values[index].Trim();
    }

    private static int ReadInt(IReadOnlyList<string> values, int index)
    {
        if (index < 0 || index >= values.Count)
            return 0;
        return int.TryParse(values[index], out var parsed) ? parsed : 0;
    }

    private static IReadOnlyList<string> ParseCsvLine(string line)
    {
        var values = new List<string>();
        if (line is null)
            return values;

        var current = new System.Text.StringBuilder();
        var inQuotes = false;
        for (var i = 0; i < line.Length; i++)
        {
            var ch = line[i];
            if (ch == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    current.Append('"');
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }
                continue;
            }

            if (ch == ',' && !inQuotes)
            {
                values.Add(current.ToString());
                current.Clear();
                continue;
            }

            current.Append(ch);
        }

        values.Add(current.ToString());
        return values;
    }
}

public sealed record OperationalReportFileDescriptor(
    string FileName,
    string FullPath,
    string Category,
    DateTime ModifiedAtLocal,
    long SizeBytes);

public sealed record OperationalBundleSummaryPreview(
    string SourceFilePath,
    IReadOnlyList<OperationalBundleWindowSummary> Rows);

public sealed record OperationalBundleWindowSummary(
    string Window,
    int LookbackHours,
    int FleetTotal,
    int FleetConnected,
    int FleetOffline,
    int SyncOpen,
    int SyncFailed,
    int PendingDelivery,
    int CommandFailures,
    int TelemetryWarnings,
    int TelemetryErrors);
