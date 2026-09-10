using EJLive.Core.Engine;
using EJLive.Core.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EJLive.Tests.Track09;

[TestClass]
public sealed class CashwayEjTransactionParserTests
{
    private readonly CashwayEjTransactionParser _parser = new();
    private static string SamplePath(string fileName) =>
        Path.Combine(AppContext.BaseDirectory, "Samples", fileName);

    [TestMethod]
    public void Parse_ApprovedDispenseTaken_ReturnsSuccess()
    {
        var lines = new List<string>
        {
            "2026-05-27 09:11:21 [TXN_START]",
            "CARD NUMBER: 123456******7890",
            "STAN: 900001",
            "RRN: 800001",
            "AMOUNT: 250.00",
            "CURRENCY: SAR",
            "APPROVED",
            "NOTES PRESENTED",
            "NOTES TAKEN",
            "CASSETTE 1: 10",
            "[TXN_END]"
        };

        var transactions = _parser.Parse(lines, "ATM-CW-001");

        Assert.AreEqual(1, transactions.Count);
        Assert.AreEqual(TransactionClassification.Success, transactions[0].Classification);
        Assert.AreEqual("900001", transactions[0].STAN);
        Assert.AreEqual("800001", transactions[0].RRN);
        Assert.AreEqual(250.00m, transactions[0].Amount);
    }

    [TestMethod]
    public void Parse_ApprovedWithoutTake_ReturnsApprovedNoDispense()
    {
        var lines = new List<string>
        {
            "CASHWAY TX START",
            "CARD NO: 654321******7890",
            "STAN: 900002",
            "RRN: 800002",
            "AMOUNT: 125.00",
            "APPROVED",
            "NOTES PRESENTED",
            "CASHWAY TX END"
        };

        var transactions = _parser.Parse(lines, "ATM-CW-002");

        Assert.AreEqual(1, transactions.Count);
        Assert.AreEqual(TransactionClassification.ApprovedNoDispense, transactions[0].Classification);
    }

    [TestMethod]
    public void Parse_RealAnonymizedFixture_ReturnsSuccessAndRetract()
    {
        var lines = File.ReadAllLines(SamplePath("CASHWAY_EJ_REAL_ANONYMIZED.LOG")).ToList();

        var transactions = _parser.Parse(lines, "ATM-CW-FIXTURE-01");

        Assert.AreEqual(2, transactions.Count);
        Assert.AreEqual(TransactionClassification.Success, transactions[0].Classification);
        Assert.AreEqual(TransactionClassification.Retract, transactions[1].Classification);
        Assert.AreEqual(400.00m, transactions[0].Amount);
        Assert.AreEqual("SAR", transactions[0].Currency);
    }

    [TestMethod]
    public void Parse_EdgeAnonymizedFixture_CoversApprovedNoDispenseAndJam()
    {
        var lines = File.ReadAllLines(SamplePath("CASHWAY_EJ_EDGE_ANONYMIZED.LOG")).ToList();

        var transactions = _parser.Parse(lines, "ATM-CW-FIXTURE-02");

        Assert.AreEqual(3, transactions.Count);
        Assert.AreEqual(TransactionClassification.ApprovedNoDispense, transactions[0].Classification);
        Assert.AreEqual(TransactionClassification.CashJam, transactions[1].Classification);
        Assert.AreEqual(TransactionClassification.HostDeclined, transactions[2].Classification);
        Assert.AreEqual("910102", transactions[1].STAN);
    }
}
