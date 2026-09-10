using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using EJLive.Core.Models;
using EJLive.Shared;

namespace EJLive.Core.Services
{
    /// <summary>
    /// مدير التنبيهات الكامل — Health Rules + Priority Queue + Event System
    /// يطبق: L-09 (Health Rules), L-08 (CSC Monitoring), D-06 (Toast Events)
    /// قواعد الصحة: لا بيانات >5 دق يلوّن أصفر → >10 دق رمادي → خطأ متكرر يُنبّه
    /// </summary>
    public sealed class AlertManager : IDisposable
    {
        #region Singleton
        private static AlertManager? _instance;
        private static readonly object _lock = new object();
        public static AlertManager Instance
        {
            get
            {
                lock (_lock)
                    return _instance ??= new AlertManager();
            }
        }
        #endregion

        private readonly ConcurrentQueue<AlertPayload> _queue = new ConcurrentQueue<AlertPayload>();
        private readonly ConcurrentDictionary<string, AlertPayload> _activeAlerts = new ConcurrentDictionary<string, AlertPayload>();
        private readonly List<AlertPayload> _alertsTimeline = new List<AlertPayload>();
        private readonly object _alertsLock = new object();
        private readonly Thread _processorThread;
        private volatile bool _running = true;

        public event EventHandler<AlertPayload>? AlertRaised;
        public event EventHandler<AlertPayload>? OnAlert;
        public event EventHandler<AlertPayload>? OnCritical;
        public event EventHandler<string>? OnAlertResolved;

        // إحصائيات
        public int TotalAlerts   { get; private set; }
        public int ActiveCount   => _activeAlerts.Count;
        public int CriticalCount { get; private set; }
        public IReadOnlyList<AlertPayload> Alerts
        {
            get
            {
                lock (_alertsLock)
                    return _alertsTimeline.ToArray();
            }
        }

        public AlertManager()
        {
            _processorThread = new Thread(ProcessLoop) { IsBackground = true, Name = "EJLive.AlertManager" };
            _processorThread.Start();
        }

        // ==========================================
        // إنشاء تنبيهات
        // ==========================================

        public void Alert(AlertSeverity severity, string title, string message, string? source = null)
        {
            _ = Raise(severity, title, message, source ?? string.Empty);
        }

        public AlertPayload Raise(AlertSeverity severity, string title, string message, string? source, string dedupeKey = "")
        {
            if (!string.IsNullOrWhiteSpace(dedupeKey) &&
                _activeAlerts.TryGetValue(dedupeKey, out var existing) &&
                !existing.IsRead)
                return existing;

            var alert = new AlertPayload
            {
                AlertId = Guid.NewGuid().ToString("N"),
                Severity = severity,
                Title = title ?? string.Empty,
                Message = message ?? string.Empty,
                Source = source ?? string.Empty,
                DedupeKey = dedupeKey ?? string.Empty,
                CreatedAt = DateTime.UtcNow
            };

            _queue.Enqueue(alert);
            return alert;
        }

        public void Info(string title, string msg, string? src = null)     => Alert(AlertSeverity.Info,     title, msg, src);
        public void Warning(string title, string msg, string? src = null)  => Alert(AlertSeverity.Warning,  title, msg, src);
        public void Critical(string title, string msg, string? src = null) => Alert(AlertSeverity.Critical, title, msg, src);
        public void Emergency(string title, string msg, string? src = null) => Alert(AlertSeverity.Emergency, title, msg, src);

        // ==========================================
        // Health Rules (L-09)
        // ==========================================

