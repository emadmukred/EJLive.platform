using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using EJLive.Core.Engine;
using EJLive.Core.Models;
using EJLive.Server.WinForms.Services;
using EJLive.Shared;

namespace EJLive.Server.WinForms
{
    /// <summary>
    /// واجهة قراءة الجورنال التفصيلية — تُفتح بالضغط المزدوج على بطاقة الصراف
    /// أزرار بحث مخصصة: Approved | Power Up | E3 | Cash Error | M-18 | M-02/03/05 | M-10/11
    /// بحث نصي حر + تمييز النتائج
    /// </summary>
    public class JournalViewerForm : Form
    {
        private readonly ATMInfo                    _atm;
        private readonly string                     _archivePath;
        private readonly string                     _atmId;
        private readonly string                     _atmType;
        private readonly TransactionAnalysisEngine  _engine;
        private readonly ArchiveManager             _archiveManager;
        private string                              _rawText;

        private RichTextBox _rtbJournal;
        private TextBox     _txtSearch;
        private Label       _lblStats;
        private Panel       _toolPanel;
        private Button[]    _filterBtns;
        private Label       _lblResultCount;
        private CheckBox    _chkHighlight;

        public JournalViewerForm()
            : this(CreateDesignTimeATM(), null)
        {
        }

        public JournalViewerForm(ATMInfo atm, TransactionAnalysisEngine engine)
        {
            _atm     = atm ?? CreateDesignTimeATM();
            _atmId   = _atm.ATM_ID;
            _atmType = _atm.ATM_Type ?? "NCR";
            _engine  = engine;
            InitializeForm();
            LoadFromATM();
        }

        private static ATMInfo CreateDesignTimeATM()
        {
            return new ATMInfo
            {
                ATM_ID = "ATM001",
                ATM_Name = "Design Preview ATM",
                ATM_Type = "NCR",
                ConnectionStatus = ConnectionStatus.Connected,
                LastJournalFile = "EJ260511.log",
                LastSyncUtc = DateTime.UtcNow.AddMinutes(-3)
            };
        }

        public JournalViewerForm(string archivePath, string atmId, string atmType,
                                  TransactionAnalysisEngine engine, ArchiveManager archiveManager)
        {
            _archivePath    = archivePath;
            _atmId          = atmId;
            _atmType        = atmType;
            _engine         = engine;
            _archiveManager = archiveManager;
            InitializeForm();
            LoadFromArchive();
        }

        private void InitializeForm()
        {
            Text          = $"📋 قارئ الجورنال — {_atmId} ({_atmType})";
            Size          = new Size(1100, 720);
            MinimumSize   = new Size(900, 500);
            StartPosition = FormStartPosition.CenterParent;
            BackColor     = LightUiTheme.Window;
            ForeColor     = LightUiTheme.Text;
            Font          = new Font("Segoe UI", 9f);

            BuildUI();
        }

