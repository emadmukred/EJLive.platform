using System;
using EJLive.Core;

namespace EJLive.Client.WinForms.Services
{
    /// <summary>
    /// ثوابت خاصة بالخدمات المتقدمة في جانب العميل (Client-side Advanced Services)
    /// تُعرّف باسم Constants ليُمكن استخدامها مباشرة في AdvancedNetworkManager و AdvancedJournalProcessor
    /// </summary>
    internal static class Constants
    {
        // ==========================================
        // إعدادات الشبكة
        // ==========================================

        /// <summary>مهلة الاتصال بالخادم — 10 ثوانٍ</summary>
        public const int ConnectionTimeout = 10_000;

        /// <summary>الحجم الأقصى لرسالة واحدة — 100 MB</summary>
        public const int MaxMessageSize = 100 * 1024 * 1024;

        /// <summary>فترة Heartbeat — 30 ثانية (بالميلي ثانية)</summary>
        public const int HeartbeatInterval = AppConstants.HeartbeatIntervalSec * 1000;

        /// <summary>مهلة استقبال القراءة — 60 ثانية</summary>
        public const int ReceiveTimeout = 60_000;

        // ==========================================
        // أنماط ملفات الجورنال لكل نوع صراف
        // ==========================================

        public static class EJPatterns
        {
            // NCR — ملفات الجورنال الثابتة
            public static readonly string[] NCRFiles = new[]
            {
                AppConstants.NCR_EJData,   // EJDATA.LOG
                AppConstants.NCR_EJRcpy,   // EJRCPY.LOG
                AppConstants.NCR_EJDataLob // EJDATA.LOb
            };

            // GRG — الملفات تبدأ بـ EJ_
            public const string GRGPrefix = "EJ_";

            // Wincor Nixdorf — الملفات تبدأ بـ EJ أو هي *.ej
            public const string WNPrefix = "EJ";

            // Diebold Nixdorf — غالبًا تستخدم EJ*.jrn / EJ*.log
            public const string DNPrefix = "EJ";

            // Hyosung — غالبًا EJ_*.dat
            public const string HYPrefix = "EJ_";

            // امتدادات معروفة للجورنال
            public static readonly string[] ValidExtensions = new[]
            {
                ".log", ".dat", ".ej", ".lob", ".txt", ".jrn"
            };
        }
    }
}
