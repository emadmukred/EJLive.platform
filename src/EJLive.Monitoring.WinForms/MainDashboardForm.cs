using EJLive.Core;
using EJLive.Core.Models;
using EJLive.Core.Services;
using EJLive.Core.Xfs;
using EJLive.Server.Services;

namespace EJLive.Monitoring.WinForms;

public sealed class MainDashboardForm : Form
{
    private readonly OperationalStateStore _stateStore = new();
    private readonly XfsLogAnalysisService _xfsLogAnalysis = new();
    private readonly OperationalReportCatalogService _reportCatalog = new();
    private readonly ClientTelemetryHistoryService _telemetryHistory = new();
    private DataGridView _overviewGrid = null!;
    private DataGridView _xfsGrid = null!;
    private DataGridView _cashMatrixGrid = null!;
    private DataGridView _terminalListGrid = null!;
    private DataGridView _reportsFilesGrid = null!;
    private DataGridView _reportsWindowGrid = null!;
    private FlowLayoutPanel _mapPanel = null!;
    private RichTextBox _vendorLog = null!;
    private Label _totalValue = null!;
    private Label _onlineValue = null!;
    private Label _syncingValue = null!;
    private Label _offlineValue = null!;
    private Label _healthValue = null!;
    private Label _reportsInfo = null!;
    private DateTime _lastTelemetryRefreshUtc = DateTime.MinValue;

    public MainDashboardForm()
    {
        Text = "EJLive Monitoring Dashboard";
        MinimumSize = new Size(1060, 700);
        Size = new Size(1180, 780);
        StartPosition = FormStartPosition.CenterScreen;
        Font = new Font("Segoe UI", 9F);
        DoubleBuffered = true;
        InitializeUi();
    }

    private void InitializeUi()
    {
        var tabs = new TabControl { Dock = DockStyle.Fill };
        tabs.TabPages.Add(BuildOverviewTab());
        tabs.TabPages.Add(BuildCashMatrixTab());
        tabs.TabPages.Add(BuildTerminalListTab());
        tabs.TabPages.Add(BuildOperationalMapTab());
        tabs.TabPages.Add(BuildDeviceStateTab());
        tabs.TabPages.Add(BuildSyncTab());
        tabs.TabPages.Add(BuildXfsEventsTab());
        tabs.TabPages.Add(BuildVendorLogsTab());
        tabs.TabPages.Add(BuildReportsTab());
        Controls.Add(tabs);
    }

    private TabPage BuildOverviewTab()
    {
        var tab = new TabPage("Overview");
        var root = Ui.Stack();
        var actions = Ui.Flow();
        actions.Controls.Add(Ui.Button("Refresh", RefreshOverview));
        actions.Controls.Add(Ui.Button("Open Overview Window", () => OpenDetachedGridWindow("Overview", _overviewGrid)));
        actions.Controls.Add(Ui.Button("Raise Health Review", () => MessageBox.Show(this, "Health review queued.", "Monitoring")));
        var summary = Ui.CardRow(5);
        _totalValue = Ui.AddMetricCard(summary, "Total ATMs", "0", Color.FromArgb(46, 134, 222));
        _onlineValue = Ui.AddMetricCard(summary, "Online", "0", Color.FromArgb(16, 172, 132));
        _syncingValue = Ui.AddMetricCard(summary, "Syncing", "0", Color.FromArgb(255, 159, 67));
        _offlineValue = Ui.AddMetricCard(summary, "Offline", "0", Color.FromArgb(238, 82, 83));
        _healthValue = Ui.AddMetricCard(summary, "Avg Health", "0%", Color.FromArgb(95, 39, 205));
        _overviewGrid = Ui.Grid();
        _overviewGrid.Columns.Add("ATM", "ATM");
        _overviewGrid.Columns.Add("Status", "Status");
        _overviewGrid.Columns.Add("Health", "Health");
        _overviewGrid.Columns.Add("LastHeartbeat", "Last Heartbeat");
        root.Controls.Add(_overviewGrid);
        root.Controls.Add(summary);
        root.Controls.Add(actions);
        tab.Controls.Add(root);
        RefreshOverview();
        return tab;
    }

