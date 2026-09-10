using System;
using System.Collections.Generic;

namespace EJLive.Core.Vendors
{
    /// <summary>
    /// Provides vendor-specific ATM paths for journals, logs, images, and content.
    /// Each provider defines source/backup/inbox paths, extensions, rollover behavior,
    /// validation rules, and write permission requirements.
    /// </summary>
    public interface IAtmVendorPathProvider
    {
        string Vendor { get; }
        string[] JournalSourcePaths { get; }
        string[] JournalBackupPaths { get; }
        string[] TraceLogPaths { get; }
        string ImageInboxPath { get; }
        string[] ImageDestinationPaths { get; }
        string ScreenshotCachePath { get; }
        string[] SupportedExtensions { get; }
        string RolloverBehavior { get; }
        bool RequiresRestartForContentDeploy { get; }
        string[] WritePermissionNotes { get; }
    }

    /// <summary>
    /// Registry that resolves vendor path providers by vendor name.
    /// Sent to server as part of the client capability snapshot.
    /// </summary>
    public sealed class VendorPathRegistry
    {
        private readonly Dictionary<string, IAtmVendorPathProvider> _providers = new Dictionary<string, IAtmVendorPathProvider>(StringComparer.OrdinalIgnoreCase);

        public void Register(IAtmVendorPathProvider provider)
        {
            if (provider == null) throw new ArgumentNullException(nameof(provider));
            _providers[provider.Vendor] = provider;
        }

        public IAtmVendorPathProvider? Resolve(string vendor)
        {
            _providers.TryGetValue(vendor ?? string.Empty, out var result);
            return result;
        }

        public IReadOnlyCollection<string> RegisteredVendors => _providers.Keys;
    }

    // ---- Vendor-specific providers ----

    public sealed class NcrVendorPathProvider : IAtmVendorPathProvider
    {
        public string Vendor => "NCR";
        public string[] JournalSourcePaths => new[] { @"C:\NCR\EJDATA", @"D:\NCR\EJDATA" };
        public string[] JournalBackupPaths => new[] { @"C:\NCR\EJBackup", @"D:\NCR\EJBackup" };
        public string[] TraceLogPaths => new[] { @"C:\NCR\Logs", @"C:\NCR\Trace" };
        public string ImageInboxPath => @"C:\EJLive\Content\Staging\NCR";
        public string[] ImageDestinationPaths => new[] { @"C:\NCR\Content", @"D:\NCR\Content" };
        public string ScreenshotCachePath => @"C:\EJLive\Screenshots\NCR";
        public string[] SupportedExtensions => new[] { ".LOG", ".TXT", ".EJ", ".TRC" };
        public string RolloverBehavior => "Single-file overwrite (EJDATA.LOG)";
        public bool RequiresRestartForContentDeploy => true;
        public string[] WritePermissionNotes => new[] { "Content destination requires NCR application restart" };
    }

    public sealed class GrgVendorPathProvider : IAtmVendorPathProvider
    {
        public string Vendor => "GRG";
        public string[] JournalSourcePaths => new[] { @"C:\GRG\Journal", @"D:\GRG\Journal" };
        public string[] JournalBackupPaths => new[] { @"C:\GRG\JournalBackup", @"D:\GRG\JournalBackup" };
        public string[] TraceLogPaths => new[] { @"C:\GRG\Trace", @"C:\GRG\Logs" };
        public string ImageInboxPath => @"C:\EJLive\Content\Staging\GRG";
        public string[] ImageDestinationPaths => new[] { @"C:\GRG\Content", @"D:\GRG\Content" };
        public string ScreenshotCachePath => @"C:\EJLive\Screenshots\GRG";
        public string[] SupportedExtensions => new[] { ".EJ", ".TXT", ".TRC", ".LOG", ".DAT" };
        public string RolloverBehavior => "Daily files (YYYYMMDD naming pattern)";
        public bool RequiresRestartForContentDeploy => false;
        public string[] WritePermissionNotes => Array.Empty<string>();
    }

    public sealed class WincorVendorPathProvider : IAtmVendorPathProvider
    {
        public string Vendor => "Wincor";
        public string[] JournalSourcePaths => new[] { @"C:\Wincor\Journal", @"D:\Wincor\Journal" };
        public string[] JournalBackupPaths => new[] { @"C:\Wincor\JournalBackup", @"D:\Wincor\JournalBackup" };
        public string[] TraceLogPaths => new[] { @"C:\Wincor\Logs", @"C:\Wincor\ProView\Logs" };
        public string ImageInboxPath => @"C:\EJLive\Content\Staging\Wincor";
        public string[] ImageDestinationPaths => new[] { @"C:\Wincor\Content", @"C:\ProView\Content" };
        public string ScreenshotCachePath => @"C:\EJLive\Screenshots\Wincor";
        public string[] SupportedExtensions => new[] { ".LOG", ".TXT", ".PRV", ".EJ" };
        public string RolloverBehavior => "Configured by ProView — varies per model";
        public bool RequiresRestartForContentDeploy => true;
        public string[] WritePermissionNotes => new[] { "ProView service restart may be required" };
    }

