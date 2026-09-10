using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using EJLive.Core.Engine;
using EJLive.Core.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EJLive.Tests.Track17
{
    [TestClass]
    public class StructuredLoggerTests
    {
        private string _tempDirectory = string.Empty;

        [TestInitialize]
        public void Initialize()
        {
            _tempDirectory = Path.Combine(Path.GetTempPath(), $"EJLive-Test-{Guid.NewGuid()}");
            Directory.CreateDirectory(_tempDirectory);
        }

        [TestCleanup]
        public void Cleanup()
        {
            try
            {
                if (Directory.Exists(_tempDirectory))
                {
                    Directory.Delete(_tempDirectory, recursive: true);
                }
            }
            catch
            {
                // Best-effort cleanup
            }
        }

        [TestMethod]
        public void Constructor_CreatesDirectoryIfMissing()
        {
            string newDir = Path.Combine(_tempDirectory, "nested", "logs");
            Assert.IsFalse(Directory.Exists(newDir));

            using (var logger = new StructuredLogger(newDir))
            {
                Assert.IsTrue(Directory.Exists(newDir));
            }
        }

        [TestMethod]
        public void LogEvent_WritesValidJsonLines()
        {
            using var logger = new StructuredLogger(_tempDirectory);
            Guid correlationId = Guid.NewGuid();
            logger.LogSync(correlationId, "ATM-001", "Sync started", OperationalSeverity.Info);

            string[] files = Directory.GetFiles(_tempDirectory, "*.jsonl");
            Assert.AreEqual(1, files.Length);

            string[] lines = File.ReadAllLines(files[0]);
            Assert.AreEqual(1, lines.Length);

            using var doc = JsonDocument.Parse(lines[0]);
            Assert.AreEqual("Sync", doc.RootElement.GetProperty("eventType").GetString());
            Assert.AreEqual("ATM-001", doc.RootElement.GetProperty("atmId").GetString());
            Assert.AreEqual(correlationId.ToString(), doc.RootElement.GetProperty("correlationId").GetString());
        }

        [TestMethod]
        public void LogEvent_CarriesCorrelationIdAcrossEventTypes()
        {
            using var logger = new StructuredLogger(_tempDirectory);
            Guid correlationId = Guid.NewGuid();

            logger.LogSync(correlationId, "ATM-002", "Sync", OperationalSeverity.Info);
            logger.LogCommand(correlationId, "ATM-002", "Command", OperationalSeverity.Info);
            logger.LogHeartbeat(correlationId, "ATM-002", "Heartbeat", OperationalSeverity.Debug);
            logger.LogParser(correlationId, "ATM-002", "Parser", OperationalSeverity.Warning);

            string[] files = Directory.GetFiles(_tempDirectory, "*.jsonl");
            string[] lines = File.ReadAllLines(files[0]);
            Assert.AreEqual(4, lines.Length);

            foreach (string line in lines)
            {
                using var doc = JsonDocument.Parse(line);
                Assert.AreEqual(correlationId.ToString(), doc.RootElement.GetProperty("correlationId").GetString());
            }
        }

        [TestMethod]
        public void LogEvent_AppliesRedaction()
        {
            using var logger = new StructuredLogger(_tempDirectory);
            Guid correlationId = Guid.NewGuid();
            logger.LogSync(correlationId, "ATM-003", "Card 4111111111111111 used for payment", OperationalSeverity.Info);

            string[] files = Directory.GetFiles(_tempDirectory, "*.jsonl");
            string[] lines = File.ReadAllLines(files[0]);

            using var doc = JsonDocument.Parse(lines[0]);
            string message = doc.RootElement.GetProperty("message").GetString()!;
            StringAssert.Contains(message, "[REDACTED-CARD]");
            StringAssert.DoesNotMatch(message, new System.Text.RegularExpressions.Regex(@"4111111111111111"));
        }

        [TestMethod]
        public void LogRotation_CreatesNewFile_WhenMaxSizeExceeded()
        {
            // Use a very small max file size to force rotation quickly
            using var logger = new StructuredLogger(_tempDirectory, maxFileSizeBytes: 200);
            Guid correlationId = Guid.NewGuid();

            for (int i = 0; i < 20; i++)
            {
                logger.LogSync(correlationId, "ATM-004", $"Event number {i} with padding to exceed size limit quickly", OperationalSeverity.Debug);
            }

            string[] files = Directory.GetFiles(_tempDirectory, "*.jsonl");
            Assert.IsTrue(files.Length >= 2, $"Expected at least 2 log files, found {files.Length}");
        }

        [TestMethod]
        public void HighVolume_DoesNotThrow()
        {
            using var logger = new StructuredLogger(_tempDirectory);
            Guid correlationId = Guid.NewGuid();

            Parallel.For(0, 1000, i =>
            {
                logger.LogSync(correlationId, "ATM-005", $"High volume event {i}", OperationalSeverity.Debug);
            });

            string[] files = Directory.GetFiles(_tempDirectory, "*.jsonl");
            int totalLines = files.Sum(f => File.ReadAllLines(f).Length);
            Assert.AreEqual(1000, totalLines);
        }

        [TestMethod]
        public void DisconnectedQueue_BuffersEvents()
        {
            int deliveryAttempts = 0;
            Func<OperationalEvent, bool> alwaysFail = _ =>
            {
                Interlocked.Increment(ref deliveryAttempts);
                return false;
            };

            using var logger = new StructuredLogger(_tempDirectory, serverEnqueueCallback: alwaysFail);
            Guid correlationId = Guid.NewGuid();

            logger.LogSync(correlationId, "ATM-006", "Buffered event", OperationalSeverity.Info);
            logger.LogCommand(correlationId, "ATM-006", "Buffered event 2", OperationalSeverity.Info);

            Assert.AreEqual(2, logger.ServerQueueDepth);
        }

        [TestMethod]
        public void FlushServerQueue_DeliversBufferedEvents()
        {
            var delivered = new List<OperationalEvent>();
            Func<OperationalEvent, bool> enqueue = ev =>
            {
                lock (delivered)
                {
                    delivered.Add(ev);
                }
                return true;
            };

            using var logger = new StructuredLogger(_tempDirectory, serverEnqueueCallback: enqueue);
            Guid correlationId = Guid.NewGuid();

            logger.LogSync(correlationId, "ATM-007", "Event A", OperationalSeverity.Info);
            logger.LogCommand(correlationId, "ATM-007", "Event B", OperationalSeverity.Info);

            int flushed = logger.FlushServerQueue();

            Assert.AreEqual(2, flushed);
            Assert.AreEqual(0, logger.ServerQueueDepth);
            Assert.AreEqual(2, delivered.Count);
        }

        [TestMethod]
        public void FlushServerQueue_RetainsEvents_OnDeliveryFailure()
        {
            int attemptCount = 0;
            Func<OperationalEvent, bool> failOnce = _ =>
            {
                if (Interlocked.Increment(ref attemptCount) <= 1)
                {
                    return false;
                }
                return true;
            };

            using var logger = new StructuredLogger(_tempDirectory, serverEnqueueCallback: failOnce);
            Guid correlationId = Guid.NewGuid();

            logger.LogSync(correlationId, "ATM-008", "Event", OperationalSeverity.Info);

            // First flush fails for the first item
            int flushed1 = logger.FlushServerQueue();
            Assert.AreEqual(0, flushed1);
            Assert.AreEqual(1, logger.ServerQueueDepth);

            // Second flush succeeds
            int flushed2 = logger.FlushServerQueue();
            Assert.AreEqual(1, flushed2);
            Assert.AreEqual(0, logger.ServerQueueDepth);
        }

        [TestMethod]
        public void BoundaryException_LogsWithErrorSeverity()
        {
            using var logger = new StructuredLogger(_tempDirectory);
            Guid correlationId = Guid.NewGuid();
            var ex = new InvalidOperationException("Something went wrong");

            logger.LogBoundaryException(correlationId, "ATM-009", "ProcessingBatch", ex);

            string[] files = Directory.GetFiles(_tempDirectory, "*.jsonl");
            string[] lines = File.ReadAllLines(files[0]);

            using var doc = JsonDocument.Parse(lines[0]);
            Assert.AreEqual("Boundary", doc.RootElement.GetProperty("eventType").GetString());
            Assert.AreEqual("Error", doc.RootElement.GetProperty("severity").GetString());
            StringAssert.Contains(doc.RootElement.GetProperty("message").GetString(), "Something went wrong");

            string metadataJson = doc.RootElement.GetProperty("metadataJson").GetString()!;
            using var metaDoc = JsonDocument.Parse(metadataJson);
            Assert.AreEqual("InvalidOperationException", metaDoc.RootElement.GetProperty("exceptionType").GetString());
        }

        [TestMethod]
        public void SeverityTaxonomy_AppliesCorrectLevels()
        {
            using var logger = new StructuredLogger(_tempDirectory);
            Guid correlationId = Guid.NewGuid();

            logger.LogSync(correlationId, "ATM-010", "Debug", OperationalSeverity.Debug);
            logger.LogSync(correlationId, "ATM-010", "Info", OperationalSeverity.Info);
            logger.LogSync(correlationId, "ATM-010", "Warning", OperationalSeverity.Warning);
            logger.LogSync(correlationId, "ATM-010", "Error", OperationalSeverity.Error);
            logger.LogSync(correlationId, "ATM-010", "Critical", OperationalSeverity.Critical);

            string[] files = Directory.GetFiles(_tempDirectory, "*.jsonl");
            string[] lines = File.ReadAllLines(files[0]);
            Assert.AreEqual(5, lines.Length);

            var expected = new[] { "Debug", "Info", "Warning", "Error", "Critical" };
            for (int i = 0; i < lines.Length; i++)
            {
                using var doc = JsonDocument.Parse(lines[i]);
                Assert.AreEqual(expected[i], doc.RootElement.GetProperty("severity").GetString());
            }
        }

        [TestMethod]
        public void Dispose_DoesNotThrow()
        {
            var logger = new StructuredLogger(_tempDirectory);
            logger.Dispose();
            logger.Dispose(); // idempotent
        }
    }
}
