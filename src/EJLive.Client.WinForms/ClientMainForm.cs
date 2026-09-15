using EJLive.Client.WinForms.Services;
using EJLive.Core.UI;

namespace EJLive.Client.WinForms;

/// <summary>
/// Read-only companion for the headless ATM agent. The form never starts agent
/// loops or executes terminal commands; it only renders the service health file.
///
/// Wave 5 / C-28 — Designer partial split (SS-10 process, proven on
/// <c>JournalStudioForm</c>): this file carries behaviour only; the control tree
/// lives in <c>ClientMainForm.Designer.cs</c> and every event binding lives in
/// <see cref="WireEvents"/>, so a designer regeneration of the sibling partial
/// can never orphan a handler.
///
/// Control → function map (designer fields, set by this partial):
///   _serviceState / _connectionState / _heartbeatState / _syncState — card values, ApplySnapshot
///   _atmValue / _sessionValue / _queueValue / _trafficValue / _errorValue — detail rows
///   _componentGrid — per-component runtime table (rows added at runtime, never by the designer)
///   _events — rolling event feed, capped at 100 lines
///   _languageButton — toggles the en/ar surface
///   _refreshButton — manual snapshot refresh
///   _snapshotSource — provenance line under the title
/// </summary>
public sealed partial class ClientMainForm : Form
{
    private const int HeartbeatIntervalSeconds = 30;
    private const int HeartbeatTimeoutSeconds = 95;
    private static readonly Color Muted = Color.FromArgb(100, 116, 139);
    private static readonly Color Healthy = Color.FromArgb(5, 150, 105);
    private static readonly Color Warning = Color.FromArgb(217, 119, 6);
    private static readonly Color Failed = Color.FromArgb(220, 38, 38);

