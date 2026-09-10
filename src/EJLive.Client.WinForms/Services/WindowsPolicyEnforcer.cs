using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.ServiceProcess;
using EJLive.Core;
using EJLive.Core.Models;
using EJLive.Core.Services;
using EJLive.Shared;
using AppConfig = EJLive.Shared.AppConfig;
using AppConstants = EJLive.Core.AppConstants;

namespace EJLive.Client.WinForms.Services;

public enum WindowsPolicyProfileMode
{
    Audit = 0,
    Enforce = 1,
    DomainGpoRespect = 2
}

public sealed class WindowsPolicyEnforcementResult
{
    public bool Success { get; init; }
    public bool RequiresAdministrator { get; init; }
    public bool OperationalAutomationReady { get; init; }
    public WindowsPolicyProfileMode ProfileMode { get; init; } = WindowsPolicyProfileMode.Enforce;
    public string WhyFailed { get; init; } = string.Empty;
    public IReadOnlyList<PolicyConflictDiagnostic> PolicyFailures { get; init; } = Array.Empty<PolicyConflictDiagnostic>();
    public IReadOnlyList<PolicyFailureDetail> WhyFailedDetails { get; init; } = Array.Empty<PolicyFailureDetail>();
    public string Message { get; init; } = string.Empty;
    public RemoteDesktopReadinessReport? Readiness { get; init; }
    public EndpointConfigurationReport? EndpointConfiguration { get; init; }
}

public sealed class PolicyFailureDetail
{
    public string Key { get; init; } = string.Empty;
    public string FailureCode { get; init; } = string.Empty;
    public string Reason { get; init; } = string.Empty;
    public string Source { get; init; } = string.Empty;
    public string Expected { get; init; } = string.Empty;
    public string Actual { get; init; } = string.Empty;
    public string SuggestedAction { get; init; } = string.Empty;
}

/// <summary>
/// Operational policy enforcer that applies and audits Windows remote-management baseline.
/// Designed for SYSTEM/Admin runtime and periodic self-healing loops.
/// </summary>
public sealed class WindowsPolicyEnforcer
{
    private const string LoggerSource = "WindowsPolicyEnforcer";
    private readonly Func<AppConfig> _configAccessor;

    public WindowsPolicyEnforcer(Func<AppConfig>? configAccessor = null)
    {
        _configAccessor = configAccessor ?? (() =>
        {
            var cfg = AppConfig.Load();
            cfg.ApplyDefaults();
            return cfg;
        });
    }

    public WindowsPolicyEnforcementResult EnforceBaseline() => EnforceBaseline(null);

