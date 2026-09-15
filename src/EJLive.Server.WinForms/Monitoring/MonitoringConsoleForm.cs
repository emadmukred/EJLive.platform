using EJLive.Core;
using EJLive.Core.Models;
using EJLive.Core.Services;
using EJLive.Core.UI;
using EJLive.Core.Xfs;
using EJLive.Server.Services;

namespace EJLive.Server.WinForms.Monitoring;

/// <summary>
/// NOC / Windows Operations Console. Wave 5 / C-29 — Designer partial split
/// (SS-10 process, proven on <c>JournalStudioForm</c>): this file carries
/// behaviour only; the control tree lives in <c>MonitoringConsoleForm.Designer.cs</c>
/// and every event binding lives in <see cref="WireEvents"/>, so a designer
/// regeneration of the sibling partial can never orphan a handler.
///
/// Wave 6 / C-34 — unified with the central server. This surface was the whole of
/// the separate <c>EJLive.Monitoring.WinForms</c> host (<c>MainDashboardForm</c>,
/// shipped as <c>EJLive.Monitoring.exe</c>). It now lives inside
/// <c>EJLive.Server.WinForms</c> and is hosted two ways from one codebase:
///   * embedded — <see cref="ApplyEmbeddedChrome"/> + parented into the
///     "NOC Monitoring" tab of <c>ServerMainForm</c> (<c>EnsureNocConsole</c>), so
///     operators watch the fleet in the same process that serves it;
///   * detached — <c>new MonitoringConsoleForm().Show(owner)</c> from the tab's
///     "Open Detached Window" button, for a second screen.
/// The host contract is exactly two members (<see cref="ApplyEmbeddedChrome"/>,
/// <see cref="RefreshConsole"/>); everything else stays self-contained, so the
/// console can be re-hosted without touching its own control tree.
/// The retired standalone host (<c>Program.cs</c>, <c>AssemblyInfo.cs</c>,
/// <c>app.config</c>, <c>packages.config</c>) and the consumerless
/// <c>Models/DashboardModels.cs</c> are archived under
/// <c>src/_reference/uncompiled/EJLive.Monitoring.WinForms/retired-host/</c>.
///
/// Control → function map (designer fields, driven by this partial):
///   _overviewGrid + _totalValue/_onlineValue/_syncingValue/_offlineValue/_healthValue —
///       fleet overview, refreshed by <see cref="RefreshOverview"/> / <see cref="UpdateSummary"/>
///   _cashMatrixGrid / _terminalListGrid — per-terminal cash + status tables
///   _mapPanel — operational map card wall (cards are runtime content, rebuilt per refresh)
///   _xfsGrid / _vendorLog — XFS sample loader and vendor probable-cause analysis
///   _reportsWindowGrid / _reportsFilesGrid / _reportsInfo — ops bundle + report index
///   _smartUploadBox / _smartFindingsGrid / _smartCassetteGrid / _smartHourlyGrid /
///       _smartCritical / _smartWarning / _smartInfo / _smartSummary — Smart Analysis (SS-27)
/// </summary>
public sealed partial class MonitoringConsoleForm : Form
{
    private readonly OperationalStateStore _stateStore = new();
    private readonly XfsLogAnalysisService _xfsLogAnalysis = new();
    private readonly OperationalReportCatalogService _reportCatalog = new();
    private readonly ClientTelemetryHistoryService _telemetryHistory = new();
    private DateTime _lastTelemetryRefreshUtc = DateTime.MinValue;

    public MonitoringConsoleForm()
    {
        InitializeComponent();
        WireEvents();
        PrepareGrids();
        PerformInitialRefresh();
    }

