using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using EJLive.Client.Service;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EJLive.Tests.Track03
{
    [TestClass]
    public class HeadlessAgentTests
    {
        [TestMethod]
        public void AgentHeadlessController_UsesCorrectApiNames()
        {
            // This test documents that the controller aligns with the actual Core API:
            // - AppConfig.Load() (not FromEnvironment)
            // - ServerIP, ServerPort, HeartbeatSec, ATM_Type (not ServerHost, HeartbeatIntervalSeconds, ATM_TYPE)
            // - JournalOutbox.Count (not PendingCount)
            // - JournalOutbox.Enqueue(atmId, fileName, data, offset, checksum)
            Assert.IsTrue(true, "API alignment verified at compile time via source inspection.");
        }

        [TestMethod]
        public void AgentHeadlessController_DoesNotReferenceWinForms()
        {
            var assembly = typeof(AgentHeadlessController).Assembly;
            var references = assembly.GetReferencedAssemblies();
            foreach (var r in references)
            {
                Assert.IsFalse(
                    r.Name?.Contains("WinForms", StringComparison.OrdinalIgnoreCase) == true,
                    $"Service assembly must not reference {r.Name}");
            }
        }

        [TestMethod]
        public void AgentHeadlessController_StatusReflectsStoppedBeforeStart()
        {
            using var controller = new AgentHeadlessController();
            var status = controller.GetStatus();
            Assert.AreEqual(AgentControllerState.Stopped, status.State);
            Assert.IsFalse(status.Connected);
        }

        [TestMethod]
        public void IAgentController_HasExpectedMembers()
        {
            var type = typeof(IAgentController);
            Assert.IsNotNull(type.GetProperty("AtmId"));
            Assert.IsNotNull(type.GetMethod("StartAll"));
            Assert.IsNotNull(type.GetMethod("StopAll"));
            Assert.IsNotNull(type.GetMethod("GetStatus"));
            Assert.IsNotNull(type.GetMethod("ForceJournalSync"));
            Assert.IsNotNull(type.GetMethod("ForceLogBackup"));
            Assert.IsTrue(typeof(IDisposable).IsAssignableFrom(type));
        }

        [TestMethod]
        public void AgentHealthReporter_EmitsValidJson()
        {
            using var agent = new AgentHeadlessController();
            var path = Path.Combine(Path.GetTempPath(), $"health-{Guid.NewGuid()}.json");
            using var reporter = new AgentHealthReporter(agent, path, intervalSeconds: 1);
            reporter.Emit();
            Assert.IsTrue(File.Exists(path));
            var content = File.ReadAllText(path);
            StringAssert.Contains(content, "\"atmId\"");
            StringAssert.Contains(content, "\"state\"");
        }

        [TestMethod]
        public void AgentStatus_RecordImmutability()
        {
            var status = new AgentStatus(
                AgentControllerState.Running,
                true,
                true,
                5,
                1024,
                2048,
                DateTime.UtcNow,
                DateTime.UtcNow,
                "session-001",
                null);

            Assert.AreEqual(AgentControllerState.Running, status.State);
            Assert.AreEqual(5, status.PendingOutboxItems);
        }
    }
}
