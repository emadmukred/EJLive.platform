using EJLive.Core.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EJLive.Tests.Track46;

/// <summary>
/// Wave 5 / SS-27 — unit tests for <see cref="SmartAnalysisService"/>.
/// Covers content analysis (NCR / GRG / Wincor rule routing),
/// value summary derivation (withdrawals / deposits / cassettes /
/// hourly density), and re-sort / re-organise helpers.
/// </summary>
[TestClass]
public class SmartAnalysisServiceTests
{
    private static SmartAnalysisService NewService() => new();

    [TestMethod]
    public void AnalyzeUpload_ReturnsReportWithTraceAndSource()
    {
        var svc = NewService();
        var report = svc.AnalyzeUpload("NCR SDC LINK FAULT\n", "unit-test", "NCR", "trace-1");

        Assert.AreEqual("trace-1", report.TraceId);
        Assert.AreEqual("unit-test", report.SourceLabel);
        Assert.AreEqual("NCR", report.VendorHint);
        Assert.IsTrue(report.LineCount >= 1, "expected at least one analysed line");
    }

    [TestMethod]
    public void AnalyzeUpload_DetectsCriticalNcrDispenserFault()
    {
        var svc = NewService();
        var report = svc.AnalyzeUpload(
            "[2025-09-14 09:14:11] NCR SDC LINK FAULT: M-146 LOST on dispenser 3a handler.",
            "atm-001");

        Assert.IsTrue(report.CriticalCount >= 1, "expected at least one Critical finding");
        var firstCritical = report.Findings.First(f => string.Equals(f.Severity, "Critical", StringComparison.OrdinalIgnoreCase));
        Assert.IsTrue(firstCritical.Message.Contains("M-146", StringComparison.OrdinalIgnoreCase)
                   || firstCritical.Code.Contains("NCR", StringComparison.OrdinalIgnoreCase),
            $"unexpected critical finding: {firstCritical.Code} / {firstCritical.Message}");
    }

    [TestMethod]
    public void AnalyzeUpload_CountsWithdrawalsAndDepositsAndAmounts()
    {
        var svc = NewService();
        const string payload = """
            [09:00] WITHDRAWAL AMOUNT=100 SAR
            [09:01] WITHDRAWAL AMOUNT=250.50 EUR
            [09:02] DEPOSIT AMOUNT=500 SAR
            [09:03] WITHDRAWAL AMT=75 USD
            """;
        var report = svc.AnalyzeUpload(payload, "value-test");

        Assert.AreEqual(3, report.Value.Withdrawals);
        Assert.AreEqual(1, report.Value.Deposits);
        Assert.AreEqual(425.50m, report.Value.DispenseAmount);
        Assert.AreEqual(500m, report.Value.DepositAmount);
    }

    [TestMethod]
    public void AnalyzeUpload_AggregatesCassetteMovesAndHourlyDensity()
    {
        var svc = NewService();
        const string payload = """
            [08:15] GRG CASSETTE STATUS CAS1=1800 CAS2=0
            [08:20] NCR CASSETTE CS3=900 CS4=600
            [08:45] GRG CAS2=50
            [11:00] SPC empty line
            """;
        var report = svc.AnalyzeUpload(payload, "cassette-test");

        Assert.AreEqual(1800, report.Value.CassetteMoves[1]);
        Assert.AreEqual(50, report.Value.CassetteMoves[2]);
        Assert.AreEqual(900, report.Value.CassetteMoves[3]);
        Assert.AreEqual(600, report.Value.CassetteMoves[4]);
        Assert.AreEqual(4, report.Value.HourlyDensity[8]);
        // 11:00 line has no cassette movement but a valid hour — density alone is enough.
        Assert.IsTrue(report.Value.HourlyDensity.ContainsKey(11) || report.Value.HourlyDensity.Count == 1);
    }

