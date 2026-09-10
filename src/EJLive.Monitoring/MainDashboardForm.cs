using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Windows.Forms;
using EJLive.Core;
using EJLive.Core.Engine;
using EJLive.Core.Models;
using EJLive.Core.Services;
using EJLive.Server.WinForms;
using EJLive.Server.WinForms.Services;
using EJLive.Shared;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using EJLive.Core;
using EJLive.Monitoring.WinForms.Models;
using EJLive.Shared.Monitoring;

namespace EJLive.Monitoring.WinForms
{
    public partial class MainDashboardForm : Form
    {
namespace EJLive.Monitoring.WinForms
{
    /// <summary>
    /// لوحة مراقبة NOC المستقلة الكاملة
    /// تتصل بـ EJServer مباشرة وتعرض: بطاقات الصرافات + مؤشرات حية + سجل التنبيهات
    /// تدعم: اتصال مباشر بـ ServerEngine (للعرض فقط) أو عبر Polling
    /// </summary>
    public partial class MainDashboardForm : Form
    {
        #region Controls
        private Panel            _headerPanel;
        private Label            _lblTitle, _lblSubtitle, _lblServerStatus;
        private Panel            _metricsBar;
        private Label[]          _metricLabels = new Label[6];
        private Label[]          _metricValues = new Label[6];
        private FlowLayoutPanel  _pnlATMCards;
        private Label            _lblFleetNarrative;
        private Panel            _alertsPanel;
        private ListBox          _lstAlerts;
        private ListBox          _lstRemoteTasks;
        private ListBox          _lstFaultHistory;
        private Label            _lblAlertsHeader;
        private TabControl       _rightTabs;
        private Panel            _footerBar;
        private Label            _lblFooterTime, _lblFooterVersion;
        private Timer            _refreshTimer;
        private Timer            _clockTimer;
        private Button           _btnRefresh, _btnConnectServer, _btnFullscreen;
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
        #endregion

        #region Services
        private EJServerService  _serverService;
        private DatabaseManager  _db;
        private FleetPredictionEngine _predictionEngine;
        private readonly Dictionary<string, ATMCardPanel> _cardPanels = new Dictionary<string, ATMCardPanel>();
        private readonly HashSet<string> _faultEventKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private bool             _isFullscreen;
        private bool             _isConnectedToServer;
        private string           _selectedAtmId;
        #endregion

        // ==========================================
        // بناء الفورم
        // ==========================================

        public MainDashboardForm()
        {
            if (IsVisualStudioDesigner())
            {
                InitializeComponent();
                return;
            }

            InitializeServices();
            InitializeForm();
            BuildUI();
            InitializeTimers();
        }