    private readonly string _healthFilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
        "EJLive",
        "Agent",
        "health.json");

    private readonly IClientServiceGateway _gateway;
    private readonly System.Windows.Forms.Timer _refreshTimer = new() { Interval = 2_000 };
    private bool _arabic;
    private int _refreshBusy;

    public ClientMainForm()
    {
        _gateway = new InProcessClientServiceGateway(
            () => new ClientGatewayContext(),
            _healthFilePath);

        InitializeComponent();
        WireEvents();
        ApplyLanguage(arabic: false);
    }

    /// <summary>
    /// Every event binding for this surface, in one place (SS-10). The designer
    /// partial deliberately contains no bindings; a regenerated
    /// <c>ClientMainForm.Designer.cs</c> therefore cannot drop a handler.
    /// </summary>
    private void WireEvents()
    {
        _languageButton.Click += (_, _) => ApplyLanguage(!_arabic);
        _refreshButton.Click += async (_, _) => await RefreshSnapshotAsync().ConfigureAwait(true);

        Shown += async (_, _) =>
        {
            await RefreshSnapshotAsync().ConfigureAwait(true);
            _refreshTimer.Start();
        };
        FormClosed += (_, _) => _refreshTimer.Stop();
        _refreshTimer.Tick += async (_, _) => await RefreshSnapshotAsync().ConfigureAwait(true);
    }

    /// <summary>
    /// Reflection-tested bridge (Track08 / C-31) over the canonical health-file
    /// parser: the form never re-implements the schema —
    /// <see cref="ServiceHealthSnapshot.TryParseJson"/> is the one owner, and the
    /// in-process gateway reads through it too.
    /// </summary>
    private static bool TryParseServiceHealthSnapshot(string? json, out ServiceHealthSnapshot snapshot)
        => ServiceHealthSnapshot.TryParseJson(json, out snapshot);

    private async Task RefreshSnapshotAsync()
    {
        if (Interlocked.Exchange(ref _refreshBusy, 1) == 1)
            return;

        try
        {
            var snapshot = await _gateway.GetRuntimeSnapshotAsync().ConfigureAwait(true);
            ApplySnapshot(snapshot);
        }
        catch (Exception ex)
        {
            SetStatus(_serviceState, "Unavailable");
            _snapshotSource.Text = _arabic
                ? "تعذر قراءة حالة الخدمة"
                : "Unable to read service health";
            AddEvent($"Health read failed: {ex.Message}");
        }
        finally
        {
            Interlocked.Exchange(ref _refreshBusy, 0);
        }
    }

    private void ApplySnapshot(ClientRuntimeSnapshot snapshot)
    {
        _snapshotSource.Text = $"{snapshot.SnapshotSource}  •  {snapshot.CapturedAtUtc.ToLocalTime():yyyy-MM-dd HH:mm:ss}";
        SetStatus(_serviceState, snapshot.AgentState);
        SetStatus(_connectionState, snapshot.Connected ? "Connected" : "Disconnected");
        SetStatus(_heartbeatState, ClassifyHeartbeatServiceStatus(snapshot.LastHeartbeatUtc, out var heartbeatDetail));
        SetStatus(_syncState, snapshot.LastJournalSyncUtc.HasValue ? "Observed" : "Waiting");

        _atmValue.Text = EmptyAsDash(snapshot.AtmId);
        _sessionValue.Text = EmptyAsDash(snapshot.SessionId);
        _queueValue.Text = $"{Math.Max(0, snapshot.PendingOutboxItems):N0} item(s)";
        _trafficValue.Text = $"\u2191 {UiHelpers.FormatBytes(snapshot.TotalBytesSent)}   \u2193 {UiHelpers.FormatBytes(snapshot.TotalBytesReceived)}";
        _errorValue.Text = EmptyAsDash(snapshot.LastError);
        _errorValue.ForeColor = string.IsNullOrWhiteSpace(snapshot.LastError) ? Muted : Failed;

        _componentGrid.Rows.Clear();
        AddComponent("Agent Controller", snapshot.AgentState, $"uptime={Math.Max(0, snapshot.UptimeSeconds):F0}s");
        AddComponent("Handshake", snapshot.HandshakeComplete ? "Running" : "Waiting", EmptyAsDash(snapshot.SessionId));
        AddComponent("Heartbeat", _heartbeatState.Text, heartbeatDetail);
        AddComponent("Outbox", "Running", $"{Math.Max(0, snapshot.PendingOutboxItems)} pending");
        AddComponent(
            "Journal Sync",
            snapshot.LastJournalSyncUtc.HasValue ? "Running" : "Waiting",
            snapshot.LastJournalSyncUtc?.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss") ?? "No acknowledgement yet");

        foreach (var component in snapshot.Components)
            AddComponent(component.Name, component.Status, component.Detail);

        AddEvent($"Snapshot: state={snapshot.AgentState}, connected={snapshot.Connected}, pending={snapshot.PendingOutboxItems}");
    }

    private void AddComponent(string name, string status, string detail)
    {
        var row = _componentGrid.Rows.Add(name, status, detail);
        _componentGrid.Rows[row].Cells[1].Style.ForeColor = StatusColor(status);
    }

    private void ApplyLanguage(bool arabic)
    {
        _arabic = arabic;
        RightToLeft = arabic ? RightToLeft.Yes : RightToLeft.No;
        RightToLeftLayout = arabic;
        _title.Text = arabic ? "لوحة حالة عميل EJLive" : "EJLive Client Companion";
        _languageButton.Text = arabic ? "English" : "العربية";
        Text = _title.Text;
    }

    private void AddEvent(string message)
    {
        var line = $"{DateTime.Now:HH:mm:ss}  {message}";
        if (_events.Items.Count > 0 && string.Equals(_events.Items[0]?.ToString(), line, StringComparison.Ordinal))
            return;

        _events.Items.Insert(0, line);
        while (_events.Items.Count > 100)
            _events.Items.RemoveAt(_events.Items.Count - 1);
    }

    private static void SetStatus(Label label, string status)
    {
        label.Text = EmptyAsDash(status);
        label.ForeColor = StatusColor(status);
    }

    private static Color StatusColor(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
            return Muted;

        return status.Trim().ToUpperInvariant() switch
        {
            "RUNNING" or "CONNECTED" or "OBSERVED" => Healthy,
            "FAILED" or "DISCONNECTED" or "UNAVAILABLE" => Failed,
            _ => Warning
        };
    }

    private static string EmptyAsDash(string? value) => string.IsNullOrWhiteSpace(value) ? "-" : value.Trim();

    private static string ClassifyHeartbeatServiceStatus(DateTime? lastHeartbeatUtc, out string detail)
    {
        if (!lastHeartbeatUtc.HasValue)
        {
            detail = "No heartbeat published yet.";
            return "Stopped";
        }

        var elapsed = DateTime.UtcNow - lastHeartbeatUtc.Value.ToUniversalTime();
        if (elapsed < TimeSpan.Zero)
            elapsed = TimeSpan.Zero;
        detail = $"age={elapsed.TotalSeconds:F0}s";

        if (elapsed <= TimeSpan.FromSeconds(Math.Max(10, HeartbeatIntervalSeconds * 2)))
            return "Running";
        if (elapsed <= TimeSpan.FromSeconds(HeartbeatTimeoutSeconds))
            return "Warning";
        if (elapsed <= TimeSpan.FromSeconds(HeartbeatTimeoutSeconds * 3))
            return "Degraded";
        return "Failed";
    }
}
