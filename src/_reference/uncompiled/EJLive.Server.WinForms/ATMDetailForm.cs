using System;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using EJLive.Core.Models;

namespace EJLive.Server.WinForms
{
    public partial class ATMDetailForm : Form
    {
        public ATMDetailForm()
        {
            InitializeComponent();
        }

        public void BindData(string atmId, string atmName, string atmType, VendorRootProfile profile)
        {
            txtAtmId.Text = atmId ?? string.Empty;
            txtAtmName.Text = atmName ?? string.Empty;
            txtAtmType.Text = atmType ?? string.Empty;
            txtLineage.Text = profile != null ? profile.PlatformLineage.ToString() : "Unknown";
            rtbCapabilities.Text = RenderProfile(profile);
        }

        private string RenderProfile(VendorRootProfile profile)
        {
            if (profile == null)
                return "No root capability profile available.";

            var sb = new StringBuilder();
            sb.AppendLine("Vendor Root Capability Profile");
            sb.AppendLine("==============================");
            sb.AppendLine("Vendor: " + (profile.VendorName ?? "Unknown"));
            sb.AppendLine("Lineage: " + profile.PlatformLineage);
            sb.AppendLine("Filter.ini: " + profile.HasFilterIni);
            sb.AppendLine("XFS Media Templates: " + profile.HasXfsMediaTemplates);
            sb.AppendLine("Dispenser Config Data: " + profile.HasDispenserConfigData);
            sb.AppendLine("Keyboard Map Data: " + profile.HasKeyboardMapData);
            sb.AppendLine("KBAPE Config: " + profile.HasKbapeConfig);
            sb.AppendLine("Hint: " + (profile.FilterHeaderHint ?? "--"));
            sb.AppendLine();
            sb.AppendLine("Artifacts:");
            foreach (var artifact in profile.Artifacts.OrderBy(a => a.ArtifactType).ThenBy(a => a.RelativePath))
            {
                sb.AppendLine("- [" + artifact.ArtifactType + "] " + artifact.RelativePath);
                if (!string.IsNullOrWhiteSpace(artifact.Summary))
                    sb.AppendLine("  " + artifact.Summary);
            }
            return sb.ToString();
        }
    }
}
