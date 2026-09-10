using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EJLive.Core.Engine;
using EJLive.Core.Models;

namespace EJLive.Tests
{
    [TestClass]
    public sealed class StatusReducerContractTests
    {
        [TestMethod]
        public void Reducer_GetOrCreate_SameAtmId_ReturnsSameInstance()
        {
            using var reducer = new ATMRealTimeStatusReducer();
            var s1 = reducer.GetOrCreate("ATM001");
            var s2 = reducer.GetOrCreate("ATM001");
            Assert.AreSame(s1, s2);
        }

        [TestMethod]
        public void Reducer_GetOrCreate_DifferentAtmId_ReturnsDifferentInstances()
        {
            using var reducer = new ATMRealTimeStatusReducer();
            var s1 = reducer.GetOrCreate("ATM001");
            var s2 = reducer.GetOrCreate("ATM002");
            Assert.AreNotSame(s1, s2);
        }

        [TestMethod]
        public void Reducer_SetConnectionState_FiresSnapshotUpdated()
        {
            using var reducer = new ATMRealTimeStatusReducer();
            ATMRealTimeStatusSnapshot? received = null;
            reducer.OnSnapshotUpdated += (_, s) => received = s;

            reducer.SetConnectionState("ATM001", "Online", true, DateTime.UtcNow,
                sessionId: "SESS001");

            Assert.IsNotNull(received);
            Assert.AreEqual("ATM001", received!.ATM_ID);
            Assert.IsTrue(received.IsOnline);
            Assert.AreEqual("Online", received.ConnectionState);
            Assert.AreEqual("SESS001", received.SessionId);
            Assert.IsTrue(received.EvidenceSources.Contains("Heartbeat"));
        }

        [TestMethod]
        public void Reducer_AllEvidenceSets_AccumulateEvidenceSources()
        {
            using var reducer = new ATMRealTimeStatusReducer();
            var finalSnapshot = new List<ATMRealTimeStatusSnapshot>();
            reducer.OnSnapshotUpdated += (_, s) => finalSnapshot.Add(s);

            reducer.SetConnectionState("ATM001", "Online", true, DateTime.UtcNow);
            reducer.SetJournalEvidence("ATM001", lastJournalFile: "EJFILE.TXT", lastSyncUtc: DateTime.UtcNow, journalDeltaLines: 100);
            reducer.SetNdcComEvidence("ATM001", true, ndcState: "Active", lastMessageUtc: DateTime.UtcNow);
            reducer.SetXfsEvidence("ATM001", true, deviceStates: new Dictionary<string, string> { { "PRINTER", "OK" } });
            reducer.SetCashEvidence("ATM001", true, cassetteRemaining: new Dictionary<string, long> { { "CASS1", 1000 } }, totalCash: 1000);

            var latest = reducer.GetLatest("ATM001");
            Assert.IsNotNull(latest);
            // Evidence sources: Heartbeat, Journal, NDC/COM, XFS, Cash = 5
            Assert.AreEqual(5, latest!.EvidenceSources.Count);
            // With weighted confidence (max 10), 5 sources fully available = some score
            Assert.IsTrue(latest.ConfidenceScore > 0);
        }

        [TestMethod]
        public void Reducer_Remove_RemovesTracking()
        {
            using var reducer = new ATMRealTimeStatusReducer();
            reducer.SetConnectionState("ATM001", "Online", true, DateTime.UtcNow);
            Assert.AreEqual(1, reducer.TrackedAtmIds.Count);

            bool removed = reducer.Remove("ATM001");
            Assert.IsTrue(removed);
            Assert.AreEqual(0, reducer.TrackedAtmIds.Count);
            Assert.IsNull(reducer.GetLatest("ATM001"));
        }

        [TestMethod]
        public void Reducer_ConfidenceRecalculatesOnEachUpdate()
        {
            using var reducer = new ATMRealTimeStatusReducer();
            reducer.SetConnectionState("ATM001", "Online", true, DateTime.UtcNow);
            var afterHeartbeat = reducer.GetLatest("ATM001");
            Assert.IsTrue(afterHeartbeat!.ConfidenceScore > 0);

            reducer.SetJournalEvidence("ATM001", lastJournalFile: "EJ.TXT", lastSyncUtc: DateTime.UtcNow);
            var afterJournal = reducer.GetLatest("ATM001");
            // Confidence should increase with more evidence
            Assert.IsTrue(afterJournal!.ConfidenceScore >= afterHeartbeat.ConfidenceScore);
        }

        [TestMethod]
        public void Reducer_TrackedAtmIds_ReflectsAllActiveAtms()
        {
            using var reducer = new ATMRealTimeStatusReducer();
            reducer.SetConnectionState("ATM001", "Online", true, DateTime.UtcNow);
            reducer.SetConnectionState("ATM002", "Offline", false, null);
            reducer.SetConnectionState("ATM003", "Online", true, DateTime.UtcNow);

            var ids = reducer.TrackedAtmIds;
            Assert.AreEqual(3, ids.Count);
            Assert.IsTrue(ids.Contains("ATM001"));
            Assert.IsTrue(ids.Contains("ATM002"));
            Assert.IsTrue(ids.Contains("ATM003"));
        }
    }
}
