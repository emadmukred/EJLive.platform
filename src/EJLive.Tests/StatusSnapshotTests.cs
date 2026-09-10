using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EJLive.Core.Models;

namespace EJLive.Tests
{
    [TestClass]
    public sealed class StatusSnapshotTests
    {
        [TestMethod]
        public void Snapshot_DefaultConfidence_Minimal()
        {
            var snapshot = new ATMRealTimeStatusSnapshot();
            snapshot.ComputeConfidence();
            // TotalCashRemaining defaults to 0 (>=0 check passes), adding 1 cash weight
            // Default confidence: 1/10 = 0.1 (not zero, because cash counter defaults to 0)
            Assert.AreEqual(0.1, snapshot.ConfidenceScore, 0.001);
        }

        [TestMethod]
        public void Snapshot_HeartbeatOnly_IncreasedConfidence()
        {
            var snapshot = new ATMRealTimeStatusSnapshot
            {
                LastHeartbeatUtc = DateTime.UtcNow
            };
            snapshot.ComputeConfidence();
            // Heartbeat(1) + DefaultCash(1) = 2 out of 10 = 0.2
            Assert.AreEqual(0.2, snapshot.ConfidenceScore, 0.001);
        }

        [TestMethod]
        public void Snapshot_AllEvidence_HighConfidence()
        {
            var snapshot = new ATMRealTimeStatusSnapshot
            {
                ATM_ID = "ATM001",
                ATM_Name = "Test ATM",
                Vendor = "NCR",
                LastHeartbeatUtc = DateTime.UtcNow,
                IsOnline = true,
                LastJournalFile = "EJ20260606.TXT",
                LastJournalLine = 100,
                NdcComAvailable = true,
                LastNdcMessageUtc = DateTime.UtcNow,
                XfsAvailable = true,
                DeviceStates = new Dictionary<string, string> { { "CARDREADER", "OK" } },
                CashDispenserStatus = "OK",
                CashDataAvailable = true,
                CassetteRemaining = new Dictionary<string, long> { { "CASS1", 500 } },
                TotalCashRemaining = 500
            };
            snapshot.ComputeConfidence();
            // Heartbeat(1) + Journal(2) + NDC/COM(2) + XFS(2) + Cash(2) = 9 out of 10
            Assert.AreEqual(0.9, snapshot.ConfidenceScore, 0.001);
        }

        [TestMethod]
        public void Snapshot_HasCorrectSchemaVersion()
        {
            var snapshot = new ATMRealTimeStatusSnapshot();
            Assert.AreEqual("v2.0", snapshot.SnapshotVersion);
        }

        [TestMethod]
        public void Snapshot_DefaultEvidenceSources_IsEmpty()
        {
            var snapshot = new ATMRealTimeStatusSnapshot();
            Assert.AreEqual(0, snapshot.EvidenceSources.Count);
        }

        [TestMethod]
        public void Snapshot_SnapshotUtc_IsSet()
        {
            var before = DateTime.UtcNow.AddSeconds(-1);
            var snapshot = new ATMRealTimeStatusSnapshot();
            var after = DateTime.UtcNow.AddSeconds(1);
            Assert.IsTrue(snapshot.SnapshotUtc >= before && snapshot.SnapshotUtc <= after);
        }

        [TestMethod]
        public void Snapshot_GenerateRecommendation_OnlineAtm_NoAction()
        {
            var snapshot = new ATMRealTimeStatusSnapshot
            {
                IsOnline = true,
                OperationalMode = "InService",
                LastHeartbeatUtc = DateTime.UtcNow,
                LastJournalFile = "EJ.TXT",
                LastJournalLine = 50,
                AgentServiceState = "Running",
                ConfidenceScore = 0.8
            };
            snapshot.ComputeConfidence();
            snapshot.GenerateRecommendation();
            Assert.IsNotNull(snapshot.RecommendedAction);
            Assert.AreEqual("None", snapshot.Priority);
        }

        [TestMethod]
        public void Snapshot_CashOutState_TriggersCriticalPriority()
        {
            var snapshot = new ATMRealTimeStatusSnapshot
            {
                CashOut = true,
                TotalCashRemaining = 0,
                CashDataAvailable = true
            };
            snapshot.GenerateRecommendation();
            // CashOut triggers URGENT replenishment in recommendation
            StringAssert.Contains(snapshot.RecommendedAction, "Replenish");
        }

        [TestMethod]
        public void Snapshot_ApprovedNoDispense_UpdatesCashImpact()
        {
            var snapshot = new ATMRealTimeStatusSnapshot
            {
                LastHeartbeatUtc = DateTime.UtcNow,
                ApprovedNoDispense = true,
                CashDataAvailable = true,
                CassetteRemaining = new Dictionary<string, long> { { "CASS1", 100 } }
            };
            snapshot.ComputeConfidence();
            Assert.AreEqual("Critical", snapshot.CashImpact);
        }

        [TestMethod]
        public void Snapshot_DeviceFault_UpdatesDeviceImpact()
        {
            var snapshot = new ATMRealTimeStatusSnapshot
            {
                LastHeartbeatUtc = DateTime.UtcNow,
                CashDispenserStatus = "Fault",
                CardReaderStatus = "OK"
            };
            snapshot.ComputeConfidence();
            Assert.AreEqual("Critical", snapshot.DeviceImpact);
        }
    }
}