    private TabPage BuildCashMatrixTab()
    {
        var tab = new TabPage("Cash Matrix");
        var root = Ui.Stack();
        var actions = Ui.Flow();
        actions.Controls.Add(Ui.Button("Refresh Matrix", RefreshTerminalDashboards));
        actions.Controls.Add(Ui.Button("Open Matrix Window", () => OpenDetachedGridWindow("Cash Matrix", _cashMatrixGrid)));

        _cashMatrixGrid = Ui.Grid();
        _cashMatrixGrid.Columns.Add("ATM", "ATM");
        _cashMatrixGrid.Columns.Add("Branch", "Branch");
        _cashMatrixGrid.Columns.Add("Region", "Region");
        _cashMatrixGrid.Columns.Add("Vendor", "Vendor");
        _cashMatrixGrid.Columns.Add("Source", "Source");
        _cashMatrixGrid.Columns.Add("Updated", "Updated");
        _cashMatrixGrid.Columns.Add("Cass1", "Cass1");
        _cashMatrixGrid.Columns.Add("Cass2", "Cass2");
        _cashMatrixGrid.Columns.Add("Cass3", "Cass3");
        _cashMatrixGrid.Columns.Add("Cass4", "Cass4");
        _cashMatrixGrid.Columns.Add("Remaining", "Remaining");
        _cashMatrixGrid.Columns.Add("Loaded", "Loaded");
        _cashMatrixGrid.Columns.Add("DispenseOut", "Dispense Out");
        _cashMatrixGrid.Columns.Add("Reject", "Reject");
        _cashMatrixGrid.Columns.Add("Retract", "Retract");
        _cashMatrixGrid.Columns.Add("Band", "Cash Band");

        root.Controls.Add(_cashMatrixGrid);
        root.Controls.Add(actions);
        tab.Controls.Add(root);
        RefreshTerminalDashboards();
        return tab;
    }

    private TabPage BuildTerminalListTab()
    {
        var tab = new TabPage("Terminal List");
        var root = Ui.Stack();
        var actions = Ui.Flow();
        actions.Controls.Add(Ui.Button("Refresh List", RefreshTerminalDashboards));
        actions.Controls.Add(Ui.Button("Open List Window", () => OpenDetachedGridWindow("Terminal List", _terminalListGrid)));

        _terminalListGrid = Ui.Grid();
        _terminalListGrid.Columns.Add("ATM", "ATM");
        _terminalListGrid.Columns.Add("Branch", "Branch");
        _terminalListGrid.Columns.Add("Region", "Region");
        _terminalListGrid.Columns.Add("Vendor", "Vendor");
        _terminalListGrid.Columns.Add("Network", "Network");
        _terminalListGrid.Columns.Add("Status", "Status");
        _terminalListGrid.Columns.Add("Connection", "Connection");
        _terminalListGrid.Columns.Add("Health", "Health");
        _terminalListGrid.Columns.Add("Supervisor", "Supervisor");
        _terminalListGrid.Columns.Add("Alerts", "Alerts");
        _terminalListGrid.Columns.Add("LastTx", "Last Tx");
        _terminalListGrid.Columns.Add("LastHeartbeat", "Last Heartbeat");
        _terminalListGrid.Columns.Add("LastSync", "Last Sync");
        _terminalListGrid.Columns.Add("Remaining", "Remaining");

        root.Controls.Add(_terminalListGrid);
        root.Controls.Add(actions);
        tab.Controls.Add(root);
        RefreshTerminalDashboards();
        return tab;
    }

