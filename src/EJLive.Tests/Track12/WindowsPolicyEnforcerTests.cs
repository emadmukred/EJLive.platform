using System.Collections.Generic;
using EJLive.Core.Engine;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EJLive.Tests.Track12
{
    /// <summary>
    /// MSTest suite for SafeWindowsPolicyEnforcer.
    /// </summary>
    [TestClass]
    public class WindowsPolicyEnforcerTests
    {
        [TestMethod]
        public void NonAdmin_IsRejected()
        {
            var config = new SafeWindowsPolicyConfig
            {
                Mode = PolicyEnforcementMode.Audit,
                ExplicitEnforceEnabled = false
            };

            var enforcer = new SafeWindowsPolicyEnforcer(
                config,
                isElevated: () => false,
                isDomainJoined: () => false,
                hasGpoOverride: _ => false);

            var success = enforcer.TryApplyRdpPolicy(
                "RDP",
                beforeJson: "{}",
                afterJson: "{}",
                rollbackJson: "{}",
                operatorId: "admin-1",
                reason: "Test",
                out var error);

            Assert.IsFalse(success, "Expected non-admin to be rejected.");
            StringAssert.Contains(error, "Administrator elevation is required");
        }

        [TestMethod]
        public void DomainJoined_WithGpoOverride_SkipsLocalPolicy()
        {
            var config = new SafeWindowsPolicyConfig
            {
                Mode = PolicyEnforcementMode.Audit,
                DomainGpoRespect = true
            };

            var enforcer = new SafeWindowsPolicyEnforcer(
                config,
                isElevated: () => true,
                isDomainJoined: () => true,
                hasGpoOverride: pt => pt == "RDP");

            var success = enforcer.TryApplyRdpPolicy(
                "RDP",
                beforeJson: "{}",
                afterJson: "{}",
                rollbackJson: "{}",
                operatorId: "admin-1",
                reason: "Test",
                out var error);

            Assert.IsFalse(success, "Expected GPO override to block local policy write.");
            StringAssert.Contains(error, "Domain GPO override detected");
        }

        [TestMethod]
        public void InvalidAccount_PasswordChangeRejected()
        {
            var config = new SafeWindowsPolicyConfig
            {
                Mode = PolicyEnforcementMode.Audit,
                AllowedPasswordAccounts = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase) { "AllowedUser" }
            };

            var enforcer = new SafeWindowsPolicyEnforcer(
                config,
                isElevated: () => true,
                isDomainJoined: () => false,
                hasGpoOverride: _ => false);

            var success = enforcer.TryApplyPasswordPolicy(
                accountName: "BadActor",
                encryptedPayload: "enc:payload",
                beforeJson: "{}",
                afterJson: "{}",
                rollbackJson: "{}",
                operatorId: "admin-1",
                reason: "Test",
                out var error);

            Assert.IsFalse(success, "Expected invalid account password change to be rejected.");
            StringAssert.Contains(error, "is not in the allowed password accounts list");
        }

        [TestMethod]
        public void WinRm_Disabled_IsBlocked()
        {
            var config = new SafeWindowsPolicyConfig
            {
                Mode = PolicyEnforcementMode.Audit
            };

            var enforcer = new SafeWindowsPolicyEnforcer(
                config,
                isElevated: () => true,
                isDomainJoined: () => false,
                hasGpoOverride: _ => false);

            var success = enforcer.TryApplyWinRmPolicy(
                beforeJson: "{ \"Enabled\": true }",
                afterJson: "{ \"Enabled\": false }",
                rollbackJson: "{}",
                operatorId: "admin-1",
                reason: "Test",
                out var error);

            Assert.IsFalse(success, "Expected disabling WinRM to be blocked.");
            StringAssert.Contains(error, "Disabling WinRM via policy enforcer is not permitted");
        }

        [TestMethod]
        public void FirewallScopeMissing_IsRejected()
        {
            var config = new SafeWindowsPolicyConfig
            {
                Mode = PolicyEnforcementMode.Audit,
                ServerIp = null
            };

            var enforcer = new SafeWindowsPolicyEnforcer(
                config,
                isElevated: () => true,
                isDomainJoined: () => false,
                hasGpoOverride: _ => false);

            var success = enforcer.TryApplyFirewallPolicy(
                beforeJson: "{}",
                afterJson: "{ \"Enabled\": true }",
                rollbackJson: "{}",
                operatorId: "admin-1",
                reason: "Test",
                out var error);

            Assert.IsFalse(success, "Expected missing firewall scope to be rejected.");
            StringAssert.Contains(error, "Firewall scope is missing");
        }

        [TestMethod]
        public void Firewall_NeverDisable_IsRejected()
        {
            var config = new SafeWindowsPolicyConfig
            {
                Mode = PolicyEnforcementMode.Audit,
                ServerIp = "192.168.1.10"
            };

            var enforcer = new SafeWindowsPolicyEnforcer(
                config,
                isElevated: () => true,
                isDomainJoined: () => false,
                hasGpoOverride: _ => false);

            var success = enforcer.TryApplyFirewallPolicy(
                beforeJson: "{ \"Enabled\": true }",
                afterJson: "{ \"Enabled\": false }",
                rollbackJson: "{}",
                operatorId: "admin-1",
                reason: "Test",
                out var error);

            Assert.IsFalse(success, "Expected disabling firewall to be rejected.");
            StringAssert.Contains(error, "Disabling Defender or Firewall is not permitted");
        }

        [TestMethod]
        public void AuditMode_CapturesSnapshot_NoSystemChange()
        {
            var config = new SafeWindowsPolicyConfig
            {
                Mode = PolicyEnforcementMode.Audit
            };

            var enforcer = new SafeWindowsPolicyEnforcer(
                config,
                isElevated: () => true,
                isDomainJoined: () => false,
                hasGpoOverride: _ => false);

            var success = enforcer.TryApplyRdpPolicy(
                "RDP",
                beforeJson: "{ \"Port\": 3389 }",
                afterJson: "{ \"Port\": 3390 }",
                rollbackJson: "{ \"Port\": 3389 }",
                operatorId: "admin-1",
                reason: "Change RDP port",
                out var error);

            Assert.IsTrue(success, "Expected audit mode to succeed.");
            StringAssert.Contains(error, "audited only");
            Assert.AreEqual(1, enforcer.Snapshots.Count, "Expected one snapshot to be captured.");
            Assert.AreEqual("RDP", enforcer.Snapshots[0].PolicyType);
            Assert.AreEqual("admin-1", enforcer.Snapshots[0].OperatorId);
        }

        [TestMethod]
        public void EnforceMode_RequiresExplicitEnablement()
        {
            var config = new SafeWindowsPolicyConfig
            {
                Mode = PolicyEnforcementMode.Enforce,
                ExplicitEnforceEnabled = false
            };

            var enforcer = new SafeWindowsPolicyEnforcer(
                config,
                isElevated: () => true,
                isDomainJoined: () => false,
                hasGpoOverride: _ => false);

            var success = enforcer.TryApplyRdpPolicy(
                "RDP",
                beforeJson: "{}",
                afterJson: "{}",
                rollbackJson: "{}",
                operatorId: "admin-1",
                reason: "Test",
                out var error);

            Assert.IsFalse(success, "Expected Enforce mode without explicit enablement to be rejected.");
            StringAssert.Contains(error, "Enforce mode is not explicitly enabled");
        }

        [TestMethod]
        public void Password_AllowedAccount_WithEncryptedPayload_Succeeds()
        {
            var config = new SafeWindowsPolicyConfig
            {
                Mode = PolicyEnforcementMode.Audit,
                AllowedPasswordAccounts = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase) { "ServiceAccount" }
            };

            var enforcer = new SafeWindowsPolicyEnforcer(
                config,
                isElevated: () => true,
                isDomainJoined: () => false,
                hasGpoOverride: _ => false);

            var success = enforcer.TryApplyPasswordPolicy(
                accountName: "ServiceAccount",
                encryptedPayload: "enc:securepayload",
                beforeJson: "{}",
                afterJson: "{}",
                rollbackJson: "{}",
                operatorId: "admin-1",
                reason: "Rotate password",
                out var error);

            Assert.IsTrue(success, "Expected allowed account with encrypted payload to succeed.");
            Assert.AreEqual(1, enforcer.Snapshots.Count);
        }

        [TestMethod]
        public void Password_MissingEncryptedPayload_IsRejected()
        {
            var config = new SafeWindowsPolicyConfig
            {
                Mode = PolicyEnforcementMode.Audit,
                AllowedPasswordAccounts = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase) { "ServiceAccount" }
            };

            var enforcer = new SafeWindowsPolicyEnforcer(
                config,
                isElevated: () => true,
                isDomainJoined: () => false,
                hasGpoOverride: _ => false);

            var success = enforcer.TryApplyPasswordPolicy(
                accountName: "ServiceAccount",
                encryptedPayload: "   ",
                beforeJson: "{}",
                afterJson: "{}",
                rollbackJson: "{}",
                operatorId: "admin-1",
                reason: "Rotate password",
                out var error);

            Assert.IsFalse(success, "Expected missing encrypted payload to be rejected.");
            StringAssert.Contains(error, "Password payload must be encrypted");
        }
    }
}
