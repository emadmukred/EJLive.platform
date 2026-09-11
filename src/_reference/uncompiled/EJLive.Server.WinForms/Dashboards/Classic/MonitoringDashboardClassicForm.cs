#nullable disable

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using EJLive.Core;
using EJLive.Core.Models;
using EJLive.Shared;

namespace EJLive.Server.WinForms.Dashboards.Classic
{
    /// <summary>
    /// لوحة المراقبة المركزية الذكية
    /// تعرض حالة جميع الصرافات في الوقت الحقيقي
    /// مع إحصائيات شاملة وتنبيهات ذكية
    /// </summary>
    public partial class MonitoringDashboardClassicForm : Form
    {
        // ============================================
        // المتغيرات
        // ============================================
        private List<ATMInfo> _atmList;
        private System.Windows.Forms.Timer _refreshTimer;
        private System.Windows.Forms.Timer _alertTimer;
        private int _totalAtms;
        private int _connectedAtms;
        private int _syncingAtms;
        private int _errorAtms;
        private int _offlineAtms;
        private DateTime _lastRefresh;

        // عناصر الواجهة
        private Panel headerPanel;
        private Panel statsPanel;
        private StatusStrip statusStrip;
        private ListView atmListView;
        private ListView alertsListView;
        private TabControl detailsTabControl;

        // بطاقات الإحصائيات
        private Label totalAtmsLabel;
        private Label connectedAtmsLabel;
        private Label syncingAtmsLabel;
        private Label errorAtmsLabel;
        private Label offlineAtmsLabel;

        // ============================================
        // المنشئ
        // ============================================
        public MonitoringDashboardClassicForm()
        {
            InitializeComponent();
            InitializeDashboard();
            InitializeData();
            SetupTimers();
            LoadDemoData();
        }

        /// <summary>
        /// تهيئة لوحة المراقبة
        /// </summary>
        private void InitializeDashboard()
        {
            // إعدادات النافذة
            this.Text = $"EJLive Enterprise - Monitoring Dashboard v{AppConstants.AppVersion}";
            this.Size = new Size(1600, 950);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(30, 30, 35);
            this.Font = new Font("Segoe UI", 9.5f);
            this.DoubleBuffered = true;

            // إنشاء الهيكل
            CreateHeader();
            CreateStatsPanel();
            CreateMainContent();
            CreateStatusBar();
            LightUiTheme.Apply(this);
        }

        /// <summary>
        /// إنشاء الرأس
        /// </summary>
        private void CreateHeader()
        {
            headerPanel = new Panel();
            headerPanel.Dock = DockStyle.Top;
            headerPanel.Height = 70;
            headerPanel.BackColor = Color.FromArgb(20, 20, 25);
            headerPanel.Padding = new Padding(20, 10, 20, 10);

            // العنوان
            Label lblTitle = new Label();
            lblTitle.Text = "EJLive Enterprise - Monitoring Dashboard";
            lblTitle.Font = new Font("Segoe UI", 18f, FontStyle.Bold);
            lblTitle.ForeColor = Color.White;
            lblTitle.Location = new Point(20, 10);
            lblTitle.AutoSize = true;

            // الإصدار
            Label lblVersion = new Label();
            lblVersion.Text = $"v{AppConstants.AppVersion}";
            lblVersion.Font = new Font("Segoe UI", 10f);
            lblVersion.ForeColor = Color.FromArgb(150, 150, 150);
            lblVersion.Location = new Point(20, 45);
            lblVersion.AutoSize = true;

            // وقت آخر تحديث
            Label lblLastRefresh = new Label();
            lblLastRefresh.Text = $"آخر تحديث: {DateTime.Now:HH:mm:ss}";
            lblLastRefresh.Font = new Font("Segoe UI", 10f);
            lblLastRefresh.ForeColor = Color.FromArgb(100, 200, 100);
            lblLastRefresh.Location = new Point(1300, 25);
            lblLastRefresh.AutoSize = true;

            // زر التحديث
            Button btnRefresh = new Button();
            btnRefresh.Text = "⟳ تحديث";
            btnRefresh.FlatStyle = FlatStyle.Flat;
            btnRefresh.BackColor = Color.FromArgb(0, 123, 255);
            btnRefresh.ForeColor = Color.White;
            btnRefresh.Size = new Size(100, 35);
            btnRefresh.Location = new Point(1450, 18);
            btnRefresh.Click += (s, e) => RefreshDashboard();

            headerPanel.Controls.AddRange(new Control[] { lblTitle, lblVersion, lblLastRefresh, btnRefresh });
            this.Controls.Add(headerPanel);
        }