    public WindowsPolicyEnforcementResult EnforceBaseline(WindowsPolicyProfileMode? profileMode)
    {
        try
        {
            var config = _configAccessor();
            config.ApplyDefaults();
            var effectiveMode = profileMode ?? ResolveProfileMode(config.WindowsPolicyProfileMode);

            if (effectiveMode == WindowsPolicyProfileMode.Audit)
                return RunAuditOnlyBaseline(effectiveMode);

            var runtimeBaseline = EnforceOperationalRuntimeBaseline(config);
            var termServiceGate = EnsureTermServiceRunning();
            var preReadiness = WindowsRemoteAccessService.EvaluateRemoteDesktopReadiness();
            var respectDomainGpo = effectiveMode == WindowsPolicyProfileMode.DomainGpoRespect;
            var skipLocalPolicyWrites = respectDomainGpo &&
                                        (preReadiness.DomainJoinedLikely || preReadiness.DomainPolicyOverrideLikely);

            var baseline = skipLocalPolicyWrites
                ? new RemoteAccessResult
                {
                    Success = true,
                    Message =
                        "Domain-GPO-Respect: local RDP policy writes skipped (domain/GPO override likely); endpoint scope and diagnostics still applied."
                }
                : WindowsRemoteAccessService.EnsureRemoteAdministrationBaseline(
                    config.AllowedPasswordAccounts,
                    enforceNla: true,
                    enableLocalAdminTokenPolicy: true,
                    enableWinRm: config.EnableWinRmBootstrap,
                    enableRemoteRegistry: config.EnableRemoteRegistryBootstrap);

            var endpointConfig = WindowsRemoteAccessService.ApplyScopedEndpointConfiguration(
                config.ServerIP,
                config.ServerPort,
                config.EnforceScopedFirewallRule,
                config.ScopedFirewallPort,
                config.ScopedFirewallRemoteAddresses,
                config.ConfigureDefenderExclusions,
                config.DefenderExclusionPaths,
                config.HelpdeskAdGroup);

            var readiness = WindowsRemoteAccessService.EvaluateRemoteDesktopReadiness();
            var policyFailures = BuildPolicyFailures(readiness);
            var whyFailedDetails = BuildWhyFailedDetails(effectiveMode, readiness, policyFailures, skipLocalPolicyWrites);
            var whyFailed = BuildWhyFailed(whyFailedDetails);
            var success = runtimeBaseline.Ready &&
                          termServiceGate.Success &&
                          baseline.Success &&
                          endpointConfig.Success &&
                          readiness.RemoteExecutionReady;
            var message = string.Join("; ", new[]
            {
                runtimeBaseline.Message,
                termServiceGate.Message,
                baseline.Message,
                endpointConfig.Summary,
                readiness.ToSummary(),
                whyFailed
            }.Where(value => !string.IsNullOrWhiteSpace(value)));

            return new WindowsPolicyEnforcementResult
            {
                Success = success,
                OperationalAutomationReady = runtimeBaseline.Ready,
                RequiresAdministrator = baseline.RequiresAdministrator ||
                                        endpointConfig.RequiresAdministrator ||
                                        termServiceGate.RequiresAdministrator ||
                                        !readiness.IsAdministrator,
                ProfileMode = effectiveMode,
                WhyFailed = whyFailed,
                PolicyFailures = policyFailures,
                WhyFailedDetails = whyFailedDetails,
                Message = message,
                Readiness = readiness,
                EndpointConfiguration = endpointConfig
            };
        }
        catch (UnauthorizedAccessException ex)
        {
            LogUnauthorizedAccess("EnforceBaseline", ex);
            return BuildUnauthorizedResult(ex, profileMode ?? WindowsPolicyProfileMode.Enforce);
        }
        catch (Exception ex)
        {
            AppLogger.Instance.Error($"Baseline execution failed: {ex.Message}", LoggerSource);
            return new WindowsPolicyEnforcementResult
            {
                Success = false,
                RequiresAdministrator = false,
                OperationalAutomationReady = false,
                ProfileMode = profileMode ?? WindowsPolicyProfileMode.Enforce,
                WhyFailed = "Baseline execution exception: " + ex.Message,
                WhyFailedDetails = new[]
                {
                    new PolicyFailureDetail
                    {
                        Key = "PolicyEnforcer.Exception",
                        FailureCode = "BASELINE_EXCEPTION",
                        Reason = ex.Message,
                        Source = LoggerSource,
                        Expected = "Baseline execution completes without exception.",
                        Actual = ex.GetType().Name,
                        SuggestedAction = "Inspect stack trace and fix baseline execution path."
                    }
                },
                Message = "Baseline execution failed: " + ex.Message
            };
        }
    }

    /// <summary>
    /// Compatibility alias for legacy policy-enforcer integrations.
    /// Applies the same hardened baseline without replacing current architecture.
    /// </summary>
    public WindowsPolicyEnforcementResult ApplyForcedConfiguration() => EnforceBaseline();

    /// <summary>
    /// Compatibility alias with explicit profile override.
    /// </summary>
    public WindowsPolicyEnforcementResult ApplyForcedConfiguration(WindowsPolicyProfileMode profileMode) =>
        EnforceBaseline(profileMode);

