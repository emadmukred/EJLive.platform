using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EJLive.Core.Engine;
using EJLive.Core.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EJLive.Tests.Track06
{
    [TestClass]
    public class ChunkedTransferTests
    {
        [TestMethod]
        public void TransferSession_TotalChunks_CalculatedCorrectly()
        {
            var session = new TransferSession { Length = 100000, ChunkSize = 32768 };
            Assert.AreEqual(4, session.TotalChunks);
        }

        [TestMethod]
        public void TransferSession_IsComplete_WhenAllChunksReceived()
        {
            var session = new TransferSession { Length = 10, ChunkSize = 5 };
            session.ReceivedChunks.Add(0);
            session.ReceivedChunks.Add(1);
            Assert.IsTrue(session.IsComplete);
        }

        [TestMethod]
        public void ChunkPayload_Record_HasRequiredFields()
        {
            var payload = new ChunkPayload(Guid.NewGuid(), 0, new byte[] { 1, 2, 3 }, 0, 3, "ABC");
            Assert.AreEqual(3, payload.Length);
            Assert.AreEqual("ABC", payload.ChunkHash);
        }

        [TestMethod]
        public void ChunkAck_Record_HasNextExpectedOffset()
        {
            var ack = new ChunkAck(Guid.NewGuid(), 0, true, 4096, null);
            Assert.IsTrue(ack.Ok);
            Assert.AreEqual(4096, ack.NextExpectedOffset);
        }
    }
}
