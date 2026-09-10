using System;
using System.Collections.Generic;using System.Linq;

namespace EJLive.Core.Vendors
{
    public interface IAtmVendorPathProvider
    {
        string Vendor { get; }
        string[] JournalSourcePaths { get; }
        string[] JournalBackupPaths { get; }
        string TraceLogPath { get; }
        string ImageInboxPath { get; }
        string ImageDestinationPath { get; }
        string ScreenshotCachePath { get; }
        string[] SupportedExtensions { get; }
        bool RequiresRestart { get; }
    }

    public class VendorPathRegistry
    {
        private readonly Dictionary<string, IAtmVendorPathProvider> _providers;

        public VendorPathRegistry(IEnumerable<IAtmVendorPathProvider> providers)
        {
            _providers = providers.ToDictionary(p => p.Vendor.ToUpperInvariant(), p => p);
        }

        public IAtmVendorPathProvider GetProvider(string vendor) =>
            _providers.TryGetValue(vendor?.ToUpperInvariant() ?? "", out var p) ? p : null;

        public IReadOnlyCollection<IAtmVendorPathProvider> Providers => _providers.Values.ToList().AsReadOnly();
    }

    public class NcrVendorPathProvider : IAtmVendorPathProvider
    {
        public string Vendor => "NCR";
        public string[] JournalSourcePaths => new[] { @"C:\Program Files\NCR APATRA\Advance NDC\Data\" };
        public string[] JournalBackupPaths => new[] { @"C:\NCR_BackupLog\" };
        public string TraceLogPath => @"C:\Program Files\NCR APATRA\Advance NDC\Logs\";
        public string ImageInboxPath => @"C:\EJLive\NCR\ImageInbox\";
        public string ImageDestinationPath => @"C:\Program Files\NCR APATRA\Advance NDC\Images\";
        public string ScreenshotCachePath => @"C:\EJLive\NCR\Screenshots\";
        public string[] SupportedExtensions => new[] { ".LOG", ".LOb", ".log" };
        public bool RequiresRestart => false;
    }

    public class GrgVendorPathProvider : IAtmVendorPathProvider
    {
        public string Vendor => "GRG";
        public string[] JournalSourcePaths => new[] { @"C:\GRG\Journal\", @"C:\GRG\Data\" };
        public string[] JournalBackupPaths => new[] { @"C:\GRG_BackupLog\" };
        public string TraceLogPath => @"C:\GRG\Trace\";
        public string ImageInboxPath => @"C:\EJLive\GRG\ImageInbox\";
        public string ImageDestinationPath => @"C:\GRG\Images\";
        public string ScreenshotCachePath => @"C:\EJLive\GRG\Screenshots\";
        public string[] SupportedExtensions => new[] { ".EJ", ".TRACE", ".log" };
        public bool RequiresRestart => true;
    }

    public class WincorVendorPathProvider : IAtmVendorPathProvider
    {
        public string Vendor => "Wincor";
        public string[] JournalSourcePaths => new[] { @"C:\Wincor\Journal\", @"C:\Nixdorf\Journal\" };
        public string[] JournalBackupPaths => new[] { @"C:\Wincor_BackupLog\" };
        public string TraceLogPath => @"C:\Wincor\Logs\";
        public string ImageInboxPath => @"C:\EJLive\Wincor\ImageInbox\";
        public string ImageDestinationPath => @"C:\Wincor\Images\";
        public string ScreenshotCachePath => @"C:\EJLive\Wincor\Screenshots\";
        public string[] SupportedExtensions => new[] { ".ej", ".log", ".dat" };
        public bool RequiresRestart => true;
    }

    public class DieboldVendorPathProvider : IAtmVendorPathProvider
    {
        public string Vendor => "Diebold";
        public string[] JournalSourcePaths => new[] { @"C:\Diebold\Journal\", @"C:\Agilis\Journal\" };
        public string[] JournalBackupPaths => new[] { @"C:\Diebold_BackupLog\" };
        public string TraceLogPath => @"C:\Diebold\Logs\";
        public string ImageInboxPath => @"C:\EJLive\Diebold\ImageInbox\";
        public string ImageDestinationPath => @"C:\Diebold\Images\";
        public string ScreenshotCachePath => @"C:\EJLive\Diebold\Screenshots\";
        public string[] SupportedExtensions => new[] { ".EJ", ".evt", ".log" };
        public bool RequiresRestart => true;
    }

    public class HyosungVendorPathProvider : IAtmVendorPathProvider
    {
        public string Vendor => "Hyosung";
        public string[] JournalSourcePaths => new[] { @"C:\Hyosung\Journal\", @"C:\Hyosung\Data\" };
        public string[] JournalBackupPaths => new[] { @"C:\Hyosung_BackupLog\" };
        public string TraceLogPath => @"C:\Hyosung\Logs\";
        public string ImageInboxPath => @"C:\EJLive\Hyosung\ImageInbox\";
        public string ImageDestinationPath => @"C:\Hyosung\Images\";
        public string ScreenshotCachePath => @"C:\EJLive\Hyosung\Screenshots\";
        public string[] SupportedExtensions => new[] { ".log", ".txt", ".dat" };
        public bool RequiresRestart => false;
    }

    public class GenericVendorPathProvider : IAtmVendorPathProvider
    {
        public string Vendor => "Generic";
        public string[] JournalSourcePaths => new[] { @"C:\ATM\Journal\" };
        public string[] JournalBackupPaths => new[] { @"C:\ATM_BackupLog\" };
        public string TraceLogPath => @"C:\ATM\Logs\";
        public string ImageInboxPath => @"C:\EJLive\Generic\ImageInbox\";
        public string ImageDestinationPath => @"C:\ATM\Images\";
        public string ScreenshotCachePath => @"C:\EJLive\Generic\Screenshots\";
        public string[] SupportedExtensions => new[] { ".log", ".txt", ".dat", ".ej" };
        public bool RequiresRestart => false;
    }
}