    /// <summary>
    /// Every event binding for this surface, in one place (SS-10). The designer
    /// partial deliberately contains no bindings; a regenerated
    /// <c>MonitoringConsoleForm.Designer.cs</c> therefore cannot drop a handler.
    /// Wave 6 / C-33: the eleven bindings to parameterless refresh commands are
    /// discard lambdas (<c>(_, _) =&gt; RefreshOverview()</c>), never bare method
    /// groups — a method group with no <c>(object?, EventArgs)</c> overload does not
    /// convert to <see cref="EventHandler"/> (CS0123), which is what kept the Windows
    /// CI build red through Wave 5.
    /// </summary>
    private void WireEvents()
    {
        _overviewRefreshButton.Click += (_, _) => RefreshOverview();
        _overviewWindowButton.Click += () => OpenDetachedGridWindow("Overview", _overviewGrid);
        _overviewReviewButton.Click += () => MessageBox.Show(this, "Health review queued.", "Monitoring");

        _cashMatrixRefreshButton.Click += (_, _) => RefreshTerminalDashboards();
        _cashMatrixWindowButton.Click += () => OpenDetachedGridWindow("Cash Matrix", _cashMatrixGrid);

        _terminalListRefreshButton.Click += (_, _) => RefreshTerminalDashboards();
        _terminalListWindowButton.Click += () => OpenDetachedGridWindow("Terminal List", _terminalListGrid);

        _mapRefreshButton.Click += (_, _) => RefreshOperationalMap();
        _mapReviewButton.Click += () => MessageBox.Show(this, "Map health review queued.", "Monitoring");

        _xfsNcrButton.Click += () => LoadXfs("NCR ERROR DISPENSER TIMEOUT");
        _xfsGrgButton.Click += () => LoadXfs("GRG TRACE JOURNAL OPEN");
        _xfsWincorButton.Click += () => LoadXfs("WINCOR WOSA/XFS SP ERROR: CDM CASH UNIT EMPTY");
        _xfsHyosungButton.Click += () => LoadXfs("HYOSUNG HCDM DISPENSE FAULT: TAKE CASH TIMEOUT");
        _xfsWindowButton.Click += () => OpenDetachedGridWindow("XFS Events", _xfsGrid);
        _xfsClearButton.Click += () => _xfsGrid.Rows.Clear();

        _vendorAnalyzeButton.Click += (_, _) => AnalyzeVendorLog();
        _vendorExtractButton.Click += (_, _) => AnalyzeVendorLog();
        _vendorClearButton.Click += () => _vendorLog.Clear();

        _reportsRefreshButton.Click += (_, _) => RefreshReportsIndex();
        _reportsBundleButton.Click += (_, _) => LoadLatestOpsBundleSummary();
        _reportsWindowSummaryButton.Click += () => OpenDetachedGridWindow("Ops Windows", _reportsWindowGrid);
        _reportsFilesIndexButton.Click += () => OpenDetachedGridWindow("Report Files", _reportsFilesGrid);

        _smartAnalyzeButton.Click += (_, _) => RunSmartAnalysis();
        _smartReSortButton.Click += () => ReSortSmartFindings("severity");
        _smartGroupButton.Click += () => ReSortSmartFindings("category");
        _smartSampleButton.Click += (_, _) => LoadSmartAnalysisSample();
        _smartClearButton.Click += (_, _) => ClearSmartAnalysis();
    }

    /// <summary>
    /// Runtime performance pass over the designer-built grids:
    /// <see cref="ControlRenderingExtensions.EnableDoubleBuffering"/> is a
    /// reflection tweak the designer cannot express, so it is applied here
    /// (the visual theme it complements is set in the designer partial).
    /// </summary>
    private void PrepareGrids()
    {
        _overviewGrid.EnableDoubleBuffering();
        _cashMatrixGrid.EnableDoubleBuffering();
        _terminalListGrid.EnableDoubleBuffering();
        _deviceStateGrid.EnableDoubleBuffering();
        _syncStatusGrid.EnableDoubleBuffering();
        _xfsGrid.EnableDoubleBuffering();
        _reportsWindowGrid.EnableDoubleBuffering();
        _reportsFilesGrid.EnableDoubleBuffering();
        _smartFindingsGrid.EnableDoubleBuffering();
        _smartCassetteGrid.EnableDoubleBuffering();
        _smartHourlyGrid.EnableDoubleBuffering();
    }

    /// <summary>
    /// Seed + first paint, in the exact order the legacy tab builders applied
    /// it (each <c>Build*Tab</c> finished with its own refresh call). The static
    /// Device-State rows and the <c>DateTime.Now</c>-relative Realtime-Sync demo
    /// rows live here because a designer partial cannot express relative dates.
    /// </summary>
    private void PerformInitialRefresh()
    {
        _deviceStateGrid.Rows.Add("Card Reader", "XFS", "Ready");
        _deviceStateGrid.Rows.Add("Cash Dispenser", "XFS", "Ready");
        _deviceStateGrid.Rows.Add("Journal File", "File System", "Watching");
        _deviceStateGrid.Rows.Add("Network Link", "Transport", "Online");

        _syncStatusGrid.Rows.Add("JournalOutbox", 3, 1, DateTime.Now.AddSeconds(-20));
        _syncStatusGrid.Rows.Add("ImageSync", 0, 0, DateTime.Now.AddMinutes(-2));

        RefreshOverview();
        RefreshTerminalDashboards();
        RefreshTerminalDashboards();
        RefreshOperationalMap();
        RefreshReportsIndex();
        LoadLatestOpsBundleSummary();
    }

    // -----------------------------------------------------------------
    // Wave 6 / C-34 — hosting contract with the central server console
    // -----------------------------------------------------------------

    /// <summary>
    /// Re-shapes the console to live as a child of a <c>TabPage</c> host (the
    /// "NOC Monitoring" tab of <c>ServerMainForm</c>): a child form must not be
    /// top-level, must not paint its own caption, and must not enforce a
    /// <see cref="Form.MinimumSize"/> the host panel cannot honour. Called by the
    /// host immediately before <c>Controls.Add</c>; the standalone window path
    /// (<c>Open Detached Window</c>) never calls it and keeps the full chrome.
    /// </summary>
    public void ApplyEmbeddedChrome()
    {
        TopLevel = false;
        FormBorderStyle = FormBorderStyle.None;
        Dock = DockStyle.Fill;
        MinimumSize = Size.Empty;
    }

    /// <summary>
    /// Host-facing refresh. Re-runs the same chain <see cref="PerformInitialRefresh"/>
    /// seeds at construction — overview (which cascades into the terminal dashboards
    /// and the operational map), then the report index and the latest ops bundle —
    /// without re-adding the two static demo tables, which are seeded exactly once.
    /// </summary>
    public void RefreshConsole()
    {
        RefreshOverview();
        RefreshReportsIndex();
        LoadLatestOpsBundleSummary();
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
