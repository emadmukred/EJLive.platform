using EJLive.Core.Engine;
using EJLive.Core.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EJLive.Tests.Track10;

[TestClass]
public sealed class CorrelationEngineTests
{
    private readonly CorrelationEngine _engine = new(
        mediumWindow: TimeSpan.FromMinutes(2),
        weakWindow: TimeSpan.FromMinutes(5));

    private const string AtmId = "ATM-CORR-01";

    [TestMethod]
    public void Correlate_StrongMatch_BySTAN_LinksEventToTransaction()
    {
        var baseTime = new DateTime(2024, 5, 22, 10, 0, 0, DateTimeKind.Utc);

        var transactions = new List<EjTransaction>
        {
            new(
                TransactionId: "TXN-001",
                StartLine: 0,
                EndLine: 10,
                ATM_ID: AtmId,
                CardNumber: "123456******7890",
                AccountNumber: "12********34",
                Amount: 200.00m,
                Currency: "USD",
                STAN: "123456",
                RRN: "789012",
                Cassette1: 10,
                Cassette2: null,
                Cassette3: null,
                Cassette4: null,
                MCode: "",
                RCode: "",
                RawLines: new List<string>(),
                Classification: TransactionClassification.Success,
                Confidence: 0.98,
                Timestamp: baseTime.AddSeconds(30)
            )
        };

        var events = new List<NormalizedVendorEvent>
        {
            new(
                EventId: "EVT-001",
                ATM_ID: AtmId,
                Vendor: "NCR",
                DeviceClass: "CashDispenser",
                Severity: "ERROR",
                Code: "CDM-001",
                Message: "Cash dispense error STAN: 123456",
                Timestamp: baseTime.AddSeconds(35),
                RawLine: "Cash dispense error STAN: 123456",
                SourceFile: "NCR_XFS_20240522.TRC",
                ConfidenceScore: 0.90,
                ImpactedTransactionId: string.Empty,
                CorrelationReason: string.Empty,
                FalsePositiveRisk: 0.50,
                OperatorExplanation: string.Empty
            )
        };

        var result = _engine.Correlate(events, transactions);

        Assert.AreEqual(1, result.Count);
        var correlated = result[0];
        Assert.AreEqual("TXN-001", correlated.ImpactedTransactionId);
        StringAssert.Contains(correlated.CorrelationReason, "Strong match");
        Assert.IsTrue(correlated.FalsePositiveRisk < 0.1);
    }

    [TestMethod]
    public void Correlate_StrongMatch_ByRRN_LinksEventToTransaction()
    {
        var baseTime = new DateTime(2024, 5, 22, 11, 0, 0, DateTimeKind.Utc);

        var transactions = new List<EjTransaction>
        {
            new(
                TransactionId: "TXN-002",
                StartLine: 0,
                EndLine: 5,
                ATM_ID: AtmId,
                CardNumber: "",
                AccountNumber: "",
                Amount: 100.00m,
                Currency: "EUR",
                STAN: "999888",
                RRN: "555444",
                Cassette1: null,
                Cassette2: null,
                Cassette3: null,
                Cassette4: null,
                MCode: "",
                RCode: "Z51",
                RawLines: new List<string>(),
                Classification: TransactionClassification.HostDeclined,
                Confidence: 0.95,
                Timestamp: baseTime.AddMinutes(1)
            )
        };

        var events = new List<NormalizedVendorEvent>
        {
            new(
                EventId: "EVT-002",
                ATM_ID: AtmId,
                Vendor: "NCR",
                DeviceClass: "Status",
                Severity: "INFO",
                Code: "STS-100",
                Message: "Host response RRN: 555444 declined",
                Timestamp: baseTime.AddMinutes(1).AddSeconds(10),
                RawLine: "Host response RRN: 555444 declined",
                SourceFile: "NCR_XFS_20240522.TRC",
                ConfidenceScore: 0.85,
                ImpactedTransactionId: string.Empty,
                CorrelationReason: string.Empty,
                FalsePositiveRisk: 0.50,
                OperatorExplanation: string.Empty
            )
        };

        var result = _engine.Correlate(events, transactions);

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("TXN-002", result[0].ImpactedTransactionId);
        StringAssert.Contains(result[0].CorrelationReason, "Strong match");
    }