        public void CheckATMHealth(ATMInfo atm)
        {
            if (atm == null) return;
            var elapsed = (DateTime.UtcNow - atm.LastHeartbeatUtc).TotalMinutes;

            if (elapsed > AppConstants.AlertDisconnectCriticalMin)
            {
                var key = $"disconnect_critical_{atm.ATM_ID}";
                if (!_activeAlerts.ContainsKey(key))
                {
                    var alert = BuildAlert(AlertSeverity.Critical,
                        $"انقطاع حرج: {atm.ATM_Name ?? atm.ATM_ID}",
                        $"لا اتصال منذ {elapsed:F0} دقيقة — تحقق فوري مطلوب",
                        atm.ATM_ID ?? string.Empty, key);
                    _queue.Enqueue(alert);
                }
            }
            else if (elapsed > AppConstants.AlertDisconnectWarningMin)
            {
                var key = $"disconnect_warn_{atm.ATM_ID}";
                if (!_activeAlerts.ContainsKey(key))
                {
                    var alert = BuildAlert(AlertSeverity.Warning,
                        $"تحذير انقطاع: {atm.ATM_Name ?? atm.ATM_ID}",
                        $"لا اتصال منذ {elapsed:F0} دقيقة",
                        atm.ATM_ID ?? string.Empty, key);
                    _queue.Enqueue(alert);
                }
            }
            else
            {
                // تم استعادة الاتصال — إلغاء تنبيهات الانقطاع
                ResolvePattern(atm.ATM_ID ?? string.Empty, "disconnect_");
            }

            // فحص عدم وجود بيانات جورنال
            var noDataElapsed = (DateTime.UtcNow - atm.LastSyncUtc).TotalMinutes;
            if (noDataElapsed > AppConstants.AlertNoDataCriticalMin)
            {
                var key = $"nodata_critical_{atm.ATM_ID}";
                if (!_activeAlerts.ContainsKey(key))
                {
                    _queue.Enqueue(BuildAlert(AlertSeverity.Critical,
                        $"لا بيانات جورنال: {atm.ATM_Name ?? atm.ATM_ID}",
                        $"لم تصل بيانات جورنال منذ {(int)noDataElapsed / 60} ساعة — الصراف قد يكون متوقفًا",
                        atm.ATM_ID ?? string.Empty, key));
                }
            }
            else if (noDataElapsed > AppConstants.AlertNoDataWarningMin)
            {
                var key = $"nodata_warn_{atm.ATM_ID}";
                if (!_activeAlerts.ContainsKey(key))
                {
                    _queue.Enqueue(BuildAlert(AlertSeverity.Warning,
                        $"تأخر جورنال: {atm.ATM_Name ?? atm.ATM_ID}",
                        $"لم تصل بيانات جورنال منذ {(int)noDataElapsed} دقيقة",
                        atm.ATM_ID ?? string.Empty, key));
                }
            }

            // فحص وضع Supervisor
            if (atm.IsSupervisorMode)
            {
                var key = $"supervisor_{atm.ATM_ID}";
                if (!_activeAlerts.ContainsKey(key))
                    _queue.Enqueue(BuildAlert(AlertSeverity.Warning,
                        $"وضع Supervisor: {atm.ATM_Name ?? atm.ATM_ID}",
                        "الصراف في وضع المشرف — قد لا يقدم خدمة للعملاء",
                        atm.ATM_ID ?? string.Empty, key));
            }
            else
            {
                ResolvePattern(atm.ATM_ID ?? string.Empty, "supervisor_");
            }

            // فحص أخطاء المزامنة المتكررة
            if (atm.ConsecutiveSyncFailures >= 3)
            {
                var key = $"syncfail_{atm.ATM_ID}";
                if (!_activeAlerts.ContainsKey(key))
                    _queue.Enqueue(BuildAlert(AlertSeverity.Critical,
                        $"فشل مزامنة متكرر: {atm.ATM_Name ?? atm.ATM_ID}",
                        $"فشلت مزامنة الجورنال {atm.ConsecutiveSyncFailures} مرات متتالية",
                        atm.ATM_ID ?? string.Empty, key));
            }
        }

        public void CheckDiskSpace(string serverName, double freeGB, double totalGB)
        {
            var pct = freeGB / totalGB * 100;
            if (pct < 5)
                Critical("مساحة تخزين حرجة", $"المساحة المتبقية على {serverName}: {freeGB:F1} GB ({pct:F0}%)", serverName);
            else if (pct < 15)
                Warning("تحذير مساحة تخزين", $"المساحة المتبقية على {serverName}: {freeGB:F1} GB ({pct:F0}%)", serverName);
        }

        // ==========================================
        // إلغاء التنبيهات
        // ==========================================

        public void Resolve(string alertKey)
        {
            if (_activeAlerts.TryRemove(alertKey, out _))
                OnAlertResolved?.Invoke(this, alertKey);
        }

        private void ResolvePattern(string atmId, string prefix)
        {
            foreach (var key in _activeAlerts.Keys)
                if (key.StartsWith(prefix + atmId) || key.StartsWith(prefix) && key.Contains(atmId))
                    Resolve(key);
        }

        // ==========================================
        // حلقة المعالجة
        // ==========================================

        private void ProcessLoop()
        {
            while (_running)
            {
                try
                {
                    Thread.Sleep(200);
                    while (_queue.TryDequeue(out var alert))
                    {
                        TotalAlerts++;
                        if (alert.Severity == AlertSeverity.Critical || alert.Severity == AlertSeverity.Emergency)
                            CriticalCount++;

                        if (!string.IsNullOrEmpty(alert.DedupeKey))
                            _activeAlerts[alert.DedupeKey] = alert;

                        lock (_alertsLock)
                            _alertsTimeline.Add(alert);

                        AlertRaised?.Invoke(this, alert);
                        OnAlert?.Invoke(this, alert);
                        if (alert.Severity >= AlertSeverity.Critical)
                            OnCritical?.Invoke(this, alert);
                    }
                }
                catch { }
            }
        }

        private AlertPayload BuildAlert(AlertSeverity sev, string title, string msg, string src, string dedupeKey)
            => new AlertPayload
            {
                AlertId    = Guid.NewGuid().ToString("N"),
                Severity   = sev,
                Title      = title,
                Message    = msg,
                Source     = src,
                DedupeKey  = dedupeKey,
                CreatedAt  = DateTime.UtcNow
            };

        public IEnumerable<AlertPayload> GetActiveAlerts() => _activeAlerts.Values;

        public void Dispose()
        {
            _running = false;
        }
    }
}