        private static bool IsVisualStudioDesigner()
        {
            if (System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime)
                return true;

            var processName = System.Diagnostics.Process.GetCurrentProcess().ProcessName;
            return processName.IndexOf("devenv", StringComparison.OrdinalIgnoreCase) >= 0
                || processName.IndexOf("DesignToolsServer", StringComparison.OrdinalIgnoreCase) >= 0
                || processName.IndexOf("XDesProc", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private void InitializeServices()
        {
            _db = DatabaseManager.Instance;
            _db.Initialize(AppConstants.DefaultDatabasePath);
            AppLogger.Instance.Initialize(AppConstants.DefaultLogPath, "noc");
            _predictionEngine = new FleetPredictionEngine();

            AlertManager.Instance.OnAlert    += (s, a) => AddAlert(a);
            AlertManager.Instance.OnCritical += (s, a) => FlashAlert(a);
        }

        private void InitializeForm()
        {
            Text            = $"EJLive NOC Dashboard v{AppConstants.AppVersion}";
            Size            = new Size(1600, 900);
            MinimumSize     = new Size(1100, 700);
            StartPosition   = FormStartPosition.CenterScreen;
            WindowState     = FormWindowState.Maximized;
            BackColor       = LightUiTheme.Window;
            ForeColor       = LightUiTheme.Text;
            Font            = new Font("Segoe UI", 9.5f);
            FormBorderStyle = FormBorderStyle.Sizable;
        }

        // ==========================================
        // بناء الواجهة
        // ==========================================

        private void BuildUI()
        {
            BuildMainSplit();
            BuildFooter();
            BuildMetricsBar();
            BuildToolbar();
            BuildHeader();
            LightUiTheme.Apply(this);
            AddAlert(new AlertPayload
            {
                Severity = AlertSeverity.Info,
                Title = "NOC جاهز",
                Message = "ابدأ الاتصال بالخادم لمراقبة الصرافات والتنبيهات لحظياً.",
                Source = "NOC"
            });
        }

        private void BuildHeader()
        {
            _headerPanel = new Panel
            {
                Height    = 78,
                Dock      = DockStyle.Top,
                BackColor = LightUiTheme.Surface,
                Padding   = new Padding(20, 12, 20, 12)
            };

            // تدرج
            _headerPanel.Paint += (s, e) =>
            {
                var rect = _headerPanel.ClientRectangle;
                using var brush = new System.Drawing.Drawing2D.LinearGradientBrush(
                    rect, LightUiTheme.Surface, Color.FromArgb(235, 241, 248),
                    System.Drawing.Drawing2D.LinearGradientMode.Horizontal);
                e.Graphics.FillRectangle(brush, rect);
            };

            _lblTitle = new Label
            {
                Text      = "EJLive Enterprise — NOC Dashboard",
                Font      = new Font("Segoe UI", 18f, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 102, 204),
                AutoSize  = true,
                Location  = new Point(20, 10)
            };

            _lblSubtitle = new Label
            {
                Text      = $"مركز مراقبة الصرافات — v{AppConstants.AppVersion}",
                Font      = new Font("Segoe UI", 10f),
                ForeColor = LightUiTheme.Muted,
                AutoSize  = true,
                Location  = new Point(22, 44)
            };

            _lblServerStatus = new Label
            {
                Text      = "○ غير متصل بالخادم",
                Font      = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = LightUiTheme.Muted,
                AutoSize  = true,
                Location  = new Point(650, 27),
                Anchor    = AnchorStyles.Top | AnchorStyles.Right
            };

            _headerPanel.Controls.AddRange(new Control[] { _lblTitle, _lblSubtitle, _lblServerStatus });
            _headerPanel.Resize += (s, e) =>
            {
                _lblServerStatus.Left = Math.Max(650, _headerPanel.ClientSize.Width - _lblServerStatus.Width - 28);
            };
            Controls.Add(_headerPanel);
        }

        private void BuildToolbar()
        {
            _toolbarPanel = new Panel
            {
                Height    = 58,
                Dock      = DockStyle.Top,
                BackColor = LightUiTheme.SurfaceAlt,
                Padding   = new Padding(12, 8, 12, 8)
            };

            var flow = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, WrapContents = false, AutoScroll = true, Padding = Padding.Empty };

            _txtServerIP  = new TextBox { Text = "192.168.1.100", Width = 180, BackColor = LightUiTheme.Surface, ForeColor = LightUiTheme.Text, BorderStyle = BorderStyle.FixedSingle, Margin = new Padding(4, 6, 4, 4) };
            _btnConnectServer = MakeButton("اتصال بالخادم", Color.FromArgb(20, 70, 40), () => OnConnectToServer(), 160);
            _cmbDisplayMode   = new ComboBox { Width = 150, DropDownStyle = ComboBoxStyle.DropDownList, BackColor = LightUiTheme.Surface, ForeColor = LightUiTheme.Text, FlatStyle = FlatStyle.System, Margin = new Padding(4, 6, 4, 4) };
            _cmbDisplayMode.Items.AddRange(new object[] { "بطاقات (4×N)", "بطاقات (6×N)", "عرض كامل", "جدول" });
            _cmbDisplayMode.SelectedIndex = 0;
            _cmbDisplayMode.SelectedIndexChanged += (s, e) => ArrangeCards();
            _btnRefresh     = MakeButton("تحديث",        Color.FromArgb(30, 50, 80), () => OnRefresh(), 118);
            _btnFullscreen  = MakeButton("ملء الشاشة",   Color.FromArgb(40, 30, 60), () => OnToggleFullscreen(), 140);
            _lblOpsTarget   = new Label
            {
                Text = "الهدف: تلقائي",
                Width = 180,
                Height = 34,
                Margin = new Padding(4, 6, 4, 4),
                ForeColor = LightUiTheme.Text,
                BackColor = LightUiTheme.Surface,
                BorderStyle = BorderStyle.FixedSingle,
                TextAlign = ContentAlignment.MiddleCenter
            };
            _cmbQuickCommand = new ComboBox
            {
                Width = 165,
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = LightUiTheme.Surface,
                ForeColor = LightUiTheme.Text,
                FlatStyle = FlatStyle.System,
                Margin = new Padding(4, 6, 4, 4)
            };
            _cmbQuickCommand.Items.AddRange(new object[]
            {
                "مزامنة وقت",
                "مزامنة مجلد",
                "مزامنة صور",
                "تغيير كلمة سر",
                "مزامنة جورنال فورية",
                "إحصاءات التشغيل"
            });
            _cmbQuickCommand.SelectedIndexChanged += (s, e) => ApplyQuickCommandTemplate();

            _txtQuickParam = new TextBox
            {
                Width = 280,
                BackColor = LightUiTheme.Surface,
                ForeColor = LightUiTheme.Text,
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(4, 6, 4, 4)
            };
            _btnQuickSend = MakeButton("تنفيذ فوري", Color.FromArgb(86, 56, 118), OnSendQuickCommand, 130);

            flow.Controls.AddRange(new Control[] {
                MakeLabel("IP الخادم:"), _txtServerIP, _btnConnectServer,
                MakeLabel("  عرض:"), _cmbDisplayMode, _btnRefresh, _btnFullscreen,
                MakeLabel("  الصراف:"), _lblOpsTarget,
                _cmbQuickCommand, _txtQuickParam, _btnQuickSend
            });

            _cmbQuickCommand.SelectedIndex = 0;
            ApplyQuickCommandTemplate();
            _toolbarPanel.Controls.Add(flow);
            Controls.Add(_toolbarPanel);
        }

        private void BuildMetricsBar()
        {
            _metricsBar = new Panel
            {
                Height    = 112,
                Dock      = DockStyle.Top,
                BackColor = LightUiTheme.Window,
                Padding   = new Padding(12, 10, 12, 10)
            };

            var flow = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, WrapContents = false, AutoScroll = true, Padding = Padding.Empty };

            string[] mLabels = { "متصل", "منقطع", "جورنال اليوم", "تنبيهات نشطة", "مخاطر حرجة", "آخر تحديث" };
            string[] mIcons  = { "ON", "OFF", "EJ", "ALRT", "RISK", "TIME" };
            Color[]  mColors = {
                Color.FromArgb(52,199,89), Color.FromArgb(255,69,58),
                Color.FromArgb(10,132,255), Color.FromArgb(255,149,0),
                Color.FromArgb(255,69,58), Color.FromArgb(142,142,147)
            };

            for (int i = 0; i < 6; i++)
            {
                const int cardWidth = 190;
                var card = new Panel { Width = cardWidth, Height = 88, Margin = new Padding(6, 2, 6, 4), BackColor = LightUiTheme.Surface, BorderStyle = BorderStyle.FixedSingle };
                var iconLabel = new Label { Text = mIcons[i], Location = new Point(10, 10), Size = new Size(28, 30), TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 14f) };
                _metricValues[i] = new Label { Text = "—", Location = new Point(44, 8), Size = new Size(cardWidth - 54, 38), Font = new Font("Segoe UI", 20f, FontStyle.Bold), ForeColor = mColors[i], TextAlign = ContentAlignment.MiddleLeft, AutoEllipsis = true };
                _metricLabels[i] = new Label { Text = mLabels[i], Location = new Point(10, 60), Size = new Size(cardWidth - 20, 22), Font = new Font("Segoe UI", 8.5f), ForeColor = LightUiTheme.Muted, TextAlign = ContentAlignment.MiddleLeft, AutoEllipsis = true };
                card.Controls.AddRange(new Control[] { iconLabel, _metricValues[i], _metricLabels[i] });
                flow.Controls.Add(card);
            }
            _metricsBar.Controls.Add(flow);
            Controls.Add(_metricsBar);
        }

        private void BuildMainSplit()
        {
            _splitMain = new SplitContainer
            {
                Dock             = DockStyle.Fill,
                Orientation      = Orientation.Vertical,
                SplitterDistance = 1100,
                SplitterWidth    = 4,
                BackColor        = LightUiTheme.Border
            };

            // ═ الجانب الأيسر: بطاقات الصرافات
            var leftPanel = new Panel { Dock = DockStyle.Fill, BackColor = LightUiTheme.Window };
            var cardHeader = new Label
            {
                Text      = "حالة الصرافات",
                Height    = 32,
                Dock      = DockStyle.Top,
                Font      = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 102, 204),
                BackColor = LightUiTheme.Surface,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding   = new Padding(12, 0, 0, 0)
            };
            _lblFleetNarrative = new Label
            {
                Text = "لا توجد صرافات معروضة حتى الآن. عند تشغيل الخادم واتصال العملاء ستظهر البطاقات هنا مع الصحة، آخر مزامنة، والتنبيهات.",
                Dock = DockStyle.Top,
                Height = 50,
                BackColor = LightUiTheme.SurfaceAlt,
                ForeColor = LightUiTheme.Muted,
                Padding = new Padding(12, 6, 12, 6),
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 9f)
            };
            _pnlATMCards = new FlowLayoutPanel
            {
                Dock          = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents  = true,
                AutoScroll    = true,
                BackColor     = LightUiTheme.Window,
                Padding       = new Padding(10)
            };

            _lblFleetMatrixSummary = new Label
            {
                Dock = DockStyle.Bottom,
                Height = 26,
                BackColor = LightUiTheme.Surface,
                ForeColor = LightUiTheme.Muted,
                Padding = new Padding(10, 4, 10, 4),
                TextAlign = ContentAlignment.MiddleLeft,
                Visible = false
            };

            _dgvFleetMatrix = CreateFleetMatrixGrid();
            _dgvFleetMatrix.Visible = false;

            leftPanel.Controls.AddRange(new Control[] { _dgvFleetMatrix, _pnlATMCards, _lblFleetMatrixSummary, _lblFleetNarrative, cardHeader });
            _splitMain.Panel1.Controls.Add(leftPanel);

