using EJLive.Core.Models;
using System.Text;

namespace EJLive.Core.Services;

public sealed class ClientTelemetryAnalyticsService
{
    public ClientTelemetryAnalyticsSnapshot BuildSnapshot(
        IEnumerable<AuditLogEntry>? auditEntries,
        int maxTimelineRows = 1000,
        int maxAtmRows = 500)
    {
        var events = ExtractTelemetryEvents(auditEntries)
            .OrderByDescending(item => item.CreatedAtUtc)
            .Take(Math.Max(1, maxTimelineRows))
            .ToArray();

        var totalEvents = events.Length;
        var warningEvents = events.Count(IsWarning);
        var errorEvents = events.Count(IsError);
        var latestEventUtc = events.Select(item => (DateTime?)item.CreatedAtUtc).FirstOrDefault();

        var atmSummaries = events
            .GroupBy(item => item.ATM_ID, StringComparer.OrdinalIgnoreCase)
            .Select(group =>
            {
                var latest = group.OrderByDescending(item => item.CreatedAtUtc).First();
                return new ClientTelemetryAtmSummaryRow(
                    ATM_ID: group.Key,
                    TotalEvents: group.Count(),
                    WarningEvents: group.Count(IsWarning),
                    ErrorEvents: group.Count(IsError),
                    LastEventType: latest.EventType,
                    LastEventUtc: latest.CreatedAtUtc);
            })
            .OrderByDescending(item => item.ErrorEvents)
            .ThenByDescending(item => item.WarningEvents)
            .ThenByDescending(item => item.LastEventUtc)
            .Take(Math.Max(1, maxAtmRows))
            .ToArray();

        var topEventTypes = events
            .GroupBy(item => item.EventType, StringComparer.OrdinalIgnoreCase)
            .Select(group => new TelemetryEventTypeSummaryRow(group.Key, group.Count()))
            .OrderByDescending(item => item.Count)
            .ThenBy(item => item.EventType, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return new ClientTelemetryAnalyticsSnapshot(
            TotalEvents: totalEvents,
            WarningEvents: warningEvents,
            ErrorEvents: errorEvents,
            DistinctAtms: atmSummaries.Length,
            LatestEventUtc: latestEventUtc,
            TimelineRows: events,
            AtmSummaryRows: atmSummaries,
            TopEventTypes: topEventTypes);
    }

    public string ExportTimelineCsv(string reportsFolder, ClientTelemetryAnalyticsSnapshot snapshot, DateTime generatedAtLocal)
    {
        Directory.CreateDirectory(reportsFolder);
        var path = Path.Combine(reportsFolder, $"telemetry-timeline-{generatedAtLocal:yyyyMMdd-HHmmss}.csv");
        using var writer = new StreamWriter(path, false, Encoding.UTF8);
        writer.WriteLine("created_at_utc,atm_id,severity,event_type,detail");
        foreach (var row in snapshot.TimelineRows)
        {
            writer.WriteLine(string.Join(",",
                Csv(row.CreatedAtUtc.ToString("O")),
                Csv(row.ATM_ID),
                Csv(row.Severity),
                Csv(row.EventType),
                Csv(row.Detail)));
        }

        return path;
    }

    public string ExportAtmSummaryCsv(string reportsFolder, ClientTelemetryAnalyticsSnapshot snapshot, DateTime generatedAtLocal)
    {
        Directory.CreateDirectory(reportsFolder);
        var path = Path.Combine(reportsFolder, $"telemetry-atm-summary-{generatedAtLocal:yyyyMMdd-HHmmss}.csv");
        using var writer = new StreamWriter(path, false, Encoding.UTF8);
        writer.WriteLine("atm_id,total_events,warning_events,error_events,last_event_type,last_event_utc");
        foreach (var row in snapshot.AtmSummaryRows)
        {
            writer.WriteLine(string.Join(",",
                Csv(row.ATM_ID),
                Csv(row.TotalEvents),
                Csv(row.WarningEvents),
                Csv(row.ErrorEvents),
                Csv(row.LastEventType),
                Csv(row.LastEventUtc.ToString("O"))));
        }

        return path;
    }

    private static IReadOnlyList<ClientTelemetryEventRow> ExtractTelemetryEvents(IEnumerable<AuditLogEntry>? auditEntries)
    {
        if (auditEntries is null)
            return Array.Empty<ClientTelemetryEventRow>();

        var rows = new List<ClientTelemetryEventRow>();
        foreach (var entry in auditEntries)
        {
            if (!TryParseTelemetryEntry(entry, out var row))
                continue;
            rows.Add(row);
        }

        return rows;
    }

    private static bool TryParseTelemetryEntry(AuditLogEntry entry, out ClientTelemetryEventRow row)
    {
        row = default;
        var action = entry.Action ?? string.Empty;
        if (!action.Contains("ClientTelemetry", StringComparison.OrdinalIgnoreCase))
            return false;

        var atmId = string.IsNullOrWhiteSpace(entry.Target) ? "UNKNOWN" : entry.Target.Trim();
        var parts = (entry.Details ?? string.Empty).Split('|', 3, StringSplitOptions.None);
        var severity = NormalizeSeverity(parts.ElementAtOrDefault(0));
        var eventType = NormalizeEventType(parts.ElementAtOrDefault(1));
        var detail = parts.ElementAtOrDefault(2) ?? string.Empty;
        var occurredAt = entry.CreatedAtUtc == DateTime.MinValue ? DateTime.UtcNow : entry.CreatedAtUtc;

        row = new ClientTelemetryEventRow(
            CreatedAtUtc: occurredAt,
            ATM_ID: atmId,
            Severity: severity,
            EventType: eventType,
            Detail: detail);
        return true;
    }

    private static string NormalizeSeverity(string? value)
    {
        var severity = (value ?? string.Empty).Trim().ToLowerInvariant();
        return severity.Length == 0 ? "info" : severity;
    }

    private static string NormalizeEventType(string? value)
    {
        var eventType = (value ?? string.Empty).Trim();
        return eventType.Length == 0 ? "event" : eventType;
    }

    private static bool IsWarning(ClientTelemetryEventRow row)
        => row.Severity is "warning" or "warn";

    private static bool IsError(ClientTelemetryEventRow row)
        => row.Severity is "error" or "critical" or "fatal";

    private static string Csv(object? value)
    {
        var text = Convert.ToString(value) ?? string.Empty;
        return "\"" + text.Replace("\"", "\"\"", StringComparison.Ordinal) + "\"";
    }
}

public sealed record ClientTelemetryAnalyticsSnapshot(
    int TotalEvents,
    int WarningEvents,
    int ErrorEvents,
    int DistinctAtms,
    DateTime? LatestEventUtc,
    IReadOnlyList<ClientTelemetryEventRow> TimelineRows,
    IReadOnlyList<ClientTelemetryAtmSummaryRow> AtmSummaryRows,
    IReadOnlyList<TelemetryEventTypeSummaryRow> TopEventTypes);

public readonly record struct ClientTelemetryEventRow(
    DateTime CreatedAtUtc,
    string ATM_ID,
    string Severity,
    string EventType,
    string Detail);

public sealed record ClientTelemetryAtmSummaryRow(
    string ATM_ID,
    int TotalEvents,
    int WarningEvents,
    int ErrorEvents,
    string LastEventType,
    DateTime LastEventUtc);

public sealed record TelemetryEventTypeSummaryRow(
    string EventType,
    int Count);
