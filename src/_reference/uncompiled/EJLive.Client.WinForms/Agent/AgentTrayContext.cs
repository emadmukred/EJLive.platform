namespace EJLive.Client.WinForms.Agent;

/// <summary>
/// Background tray context that hosts the unified agent bootstrapper.
/// </summary>
public sealed class AgentTrayContext : ApplicationContext
{
    private readonly NotifyIcon _trayIcon;
    private readonly AgentBootstrapper _bootstrapper;
    private readonly SynchronizationContext _uiContext;
    private bool _disposed;

    public AgentTrayContext()
    {
        _uiContext = SynchronizationContext.Current ?? new WindowsFormsSynchronizationContext();
        _bootstrapper = new AgentBootstrapper();
        _bootstrapper.OnLog += HandleAgentLog;

        var menu = new ContextMenuStrip();
        menu.Items.Add("Take Screenshot", null, (_, _) => _bootstrapper.TakeScreenshotNow());
        menu.Items.Add("Run Backup", null, (_, _) => _bootstrapper.BackupNow());
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("Exit", null, (_, _) => ExitApplication());

        _trayIcon = new NotifyIcon
        {
            Text = "EJLive Agent",
            Icon = SystemIcons.Application,
            ContextMenuStrip = menu,
            Visible = true
        };
        _trayIcon.DoubleClick += (_, _) => ShowStatusBalloon("EJLive Agent is running.");

        _bootstrapper.StartAll();
        ShowStatusBalloon("EJLive Agent started.");
    }

    private void HandleAgentLog(string message)
    {
        _uiContext.Post(_ =>
        {
            if (_disposed)
                return;
            var clipped = message.Length > 60 ? message.Substring(0, 60) : message;
            _trayIcon.Text = $"EJLive Agent - {clipped}";
        }, null);
    }

    private void ShowStatusBalloon(string message)
    {
        if (_disposed)
            return;
        _trayIcon.BalloonTipTitle = "EJLive Agent";
        _trayIcon.BalloonTipText = message;
        _trayIcon.BalloonTipIcon = ToolTipIcon.Info;
        _trayIcon.ShowBalloonTip(2500);
    }

    private void ExitApplication()
    {
        _bootstrapper.StopAll();
        _trayIcon.Visible = false;
        ExitThread();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _disposed = true;
            _bootstrapper.Dispose();
            _trayIcon.Dispose();
        }

        base.Dispose(disposing);
    }
}
