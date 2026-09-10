using EJLive.Core.Engine;
using EJLive.Core.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EJLive.Tests.Track13;

[TestClass]
public class RemoteAssistanceTests
{
    private static RemoteSessionRequest CreateRequest(
        RemoteSessionType type = RemoteSessionType.RemoteAssistance,
        int durationMinutes = 30)
    {
        return new RemoteSessionRequest
        {
            RequestId = Guid.NewGuid(),
            OperatorId = "OP001",
            AtmId = "ATM001",
            Reason = "Test remote assistance",
            RequestedDurationMinutes = durationMinutes,
            RequestedUtc = DateTime.UtcNow.AddMinutes(-5),
            ApprovedUtc = DateTime.UtcNow.AddMinutes(-1),
            ApprovedBy = "SUPERVISOR01",
            SessionType = type
        };
    }

    [TestMethod]
    public void CreateExecutionPlan_NoActiveSession_ReturnsAllowedPlan()
    {
        // Arrange
        var policy = new TestPolicy(allowType: true, allowOperator: true, allowNoConsent: false);
        var auditStore = new TestAuditStore();
        var notifier = new TestNotifier();
        var engine = new RemoteAssistanceSession(policy, auditStore, notifier);

        var request = CreateRequest();
        var activeSessions = Array.Empty<RemoteSessionAudit>();

        // Act
        var plan = engine.CreateExecutionPlan(request, activeSessions);

        // Assert
        Assert.IsTrue(plan.IsAllowed);
        Assert.IsTrue(plan.RequiresConsentPrompt);
        Assert.AreEqual(request.RequestId, plan.RequestId);
    }

    [TestMethod]
    public void CreateExecutionPlan_Session0Service_DeniesDueToActiveSession()
    {
        // Arrange
        var policy = new TestPolicy(allowType: true, allowOperator: true, allowNoConsent: false);
        var auditStore = new TestAuditStore();
        var notifier = new TestNotifier();
        var engine = new RemoteAssistanceSession(policy, auditStore, notifier);

        var request = CreateRequest();
        var activeSessions = new List<RemoteSessionAudit>
        {
            new RemoteSessionAudit
            {
                RequestId = Guid.NewGuid(),
                SessionId = Guid.NewGuid(),
                StartUtc = DateTime.UtcNow.AddMinutes(-10),
                Outcome = RemoteSessionOutcome.Success
            }
        };

        // Act
        var plan = engine.CreateExecutionPlan(request, activeSessions);

        // Assert
        Assert.IsFalse(plan.IsAllowed);
        Assert.IsNotNull(plan.DenialReason);
        StringAssert.Contains(plan.DenialReason, "active session");
    }

    [TestMethod]
    public void CreateExecutionPlan_DeniedPolicy_ReturnsDeniedPlan()
    {
        // Arrange
        var policy = new TestPolicy(allowType: false, allowOperator: true, allowNoConsent: false);
        var auditStore = new TestAuditStore();
        var notifier = new TestNotifier();
        var engine = new RemoteAssistanceSession(policy, auditStore, notifier);

        var request = CreateRequest(type: RemoteSessionType.Rdp);
        var activeSessions = Array.Empty<RemoteSessionAudit>();

        // Act
        var plan = engine.CreateExecutionPlan(request, activeSessions);

        // Assert
        Assert.IsFalse(plan.IsAllowed);
        Assert.IsNotNull(plan.DenialReason);
        StringAssert.Contains(plan.DenialReason, "not permitted");
    }

    [TestMethod]
    public void CreateExecutionPlan_BlockedNoConsent_RequiresConsentPrompt()
    {
        // Arrange
        var policy = new TestPolicy(allowType: true, allowOperator: true, allowNoConsent: true);
        var auditStore = new TestAuditStore(hasWaiver: false); // no waiver in audit
        var notifier = new TestNotifier();
        var engine = new RemoteAssistanceSession(policy, auditStore, notifier);

        var request = CreateRequest();
        var activeSessions = Array.Empty<RemoteSessionAudit>();

        // Act
        var plan = engine.CreateExecutionPlan(request, activeSessions);

        // Assert
        Assert.IsTrue(plan.IsAllowed);
        Assert.IsTrue(plan.RequiresConsentPrompt);
        Assert.IsFalse(plan.NoConsentAllowed);
    }

    [TestMethod]
    public async Task StopSession_RecordsStopAndNotifies()
    {
        // Arrange
        var policy = new TestPolicy(allowType: true, allowOperator: true, allowNoConsent: false);
        var auditStore = new TestAuditStore();
        var notifier = new TestNotifier();
        var engine = new RemoteAssistanceSession(policy, auditStore, notifier);

        var request = CreateRequest();
        var plan = engine.CreateExecutionPlan(request, Array.Empty<RemoteSessionAudit>());
        var audit = await engine.StartAsync(plan);

        // Act
        var stopped = await engine.StopAsync(audit, "Operator ended session.");

        // Assert
        Assert.IsNotNull(stopped.EndUtc);
        Assert.AreEqual(RemoteSessionOutcome.Stopped, stopped.Outcome);
        Assert.AreEqual("Operator ended session.", stopped.StopReason);
        Assert.IsTrue(auditStore.Stops.Count > 0);
        Assert.IsTrue(notifier.StopNotifications.Count > 0);
    }

    // Test helpers

    private class TestPolicy : IRemoteSessionPolicy
    {
        private readonly bool _allowType;
        private readonly bool _allowOperator;
        private readonly bool _allowNoConsent;

        public TestPolicy(bool allowType, bool allowOperator, bool allowNoConsent)
        {
            _allowType = allowType;
            _allowOperator = allowOperator;
            _allowNoConsent = allowNoConsent;
        }

        public bool IsSessionTypeAllowed(RemoteSessionType sessionType) => _allowType;
        public bool IsOperatorAllowed(string operatorId) => _allowOperator;
        public bool AllowNoConsentPrompt(RemoteSessionType sessionType) => _allowNoConsent;
    }

    private class TestAuditStore : IRemoteSessionAuditStore
    {
        private readonly bool _hasWaiver;
        public List<RemoteSessionAudit> Starts { get; } = new();
        public List<RemoteSessionAudit> Stops { get; } = new();

        public TestAuditStore(bool hasWaiver = false)
        {
            _hasWaiver = hasWaiver;
        }

        public Task RecordStartAsync(RemoteSessionAudit audit, CancellationToken cancellationToken = default)
        {
            Starts.Add(audit);
            return Task.CompletedTask;
        }

        public Task RecordStopAsync(RemoteSessionAudit audit, CancellationToken cancellationToken = default)
        {
            Stops.Add(audit);
            return Task.CompletedTask;
        }

        public bool HasExplicitConsentWaiver(RemoteSessionRequest request) => _hasWaiver;
    }

    private class TestNotifier : IRemoteSessionNotifier
    {
        public List<RemoteSessionAudit> StartNotifications { get; } = new();
        public List<RemoteSessionAudit> StopNotifications { get; } = new();

        public Task NotifySessionStartedAsync(RemoteSessionAudit audit, CancellationToken cancellationToken = default)
        {
            StartNotifications.Add(audit);
            return Task.CompletedTask;
        }

        public Task NotifySessionStoppedAsync(RemoteSessionAudit audit, CancellationToken cancellationToken = default)
        {
            StopNotifications.Add(audit);
            return Task.CompletedTask;
        }
    }
}