        /// <summary>
        /// إنشاء لوحة الإحصائيات
        /// </summary>
        private void CreateStatsPanel()
        {
            statsPanel = new Panel();
            statsPanel.Dock = DockStyle.Top;
            statsPanel.Height = 130;
            statsPanel.BackColor = Color.FromArgb(25, 25, 30);
            statsPanel.Padding = new Padding(20, 15, 20, 15);

            // بطاقة: إجمالي الصرافات
            Panel cardTotal = CreateDashboardCard("إجمالي الصرافات", "15", Color.FromArgb(0, 123, 255), new Point(20, 10));
            totalAtmsLabel = (Label)cardTotal.Controls[1];

            // بطاقة: المتصلة
            Panel cardConnected = CreateDashboardCard("المتصلة", "11", Color.FromArgb(40, 167, 69), new Point(290, 10));
            connectedAtmsLabel = (Label)cardConnected.Controls[1];

            // بطاقة: قيد المزامنة
            Panel cardSyncing = CreateDashboardCard("قيد المزامنة", "9", Color.FromArgb(255, 193, 7), new Point(560, 10));
            syncingAtmsLabel = (Label)cardSyncing.Controls[1];

            // بطاقة: أخطاء
            Panel cardErrors = CreateDashboardCard("أخطاء", "4", Color.FromArgb(220, 53, 69), new Point(830, 10));
            errorAtmsLabel = (Label)cardErrors.Controls[1];

            // بطاقة: غير متصلة
            Panel cardOffline = CreateDashboardCard("غير متصلة", "4", Color.FromArgb(108, 117, 125), new Point(1100, 10));
            offlineAtmsLabel = (Label)cardOffline.Controls[1];

            statsPanel.Controls.AddRange(new Control[] { cardTotal, cardConnected, cardSyncing, cardErrors, cardOffline });
            this.Controls.Add(statsPanel);
        }

        /// <summary>
        /// إنشاء بطاقة لوحة المراقبة
        /// </summary>
        private Panel CreateDashboardCard(string title, string value, Color accentColor, Point location)
        {
            Panel card = new Panel();
            card.Location = location;
            card.Size = new Size(250, 100);
            card.BackColor = Color.FromArgb(40, 40, 48);
            card.Padding = new Padding(15);

            // شريط ملون علوي
            Panel colorBar = new Panel();
            colorBar.Dock = DockStyle.Top;
            colorBar.Height = 4;
            colorBar.BackColor = accentColor;

            // العنوان
            Label lblTitle = new Label();
            lblTitle.Text = title;
            lblTitle.Font = new Font("Segoe UI", 10f);
            lblTitle.ForeColor = Color.FromArgb(180, 180, 180);
            lblTitle.Location = new Point(15, 15);
            lblTitle.AutoSize = true;

            // القيمة
            Label lblValue = new Label();
            lblValue.Text = value;
            lblValue.Font = new Font("Segoe UI", 28f, FontStyle.Bold);
            lblValue.ForeColor = accentColor;
            lblValue.Location = new Point(15, 40);
            lblValue.AutoSize = true;

            card.Controls.Add(lblValue);
            card.Controls.Add(lblTitle);
            card.Controls.Add(colorBar);

            return card;
        }

        /// <summary>
        /// إنشاء المحتوى الرئيسي
        /// </summary>
        private void CreateMainContent()
        {
            // تقسيم أفقي
            SplitContainer mainSplit = new SplitContainer();
            mainSplit.Dock = DockStyle.Fill;
            mainSplit.Orientation = Orientation.Horizontal;
            mainSplit.SplitterDistance = 450;
            mainSplit.BackColor = Color.FromArgb(30, 30, 35);
            mainSplit.SplitterWidth = 3;

            // الجزء العلوي - قائمة الصرافات
            CreateATMListPanel(mainSplit.Panel1);

            // الجزء السفلي - التفاصيل والتنبيهات
            CreateDetailsPanel(mainSplit.Panel2);

            this.Controls.Add(mainSplit);
        }

