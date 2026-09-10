using EJLive.Core.Models;

namespace EJLive.Core.Services;

public sealed class UnifiedServerAnalyticsService
{
    public UnifiedServerAnalyticsSnapshot BuildSnapshot(
        IEnumerable<ATMInfo>? atms,
        IEnumerable<JournalSyncRecord>? syncRecords,
        IEnumerable<JournalDeliveryReceipt>? deliveries,
        IEnumerable<AuditLogEntry>? auditEntries,
        DateTime? nowUtc = null)
    {
        var now = nowUtc ?? DateTime.UtcNow;
        var atmList = (atms ?? Array.Empty<ATMInfo>()).ToArray();
        var syncList = (syncRecords ?? Array.Empty<JournalSyncRecord>()).ToArray();
        var deliveryList = (deliveries ?? Array.Empty<JournalDeliveryReceipt>()).ToArray();
        var auditList = (auditEntries ?? Array.Empty<AuditLogEntry>())
            .OrderByDescending(entry => entry.CreatedAtUtc)
            .ToArray();

        var fleetSummary = BuildFleetSummary(atmList);
        var syncSummary = BuildSyncSummary(syncList);
        var commandDispatches = auditList.Count(IsCommandDispatch);
        var commandResults = auditList.Count(IsCommandResult);
        var commandFailures = auditList.Count(IsCommandFailure);
        var lastCommandAt = auditList
            .Where(entry => IsCommandAudit(entry.Action))
            .Select(entry => (DateTime?)entry.CreatedAtUtc)
            .FirstOrDefault();
        var telemetryEvents = ExtractTelemetryEvents(auditList);
        var telemetryWarnings = telemetryEvents.Count(IsTelemetryWarning);
        var telemetryErrors = telemetryEvents.Count(IsTelemetryError);
        var networkDisconnectEvents = telemetryEvents.Count(item =>
            item.EventType.Contains("network_disconnected", StringComparison.OrdinalIgnoreCase));
        var handshakeMissingEvents = telemetryEvents.Count(item =>
            item.EventType.Contains("handshake_missing", StringComparison.OrdinalIgnoreCase));
        var fileRetryEvents = telemetryEvents.Count(item =>
            item.EventType.Contains("file_retry", StringComparison.OrdinalIgnoreCase));
        var lastTelemetryAt = telemetryEvents
            .Select(item => (DateTime?)item.CreatedAtUtc)
            .OrderByDescending(item => item)
            .FirstOrDefault();

        var rows = BuildAtmRows(now, atmList, syncList, deliveryList, auditList, telemetryEvents);

        return new UnifiedServerAnalyticsSnapshot(
            fleetSummary,
            syncSummary,
            deliveryList.Count(item => item.Confirmed),
            deliveryList.Count(item => !item.Confirmed),
            deliveryList.Count(item => !item.Confirmed &&
                                       item.Detail.Contains("fail", StringComparison.OrdinalIgnoreCase)),
            commandDispatches,
            commandResults,
            commandFailures,
            lastCommandAt,
            telemetryEvents.Count,
            telemetryWarnings,
            telemetryErrors,
            networkDisconnectEvents,
            handshakeMissingEvents,
            fileRetryEvents,
            lastTelemetryAt,
            rows);
    }

    private static FleetSummary BuildFleetSummary(IReadOnlyCollection<ATMInfo> atms)
    {
        if (atms.Count == 0)
            return new FleetSummary();

        return new FleetSummary
        {
            Total = atms.Count,
            Connected = atms.Count(atm => atm.ConnectionStatus != ConnectionStatus.Disconnected),
            Syncing = atms.Count(atm => atm.ConnectionStatus == ConnectionStatus.Syncing),
            Offline = atms.Count(atm => atm.ConnectionStatus == ConnectionStatus.Disconnected),
            AverageHealth = (int)Math.Round(atms.Average(atm => atm.HealthScore), MidpointRounding.AwayFromZero)
        };
    }

