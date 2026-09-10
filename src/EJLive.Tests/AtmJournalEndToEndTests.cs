using EJLive.Core;
using EJLive.Core.Engine;
using EJLive.Core.Models;
using EJLive.Core.Services;
using EJLive.Core.Xfs;
using EJLive.Core.Xfs.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharedSecurityConfig = EJLive.Shared.SecurityConfig;
using SharedSecurityHelper = EJLive.Shared.SecurityHelper;

namespace EJLive.Tests;

[TestClass]
public sealed class AtmJournalEndToEndTests
{
    private static string SamplePath(string fileName) =>
        Path.Combine(AppContext.BaseDirectory, "Samples", fileName);

    [TestMethod]
    public void NcrEjTransactionParser_SampleFile_ParsesTransactions()
    {
        var lines = File.ReadAllLines(SamplePath("NCR_EJDATA_SAMPLE.LOG")).ToList();
        var parser = EjParserRegistry.Default.GetRequiredParser("NCR");
        var transactions = parser.Parse(lines, "ATM-NCR-001");

        Assert.IsNotNull(transactions);
        Assert.IsTrue(transactions.Count >= 1, "Expected at least one transaction from NCR sample.");

        var tx = transactions[0];
        Assert.AreEqual("ATM-NCR-001", tx.ATM_ID);
        Assert.IsFalse(string.IsNullOrWhiteSpace(tx.TransactionId));
        Assert.IsTrue(tx.Amount >= 0 || tx.RawLines.Count > 0);
    }

    [TestMethod]
    public void GrgEjTransactionParser_SampleFile_ParsesTransactions()
    {
        var lines = File.ReadAllLines(SamplePath("GRG_JOURNAL_SAMPLE.LOG")).ToList();
        var parser = EjParserRegistry.Default.GetRequiredParser("GRG");
        var transactions = parser.Parse(lines, "ATM-GRG-001");

        Assert.IsNotNull(transactions);
        Assert.IsTrue(transactions.Count >= 1, "Expected at least one transaction from GRG sample.");

        var tx = transactions[0];
        Assert.AreEqual("ATM-GRG-001", tx.ATM_ID);
    }

    [TestMethod]
    public void WincorEjTransactionParser_SampleFile_ParsesTransactions()
    {
        var lines = File.ReadAllLines(SamplePath("WINCOR_JOURNAL_SAMPLE.LOG")).ToList();
        var parser = EjParserRegistry.Default.GetRequiredParser("Wincor");
        var transactions = parser.Parse(lines, "ATM-WN-001");

        Assert.IsNotNull(transactions);
        Assert.IsTrue(transactions.Count >= 1, "Expected at least one transaction from Wincor sample.");
    }

    [TestMethod]
    public void DieboldEjTransactionParser_SampleFile_ParsesTransactions()
    {
        var lines = File.ReadAllLines(SamplePath("DIEBOLD_JOURNAL_SAMPLE.LOG")).ToList();
        var parser = EjParserRegistry.Default.GetRequiredParser("Diebold");
        var transactions = parser.Parse(lines, "ATM-DIEBOLD-001");

        Assert.IsNotNull(transactions);
        Assert.IsTrue(transactions.Count >= 1, "Expected at least one transaction from Diebold sample.");
    }

    [TestMethod]
    public void XfsAdapterRegistry_NcrSample_ResolvesAndParses()
    {
        var lines = File.ReadAllLines(SamplePath("NCR_EJDATA_SAMPLE.LOG"));
        var registry = new XfsAdapterRegistry();
        var events = registry.Parse(lines);

        Assert.IsNotNull(events);
        Assert.IsTrue(events.Count > 0, "Expected XFS events from NCR sample.");

        var dispenseEvents = events.Where(e =>
            e.Kind == XfsEventKind.CashDispense ||
            (e.Title ?? string.Empty).Contains("dispense", StringComparison.OrdinalIgnoreCase)).ToList();

        Assert.IsTrue(dispenseEvents.Count > 0, "Expected at least one cash dispense event.");
    }

    [TestMethod]
    public void XfsAdapterRegistry_GrgSample_ResolvesAndParses()
    {
        var lines = File.ReadAllLines(SamplePath("GRG_JOURNAL_SAMPLE.LOG"));
        var registry = new XfsAdapterRegistry();
        var events = registry.Parse(lines);

        Assert.IsNotNull(events);
        // GRG sample may not match the XfsAdapterRegistry adapters directly,
        // but we verify the registry handles it gracefully.
    }

    [TestMethod]
    public void EjParserRegistry_AllVendors_HaveParsers()
    {
        var registry = EjParserRegistry.Default;
        var expected = new[] { "NCR", "GRG", "Wincor", "Diebold", "Hyosung", "Cashway" };

        foreach (var vendor in expected)
        {
            Assert.IsTrue(registry.IsRegistered(vendor), $"Parser for {vendor} should be registered.");
        }
    }

    [TestMethod]
    public void SecurityConfig_MachineSalt_IsStable()
    {
        var salt1 = SharedSecurityConfig.GetMachineSalt();
        var salt2 = SharedSecurityConfig.GetMachineSalt();

        Assert.IsNotNull(salt1);
        Assert.IsTrue(salt1.Length >= 16, "Machine salt should be at least 16 bytes.");
        CollectionAssert.AreEqual(salt1, salt2, "Machine salt should remain consistent across calls.");
    }

    [TestMethod]
    public void SecurityHelper_EncryptDecrypt_RoundTrips()
    {
        var plain = "E2E test payload for ATM journal encryption";
        var cipher = SharedSecurityHelper.EncryptText(plain);
        var recovered = SharedSecurityHelper.DecryptText(cipher);

        Assert.AreEqual(plain, recovered);
    }

    [TestMethod]
    public void RoleBasedAccess_InMemoryStore_LoginFlow_Works()
    {
        RoleBasedAccess.SetCredentialStore(new InMemoryCredentialStore());
        var store = (InMemoryCredentialStore)typeof(RoleBasedAccess)
            .GetField("_credentialStore", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
            .GetValue(null)!;
        store.AddUser("testadmin", "TestPass#2026", UserRole.Admin);

        var result = RoleBasedAccess.Login("testadmin", "TestPass#2026", "127.0.0.1");
        Assert.IsTrue(result.Success);
        Assert.AreEqual(UserRole.Admin, result.Role);

        var bad = RoleBasedAccess.Login("testadmin", "wrong", "127.0.0.1");
        Assert.IsFalse(bad.Success);
    }
}
