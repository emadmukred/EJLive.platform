using EJLive.Core.Enums;
using EJLive.Core.Models;
using EJLive.Core.Utils;
using EJLive.Server.Services;
using System.ComponentModel;
using System.Drawing.Drawing2D;

namespace EJLive.Server.Forms
{
    public partial class DashboardForm : Form
    {
        private readonly AppSettings _settings;
        private readonly ServerServiceController _serviceController;
        private System.Windows.Forms.Timer _refreshTimer;
        private DataGridView _dgvDevices;
        private Panel _alertsPanel;
        private FlowLayoutPanel _kpiPanel;
        private Label _lblOnlineCount;
        private Label _lblTotalCount;
        private Label _lblCriticalCount;
        private Label _lblSyncRate;
        private ListView _alertListView;
        private TextBox _txtSearch;
        private ComboBox _cmbFilter;
        private RichTextBox _logBox;
        public DashboardForm(AppSettings settings)
        _settings = settings;
        _serviceController = new ServerServiceController(settings);
        InitializeComponent();
        SetupEventHandlers();
        _serviceController.Start();
    }

}
