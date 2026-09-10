// ============================================================================
//  LanguageManager.cs - Bilingual Resource Manager (Arabic/English)
//  Handles RTL/LTR switching, culture persistence, and text localization.
// ============================================================================

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Windows.Forms;

namespace EJLive.Shared.UI
{
    /// <summary>
    /// Manages bilingual support for Arabic (ar-SA) and English (en-US) across all EJLive applications.
    /// Provides RTL/LTR direction switching and text resource lookup.
    /// </summary>
    public static class LanguageManager
    {
        // ──────────────────────────────────────────────
        //  Supported Cultures
        // ──────────────────────────────────────────────

        public static readonly CultureInfo ArabicCulture = new CultureInfo("ar-SA");
        public static readonly CultureInfo EnglishCulture = new CultureInfo("en-US");

        private static CultureInfo _currentCulture = EnglishCulture;
        private static Dictionary<string, Dictionary<string, string>> _resources;
        private static readonly string SettingsPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, "ejlive_language.json");

        /// <summary>Gets or sets the current UI culture.</summary>
        public static CultureInfo CurrentCulture
        {
            get => _currentCulture;
            set
            {
                if (_currentCulture.Name == value.Name) return;
                _currentCulture = value;
                Thread.CurrentThread.CurrentUICulture = value;
                Thread.CurrentThread.CurrentCulture = value;
                SavePreference();
            }
        }

        /// <summary>Returns true if the current language is Arabic (RTL).</summary>
        public static bool IsArabic => _currentCulture.Name == "ar-SA";

        /// <summary>Returns true if the current language is English (LTR).</summary>
        public static bool IsEnglish => _currentCulture.Name == "en-US";

        /// <summary>Event fired when language is changed.</summary>
        public static event Action<bool> LanguageChanged; // true = Arabic/RTL, false = English/LTR

        // ──────────────────────────────────────────────
        //  Initialization & Persistence
        // ──────────────────────────────────────────────

        /// <summary>Initializes the language manager and loads saved preference.</summary>
        public static void Initialize()
        {
            LoadResources();
            LoadPreference();
            ApplyCultureToThread();
        }

        /// <summary>Switches to the specified language and updates all open forms.</summary>
        public static void SwitchLanguage(CultureInfo culture)
        {
            if (_currentCulture.Name == culture.Name) return;
            CurrentCulture = culture;
            LanguageChanged?.Invoke(IsArabic);
        }

        /// <summary>Toggles between Arabic and English.</summary>
        public static void ToggleLanguage()
        {
            SwitchLanguage(IsArabic ? EnglishCulture : ArabicCulture);
        }

        private static void ApplyCultureToThread()
        {
            Thread.CurrentThread.CurrentUICulture = _currentCulture;
            Thread.CurrentThread.CurrentCulture = _currentCulture;
        }