    private TabPage BuildOperationalMapTab()
    {
        var tab = new TabPage("Operational Map");
        var root = Ui.Stack();
        var actions = Ui.Flow();
        actions.Controls.Add(Ui.Button("Refresh Map", RefreshOperationalMap));
        actions.Controls.Add(Ui.Button("Raise Health Review", () => MessageBox.Show(this, "Map health review queued.", "Monitoring")));
        _mapPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            WrapContents = true,
            Padding = new Padding(12),
            BackColor = Color.FromArgb(246, 248, 250)
        };
        var legend = new Label
        {
            Dock = DockStyle.Top,
            Height = 34,
            Text = "Green: active | Yellow: idle | Blue: syncing | Red: offline | Gray: critical",
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(12, 0, 0, 0),
            ForeColor = Color.FromArgb(71, 85, 105),
            BackColor = Color.FromArgb(241, 245, 249)
        };
        root.Controls.Add(_mapPanel);
        root.Controls.Add(legend);
        root.Controls.Add(actions);
        tab.Controls.Add(root);
        RefreshOperationalMap();
        return tab;
    }

    private TabPage BuildDeviceStateTab()
    {
        var tab = new TabPage("Device State");
        var grid = Ui.Grid();
        grid.Columns.Add("Device", "Device");
        grid.Columns.Add("Layer", "Layer");
        grid.Columns.Add("State", "State");
        grid.Rows.Add("Card Reader", "XFS", "Ready");
        grid.Rows.Add("Cash Dispenser", "XFS", "Ready");
        grid.Rows.Add("Journal File", "File System", "Watching");
        grid.Rows.Add("Network Link", "Transport", "Online");
        tab.Controls.Add(grid);
        return tab;
    }

    private TabPage BuildSyncTab()
    {
        var tab = new TabPage("Realtime Sync");
        var grid = Ui.Grid();
        grid.Columns.Add("Queue", "Queue");
        grid.Columns.Add("Pending", "Pending");
        grid.Columns.Add("Retry", "Retry");
        grid.Columns.Add("LastAck", "Last Ack");
        grid.Rows.Add("JournalOutbox", 3, 1, DateTime.Now.AddSeconds(-20));
        grid.Rows.Add("ImageSync", 0, 0, DateTime.Now.AddMinutes(-2));
        tab.Controls.Add(grid);
        return tab;
    }

    private TabPage BuildXfsEventsTab()
    {
        var tab = new TabPage("XFS Events");
        var root = Ui.Stack();
        var actions = Ui.Flow();
        actions.Controls.Add(Ui.Button("Load NCR Sample", () => LoadXfs("NCR ERROR DISPENSER TIMEOUT")));
        actions.Controls.Add(Ui.Button("Load GRG Sample", () => LoadXfs("GRG TRACE JOURNAL OPEN")));
        actions.Controls.Add(Ui.Button("Load Wincor Sample", () => LoadXfs("WINCOR WOSA/XFS SP ERROR: CDM CASH UNIT EMPTY")));
        actions.Controls.Add(Ui.Button("Load Hyosung Sample", () => LoadXfs("HYOSUNG HCDM DISPENSE FAULT: TAKE CASH TIMEOUT")));
        actions.Controls.Add(Ui.Button("Open XFS Window", () => OpenDetachedGridWindow("XFS Events", _xfsGrid)));
        actions.Controls.Add(Ui.Button("Clear", () => _xfsGrid.Rows.Clear()));
        _xfsGrid = Ui.Grid();
        _xfsGrid.Columns.Add("Vendor", "Vendor");
        _xfsGrid.Columns.Add("Component", "Component");
        _xfsGrid.Columns.Add("Severity", "Severity");
        _xfsGrid.Columns.Add("Message", "Message");
        root.Controls.Add(_xfsGrid);
        root.Controls.Add(actions);
        tab.Controls.Add(root);
        return tab;
    }

    private TabPage BuildVendorLogsTab()
    {
        var tab = new TabPage("Vendor Logs");
        var root = Ui.Stack();
        var actions = Ui.Flow();
        actions.Controls.Add(Ui.Button("Analyze Log", AnalyzeVendorLog));
        actions.Controls.Add(Ui.Button("Extract Probable Cause", AnalyzeVendorLog));
        actions.Controls.Add(Ui.Button("Clear", () => _vendorLog.Clear()));
        _vendorLog = Ui.LogBox();
        _vendorLog.Text = "Paste NCR, GRG, Diebold, or Wincor log text here.";
        root.Controls.Add(_vendorLog);
        root.Controls.Add(actions);
        tab.Controls.Add(root);
        return tab;
    }

    private TabPage BuildReportsTab()
    {
        var tab = new TabPage("Reports");
        var root = Ui.Stack();
        var actions = Ui.Flow();
        actions.Controls.Add(Ui.Button("Refresh Reports", RefreshReportsIndex));
        actions.Controls.Add(Ui.Button("Load Latest Ops Bundle", LoadLatestOpsBundleSummary));
        actions.Controls.Add(Ui.Button("Open Windows Summary", () => OpenDetachedGridWindow("Ops Windows", _reportsWindowGrid)));
        actions.Controls.Add(Ui.Button("Open Files Index", () => OpenDetachedGridWindow("Report Files", _reportsFilesGrid)));
        _reportsInfo = new Label
        {
            AutoSize = true,
            Padding = new Padding(8, 8, 0, 0),
            ForeColor = Color.FromArgb(71, 85, 105)
        };
        actions.Controls.Add(_reportsInfo);

        _reportsWindowGrid = Ui.Grid();
        _reportsWindowGrid.Columns.Add("Window", "Window");
        _reportsWindowGrid.Columns.Add("Hours", "Hours");
        _reportsWindowGrid.Columns.Add("Fleet", "Fleet");
        _reportsWindowGrid.Columns.Add("Connected", "Connected");
        _reportsWindowGrid.Columns.Add("Offline", "Offline");
        _reportsWindowGrid.Columns.Add("SyncOpen", "Sync Open");
        _reportsWindowGrid.Columns.Add("SyncFailed", "Sync Failed");
        _reportsWindowGrid.Columns.Add("PendingDel", "Pending Delivery");
        _reportsWindowGrid.Columns.Add("CmdFail", "Command Failures");
        _reportsWindowGrid.Columns.Add("TelWarn", "Telemetry Warnings");
        _reportsWindowGrid.Columns.Add("TelErr", "Telemetry Errors");

        _reportsFilesGrid = Ui.Grid();
        _reportsFilesGrid.Columns.Add("File", "File");
        _reportsFilesGrid.Columns.Add("Category", "Category");
        _reportsFilesGrid.Columns.Add("Modified", "Modified");
        _reportsFilesGrid.Columns.Add("SizeKB", "Size KB");

        var split = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Horizontal,
            SplitterDistance = 210
        };
        split.Panel1.Controls.Add(_reportsWindowGrid);
        split.Panel2.Controls.Add(_reportsFilesGrid);

        root.Controls.Add(split);
        root.Controls.Add(actions);
        tab.Controls.Add(root);
        RefreshReportsIndex();
        LoadLatestOpsBundleSummary();
        return tab;
    }

    private void RefreshReportsIndex()
    {
        if (_reportsFilesGrid is null)
            return;

        var files = _reportCatalog.GetLatestReportFiles(AppConstants.DefaultReportsPath, 120);
        _reportsFilesGrid.SuspendLayout();
        _reportsFilesGrid.Rows.Clear();
        foreach (var file in files)
        {
            var index = _reportsFilesGrid.Rows.Add(
                file.FileName,
                file.Category,
                file.ModifiedAtLocal.ToString("yyyy-MM-dd HH:mm:ss"),
                Math.Max(1, file.SizeBytes / 1024));

            _reportsFilesGrid.Rows[index].DefaultCellStyle.BackColor = file.Category.Contains("bundle", StringComparison.OrdinalIgnoreCase)
                ? Color.FromArgb(239, 247, 255)
                : Color.FromArgb(245, 249, 252);
        }
        _reportsFilesGrid.ResumeLayout();

        if (_reportsInfo is not null)
            _reportsInfo.Text = files.Count == 0
                ? "No report files found yet."
                : $"Indexed {files.Count} report file(s). Latest: {files[0].FileName}";
    }

    private void LoadLatestOpsBundleSummary()
    {
        if (_reportsWindowGrid is null)
            return;

        _reportsWindowGrid.SuspendLayout();
        _reportsWindowGrid.Rows.Clear();
        var summary = _reportCatalog.LoadLatestBundleSummary(AppConstants.DefaultReportsPath);
        foreach (var row in summary.Rows)
        {
            var index = _reportsWindowGrid.Rows.Add(
                row.Window,
                row.LookbackHours,
                row.FleetTotal,
                row.FleetConnected,
                row.FleetOffline,
                row.SyncOpen,
                row.SyncFailed,
                row.PendingDelivery,
                row.CommandFailures,
                row.TelemetryWarnings,
                row.TelemetryErrors);

            var atRisk = row.FleetOffline > 0 || row.SyncFailed > 0 || row.CommandFailures > 0 || row.TelemetryErrors > 0;
            _reportsWindowGrid.Rows[index].DefaultCellStyle.BackColor = atRisk
                ? Color.FromArgb(255, 239, 239)
                : Color.FromArgb(239, 252, 246);
        }
        _reportsWindowGrid.ResumeLayout();

        if (_reportsInfo is not null && !string.IsNullOrWhiteSpace(summary.SourceFilePath))
            _reportsInfo.Text = $"{_reportsInfo.Text} | Bundle source: {summary.SourceFilePath} ({summary.Rows.Count} window rows)";
    }

    private void RefreshOverview()
    {
        RefreshCashTelemetryFromServer();
        _overviewGrid.SuspendLayout();
        _overviewGrid.Rows.Clear();
        foreach (var atm in _stateStore.Snapshot)
        {
            var index = _overviewGrid.Rows.Add(atm.ATM_ID, atm.ConnectionStatus, atm.HealthScore, atm.LastHeartbeatUtc.ToLocalTime());
            _overviewGrid.Rows[index].DefaultCellStyle.BackColor = atm.ConnectionStatus switch
            {
                ConnectionStatus.Connected => Color.FromArgb(239, 252, 246),
                ConnectionStatus.Syncing => Color.FromArgb(239, 247, 255),
                ConnectionStatus.Disconnected => Color.FromArgb(255, 239, 239),
                _ => Color.White
            };
        }
        _overviewGrid.ResumeLayout();
        UpdateSummary();
        RefreshTerminalDashboards();
        RefreshOperationalMap();
    }

    private void RefreshTerminalDashboards()
    {
        if (_cashMatrixGrid is null && _terminalListGrid is null)
            return;

        RefreshCashTelemetryFromServer();

        var snapshot = BuildTerminalSnapshot();
        if (_cashMatrixGrid is not null)
        {
            _cashMatrixGrid.SuspendLayout();
            _cashMatrixGrid.Rows.Clear();
            foreach (var terminal in snapshot)
            {
                var cash = terminal.Cash;
                var index = _cashMatrixGrid.Rows.Add(
                    terminal.TerminalId,
                    terminal.BranchName,
                    terminal.Region,
                    terminal.Vendor,
                    cash.Source,
                    ToLocalView(cash.UpdatedAtUtc),
                    cash.Cassette1,
                    cash.Cassette2,
                    cash.Cassette3,
                    cash.Cassette4,
                    cash.Remaining,
                    cash.Loaded,
                    cash.DispenseOut,
                    cash.Reject,
                    cash.Retract,
                    GetCashBand(cash));

                _cashMatrixGrid.Rows[index].DefaultCellStyle.BackColor = GetCashBandColor(cash);
            }
            _cashMatrixGrid.ResumeLayout();
        }

        if (_terminalListGrid is not null)
        {
            _terminalListGrid.SuspendLayout();
            _terminalListGrid.Rows.Clear();
            foreach (var terminal in snapshot)
            {
                var index = _terminalListGrid.Rows.Add(
                    terminal.TerminalId,
                    terminal.BranchName,
                    terminal.Region,
                    terminal.Vendor,
                    terminal.Network,
                    terminal.Status,
                    terminal.ConnectionStatus,
                    terminal.HealthScore,
                    terminal.SupervisorMode ? "Yes" : "No",
                    terminal.ActiveAlerts,
                    terminal.LastTransaction,
                    ToLocalView(terminal.LastHeartbeatUtc),
                    ToLocalView(terminal.LastEjSyncUtc),
                    terminal.Cash.Remaining);

                _terminalListGrid.Rows[index].DefaultCellStyle.BackColor = terminal.ConnectionStatus switch
                {
                    ConnectionStatus.Connected => Color.FromArgb(239, 252, 246),
                    ConnectionStatus.Syncing => Color.FromArgb(239, 247, 255),
                    ConnectionStatus.Disconnected => Color.FromArgb(255, 239, 239),
                    _ => Color.White
                };
            }
            _terminalListGrid.ResumeLayout();
        }
    }

    private void RefreshOperationalMap()
    {
        if (_mapPanel is null)
            return;

        _mapPanel.SuspendLayout();
        try
        {
            _mapPanel.Controls.Clear();
            foreach (var atm in _stateStore.Snapshot)
                _mapPanel.Controls.Add(CreateMapCard(atm));
        }
        finally
        {
            _mapPanel.ResumeLayout();
        }
    }

    private static Control CreateMapCard(ATMInfo atm)
    {
        var accent = atm.GetCardColor();
        var card = new Panel
        {
            Width = 240,
            Height = 138,
            Margin = new Padding(8),
            BackColor = Color.White,
            Padding = new Padding(10)
        };
        var bar = new Panel { Dock = DockStyle.Top, Height = 5, BackColor = accent };
        var body = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 5, Padding = new Padding(0, 8, 0, 0) };
        for (var i = 0; i < 5; i++)
            body.RowStyles.Add(new RowStyle(SizeType.Percent, 20));
        body.Controls.Add(CardLabel(atm.ATM_ID ?? "UNKNOWN", 10F, FontStyle.Bold, Color.FromArgb(31, 41, 55)), 0, 0);
        body.Controls.Add(CardLabel(atm.ATM_Name ?? atm.ATM_ID ?? "ATM", 8.5F, FontStyle.Regular, Color.FromArgb(71, 85, 105)), 0, 1);
        body.Controls.Add(CardLabel(atm.GetStatusLabel(), 8.5F, FontStyle.Bold, accent), 0, 2);
        body.Controls.Add(CardLabel($"Health {atm.HealthScore}% | {atm.NetworkType}", 8F, FontStyle.Regular, Color.FromArgb(100, 116, 139)), 0, 3);
        body.Controls.Add(CardLabel($"Heartbeat {ElapsedUtc(atm.LastHeartbeatUtc)}", 8F, FontStyle.Regular, Color.FromArgb(100, 116, 139)), 0, 4);
        card.Controls.Add(body);
        card.Controls.Add(bar);
        return card;
    }

    private static Label CardLabel(string text, float size, FontStyle style, Color color)
    {
        return new Label
        {
            Text = text,
            Dock = DockStyle.Fill,
            AutoEllipsis = true,
            TextAlign = ContentAlignment.MiddleLeft,
            Font = new Font("Segoe UI", size, style),
            ForeColor = color
        };
    }

    private static string ElapsedUtc(DateTime utc)
    {
        if (utc == DateTime.MinValue)
            return "-";
        var elapsed = DateTime.UtcNow - utc;
        if (elapsed.TotalSeconds < 60)
            return "now";
        if (elapsed.TotalMinutes < 60)
            return $"{(int)elapsed.TotalMinutes} min ago";
        return utc.ToLocalTime().ToString("yyyy-MM-dd HH:mm");
    }

    private List<TerminalLiveSummary> BuildTerminalSnapshot()
    {
        var list = new List<TerminalLiveSummary>();
        foreach (var atm in _stateStore.Snapshot)
            list.Add(ToTerminalSummary(atm));
        return list;
    }

    private static TerminalLiveSummary ToTerminalSummary(ATMInfo atm)
    {
        var terminalId = atm.ATM_ID ?? atm.ATMId ?? "UNKNOWN";
        var branch = !string.IsNullOrWhiteSpace(atm.BranchName) ? atm.BranchName : (atm.ATM_Name ?? terminalId);
        var network = string.IsNullOrWhiteSpace(atm.NetworkType) ? "LAN" : atm.NetworkType;

        return new TerminalLiveSummary
        {
            TerminalId = terminalId,
            BranchName = branch ?? terminalId,
            Region = atm.Region ?? "Unknown",
            Vendor = atm.ATMType.ToString(),
            Network = network,
            ConnectionStatus = atm.ConnectionStatus,
            Status = atm.Status,
            HealthScore = atm.HealthScore,
            SupervisorMode = atm.IsSupervisorMode,
            ActiveAlerts = atm.NeedsAlert() ? 1 : 0,
            LastHeartbeatUtc = atm.LastHeartbeatUtc,
            LastEjSyncUtc = atm.LastSyncUtc,
            LastTransaction = atm.LastTransaction ?? string.Empty,
            Cash = ToCashStatus(atm)
        };
    }

    private static TerminalCashStatus ToCashStatus(ATMInfo atm)
    {
        if (atm.HasCashTelemetry)
        {
            var cass1 = Math.Max(0, atm.Cassette1Remaining);
            var cass2 = Math.Max(0, atm.Cassette2Remaining);
            var cass3 = Math.Max(0, atm.Cassette3Remaining);
            var cass4 = Math.Max(0, atm.Cassette4Remaining);
            var remaining = Math.Max(0, atm.ATMCache);
            var cassTotal = cass1 + cass2 + cass3 + cass4;

            if (remaining <= 0 && cassTotal > 0)
            {
                remaining = cassTotal;
            }
            else if (remaining > 0 && cassTotal <= 0)
            {
                (cass1, cass2, cass3, cass4) = DistributeCassettesFromRemaining(remaining);
            }

            var dispenseOut = Math.Max(0, atm.TotalDispensed);
            var loaded = atm.CashLoadedTotal > 0 ? atm.CashLoadedTotal : remaining + dispenseOut;
            loaded = Math.Max(loaded, remaining);

            return new TerminalCashStatus
            {
                Source = "Telemetry",
                Cassette1 = cass1,
                Cassette2 = cass2,
                Cassette3 = cass3,
                Cassette4 = cass4,
                Remaining = remaining,
                Loaded = loaded,
                DepositIn = Math.Max(0, atm.CashDepositInTotal),
                DispenseOut = dispenseOut,
                Reject = Math.Max(0, atm.CashRejectCount),
                Retract = Math.Max(0, atm.CashRetractCount),
                UpdatedAtUtc = atm.CashTelemetryUpdatedAtUtc > DateTime.MinValue ? atm.CashTelemetryUpdatedAtUtc : DateTime.UtcNow
            };
        }

        var fallbackRemaining = Math.Max(0, atm.ATMCache);
        var fallbackDispensed = Math.Max(0, atm.TotalDispensed);
        var fallbackLoaded = fallbackRemaining + fallbackDispensed;

        var (fallbackCass1, fallbackCass2, fallbackCass3, fallbackCass4) = DistributeCassettesFromRemaining(fallbackRemaining);

        return new TerminalCashStatus
        {
            Source = "Derived",
            Cassette1 = fallbackCass1,
            Cassette2 = fallbackCass2,
            Cassette3 = fallbackCass3,
            Cassette4 = fallbackCass4,
            Remaining = fallbackRemaining,
            Loaded = fallbackLoaded,
            DepositIn = 0,
            DispenseOut = fallbackDispensed,
            Reject = 0,
            Retract = 0,
            UpdatedAtUtc = DateTime.UtcNow
        };
    }

    private void RefreshCashTelemetryFromServer()
    {
        var now = DateTime.UtcNow;
        if ((now - _lastTelemetryRefreshUtc) < TimeSpan.FromSeconds(30))
            return;

        foreach (var atm in _telemetryHistory.LoadLatest(TimeSpan.FromHours(48), 10000))
            _stateStore.Upsert(atm);

        _lastTelemetryRefreshUtc = now;
    }

    private static (int Cass1, int Cass2, int Cass3, int Cass4) DistributeCassettesFromRemaining(int remaining)
    {
        var safeRemaining = Math.Max(0, remaining);
        var cassBase = safeRemaining / 4;
        var remainder = safeRemaining % 4;
        return (
            cassBase + (remainder > 0 ? 1 : 0),
            cassBase + (remainder > 1 ? 1 : 0),
            cassBase + (remainder > 2 ? 1 : 0),
            cassBase);
    }

    private static string GetCashBand(TerminalCashStatus cash)
    {
        if (cash.IsEmpty)
            return "EMPTY";
        if (cash.IsLowCash)
            return "LOW";
        return "OK";
    }

    private static Color GetCashBandColor(TerminalCashStatus cash)
    {
        if (cash.IsEmpty)
            return Color.FromArgb(255, 239, 239);
        if (cash.IsLowCash)
            return Color.FromArgb(255, 247, 233);
        return Color.FromArgb(239, 252, 246);
    }

    private static string ToLocalView(DateTime utc)
    {
        if (utc == DateTime.MinValue)
            return "-";
        return utc.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss");
    }

    private void OpenDetachedGridWindow(string title, DataGridView source)
    {
        if (source is null)
            return;

        var window = new Form
        {
            Text = $"EJLive Monitoring - {title}",
            Width = 1120,
            Height = 700,
            StartPosition = FormStartPosition.CenterParent,
            Font = Font
        };

        var detachedGrid = Ui.Grid();
        detachedGrid.Columns.Clear();
        foreach (DataGridViewColumn column in source.Columns)
            detachedGrid.Columns.Add(column.Name, column.HeaderText);

        foreach (DataGridViewRow sourceRow in source.Rows)
        {
            var values = new object[sourceRow.Cells.Count];
            for (var index = 0; index < sourceRow.Cells.Count; index++)
                values[index] = sourceRow.Cells[index].Value;

            var newIndex = detachedGrid.Rows.Add(values);
            detachedGrid.Rows[newIndex].DefaultCellStyle.BackColor = sourceRow.DefaultCellStyle.BackColor;
        }

        window.Controls.Add(detachedGrid);
        window.Show(this);
    }

    private void LoadXfs(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return;

        foreach (var finding in _xfsLogAnalysis.AnalyzeOperationalFindings(text))
            _xfsGrid.Rows.Add(finding.Vendor, finding.Category, finding.Severity, finding.Message);
    }

    private void AnalyzeVendorLog()
    {
        var lines = _vendorLog.Lines.ToArray();
        var findings = _xfsLogAnalysis.AnalyzeLines(lines).ToArray();
        _vendorLog.AppendText(Environment.NewLine + "Probable cause candidates:" + Environment.NewLine);
        foreach (var finding in findings.DefaultIfEmpty("No critical vendor errors detected."))
            _vendorLog.AppendText(finding + Environment.NewLine);
    }

    private void UpdateSummary()
    {
        if (_totalValue is null)
            return;

        var summary = _stateStore.BuildSummary();
        _totalValue.Text = summary.Total.ToString();
        _onlineValue.Text = summary.Connected.ToString();
        _syncingValue.Text = summary.Syncing.ToString();
        _offlineValue.Text = summary.Offline.ToString();
        _healthValue.Text = $"{summary.AverageHealth}%";
    }

}