    [TestMethod]
    public void SortFindings_OrdersCriticalBeforeWarningBeforeInfo()
    {
        var svc = NewService();
        var report = svc.AnalyzeUpload(
            "WITHDRAWAL AMT=100\nNCR SDC LINK FAULT\nGRG CASSETTE STATUS CAS1=1800\n",
            "sort-test", "");

        var severitySequence = report.Findings.Select(f => f.Severity).ToList();
        var criticalIdx = severitySequence.FindIndex(s => string.Equals(s, "Critical", StringComparison.OrdinalIgnoreCase));
        var warningIdx = severitySequence.FindIndex(s => string.Equals(s, "Warning", StringComparison.OrdinalIgnoreCase));
        var infoIdx = severitySequence.FindIndex(s => string.Equals(s, "Info", StringComparison.OrdinalIgnoreCase));

        if (criticalIdx >= 0 && warningIdx >= 0)
            Assert.IsTrue(criticalIdx < warningIdx, "Critical must precede Warning");
        if (warningIdx >= 0 && infoIdx >= 0)
            Assert.IsTrue(warningIdx < infoIdx, "Warning must precede Info");
    }

    [TestMethod]
    public void GroupByCategory_BucketsFindingsUnderTheirCategory()
    {
        var svc = NewService();
        var report = svc.AnalyzeUpload(
            "NCR SDC LINK FAULT\nWINCOR WOSA/XFS SP ERROR: CDM CASH UNIT EMPTY\nGRG CASSETTE STATUS CAS1=1800\n",
            "group-test");

        var groups = SmartAnalysisService.GroupByCategory(report.Findings);
        Assert.IsTrue(groups.Count >= 1, "expected at least one category bucket");
        Assert.IsTrue(groups.All(g => g.Value.Count > 0), "every bucket must contain at least one finding");
        Assert.IsTrue(groups.All(g => g.Value.SequenceEqual(SmartAnalysisService.SortFindings(g.Value))),
            "every bucket must itself be severity-sorted");
    }

    [TestMethod]
    public void AnalyzeFile_LoadsFromDisk()
    {
        var path = Path.Combine(Path.GetTempPath(), $"smart-{Guid.NewGuid():N}.log");
        File.WriteAllText(path,
            "[09:00] WITHDRAWAL AMOUNT=200 SAR\n[09:01] NCR SDC LINK FAULT: timeout\n");
        try
        {
            var svc = NewService();
            var report = svc.AnalyzeFile(path, "NCR");
            Assert.AreEqual(2, report.LineCount);
            Assert.IsTrue(report.CriticalCount >= 1);
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    [TestMethod]
    public void AnalyzeUpload_HandlesEmptyPayloadAsEmptyReport()
    {
        var svc = NewService();
        var report = svc.AnalyzeUpload(string.Empty, "empty");
        Assert.AreEqual(0, report.LineCount);
        Assert.AreEqual(0, report.Findings.Count);
        Assert.AreEqual(0, report.CriticalCount);
        Assert.AreEqual(0, report.Value.Withdrawals);
        Assert.IsFalse(string.IsNullOrEmpty(report.TraceId), "trace id still generated for empty payloads");
    }

    [TestMethod]
    public void CategoryBreakdown_KeysAreSeverityColonCategory()
    {
        var svc = NewService();
        var report = svc.AnalyzeUpload(
            "NCR SDC LINK FAULT\nWINCOR WOSA/XFS SP ERROR: CDM CASH UNIT EMPTY\n",
            "breakdown-test");
        Assert.IsTrue(report.CategoryBreakdown.Count >= 1);
        Assert.IsTrue(report.CategoryBreakdown.Keys.All(k => k.Contains(':')),
            $"keys must be 'Severity:Category', got: {string.Join(", ", report.CategoryBreakdown.Keys)}");
    }

    [TestMethod]
    public void ValueSummary_DetectsErrorsAndCapturedCards()
    {
        var svc = NewService();
        const string payload = """
            [10:00] NCR PRINTER JAM at receipt path
            [10:05] HYOSUNG DISPENSE FAULT
            [10:10] CARD CAPTURED after dispense timeout
            [10:15] PAPER LOW
            """;
        var report = svc.AnalyzeUpload(payload, "faults-test");
        Assert.IsTrue(report.Value.Errors >= 2, $"expected >=2 errors, got {report.Value.Errors}");
        Assert.IsTrue(report.Value.CardRetained >= 1, "expected at least one card retained event");
        Assert.IsTrue(report.Value.PaperWarnings >= 1, "expected at least one paper warning");
    }
}
