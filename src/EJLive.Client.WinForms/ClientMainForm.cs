using EJLive.Client.WinForms.Services;

namespace EJLive.Client.WinForms;

/// <summary>
/// Read-only companion for the headless ATM agent. The form never starts agent
/// loops or executes terminal commands; it only renders the service health file.
/// </summary>
public sealed class ClientMainForm : Form
{
    private const int HeartbeatIntervalSeconds = 30;
    private const int HeartbeatTimeoutSeconds = 95;
    private static readonly Color Surface = Color.FromArgb(248, 250, 252);
    private static readonly Color Card = Color.White;
    private static readonly Color Ink = Color.FromArgb(30, 41, 59);
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

    private Label _title = null!;
    private Label _snapshotSource = null!;
    private Label _serviceState = null!;
    private Label _connectionState = null!;
    private Label _heartbeatState = null!;
    private Label _syncState = null!;
    private Label _atmValue = null!;
    private Label _sessionValue = null!;
    private Label _queueValue = null!;
    private Label _trafficValue = null!;
    private Label _errorValue = null!;
    private DataGridView _componentGrid = null!;
    private ListBox _events = null!;
    private Button _languageButton = null!;
    private bool _arabic;
    private int _refreshBusy;

    public ClientMainForm()
    {
        _gateway = new InProcessClientServiceGateway(
            () => new ClientGatewayContext(),
            _healthFilePath);

        InitializeForm();
        ApplyLanguage(arabic: false);

        Shown += async (_, _) =>
        {
            await RefreshSnapshotAsync().ConfigureAwait(true);
            _refreshTimer.Start();
        };
        FormClosed += (_, _) => _refreshTimer.Stop();
        _refreshTimer.Tick += async (_, _) => await RefreshSnapshotAsync().ConfigureAwait(true);
    }

