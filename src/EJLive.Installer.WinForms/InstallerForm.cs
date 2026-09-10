using EJLive.Application.Installation;

namespace EJLive.Installer.WinForms;

public sealed class InstallerForm : Form
{
    private readonly ListBox _steps = new() { Dock = DockStyle.Left, Width = 220 };
    private readonly RichTextBox _details = new()
    {
        Dock = DockStyle.Fill,
        Font = new Font("Consolas", 9F),
        ReadOnly = true
    };
    private readonly FlowLayoutPanel _actions = new()
    {
        Dock = DockStyle.Bottom,
        Height = 52,
        Padding = new Padding(8),
        FlowDirection = FlowDirection.RightToLeft
    };
    private bool _operationInProgress;

    public InstallerForm()
    {
        Text = "EJLive Installer";
        MinimumSize = new Size(840, 560);
        Size = new Size(940, 640);
        StartPosition = FormStartPosition.CenterScreen;
        Font = new Font("Segoe UI", 9F);

        _steps.Items.AddRange(new object[]
        {
            "Overview",
            "Prerequisites",
            "Database",
            "Client Runtime",
            "Server Runtime",
            "Windows Services",
            "Finish"
        });
        _steps.SelectedIndexChanged += (_, _) => ShowStep();
        _steps.SelectedIndex = 0;

        _actions.Controls.Add(CreateActionButton("Install", RunInstallAsync));
        _actions.Controls.Add(CreateActionButton("Uninstall", RunUninstallAsync));
        _actions.Controls.Add(CreateActionButton("Validate", RunValidationAsync));
        _actions.Controls.Add(CreateActionButton("Open Data Folder", OpenDataFolderAsync));

        Controls.Add(_details);
        Controls.Add(_steps);
        Controls.Add(_actions);
    }

    private void ShowStep()
    {
        _details.Text = _steps.SelectedItem?.ToString() switch
        {
            "Overview" => "EJLive Installer provisions the database, client runtime, service registration, and governed Windows integration.",
            "Prerequisites" => ".NET 8 Windows Desktop runtime and write access to ProgramData are required.",
            "Database" => $"SQLite database: {InstallerAutomationRunner.DatabasePath}",
            "Client Runtime" => $"Client outbox: {InstallerAutomationRunner.ClientOutboxPath}{Environment.NewLine}Client inbox: {InstallerAutomationRunner.ClientInboxPath}",
            "Server Runtime" => $"Server share: {InstallerAutomationRunner.ServerSharePath}{Environment.NewLine}Archive: {InstallerAutomationRunner.ArchivePath}",
            "Windows Services" => "The installer deploys EJLive.Client.Service and configures service recovery, startup, and the selected Windows policy profile.",
            "Finish" => "Run Validate after installation and review every reported action before closing the installer.",
            _ => string.Empty
        };
    }

    private Task RunInstallAsync() =>
        ExecuteAsync("Install", InstallerAutomationRunner.RunInstall);

    private async Task RunUninstallAsync()
    {
        var decision = MessageBox.Show(
            this,
            "This removes EJLive client service registration, startup hooks, security rules, and client runtime data under ProgramData. Continue?",
            "EJLive Uninstall",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);
        if (decision == DialogResult.Yes)
            await ExecuteAsync("Uninstall", InstallerAutomationRunner.RunUninstall);
    }

    private Task RunValidationAsync() =>
        ExecuteAsync("Validation", InstallerAutomationRunner.RunValidate);

    private Task OpenDataFolderAsync() =>
        ExecuteAsync("Open Data Folder", InstallerAutomationRunner.OpenDataFolder);

    private async Task ExecuteAsync(string operationName, Func<InstallerExecutionResult> operation)
    {
        if (_operationInProgress)
            return;

        _operationInProgress = true;
        _actions.Enabled = false;
        UseWaitCursor = true;
        _details.AppendText($"{Environment.NewLine}{operationName} started...{Environment.NewLine}");

        try
        {
            var result = await Task.Run(operation);
            foreach (var line in result.Lines)
                _details.AppendText(line + Environment.NewLine);

            _details.AppendText(
                $"{operationName} {(result.Success ? "completed" : "failed")} (exit={result.ExitCode}).{Environment.NewLine}");
        }
        catch (Exception ex)
        {
            _details.AppendText($"{operationName} failed: {ex.Message}{Environment.NewLine}");
        }
        finally
        {
            UseWaitCursor = false;
            _actions.Enabled = true;
            _operationInProgress = false;
        }
    }

    private static Button CreateActionButton(string text, Func<Task> action)
    {
        var button = new Button { Text = text, AutoSize = true };
        button.Click += async (_, _) => await action();
        return button;
    }

}