    public WindowsPolicyEnforcementResult ProbeReadiness()
    {
        try
        {
            var readiness = WindowsRemoteAccessService.EvaluateRemoteDesktopReadiness();
            var policyFailures = BuildPolicyFailures(readiness);
            var whyFailedDetails = BuildWhyFailedDetails(
                WindowsPolicyProfileMode.Audit,
                readiness,
                policyFailures,
                skippedByDomainGpoRespect: false);
            var whyFailed = BuildWhyFailed(whyFailedDetails);
            return new WindowsPolicyEnforcementResult
            {
                Success = readiness.RemoteExecutionReady,
                RequiresAdministrator = !readiness.IsAdministrator,
                OperationalAutomationReady = true,
                ProfileMode = WindowsPolicyProfileMode.Audit,
                WhyFailed = whyFailed,
                PolicyFailures = policyFailures,
                WhyFailedDetails = whyFailedDetails,
                Message = string.Join("; ", new[] { readiness.ToSummary(), whyFailed }
                    .Where(value => !string.IsNullOrWhiteSpace(value))),
                Readiness = readiness,
                EndpointConfiguration = null
            };
        }
        catch (UnauthorizedAccessException ex)
        {
            LogUnauthorizedAccess("ProbeReadiness", ex);
            return BuildUnauthorizedResult(ex, WindowsPolicyProfileMode.Audit);
        }
    }