    private static SyncSummary BuildSyncSummary(IReadOnlyCollection<JournalSyncRecord> records)
    {
        if (records.Count == 0)
            return new SyncSummary();

        return new SyncSummary
        {
            Total = records.Count,
            Pending = records.Count(record => record.State == JournalSyncState.Pending),
            InProgress = records.Count(record => record.State is JournalSyncState.Syncing or JournalSyncState.ReSyncing),
            Completed = records.Count(record => record.State == JournalSyncState.Completed),
            Failed = records.Count(record => record.State == JournalSyncState.Failed),
            AverageProgress = (int)Math.Round(records.Average(record => record.ProgressPercent), MidpointRounding.AwayFromZero)
        };
    }

    private static IReadOnlyList<UnifiedAtmOperationalAnalyticsRow> BuildAtmRows(
        DateTime nowUtc,
        IReadOnlyCollection<ATMInfo> atms,
        IReadOnlyCollection<JournalSyncRecord> syncRecords,
        IReadOnlyCollection<JournalDeliveryReceipt> deliveries,
        IReadOnlyCollection<AuditLogEntry> auditEntries,
        IReadOnlyCollection<TelemetryAuditEntry> telemetryEntries)
    {
        var ids = atms
            .Select(atm => atm.ATM_ID)
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Select(id => id!.Trim())
            .Concat(syncRecords
                .Select(record => record.ATM_ID)
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Select(id => id.Trim()))
            .Concat(deliveries
                .Select(record => record.ATM_ID)
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Select(id => id.Trim()))
            .Concat(auditEntries
                .Select(entry => entry.Target)
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Select(id => id.Trim()))
            .Concat(telemetryEntries
                .Select(item => item.ATM_ID)
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Select(id => id.Trim()))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(id => id, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var rows = new List<UnifiedAtmOperationalAnalyticsRow>(ids.Length);
        foreach (var atmId in ids)
        {
            var atm = atms.FirstOrDefault(item => string.Equals(item.ATM_ID, atmId, StringComparison.OrdinalIgnoreCase));
            var atmSync = syncRecords.Where(record => string.Equals(record.ATM_ID, atmId, StringComparison.OrdinalIgnoreCase)).ToArray();
            var atmDeliveries = deliveries.Where(record => string.Equals(record.ATM_ID, atmId, StringComparison.OrdinalIgnoreCase)).ToArray();
            var atmAudit = auditEntries.Where(entry => string.Equals(entry.Target, atmId, StringComparison.OrdinalIgnoreCase)).ToArray();
            var atmTelemetry = telemetryEntries.Where(entry => string.Equals(entry.ATM_ID, atmId, StringComparison.OrdinalIgnoreCase)).ToArray();
            var lastHeartbeatUtc = atm?.LastHeartbeatUtc;
            var lastTelemetryAtUtc = atmTelemetry
                .Select(entry => (DateTime?)entry.CreatedAtUtc)
                .OrderByDescending(entry => entry)
                .FirstOrDefault();
            var minutesSinceHeartbeat = lastHeartbeatUtc.HasValue && lastHeartbeatUtc.Value > DateTime.MinValue
                ? Math.Max(0, (int)Math.Round((nowUtc - lastHeartbeatUtc.Value).TotalMinutes))
                : int.MaxValue;

            rows.Add(new UnifiedAtmOperationalAnalyticsRow(
                atmId,
                atm?.ATM_Type ?? "UNKNOWN",
                atm?.HealthScore ?? 0,
                atm?.ConnectionStatus ?? ConnectionStatus.Disconnected,
                atmSync.Count(record => record.State is JournalSyncState.Pending or JournalSyncState.Syncing or JournalSyncState.ReSyncing),
                atmSync.Count(record => record.State == JournalSyncState.Failed),
                atmSync.Count(record => record.State == JournalSyncState.Completed),
                atmDeliveries.Count(record => !record.Confirmed),
                atmAudit.Count(IsCommandFailure),
                atmTelemetry.Count(IsTelemetryWarning),
                atmTelemetry.Count(IsTelemetryError),
                lastTelemetryAtUtc,
                lastHeartbeatUtc,
                minutesSinceHeartbeat));
        }

        return rows;
    }

    private static bool IsCommandAudit(string? action)
    {
        var value = (action ?? string.Empty).Trim();
        return value.Contains("Command", StringComparison.OrdinalIgnoreCase) ||
               value.Contains("ConnectivityProbe", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsCommandDispatch(AuditLogEntry entry)
    {
        var action = entry.Action ?? string.Empty;
        return action.Contains("Dispatch", StringComparison.OrdinalIgnoreCase) ||
               action.Contains("Broadcast", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsCommandResult(AuditLogEntry entry)
    {
        var action = entry.Action ?? string.Empty;
        return action.Contains("Result", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsCommandFailure(AuditLogEntry entry)
    {
        var action = entry.Action ?? string.Empty;
        var detail = entry.Details ?? string.Empty;
        return action.Contains("Failed", StringComparison.OrdinalIgnoreCase) ||
               detail.Contains("fail", StringComparison.OrdinalIgnoreCase);
    }

    private static IReadOnlyList<TelemetryAuditEntry> ExtractTelemetryEvents(IReadOnlyCollection<AuditLogEntry> auditEntries)
    {
        var list = new List<TelemetryAuditEntry>();
        foreach (var entry in auditEntries)
        {
            if (TryParseTelemetryAudit(entry, out var telemetry))
                list.Add(telemetry);
        }

        return list;
    }

    private static bool TryParseTelemetryAudit(AuditLogEntry entry, out TelemetryAuditEntry telemetry)
    {
        telemetry = default;
        var action = entry.Action ?? string.Empty;
        if (!action.Contains("ClientTelemetry", StringComparison.OrdinalIgnoreCase))
            return false;

        var atmId = string.IsNullOrWhiteSpace(entry.Target) ? "UNKNOWN" : entry.Target.Trim();
        var detail = entry.Details ?? string.Empty;
        var parts = detail.Split('|', 3, StringSplitOptions.None);
        var severity = parts.ElementAtOrDefault(0) ?? "info";
        var eventType = parts.ElementAtOrDefault(1) ?? "event";
        var message = parts.ElementAtOrDefault(2) ?? detail;

        telemetry = new TelemetryAuditEntry(
            atmId,
            severity.Trim().ToLowerInvariant(),
            eventType.Trim(),
            message,
            entry.CreatedAtUtc);
        return true;
    }

    private static bool IsTelemetryWarning(TelemetryAuditEntry entry)
    {
        return entry.Severity is "warning" or "warn";
    }

    private static bool IsTelemetryError(TelemetryAuditEntry entry)
    {
        return entry.Severity is "error" or "critical" or "fatal";
    }

    private readonly record struct TelemetryAuditEntry(
        string ATM_ID,
        string Severity,
        string EventType,
        string Detail,
        DateTime CreatedAtUtc);
}

public sealed record UnifiedServerAnalyticsSnapshot(
    FleetSummary Fleet,
    SyncSummary Sync,
    int ConfirmedDeliveries,
    int PendingDeliveries,
    int FailedDeliveries,
    int CommandDispatches,
    int CommandResults,
    int CommandFailures,
    DateTime? LastCommandAtUtc,
    int TelemetryEvents,
    int TelemetryWarnings,
    int TelemetryErrors,
    int NetworkDisconnectEvents,
    int HandshakeMissingEvents,
    int FileRetryEvents,
    DateTime? LastTelemetryAtUtc,
    IReadOnlyList<UnifiedAtmOperationalAnalyticsRow> AtmRows);

public sealed record UnifiedAtmOperationalAnalyticsRow(
    string ATM_ID,
    string ATM_Type,
    int HealthScore,
    ConnectionStatus ConnectionStatus,
    int SyncOpen,
    int SyncFailed,
    int SyncCompleted,
    int PendingDeliveries,
    int CommandFailures,
    int TelemetryWarnings,
    int TelemetryErrors,
    DateTime? LastTelemetryAtUtc,
    DateTime? LastHeartbeatUtc,
    int MinutesSinceHeartbeat);
