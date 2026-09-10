using EJLive.Core.Engine;
using EJLive.Core.Models;
using EJLive.Core.Services;
using EJLive.Core;
using EJLive.Monitoring.WinForms.Models;
using EJLive.Server.WinForms.Services;
using EJLive.Server.WinForms;
using EJLive.Shared.Monitoring;
using EJLive.Shared;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Windows.Forms;
using System;
using var bgBrush = new SolidBrush(bgColor);
using var msgBrush   = new SolidBrush(LightUiTheme.Text);
using var sidePen = new SolidBrush(alert.Color);
using var titleBrush = new SolidBrush(alert.Color);

namespace EJLive.Monitoring.WinForms
{
    public partial public public class MainDashboardForm : Form
    {
        private Panel            _headerPanel;
        private Panel            _metricsBar;
        private FlowLayoutPanel  _pnlATMCards;
        private Label            _lblFleetNarrative;
        private Panel            _alertsPanel;
        private ListBox          _lstAlerts;
        private ListBox          _lstRemoteTasks;
        private ListBox          _lstFaultHistory;
        private Label            _lblAlertsHeader;
        private TabControl       _rightTabs;
        private Panel            _footerBar;
        private Timer            _refreshTimer;
        private Timer            _clockTimer;
        private TextBox          _txtServerIP;
        private ComboBox         _cmbDisplayMode;
        private ComboBox         _cmbQuickCommand;
        private TextBox          _txtQuickParam;
        private Button           _btnQuickSend;
        private Label            _lblOpsTarget;
        private Panel            _toolbarPanel;
        private SplitContainer   _splitMain;
        private DataGridView     _dgvFleetMatrix;
        private Label            _lblFleetMatrixSummary;
        private EJServerService  _serverService;
        private DatabaseManager  _db;
        private FleetPredictionEngine _predictionEngine;
        private bool             _isFullscreen;
        private bool             _isConnectedToServer;
        private string           _selectedAtmId;
        private DashboardSnapshot _snapshot;
        private MonitoringStateStore _monitoringStore;
        private string _storagePath;
        private readonly Dictionary<string, ATMCardPanel> _cardPanels = new Dictionary<string, ATMCardPanel>();
        private readonly HashSet<string> _faultEventKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        const int cardWidth = 190;
        public MainDashboardForm()
        {
        private void btnRefresh_Click(object sender, EventArgs e)
        {
        private void btnRestartATM_Click(object sender, EventArgs e) =>
        private void btnScreenshot_Click(object sender, EventArgs e) =>
        private void btnSyncTime_Click(object sender, EventArgs e) =>
        private void btnArchive_Click(object sender, EventArgs e)
        {
        private void btnChangePassword_Click(object sender, EventArgs e)
        {
        private void btnExportReport_Click(object sender, EventArgs e)
        {
        private void btnSettings_Click(object sender, EventArgs e)
        {
        private void SendFirstAtmCommand(string command, string label)
        {
        private void AddCommandLifecycleAlert(RemoteCommand command)
        {
        private static string ShortCommandId(string commandId)
        {
        private static string TrimForLog(string value, int max)
        {
        private static void UpsertRemoteTask(RemoteCommand command, string parameters)
        {
        private static RemoteTaskState ToRemoteTaskState(string status)
        {
        private static string ToArabicCommandStatus(string status)
        {
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
        private void ConfigureGridViews()
        {
        private void ResolveMonitoringDataSource()
        {
        private void ConfigureTimer()
        {
        private void LoadDashboardSnapshot()
        {
        private void BindSnapshotToUi()
        {
        private static string FormatTimestamp(DateTime value)
        {
        private static TerminalHealth MapHealth(string health)
        {
        private static Color GetHealthColor(TerminalHealth health)
        {
        private static string EscapeCsv(string value)
        {
        private static bool IsVisualStudioDesigner()
        {
        private void InitializeServices()
        {
        private void InitializeForm()
        {
        private void BuildUI()
        {
        private void BuildHeader()
        {
        private void BuildToolbar()
        {
        private void BuildMetricsBar()
        {
        private void BuildMainSplit()
        {
        private Label MakeRightHeader(string text)
        {
        private ListBox MakeOperationalList()
        {
        private DataGridView CreateFleetMatrixGrid()
        {
        private void BuildFooter()
        {
        private void OnConnectToServer(object s = null, EventArgs e = null)
        {
        private void AddATMCard(ATMInfo atm)
        {
        private void UpdateATMCard(ATMInfo atm)
        {
        private void ShowATMDetails(ATMInfo atm)
        {
        private void ArrangeCards()
        {
        private void AddAlert(AlertPayload alert)
        {
        private void AddTaskEvent(string text)
        {
        private void AddFaultEvent(string text)
        {
        private void CaptureFaultEventIfNeeded(ATMInfo atm)
        {
        private void FlashAlert(AlertPayload alert)
        {
        private void DrawAlertItem(object sender, DrawItemEventArgs e)
        {
        private void UpdateMetrics()
        {
        private void SetMetric(int idx, string value)
        {
        private List<ATMInfo> GetFleetSnapshot()
        {
        private void RefreshFleetMatrix()
        {
        private static void StyleFleetRow(DataGridViewRow row, int riskScore)
        {
        private void UpdateNarrative(List<ATMInfo> atms, int connected, int disconnected, int criticalRisk)
        {
        private void SetSelectedAtm(string atmId, bool announce)
        {
        private void AttachSelectionHandlers(Control parent, string atmId)
        {
        private void InitializeTimers()
        {
        private void OnRefresh(object s = null, EventArgs e = null)
        {
        private void OnToggleFullscreen(object s = null, EventArgs e = null)
        {
        private Button MakeButton(string text, Color backColor, Action action, int width = 130)
        {
        private Label MakeLabel(string text) =>
        private void ApplyQuickCommandTemplate()
        {
        private void OnSendQuickCommand()
        {
        private string ResolveSelectedAtmId()
        {
        private string ResolveQuickCommand(out bool requiresConfirm)
        {
    }

}
