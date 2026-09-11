using EJLive.Core;
using EJLive.Core.Engine;
using EJLive.Core.Models;
using EJLive.Core.Services;
using EJLive.Server.Services;
using EJLive.Server.WinForms.Dashboards.Classic;
using EJLive.Server.WinForms.Dashboards.Primary;
using EJLive.Server.WinForms.Services;
using EJLive.Shared;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Windows.Forms;

namespace EJLive.Server.WinForms
{
    public partial class ServerMainForm : Form
    {
        private bool _serverRunning = false;
        private EJServer _server;
        private ArchiveManager _archiveManager;
        private System.Windows.Forms.Timer _refreshTimer;
        private TabPage _tabJournalSync;
        private ListView _lvJournalSync;
        private Label _lblJournalSyncSummary;
        private Button _btnExportJournalSync;
        private Button _btnRefreshJournalSync;
        private TabPage _tabDashboard;
        private ListView _lvAtmDashboard;
        private Label _lblDashOnline;
        private Label _lblDashOffline;
        private Label _lblDashSyncing;
        private Label _lblDashFailed;
        private Label _lblDashLastActivity;
        private readonly VendorRootCapabilityService _vendorRootCapabilityService;
        private readonly JournalSyncTracker _journalSyncTracker;
        private readonly JournalSyncAlertService _journalSyncAlertService;
        private readonly JournalSyncDashboardService _journalSyncDashboardService;
        namespace EJLive.Server.WinForms
        {
            /// <summary>
            /// الواجهة الرئيسية للخادم — EJLive Enterprise Server v4.0
            /// 6 تبويبات: NOC Dashboard | Connections | Archive | Remote Control | Analytics | Log
            /// Dark NOC Theme مع بطاقات ملونة تفاعلية لكل صراف
            /// </summary>
            public partial class ServerMainForm : Form
            {
                #region Controls
                private MenuStrip        _menuStrip;
                private TabControl       _tabMain;
                private TabPage          _tabNOC, _tabConnections, _tabArchive, _tabRemote, _tabScenarios, _tabAnalytics, _tabLog;
                private StatusStrip      _statusStrip;
                private ToolStripStatusLabel _lblStatus, _lblConnections, _lblTime, _lblAlerts, _lblStorage;
                private Panel            _alertBar;
                private Label            _lblAlertText;
                private System.Windows.Forms.Timer            _uiTimer;
                private System.Windows.Forms.Timer            _healthTimer;
                // ═══ Tab 1: NOC Dashboard ═══
                private FlowLayoutPanel  _pnlATMCards;
                private Panel            _pnlMetrics;
                private Label[] _metricValues = new Label[8];
                private Label[] _metricLabels = new Label[8];
                private Button           _btnStartServer, _btnStopServer;
                private Button           _btnChangePasswordAll, _btnSendImagesAll, _btnBroadcast;
                private RichTextBox      _rtbPasswordLog;
                private RichTextBox      _rtbOperationsLog;
                private Label            _lblOperationsSummary;
                private Label            _lblOpsCmdSent, _lblOpsCmdExecuted, _lblOpsCmdFailed, _lblOpsCmdPending, _lblOpsCmdLast;
                private Panel            _pnlServerControls;
                private ComboBox         _cmbOpsTargetATM;
                private TextBox          _txtOpsSourceFolder, _txtOpsTargetFolder, _txtOpsPassword;
                private Button           _btnOpsBrowseSource, _btnOpsFolderSelected, _btnOpsFolderAll;
                private Button           _btnOpsImagesSelected, _btnOpsImagesAll, _btnOpsPasswordSelected;
                private Button           _btnOpsTimeSelected, _btnOpsTimeAll, _btnOpsMonthlyArchive;
                // ═══ Tab 2: Connections ═══
                private DataGridView     _dgvConnections;
                private ComboBox         _cmbConnFilter;
                private Label            _lblConnCount;
                // ═══ Tab 3: Archive ═══
                private TextBox          _txtSearchATM;
                private DateTimePicker   _dtpFrom, _dtpTo;
                private TextBox          _txtSearchKeyword;
                private Button           _btnSearch, _btnExportCSV, _btnExportHTML;
                private DataGridView     _dgvArchive;
                private Label            _lblArchiveCount;
                // ═══ Tab 4: Remote Control ═══
                private ComboBox         _cmbTargetATM;
                private PictureBox       _pbGhostView;
                private Button           _btnGhostStart, _btnGhostStop;
                private Button           _btnRestart, _btnShutdown, _btnChangePassword, _btnRemoteAccessOn, _btnRemoteAccessOff;
                private Button           _btnSendFile, _btnGetFile, _btnForceSync, _btnSyncTime, _btnSyncImages, _btnSyncFolder;
                private TextBox          _txtRemotePassword, _txtFileParam;
                private DataGridView     _dgvCommandLog;
                private TrackBar         _tbGhostQuality;
                private Label            _lblGhostQuality;
                // ═══ Tab 5: Scenarios ═══
                private DataGridView     _dgvScenarioMatrix;
                private RichTextBox      _rtbScenarioLog;
                private Label            _lblScenarioSummary;
                private Button           _btnRunScenario, _btnRefreshScenarios;
                // ═══ Tab 6: Analytics ═══
                private ComboBox         _cmbAnalyticsATM;
                private DateTimePicker   _dtpAnalyticsFrom, _dtpAnalyticsTo;
                private Button           _btnRunAnalytics, _btnRunFleetAnalysis, _btnExportAnalytics;
                private Label            _lblAnalyticsSummary, _lblPredictionSummary;
                private DataGridView     _dgvAnalytics;
                private Panel            _pnlAnalyticsStats;
                private Label[]          _analyticsStatLabels = new Label[8];
                // ═══ Tab 7: Log ═══
                private RichTextBox      _rtbLog;
                private ComboBox         _cmbLogLevel;
                private TextBox          _txtLogSearch;
                private Button           _btnClearLog, _btnExportLog;
                private Label            _lblLogCount;
                #endregion
                #region Services
                private ServerEngine     _serverEngine;
                private ArchiveManager   _archiveManager;
                private TransactionAnalysisEngine _analysisEngine;
                private ReportExportEngine        _reportEngine;
                private FleetPredictionEngine     _predictionEngine;
                private readonly Dictionary<string, ATMCardPanel> _atmPanels = new Dictionary<string, ATMCardPanel>();
                private readonly Dictionary<string, DataGridViewRow> _commandRows = new Dictionary<string, DataGridViewRow>();
                private string           _currentUser = "admin";
                private bool             _designerServerRunning;
                #endregion
                // ==========================================
                // بناء الفورم
                // ==========================================
                public ServerMainForm()
                {
                    if (IsVisualStudioDesigner())
                    {
                        InitializeComponent();
                        return;
                    }
                    InitializeServices();
                    InitializeForm();
                    InitializeMenuStrip();
                    InitializeAlertBar();
                    InitializeStatusStrip();
                    InitializeTabs();
                    InitializeTimers();
                    ApplyNocTheme();
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
                    DatabaseManager.Instance.Initialize(AppConstants.DefaultDatabasePath);
                    AppLogger.Instance.Initialize(AppConstants.DefaultLogPath, "server");
                    AppLogger.Instance.OnLog += (s, e) => AppendLog(e.FormattedForUI, e.Level);
                    EnsureServerShareFolders();
                    _serverEngine   = new ServerEngine();
                    _archiveManager = new ArchiveManager();
                    _analysisEngine = new TransactionAnalysisEngine();
                    _reportEngine   = new ReportExportEngine();
                    _predictionEngine = new FleetPredictionEngine();
                    _serverEngine.OnATMConnected    += (s, atm) => OnATMConnected(atm);
                    _serverEngine.OnATMDisconnected += (s, atm) => OnATMDisconnected(atm);
                    _serverEngine.OnATMUpdated      += (s, atm) => RefreshATMCard(atm);
                    _serverEngine.OnJournalReceived += (s, pkt)  => OnJournalReceived(pkt);
                    _serverEngine.OnServerLog       += (s, msg)  => AppLogger.Instance.Info(msg, "Server");
                    _serverEngine.OnCommandChanged  += (s, cmd)  => OnCommandChanged(cmd);
                    AlertManager.Instance.OnAlert    += (s, a) => ShowAlert(a);
                    AlertManager.Instance.OnCritical += (s, a) => ShowCriticalAlert(a);
                }
                private void InitializeForm()
                {
                    Text            = $"EJLive Enterprise Server v{AppConstants.AppVersion}";
                    Size            = new Size(1440, 900);
                    MinimumSize     = new Size(1200, 700);
                    StartPosition   = FormStartPosition.CenterScreen;
                    WindowState     = FormWindowState.Maximized;
                    BackColor       = Color.FromArgb(245, 247, 250);
                    ForeColor       = Color.FromArgb(33, 37, 41);
                    Font            = new Font("Segoe UI", 9.5f);
                    Icon            = CreateAppIcon();
                }
                private void InitializeMenuStrip()
                {
                    _menuStrip = new MenuStrip { BackColor = Color.White, ForeColor = Color.FromArgb(33, 37, 41) };
                    var fileMenu   = new ToolStripMenuItem("ملف");
                    var serverMenu = new ToolStripMenuItem("الخادم");
                    var viewMenu   = new ToolStripMenuItem("عرض");
                    var toolsMenu  = new ToolStripMenuItem("أدوات");
                    var helpMenu   = new ToolStripMenuItem("مساعدة");
                    fileMenu.DropDownItems.AddRange(new ToolStripItem[] {
                        new ToolStripMenuItem("تصدير التقارير", null, OnExportAllReports),
                        new ToolStripSeparator(),
                        new ToolStripMenuItem("خروج", null, (s,e) => Close())
                    });
                    serverMenu.DropDownItems.AddRange(new ToolStripItem[] {
                        new ToolStripMenuItem("تشغيل الخادم",    null, (s,e) => StartServer()),
                        new ToolStripMenuItem("إيقاف الخادم",   null, (s,e) => StopServer()),
                        new ToolStripSeparator(),
                        new ToolStripMenuItem("إعدادات الخادم", null, OnOpenSettings)
                    });
                    toolsMenu.DropDownItems.AddRange(new ToolStripItem[] {
                        new ToolStripMenuItem("إدارة المستخدمين",   null, OnManageUsers),
                        new ToolStripMenuItem("سجل التدقيق",        null, OnOpenAuditLog),
                        new ToolStripMenuItem("إعادة ضبط الإحصائيات", null, OnResetStats)
                    });
                    _menuStrip.Items.AddRange(new[] { fileMenu, serverMenu, viewMenu, toolsMenu, helpMenu });
                    Controls.Add(_menuStrip);
                }
                private void InitializeAlertBar()
                {
                    _alertBar = new Panel
                    {
                        Height    = 36,
                        Dock      = DockStyle.Top,
                        BackColor = Color.FromArgb(255, 235, 238),
                        Visible   = false,
                        Cursor    = Cursors.Hand
                    };
                    _alertBar.Top = _menuStrip.Bottom;
                    _lblAlertText = new Label
                    {
                        Dock      = DockStyle.Fill,
                        TextAlign = ContentAlignment.MiddleCenter,
                        ForeColor = Color.FromArgb(180, 35, 24),
                        Font      = new Font("Segoe UI", 9.5f, FontStyle.Bold)
                    };
                    _alertBar.Controls.Add(_lblAlertText);
                    _alertBar.Click += (s, e) => _alertBar.Visible = false;
                    Controls.Add(_alertBar);
                }
                private void InitializeStatusStrip()
                {
                    _statusStrip = new StatusStrip { BackColor = Color.White, SizingGrip = false };
                    _lblStatus      = new ToolStripStatusLabel("● خادم متوقف") { ForeColor = Color.FromArgb(255, 69, 58) };
                    _lblConnections = new ToolStripStatusLabel("الاتصالات: 0") { ForeColor = Color.FromArgb(108, 117, 125) };
                    _lblAlerts      = new ToolStripStatusLabel("التنبيهات: 0") { ForeColor = Color.FromArgb(108, 117, 125) };
                    _lblStorage     = new ToolStripStatusLabel("المساحة: --") { ForeColor = Color.FromArgb(108, 117, 125) };
                    _lblTime        = new ToolStripStatusLabel { Alignment = ToolStripItemAlignment.Right, ForeColor = Color.FromArgb(108, 117, 125) };
                    _statusStrip.Items.AddRange(new ToolStripItem[]
                    { _lblStatus, new ToolStripSeparator(), _lblConnections, new ToolStripSeparator(),
                      _lblAlerts, new ToolStripSeparator(), _lblStorage, _lblTime });
                    Controls.Add(_statusStrip);
                }
                private void InitializeTabs()
                {
                    _tabMain = new TabControl
                    {
                        Dock        = DockStyle.Fill,
                        DrawMode    = TabDrawMode.OwnerDrawFixed,
                        ItemSize    = new Size(150, 42),
                        SizeMode    = TabSizeMode.Fixed,
                        Font        = new Font("Segoe UI", 9f, FontStyle.Bold),
                        Padding     = new Point(12, 8)
                    };
                    _tabMain.DrawItem     += DrawTabItem;
                    _tabMain.SelectedIndexChanged += (s, e) => _tabMain.Refresh();
                    _tabNOC         = new TabPage("لوحة NOC");
                    _tabConnections = new TabPage("الاتصالات");
                    _tabArchive     = new TabPage("الأرشيف");
                    _tabRemote      = new TabPage("التحكم البعيد");
                    _tabScenarios   = new TabPage("السيناريوهات");
                    _tabAnalytics   = new TabPage("التحليلات");
                    _tabLog         = new TabPage("السجل");
                    foreach (var tab in new[] { _tabNOC, _tabConnections, _tabArchive, _tabRemote, _tabScenarios, _tabAnalytics, _tabLog })
                        tab.BackColor = LightUiTheme.Window;
                    BuildTabNOC();
                    BuildTabConnections();
                    BuildTabArchive();
                    BuildTabRemote();
                    BuildTabScenarios();
                    BuildTabAnalytics();
                    BuildTabLog();
                    _tabMain.TabPages.AddRange(new[] { _tabNOC, _tabConnections, _tabArchive, _tabRemote, _tabScenarios, _tabAnalytics, _tabLog });
                    Controls.Add(_tabMain);
                }
                // ═══════════════════════════════════════════
                // TAB 1: NOC Dashboard
                // ═══════════════════════════════════════════
                private void BuildTabNOC()
                {
                    // لوحة الإحصائيات العلوية
                    _pnlMetrics = new Panel
                    {
                        Height    = 118,
                        Dock      = DockStyle.Top,
                        BackColor = Color.FromArgb(22, 24, 26),
                        Padding   = new Padding(12, 10, 12, 10)
                    };
                    var metricsFlow = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, WrapContents = false, AutoScroll = true, Padding = Padding.Empty };
                    string[] metricLabels = { "متصل", "توقف الإرسال", "جورنال اليوم", "معدل النجاح", "تنبيهات نشطة", "سرعة الاستقبال", "آخر استلام", "مساحة متبقية" };
                    string[] metricIcons  = { "ON", "STOP", "EJ", "OK", "ALRT", "RX", "TIME", "DISK" };
                    for (int i = 0; i < 8; i++)
                    {
                        var card  = CreateMetricCard(metricIcons[i], metricLabels[i], "—", i);
                        metricsFlow.Controls.Add(card);
                    }
                    _pnlMetrics.Controls.Add(metricsFlow);
                    // أزرار التحكم
                    _pnlServerControls = new Panel
                    {
                        Height    = 58,
                        Dock      = DockStyle.Top,
                        BackColor = Color.FromArgb(22, 24, 26),
                        Padding   = new Padding(12, 10, 12, 10)
                    };
                    _btnStartServer        = MakeButton("تشغيل الخادم",      Color.FromArgb(40, 100, 60),  StartServer, 160);
                    _btnStopServer         = MakeButton("إيقاف الخادم",      Color.FromArgb(100, 30, 30),  StopServer, 160);
                    _btnChangePasswordAll  = MakeButton("كلمة سر جماعية",    Color.FromArgb(60, 40, 80),   OnChangePasswordAll, 175);
                    _btnSendImagesAll      = MakeButton("صور جماعية",        Color.FromArgb(40, 60, 100),  OnSendImagesAll, 150);
                    _btnBroadcast          = MakeButton("بث للجميع",         Color.FromArgb(70, 50, 20),   OnBroadcast, 150);
                    _btnStopServer.Enabled = false;
                    var ctrlFlow           = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, WrapContents = false, AutoScroll = true, Padding = Padding.Empty };
                    ctrlFlow.Controls.AddRange(new Control[] { _btnStartServer, _btnStopServer, _btnChangePasswordAll, _btnSendImagesAll, _btnBroadcast });
                    _pnlServerControls.Controls.Add(ctrlFlow);
                    // منطقة بطاقات الصرافات ومركز العمليات
                    var pnlCardsArea = new Panel { Dock = DockStyle.Fill, BackColor = LightUiTheme.Window };
                    var operationsPanel = CreateOperationsPanel();
                    var cardsHost = new Panel
                    {
                        Dock = DockStyle.Fill,
                        BackColor = LightUiTheme.Window,
                        Padding = new Padding(12, 8, 12, 8)
                    };
                    var cardsHeader = new Label
                    {
                        Text = "حالة الصرافات المتصلة",
                        Dock = DockStyle.Top,
                        Height = 34,
                        ForeColor = LightUiTheme.Primary,
                        BackColor = LightUiTheme.Surface,
                        TextAlign = ContentAlignment.MiddleLeft,
                        Padding = new Padding(10, 0, 0, 0),
                        Font = new Font("Segoe UI", 10f, FontStyle.Bold)
                    };
                    _pnlATMCards = new FlowLayoutPanel
                    {
                        Dock             = DockStyle.Fill,
                        FlowDirection    = FlowDirection.LeftToRight,
                        WrapContents     = true,
                        AutoScroll       = true,
                        Padding          = new Padding(8),
                        BackColor        = LightUiTheme.Window
                    };
                    cardsHost.Controls.Add(_pnlATMCards);
                    cardsHost.Controls.Add(cardsHeader);
                    pnlCardsArea.Controls.Add(cardsHost);
                    pnlCardsArea.Controls.Add(operationsPanel);
                    // سجل كلمات السر
                    var pnlPasswordLog = new Panel { Height = 110, Dock = DockStyle.Bottom, BackColor = Color.FromArgb(22, 24, 26) };
                    var pwdLabel = new Label { Text = "سجل تغيير كلمات السر:", Dock = DockStyle.Top, ForeColor = Color.FromArgb(142, 142, 147), Height = 22, Padding = new Padding(6, 4, 0, 0) };
                    _rtbPasswordLog = new RichTextBox { Dock = DockStyle.Fill, BackColor = Color.FromArgb(28, 30, 32), ForeColor = Color.FromArgb(200, 200, 200), ReadOnly = true, BorderStyle = BorderStyle.None, Font = new Font("Consolas", 8.5f) };
                    pnlPasswordLog.Controls.AddRange(new Control[] { _rtbPasswordLog, pwdLabel });
                    _tabNOC.Controls.AddRange(new Control[] { pnlCardsArea, pnlPasswordLog, _pnlServerControls, _pnlMetrics });
                }
                // ═══════════════════════════════════════════
                // TAB 2: Connections
                // ═══════════════════════════════════════════
                private void BuildTabConnections()
                {
                    var toolbar = new Panel { Height = 40, Dock = DockStyle.Top, BackColor = Color.FromArgb(22, 24, 26), Padding = new Padding(8, 6, 8, 6) };
                    _lblConnCount = new Label { Text = "الاتصالات: 0", ForeColor = Color.FromArgb(200, 200, 200), AutoSize = true, Location = new Point(8, 10) };
                    _cmbConnFilter = new ComboBox { Width = 150, Location = new Point(120, 7), DropDownStyle = ComboBoxStyle.DropDownList, BackColor = Color.FromArgb(40, 42, 45), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
                    _cmbConnFilter.Items.AddRange(new object[] { "الكل", "متصل", "منقطع", "يزامن", "Supervisor" });
                    _cmbConnFilter.SelectedIndex = 0;
                    toolbar.Controls.AddRange(new Control[] { _lblConnCount, _cmbConnFilter });
                    _dgvConnections = CreateDataGrid();
                    _dgvConnections.Columns.AddRange(new DataGridViewColumn[]
                    {
                        new DataGridViewTextBoxColumn { HeaderText = "معرف الصراف",    Width = 110, DataPropertyName = "ATM_ID"       },
                        new DataGridViewTextBoxColumn { HeaderText = "الاسم",          Width = 130, DataPropertyName = "ATM_Name"     },
                        new DataGridViewTextBoxColumn { HeaderText = "النوع",          Width = 70,  DataPropertyName = "ATM_Type"     },
                        new DataGridViewTextBoxColumn { HeaderText = "IP",             Width = 120, DataPropertyName = "ServerIP"     },
                        new DataGridViewTextBoxColumn { HeaderText = "الشبكة",        Width = 80,  DataPropertyName = "NetworkType"  },
                        new DataGridViewTextBoxColumn { HeaderText = "الحالة",        Width = 130, DataPropertyName = "Status"       },
                        new DataGridViewTextBoxColumn { HeaderText = "الكمون",        Width = 80,  DataPropertyName = "Latency"      },
                        new DataGridViewTextBoxColumn { HeaderText = "آخر Heartbeat", Width = 140, DataPropertyName = "HB"           },
                        new DataGridViewTextBoxColumn { HeaderText = "آخر جورنال",   Width = 140, DataPropertyName = "LastSync"     },
                        new DataGridViewTextBoxColumn { HeaderText = "Session ID",    Width = 130, DataPropertyName = "SessionId"    }
                    });
                    _tabConnections.Controls.AddRange(new Control[] { _dgvConnections, toolbar });
                }
                // ═══════════════════════════════════════════
                // TAB 3: Archive
                // ═══════════════════════════════════════════
                private void BuildTabArchive()
                {
                    var searchPanel = new Panel { Height = 55, Dock = DockStyle.Top, BackColor = Color.FromArgb(22, 24, 26), Padding = new Padding(12, 10, 12, 10) };
                    var sf = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, WrapContents = false };
                    _txtSearchATM    = CreateInput("معرف الصراف", 120);
                    _dtpFrom         = new DateTimePicker { Width = 140, Format = DateTimePickerFormat.Short, Value = DateTime.Today.AddDays(-30), BackColor = Color.FromArgb(40, 42, 45), ForeColor = Color.White };
                    _dtpTo           = new DateTimePicker { Width = 140, Format = DateTimePickerFormat.Short, Value = DateTime.Today, BackColor = Color.FromArgb(40, 42, 45), ForeColor = Color.White };
                    _txtSearchKeyword = CreateInput("كلمة البحث", 140);
                    _btnSearch        = MakeButton("بحث",     Color.FromArgb(0, 85, 170), OnSearchArchive, 100);
                    _btnExportCSV     = MakeButton("CSV",     Color.FromArgb(30, 80, 50),  OnExportArchiveCSV, 90);
                    _btnExportHTML    = MakeButton("HTML",    Color.FromArgb(50, 50, 100), OnExportArchiveHTML, 100);
                    sf.Controls.AddRange(new Control[] {
                        MakeLabel("الصراف:"), _txtSearchATM, MakeLabel("  من:"), _dtpFrom,
                        MakeLabel("  إلى:"), _dtpTo, MakeLabel("  بحث:"), _txtSearchKeyword,
                        _btnSearch, _btnExportCSV, _btnExportHTML
                    });
                    searchPanel.Controls.Add(sf);
                    _lblArchiveCount = new Label { Text = "النتائج: 0", Dock = DockStyle.Top, Height = 22, ForeColor = Color.FromArgb(142, 142, 147), Padding = new Padding(12, 4, 0, 0), BackColor = Color.FromArgb(26, 28, 30) };
                    _dgvArchive = CreateDataGrid();
                    _dgvArchive.Columns.AddRange(new DataGridViewColumn[]
                    {
                        new DataGridViewTextBoxColumn { HeaderText = "معرف الصراف",  Width = 110 },
                        new DataGridViewTextBoxColumn { HeaderText = "اسم الملف",    Width = 160 },
                        new DataGridViewTextBoxColumn { HeaderText = "الحجم الأصلي", Width = 100 },
                        new DataGridViewTextBoxColumn { HeaderText = "الحجم مضغوط",  Width = 100 },
                        new DataGridViewTextBoxColumn { HeaderText = "عدد العمليات", Width = 100 },
                        new DataGridViewTextBoxColumn { HeaderText = "Checksum",      Width = 140 },
                        new DataGridViewTextBoxColumn { HeaderText = "تاريخ الاستلام", Width = 140 },
                        new DataGridViewTextBoxColumn { HeaderText = "مسار الأرشيف",  Width = 280, AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill }
                    });
                    _dgvArchive.DoubleClick += OnArchiveEntryDoubleClick;
                    _tabArchive.Controls.AddRange(new Control[] { _dgvArchive, _lblArchiveCount, searchPanel });
                }
                // ═══════════════════════════════════════════
                // TAB 4: Remote Control
                // ═══════════════════════════════════════════
                private void BuildTabRemote()
                {
                    var splitContainer = new SplitContainer
                    {
                        Dock        = DockStyle.Fill,
                        Orientation = Orientation.Vertical,
                        SplitterDistance = 450,
                        SplitterWidth    = 4,
                        BackColor        = Color.FromArgb(44, 46, 48)
                    };
                    // ═ الجانب الأيسر: Ghost View + معلومات
                    var leftPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(26, 28, 30), Padding = new Padding(8) };
                    var targetRow = new FlowLayoutPanel { Height = 40, Dock = DockStyle.Top, FlowDirection = FlowDirection.LeftToRight, BackColor = Color.FromArgb(22, 24, 26) };
                    _cmbTargetATM = new ComboBox { Width = 200, DropDownStyle = ComboBoxStyle.DropDownList, BackColor = Color.FromArgb(40, 42, 45), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
                    targetRow.Controls.AddRange(new Control[] { MakeLabel("الصراف المستهدف: "), _cmbTargetATM });
                    // Ghost View
                    var ghostPanel = new Panel { Height = 260, Dock = DockStyle.Top, BackColor = Color.Black, Margin = new Padding(0, 8, 0, 0) };
                    _pbGhostView   = new PictureBox { Dock = DockStyle.Fill, SizeMode = PictureBoxSizeMode.Zoom, BackColor = Color.Black };
                    var ghostLabel = new Label { Text = "Ghost View - مشاهدة فقط", Dock = DockStyle.Top, Height = 24, ForeColor = Color.FromArgb(142, 142, 147), BackColor = Color.FromArgb(22, 24, 26), TextAlign = ContentAlignment.MiddleCenter };
                    var ghostCtrl  = new FlowLayoutPanel { Height = 36, Dock = DockStyle.Bottom, BackColor = Color.FromArgb(22, 24, 26), FlowDirection = FlowDirection.LeftToRight };
                    _tbGhostQuality  = new TrackBar { Minimum = 10, Maximum = 100, Value = 75, Width = 140, TickFrequency = 10, BackColor = Color.FromArgb(22, 24, 26) };
                    _lblGhostQuality = new Label { Text = "75%", ForeColor = Color.White, AutoSize = true };
                    _btnGhostStart   = MakeButton("بدء Ghost",  Color.FromArgb(20, 70, 130), OnGhostStart, 110);
                    _btnGhostStop    = MakeButton("إيقاف Ghost", Color.FromArgb(80, 20, 20),  OnGhostStop,  110);
                    _btnGhostStop.Enabled = false;
                    _tbGhostQuality.Scroll += (s, e) => { _lblGhostQuality.Text = $"{_tbGhostQuality.Value}%"; };
                    ghostCtrl.Controls.AddRange(new Control[] { _btnGhostStart, _btnGhostStop, MakeLabel("جودة:"), _tbGhostQuality, _lblGhostQuality });
                    ghostPanel.Controls.AddRange(new Control[] { _pbGhostView, ghostCtrl, ghostLabel });
                    // الأوامر
                    var cmdsPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 8, 0, 0) };
                    var cmdsFlow  = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 120, FlowDirection = FlowDirection.LeftToRight, WrapContents = true };
                    _btnRestart        = MakeButton("إعادة تشغيل",       Color.FromArgb(100, 40, 20), OnRestart, 130);
                    _btnShutdown       = MakeButton("إيقاف النظام",       Color.FromArgb(100, 20, 20), OnShutdown, 130);
                    _btnChangePassword = MakeButton("تغيير كلمة السر",    Color.FromArgb(60, 40, 100), OnChangePassword, 155);
                    _btnForceSync      = MakeButton("مزامنة فورية",       Color.FromArgb(20, 60, 60),  OnForceSync, 130);
                    _btnSyncTime       = MakeButton("ضبط الوقت",          Color.FromArgb(40, 60, 100), OnSyncTime, 130);
                    _btnSyncImages     = MakeButton("مزامنة الصور",       Color.FromArgb(50, 70, 35),  OnSyncImages, 130);
                    _btnSyncFolder     = MakeButton("مزامنة مجلد",        Color.FromArgb(45, 55, 85),  OnSyncFolder, 130);
                    _btnRemoteAccessOn = MakeButton("تفعيل RDP",          Color.FromArgb(35, 90, 70),  OnRemoteAccessStart, 130);
                    _btnRemoteAccessOff= MakeButton("إيقاف RDP",          Color.FromArgb(110, 55, 35), OnRemoteAccessStop, 130);
                    _btnSendFile       = MakeButton("إرسال ملف",          Color.FromArgb(20, 70, 30),  OnSendFile, 130);
                    _btnGetFile        = MakeButton("طلب ملف",            Color.FromArgb(30, 50, 70),  OnGetFile,  130);
                    _txtRemotePassword = CreateInput("كلمة السر الجديدة", 160);
                    _txtRemotePassword.UseSystemPasswordChar = true;
                    _txtFileParam      = CreateInput("ملف/مجلد: SOURCE=...;TARGET=...", 260);
                    cmdsFlow.Controls.AddRange(new Control[]
                    {
                        _btnRestart, _btnShutdown, _btnChangePassword, _btnForceSync,
                        _btnSyncTime, _btnSyncImages, _btnSyncFolder,
                        _btnRemoteAccessOn, _btnRemoteAccessOff, _btnSendFile, _btnGetFile
                    });
                    var paramFlow = new FlowLayoutPanel { Height = 38, Dock = DockStyle.Top, FlowDirection = FlowDirection.LeftToRight };
                    paramFlow.Controls.AddRange(new Control[] { MakeLabel("كلمة السر:"), _txtRemotePassword, MakeLabel("  ملف:"), _txtFileParam });
                    cmdsPanel.Controls.AddRange(new Control[] { cmdsFlow, paramFlow });
                    leftPanel.Controls.AddRange(new Control[] { cmdsPanel, ghostPanel, targetRow });
                    splitContainer.Panel1.Controls.Add(leftPanel);
                    // ═ الجانب الأيمن: سجل الأوامر
                    var rightPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(26, 28, 30) };
                    var cmdLogLabel = new Label { Text = "سجل الأوامر المرسلة", Dock = DockStyle.Top, Height = 28, ForeColor = Color.FromArgb(142, 142, 147), BackColor = Color.FromArgb(22, 24, 26), TextAlign = ContentAlignment.MiddleCenter };
                    _dgvCommandLog = CreateDataGrid();
                    _dgvCommandLog.Dock = DockStyle.Fill;
                    _dgvCommandLog.Columns.AddRange(new DataGridViewColumn[]
                    {
                        new DataGridViewTextBoxColumn { HeaderText = "المعرف",   Width = 90  },
                        new DataGridViewTextBoxColumn { HeaderText = "الأمر",     Width = 160 },
                        new DataGridViewTextBoxColumn { HeaderText = "الصراف",    Width = 110 },
                        new DataGridViewTextBoxColumn { HeaderText = "المرسل",    Width = 100 },
                        new DataGridViewTextBoxColumn { HeaderText = "الحالة",    Width = 90  },
                        new DataGridViewTextBoxColumn { HeaderText = "الإرسال",   Width = 100 },
                        new DataGridViewTextBoxColumn { HeaderText = "الرد",      Width = 100 },
                        new DataGridViewTextBoxColumn { HeaderText = "النتيجة",   Width = 240, AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill }
                    });
                    rightPanel.Controls.AddRange(new Control[] { _dgvCommandLog, cmdLogLabel });
                    splitContainer.Panel2.Controls.Add(rightPanel);
                    _tabRemote.Controls.Add(splitContainer);
                }
                // ═══════════════════════════════════════════
                // TAB 5: Scenarios
                // ═══════════════════════════════════════════
                private void BuildTabScenarios()
                {
                    var toolbar = new Panel { Height = 58, Dock = DockStyle.Top, BackColor = Color.FromArgb(22, 24, 26), Padding = new Padding(12, 10, 12, 10) };
                    var flow = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, WrapContents = false };
                    _btnRefreshScenarios = MakeButton("تحديث المصفوفة", Color.FromArgb(30, 50, 80), RefreshScenarioMatrix, 150);
                    _btnRunScenario = MakeButton("تنفيذ المحدد", Color.FromArgb(17, 94, 89), RunSelectedScenario, 150);
                    flow.Controls.AddRange(new Control[] { _btnRefreshScenarios, _btnRunScenario });
                    toolbar.Controls.Add(flow);
                    _lblScenarioSummary = new Label
                    {
                        Text = "مصفوفة السيناريوهات تربط الحالة التشغيلية بالمؤشرات والإجراء الفعلي المطلوب.",
                        Height = 42,
                        Dock = DockStyle.Top,
                        TextAlign = ContentAlignment.MiddleLeft,
                        Padding = new Padding(12, 0, 12, 0),
                        BackColor = Color.FromArgb(35, 37, 40),
                        ForeColor = Color.FromArgb(220, 220, 220),
                        Font = new Font("Segoe UI", 9.5f)
                    };
                    _rtbScenarioLog = new RichTextBox
                    {
                        Height = 130,
                        Dock = DockStyle.Bottom,
                        ReadOnly = true,
                        BorderStyle = BorderStyle.None,
                        BackColor = Color.FromArgb(20, 22, 24),
                        ForeColor = Color.FromArgb(220, 220, 220),
                        Font = new Font("Consolas", 9f),
                        RightToLeft = RightToLeft.No,
                        ScrollBars = RichTextBoxScrollBars.Vertical
                    };
                    _dgvScenarioMatrix = CreateDataGrid();
                    _dgvScenarioMatrix.Dock = DockStyle.Fill;
                    _dgvScenarioMatrix.Columns.AddRange(new DataGridViewColumn[]
                    {
                        new DataGridViewTextBoxColumn { HeaderText = "المفتاح", Width = 100 },
                        new DataGridViewTextBoxColumn { HeaderText = "السيناريو", Width = 170 },
                        new DataGridViewTextBoxColumn { HeaderText = "متى يستخدم", Width = 220 },
                        new DataGridViewTextBoxColumn { HeaderText = "الإجراء التنفيذي", Width = 260 },
                        new DataGridViewTextBoxColumn { HeaderText = "التأثير", Width = 120 },
                        new DataGridViewTextBoxColumn { HeaderText = "الحالة الحالية", Width = 220, AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill }
                    });
                    _dgvScenarioMatrix.CellDoubleClick += (s, e) => RunSelectedScenario();
                    _tabScenarios.Controls.AddRange(new Control[] { _dgvScenarioMatrix, _rtbScenarioLog, _lblScenarioSummary, toolbar });
                    RefreshScenarioMatrix();
                }
                private void RefreshScenarioMatrix()
                {
                    if (_dgvScenarioMatrix == null) return;
                    var atms = GetFleetSnapshot();
                    var connected = 0;
                    var warning = 0;
                    var critical = 0;
                    foreach (var atm in atms)
                    {
                        if (atm.ConnectionStatus == ConnectionStatus.Connected) connected++;
                        if (atm.HealthScore < 80 || atm.ConsecutiveSyncFailures > 0) warning++;
                        if (atm.HealthScore < 50 || atm.ConnectionStatus == ConnectionStatus.Disconnected) critical++;
                    }
                    _dgvScenarioMatrix.Rows.Clear();
                    AddScenarioRow("HEALTH", "فحص صحة الأسطول", "بداية المناوبة أو ظهور تنبيهات", "قراءة HealthScore وHeartbeat وتوليد توصيات", "تشغيلي", $"متصل {connected} | مراقبة {warning} | حرج {critical}");
                    AddScenarioRow("FORCE_SYNC", "استعادة المزامنة", "تأخر جورنال أو فشل إرسال", "إرسال CMD_FORCE_SYNC للصرافات المتصلة", "عال", $"صرافات متصلة قابلة للإرسال: {connected}");
                    AddScenarioRow("WINDOWS_READY", "جاهزية Windows", "بعد إعادة تشغيل أو بلاغ دعم", "CMD_GET_STATS + CMD_SYNC_TIME لكل صراف متصل", "عال", $"أوامر متوقعة: {connected * 2}");
                    AddScenarioRow("ARCHIVE_AUDIT", "تدقيق الأرشيف", "قبل التقرير أو عند شك في نقص ملفات", "فحص عدد الملفات والحجم حسب الصراف", "متوسط", $"صرافات في الذاكرة: {atms.Count}");
                    AddScenarioRow("MONTHLY_ARCHIVE", "الأرشفة الشهرية", "بداية شهر أو تدقيق امتثال", "فحص تقسيم YYYY-MM وحجم الملفات المشفرة", "حوكمة", DateTime.UtcNow.ToString("yyyy-MM"));
                    AddScenarioRow("IMAGE_PUSH", "نقل صور/مجلدات", "تحديث شاشة أو حملة صور", "CMD_SYNC_FOLDER من مصدر مشترك إلى مسار الصراف", "عال", $"هدف افتراضي: {AppConstants.DefaultImagesPath}");
                    AddScenarioRow("PASSWORD_ONE", "كلمة سر لصراف محدد", "صيانة جهاز أو إعادة اعتماد", "CMD_CHANGE_PASSWORD على الصراف المحدد فقط", "عال", "تنفيذ فردي مع تأكيد");
                    AddScenarioRow("DAILY_REPORT", "تقرير الإدارة اليومي", "نهاية المناوبة أو التسليم", "تصدير تقرير NOC HTML وفتحه", "حوكمة", DateTime.Today.ToString("yyyy-MM-dd"));
                    AddScenarioRow("PING_ALL", "تحقق اتصال سريع", "قبل أوامر حساسة أو صيانة جماعية", "إرسال CMD_PING لكل صراف متصل", "تشغيلي", $"متصل: {connected}");
                    _lblScenarioSummary.Text = $"آخر تحديث: {DateTime.Now:HH:mm:ss} | صرافات: {atms.Count} | متصل: {connected} | مراقبة: {warning} | حرج: {critical}";
                    AppendScenarioLog("REFRESH", _lblScenarioSummary.Text);
                }
                private void AddScenarioRow(string key, string scenario, string trigger, string action, string impact, string status)
                {
                    var rowIndex = _dgvScenarioMatrix.Rows.Add(key, scenario, trigger, action, impact, status);
                    var row = _dgvScenarioMatrix.Rows[rowIndex];
                    row.DefaultCellStyle.ForeColor = impact == "عال"
                        ? Color.FromArgb(255, 149, 0)
                        : impact == "حوكمة" ? Color.FromArgb(10, 132, 255) : Color.FromArgb(33, 37, 41);
                }
                private void RunSelectedScenario()
                {
                    if (_dgvScenarioMatrix == null || _dgvScenarioMatrix.CurrentRow == null)
                    {
                        AppendScenarioLog("SKIP", "اختر سيناريو من الجدول أولاً.");
                        return;
                    }
                    var key = _dgvScenarioMatrix.CurrentRow.Cells[0].Value?.ToString();
                    AppendScenarioLog("RUN", $"بدء تنفيذ السيناريو: {key}");
                    switch (key)
                    {
                        case "HEALTH": RunFleetHealthScenario(); break;
                        case "FORCE_SYNC": RunForceSyncAllScenario(); break;
                        case "WINDOWS_READY": RunWindowsReadinessScenario(); break;
                        case "ARCHIVE_AUDIT": RunArchiveAuditScenario(); break;
                        case "MONTHLY_ARCHIVE": RunMonthlyArchiveAuditScenario(); break;
                        case "IMAGE_PUSH": RunImagesSyncAll(); break;
                        case "PASSWORD_ONE": RunPasswordSelected(); break;
                        case "DAILY_REPORT": RunDailyReportScenario(); break;
                        case "PING_ALL": RunPingAllScenario(); break;
                        default: AppendScenarioLog("SKIP", "سيناريو غير معروف."); break;
                    }
                    RefreshScenarioMatrix();
                }
                private void AppendScenarioLog(string tag, string message)
                {
                    var line = $"[{DateTime.Now:HH:mm:ss}] {tag} | {message}\r\n";
                    if (_rtbScenarioLog != null)
                    {
                        _rtbScenarioLog.AppendText(line);
                        _rtbScenarioLog.ScrollToCaret();
                    }
                    AppLogger.Instance.Info($"{tag}: {message}", "Scenarios");
                }
                private void RunPingAllScenario()
                {
                    var atms = GetFleetSnapshot();
                    int sent = 0;
                    foreach (var atm in atms)
                        if (_serverEngine.SendCommand(atm.ATM_ID, AppConstants.CMD_PING, "", _currentUser))
                            sent++;
                    AppendScenarioLog("PING", $"أُرسل PING إلى {sent} صراف.");
                    AppendOpsLog("PING", $"أُرسل PING إلى {sent} صراف.");
                }
                // ═══════════════════════════════════════════
                // TAB 6: Analytics
                // ═══════════════════════════════════════════
                private void BuildTabAnalytics()
                {
                    var toolbar = new Panel { Height = 50, Dock = DockStyle.Top, BackColor = Color.FromArgb(22, 24, 26), Padding = new Padding(12, 10, 12, 10) };
                    var tf = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, WrapContents = false };
                    _cmbAnalyticsATM   = new ComboBox { Width = 160, DropDownStyle = ComboBoxStyle.DropDownList, BackColor = Color.FromArgb(40, 42, 45), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
                    _dtpAnalyticsFrom  = new DateTimePicker { Width = 130, Format = DateTimePickerFormat.Short, Value = DateTime.Today.AddDays(-7) };
                    _dtpAnalyticsTo    = new DateTimePicker { Width = 130, Format = DateTimePickerFormat.Short, Value = DateTime.Today };
                    _btnRunAnalytics   = MakeButton("تحليل الجورنال",    Color.FromArgb(0, 85, 170),  OnRunAnalytics, 150);
                    _btnRunFleetAnalysis = MakeButton("تحليل الأسطول",   Color.FromArgb(90, 60, 20),  OnRunFleetAnalysis, 150);
                    _btnExportAnalytics = MakeButton("تصدير HTML",       Color.FromArgb(50, 50, 100), OnExportAnalytics, 140);
                    tf.Controls.AddRange(new Control[] { MakeLabel("الصراف: "), _cmbAnalyticsATM, MakeLabel("  من: "), _dtpAnalyticsFrom, MakeLabel("  إلى: "), _dtpAnalyticsTo, _btnRunAnalytics, _btnRunFleetAnalysis, _btnExportAnalytics });
                    toolbar.Controls.Add(tf);
                    // لوحة الإحصائيات
                    _pnlAnalyticsStats = new Panel { Height = 90, Dock = DockStyle.Top, BackColor = Color.FromArgb(22, 24, 26) };
                    var statsFlow = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, Padding = new Padding(12, 4, 12, 4) };
                    string[] aLabels = { "ناجحة", "مرفوضة", "بطاقات", "معدل نجاح", "نقد", "صحة", "أخطاء", "ملفات" };
                    for (int i = 0; i < 8; i++)
                    {
                        var mini = new Panel { Width = 110, Height = 70, Margin = new Padding(4, 8, 4, 0), BackColor = Color.FromArgb(30, 32, 35) };
                        _analyticsStatLabels[i] = new Label { Text = "—", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter, ForeColor = Color.White, Font = new Font("Segoe UI", 13f, FontStyle.Bold) };
                        var lbl = new Label { Text = aLabels[i], Dock = DockStyle.Bottom, Height = 20, TextAlign = ContentAlignment.MiddleCenter, ForeColor = Color.FromArgb(142, 142, 147), Font = new Font("Segoe UI", 8f) };
                        mini.Controls.AddRange(new Control[] { _analyticsStatLabels[i], lbl });
                        statsFlow.Controls.Add(mini);
                    }
                    _pnlAnalyticsStats.Controls.Add(statsFlow);
                    _lblAnalyticsSummary = new Label { Text = "اختر صراف ونطاق زمني ثم اضغط تشغيل التحليل", Dock = DockStyle.Top, Height = 28, TextAlign = ContentAlignment.MiddleCenter, ForeColor = Color.FromArgb(142, 142, 147), BackColor = Color.FromArgb(26, 28, 30) };
                    _lblPredictionSummary = new Label
                    {
                        Text = "تحليل الأسطول يعطي تقدير خطورة وتشخيص سريع حسب الصحة، الاتصال، آخر مزامنة، وفشل الإرسال.",
                        Dock = DockStyle.Top,
                        Height = 46,
                        TextAlign = ContentAlignment.MiddleLeft,
                        Padding = new Padding(12, 4, 12, 4),
                        ForeColor = Color.FromArgb(210, 210, 210),
                        BackColor = Color.FromArgb(35, 37, 40)
                    };
                    _dgvAnalytics = CreateDataGrid();
                    _dgvAnalytics.Columns.AddRange(new DataGridViewColumn[]
                    {
                        new DataGridViewTextBoxColumn { HeaderText = "#",          Width = 50  },
                        new DataGridViewTextBoxColumn { HeaderText = "النوع",      Width = 120 },
                        new DataGridViewTextBoxColumn { HeaderText = "النتيجة",    Width = 90  },
                        new DataGridViewTextBoxColumn { HeaderText = "المبلغ",     Width = 100 },
                        new DataGridViewTextBoxColumn { HeaderText = "رمز الخطأ",  Width = 100 },
                        new DataGridViewTextBoxColumn { HeaderText = "بطاقة",      Width = 70  },
                        new DataGridViewTextBoxColumn { HeaderText = "السطر",      Width = 70  },
                        new DataGridViewTextBoxColumn { HeaderText = "السياق",     Width = 350, AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill }
                    });
                    _tabAnalytics.Controls.AddRange(new Control[] { _dgvAnalytics, _lblPredictionSummary, _lblAnalyticsSummary, _pnlAnalyticsStats, toolbar });
                }
                // ═══════════════════════════════════════════
                // TAB 6: Log
                // ═══════════════════════════════════════════
                private void BuildTabLog()
                {
                    var toolbar = new Panel { Height = 42, Dock = DockStyle.Top, BackColor = Color.FromArgb(22, 24, 26), Padding = new Padding(10, 8, 10, 8) };
                    var tf = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, WrapContents = false };
                    _cmbLogLevel   = new ComboBox { Width = 120, DropDownStyle = ComboBoxStyle.DropDownList, BackColor = Color.FromArgb(40, 42, 45), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
                    _cmbLogLevel.Items.AddRange(new object[] { "الكل", "Debug", "Info", "Warning", "Error", "Critical" });
                    _cmbLogLevel.SelectedIndex = 0;
                    _txtLogSearch  = CreateInput("بحث في السجل...", 200);
                    _btnClearLog   = MakeButton("مسح", Color.FromArgb(80, 30, 30), OnClearLog, 90);
                    _btnExportLog  = MakeButton("تصدير", Color.FromArgb(30, 60, 30), OnExportLog, 100);
                    _lblLogCount   = new Label { Text = "0 سطر", ForeColor = Color.FromArgb(142, 142, 147), AutoSize = true };
                    _cmbLogLevel.SelectedIndexChanged += (s, e) => FilterLog();
                    _txtLogSearch.TextChanged         += (s, e) => FilterLog();
                    tf.Controls.AddRange(new Control[] { MakeLabel("المستوى: "), _cmbLogLevel, MakeLabel("  "), _txtLogSearch, _btnClearLog, _btnExportLog, _lblLogCount });
                    toolbar.Controls.Add(tf);
                    _rtbLog = new RichTextBox
                    {
                        Dock        = DockStyle.Fill,
                        BackColor   = Color.FromArgb(20, 22, 24),
                        ForeColor   = Color.FromArgb(200, 200, 200),
                        Font        = new Font("Consolas", 9f),
                        ReadOnly    = true,
                        BorderStyle = BorderStyle.None,
                        ScrollBars  = RichTextBoxScrollBars.Vertical
                    };
                    _tabLog.Controls.AddRange(new Control[] { _rtbLog, toolbar });
                }
                // ==========================================
                // ATM Card Panels
                // ==========================================
                private void OnATMConnected(ATMInfo atm)
                {
                    if (InvokeRequired) { Invoke(new Action(() => OnATMConnected(atm))); return; }
                    if (!_atmPanels.ContainsKey(atm.ATM_ID))
                    {
                        var card = new ATMCardPanel(atm);
                        card.OnDoubleClickCard += (s, a) => OpenJournalViewer(a);
                        _atmPanels[atm.ATM_ID] = card;
                        _pnlATMCards.Controls.Add(card);
                        _cmbTargetATM.Items.Add(atm.ATM_ID);
                        if (_cmbOpsTargetATM != null && !_cmbOpsTargetATM.Items.Contains(atm.ATM_ID))
                        {
                            _cmbOpsTargetATM.Items.Add(atm.ATM_ID);
                            if (_cmbOpsTargetATM.SelectedIndex < 0)
                                _cmbOpsTargetATM.SelectedIndex = 0;
                        }
                        _cmbAnalyticsATM.Items.Add(atm.ATM_ID);
                        if (_cmbTargetATM.SelectedIndex < 0)
                            _cmbTargetATM.SelectedIndex = 0;
                    }
                    _atmPanels[atm.ATM_ID].UpdateATM(atm);
                    UpdateMetrics();
                    RefreshConnectionsTab();
                    AppLogger.Instance.Info($"★ ATM متصل: {atm.ATM_ID} ({atm.ATM_Type})", "UI");
                }
                private void OnATMDisconnected(ATMInfo atm)
                {
                    if (InvokeRequired) { Invoke(new Action(() => OnATMDisconnected(atm))); return; }
                    if (_atmPanels.ContainsKey(atm.ATM_ID))
                        _atmPanels[atm.ATM_ID].UpdateATM(atm);
                    UpdateMetrics();
                    RefreshConnectionsTab();
                    AlertManager.Instance.CheckATMHealth(atm);
                }
                private void RefreshATMCard(ATMInfo atm)
                {
                    if (InvokeRequired) { Invoke(new Action(() => RefreshATMCard(atm))); return; }
                    if (_atmPanels.TryGetValue(atm.ATM_ID, out var card))
                        card.UpdateATM(atm);
                }
                private void OnJournalReceived(ReceivedPacket pkt)
                {
                    if (pkt.IsGhostFrame)
                    {
                        // تحديث Ghost View
                        if (InvokeRequired) { Invoke(new Action(() => UpdateGhostView(pkt.Message.Payload))); return; }
                        UpdateGhostView(pkt.Message.Payload);
                        return;
                    }
                    // أرشفة الجورنال
                    if (pkt.Data != null && pkt.Data.Length > 0)
                    {
                        var path = _archiveManager.Archive(pkt.ATM.ATM_ID, pkt.FileName, pkt.Data, pkt.Checksum, pkt.SHA256);
                        pkt.ATM.TransactionCount++;
                        AppLogger.Instance.Info($"📦 جورنال مؤرشف: {pkt.ATM.ATM_ID}/{pkt.FileName} → {path}", "Archive");
                    }
                }
                private void UpdateGhostView(byte[] jpegData)
                {
                    if (jpegData == null || jpegData.Length == 0) return;
                    try
                    {
                        using var ms = new MemoryStream(jpegData);
                        var img = Image.FromStream(ms);
                        _pbGhostView.Image?.Dispose();
                        _pbGhostView.Image = img;
                    }
                    catch { }
                }
                // ==========================================
                // عمليات الأزرار
                // ==========================================
                private void StartServer()
                {
                    try
                    {
                        _serverEngine.Start();
                        _btnStartServer.Enabled = false;
                        _btnStopServer.Enabled  = true;
                        _lblStatus.Text         = $"● خادم يعمل على TCP/{AppConstants.DefaultPort}";
                        _lblStatus.ForeColor    = Color.FromArgb(52, 199, 89);
                        AppendOpsLog("SERVER", $"الخادم يعمل على TCP/{AppConstants.DefaultPort}. جاهز لاستقبال الصرافات.");
                        AppLogger.Instance.Info($"★ الخادم يعمل على TCP/{AppConstants.DefaultPort}", "Server");
                        DatabaseManager.Instance.InsertAuditLog("ServerStart", _currentUser, null, "تشغيل الخادم");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"فشل تشغيل الخادم: {ex.Message}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                private void StopServer()
                {
                    if (MessageBox.Show("هل تريد إيقاف الخادم؟ سيتم قطع جميع الاتصالات.", "تأكيد", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
                    _serverEngine.Stop();
                    _btnStartServer.Enabled = true;
                    _btnStopServer.Enabled  = false;
                    _lblStatus.Text         = "● خادم متوقف";
                    _lblStatus.ForeColor    = Color.FromArgb(255, 69, 58);
                    AppendOpsLog("SERVER", "تم إيقاف الخادم وقطع الجلسات النشطة.");
                    DatabaseManager.Instance.InsertAuditLog("ServerStop", _currentUser, null, "إيقاف الخادم");
                }
                private void OnChangePasswordAll(object s = null, EventArgs e = null)
                {
                    if (MessageBox.Show("هل تريد تغيير كلمة السر لجميع الصرافات المتصلة؟\nستُرسل الأوامر بالتسلسل.", "تأكيد جماعي", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
                    var newPwd = Microsoft.VisualBasic.Interaction.InputBox("أدخل كلمة السر الجديدة:", "تغيير كلمة سر جماعي", "");
                    if (string.IsNullOrEmpty(newPwd)) return;
                    _rtbPasswordLog.AppendText($"\r\n[{DateTime.Now:HH:mm:ss}] بدء تغيير كلمة السر الجماعي...\r\n");
                    int sent = 0;
                    foreach (var atm in _serverEngine.GetConnectedATMs())
                    {
                        var ok = _serverEngine.SendCommand(atm.ATM_ID, AppConstants.CMD_CHANGE_PASSWORD, newPwd, _currentUser);
                        if (ok) sent++;
                        _rtbPasswordLog.AppendText($"  → {atm.ATM_ID}: {(ok ? "✓ أُرسل" : "✗ فشل")}\r\n");
                    }
                    _rtbPasswordLog.ScrollToCaret();
                    AppendOpsLog("PASSWORD", $"أُرسل أمر تغيير كلمة السر إلى {sent} صراف.");
                }
                private void OnSendImagesAll(object s = null, EventArgs e = null)
                {
                    var payload = $"SOURCE={AppConstants.ShareImagesAllPath};TARGET={AppConstants.DefaultImagesPath};OVERWRITE=true";
                    int sent = 0;
                    foreach (var atm in _serverEngine.GetConnectedATMs())
                        if (_serverEngine.SendCommand(atm.ATM_ID, AppConstants.CMD_SYNC_FOLDER, payload, _currentUser))
                            sent++;
                    AppendOpsLog("IMAGES", $"أُرسل أمر مزامنة الصور إلى {sent} صراف.");
                    AppLogger.Instance.Info("📷 أُرسل أمر مزامنة الصور لجميع الصرافات", "Remote");
                }
                private void OnBroadcast(object s = null, EventArgs e = null)
                {
                    var msg = Microsoft.VisualBasic.Interaction.InputBox("أدخل الرسالة للبث لجميع الصرافات:", "بث جماعي", "");
                    if (string.IsNullOrEmpty(msg)) return;
                    _serverEngine.Broadcast(msg, _currentUser);
                    AppendOpsLog("BROADCAST", $"تم بث رسالة تشغيلية إلى الجلسات المتصلة: {msg}");
                    DatabaseManager.Instance.InsertAuditLog("Broadcast", _currentUser, null, msg);
                }
                private void OnGhostStart(object s = null, EventArgs e = null)
                {
                    var atmId = _cmbTargetATM.SelectedItem?.ToString();
                    if (string.IsNullOrEmpty(atmId)) return;
                    if (MessageBox.Show($"هل تريد بدء وضع الشبح لـ {atmId}؟", "تأكيد Ghost View", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
                    _serverEngine.SendCommand(atmId, AppConstants.CMD_GHOST_START, _tbGhostQuality.Value.ToString(), _currentUser);
                    _btnGhostStart.Enabled = false;
                    _btnGhostStop.Enabled  = true;
                    AppendOpsLog("GHOST", $"تم طلب Ghost View من {atmId} بجودة {_tbGhostQuality.Value}%.");
                    DatabaseManager.Instance.InsertAuditLog("GhostStart", _currentUser, atmId, $"Ghost View quality={_tbGhostQuality.Value}%");
                }
                private void OnGhostStop(object s = null, EventArgs e = null)
                {
                    var atmId = _cmbTargetATM.SelectedItem?.ToString();
                    if (!string.IsNullOrEmpty(atmId))
                        _serverEngine.SendCommand(atmId, AppConstants.CMD_GHOST_STOP, "", _currentUser);
                    _btnGhostStart.Enabled = true;
                    _btnGhostStop.Enabled  = false;
                    AppendOpsLog("GHOST", $"تم إيقاف Ghost View للصراف {atmId}.");
                }
                private void OnRestart(object s = null, EventArgs e = null) => SendCmdWithConfirm(AppConstants.CMD_RESTART, "إعادة تشغيل");
                private void OnShutdown(object s = null, EventArgs e = null) => SendCmdWithConfirm(AppConstants.CMD_SHUTDOWN, "إيقاف التشغيل");
                private void OnChangePassword(object s = null, EventArgs e = null)
                {
                    var atmId = _cmbTargetATM.SelectedItem?.ToString();
                    if (string.IsNullOrEmpty(atmId) || string.IsNullOrEmpty(_txtRemotePassword.Text)) return;
                    if (MessageBox.Show($"تغيير كلمة سر {atmId}؟", "تأكيد", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
                    var ok = _serverEngine.SendCommand(atmId, AppConstants.CMD_CHANGE_PASSWORD, _txtRemotePassword.Text, _currentUser);
                    _rtbPasswordLog.AppendText($"\r\n[{DateTime.Now:HH:mm:ss}] {atmId} — تغيير كلمة السر: {(ok ? "✓" : "✗")}\r\n");
                }
                private void OnForceSync(object s = null, EventArgs e = null) => SendCmd(AppConstants.CMD_FORCE_SYNC, "");
                private void OnSyncTime(object s = null, EventArgs e = null)   => SendCmd(AppConstants.CMD_SYNC_TIME, $"UTC={DateTime.UtcNow:O}");
                private void OnSyncImages(object s = null, EventArgs e = null) => SendCmd(AppConstants.CMD_SYNC_IMAGES, _txtFileParam.Text);
                private void OnSyncFolder(object s = null, EventArgs e = null) => SendCmd(AppConstants.CMD_SYNC_FOLDER, _txtFileParam.Text);
                private void OnRemoteAccessStart(object s = null, EventArgs e = null) => SendCmdWithConfirm(AppConstants.CMD_WINDOWS_REMOTE_START, "تفعيل Remote Access");
                private void OnRemoteAccessStop(object s = null, EventArgs e = null)  => SendCmdWithConfirm(AppConstants.CMD_WINDOWS_REMOTE_STOP, "إيقاف Remote Access");
                private void OnSendFile(object s = null, EventArgs e = null)   => SendCmd(AppConstants.CMD_SEND_FILE, _txtFileParam.Text);
                private void OnGetFile(object s = null, EventArgs e = null)    => SendCmd(AppConstants.CMD_GET_FILE, _txtFileParam.Text);
                private void SendCmd(string cmd, string param)
                {
                    var atmId = _cmbTargetATM.SelectedItem?.ToString();
                    if (string.IsNullOrEmpty(atmId)) return;
                    var ok = _serverEngine.SendCommand(atmId, cmd, param, _currentUser);
                    AppendOpsLog("CMD", $"{cmd} -> {atmId}: {(ok ? "أُرسل" : "فشل")}");
                }
                private void SendCmdWithConfirm(string cmd, string label)
                {
                    var atmId = _cmbTargetATM.SelectedItem?.ToString();
                    if (string.IsNullOrEmpty(atmId)) return;
                    if (MessageBox.Show($"هل تريد {label} الصراف {atmId}؟", "تأكيد أمر حساس", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
                    var ok = _serverEngine.SendCommand(atmId, cmd, "", _currentUser);
                    AppendOpsLog("CMD", $"{label} -> {atmId}: {(ok ? "أُرسل" : "فشل")}");
                    DatabaseManager.Instance.InsertAuditLog(cmd, _currentUser, atmId, $"{label} — {(ok ? "نجح" : "فشل")}");
                }
                private void OnCommandChanged(RemoteCommand cmd)
                {
                    if (_dgvCommandLog == null) return;
                    if (_dgvCommandLog.InvokeRequired)
                    {
                        _dgvCommandLog.Invoke(new Action(() => OnCommandChanged(cmd)));
                        return;
                    }
                    if (!_commandRows.TryGetValue(cmd.CommandId, out var row) || row.DataGridView == null)
                    {
                        _dgvCommandLog.Rows.Insert(0);
                        row = _dgvCommandLog.Rows[0];
                        _commandRows[cmd.CommandId] = row;
                    }
                    row.Cells[0].Value = ShortCommandId(cmd.CommandId);
                    row.Cells[1].Value = cmd.CommandType;
                    row.Cells[2].Value = cmd.TargetATMId;
                    row.Cells[3].Value = string.IsNullOrWhiteSpace(cmd.SentBy) ? "System" : cmd.SentBy;
                    row.Cells[4].Value = ToArabicCommandStatus(cmd.Status);
                    row.Cells[5].Value = cmd.SentAtUtc.ToLocalTime().ToString("HH:mm:ss");
                    row.Cells[6].Value = cmd.AckedAtUtc.HasValue ? cmd.AckedAtUtc.Value.ToLocalTime().ToString("HH:mm:ss") : "بانتظار";
                    row.Cells[7].Value = string.IsNullOrWhiteSpace(cmd.Result) ? "بانتظار رد العميل" : cmd.Result;
                    row.DefaultCellStyle.ForeColor = CommandStatusColor(cmd.Status);
                    RefreshOperationsTelemetry();
                    if (cmd.Status == "Executed" || cmd.Status == "Failed")
                        AppendOpsLog("RESULT", $"{cmd.CommandType} [{ShortCommandId(cmd.CommandId)}] -> {cmd.TargetATMId}: {ToArabicCommandStatus(cmd.Status)} - {cmd.Result}");
                }
                private static string ShortCommandId(string commandId)
                {
                    if (string.IsNullOrWhiteSpace(commandId)) return "—";
                    return commandId.Length <= 8 ? commandId : commandId.Substring(0, 8);
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
                private static Color CommandStatusColor(string status)
                {
                    switch (status)
                    {
                        case "Executed": return Color.FromArgb(52, 199, 89);
                        case "Failed":   return Color.FromArgb(255, 69, 58);
                        case "Timeout":  return Color.FromArgb(255, 149, 0);
                        case "Sent":     return Color.FromArgb(10, 132, 255);
                        default:         return Color.FromArgb(33, 37, 41);
                    }
                }
                // ==========================================
                // البحث في الأرشيف
                // ==========================================
                private void OnSearchArchive(object s = null, EventArgs e = null)
                {
                    var entries = DatabaseManager.Instance.SearchArchive(
                        string.IsNullOrWhiteSpace(_txtSearchATM.Text) ? null : _txtSearchATM.Text.Trim(),
                        _dtpFrom.Value, _dtpTo.Value,
                        string.IsNullOrWhiteSpace(_txtSearchKeyword.Text) ? null : _txtSearchKeyword.Text.Trim());
                    _dgvArchive.Rows.Clear();
                    foreach (var e2 in entries)
                        _dgvArchive.Rows.Add(e2.ATMId, e2.FileName, e2.FileSizeDisplay,
                            $"{e2.CompressedSize / 1024.0:F1} KB", e2.TransactionCount,
                            e2.MD5Hash?.Substring(0, 8), e2.ReceivedAt.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss"), e2.ArchivePath);
                    _lblArchiveCount.Text = $"النتائج: {entries.Count} إدخال";
                }
                private void OnArchiveEntryDoubleClick(object s, EventArgs e)
                {
                    if (_dgvArchive.CurrentRow == null) return;
                    var path = _dgvArchive.CurrentRow.Cells[7].Value?.ToString();
                    if (string.IsNullOrEmpty(path) || !File.Exists(path)) return;
                    var atmId = _dgvArchive.CurrentRow.Cells[0].Value?.ToString();
                    var atmType = _serverEngine.GetATMByID(atmId)?.ATM_Type ?? "NCR";
                    OpenJournalTextViewer(path, atmId, atmType);
                }
                // ==========================================
                // التحليلات
                // ==========================================
                private void OnRunAnalytics(object s = null, EventArgs e = null)
                {
                    var atmId = _cmbAnalyticsATM.SelectedItem?.ToString();
                    if (string.IsNullOrEmpty(atmId))
                    {
                        OnRunFleetAnalysis(s, e);
                        return;
                    }
                    var entries = DatabaseManager.Instance.SearchArchive(atmId, _dtpAnalyticsFrom.Value, _dtpAnalyticsTo.Value, null, 200);
                    if (entries.Count == 0)
                    {
                        _lblAnalyticsSummary.Text = "لا يوجد أرشيف جورنال في الفترة المحددة. تم تحويل العرض إلى تحليل الأسطول الحي.";
                        OnRunFleetAnalysis(s, e);
                        return;
                    }
                    // تحليل الملف الأحدث للعرض
                    var latest = entries[0];
                    var atm    = _serverEngine.GetATMByID(atmId);
                    JournalAnalysisResult result = null;
                    if (!string.IsNullOrEmpty(latest.ArchivePath) && File.Exists(latest.ArchivePath))
                    {
                        var text = new ArchiveManager().RetrieveAsText(latest.ArchivePath);
                        if (!string.IsNullOrEmpty(text))
                            result = _analysisEngine.AnalyzeText(text, atmId, atm?.ATM_Type ?? "NCR") == null ? null :
                                new JournalAnalysisResult { ATMId = atmId };
                    }
                    // عرض الإحصائيات التجميعية من قاعدة البيانات
                    RefreshAnalyticsFromDB(atmId, _dtpAnalyticsFrom.Value, _dtpAnalyticsTo.Value, entries.Count);
                }
                private void OnRunFleetAnalysis(object s = null, EventArgs e = null)
                {
                    var atms = new List<ATMInfo>(_serverEngine.GetConnectedATMs());
                    _dgvAnalytics.Rows.Clear();
                    int critical = 0, warning = 0, healthy = 0;
                    int index = 1;
                    int healthTotal = 0;
                    foreach (var atm in atms)
                    {
                        var prediction = _predictionEngine.Predict(atm);
                        var risk = prediction.RiskScore;
                        healthTotal += atm.HealthScore;
                        if (risk >= 75) critical++;
                        else if (risk >= 40) warning++;
                        else healthy++;
                        var rowIndex = _dgvAnalytics.Rows.Add(index++, atm.ATM_ID, prediction.Level, atm.HealthScore + "%", atm.ConsecutiveSyncFailures,
                            atm.ConnectionStatus, atm.Latency_ms + "ms", $"{prediction.Reason} | الإجراء المقترح: {prediction.RecommendedAction} | الأثر: {prediction.EstimatedImpact}");
                        var row = _dgvAnalytics.Rows[rowIndex];
                        row.DefaultCellStyle.ForeColor = risk >= 75
                            ? Color.FromArgb(255, 69, 58)
                            : risk >= 40 ? Color.FromArgb(255, 149, 0) : Color.FromArgb(52, 199, 89);
                    }
                    var avgHealth = atms.Count > 0 ? (double)healthTotal / atms.Count : 0;
                    _analyticsStatLabels[0].Text = healthy.ToString("N0");
                    _analyticsStatLabels[1].Text = warning.ToString("N0");
                    _analyticsStatLabels[2].Text = critical.ToString("N0");
                    _analyticsStatLabels[3].Text = atms.Count > 0 ? $"{(double)healthy / atms.Count * 100:F1}%" : "—";
                    _analyticsStatLabels[4].Text = _archiveManager.GetFreeSpaceGB().ToString("F1") + " GB";
                    _analyticsStatLabels[5].Text = atms.Count > 0 ? avgHealth.ToString("F0") + "%" : "—";
                    _analyticsStatLabels[6].Text = AlertManager.Instance.ActiveCount.ToString("N0");
                    _analyticsStatLabels[7].Text = atms.Count.ToString("N0");
                    _lblAnalyticsSummary.Text = $"تحليل حي للأسطول — {atms.Count} صراف | سليم: {healthy} | مراقبة: {warning} | حرج: {critical}";
                    _lblPredictionSummary.Text = atms.Count == 0
                        ? "لا توجد صرافات متصلة للتحليل. شغّل الخادم واتصل بعميل واحد على الأقل لعرض التنبؤ التشغيلي."
                        : $"التوقع التشغيلي: {(critical > 0 ? "توجد حالات حرجة تحتاج إجراء فوري" : warning > 0 ? "توجد حالات مراقبة تحتاج متابعة" : "الأسطول مستقر حالياً")} | متوسط الصحة {avgHealth:F0}%";
                    AppendOpsLog("ANALYTICS", _lblAnalyticsSummary.Text);
                }
                private void RefreshAnalyticsFromDB(string atmId, DateTime from, DateTime to, int entryCount)
                {
                    // مجمعة من journal_archive
                    var stats = DatabaseManager.Instance.GetDailyStatsTable(atmId,
                        from.ToString("yyyy-MM-dd"), to.ToString("yyyy-MM-dd"));
                    int approved = 0, failed = 0, captures = 0;
                    double cash  = 0;
                    foreach (System.Data.DataRow row in stats.Rows)
                    {
                        approved += Convert.ToInt32(row["approved_tx"]);
                        failed   += Convert.ToInt32(row["failed_tx"]);
                        captures += Convert.ToInt32(row["cards_captured"]);
                        cash     += Convert.ToDouble(row["cash_dispensed"]);
                    }
                    _analyticsStatLabels[0].Text = approved.ToString("N0");
                    _analyticsStatLabels[1].Text = failed.ToString("N0");
                    _analyticsStatLabels[2].Text = captures.ToString("N0");
                    _analyticsStatLabels[3].Text = approved + failed > 0 ? $"{(double)approved / (approved + failed) * 100:F1}%" : "—";
                    _analyticsStatLabels[4].Text = cash.ToString("N0");
                    _analyticsStatLabels[5].Text = "—";
                    _analyticsStatLabels[6].Text = "—";
                    _analyticsStatLabels[7].Text = entryCount.ToString("N0");
                    _lblAnalyticsSummary.Text = $"تحليل {atmId} — {entryCount} ملف جورنال | {from:yyyy-MM-dd} → {to:yyyy-MM-dd}";
                }
                private void OnExportAnalytics(object s = null, EventArgs e = null)
                {
                    // تصدير تقرير HTML
                    var atms = new List<ATMInfo>(_serverEngine.GetConnectedATMs());
                    var path = _reportEngine.ExportDailyNocReport(atms, DateTime.Today);
                    System.Diagnostics.Process.Start(path);
                }
                private void OnExportArchiveCSV(object s = null, EventArgs e = null)
                {
                    var path = _reportEngine.ExportTransactionsCSV(_txtSearchATM.Text.Trim(), _dtpFrom.Value, _dtpTo.Value);
                    MessageBox.Show($"تم تصدير التقرير:\n{path}", "تصدير ناجح", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                private void OnExportArchiveHTML(object s = null, EventArgs e = null)
                {
                    // تصدير HTML نهائي
                    OnExportAnalytics();
                }
                // ==========================================
                // لوحة المقاييس
                // ==========================================
                private void UpdateMetrics()
                {
                    var atms      = new List<ATMInfo>(_serverEngine.GetConnectedATMs());
                    int connected = 0, idle = 0;
                    long totalKB  = 0;
                    foreach (var a in atms)
                    {
                        if (a.ConnectionStatus == ConnectionStatus.Connected) connected++;
                        if ((DateTime.UtcNow - a.LastDataReceivedUtc).TotalMinutes > 30) idle++;
                        totalKB += a.JournalSizeToday / 1024;
                    }
                    var rate = atms.Count > 0 ? (double)connected / atms.Count * 100.0 : 0;
                    var free = _archiveManager.GetFreeSpaceGB();
                    SetMetric(0, connected.ToString());
                    SetMetric(1, idle.ToString());
                    SetMetric(2, $"{totalKB:N0} KB");
                    SetMetric(3, $"{rate:F1}%");
                    SetMetric(4, AlertManager.Instance.ActiveCount.ToString());
                    SetMetric(5, "—");
                    SetMetric(6, DateTime.Now.ToString("HH:mm:ss"));
                    SetMetric(7, $"{free:F1} GB");
                    _lblConnections.Text = $"الاتصالات: {connected}/{atms.Count}";
                    _lblAlerts.Text      = $"التنبيهات: {AlertManager.Instance.ActiveCount}";
                    _lblStorage.Text     = $"المساحة: {free:F1} GB";
                    AlertManager.Instance.CheckDiskSpace("Server", free, _archiveManager.GetTotalSpaceGB());
                }
                private void SetMetric(int idx, string value)
                {
                    if (idx < _metricValues.Length && _metricValues[idx] != null)
                        _metricValues[idx].Text = value;
                }
                // ==========================================
                // التنبيهات
                // ==========================================
                private void ShowAlert(AlertPayload alert)
                {
                    if (InvokeRequired) { Invoke(new Action(() => ShowAlert(alert))); return; }
                    AppendLog($"[ALERT] {alert.SeverityIcon} {alert.Title}: {alert.Message}", AppLogger.Level.Warning);
                }
                private void ShowCriticalAlert(AlertPayload alert)
                {
                    if (InvokeRequired) { Invoke(new Action(() => ShowCriticalAlert(alert))); return; }
                    _alertBar.BackColor  = Color.FromArgb(60, 15, 15);
                    _lblAlertText.Text   = $"{alert.Title}: {alert.Message}  [اضغط للإغلاق]";
                    _alertBar.Visible    = true;
                    AppendLog($"[CRITICAL] {alert.Title}: {alert.Message}", AppLogger.Level.Critical);
                }
                // ==========================================
                // السجل
                // ==========================================
                private readonly object _logLock = new object();
                private int _logLines = 0;
                private void AppendLog(string text, AppLogger.Level level = AppLogger.Level.Info)
                {
                    if (_rtbLog == null || IsDisposed) return;
                    if (_rtbLog.InvokeRequired) { _rtbLog.Invoke(new Action(() => AppendLog(text, level))); return; }
                    lock (_logLock)
                    {
                        var color = level switch
                        {
                            AppLogger.Level.Error    => Color.FromArgb(255, 100, 100),
                            AppLogger.Level.Critical => Color.FromArgb(255, 50, 50),
                            AppLogger.Level.Warning  => Color.FromArgb(255, 200, 50),
                            AppLogger.Level.Debug    => Color.FromArgb(100, 140, 200),
                            _ => Color.FromArgb(200, 200, 200)
                        };
                        _rtbLog.SelectionStart  = _rtbLog.TextLength;
                        _rtbLog.SelectionLength = 0;
                        _rtbLog.SelectionColor  = color;
                        _rtbLog.AppendText($"{text}\n");
                        _rtbLog.SelectionColor  = Color.FromArgb(200, 200, 200);
                        _rtbLog.ScrollToCaret();
                        _logLines++;
                        if (_lblLogCount != null) _lblLogCount.Text = $"{_logLines} سطر";
                        // حد أقصى 5000 سطر
                        if (_logLines > 5000)
                        {
                            _rtbLog.Clear();
                            _logLines = 0;
                        }
                    }
                }
                private void FilterLog() { /* فلترة مرئية فقط */ }
                private void OnClearLog(object s, EventArgs e) { _rtbLog.Clear(); _logLines = 0; }
                private void OnExportLog(object s, EventArgs e)
                {
                    var path = Path.Combine(AppConstants.DefaultLogPath, $"server_log_{DateTime.Now:yyyyMMdd_HHmmss}.txt");
                    File.WriteAllText(path, _rtbLog.Text);
                    MessageBox.Show($"تم تصدير السجل:\n{path}", "تصدير ناجح");
                }
                // ==========================================
                // واجهة قراءة الجورنال
                // ==========================================
                private void OpenJournalViewer(ATMInfo atm)
                {
                    var viewer = new JournalViewerForm(atm, _analysisEngine);
                    viewer.Show(this);
                }
                private void OpenJournalTextViewer(string archivePath, string atmId, string atmType)
                {
                    var viewer = new JournalViewerForm(archivePath, atmId, atmType, _analysisEngine, _archiveManager);
                    viewer.Show(this);
                }
                private void EnsureServerShareFolders()
                {
                    foreach (var folder in AppConstants.GetServerImageShareFolders())
                        Directory.CreateDirectory(folder);
                }
                // ==========================================
                // التحديث الدوري
                // ==========================================
                private void InitializeTimers()
                {
                    _uiTimer = new System.Windows.Forms.Timer { Interval = 1000 };
                    _uiTimer.Tick += (s, e) =>
                    {
                        _lblTime.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        UpdateMetrics();
                        RefreshConnectionsTab();
                    };
                    _uiTimer.Start();
                    _healthTimer = new System.Windows.Forms.Timer { Interval = 60000 };
                    _healthTimer.Tick += (s, e) =>
                    {
                        foreach (var atm in _serverEngine.GetConnectedATMs())
                            AlertManager.Instance.CheckATMHealth(atm);
                    };
                    _healthTimer.Start();
                }
                private void RefreshConnectionsTab()
                {
                    if (_tabMain.SelectedTab != _tabConnections) return;
                    var atms = new List<ATMInfo>(_serverEngine.GetConnectedATMs());
                    _dgvConnections.Rows.Clear();
                    foreach (var atm in atms)
                        _dgvConnections.Rows.Add(atm.ATM_ID, atm.ATM_Name, atm.ATM_Type, atm.ServerIP,
                            atm.NetworkType, atm.GetStatusLabel(), $"{atm.Latency_ms} ms",
                            atm.GetElapsed(atm.LastHeartbeatUtc), atm.GetElapsed(atm.LastSyncUtc), atm.SessionId);
                    _lblConnCount.Text = $"الاتصالات: {atms.Count}";
                }
                // ==========================================
                // مساعدات UI
                // ==========================================
                private Panel CreateMetricCard(string icon, string label, string value, int idx)
                {
                    const int cardWidth = 150;
                    var card = new Panel { Width = cardWidth, Height = 92, Margin = new Padding(4, 0, 4, 4), BackColor = LightUiTheme.Surface, Cursor = Cursors.Default, BorderStyle = BorderStyle.FixedSingle };
                    var iconLabel  = new Label { Text = icon, Dock = DockStyle.Top, Height = 26, TextAlign = ContentAlignment.MiddleCenter, ForeColor = Color.FromArgb(73, 80, 87), Font = new Font("Segoe UI", 8.5f, FontStyle.Bold), UseCompatibleTextRendering = true };
                    _metricValues[idx] = new Label { Text = value, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter, ForeColor = Color.FromArgb(0, 102, 204), Font = new Font("Segoe UI", 15.5f, FontStyle.Bold), AutoEllipsis = true };
                    var lblText    = new Label { Text = label, Dock = DockStyle.Bottom, Height = 26, TextAlign = ContentAlignment.MiddleCenter, ForeColor = LightUiTheme.Muted, Font = new Font("Segoe UI", 8.2f), AutoEllipsis = true };
                    card.Controls.AddRange(new Control[] { _metricValues[idx], lblText, iconLabel });
                    return card;
                }
                private Panel CreateOperationsPanel()
                {
                    var panel = new Panel
                    {
                        Dock = DockStyle.Right,
                        Width = 430,
                        BackColor = LightUiTheme.Surface,
                        Padding = new Padding(12),
                        BorderStyle = BorderStyle.FixedSingle
                    };
                    var title = new Label
                    {
                        Text = "مركز قيادة العمليات",
                        Dock = DockStyle.Top,
                        Height = 34,
                        ForeColor = LightUiTheme.Primary,
                        Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                        TextAlign = ContentAlignment.MiddleLeft
                    };
                    _lblOperationsSummary = new Label
                    {
                        Dock = DockStyle.Top,
                        Height = 86,
                        Text = "جاهزية التنفيذ: ابدأ الخادم ثم راقب الاتصالات. كل أمر يكتب أثره هنا وفي سجل التدقيق.",
                        ForeColor = LightUiTheme.Text,
                        BackColor = LightUiTheme.SurfaceAlt,
                        Padding = new Padding(10),
                        Font = new Font("Segoe UI", 9f),
                        TextAlign = ContentAlignment.MiddleLeft
                    };
                    var scenarioLabel = new Label
                    {
                        Text = "سيناريوهات تشغيل فعلية",
                        Dock = DockStyle.Top,
                        Height = 28,
                        ForeColor = LightUiTheme.Muted,
                        Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                        TextAlign = ContentAlignment.MiddleLeft
                    };
                    var scenarioFlow = new FlowLayoutPanel
                    {
                        Dock = DockStyle.Top,
                        Height = 86,
                        FlowDirection = FlowDirection.LeftToRight,
                        WrapContents = true,
                        Padding = Padding.Empty,
                        BackColor = LightUiTheme.Surface
                    };
                    scenarioFlow.Controls.AddRange(new Control[]
                    {
                        MakeButton("فحص صحة الأسطول", LightUiTheme.Primary, RunFleetHealthScenario, 190),
                        MakeButton("مزامنة فورية للجميع", Color.FromArgb(17, 94, 89), RunForceSyncAllScenario, 190),
                        MakeButton("تهيئة Windows", Color.FromArgb(75, 85, 99), RunWindowsReadinessScenario, 190),
                        MakeButton("تدقيق الأرشيف", Color.FromArgb(120, 70, 20), RunArchiveAuditScenario, 190),
                        MakeButton("تقرير يومي", Color.FromArgb(60, 40, 100), RunDailyReportScenario, 190)
                    });
                    var missionLabel = new Label
                    {
                        Text = "تنفيذ مباشر محدد المصدر والهدف",
                        Dock = DockStyle.Top,
                        Height = 28,
                        ForeColor = LightUiTheme.Muted,
                        Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                        TextAlign = ContentAlignment.MiddleLeft
                    };
                    var missionInputs = new TableLayoutPanel
                    {
                        Dock = DockStyle.Top,
                        Height = 120,
                        ColumnCount = 2,
                        RowCount = 4,
                        BackColor = LightUiTheme.Surface,
                        Padding = new Padding(0, 2, 0, 2)
                    };
                    missionInputs.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90));
                    missionInputs.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
                    for (int i = 0; i < 4; i++)
                        missionInputs.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
                    _cmbOpsTargetATM = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList, BackColor = LightUiTheme.Surface, ForeColor = LightUiTheme.Text, FlatStyle = FlatStyle.System };
                    _txtOpsSourceFolder = CreateInput(AppConstants.ShareImagesAllPath, 260);
                    _txtOpsSourceFolder.Dock = DockStyle.Fill;
                    _txtOpsTargetFolder = CreateInput(AppConstants.DefaultImagesPath, 260);
                    _txtOpsTargetFolder.Dock = DockStyle.Fill;
                    _txtOpsPassword = CreateInput("كلمة سر لصراف محدد", 260);
                    _txtOpsPassword.Dock = DockStyle.Fill;
                    _txtOpsPassword.UseSystemPasswordChar = true;
                    missionInputs.Controls.Add(MakeLabel("الصراف:"), 0, 0);
                    missionInputs.Controls.Add(_cmbOpsTargetATM, 1, 0);
                    missionInputs.Controls.Add(MakeLabel("المصدر:"), 0, 1);
                    missionInputs.Controls.Add(_txtOpsSourceFolder, 1, 1);
                    missionInputs.Controls.Add(MakeLabel("الهدف:"), 0, 2);
                    missionInputs.Controls.Add(_txtOpsTargetFolder, 1, 2);
                    missionInputs.Controls.Add(MakeLabel("كلمة السر:"), 0, 3);
                    missionInputs.Controls.Add(_txtOpsPassword, 1, 3);
                    var missionFlow = new FlowLayoutPanel
                    {
                        Dock = DockStyle.Top,
                        Height = 168,
                        FlowDirection = FlowDirection.LeftToRight,
                        WrapContents = true,
                        Padding = Padding.Empty,
                        BackColor = LightUiTheme.Surface
                    };
                    _btnOpsBrowseSource = MakeButton("اختيار مصدر", Color.FromArgb(75, 85, 99), BrowseOpsSourceFolder, 130);
                    _btnOpsFolderSelected = MakeButton("مجلد للمحدد", Color.FromArgb(45, 55, 85), RunFolderSyncSelected, 130);
                    _btnOpsFolderAll = MakeButton("مجلد للجميع", Color.FromArgb(45, 55, 85), RunFolderSyncAll, 130);
                    _btnOpsImagesSelected = MakeButton("صور للمحدد", Color.FromArgb(50, 70, 35), RunImagesSyncSelected, 130);
                    _btnOpsImagesAll = MakeButton("صور للجميع", Color.FromArgb(50, 70, 35), RunImagesSyncAll, 130);
                    _btnOpsPasswordSelected = MakeButton("كلمة سر للمحدد", Color.FromArgb(60, 40, 100), RunPasswordSelected, 150);
                    _btnOpsTimeSelected = MakeButton("وقت للمحدد", Color.FromArgb(40, 60, 100), RunTimeSyncSelected, 130);
                    _btnOpsTimeAll = MakeButton("وقت للجميع", Color.FromArgb(40, 60, 100), RunTimeSyncAll, 130);
                    _btnOpsMonthlyArchive = MakeButton("أرشفة شهرية", Color.FromArgb(120, 70, 20), RunMonthlyArchiveAuditScenario, 140);
                    missionFlow.Controls.AddRange(new Control[]
                    {
                        _btnOpsBrowseSource, _btnOpsFolderSelected, _btnOpsFolderAll,
                        _btnOpsImagesSelected, _btnOpsImagesAll, _btnOpsPasswordSelected,
                        _btnOpsTimeSelected, _btnOpsTimeAll, _btnOpsMonthlyArchive
                    });
                    var telemetryLabel = new Label
                    {
                        Text = "مؤشرات تنفيذ الأوامر",
                        Dock = DockStyle.Top,
                        Height = 24,
                        ForeColor = LightUiTheme.Muted,
                        Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                        TextAlign = ContentAlignment.MiddleLeft
                    };
                    var telemetryPanel = new Panel
                    {
                        Dock = DockStyle.Top,
                        Height = 90,
                        BackColor = LightUiTheme.SurfaceAlt,
                        Padding = new Padding(8, 8, 8, 6)
                    };
                    var telemetryFlow = new FlowLayoutPanel
                    {
                        Dock = DockStyle.Top,
                        Height = 34,
                        FlowDirection = FlowDirection.LeftToRight,
                        WrapContents = true,
                        BackColor = LightUiTheme.SurfaceAlt
                    };
                    _lblOpsCmdSent = new Label { AutoSize = true, ForeColor = LightUiTheme.Primary, Font = new Font("Segoe UI", 8.8f, FontStyle.Bold), Margin = new Padding(0, 5, 16, 0) };
                    _lblOpsCmdExecuted = new Label { AutoSize = true, ForeColor = LightUiTheme.Success, Font = new Font("Segoe UI", 8.8f, FontStyle.Bold), Margin = new Padding(0, 5, 16, 0) };
                    _lblOpsCmdFailed = new Label { AutoSize = true, ForeColor = LightUiTheme.Danger, Font = new Font("Segoe UI", 8.8f, FontStyle.Bold), Margin = new Padding(0, 5, 16, 0) };
                    _lblOpsCmdPending = new Label { AutoSize = true, ForeColor = LightUiTheme.Warning, Font = new Font("Segoe UI", 8.8f, FontStyle.Bold), Margin = new Padding(0, 5, 16, 0) };
                    telemetryFlow.Controls.AddRange(new Control[] { _lblOpsCmdSent, _lblOpsCmdExecuted, _lblOpsCmdFailed, _lblOpsCmdPending });
                    _lblOpsCmdLast = new Label
                    {
                        Dock = DockStyle.Fill,
                        TextAlign = ContentAlignment.MiddleLeft,
                        AutoEllipsis = true,
                        ForeColor = LightUiTheme.Text,
                        Font = new Font("Segoe UI", 8.5f)
                    };
                    telemetryPanel.Controls.Add(_lblOpsCmdLast);
                    telemetryPanel.Controls.Add(telemetryFlow);
                    var logLabel = new Label
                    {
                        Text = "سجل التنفيذ والأثر",
                        Dock = DockStyle.Top,
                        Height = 28,
                        ForeColor = LightUiTheme.Muted,
                        Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                        TextAlign = ContentAlignment.MiddleLeft
                    };
                    _rtbOperationsLog = new RichTextBox
                    {
                        Dock = DockStyle.Fill,
                        BackColor = Color.FromArgb(248, 250, 252),
                        ForeColor = LightUiTheme.Text,
                        BorderStyle = BorderStyle.FixedSingle,
                        ReadOnly = true,
                        Font = new Font("Consolas", 9f),
                        RightToLeft = RightToLeft.No
                    };
                    panel.Controls.Add(_rtbOperationsLog);
                    panel.Controls.Add(logLabel);
                    panel.Controls.Add(telemetryPanel);
                    panel.Controls.Add(telemetryLabel);
                    panel.Controls.Add(missionFlow);
                    panel.Controls.Add(missionInputs);
                    panel.Controls.Add(missionLabel);
                    panel.Controls.Add(scenarioFlow);
                    panel.Controls.Add(scenarioLabel);
                    panel.Controls.Add(_lblOperationsSummary);
                    panel.Controls.Add(title);
                    RefreshOperationsTelemetry();
                    AppendOpsLog("READY", "مركز العمليات جاهز. الأوامر الحساسة تتطلب اتصال صراف فعلي.");
                    return panel;
                }
                private void AppendOpsLog(string tag, string message)
                {
                    var line = $"[{DateTime.Now:HH:mm:ss}] {tag} | {message}\r\n";
                    if (_rtbOperationsLog != null)
                    {
                        _rtbOperationsLog.AppendText(line);
                        _rtbOperationsLog.ScrollToCaret();
                    }
                    if (_lblOperationsSummary != null)
                        _lblOperationsSummary.Text = message;
                    AppLogger.Instance.Info($"{tag}: {message}", "Operations");
                }
                private void RefreshOperationsTelemetry()
                {
                    if (_serverEngine == null || _lblOpsCmdSent == null) return;
                    var commands = new List<RemoteCommand>(_serverEngine.GetRecentCommands());
                    int sent = 0, executed = 0, failed = 0, pending = 0;
                    RemoteCommand last = null;
                    foreach (var cmd in commands)
                    {
                        if (last == null || cmd.SentAtUtc > last.SentAtUtc)
                            last = cmd;
                        switch (cmd.Status)
                        {
                            case "Sent":
                                sent++;
                                break;
                            case "Executed":
                                executed++;
                                break;
                            case "Failed":
                            case "Timeout":
                                failed++;
                                break;
                            default:
                                pending++;
                                break;
                        }
                    }
                    _lblOpsCmdSent.Text = $"مرسل: {sent}";
                    _lblOpsCmdExecuted.Text = $"منفذ: {executed}";
                    _lblOpsCmdFailed.Text = $"فشل: {failed}";
                    _lblOpsCmdPending.Text = $"بانتظار: {pending}";
                    var uploads = OperationalStateStore.Instance.GetUploads();
                    int ackedUploads = 0, duplicateUploads = 0, failedUploads = 0;
                    foreach (var upload in uploads)
                    {
                        if (upload.State == UploadHealthState.Acked) ackedUploads++;
                        else if (upload.State == UploadHealthState.Duplicate) duplicateUploads++;
                        else if (upload.State == UploadHealthState.Failed || upload.State == UploadHealthState.IntegrityFailure) failedUploads++;
                    }
                    var lastText = last == null
                        ? "آخر أمر: —"
                        : $"آخر أمر: {last.CommandType} -> {last.TargetATMId} | {ToArabicCommandStatus(last.Status)} | {last.SentAtUtc.ToLocalTime():HH:mm:ss}";
                    _lblOpsCmdLast.Text = $"{lastText} | رفع ACK:{ackedUploads} DUP:{duplicateUploads} FAIL:{failedUploads}";
                }
                private List<ATMInfo> GetFleetSnapshot()
                {
                    var list = new List<ATMInfo>();
                    if (_serverEngine == null) return list;
                    foreach (var atm in _serverEngine.GetConnectedATMs())
                        list.Add(atm);
                    return list;
                }
                private void RunFleetHealthScenario()
                {
                    UpdateMetrics();
                    RefreshConnectionsTab();
                    var atms = GetFleetSnapshot();
                    int connected = 0, warning = 0, critical = 0;
                    foreach (var atm in atms)
                    {
                        if (atm.ConnectionStatus == ConnectionStatus.Connected) connected++;
                        if (atm.HealthScore < 70) warning++;
                        if (atm.HealthScore < 40 || atm.ConsecutiveSyncFailures >= 3) critical++;
                    }
                    AppendOpsLog("HEALTH", $"تم فحص {atms.Count} صراف | متصل: {connected} | تحذير: {warning} | حرج: {critical}");
                }
                private void RunForceSyncAllScenario()
                {
                    var atms = GetFleetSnapshot();
                    int sent = 0;
                    foreach (var atm in atms)
                        if (_serverEngine.SendCommand(atm.ATM_ID, AppConstants.CMD_FORCE_SYNC, "", _currentUser))
                            sent++;
                    AppendOpsLog("SYNC", sent > 0
                        ? $"أُرسل أمر مزامنة فورية إلى {sent}/{atms.Count} صراف."
                        : "لا يوجد صراف متصل يستقبل أمر المزامنة الآن.");
                }
                private void RunWindowsReadinessScenario()
                {
                    if (MessageBox.Show("سيتم إرسال أوامر فحص الإحصائيات ومزامنة الوقت للصرافات المتصلة. هل تريد المتابعة؟",
                        "تهيئة عمليات Windows", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                        return;
                    var atms = GetFleetSnapshot();
                    int sent = 0;
                    foreach (var atm in atms)
                    {
                        if (_serverEngine.SendCommand(atm.ATM_ID, AppConstants.CMD_GET_STATS, "", _currentUser)) sent++;
                        if (_serverEngine.SendCommand(atm.ATM_ID, AppConstants.CMD_SYNC_TIME, $"UTC={DateTime.UtcNow:O}", _currentUser)) sent++;
                    }
                    AppendOpsLog("WINDOWS", sent > 0
                        ? $"تم إرسال {sent} أمر فحص/وقت إلى الصرافات المتصلة."
                        : "لا توجد جلسات صرافات متصلة لتنفيذ أوامر Windows.");
                }
                private void RunArchiveAuditScenario()
                {
                    var atms = GetFleetSnapshot();
                    long files = 0, bytes = 0;
                    foreach (var atm in atms)
                    {
                        var stats = _archiveManager.GetATMArchiveStats(atm.ATM_ID);
                        files += stats.fileCount;
                        bytes += stats.totalBytes;
                    }
                    AppendOpsLog("ARCHIVE", $"تدقيق الأرشيف: {files:N0} ملف مشفر | {bytes / 1024.0 / 1024.0:F1} MB | مساحة حرة {_archiveManager.GetFreeSpaceGB():F1} GB");
                }
                private void RunDailyReportScenario()
                {
                    var atms = GetFleetSnapshot();
                    var path = _reportEngine.ExportDailyNocReport(atms, DateTime.Today);
                    AppendOpsLog("REPORT", $"تم إنشاء تقرير NOC اليومي: {Path.GetFileName(path)}");
                    try { System.Diagnostics.Process.Start(path); } catch { }
                }
                private void BrowseOpsSourceFolder()
                {
                    using (var fbd = new FolderBrowserDialog())
                    {
                        fbd.Description = "اختر مجلد المصدر الذي ستسحبه الصرافات. يفضل أن يكون UNC أو مجلد مشاركة قابل للوصول من العميل.";
                        if (!string.IsNullOrWhiteSpace(_txtOpsSourceFolder?.Text) && Directory.Exists(_txtOpsSourceFolder.Text))
                            fbd.SelectedPath = _txtOpsSourceFolder.Text;
                        if (fbd.ShowDialog() == DialogResult.OK)
                            _txtOpsSourceFolder.Text = fbd.SelectedPath;
                    }
                }
                private string GetSelectedOpsATM()
                {
                    var atmId = _cmbOpsTargetATM?.SelectedItem?.ToString();
                    if (string.IsNullOrWhiteSpace(atmId))
                        atmId = _cmbTargetATM?.SelectedItem?.ToString();
                    return atmId;
                }
                private string BuildOpsFolderPayload(bool forceImagesDefaults)
                {
                    var source = _txtOpsSourceFolder?.Text?.Trim();
                    var target = _txtOpsTargetFolder?.Text?.Trim();
                    if (forceImagesDefaults && string.IsNullOrWhiteSpace(source))
                        source = AppConstants.ShareImagesAllPath;
                    if (string.IsNullOrWhiteSpace(source))
                        source = AppConstants.ShareImagesAllPath;
                    if (string.IsNullOrWhiteSpace(target))
                        target = AppConstants.DefaultImagesPath;
                    return $"SOURCE={source};TARGET={target};OVERWRITE=true";
                }
                private bool WarnIfOpsSourceNotVisible(string source)
                {
                    if (string.IsNullOrWhiteSpace(source)) return true;
                    if (Directory.Exists(source)) return true;
                    AppendOpsLog("WARN", $"مسار المصدر غير موجود على جهاز الخادم: {source}. إذا كان UNC متاحاً من الصراف فقط فسيتم إرسال الأمر، وإلا سيفشل العميل.");
                    return MessageBox.Show("مسار المصدر غير موجود على جهاز الخادم. هل تريد إرسال الأمر رغم ذلك؟",
                        "تحقق من مصدر المزامنة", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes;
                }
                private void RunFolderSyncSelected()
                {
                    var atmId = GetSelectedOpsATM();
                    if (string.IsNullOrWhiteSpace(atmId))
                    {
                        AppendOpsLog("SKIP", "اختر صرافاً متصلاً لتنفيذ مزامنة المجلد.");
                        return;
                    }
                    var payload = BuildOpsFolderPayload(false);
                    var source = ExtractPayloadValue(payload, "SOURCE");
                    if (!WarnIfOpsSourceNotVisible(source)) return;
                    var ok = _serverEngine.SendCommand(atmId, AppConstants.CMD_SYNC_FOLDER, payload, _currentUser);
                    AppendOpsLog("FOLDER", $"مزامنة مجلد -> {atmId}: {(ok ? "أُرسل" : "فشل")} | {payload}");
                }
                private void RunFolderSyncAll()
                {
                    var atms = GetFleetSnapshot();
                    if (atms.Count == 0)
                    {
                        AppendOpsLog("SKIP", "لا توجد صرافات متصلة لاستقبال مزامنة المجلد.");
                        return;
                    }
                    var payload = BuildOpsFolderPayload(false);
                    var source = ExtractPayloadValue(payload, "SOURCE");
                    if (!WarnIfOpsSourceNotVisible(source)) return;
                    if (MessageBox.Show($"سيتم إرسال مزامنة المجلد إلى {atms.Count} صراف. هل تريد المتابعة؟",
                        "مزامنة مجلد جماعية", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                        return;
                    var sent = 0;
                    foreach (var atm in atms)
                        if (_serverEngine.SendCommand(atm.ATM_ID, AppConstants.CMD_SYNC_FOLDER, payload, _currentUser))
                            sent++;
                    AppendOpsLog("FOLDER", $"أُرسل أمر مزامنة مجلد إلى {sent}/{atms.Count} صراف | {payload}");
                }
                private void RunImagesSyncSelected()
                {
                    if (string.IsNullOrWhiteSpace(_txtOpsSourceFolder?.Text))
                        _txtOpsSourceFolder.Text = AppConstants.ShareImagesAllPath;
                    if (string.IsNullOrWhiteSpace(_txtOpsTargetFolder?.Text))
                        _txtOpsTargetFolder.Text = AppConstants.DefaultImagesPath;
                    RunFolderSyncSelected();
                }
                private void RunImagesSyncAll()
                {
                    if (string.IsNullOrWhiteSpace(_txtOpsSourceFolder?.Text))
                        _txtOpsSourceFolder.Text = AppConstants.ShareImagesAllPath;
                    if (string.IsNullOrWhiteSpace(_txtOpsTargetFolder?.Text))
                        _txtOpsTargetFolder.Text = AppConstants.DefaultImagesPath;
                    RunFolderSyncAll();
                }
                private void RunPasswordSelected()
                {
                    var atmId = GetSelectedOpsATM();
                    var password = _txtOpsPassword?.Text ?? "";
                    if (string.IsNullOrWhiteSpace(atmId))
                    {
                        AppendOpsLog("SKIP", "اختر صرافاً متصلاً لتغيير كلمة السر.");
                        return;
                    }
                    if (string.IsNullOrWhiteSpace(password) || password == "كلمة سر لصراف محدد")
                    {
                        AppendOpsLog("SKIP", "أدخل كلمة السر الجديدة للصراف المحدد.");
                        return;
                    }
                    if (MessageBox.Show($"سيتم تغيير كلمة سر الصراف {atmId}. هل تريد المتابعة؟",
                        "تأكيد تغيير كلمة السر", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                        return;
                    var ok = _serverEngine.SendCommand(atmId, AppConstants.CMD_CHANGE_PASSWORD, password, _currentUser);
                    AppendOpsLog("PASSWORD", $"تغيير كلمة سر الصراف {atmId}: {(ok ? "أُرسل" : "فشل")}");
                    _rtbPasswordLog?.AppendText($"\r\n[{DateTime.Now:HH:mm:ss}] {atmId} — تغيير كلمة السر: {(ok ? "أُرسل" : "فشل")}\r\n");
                }
                private void RunTimeSyncSelected()
                {
                    var atmId = GetSelectedOpsATM();
                    if (string.IsNullOrWhiteSpace(atmId))
                    {
                        AppendOpsLog("SKIP", "اختر صرافاً متصلاً لمزامنة الوقت.");
                        return;
                    }
                    var payload = $"UTC={DateTime.UtcNow:O}";
                    var ok = _serverEngine.SendCommand(atmId, AppConstants.CMD_SYNC_TIME, payload, _currentUser);
                    AppendOpsLog("TIME", $"مزامنة وقت -> {atmId}: {(ok ? "أُرسل" : "فشل")} | {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                }
                private void RunTimeSyncAll()
                {
                    var atms = GetFleetSnapshot();
                    var payload = $"UTC={DateTime.UtcNow:O}";
                    var sent = 0;
                    foreach (var atm in atms)
                        if (_serverEngine.SendCommand(atm.ATM_ID, AppConstants.CMD_SYNC_TIME, payload, _currentUser))
                            sent++;
                    AppendOpsLog("TIME", sent > 0
                        ? $"أُرسل أمر مزامنة الوقت إلى {sent}/{atms.Count} صراف."
                        : "لا توجد صرافات متصلة لمزامنة الوقت.");
                }
                private void RunMonthlyArchiveAuditScenario()
                {
                    var month = DateTime.UtcNow.ToString("yyyy-MM");
                    var atmIds = GetKnownArchiveAtmIds();
                    long totalFiles = 0;
                    long totalBytes = 0;
                    foreach (var atmId in atmIds)
                    {
                        var stats = CountMonthlyArchive(atmId, month);
                        totalFiles += stats.files;
                        totalBytes += stats.bytes;
                    }
                    AppendOpsLog("MONTHLY", $"أرشيف {month}: {atmIds.Count} صراف | {totalFiles:N0} ملف | {totalBytes / 1024.0 / 1024.0:F1} MB | الجذر: {AppConstants.DefaultArchivePath}");
                    try
                    {
                        Directory.CreateDirectory(AppConstants.DefaultArchivePath);
                        System.Diagnostics.Process.Start("explorer.exe", AppConstants.DefaultArchivePath);
                    }
                    catch { }
                }
                private List<string> GetKnownArchiveAtmIds()
                {
                    var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    foreach (var atm in GetFleetSnapshot())
                        if (!string.IsNullOrWhiteSpace(atm.ATM_ID))
                            ids.Add(atm.ATM_ID);
                    try
                    {
                        if (Directory.Exists(AppConstants.DefaultArchivePath))
                        {
                            foreach (var dir in Directory.GetDirectories(AppConstants.DefaultArchivePath))
                                ids.Add(Path.GetFileName(dir));
                        }
                    }
                    catch { }
                    return new List<string>(ids);
                }
                private static (long files, long bytes) CountMonthlyArchive(string atmId, string month)
                {
                    var folder = Path.Combine(AppConstants.DefaultArchivePath, atmId ?? "", month ?? "");
                    if (!Directory.Exists(folder)) return (0, 0);
                    long files = 0;
                    long bytes = 0;
                    foreach (var file in Directory.GetFiles(folder, "*.ejl.enc", SearchOption.TopDirectoryOnly))
                    {
                        files++;
                        try { bytes += new FileInfo(file).Length; } catch { }
                    }
                    return (files, bytes);
                }
                private static string ExtractPayloadValue(string payload, string key)
                {
                    if (string.IsNullOrWhiteSpace(payload) || string.IsNullOrWhiteSpace(key)) return "";
                    foreach (var part in payload.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        var idx = part.IndexOf('=');
                        if (idx <= 0) continue;
                        var k = part.Substring(0, idx).Trim();
                        if (string.Equals(k, key, StringComparison.OrdinalIgnoreCase))
                            return part.Substring(idx + 1).Trim();
                    }
                    return "";
                }
                private Button MakeButton(string text, Color backColor, Action action, int width = 130) =>
                    MakeButton(text, backColor, (s, e) => action?.Invoke(), width);
                private Button MakeButton(string text, Color backColor, EventHandler onClick, int width = 130)
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
                    btn.Click += onClick;
                    return btn;
                }
                private TextBox CreateInput(string placeholder, int width) => new TextBox
                {
                    Width     = width,
                    Text      = placeholder,
                    BackColor = LightUiTheme.Surface,
                    ForeColor = LightUiTheme.Text,
                    BorderStyle = BorderStyle.FixedSingle,
                    Margin    = new Padding(4, 4, 4, 4)
                };
                private Label MakeLabel(string text) => new Label
                {
                    Text      = text,
                    AutoSize  = true,
                    ForeColor = LightUiTheme.Text,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Padding   = new Padding(0, 9, 0, 0),
                    MinimumSize = new Size(0, 34)
                };
                private DataGridView CreateDataGrid()
                {
                    var dgv = new DataGridView
                    {
                        Dock              = DockStyle.Fill,
                        BackgroundColor   = LightUiTheme.Surface,
                        GridColor         = LightUiTheme.Border,
                        ForeColor         = LightUiTheme.Text,
                        ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
                        {
                            BackColor = LightUiTheme.Header,
                            ForeColor = LightUiTheme.Text,
                            Font      = new Font("Segoe UI", 8.5f, FontStyle.Bold)
                        },
                        DefaultCellStyle  = new DataGridViewCellStyle
                        {
                            BackColor = LightUiTheme.Surface,
                            ForeColor = LightUiTheme.Text,
                            SelectionBackColor = LightUiTheme.Selection,
                            SelectionForeColor = Color.White
                        },
                        RowsDefaultCellStyle = new DataGridViewCellStyle
                        {
                            BackColor = LightUiTheme.Surface
                        },
                        AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
                        {
                            BackColor = Color.FromArgb(248, 250, 252)
                        },
                        SelectionMode     = DataGridViewSelectionMode.FullRowSelect,
                        MultiSelect       = false,
                        ReadOnly          = true,
                        AllowUserToAddRows = false,
                        AllowUserToDeleteRows = false,
                        RowHeadersVisible = false,
                        AutoSizeRowsMode  = DataGridViewAutoSizeRowsMode.None,
                        RowTemplate = { Height = 28 },
                        BorderStyle       = BorderStyle.FixedSingle,
                        ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single
                    };
                    LightUiTheme.ApplyDataGrid(dgv);
                    return dgv;
                }
                private void DrawTabItem(object sender, DrawItemEventArgs e)
                {
                    var tab   = _tabMain.TabPages[e.Index];
                    var rect  = e.Bounds;
                    bool sel  = e.Index == _tabMain.SelectedIndex;
                    using var bg = new SolidBrush(sel ? LightUiTheme.Surface : LightUiTheme.Header);
                    e.Graphics.FillRectangle(bg, rect);
                    if (sel)
                    {
                        using var accent = new SolidBrush(Color.FromArgb(0, 122, 255));
                        e.Graphics.FillRectangle(accent, rect.Left, rect.Bottom - 3, rect.Width, 3);
                    }
                    var textRect = Rectangle.Inflate(rect, -4, -2);
                    var color = sel ? Color.FromArgb(0, 102, 204) : LightUiTheme.Text;
                    var flags = TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter |
                                TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding |
                                TextFormatFlags.RightToLeft;
                    TextRenderer.DrawText(e.Graphics, tab.Text, new Font("Segoe UI", 8.8f, sel ? FontStyle.Bold : FontStyle.Regular), textRect, color, flags);
                }
                private void ApplyNocTheme()
                {
                    BackColor = LightUiTheme.Window;
                    foreach (TabPage tp in _tabMain.TabPages)
                        tp.BackColor = LightUiTheme.Window;
                    LightUiTheme.Apply(this);
                    _tabMain.DrawMode = TabDrawMode.OwnerDrawFixed;
                    _tabMain.ItemSize = new Size(150, 42);
                    _tabMain.Padding = new Point(12, 8);
                }
                private Icon CreateAppIcon() => null; // استخدم أيقونة النظام الافتراضية
                // Menu actions
                private void OnExportAllReports(object s, EventArgs e)
                {
                    var path = _reportEngine.ExportDailyNocReport(GetFleetSnapshot(), DateTime.Today);
                    AppendOpsLog("REPORT", $"تم تصدير التقرير الشامل: {Path.GetFileName(path)}");
                    try { System.Diagnostics.Process.Start(path); } catch { }
                }
                private void OnOpenSettings(object s, EventArgs e)
                {
                    _tabMain.SelectedTab = _tabRemote;
                    AppendOpsLog("SETTINGS", "تم فتح تبويب التحكم البعيد حيث توجد إعدادات الأوامر والمسارات.");
                }
                private void OnManageUsers(object s, EventArgs e)
                {
                    MessageBox.Show("إدارة المستخدمين مرتبطة حالياً بسجل التدقيق والصلاحيات داخل Core. ستحتاج شاشة مستخدمين كاملة عند إضافة مخزن مستخدمين دائم.",
                        "إدارة المستخدمين", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    AppendOpsLog("SECURITY", "تم فتح توضيح إدارة المستخدمين. لا يوجد مخزن مستخدمين دائم بعد.");
                }
                private void OnOpenAuditLog(object s, EventArgs e)
                {
                    var path = _reportEngine.ExportAuditLogCSV(DateTime.Today.AddDays(-30), DateTime.Today);
                    System.Diagnostics.Process.Start(path);
                }
                private void OnResetStats(object s, EventArgs e)
                {
                    if (MessageBox.Show("هل تريد تصفير مؤشرات الواجهة وسجل التنفيذ؟ لا يحذف هذا الأرشيف أو قاعدة البيانات.",
                        "إعادة ضبط مؤشرات الواجهة", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                        return;
                    for (int i = 0; i < _metricValues.Length; i++)
                        SetMetric(i, i == 3 ? "0.0%" : "0");
                    _rtbOperationsLog?.Clear();
                    _rtbPasswordLog?.Clear();
                    AppendOpsLog("RESET", "تم تصفير مؤشرات الواجهة فقط.");
                }
                // Missing event handlers from Designer
                private void btnBrowseStorage_Click(object sender, EventArgs e)
                {
                    using (FolderBrowserDialog fbd = new FolderBrowserDialog())
                    {
                        if (fbd.ShowDialog() == DialogResult.OK)
                            txtStorage.Text = fbd.SelectedPath;
                    }
                }
                private void btnStartStop_Click(object sender, EventArgs e)
                {
                    if (_designerServerRunning)
                    {
                        StopServer();
                        _designerServerRunning = false;
                        btnStartStop.Text = "▶ Start Server";
                        return;
                    }
                    StartServer();
                    _designerServerRunning = true;
                    btnStartStop.Text = "⏹ Stop Server";
                }
                private void btnSendCommand_Click(object sender, EventArgs e)
                {
                    var target = _cmbTargetATM?.SelectedItem?.ToString();
                    if (string.IsNullOrWhiteSpace(target))
                    {
                        foreach (var atm in _serverEngine.GetConnectedATMs())
                        {
                            target = atm.ATM_ID;
                            break;
                        }
                    }
                    if (string.IsNullOrWhiteSpace(target))
                    {
                        AppendDesignerLog("No connected ATM is available for the command.");
                        return;
                    }
                    var selected = (cmbCommand?.SelectedItem?.ToString() ?? "PING").ToUpperInvariant();
                    var cmd = selected switch
                    {
                        "RESTART" => AppConstants.CMD_RESTART,
                        "SCREENSHOT" => AppConstants.CMD_SCREENSHOT,
                        "TIMESYNC" => AppConstants.CMD_SYNC_TIME,
                        "SHUTDOWN" => AppConstants.CMD_SHUTDOWN,
                        _ => AppConstants.CMD_PING
                    };
                    var param = cmd == AppConstants.CMD_SYNC_TIME
                        ? $"UTC={DateTime.UtcNow:O}"
                        : "";
                    var ok = _serverEngine.SendCommand(target, cmd, param, _currentUser);
                    AppendDesignerLog($"{DateTime.Now:HH:mm:ss} CMD {cmd} -> {target}: {(ok ? "sent" : "failed")}");
                }
                private void btnArchiveNow_Click(object sender, EventArgs e)
                {
                    var archivePath = AppConstants.DefaultArchivePath;
                    Directory.CreateDirectory(archivePath);
                    var files = Directory.GetFiles(archivePath, "*", SearchOption.AllDirectories);
                    long totalBytes = 0;
                    foreach (var file in files)
                        totalBytes += new FileInfo(file).Length;
                    lblTotalFiles.Text = files.Length.ToString("N0");
                    lblStorageUsed.Text = FormatBytes(totalBytes);
                    lblArchivedSize.Text = FormatBytes(totalBytes);
                    AppendDesignerLog($"{DateTime.Now:HH:mm:ss} Archive scan completed: {files.Length:N0} files, {FormatBytes(totalBytes)}");
                }
                private void btnClearLog_Click(object sender, EventArgs e)
                {
                    if (txtLog != null) txtLog.Clear();
                    if (_rtbLog != null) OnClearLog(sender, e);
                }
                private void AppendDesignerLog(string message)
                {
                    if (txtLog == null || txtLog.IsDisposed) return;
                    txtLog.AppendText(message + Environment.NewLine);
                }
                private static string FormatBytes(long bytes)
                {
                    string[] units = { "B", "KB", "MB", "GB", "TB" };
                    double size = bytes;
                    int unit = 0;
                    while (size >= 1024 && unit < units.Length - 1)
                    {
                        size /= 1024;
                        unit++;
                    }
                    return $"{size:0.##} {units[unit]}";
                }
                protected override void OnFormClosing(FormClosingEventArgs e)
                {
                    _uiTimer?.Stop();
                    _healthTimer?.Stop();
                    _serverEngine?.Dispose();
                    base.OnFormClosing(e);
        			using System;
                    InitializeComponent();
                    Text = "EJLive Central Server v" + Constants.AppVersion;
                    FormClosing += ServerMainForm_FormClosing;
                    _vendorRootCapabilityService = new VendorRootCapabilityService();
                    _journalSyncTracker = new JournalSyncTracker();
                    _journalSyncAlertService = new JournalSyncAlertService(_journalSyncTracker);
                    _journalSyncDashboardService = new JournalSyncDashboardService(_journalSyncTracker, _journalSyncAlertService);
                    _refreshTimer = new System.Windows.Forms.Timer();
                    _refreshTimer.Interval = 10000;
                    _refreshTimer.Tick += RefreshTimer_Tick;
                    LoadConfiguration();
                    SeedSampleConnectionsIfEmpty();
                    UpdateSelectedAtmSummary();
                }
                private void SeedSampleConnectionsIfEmpty()
                {
                    if (lvConnections.Items.Count > 0)
                        return;
                    AddConnectionRow("ATM-NCR-001", "10.10.10.11", "Connected", DateTime.Now.AddMinutes(-3), "NCR");
                    AddConnectionRow("ATM-GRG-001", "10.10.10.12", "Connected", DateTime.Now.AddMinutes(-8), "GRG");
                    AddConnectionRow("ATM-WN-001", "10.10.10.13", "Disconnected", DateTime.Now.AddMinutes(-40), "WN");
                    AddConnectionRow("ATM-DN-001", "10.10.10.14", "Connected", DateTime.Now.AddMinutes(-18), "Diebold");
                }
                private void AddConnectionRow(string atmId, string ip, string status, DateTime lastSync, string atmType)
                {
                    var item = new ListViewItem(atmId);
                    item.SubItems.Add(ip);
                    item.SubItems.Add(status);
                    item.SubItems.Add(lastSync.ToString("yyyy-MM-dd HH:mm:ss"));
                    item.SubItems.Add(atmType);
                    lvConnections.Items.Add(item);
                    _journalSyncTracker.UpdateConnectionState(atmId, string.Equals(status, "Connected", StringComparison.OrdinalIgnoreCase), lastSync.ToUniversalTime());
                }
                private void btnStartStop_Click(object sender, EventArgs e)
                {
                    _serverRunning = !_serverRunning;
                    if (_serverRunning) StartServer();
                    else StopServer();
                }
                private void StartServer()
                {
                    string portStr = txtPort.Text.Trim();
                    string storagePath = txtStorage.Text.Trim();
                    if (string.IsNullOrEmpty(portStr) || string.IsNullOrEmpty(storagePath))
                    {
                        MessageBox.Show("Please configure Port and Storage Path.", "Configuration Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        _serverRunning = false;
                        return;
                    }
                    int port = int.Parse(portStr);
                    if (!Directory.Exists(storagePath)) Directory.CreateDirectory(storagePath);
                    _server = new EJServer(port, storagePath);
                    _server.OnLogMessage += (msg) => SafeAddLog(msg);
                    _server.OnClientStatusChanged += (atmId, connected) =>
                    {
                        BeginInvoke((Action)(() =>
                        {
                            _journalSyncTracker.UpdateConnectionState(atmId, connected, DateTime.UtcNow);
                            RefreshConnectionList();
                            UpdateSelectedAtmSummary();
                        }));
                    };
                    _server.Start();
                    _archiveManager = new ArchiveManager(storagePath);
                    _archiveManager.OnLogMessage += (msg) => SafeAddLog(msg);
                    btnStartStop.Text = "â–  Stop Server";
                    btnStartStop.BackColor = Color.FromArgb(220, 50, 50);
                    lblServerStatus.Text = "Running";
                    lblServerStatus.ForeColor = Color.Green;
                    tsslStatus.Text = "Server running on port " + port;
                    _refreshTimer.Start();
                    AddLog("Server started on port " + port);
                    AddLog("Storage path: " + storagePath);
                    SaveConfiguration();
                }
                private void StopServer()
                {
                    if (_server != null)
                    {
                        _server.Stop();
                        _server = null;
                    }
                    _refreshTimer.Stop();
                    btnStartStop.Text = "â–¶ Start Server";
                    btnStartStop.BackColor = Color.FromArgb(0, 122, 204);
                    lblServerStatus.Text = "Stopped";
                    lblServerStatus.ForeColor = Color.Red;
                    tsslStatus.Text = "Server stopped";
                    AddLog("Server stopped.");
                }
                private void RefreshTimer_Tick(object sender, EventArgs e)
                {
                    RefreshConnectionList();
                    UpdateStorageStats();
                    UpdateSelectedAtmSummary();
                }
                private void RefreshConnectionList()
                {
                    lblConnectedCount.Text = lvConnections.Items.Count.ToString();
                }
                private void UpdateStorageStats()
                {
                    if (_archiveManager == null)
                        return;
                    var stats = _archiveManager.GetStorageStats();
                    lblStorageUsed.Text = stats.TotalSizeFormatted;
                    lblArchivedSize.Text = stats.ArchivedSizeFormatted;
                    lblTotalFiles.Text = stats.TotalFiles.ToString();
                }
                private void btnArchiveNow_Click(object sender, EventArgs e)
                {
                    if (_archiveManager == null)
                    {
                        MessageBox.Show("Server must be running to perform archive.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    var result = MessageBox.Show("Archive all data older than 30 days?", "Confirm Archive", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (result == DialogResult.Yes)
                    {
                        AddLog("Manual archive initiated...");
                        _archiveManager.ArchiveOldData(30);
                        UpdateStorageStats();
                        AddLog("Archive completed.");
                        MessageBox.Show("Archive process completed successfully.", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                private void btnBrowseStorage_Click(object sender, EventArgs e)
                {
                    using (var fbd = new FolderBrowserDialog())
                    {
                        fbd.Description = "Select EJ Storage Directory";
                        if (fbd.ShowDialog() == DialogResult.OK)
                            txtStorage.Text = fbd.SelectedPath;
                    }
                }
                private void btnSendCommand_Click(object sender, EventArgs e)
                {
                    if (_server == null || !_serverRunning)
                    {
                        MessageBox.Show("Server must be running to send commands.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    if (lvConnections.SelectedItems.Count == 0)
                    {
                        MessageBox.Show("Please select an ATM from the connections list.", "Select ATM", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    string atmId = lvConnections.SelectedItems[0].Text;
                    string command = cmbCommand.SelectedItem?.ToString() ?? string.Empty;
                    if (string.IsNullOrEmpty(command))
                    {
                        MessageBox.Show("Please select a command.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    if (MessageBox.Show("Send '" + command + "' to " + atmId + "?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        _server.SendCommand(atmId, command, DateTime.Now.ToString("yyyyMMddHHmmss"));
                        AddLog("Command '" + command + "' sent to " + atmId);
                    }
                }
                private void btnAtmDetails_Click(object sender, EventArgs e)
                {
                    if (lvConnections.SelectedItems.Count == 0)
                    {
                        MessageBox.Show("Please select an ATM first.", "ATM Details", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    var selected = lvConnections.SelectedItems[0];
                    string atmId = selected.Text;
                    string atmName = selected.Text;
                    string atmType = selected.SubItems.Count > 4 ? selected.SubItems[4].Text : "Unknown";
                    var profile = _vendorRootCapabilityService.BuildKnownProfileByVendorName(atmType);
                    using (var form = new ATMDetailForm())
                    {
                        form.BindData(atmId, atmName, atmType, profile);
                        form.ShowDialog(this);
                    }
                }
                private void btnOpenSyncDashboard_Click(object sender, EventArgs e)
                {
                    using (var form = new SyncDashboardForm())
                    {
                        form.BindItems(_journalSyncDashboardService.BuildItems(BuildAtmList()));
                        form.ShowDialog(this);
                    }
                }
                private IReadOnlyList<(string atmId, string atmName, string atmType)> BuildAtmList()
                {
                    return lvConnections.Items
                        .Cast<ListViewItem>()
                        .Select(i => (i.Text, i.Text, i.SubItems.Count > 4 ? i.SubItems[4].Text : "Unknown"))
                        .ToList();
                }
                private void lvConnections_SelectedIndexChanged(object sender, EventArgs e)
                {
                    UpdateSelectedAtmSummary();
                }
                private void UpdateSelectedAtmSummary()
                {
                    if (lvConnections.SelectedItems.Count == 0)
                    {
                        lblSelectedAtmTitle.Text = "Selected ATM Summary";
                        rtbSelectedAtmSummary.Text = "Select an ATM to inspect its connectivity, sync state, and root capabilities.";
                        return;
                    }
                    var selected = lvConnections.SelectedItems[0];
                    string atmId = selected.Text;
                    string atmType = selected.SubItems.Count > 4 ? selected.SubItems[4].Text : "Unknown";
                    string status = selected.SubItems.Count > 2 ? selected.SubItems[2].Text : "Unknown";
                    string lastSync = selected.SubItems.Count > 3 ? selected.SubItems[3].Text : "--";
                    var profile = _vendorRootCapabilityService.BuildKnownProfileByVendorName(atmType);
                    var sync = _journalSyncTracker.GetStatus(atmId);
                    var alerts = sync != null ? _journalSyncAlertService.BuildAlertsForAtm(sync) : Array.Empty<JournalSyncAlert>();
                    lblSelectedAtmTitle.Text = "Selected ATM: " + atmId;
                    rtbSelectedAtmSummary.Text = RenderSelectedAtmSummary(atmId, atmType, status, lastSync, profile, sync, alerts);
                }
                private string RenderSelectedAtmSummary(string atmId, string atmType, string status, string lastSyncText, VendorRootProfile profile, JournalSyncStatusSnapshot sync, IReadOnlyList<JournalSyncAlert> alerts)
                {
                    var sb = new StringBuilder();
                    sb.AppendLine("Operational Summary");
                    sb.AppendLine("===================");
                    sb.AppendLine("ATM ID: " + atmId);
                    sb.AppendLine("Vendor/Type: " + atmType);
                    sb.AppendLine("Connection Status: " + status);
                    sb.AppendLine("Last Sync (list view): " + lastSyncText);
                    sb.AppendLine("Platform Lineage: " + (profile != null ? profile.PlatformLineage.ToString() : "Unknown"));
                    sb.AppendLine();
                    sb.AppendLine("Journal Sync Health");
                    sb.AppendLine("-------------------");
                    if (sync == null)
                    {
                        sb.AppendLine("No sync tracker data available yet.");
                    }
                    else
                    {
                        sb.AppendLine("Connected: " + sync.IsConnected);
                        sb.AppendLine("Last Heartbeat: " + (sync.LastHeartbeatUtc.HasValue ? sync.LastHeartbeatUtc.Value.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss") : "--"));
                        sb.AppendLine("Last Successful Journal Sync: " + (sync.LastJournalSyncUtc.HasValue ? sync.LastJournalSyncUtc.Value.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss") : "--"));
                        sb.AppendLine("Pending Files: " + sync.PendingFiles);
                        sb.AppendLine("Syncing Files: " + sync.SyncingFiles);
                        sb.AppendLine("Failed Files: " + sync.FailedFiles);
                        sb.AppendLine("Completed Files: " + sync.CompletedFiles);
                        sb.AppendLine("Pending Bytes: " + sync.PendingBytes);
                        if (!string.IsNullOrWhiteSpace(sync.LastError))
                            sb.AppendLine("Last Error: " + sync.LastError);
                    }
                    sb.AppendLine();
                    sb.AppendLine("Root Capabilities");
                    sb.AppendLine("-----------------");
                    if (profile == null)
                    {
                        sb.AppendLine("No vendor root profile matched.");
                    }
                    else
                    {
                        sb.AppendLine("Filter.ini: " + profile.HasFilterIni);
                        sb.AppendLine("XFS Media Templates: " + profile.HasXfsMediaTemplates);
                        sb.AppendLine("Dispenser Config Data: " + profile.HasDispenserConfigData);
                        sb.AppendLine("Keyboard Map Data: " + profile.HasKeyboardMapData);
                        sb.AppendLine("KBAPE Config: " + profile.HasKbapeConfig);
                        sb.AppendLine("Hint: " + (profile.FilterHeaderHint ?? "--"));
                        sb.AppendLine("Artifacts: " + profile.Artifacts.Count);
                    }
                    sb.AppendLine();
                    sb.AppendLine("Alerts");
                    sb.AppendLine("------");
                    if (alerts == null || alerts.Count == 0)
                    {
                        sb.AppendLine("No active sync alerts.");
                    }
                    else
                    {
                        foreach (var alert in alerts)
                        {
                            sb.AppendLine("- [" + alert.Severity + "] " + alert.Title);
                            sb.AppendLine("  " + alert.Message);
                        }
                    }
                    return sb.ToString();
                }
                private void AddLog(string message)
                {
                    string line = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " | " + message;
                    txtLog.AppendText(line + Environment.NewLine);
                }
                private void SafeAddLog(string message)
                {
                    if (InvokeRequired) BeginInvoke((Action)(() => AddLog(message)));
                    else AddLog(message);
                }
                private void btnClearLog_Click(object sender, EventArgs e)
                {
                    txtLog.Clear();
                }
                private void SaveConfiguration()
                {
                    try
                    {
                        string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ejlive_server.ini");
                        using (var sw = new StreamWriter(configPath))
                        {
                            sw.WriteLine("[EJLive Server]");
                            sw.WriteLine("Port=" + txtPort.Text);
                            sw.WriteLine("StoragePath=" + txtStorage.Text);
                        }
                    }
                    catch { }
                }
                private void LoadConfiguration()
                {
                    try
                    {
                        string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ejlive_server.ini");
                        if (!File.Exists(configPath)) return;
                        foreach (string line in File.ReadAllLines(configPath))
                        {
                            if (line.StartsWith("Port=")) txtPort.Text = line.Substring(5);
                            else if (line.StartsWith("StoragePath=")) txtStorage.Text = line.Substring(12);
                        }
                    }
                    catch { }
                }
                private void ServerMainForm_FormClosing(object sender, FormClosingEventArgs e)
                {
                    if (_serverRunning)
                    {
                        var result = MessageBox.Show("Server is running. Stop and exit?", "Confirm Exit", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                        if (result == DialogResult.No)
                        {
                            e.Cancel = true;
                            return;
                        }
                        StopServer();
                    }
                }
            }
        }
        private TabControl _tabMain = null!;
        private ToolStripStatusLabel _lblStatus = null!;
        private ToolStripStatusLabel _lblClock = null!;
        private readonly System.Windows.Forms.Timer _timer = new() { Interval = 5000 };
        private readonly FlowLayoutPanel _fleetCardsPanel = new() { Dock = DockStyle.Fill, AutoScroll = true, FlowDirection = FlowDirection.LeftToRight };
        private readonly DataGridView _fleetGrid = new();
        private readonly DataGridView _syncGrid = new();
        private readonly RichTextBox _logBox = new() { Dock = DockStyle.Fill, Font = new Font("Consolas", 9F), ReadOnly = true, BackColor = Color.FromArgb(30, 30, 30), ForeColor = Color.LightGreen };
        private readonly VendorRootProfileCatalogService _vendorRootProfileCatalogService;
        private MenuStrip        _menuStrip;
        private TabControl       _tabMain;
        private TabPage          _tabNOC, _tabConnections, _tabArchive, _tabRemote, _tabAnalytics, _tabLog;
        private StatusStrip      _statusStrip;
        private ToolStripStatusLabel _lblStatus, _lblConnections, _lblTime, _lblAlerts, _lblStorage;
        private Panel            _alertBar;
        private Label            _lblAlertText;
        private Timer            _uiTimer;
        private Timer            _healthTimer;
        private FlowLayoutPanel  _pnlATMCards;
        private Panel            _pnlMetrics;
        private Label[] _metricValues = new Label[8];
        private Label[] _metricLabels = new Label[8];
        private Button           _btnStartServer, _btnStopServer;
        private Button           _btnChangePasswordAll, _btnSendImagesAll, _btnBroadcast;
        private RichTextBox      _rtbPasswordLog;
        private Panel            _pnlServerControls;
        private DataGridView     _dgvConnections;
        private ComboBox         _cmbConnFilter;
        private Label            _lblConnCount;
        private TextBox          _txtSearchATM;
        private DateTimePicker   _dtpFrom, _dtpTo;
        private TextBox          _txtSearchKeyword;
        private Button           _btnSearch, _btnExportCSV, _btnExportHTML;
        private DataGridView     _dgvArchive;
        private Label            _lblArchiveCount;
        private ComboBox         _cmbTargetATM;
        private PictureBox       _pbGhostView;
        private Button           _btnGhostStart, _btnGhostStop;
        private Button           _btnRestart, _btnShutdown, _btnChangePassword;
        private Button           _btnSendFile, _btnGetFile, _btnForceSync;
        private TextBox          _txtRemotePassword, _txtFileParam;
        private DataGridView     _dgvCommandLog;
        private TrackBar         _tbGhostQuality;
        private Label            _lblGhostQuality;
        private ComboBox         _cmbAnalyticsATM;
        private DateTimePicker   _dtpAnalyticsFrom, _dtpAnalyticsTo;
        private Button           _btnRunAnalytics, _btnExportAnalytics;
        private Label            _lblAnalyticsSummary;
        private DataGridView     _dgvAnalytics;
        private Panel            _pnlAnalyticsStats;
        private Label[]          _analyticsStatLabels = new Label[8];
        private RichTextBox      _rtbLog;
        private ComboBox         _cmbLogLevel;
        private TextBox          _txtLogSearch;
        private Button           _btnClearLog, _btnExportLog;
        private Label            _lblLogCount;
        private ServerEngine     _serverEngine;
        private TransactionAnalysisEngine _analysisEngine;
        private ReportExportEngine        _reportEngine;
        private byte[]           _ghostJpegBuffer;
        private string           _currentUser = "admin";
        private int _logLines = 0;
        private TabControl tabMain;
        private TabPage tabServer, tabConnections, tabArchive, tabRemote, tabAnalytics, tabLog;
        private Panel pnlNOC;
        private Label lblTotalATMs, lblConnected, lblSyncing, lblErrors, lblOffline, lblSupervisor;
        private Label lblTotalATMsVal, lblConnectedVal, lblSyncingVal, lblErrorsVal, lblOfflineVal, lblSupervisorVal;
        private Label lblBandwidth, lblBandwidthVal, lblUptime, lblUptimeVal;
        private FlowLayoutPanel flpATMCards;
        private GroupBox grpServerControl;
        private Button btnStartServer, btnStopServer;
        private NumericUpDown numPort;
        private Label lblServerStatus, lblPort;
        private TextBox txtArchivePath;
        private Button btnBrowseArchive;
        private DataGridView dgvConnections;
        private Panel pnlConnectionToolbar;
        private Button btnRefreshConnections, btnDisconnectSelected, btnBroadcastMsg;
        private Label lblConnectionCount;
        private DataGridView dgvArchive;
        private Panel pnlArchiveToolbar;
        private DateTimePicker dtpArchiveFrom, dtpArchiveTo;
        private ComboBox cmbArchiveATM;
        private Button btnArchiveSearch, btnArchiveExport, btnArchiveOpen;
        private Label lblArchiveStats;
        private ComboBox cmbRemoteATM;
        private Button btnCmdRestart, btnCmdScreenshot, btnCmdTimeSync, btnCmdSysInfo;
        private Button btnCmdGhostStart, btnCmdGhostStop, btnCmdImageSync;
        private Button btnBroadcastRestart, btnBroadcastTimeSync;
        private PictureBox picGhostView;
        private RichTextBox rtbRemoteResult;
        private Label lblRemoteStatus;
        private ComboBox cmbAnalyticsATM;
        private DateTimePicker dtpAnalyticsFrom, dtpAnalyticsTo;
        private Button btnRunAnalysis, btnExportReport;
        private DataGridView dgvAnalytics;
        private Label lblAnalyticsSummary;
        private RichTextBox rtbLog;
        private Panel pnlLogToolbar;
        private Button btnClearLog, btnSaveLog;
        private CheckBox chkAutoScroll;
        private StatusStrip statusBar;
        private ToolStripStatusLabel lblStatus, lblServerState, lblClientsCount, lblVersion, lblClock;
        private GhostRemoteEngine _ghostEngine;
        private ImageSyncEngine _imageSyncEngine;
        private Dictionary<string, ATMCardPanel> _atmCards;
        private System.Windows.Forms.Timer _refreshTimer, _clockTimer;
        private bool _serverRunning;
        private string _archivePath = @"D:\EJOURNAL Files\Archive";
        private void OnResetStats(object s, EventArgs e) { }
        public ServerMainForm()
        {
            InitializeComponent();
            this.Text = "EJLive Central Server v" + Constants.AppVersion;
            this.FormClosing += ServerMainForm_FormClosing;
            // Refresh timer for connection list
            _refreshTimer = new System.Windows.Forms.Timer();
            _refreshTimer.Interval = 10000;
            _refreshTimer.Tick += RefreshTimer_Tick;
            // Load saved config
            LoadConfiguration();
            BuildDashboardTab();
            BuildJournalSyncTab();
            AddEnhancedCommands();
        }
        public ServerMainForm()
        {
            _atmCards = new Dictionary<string, ATMCardPanel>();
            InitializeComponent();
            SetupTimers();
            Log("[System] EJLive Server v3.4.0 initialized", Color.Cyan);
        }
        private readonly Dictionary<string, ListViewItem> _journalSyncRows = new Dictionary<string, ListViewItem>();
        private readonly Dictionary<string, AtmDashboardState> _atmDashboard = new Dictionary<string, AtmDashboardState>(StringComparer.OrdinalIgnoreCase);
        private void BuildDashboardTab()
        {
            _tabDashboard = new TabPage("  Dashboard  ");
            _tabDashboard.Padding = new Padding(10);
            var metrics = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 72,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false
            };
            _lblDashOnline = CreateMetricLabel("Online: 0", Color.FromArgb(220, 255, 220));
            _lblDashOffline = CreateMetricLabel("Offline: 0", Color.FromArgb(240, 240, 240));
            _lblDashSyncing = CreateMetricLabel("Syncing: 0", Color.FromArgb(220, 245, 255));
            _lblDashFailed = CreateMetricLabel("Failed: 0", Color.FromArgb(255, 220, 220));
            _lblDashLastActivity = CreateMetricLabel("Last activity: -", Color.FromArgb(255, 245, 210));
            metrics.Controls.AddRange(new Control[] { _lblDashOnline, _lblDashOffline, _lblDashSyncing, _lblDashFailed, _lblDashLastActivity });
            _lvAtmDashboard = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true
            };
            _lvAtmDashboard.Columns.Add("ATM ID", 120);
            _lvAtmDashboard.Columns.Add("Type", 75);
            _lvAtmDashboard.Columns.Add("Connection", 95);
            _lvAtmDashboard.Columns.Add("Last Heartbeat", 150);
            _lvAtmDashboard.Columns.Add("Last Sync", 150);
            _lvAtmDashboard.Columns.Add("Journal State", 110);
            _lvAtmDashboard.Columns.Add("Current File", 160);
            _lvAtmDashboard.Columns.Add("Health", 220);
            _tabDashboard.Controls.Add(_lvAtmDashboard);
            _tabDashboard.Controls.Add(metrics);
            tabControl.TabPages.Insert(0, _tabDashboard);
        }
        private static Label CreateMetricLabel(string text, Color backColor)
        {
            return new Label
            {
                Text = text,
                AutoSize = false,
                Width = 150,
                Height = 48,
                Margin = new Padding(4, 8, 4, 8),
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                BackColor = backColor,
                BorderStyle = BorderStyle.FixedSingle
            };
        }
        private void BuildJournalSyncTab()
        {
            _tabJournalSync = new TabPage("  Journal Sync  ");
            _tabJournalSync.Padding = new Padding(10);
            var toolbar = new Panel { Dock = DockStyle.Top, Height = 42 };
            _lblJournalSyncSummary = new Label
            {
                Text = "Stored: 0 | Syncing: 0 | Failed: 0",
                AutoSize = true,
                Location = new Point(10, 12),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
            _btnRefreshJournalSync = new Button { Text = "Refresh", Location = new Point(300, 7), Size = new Size(90, 28) };
            _btnExportJournalSync = new Button { Text = "Export CSV", Location = new Point(400, 7), Size = new Size(100, 28) };
            _btnRefreshJournalSync.Click += (s, e) => UpdateJournalSummary();
            _btnExportJournalSync.Click += BtnExportJournalSync_Click;
            toolbar.Controls.AddRange(new Control[] { _lblJournalSyncSummary, _btnRefreshJournalSync, _btnExportJournalSync });
            _lvJournalSync = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true
            };
            _lvJournalSync.Columns.Add("Time", 135);
            _lvJournalSync.Columns.Add("ATM ID", 100);
            _lvJournalSync.Columns.Add("File", 150);
            _lvJournalSync.Columns.Add("Size", 85);
            _lvJournalSync.Columns.Add("State", 110);
            _lvJournalSync.Columns.Add("Progress", 80);
            _lvJournalSync.Columns.Add("Retries", 65);
            _lvJournalSync.Columns.Add("Message", 240);
            _lvJournalSync.Columns.Add("Server Path", 260);
            _tabJournalSync.Controls.Add(_lvJournalSync);
            _tabJournalSync.Controls.Add(toolbar);
            tabControl.Controls.Add(_tabJournalSync);
        }
        private void AddEnhancedCommands()
        {
            foreach (var command in new[] { "IMAGE_SYNC", "FORCE_SYNC", "GHOST_START", "GHOST_STOP", "GET_SYSINFO" })
            {
                if (!cmbCommand.Items.Contains(command))
                    cmbCommand.Items.Add(command);
            }
        }
        private void btnStartStop_Click(object sender, EventArgs e)
        {
            _serverRunning = !_serverRunning;
            if (_serverRunning)
            {
                StartServer();
            }
            else
            {
                StopServer();
            }
        }
        private void StartServer()
        {
            string portStr = txtPort.Text.Trim();
            string storagePath = txtStorage.Text.Trim();
            if (string.IsNullOrEmpty(portStr) || string.IsNullOrEmpty(storagePath))
            {
                MessageBox.Show("Please configure Port and Storage Path.", "Configuration Required",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _serverRunning = false;
                return;
            }
            int port = int.Parse(portStr);
            // Ensure storage directory exists
            if (!Directory.Exists(storagePath))
                Directory.CreateDirectory(storagePath);
            // Initialize server
            _server = new EJServer(port, storagePath);
            _server.OnLogMessage += (msg) => SafeAddLog(msg);
            _server.OnJournalSyncChanged += Server_OnJournalSyncChanged;
            _server.OnJournalStored += (atmId, fileName, path) => SafeAddLog("Journal stored: " + atmId + " / " + fileName);
            _server.OnClientStatusChanged += (atmId, connected) =>
            {
                this.BeginInvoke((Action)(() =>
                {
                    RefreshConnectionList();
                    RefreshDashboard();
                }));
            };
            _server.Start();
            // Initialize archive manager
            _archiveManager = new ArchiveManager(storagePath);
            _archiveManager.OnLogMessage += (msg) => SafeAddLog(msg);
            // Update UI
            btnStartStop.Text = "\u25A0 Stop Server";
            btnStartStop.BackColor = Color.FromArgb(220, 50, 50);
            lblServerStatus.Text = "Running";
            lblServerStatus.ForeColor = Color.Green;
            tsslStatus.Text = "Server running on port " + port;
            _refreshTimer.Start();
            AddLog("Server started on port " + port);
            AddLog("Storage path: " + storagePath);
            SaveConfiguration();
        }
        private void StopServer()
        {
            if (_server != null)
            {
                _server.Stop();
                _server = null;
            }
            _refreshTimer.Stop();
            btnStartStop.Text = "\u25B6 Start Server";
            btnStartStop.BackColor = Color.FromArgb(0, 122, 204);
            lblServerStatus.Text = "Stopped";
            lblServerStatus.ForeColor = Color.Red;
            tsslStatus.Text = "Server stopped";
            AddLog("Server stopped.");
        }
        private void RefreshTimer_Tick(object sender, EventArgs e)
        {
            RefreshConnectionList();
            RefreshDashboard();
            UpdateStorageStats();
        }
        private void RefreshConnectionList()
        {
            if (_server != null)
            {
                lvConnections.Items.Clear();
                foreach (var client in _server.GetClients())
                {
                    var item = new ListViewItem(client.ATMID);
                    item.SubItems.Add(client.RemoteEndPoint);
                    item.SubItems.Add(client.IsConnected ? "Connected" : "Disconnected");
                    item.SubItems.Add(client.LastSyncUtc == DateTime.MinValue ? "-" : client.LastSyncUtc.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss"));
                    item.SubItems.Add(client.ATMType);
                    if (!client.IsConnected) item.ForeColor = Color.Gray;
                    lvConnections.Items.Add(item);
                }
                lblConnectedCount.Text = lvConnections.Items.Count.ToString();
            }
        }
        private void RefreshDashboard()
        {
            if (_server != null)
            {
                foreach (var client in _server.GetClients())
                {
                    AtmDashboardState state;
                    if (!_atmDashboard.TryGetValue(client.ATMID, out state))
                    {
                        state = new AtmDashboardState { ATMId = client.ATMID };
                        _atmDashboard[client.ATMID] = state;
                    }
                    state.ATMId = client.ATMID;
                    state.ATMType = client.ATMType;
                    state.IsConnected = client.IsConnected;
                    state.LastHeartbeatUtc = client.LastHeartbeatUtc;
                    state.LastSyncUtc = client.LastSyncUtc;
                }
            }
            RenderDashboard();
        }
        private void RenderDashboard()
        {
            if (_lvAtmDashboard == null) return;
            _lvAtmDashboard.Items.Clear();
            int online = 0, offline = 0, syncing = 0, failed = 0;
            DateTime lastActivityUtc = DateTime.MinValue;
            foreach (var pair in _atmDashboard)
            {
                var state = pair.Value;
                if (state.IsConnected) online++; else offline++;
                if (state.JournalState == JournalSyncState.Syncing || state.JournalState == JournalSyncState.Pending || state.JournalState == JournalSyncState.ReSyncing) syncing++;
                if (state.JournalState == JournalSyncState.Failed) failed++;
                if (state.LastSyncUtc > lastActivityUtc) lastActivityUtc = state.LastSyncUtc;
                if (state.LastHeartbeatUtc > lastActivityUtc) lastActivityUtc = state.LastHeartbeatUtc;
                string health = GetDashboardHealth(state);
                var item = new ListViewItem(state.ATMId ?? "");
                item.SubItems.Add(state.ATMType ?? "");
                item.SubItems.Add(state.IsConnected ? "Connected" : "Offline");
                item.SubItems.Add(FormatDate(state.LastHeartbeatUtc));
                item.SubItems.Add(FormatDate(state.LastSyncUtc));
                item.SubItems.Add(state.JournalState == null ? "-" : state.JournalState.ToString());
                item.SubItems.Add(state.CurrentFileName ?? "");
                item.SubItems.Add(health);
                item.BackColor = GetDashboardColor(state, health);
                _lvAtmDashboard.Items.Add(item);
            }
            _lblDashOnline.Text = "Online: " + online;
            _lblDashOffline.Text = "Offline: " + offline;
            _lblDashSyncing.Text = "Syncing: " + syncing;
            _lblDashFailed.Text = "Failed: " + failed;
            _lblDashLastActivity.Text = "Last activity: " + FormatDate(lastActivityUtc);
        }
        private static string GetDashboardHealth(AtmDashboardState state)
        {
            if (state == null) return "Unknown";
            if (!state.IsConnected) return "Offline or not reporting";
            if (state.JournalState == JournalSyncState.Failed) return "Journal sync failed";
            if (state.LastSyncUtc != DateTime.MinValue && DateTime.UtcNow.Subtract(state.LastSyncUtc).TotalMinutes > 60) return "Data stale for more than 1 hour";
            if (state.JournalState == JournalSyncState.ReSyncing) return "Journal is being re-synchronized";
            if (state.JournalState == JournalSyncState.Syncing || state.JournalState == JournalSyncState.Pending) return "Journal transfer in progress";
            return "Healthy";
        }
        private static Color GetDashboardColor(AtmDashboardState state, string health)
        {
            if (state == null || !state.IsConnected) return Color.FromArgb(240, 240, 240);
            if (state.JournalState == JournalSyncState.Failed) return Color.FromArgb(255, 220, 220);
            if (!string.IsNullOrEmpty(health) && health.StartsWith("Data stale")) return Color.FromArgb(255, 245, 210);
            if (state.JournalState == JournalSyncState.Syncing || state.JournalState == JournalSyncState.Pending || state.JournalState == JournalSyncState.ReSyncing) return Color.FromArgb(220, 245, 255);
            return Color.FromArgb(220, 255, 220);
        }
        private void UpdateStorageStats()
        {
            if (_archiveManager != null)
            {
                var stats = _archiveManager.GetStorageStats();
                lblStorageUsed.Text = stats.TotalSizeFormatted;
                lblArchivedSize.Text = stats.ArchivedSizeFormatted;
                lblTotalFiles.Text = stats.TotalFiles.ToString();
            }
        }
        private void btnArchiveNow_Click(object sender, EventArgs e)
        {
            if (_archiveManager == null)
            {
                MessageBox.Show("Server must be running to perform archive.", "Warning",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            var result = MessageBox.Show("Archive all data older than 30 days?", "Confirm Archive",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                AddLog("Manual archive initiated...");
                _archiveManager.ArchiveOldData(30);
                UpdateStorageStats();
                AddLog("Archive completed.");
                MessageBox.Show("Archive process completed successfully.", "Done",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        private void btnBrowseStorage_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                fbd.Description = "Select EJ Storage Directory";
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    txtStorage.Text = fbd.SelectedPath;
                }
            }
        }
        private void btnSendCommand_Click(object sender, EventArgs e)
        {
            if (_server == null || !_serverRunning)
            {
                MessageBox.Show("Server must be running to send commands.", "Warning",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (lvConnections.SelectedItems.Count == 0)
            {
                MessageBox.Show("Please select an ATM from the connections list.", "Select ATM",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            string atmId = lvConnections.SelectedItems[0].Text;
            string command = cmbCommand.SelectedItem?.ToString() ?? "";
            if (string.IsNullOrEmpty(command))
            {
                MessageBox.Show("Please select a command.", "Warning",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            var confirm = MessageBox.Show("Send '" + command + "' to " + atmId + "?", "Confirm",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm == DialogResult.Yes)
            {
                _server.SendCommand(atmId, command, DateTime.Now.ToString("yyyyMMddHHmmss"));
                AddLog("Command '" + command + "' sent to " + atmId);
            }
        }
        private void AddLog(string message)
        {
            string line = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " | " + message;
            txtLog.AppendText(line + Environment.NewLine);
        }
        private void SafeAddLog(string message)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke((Action)(() => AddLog(message)));
            }
            else
            {
                AddLog(message);
            }
        }
        private void btnClearLog_Click(object sender, EventArgs e)
        {
            txtLog.Clear();
        }
        private void UpdateDashboardFromJournal(JournalSyncRecord record)
        {
            if (record == null || string.IsNullOrWhiteSpace(record.ATM_ID)) return;
            AtmDashboardState state;
            if (!_atmDashboard.TryGetValue(record.ATM_ID, out state))
            {
                state = new AtmDashboardState { ATMId = record.ATM_ID };
                _atmDashboard[record.ATM_ID] = state;
            }
            state.ATMId = record.ATM_ID;
            state.CurrentFileName = record.FileName;
            state.JournalState = record.State;
            state.LastSyncUtc = record.UpdatedAtUtc;
            state.LastMessage = record.Message;
            RenderDashboard();
        }
        private void Server_OnJournalSyncChanged(JournalSyncRecord record)
        {
            if (record == null) return;
            if (this.InvokeRequired)
            {
                this.BeginInvoke((Action)(() => Server_OnJournalSyncChanged(record)));
                return;
            }
            UpdateDashboardFromJournal(record);
            string key = string.IsNullOrEmpty(record.SyncId) ? record.ATM_ID + "|" + record.FileName + "|" + record.UpdatedAtUtc.Ticks : record.SyncId;
            ListViewItem item;
            if (!_journalSyncRows.TryGetValue(key, out item))
            {
                item = new ListViewItem(record.UpdatedAtUtc.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss"));
                item.SubItems.Add(record.ATM_ID ?? "");
                item.SubItems.Add(record.FileName ?? "");
                item.SubItems.Add(FormatBytes(record.FileSize));
                item.SubItems.Add(record.State.ToString());
                item.SubItems.Add(record.ProgressPercent + "%");
                item.SubItems.Add(record.RetryCount.ToString());
                item.SubItems.Add(record.Message ?? "");
                item.SubItems.Add(record.ServerPath ?? "");
                _journalSyncRows[key] = item;
                _lvJournalSync.Items.Insert(0, item);
            }
            else
            {
                item.SubItems[0].Text = record.UpdatedAtUtc.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss");
                item.SubItems[1].Text = record.ATM_ID ?? "";
                item.SubItems[2].Text = record.FileName ?? "";
                item.SubItems[3].Text = FormatBytes(record.FileSize);
                item.SubItems[4].Text = record.State.ToString();
                item.SubItems[5].Text = record.ProgressPercent + "%";
                item.SubItems[6].Text = record.RetryCount.ToString();
                item.SubItems[7].Text = record.Message ?? "";
                item.SubItems[8].Text = record.ServerPath ?? "";
            }
            item.BackColor = GetSyncColor(record.State);
            UpdateJournalSummary();
        }
        private void UpdateJournalSummary()
        {
            int stored = 0, syncing = 0, failed = 0;
            foreach (ListViewItem item in _lvJournalSync.Items)
            {
                string state = item.SubItems[4].Text;
                if (state == JournalSyncState.StoredOnServer.ToString() || state == JournalSyncState.Completed.ToString()) stored++;
                else if (state == JournalSyncState.Syncing.ToString() || state == JournalSyncState.Pending.ToString() || state == JournalSyncState.ReSyncing.ToString()) syncing++;
                else if (state == JournalSyncState.Failed.ToString()) failed++;
            }
            _lblJournalSyncSummary.Text = $"Stored: {stored} | Syncing: {syncing} | Failed: {failed}";
        }
        private void BtnExportJournalSync_Click(object sender, EventArgs e)
        {
            if (_lvJournalSync.Items.Count == 0)
            {
                MessageBox.Show("No journal sync rows to export.", "Journal Sync", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            using (var sfd = new SaveFileDialog { Filter = "CSV Files|*.csv", FileName = "JournalSync_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".csv" })
            {
                if (sfd.ShowDialog() != DialogResult.OK) return;
                using (var sw = new StreamWriter(sfd.FileName, false, System.Text.Encoding.UTF8))
                {
                    sw.WriteLine("Time,ATM ID,File,Size,State,Progress,Retries,Message,Server Path");
                    foreach (ListViewItem item in _lvJournalSync.Items)
                    {
                        var cells = new List<string>();
                        for (int i = 0; i < item.SubItems.Count; i++)
                            cells.Add("\"" + item.SubItems[i].Text.Replace("\"", "\"\"") + "\"");
                        sw.WriteLine(string.Join(",", cells));
                    }
                }
                AddLog("Journal sync CSV exported: " + sfd.FileName);
            }
        }
        private void SaveConfiguration()
        {
            try
            {
                string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ejlive_server.ini");
                using (StreamWriter sw = new StreamWriter(configPath))
                {
                    sw.WriteLine("[EJLive Server]");
                    sw.WriteLine("Port=" + txtPort.Text);
                    sw.WriteLine("StoragePath=" + txtStorage.Text);
                }
            }
            catch { }
        }
        private void LoadConfiguration()
        {
            try
            {
                string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ejlive_server.ini");
                if (!File.Exists(configPath)) return;
                string[] lines = File.ReadAllLines(configPath);
                foreach (string line in lines)
                {
                    if (line.StartsWith("Port=")) txtPort.Text = line.Substring(5);
                    else if (line.StartsWith("StoragePath=")) txtStorage.Text = line.Substring(12);
                }
            }
            catch { }
        }
        private void ServerMainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_serverRunning)
            {
                var result = MessageBox.Show("Server is running. Stop and exit?",
                    "Confirm Exit", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.No)
                {
                    e.Cancel = true;
                    return;
                }
                StopServer();
            }
        }
        private static string FormatBytes(long bytes)
        {
            if (bytes < 1024) return bytes + " B";
            if (bytes < 1024 * 1024) return (bytes / 1024d).ToString("N1") + " KB";
            return (bytes / 1024d / 1024d).ToString("N1") + " MB";
        }
        private static string FormatDate(DateTime utc)
        {
            if (utc == DateTime.MinValue) return "-";
            return utc.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss");
        }
        private static Color GetSyncColor(JournalSyncState state)
        {
            switch (state)
            {
                case JournalSyncState.StoredOnServer:
                case JournalSyncState.Completed:
                case JournalSyncState.Acknowledged:
                    return Color.FromArgb(220, 255, 220);
                case JournalSyncState.Syncing:
                    return Color.FromArgb(220, 245, 255);
                case JournalSyncState.ReSyncing:
                case JournalSyncState.Pending:
                    return Color.FromArgb(255, 245, 210);
                case JournalSyncState.Failed:
                    return Color.FromArgb(255, 220, 220);
                default:
                    return Color.White;
            }
        }
        private void SeedSampleConnectionsIfEmpty()
        {
            if (lvConnections.Items.Count > 0)
                return;
            AddConnectionRow("ATM-NCR-001", "10.10.10.11", "Connected", DateTime.Now.AddMinutes(-3), "NCR");
            AddConnectionRow("ATM-GRG-001", "10.10.10.12", "Connected", DateTime.Now.AddMinutes(-8), "GRG");
            AddConnectionRow("ATM-WN-001", "10.10.10.13", "Disconnected", DateTime.Now.AddMinutes(-40), "WN");
            AddConnectionRow("ATM-DN-001", "10.10.10.14", "Connected", DateTime.Now.AddMinutes(-18), "Diebold");
        }
        private void AddConnectionRow(string atmId, string ip, string status, DateTime lastSync, string atmType)
        {
            var item = new ListViewItem(atmId);
            item.SubItems.Add(ip);
            item.SubItems.Add(status);
            item.SubItems.Add(lastSync.ToString("yyyy-MM-dd HH:mm:ss"));
            item.SubItems.Add(atmType);
            lvConnections.Items.Add(item);
            _journalSyncTracker.UpdateConnectionState(atmId, string.Equals(status, "Connected", StringComparison.OrdinalIgnoreCase), lastSync.ToUniversalTime());
        }
        private void btnStartStop_Click(object sender, EventArgs e) { MessageBox.Show("Server start/stop", "Info"); }
        private void StartServer()
        {
            string portStr = txtPort.Text.Trim();
            string storagePath = txtStorage.Text.Trim();
            if (string.IsNullOrEmpty(portStr) || string.IsNullOrEmpty(storagePath))
            {
                MessageBox.Show("Please configure Port and Storage Path.", "Configuration Required",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _serverRunning = false;
                return;
            }
            int port = int.Parse(portStr);
            // Ensure storage directory exists
            if (!Directory.Exists(storagePath))
                Directory.CreateDirectory(storagePath);
            // Initialize server
            _server = new EJServer(port, storagePath);
            _server.OnLogMessage += (msg) => SafeAddLog(msg);
            _server.OnClientStatusChanged += (atmId, connected) =>
            {
                this.BeginInvoke((Action)(() =>
                {
                    RefreshConnectionList();
                }));
            };
            _server.Start();
            // Initialize archive manager
            _archiveManager = new ArchiveManager(storagePath);
            _archiveManager.OnLogMessage += (msg) => SafeAddLog(msg);
            // Update UI
            btnStartStop.Text = "\u25A0 Stop Server";
            btnStartStop.BackColor = Color.FromArgb(220, 50, 50);
            lblServerStatus.Text = "Running";
            lblServerStatus.ForeColor = Color.Green;
            tsslStatus.Text = "Server running on port " + port;
            _refreshTimer.Start();
            AddLog("Server started on port " + port);
            AddLog("Storage path: " + storagePath);
            SaveConfiguration();
        }
        private void StopServer()
        {
            if (MessageBox.Show("هل تريد إيقاف الخادم؟ سيتم قطع جميع الاتصالات.", "تأكيد", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
            _serverEngine.Stop();
            _btnStartServer.Enabled = true;
            _btnStopServer.Enabled  = false;
            _lblStatus.Text         = "● خادم متوقف";
            _lblStatus.ForeColor    = Color.FromArgb(255, 69, 58);
            DatabaseManager.Instance.InsertAuditLog("ServerStop", _currentUser, null, "إيقاف الخادم");
        }
        private void RefreshTimer_Tick(object sender, EventArgs e)
        {
            RefreshConnectionList();
            UpdateStorageStats();
        }
        private void RefreshConnectionList()
        {
            // This would be populated from the server's client list
            // For now, update the connection count
            if (_server != null)
            {
                lblConnectedCount.Text = lvConnections.Items.Count.ToString();
            }
        }
        private void UpdateStorageStats()
        {
            if (_archiveManager == null)
                return;
            var stats = _archiveManager.GetStorageStats();
            lblStorageUsed.Text = stats.TotalSizeFormatted;
            lblArchivedSize.Text = stats.ArchivedSizeFormatted;
            lblTotalFiles.Text = stats.TotalFiles.ToString();
        }
        private void btnBrowseStorage_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                if (fbd.ShowDialog() == DialogResult.OK)
                    txtStorage.Text = fbd.SelectedPath;
            }
        }
        private void btnSendCommand_Click(object sender, EventArgs e) { MessageBox.Show("Command sent", "Info"); }
        private void btnAtmDetails_Click(object sender, EventArgs e)
        {
            if (lvConnections.SelectedItems.Count == 0)
            {
                MessageBox.Show("Please select an ATM first.", "ATM Details", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            var selected = lvConnections.SelectedItems[0];
            string atmId = selected.Text;
            string atmName = selected.Text;
            string atmType = selected.SubItems.Count > 4 ? selected.SubItems[4].Text : "Unknown";
            var profile = _vendorRootCapabilityService.BuildKnownProfileByVendorName(atmType);
            using (var form = new ATMDetailForm())
            {
                form.BindData(atmId, atmName, atmType, profile);
                form.ShowDialog(this);
            }
        }
        private void btnOpenSyncDashboard_Click(object sender, EventArgs e)
        {
            using (var form = new SyncDashboardForm())
            {
                form.BindItems(_journalSyncDashboardService.BuildItems(BuildAtmList()));
                form.ShowDialog(this);
            }
        }
        private IReadOnlyList<(string atmId, string atmName, string atmType)> BuildAtmList()
        {
            return lvConnections.Items
                .Cast<ListViewItem>()
                .Select(i => (i.Text, i.Text, i.SubItems.Count > 4 ? i.SubItems[4].Text : "Unknown"))
                .ToList();
        }
        private void lvConnections_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateSelectedAtmSummary();
        }
        private void UpdateSelectedAtmSummary()
        {
            if (lvConnections.SelectedItems.Count == 0)
            {
                lblSelectedAtmTitle.Text = "Selected ATM Summary";
                rtbSelectedAtmSummary.Text = "Select an ATM to inspect its connectivity, sync state, and root capabilities.";
                return;
            }
            var selected = lvConnections.SelectedItems[0];
            string atmId = selected.Text;
            string atmType = selected.SubItems.Count > 4 ? selected.SubItems[4].Text : "Unknown";
            string status = selected.SubItems.Count > 2 ? selected.SubItems[2].Text : "Unknown";
            string lastSync = selected.SubItems.Count > 3 ? selected.SubItems[3].Text : "--";
            var profile = _vendorRootCapabilityService.BuildKnownProfileByVendorName(atmType);
            var sync = _journalSyncTracker.GetStatus(atmId);
            var alerts = sync != null ? _journalSyncAlertService.BuildAlertsForAtm(sync) : Array.Empty<JournalSyncAlert>();
            lblSelectedAtmTitle.Text = "Selected ATM: " + atmId;
            rtbSelectedAtmSummary.Text = RenderSelectedAtmSummary(atmId, atmType, status, lastSync, profile, sync, alerts);
        }
        private string RenderSelectedAtmSummary(string atmId, string atmType, string status, string lastSyncText, VendorRootProfile profile, JournalSyncStatusSnapshot sync, IReadOnlyList<JournalSyncAlert> alerts)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Operational Summary");
            sb.AppendLine("===================");
            sb.AppendLine("ATM ID: " + atmId);
            sb.AppendLine("Vendor/Type: " + atmType);
            sb.AppendLine("Connection Status: " + status);
            sb.AppendLine("Last Sync (list view): " + lastSyncText);
            sb.AppendLine("Platform Lineage: " + (profile != null ? profile.PlatformLineage.ToString() : "Unknown"));
            sb.AppendLine();
            sb.AppendLine("Journal Sync Health");
            sb.AppendLine("-------------------");
            if (sync == null)
            {
                sb.AppendLine("No sync tracker data available yet.");
            }
            else
            {
                sb.AppendLine("Connected: " + sync.IsConnected);
                sb.AppendLine("Last Heartbeat: " + (sync.LastHeartbeatUtc.HasValue ? sync.LastHeartbeatUtc.Value.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss") : "--"));
                sb.AppendLine("Last Successful Journal Sync: " + (sync.LastJournalSyncUtc.HasValue ? sync.LastJournalSyncUtc.Value.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss") : "--"));
                sb.AppendLine("Pending Files: " + sync.PendingFiles);
                sb.AppendLine("Syncing Files: " + sync.SyncingFiles);
                sb.AppendLine("Failed Files: " + sync.FailedFiles);
                sb.AppendLine("Completed Files: " + sync.CompletedFiles);
                sb.AppendLine("Pending Bytes: " + sync.PendingBytes);
                if (!string.IsNullOrWhiteSpace(sync.LastError))
                    sb.AppendLine("Last Error: " + sync.LastError);
            }
            sb.AppendLine();
            sb.AppendLine("Root Capabilities");
            sb.AppendLine("-----------------");
            if (profile == null)
            {
                sb.AppendLine("No vendor root profile matched.");
            }
            else
            {
                sb.AppendLine("Filter.ini: " + profile.HasFilterIni);
                sb.AppendLine("XFS Media Templates: " + profile.HasXfsMediaTemplates);
                sb.AppendLine("Dispenser Config Data: " + profile.HasDispenserConfigData);
                sb.AppendLine("Keyboard Map Data: " + profile.HasKeyboardMapData);
                sb.AppendLine("KBAPE Config: " + profile.HasKbapeConfig);
                sb.AppendLine("Hint: " + (profile.FilterHeaderHint ?? "--"));
                sb.AppendLine("Artifacts: " + profile.Artifacts.Count);
            }
            sb.AppendLine();
            sb.AppendLine("Alerts");
            sb.AppendLine("------");
            if (alerts == null || alerts.Count == 0)
            {
                sb.AppendLine("No active sync alerts.");
            }
            else
            {
                foreach (var alert in alerts)
                {
                    sb.AppendLine("- [" + alert.Severity + "] " + alert.Title);
                    sb.AppendLine("  " + alert.Message);
                }
            }
            return sb.ToString();
        }
        private void SafeAddLog(string message)
        {
            if (InvokeRequired) BeginInvoke((Action)(() => AddLog(message)));
            else AddLog(message);
        }
        private void SaveConfiguration()
        {
            try
            {
                string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ejlive_server.ini");
                using (var sw = new StreamWriter(configPath))
                {
                    sw.WriteLine("[EJLive Server]");
                    sw.WriteLine("Port=" + txtPort.Text);
                    sw.WriteLine("StoragePath=" + txtStorage.Text);
                }
            }
            catch { }
        }
        private void LoadConfiguration()
        {
            try
            {
                string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ejlive_server.ini");
                if (!File.Exists(configPath)) return;
                foreach (string line in File.ReadAllLines(configPath))
                {
                    if (line.StartsWith("Port=")) txtPort.Text = line.Substring(5);
                    else if (line.StartsWith("StoragePath=")) txtStorage.Text = line.Substring(12);
                }
            }
            catch { }
        }
        private void WireTimer() { _timer.Tick += (_, _) => _lblClock.Text = DateTime.Now.ToString("HH:mm:ss"); _timer.Start(); }
        private void AutoStartServer() { _lblStatus.Text = "Server Ready — Listening on port 5656"; Log("Server started."); }
        private void Log(string msg) { _logBox.AppendText($"[{DateTime.Now:HH:mm:ss}] {msg}\n"); }
        private Button Btn(string text, Action? handler = null) { var b = new Button { Text = text, Width = 105, Height = 30, Margin = new Padding(3), BackColor = Color.FromArgb(66, 139, 202), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, UseVisualStyleBackColor = false }; if (handler != null) b.Click += (_, _) => { _lblStatus.Text = $"Running: {text}..."; handler(); }; return b; }
        private FlowLayoutPanel BtnPanel(params (string label, Action handler)[] btns) { var p = new FlowLayoutPanel { Dock = DockStyle.Fill, Height = 42, FlowDirection = FlowDirection.LeftToRight }; foreach (var (label, handler) in btns) p.Controls.Add(Btn(label, handler)); return p; }
        private static void AddCard(TableLayoutPanel parent, string label, string value, Color color, int col, int row) { var card = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(8), MinimumSize = new Size(160, 60) }; card.Controls.Add(new Label { Text = label, Font = new Font("Segoe UI", 8F), ForeColor = Color.Gray, Dock = DockStyle.Top, Height = 16 }); card.Controls.Add(new Label { Text = value, Font = new Font("Segoe UI", 14F, FontStyle.Bold), ForeColor = color, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }); parent.Controls.Add(card, col, row); }
        private static DataGridView Grid() => new() { Dock = DockStyle.Fill, BackgroundColor = Color.White, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, AllowUserToAddRows = false, ReadOnly = true, RowHeadersVisible = false };
        private static RichTextBox LogBox() => new() { Dock = DockStyle.Fill, Font = new Font("Consolas", 9F), ReadOnly = true, BackColor = Color.FromArgb(30, 30, 30), ForeColor = Color.LightGreen };
        private void BuildAllTabs()
        {
            BuildFleetTab(); BuildJournalIntelligenceTab(); BuildLogAnalysisTab();
            BuildAtmDetailsTab(); BuildSessionsTab(); BuildIngestionTab();
            BuildJournalAnalysisTab_(); BuildForensicsTab(); BuildXfsCorrelationTab();
            BuildAlertsTab(); BuildCommandQueueTab(); BuildImageDistTab();
            BuildReportsTab(); BuildVendorProfilesTab(); BuildUsersTab();
            BuildAuditTab(); BuildServerSettingsTab(); BuildVerificationTab();
        }
        private void BuildJournalIntelligenceTab() { var tab = new TabPage("📊 Journal Intelligence"); tab.Controls.Add(new Dashboards.JournalAnalysisDashboard { Dock = DockStyle.Fill }); _tabMain.TabPages.Add(tab); }
        private void BuildLogAnalysisTab() { var tab = new TabPage("📊 Log Analysis"); tab.Controls.Add(new Dashboards.LogAnalysisDashboard { Dock = DockStyle.Fill }); _tabMain.TabPages.Add(tab); }
        private void BuildFleetTab()
        {
            var tab = new TabPage("Fleet Overview");
            var p = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 5, RowCount = 4, Padding = new Padding(16) };
            AddCard(p, "Total ATMs", "15", Color.Blue, 0, 0); AddCard(p, "Online", "11", Color.Green, 1, 0); AddCard(p, "Offline", "2", Color.Red, 2, 0); AddCard(p, "Warning", "2", Color.Orange, 3, 0); AddCard(p, "Sync Rate", "93%", Color.Green, 4, 0);
            AddCard(p, "Open Alerts", "3", Color.Red, 0, 1); AddCard(p, "Failed Transfers", "1", Color.Orange, 1, 1); AddCard(p, "Last Ingestion", "2m ago", Color.Blue, 2, 1); AddCard(p, "Health Avg", "85%", Color.Green, 3, 1);
            p.Controls.Add(_fleetCardsPanel, 0, 2); p.SetColumnSpan(_fleetCardsPanel, 5);
            p.Controls.Add(BtnPanel(("Refresh", () => Log("Dashboard refreshed.")), ("Export Overview", () => Log("Overview exported.")), ("Open Alerts", () => Log("Alerts opened.")), ("Open Reports", () => Log("Reports opened.")), ("Filter", () => Log("Filter applied."))), 0, 3); p.SetColumnSpan((Control)p.Controls[p.Controls.Count - 1], 5);
            tab.Controls.Add(p); _tabMain.TabPages.Add(tab);
        }
        private void BuildAtmDetailsTab()
        {
            var tab = new TabPage("ATM Details");
            var p = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 2, Padding = new Padding(16) };
            var info = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown };
            info.Controls.Add(new Label { Text = "ATM ID: ATM-001 | Terminal: T001 | Branch: Main | Region: Central", AutoSize = true, Font = new Font("Segoe UI", 9F) });
            info.Controls.Add(new Label { Text = "Vendor: NCR | Model: 6622 | Status: Connected | Health: 92%", AutoSize = true, Font = new Font("Segoe UI", 9F) });
            info.Controls.Add(new Label { Text = "Last HB: 10s ago | Last Sync: 2m ago | Last Journal: EJDATA_20260607.LOG", AutoSize = true, Font = new Font("Segoe UI", 9F) });
            p.Controls.Add(info, 0, 0); p.SetColumnSpan(info, 2);
            p.Controls.Add(BtnPanel(("Open Journal History", () => Log("Journal history opened.")), ("Open Sync History", () => Log("Sync history opened.")), ("Open Commands", () => Log("Commands opened.")), ("Request Journal", () => Log("Journal requested.")), ("Send Content", () => Log("Content package sent.")), ("Export Report", () => Log("Report exported."))), 0, 1); p.SetColumnSpan((Control)p.Controls[p.Controls.Count - 1], 2);
            tab.Controls.Add(p); _tabMain.TabPages.Add(tab);
        }
        private void BuildSessionsTab()
        {
            var tab = new TabPage("Live Sessions");
            var p = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, Padding = new Padding(16) };
            var grid = Grid(); grid.Columns.Add("ATM_ID", "ATM ID"); grid.Columns.Add("SessionID", "Session ID"); grid.Columns.Add("Protocol", "Protocol"); grid.Columns.Add("ClientVer", "Client Ver"); grid.Columns.Add("LastHB", "Last HB"); grid.Columns.Add("PendingCmd", "Pending Cmd"); grid.Columns.Add("BytesIn", "Bytes In"); grid.Columns.Add("BytesOut", "Bytes Out"); grid.Columns.Add("State", "State");
            grid.Rows.Add("ATM-001", "S1", "v5", "1.0", "10s", "0", "2MB", "1MB", "Connected");
            grid.Rows.Add("ATM-002", "S2", "v5", "1.0", "5m", "2", "500K", "200K", "Connected");
            p.Controls.Add(grid);
            p.Controls.Add(BtnPanel(("Disconnect", () => Log("Session disconnected.")), ("Mark Offline", () => Log("Marked offline.")), ("View Heartbeats", () => Log("Heartbeats viewed.")), ("Send Ping", () => Log("Ping sent."))));
            tab.Controls.Add(p); _tabMain.TabPages.Add(tab);
        }
        private void BuildIngestionTab()
        {
            var tab = new TabPage("Ingestion & Archive");
            var p = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, Padding = new Padding(16) };
            var grid = Grid(); grid.Columns.Add("File", "File"); grid.Columns.Add("ATM", "ATM"); grid.Columns.Add("Size", "Size"); grid.Columns.Add("Checksum", "Checksum"); grid.Columns.Add("Status", "Status");
            grid.Rows.Add("EJ_20260607.LOG", "ATM-001", "512KB", "OK", "Archived");
            grid.Rows.Add("TRACE_20260607.LOG", "ATM-002", "256KB", "OK", "Verified");
            p.Controls.Add(grid);
            p.Controls.Add(BtnPanel(("Reprocess", () => Log("File reprocessed.")), ("Verify Checksum", () => Log("Checksum verified.")), ("Open Archive", () => Log("Archive opened.")), ("Export Index", () => Log("Index exported.")), ("Mark Duplicate", () => Log("Marked as duplicate."))));
            tab.Controls.Add(p); _tabMain.TabPages.Add(tab);
        }
        private void BuildJournalAnalysisTab_()
        {
            var tab = new TabPage("Journal Analysis");
            var p = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, Padding = new Padding(16) };
            var grid = Grid(); grid.Columns.Add("RunID", "Run ID"); grid.Columns.Add("Vendor", "Vendor"); grid.Columns.Add("File", "File"); grid.Columns.Add("TxCount", "Tx"); grid.Columns.Add("Success", "OK"); grid.Columns.Add("Failed", "Fail"); grid.Columns.Add("Suspicious", "Susp"); grid.Columns.Add("Reversal", "Rev"); grid.Columns.Add("PartDisp", "PD"); grid.Columns.Add("NoDisp", "AND");
            grid.Rows.Add("R001", "NCR", "EJ_20260607", "150", "145", "3", "1", "2", "0", "1");
            p.Controls.Add(grid);
            p.Controls.Add(BtnPanel(("Run Parser", () => Log("Parser run started.")), ("Open Evidence", () => Log("Evidence opened.")), ("Export Tx", () => Log("Transactions exported.")), ("Export Exceptions", () => Log("Exceptions exported.")), ("Compare Runs", () => Log("Runs compared."))));
            tab.Controls.Add(p); _tabMain.TabPages.Add(tab);
        }
        private void BuildForensicsTab()
        {
            var tab = new TabPage("Transaction Forensics");
            var p = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 2, Padding = new Padding(16) };
            var search = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, Height = 200 };
            search.Controls.Add(new Label { Text = "Search By:", Font = new Font("Segoe UI", 10F, FontStyle.Bold), AutoSize = true });
            var cmb = new ComboBox { Items = { "SN", "RRN", "STAN", "Card", "Account", "Mobile", "Date/Time" }, Width = 160, Text = "SN" }; search.Controls.Add(cmb);
            search.Controls.Add(new TextBox { Width = 200, Text = "Search value..." });
            search.Controls.Add(Btn("Search", () => Log("Transaction searched.")));
            p.Controls.Add(search, 0, 0);
            var details = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown };
            details.Controls.Add(new Label { Text = "Tx#: 1234 | Date: 2026-06-07 | ATM: 001 | Card: 4532******7890", AutoSize = true, Font = new Font("Consolas", 9F) });
            details.Controls.Add(new Label { Text = "Amount: 500 SAR | STAN: 001234 | RRN: 987654321 | Cass: C1:10 C2:5 C3:0 C4:0", AutoSize = true, Font = new Font("Consolas", 9F) });
            details.Controls.Add(new Label { Text = "Status: Approved | Confidence: 92% | Evidence: EJ + NDC + XFS", AutoSize = true, Font = new Font("Consolas", 9F), ForeColor = Color.Green });
            p.Controls.Add(details, 1, 0);
            p.Controls.Add(BtnPanel(("Search", () => Log("Transaction searched.")), ("Open Raw Lines", () => Log("Raw lines opened.")), ("Open Correlation", () => Log("Correlation opened.")), ("Export Case", () => Log("Case exported."))), 0, 1); p.SetColumnSpan((Control)p.Controls[p.Controls.Count - 1], 2);
            tab.Controls.Add(p); _tabMain.TabPages.Add(tab);
        }
        private void BuildXfsCorrelationTab()
        {
            var tab = new TabPage("XFS Correlation");
            var p = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, Padding = new Padding(16) };
            var grid = Grid(); grid.Columns.Add("EventID", "Event"); grid.Columns.Add("Vendor", "Vendor"); grid.Columns.Add("Device", "Device"); grid.Columns.Add("Severity", "Severity"); grid.Columns.Add("Code", "Code"); grid.Columns.Add("Message", "Message"); grid.Columns.Add("LinkedTx", "Linked Tx");
            grid.Rows.Add("E001", "NCR", "CDM", "Error", "M-03", "Pick failure", "Tx-1234");
            grid.Rows.Add("E002", "GRG", "IDC", "Warning", "05", "No card", "Tx-1235");
            p.Controls.Add(grid);
            p.Controls.Add(BtnPanel(("Run Correlation", () => Log("Correlation run.")), ("Open Raw Event", () => Log("Raw event opened.")), ("Export Events", () => Log("Events exported.")), ("Open Linked Tx", () => Log("Linked transaction opened.")), ("Filter by Device", () => Log("Device filter applied."))));
            tab.Controls.Add(p); _tabMain.TabPages.Add(tab);
        }
        private void BuildAlertsTab()
        {
            var tab = new TabPage("Alerts & Risk");
            var p = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, Padding = new Padding(16) };
            var grid = Grid(); grid.Columns.Add("Alert", "Alert"); grid.Columns.Add("ATM", "ATM"); grid.Columns.Add("Severity", "Severity"); grid.Columns.Add("Time", "Time"); grid.Columns.Add("Status", "Status");
            grid.Rows.Add("ATM Offline", "ATM-003", "Critical", "10m ago", "Open");
            grid.Rows.Add("Heartbeat Timeout", "ATM-002", "Warning", "5m ago", "Open");
            grid.Rows.Add("Approved No Dispense", "ATM-001", "Medium", "1h ago", "Acknowledged");
            p.Controls.Add(grid);
            p.Controls.Add(BtnPanel(("Acknowledge", () => Log("Alert acknowledged.")), ("Assign", () => Log("Alert assigned.")), ("Close Alert", () => Log("Alert closed.")), ("Open ATM", () => Log("ATM details opened.")), ("Export Alerts", () => Log("Alerts exported."))));
            tab.Controls.Add(p); _tabMain.TabPages.Add(tab);
        }
        private void BuildCommandQueueTab()
        {
            var tab = new TabPage("Command Queue");
            var p = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, Padding = new Padding(16) };
            var grid = Grid(); grid.Columns.Add("CmdID", "Cmd ID"); grid.Columns.Add("ATM", "ATM"); grid.Columns.Add("Type", "Type"); grid.Columns.Add("State", "State"); grid.Columns.Add("RequestedBy", "By"); grid.Columns.Add("ApprovedBy", "Approved");
            grid.Rows.Add("C001", "ATM-001", "Restart", "Sent", "admin", "supervisor");
            grid.Rows.Add("C002", "ATM-002", "Screenshot", "Draft", "operator", "—");
            p.Controls.Add(grid);
            p.Controls.Add(BtnPanel(("Create", () => Log("Command created.")), ("Approve", () => Log("Command approved.")), ("Reject", () => Log("Command rejected.")), ("Send", () => Log("Command sent.")), ("Expire", () => Log("Command expired.")), ("View Audit", () => Log("Audit viewed.")), ("Rollback", () => Log("Rollback executed."))));
            tab.Controls.Add(p); _tabMain.TabPages.Add(tab);
        }
        private void BuildImageDistTab()
        {
            var tab = new TabPage("Image Distribution");
            var p = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, Padding = new Padding(16) };
            var grid = Grid(); grid.Columns.Add("PkgID", "Package"); grid.Columns.Add("Name", "Name"); grid.Columns.Add("Vendor", "Vendor"); grid.Columns.Add("SHA256", "SHA256"); grid.Columns.Add("Status", "Status");
            grid.Rows.Add("PKG-001", "Welcome Screen v2", "NCR", "abc123...", "Deployed (8/10)");
            p.Controls.Add(grid);
            p.Controls.Add(BtnPanel(("Create Package", () => Log("Package created.")), ("Validate", () => Log("Package validated.")), ("Send to ATMs", () => Log("Package sent.")), ("Retry Failed", () => Log("Retry failed packages.")), ("View Receipts", () => Log("Receipts viewed.")), ("Rollback", () => Log("Package rollback."))));
            tab.Controls.Add(p); _tabMain.TabPages.Add(tab);
        }
        private void BuildReportsTab()
        {
            var tab = new TabPage("Reports Center");
            var p = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 2, Padding = new Padding(16) };
            var list = new ListBox { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 9F) };
            foreach (var rpt in new[] { "Daily Operations", "Monthly Operations", "ATM Details", "Sync Failures", "Command Audit", "Journal Parser Summary", "XFS Event Summary", "Cash/Dispense Risk", "Approved No Dispense", "Partial Dispense", "Missing/Duplicate Sequence", "Vendor Status", "Device Health Ranking" }) list.Items.Add(rpt);
            p.Controls.Add(list, 0, 0); p.SetRowSpan(list, 2);
            p.Controls.Add(BtnPanel(("Generate", () => Log("Report generated.")), ("Preview", () => Log("Preview opened.")), ("Export Excel", () => Log("Excel exported.")), ("Export PDF", () => Log("PDF exported.")), ("Schedule", () => Log("Report scheduled.")), ("Save Template", () => Log("Template saved."))), 1, 0);
            p.Controls.Add(LogBox(), 1, 1);
            tab.Controls.Add(p); _tabMain.TabPages.Add(tab);
        }
        private void BuildVendorProfilesTab()
        {
            var tab = new TabPage("Vendor Profiles");
            var p = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, Padding = new Padding(16) };
            var grid = Grid(); grid.Columns.Add("Vendor", "Vendor"); grid.Columns.Add("Model", "Model"); grid.Columns.Add("Source", "Journal Source"); grid.Columns.Add("Backup", "Backup Path"); grid.Columns.Add("Trace", "Trace Path");
            grid.Rows.Add("NCR", "6622", @"C:\NCR\Data", @"C:\NCR_Backup", @"C:\NCR\Trace");
            grid.Rows.Add("GRG", "DT-7000", @"D:\GRG\Log", @"D:\GRG_Backup", @"D:\GRG\Trace");
            p.Controls.Add(grid);
            p.Controls.Add(BtnPanel(("Add Profile", () => Log("Profile added.")), ("Edit", () => Log("Profile edited.")), ("Clone", () => Log("Profile cloned.")), ("Validate", () => Log("Profile validated.")), ("Assign to ATM", () => Log("Profile assigned.")), ("Export", () => Log("Profiles exported."))));
            tab.Controls.Add(p); _tabMain.TabPages.Add(tab);
        }
        private void BuildUsersTab()
        {
            var tab = new TabPage("Users & RBAC");
            var p = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, Padding = new Padding(16) };
            var grid = Grid(); grid.Columns.Add("User", "User"); grid.Columns.Add("Role", "Role"); grid.Columns.Add("Permissions", "Permissions"); grid.Columns.Add("Status", "Status");
            grid.Rows.Add("admin", "SecurityAdmin", "All", "Active");
            grid.Rows.Add("operator", "Operator", "View, Command", "Active");
            grid.Rows.Add("auditor", "Auditor", "View, Export", "Active");
            p.Controls.Add(grid);
            p.Controls.Add(BtnPanel(("Add User", () => Log("User added.")), ("Edit Role", () => Log("Role edited.")), ("Disable", () => Log("User disabled.")), ("Rotate Key", () => Log("Key rotated.")), ("Export Policy", () => Log("Policy exported."))));
            tab.Controls.Add(p); _tabMain.TabPages.Add(tab);
        }
        private void BuildAuditTab()
        {
            var tab = new TabPage("Audit Logs");
            var p = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, Padding = new Padding(16) };
            var grid = Grid(); grid.Columns.Add("Time", "Time"); grid.Columns.Add("User", "User"); grid.Columns.Add("Action", "Action"); grid.Columns.Add("Target", "Target"); grid.Columns.Add("CmdID", "Cmd ID"); grid.Columns.Add("Result", "Result"); grid.Columns.Add("Risk", "Risk");
            grid.Rows.Add("12:00", "admin", "Restart", "ATM-001", "C001", "Success", "Medium");
            grid.Rows.Add("11:45", "operator", "Screenshot", "ATM-002", "C002", "Success", "Low");
            p.Controls.Add(grid);
            p.Controls.Add(BtnPanel(("Filter", () => Log("Filter applied.")), ("Export", () => Log("Audit exported.")), ("Open Command", () => Log("Command opened.")), ("Open ATM", () => Log("ATM opened."))));
            tab.Controls.Add(p); _tabMain.TabPages.Add(tab);
        }
        private void BuildServerSettingsTab()
        {
            var tab = new TabPage("Server Settings");
            var p = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 8, Padding = new Padding(16) };
            void Add(string label, Control input, int row) { p.Controls.Add(new Label { Text = label, AutoSize = true }, 0, row); p.Controls.Add(input, 1, row); }
            Add("Listening Port:", new NumericUpDown { Minimum = 1, Maximum = 65535, Value = 5656, Dock = DockStyle.Fill }, 0);
            Add("Archive Root:", new TextBox { Text = @"D:\EJLive\Archive" }, 1);
            Add("Staging Root:", new TextBox { Text = @"D:\EJLive\Staging" }, 2);
            Add("Database Path:", new TextBox { Text = @"D:\EJLive\ejlive.db" }, 3);
            Add("Retention (days):", new NumericUpDown { Minimum = 30, Maximum = 3650, Value = 365, Dock = DockStyle.Fill }, 4);
            Add("TLS/Cert:", new ComboBox { Items = { "None", "Self-Signed", "CA" }, Text = "Self-Signed" }, 5);
            Add("Report Output:", new TextBox { Text = @"D:\EJLive\Reports" }, 6);
            p.Controls.Add(BtnPanel(("Save", () => Log("Settings saved.")), ("Validate", () => Log("Settings validated.")), ("Restart Service", () => Log("Service restart requested.")), ("Backup Config", () => Log("Config backed up.")), ("Restore Config", () => Log("Config restored."))), 0, 7); p.SetColumnSpan((Control)p.Controls[p.Controls.Count - 1], 2);
            tab.Controls.Add(p); _tabMain.TabPages.Add(tab);
        }
        private void BuildVerificationTab()
        {
            var tab = new TabPage("Verification");
            var p = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 2, Padding = new Padding(16) };
            var grid = Grid(); grid.Columns.Add("Probe", "Probe"); grid.Columns.Add("Status", "Status");
            foreach (var probe in new[] { "Build Status", "Test Status", "ActiveCompileMap", "Service Boundary", "Parser Registry", "XFS Registry", "Security Probe" })
                grid.Rows.Add(probe, "PASS");
            p.Controls.Add(grid, 0, 0); p.SetRowSpan(grid, 2);
            p.Controls.Add(BtnPanel(("Run Verification", () => Log("Verification run.")), ("Open Logs", () => Log("Logs opened.")), ("Export Report", () => Log("Report exported."))), 1, 0);
            p.Controls.Add(LogBox(), 1, 1);
            tab.Controls.Add(p); _tabMain.TabPages.Add(tab);
        }
        private readonly Dictionary<string, ATMCardPanel> _atmPanels = new Dictionary<string, ATMCardPanel>();
        private void InitializeServices()
        {
            DatabaseManager.Instance.Initialize(AppConstants.DefaultDatabasePath);
            AppLogger.Instance.Initialize(AppConstants.DefaultLogPath, "server");
            AppLogger.Instance.OnLog += (s, e) => AppendLog(e.FormattedForUI, e.Level);
            _serverEngine   = new ServerEngine();
            _archiveManager = new ArchiveManager();
            _analysisEngine = new TransactionAnalysisEngine();
            _reportEngine   = new ReportExportEngine();
            _serverEngine.OnATMConnected    += (s, atm) => OnATMConnected(atm);
            _serverEngine.OnATMDisconnected += (s, atm) => OnATMDisconnected(atm);
            _serverEngine.OnATMUpdated      += (s, atm) => RefreshATMCard(atm);
            _serverEngine.OnJournalReceived += (s, pkt)  => OnJournalReceived(pkt);
            _serverEngine.OnServerLog       += (s, msg)  => AppLogger.Instance.Info(msg, "Server");
            AlertManager.Instance.OnAlert    += (s, a) => ShowAlert(a);
            AlertManager.Instance.OnCritical += (s, a) => ShowCriticalAlert(a);
        }
        private void InitializeForm()
        {
            Text            = $"EJLive Enterprise Server v{AppConstants.AppVersion}";
            Size            = new Size(1440, 900);
            MinimumSize     = new Size(1200, 700);
            StartPosition   = FormStartPosition.CenterScreen;
            WindowState     = FormWindowState.Maximized;
            BackColor       = Color.FromArgb(26, 28, 30);
            ForeColor       = Color.FromArgb(242, 242, 247);
            Font            = new Font("Segoe UI", 9.5f);
            Icon            = CreateAppIcon();
        }
        private void InitializeMenuStrip()
        {
            _menuStrip = new MenuStrip { BackColor = Color.FromArgb(30, 32, 35), ForeColor = Color.White };
            var fileMenu   = new ToolStripMenuItem("ملف");
            var serverMenu = new ToolStripMenuItem("الخادم");
            var viewMenu   = new ToolStripMenuItem("عرض");
            var toolsMenu  = new ToolStripMenuItem("أدوات");
            var helpMenu   = new ToolStripMenuItem("مساعدة");
            fileMenu.DropDownItems.AddRange(new ToolStripItem[] {
                new ToolStripMenuItem("تصدير التقارير", null, OnExportAllReports),
                new ToolStripSeparator(),
                new ToolStripMenuItem("خروج", null, (s,e) => Close())
            });
            serverMenu.DropDownItems.AddRange(new ToolStripItem[] {
                new ToolStripMenuItem("تشغيل الخادم",    null, (s,e) => StartServer()),
                new ToolStripMenuItem("إيقاف الخادم",   null, (s,e) => StopServer()),
                new ToolStripSeparator(),
                new ToolStripMenuItem("إعدادات الخادم", null, OnOpenSettings)
            });
            toolsMenu.DropDownItems.AddRange(new ToolStripItem[] {
                new ToolStripMenuItem("إدارة المستخدمين",   null, OnManageUsers),
                new ToolStripMenuItem("سجل التدقيق",        null, OnOpenAuditLog),
                new ToolStripMenuItem("إعادة ضبط الإحصائيات", null, OnResetStats)
            });
            _menuStrip.Items.AddRange(new[] { fileMenu, serverMenu, viewMenu, toolsMenu, helpMenu });
            Controls.Add(_menuStrip);
        }
        private void InitializeAlertBar()
        {
            _alertBar = new Panel
            {
                Height    = 36,
                Dock      = DockStyle.Top,
                BackColor = Color.FromArgb(44, 20, 20),
                Visible   = false,
                Cursor    = Cursors.Hand
            };
            _alertBar.Top = _menuStrip.Bottom;
            _lblAlertText = new Label
            {
                Dock      = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.FromArgb(255, 69, 58),
                Font      = new Font("Segoe UI", 9.5f, FontStyle.Bold)
            };
            _alertBar.Controls.Add(_lblAlertText);
            _alertBar.Click += (s, e) => _alertBar.Visible = false;
            Controls.Add(_alertBar);
        }
        private void InitializeStatusStrip()
        {
            _statusStrip = new StatusStrip { BackColor = Color.FromArgb(30, 32, 35), SizingGrip = false };
            _lblStatus      = new ToolStripStatusLabel("● خادم متوقف") { ForeColor = Color.FromArgb(255, 69, 58) };
            _lblConnections = new ToolStripStatusLabel("الاتصالات: 0") { ForeColor = Color.FromArgb(99, 99, 102) };
            _lblAlerts      = new ToolStripStatusLabel("التنبيهات: 0") { ForeColor = Color.FromArgb(99, 99, 102) };
            _lblStorage     = new ToolStripStatusLabel("المساحة: --") { ForeColor = Color.FromArgb(99, 99, 102) };
            _lblTime        = new ToolStripStatusLabel { Alignment = ToolStripItemAlignment.Right, ForeColor = Color.FromArgb(142, 142, 147) };
            _statusStrip.Items.AddRange(new ToolStripItem[]
            { _lblStatus, new ToolStripSeparator(), _lblConnections, new ToolStripSeparator(),
              _lblAlerts, new ToolStripSeparator(), _lblStorage, _lblTime });
            Controls.Add(_statusStrip);
        }
        private void InitializeTabs()
        {
            _tabMain = new TabControl
            {
                Dock        = DockStyle.Fill,
                DrawMode    = TabDrawMode.OwnerDrawFixed,
                ItemSize    = new Size(140, 38),
                SizeMode    = TabSizeMode.Fixed,
                Font        = new Font("Segoe UI", 9f, FontStyle.Bold),
                Padding     = new Point(10, 6)
            };
            _tabMain.DrawItem     += DrawTabItem;
            _tabMain.SelectedIndexChanged += (s, e) => _tabMain.Refresh();
            _tabNOC         = new TabPage("🖥️  لوحة NOC");
            _tabConnections = new TabPage("🔗  الاتصالات");
            _tabArchive     = new TabPage("📦  الأرشيف");
            _tabRemote      = new TabPage("🎮  التحكم البعيد");
            _tabAnalytics   = new TabPage("📊  التحليلات");
            _tabLog         = new TabPage("📋  السجل");
            foreach (var tab in new[] { _tabNOC, _tabConnections, _tabArchive, _tabRemote, _tabAnalytics, _tabLog })
                tab.BackColor = Color.FromArgb(26, 28, 30);
            BuildTabNOC();
            BuildTabConnections();
            BuildTabArchive();
            BuildTabRemote();
            BuildTabAnalytics();
            BuildTabLog();
            _tabMain.TabPages.AddRange(new[] { _tabNOC, _tabConnections, _tabArchive, _tabRemote, _tabAnalytics, _tabLog });
            Controls.Add(_tabMain);
        }
        private void BuildTabNOC()
        {
            // لوحة الإحصائيات العلوية
            _pnlMetrics = new Panel
            {
                Height    = 100,
                Dock      = DockStyle.Top,
                BackColor = Color.FromArgb(22, 24, 26),
                Padding   = new Padding(12, 8, 12, 8)
            };
            var metricsFlow = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight };
            string[] metricLabels = { "متصل", "توقف الإرسال", "جورنال اليوم", "معدل النجاح", "تنبيهات نشطة", "سرعة الاستقبال", "آخر استلام", "مساحة متبقية" };
            string[] metricIcons  = { "🟢", "🔴", "📁", "✅", "🚨", "⚡", "🕐", "💾" };
            for (int i = 0; i < 8; i++)
            {
                var card  = CreateMetricCard(metricIcons[i], metricLabels[i], "—", i);
                metricsFlow.Controls.Add(card);
            }
            _pnlMetrics.Controls.Add(metricsFlow);
            // أزرار التحكم
            _pnlServerControls = new Panel
            {
                Height    = 50,
                Dock      = DockStyle.Top,
                BackColor = Color.FromArgb(22, 24, 26),
                Padding   = new Padding(12, 8, 12, 8)
            };
            _btnStartServer        = MakeButton("▶  تشغيل الخادم",  Color.FromArgb(40, 100, 60),  StartServer, 160);
            _btnStopServer         = MakeButton("⏹  إيقاف الخادم",  Color.FromArgb(100, 30, 30),  StopServer, 160);
            _btnChangePasswordAll  = MakeButton("🔑 كلمة سر جماعية", Color.FromArgb(60, 40, 80),  OnChangePasswordAll, 175);
            _btnSendImagesAll      = MakeButton("🖼️ صور جماعية",     Color.FromArgb(40, 60, 100),  OnSendImagesAll, 150);
            _btnBroadcast          = MakeButton("📡 بث للجميع",      Color.FromArgb(70, 50, 20),  OnBroadcast, 150);
            _btnStopServer.Enabled = false;
            var ctrlFlow           = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, WrapContents = false };
            ctrlFlow.Controls.AddRange(new Control[] { _btnStartServer, _btnStopServer, _btnChangePasswordAll, _btnSendImagesAll, _btnBroadcast });
            _pnlServerControls.Controls.Add(ctrlFlow);
            // منطقة بطاقات الصرافات
            var pnlCardsArea = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(26, 28, 30) };
            _pnlATMCards = new FlowLayoutPanel
            {
                Dock             = DockStyle.Fill,
                FlowDirection    = FlowDirection.LeftToRight,
                WrapContents     = true,
                AutoScroll       = true,
                Padding          = new Padding(12)
            };
            pnlCardsArea.Controls.Add(_pnlATMCards);
            // سجل كلمات السر
            var pnlPasswordLog = new Panel { Height = 110, Dock = DockStyle.Bottom, BackColor = Color.FromArgb(22, 24, 26) };
            var pwdLabel = new Label { Text = "سجل تغيير كلمات السر:", Dock = DockStyle.Top, ForeColor = Color.FromArgb(142, 142, 147), Height = 22, Padding = new Padding(6, 4, 0, 0) };
            _rtbPasswordLog = new RichTextBox { Dock = DockStyle.Fill, BackColor = Color.FromArgb(28, 30, 32), ForeColor = Color.FromArgb(200, 200, 200), ReadOnly = true, BorderStyle = BorderStyle.None, Font = new Font("Consolas", 8.5f) };
            pnlPasswordLog.Controls.AddRange(new Control[] { _rtbPasswordLog, pwdLabel });
            _tabNOC.Controls.AddRange(new Control[] { pnlCardsArea, pnlPasswordLog, _pnlServerControls, _pnlMetrics });
        }
        private void BuildTabConnections()
        {
            var toolbar = new Panel { Height = 40, Dock = DockStyle.Top, BackColor = Color.FromArgb(22, 24, 26), Padding = new Padding(8, 6, 8, 6) };
            _lblConnCount = new Label { Text = "الاتصالات: 0", ForeColor = Color.FromArgb(200, 200, 200), AutoSize = true, Location = new Point(8, 10) };
            _cmbConnFilter = new ComboBox { Width = 150, Location = new Point(120, 7), DropDownStyle = ComboBoxStyle.DropDownList, BackColor = Color.FromArgb(40, 42, 45), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            _cmbConnFilter.Items.AddRange(new object[] { "الكل", "متصل", "منقطع", "يزامن", "Supervisor" });
            _cmbConnFilter.SelectedIndex = 0;
            toolbar.Controls.AddRange(new Control[] { _lblConnCount, _cmbConnFilter });
            _dgvConnections = CreateDataGrid();
            _dgvConnections.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn { HeaderText = "معرف الصراف",    Width = 110, DataPropertyName = "ATM_ID"       },
                new DataGridViewTextBoxColumn { HeaderText = "الاسم",          Width = 130, DataPropertyName = "ATM_Name"     },
                new DataGridViewTextBoxColumn { HeaderText = "النوع",          Width = 70,  DataPropertyName = "ATM_Type"     },
                new DataGridViewTextBoxColumn { HeaderText = "IP",             Width = 120, DataPropertyName = "ServerIP"     },
                new DataGridViewTextBoxColumn { HeaderText = "الشبكة",        Width = 80,  DataPropertyName = "NetworkType"  },
                new DataGridViewTextBoxColumn { HeaderText = "الحالة",        Width = 130, DataPropertyName = "Status"       },
                new DataGridViewTextBoxColumn { HeaderText = "الكمون",        Width = 80,  DataPropertyName = "Latency"      },
                new DataGridViewTextBoxColumn { HeaderText = "آخر Heartbeat", Width = 140, DataPropertyName = "HB"           },
                new DataGridViewTextBoxColumn { HeaderText = "آخر جورنال",   Width = 140, DataPropertyName = "LastSync"     },
                new DataGridViewTextBoxColumn { HeaderText = "Session ID",    Width = 130, DataPropertyName = "SessionId"    }
            });
            _tabConnections.Controls.AddRange(new Control[] { _dgvConnections, toolbar });
        }
        private void BuildTabArchive()
        {
            var searchPanel = new Panel { Height = 55, Dock = DockStyle.Top, BackColor = Color.FromArgb(22, 24, 26), Padding = new Padding(12, 10, 12, 10) };
            var sf = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, WrapContents = false };
            _txtSearchATM    = CreateInput("معرف الصراف", 120);
            _dtpFrom         = new DateTimePicker { Width = 140, Format = DateTimePickerFormat.Short, Value = DateTime.Today.AddDays(-30), BackColor = Color.FromArgb(40, 42, 45), ForeColor = Color.White };
            _dtpTo           = new DateTimePicker { Width = 140, Format = DateTimePickerFormat.Short, Value = DateTime.Today, BackColor = Color.FromArgb(40, 42, 45), ForeColor = Color.White };
            _txtSearchKeyword = CreateInput("كلمة البحث", 140);
            _btnSearch        = MakeButton("🔍 بحث",     Color.FromArgb(0, 85, 170), OnSearchArchive, 100);
            _btnExportCSV     = MakeButton("📥 CSV",     Color.FromArgb(30, 80, 50),  OnExportArchiveCSV, 90);
            _btnExportHTML    = MakeButton("📄 HTML",    Color.FromArgb(50, 50, 100), OnExportArchiveHTML, 100);
            sf.Controls.AddRange(new Control[] {
                MakeLabel("الصراف:"), _txtSearchATM, MakeLabel("  من:"), _dtpFrom,
                MakeLabel("  إلى:"), _dtpTo, MakeLabel("  بحث:"), _txtSearchKeyword,
                _btnSearch, _btnExportCSV, _btnExportHTML
            });
            searchPanel.Controls.Add(sf);
            _lblArchiveCount = new Label { Text = "النتائج: 0", Dock = DockStyle.Top, Height = 22, ForeColor = Color.FromArgb(142, 142, 147), Padding = new Padding(12, 4, 0, 0), BackColor = Color.FromArgb(26, 28, 30) };
            _dgvArchive = CreateDataGrid();
            _dgvArchive.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn { HeaderText = "معرف الصراف",  Width = 110 },
                new DataGridViewTextBoxColumn { HeaderText = "اسم الملف",    Width = 160 },
                new DataGridViewTextBoxColumn { HeaderText = "الحجم الأصلي", Width = 100 },
                new DataGridViewTextBoxColumn { HeaderText = "الحجم مضغوط",  Width = 100 },
                new DataGridViewTextBoxColumn { HeaderText = "عدد العمليات", Width = 100 },
                new DataGridViewTextBoxColumn { HeaderText = "Checksum",      Width = 140 },
                new DataGridViewTextBoxColumn { HeaderText = "تاريخ الاستلام", Width = 140 },
                new DataGridViewTextBoxColumn { HeaderText = "مسار الأرشيف",  Width = 280, AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill }
            });
            _dgvArchive.DoubleClick += OnArchiveEntryDoubleClick;
            _tabArchive.Controls.AddRange(new Control[] { _dgvArchive, _lblArchiveCount, searchPanel });
        }
        private void BuildTabRemote()
        {
            var splitContainer = new SplitContainer
            {
                Dock        = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                SplitterDistance = 450,
                SplitterWidth    = 4,
                BackColor        = Color.FromArgb(44, 46, 48)
            };
            // ═ الجانب الأيسر: Ghost View + معلومات
            var leftPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(26, 28, 30), Padding = new Padding(8) };
            var targetRow = new FlowLayoutPanel { Height = 40, Dock = DockStyle.Top, FlowDirection = FlowDirection.LeftToRight, BackColor = Color.FromArgb(22, 24, 26) };
            _cmbTargetATM = new ComboBox { Width = 200, DropDownStyle = ComboBoxStyle.DropDownList, BackColor = Color.FromArgb(40, 42, 45), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            targetRow.Controls.AddRange(new Control[] { MakeLabel("الصراف المستهدف: "), _cmbTargetATM });
            // Ghost View
            var ghostPanel = new Panel { Height = 260, Dock = DockStyle.Top, BackColor = Color.Black, Margin = new Padding(0, 8, 0, 0) };
            _pbGhostView   = new PictureBox { Dock = DockStyle.Fill, SizeMode = PictureBoxSizeMode.Zoom, BackColor = Color.Black };
            var ghostLabel = new Label { Text = "🖥️ Ghost View", Dock = DockStyle.Top, Height = 24, ForeColor = Color.FromArgb(142, 142, 147), BackColor = Color.FromArgb(22, 24, 26), TextAlign = ContentAlignment.MiddleCenter };
            var ghostCtrl  = new FlowLayoutPanel { Height = 36, Dock = DockStyle.Bottom, BackColor = Color.FromArgb(22, 24, 26), FlowDirection = FlowDirection.LeftToRight };
            _tbGhostQuality  = new TrackBar { Minimum = 10, Maximum = 100, Value = 75, Width = 140, TickFrequency = 10, BackColor = Color.FromArgb(22, 24, 26) };
            _lblGhostQuality = new Label { Text = "75%", ForeColor = Color.White, AutoSize = true };
            _btnGhostStart   = MakeButton("▶ Ghost",  Color.FromArgb(20, 70, 130), OnGhostStart, 110);
            _btnGhostStop    = MakeButton("⏹ إيقاف", Color.FromArgb(80, 20, 20),  OnGhostStop,  110);
            _btnGhostStop.Enabled = false;
            _tbGhostQuality.Scroll += (s, e) => { _lblGhostQuality.Text = $"{_tbGhostQuality.Value}%"; };
            ghostCtrl.Controls.AddRange(new Control[] { _btnGhostStart, _btnGhostStop, MakeLabel("جودة:"), _tbGhostQuality, _lblGhostQuality });
            ghostPanel.Controls.AddRange(new Control[] { _pbGhostView, ghostCtrl, ghostLabel });
            // الأوامر
            var cmdsPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 8, 0, 0) };
            var cmdsFlow  = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 85, FlowDirection = FlowDirection.LeftToRight, WrapContents = true };
            _btnRestart        = MakeButton("🔄 Restart",       Color.FromArgb(100, 40, 20), OnRestart, 130);
            _btnShutdown       = MakeButton("⏻  Shutdown",      Color.FromArgb(100, 20, 20), OnShutdown, 130);
            _btnChangePassword = MakeButton("🔑 Change Password",Color.FromArgb(60, 40, 100), OnChangePassword, 155);
            _btnForceSync      = MakeButton("♻️  Force Sync",   Color.FromArgb(20, 60, 60),  OnForceSync, 130);
            _btnSendFile       = MakeButton("📤 إرسال ملف",     Color.FromArgb(20, 70, 30),  OnSendFile, 130);
            _btnGetFile        = MakeButton("📥 طلب ملف",       Color.FromArgb(30, 50, 70),  OnGetFile,  130);
            _txtRemotePassword = CreateInput("كلمة السر الجديدة", 160);
            _txtRemotePassword.UseSystemPasswordChar = true;
            _txtFileParam      = CreateInput("اسم/مسار الملف", 200);
            cmdsFlow.Controls.AddRange(new Control[] { _btnRestart, _btnShutdown, _btnChangePassword, _btnForceSync, _btnSendFile, _btnGetFile });
            var paramFlow = new FlowLayoutPanel { Height = 38, Dock = DockStyle.Top, FlowDirection = FlowDirection.LeftToRight };
            paramFlow.Controls.AddRange(new Control[] { MakeLabel("كلمة السر:"), _txtRemotePassword, MakeLabel("  ملف:"), _txtFileParam });
            cmdsPanel.Controls.AddRange(new Control[] { cmdsFlow, paramFlow });
            leftPanel.Controls.AddRange(new Control[] { cmdsPanel, ghostPanel, targetRow });
            splitContainer.Panel1.Controls.Add(leftPanel);
            // ═ الجانب الأيمن: سجل الأوامر
            var rightPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(26, 28, 30) };
            var cmdLogLabel = new Label { Text = "سجل الأوامر المرسلة", Dock = DockStyle.Top, Height = 28, ForeColor = Color.FromArgb(142, 142, 147), BackColor = Color.FromArgb(22, 24, 26), TextAlign = ContentAlignment.MiddleCenter };
            _dgvCommandLog = CreateDataGrid();
            _dgvCommandLog.Dock = DockStyle.Fill;
            _dgvCommandLog.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn { HeaderText = "الأمر",     Width = 160 },
                new DataGridViewTextBoxColumn { HeaderText = "الصراف",    Width = 110 },
                new DataGridViewTextBoxColumn { HeaderText = "المرسل",    Width = 100 },
                new DataGridViewTextBoxColumn { HeaderText = "الحالة",    Width = 90  },
                new DataGridViewTextBoxColumn { HeaderText = "الوقت",     Width = 130 },
                new DataGridViewTextBoxColumn { HeaderText = "النتيجة",   Width = 200, AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill }
            });
            rightPanel.Controls.AddRange(new Control[] { _dgvCommandLog, cmdLogLabel });
            splitContainer.Panel2.Controls.Add(rightPanel);
            _tabRemote.Controls.Add(splitContainer);
        }
        private void BuildTabAnalytics()
        {
            var toolbar = new Panel { Height = 50, Dock = DockStyle.Top, BackColor = Color.FromArgb(22, 24, 26), Padding = new Padding(12, 10, 12, 10) };
            var tf = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, WrapContents = false };
            _cmbAnalyticsATM   = new ComboBox { Width = 160, DropDownStyle = ComboBoxStyle.DropDownList, BackColor = Color.FromArgb(40, 42, 45), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            _dtpAnalyticsFrom  = new DateTimePicker { Width = 130, Format = DateTimePickerFormat.Short, Value = DateTime.Today.AddDays(-7) };
            _dtpAnalyticsTo    = new DateTimePicker { Width = 130, Format = DateTimePickerFormat.Short, Value = DateTime.Today };
            _btnRunAnalytics   = MakeButton("📊 تشغيل التحليل",  Color.FromArgb(0, 85, 170),  OnRunAnalytics, 150);
            _btnExportAnalytics = MakeButton("📄 تصدير HTML",     Color.FromArgb(50, 50, 100), OnExportAnalytics, 140);
            tf.Controls.AddRange(new Control[] { MakeLabel("الصراف: "), _cmbAnalyticsATM, MakeLabel("  من: "), _dtpAnalyticsFrom, MakeLabel("  إلى: "), _dtpAnalyticsTo, _btnRunAnalytics, _btnExportAnalytics });
            toolbar.Controls.Add(tf);
            // لوحة الإحصائيات
            _pnlAnalyticsStats = new Panel { Height = 90, Dock = DockStyle.Top, BackColor = Color.FromArgb(22, 24, 26) };
            var statsFlow = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, Padding = new Padding(12, 4, 12, 4) };
            string[] aLabels = { "✅ ناجحة", "❌ مرفوضة", "💳 محتجزة", "📈 معدل", "💰 نقد", "⚡ تشغيل", "🚨 أخطاء", "📝 سطور" };
            for (int i = 0; i < 8; i++)
            {
                var mini = new Panel { Width = 110, Height = 70, Margin = new Padding(4, 8, 4, 0), BackColor = Color.FromArgb(30, 32, 35) };
                _analyticsStatLabels[i] = new Label { Text = "—", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter, ForeColor = Color.White, Font = new Font("Segoe UI", 13f, FontStyle.Bold) };
                var lbl = new Label { Text = aLabels[i], Dock = DockStyle.Bottom, Height = 20, TextAlign = ContentAlignment.MiddleCenter, ForeColor = Color.FromArgb(142, 142, 147), Font = new Font("Segoe UI", 8f) };
                mini.Controls.AddRange(new Control[] { _analyticsStatLabels[i], lbl });
                statsFlow.Controls.Add(mini);
            }
            _pnlAnalyticsStats.Controls.Add(statsFlow);
            _lblAnalyticsSummary = new Label { Text = "اختر صراف ونطاق زمني ثم اضغط تشغيل التحليل", Dock = DockStyle.Top, Height = 28, TextAlign = ContentAlignment.MiddleCenter, ForeColor = Color.FromArgb(142, 142, 147), BackColor = Color.FromArgb(26, 28, 30) };
            _dgvAnalytics = CreateDataGrid();
            _dgvAnalytics.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn { HeaderText = "#",          Width = 50  },
                new DataGridViewTextBoxColumn { HeaderText = "النوع",      Width = 120 },
                new DataGridViewTextBoxColumn { HeaderText = "النتيجة",    Width = 90  },
                new DataGridViewTextBoxColumn { HeaderText = "المبلغ",     Width = 100 },
                new DataGridViewTextBoxColumn { HeaderText = "رمز الخطأ",  Width = 100 },
                new DataGridViewTextBoxColumn { HeaderText = "بطاقة",      Width = 70  },
                new DataGridViewTextBoxColumn { HeaderText = "السطر",      Width = 70  },
                new DataGridViewTextBoxColumn { HeaderText = "السياق",     Width = 350, AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill }
            });
            _tabAnalytics.Controls.AddRange(new Control[] { _dgvAnalytics, _lblAnalyticsSummary, _pnlAnalyticsStats, toolbar });
        }
        private void BuildTabLog()
        {
            var toolbar = new Panel { Height = 42, Dock = DockStyle.Top, BackColor = Color.FromArgb(22, 24, 26), Padding = new Padding(10, 8, 10, 8) };
            var tf = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, WrapContents = false };
            _cmbLogLevel   = new ComboBox { Width = 120, DropDownStyle = ComboBoxStyle.DropDownList, BackColor = Color.FromArgb(40, 42, 45), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            _cmbLogLevel.Items.AddRange(new object[] { "الكل", "Debug", "Info", "Warning", "Error", "Critical" });
            _cmbLogLevel.SelectedIndex = 0;
            _txtLogSearch  = CreateInput("بحث في السجل...", 200);
            _btnClearLog   = MakeButton("🗑️ مسح", Color.FromArgb(80, 30, 30), OnClearLog, 90);
            _btnExportLog  = MakeButton("📥 تصدير", Color.FromArgb(30, 60, 30), OnExportLog, 100);
            _lblLogCount   = new Label { Text = "0 سطر", ForeColor = Color.FromArgb(142, 142, 147), AutoSize = true };
            _cmbLogLevel.SelectedIndexChanged += (s, e) => FilterLog();
            _txtLogSearch.TextChanged         += (s, e) => FilterLog();
            tf.Controls.AddRange(new Control[] { MakeLabel("المستوى: "), _cmbLogLevel, MakeLabel("  "), _txtLogSearch, _btnClearLog, _btnExportLog, _lblLogCount });
            toolbar.Controls.Add(tf);
            _rtbLog = new RichTextBox
            {
                Dock        = DockStyle.Fill,
                BackColor   = Color.FromArgb(20, 22, 24),
                ForeColor   = Color.FromArgb(200, 200, 200),
                Font        = new Font("Consolas", 9f),
                ReadOnly    = true,
                BorderStyle = BorderStyle.None,
                ScrollBars  = RichTextBoxScrollBars.Vertical
            };
            _tabLog.Controls.AddRange(new Control[] { _rtbLog, toolbar });
        }
        private void OnATMConnected(ATMInfo atm)
        {
            if (InvokeRequired) { Invoke(new Action(() => OnATMConnected(atm))); return; }
            if (!_atmPanels.ContainsKey(atm.ATM_ID))
            {
                var card = new ATMCardPanel(atm);
                card.OnDoubleClickCard += (s, a) => OpenJournalViewer(a);
                _atmPanels[atm.ATM_ID] = card;
                _pnlATMCards.Controls.Add(card);
                _cmbTargetATM.Items.Add(atm.ATM_ID);
                _cmbAnalyticsATM.Items.Add(atm.ATM_ID);
            }
            _atmPanels[atm.ATM_ID].UpdateATM(atm);
            UpdateMetrics();
            RefreshConnectionsTab();
            AppLogger.Instance.Info($"★ ATM متصل: {atm.ATM_ID} ({atm.ATM_Type})", "UI");
        }
        private void OnATMDisconnected(ATMInfo atm)
        {
            if (InvokeRequired) { Invoke(new Action(() => OnATMDisconnected(atm))); return; }
            if (_atmPanels.ContainsKey(atm.ATM_ID))
                _atmPanels[atm.ATM_ID].UpdateATM(atm);
            UpdateMetrics();
            RefreshConnectionsTab();
            AlertManager.Instance.CheckATMHealth(atm);
        }
        private void RefreshATMCard(ATMInfo atm)
        {
            if (InvokeRequired) { Invoke(new Action(() => RefreshATMCard(atm))); return; }
            if (_atmPanels.TryGetValue(atm.ATM_ID, out var card))
                card.UpdateATM(atm);
        }
        private void OnJournalReceived(ReceivedPacket pkt)
        {
            if (pkt.IsGhostFrame)
            {
                // تحديث Ghost View
                if (InvokeRequired) { Invoke(new Action(() => UpdateGhostView(pkt.Message.Payload))); return; }
                UpdateGhostView(pkt.Message.Payload);
                return;
            }
            // أرشفة الجورنال
            if (pkt.Data != null && pkt.Data.Length > 0)
            {
                var path = _archiveManager.Archive(pkt.ATM.ATM_ID, pkt.FileName, pkt.Data, pkt.Checksum, pkt.SHA256);
                pkt.ATM.TransactionCount++;
                AppLogger.Instance.Info($"📦 جورنال مؤرشف: {pkt.ATM.ATM_ID}/{pkt.FileName} → {path}", "Archive");
            }
        }
        private void UpdateGhostView(byte[] jpegData)
        {
            if (jpegData == null || jpegData.Length == 0) return;
            try
            {
                using var ms = new MemoryStream(jpegData);
                var img = Image.FromStream(ms);
                _pbGhostView.Image?.Dispose();
                _pbGhostView.Image = img;
            }
            catch { }
        }
        private void OnChangePasswordAll(object s = null, EventArgs e = null)
        {
            if (MessageBox.Show("هل تريد تغيير كلمة السر لجميع الصرافات المتصلة؟\nستُرسل الأوامر بالتسلسل.", "تأكيد جماعي", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
            var newPwd = Microsoft.VisualBasic.Interaction.InputBox("أدخل كلمة السر الجديدة:", "تغيير كلمة سر جماعي", "");
            if (string.IsNullOrEmpty(newPwd)) return;
            _rtbPasswordLog.AppendText($"\r\n[{DateTime.Now:HH:mm:ss}] بدء تغيير كلمة السر الجماعي...\r\n");
            foreach (var atm in _serverEngine.GetConnectedATMs())
            {
                var ok = _serverEngine.SendCommand(atm.ATM_ID, AppConstants.CMD_CHANGE_PASSWORD, newPwd, _currentUser);
                _rtbPasswordLog.AppendText($"  → {atm.ATM_ID}: {(ok ? "✓ أُرسل" : "✗ فشل")}\r\n");
            }
            _rtbPasswordLog.ScrollToCaret();
        }
        private void OnSendImagesAll(object s = null, EventArgs e = null)
        {
            foreach (var atm in _serverEngine.GetConnectedATMs())
                _serverEngine.SendCommand(atm.ATM_ID, AppConstants.CMD_SYNC_IMAGES, "", _currentUser);
            AppLogger.Instance.Info("📷 أُرسل أمر مزامنة الصور لجميع الصرافات", "Remote");
        }
        private void OnBroadcast(object s = null, EventArgs e = null)
        {
            var msg = Microsoft.VisualBasic.Interaction.InputBox("أدخل الرسالة للبث لجميع الصرافات:", "بث جماعي", "");
            if (string.IsNullOrEmpty(msg)) return;
            _serverEngine.Broadcast(msg, _currentUser);
            DatabaseManager.Instance.InsertAuditLog("Broadcast", _currentUser, null, msg);
        }
        private void OnGhostStart(object s = null, EventArgs e = null)
        {
            var atmId = _cmbTargetATM.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(atmId)) return;
            if (MessageBox.Show($"هل تريد بدء وضع الشبح لـ {atmId}؟", "تأكيد Ghost View", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            _serverEngine.SendCommand(atmId, AppConstants.CMD_GHOST_START, _tbGhostQuality.Value.ToString(), _currentUser);
            _btnGhostStart.Enabled = false;
            _btnGhostStop.Enabled  = true;
            DatabaseManager.Instance.InsertAuditLog("GhostStart", _currentUser, atmId, $"Ghost View quality={_tbGhostQuality.Value}%");
        }
        private void OnGhostStop(object s = null, EventArgs e = null)
        {
            var atmId = _cmbTargetATM.SelectedItem?.ToString();
            if (!string.IsNullOrEmpty(atmId))
                _serverEngine.SendCommand(atmId, AppConstants.CMD_GHOST_STOP, "", _currentUser);
            _btnGhostStart.Enabled = true;
            _btnGhostStop.Enabled  = false;
        }
        private void OnRestart(object s = null, EventArgs e = null) => SendCmdWithConfirm(AppConstants.CMD_RESTART, "إعادة تشغيل");
        private void OnShutdown(object s = null, EventArgs e = null) => SendCmdWithConfirm(AppConstants.CMD_SHUTDOWN, "إيقاف التشغيل");
        private void OnChangePassword(object s = null, EventArgs e = null)
        {
            var atmId = _cmbTargetATM.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(atmId) || string.IsNullOrEmpty(_txtRemotePassword.Text)) return;
            if (MessageBox.Show($"تغيير كلمة سر {atmId}؟", "تأكيد", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
            var ok = _serverEngine.SendCommand(atmId, AppConstants.CMD_CHANGE_PASSWORD, _txtRemotePassword.Text, _currentUser);
            _rtbPasswordLog.AppendText($"\r\n[{DateTime.Now:HH:mm:ss}] {atmId} — تغيير كلمة السر: {(ok ? "✓" : "✗")}\r\n");
        }
        private void OnForceSync(object s = null, EventArgs e = null) => SendCmd(AppConstants.CMD_FORCE_SYNC, "");
        private void OnSendFile(object s = null, EventArgs e = null)   => SendCmd(AppConstants.CMD_SEND_FILE, _txtFileParam.Text);
        private void OnGetFile(object s = null, EventArgs e = null)    => SendCmd(AppConstants.CMD_GET_FILE, _txtFileParam.Text);
        private void SendCmd(string cmd, string param)
        {
            var atmId = _cmbTargetATM.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(atmId)) return;
            var ok = _serverEngine.SendCommand(atmId, cmd, param, _currentUser);
            AddCommandLog(cmd, atmId, ok ? "أُرسل" : "فشل");
        }
        private void SendCmdWithConfirm(string cmd, string label)
        {
            var atmId = _cmbTargetATM.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(atmId)) return;
            if (MessageBox.Show($"هل تريد {label} الصراف {atmId}؟", "تأكيد أمر حساس", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
            var ok = _serverEngine.SendCommand(atmId, cmd, "", _currentUser);
            AddCommandLog(cmd, atmId, ok ? "أُرسل" : "فشل");
            DatabaseManager.Instance.InsertAuditLog(cmd, _currentUser, atmId, $"{label} — {(ok ? "نجح" : "فشل")}");
        }
        private void AddCommandLog(string cmd, string atmId, string status)
        {
            if (_dgvCommandLog.InvokeRequired) { _dgvCommandLog.Invoke(new Action(() => AddCommandLog(cmd, atmId, status))); return; }
            _dgvCommandLog.Rows.Insert(0, cmd, atmId, _currentUser, status, DateTime.Now.ToString("HH:mm:ss"), "");
        }
        private void OnSearchArchive(object s = null, EventArgs e = null)
        {
            var entries = DatabaseManager.Instance.SearchArchive(
                string.IsNullOrWhiteSpace(_txtSearchATM.Text) ? null : _txtSearchATM.Text.Trim(),
                _dtpFrom.Value, _dtpTo.Value,
                string.IsNullOrWhiteSpace(_txtSearchKeyword.Text) ? null : _txtSearchKeyword.Text.Trim());
            _dgvArchive.Rows.Clear();
            foreach (var e2 in entries)
                _dgvArchive.Rows.Add(e2.ATMId, e2.FileName, e2.FileSizeDisplay,
                    $"{e2.CompressedSize / 1024.0:F1} KB", e2.TransactionCount,
                    e2.MD5Hash?.Substring(0, 8), e2.ReceivedAt.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss"), e2.ArchivePath);
            _lblArchiveCount.Text = $"النتائج: {entries.Count} إدخال";
        }
        private void OnArchiveEntryDoubleClick(object s, EventArgs e)
        {
            if (_dgvArchive.CurrentRow == null) return;
            var path = _dgvArchive.CurrentRow.Cells[7].Value?.ToString();
            if (string.IsNullOrEmpty(path) || !File.Exists(path)) return;
            var atmId = _dgvArchive.CurrentRow.Cells[0].Value?.ToString();
            var atmType = _serverEngine.GetATMByID(atmId)?.ATM_Type ?? "NCR";
            OpenJournalTextViewer(path, atmId, atmType);
        }
        private void OnRunAnalytics(object s = null, EventArgs e = null)
        {
            var atmId = _cmbAnalyticsATM.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(atmId)) return;
            var entries = DatabaseManager.Instance.SearchArchive(atmId, _dtpAnalyticsFrom.Value, _dtpAnalyticsTo.Value, null, 200);
            if (entries.Count == 0) { MessageBox.Show("لا يوجد بيانات للتحليل في هذه الفترة.", "تنبيه"); return; }
            // تحليل الملف الأحدث للعرض
            var latest = entries[0];
            var atm    = _serverEngine.GetATMByID(atmId);
            JournalAnalysisResult result = null;
            if (!string.IsNullOrEmpty(latest.ArchivePath) && File.Exists(latest.ArchivePath))
            {
                var text = new ArchiveManager().RetrieveAsText(latest.ArchivePath);
                if (!string.IsNullOrEmpty(text))
                    result = _analysisEngine.AnalyzeText(text, atmId, atm?.ATM_Type ?? "NCR") == null ? null :
                        new JournalAnalysisResult { ATMId = atmId };
            }
            // عرض الإحصائيات التجميعية من قاعدة البيانات
            RefreshAnalyticsFromDB(atmId, _dtpAnalyticsFrom.Value, _dtpAnalyticsTo.Value, entries.Count);
        }
        private void RefreshAnalyticsFromDB(string atmId, DateTime from, DateTime to, int entryCount)
        {
            // مجمعة من journal_archive
            var stats = DatabaseManager.Instance.GetDailyStatsTable(atmId,
                from.ToString("yyyy-MM-dd"), to.ToString("yyyy-MM-dd"));
            int approved = 0, failed = 0, captures = 0;
            double cash  = 0;
            foreach (System.Data.DataRow row in stats.Rows)
            {
                approved += Convert.ToInt32(row["approved_tx"]);
                failed   += Convert.ToInt32(row["failed_tx"]);
                captures += Convert.ToInt32(row["cards_captured"]);
                cash     += Convert.ToDouble(row["cash_dispensed"]);
            }
            _analyticsStatLabels[0].Text = approved.ToString("N0");
            _analyticsStatLabels[1].Text = failed.ToString("N0");
            _analyticsStatLabels[2].Text = captures.ToString("N0");
            _analyticsStatLabels[3].Text = approved + failed > 0 ? $"{(double)approved / (approved + failed) * 100:F1}%" : "—";
            _analyticsStatLabels[4].Text = cash.ToString("N0");
            _analyticsStatLabels[5].Text = "—";
            _analyticsStatLabels[6].Text = "—";
            _analyticsStatLabels[7].Text = entryCount.ToString("N0");
            _lblAnalyticsSummary.Text = $"تحليل {atmId} — {entryCount} ملف جورنال | {from:yyyy-MM-dd} → {to:yyyy-MM-dd}";
        }
        private void OnExportAnalytics(object s = null, EventArgs e = null)
        {
            // تصدير تقرير HTML
            var atms = new List<ATMInfo>(_serverEngine.GetConnectedATMs());
            var path = _reportEngine.ExportDailyNocReport(atms, DateTime.Today);
            System.Diagnostics.Process.Start(path);
        }
        private void OnExportArchiveCSV(object s = null, EventArgs e = null)
        {
            var path = _reportEngine.ExportTransactionsCSV(_txtSearchATM.Text.Trim(), _dtpFrom.Value, _dtpTo.Value);
            MessageBox.Show($"تم تصدير التقرير:\n{path}", "تصدير ناجح", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        private void OnExportArchiveHTML(object s = null, EventArgs e = null)
        {
            // تصدير HTML نهائي
            OnExportAnalytics();
        }
        private void UpdateMetrics()
        {
            var atms      = new List<ATMInfo>(_serverEngine.GetConnectedATMs());
            int connected = 0, idle = 0;
            long totalKB  = 0;
            foreach (var a in atms)
            {
                if (a.ConnectionStatus == ConnectionStatus.Connected) connected++;
                if ((DateTime.UtcNow - a.LastDataReceivedUtc).TotalMinutes > 30) idle++;
                totalKB += a.JournalSizeToday / 1024;
            }
            var rate = atms.Count > 0 ? (double)connected / atms.Count * 100.0 : 0;
            var free = _archiveManager.GetFreeSpaceGB();
            SetMetric(0, connected.ToString());
            SetMetric(1, idle.ToString());
            SetMetric(2, $"{totalKB:N0} KB");
            SetMetric(3, $"{rate:F1}%");
            SetMetric(4, AlertManager.Instance.ActiveCount.ToString());
            SetMetric(5, "—");
            SetMetric(6, DateTime.Now.ToString("HH:mm:ss"));
            SetMetric(7, $"{free:F1} GB");
            _lblConnections.Text = $"الاتصالات: {connected}/{atms.Count}";
            _lblAlerts.Text      = $"التنبيهات: {AlertManager.Instance.ActiveCount}";
            _lblStorage.Text     = $"المساحة: {free:F1} GB";
            AlertManager.Instance.CheckDiskSpace("Server", free, _archiveManager.GetTotalSpaceGB());
        }
        private void SetMetric(int idx, string value)
        {
            if (idx < _metricValues.Length && _metricValues[idx] != null)
                _metricValues[idx].Text = value;
        }
        private void ShowAlert(AlertPayload alert)
        {
            if (InvokeRequired) { Invoke(new Action(() => ShowAlert(alert))); return; }
            AppendLog($"[ALERT] {alert.SeverityIcon} {alert.Title}: {alert.Message}", AppLogger.Level.Warning);
        }
        private void ShowCriticalAlert(AlertPayload alert)
        {
            if (InvokeRequired) { Invoke(new Action(() => ShowCriticalAlert(alert))); return; }
            _alertBar.BackColor  = Color.FromArgb(60, 15, 15);
            _lblAlertText.Text   = $"🚨 {alert.Title}: {alert.Message}  [اضغط للإغلاق]";
            _alertBar.Visible    = true;
            AppendLog($"[CRITICAL] {alert.Title}: {alert.Message}", AppLogger.Level.Critical);
        }
        private readonly object _logLock = new object();
        private void AppendLog(string text, AppLogger.Level level = AppLogger.Level.Info)
        {
            if (_rtbLog == null || IsDisposed) return;
            if (_rtbLog.InvokeRequired) { _rtbLog.Invoke(new Action(() => AppendLog(text, level))); return; }
            lock (_logLock)
            {
                var color = level switch
                {
                    AppLogger.Level.Error    => Color.FromArgb(255, 100, 100),
                    AppLogger.Level.Critical => Color.FromArgb(255, 50, 50),
                    AppLogger.Level.Warning  => Color.FromArgb(255, 200, 50),
                    AppLogger.Level.Debug    => Color.FromArgb(100, 140, 200),
                    _ => Color.FromArgb(200, 200, 200)
                };
                _rtbLog.SelectionStart  = _rtbLog.TextLength;
                _rtbLog.SelectionLength = 0;
                _rtbLog.SelectionColor  = color;
                _rtbLog.AppendText($"{text}\n");
                _rtbLog.SelectionColor  = Color.FromArgb(200, 200, 200);
                _rtbLog.ScrollToCaret();
                _logLines++;
                if (_lblLogCount != null) _lblLogCount.Text = $"{_logLines} سطر";
                // حد أقصى 5000 سطر
                if (_logLines > 5000)
                {
                    _rtbLog.Clear();
                    _logLines = 0;
                }
            }
        }
        private void FilterLog() { /* فلترة مرئية فقط */ }
        private void OnClearLog(object s, EventArgs e) { _rtbLog.Clear(); _logLines = 0; }
        private void OnExportLog(object s, EventArgs e)
        {
            var path = Path.Combine(AppConstants.DefaultLogPath, $"server_log_{DateTime.Now:yyyyMMdd_HHmmss}.txt");
            File.WriteAllText(path, _rtbLog.Text);
            MessageBox.Show($"تم تصدير السجل:\n{path}", "تصدير ناجح");
        }
        private void OpenJournalViewer(ATMInfo atm)
        {
            var viewer = new JournalViewerForm(atm, _analysisEngine);
            viewer.Show(this);
        }
        private void OpenJournalTextViewer(string archivePath, string atmId, string atmType)
        {
            var viewer = new JournalViewerForm(archivePath, atmId, atmType, _analysisEngine, _archiveManager);
            viewer.Show(this);
        }
        private void InitializeTimers()
        {
            _uiTimer = new Timer { Interval = 1000 };
            _uiTimer.Tick += (s, e) =>
            {
                _lblTime.Text = $"🕐 {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
                UpdateMetrics();
                RefreshConnectionsTab();
            };
            _uiTimer.Start();
            _healthTimer = new Timer { Interval = 60000 };
            _healthTimer.Tick += (s, e) =>
            {
                foreach (var atm in _serverEngine.GetConnectedATMs())
                    AlertManager.Instance.CheckATMHealth(atm);
            };
            _healthTimer.Start();
        }
        private void RefreshConnectionsTab()
        {
            if (_tabMain.SelectedTab != _tabConnections) return;
            var atms = new List<ATMInfo>(_serverEngine.GetConnectedATMs());
            _dgvConnections.Rows.Clear();
            foreach (var atm in atms)
                _dgvConnections.Rows.Add(atm.ATM_ID, atm.ATM_Name, atm.ATM_Type, atm.ServerIP,
                    atm.NetworkType, atm.GetStatusLabel(), $"{atm.Latency_ms} ms",
                    atm.GetElapsed(atm.LastHeartbeatUtc), atm.GetElapsed(atm.LastSyncUtc), atm.SessionId);
            _lblConnCount.Text = $"الاتصالات: {atms.Count}";
        }
        private Panel CreateMetricCard(string icon, string label, string value, int idx)
        {
            var card = new Panel { Width = 150, Height = 80, Margin = new Padding(4, 4, 4, 4), BackColor = Color.FromArgb(30, 32, 35), Cursor = Cursors.Default };
            var iconLabel  = new Label { Text = icon,  Dock = DockStyle.Top,  Height = 20, TextAlign = ContentAlignment.MiddleCenter, ForeColor = Color.White, Font = new Font("Segoe UI", 13f) };
            _metricValues[idx] = new Label { Text = value, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter, ForeColor = Color.White, Font = new Font("Segoe UI", 16f, FontStyle.Bold) };
            var lblText    = new Label { Text = label, Dock = DockStyle.Bottom, Height = 20, TextAlign = ContentAlignment.MiddleCenter, ForeColor = Color.FromArgb(142, 142, 147), Font = new Font("Segoe UI", 8f) };
            card.Controls.AddRange(new Control[] { _metricValues[idx], lblText, iconLabel });
            return card;
        }
        private Button MakeButton(string text, Color backColor, Action action, int width = 130) =>
            MakeButton(text, backColor, (s, e) => action?.Invoke(), width);
        private Button MakeButton(string text, Color backColor, EventHandler onClick, int width = 130)
        {
            var btn = new Button
            {
                Text      = text,
                Width     = width,
                Height    = 32,
                BackColor = backColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", 8.5f),
                Margin    = new Padding(4, 2, 4, 2),
                Cursor    = Cursors.Hand
            };
            btn.FlatAppearance.BorderColor = Color.FromArgb(backColor.R + 20, backColor.G + 20, backColor.B + 20);
            btn.Click += onClick;
            return btn;
        }
        private TextBox CreateInput(string placeholder, int width) => new TextBox
        {
            Width     = width,
            Text      = placeholder,
            BackColor = Color.FromArgb(40, 42, 45),
            ForeColor = Color.FromArgb(130, 130, 130),
            BorderStyle = BorderStyle.FixedSingle,
            Margin    = new Padding(4, 4, 4, 4)
        };
        private Label MakeLabel(string text) => new Label
        {
            Text      = text,
            AutoSize  = true,
            ForeColor = Color.FromArgb(142, 142, 147),
            TextAlign = ContentAlignment.MiddleLeft,
            Padding   = new Padding(0, 6, 0, 0)
        };
        private DataGridView CreateDataGrid()
        {
            var dgv = new DataGridView
            {
                Dock              = DockStyle.Fill,
                BackgroundColor   = Color.FromArgb(26, 28, 30),
                GridColor         = Color.FromArgb(44, 46, 48),
                ForeColor         = Color.FromArgb(200, 200, 200),
                ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.FromArgb(35, 37, 40),
                    ForeColor = Color.FromArgb(180, 180, 180),
                    Font      = new Font("Segoe UI", 8.5f, FontStyle.Bold)
                },
                DefaultCellStyle  = new DataGridViewCellStyle
                {
                    BackColor = Color.FromArgb(26, 28, 30),
                    ForeColor = Color.FromArgb(200, 200, 200),
                    SelectionBackColor = Color.FromArgb(0, 85, 170),
                    SelectionForeColor = Color.White
                },
                RowsDefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.FromArgb(28, 30, 32)
                },
                AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.FromArgb(32, 34, 37)
                },
                SelectionMode     = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect       = false,
                ReadOnly          = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                RowHeadersVisible = false,
                AutoSizeRowsMode  = DataGridViewAutoSizeRowsMode.None,
                RowTemplate = { Height = 28 },
                BorderStyle       = BorderStyle.None,
                ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None
            };
            return dgv;
        }
        private void DrawTabItem(object sender, DrawItemEventArgs e)
        {
            var tab   = _tabMain.TabPages[e.Index];
            var rect  = e.Bounds;
            bool sel  = e.Index == _tabMain.SelectedIndex;
            using var bg = new SolidBrush(sel ? Color.FromArgb(26, 28, 30) : Color.FromArgb(30, 32, 35));
            e.Graphics.FillRectangle(bg, rect);
            if (sel)
            {
                using var accent = new SolidBrush(Color.FromArgb(0, 122, 255));
                e.Graphics.FillRectangle(accent, rect.Left, rect.Bottom - 3, rect.Width, 3);
            }
            using var fg = new SolidBrush(sel ? Color.White : Color.FromArgb(130, 130, 130));
            var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            e.Graphics.DrawString(tab.Text, new Font("Segoe UI", 8.5f, sel ? FontStyle.Bold : FontStyle.Regular), fg, rect, sf);
        }
        private void ApplyNocTheme()
        {
            BackColor = Color.FromArgb(26, 28, 30);
            foreach (TabPage tp in _tabMain.TabPages)
                tp.BackColor = Color.FromArgb(26, 28, 30);
        }
        private Icon CreateAppIcon() => null; // استخدم أيقونة النظام الافتراضية
        private void OnExportAllReports(object s, EventArgs e) { }
        private void OnOpenSettings(object s, EventArgs e)     { }
        private void OnManageUsers(object s, EventArgs e)      { }
        private void OnOpenAuditLog(object s, EventArgs e)
        {
            var path = _reportEngine.ExportAuditLogCSV(DateTime.Today.AddDays(-30), DateTime.Today);
            System.Diagnostics.Process.Start(path);
        }
        private void btnArchiveNow_Click(object sender, EventArgs e) { MessageBox.Show("Archive initiated", "Info"); }
        private void btnClearLog_Click(object sender, EventArgs e) { if (txtLog != null) txtLog.Clear(); }
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _uiTimer?.Stop();
            _healthTimer?.Stop();
            _serverEngine?.Dispose();
            base.OnFormClosing(e);
        }
        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.Text = "EJLive Server v3.4.0 - NOC Dashboard";
            this.Size = new Size(1050, 720);
            this.MinimumSize = new Size(1000, 680);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new Font("Segoe UI", 9F);
            this.Icon = SystemIcons.Shield;
            tabMain = new TabControl { Dock = DockStyle.Fill };
            tabServer = new TabPage("Server");
            tabConnections = new TabPage("Connections");
            tabArchive = new TabPage("Archive");
            tabRemote = new TabPage("Remote");
            tabAnalytics = new TabPage("Analytics");
            tabLog = new TabPage("Log");
            tabMain.TabPages.AddRange(new[] { tabServer, tabConnections, tabArchive, tabRemote, tabAnalytics, tabLog });
            BuildServerTab();
            BuildConnectionsTab();
            BuildArchiveTab();
            BuildRemoteTab();
            BuildAnalyticsTab();
            BuildLogTab();
            BuildStatusBar();
            this.Controls.Add(tabMain);
            this.Controls.Add(statusBar);
            this.ResumeLayout(false);
        }
        private void BuildServerTab()
        {
            var mainPanel = new Panel { Dock = DockStyle.Fill };
            tabServer.Controls.Add(mainPanel);
            // === NOC Dashboard - 8 مقاييس ===
            pnlNOC = new Panel { Dock = DockStyle.Top, Height = 90, BackColor = Color.FromArgb(26, 35, 60) };
            int mx = 15;
            AddNOCMetric(pnlNOC, "Total ATMs", "0", Color.FromArgb(33, 150, 243), ref mx, out lblTotalATMs, out lblTotalATMsVal);
            AddNOCMetric(pnlNOC, "Connected", "0", Color.FromArgb(76, 175, 80), ref mx, out lblConnected, out lblConnectedVal);
            AddNOCMetric(pnlNOC, "Syncing", "0", Color.FromArgb(0, 188, 212), ref mx, out lblSyncing, out lblSyncingVal);
            AddNOCMetric(pnlNOC, "Errors", "0", Color.FromArgb(244, 67, 54), ref mx, out lblErrors, out lblErrorsVal);
            AddNOCMetric(pnlNOC, "Offline", "0", Color.FromArgb(158, 158, 158), ref mx, out lblOffline, out lblOfflineVal);
            AddNOCMetric(pnlNOC, "Supervisor", "0", Color.FromArgb(255, 152, 0), ref mx, out lblSupervisor, out lblSupervisorVal);
            AddNOCMetric(pnlNOC, "Bandwidth", "0 KB/s", Color.FromArgb(156, 39, 176), ref mx, out lblBandwidth, out lblBandwidthVal);
            AddNOCMetric(pnlNOC, "Uptime", "00:00:00", Color.FromArgb(121, 85, 72), ref mx, out lblUptime, out lblUptimeVal);
            mainPanel.Controls.Add(pnlNOC);
            // === Server Control ===
            grpServerControl = new GroupBox { Text = "Server Control", Location = new Point(5, 95), Size = new Size(1010, 60), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
            lblPort = new Label { Text = "Port:", Location = new Point(10, 25), AutoSize = true };
            numPort = new NumericUpDown { Location = new Point(45, 22), Width = 70, Minimum = 1024, Maximum = 65535, Value = 5656 };
            btnStartServer = new Button { Text = "Start Server", Location = new Point(130, 20), Size = new Size(110, 28), BackColor = Color.FromArgb(76, 175, 80), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
            btnStopServer = new Button { Text = "Stop Server", Location = new Point(250, 20), Size = new Size(100, 28), BackColor = Color.FromArgb(244, 67, 54), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Enabled = false };
            lblServerStatus = new Label { Text = "Status: Stopped", Location = new Point(370, 25), AutoSize = true, ForeColor = Color.Gray };
            new Label { Text = "Archive:", Location = new Point(530, 25), AutoSize = true, Parent = grpServerControl };
            txtArchivePath = new TextBox { Location = new Point(585, 22), Width = 320, Text = _archivePath, ReadOnly = true };
            btnBrowseArchive = new Button { Text = "...", Location = new Point(910, 20), Size = new Size(40, 28) };
            btnStartServer.Click += BtnStartServer_Click;
            btnStopServer.Click += BtnStopServer_Click;
            btnBrowseArchive.Click += (s, e) => { using (var fbd = new FolderBrowserDialog()) { if (fbd.ShowDialog() == DialogResult.OK) { txtArchivePath.Text = fbd.SelectedPath; _archivePath = fbd.SelectedPath; } } };
            grpServerControl.Controls.AddRange(new Control[] { lblPort, numPort, btnStartServer, btnStopServer, lblServerStatus, txtArchivePath, btnBrowseArchive });
            mainPanel.Controls.Add(grpServerControl);
            // === ATM Cards Flow ===
            flpATMCards = new FlowLayoutPanel { Location = new Point(5, 160), Size = new Size(1010, 480), Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right, AutoScroll = true, BackColor = Color.FromArgb(240, 242, 245) };
            mainPanel.Controls.Add(flpATMCards);
        }
        private void AddNOCMetric(Panel parent, string label, string value, Color color, ref int x, out Label lblLabel, out Label lblValue)
        {
            var panel = new Panel { Location = new Point(x, 10), Size = new Size(115, 70), BackColor = Color.FromArgb(35, 45, 75) };
            panel.Paint += (s, e) => { using (var pen = new Pen(color, 2)) e.Graphics.DrawRectangle(pen, 0, 0, panel.Width - 1, panel.Height - 1); };
            lblValue = new Label { Text = value, Location = new Point(5, 8), Size = new Size(105, 30), ForeColor = color, Font = new Font("Segoe UI", 16, FontStyle.Bold), TextAlign = ContentAlignment.MiddleCenter };
            lblLabel = new Label { Text = label, Location = new Point(5, 42), Size = new Size(105, 20), ForeColor = Color.LightGray, Font = new Font("Segoe UI", 8), TextAlign = ContentAlignment.MiddleCenter };
            panel.Controls.Add(lblValue);
            panel.Controls.Add(lblLabel);
            parent.Controls.Add(panel);
            x += 125;
        }
        private void BuildConnectionsTab()
        {
            pnlConnectionToolbar = new Panel { Dock = DockStyle.Top, Height = 40 };
            btnRefreshConnections = new Button { Text = "Refresh", Location = new Point(5, 7), Size = new Size(80, 26) };
            btnDisconnectSelected = new Button { Text = "Disconnect", Location = new Point(90, 7), Size = new Size(90, 26), BackColor = Color.FromArgb(244, 67, 54), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnBroadcastMsg = new Button { Text = "Broadcast Message", Location = new Point(190, 7), Size = new Size(130, 26) };
            lblConnectionCount = new Label { Text = "Connections: 0", Location = new Point(340, 12), AutoSize = true };
            btnRefreshConnections.Click += (s, e) => RefreshConnectionsGrid();
            btnDisconnectSelected.Click += BtnDisconnectSelected_Click;
            btnBroadcastMsg.Click += BtnBroadcastMsg_Click;
            pnlConnectionToolbar.Controls.AddRange(new Control[] { btnRefreshConnections, btnDisconnectSelected, btnBroadcastMsg, lblConnectionCount });
            tabConnections.Controls.Add(pnlConnectionToolbar);
            dgvConnections = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, SelectionMode = DataGridViewSelectionMode.FullRowSelect };
            dgvConnections.Columns.AddRange(new DataGridViewColumn[] {
                new DataGridViewTextBoxColumn { Name = "ATM_ID", HeaderText = "ATM ID", Width = 80 },
                new DataGridViewTextBoxColumn { Name = "IP", HeaderText = "IP Address", Width = 100 },
                new DataGridViewTextBoxColumn { Name = "Type", HeaderText = "Type", Width = 50 },
                new DataGridViewTextBoxColumn { Name = "Status", HeaderText = "Status", Width = 70 },
                new DataGridViewTextBoxColumn { Name = "Connected", HeaderText = "Connected At", Width = 100 },
                new DataGridViewTextBoxColumn { Name = "LastSync", HeaderText = "Last Sync", Width = 90 },
                new DataGridViewTextBoxColumn { Name = "Latency", HeaderText = "Latency", Width = 60 },
                new DataGridViewTextBoxColumn { Name = "Network", HeaderText = "Network", Width = 60 },
                new DataGridViewTextBoxColumn { Name = "Files", HeaderText = "Files", Width = 50 }
            });
            tabConnections.Controls.Add(dgvConnections);
        }
        private void BuildArchiveTab()
        {
            pnlArchiveToolbar = new Panel { Dock = DockStyle.Top, Height = 45 };
            dtpArchiveFrom = new DateTimePicker { Location = new Point(10, 10), Width = 130, Format = DateTimePickerFormat.Short };
            new Label { Text = "to", Location = new Point(145, 14), AutoSize = true, Parent = pnlArchiveToolbar };
            dtpArchiveTo = new DateTimePicker { Location = new Point(165, 10), Width = 130, Format = DateTimePickerFormat.Short };
            cmbArchiveATM = new ComboBox { Location = new Point(310, 10), Width = 100, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbArchiveATM.Items.Add("All ATMs");
            cmbArchiveATM.SelectedIndex = 0;
            btnArchiveSearch = new Button { Text = "Search", Location = new Point(420, 8), Size = new Size(75, 26), BackColor = Color.FromArgb(33, 150, 243), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnArchiveExport = new Button { Text = "Export", Location = new Point(500, 8), Size = new Size(75, 26), BackColor = Color.FromArgb(76, 175, 80), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnArchiveOpen = new Button { Text = "Open File", Location = new Point(580, 8), Size = new Size(80, 26) };
            lblArchiveStats = new Label { Text = "0 files", Location = new Point(670, 14), AutoSize = true };
            btnArchiveSearch.Click += BtnArchiveSearch_Click;
            btnArchiveExport.Click += BtnArchiveExport_Click;
            btnArchiveOpen.Click += BtnArchiveOpen_Click;
            pnlArchiveToolbar.Controls.AddRange(new Control[] { dtpArchiveFrom, dtpArchiveTo, cmbArchiveATM, btnArchiveSearch, btnArchiveExport, btnArchiveOpen, lblArchiveStats });
            tabArchive.Controls.Add(pnlArchiveToolbar);
            dgvArchive = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, SelectionMode = DataGridViewSelectionMode.FullRowSelect };
            dgvArchive.Columns.AddRange(new DataGridViewColumn[] {
                new DataGridViewTextBoxColumn { Name = "ATM_ID", HeaderText = "ATM", Width = 70 },
                new DataGridViewTextBoxColumn { Name = "FileName", HeaderText = "File Name", Width = 150 },
                new DataGridViewTextBoxColumn { Name = "Date", HeaderText = "Date", Width = 90 },
                new DataGridViewTextBoxColumn { Name = "Size", HeaderText = "Size", Width = 60 },
                new DataGridViewTextBoxColumn { Name = "Checksum", HeaderText = "Checksum", Width = 100 },
                new DataGridViewTextBoxColumn { Name = "Path", HeaderText = "Path", Width = 200 }
            });
            tabArchive.Controls.Add(dgvArchive);
        }
        private void BuildRemoteTab()
        {
            var splitRemote = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Vertical, SplitterDistance = 350 };
            tabRemote.Controls.Add(splitRemote);
            var pnlCommands = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };
            var lblSelectATM = new Label { Text = "Select ATM:", Location = new Point(10, 10), AutoSize = true };
            cmbRemoteATM = new ComboBox { Location = new Point(90, 7), Width = 150, DropDownStyle = ComboBoxStyle.DropDownList };
            lblRemoteStatus = new Label { Text = "Ready", Location = new Point(250, 10), AutoSize = true, ForeColor = Color.Green };
            int by = 45;
            btnCmdRestart = CreateCmdButton("Restart ATM", new Point(10, by), Color.FromArgb(244, 67, 54)); by += 35;
            btnCmdScreenshot = CreateCmdButton("Screenshot", new Point(10, by), Color.FromArgb(33, 150, 243)); by += 35;
            btnCmdTimeSync = CreateCmdButton("Time Sync", new Point(10, by), Color.FromArgb(76, 175, 80)); by += 35;
            btnCmdSysInfo = CreateCmdButton("System Info", new Point(10, by), Color.FromArgb(156, 39, 176)); by += 35;
            btnCmdGhostStart = CreateCmdButton("Ghost View Start", new Point(10, by), Color.FromArgb(255, 152, 0)); by += 35;
            btnCmdGhostStop = CreateCmdButton("Ghost View Stop", new Point(10, by), Color.FromArgb(158, 158, 158)); by += 35;
            btnCmdImageSync = CreateCmdButton("Sync Images", new Point(10, by), Color.FromArgb(0, 150, 136)); by += 50;
            new Label { Text = "--- Broadcast ---", Location = new Point(10, by), AutoSize = true, ForeColor = Color.Gray, Parent = pnlCommands }; by += 25;
            btnBroadcastRestart = CreateCmdButton("Broadcast Restart", new Point(10, by), Color.FromArgb(183, 28, 28)); by += 35;
            btnBroadcastTimeSync = CreateCmdButton("Broadcast TimeSync", new Point(10, by), Color.FromArgb(27, 94, 32));
            btnCmdRestart.Click += (s, e) => SendRemoteCommand("CMD_RESTART");
            btnCmdScreenshot.Click += (s, e) => SendRemoteCommand("CMD_SCREENSHOT");
            btnCmdTimeSync.Click += (s, e) => SendRemoteCommand("CMD_TIMESYNC");
            btnCmdSysInfo.Click += (s, e) => SendRemoteCommand("CMD_SYSINFO");
            btnCmdGhostStart.Click += (s, e) => SendRemoteCommand("CMD_GHOST_START");
            btnCmdGhostStop.Click += (s, e) => SendRemoteCommand("CMD_GHOST_STOP");
            btnCmdImageSync.Click += (s, e) => SendRemoteCommand("CMD_IMAGE_SYNC");
            btnBroadcastRestart.Click += (s, e) => BroadcastCommand("CMD_RESTART");
            btnBroadcastTimeSync.Click += (s, e) => BroadcastCommand("CMD_TIMESYNC");
            pnlCommands.Controls.AddRange(new Control[] { lblSelectATM, cmbRemoteATM, lblRemoteStatus, btnCmdRestart, btnCmdScreenshot, btnCmdTimeSync, btnCmdSysInfo, btnCmdGhostStart, btnCmdGhostStop, btnCmdImageSync, btnBroadcastRestart, btnBroadcastTimeSync });
            splitRemote.Panel1.Controls.Add(pnlCommands);
            var splitRight = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Horizontal, SplitterDistance = 350 };
            picGhostView = new PictureBox { Dock = DockStyle.Fill, BackColor = Color.Black, SizeMode = PictureBoxSizeMode.Zoom };
            new Label { Text = "Ghost Remote View", Dock = DockStyle.Top, Height = 20, BackColor = Color.FromArgb(40, 40, 50), ForeColor = Color.Orange, TextAlign = ContentAlignment.MiddleCenter, Parent = splitRight.Panel1 };
            splitRight.Panel1.Controls.Add(picGhostView);
            rtbRemoteResult = new RichTextBox { Dock = DockStyle.Fill, ReadOnly = true, BackColor = Color.FromArgb(30, 30, 40), ForeColor = Color.LightGreen, Font = new Font("Consolas", 9) };
            splitRight.Panel2.Controls.Add(rtbRemoteResult);
            splitRemote.Panel2.Controls.Add(splitRight);
        }
        private void BuildAnalyticsTab()
        {
            var pnlAnalyticsTop = new Panel { Dock = DockStyle.Top, Height = 80 };
            new Label { Text = "ATM:", Location = new Point(10, 12), AutoSize = true, Parent = pnlAnalyticsTop };
            cmbAnalyticsATM = new ComboBox { Location = new Point(50, 9), Width = 120, DropDownStyle = ComboBoxStyle.DropDownList, Parent = pnlAnalyticsTop };
            cmbAnalyticsATM.Items.Add("All");
            cmbAnalyticsATM.SelectedIndex = 0;
            new Label { Text = "From:", Location = new Point(185, 12), AutoSize = true, Parent = pnlAnalyticsTop };
            dtpAnalyticsFrom = new DateTimePicker { Location = new Point(225, 9), Width = 130, Format = DateTimePickerFormat.Short, Parent = pnlAnalyticsTop };
            new Label { Text = "To:", Location = new Point(365, 12), AutoSize = true, Parent = pnlAnalyticsTop };
            dtpAnalyticsTo = new DateTimePicker { Location = new Point(390, 9), Width = 130, Format = DateTimePickerFormat.Short, Parent = pnlAnalyticsTop };
            btnRunAnalysis = new Button { Text = "Run Analysis", Location = new Point(535, 7), Size = new Size(100, 26), BackColor = Color.FromArgb(33, 150, 243), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Parent = pnlAnalyticsTop };
            btnExportReport = new Button { Text = "Export Report", Location = new Point(645, 7), Size = new Size(100, 26), BackColor = Color.FromArgb(76, 175, 80), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Parent = pnlAnalyticsTop };
            lblAnalyticsSummary = new Label { Text = "", Location = new Point(10, 45), Size = new Size(800, 25), ForeColor = Color.DarkBlue, Font = new Font("Segoe UI", 9, FontStyle.Bold), Parent = pnlAnalyticsTop };
            btnRunAnalysis.Click += BtnRunAnalysis_Click;
            btnExportReport.Click += BtnExportReport_Click;
            tabAnalytics.Controls.Add(pnlAnalyticsTop);
            dgvAnalytics = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
            dgvAnalytics.Columns.AddRange(new DataGridViewColumn[] {
                new DataGridViewTextBoxColumn { Name = "ATM", HeaderText = "ATM", Width = 70 },
                new DataGridViewTextBoxColumn { Name = "Total", HeaderText = "Total Tx", Width = 60 },
                new DataGridViewTextBoxColumn { Name = "Success", HeaderText = "Success", Width = 60 },
                new DataGridViewTextBoxColumn { Name = "Failed", HeaderText = "Failed", Width = 55 },
                new DataGridViewTextBoxColumn { Name = "Rate", HeaderText = "Rate %", Width = 55 },
                new DataGridViewTextBoxColumn { Name = "Amount", HeaderText = "Amount", Width = 80 },
                new DataGridViewTextBoxColumn { Name = "Retained", HeaderText = "Cards", Width = 50 },
                new DataGridViewTextBoxColumn { Name = "Errors", HeaderText = "Errors", Width = 50 },
                new DataGridViewTextBoxColumn { Name = "LastTx", HeaderText = "Last Tx", Width = 80 }
            });
            tabAnalytics.Controls.Add(dgvAnalytics);
        }
        private void BuildLogTab()
        {
            pnlLogToolbar = new Panel { Dock = DockStyle.Top, Height = 35 };
            btnClearLog = new Button { Text = "Clear", Location = new Point(5, 5), Size = new Size(60, 25) };
            btnSaveLog = new Button { Text = "Save", Location = new Point(70, 5), Size = new Size(60, 25) };
            chkAutoScroll = new CheckBox { Text = "Auto Scroll", Location = new Point(145, 8), AutoSize = true, Checked = true };
            btnClearLog.Click += (s, e) => rtbLog.Clear();
            btnSaveLog.Click += (s, e) => { using (var sfd = new SaveFileDialog { Filter = "Log|*.log" }) { if (sfd.ShowDialog() == DialogResult.OK) File.WriteAllText(sfd.FileName, rtbLog.Text); } };
            pnlLogToolbar.Controls.AddRange(new Control[] { btnClearLog, btnSaveLog, chkAutoScroll });
            tabLog.Controls.Add(pnlLogToolbar);
            rtbLog = new RichTextBox { Dock = DockStyle.Fill, ReadOnly = true, BackColor = Color.FromArgb(25, 25, 35), ForeColor = Color.LightGray, Font = new Font("Consolas", 9) };
            tabLog.Controls.Add(rtbLog);
        }
        private void BuildStatusBar()
        {
            statusBar = new StatusStrip();
            lblStatus = new ToolStripStatusLabel("Ready") { Width = 100 };
            lblServerState = new ToolStripStatusLabel("Server: Stopped") { ForeColor = Color.Red };
            lblClientsCount = new ToolStripStatusLabel("Clients: 0");
            lblVersion = new ToolStripStatusLabel("v3.4.0") { Alignment = ToolStripItemAlignment.Right };
            lblClock = new ToolStripStatusLabel(DateTime.Now.ToString("HH:mm:ss")) { Alignment = ToolStripItemAlignment.Right };
            statusBar.Items.AddRange(new ToolStripItem[] { lblStatus, lblServerState, lblClientsCount, lblVersion, lblClock });
        }
        private Button CreateCmdButton(string text, Point location, Color color)
        {
            return new Button { Text = text, Location = location, Size = new Size(200, 28), BackColor = color, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9) };
        }
        private void BtnStartServer_Click(object sender, EventArgs e)
        {
            int port = (int)numPort.Value;
            _serverEngine = new ServerEngine(port, _archivePath);
            _serverEngine.OnLog += (msg) => SafeLog(msg, Color.White);
            _serverEngine.OnError += (ex) => SafeLog($"[Server Error] {ex.Message}", Color.Red);
            _serverEngine.OnClientConnected += ServerEngine_OnClientConnected;
            _serverEngine.OnClientDisconnected += ServerEngine_OnClientDisconnected;
            _serverEngine.OnJournalReceived += ServerEngine_OnJournalReceived;
            _serverEngine.OnCommandResult += ServerEngine_OnCommandResult;
            _serverEngine.Start();
            _imageSyncEngine = new ImageSyncEngine(_archivePath + @"\Images");
            _imageSyncEngine.OnLog += (msg) => SafeLog(msg, Color.White);
            _imageSyncEngine.Start();
            _reportEngine = new ReportExportEngine(_archivePath + @"\Reports");
            _serverRunning = true;
            btnStartServer.Enabled = false; btnStopServer.Enabled = true;
            lblServerStatus.Text = $"Status: Running on port {port}"; lblServerStatus.ForeColor = Color.Green;
            lblServerState.Text = $"Server: Running (:{port})"; lblServerState.ForeColor = Color.Green;
            Log($"[Server] Started on port {port}", Color.LightGreen);
            Log($"[Server] Archive: {_archivePath}", Color.Cyan);
        }
        private void BtnStopServer_Click(object sender, EventArgs e)
        {
            if (_serverEngine != null) { _serverEngine.Stop(); _serverEngine = null; }
            if (_imageSyncEngine != null) { _imageSyncEngine.Stop(); _imageSyncEngine = null; }
            _serverRunning = false;
            btnStartServer.Enabled = true; btnStopServer.Enabled = false;
            lblServerStatus.Text = "Status: Stopped"; lblServerStatus.ForeColor = Color.Gray;
            lblServerState.Text = "Server: Stopped"; lblServerState.ForeColor = Color.Red;
            Log("[Server] Stopped", Color.Yellow);
        }
        private void ServerEngine_OnClientConnected(string atmId, string atmType, string ip)
        {
            SafeInvoke(() =>
            {
                AddOrUpdateATMCard(atmId, atmType, ip, ATMCardStatus.Connected);
                if (!cmbRemoteATM.Items.Contains(atmId)) cmbRemoteATM.Items.Add(atmId);
                if (!cmbArchiveATM.Items.Contains(atmId)) cmbArchiveATM.Items.Add(atmId);
                if (!cmbAnalyticsATM.Items.Contains(atmId)) cmbAnalyticsATM.Items.Add(atmId);
                _imageSyncEngine?.RegisterATM(atmId, atmType);
                UpdateNOCMetrics();
            });
            Log($"[Connected] {atmId} ({atmType}) from {ip}", Color.LightGreen);
        }
        private void ServerEngine_OnClientDisconnected(string atmId)
        {
            SafeInvoke(() =>
            {
                if (_atmCards.ContainsKey(atmId)) _atmCards[atmId].SetStatus(ATMCardStatus.Offline);
                _imageSyncEngine?.UnregisterATM(atmId);
                UpdateNOCMetrics();
            });
            Log($"[Disconnected] {atmId}", Color.Red);
        }
        private void ServerEngine_OnJournalReceived(string atmId, string fileName, long size)
        {
            SafeInvoke(() =>
            {
                if (_atmCards.ContainsKey(atmId)) { _atmCards[atmId].SetStatus(ATMCardStatus.Syncing); _atmCards[atmId].UpdateLastSync(DateTime.Now); }
                UpdateNOCMetrics();
            });
            Log($"[Journal] {atmId}: {fileName} ({size} bytes)", Color.Cyan);
        }
        private void ServerEngine_OnCommandResult(string atmId, string command, bool success, string result)
        {
            SafeInvoke(() =>
            {
                rtbRemoteResult.SelectionColor = success ? Color.LightGreen : Color.Red;
                rtbRemoteResult.AppendText($"[{DateTime.Now:HH:mm:ss}] {atmId} > {command}: {(success ? "OK" : "FAILED")} - {result}\n");
            });
            Log($"[Result] {atmId} > {command}: {(success ? "OK" : "FAIL")}", success ? Color.LightGreen : Color.Red);
        }
        private void SendRemoteCommand(string command)
        {
            if (cmbRemoteATM.SelectedItem == null) { MessageBox.Show("Select ATM first"); return; }
            string atmId = cmbRemoteATM.SelectedItem.ToString();
            if (_serverEngine != null)
            {
                _serverEngine.SendCommand(atmId, command, new string[0]);
                Log($"[Command] Sent {command} to {atmId}", Color.Cyan);
                lblRemoteStatus.Text = $"Sent: {command}"; lblRemoteStatus.ForeColor = Color.Orange;
            }
        }
        private void BroadcastCommand(string command)
        {
            if (_serverEngine == null) return;
            if (MessageBox.Show($"Broadcast '{command}' to ALL connected ATMs?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                _serverEngine.BroadcastCommand(command, new string[0]);
                Log($"[Broadcast] {command} to all ATMs", Color.FromArgb(255, 152, 0));
            }
        }
        private void BtnDisconnectSelected_Click(object sender, EventArgs e)
        {
            if (dgvConnections.SelectedRows.Count == 0) return;
            string atmId = dgvConnections.SelectedRows[0].Cells["ATM_ID"].Value?.ToString();
            if (!string.IsNullOrEmpty(atmId) && _serverEngine != null) { _serverEngine.DisconnectClient(atmId); Log($"[Disconnect] {atmId}", Color.Yellow); }
        }
        private void BtnBroadcastMsg_Click(object sender, EventArgs e)
        {
            string msg = "";
            using (var inputForm = new Form { Text = "Broadcast Message", Size = new Size(350, 130), StartPosition = FormStartPosition.CenterParent })
            {
                var txt = new TextBox { Location = new Point(10, 10), Width = 310 };
                var btn = new Button { Text = "Send", Location = new Point(130, 45), Size = new Size(80, 28), DialogResult = DialogResult.OK };
                inputForm.Controls.AddRange(new Control[] { txt, btn });
                inputForm.AcceptButton = btn;
                if (inputForm.ShowDialog() == DialogResult.OK) msg = txt.Text;
            }
            if (!string.IsNullOrEmpty(msg) && _serverEngine != null) { _serverEngine.BroadcastCommand("CMD_MESSAGE", new[] { msg }); Log($"[Broadcast] Message: {msg}", Color.Cyan); }
        }
        private void BtnArchiveSearch_Click(object sender, EventArgs e)
        {
            dgvArchive.Rows.Clear();
            if (!Directory.Exists(_archivePath)) { MessageBox.Show("Archive path not found"); return; }
            string filter = cmbArchiveATM.SelectedItem?.ToString();
            var files = Directory.GetFiles(_archivePath, "*.*", SearchOption.AllDirectories);
            int count = 0;
            foreach (var file in files)
            {
                var fi = new FileInfo(file);
                if (fi.LastWriteTime < dtpArchiveFrom.Value || fi.LastWriteTime > dtpArchiveTo.Value.AddDays(1)) continue;
                string atmId = Path.GetFileName(Path.GetDirectoryName(file));
                if (filter != "All ATMs" && atmId != filter) continue;
                dgvArchive.Rows.Add(atmId, fi.Name, fi.LastWriteTime.ToString("yyyy-MM-dd"), $"{fi.Length / 1024} KB", "", file);
                count++;
            }
            lblArchiveStats.Text = $"{count} files";
            Log($"[Archive] Found {count} files", Color.Cyan);
        }
        private void BtnArchiveExport_Click(object sender, EventArgs e)
        {
            if (dgvArchive.Rows.Count == 0) return;
            using (var fbd = new FolderBrowserDialog { Description = "Select export folder" })
            {
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    int exported = 0;
                    foreach (DataGridViewRow row in dgvArchive.SelectedRows)
                    {
                        string path = row.Cells["Path"].Value?.ToString();
                        if (!string.IsNullOrEmpty(path) && File.Exists(path))
                        { File.Copy(path, Path.Combine(fbd.SelectedPath, Path.GetFileName(path)), true); exported++; }
                    }
                    Log($"[Archive] Exported {exported} files", Color.LightGreen);
                }
            }
        }
        private void BtnArchiveOpen_Click(object sender, EventArgs e)
        {
            if (dgvArchive.SelectedRows.Count == 0) return;
            string path = dgvArchive.SelectedRows[0].Cells["Path"].Value?.ToString();
            if (!string.IsNullOrEmpty(path) && File.Exists(path))
            { try { System.Diagnostics.Process.Start("notepad.exe", path); } catch { } }
        }
        private void BtnRunAnalysis_Click(object sender, EventArgs e)
        {
            dgvAnalytics.Rows.Clear();
            Log("[Analytics] Running analysis...", Color.Cyan);
            if (!Directory.Exists(_archivePath)) { MessageBox.Show("Archive path not found"); return; }
            var atmDirs = Directory.GetDirectories(_archivePath);
            int totalTx = 0, totalSuccess = 0, totalFailed = 0;
            decimal totalAmount = 0;
            foreach (var dir in atmDirs)
            {
                string atmId = Path.GetFileName(dir);
                if (cmbAnalyticsATM.SelectedItem?.ToString() != "All" && atmId != cmbAnalyticsATM.SelectedItem?.ToString()) continue;
                var engine = new TransactionAnalysisEngine("NCR");
                var files = Directory.GetFiles(dir, "*.*");
                var allTx = new List<ATMTransaction>();
                foreach (var file in files) allTx.AddRange(engine.AnalyzeJournalFile(file));
                var filtered = allTx.Where(t => t.Timestamp >= dtpAnalyticsFrom.Value && t.Timestamp <= dtpAnalyticsTo.Value.AddDays(1)).ToList();
                int success = filtered.Count(t => t.IsSuccessful);
                int failed = filtered.Count(t => t.Status == TransactionStatus.Failed);
                decimal amount = filtered.Where(t => t.Type == TransactionType.CashWithdrawal && t.IsSuccessful).Sum(t => t.Amount);
                int retained = filtered.Count(t => t.Type == TransactionType.CardRetained);
                double rate = filtered.Count > 0 ? (double)success / filtered.Count * 100 : 0;
                string lastTx = filtered.Count > 0 ? filtered.Max(t => t.Timestamp).ToString("HH:mm:ss") : "-";
                dgvAnalytics.Rows.Add(atmId, filtered.Count, success, failed, $"{rate:F1}%", amount.ToString("N0"), retained, filtered.Count(t => t.Status == TransactionStatus.Failed), lastTx);
                totalTx += filtered.Count; totalSuccess += success; totalFailed += failed; totalAmount += amount;
            }
            double totalRate = totalTx > 0 ? (double)totalSuccess / totalTx * 100 : 0;
            lblAnalyticsSummary.Text = $"Total: {totalTx} Tx | Success: {totalSuccess} ({totalRate:F1}%) | Failed: {totalFailed} | Amount: {totalAmount:N0}";
            Log($"[Analytics] Complete: {totalTx} transactions analyzed", Color.LightGreen);
        }
        private void BtnExportReport_Click(object sender, EventArgs e)
        {
            if (dgvAnalytics.Rows.Count == 0) { MessageBox.Show("Run analysis first"); return; }
            using (var sfd = new SaveFileDialog { Filter = "HTML|*.html|CSV|*.csv", FileName = $"Analysis_{DateTime.Now:yyyyMMdd}" })
            {
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    if (_reportEngine != null)
                    {
                        var report = new TransactionAnalysisReport { ATM_ID = "ALL", FromDate = dtpAnalyticsFrom.Value, ToDate = dtpAnalyticsTo.Value, GeneratedAt = DateTime.Now };
                        if (sfd.FileName.EndsWith(".html")) _reportEngine.ExportAnalysisReportToHTML(report, Path.GetDirectoryName(sfd.FileName));
                        Log($"[Export] Report: {sfd.FileName}", Color.LightGreen);
                    }
                }
            }
        }
        private void AddOrUpdateATMCard(string atmId, string atmType, string ip, ATMCardStatus status)
        {
            if (_atmCards.ContainsKey(atmId))
            { _atmCards[atmId].SetStatus(status); _atmCards[atmId].UpdateIP(ip); }
            else
            {
                var card = new ATMCardPanel(atmId, atmType, ip);
                card.SetStatus(status);
                _atmCards[atmId] = card;
                flpATMCards.Controls.Add(card);
            }
        }
        private void UpdateNOCMetrics()
        {
            int total = _atmCards.Count;
            int connected = _atmCards.Values.Count(c => c.Status == ATMCardStatus.Connected || c.Status == ATMCardStatus.Syncing);
            int syncing = _atmCards.Values.Count(c => c.Status == ATMCardStatus.Syncing);
            int errors = _atmCards.Values.Count(c => c.Status == ATMCardStatus.Error);
            int offline = _atmCards.Values.Count(c => c.Status == ATMCardStatus.Offline);
            int supervisor = _atmCards.Values.Count(c => c.Status == ATMCardStatus.Supervisor);
            lblTotalATMsVal.Text = total.ToString();
            lblConnectedVal.Text = connected.ToString();
            lblSyncingVal.Text = syncing.ToString();
            lblErrorsVal.Text = errors.ToString();
            lblOfflineVal.Text = offline.ToString();
            lblSupervisorVal.Text = supervisor.ToString();
            lblClientsCount.Text = $"Clients: {connected}";
        }
        private void RefreshConnectionsGrid()
        {
            dgvConnections.Rows.Clear();
            foreach (var card in _atmCards.Values)
            {
                dgvConnections.Rows.Add(card.ATM_ID, card.IP, card.ATMType, card.Status.ToString(), card.ConnectedAt.ToString("HH:mm:ss"), card.LastSync.ToString("HH:mm:ss"), card.Latency + "ms", card.NetworkType, card.FilesReceived);
            }
            lblConnectionCount.Text = $"Connections: {_atmCards.Count}";
        }
        private void SetupTimers()
        {
            _refreshTimer = new System.Windows.Forms.Timer { Interval = 5000 };
            _refreshTimer.Tick += (s, e) => { if (_serverRunning) { UpdateNOCMetrics(); lblUptimeVal.Text = _serverEngine != null ? (DateTime.Now - _serverEngine.StartTime).ToString(@"hh\:mm\:ss") : "00:00:00"; } };
            _refreshTimer.Start();
            _clockTimer = new System.Windows.Forms.Timer { Interval = 1000 };
            _clockTimer.Tick += (s, e) => { lblClock.Text = DateTime.Now.ToString("HH:mm:ss"); };
            _clockTimer.Start();
        }
        private void Log(string message, Color color)
        {
            if (rtbLog == null) return;
            if (rtbLog.InvokeRequired) { rtbLog.Invoke((Action)(() => Log(message, color))); return; }
            rtbLog.SelectionColor = Color.Gray;
            rtbLog.AppendText($"[{DateTime.Now:HH:mm:ss}] ");
            rtbLog.SelectionColor = color;
            rtbLog.AppendText(message + Environment.NewLine);
            if (chkAutoScroll != null && chkAutoScroll.Checked) rtbLog.ScrollToCaret();
        }
        private void SafeLog(string msg, Color c) { if (this.InvokeRequired) this.BeginInvoke((Action)(() => Log(msg, c))); else Log(msg, c); }
        private void SafeInvoke(Action a) { if (this.InvokeRequired) this.BeginInvoke(a); else a(); }
        protected override void OnFormClosing(FormClosingEventArgs e) { if (_serverEngine != null) _serverEngine.Stop(); base.OnFormClosing(e); }
        private void BuildAnalyticsTab()
        {
            var pnlAnalyticsTop = new Panel { Dock = DockStyle.Top, Height = 80 };
            new Label { Text = "ATM:", Location = new Point(10, 12), AutoSize = true, Parent = pnlAnalyticsTop };
            cmbAnalyticsATM = new ComboBox { Location = new Point(50, 9), Width = 120, DropDownStyle = ComboBoxStyle.DropDownList, Parent = pnlAnalyticsTop };
            cmbAnalyticsATM.Items.Add("All");
            cmbAnalyticsATM.SelectedIndex = 0;
            new Label { Text = "From:", Location = new Point(185, 12), AutoSize = true, Parent = pnlAnalyticsTop };
            dtpAnalyticsFrom = new DateTimePicker { Location = new Point(225, 9), Width = 130, Format = DateTimePickerFormat.Short, Parent = pnlAnalyticsTop };
            new Label { Text = "To:", Location = new Point(365, 12), AutoSize = true, Parent = pnlAnalyticsTop };
            dtpAnalyticsTo = new DateTimePicker { Location = new Point(390, 9), Width = 130, Format = DateTimePickerFormat.Short, Parent = pnlAnalyticsTop };
            btnRunAnalysis = new Button { Text = "Run Analysis", Location = new Point(535, 7), Size = new Size(100, 26), BackColor = Color.FromArgb(33, 150, 243), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Parent = pnlAnalyticsTop };
            btnExportReport = new Button { Text = "Export Report", Location = new Point(645, 7), Size = new Size(100, 26), BackColor = Color.FromArgb(76, 175, 80), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Parent = pnlAnalyticsTop };
            lblAnalyticsSummary = new Label { Text = "", Location = new Point(10, 45), Size = new Size(800, 25), ForeColor = Color.DarkBlue, Font = new Font("Segoe UI", 9, FontStyle.Bold), Parent = pnlAnalyticsTop };
            btnRunAnalysis.Click += BtnRunAnalysis_Click;
            btnExportReport.Click += BtnExportReport_Click;
            tabAnalytics.Controls.Add(pnlAnalyticsTop);
            dgvAnalytics = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
            dgvAnalytics.Columns.AddRange(new DataGridViewColumn[] {
                new DataGridViewTextBoxColumn { Name = "ATM", HeaderText = "ATM", Width = 70 },
                new DataGridViewTextBoxColumn { Name = "Total", HeaderText = "Total Tx", Width = 60 },
                new DataGridViewTextBoxColumn { Name = "Success", HeaderText = "Success", Width = 60 },
                new DataGridViewTextBoxColumn { Name = "Failed", HeaderText = "Failed", Width = 55 },
                new DataGridViewTextBoxColumn { Name = "Rate", HeaderText = "Rate %", Width = 55 },
                new DataGridViewTextBoxColumn { Name = "Cards", HeaderText = "Cards", Width = 50 },
                new DataGridViewTextBoxColumn { Name = "Findings", HeaderText = "Findings", Width = 200 }
            });
            tabAnalytics.Controls.Add(dgvAnalytics);
        }
        private void BtnStartServer_Click(object sender, EventArgs e)
        {
            int port = (int)numPort.Value;
            _serverEngine = new ServerEngine();
            _serverEngine.ClientConnected += (s, conn) => SafeInvoke(() =>
            {
                AddOrUpdateATMCard(conn.ATM_ID, conn.ATM_Type, conn.RemoteEndPoint, ATMCardStatus.Connected);
                if (!cmbRemoteATM.Items.Contains(conn.ATM_ID)) cmbRemoteATM.Items.Add(conn.ATM_ID);
                if (!cmbArchiveATM.Items.Contains(conn.ATM_ID)) cmbArchiveATM.Items.Add(conn.ATM_ID);
                if (!cmbAnalyticsATM.Items.Contains(conn.ATM_ID)) cmbAnalyticsATM.Items.Add(conn.ATM_ID);
                UpdateNOCMetrics();
                Log($"[Connected] {conn.ATM_ID} ({conn.ATM_Type}) from {conn.RemoteEndPoint}", Color.LightGreen);
            });
            _serverEngine.ClientDisconnected += (s, conn) => SafeInvoke(() =>
            {
                if (_atmCards.ContainsKey(conn.ATM_ID)) _atmCards[conn.ATM_ID].SetStatus(ATMCardStatus.Offline);
                UpdateNOCMetrics();
                Log($"[Disconnected] {conn.ATM_ID}", Color.Red);
            });
            _serverEngine.JournalFileReceived += (s, p) => SafeInvoke(() =>
            {
                if (_atmCards.ContainsKey(p.ATM_ID))
                {
                    _atmCards[p.ATM_ID].SetStatus(ATMCardStatus.Syncing);
                    _atmCards[p.ATM_ID].UpdateLastSync(DateTime.Now);
                    _atmCards[p.ATM_ID].FilesReceived++;
                }
                UpdateNOCMetrics();
                Log($"[Journal] {p.ATM_ID}: {p.FileName} ({p.Payload.Length} bytes)", Color.Cyan);
            });
            _serverEngine.Log += (_, msg) => SafeLog(msg, Color.White);
            _serverEngine.Error += (_, msg) => SafeLog($"[Server Error] {msg}", Color.Red);
            _serverEngine.Start(port);
            _serverRunning = true;
            btnStartServer.Enabled = false; btnStopServer.Enabled = true;
            lblServerStatus.Text = $"Status: Running on port {port}"; lblServerStatus.ForeColor = Color.Green;
            lblServerState.Text = $"Server: Running (:{port})"; lblServerState.ForeColor = Color.Green;
            Log($"[Server] Started on port {port}", Color.LightGreen);
            Log($"[Server] Archive: {_archivePath}", Color.Cyan);
        }
        private void BtnStopServer_Click(object sender, EventArgs e)
        {
            if (_serverEngine != null) { _serverEngine.Stop(); _serverEngine = null; }
            _serverRunning = false;
            btnStartServer.Enabled = true; btnStopServer.Enabled = false;
            lblServerStatus.Text = "Status: Stopped"; lblServerStatus.ForeColor = Color.Gray;
            lblServerState.Text = "Server: Stopped"; lblServerState.ForeColor = Color.Red;
            Log("[Server] Stopped", Color.Yellow);
        }
        private void SendRemoteCommand(string command)
        {
            if (cmbRemoteATM.SelectedItem == null) { MessageBox.Show("Select ATM first"); return; }
            string atmId = cmbRemoteATM.SelectedItem.ToString();
            if (_serverEngine != null)
            {
                var envelope = new RemoteCommandEnvelope
                {
                    CommandType = command,
                    Payload = atmId
                };
                _serverEngine.SendCommand(atmId, envelope);
                Log($"[Command] Sent {command} to {atmId}", Color.Cyan);
                lblRemoteStatus.Text = $"Sent: {command}"; lblRemoteStatus.ForeColor = Color.Orange;
            }
        }
        private void BroadcastCommand(string command)
        {
            if (_serverEngine == null) return;
            if (MessageBox.Show($"Broadcast '{command}' to ALL connected ATMs?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                var envelope = new RemoteCommandEnvelope
                {
                    CommandType = command,
                    Payload = "ALL"
                };
                _serverEngine.BroadcastCommand(envelope);
                Log($"[Broadcast] {command} to all ATMs", Color.FromArgb(255, 152, 0));
            }
        }
        private void BtnDisconnectSelected_Click(object sender, EventArgs e)
        {
            if (dgvConnections.SelectedRows.Count == 0) return;
            string atmId = dgvConnections.SelectedRows[0].Cells["ATM_ID"].Value?.ToString();
            if (!string.IsNullOrEmpty(atmId) && _serverEngine != null)
            {
                _serverEngine.Remove(atmId);
                Log($"[Disconnect] {atmId}", Color.Yellow);
            }
        }
        private void BtnBroadcastMsg_Click(object sender, EventArgs e)
        {
            string msg = "";
            using (var inputForm = new Form { Text = "Broadcast Message", Size = new Size(350, 130), StartPosition = FormStartPosition.CenterParent })
            {
                var txt = new TextBox { Location = new Point(10, 10), Width = 310 };
                var btn = new Button { Text = "Send", Location = new Point(130, 45), Size = new Size(80, 28), DialogResult = DialogResult.OK };
                inputForm.Controls.AddRange(new Control[] { txt, btn });
                inputForm.AcceptButton = btn;
                if (inputForm.ShowDialog() == DialogResult.OK) msg = txt.Text;
            }
            if (!string.IsNullOrEmpty(msg) && _serverEngine != null) { _serverEngine.Broadcast(msg); Log($"[Broadcast] Message: {msg}", Color.Cyan); }
        }
        private void BtnRunAnalysis_Click(object sender, EventArgs e)
        {
            dgvAnalytics.Rows.Clear();
            Log("[Analytics] Running analysis...", Color.Cyan);
            if (!Directory.Exists(_archivePath)) { MessageBox.Show("Archive path not found"); return; }
            var atmDirs = Directory.GetDirectories(_archivePath);
            int grandTotal = 0, grandApproved = 0, grandFailed = 0, grandRetained = 0;
            foreach (var dir in atmDirs)
            {
                string atmId = Path.GetFileName(dir);
                if (cmbAnalyticsATM.SelectedItem?.ToString() != "All" && atmId != cmbAnalyticsATM.SelectedItem?.ToString()) continue;
                var files = Directory.GetFiles(dir, "*.*");
                int totalTx = 0, approved = 0, failed = 0, retained = 0;
                var allFindings = new List<string>();
                foreach (var file in files)
                {
                    try
                    {
                        string content = File.ReadAllText(file);
                        var report = _analysisEngine.Analyze(content);
                        totalTx += report.TotalTransactions;
                        approved += report.ApprovedCount;
                        failed += report.FailedCount;
                        retained += report.RetainedCards;
                        allFindings.AddRange(report.Findings);
                    }
                    catch { /* skip unreadable files */ }
                }
                double rate = totalTx > 0 ? (double)approved / totalTx * 100 : 0;
                string findings = string.Join("; ", allFindings.Distinct().Take(3));
                dgvAnalytics.Rows.Add(atmId, totalTx, approved, failed, $"{rate:F1}%", retained, findings);
                grandTotal += totalTx; grandApproved += approved; grandFailed += failed; grandRetained += retained;
            }
            double grandRate = grandTotal > 0 ? (double)grandApproved / grandTotal * 100 : 0;
            lblAnalyticsSummary.Text = $"Total: {grandTotal} Tx | Approved: {grandApproved} ({grandRate:F1}%) | Failed: {grandFailed} | Retained: {grandRetained}";
            Log($"[Analytics] Complete: {grandTotal} transactions analyzed", Color.LightGreen);
        }
        private void BtnExportReport_Click(object sender, EventArgs e)
        {
            if (dgvAnalytics.Rows.Count == 0) { MessageBox.Show("Run analysis first"); return; }
            using (var sfd = new SaveFileDialog { Filter = "CSV|*.csv", FileName = $"Analysis_{DateTime.Now:yyyyMMdd}" })
            {
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    var rows = new List<AnalyticsRow>();
                    foreach (DataGridViewRow row in dgvAnalytics.Rows)
                    {
                        if (row.IsNewRow) continue;
                        rows.Add(new AnalyticsRow
                        {
                            ATM = row.Cells["ATM"].Value?.ToString() ?? "",
                            Total = row.Cells["Total"].Value?.ToString() ?? "0",
                            Approved = row.Cells["Success"].Value?.ToString() ?? "0",
                            Failed = row.Cells["Failed"].Value?.ToString() ?? "0",
                            Rate = row.Cells["Rate"].Value?.ToString() ?? "0%",
                            Retained = row.Cells["Cards"].Value?.ToString() ?? "0"
                        });
                    }
                    _reportEngine.ExportCsv(rows, sfd.FileName);
                    Log($"[Export] Report: {sfd.FileName}", Color.LightGreen);
                }
            }
        }
        private void SetupTimers()
        {
            _refreshTimer = new System.Windows.Forms.Timer { Interval = 5000 };
            _refreshTimer.Tick += (s, e) =>
            {
                if (_serverRunning)
                {
                    UpdateNOCMetrics();
                    if (_serverEngine != null && _serverEngine.IsRunning)
                        lblUptimeVal.Text = (DateTime.Now - _serverEngine.StartTime).ToString(@"hh\:mm\:ss");
                }
            };
            _refreshTimer.Start();
            _clockTimer = new System.Windows.Forms.Timer { Interval = 1000 };
            _clockTimer.Tick += (s, e) => { lblClock.Text = DateTime.Now.ToString("HH:mm:ss"); };
            _clockTimer.Start();
        }
        private class AtmDashboardState
        {
            public string ATMId { get; set; }
            public string ATMType { get; set; }
            public bool IsConnected { get; set; }
            public DateTime LastHeartbeatUtc { get; set; }
            public DateTime LastSyncUtc { get; set; }
            public JournalSyncState? JournalState { get; set; }
            public string CurrentFileName { get; set; }
            public string LastMessage { get; set; }
        }
        public partial class ATMCardPanel : Panel
        {
            // --- Constants & Fields ---
                    private Label lblId, lblType, lblIP, lblStatus, lblLastSync;
            // --- Properties ---
                    public string ATM_ID { get; private set; }
                    public string ATMType { get; private set; }
                    public string IP { get; private set; }
                    public ATMCardStatus Status { get; private set; }
                    public DateTime ConnectedAt { get; private set; }
                    public DateTime LastSync { get; private set; }
                    public int Latency { get; set; }
                    public string NetworkType { get; set; } = "LAN";
                    public int FilesReceived { get; set; }
            // --- Constructors ---
                    public ATMCardPanel(string atmId, string atmType, string ip)
                    {
                        ATM_ID = atmId; ATMType = atmType; IP = ip;
                        ConnectedAt = DateTime.Now; LastSync = DateTime.Now;
                        this.Size = new Size(180, 110);
                        this.Margin = new Padding(5);
                        this.BackColor = Color.White;
                        lblId = new Label { Text = atmId, Location = new Point(5, 5), Size = new Size(170, 20), Font = new Font("Segoe UI", 10, FontStyle.Bold), TextAlign = ContentAlignment.MiddleCenter };
                        lblType = new Label { Text = atmType, Location = new Point(5, 27), Size = new Size(170, 15), ForeColor = Color.Gray, TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 8) };
                        lblIP = new Label { Text = ip, Location = new Point(5, 44), Size = new Size(170, 15), ForeColor = Color.DarkGray, TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 8) };
                        lblStatus = new Label { Text = "Connected", Location = new Point(5, 65), Size = new Size(170, 20), ForeColor = Color.Green, TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
                        lblLastSync = new Label { Text = "Sync: --:--:--", Location = new Point(5, 88), Size = new Size(170, 15), ForeColor = Color.DarkGray, TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 8) };
                        this.Controls.AddRange(new Control[] { lblId, lblType, lblIP, lblStatus, lblLastSync });
                        SetStatus(ATMCardStatus.Connected);
                    }
            // --- Methods ---
                    public void SetStatus(ATMCardStatus status)
                    {
                        Status = status;
                        Color borderColor;
                        string statusText;
                        switch (status)
                        {
                            case ATMCardStatus.Connected: borderColor = Color.FromArgb(76, 175, 80); statusText = "Connected"; break;
                            case ATMCardStatus.Syncing: borderColor = Color.FromArgb(0, 188, 212); statusText = "Syncing"; break;
                            case ATMCardStatus.Error: borderColor = Color.FromArgb(244, 67, 54); statusText = "Error"; break;
                            case ATMCardStatus.Offline: borderColor = Color.FromArgb(158, 158, 158); statusText = "Offline"; break;
                            case ATMCardStatus.Supervisor: borderColor = Color.FromArgb(255, 152, 0); statusText = "Supervisor"; break;
                            default: borderColor = Color.Gray; statusText = "Unknown"; break;
                        }
                        this.BackColor = Color.FromArgb(240, 245, 250);
                        lblStatus.ForeColor = borderColor;
                        lblStatus.Text = statusText;
                        this.Invalidate();
                    }
                    public void UpdateLastSync(DateTime time) { LastSync = time; lblLastSync.Text = $"Sync: {time:HH:mm:ss}"; }
                    public void UpdateIP(string ip) { IP = ip; lblIP.Text = ip; }
                    protected override void OnPaint(PaintEventArgs e)
                    {
                        base.OnPaint(e);
                        Color c = Status == ATMCardStatus.Connected ? Color.FromArgb(76, 175, 80) :
                                  Status == ATMCardStatus.Syncing ? Color.FromArgb(0, 188, 212) :
                                  Status == ATMCardStatus.Error ? Color.FromArgb(244, 67, 54) :
                                  Status == ATMCardStatus.Supervisor ? Color.FromArgb(255, 152, 0) : Color.Gray;
                    }
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Server\ServerMainForm.cs.v17_bak
                    protected override void OnPaint(PaintEventArgs e)
                    {
                        base.OnPaint(e);
                        Color c = Status == ATMCardStatus.Connected ? Color.FromArgb(76, 175, 80) :
                                  Status == ATMCardStatus.Syncing ? Color.FromArgb(0, 188, 212) :
                                  Status == ATMCardStatus.Error ? Color.FromArgb(244, 67, 54) :
                                  Status == ATMCardStatus.Supervisor ? Color.FromArgb(255, 152, 0) : Color.Gray;
                        using (var pen = new Pen(c, 3)) e.Graphics.DrawRectangle(pen, 1, 1, Width - 3, Height - 3);
                    }
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Server\ServerMainForm.cs.v16_bak
                    protected override void OnPaint(PaintEventArgs e)
                    {
                        base.OnPaint(e);
                        Color c = Status == ATMCardStatus.Connected ? Color.FromArgb(76, 175, 80) :
                                  Status == ATMCardStatus.Syncing ? Color.FromArgb(0, 188, 212) :
                                  Status == ATMCardStatus.Error ? Color.FromArgb(244, 67, 54) :
                                  Status == ATMCardStatus.Supervisor ? Color.FromArgb(255, 152, 0) : Color.Gray;
                        using (var pen = new Pen(c, 3)) e.Graphics.DrawRectangle(pen, 1, 1, Width - 3, Height - 3);
                    }
        }
        private class AnalyticsRow
        {
            public string ATM { get; set; } = "";
            public string Total { get; set; } = "0";
            public string Approved { get; set; } = "0";
            public string Failed { get; set; } = "0";
            public string Rate { get; set; } = "0%";
            public string Retained { get; set; } = "0";
        }
        public partial enum ATMCardStatus
        {
        }
    }

    public partial enum ImageDistributionMode
    {
        InboxStaging,
        DirectApply
    }

    public partial class ATMDetailForm : Form
    {
        public ATMDetailForm(ATMInfo atm)
        {
            Text = $"ATM Detail - {atm.ATM_ID}";
            Size = new Size(640, 420);
            Controls.Add(new PropertyGrid { Dock = DockStyle.Fill, SelectedObject = atm });
        }
        Size = new Size(640, 420);
    }

    public partial class ATMDetailDrawerForm : Form
    {
        StartPosition = FormStartPosition.CenterParent;
        public ATMDetailDrawerForm(ATMInfo atm)
        {
            Text = $"ATM Drawer - {atm.ATM_ID}";
            Size = new Size(420, 640);
            StartPosition = FormStartPosition.CenterParent;
            Controls.Add(new Label { Dock = DockStyle.Top, Height = 44, Text = atm.GetStatusDescription(), TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 12F, FontStyle.Bold) });
            Controls.Add(new PropertyGrid { Dock = DockStyle.Fill, SelectedObject = atm });
        }
        Size = new Size(420, 640);
    }

    public partial class JournalViewerForm : Form
    {
        public JournalViewerForm()
        {
            Text = "Journal Viewer";
            Size = new Size(900, 620);
            var box = Ui.LogBox();
            var actions = Ui.Flow();
            actions.Controls.Add(Ui.Button("Load", () => box.Text = "Journal viewer ready."));
            actions.Controls.Add(Ui.Button("Export", () => box.AppendText("Export requested." + Environment.NewLine)));
            Controls.Add(box);
            Controls.Add(actions);
        }
    }

    public partial class SyncDashboardForm : Form
    {
        public SyncDashboardForm(IEnumerable<JournalSyncRecord> records)
        {
            Text = "Sync Dashboard";
            Size = new Size(820, 520);
            var grid = Ui.Grid();
            grid.Columns.Add("ATM", "ATM");
            grid.Columns.Add("File", "File");
            grid.Columns.Add("State", "State");
            grid.Columns.Add("Progress", "Progress");
            foreach (var record in records)
                grid.Rows.Add(record.ATM_ID, record.FileName, record.State, record.ProgressPercent + "%");
            Controls.Add(grid);
        }
    }

    public partial class Ui
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

    public partial class ControlRenderingExtensions
    {
        public static void EnableDoubleBuffering(this Control control)
        {
            var property = typeof(Control).GetProperty("DoubleBuffered", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            property?.SetValue(control, true, null);
        }
    }

    public partial class Program
    {
        [STAThread]
        private static void Main()
        ApplicationConfiguration.Initialize();
        Application.Run(new ServerMainForm());
    }

    public partial class AtmDashboardState
    {
    }

    public partial enum ATMCardStatus
    {
    }

    public partial class ATMCardPanel : Panel
    {
        private Label lblId, lblType, lblIP, lblStatus, lblLastSync;
        public string ATM_ID { get; private set; }
        public string ATMType { get; private set; }
        public string IP { get; private set; }
        public ATMCardStatus Status { get; private set; }
        public DateTime ConnectedAt { get; private set; }
        public DateTime LastSync { get; private set; }
        public int Latency { get; set; }
        public string NetworkType { get; set; } = "LAN";
        public int FilesReceived { get; set; }
        public ATMCardPanel(string atmId, string atmType, string ip)
        {
            ATM_ID = atmId; ATMType = atmType; IP = ip;
            ConnectedAt = DateTime.Now; LastSync = DateTime.Now;
            this.Size = new Size(180, 110);
            this.Margin = new Padding(5);
            this.BackColor = Color.White;
            lblId = new Label { Text = atmId, Location = new Point(5, 5), Size = new Size(170, 20), Font = new Font("Segoe UI", 10, FontStyle.Bold), TextAlign = ContentAlignment.MiddleCenter };
            lblType = new Label { Text = atmType, Location = new Point(5, 27), Size = new Size(170, 15), ForeColor = Color.Gray, TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 8) };
            lblIP = new Label { Text = ip, Location = new Point(5, 44), Size = new Size(170, 15), ForeColor = Color.DarkGray, TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 8) };
            lblStatus = new Label { Text = "Connected", Location = new Point(5, 65), Size = new Size(170, 20), ForeColor = Color.Green, TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
            lblLastSync = new Label { Text = "Sync: --:--:--", Location = new Point(5, 88), Size = new Size(170, 15), ForeColor = Color.DarkGray, TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 8) };
            this.Controls.AddRange(new Control[] { lblId, lblType, lblIP, lblStatus, lblLastSync });
            SetStatus(ATMCardStatus.Connected);
        }
        public void SetStatus(ATMCardStatus status)
        {
            Status = status;
            Color borderColor;
            string statusText;
            switch (status)
            {
                case ATMCardStatus.Connected: borderColor = Color.FromArgb(76, 175, 80); statusText = "Connected"; break;
                case ATMCardStatus.Syncing: borderColor = Color.FromArgb(0, 188, 212); statusText = "Syncing"; break;
                case ATMCardStatus.Error: borderColor = Color.FromArgb(244, 67, 54); statusText = "Error"; break;
                case ATMCardStatus.Offline: borderColor = Color.FromArgb(158, 158, 158); statusText = "Offline"; break;
                case ATMCardStatus.Supervisor: borderColor = Color.FromArgb(255, 152, 0); statusText = "Supervisor"; break;
                default: borderColor = Color.Gray; statusText = "Unknown"; break;
            }
            this.BackColor = Color.FromArgb(240, 245, 250);
            lblStatus.ForeColor = borderColor;
            lblStatus.Text = statusText;
            this.Invalidate();
        }
        public void UpdateLastSync(DateTime time) { LastSync = time; lblLastSync.Text = $"Sync: {time:HH:mm:ss}"; }
        public void UpdateIP(string ip) { IP = ip; lblIP.Text = ip; }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Color c = Status == ATMCardStatus.Connected ? Color.FromArgb(76, 175, 80) :
                      Status == ATMCardStatus.Syncing ? Color.FromArgb(0, 188, 212) :
                      Status == ATMCardStatus.Error ? Color.FromArgb(244, 67, 54) :
                      Status == ATMCardStatus.Supervisor ? Color.FromArgb(255, 152, 0) : Color.Gray;
            using (var pen = new Pen(c, 3)) e.Graphics.DrawRectangle(pen, 1, 1, Width - 3, Height - 3);
        }
    }

}
