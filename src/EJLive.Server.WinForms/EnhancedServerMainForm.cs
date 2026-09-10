using EJLive.Core;
using EJLive.Core.Engine;
using EJLive.Core.Models;
using EJLive.Core.Services;
using EJLive.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EJLive.Server.WinForms
{
    public partial class EnhancedServerMainForm : Form
    {
        private readonly UnifiedBusinessRuntime _runtime;
        private readonly ServerEngine _serverEngine;
        private readonly AlertManager _alertManager;
        private readonly DatabaseManager _database;
        private Timer _refreshTimer;
        private bool _isRunning;
        public EnhancedServerMainForm()
        {
            InitializeComponent();
            _database = DatabaseManager.Instance;
            _database.Initialize();
            _runtime = UnifiedBusinessRuntime.CreateInitialized();
            _serverEngine = new ServerEngine(AppConstants.DefaultPort);
            _serverEngine.OnATMConnected += OnATMConnectedHandler;
            _serverEngine.OnATMDisconnected += OnATMDisconnectedHandler;
            _serverEngine.OnATMUpdated += OnATMUpdatedHandler;
            _serverEngine.OnJournalReceived += OnJournalReceivedHandler;
            _serverEngine.OnCommandChanged += OnCommandChangedHandler;
            _serverEngine.OnServerLog += OnServerLogHandler;
            _alertManager = AlertManager.Instance;
            _alertManager.AlertRaised += OnAlertRaisedHandler;
            InitializeEnhancedUI();
        }
        public void InitializeEnhancedUI()
        {
            this.Text = $"EJLive Server v{AppConstants.AppVersion} - Enhanced Main Form";
            CreateEnhancedLayout();
            SetupAutoRefreshTimer();
            StartBackgroundMonitoring();
        }
        private void CreateEnhancedLayout()
        {
            // في الإصدار الحقيقي، سيتم إنشاء علامات تبويب للأقسام المختلفة:
            // - لوحة التحكم الرئيسية
            // - مراقبة الأجهزة
            // - مزامنة المجلات
            // - التقارير
            // - إعدادات النظام
        }
        private void SetupAutoRefreshTimer()
        {
            _refreshTimer = new Timer
            {
                Interval = 30000 // تحديث كل 30 ثانية
            };
            _refreshTimer.Tick += async (s, e) => await RefreshSystemState();
            _refreshTimer.Start();
        }
        private void StartBackgroundMonitoring()
        {
            _isRunning = true;
            Task.Run(async () =>
            {
                while (_isRunning)
                {
                    try
                    {
                        await Task.Delay(5000);
                        var snapshot = _runtime.BuildSnapshot();
                        if (this.InvokeRequired)
                        {
                            this.Invoke((MethodInvoker)(() => UpdateSystemDisplay(snapshot)));
                        }
                    }
                    catch (Exception ex)
                    {
                        AppLogger.Instance.Error($"Background monitoring error: {ex.Message}", "Server");
                    }
                }
            });
        }
        private void OnATMConnectedHandler(object sender, ATMInfo atm)
        {
            AppLogger.Instance.Info($"ATM connected: {atm.ATMId}", "Server");
        }
        private void OnATMDisconnectedHandler(object sender, ATMInfo atm)
        {
            AppLogger.Instance.Info($"ATM disconnected: {atm.ATMId}", "Server");
        }
        private void OnATMUpdatedHandler(object sender, ATMInfo atm)
        {
            // ATM state updated - refresh can pick this up
        }
        private void OnJournalReceivedHandler(object sender, ReceivedPacket packet)
        {
            AppLogger.Instance.Info($"Journal received: {packet.FileName} from {packet.ATM?.ATMId}", "Server");
        }
        private void OnCommandChangedHandler(object sender, RemoteCommand command)
        {
            AppLogger.Instance.Info($"Command status changed: {command.CommandId} -> {command.Status}", "Server");
        }
        private void OnServerLogHandler(object sender, string message)
        {
            AppLogger.Instance.Info(message, "Server");
        }
        private void OnAlertRaisedHandler(object sender, AlertPayload alert)
        {
            if (this.InvokeRequired)
            {
                this.Invoke((MethodInvoker)(() => ShowAlert(alert)));
                return;
            }
            ShowAlert(alert);
        }
        private async Task RefreshSystemState()
        {
            try
            {
                var atms = await _database.GetATMsAsync();
                var onlineCount = atms.Count(a => a.Status == "online");
                var totalCount = atms.Count;
                var status = $"{onlineCount}/{totalCount} أجهزة متصلة";
                UpdateSyncStatusDisplay(status);
                UpdateStatisticsDisplay(atms);
            }
            catch (Exception ex)
            {
                AppLogger.Instance.Error($"Error refreshing system state: {ex.Message}", "Server");
            }
        }
        private void UpdateSystemDisplay(UnifiedRuntimeSnapshot snapshot)
        {
            var fleet = snapshot.Fleet;
            AppLogger.Instance.Info(
                $"Fleet: {fleet.Connected}/{fleet.Total} connected, " +
                $"Health: {fleet.AverageHealth}%, " +
                $"Alerts: {snapshot.ActiveAlerts}",
                "Dashboard");
        }
        private void ShowAlert(AlertPayload alert)
        {
            AppLogger.Instance.Warning(
                $"Alert: [{alert.Severity}] {alert.Title}: {alert.Message}",
                "Alerts");
        }
        private void UpdateSyncStatusDisplay(string status)
        {
            // تحديث عرض حالة المزامنة
        }
        private void UpdateStatisticsDisplay(List<ATMInfo> atms)
        {
            var stats = new Dictionary<string, object>
            {
                ["TotalATMs"] = atms.Count,
                ["OnlineATMs"] = atms.Count(a => a.Status == "online"),
                ["OfflineATMs"] = atms.Count(a => a.Status == "offline"),
                ["WarningATMs"] = atms.Count(a => a.Status == "warning")
            };
            // تحديث عرض الإحصائيات في الواجهة
            AppLogger.Instance.Info(
                $"Stats: {stats["TotalATMs"]} ATMs, " +
                $"{stats["OnlineATMs"]} online, " +
                $"{stats["OfflineATMs"]} offline",
                "Stats");
        }
        public void StartServer()
        {
            _serverEngine.Start();
        }
        public void StopServer()
        {
            _serverEngine.Stop();
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _isRunning = false;
                _refreshTimer?.Stop();
                _refreshTimer?.Dispose();
                _serverEngine?.Stop();
                _runtime?.Dispose();
            }
            base.Dispose(disposing);
        }
    }

}
