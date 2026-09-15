using System.Text.Json;
using EJLive.Core;
using EJLive.Core.Data.Repositories;
using EJLive.Core.Engine;
using EJLive.Core.Models;
using EJLive.Core.Services;
using AppConstants = EJLive.Core.AppConstants;

namespace EJLive.Server.Services;

/// <summary>
/// Applies client telemetry to operational ATM state and raises operational alerts.
/// UI consumers receive a result snapshot and remain responsible only for presentation.
/// </summary>
public sealed class ClientTelemetryStateService
{
    private readonly OperationalStateStore _stateStore;
    private readonly AlertManager _alerts;
    private readonly IClientHealthSnapshotRepository? _healthSink;
    private readonly IATMRegistryRepository? _registrySink;

    /// <summary>
    /// <paramref name="healthSink"/> and <paramref name="registrySink"/> enable the SS8 telemetry
    /// lane (<c>telemetry → client_health_snapshots</c>) and terminal discovery persistence
    /// (<c>atm_registry</c>). They are optional so unit tests of the state projection remain
    /// database-free; the server host supplies them once the bootstrap has opened the archive.
    /// </summary>
    public ClientTelemetryStateService(
        OperationalStateStore stateStore,
        AlertManager alerts,
        IClientHealthSnapshotRepository? healthSink = null,
        IATMRegistryRepository? registrySink = null)
    {
        _stateStore = stateStore ?? throw new ArgumentNullException(nameof(stateStore));
        _alerts = alerts ?? throw new ArgumentNullException(nameof(alerts));
        _healthSink = healthSink;
        _registrySink = registrySink;
    }

    public ClientTelemetryStateUpdate Apply(ClientTelemetryPacket packet)
    {
        ArgumentNullException.ThrowIfNull(packet);

        var atmId = string.IsNullOrWhiteSpace(packet.ATM_ID) ? "UNKNOWN" : packet.ATM_ID.Trim();
        var reportedAtUtc = packet.ReportedAtUtc > DateTime.MinValue
            ? packet.ReportedAtUtc.ToUniversalTime()
            : DateTime.UtcNow;
        var eventType = string.IsNullOrWhiteSpace(packet.EventType) ? "event" : packet.EventType.Trim();
        var severity = string.IsNullOrWhiteSpace(packet.Severity)
            ? "info"
            : packet.Severity.Trim().ToLowerInvariant();

        if (!_stateStore.TryGet(atmId, out var atm) || atm is null)
        {
            atm = new ATMInfo
            {
                ATM_ID = atmId,
                ATM_Name = atmId,
                ATM_Type = AppConstants.ATM_TYPE_NCR,
                ConnectionStatus = ConnectionStatus.Connected,
                Status = ATMStatus.Online,
                IsConnected = true,
                ConnectedAtUtc = reportedAtUtc,
                LastHeartbeatUtc = reportedAtUtc,
                LastDataReceivedUtc = reportedAtUtc,
                HealthScore = 100
            };
        }

        atm.LastDataReceivedUtc = reportedAtUtc;

        if (eventType.Contains("network_connected", StringComparison.OrdinalIgnoreCase))
        {
            atm.ConnectionStatus = ConnectionStatus.Connected;
            atm.Status = ATMStatus.Online;
            atm.IsConnected = true;
        }
        else if (eventType.Contains("network_disconnected", StringComparison.OrdinalIgnoreCase))
        {
            atm.ConnectionStatus = ConnectionStatus.WaitingReply;
            atm.Status = ATMStatus.Warning;
        }
        else if (eventType.Contains("handshake_missing", StringComparison.OrdinalIgnoreCase))
        {
            atm.ConnectionStatus = ConnectionStatus.WaitingReply;
        }

        if (eventType.Contains("pulse", StringComparison.OrdinalIgnoreCase) ||
            eventType.Contains("heartbeat", StringComparison.OrdinalIgnoreCase) ||
            eventType.Contains("network_connected", StringComparison.OrdinalIgnoreCase))
        {
            atm.LastHeartbeatUtc = reportedAtUtc;
        }

        ApplyCashTelemetry(atm, eventType, packet.Detail, reportedAtUtc);

        if (severity == "warning")
            atm.HealthScore = Math.Max(0, atm.HealthScore - 1);
        else if (severity == "error")
            atm.HealthScore = Math.Max(0, atm.HealthScore - 3);

        _stateStore.Upsert(atm);

        var alertRaised = severity is "warning" or "error";
        if (alertRaised)
        {
            var alertSeverity = severity == "error" ? AlertSeverity.Critical : AlertSeverity.Warning;
            _alerts.Raise(
                alertSeverity,
                "Client telemetry event",
                $"{atmId}: {eventType} - {packet.Detail}",
                "ClientTelemetry");
        }

        PersistTelemetrySideEffects(atm, atmId, reportedAtUtc, severity, packet);

        return new ClientTelemetryStateUpdate(atm, atmId, eventType, severity, reportedAtUtc, alertRaised);
    }

