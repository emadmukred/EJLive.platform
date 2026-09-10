using System;
using System.Collections.Generic;
using EJLive.Core.Engine;
using EJLive.Core.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EJLive.Tests.Track11
{
    /// <summary>
    /// MSTest suite for SafeRemoteCommandExecutor and RemoteCommandPolicy.
    /// </summary>
    [TestClass]
    public class SafeRemoteCommandTests
    {
        private static HashSet<string> DefaultPresets => new(StringComparer.OrdinalIgnoreCase)
        {
            "RestartService",
            "CollectDiagnostics",
            "Screenshot"
        };

        private static RemoteCommandEnvelope CreateEnvelope(
            string commandType,
            string payload,
            string signature,
            DateTime timestampUtc,
            DateTime expiryUtc,
            string issuerRole,
            string issuerId,
            CommandRiskLevel riskLevel)
        {
            return new RemoteCommandEnvelope
            {
                CommandId = Guid.NewGuid().ToString("N"),
                CommandType = commandType,
                Payload = payload,
                Signature = signature,
                TimestampUtc = timestampUtc,
                ExpiryUtc = expiryUtc,
                IssuerRole = issuerRole,
                IssuerId = issuerId,
                RiskLevel = riskLevel
            };
        }

        [TestMethod]
        public void TamperedSignature_IsRejected()
        {
            var envelope = CreateEnvelope(
                "Screenshot",
                "{}",
                "INVALID", // does not start with "sig:"
                DateTime.UtcNow.AddMinutes(-1),
                DateTime.UtcNow.AddMinutes(5),
                "Admin",
                "user-1",
                CommandRiskLevel.Low);

            var executor = new SafeRemoteCommandExecutor(DefaultPresets, false, _ => string.Empty);
            var success = executor.TryExecute(envelope, DateTime.UtcNow, false, false, out var result);

            Assert.IsFalse(success, "Expected tampered signature to be rejected.");
            StringAssert.Contains(result, "Signature verification failed.");
        }

        [TestMethod]
        public void StaleTimestamp_IsRejected()
        {
            var envelope = CreateEnvelope(
                "Screenshot",
                "{}",
                "sig:valid",
                DateTime.UtcNow.AddHours(-2),
                DateTime.UtcNow.AddHours(-1), // already expired
                "Admin",
                "user-1",
                CommandRiskLevel.Low);

            var executor = new SafeRemoteCommandExecutor(DefaultPresets, false, _ => string.Empty);
            var success = executor.TryExecute(envelope, DateTime.UtcNow, false, false, out var result);

            Assert.IsFalse(success, "Expected stale timestamp to be rejected.");
            StringAssert.Contains(result, "Command has expired.");
        }

        [TestMethod]
        public void SupportRole_TriesRestart_IsRejected()
        {
            var envelope = CreateEnvelope(
                "Restart",
                "{}",
                "sig:valid",
                DateTime.UtcNow.AddMinutes(-1),
                DateTime.UtcNow.AddMinutes(5),
                "Support",
                "user-2",
                CommandRiskLevel.High);

            var executor = new SafeRemoteCommandExecutor(DefaultPresets, false, _ => string.Empty);
            var success = executor.TryExecute(envelope, DateTime.UtcNow, false, false, out var result);

            Assert.IsFalse(success, "Expected Support role attempting restart to be rejected.");
            StringAssert.Contains(result, "Issuer is not authorized for this command risk level.");
        }

        [TestMethod]
        public void Screenshot_Allowed_ForSupport()
        {
            var envelope = CreateEnvelope(
                "Screenshot",
                "{}",
                "sig:valid",
                DateTime.UtcNow.AddMinutes(-1),
                DateTime.UtcNow.AddMinutes(5),
                "Support",
                "user-2",
                CommandRiskLevel.Medium);

            var executor = new SafeRemoteCommandExecutor(DefaultPresets, false, _ => string.Empty);
            var success = executor.TryExecute(envelope, DateTime.UtcNow, false, false, out var result);

            Assert.IsTrue(success, "Expected Screenshot to be allowed for Support.");
            Assert.IsTrue(result.Contains("\"status\": \"ok\""), "Expected successful execution result.");
        }

        [TestMethod]
        public void Password_Blocked_WhenFeatureFlagDisabled()
        {
            var envelope = CreateEnvelope(
                "Password",
                "{ \"PresetName\": \"ChangePassword\" }",
                "sig:valid",
                DateTime.UtcNow.AddMinutes(-1),
                DateTime.UtcNow.AddMinutes(5),
                "Admin",
                "user-1",
                CommandRiskLevel.Critical);

            var executor = new SafeRemoteCommandExecutor(DefaultPresets, passwordCommandsEnabled: false, _ => string.Empty);
            var success = executor.TryExecute(envelope, DateTime.UtcNow, inMaintenanceWindow: true, operatorConfirmed: true, out var result);

            Assert.IsFalse(success, "Expected Password command to be blocked when feature flag is disabled.");
            StringAssert.Contains(result, "Password commands are blocked because the feature flag is disabled.");
        }

        [TestMethod]
        public void CriticalCommand_RequiresMaintenanceWindowAndConfirmation()
        {
            var envelope = CreateEnvelope(
                "ExecuteScript",
                "{ \"PresetName\": \"RestartService\" }",
                "sig:valid",
                DateTime.UtcNow.AddMinutes(-1),
                DateTime.UtcNow.AddMinutes(5),
                "Admin",
                "user-1",
                CommandRiskLevel.Critical);

            var executor = new SafeRemoteCommandExecutor(DefaultPresets, false, _ => string.Empty);

            // Without maintenance window and confirmation
            var success1 = executor.TryExecute(envelope, DateTime.UtcNow, inMaintenanceWindow: false, operatorConfirmed: false, out var _);
            Assert.IsFalse(success1, "Expected Critical command to be rejected without maintenance window and confirmation.");

            // With maintenance window but no confirmation
            var success2 = executor.TryExecute(envelope, DateTime.UtcNow, inMaintenanceWindow: true, operatorConfirmed: false, out var _);
            Assert.IsFalse(success2, "Expected Critical command to be rejected without operator confirmation.");

            // With both
            var success3 = executor.TryExecute(envelope, DateTime.UtcNow, inMaintenanceWindow: true, operatorConfirmed: true, out var result3);
            Assert.IsTrue(success3, "Expected Critical command to succeed with maintenance window and confirmation.");
            Assert.IsTrue(result3.Contains("\"status\": \"ok\""), "Expected successful execution result.");
        }

        [TestMethod]
        public void ArbitraryShell_IsRejected()
        {
            var envelope = CreateEnvelope(
                "ArbitraryShell",
                "whoami",
                "sig:valid",
                DateTime.UtcNow.AddMinutes(-1),
                DateTime.UtcNow.AddMinutes(5),
                "Admin",
                "user-1",
                CommandRiskLevel.High);

            var executor = new SafeRemoteCommandExecutor(DefaultPresets, false, _ => string.Empty);
            var success = executor.TryExecute(envelope, DateTime.UtcNow, false, false, out var result);

            Assert.IsFalse(success, "Expected arbitrary shell to be rejected.");
            StringAssert.Contains(result, "Arbitrary shell commands are not permitted.");
        }

        [TestMethod]
        public void ExecuteScript_NotInAllowlist_IsRejected()
        {
            var envelope = CreateEnvelope(
                "ExecuteScript",
                "{ \"PresetName\": \"UnknownScript\" }",
                "sig:valid",
                DateTime.UtcNow.AddMinutes(-1),
                DateTime.UtcNow.AddMinutes(5),
                "Admin",
                "user-1",
                CommandRiskLevel.Medium);

            var executor = new SafeRemoteCommandExecutor(DefaultPresets, false, _ => string.Empty);
            var success = executor.TryExecute(envelope, DateTime.UtcNow, false, false, out var result);

            Assert.IsFalse(success, "Expected disallowed script preset to be rejected.");
            StringAssert.Contains(result, "is not in the allowlist.");
        }

        [TestMethod]
        public void QueueItem_Recorded_OnFailure()
        {
            var envelope = CreateEnvelope(
                "ArbitraryShell",
                "whoami",
                "sig:valid",
                DateTime.UtcNow.AddMinutes(-1),
                DateTime.UtcNow.AddMinutes(5),
                "Admin",
                "user-1",
                CommandRiskLevel.High);

            var executor = new SafeRemoteCommandExecutor(DefaultPresets, false, _ => string.Empty);
            executor.TryExecute(envelope, DateTime.UtcNow, false, false, out var _);

            Assert.AreEqual(1, executor.CommandQueue.Count, "Expected one queue item to be recorded.");
            Assert.AreEqual(CommandQueueStatus.Failed, executor.CommandQueue[0].Status);
        }

        [TestMethod]
        public void QueueItem_Recorded_OnSuccess()
        {
            var envelope = CreateEnvelope(
                "Screenshot",
                "{}",
                "sig:valid",
                DateTime.UtcNow.AddMinutes(-1),
                DateTime.UtcNow.AddMinutes(5),
                "Support",
                "user-2",
                CommandRiskLevel.Medium);

            var executor = new SafeRemoteCommandExecutor(DefaultPresets, false, _ => string.Empty);
            executor.TryExecute(envelope, DateTime.UtcNow, false, false, out var _);

            Assert.AreEqual(1, executor.CommandQueue.Count, "Expected one queue item to be recorded.");
            Assert.AreEqual(CommandQueueStatus.Completed, executor.CommandQueue[0].Status);
        }
    }
}
