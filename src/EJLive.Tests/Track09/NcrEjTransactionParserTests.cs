using EJLive.Core.Engine;
using EJLive.Core.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EJLive.Tests.Track09;

[TestClass]
public sealed class NcrEjTransactionParserTests
{
    private readonly NcrEjTransactionParser _parser = new();
    private const string AtmId = "ATM-TEST-01";

    [TestMethod]
    public void Parse_SuccessTransaction_ReturnsSuccessClassification()
    {
        var lines = new List<string>
        {
            "00:01:15 *TRANSACTION START*",
            "00:01:16 CARD INSERTED",
            "00:01:18 ATR RECEIVED",
            "00:01:20 PIN ENTERED",
            "00:01:22 GENAC 1 : ARQC",
            "00:01:24 STAN: 123456",
            "00:01:25 RRN: 789012",
            "00:01:26 AMOUNT: 200.00",
            "00:01:26 CURRENCY: USD",
            "00:01:27 ACCOUNT: 12345678901234",
            "00:01:28 CARD NUMBER: 123456******7890",
            "00:01:30 APPROVED",
            "00:01:32 GENAC 2 : TC",
            "00:01:35 NOTES STACKED",
            "00:01:36 CASS?ETTE 1: 10",
            "00:01:37 CASS?ETTE 2: 10",
            "00:01:38 NOTES PRESENTED",
            "00:01:40 NOTES TAKEN",
            "00:01:42 TRANSACTION END"
        };

        var result = _parser.Parse(lines, AtmId);

        Assert.AreEqual(1, result.Count);
        var tx = result[0];
        Assert.AreEqual(TransactionClassification.Success, tx.Classification);
        Assert.IsTrue(tx.Confidence >= 0.90);
        Assert.AreEqual("123456", tx.STAN);
        Assert.AreEqual("789012", tx.RRN);
        Assert.AreEqual(200.00m, tx.Amount);
        Assert.AreEqual("USD", tx.Currency);
        Assert.AreEqual(10, tx.Cassette1);
        Assert.AreEqual(10, tx.Cassette2);
        Assert.AreEqual(19, tx.RawLines.Count);
    }

    [TestMethod]
    public void Parse_DeclinedTransaction_ReturnsHostDeclined()
    {
        var lines = new List<string>
        {
            "00:02:10 *TRANSACTION START*",
            "00:02:11 CARD INSERTED",
            "00:02:13 PIN ENTERED",
            "00:02:15 GENAC 1 : ARQC",
            "00:02:16 STAN: 111222",
            "00:02:17 RRN: 333444",
            "00:02:18 AMOUNT: 500.00",
            "00:02:19 CURRENCY: EUR",
            "00:02:20 DECLINED",
            "00:02:21 R-CODE: Z51",
            "00:02:25 TRANSACTION END"
        };

        var result = _parser.Parse(lines, AtmId);

        Assert.AreEqual(1, result.Count);
        var tx = result[0];
        Assert.AreEqual(TransactionClassification.HostDeclined, tx.Classification);
        Assert.IsTrue(tx.Confidence >= 0.90);
        Assert.AreEqual("111222", tx.STAN);
        Assert.AreEqual("Z51", tx.RCode);
    }

    [TestMethod]
    public void Parse_ReversalTransaction_ReturnsReversal()
    {
        var lines = new List<string>
        {
            "00:03:00 *TRANSACTION START*",
            "00:03:01 CARD INSERTED",
            "00:03:03 PIN ENTERED",
            "00:03:05 GENAC 1 : ARQC",
            "00:03:06 STAN: 555666",
            "00:03:07 RRN: 777888",
            "00:03:08 AMOUNT: 100.00",
            "00:03:09 CURRENCY: GBP",
            "00:03:10 APPROVED",
            "00:03:12 NOTES STACKED",
            "00:03:13 NOTES PRESENTED",
            "00:03:14 REVERSAL",
            "00:03:15 TRANSACTION END"
        };

        var result = _parser.Parse(lines, AtmId);

        Assert.AreEqual(1, result.Count);
        var tx = result[0];
        Assert.AreEqual(TransactionClassification.Reversal, tx.Classification);
        Assert.IsTrue(tx.Confidence >= 0.95);
    }

    [TestMethod]
    public void Parse_ApprovedNoDispense_ReturnsApprovedNoDispense()
    {
        var lines = new List<string>
        {
            "00:04:00 *TRANSACTION START*",
            "00:04:01 CARD INSERTED",
            "00:04:03 PIN ENTERED",
            "00:04:05 GENAC 1 : ARQC",
            "00:04:06 STAN: 999000",
            "00:04:07 RRN: 111222",
            "00:04:08 AMOUNT: 50.00",
            "00:04:09 CURRENCY: USD",
            "00:04:10 APPROVED",
            "00:04:11 GENAC 2 : TC",
            "00:04:15 M-CODE: NO_CASH",
            "00:04:20 TRANSACTION END"
        };

        var result = _parser.Parse(lines, AtmId);

        Assert.AreEqual(1, result.Count);
        var tx = result[0];
        Assert.AreEqual(TransactionClassification.ApprovedNoDispense, tx.Classification);
        Assert.IsTrue(tx.Confidence >= 0.85);
    }

