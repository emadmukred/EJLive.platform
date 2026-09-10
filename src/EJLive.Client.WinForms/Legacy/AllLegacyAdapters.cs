using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using EJLive.Shared;

namespace EJLive.Client.WinForms.Services
{
    public sealed class ClientSyncBridge
        {
            public bool IsRunning { get; set; }
            public int PendingCount { get; set; }
            public void Start() { IsRunning = true; }
            public void Stop() { IsRunning = false; }
        }
    public sealed class JournalSyncAgent
        {
            public bool IsRunning { get; set; }
            public string Status { get; set; } = "Idle";
            public void Start() { IsRunning = true; }
            public void Stop() { IsRunning = false; }
        }
    public partial public public sealed class WindowsPolicyEnforcer
        {
            private Label _lblTitle = null!, _lblStatus = null!, _lblInfo = null!;
            private Label _lblMessage = null!;
            public WindowsPolicyEnforcer(Func<AppConfig>? configAccessor = null) {
            public bool IsRunning { get; set; }
            public int PendingCount { get; set; }
            public string Status { get; set; }
            public string AtmId { get; set; }
            public string AtmName { get; set; }
            public Color StatusColor { get; set; }
            public bool EnforceBaseline() =>
            public bool ApplyForcedConfiguration() =>
            public bool IsSystemElevated() =>
            public static void BindControlTexts(Control form, string formName) {
            public void Start() {
            public void Stop() {
            public bool SendHttpCommand(string url, string json) =>
            public string? GetHttpResponse(string url) =>
            public void Refresh()
            {
        }
    
        public partial public public static class UIBinder
        {
            private Label _lblTitle = null!, _lblStatus = null!, _lblInfo = null!;
            private Label _lblMessage = null!;
            public bool IsRunning { get; set; }
            public int PendingCount { get; set; }
            public string Status { get; set; }
            public string AtmId { get; set; }
            public string AtmName { get; set; }
            public Color StatusColor { get; set; }
            public static void BindControlTexts(Control form, string formName) {
            public void Start() {
            public void Stop() {
            public bool SendHttpCommand(string url, string json) =>
            public string? GetHttpResponse(string url) =>
            public void Refresh()
            {
        }
    
        public partial public public sealed class ClientSyncBridge
        {
            private Label _lblTitle = null!, _lblStatus = null!, _lblInfo = null!;
            private Label _lblMessage = null!;
            public bool IsRunning { get; set; }
            public int PendingCount { get; set; }
            public string Status { get; set; }
            public string AtmId { get; set; }
            public string AtmName { get; set; }
            public Color StatusColor { get; set; }
            public void Start() {
            public void Stop() {
            public bool SendHttpCommand(string url, string json) =>
            public string? GetHttpResponse(string url) =>
            public void Refresh()
            {
        }
    
        public partial public public sealed class JournalSyncAgent
        {
            private Label _lblTitle = null!, _lblStatus = null!, _lblInfo = null!;
            private Label _lblMessage = null!;
            public bool IsRunning { get; set; }
            public string Status { get; set; }
            public string AtmId { get; set; }
            public string AtmName { get; set; }
            public Color StatusColor { get; set; }
            public void Start() {
            public void Stop() {
            public bool SendHttpCommand(string url, string json) =>
            public string? GetHttpResponse(string url) =>
            public void Refresh()
            {
        }
    
        public partial public public sealed class NetworkServiceAdapter
        {
            public bool SendHttpCommand(string url, string json) =>
            public string? GetHttpResponse(string url) =>
        }
    
        public partial public public sealed class ATMCardControl : Panel
        {
            private Label _lblTitle = null!, _lblStatus = null!, _lblInfo = null!;
            private Label _lblMessage = null!;
            public ATMCardControl()
            {
            public string AtmId { get; set; }
            public string AtmName { get; set; }
            public string Status { get; set; }
            public Color StatusColor { get; set; }
            public void Refresh()
            {
        }
    
        public partial public public sealed class ToastNotification : Form
        {
            private Label _lblMessage = null!;
            public ToastNotification(string message, int durationMs = 3000)
            {
        }
    
    }
    public partial public sealed class WindowsPolicyEnforcer
        {
            public WindowsPolicyEnforcer(Func<AppConfig>? configAccessor = null) {
            public bool EnforceBaseline() =>
            public bool ApplyForcedConfiguration() =>
            public bool IsSystemElevated() =>
        }
    
        public partial public static class UIBinder
        {
            public static void BindControlTexts(Control form, string formName) {
        }
    
        public partial public sealed class ClientSyncBridge
        {
            public bool IsRunning { get; set; }
            public int PendingCount { get; set; }
            public void Start() {
            public void Stop() {
        }
    
        public partial public sealed class JournalSyncAgent
        {
            public bool IsRunning { get; set; }
            public string Status { get; set; }
            public void Start() {
            public void Stop() {
        }
    
        public partial public sealed class NetworkServiceAdapter
        {
            public bool SendHttpCommand(string url, string json) =>
            public string? GetHttpResponse(string url) =>
        }
    
        public partial public sealed class ATMCardControl : Panel
        {
            private Label _lblTitle = null!, _lblStatus = null!, _lblInfo = null!;
            public ATMCardControl()
            {
            public string AtmId { get; set; }
            public string AtmName { get; set; }
            public string Status { get; set; }
            public Color StatusColor { get; set; }
            public void Refresh()
            {
        }
    
        public partial public sealed class ToastNotification : Form
        {
            private Label _lblMessage = null!;
            public ToastNotification(string message, int durationMs = 3000)
            {
        }
    
    }

    // Class: ATMCardControl (from 2 sources)
        public sealed partial class ATMCardControl : Panel
        {
        }
    // Class: ClientSyncBridge (from 2 sources)
        public sealed partial class ClientSyncBridge
        {
        }
    // Class: JournalSyncAgent (from 2 sources)
        public sealed partial class JournalSyncAgent
        {
        }
    public sealed class NetworkServiceAdapter
        {
            public bool SendHttpCommand(string url, string json) => true;
            public string? GetHttpResponse(string url) => null;
        }
    public static class UIBinder
        {
            public static void BindControlTexts(Control form, string formName) { }
        }
    // Class: UIBinder (from 2 sources)
        public static partial class UIBinder
        {
        }
    public sealed class WindowsPolicyEnforcer
        {
            private readonly Func<AppConfig>? _configAccessor;
            public WindowsPolicyEnforcer(Func<AppConfig>? configAccessor = null) { _configAccessor = configAccessor; }
            public bool EnforceBaseline() => true;
            public bool ApplyForcedConfiguration() => true;
            public bool IsSystemElevated() => false;
        }
    // Class: WindowsPolicyEnforcer (from 3 sources)
        public sealed partial class WindowsPolicyEnforcer
        {
            // --- Constants & Fields ---
                    private readonly Func<AppConfig>? _configAccessor;
    
    
            // --- Constructors ---
                    public WindowsPolicyEnforcer(Func<AppConfig>? configAccessor = null) { _configAccessor = configAccessor; }
    
    
            // --- Methods ---
                    public bool EnforceBaseline() => true;
    
                    public bool ApplyForcedConfiguration() => true;
    
                    public bool IsSystemElevated() => false;
    
    
        }
    public partial class WindowsPolicyEnforcer
        {
            private readonly Func<AppConfig>? _configAccessor;
    
    
            public WindowsPolicyEnforcer(Func<AppConfig>? configAccessor = null) { _configAccessor = configAccessor; }
    
    
            public bool EnforceBaseline() => true;
    
    
            public bool ApplyForcedConfiguration() => true;
    
    
            public bool IsSystemElevated() => false;
    
    
        }

    public partial class ATMCardControl : Panel
        {
        }
    public partial class ClientSyncBridge
        {
        }
    public partial class JournalSyncAgent
        {
        }
    public partial class UIBinder
        {
        }
}

namespace EJLive.Client.WinForms.Controls
{
    public sealed class ATMCardControl : Panel
        {
            private Label _lblTitle = null!, _lblStatus = null!, _lblInfo = null!;
            public string AtmId { get; set; } = "";
            public string AtmName { get; set; } = "";
            public string Status { get; set; } = "Unknown";
            public Color StatusColor { get; set; } = Color.Gray;
    
            public ATMCardControl()
            {
                Size = new Size(280, 180);
                BackColor = Color.White;
                BorderStyle = BorderStyle.FixedSingle;
                Padding = new Padding(12);
    
                _lblTitle = new Label { Font = new Font("Segoe UI", 11F, FontStyle.Bold), Dock = DockStyle.Top, Height = 24 };
                _lblStatus = new Label { Font = new Font("Segoe UI", 9F), Dock = DockStyle.Top, Height = 18, ForeColor = StatusColor };
                _lblInfo = new Label { Font = new Font("Segoe UI", 8F), Dock = DockStyle.Fill };
    
                Controls.Add(_lblInfo);
                Controls.Add(_lblStatus);
                Controls.Add(_lblTitle);
    
                Refresh();
            }
    
            public void Refresh()
            {
                _lblTitle.Text = $"{AtmName} ({AtmId})";
                _lblStatus.Text = Status;
                _lblStatus.ForeColor = StatusColor;
            }
        }

    public sealed class ToastNotification : Form
        {
            private Label _lblMessage = null!;
            public ToastNotification(string message, int durationMs = 3000)
            {
                FormBorderStyle = FormBorderStyle.None;
                StartPosition = FormStartPosition.Manual;
                ShowInTaskbar = false;
                TopMost = true;
                Size = new Size(300, 60);
                BackColor = Color.FromArgb(50, 50, 50);
    
                _lblMessage = new Label
                {
                    Text = message,
                    ForeColor = Color.White,
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = new Font("Segoe UI", 9F)
                };
                Controls.Add(_lblMessage);
    
                var screen = Screen.PrimaryScreen?.WorkingArea ?? new Rectangle(0, 0, 1920, 1080);
                Location = new Point(screen.Right - Width - 20, screen.Bottom - Height - 40);
    
                var timer = new System.Windows.Forms.Timer { Interval = durationMs };
                timer.Tick += (_, _) => { timer.Stop(); Close(); };
                timer.Start();
            }
        }
}