    [TestMethod]
    public void Correlate_MediumMatch_ByTimestampAndDeviceClass_LinksEventToTransaction()
    {
        var baseTime = new DateTime(2024, 5, 22, 12, 0, 0, DateTimeKind.Utc);

        var transactions = new List<EjTransaction>
        {
            new(
                TransactionId: "TXN-003",
                StartLine: 0,
                EndLine: 8,
                ATM_ID: AtmId,
                CardNumber: "",
                AccountNumber: "",
                Amount: 300.00m,
                Currency: "USD",
                STAN: "111222",
                RRN: "333444",
                Cassette1: 5,
                Cassette2: 5,
                Cassette3: null,
                Cassette4: null,
                MCode: "",
                RCode: "",
                RawLines: new List<string>(),
                Classification: TransactionClassification.Success,
                Confidence: 0.97,
                Timestamp: baseTime.AddMinutes(2)
            )
        };

        var events = new List<NormalizedVendorEvent>
        {
            new(
                EventId: "EVT-003",
                ATM_ID: AtmId,
                Vendor: "NCR",
                DeviceClass: "CashDispenser",
                Severity: "WARNING",
                Code: "CDM-020",
                Message: "Cassette 1 low cash warning",
                Timestamp: baseTime.AddMinutes(2).AddSeconds(45),
                RawLine: "Cassette 1 low cash warning",
                SourceFile: $"NCR_XFS_{AtmId}_20240522.TRC",
                ConfidenceScore: 0.80,
                ImpactedTransactionId: string.Empty,
                CorrelationReason: string.Empty,
                FalsePositiveRisk: 0.50,
                OperatorExplanation: string.Empty
            )
        };

        var result = _engine.Correlate(events, transactions);

        Assert.AreEqual(1, result.Count);
        var correlated = result[0];
        Assert.AreEqual("TXN-003", correlated.ImpactedTransactionId);
        StringAssert.Contains(correlated.CorrelationReason, "Medium match");
        Assert.IsTrue(correlated.FalsePositiveRisk < 0.3);
    }

    [TestMethod]
    public void Correlate_WeakMatch_ByNearbyTimestamp_LinksEventToTransaction()
    {
        var baseTime = new DateTime(2024, 5, 22, 13, 0, 0, DateTimeKind.Utc);

        var transactions = new List<EjTransaction>
        {
            new(
                TransactionId: "TXN-004",
                StartLine: 0,
                EndLine: 6,
                ATM_ID: AtmId,
                CardNumber: "",
                AccountNumber: "",
                Amount: 50.00m,
                Currency: "GBP",
                STAN: "777888",
                RRN: "999000",
                Cassette1: 2,
                Cassette2: null,
                Cassette3: null,
                Cassette4: null,
                MCode: "",
                RCode: "",
                RawLines: new List<string>(),
                Classification: TransactionClassification.PartialDispense,
                Confidence: 0.85,
                Timestamp: baseTime.AddMinutes(3)
            )
        };

        var events = new List<NormalizedVendorEvent>
        {
            new(
                EventId: "EVT-004",
                ATM_ID: AtmId,
                Vendor: "NCR",
                DeviceClass: "CashDispenser",
                Severity: "ERROR",
                Code: "CDM-500",
                Message: "Unexpected sensor state after dispense",
                Timestamp: baseTime.AddMinutes(3).AddSeconds(120),
                RawLine: "Unexpected sensor state after dispense",
                SourceFile: $"NCR_XFS_{AtmId}_20240522.TRC",
                ConfidenceScore: 0.75,
                ImpactedTransactionId: string.Empty,
                CorrelationReason: string.Empty,
                FalsePositiveRisk: 0.50,
                OperatorExplanation: string.Empty
            )
        };

        var result = _engine.Correlate(events, transactions);

        Assert.AreEqual(1, result.Count);
        var correlated = result[0];
        Assert.AreEqual("TXN-004", correlated.ImpactedTransactionId);
        StringAssert.Contains(correlated.CorrelationReason, "Weak match");
        Assert.IsTrue(correlated.FalsePositiveRisk >= 0.5);
    }

    [TestMethod]
    public void Correlate_NoMatch_ReturnsEmptyTransactionId()
    {
        var baseTime = new DateTime(2024, 5, 22, 14, 0, 0, DateTimeKind.Utc);

        var transactions = new List<EjTransaction>
        {
            new(
                TransactionId: "TXN-005",
                StartLine: 0,
                EndLine: 5,
                ATM_ID: AtmId,
                CardNumber: "",
                AccountNumber: "",
                Amount: 150.00m,
                Currency: "USD",
                STAN: "000111",
                RRN: "222333",
                Cassette1: null,
                Cassette2: null,
                Cassette3: null,
                Cassette4: null,
                MCode: "",
                RCode: "",
                RawLines: new List<string>(),
                Classification: TransactionClassification.Success,
                Confidence: 0.96,
                Timestamp: baseTime
            )
        };

        var events = new List<NormalizedVendorEvent>
        {
            new(
                EventId: "EVT-005",
                ATM_ID: "ATM-DIFFERENT-02",
                Vendor: "NCR",
                DeviceClass: "Printer",
                Severity: "INFO",
                Code: "PRN-001",
                Message: "Receipt paper low",
                Timestamp: baseTime.AddHours(2),
                RawLine: "Receipt paper low",
                SourceFile: "NCR_XFS_20240522.TRC",
                ConfidenceScore: 0.70,
                ImpactedTransactionId: string.Empty,
                CorrelationReason: string.Empty,
                FalsePositiveRisk: 0.50,
                OperatorExplanation: string.Empty
            )
        };

        var result = _engine.Correlate(events, transactions);

        Assert.AreEqual(1, result.Count);
        var correlated = result[0];
        Assert.AreEqual(string.Empty, correlated.ImpactedTransactionId);
        StringAssert.Contains(correlated.CorrelationReason, "No correlation");
        Assert.AreEqual(1.0, correlated.FalsePositiveRisk);
    }