    /// <summary>
    /// Projection writes are best-effort by contract: a database hiccup must never drop a
    /// live telemetry frame from the in-memory fleet state, so failures are logged and
    /// swallowed WITH a reason (SS-15), not silenced.
    /// </summary>
    private void PersistTelemetrySideEffects(ATMInfo atm, string atmId, DateTime reportedAtUtc, string severity, ClientTelemetryPacket packet)
    {
        if (_registrySink is not null)
        {
            try
            {
                _registrySink.Upsert(new AtmRegistrationRecord(
                    AtmId: atmId,
                    AtmName: atm.ATM_Name ?? string.Empty,
                    AtmType: string.IsNullOrWhiteSpace(atm.ATM_Type) ? "NCR" : atm.ATM_Type,
                    IpAddress: atm.ServerIP ?? string.Empty,
                    RegisteredAtUtc: reportedAtUtc,
                    LastHeartbeatUtc: atm.LastHeartbeatUtc == DateTime.MinValue ? reportedAtUtc : atm.LastHeartbeatUtc,
                    LastDataReceivedUtc: reportedAtUtc));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("telemetry registry projection failed: " + ex.Message);
            }
        }

        if (_healthSink is not null)
        {
            try
            {
                _healthSink.Save(new ClientHealthSnapshotRecord(
                    SnapshotId: Guid.NewGuid().ToString("N"),
                    AtmId: atmId,
                    AgentState: atm.ConnectionStatus.ToString(),
                    NetworkConnected: atm.ConnectionStatus == ConnectionStatus.Connected,
                    SessionId: null,
                    LastHeartbeatUtc: atm.LastHeartbeatUtc == DateTime.MinValue ? null : atm.LastHeartbeatUtc,
                    LastSyncUtc: atm.LastDataReceivedUtc == DateTime.MinValue ? null : atm.LastDataReceivedUtc,
                    OutboxCount: 0,
                    ErrorCount: severity == "error" ? 1 : 0,
                    LastError: severity == "error" ? packet.Detail : null,
                    SnapshotUtc: reportedAtUtc));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("telemetry health snapshot projection failed: " + ex.Message);
            }
        }
    }

    public static void ApplyCashTelemetry(ATMInfo atm, string eventType, string detail, DateTime reportedAtUtc)
    {
        ArgumentNullException.ThrowIfNull(atm);
        if (string.IsNullOrWhiteSpace(eventType))
            return;

        if (eventType.Contains("cash_status", StringComparison.OrdinalIgnoreCase))
        {
            ApplyCashTelemetryFromKeyValueDetail(atm, detail, reportedAtUtc);
            return;
        }

        if (!eventType.Contains("pulse_json", StringComparison.OrdinalIgnoreCase) || string.IsNullOrWhiteSpace(detail))
            return;

        try
        {
            using var document = JsonDocument.Parse(detail);
            if (!document.RootElement.TryGetProperty("cash", out var cashElement) ||
                cashElement.ValueKind == JsonValueKind.Null)
            {
                return;
            }

            if (TryReadMetric(cashElement, "cass1", out var cassette1))
                atm.Cassette1Remaining = Math.Max(0, cassette1);
            if (TryReadMetric(cashElement, "cass2", out var cassette2))
                atm.Cassette2Remaining = Math.Max(0, cassette2);
            if (TryReadMetric(cashElement, "cass3", out var cassette3))
                atm.Cassette3Remaining = Math.Max(0, cassette3);
            if (TryReadMetric(cashElement, "cass4", out var cassette4))
                atm.Cassette4Remaining = Math.Max(0, cassette4);
            if (TryReadMetric(cashElement, "remaining", out var remaining))
                atm.ATMCache = Math.Max(0, remaining);
            if (TryReadMetric(cashElement, "loaded", out var loaded))
                atm.CashLoadedTotal = Math.Max(0, loaded);
            if (TryReadMetric(cashElement, "depositIn", out var depositIn))
                atm.CashDepositInTotal = Math.Max(0, depositIn);
            if (TryReadMetric(cashElement, "dispenseOut", out var dispenseOut))
                atm.TotalDispensed = Math.Max(0, dispenseOut);
            if (TryReadMetric(cashElement, "reject", out var reject))
                atm.CashRejectCount = Math.Max(0, reject);
            if (TryReadMetric(cashElement, "retract", out var retract))
                atm.CashRetractCount = Math.Max(0, retract);

            atm.CashTelemetryUpdatedAtUtc =
                cashElement.TryGetProperty("updatedAtUtc", out var updatedAtElement) &&
                DateTime.TryParse(updatedAtElement.ToString(), out var updatedAt)
                    ? updatedAt.ToUniversalTime()
                    : reportedAtUtc;
        }
        catch (JsonException)
        {
            // Malformed telemetry does not overwrite the last valid cash state.
        }
    }