        /// <summary>
        /// إنشاء لوحة قائمة الصرافات
        /// </summary>
        private void CreateATMListPanel(Panel parent)
        {
            GroupBox grpATMs = new GroupBox();
            grpATMs.Text = "  حالة الصرافات  ";
            grpATMs.Dock = DockStyle.Fill;
            grpATMs.ForeColor = Color.White;
            grpATMs.Font = new Font("Segoe UI", 10f, FontStyle.Bold);
            grpATMs.Padding = new Padding(5);

            atmListView = new ListView();
            atmListView.Dock = DockStyle.Fill;
            atmListView.View = View.Details;
            atmListView.FullRowSelect = true;
            atmListView.GridLines = true;
            atmListView.BackColor = Color.FromArgb(35, 35, 40);
            atmListView.ForeColor = Color.White;
            atmListView.Font = new Font("Segoe UI", 9.5f);
            atmListView.BorderStyle = BorderStyle.None;

            // الأعمدة
            atmListView.Columns.Add("#", 40);
            atmListView.Columns.Add("معرف الصراف", 100);
            atmListView.Columns.Add("الاسم", 120);
            atmListView.Columns.Add("عنوان IP", 130);
            atmListView.Columns.Add("النوع", 60);
            atmListView.Columns.Add("الحالة", 110);
            atmListView.Columns.Add("الاتصال", 100);
            atmListView.Columns.Add("آخر مزامنة", 150);
            atmListView.Columns.Add("ملفات معلقة", 90);
            atmListView.Columns.Add("معدل النجاح", 90);
            atmListView.Columns.Add("الموقع", 120);
            atmListView.Columns.Add("الفرع", 80);

            // قائمة السياق
            ContextMenuStrip contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add("عرض التفاصيل", null, (s, e) => ShowATMDetails());
            contextMenu.Items.Add("إرسال أمر", null, (s, e) => SendCommandToATM());
            contextMenu.Items.Add(new ToolStripSeparator());
            contextMenu.Items.Add("بدء المزامنة", null, (s, e) => StartATMSync());
            contextMenu.Items.Add("إيقاف المزامنة", null, (s, e) => StopATMSync());
            contextMenu.Items.Add(new ToolStripSeparator());
            contextMenu.Items.Add("قطع الاتصال", null, (s, e) => DisconnectATM());
            atmListView.ContextMenuStrip = contextMenu;

            grpATMs.Controls.Add(atmListView);
            parent.Controls.Add(grpATMs);
        }

        /// <summary>
        /// إنشاء لوحة التفاصيل والتنبيهات
        /// </summary>
        private void CreateDetailsPanel(Panel parent)
        {
            detailsTabControl = new TabControl();
            detailsTabControl.Dock = DockStyle.Fill;
            detailsTabControl.Font = new Font("Segoe UI", 9.5f);

            // تبويب التنبيهات
            TabPage alertsTab = new TabPage("  التنبيهات  ");
            alertsTab.BackColor = Color.FromArgb(35, 35, 40);

            alertsListView = new ListView();
            alertsListView.Dock = DockStyle.Fill;
            alertsListView.View = View.Details;
            alertsListView.FullRowSelect = true;
            alertsListView.GridLines = true;
            alertsListView.BackColor = Color.FromArgb(35, 35, 40);
            alertsListView.ForeColor = Color.White;
            alertsListView.Font = new Font("Segoe UI", 9.5f);

            alertsListView.Columns.Add("الوقت", 150);
            alertsListView.Columns.Add("المستوى", 80);
            alertsListView.Columns.Add("الصراف", 100);
            alertsListView.Columns.Add("الرسالة", 500);
            alertsListView.Columns.Add("الحالة", 80);

            alertsTab.Controls.Add(alertsListView);
            detailsTabControl.TabPages.Add(alertsTab);

            // تبويب الأداء
            TabPage performanceTab = new TabPage("  الأداء  ");
            performanceTab.BackColor = Color.FromArgb(35, 35, 40);

            ListView lvPerformance = new ListView();
            lvPerformance.Dock = DockStyle.Fill;
            lvPerformance.View = View.Details;
            lvPerformance.FullRowSelect = true;
            lvPerformance.GridLines = true;
            lvPerformance.BackColor = Color.FromArgb(35, 35, 40);
            lvPerformance.ForeColor = Color.White;

            lvPerformance.Columns.Add("الصراف", 100);
            lvPerformance.Columns.Add("وقت الاستجابة", 120);
            lvPerformance.Columns.Add("CPU", 80);
            lvPerformance.Columns.Add("الذاكرة", 80);
            lvPerformance.Columns.Add("النطاق الترددي", 120);
            lvPerformance.Columns.Add("الحزم المرسلة", 100);
            lvPerformance.Columns.Add("الحزم المستقبلة", 100);
            lvPerformance.Columns.Add("الأخطاء", 80);

            performanceTab.Controls.Add(lvPerformance);
            detailsTabControl.TabPages.Add(performanceTab);

            // تبويب السجل الحي
            TabPage liveLogTab = new TabPage("  السجل الحي  ");
            liveLogTab.BackColor = Color.FromArgb(20, 20, 25);

            TextBox txtLiveLog = new TextBox();
            txtLiveLog.Dock = DockStyle.Fill;
            txtLiveLog.Multiline = true;
            txtLiveLog.ReadOnly = true;
            txtLiveLog.ScrollBars = ScrollBars.Both;
            txtLiveLog.BackColor = Color.FromArgb(20, 20, 25);
            txtLiveLog.ForeColor = Color.LightGreen;
            txtLiveLog.Font = new Font("Consolas", 9.5f);
            txtLiveLog.WordWrap = false;

            liveLogTab.Controls.Add(txtLiveLog);
            detailsTabControl.TabPages.Add(liveLogTab);

            // تبويب الخريطة
            TabPage mapTab = new TabPage("  خريطة التوزيع  ");
            mapTab.BackColor = Color.FromArgb(35, 35, 40);

            Label lblMapPlaceholder = new Label();
            lblMapPlaceholder.Text = "خريطة توزيع الصرافات\n(يمكن دمج خريطة تفاعلية هنا)";
            lblMapPlaceholder.Dock = DockStyle.Fill;
            lblMapPlaceholder.ForeColor = Color.Gray;
            lblMapPlaceholder.Font = new Font("Segoe UI", 14f);
            lblMapPlaceholder.TextAlign = ContentAlignment.MiddleCenter;

            mapTab.Controls.Add(lblMapPlaceholder);
            detailsTabControl.TabPages.Add(mapTab);

            parent.Controls.Add(detailsTabControl);
        }

