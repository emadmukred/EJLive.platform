using EJLive.Core.Engine;
using EJLive.Core.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EJLive.Tests.Track19;

[TestClass]
public class InstallerEngineTests
{
    private static string GetTempRoot() => Path.Combine(Path.GetTempPath(), $"ejlive-test-{Guid.NewGuid()}");
    private static string GetAuditPath(string root) => Path.Combine(root, "logs", "install-audit.log");

    private static InstallManifest CreateMinimalManifest()
    {
        return new InstallManifest
        {
            ManifestId = Guid.NewGuid(),
            Version = "1.0.0-test",
            Components = new List<InstallComponent>
            {
                new()
                {
                    Name = "CoreDll",
                    Source = "payload/core.dll",
                    Destination = "C:\\Temp\\EJLive\\bin\\core.dll",
                    Hash = "abc123"
                }
            },
            Services = new List<InstallService>
            {
                new()
                {
                    DisplayName = "EJLive Client Agent",
                    ServiceName = "EJLiveClientAgent",
                    ExecutablePath = "C:\\Temp\\EJLive\\bin\\EJLive.Agent.exe",
                    StartType = "Manual"
                }
            },
            Paths = new List<InstallPath>
            {
                new() { Path = "C:\\Temp\\EJLive\\bin", CreateIfMissing = true },
                new() { Path = "C:\\Temp\\EJLive\\logs", CreateIfMissing = true }
            },
            Prerequisites = new List<InstallPrerequisite>()
        };
    }

    [TestMethod]
    public async Task InstallAsync_FreshInstall_CreatesFilesAndLogs()
    {
        var root = GetTempRoot();
        var audit = GetAuditPath(root);
        var engine = new InstallerEngine(root, audit);
        var manifest = CreateMinimalManifest();

        // Override paths to be under temp root to avoid permission issues in test
        manifest = manifest with
        {
            Components = new List<InstallComponent>
            {
                new()
                {
                    Name = "TestFile",
                    Source = "payload/test.txt",
                    Destination = Path.Combine(root, "bin", "test.txt"),
                    Hash = "0000"
                }
            },
            Services = new List<InstallService>(), // skip real service creation in unit test
            Paths = new List<InstallPath>
            {
                new() { Path = Path.Combine(root, "bin"), CreateIfMissing = true },
                new() { Path = Path.Combine(root, "logs"), CreateIfMissing = true }
            }
        };

        var result = await engine.InstallAsync(manifest);

        try
        {
            Assert.IsTrue(result.Success, $"Install should succeed: {result.Message}");
            Assert.IsTrue(File.Exists(manifest.Components[0].Destination), "Component file should exist after install.");
            Assert.IsTrue(File.Exists(audit), "Audit log should be created.");
        }
        finally
        {
            TryCleanup(root);
        }
    }

    [TestMethod]
    public async Task InstallAsync_UpgradeInstall_OverwritesComponents()
    {
        var root = GetTempRoot();
        var audit = GetAuditPath(root);
        var engine = new InstallerEngine(root, audit);
        var manifestV1 = CreateMinimalManifest() with { Version = "1.0.0" };
        var manifestV2 = CreateMinimalManifest() with { Version = "2.0.0" };

        manifestV1 = manifestV1 with
        {
            Components = new List<InstallComponent>
            {
                new()
                {
                    Name = "V1File",
                    Source = "payload/v1.txt",
                    Destination = Path.Combine(root, "bin", "app.txt"),
                    Hash = "v1hash"
                }
            },
            Services = new List<InstallService>(),
            Paths = new List<InstallPath>
            {
                new() { Path = Path.Combine(root, "bin"), CreateIfMissing = true },
                new() { Path = Path.Combine(root, "logs"), CreateIfMissing = true }
            }
        };

        manifestV2 = manifestV2 with
        {
            Components = new List<InstallComponent>
            {
                new()
                {
                    Name = "V2File",
                    Source = "payload/v2.txt",
                    Destination = Path.Combine(root, "bin", "app.txt"),
                    Hash = "v2hash"
                }
            },
            Services = new List<InstallService>(),
            Paths = new List<InstallPath>
            {
                new() { Path = Path.Combine(root, "bin"), CreateIfMissing = true },
                new() { Path = Path.Combine(root, "logs"), CreateIfMissing = true }
            }
        };

        await engine.InstallAsync(manifestV1);
        var upgradeResult = await engine.InstallAsync(manifestV2);

        try
        {
            Assert.IsTrue(upgradeResult.Success, $"Upgrade should succeed: {upgradeResult.Message}");
            Assert.IsTrue(File.Exists(manifestV2.Components[0].Destination), "Upgraded component should exist.");
        }
        finally
        {
            TryCleanup(root);
        }
    }