    public sealed class DieboldVendorPathProvider : IAtmVendorPathProvider
    {
        public string Vendor => "Diebold";
        public string[] JournalSourcePaths => new[] { @"C:\Diebold\Journal", @"D:\Diebold\Journal" };
        public string[] JournalBackupPaths => new[] { @"C:\Diebold\JournalBackup", @"D:\Diebold\JournalBackup" };
        public string[] TraceLogPaths => new[] { @"C:\Diebold\Logs", @"C:\Diebold\Agilis\Logs" };
        public string ImageInboxPath => @"C:\EJLive\Content\Staging\Diebold";
        public string[] ImageDestinationPaths => new[] { @"C:\Diebold\Content", @"C:\Agilis\Content" };
        public string ScreenshotCachePath => @"C:\EJLive\Screenshots\Diebold";
        public string[] SupportedExtensions => new[] { ".LOG", ".TXT", ".EJ", ".AGL" };
        public string RolloverBehavior => "Agilis-managed rotation; varies per model";
        public bool RequiresRestartForContentDeploy => true;
        public string[] WritePermissionNotes => new[] { "Agilis application restart required for content changes" };
    }

    public sealed class HyosungVendorPathProvider : IAtmVendorPathProvider
    {
        public string Vendor => "Hyosung";
        public string[] JournalSourcePaths => new[] { @"C:\Hyosung\Journal", @"D:\Hyosung\Journal" };
        public string[] JournalBackupPaths => new[] { @"C:\Hyosung\JournalBackup", @"D:\Hyosung\JournalBackup" };
        public string[] TraceLogPaths => new[] { @"C:\Hyosung\Logs", @"C:\Hyosung\Trace" };
        public string ImageInboxPath => @"C:\EJLive\Content\Staging\Hyosung";
        public string[] ImageDestinationPaths => new[] { @"C:\Hyosung\Content", @"D:\Hyosung\Content" };
        public string ScreenshotCachePath => @"C:\EJLive\Screenshots\Hyosung";
        public string[] SupportedExtensions => new[] { ".LOG", ".TXT", ".EJ", ".DAT" };
        public string RolloverBehavior => "Varies per model/firmware";
        public bool RequiresRestartForContentDeploy => false;
        public string[] WritePermissionNotes => Array.Empty<string>();
    }

    public sealed class CashwayVendorPathProvider : IAtmVendorPathProvider
    {
        public string Vendor => "Cashway";
        public string[] JournalSourcePaths => new[] { @"C:\Cashway\Journal", @"D:\Cashway\Journal" };
        public string[] JournalBackupPaths => new[] { @"C:\Cashway\JournalBackup", @"D:\Cashway\JournalBackup" };
        public string[] TraceLogPaths => new[] { @"C:\Cashway\Logs", @"C:\Cashway\Trace" };
        public string ImageInboxPath => @"C:\EJLive\Content\Staging\Cashway";
        public string[] ImageDestinationPaths => new[] { @"C:\Cashway\Content", @"D:\Cashway\Content" };
        public string ScreenshotCachePath => @"C:\EJLive\Screenshots\Cashway";
        public string[] SupportedExtensions => new[] { ".LOG", ".TXT", ".EJ" };
        public string RolloverBehavior => "Varies per model";
        public bool RequiresRestartForContentDeploy => false;
        public string[] WritePermissionNotes => Array.Empty<string>();
    }

    public sealed class GenericVendorPathProvider : IAtmVendorPathProvider
    {
        public string Vendor => "Generic";
        public string[] JournalSourcePaths => new[] { @"C:\ATM\Journal" };
        public string[] JournalBackupPaths => new[] { @"C:\ATM\JournalBackup" };
        public string[] TraceLogPaths => new[] { @"C:\ATM\Logs" };
        public string ImageInboxPath => @"C:\EJLive\Content\Staging";
        public string[] ImageDestinationPaths => new[] { @"C:\ATM\Content" };
        public string ScreenshotCachePath => @"C:\EJLive\Screenshots";
        public string[] SupportedExtensions => new[] { ".LOG", ".TXT", ".EJ" };
        public string RolloverBehavior => "Unknown — manual configuration required";
        public bool RequiresRestartForContentDeploy => true;
        public string[] WritePermissionNotes => new[] { "Unverified — operator must validate all paths before activation" };
    }
}