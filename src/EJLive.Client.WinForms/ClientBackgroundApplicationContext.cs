namespace EJLive.Client.WinForms;

public sealed class ClientBackgroundApplicationContext : ApplicationContext
{
    private readonly ClientMainForm _form;
    private readonly EJLive.Client.WinForms.Services.SessionCompanionIpcServer _ipcServer;

    public ClientBackgroundApplicationContext()
    {
        _ipcServer = new EJLive.Client.WinForms.Services.SessionCompanionIpcServer();
        _ipcServer.Start();

        _form = new ClientMainForm
        {
            ShowInTaskbar = false,
            WindowState = FormWindowState.Minimized
        };
        _form.FormClosed += (_, _) => ExitThread();
        _form.Shown += (_, _) => _form.Hide();
        MainForm = _form;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _ipcServer.Dispose();
            _form.Dispose();
        }

        base.Dispose(disposing);
    }
}