        /// <summary>
        /// إنشاء شريط الحالة
        /// </summary>
        private void CreateStatusBar()
        {
            statusStrip = new StatusStrip();
            statusStrip.BackColor = Color.FromArgb(20, 20, 25);

            ToolStripStatusLabel tsslStatus = new ToolStripStatusLabel("● النظام يعمل");
            tsslStatus.ForeColor = Color.LightGreen;

            ToolStripStatusLabel tsslATMs = new ToolStripStatusLabel($"الصرافات: {_totalAtms}");
            tsslATMs.ForeColor = Color.White;
            tsslATMs.BorderSides = ToolStripStatusLabelBorderSides.Left;

            ToolStripStatusLabel tsslConnected = new ToolStripStatusLabel($"متصلة: {_connectedAtms}");
            tsslConnected.ForeColor = Color.LightGreen;
            tsslConnected.BorderSides = ToolStripStatusLabelBorderSides.Left;

            ToolStripStatusLabel tsslErrors = new ToolStripStatusLabel($"أخطاء: {_errorAtms}");
            tsslErrors.ForeColor = Color.Salmon;
            tsslErrors.BorderSides = ToolStripStatusLabelBorderSides.Left;

            ToolStripStatusLabel tsslTime = new ToolStripStatusLabel(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            tsslTime.ForeColor = Color.LightGray;
            tsslTime.Alignment = ToolStripItemAlignment.Right;

            statusStrip.Items.AddRange(new ToolStripItem[] { tsslStatus, tsslATMs, tsslConnected, tsslErrors, tsslTime });
            this.Controls.Add(statusStrip);
        }

        // ============================================
        // تهيئة البيانات
        // ============================================
        private void InitializeData()
        {
            _atmList = new List<ATMInfo>();
            _totalAtms = 0;
            _connectedAtms = 0;
            _syncingAtms = 0;
            _errorAtms = 0;
            _offlineAtms = 0;
            _lastRefresh = DateTime.Now;
        }

        private void SetupTimers()
        {
            _refreshTimer = new System.Windows.Forms.Timer();
            _refreshTimer.Interval = 5000; // تحديث كل 5 ثوان
            _refreshTimer.Tick += (s, e) => RefreshDashboard();
            _refreshTimer.Start();

            _alertTimer = new System.Windows.Forms.Timer();
            _alertTimer.Interval = 10000; // فحص التنبيهات كل 10 ثوان
            _alertTimer.Tick += (s, e) => CheckAlerts();
            _alertTimer.Start();
        }

        /// <summary>
        /// تحميل بيانات تجريبية
        /// </summary>
        private void LoadDemoData()
        {
            string[] atmNames = { "ATM-001", "ATM-002", "ATM-003", "ATM-004", "ATM-005",
                                  "ATM-006", "ATM-007", "ATM-008", "ATM-009", "ATM-010",
                                  "ATM-011", "ATM-012", "ATM-013", "ATM-014", "ATM-015" };

            string[] locations = { "الرياض - العليا", "جدة - التحلية", "الدمام - الكورنيش",
                                   "مكة - العزيزية", "المدينة - المركزية", "الطائف - الشهار",
                                   "أبها - المفتاحة", "تبوك - المركز", "حائل - الصناعية",
                                   "نجران - الفيصلية", "جازان - المركز", "الباحة - المنطقة",
                                   "القصيم - بريدة", "الجوف - سكاكا", "عسير - خميس" };

            string[] branches = { "BR001", "BR002", "BR003", "BR004", "BR005",
                                  "BR006", "BR007", "BR008", "BR009", "BR010",
                                  "BR011", "BR012", "BR013", "BR014", "BR015" };

            ATMStatus[] statuses = { ATMStatus.InService, ATMStatus.InService, ATMStatus.InService,
                                     ATMStatus.ConnectedOnly, ATMStatus.InService, ATMStatus.InService,
                                     ATMStatus.WaitingResponse, ATMStatus.InService, ATMStatus.InService,
                                     ATMStatus.OutOfService, ATMStatus.InService, ATMStatus.CriticalFault,
                                     ATMStatus.InService, ATMStatus.Offline, ATMStatus.Offline };

            Random rnd = new Random();

            for (int i = 0; i < 15; i++)
            {
                ATMInfo atm = new ATMInfo();
                atm.ATMId = atmNames[i];
                atm.ATMName = $"صراف {i + 1}";
                atm.IPAddress = $"192.168.1.{100 + i}";
                atm.ATMType = (ATMType)(i % 3);
                atm.Status = statuses[i];
                atm.ConnectionStatus = statuses[i] == ATMStatus.Offline ? ConnectionStatus.Disconnected : ConnectionStatus.Connected;
                atm.LastConnectionTime = DateTime.Now.AddMinutes(-rnd.Next(1, 60));
                atm.LastSyncTime = DateTime.Now.AddMinutes(-rnd.Next(1, 120));
                atm.PendingJournalCount = rnd.Next(0, 10);
                atm.SuccessRate = 85.0 + rnd.NextDouble() * 15.0;
                atm.Location = locations[i];
                atm.BranchCode = branches[i];
                atm.TotalTransactionsSynced = rnd.Next(100, 5000);

                _atmList.Add(atm);
            }

            _totalAtms = _atmList.Count;
            _connectedAtms = _atmList.Count(a => a.ConnectionStatus == ConnectionStatus.Connected);
            _syncingAtms = _atmList.Count(a => a.Status == ATMStatus.InService);
            _errorAtms = _atmList.Count(a => a.Status == ATMStatus.CriticalFault || a.Status == ATMStatus.OutOfService);
            _offlineAtms = _atmList.Count(a => a.Status == ATMStatus.Offline);

            RefreshATMList();
            LoadDemoAlerts();
        }

        /// <summary>
        /// تحميل تنبيهات تجريبية
        /// </summary>
        private void LoadDemoAlerts()
        {
            AddAlert("Critical", "ATM-012", "عطل حرج - فشل في قراءة الجورنال");
            AddAlert("Error", "ATM-010", "خارج الخدمة - لا يوجد اتصال");
            AddAlert("Warning", "ATM-007", "تأخير في الاستجابة (8.5 ثانية)");
            AddAlert("Warning", "ATM-004", "ملفات معلقة تجاوزت الحد (8 ملفات)");
            AddAlert("Info", "ATM-001", "تمت المزامنة بنجاح - 3 ملفات");
            AddAlert("Info", "ATM-005", "اتصال جديد من 192.168.1.104");
        }

        // ============================================
        // وظائف التحديث
        // ============================================
        private void RefreshDashboard()
        {
            _lastRefresh = DateTime.Now;
            UpdateStats();
            RefreshATMList();
        }

        private void UpdateStats()
        {
            totalAtmsLabel.Text = _totalAtms.ToString();
            connectedAtmsLabel.Text = _connectedAtms.ToString();
            syncingAtmsLabel.Text = _syncingAtms.ToString();
            errorAtmsLabel.Text = _errorAtms.ToString();
            offlineAtmsLabel.Text = _offlineAtms.ToString();
        }

        private void RefreshATMList()
        {
            atmListView.Items.Clear();
            int index = 1;

            foreach (ATMInfo atm in _atmList)
            {
                ListViewItem item = new ListViewItem(index.ToString());
                item.SubItems.Add(atm.ATMId);
                item.SubItems.Add(atm.ATMName);
                item.SubItems.Add(atm.IPAddress);
                item.SubItems.Add(atm.ATMType.ToString());
                item.SubItems.Add(atm.GetStatusDescription());
                item.SubItems.Add(atm.GetConnectionStatusDescription());
                item.SubItems.Add(atm.LastSyncTime.ToString("yyyy-MM-dd HH:mm"));
                item.SubItems.Add(atm.PendingJournalCount.ToString());
                item.SubItems.Add($"{atm.SuccessRate:F1}%");
                item.SubItems.Add(atm.Location);
                item.SubItems.Add(atm.BranchCode);

                // تلوين حسب الحالة
                switch (atm.Status)
                {
                    case ATMStatus.InService:
                        item.ForeColor = Color.LightGreen;
                        break;
                    case ATMStatus.ConnectedOnly:
                        item.ForeColor = Color.FromArgb(100, 200, 255);
                        break;
                    case ATMStatus.WaitingResponse:
                        item.ForeColor = Color.Yellow;
                        break;
                    case ATMStatus.OutOfService:
                        item.ForeColor = Color.Orange;
                        break;
                    case ATMStatus.CriticalFault:
                        item.ForeColor = Color.Red;
                        break;
                    case ATMStatus.Offline:
                        item.ForeColor = Color.Gray;
                        break;
                }

                atmListView.Items.Add(item);
                index++;
            }
        }

        private void AddAlert(string level, string atmId, string message)
        {
            ListViewItem item = new ListViewItem(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            item.SubItems.Add(level);
            item.SubItems.Add(atmId);
            item.SubItems.Add(message);
            item.SubItems.Add("جديد");

            switch (level)
            {
                case "Critical":
                    item.ForeColor = Color.Red;
                    break;
                case "Error":
                    item.ForeColor = Color.Orange;
                    break;
                case "Warning":
                    item.ForeColor = Color.Yellow;
                    break;
                default:
                    item.ForeColor = Color.LightGreen;
                    break;
            }

            alertsListView.Items.Insert(0, item);
        }

        private void CheckAlerts()
        {
            // فحص التنبيهات الجديدة
            foreach (ATMInfo atm in _atmList)
            {
                if (atm.Status == ATMStatus.CriticalFault)
                {
                    // إضافة تنبيه حرج
                }
            }
        }

        // ============================================
        // وظائف التفاعل
        // ============================================
        private void ShowATMDetails()
        {
            if (atmListView.SelectedItems.Count == 0)
                return;

            string atmId = atmListView.SelectedItems[0].SubItems[1].Text;
            ATMInfo atm = _atmList.FirstOrDefault(a => a.ATMId == atmId);
            if (atm != null)
            {
                MessageBox.Show(atm.ToString(), "تفاصيل الصراف", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void SendCommandToATM()
        {
            if (atmListView.SelectedItems.Count == 0)
                return;

            string atmId = atmListView.SelectedItems[0].SubItems[1].Text;
            MessageBox.Show($"إرسال أمر إلى: {atmId}", "أمر بعيد");
        }

        private void StartATMSync()
        {
            if (atmListView.SelectedItems.Count == 0)
                return;

            string atmId = atmListView.SelectedItems[0].SubItems[1].Text;
            AddAlert("Info", atmId, "تم بدء المزامنة");
        }

        private void StopATMSync()
        {
            if (atmListView.SelectedItems.Count == 0)
                return;

            string atmId = atmListView.SelectedItems[0].SubItems[1].Text;
            AddAlert("Info", atmId, "تم إيقاف المزامنة");
        }

        private void DisconnectATM()
        {
            if (atmListView.SelectedItems.Count == 0)
                return;

            string atmId = atmListView.SelectedItems[0].SubItems[1].Text;
            var result = MessageBox.Show($"هل تريد قطع الاتصال عن {atmId}؟", "تأكيد", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                AddAlert("Warning", atmId, "تم قطع الاتصال بأمر من المسؤول");
            }
        }
    }
}


