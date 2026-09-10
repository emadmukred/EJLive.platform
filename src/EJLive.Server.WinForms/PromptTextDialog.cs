namespace EJLive.Server.WinForms;

internal static class PromptTextDialog
{
    public static string ShowDialog(
        IWin32Window owner,
        string title,
        string label,
        string defaultValue,
        bool isPassword = false)
    {
        using var dialog = new Form
        {
            Text = title,
            StartPosition = FormStartPosition.CenterParent,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MinimizeBox = false,
            MaximizeBox = false,
            ShowInTaskbar = false,
            ClientSize = new Size(460, 142),
            Font = new Font("Segoe UI", 9F)
        };

        var labelControl = new Label
        {
            Left = 16,
            Top = 14,
            Width = 426,
            Height = 20,
            Text = label
        };

        var input = new TextBox
        {
            Left = 16,
            Top = 38,
            Width = 426,
            Height = 28,
            Text = defaultValue ?? string.Empty,
            UseSystemPasswordChar = isPassword
        };

        var ok = new Button
        {
            Text = "OK",
            Left = 286,
            Top = 84,
            Width = 74,
            Height = 30,
            DialogResult = DialogResult.OK
        };
        var cancel = new Button
        {
            Text = "Cancel",
            Left = 368,
            Top = 84,
            Width = 74,
            Height = 30,
            DialogResult = DialogResult.Cancel
        };

        dialog.Controls.Add(labelControl);
        dialog.Controls.Add(input);
        dialog.Controls.Add(ok);
        dialog.Controls.Add(cancel);
        dialog.AcceptButton = ok;
        dialog.CancelButton = cancel;

        if (dialog.ShowDialog(owner) != DialogResult.OK)
            return string.Empty;

        return input.Text.Trim();
    }
}
