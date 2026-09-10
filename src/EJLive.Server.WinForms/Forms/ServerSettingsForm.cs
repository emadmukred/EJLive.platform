using EJLive.Core.Models;

namespace EJLive.Server.Forms
{
    public partial class ServerSettingsForm : Form
    {
        private readonly AppSettings _settings;
        private NumericUpDown _numPort;
        private NumericUpDown _numWorkers;
        private NumericUpDown _numChunkSize;
        private NumericUpDown _numRetryAttempts;
        private NumericUpDown _numRetryDelay;
        private CheckBox _chkExponentialBackoff;
        private CheckBox _chkJitter;
        private CheckBox _chkHMAC;
        private TextBox _txtSharedImageFolder;
        private TextBox _txtArchivePath;
        private NumericUpDown _numRetentionDays;
        private CheckBox _chkCompressArchive;
        private NumericUpDown _numRefreshInterval;
        public ServerSettingsForm(AppSettings settings)
        _settings = settings;
        InitializeComponent();
        LoadSettings();
    }

}
