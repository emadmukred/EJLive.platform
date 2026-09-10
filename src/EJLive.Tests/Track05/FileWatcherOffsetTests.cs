using System;
using System.IO;
using System.Text;
using System.Threading;
using EJLive.Core.Data;
using EJLive.Core.Engine;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EJLive.Tests.Track05
{
    [TestClass]
    public class FileWatcherOffsetTests
    {
        [TestMethod]
        public void JournalOffsetStore_SaveAndRetrieve()
        {
            var db = Path.Combine(Path.GetTempPath(), $"offset-{Guid.NewGuid()}.db");
            using var store = new JournalOffsetStore(db);
            store.SaveOffset("ATM-01", "C:/EJ/EJDATA.LOG", "inode-123-20260523", "NCR_Overwrite", 4096, 42, "abc123");
            var rec = store.GetOffset("ATM-01", "C:/EJ/EJDATA.LOG", "inode-123-20260523");
            Assert.IsNotNull(rec);
            Assert.AreEqual(4096, rec.LastOffset);
            Assert.AreEqual(42, rec.LastLine);
            Assert.AreEqual("abc123", rec.Checksum);
        }

        [TestMethod]
        public void CompleteLineReadWindow_ReturnsCompleteLinesOnly()
        {
            var path = Path.Combine(Path.GetTempPath(), $"offset-{Guid.NewGuid()}.log");
            File.WriteAllText(path, "line1\nline2\nline3\npartial");
            var reader = new CompleteLineReadWindow(path, maxRetries: 3, retryDelayMs: 50);
            var result = reader.ReadDelta(0, settleMilliseconds: 0);
            var text = Encoding.UTF8.GetString(result.Data);
            StringAssert.Contains(text, "line1");
            StringAssert.Contains(text, "line2");
            StringAssert.Contains(text, "line3");
            StringAssert.DoesNotMatch(text, new System.Text.RegularExpressions.Regex("partial"));
            Assert.IsTrue(result.EndOffset < new FileInfo(path).Length);
        }

        [TestMethod]
        public void CompleteLineReadWindow_HandlesTruncate()
        {
            var path = Path.Combine(Path.GetTempPath(), $"truncate-{Guid.NewGuid()}.log");
            File.WriteAllText(path, "short\n");
            var reader = new CompleteLineReadWindow(path);
            var result = reader.ReadDelta(9999, settleMilliseconds: 0); // offset beyond length
            Assert.AreEqual(0, result.StartOffset);
            Assert.AreEqual(6, result.EndOffset);
        }
    }
}