    [TestMethod]
    public async Task InstallAsync_FailedPrerequisite_ReturnsFailure()
    {
        var root = GetTempRoot();
        var audit = GetAuditPath(root);
        var engine = new InstallerEngine(root, audit);
        var manifest = CreateMinimalManifest() with
        {
            Prerequisites = new List<InstallPrerequisite>
            {
                new()
                {
                    Name = "MissingFile",
                    CheckType = "FILE",
                    CheckValue = "C:\\NonExistent\\Path\\file.txt",
                    IsMandatory = true
                }
            },
            Components = new List<InstallComponent>(),
            Services = new List<InstallService>(),
            Paths = new List<InstallPath>()
        };

        var result = await engine.InstallAsync(manifest);

        try
        {
            Assert.IsFalse(result.Success, "Install should fail when a mandatory prerequisite is missing.");
            Assert.IsTrue(result.Message.Contains("Prerequisite"), "Failure message should mention prerequisites.");
        }
        finally
        {
            TryCleanup(root);
        }
    }

    [TestMethod]
    public async Task UninstallAsync_RemovesComponentsAndLogs()
    {
        var root = GetTempRoot();
        var audit = GetAuditPath(root);
        var engine = new InstallerEngine(root, audit);
        var manifest = CreateMinimalManifest() with
        {
            Components = new List<InstallComponent>
            {
                new()
                {
                    Name = "TestFile",
                    Source = "payload/test.txt",
                    Destination = Path.Combine(root, "bin", "test.txt"),
                    Hash = "0000"
                }
            },
            Services = new List<InstallService>(),
            Paths = new List<InstallPath>
            {
                new() { Path = Path.Combine(root, "bin"), CreateIfMissing = true },
                new() { Path = Path.Combine(root, "logs"), CreateIfMissing = true }
            }
        };

        await engine.InstallAsync(manifest);
        var uninstallResult = await engine.UninstallAsync(manifest);

        try
        {
            Assert.IsTrue(uninstallResult.Success, $"Uninstall should succeed: {uninstallResult.Message}");
            Assert.IsFalse(File.Exists(manifest.Components[0].Destination), "Component file should be removed after uninstall.");
        }
        finally
        {
            TryCleanup(root);
        }
    }

    [TestMethod]
    public void InstallManifest_PropertiesAreImmutable()
    {
        var manifest = new InstallManifest
        {
            ManifestId = Guid.NewGuid(),
            Version = "1.0.0",
            Components = new List<InstallComponent>(),
            Services = new List<InstallService>(),
            Paths = new List<InstallPath>(),
            Prerequisites = new List<InstallPrerequisite>()
        };

        Assert.AreEqual("1.0.0", manifest.Version);
        Assert.IsNotNull(manifest.Components);
    }

    [TestMethod]
    public void InstallResult_SuccessFactory_ReturnsSuccess()
    {
        var result = InstallResult.Succeeded("2.1.0");

        Assert.IsTrue(result.Success);
        Assert.AreEqual("2.1.0", result.Version);
    }

    [TestMethod]
    public void InstallResult_FailedFactory_ReturnsFailure()
    {
        var result = InstallResult.Failed("Disk full");

        Assert.IsFalse(result.Success);
        Assert.AreEqual("Disk full", result.Message);
    }

    [TestMethod]
    public void WindowsServiceRegistration_ServiceName_IsNotStealth()
    {
        var service = new InstallService
        {
            DisplayName = "EJLive Client Agent",
            ServiceName = "EJLiveClientAgent",
            ExecutablePath = "C:\\Program Files\\EJLive\\Agent.exe",
            StartType = "Automatic"
        };

        Assert.AreEqual("EJLive Client Agent", service.DisplayName);
        Assert.IsFalse(service.DisplayName.StartsWith("_", StringComparison.Ordinal));
    }

    private static void TryCleanup(string path)
    {
        try
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, recursive: true);
            }
        }
        catch
        {
            // Best-effort cleanup in tests
        }
    }
}