        private static void SavePreference()
        {
            try
            {
                var data = new { language = _currentCulture.Name };
                File.WriteAllText(SettingsPath, JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true }));
            }
            catch { /* ignore persistence errors */ }
        }

        private static void LoadPreference()
        {
            try
            {
                if (File.Exists(SettingsPath))
                {
                    var json = File.ReadAllText(SettingsPath);
                    var doc = JsonDocument.Parse(json);
                    if (doc.RootElement.TryGetProperty("language", out var lang))
                    {
                        var name = lang.GetString();
                        if (name == "ar-SA")
                            _currentCulture = ArabicCulture;
                        else if (name == "en-US")
                            _currentCulture = EnglishCulture;
                    }
                }
            }
            catch { _currentCulture = EnglishCulture; }
        }

        // ──────────────────────────────────────────────
        //  Resource Loading (Embedded / Fallback)
        // ──────────────────────────────────────────────

        private static void LoadResources()
        {
            _resources = new Dictionary<string, Dictionary<string, string>>();
            BuildEnglishResources();
            BuildArabicResources();
        }

        private static void BuildEnglishResources()
        {
            var en = new Dictionary<string, string>();

            // ── Common / General ──
            en["App.Title"] = "EJLive Enterprise Suite";
            en["App.Ok"] = "OK";
            en["App.Cancel"] = "Cancel";
            en["App.Save"] = "Save";
            en["App.Delete"] = "Delete";
            en["App.Close"] = "Close";
            en["App.Refresh"] = "Refresh";
            en["App.Browse"] = "Browse...";
            en["App.ClearLog"] = "Clear Log";
            en["App.Language"] = "Language";
            en["App.Arabic"] = "العربية";
            en["App.English"] = "English";
            en["App.Ready"] = "Ready";
            en["App.Processing"] = "Processing...";
            en["App.Error"] = "Error";
            en["App.Warning"] = "Warning";
            en["App.Info"] = "Information";
            en["App.Confirm"] = "Confirm";
            en["App.Exit"] = "Exit";

            // ── Client MainForm ──
            en["Client.Title"] = "EJLive ATM Client";
            en["Client.Subtitle"] = "Electronic Journal Synchronization Service";
            en["Client.ActivityLog"] = "Activity Log";
            en["Client.ServicesRunning"] = "Services Running...";
            en["Client.ServicesStopped"] = "Services Stopped";

            // ── Server MainForm ──
            en["Server.Title"] = "EJLive Central Server";
            en["Server.Version"] = "Electronic Journal Server & Archive Manager";
            en["Server.TabServer"] = "  Server  ";
            en["Server.TabConnections"] = "  Connections  ";
            en["Server.TabArchive"] = "  Archive  ";
            en["Server.TabLog"] = "  Log  ";
            en["Server.Config"] = "Server Configuration";
            en["Server.Port"] = "Listen Port:";
            en["Server.Storage"] = "Storage Path:";
            en["Server.Status"] = "Status:";
            en["Server.Stopped"] = "Stopped";
            en["Server.Running"] = "Running";
            en["Server.Connected"] = "Connected ATMs:";
            en["Server.StartServer"] = "Start Server";
            en["Server.StopServer"] = "Stop Server";
            en["Server.SendCommand"] = "Send Command";
            en["Server.AtmDetails"] = "ATM Details";
            en["Server.SyncDashboard"] = "Sync Dashboard";
            en["Server.Actions"] = "Server Actions";
            en["Server.AtmSummary"] = "Selected ATM Summary";
            en["Server.StorageStats"] = "Storage Statistics";
            en["Server.TotalStorage"] = "Total Storage Used:";
            en["Server.ArchivedSize"] = "Archived Size:";
            en["Server.TotalFiles"] = "Total Files:";
            en["Server.ArchiveNow"] = "Archive Now";
            en["Server.AtmId"] = "ATM ID";
            en["Server.IpAddress"] = "IP Address";
            en["Server.StatusCol"] = "Status";
            en["Server.LastSync"] = "Last Sync";
            en["Server.AtmType"] = "ATM Type";
            en["Server.SelectPreview"] = "Select an ATM to see its Root capabilities and operational summary.";

            // ── Monitoring Dashboard ──
            en["Monitor.Title"] = "EJLive Monitoring Dashboard";
            en["Monitor.Subtitle"] = "Real-time Fleet Monitoring";
            en["Monitor.TotalAtms"] = "Total ATMs";
            en["Monitor.Online"] = "Online";
            en["Monitor.Offline"] = "Offline";
            en["Monitor.Alerts"] = "Alerts";

            // ── Installer ──
            en["Installer.Title"] = "EJLive Installer";
            en["Installer.Welcome"] = "Welcome to EJLive Setup";
            en["Installer.Install"] = "Install";
            en["Installer.Progress"] = "Installation Progress";

            _resources["en-US"] = en;
        }

        private static void BuildArabicResources()
        {
            var ar = new Dictionary<string, string>();

            // ── Common / عام ──
            ar["App.Title"] = "مجموعة EJLive المؤسسية";
            ar["App.Ok"] = "موافق";
            ar["App.Cancel"] = "إلغاء";
            ar["App.Save"] = "حفظ";
            ar["App.Delete"] = "حذف";
            ar["App.Close"] = "إغلاق";
            ar["App.Refresh"] = "تحديث";
            ar["App.Browse"] = "تصفح...";
            ar["App.ClearLog"] = "مسح السجل";
            ar["App.Language"] = "اللغة";
            ar["App.Arabic"] = "العربية";
            ar["App.English"] = "English";
            ar["App.Ready"] = "جاهز";
            ar["App.Processing"] = "جارٍ المعالجة...";
            ar["App.Error"] = "خطأ";
            ar["App.Warning"] = "تحذير";
            ar["App.Info"] = "معلومات";
            ar["App.Confirm"] = "تأكيد";
            ar["App.Exit"] = "خروج";

            // ── العميل ──
            ar["Client.Title"] = "عميل أجهزة الصراف الآلي EJLive";
            ar["Client.Subtitle"] = "خدمة مزامنة السجل الإلكتروني";
            ar["Client.ActivityLog"] = "سجل النشاط";
            ar["Client.ServicesRunning"] = "الخدمات قيد التشغيل...";
            ar["Client.ServicesStopped"] = "الخدمات متوقفة";

            // ── الخادم المركزي ──
            ar["Server.Title"] = "الخادم المركزي EJLive";
            ar["Server.Version"] = "خادم السجل الإلكتروني ومدير الأرشيف";
            ar["Server.TabServer"] = "  الخادم  ";
            ar["Server.TabConnections"] = "  الاتصالات  ";
            ar["Server.TabArchive"] = "  الأرشيف  ";
            ar["Server.TabLog"] = "  السجل  ";
            ar["Server.Config"] = "إعدادات الخادم";
            ar["Server.Port"] = "منفذ الاستماع:";
            ar["Server.Storage"] = "مسار التخزين:";
            ar["Server.Status"] = "الحالة:";
            ar["Server.Stopped"] = "متوقف";
            ar["Server.Running"] = "قيد التشغيل";
            ar["Server.Connected"] = "الأجهزة المتصلة:";
            ar["Server.StartServer"] = "تشغيل الخادم";
            ar["Server.StopServer"] = "إيقاف الخادم";
            ar["Server.SendCommand"] = "إرسال أمر";
            ar["Server.AtmDetails"] = "تفاصيل الجهاز";
            ar["Server.SyncDashboard"] = "لوحة المزامنة";
            ar["Server.Actions"] = "إجراءات الخادم";
            ar["Server.AtmSummary"] = "ملخص الجهاز المحدد";
            ar["Server.StorageStats"] = "إحصائيات التخزين";
            ar["Server.TotalStorage"] = "إجمالي المساحة المستخدمة:";
            ar["Server.ArchivedSize"] = "حجم الأرشيف:";
            ar["Server.TotalFiles"] = "إجمالي الملفات:";
            ar["Server.ArchiveNow"] = "أرشفة الآن";
            ar["Server.AtmId"] = "معرف الجهاز";
            ar["Server.IpAddress"] = "عنوان IP";
            ar["Server.StatusCol"] = "الحالة";
            ar["Server.LastSync"] = "آخر مزامنة";
            ar["Server.AtmType"] = "نوع الجهاز";
            ar["Server.SelectPreview"] = "اختر جهاز صراف آلي لعرض قدراته وملخصه التشغيلي.";

            // ── لوحة المراقبة ──
            ar["Monitor.Title"] = "لوحة مراقبة EJLive";
            ar["Monitor.Subtitle"] = "مراقبة الأسطول في الوقت الفعلي";
            ar["Monitor.TotalAtms"] = "إجمالي الأجهزة";
            ar["Monitor.Online"] = "متصل";
            ar["Monitor.Offline"] = "غير متصل";
            ar["Monitor.Alerts"] = "تنبيهات";

            // ── المثبت ──
            ar["Installer.Title"] = "مثبت EJLive";
            ar["Installer.Welcome"] = "مرحباً بك في إعداد EJLive";
            ar["Installer.Install"] = "تثبيت";
            ar["Installer.Progress"] = "تقدم التثبيت";

            _resources["ar-SA"] = ar;
        }

        // ──────────────────────────────────────────────
        //  Text Lookup
        // ──────────────────────────────────────────────

        /// <summary>Gets the localized text for the given key.</summary>
        public static string GetString(string key, string defaultValue = null)
        {
            var cultureKey = _currentCulture.Name;
            if (_resources.TryGetValue(cultureKey, out var dict) && dict.TryGetValue(key, out var value))
                return value;

            // Fallback to English
            if (cultureKey != "en-US" && _resources.TryGetValue("en-US", out var enDict) && enDict.TryGetValue(key, out var enValue))
                return enValue;

            return defaultValue ?? key;
        }

        // ──────────────────────────────────────────────
        //  RTL/LTR Application
        // ──────────────────────────────────────────────

        /// <summary>
        /// Applies RTL or LTR layout to all controls in the form tree.
        /// </summary>
        public static void ApplyDirection(Control root, bool isRtl)
        {
            if (root == null) return;
            root.RightToLeft = isRtl ? RightToLeft.Yes : RightToLeft.No;

            // Apply RTL-specific padding adjustments for Arabic
            if (isRtl && root is Form form)
            {
                form.RightToLeftLayout = true;
            }

            foreach (Control child in root.Controls)
            {
                ApplyDirection(child, isRtl);
            }
        }

        /// <summary>
        /// Applies localized text to all common controls in the form using the Tag/Name convention.
        /// Override this in each form for custom binding.
        /// </summary>
        public static void ApplyLocalizedTexts(Control root)
        {
            if (root == null) return;

            foreach (Control ctrl in GetAllControls(root))
            {
                var key = ctrl.Tag as string;
                if (string.IsNullOrEmpty(key)) continue;

                var text = GetString(key, null);
                if (text == null) continue;

                switch (ctrl)
                {
                    case Label label:
                        label.Text = text;
                        break;
                    case Button button:
                        button.Text = text;
                        break;
                    case CheckBox check:
                        check.Text = text;
                        break;
                    case GroupBox group:
                        group.Text = text;
                        break;
                    case TabPage tab:
                        tab.Text = text;
                        break;
                    case Form form:
                        form.Text = text;
                        break;
                }
            }
        }

        private static IEnumerable<Control> GetAllControls(Control parent)
        {
            yield return parent;
            foreach (Control child in parent.Controls)
            {
                foreach (var grandchild in GetAllControls(child))
                    yield return grandchild;
            }
        }
    }
}