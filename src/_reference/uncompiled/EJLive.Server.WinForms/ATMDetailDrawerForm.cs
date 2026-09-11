using System;
using System.Drawing;
using System.Windows.Forms;
using EJLive.Core;
using EJLive.Core.Engine;
using EJLive.Core.Models;
using EJLive.Shared;

namespace EJLive.Server.WinForms
{
    /// <summary>
    /// درج تفاصيل الصراف — 3 تبويبات: نظرة عامة / سجل EJ / الأعطال
    /// يطبق: D-11 (ATM Detail Drawer with 3 tabs)
    /// يُفتح بالنقر المزدوج على بطاقة الصراف في NOC Dashboard
    /// </summary>
    public class ATMDetailDrawerForm : Form
    {
        private readonly ATMInfo _atm;
        private readonly TransactionAnalysisEngine _analysis;
        private TabControl _tabs;

        public ATMDetailDrawerForm()
        {
            _atm = CreateDesignTimeATM();
            _analysis = null;
            BuildUI();
        }

        public ATMDetailDrawerForm(ATMInfo atm, TransactionAnalysisEngine analysis)
        {
            _atm = atm ?? CreateDesignTimeATM();
            _analysis = analysis;
            BuildUI();
        }

        private static ATMInfo CreateDesignTimeATM()
        {
            var now = DateTime.UtcNow;
            return new ATMInfo
            {
                ATM_ID = "ATM001",
                ATM_Name = "Design Preview ATM",
                ATM_Type = AppConstants.ATM_TYPE_NCR,
                BranchName = "Main Branch",
                Region = "NOC",
                ServerIP = "192.168.1.100",
                NetworkType = "LAN",
                Latency_ms = 18,
                ConnectionStatus = ConnectionStatus.Connected,
                ConnectedAtUtc = now.AddMinutes(-15),
                LastHeartbeatUtc = now.AddSeconds(-20),
                LastSyncUtc = now.AddMinutes(-2),
                CashDispensed = 125000,
                TotalTransactions = 42,
                ApprovedTransactions = 40,
                LastJournalFile = "EJ260511.log",
                LastErrorMessage = "Sample design-time fault"
            };
        }

        private void BuildUI()
        {
            Text = $"تفاصيل الصراف: {_atm.ATMId}";
            Size = new Size(780, 560);
            StartPosition = FormStartPosition.CenterParent;
            BackColor = LightUiTheme.Window;
            ForeColor = LightUiTheme.Text;
            Font = new Font("Segoe UI", 9.5F);

            _tabs = new TabControl
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10F)
            };

            _tabs.TabPages.Add(BuildOverviewTab());
            _tabs.TabPages.Add(BuildEJLogTab());
            _tabs.TabPages.Add(BuildFaultsTab());

