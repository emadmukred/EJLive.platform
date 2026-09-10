using EJLive.Core;
using EJLive.Core.Models;
using EJLive.Core.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EJLive.Tests.Track08;

[TestClass]
public class VendorPathRegistryTests
{
    [TestMethod]
    public void VendorPathRegistry_IncludesRequiredProviders()
    {
        var registry = new VendorPathRegistry();
        var config = new AppConfig();
        var profiles = registry.GetAllProfiles(config);
        var keys = profiles.Select(profile => profile.VendorKey).ToHashSet(StringComparer.OrdinalIgnoreCase);

        Assert.IsTrue(keys.Contains(AppConstants.ATM_TYPE_NCR));
        Assert.IsTrue(keys.Contains(AppConstants.ATM_TYPE_GRG));
        Assert.IsTrue(keys.Contains(AppConstants.ATM_TYPE_WN));
        Assert.IsTrue(keys.Contains(AppConstants.ATM_TYPE_DN));
        Assert.IsTrue(keys.Contains(AppConstants.ATM_TYPE_HY));
        Assert.IsTrue(keys.Contains(AppConstants.ATM_TYPE_CW));
        Assert.IsTrue(keys.Contains("GENERIC"));
    }

    [TestMethod]
    public void VendorPathRegistry_Resolve_UsesConfiguredOverrides()
    {
        var config = new AppConfig
        {
            ATM_Type = AppConstants.ATM_TYPE_NCR,
            SourcePath = @"D:\CustomSource\",
            BackupPath = @"D:\CustomBackup\",
            ImageInboxPath = @"D:\CustomInbox\"
        };

        var registry = new VendorPathRegistry();
        var profile = registry.Resolve(config.ATM_Type, config);

        Assert.AreEqual(AppConstants.ATM_TYPE_NCR, profile.VendorKey);
        Assert.AreEqual(Path.GetFullPath(config.SourcePath), profile.JournalSourcePaths[0]);
        Assert.AreEqual(Path.GetFullPath(config.BackupPath), profile.BackupPaths[0]);
        Assert.AreEqual(Path.GetFullPath(config.ImageInboxPath), profile.ImageInboxPath);
        Assert.IsTrue(profile.ImageDestinationPaths[0].Contains("Images", StringComparison.OrdinalIgnoreCase));
    }

}