    private void InitializeForm()
    {
        Text = "EJLive Client Companion";
        MinimumSize = new Size(980, 680);
        Size = new Size(1120, 760);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Surface;
        Font = new Font("Segoe UI", 9F);

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4,
            Padding = new Padding(20),
            BackColor = Surface
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 76));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 150));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 68));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 32));
        Controls.Add(root);

        root.Controls.Add(BuildHeader(), 0, 0);
        root.Controls.Add(BuildStatusCards(), 0, 1);
        root.Controls.Add(BuildDetails(), 0, 2);
        root.Controls.Add(BuildEventPanel(), 0, 3);
    }

    private Control BuildHeader()
    {
        var header = new Panel { Dock = DockStyle.Fill, BackColor = Surface };
        _title = new Label
        {
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 19F, FontStyle.Bold),
            ForeColor = Ink,
            TextAlign = ContentAlignment.MiddleLeft
        };
        _snapshotSource = new Label
        {
            Dock = DockStyle.Bottom,
            Height = 22,
            ForeColor = Muted,
            TextAlign = ContentAlignment.MiddleLeft
        };

        var actions = new FlowLayoutPanel
        {
            Dock = DockStyle.Right,
            Width = 270,
            FlowDirection = FlowDirection.RightToLeft,
            Padding = new Padding(0, 14, 0, 0)
        };
        _languageButton = CreateButton("العربية");
        _languageButton.Click += (_, _) => ApplyLanguage(!_arabic);
        var refreshButton = CreateButton("Refresh");
        refreshButton.Click += async (_, _) => await RefreshSnapshotAsync().ConfigureAwait(true);
        actions.Controls.Add(_languageButton);
        actions.Controls.Add(refreshButton);

        header.Controls.Add(_title);
        header.Controls.Add(_snapshotSource);
        header.Controls.Add(actions);
        return header;
    }

    private Control BuildStatusCards()
    {
        var cards = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 4,
            RowCount = 1,
            Padding = new Padding(0, 4, 0, 12)
        };
        for (var index = 0; index < 4; index++)
            cards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));

        cards.Controls.Add(CreateStatusCard("Agent Service", out _serviceState), 0, 0);
        cards.Controls.Add(CreateStatusCard("Server Link", out _connectionState), 1, 0);
        cards.Controls.Add(CreateStatusCard("Heartbeat", out _heartbeatState), 2, 0);
        cards.Controls.Add(CreateStatusCard("Journal Sync", out _syncState), 3, 0);
        return cards;
    }

    private Control BuildDetails()
    {
        var split = new SplitContainer
        {
            Dock = DockStyle.Fill,
            SplitterDistance = 410,
            SplitterWidth = 8,
            BackColor = Surface
        };

        var details = CreateCardPanel();
        var detailsGrid = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 5,
            Padding = new Padding(12)
        };
        detailsGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
        detailsGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        AddDetailRow(detailsGrid, 0, "ATM", out _atmValue);
        AddDetailRow(detailsGrid, 1, "Session", out _sessionValue);
        AddDetailRow(detailsGrid, 2, "Pending", out _queueValue);
        AddDetailRow(detailsGrid, 3, "Traffic", out _trafficValue);
        AddDetailRow(detailsGrid, 4, "Last error", out _errorValue);
        details.Controls.Add(detailsGrid);
        split.Panel1.Padding = new Padding(0, 0, 4, 0);
        split.Panel1.Controls.Add(details);

        _componentGrid = new DataGridView
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AllowUserToResizeRows = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            BackgroundColor = Card,
            BorderStyle = BorderStyle.None,
            RowHeadersVisible = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect
        };
        _componentGrid.Columns.Add("Component", "Component");
        _componentGrid.Columns.Add("Status", "Status");
        _componentGrid.Columns.Add("Detail", "Detail");
        _componentGrid.Columns[0].FillWeight = 32;
        _componentGrid.Columns[1].FillWeight = 18;
        _componentGrid.Columns[2].FillWeight = 50;
        split.Panel2.Padding = new Padding(4, 0, 0, 0);
        split.Panel2.Controls.Add(_componentGrid);
        return split;
    }

    private Control BuildEventPanel()
    {
        var panel = CreateCardPanel();
        panel.Padding = new Padding(12);
        _events = new ListBox
        {
            Dock = DockStyle.Fill,
            BorderStyle = BorderStyle.None,
            BackColor = Card,
            ForeColor = Ink,
            Font = new Font("Consolas", 9.5F)
        };
        panel.Controls.Add(_events);
        return panel;
    }

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
        _trafficValue.Text = $"↑ {FormatBytes(snapshot.TotalBytesSent)}   ↓ {FormatBytes(snapshot.TotalBytesReceived)}";
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

    private static Panel CreateStatusCard(string caption, out Label value)
    {
        var panel = CreateCardPanel();
        panel.Margin = new Padding(6);
        panel.Padding = new Padding(14);
        var captionLabel = new Label
        {
            Text = caption,
            Dock = DockStyle.Top,
            Height = 28,
            ForeColor = Muted,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold)
        };
        value = new Label
        {
            Text = "Waiting",
            Dock = DockStyle.Fill,
            ForeColor = Warning,
            Font = new Font("Segoe UI", 18F, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleLeft
        };
        panel.Controls.Add(value);
        panel.Controls.Add(captionLabel);
        return panel;
    }

    private static Panel CreateCardPanel() => new()
    {
        Dock = DockStyle.Fill,
        BackColor = Card,
        Margin = new Padding(4)
    };

    private static Button CreateButton(string text) => new()
    {
        Text = text,
        AutoSize = true,
        MinimumSize = new Size(112, 36),
        FlatStyle = FlatStyle.Flat,
        BackColor = Color.FromArgb(226, 232, 240),
        ForeColor = Ink,
        Font = new Font("Segoe UI", 9F, FontStyle.Bold),
        Margin = new Padding(4)
    };

    private static void AddDetailRow(TableLayoutPanel grid, int row, string caption, out Label value)
    {
        grid.RowStyles.Add(new RowStyle(SizeType.Percent, 20));
        var captionLabel = new Label
        {
            Text = caption,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            ForeColor = Muted,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold)
        };
        value = new Label
        {
            Text = "-",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            ForeColor = Ink,
            AutoEllipsis = true
        };
        grid.Controls.Add(captionLabel, 0, row);
        grid.Controls.Add(value, 1, row);
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

    private static string FormatBytes(long value)
    {
        var safe = Math.Max(0, value);
        if (safe >= 1_073_741_824)
            return $"{safe / 1_073_741_824d:F1} GB";
        if (safe >= 1_048_576)
            return $"{safe / 1_048_576d:F1} MB";
        if (safe >= 1_024)
            return $"{safe / 1_024d:F1} KB";
        return $"{safe} B";
    }

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