            Controls.Add(_tabs);
            LightUiTheme.Apply(this);
        }

        // ==========================================
        // تبويب 1: نظرة عامة
        // ==========================================

        private TabPage BuildOverviewTab()
        {
            var tab = new TabPage("📊  نظرة عامة") { BackColor = LightUiTheme.Window };
            var panel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 10,
                Padding = new Padding(16),
                BackColor = LightUiTheme.Window
            };
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));

            void AddRow(string label, string value, Color? valueColor = null)
            {
                var lbl = new Label { Text = label, ForeColor = LightUiTheme.Muted, Font = new Font("Segoe UI", 9.5F), Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleRight, Padding = new Padding(0, 0, 12, 0) };
                var val = new Label { Text = value, ForeColor = valueColor ?? LightUiTheme.Text, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft };
                panel.Controls.Add(lbl);
                panel.Controls.Add(val);
            }

            var status = _atm.ConnectionStatus == ConnectionStatus.Connected ? "متصل ✅" : "غير متصل ❌";
            var statusColor = _atm.ConnectionStatus == ConnectionStatus.Connected
                ? Color.FromArgb(0, 200, 83) : Color.FromArgb(213, 0, 0);

            AddRow("معرف الصراف:", _atm.ATMId);
            AddRow("اسم الصراف:", _atm.ATMName ?? "---");
            AddRow("نوع الصراف:", _atm.ATMType.ToString());
            AddRow("عنوان IP:", _atm.IPAddress ?? "---");
            AddRow("نوع الشبكة:", _atm.NetworkType ?? "LAN");
            AddRow("الكمون:", $"{_atm.Latency} ms");
            AddRow("الحالة:", status, statusColor);
            AddRow("آخر اتصال:", _atm.LastConnectionTime.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss"));
            AddRow("آخر مزامنة:", _atm.LastSyncTime.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss"));
            AddRow("نقد مصروف:", $"{_atm.CashDispensed:N0}");

            tab.Controls.Add(panel);
            return tab;
        }

        // ==========================================
        // تبويب 2: سجل EJ
        // ==========================================

        private TabPage BuildEJLogTab()
        {
            var tab = new TabPage("📜  سجل EJ") { BackColor = LightUiTheme.Window };

            var searchPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 42,
                BackColor = LightUiTheme.SurfaceAlt,
                Padding = new Padding(8, 7, 8, 7)
            };

            var txtSearch = new TextBox
            {
                Text = "",
                BackColor = LightUiTheme.Surface,
                ForeColor = LightUiTheme.Text,
                BorderStyle = BorderStyle.FixedSingle,
                Width = 280,
                Location = new Point(8, 8)
            };
            searchPanel.Controls.Add(txtSearch);

            var quickBtns = new[]
            {
                "Approved", "Error E3", "M-18", "M-02", "Power Reset",
                "Cash Error", "Card Captured"
            };

            int btnX = 300;
            foreach (var btn in quickBtns)
            {
                var b = new Button
                {
                    Text = btn,
                    Location = new Point(btnX, 6),
                    Size = new Size(85, 28),
                    BackColor = LightUiTheme.Surface,
                    ForeColor = LightUiTheme.Text,
                    FlatStyle = FlatStyle.Flat,
                    FlatAppearance = { BorderColor = LightUiTheme.Border },
                    Font = new Font("Segoe UI", 8F)
                };
                var filterText = btn;
                b.Click += (s, e) => txtSearch.Text = filterText;
                searchPanel.Controls.Add(b);
                btnX += 90;
            }

            var richLog = new RichTextBox
            {
                Dock = DockStyle.Fill,
                BackColor = LightUiTheme.Surface,
                ForeColor = LightUiTheme.Text,
                Font = new Font("Courier New", 9F),
                ReadOnly = true,
                BorderStyle = BorderStyle.None,
                Text = $"// سجل EJ للصراف: {_atm.ATMId}\r\n// اختر معيار بحث أو اكتب نصًا للبحث\r\n"
            };

            txtSearch.TextChanged += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtSearch.Text)) return;
                richLog.Text = $"// نتائج البحث عن: {txtSearch.Text}\r\n// (يتطلب الاتصال بمحرك التحليل)\r\n";
            };

            tab.Controls.Add(richLog);
            tab.Controls.Add(searchPanel);
            return tab;
        }

        // ==========================================
        // تبويب 3: الأعطال
        // ==========================================

        private TabPage BuildFaultsTab()
        {
            var tab = new TabPage("⚠  الأعطال") { BackColor = LightUiTheme.Window };

            var lv = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                BackColor = LightUiTheme.Surface,
                ForeColor = LightUiTheme.Text,
                BorderStyle = BorderStyle.None,
                Font = new Font("Segoe UI", 9F)
            };
            lv.Columns.Add("الوقت", 150);
            lv.Columns.Add("نوع الخطأ", 150);
            lv.Columns.Add("كود الخطأ", 100);
            lv.Columns.Add("التفاصيل", 300);
            lv.Columns.Add("تم الحل؟", 80);

            // أزرار الإجراءات
            var actPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 46,
                BackColor = LightUiTheme.SurfaceAlt,
                Padding = new Padding(8, 7, 8, 7),
                FlowDirection = FlowDirection.LeftToRight
            };

            void AddBtn(string text, Color color, Action onClick)
            {
                var btn = new Button
                {
                    Text = text,
                    BackColor = color,
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    FlatAppearance = { BorderSize = 0 },
                    Size = new Size(130, 30),
                    Cursor = Cursors.Hand,
                    Font = new Font("Segoe UI", 9F)
                };
                btn.Click += (s, e) => onClick();
                actPanel.Controls.Add(btn);
            }

            AddBtn("🔄 إعادة تشغيل", Color.FromArgb(213, 0, 0), () =>
                MessageBox.Show($"أمر إعادة التشغيل أُرسل إلى {_atm.ATMId}", "تم", MessageBoxButtons.OK));
            AddBtn("📸 لقطة شاشة", Color.FromArgb(0, 120, 80), () =>
                MessageBox.Show("جاري التقاط الشاشة...", "تنبيه", MessageBoxButtons.OK));
            AddBtn("📤 تصدير الأعطال", Color.FromArgb(13, 110, 253), () =>
                MessageBox.Show("جاري تصدير قائمة الأعطال...", "تنبيه", MessageBoxButtons.OK));

            // بيانات تجريبية
            if (!string.IsNullOrEmpty(_atm.LastError))
            {
                var item = new ListViewItem(DateTime.Now.ToString("HH:mm:ss"));
                item.SubItems.Add("خطأ في الكاش");
                item.SubItems.Add("E3");
                item.SubItems.Add(_atm.LastError);
                item.SubItems.Add("لا");
                item.ForeColor = Color.FromArgb(213, 0, 0);
                lv.Items.Add(item);
            }

            tab.Controls.Add(lv);
            tab.Controls.Add(actPanel);
            return tab;
        }
    }
}