    private static void ApplyCashTelemetryFromKeyValueDetail(
        ATMInfo atm,
        string detail,
        DateTime reportedAtUtc)
    {
        var values = ParseTelemetryDetail(detail);
        if (values.Count == 0)
            return;

        if (TryReadMetric(values, "cass1", out var cassette1))
            atm.Cassette1Remaining = Math.Max(0, cassette1);
        if (TryReadMetric(values, "cass2", out var cassette2))
            atm.Cassette2Remaining = Math.Max(0, cassette2);
        if (TryReadMetric(values, "cass3", out var cassette3))
            atm.Cassette3Remaining = Math.Max(0, cassette3);
        if (TryReadMetric(values, "cass4", out var cassette4))
            atm.Cassette4Remaining = Math.Max(0, cassette4);
        if (TryReadMetric(values, "remaining", out var remaining))
            atm.ATMCache = Math.Max(0, remaining);
        if (TryReadMetric(values, "loaded", out var loaded))
            atm.CashLoadedTotal = Math.Max(0, loaded);
        if (TryReadMetric(values, "depositIn", out var depositIn))
            atm.CashDepositInTotal = Math.Max(0, depositIn);
        if (TryReadMetric(values, "dispenseOut", out var dispenseOut))
            atm.TotalDispensed = Math.Max(0, dispenseOut);
        if (TryReadMetric(values, "reject", out var reject))
            atm.CashRejectCount = Math.Max(0, reject);
        if (TryReadMetric(values, "retract", out var retract))
            atm.CashRetractCount = Math.Max(0, retract);

        atm.CashTelemetryUpdatedAtUtc = TryReadDateTime(values, "updatedAtUtc", out var updatedAt)
            ? updatedAt
            : reportedAtUtc;
    }

    private static Dictionary<string, string> ParseTelemetryDetail(string detail)
    {
        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrWhiteSpace(detail))
            return values;

        var tokens = detail.Split(
            new[] { ';', '|' },
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        foreach (var token in tokens)
        {
            var separator = token.IndexOf('=');
            if (separator <= 0)
                continue;

            var key = token[..separator].Trim();
            if (key.Length > 0)
                values[key] = token[(separator + 1)..].Trim();
        }

        return values;
    }

    private static bool TryReadMetric(
        IReadOnlyDictionary<string, string> values,
        string key,
        out int value)
    {
        value = 0;
        return values.TryGetValue(key, out var raw) &&
               !string.IsNullOrWhiteSpace(raw) &&
               int.TryParse(raw, out value);
    }

    private static bool TryReadMetric(JsonElement element, string key, out int value)
    {
        value = 0;
        if (!element.TryGetProperty(key, out var property))
            return false;

        return property.ValueKind switch
        {
            JsonValueKind.Number => property.TryGetInt32(out value),
            JsonValueKind.String => int.TryParse(property.GetString(), out value),
            _ => false
        };
    }

    private static bool TryReadDateTime(
        IReadOnlyDictionary<string, string> values,
        string key,
        out DateTime value)
    {
        value = DateTime.MinValue;
        if (!values.TryGetValue(key, out var raw) ||
            string.IsNullOrWhiteSpace(raw) ||
            !DateTime.TryParse(raw, out var parsed))
        {
            return false;
        }

        value = parsed.ToUniversalTime();
        return true;
    }
}

public sealed record ClientTelemetryStateUpdate(
    ATMInfo Atm,
    string AtmId,
    string EventType,
    string Severity,
    DateTime ReportedAtUtc,
    bool AlertRaised);

