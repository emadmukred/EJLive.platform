using EJLive.Core;
using EJLive.Core.Models;
using EJLive.Core.Services;
using EJLive.Core.UI;
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
    private DataGridView _smartFindingsGrid = null!;
    private DataGridView _smartCassetteGrid = null!;
    private DataGridView _smartHourlyGrid = null!;
    private RichTextBox _smartUploadBox = null!;
    private Label _smartSummary = null!;
    private Label _smartCritical = null!;
    private Label _smartWarning = null!;
    private Label _smartInfo = null!;
    private TextBox _smartVendorBox = null!;
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
        tabs.TabPages.Add(BuildSmartAnalysisTab());
        Controls.Add(tabs);
    }

    private TabPage BuildOverviewTab()
    {
        var tab = new TabPage("Overview");
        var root = UiHelpers.Stack();
        var actions = UiHelpers.Flow();
        actions.Controls.Add(UiHelpers.Button("Refresh", RefreshOverview));
        actions.Controls.Add(UiHelpers.Button("Open Overview Window", () => OpenDetachedGridWindow("Overview", _overviewGrid)));
        actions.Controls.Add(UiHelpers.Button("Raise Health Review", () => MessageBox.Show(this, "Health review queued.", "Monitoring")));
        var summary = UiHelpers.CardRow(5);
        _totalValue = UiHelpers.AddMetricCard(summary, "Total ATMs", "0", Color.FromArgb(46, 134, 222));
        _onlineValue = UiHelpers.AddMetricCard(summary, "Online", "0", Color.FromArgb(16, 172, 132));
        _syncingValue = UiHelpers.AddMetricCard(summary, "Syncing", "0", Color.FromArgb(255, 159, 67));
        _offlineValue = UiHelpers.AddMetricCard(summary, "Offline", "0", Color.FromArgb(238, 82, 83));
        _healthValue = UiHelpers.AddMetricCard(summary, "Avg Health", "0%", Color.FromArgb(95, 39, 205));
        _overviewGrid = UiHelpers.Grid();
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
        var root = UiHelpers.Stack();
        var actions = UiHelpers.Flow();
        actions.Controls.Add(UiHelpers.Button("Refresh Matrix", RefreshTerminalDashboards));
        actions.Controls.Add(UiHelpers.Button("Open Matrix Window", () => OpenDetachedGridWindow("Cash Matrix", _cashMatrixGrid)));

        _cashMatrixGrid = UiHelpers.Grid();
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
        var root = UiHelpers.Stack();
        var actions = UiHelpers.Flow();
        actions.Controls.Add(UiHelpers.Button("Refresh List", RefreshTerminalDashboards));
        actions.Controls.Add(UiHelpers.Button("Open List Window", () => OpenDetachedGridWindow("Terminal List", _terminalListGrid)));

        _terminalListGrid = UiHelpers.Grid();
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
        var root = UiHelpers.Stack();
        var actions = UiHelpers.Flow();
        actions.Controls.Add(UiHelpers.Button("Refresh Map", RefreshOperationalMap));
        actions.Controls.Add(UiHelpers.Button("Raise Health Review", () => MessageBox.Show(this, "Map health review queued.", "Monitoring")));
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
        var grid = UiHelpers.Grid();
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
        var grid = UiHelpers.Grid();
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
        var root = UiHelpers.Stack();
        var actions = UiHelpers.Flow();
        actions.Controls.Add(UiHelpers.Button("Load NCR Sample", () => LoadXfs("NCR ERROR DISPENSER TIMEOUT")));
        actions.Controls.Add(UiHelpers.Button("Load GRG Sample", () => LoadXfs("GRG TRACE JOURNAL OPEN")));
        actions.Controls.Add(UiHelpers.Button("Load Wincor Sample", () => LoadXfs("WINCOR WOSA/XFS SP ERROR: CDM CASH UNIT EMPTY")));
        actions.Controls.Add(UiHelpers.Button("Load Hyosung Sample", () => LoadXfs("HYOSUNG HCDM DISPENSE FAULT: TAKE CASH TIMEOUT")));
        actions.Controls.Add(UiHelpers.Button("Open XFS Window", () => OpenDetachedGridWindow("XFS Events", _xfsGrid)));
        actions.Controls.Add(UiHelpers.Button("Clear", () => _xfsGrid.Rows.Clear()));
        _xfsGrid = UiHelpers.Grid();
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
        var root = UiHelpers.Stack();
        var actions = UiHelpers.Flow();
        actions.Controls.Add(UiHelpers.Button("Analyze Log", AnalyzeVendorLog));
        actions.Controls.Add(UiHelpers.Button("Extract Probable Cause", AnalyzeVendorLog));
        actions.Controls.Add(UiHelpers.Button("Clear", () => _vendorLog.Clear()));
        _vendorLog = UiHelpers.LogBox();
        _vendorLog.Text = "Paste NCR, GRG, Diebold, or Wincor log text here.";
        root.Controls.Add(_vendorLog);
        root.Controls.Add(actions);
        tab.Controls.Add(root);
        return tab;
    }

    private TabPage BuildReportsTab()
    {
        var tab = new TabPage("Reports");
        var root = UiHelpers.Stack();
        var actions = UiHelpers.Flow();
        actions.Controls.Add(UiHelpers.Button("Refresh Reports", RefreshReportsIndex));
        actions.Controls.Add(UiHelpers.Button("Load Latest Ops Bundle", LoadLatestOpsBundleSummary));
        actions.Controls.Add(UiHelpers.Button("Open Windows Summary", () => OpenDetachedGridWindow("Ops Windows", _reportsWindowGrid)));
        actions.Controls.Add(UiHelpers.Button("Open Files Index", () => OpenDetachedGridWindow("Report Files", _reportsFilesGrid)));
        _reportsInfo = new Label
        {
            AutoSize = true,
            Padding = new Padding(8, 8, 0, 0),
            ForeColor = Color.FromArgb(71, 85, 105)
        };
        actions.Controls.Add(_reportsInfo);

        _reportsWindowGrid = UiHelpers.Grid();
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

        _reportsFilesGrid = UiHelpers.Grid();
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

        var detachedGrid = UiHelpers.Grid();
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

    // -----------------------------------------------------------------
    // Wave 5 — Smart Analysis tab (SS-27)
    // -----------------------------------------------------------------

    private TabPage BuildSmartAnalysisTab()
    {
        var tab = new TabPage("Smart Analysis");
        var root = UiHelpers.Stack();

        // Action bar (top): upload, analyze, re-sort, re-organise.
        var actions = UiHelpers.Flow();
        actions.Controls.Add(UiHelpers.Button("Analyze Text", RunSmartAnalysis));
        actions.Controls.Add(UiHelpers.Button("Re-sort by Severity", () => ReSortSmartFindings("severity")));
        actions.Controls.Add(UiHelpers.Button("Group by Category", () => ReSortSmartFindings("category")));
        actions.Controls.Add(UiHelpers.Button("Load Sample Log", LoadSmartAnalysisSample));
        actions.Controls.Add(UiHelpers.Button("Clear", ClearSmartAnalysis));

        _smartVendorBox = new TextBox
        {
            Width = 110,
            PlaceholderText = "Vendor (NCR/GRG/Wincor)",
            Margin = new Padding(4, 8, 4, 4)
        };
        actions.Controls.Add(_smartVendorBox);

        // Metric card row: critical / warning / info totals + last summary.
        var summaryRow = UiHelpers.CardRow(4);
        _smartCritical = UiHelpers.AddMetricCard(summaryRow, "Critical Findings", "0", Color.FromArgb(238, 82, 83));
        _smartWarning = UiHelpers.AddMetricCard(summaryRow, "Warnings", "0", Color.FromArgb(255, 159, 67));
        _smartInfo = UiHelpers.AddMetricCard(summaryRow, "Info", "0", Color.FromArgb(46, 134, 222));
        _smartSummary = UiHelpers.AddMetricCard(summaryRow, "Last Trace", "—", Color.FromArgb(95, 39, 205));

        // Vertical split: input (top) + results (bottom).
        var split = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Horizontal,
            SplitterDistance = 160
        };

        _smartUploadBox = UiHelpers.LogBox();
        _smartUploadBox.Text = string.Join(Environment.NewLine, new[]
        {
            "[2025-09-14 12:01:33] NCR SDC LINK ERROR: M-146 timeout on dispenser handler.",
            "[2025-09-14 12:01:35] GRG CASSETTE STATUS: CAS1=1800 CAS2=0 CAS3=900 CAS4=600",
            "[2025-09-14 12:01:40] NCR PRINTER JAM at receipt path, customer reports stuck paper.",
            "[2025-09-14 12:02:05] WINCOR WOSA/XFS SP ERROR: CDM CASH UNIT EMPTY",
            "[2025-09-14 12:02:30] WITHDRAWAL AMOUNT=200 EUR processed.",
            "[2025-09-14 12:02:42] CARD CAPTURED — dispute opened on rejected dispense."
        });
        split.Panel1.Controls.Add(_smartUploadBox);

        // Bottom panel: findings grid + cassette/hourly mini-grids.
        var results = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3
        };
        results.RowStyles.Add(new RowStyle(SizeType.Percent, 60));
        results.RowStyles.Add(new RowStyle(SizeType.Percent, 20));
        results.RowStyles.Add(new RowStyle(SizeType.Percent, 20));

        _smartFindingsGrid = UiHelpers.Grid();
        _smartFindingsGrid.Columns.Add("Severity", "Severity");
        _smartFindingsGrid.Columns.Add("Category", "Category");
        _smartFindingsGrid.Columns.Add("Code", "Code");
        _smartFindingsGrid.Columns.Add("Vendor", "Vendor");
        _smartFindingsGrid.Columns.Add("Action", "Recommended Action");
        _smartFindingsGrid.Columns.Add("Source", "Source");
        results.Controls.Add(_smartFindingsGrid, 0, 0);

        var bottomSplit = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Vertical,
            SplitterDistance = 480
        };

        _smartCassetteGrid = UiHelpers.Grid();
        _smartCassetteGrid.Columns.Add("Slot", "Slot");
        _smartCassetteGrid.Columns.Add("Notes", "Notes");
        bottomSplit.Panel1.Controls.Add(_smartCassetteGrid);

        _smartHourlyGrid = UiHelpers.Grid();
        _smartHourlyGrid.Columns.Add("Hour", "Hour");
        _smartHourlyGrid.Columns.Add("Lines", "Lines");
        bottomSplit.Panel2.Controls.Add(_smartHourlyGrid);

        results.Controls.Add(bottomSplit, 0, 1);
        results.Controls.Add(summaryRow, 0, 2);
        split.Panel2.Controls.Add(results);

        root.Controls.Add(actions);
        root.Controls.Add(split);
        tab.Controls.Add(root);
        return tab;
    }

    private void RunSmartAnalysis()
    {
        if (_smartUploadBox is null) return;
        var payload = _smartUploadBox.Text ?? string.Empty;
        if (string.IsNullOrWhiteSpace(payload))
        {
            _smartSummary.Text = "empty payload";
            return;
        }

        var vendor = _smartVendorBox?.Text?.Trim() ?? string.Empty;
        var service = new SmartAnalysisService(_xfsLogAnalysis);
        var report = service.AnalyzeUpload(payload, "monitor-dashboard", vendor);
        RenderSmartAnalysis(report);
    }

    private void LoadSmartAnalysisSample()
    {
        if (_smartUploadBox is null) return;
        _smartUploadBox.Text = string.Join(Environment.NewLine, new[]
        {
            "[2025-09-14 09:14:11] NCR SDC LINK FAULT: M-146 LOST on dispenser 3a handler.",
            "[2025-09-14 09:14:13] GRG CIM RETRACT: cash deposit retract, bin full.",
            "[2025-09-14 09:14:21] NCR PRINTER PART REPLACE: printhead fault detected.",
            "[2025-09-14 09:14:31] WINCOR WOSA/XFS SP ERROR: CDM CASH UNIT EMPTY (CS2)",
            "[2025-09-14 09:14:45] DEPOSIT AMT=850.00 SAR processed.",
            "[2025-09-14 09:14:55] WITHDRAWAL AMT=300.00 SAR completed.",
            "[2025-09-14 09:15:02] GRG TAKE CASH TIMEOUT on dispense.",
            "[2025-09-14 09:15:11] HYOSUNG HCDM DISPENSE FAULT: retract to reject bin."
        });
        RunSmartAnalysis();
    }

    private void ClearSmartAnalysis()
    {
        _smartUploadBox?.Clear();
        _smartFindingsGrid?.Rows.Clear();
        _smartCassetteGrid?.Rows.Clear();
        _smartHourlyGrid?.Rows.Clear();
        _smartCritical.Text = "0";
        _smartWarning.Text = "0";
        _smartInfo.Text = "0";
        _smartSummary.Text = "—";
    }

    private void ReSortSmartFindings(string mode)
    {
        if (_smartFindingsGrid is null) return;
        var rows = new System.Collections.Generic.List<(int idx, string severity, string category, string code, string vendor, string action, string source)>();
        for (var i = 0; i < _smartFindingsGrid.Rows.Count; i++)
        {
            var r = _smartFindingsGrid.Rows[i];
            rows.Add((i,
                r.Cells[0].Value?.ToString() ?? string.Empty,
                r.Cells[1].Value?.ToString() ?? string.Empty,
                r.Cells[2].Value?.ToString() ?? string.Empty,
                r.Cells[3].Value?.ToString() ?? string.Empty,
                r.Cells[4].Value?.ToString() ?? string.Empty,
                r.Cells[5].Value?.ToString() ?? string.Empty));
        }
        var rank = new System.Collections.Generic.Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            ["Critical"] = 0, ["Warning"] = 1, ["Info"] = 2
        };
        rows.Sort((a, b) => string.Equals(mode, "category", StringComparison.OrdinalIgnoreCase)
            ? string.Compare(a.category, b.category, StringComparison.OrdinalIgnoreCase)
            : (rank.TryGetValue(a.severity, out var ra) ? ra : 99)
                .CompareTo(rank.TryGetValue(b.severity, out var rb) ? rb : 99));

        _smartFindingsGrid.SuspendLayout();
        _smartFindingsGrid.Rows.Clear();
        foreach (var r in rows)
            _smartFindingsGrid.Rows.Add(r.severity, r.category, r.code, r.vendor, r.action, r.source);
        _smartFindingsGrid.ResumeLayout();
        _smartSummary.Text = string.Equals(mode, "category", StringComparison.OrdinalIgnoreCase)
            ? $"grouped ({rows.Count})"
            : $"sorted ({rows.Count})";
    }

    private void RenderSmartAnalysis(SmartAnalysisReport report)
    {
        _smartFindingsGrid.SuspendLayout();
        _smartFindingsGrid.Rows.Clear();
        foreach (var f in report.Findings)
        {
            var idx = _smartFindingsGrid.Rows.Add(f.Severity, f.Category, f.Code, f.Vendor, f.RecommendedAction, f.SourceLabel);
            _smartFindingsGrid.Rows[idx].DefaultCellStyle.BackColor = string.Equals(f.Severity, "Critical", StringComparison.OrdinalIgnoreCase)
                ? Color.FromArgb(255, 239, 239)
                : string.Equals(f.Severity, "Warning", StringComparison.OrdinalIgnoreCase)
                    ? Color.FromArgb(255, 247, 233)
                    : Color.FromArgb(239, 247, 255);
        }
        _smartFindingsGrid.ResumeLayout();

        _smartCassetteGrid.SuspendLayout();
        _smartCassetteGrid.Rows.Clear();
        foreach (var kv in report.Value.CassetteMoves.OrderBy(k => k.Key))
            _smartCassetteGrid.Rows.Add($"CS{kv.Key}", kv.Value);
        _smartCassetteGrid.ResumeLayout();

        _smartHourlyGrid.SuspendLayout();
        _smartHourlyGrid.Rows.Clear();
        for (var h = 0; h < 24; h++)
        {
            var count = report.Value.HourlyDensity.TryGetValue(h, out var v) ? v : 0;
            if (count > 0)
                _smartHourlyGrid.Rows.Add($"{h:00}:00", count);
        }
        _smartHourlyGrid.ResumeLayout();

        _smartCritical.Text = report.CriticalCount.ToString();
        _smartWarning.Text = report.WarningCount.ToString();
        _smartInfo.Text = report.InfoCount.ToString();
        _smartSummary.Text = $"{report.TraceId} · {report.LineCount} lines · {report.Value.Withdrawals}W/{report.Value.Deposits}D";
    }

}