internal static class Ui
{
    public static Panel Stack() => new() { Dock = DockStyle.Fill, Padding = new Padding(8) };
    public static FlowLayoutPanel Flow() => new() { Dock = DockStyle.Top, Height = 58, Padding = new Padding(8), WrapContents = true };
    public static Button Button(string text, Action action)
    {
        var button = new Button { Text = text, AutoSize = true, Height = 32, Margin = new Padding(4) };
        button.Click += (_, _) => action();
        return button;
    }
    public static TableLayoutPanel CardRow(int columns)
    {
        var row = new TableLayoutPanel { Dock = DockStyle.Top, Height = 92, ColumnCount = columns, Padding = new Padding(4) };
        for (var i = 0; i < columns; i++)
            row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / columns));
        return row;
    }

    public static Label AddMetricCard(TableLayoutPanel row, string title, string value, Color accent)
    {
        var card = new Panel { Dock = DockStyle.Fill, Margin = new Padding(4), Padding = new Padding(10), BackColor = Color.White };
        var accentBar = new Panel { Dock = DockStyle.Left, Width = 4, BackColor = accent };
        var titleLabel = new Label { Text = title, Dock = DockStyle.Top, Height = 22, ForeColor = Color.FromArgb(90, 90, 90), Font = new Font("Segoe UI", 8F) };
        var valueLabel = new Label { Text = value, Dock = DockStyle.Fill, ForeColor = Color.FromArgb(35, 35, 35), Font = new Font("Segoe UI Semibold", 14F, FontStyle.Bold), TextAlign = ContentAlignment.MiddleLeft };
        card.Controls.Add(valueLabel);
        card.Controls.Add(titleLabel);
        card.Controls.Add(accentBar);
        row.Controls.Add(card);
        return valueLabel;
    }

    public static DataGridView Grid()
    {
        var grid = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false, AllowUserToDeleteRows = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, SelectionMode = DataGridViewSelectionMode.FullRowSelect, RowHeadersVisible = false, BackgroundColor = Color.White, BorderStyle = BorderStyle.None };
        grid.EnableDoubleBuffering();
        grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(245, 247, 250);
        grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
        grid.EnableHeadersVisualStyles = false;
        grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(250, 251, 253);
        return grid;
    }
    public static RichTextBox LogBox() => new() { Dock = DockStyle.Fill, Font = new Font("Consolas", 9F) };
}

internal static class ControlRenderingExtensions
{
    public static void EnableDoubleBuffering(this Control control)
    {
        var property = typeof(Control).GetProperty("DoubleBuffered", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        property?.SetValue(control, true, null);
    }
}