/// <summary>
/// Reconstructs the latest read-only ATM telemetry snapshot from the server audit store.
/// Database access and payload parsing stay outside WinForms.
/// </summary>
public sealed class ClientTelemetryHistoryService
{
    private readonly DatabaseManager _database;

    public ClientTelemetryHistoryService()
        : this(DatabaseManager.Instance)
    {
    }

    public ClientTelemetryHistoryService(DatabaseManager database)
    {
        _database = database ?? throw new ArgumentNullException(nameof(database));
    }

    public IReadOnlyList<ATMInfo> LoadLatest(TimeSpan lookback, int maxRows = 10000)
    {
        var now = DateTime.UtcNow;
        var from = now - (lookback <= TimeSpan.Zero ? TimeSpan.FromHours(24) : lookback);

        System.Data.DataTable table;
        try
        {
            table = _database.GetAuditLog(null, from, now, Math.Clamp(maxRows, 1, 50000));
        }
        catch
        {
            return Array.Empty<ATMInfo>();
        }

        var events = table.Rows
            .Cast<System.Data.DataRow>()
            .Select(row => ParseEvent(row, now))
            .Where(item => item is not null)
            .Cast<TelemetryAuditEvent>()
            .OrderBy(item => item.ReportedAtUtc)
            .ToArray();

        var states = new Dictionary<string, ATMInfo>(StringComparer.OrdinalIgnoreCase);
        foreach (var item in events)
        {
            if (!states.TryGetValue(item.AtmId, out var atm))
            {
                atm = new ATMInfo
                {
                    ATM_ID = item.AtmId,
                    ATM_Name = item.AtmId,
                    ATM_Type = AppConstants.ATM_TYPE_NCR,
                    ConnectionStatus = ConnectionStatus.WaitingReply,
                    Status = ATMStatus.Warning,
                    HealthScore = 80
                };
                states[item.AtmId] = atm;
            }

            atm.LastDataReceivedUtc = item.ReportedAtUtc;
            if (item.EventType.Contains("network_connected", StringComparison.OrdinalIgnoreCase))
            {
                atm.ConnectionStatus = ConnectionStatus.Connected;
                atm.Status = ATMStatus.Online;
                atm.IsConnected = true;
                atm.LastHeartbeatUtc = item.ReportedAtUtc;
            }
            else if (item.EventType.Contains("network_disconnected", StringComparison.OrdinalIgnoreCase))
            {
                atm.ConnectionStatus = ConnectionStatus.Disconnected;
                atm.Status = ATMStatus.Offline;
                atm.IsConnected = false;
            }
            else if (item.EventType.Contains("heartbeat", StringComparison.OrdinalIgnoreCase) ||
                     item.EventType.Contains("pulse", StringComparison.OrdinalIgnoreCase))
            {
                atm.LastHeartbeatUtc = item.ReportedAtUtc;
            }

            ClientTelemetryStateService.ApplyCashTelemetry(
                atm,
                item.EventType,
                item.Payload,
                item.ReportedAtUtc);
        }

        return states.Values
            .OrderBy(item => item.ATM_ID, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static TelemetryAuditEvent? ParseEvent(System.Data.DataRow row, DateTime fallbackUtc)
    {
        var action = Convert.ToString(row["action"]) ?? string.Empty;
        if (!action.Contains("ClientTelemetry", StringComparison.OrdinalIgnoreCase))
            return null;

        var atmId = (Convert.ToString(row["atm_id"]) ?? string.Empty).Trim();
        if (atmId.Length == 0)
            return null;

        var detail = Convert.ToString(row["details"]) ?? string.Empty;
        var parts = detail.Split('|', 3, StringSplitOptions.None);
        var eventType = parts.ElementAtOrDefault(1) ?? string.Empty;
        var payload = parts.ElementAtOrDefault(2) ?? detail;
        var performedAtText = Convert.ToString(row["performed_at"]) ?? string.Empty;
        var reportedAtUtc = DateTime.TryParse(performedAtText, out var parsedAt)
            ? parsedAt.ToUniversalTime()
            : fallbackUtc;

        return new TelemetryAuditEvent(atmId, eventType, payload, reportedAtUtc);
    }

    private sealed record TelemetryAuditEvent(
        string AtmId,
        string EventType,
        string Payload,
        DateTime ReportedAtUtc);
}