    /// <summary>
    /// Compatibility probe used by legacy integrations.
    /// Returns true when running as LocalSystem or Administrator.
    /// </summary>
    public bool IsSystemElevated()
    {
        try
        {
            using var identity = WindowsIdentity.GetCurrent();
            if (identity?.User?.Value == "S-1-5-18")
                return true;
            var principal = new WindowsPrincipal(identity ?? throw new InvalidOperationException("Windows identity unavailable."));
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
        catch
        {
            return false;
        }
    }

    private static OperationalRuntimeBaselineResult EnforceOperationalRuntimeBaseline(AppConfig config)
    {
        var changed = false;
        var notes = new List<string>();

        if (!config.AutoConnect)
        {
            config.AutoConnect = true;
            changed = true;
            notes.Add("AutoConnect=true");
        }

        if (!config.AutoBackup)
        {
            config.AutoBackup = true;
            changed = true;
            notes.Add("AutoBackup=true");
        }

        if (!config.AutoEnableRemoteAccess)
        {
            config.AutoEnableRemoteAccess = true;
            changed = true;
            notes.Add("AutoEnableRemoteAccess=true");
        }

        if (!config.AutoPrepareWindowsRuntime)
        {
            config.AutoPrepareWindowsRuntime = true;
            changed = true;
            notes.Add("AutoPrepareWindowsRuntime=true");
        }

        if (!config.AllowLocalWindowsPasswordChange)
        {
            config.AllowLocalWindowsPasswordChange = true;
            changed = true;
            notes.Add("AllowLocalWindowsPasswordChange=true");
        }

        if (!config.RequireEncryptedWindowsPasswordPayload)
        {
            config.RequireEncryptedWindowsPasswordPayload = true;
            changed = true;
            notes.Add("RequireEncryptedWindowsPasswordPayload=true");
        }

        var normalizedHeartbeat = Math.Clamp(config.HeartbeatIntervalSec, 5, 60);
        if (config.HeartbeatIntervalSec != normalizedHeartbeat)
        {
            config.HeartbeatIntervalSec = normalizedHeartbeat;
            changed = true;
            notes.Add("HeartbeatIntervalSec normalized");
        }

        var normalizedReconnect = Math.Clamp(config.ReconnectIntervalSec, 5, 120);
        if (config.ReconnectIntervalSec != normalizedReconnect)
        {
            config.ReconnectIntervalSec = normalizedReconnect;
            changed = true;
            notes.Add("ReconnectIntervalSec normalized");
        }

        EnsureDirectory(AppConstants.DefaultClientOutboxPath, notes);
        EnsureDirectory(AppConstants.DefaultClientInboxPath, notes);
        EnsureDirectory(AppConstants.DefaultLogPath, notes);
        EnsureDirectory(AppConstants.DefaultImagesPath, notes);
        EnsureDirectory(config.SourcePath, notes);
        EnsureDirectory(config.BackupPath, notes);
        EnsureDirectory(config.ImageInboxPath, notes);

        if (changed)
        {
            config.Save();
            AgentConfigurationXmlService.SaveAppConfig(config);
        }

        var message = notes.Count == 0
            ? "Operational baseline already aligned."
            : "Operational baseline aligned: " + string.Join(", ", notes);
        return new OperationalRuntimeBaselineResult
        {
            Ready = true,
            Message = message
        };
    }

    private static void EnsureDirectory(string path, ICollection<string> notes)
    {
        if (string.IsNullOrWhiteSpace(path))
            return;

        try
        {
            var full = Path.GetFullPath(path.Trim());
            if (Directory.Exists(full))
                return;

            Directory.CreateDirectory(full);
            notes.Add("mkdir:" + full);
        }
        catch
        {
            // Best-effort directory hydration.
        }
    }

    private sealed class OperationalRuntimeBaselineResult
    {
        public bool Ready { get; init; }
        public string Message { get; init; } = string.Empty;
    }

    private WindowsPolicyEnforcementResult RunAuditOnlyBaseline(WindowsPolicyProfileMode mode)
    {
        var readiness = WindowsRemoteAccessService.EvaluateRemoteDesktopReadiness();
        var policyFailures = BuildPolicyFailures(readiness);
        var whyFailedDetails = BuildWhyFailedDetails(mode, readiness, policyFailures, skippedByDomainGpoRespect: false);
        var whyFailed = BuildWhyFailed(whyFailedDetails);
        var summary = string.Join("; ", new[]
        {
            "PolicyProfileMode=Audit (no local policy mutation).",
            readiness.ToSummary(),
            whyFailed
        }.Where(value => !string.IsNullOrWhiteSpace(value)));

        return new WindowsPolicyEnforcementResult
        {
            Success = readiness.RemoteExecutionReady,
            RequiresAdministrator = !readiness.IsAdministrator,
            OperationalAutomationReady = true,
            ProfileMode = mode,
            WhyFailed = whyFailed,
            PolicyFailures = policyFailures,
            WhyFailedDetails = whyFailedDetails,
            Message = summary,
            Readiness = readiness,
            EndpointConfiguration = null
        };
    }

    private static WindowsPolicyProfileMode ResolveProfileMode(string? configuredMode)
    {
        if (TryParseProfileMode(configuredMode, out var parsed))
            return parsed;

        return WindowsPolicyProfileMode.Enforce;
    }

    private static bool TryParseProfileMode(string? raw, out WindowsPolicyProfileMode mode)
    {
        mode = WindowsPolicyProfileMode.Enforce;
        if (string.IsNullOrWhiteSpace(raw))
            return false;

        var normalized = raw.Trim()
            .Replace("_", string.Empty, StringComparison.Ordinal)
            .Replace("-", string.Empty, StringComparison.Ordinal)
            .Replace(" ", string.Empty, StringComparison.Ordinal);

        if (normalized.Equals("audit", StringComparison.OrdinalIgnoreCase))
        {
            mode = WindowsPolicyProfileMode.Audit;
            return true;
        }

        if (normalized.Equals("enforce", StringComparison.OrdinalIgnoreCase))
        {
            mode = WindowsPolicyProfileMode.Enforce;
            return true;
        }

        if (normalized.Equals("domaingporespect", StringComparison.OrdinalIgnoreCase) ||
            normalized.Equals("domainpolicyrespect", StringComparison.OrdinalIgnoreCase))
        {
            mode = WindowsPolicyProfileMode.DomainGpoRespect;
            return true;
        }

        return false;
    }

    private static IReadOnlyList<PolicyConflictDiagnostic> BuildPolicyFailures(RemoteDesktopReadinessReport readiness)
    {
        if (readiness.PolicyConflictDetails.Count == 0)
            return Array.Empty<PolicyConflictDiagnostic>();

        return readiness.PolicyConflictDetails
            .Where(diagnostic => !string.IsNullOrWhiteSpace(diagnostic.Key))
            .Select(diagnostic => new PolicyConflictDiagnostic
            {
                Key = diagnostic.Key,
                Expected = diagnostic.Expected,
                Actual = diagnostic.Actual,
                Source = diagnostic.Source,
                Impact = diagnostic.Impact
            })
            .ToArray();
    }

    private static IReadOnlyList<PolicyFailureDetail> BuildWhyFailedDetails(
        WindowsPolicyProfileMode mode,
        RemoteDesktopReadinessReport readiness,
        IReadOnlyList<PolicyConflictDiagnostic> failures,
        bool skippedByDomainGpoRespect)
    {
        var details = new List<PolicyFailureDetail>();

        if (skippedByDomainGpoRespect)
        {
            details.Add(new PolicyFailureDetail
            {
                Key = "PolicyProfileMode",
                FailureCode = "DOMAIN_GPO_RESPECT_SKIP",
                Reason = "Domain-GPO-Respect mode skipped local policy mutation because domain/GPO override is likely.",
                Source = readiness.DomainJoinedLikely ? "Domain GPO" : "Local policy",
                Expected = "Local mutation allowed",
                Actual = "Local mutation skipped",
                SuggestedAction = "Apply expected values in AD/GPO and keep endpoint in compliance baseline."
            });
        }

        foreach (var failure in failures)
            details.Add(BuildPolicyFailureDetail(failure, readiness.DomainJoinedLikely));

        if (!readiness.IsAdministrator)
        {
            details.Add(new PolicyFailureDetail
            {
                Key = "Privilege:IsAdministrator",
                FailureCode = "PRIVILEGE_MISSING",
                Reason = "Process is not elevated as Administrator/LocalSystem.",
                Source = "Process token",
                Expected = "Elevated token",
                Actual = "Non-elevated token",
                SuggestedAction = "Run client service under LocalSystem or elevated Administrator."
            });
        }

        if (!readiness.TermServiceRunning)
        {
            details.Add(new PolicyFailureDetail
            {
                Key = "Service:TermService",
                FailureCode = "SERVICE_NOT_RUNNING",
                Reason = "Remote Desktop Services (TermService) is not running.",
                Source = "ServiceController",
                Expected = "Running",
                Actual = "Stopped/Unavailable",
                SuggestedAction = "Ensure TermService startup type and state are healthy."
            });
        }

        if (!readiness.RemoteDesktopEnabled)
        {
            details.Add(new PolicyFailureDetail
            {
                Key = @"HKLM\SYSTEM\CurrentControlSet\Control\Terminal Server\fDenyTSConnections",
                FailureCode = "RDP_RUNTIME_DISABLED",
                Reason = "Remote Desktop runtime flag is disabled.",
                Source = "Local runtime key",
                Expected = "0",
                Actual = "1 or missing",
                SuggestedAction = "Enable RDP runtime flag and reconcile with policy hives."
            });
        }

        if (!readiness.Port3389Listening)
        {
            details.Add(new PolicyFailureDetail
            {
                Key = "Network:TCP3389",
                FailureCode = "RDP_PORT_NOT_LISTENING",
                Reason = "RDP listener is not available on TCP 3389.",
                Source = "Net.TcpListener probe",
                Expected = "Listening",
                Actual = "Not listening",
                SuggestedAction = "Review TermService, firewall scope, and policy overrides."
            });
        }

        if (mode == WindowsPolicyProfileMode.Audit)
        {
            details.Add(new PolicyFailureDetail
            {
                Key = "PolicyProfileMode",
                FailureCode = "AUDIT_ONLY",
                Reason = "Audit profile does not mutate local policy keys by design.",
                Source = "WindowsPolicyEnforcer",
                Expected = "Enforcement may update keys",
                Actual = "Diagnostics-only execution",
                SuggestedAction = "Use Enforce or Domain-GPO-Respect to apply runtime mutation."
            });
        }

        return details
            .GroupBy(detail => detail.Key, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .ToArray();
    }

    private static PolicyFailureDetail BuildPolicyFailureDetail(
        PolicyConflictDiagnostic failure,
        bool domainJoinedLikely)
    {
        var code = ResolveFailureCode(failure.Key, failure.Impact);
        var source = string.IsNullOrWhiteSpace(failure.Source) ? "unknown-source" : failure.Source;
        var expected = string.IsNullOrWhiteSpace(failure.Expected) ? "n/a" : failure.Expected;
        var actual = string.IsNullOrWhiteSpace(failure.Actual) ? "n/a" : failure.Actual;
        var impact = string.IsNullOrWhiteSpace(failure.Impact) ? "impact-unknown" : failure.Impact;

        return new PolicyFailureDetail
        {
            Key = failure.Key ?? string.Empty,
            FailureCode = code,
            Reason = $"{impact} (expected={expected}, actual={actual}, source={source})",
            Source = source,
            Expected = expected,
            Actual = actual,
            SuggestedAction = ResolveSuggestedAction(code, domainJoinedLikely)
        };
    }

    private static string BuildWhyFailed(IReadOnlyList<PolicyFailureDetail> details)
    {
        if (details.Count == 0)
            return "Remote execution baseline is ready.";

        return string.Join(
            " ",
            details.Take(8).Select(detail =>
                $"why-failed[{detail.Key}] code={detail.FailureCode}; {detail.Reason}"));
    }

    private static string ResolveFailureCode(string key, string impact)
    {
        var normalized = (key ?? string.Empty) + "|" + (impact ?? string.Empty);
        if (normalized.IndexOf("SeRemoteInteractiveLogonRight", StringComparison.OrdinalIgnoreCase) >= 0)
            return "MISSING_REMOTE_LOGON_RIGHT";
        if (normalized.IndexOf("SeDenyRemoteInteractiveLogonRight", StringComparison.OrdinalIgnoreCase) >= 0)
            return "DENY_REMOTE_LOGON_RIGHT_PRESENT";
        if (normalized.IndexOf("fDenyTSConnections", StringComparison.OrdinalIgnoreCase) >= 0)
            return "RDP_BLOCKED_BY_POLICY";
        if (normalized.IndexOf("UserAuthentication", StringComparison.OrdinalIgnoreCase) >= 0)
            return "NLA_POLICY_MISMATCH";
        if (normalized.IndexOf("fAllowUnsolicited", StringComparison.OrdinalIgnoreCase) >= 0)
            return "SHADOW_UNSOLICITED_DISABLED";
        if (normalized.IndexOf("Shadow", StringComparison.OrdinalIgnoreCase) >= 0)
            return "SHADOW_POLICY_MISMATCH";
        if (normalized.IndexOf("AllowRemoteRPC", StringComparison.OrdinalIgnoreCase) >= 0)
            return "ALLOW_REMOTE_RPC_DISABLED";
        if (normalized.IndexOf("DisableRestrictedAdmin", StringComparison.OrdinalIgnoreCase) >= 0)
            return "RESTRICTED_ADMIN_BLOCKED";
        if (normalized.IndexOf("LAPS", StringComparison.OrdinalIgnoreCase) >= 0)
            return "LAPS_POLICY_MISSING";
        if (normalized.IndexOf("Session0Isolation", StringComparison.OrdinalIgnoreCase) >= 0)
            return "SESSION0_ISOLATION";
        return "POLICY_CONFLICT";
    }

    private static string ResolveSuggestedAction(string failureCode, bool domainJoinedLikely)
    {
        return failureCode switch
        {
            "MISSING_REMOTE_LOGON_RIGHT" =>
                "Grant SeRemoteInteractiveLogonRight to Administrators or Remote Desktop Users.",
            "DENY_REMOTE_LOGON_RIGHT_PRESENT" =>
                "Remove Administrators/Remote Desktop Users from SeDenyRemoteInteractiveLogonRight.",
            "RDP_BLOCKED_BY_POLICY" =>
                domainJoinedLikely
                    ? "Update the domain GPO for Terminal Services RDP allow policy."
                    : "Apply local policy to allow RDP and keep runtime key aligned.",
            "NLA_POLICY_MISMATCH" =>
                "Align TS_USER_AUTHENTICATION_POLICY with NLA requirement.",
            "SHADOW_POLICY_MISMATCH" =>
                "Set Shadow policy to expected mode for approved support workflow.",
            "SHADOW_UNSOLICITED_DISABLED" =>
                "Enable unsolicited shadow policy when your approved governance allows it.",
            "ALLOW_REMOTE_RPC_DISABLED" =>
                "Enable AllowRemoteRPC runtime key to support shadow attach flow.",
            "RESTRICTED_ADMIN_BLOCKED" =>
                "Align Restricted Admin policy with remote support command profile.",
            "LAPS_POLICY_MISSING" =>
                "Publish Windows LAPS policy via AD/GPO or local hardening baseline.",
            "SESSION0_ISOLATION" =>
                "Use a user-session companion for interactive operations from service context.",
            _ =>
                "Inspect policy source and align expected/actual values."
        };
    }

    private static WindowsPolicyEnforcementResult BuildUnauthorizedResult(
        UnauthorizedAccessException ex,
        WindowsPolicyProfileMode mode)
    {
        return new WindowsPolicyEnforcementResult
        {
            Success = false,
            RequiresAdministrator = true,
            OperationalAutomationReady = false,
            ProfileMode = mode,
            WhyFailed = "Administrative access denied: " + ex.Message,
            WhyFailedDetails = new[]
            {
                new PolicyFailureDetail
                {
                    Key = "Privilege:IsAdministrator",
                    FailureCode = "ADMIN_ACCESS_DENIED",
                    Reason = ex.Message,
                    Source = LoggerSource,
                    Expected = "Elevated Administrator/LocalSystem access",
                    Actual = "UnauthorizedAccessException",
                    SuggestedAction = "Run under LocalSystem/Administrator and re-apply baseline."
                }
            },
            Message = "Administrative access denied: " + ex.Message
        };
    }

    private static void LogUnauthorizedAccess(string operation, UnauthorizedAccessException ex)
    {
        AppLogger.Instance.Warning(
            $"{operation} unauthorized: {ex.Message}",
            LoggerSource);
    }

    private static ServiceGateResult EnsureTermServiceRunning()
    {
        try
        {
            using var controller = new ServiceController("TermService");
            if (controller.Status == ServiceControllerStatus.Running)
            {
                return new ServiceGateResult
                {
                    Success = true,
                    Message = "TermService already running (ServiceController)."
                };
            }

            controller.Start();
            controller.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(20));
            return new ServiceGateResult
            {
                Success = controller.Status == ServiceControllerStatus.Running,
                Message = controller.Status == ServiceControllerStatus.Running
                    ? "TermService started successfully (ServiceController)."
                    : "TermService did not reach Running state in expected time."
            };
        }
        catch (UnauthorizedAccessException ex)
        {
            LogUnauthorizedAccess("EnsureTermServiceRunning", ex);
            return new ServiceGateResult
            {
                Success = false,
                RequiresAdministrator = true,
                Message = "TermService start denied: " + ex.Message
            };
        }
        catch (Exception ex)
        {
            AppLogger.Instance.Warning("EnsureTermServiceRunning failed: " + ex.Message, LoggerSource);
            return new ServiceGateResult
            {
                Success = false,
                Message = "TermService check failed: " + ex.Message
            };
        }
    }

    private sealed class ServiceGateResult
    {
        public bool Success { get; init; }
        public bool RequiresAdministrator { get; init; }
        public string Message { get; init; } = string.Empty;
    }
}