            // ═ الجانب الأيمن: التنبيهات والمهام وتاريخ الأعطال
            _alertsPanel = new Panel { Dock = DockStyle.Fill, BackColor = LightUiTheme.Surface };
            _rightTabs = new TabControl
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9f),
                RightToLeft = RightToLeft.Yes,
                RightToLeftLayout = true
            };

            var alertsTab = new TabPage("التنبيهات") { BackColor = LightUiTheme.Surface };
            var tasksTab = new TabPage("المهام والرفع") { BackColor = LightUiTheme.Surface };
            var faultsTab = new TabPage("تاريخ الأعطال") { BackColor = LightUiTheme.Surface };

            _lblAlertsHeader = MakeRightHeader("مركز التنبيهات");
            _lstAlerts = new ListBox
            {
                Dock            = DockStyle.Fill,
                BackColor       = LightUiTheme.Surface,
                ForeColor       = LightUiTheme.Text,
                BorderStyle     = BorderStyle.None,
                Font            = new Font("Segoe UI", 8.5f),
                DrawMode        = DrawMode.OwnerDrawFixed,
                ItemHeight      = 48,
                SelectionMode   = SelectionMode.None
            };
            _lstAlerts.DrawItem += DrawAlertItem;

            _lstRemoteTasks = MakeOperationalList();
            _lstFaultHistory = MakeOperationalList();

            alertsTab.Controls.Add(_lstAlerts);
            alertsTab.Controls.Add(_lblAlertsHeader);
            tasksTab.Controls.Add(_lstRemoteTasks);
            tasksTab.Controls.Add(MakeRightHeader("Remote Task / Upload Monitor"));
            faultsTab.Controls.Add(_lstFaultHistory);
            faultsTab.Controls.Add(MakeRightHeader("Fault / Event History"));

            _rightTabs.TabPages.Add(alertsTab);
            _rightTabs.TabPages.Add(tasksTab);
            _rightTabs.TabPages.Add(faultsTab);
            _alertsPanel.Controls.Add(_rightTabs);
            _splitMain.Panel2.Controls.Add(_alertsPanel);

            Controls.Add(_splitMain);
        }

        private Label MakeRightHeader(string text)
        {
            return new Label
            {
                Text      = text,
                Height    = 32,
                Dock      = DockStyle.Top,
                Font      = new Font("Segoe UI", 10.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(255, 149, 0),
                BackColor = LightUiTheme.SurfaceAlt,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding   = new Padding(12, 0, 0, 0)
            };
        }

        private ListBox MakeOperationalList()
        {
            return new ListBox
            {
                Dock = DockStyle.Fill,
                BackColor = LightUiTheme.Surface,
                ForeColor = LightUiTheme.Text,
                BorderStyle = BorderStyle.None,
                Font = new Font("Consolas", 8.5f),
                HorizontalScrollbar = true
            };
        }

        private DataGridView CreateFleetMatrixGrid()
        {
            var grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = LightUiTheme.Window,
                BorderStyle = BorderStyle.None,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                ReadOnly = true,
                MultiSelect = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible = false,
                GridColor = LightUiTheme.Border
            };

            grid.ColumnHeadersDefaultCellStyle.BackColor = LightUiTheme.SurfaceAlt;
            grid.ColumnHeadersDefaultCellStyle.ForeColor = LightUiTheme.Text;
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 8.8f, FontStyle.Bold);
            grid.EnableHeadersVisualStyles = false;

            grid.DefaultCellStyle.BackColor = LightUiTheme.Surface;
            grid.DefaultCellStyle.ForeColor = LightUiTheme.Text;
            grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(22, 73, 138);
            grid.DefaultCellStyle.SelectionForeColor = Color.White;
            grid.DefaultCellStyle.Font = new Font("Segoe UI", 8.6f);

            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "ATM", HeaderText = "الصراف", FillWeight = 18f });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Type", HeaderText = "النوع", FillWeight = 10f });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Status", HeaderText = "الحالة", FillWeight = 13f });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Heartbeat", HeaderText = "آخر نبضة", FillWeight = 10f });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Sync", HeaderText = "آخر مزامنة", FillWeight = 10f });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Pending", HeaderText = "طابور/فشل", FillWeight = 11f });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Health", HeaderText = "الصحة", FillWeight = 9f });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Risk", HeaderText = "المخاطر", FillWeight = 9f });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Action", HeaderText = "الإجراء المقترح", FillWeight = 20f });

            grid.SelectionChanged += (s, e) =>
            {
                if (grid.CurrentRow?.Tag is ATMInfo selected)
                    SetSelectedAtm(selected.ATM_ID, true);
            };

            return grid;
        }

        private void BuildFooter()
        {
            _footerBar = new Panel
            {
                Height    = 28,
                Dock      = DockStyle.Bottom,
                BackColor = LightUiTheme.Surface
            };
            _lblFooterTime    = new Label { Text = "", AutoSize = true, ForeColor = LightUiTheme.Muted, Location = new Point(12, 6), Font = new Font("Consolas", 8.5f) };
            _lblFooterVersion = new Label { Text = AppConstants.Copyright, AutoSize = true, ForeColor = Color.FromArgb(60, 60, 65), Location = new Point(400, 6), Font = new Font("Segoe UI", 8f) };
            _footerBar.Controls.AddRange(new Control[] { _lblFooterTime, _lblFooterVersion });
            Controls.Add(_footerBar);
        }

        // ==========================================
        // الاتصال بالخادم
        // ==========================================

        private void OnConnectToServer(object s = null, EventArgs e = null)
        {
            if (_isConnectedToServer)
            {
                _serverService?.Dispose();
                _isConnectedToServer = false;
                _lblServerStatus.Text      = "○ غير متصل بالخادم";
                _lblServerStatus.ForeColor = Color.FromArgb(99, 99, 102);
                _btnConnectServer.Text     = "اتصال بالخادم";
                return;
            }

            try
            {
                _serverService = new EJServerService();
                _serverService.OnATMConnected    += (sv, atm) => BeginInvoke(new Action(() => AddATMCard(atm)));
                _serverService.OnATMDisconnected += (sv, atm) => BeginInvoke(new Action(() => UpdateATMCard(atm)));
                _serverService.OnATMUpdated      += (sv, atm) => BeginInvoke(new Action(() => UpdateATMCard(atm)));
                _serverService.OnAlert           += (sv, a)   => BeginInvoke(new Action(() => AddAlert(a)));
                _serverService.OnCommandChanged  += (sv, c)   => BeginInvoke(new Action(() => AddCommandLifecycleAlert(c)));
                _serverService.OnLog             += (sv, m)   => AppLogger.Instance.Info(m, "NOC");
                _serverService.Start();

                _isConnectedToServer           = true;
                _lblServerStatus.Text          = $"● خادم نشط على TCP/{AppConstants.DefaultPort}";
                _lblServerStatus.ForeColor     = Color.FromArgb(52, 199, 89);
                _btnConnectServer.Text         = "إيقاف الخادم";
                AddAlert(new AlertPayload { Severity = AlertSeverity.Info, Title = "خادم مُشغّل", Message = $"EJLive Server يعمل على TCP/{AppConstants.DefaultPort}" });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل تشغيل الخادم:\n{ex.Message}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ==========================================
        // بطاقات الصرافات
        // ==========================================

        private void AddATMCard(ATMInfo atm)
        {
            if (atm == null || string.IsNullOrWhiteSpace(atm.ATM_ID)) return;
            if (_cardPanels.ContainsKey(atm.ATM_ID))
            {
                UpdateATMCard(atm);
                return;
            }

            var card = new ATMCardPanel(atm);
            card.OnDoubleClickCard += (s, a) =>
            {
                SetSelectedAtm(a?.ATM_ID, true);
                ShowATMDetails(a);
            };
            card.Click += (s, e) => SetSelectedAtm(atm.ATM_ID, true);
            AttachSelectionHandlers(card, atm.ATM_ID);

            _cardPanels[atm.ATM_ID] = card;
            _pnlATMCards.Controls.Add(card);
            if (string.IsNullOrWhiteSpace(_selectedAtmId))
                SetSelectedAtm(atm.ATM_ID, false);

            UpdateMetrics();
        }

        private void UpdateATMCard(ATMInfo atm)
        {
            if (atm == null || string.IsNullOrWhiteSpace(atm.ATM_ID)) return;
            if (_cardPanels.TryGetValue(atm.ATM_ID, out var card))
                card.UpdateATM(atm);
            else
                AddATMCard(atm);

            if (!string.IsNullOrWhiteSpace(_selectedAtmId) &&
                string.Equals(_selectedAtmId, atm.ATM_ID, StringComparison.OrdinalIgnoreCase))
                SetSelectedAtm(atm.ATM_ID, false);

            CaptureFaultEventIfNeeded(atm);
            UpdateMetrics();
        }

        private void ShowATMDetails(ATMInfo atm)
        {
            var prediction = _predictionEngine.Predict(atm);
            var info = $"صراف: {atm.ATM_ID} ({atm.ATM_Name})\n" +
                       $"النوع: {atm.ATM_Type} | الشبكة: {atm.NetworkType}\n" +
                       $"الحالة: {atm.GetStatusLabel()}\n" +
                       $"آخر Heartbeat: {atm.GetElapsed(atm.LastHeartbeatUtc)}\n" +
                       $"آخر جورنال: {atm.LastJournalFile ?? "—"}\n" +
                       $"بيانات مُرسلة: {atm.TotalSyncedBytes / 1024.0:F1} KB\n" +
                       $"عمليات موافق عليها: {atm.ApprovedTransactions}\n\n" +
                       $"التنبؤ: {prediction.Level} ({prediction.RiskScore}%)\n" +
                       $"السبب: {prediction.Reason}\n" +
                       $"الإجراء: {prediction.RecommendedAction}";
            MessageBox.Show(info, $"تفاصيل الصراف — {atm.ATM_ID}", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ArrangeCards()
        {
            var mode = _cmbDisplayMode.SelectedIndex;
            var tableMode = mode == 3;

            _splitMain.Panel2Collapsed = mode == 2;
            if (_dgvFleetMatrix != null) _dgvFleetMatrix.Visible = tableMode;
            if (_lblFleetMatrixSummary != null) _lblFleetMatrixSummary.Visible = tableMode;
            if (_pnlATMCards != null) _pnlATMCards.Visible = !tableMode;

            if (tableMode)
            {
                RefreshFleetMatrix();
                return;
            }

            foreach (ATMCardPanel card in _pnlATMCards.Controls)
            {
                card.Size = mode switch
                {
                    1 => new Size(210, 155),  // 6×N
                    2 => new Size(270, 182),  // عرض كامل
                    _ => new Size(240, 170)   // 4×N
                };
            }
            _pnlATMCards.Invalidate();
        }

        // ==========================================
        // التنبيهات
        // ==========================================

        private void AddAlert(AlertPayload alert)
        {
            if (InvokeRequired) { Invoke(new Action(() => AddAlert(alert))); return; }
            _lstAlerts.Items.Insert(0, alert);
            if (_lstAlerts.Items.Count > 200)
                _lstAlerts.Items.RemoveAt(_lstAlerts.Items.Count - 1);
            _lstAlerts.Invalidate();
        }

        private void AddTaskEvent(string text)
        {
            if (_lstRemoteTasks == null || string.IsNullOrWhiteSpace(text)) return;
            if (_lstRemoteTasks.InvokeRequired) { _lstRemoteTasks.Invoke(new Action(() => AddTaskEvent(text))); return; }
            _lstRemoteTasks.Items.Insert(0, $"[{DateTime.Now:HH:mm:ss}] {text}");
            if (_lstRemoteTasks.Items.Count > 300)
                _lstRemoteTasks.Items.RemoveAt(_lstRemoteTasks.Items.Count - 1);
        }

        private void AddFaultEvent(string text)
        {
            if (_lstFaultHistory == null || string.IsNullOrWhiteSpace(text)) return;
            if (_lstFaultHistory.InvokeRequired) { _lstFaultHistory.Invoke(new Action(() => AddFaultEvent(text))); return; }
            _lstFaultHistory.Items.Insert(0, $"[{DateTime.Now:HH:mm:ss}] {text}");
            if (_lstFaultHistory.Items.Count > 300)
                _lstFaultHistory.Items.RemoveAt(_lstFaultHistory.Items.Count - 1);
        }

        private void CaptureFaultEventIfNeeded(ATMInfo atm)
        {
            var prediction = _predictionEngine.Predict(atm);
            if (prediction.RiskScore < 75 && string.IsNullOrWhiteSpace(atm.LastErrorCode))
                return;

            var key = $"{atm.ATM_ID}|{atm.LastErrorCode}|{atm.ConnectionStatus}|{prediction.RiskScore / 10}";
            if (!_faultEventKeys.Add(key))
                return;

            OperationalStateStore.Instance.UpsertFault(new FaultEventRecord
            {
                TerminalId = atm.ATM_ID,
                Vendor = atm.ATM_Type,
                DeviceCode = string.IsNullOrWhiteSpace(atm.LastErrorCode) ? "NOC" : atm.LastErrorCode,
                EventCode = atm.ConnectionStatus.ToString(),
                EventName = prediction.Level,
                Severity = prediction.RiskScore >= 75 ? FaultEventSeverity.Critical : FaultEventSeverity.Warning,
                RawLine = atm.LastErrorMessage,
                RecommendedAction = prediction.RecommendedAction,
                CorrelationId = key
            });
            AddFaultEvent($"{atm.ATM_ID} | {atm.GetStatusLabel()} | risk={prediction.RiskScore}% | {prediction.Reason}");
        }

        private void FlashAlert(AlertPayload alert)
        {
            if (InvokeRequired) { Invoke(new Action(() => FlashAlert(alert))); return; }
            AddAlert(alert);
            // وميض للتنبيهات الحرجة
            var originalColor = _headerPanel.BackColor;
            var timer = new Timer { Interval = 200 };
            int count = 0;
            timer.Tick += (s, e) =>
            {
                _headerPanel.BackColor = count % 2 == 0 ? Color.FromArgb(60, 15, 15) : originalColor;
                count++;
                if (count >= 6) { timer.Stop(); timer.Dispose(); _headerPanel.BackColor = originalColor; }
            };
            timer.Start();
        }

        private void DrawAlertItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0 || e.Index >= _lstAlerts.Items.Count) return;
            var alert = _lstAlerts.Items[e.Index] as AlertPayload;
            if (alert == null) return;

            var bgColor = alert.Severity switch
            {
                AlertSeverity.Emergency => Color.FromArgb(255, 235, 238),
                AlertSeverity.Critical  => Color.FromArgb(255, 241, 242),
                AlertSeverity.Warning   => Color.FromArgb(255, 248, 225),
                _ => e.Index % 2 == 0 ? LightUiTheme.Surface : Color.FromArgb(248, 250, 252)
            };
            using var bgBrush = new SolidBrush(bgColor);
            e.Graphics.FillRectangle(bgBrush, e.Bounds);

            // خط جانبي ملون
            using var sidePen = new SolidBrush(alert.Color);
            e.Graphics.FillRectangle(sidePen, e.Bounds.Left, e.Bounds.Top, 4, e.Bounds.Height);

            // نص التنبيه
            var titleRect = new Rectangle(e.Bounds.Left + 10, e.Bounds.Top + 4, e.Bounds.Width - 14, 22);
            var msgRect   = new Rectangle(e.Bounds.Left + 10, e.Bounds.Top + 26, e.Bounds.Width - 14, 18);
            using var titleBrush = new SolidBrush(alert.Color);
            using var msgBrush   = new SolidBrush(LightUiTheme.Text);
            e.Graphics.DrawString($"{alert.Severity} | {alert.Title}", new Font("Segoe UI", 9f, FontStyle.Bold), titleBrush, titleRect);
            e.Graphics.DrawString($"{alert.CreatedAt.ToLocalTime():HH:mm:ss} — {alert.Message}", new Font("Segoe UI", 8f), msgBrush, msgRect);
        }

        // ==========================================
        // مؤشرات لوحة التحكم
        // ==========================================

        private void UpdateMetrics()
        {
            var atms = GetFleetSnapshot();
            int connected = 0, disconnected = 0;
            long journalKB = 0;
            int criticalRisk = 0;

            foreach (var atm in atms)
            {
                if (atm.ConnectionStatus == ConnectionStatus.Connected) connected++;
                else disconnected++;
                journalKB += atm.JournalSizeToday / 1024;

                var prediction = _predictionEngine.Predict(atm);
                if (prediction.RiskScore >= 75) criticalRisk++;
            }

            SetMetric(0, connected.ToString());
            SetMetric(1, disconnected.ToString());
            SetMetric(2, $"{journalKB:N0} KB");
            SetMetric(3, AlertManager.Instance.ActiveCount.ToString());
            SetMetric(4, criticalRisk.ToString());
            SetMetric(5, DateTime.Now.ToString("HH:mm:ss"));

            RefreshFleetMatrix();
            UpdateNarrative(atms, connected, disconnected, criticalRisk);
        }

        private void SetMetric(int idx, string value)
        {
            if (idx < _metricValues.Length && _metricValues[idx] != null)
                _metricValues[idx].Text = value;
        }

        private List<ATMInfo> GetFleetSnapshot()
        {
            if (_serverService == null)
                return new List<ATMInfo>();

            var list = new List<ATMInfo>();
            foreach (var atm in _serverService.GetConnectedATMs() ?? Array.Empty<ATMInfo>())
            {
                if (atm != null)
                    list.Add(atm);
            }
            return list;
        }

        private void RefreshFleetMatrix()
        {
            if (_dgvFleetMatrix == null || _dgvFleetMatrix.IsDisposed) return;
            var atms = GetFleetSnapshot();

            _dgvFleetMatrix.Rows.Clear();
            var critical = 0;
            var warning = 0;

            foreach (var atm in atms.OrderBy(a => a.ATM_ID))
            {
                var prediction = _predictionEngine.Predict(atm);
                if (prediction.RiskScore >= 75) critical++;
                else if (prediction.RiskScore >= 40) warning++;

                var rowIndex = _dgvFleetMatrix.Rows.Add(
                    atm.ATM_ID,
                    atm.ATM_Type,
                    atm.GetStatusLabel(),
                    atm.GetElapsed(atm.LastHeartbeatUtc),
                    atm.GetElapsed(atm.LastSyncUtc),
                    $"{atm.PendingJournalCount}/{atm.ConsecutiveSyncFailures}",
                    $"{atm.HealthScore}%",
                    $"{prediction.Level} ({prediction.RiskScore}%)",
                    prediction.RecommendedAction
                );

                var row = _dgvFleetMatrix.Rows[rowIndex];
                row.Tag = atm;
                StyleFleetRow(row, prediction.RiskScore);

                if (!string.IsNullOrWhiteSpace(_selectedAtmId) &&
                    string.Equals(_selectedAtmId, atm.ATM_ID, StringComparison.OrdinalIgnoreCase))
                {
                    row.Selected = true;
                }
            }

            if (_lblFleetMatrixSummary != null)
                _lblFleetMatrixSummary.Text =
                    $"مصفوفة المراقبة: {atms.Count} صراف | حرج: {critical} | مراقبة: {warning} | مساحة الخادم: {(_serverService != null ? _serverService.FreeSpaceGB.ToString("F1") : "—")} GB";
        }

        private static void StyleFleetRow(DataGridViewRow row, int riskScore)
        {
            if (row == null) return;

            if (riskScore >= 75)
            {
                row.Cells[7].Style.ForeColor = Color.FromArgb(180, 35, 24);
                row.Cells[7].Style.Font = new Font("Segoe UI", 8.6f, FontStyle.Bold);
                row.DefaultCellStyle.BackColor = Color.FromArgb(255, 244, 244);
                return;
            }

            if (riskScore >= 40)
            {
                row.Cells[7].Style.ForeColor = Color.FromArgb(160, 90, 0);
                row.DefaultCellStyle.BackColor = Color.FromArgb(255, 250, 238);
                return;
            }

            row.Cells[7].Style.ForeColor = Color.FromArgb(20, 120, 60);
        }

        private void UpdateNarrative(List<ATMInfo> atms, int connected, int disconnected, int criticalRisk)
        {
            if (_lblFleetNarrative == null) return;

            var commands = _serverService?.GetRecentCommands() ?? Array.Empty<RemoteCommand>();
            int cmdExecuted = 0;
            int cmdFailed = 0;
            foreach (var cmd in commands)
            {
                if (string.Equals(cmd.Status, "Executed", StringComparison.OrdinalIgnoreCase)) cmdExecuted++;
                if (string.Equals(cmd.Status, "Failed", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(cmd.Status, "Timeout", StringComparison.OrdinalIgnoreCase)) cmdFailed++;
            }

            _lblFleetNarrative.Text =
                $"آخر تحديث: {DateTime.Now:HH:mm:ss} | إجمالي: {atms.Count} | متصل: {connected} | منقطع: {disconnected} | مخاطر حرجة: {criticalRisk} | أوامر منفذة: {cmdExecuted} | أوامر فاشلة: {cmdFailed}";
        }

        private void SetSelectedAtm(string atmId, bool announce)
        {
            if (string.IsNullOrWhiteSpace(atmId)) return;
            var changed = !string.Equals(_selectedAtmId, atmId, StringComparison.OrdinalIgnoreCase);
            _selectedAtmId = atmId;
            if (_lblOpsTarget != null)
                _lblOpsTarget.Text = $"الهدف: {atmId}";

            if (announce && changed)
                AddAlert(new AlertPayload { Severity = AlertSeverity.Info, Title = "تحديد الصراف", Message = $"تم اختيار الصراف {atmId} كهدف للأوامر", Source = "Selection" });
        }

        private void AttachSelectionHandlers(Control parent, string atmId)
        {
            if (parent == null) return;
            parent.Click += (s, e) => SetSelectedAtm(atmId, false);
            foreach (Control child in parent.Controls)
                AttachSelectionHandlers(child, atmId);
        }

        // ==========================================
        // التحديث الدوري
        // ==========================================

        private void InitializeTimers()
        {
            _refreshTimer = new Timer { Interval = 5000 };
            _refreshTimer.Tick += (s, e) =>
            {
                UpdateMetrics();
                _serverService?.CheckAllATMHealth();
            };
            _refreshTimer.Start();

            _clockTimer = new Timer { Interval = 1000 };
            _clockTimer.Tick += (s, e) =>
            {
                _lblFooterTime.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                SetMetric(5, DateTime.Now.ToString("HH:mm:ss"));
            };
            _clockTimer.Start();
        }

        private void OnRefresh(object s = null, EventArgs e = null)
        {
            UpdateMetrics();
            foreach (var card in _cardPanels.Values)
                card.Invalidate();
        }

        private void OnToggleFullscreen(object s = null, EventArgs e = null)
        {
            _isFullscreen = !_isFullscreen;
            if (_isFullscreen)
            {
                FormBorderStyle = FormBorderStyle.None;
                WindowState     = FormWindowState.Maximized;
                _headerPanel.Visible    = true;
                _toolbarPanel.Visible   = false;
                _btnFullscreen.Text     = "نافذة";
            }
            else
            {
                FormBorderStyle = FormBorderStyle.Sizable;
                WindowState     = FormWindowState.Maximized;
                _toolbarPanel.Visible   = true;
                _btnFullscreen.Text     = "ملء الشاشة";
            }
        }

        // ==========================================
        // مساعدات UI
        // ==========================================

        private Button MakeButton(string text, Color backColor, Action action, int width = 130)
        {
            var btn = new Button
            {
                Text      = text,
                Width     = width,
                Height    = 36,
                MinimumSize = new Size(width, 36),
                BackColor = backColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", 9f),
                Margin    = new Padding(4, 1, 4, 1),
                Cursor    = Cursors.Hand,
                AutoEllipsis = true,
                TextAlign = ContentAlignment.MiddleCenter,
                UseCompatibleTextRendering = true
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.Click += (s, e) => action?.Invoke();
            return btn;
        }

        private Label MakeLabel(string text) => new Label
        {
            Text = text, AutoSize = true, ForeColor = LightUiTheme.Text,
            TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(0, 9, 0, 0),
            MinimumSize = new Size(0, 34)
        };

        private void ApplyQuickCommandTemplate()
        {
            if (_cmbQuickCommand == null || _txtQuickParam == null) return;

            switch (_cmbQuickCommand.SelectedIndex)
            {
                case 0:
                    _txtQuickParam.Text = $"UTC={DateTime.UtcNow:O}";
                    break;
                case 1:
                    _txtQuickParam.Text = $"SOURCE={AppConstants.ShareImagesAllPath};TARGET={AppConstants.DefaultImagesPath};OVERWRITE=true";
                    break;
                case 2:
                    _txtQuickParam.Text = AppConstants.ShareImagesAllPath;
                    break;
                case 3:
                    _txtQuickParam.Text = "NewStrongPassword";
                    break;
                case 4:
                    _txtQuickParam.Text = "";
                    break;
                case 5:
                    _txtQuickParam.Text = "";
                    break;
            }
        }

        private void OnSendQuickCommand()
        {
            if (_serverService == null || !_serverService.IsRunning)
            {
                AddAlert(new AlertPayload { Severity = AlertSeverity.Warning, Title = "أمر مرفوض", Message = "شغل الخادم قبل إرسال أوامر التحكم.", Source = "NOC" });
                return;
            }

            var atmId = ResolveSelectedAtmId();
            if (string.IsNullOrWhiteSpace(atmId))
            {
                AddAlert(new AlertPayload { Severity = AlertSeverity.Warning, Title = "لا يوجد هدف", Message = "اختر صرافاً من البطاقة أو الجدول قبل تنفيذ الأمر.", Source = "NOC" });
                return;
            }

            var command = ResolveQuickCommand(out var requiresConfirm);
            var parameter = _txtQuickParam?.Text ?? "";

            if (string.IsNullOrWhiteSpace(command))
                return;

            if (requiresConfirm &&
                MessageBox.Show($"سيتم تنفيذ أمر حساس على الصراف {atmId}. هل تريد المتابعة؟",
                    "تأكيد أمر تشغيلي", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                return;

            var result = _serverService.SendCommandDetailed(atmId, command, parameter, "NOC");
            SetSelectedAtm(atmId, false);
            UpsertRemoteTask(result, parameter);
            AddTaskEvent($"{command} -> {atmId} | {ToArabicCommandStatus(result.Status)} | {ShortCommandId(result.CommandId)} | param={TrimForLog(parameter, 90)}");
            AddAlert(new AlertPayload
            {
                Severity = AlertSeverity.Info,
                Title = "أمر أُرسل",
                Message = $"{command} -> {atmId} [{ShortCommandId(result.CommandId)}] {ToArabicCommandStatus(result.Status)}",
                Source = "RemoteTask"
            });
            UpdateMetrics();
        }

        private string ResolveSelectedAtmId()
        {
            if (!string.IsNullOrWhiteSpace(_selectedAtmId))
                return _selectedAtmId;

            foreach (var atm in GetFleetSnapshot())
            {
                if (!string.IsNullOrWhiteSpace(atm.ATM_ID))
                {
                    SetSelectedAtm(atm.ATM_ID, false);
                    return atm.ATM_ID;
                }
            }
            return null;
        }

        private string ResolveQuickCommand(out bool requiresConfirm)
        {
            requiresConfirm = false;
            switch (_cmbQuickCommand?.SelectedIndex ?? -1)
            {
                case 0:
                    return AppConstants.CMD_SYNC_TIME;
                case 1:
                    return AppConstants.CMD_SYNC_FOLDER;
                case 2:
                    return AppConstants.CMD_SYNC_IMAGES;
                case 3:
                    requiresConfirm = true;
                    return AppConstants.CMD_CHANGE_PASSWORD;
                case 4:
                    return AppConstants.CMD_FORCE_SYNC;
                case 5:
                    return AppConstants.CMD_GET_STATS;
                default:
                    AddAlert(new AlertPayload { Severity = AlertSeverity.Warning, Title = "أمر غير محدد", Message = "اختر نوع أمر صحيح.", Source = "NOC" });
                    return null;
  

        // Missing event handlers from Designer
        private void btnRefresh_Click(object sender, EventArgs e)
        {
            OnRefresh();
            tsslStatus.Text = $"Refreshed at {DateTime.Now:HH:mm:ss}";
        }

        private void btnRestartATM_Click(object sender, EventArgs e) => SendFirstAtmCommand(AppConstants.CMD_RESTART, "restart");
        private void btnScreenshot_Click(object sender, EventArgs e) => SendFirstAtmCommand(AppConstants.CMD_SCREENSHOT, "screenshot");
        private void btnSyncTime_Click(object sender, EventArgs e) => SendFirstAtmCommand(AppConstants.CMD_SYNC_TIME, "time sync");
        private void btnArchive_Click(object sender, EventArgs e)
        {
            var files = Directory.Exists(AppConstants.DefaultArchivePath)
                ? Directory.GetFiles(AppConstants.DefaultArchivePath, "*", SearchOption.AllDirectories).Length
                : 0;
            tsslStatus.Text = $"Archive indexed: {files:N0} files";
        }

        private void btnChangePassword_Click(object sender, EventArgs e)
        {
            tsslStatus.Text = "Use Server > Remote Control for audited password changes.";
        }

        private void btnExportReport_Click(object sender, EventArgs e)
        {
            var report = new ReportExportEngine().ExportDailyNocReport(new List<ATMInfo>(_serverService?.GetConnectedATMs() ?? Array.Empty<ATMInfo>()), DateTime.Today);
            System.Diagnostics.Process.Start(report);
            tsslStatus.Text = $"Report exported: {Path.GetFileName(report)}";
        }

        private void btnSettings_Click(object sender, EventArgs e)
        {
            tabControl.SelectedTab = tabSettings;
            tsslStatus.Text = "Settings tab opened.";
        }

        private void SendFirstAtmCommand(string command, string label)
        {
            if (_serverService == null || !_serverService.IsRunning)
            {
                tsslStatus.Text = "Start or connect the server before sending commands.";
                return;
            }

            string atmId = null;
            foreach (var atm in _serverService.GetConnectedATMs())
            {
                atmId = atm.ATM_ID;
                break;
            }

            if (string.IsNullOrWhiteSpace(atmId))
            {
                tsslStatus.Text = $"No connected ATM for {label}.";
                return;
            }

            var result = _serverService.SendCommandDetailed(atmId, command, "", "NOC");
            tsslStatus.Text = $"{label} -> {atmId}: {ToArabicCommandStatus(result.Status)} [{ShortCommandId(result.CommandId)}]";
        }

        private void AddCommandLifecycleAlert(RemoteCommand command)
        {
            if (command == null) return;
            var severity = command.Status == "Failed" || command.Status == "Timeout"
                ? AlertSeverity.Warning
                : command.Status == "Executed" ? AlertSeverity.Info : AlertSeverity.Info;

            UpsertRemoteTask(command, command.Parameters?.ToString());
            AddTaskEvent($"{command.CommandType} -> {command.TargetATMId} | {ToArabicCommandStatus(command.Status)} | {ShortCommandId(command.CommandId)} | {TrimForLog(command.Result, 120)}");

            AddAlert(new AlertPayload
            {
                Severity = severity,
                Title = $"أمر {ToArabicCommandStatus(command.Status)}",
                Message = $"{command.CommandType} -> {command.TargetATMId} [{ShortCommandId(command.CommandId)}] {command.Result}",
                Source = "Command"
            });
        }

        private static string ShortCommandId(string commandId)
        {
            if (string.IsNullOrWhiteSpace(commandId)) return "—";
            return commandId.Length <= 8 ? commandId : commandId.Substring(0, 8);
        }

        private static string TrimForLog(string value, int max)
        {
            if (string.IsNullOrWhiteSpace(value)) return "—";
            value = value.Replace("\r", " ").Replace("\n", " ");
            return value.Length <= max ? value : value.Substring(0, max) + "...";
        }

        private static void UpsertRemoteTask(RemoteCommand command, string parameters)
        {
            if (command == null) return;
            OperationalStateStore.Instance.UpsertRemoteTask(new RemoteTaskRecord
            {
                TaskId = string.IsNullOrWhiteSpace(command.CommandId) ? Guid.NewGuid().ToString("N") : command.CommandId,
                TerminalId = command.TargetATMId,
                CommandType = command.CommandType,
                Parameters = parameters,
                AssignedBy = string.IsNullOrWhiteSpace(command.SentBy) ? "NOC" : command.SentBy,
                State = ToRemoteTaskState(command.Status),
                Result = command.Result,
                CreatedAtUtc = command.SentAtUtc == default(DateTime) ? DateTime.UtcNow : command.SentAtUtc,
                UpdatedAtUtc = DateTime.UtcNow
            });
        }

        private static RemoteTaskState ToRemoteTaskState(string status)
        {
            switch (status)
            {
                case "Created": return RemoteTaskState.Created;
                case "Sent": return RemoteTaskState.Sent;
                case "Received": return RemoteTaskState.Received;
                case "Executed": return RemoteTaskState.Executed;
                case "Failed": return RemoteTaskState.Failed;
                case "Timeout": return RemoteTaskState.Timeout;
                default: return RemoteTaskState.Assigned;
            }
        }

        private static string ToArabicCommandStatus(string status)
        {
            switch (status)
            {
                case "Created":  return "قيد الإنشاء";
                case "Sent":     return "أُرسل";
                case "Received": return "استُلم";
                case "Executed": return "نُفذ";
                case "Failed":   return "فشل";
                case "Timeout":  return "مهلة";
                default:         return string.IsNullOrWhiteSpace(status) ? "—" : status;
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _refreshTimer?.Stop();
            _clockTimer?.Stop();
            _serverService?.Dispose();
            base.OnFormClosing(e);
        private DashboardSnapshot _snapshot;
        private System.Windows.Forms.Timer _refreshTimer;
        private MonitoringStateStore _monitoringStore;
        private string _storagePath;

        public MainDashboardForm()
        {
            InitializeComponent();
            Text = "EJLive Central Monitoring Dashboard v" + Constants.AppVersion;
            ConfigureGridViews();
            ResolveMonitoringDataSource();
            ConfigureTimer();
            LoadDashboardSnapshot();
            BindSnapshotToUi();
        }

        private void ConfigureGridViews()
        {
            dgvTerminals.AutoGenerateColumns = false;
            dgvCash.AutoGenerateColumns = false;
            dgvAlerts.AutoGenerateColumns = false;
        }

        private void ResolveMonitoringDataSource()
        {
            _storagePath = MonitoringStateStore.ResolveStoragePathFromConfig(AppDomain.CurrentDomain.BaseDirectory);
            _monitoringStore = new MonitoringStateStore(_storagePath);
            _monitoringStore.EnsureInitialized();
        }

        private void ConfigureTimer()
        {
            _refreshTimer = new System.Windows.Forms.Timer();
            _refreshTimer.Interval = 10000;
            _refreshTimer.Tick += (s, e) =>
            {
                LoadDashboardSnapshot();
                BindSnapshotToUi();
                tsslStatus.Text = "Dashboard auto refreshed at " + DateTime.Now.ToString("HH:mm:ss");
            };
            _refreshTimer.Start();
        }

        private void LoadDashboardSnapshot()
        {
            MonitoringSystemState serverState = _monitoringStore.Load();

            var terminals = serverState.Terminals.Select(t => new TerminalSnapshot
            {
                TerminalId = t.TerminalId,
                BranchName = string.IsNullOrWhiteSpace(t.BranchName) ? "Unassigned" : t.BranchName,
                Region = string.IsNullOrWhiteSpace(t.Region) ? "Unknown" : t.Region,
                Vendor = string.IsNullOrWhiteSpace(t.Vendor) ? "Unknown" : t.Vendor,
                Network = string.IsNullOrWhiteSpace(t.Network) ? "Unknown" : t.Network,
                Health = MapHealth(t.Health),
                LastHeartbeat = t.LastHeartbeatUtc == DateTime.MinValue ? DateTime.MinValue : t.LastHeartbeatUtc.ToLocalTime(),
                LastEjSync = t.LastEjSyncUtc == DateTime.MinValue ? DateTime.MinValue : t.LastEjSyncUtc.ToLocalTime(),
                ActiveAlerts = t.ActiveAlerts,
                SupervisorMode = t.SupervisorMode,
                LastTransaction = string.IsNullOrWhiteSpace(t.LastTransaction) ? t.Status : t.LastTransaction,
                Cash = new CashStatusSummary
                {
                    Cass1 = t.Cash != null ? t.Cash.Cass1 : 0,
                    Cass2 = t.Cash != null ? t.Cash.Cass2 : 0,
                    Cass3 = t.Cash != null ? t.Cash.Cass3 : 0,
                    Cass4 = t.Cash != null ? t.Cash.Cass4 : 0,
                    Remaining = t.Cash != null ? t.Cash.Remaining : 0,
                    Loaded = t.Cash != null ? t.Cash.Loaded : 0,
                    DepositIn = t.Cash != null ? t.Cash.DepositIn : 0,
                    DispenseOut = t.Cash != null ? t.Cash.DispenseOut : 0,
                    Reject = t.Cash != null ? t.Cash.Reject : 0,
                    Retract = t.Cash != null ? t.Cash.Retract : 0
                }
            }).ToList();

            var alerts = serverState.Alerts.Select(a => new AlertEntry
            {
                RaisedAt = a.RaisedAtUtc.ToLocalTime(),
                TerminalId = a.TerminalId,
                Severity = a.Severity,
                Message = a.Message
            }).ToList();

            _snapshot = new DashboardSnapshot(terminals, alerts);
        }

        private void BindSnapshotToUi()
        {
            lblTotalATMs.Text = _snapshot.TotalTerminals.ToString();
            lblOnlineATMs.Text = _snapshot.OnlineTerminals.ToString();
            lblEJSynced.Text = _snapshot.EjSyncedTerminals.ToString();
            lblFaults.Text = _snapshot.CriticalTerminals.ToString();
            lblWarnings.Text = _snapshot.WarningTerminals.ToString();
            lblCitRequired.Text = _snapshot.CitRequired.ToString();
            lblSupervisorCount.Text = _snapshot.SupervisorModeCount.ToString();
            lblRemainingCash.Text = _snapshot.TotalRemainingCash.ToString("N0");
            lblLastUpdate.Text = "Last Update: " + DateTime.Now.ToString("HH:mm:ss");

            dgvTerminals.Rows.Clear();
            foreach (var terminal in _snapshot.Terminals.OrderBy(x => x.Region).ThenBy(x => x.TerminalId))
            {
                int rowIndex = dgvTerminals.Rows.Add(
                    terminal.TerminalId,
                    terminal.BranchName,
                    terminal.Region,
                    terminal.Vendor,
                    terminal.Network,
                    terminal.Health.ToString(),
                    FormatTimestamp(terminal.LastHeartbeat),
                    FormatTimestamp(terminal.LastEjSync),
                    terminal.ActiveAlerts,
                    terminal.LastTransaction);

                dgvTerminals.Rows[rowIndex].DefaultCellStyle.BackColor = GetHealthColor(terminal.Health);
            }

            dgvCash.Rows.Clear();
            foreach (var terminal in _snapshot.Terminals.OrderBy(x => x.TerminalId))
            {
                var cash = terminal.Cash ?? new CashStatusSummary();
                dgvCash.Rows.Add(
                    terminal.TerminalId,
                    cash.Cass1,
                    cash.Cass2,
                    cash.Cass3,
                    cash.Cass4,
                    cash.Remaining,
                    cash.Loaded,
                    cash.DepositIn,
                    cash.DispenseOut,
                    cash.Reject,
                    cash.Retract);
            }

            dgvAlerts.Rows.Clear();
            foreach (var alert in _snapshot.Alerts.OrderByDescending(x => x.RaisedAt))
            {
                int rowIndex = dgvAlerts.Rows.Add(
                    alert.RaisedAt.ToString("HH:mm:ss"),
                    alert.TerminalId,
                    alert.Severity,
                    alert.Message);

                if (string.Equals(alert.Severity, "Critical", StringComparison.OrdinalIgnoreCase))
                    dgvAlerts.Rows[rowIndex].DefaultCellStyle.BackColor = Color.FromArgb(255, 220, 220);
                else if (string.Equals(alert.Severity, "Warning", StringComparison.OrdinalIgnoreCase))
                    dgvAlerts.Rows[rowIndex].DefaultCellStyle.BackColor = Color.FromArgb(255, 244, 214);
            }

            tsslStatus.Text = string.Format(
                "Monitoring source: {0} | {1} terminals | {2} alerts",
                _storagePath,
                _snapshot.TotalTerminals,
                _snapshot.TotalAlerts);
        }

        private static string FormatTimestamp(DateTime value)
        {
            return value == DateTime.MinValue ? "--" : value.ToString("HH:mm:ss");
        }

        private static TerminalHealth MapHealth(string health)
        {
            if (string.Equals(health, "Online", StringComparison.OrdinalIgnoreCase))
                return TerminalHealth.Online;
            if (string.Equals(health, "Warning", StringComparison.OrdinalIgnoreCase))
                return TerminalHealth.Warning;
            if (string.Equals(health, "Critical", StringComparison.OrdinalIgnoreCase))
                return TerminalHealth.Critical;
            return TerminalHealth.Offline;
        }

        private static Color GetHealthColor(TerminalHealth health)
        {
            switch (health)
            {
                case TerminalHealth.Online:
                    return Color.FromArgb(220, 255, 220);
                case TerminalHealth.Warning:
                    return Color.FromArgb(255, 247, 220);
                case TerminalHealth.Critical:
                    return Color.FromArgb(255, 224, 224);
                default:
                    return Color.FromArgb(235, 235, 235);
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadDashboardSnapshot();
            BindSnapshotToUi();
            tsslStatus.Text = "Dashboard refreshed manually at " + DateTime.Now.ToString("HH:mm:ss");
        }

        private void btnExportReport_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "CSV Files|*.csv|Text Files|*.txt";
                sfd.Title = "Export Monitoring Report";
                sfd.FileName = "EJLive_Monitoring_" + DateTime.Now.ToString("yyyyMMdd_HHmmss");
                if (sfd.ShowDialog() != DialogResult.OK)
                    return;

                try
                {
                    using (var sw = new StreamWriter(sfd.FileName))
                    {
                        sw.WriteLine("TerminalId,Branch,Region,Vendor,Network,Health,LastHeartbeat,LastEjSync,Alerts,RemainingCash,SupervisorMode,LastTransaction");
                        foreach (var terminal in _snapshot.Terminals)
                        {
                            sw.WriteLine(string.Join(",",
                                terminal.TerminalId,
                                EscapeCsv(terminal.BranchName),
                                terminal.Region,
                                terminal.Vendor,
                                terminal.Network,
                                terminal.Health,
                                FormatTimestamp(terminal.LastHeartbeat),
                                FormatTimestamp(terminal.LastEjSync),
                                terminal.ActiveAlerts,
                                terminal.Cash != null ? terminal.Cash.Remaining.ToString() : "0",
                                terminal.SupervisorMode ? "Yes" : "No",
                                EscapeCsv(terminal.LastTransaction)));
                        }
                    }

                    MessageBox.Show("Monitoring report exported successfully.", "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Export failed: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private static string EscapeCsv(string value)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;
            if (value.Contains(",") || value.Contains("\"") || value.Contains("\n"))
                return "\"" + value.Replace("\"", "\"\"") + "\"";
            return value;
        }
    }
}