        private void BuildUI()
        {
            // ═══ شريط الأدوات ═══
            _toolPanel = new Panel { Height = 128, Dock = DockStyle.Top, BackColor = LightUiTheme.SurfaceAlt, Padding = new Padding(10, 8, 10, 8) };

            // أزرار البحث السريع
            var filterNames = new[]
            {
                "✅ Approved", "⚡ Power Up Reset", "🚨 Error E3", "💵 Cash Error",
                "⚠️ M-18", "⚠️ M-02/03/05", "⚠️ M-10/11", "💳 Card Capture",
                "❌ Declined", "🔄 الكل"
            };
            var filterEnums = new[]
            {
                JournalSearchFilter.ApprovedTransactions, JournalSearchFilter.PowerUpReset,
                JournalSearchFilter.ErrorE3, JournalSearchFilter.TotalCashError,
                JournalSearchFilter.M18, JournalSearchFilter.M02_M03_M05, JournalSearchFilter.M10_M11,
                JournalSearchFilter.CardCapture, JournalSearchFilter.Declined, JournalSearchFilter.All
            };

            var filterColors = new Color[]
            {
                Color.FromArgb(20, 70, 30), Color.FromArgb(50, 40, 20),
                Color.FromArgb(80, 20, 20), Color.FromArgb(70, 30, 10),
                Color.FromArgb(60, 30, 60), Color.FromArgb(50, 30, 60),
                Color.FromArgb(40, 30, 70), Color.FromArgb(60, 20, 40),
                Color.FromArgb(80, 20, 30), Color.FromArgb(30, 40, 60)
            };

            _filterBtns = new Button[filterNames.Length];
            var filterFlow = new FlowLayoutPanel { Height = 74, Dock = DockStyle.Top, FlowDirection = FlowDirection.LeftToRight, WrapContents = true };
            for (int i = 0; i < filterNames.Length; i++)
            {
                var idx = i;
                var btn = new Button
                {
                    Text      = filterNames[i],
                    Height    = 30,
                    BackColor = filterColors[i],
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font      = new Font("Segoe UI", 7.5f),
                    Margin    = new Padding(3, 2, 3, 2),
                    Cursor    = Cursors.Hand,
                    AutoSize  = true,
                    AutoEllipsis = true
                };
                btn.FlatAppearance.BorderSize = 0;
                btn.Click += (s, e) => ApplyFilter(filterEnums[idx]);
                filterFlow.Controls.Add(btn);
                _filterBtns[i] = btn;
            }

            // بحث نصي
            var searchRow = new FlowLayoutPanel { Height = 36, Dock = DockStyle.Bottom, FlowDirection = FlowDirection.LeftToRight, WrapContents = true };
            _txtSearch = new TextBox { Width = 300, BackColor = LightUiTheme.Surface, ForeColor = LightUiTheme.Text, BorderStyle = BorderStyle.FixedSingle, Text = "بحث حر..." };
            _txtSearch.TextChanged += (s, e) => ApplyFreeSearch();
            var btnCopy = new Button { Text = "📋 نسخ", Width = 80, Height = 28, BackColor = Color.FromArgb(30, 50, 80), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnCopy.Click += (s, e) => { if (!string.IsNullOrEmpty(_rtbJournal.SelectedText)) Clipboard.SetText(_rtbJournal.SelectedText); };
            var btnExport = new Button { Text = "📥 تصدير", Width = 90, Height = 28, BackColor = Color.FromArgb(30, 70, 40), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnExport.Click += (s, e) => ExportText();
            _chkHighlight = new CheckBox { Text = "تمييز النتائج", Checked = true, ForeColor = LightUiTheme.Text, AutoSize = true };
            _lblResultCount = new Label { Text = "0 نتيجة", ForeColor = LightUiTheme.Muted, AutoSize = true, TextAlign = ContentAlignment.MiddleLeft };
            searchRow.Controls.AddRange(new Control[] { _txtSearch, btnCopy, btnExport, _chkHighlight, _lblResultCount });

            _toolPanel.Controls.AddRange(new Control[] { filterFlow, searchRow });

            // ═══ إحصائيات ═══
            _lblStats = new Label
            {
                Dock      = DockStyle.Top,
                Height    = 28,
                BackColor = LightUiTheme.Surface,
                ForeColor = LightUiTheme.Text,
                Font      = new Font("Segoe UI", 8.5f),
                TextAlign = ContentAlignment.MiddleCenter,
                Padding   = new Padding(8, 0, 0, 0)
            };

            // ═══ منطقة النص ═══
            _rtbJournal = new RichTextBox
            {
                Dock        = DockStyle.Fill,
                BackColor   = LightUiTheme.Surface,
                ForeColor   = LightUiTheme.Text,
                Font        = new Font("Consolas", 9.5f),
                ReadOnly    = true,
                BorderStyle = BorderStyle.FixedSingle,
                ScrollBars  = RichTextBoxScrollBars.Both,
                WordWrap    = false
            };

            Controls.AddRange(new Control[] { _rtbJournal, _lblStats, _toolPanel });
            LightUiTheme.Apply(this);
        }

        private void LoadFromATM()
        {
            if (_atm == null) return;
            var path = _atm.GetSourcePath();
            if (!Directory.Exists(path)) { _rtbJournal.Text = $"المجلد غير موجود: {path}"; return; }

            var files = Directory.GetFiles(path);
            if (files.Length == 0) { _rtbJournal.Text = "لا توجد ملفات جورنال."; return; }

            LoadJournalText(files[0]);
        }

        private void LoadFromArchive()
        {
            if (_archiveManager == null || string.IsNullOrEmpty(_archivePath)) return;
            var thread = new Thread(() =>
            {
                var text = _archiveManager.RetrieveAsText(_archivePath);
                BeginInvoke(new Action(() => SetText(text ?? "فشل قراءة الأرشيف")));
            }) { IsBackground = true };
            thread.Start();
            _rtbJournal.Text = "⏳ جاري تحميل الجورنال...";
        }

        private void LoadJournalText(string path)
        {
            try
            {
                using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
                using var sr = new StreamReader(fs, Encoding.UTF8, true);
                SetText(sr.ReadToEnd());
            }
            catch (Exception ex) { SetText($"خطأ: {ex.Message}"); }
        }

        private void SetText(string text)
        {
            _rawText = text ?? "";
            _rtbJournal.Text = _rawText;
            UpdateStats();
        }

        private void ApplyFilter(JournalSearchFilter filter)
        {
            if (string.IsNullOrEmpty(_rawText)) return;
            if (filter == JournalSearchFilter.All) { SetText(_rawText); return; }

            var results = _engine.SearchLines(_rawText, filter);
            DisplayResults(results);
        }

        private void ApplyFreeSearch()
        {
            var kw = _txtSearch.Text.Trim();
            if (string.IsNullOrEmpty(kw) || kw == "بحث حر...") { if (!string.IsNullOrEmpty(_rawText)) SetText(_rawText); return; }
            var results = _engine.SearchFreeText(_rawText, kw);
            DisplayResults(results);
        }

        private void DisplayResults(List<string> lines)
        {
            _rtbJournal.Clear();
            if (lines.Count == 0) { _rtbJournal.Text = "لا توجد نتائج."; _lblResultCount.Text = "0 نتيجة"; return; }

            foreach (var line in lines)
            {
                // تلوين مميز
                var color = GetLineColor(line);
                _rtbJournal.SelectionStart  = _rtbJournal.TextLength;
                _rtbJournal.SelectionColor  = color;
                _rtbJournal.AppendText(line + "\n");
            }
            _lblResultCount.Text = $"{lines.Count} نتيجة";
        }

        private Color GetLineColor(string line)
        {
            var u = line.ToUpperInvariant();
            if (u.Contains("DISPENSE") || u.Contains("APPROVED") || u.Contains("NOTES DISPENSED"))
                return Color.FromArgb(100, 220, 100);
            if (u.Contains("DECLINED") || u.Contains("UNABLE") || u.Contains("REJECTED"))
                return Color.FromArgb(255, 100, 100);
            if (u.Contains("CARD CAPTURED") || u.Contains("RETAIN"))
                return Color.FromArgb(255, 180, 0);
            if (u.Contains("ERROR") || u.Contains("FAULT") || u.Contains("M-1") || u.Contains("M-0"))
                return Color.FromArgb(255, 100, 50);
            if (u.Contains("POWER UP") || u.Contains("RESTART") || u.Contains("STARTUP"))
                return Color.FromArgb(100, 180, 255);
            if (u.Contains("SUPERVISOR") || u.Contains("OPERATOR"))
                return Color.FromArgb(200, 150, 255);
            return LightUiTheme.Text;
        }

        private void UpdateStats()
        {
            if (string.IsNullOrEmpty(_rawText)) { _lblStats.Text = "لا توجد بيانات"; return; }
            var analysis = _engine.AnalyzeText(_rawText, _atmId, _atmType);
            var lines    = _rawText.Split('\n').Length;
            _lblStats.Text = $"📊 {_atmId} ({_atmType})  |  📝 {lines:N0} سطر  |  ✅ {analysis.Count} عملية محللة";
        }

        private void ExportText()
        {
            using var dlg = new SaveFileDialog { Filter = "Text Files|*.txt|All Files|*.*", FileName = $"journal_{_atmId}_{DateTime.Now:yyyyMMdd}" };
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                File.WriteAllText(dlg.FileName, _rtbJournal.Text, Encoding.UTF8);
                MessageBox.Show($"تم التصدير:\n{dlg.FileName}", "تصدير ناجح");
            }
        }
    }
}