    [TestMethod]
    public void Parse_PartialDispense_ReturnsPartialDispense()
    {
        var lines = new List<string>
        {
            "00:05:00 *TRANSACTION START*",
            "00:05:01 CARD INSERTED",
            "00:05:03 PIN ENTERED",
            "00:05:05 GENAC 1 : ARQC",
            "00:05:06 STAN: 444555",
            "00:05:07 RRN: 666777",
            "00:05:08 AMOUNT: 300.00",
            "00:05:09 CURRENCY: CAD",
            "00:05:10 APPROVED",
            "00:05:12 NOTES STACKED",
            "00:05:13 CASS?ETTE 1: 5",
            "00:05:14 NOTES PRESENTED",
            "00:05:30 HARDWARE FAULT",
            "00:05:35 TRANSACTION END"
        };

        var result = _parser.Parse(lines, AtmId);

        Assert.AreEqual(1, result.Count);
        var tx = result[0];
        // Hardware fault takes precedence over partial dispense in our classifier
        Assert.AreEqual(TransactionClassification.HardwareFault, tx.Classification);
    }

    [TestMethod]
    public void Parse_MultiOperationSession_ReturnsMultipleTransactions()
    {
        var lines = new List<string>
        {
            "SESSION START",
            "00:06:00 *TRANSACTION START*",
            "00:06:01 CARD INSERTED",
            "00:06:03 PIN ENTERED",
            "00:06:05 GENAC 1 : ARQC",
            "00:06:06 STAN: 100001",
            "00:06:07 RRN: 200001",
            "00:06:08 AMOUNT: 100.00",
            "00:06:09 CURRENCY: USD",
            "00:06:10 APPROVED",
            "00:06:12 NOTES STACKED",
            "00:06:13 NOTES PRESENTED",
            "00:06:14 NOTES TAKEN",
            "00:06:15 TRANSACTION END",
            "00:07:00 *TRANSACTION START*",
            "00:07:01 CARD INSERTED",
            "00:07:03 PIN ENTERED",
            "00:07:05 GENAC 1 : ARQC",
            "00:07:06 STAN: 100002",
            "00:07:07 RRN: 200002",
            "00:07:08 AMOUNT: 200.00",
            "00:07:09 CURRENCY: USD",
            "00:07:10 DECLINED",
            "00:07:11 R-CODE: Z52",
            "00:07:15 TRANSACTION END",
            "00:08:00 *TRANSACTION START*",
            "00:08:01 CARD INSERTED",
            "00:08:03 PIN ENTERED",
            "00:08:05 GENAC 1 : ARQC",
            "00:08:06 STAN: 100003",
            "00:08:07 RRN: 200003",
            "00:08:08 AMOUNT: 50.00",
            "00:08:09 CURRENCY: USD",
            "00:08:10 APPROVED",
            "00:08:12 NOTES STACKED",
            "00:08:13 NOTES PRESENTED",
            "00:08:14 NOTES TAKEN",
            "00:08:15 TRANSACTION END",
            "SESSION END"
        };

        var result = _parser.Parse(lines, AtmId);

        Assert.AreEqual(3, result.Count);

        Assert.AreEqual(TransactionClassification.Success, result[0].Classification);
        Assert.AreEqual("100001", result[0].STAN);

        Assert.AreEqual(TransactionClassification.HostDeclined, result[1].Classification);
        Assert.AreEqual("100002", result[1].STAN);

        Assert.AreEqual(TransactionClassification.Success, result[2].Classification);
        Assert.AreEqual("100003", result[2].STAN);
    }

    [TestMethod]
    public void Parse_EmptyLines_ReturnsEmptyList()
    {
        var result = _parser.Parse(new List<string>(), AtmId);
        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public void Parse_MissingIdentifiers_ReturnsMissingSTAN()
    {
        var lines = new List<string>
        {
            "00:09:00 *TRANSACTION START*",
            "00:09:01 CARD INSERTED",
            "00:09:03 PIN ENTERED",
            "00:09:05 GENAC 1 : ARQC",
            "00:09:08 AMOUNT: 75.00",
            "00:09:09 CURRENCY: USD",
            "00:09:10 REJECTED",
            "00:09:15 TRANSACTION END"
        };

        var result = _parser.Parse(lines, AtmId);

        Assert.AreEqual(1, result.Count);
        var tx = result[0];
        Assert.AreEqual(TransactionClassification.MissingSTAN, tx.Classification);
        Assert.IsTrue(string.IsNullOrEmpty(tx.STAN));
    }

    [TestMethod]
    public void Parse_CardCaptured_ReturnsCardCaptured()
    {
        var lines = new List<string>
        {
            "00:10:00 *TRANSACTION START*",
            "00:10:01 CARD INSERTED",
            "00:10:03 PIN ENTERED",
            "00:10:05 GENAC 1 : ARQC",
            "00:10:06 STAN: 888999",
            "00:10:07 RRN: 000111",
            "00:10:08 CARD CAPTURED",
            "00:10:15 TRANSACTION END"
        };

        var result = _parser.Parse(lines, AtmId);

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(TransactionClassification.CardCaptured, result[0].Classification);
    }
}
