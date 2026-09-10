using EJLive.Client.WinForms.Services;
using EJLive.Core.Enums;
using EJLive.Core.Models;
using EJLive.Core.Utils;
using EJLive.Core;
using EJLive.Shared;
using System.Diagnostics;
using System.Drawing;
using System.IO.Compression;
using System.IO;
using System.Windows.Forms;
using System;
using var process = System.Diagnostics.Process.Start(psi);

namespace EJLive.Installer.WinForms
{
    public partial public public class InstallerForm : Form
    {
        private string _selectedRole = "";
        private ComboBox _cmbInstallType;
        private ComboBox _cmbATMType;
        private TextBox _txtDeviceName;
        private TextBox _txtBranch;
        private TextBox _txtServerIP;
        private NumericUpDown _numPort;
        private TextBox _txtInstallPath;
        private ProgressBar _progressBar;
        private Label _lblStatus;
        private Button _btnInstall;
        private Button _btnCancel;
        public InstallerForm()
        {
        private void CopyDirectory(string sourceDir, string targetDir)
        {
        private void btnInstallClient_Click(object sender, EventArgs e)
        {
        private void btnInstallServer_Click(object sender, EventArgs e)
        {
        private void btnInstall_Click(object sender, EventArgs e)
        {
        private void btnBrowsePath_Click(object sender, EventArgs e)
        {
        private void cmbATMType_SelectedIndexChanged(object sender, EventArgs e)
        {
        private void btnCancel_Click(object sender, EventArgs e)
        {
        private string ResolveWritableInstallPath(string requestedPath, out bool fallbackUsed)
        {
        private string GetDefaultWritableInstallPath()
        {
        private bool CanWriteToPath(string path)
        {
        private void UpdateATMPathPreview()
        {
        private void SaveClientConfig(string atmType, string sourcePath, string backupPath)
        {
        private void CopyDeploymentFiles(string installPath)
        {
        private bool IsSamePath(string a, string b)
        {
        private void EnsureServerShareFolders()
        {
        private void ApplyInstallerTheme()
        {
        private void InitializeComponent()
        {
        private Label CreateLabel(string text)
        {
        private void UpdateUIForType()
        {
        private async void BtnInstall_Click(object? sender, EventArgs e)
        {
        private async Task InstallServerAsync(string installPath, IProgress<int> progress)
        {
        private async Task InstallClientAsync(string installPath, IProgress<int> progress)
        {
        private async Task AddFirewallRuleAsync(int port)
        {
        private void CreateShortcut(string name, string targetPath, string targetExe)
        {
    }

}