    [TestMethod]
    public void Correlate_MultipleEvents_ReturnsCorrectLinks()
    {
        var baseTime = new DateTime(2024, 5, 22, 15, 0, 0, DateTimeKind.Utc);

        var transactions = new List<EjTransaction>
        {
            new(
                TransactionId: "TXN-A",
                StartLine: 0,
                EndLine: 5,
                ATM_ID: AtmId,
                CardNumber: "",
                AccountNumber: "",
                Amount: 100.00m,
                Currency: "USD",
                STAN: "100001",
                RRN: "200001",
                Cassette1: null,
                Cassette2: null,
                Cassette3: null,
                Cassette4: null,
                MCode: "",
                RCode: "",
                RawLines: new List<string>(),
                Classification: TransactionClassification.Success,
                Confidence: 0.98,
                Timestamp: baseTime
            ),
            new(
                TransactionId: "TXN-B",
                StartLine: 10,
                EndLine: 15,
                ATM_ID: AtmId,
                CardNumber: "",
                AccountNumber: "",
                Amount: 200.00m,
                Currency: "USD",
                STAN: "100002",
                RRN: "200002",
                Cassette1: null,
                Cassette2: null,
                Cassette3: null,
                Cassette4: null,
                MCode: "",
                RCode: "Z52",
                RawLines: new List<string>(),
                Classification: TransactionClassification.HostDeclined,
                Confidence: 0.95,
                Timestamp: baseTime.AddMinutes(5)
            )
        };

        var events = new List<NormalizedVendorEvent>
        {
            new(
                EventId: "EVT-A1",
                ATM_ID: AtmId,
                Vendor: "NCR",
                DeviceClass: "CashDispenser",
                Severity: "INFO",
                Code: "CDM-010",
                Message: "Dispense ok STAN: 100001",
                Timestamp: baseTime.AddSeconds(30),
                RawLine: "Dispense ok STAN: 100001",
                SourceFile: "NCR_XFS.TRC",
                ConfidenceScore: 0.90,
                ImpactedTransactionId: string.Empty,
                CorrelationReason: string.Empty,
                FalsePositiveRisk: 0.50,
                OperatorExplanation: string.Empty
            ),
            new(
                EventId: "EVT-B1",
                ATM_ID: AtmId,
                Vendor: "NCR",
                DeviceClass: "Status",
                Severity: "ERROR",
                Code: "HOST-001",
                Message: "Decline response RRN: 200002",
                Timestamp: baseTime.AddMinutes(5).AddSeconds(15),
                RawLine: "Decline response RRN: 200002",
                SourceFile: "NCR_XFS.TRC",
                ConfidenceScore: 0.85,
                ImpactedTransactionId: string.Empty,
                CorrelationReason: string.Empty,
                FalsePositiveRisk: 0.50,
                OperatorExplanation: string.Empty
            )
        };

        var result = _engine.Correlate(events, transactions);

        Assert.AreEqual(2, result.Count);
        Assert.AreEqual("TXN-A", result[0].ImpactedTransactionId);
        Assert.AreEqual("TXN-B", result[1].ImpactedTransactionId);
    }

    [TestMethod]
    public void NcrXfsAdapter_CanHandle_NcrFile_ReturnsTrue()
    {
        var adapter = new NcrXfsAdapter();
        Assert.IsTrue(adapter.CanHandle("NCR_XFS_20240522.TRC"));
        Assert.IsTrue(adapter.CanHandle("ATM01.NTR"));
        Assert.IsTrue(adapter.CanHandle("some_ncr_log.txt"));
    }

    [TestMethod]
    public void NcrXfsAdapter_CanHandle_NonNcrFile_ReturnsFalse()
        {
        var adapter = new NcrXfsAdapter();
        Assert.IsFalse(adapter.CanHandle("Diebold_20240522.log"));
        Assert.IsFalse(adapter.CanHandle("WincorTrace.bin"));
    }

    [TestMethod]
    public void NcrXfsAdapter_Parse_ExtractsEvents()
    {
        var adapter = new NcrXfsAdapter();
        var lines = new List<string>
        {
            "10:00:00 INFO CashDispenser CDM-001 Ready",
            "10:00:15 ERROR CashDispenser CDM-500 Dispense fault STAN: 123456",
            "10:00:30 WARNING CardReader CRD-010 Card read timeout"
        };

        var result = adapter.Parse("NCR_XFS_20240522.TRC", lines);

        Assert.AreEqual(3, result.Count);
        Assert.AreEqual("NCR", result[0].Vendor);
        Assert.AreEqual("CashDispenser", result[0].DeviceClass);
        Assert.AreEqual("INFO", result[0].Severity);
        Assert.AreEqual("CDM-001", result[0].Code);

        Assert.AreEqual("ERROR", result[1].Severity);
        Assert.AreEqual("123456", result[1].RawLine.Contains("STAN: 123456") ? "123456" : string.Empty);
    }
}
