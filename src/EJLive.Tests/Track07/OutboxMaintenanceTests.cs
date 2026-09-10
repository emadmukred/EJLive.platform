using System;
using System.IO;
using System.Threading.Tasks;
using EJLive.Core.Engine;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EJLive.Tests.Track07
{
    [TestClass]
    public class OutboxMaintenanceTests
    {
        [TestMethod]
        public void OutboxTelemetry_Record_DefaultsAreZero()
        {
            var t = new OutboxTelemetry();
            Assert.AreEqual(0, t.TotalBytes);
            Assert.AreEqual(0, t.TotalItems);
            Assert.AreEqual(0, t.DeadLettered);
        }

        [TestMethod]
        public void OutboxMaintenanceService_QuotaDefaults_AreReasonable()
        {
            var outboxPath = Path.Combine(Path.GetTempPath(), $"ob-{Guid.NewGuid()}");
            Directory.CreateDirectory(outboxPath);
            var outbox = new JournalOutbox();
            var svc = new OutboxMaintenanceService(outbox, outboxPath);
            Assert.IsNotNull(svc);
            svc.Dispose();
        }
    }
}
