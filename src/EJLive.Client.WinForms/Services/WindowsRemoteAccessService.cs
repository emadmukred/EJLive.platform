using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using EJLive.Core;
using Microsoft.Win32;

namespace EJLive.Client.WinForms.Services
{
    public sealed class RemoteAccessResult
    {
        public bool Success { get; set; }
        public bool RequiresAdministrator { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public sealed class PasswordChangeResult
    {
        public bool Success { get; set; }
        public bool RequiresAdministrator { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public sealed class EndpointConfigurationReport
    {
        public bool Success { get; set; }
        public bool RequiresAdministrator { get; set; }
        public bool FirewallScoped { get; set; }
        public bool DefenderScopedExclusionsApplied { get; set; }
        public bool DomainGpoRecommendationRequired { get; set; }
        public string Summary { get; set; } = string.Empty;
        public List<string> Details { get; set; } = new List<string>();
    }

    public sealed class RemoteSessionDescriptor
    {
        public string UserName { get; set; } = string.Empty;
        public string SessionName { get; set; } = string.Empty;
        public int SessionId { get; set; }
        public string State { get; set; } = string.Empty;
        public bool IsActive => State.Equals("Active", StringComparison.OrdinalIgnoreCase);
        public bool IsDisconnected =>
            State.Equals("Disc", StringComparison.OrdinalIgnoreCase) ||
            State.Equals("Disconnected", StringComparison.OrdinalIgnoreCase);
    }

    public sealed class RemoteSessionExecutionPlan
    {
        public bool Ready { get; set; }
        public bool HasActiveSession { get; set; }
        public bool RequiresSessionCompanion { get; set; }
        public bool NoConsentAllowed { get; set; }
        public string Mode { get; set; } = "Blocked";
        public string Target { get; set; } = string.Empty;
        public int SessionId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Command { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;

        public string ToWireFormat()
        {
            return string.Join("|", new[]
            {
                "mode=" + (Mode ?? string.Empty),
                "target=" + (Target ?? string.Empty),
                "ready=" + Ready,
                "activeSession=" + HasActiveSession,
                "sessionId=" + SessionId,
                "user=" + (UserName ?? string.Empty),
                "noConsent=" + NoConsentAllowed,
                "needsCompanion=" + RequiresSessionCompanion,
                "reason=" + (Reason ?? string.Empty),
                "cmd=" + (Command ?? string.Empty)
            });
        }
    }

    public sealed class PolicyConflictDiagnostic
    {
        public string Key { get; set; } = string.Empty;
        public string Expected { get; set; } = string.Empty;
        public string Actual { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public string Impact { get; set; } = string.Empty;

        public string ToWireFormat()
        {
            return "PolicyConflict|Key=" + Key +
                   "|Expected=" + Expected +
                   "|Actual=" + Actual +
                   "|Source=" + Source +
                   "|Impact=" + Impact;
        }
    }

    public sealed class RemoteDesktopReadinessReport
    {
        public bool IsAdministrator { get; set; }
        public bool RunningAsLocalSystem { get; set; }
        public bool RemoteDesktopEnabled { get; set; }
        public bool RdpPolicyEnabled { get; set; }
        public bool NlaEnabled { get; set; }
        public bool TsUserAuthenticationPolicyEnabled { get; set; }
        public bool SingleSessionPolicyEnabled { get; set; }
        public bool TermServiceRunning { get; set; }
        public bool UmRdpServiceRunning { get; set; }
        public bool Port3389Listening { get; set; }
        public bool LocalAdminTokenPolicyEnabled { get; set; }
        public bool RemoteRegistryRunning { get; set; }
        public bool WinRmServiceRunning { get; set; }
        public bool WinRmPort5985Listening { get; set; }
        public bool LapsPolicyDetected { get; set; }
        public string LapsPolicySource { get; set; } = string.Empty;
        public bool DomainJoinedLikely { get; set; }
        public bool DomainPolicyOverrideLikely { get; set; }
        public bool ShadowPolicyNoConsentEnabled { get; set; }
        public bool ShadowUnsolicitedPolicyEnabled { get; set; }
        public bool AllowRemoteRpcEnabled { get; set; }
        public bool RestrictedAdminEnabled { get; set; }
        public bool RemoteInteractiveLogonRightGranted { get; set; }
        public bool RemoteInteractiveLogonDenied { get; set; }
        public int CurrentSessionId { get; set; }
        public bool UserInteractiveDesktop { get; set; }
        public bool RunningInSessionZero => CurrentSessionId == 0;
        public int ActiveSessions { get; set; }
        public bool ReadyForSafeRemoteAccess =>
            IsAdministrator && RemoteDesktopEnabled && TermServiceRunning && Port3389Listening;
        public bool RemoteExecutionReady =>
            ReadyForSafeRemoteAccess &&
            LocalAdminTokenPolicyEnabled &&
            RemoteRegistryRunning &&
            WinRmServiceRunning &&
            WinRmPort5985Listening &&
            TsUserAuthenticationPolicyEnabled &&
            ShadowPolicyNoConsentEnabled &&
            ShadowUnsolicitedPolicyEnabled;
        public string Guidance { get; set; } = string.Empty;
        public string WhyFailed { get; set; } = string.Empty;
        public List<string> Issues { get; set; } = new List<string>();
        public List<string> PolicyDiagnostics { get; set; } = new List<string>();
        public List<PolicyConflictDiagnostic> PolicyConflictDetails { get; set; } = new List<PolicyConflictDiagnostic>();

        public string ToSummary()
        {
            var issueText = Issues.Count == 0 ? "none" : string.Join(", ", Issues);
            return string.Join("; ",
                $"Ready={ReadyForSafeRemoteAccess}",
                $"Admin={IsAdministrator}",
                $"LocalSystem={RunningAsLocalSystem}",
                $"RDPEnabled={RemoteDesktopEnabled}",
                $"RdpPolicy={RdpPolicyEnabled}",
                $"NLA={NlaEnabled}",
                $"TSUserAuthPolicy={TsUserAuthenticationPolicyEnabled}",
                $"SingleSessionPolicy={SingleSessionPolicyEnabled}",
                $"TermService={TermServiceRunning}",
                $"UmRdpService={UmRdpServiceRunning}",
                $"Port3389={Port3389Listening}",
                $"LocalAdminTokenPolicy={LocalAdminTokenPolicyEnabled}",
                $"RemoteRegistry={RemoteRegistryRunning}",
                $"WinRM={WinRmServiceRunning}",
                $"WinRM5985={WinRmPort5985Listening}",
                $"LAPS={LapsPolicyDetected}",
                $"LapsSource={LapsPolicySource}",
                $"DomainJoined={DomainJoinedLikely}",
                $"DomainOverrideLikely={DomainPolicyOverrideLikely}",
                $"ShadowNoConsent={ShadowPolicyNoConsentEnabled}",
                $"ShadowUnsolicited={ShadowUnsolicitedPolicyEnabled}",
                $"AllowRemoteRPC={AllowRemoteRpcEnabled}",
                $"RestrictedAdmin={RestrictedAdminEnabled}",
                $"AllowRight={RemoteInteractiveLogonRightGranted}",
                $"DenyRight={RemoteInteractiveLogonDenied}",
                $"SessionId={CurrentSessionId}",
                $"Session0={RunningInSessionZero}",
                $"UserInteractive={UserInteractiveDesktop}",
                $"ExecReady={RemoteExecutionReady}",
                $"ActiveSessions={ActiveSessions}",
                $"Issues={issueText}",
                $"DiagCount={PolicyDiagnostics.Count}",
                $"WhyFailed={WhyFailed}",
                $"Guidance={Guidance}");
        }
    }

    public static class WindowsRemoteAccessService
    {
        private const string TerminalServerKey = @"SYSTEM\CurrentControlSet\Control\Terminal Server";
        private const string RdpTcpKey = @"SYSTEM\CurrentControlSet\Control\Terminal Server\WinStations\RDP-Tcp";

        public static bool IsRemoteDesktopEnabled()
        {
            try
            {
                using (var key = OpenLocalMachineSubKey(TerminalServerKey, false))
                {
                    var value = key?.GetValue("fDenyTSConnections");
                    return value != null && Convert.ToInt32(value) == 0;
                }
            }
            catch
            {
                return false;
            }
        }

        public static RemoteAccessResult EnableRemoteDesktop() => SetRemoteDesktopEnabled(true);

        public static RemoteAccessResult DisableRemoteDesktop() => SetRemoteDesktopEnabled(false);

        public static RemoteDesktopReadinessReport EvaluateRemoteDesktopReadiness()
        {
            var laps = ReadLapsPolicyState();
            var report = new RemoteDesktopReadinessReport
            {
                IsAdministrator = IsAdministrator(),
                RunningAsLocalSystem = IsRunningAsLocalSystem(),
                RemoteDesktopEnabled = IsRemoteDesktopEnabled(),
                RdpPolicyEnabled = ReadRdpPolicyEnabled(),
                NlaEnabled = ReadNlaEnabled(),
                TsUserAuthenticationPolicyEnabled = ReadTsUserAuthenticationPolicyEnabled(),
                SingleSessionPolicyEnabled = ReadSingleSessionPolicyEnabled(),
                TermServiceRunning = IsTermServiceRunning(),
                UmRdpServiceRunning = IsServiceRunning("UmRdpService"),
                Port3389Listening = IsPortListening(3389),
                LocalAdminTokenPolicyEnabled = ReadLocalAdminTokenPolicyEnabled(),
                RemoteRegistryRunning = IsServiceRunning("RemoteRegistry"),
                WinRmServiceRunning = IsServiceRunning("WinRM"),
                WinRmPort5985Listening = IsPortListening(5985),
                LapsPolicyDetected = laps.Detected,
                LapsPolicySource = laps.Source,
                ShadowPolicyNoConsentEnabled = ReadShadowPolicyNoConsentEnabled(),
                ShadowUnsolicitedPolicyEnabled = ReadShadowUnsolicitedPolicyEnabled(),
                AllowRemoteRpcEnabled = ReadAllowRemoteRpcEnabled(),
                RestrictedAdminEnabled = ReadRestrictedAdminEnabled(),
                CurrentSessionId = GetCurrentProcessSessionId(),
                UserInteractiveDesktop = IsUserInteractiveDesktop(),
                ActiveSessions = GetActiveSessionCount()
            };
            report.DomainJoinedLikely = IsDomainJoinedLikely();
            var rights = EvaluateRemoteInteractiveLogonRights();
            report.RemoteInteractiveLogonRightGranted = rights.AllowGranted;
            report.RemoteInteractiveLogonDenied = rights.DenyPresent;

            if (!report.IsAdministrator)
                report.Issues.Add("Run client service as Administrator.");
            if (!report.RunningAsLocalSystem)
                report.Issues.Add("Client is not running under LocalSystem; some machine-level operations can be limited.");
            if (!report.RemoteDesktopEnabled)
                report.Issues.Add("Remote Desktop registry flag is disabled.");
            if (!report.RdpPolicyEnabled)
                report.Issues.Add("Terminal Services policy can still deny RDP.");
            if (!report.TermServiceRunning)
                report.Issues.Add("TermService is not running.");
            if (!report.UmRdpServiceRunning)
                report.Issues.Add("UmRdpService is not running (RDP user-mode port redirector).");
            if (!report.Port3389Listening)
                report.Issues.Add("TCP 3389 is not listening (service/firewall/policy).");
            if (!report.NlaEnabled)
                report.Issues.Add("NLA is disabled; enable it for safer authentication.");
            if (!report.TsUserAuthenticationPolicyEnabled)
                report.Issues.Add("TS_USER_AUTHENTICATION_POLICY (UserAuthentication) is disabled.");
            if (!report.SingleSessionPolicyEnabled)
                report.Issues.Add("Single-session policy is not enforced.");
            if (!report.LocalAdminTokenPolicyEnabled)
                report.Issues.Add("LocalAccountTokenFilterPolicy is disabled (remote admin tokens can be filtered).");
            if (!report.RemoteRegistryRunning)
                report.Issues.Add("RemoteRegistry service is not running.");
            if (!report.WinRmServiceRunning)
                report.Issues.Add("WinRM service is not running.");
            if (!report.WinRmPort5985Listening)
                report.Issues.Add("TCP 5985 is not listening (WinRM listener/firewall/policy).");
            if (!report.LapsPolicyDetected)
                report.Issues.Add("Windows LAPS policy not detected (domain policy dependent).");
            if (!report.ShadowPolicyNoConsentEnabled)
                report.Issues.Add("RDP shadow policy is not set to full control without consent.");
            if (!report.ShadowUnsolicitedPolicyEnabled)
                report.Issues.Add("RDP shadow unsolicited policy is disabled (fAllowUnsolicited != 1).");
            if (!report.AllowRemoteRpcEnabled)
                report.Issues.Add("AllowRemoteRPC is disabled (shadow attach can fail).");
            if (!report.RestrictedAdminEnabled)
                report.Issues.Add("Restricted Admin support is disabled for /restrictedadmin.");
            if (!report.RemoteInteractiveLogonRightGranted)
                report.Issues.Add("Missing right: SeRemoteInteractiveLogonRight not granted to Administrators/Remote Desktop Users.");
            if (report.RemoteInteractiveLogonDenied)
                report.Issues.Add("Blocked right: SeDenyRemoteInteractiveLogonRight contains Administrators/Remote Desktop Users.");
            if (report.RunningInSessionZero)
                report.Issues.Add("Session 0 isolation detected: interactive capture/shadow actions require user-session companion.");

            if (report.RunningInSessionZero && report.ActiveSessions > 0)
            {
                report.Guidance = "Service is in Session 0 while users are active. Keep service for network/policy and execute interactive actions through a user-session companion.";
            }
            else if (report.ActiveSessions > 0)
            {
                report.Guidance = "Active user session detected. Use shadow/support mode to avoid interrupting ATM UI.";
            }
            else
            {
                report.Guidance = "No active user session detected. Standard admin RDP maintenance is possible.";
            }

            AppendPolicyConflictDiagnostics(report, rights);
            report.DomainPolicyOverrideLikely = report.PolicyConflictDetails.Any(diagnostic =>
                diagnostic.Source.IndexOf("domain", StringComparison.OrdinalIgnoreCase) >= 0 ||
                diagnostic.Source.IndexOf("gpo", StringComparison.OrdinalIgnoreCase) >= 0);
            report.WhyFailed = BuildWhyFailedSummary(report);

            return report;
        }

        public static string BuildHelpdeskAccessNote(string hostOrIp = "")
        {
            var target = string.IsNullOrWhiteSpace(hostOrIp) ? Environment.MachineName : hostOrIp.Trim();
            var lines = new[]
            {
                $"RDP: mstsc /v:{target} /admin",
                $"RDP RestrictedAdmin: mstsc /v:{target} /admin /restrictedadmin",
                $"RDP RemoteCredentialGuard: mstsc /v:{target} /remoteguard",
                "Session list: quser",
                $"Shadow assist (requires session id): mstsc /v:{target} /shadow:<sessionId> /control /prompt",
                "Note: Remote Credential Guard and /restrictedadmin depend on host/domain policy."
            };
            return string.Join(" ; ", lines);
        }

        public static string CaptureQuserSnapshot()
        {
            var result = RunHiddenDetailed("quser.exe", string.Empty, timeoutMs: 8000, truncateOutput: false);
            if (result.ExitCode == 0 && !string.IsNullOrWhiteSpace(result.Output))
                return result.Output;

            return "quser unavailable: " + result.Output;
        }

        public static IReadOnlyList<RemoteSessionDescriptor> GetRemoteSessions()
        {
            var snapshot = CaptureQuserSnapshot();
            return ParseQuserSnapshot(snapshot);
        }

        public static string BuildShadowAssistPlan(
            string hostOrIp = "",
            bool control = true,
            bool noConsentPrompt = false,
            bool promptForCredentials = true)
        {
            var plan = BuildSessionExecutionPlan(
                hostOrIp,
                requestNoConsentPrompt: noConsentPrompt,
                promptForCredentials: promptForCredentials);

            if (string.Equals(plan.Mode, "ShadowActiveSession", StringComparison.OrdinalIgnoreCase))
                return $"ShadowPlan: user={plan.UserName}, session={plan.SessionId}, cmd={plan.Command}";
            if (string.Equals(plan.Mode, "AdminMaintenanceSession", StringComparison.OrdinalIgnoreCase))
                return $"ShadowPlan: fallback={plan.Mode}, cmd={plan.Command}, reason={plan.Reason}";

            return $"ShadowPlan: mode={plan.Mode}, reason={plan.Reason}";
        }

        public static RemoteSessionExecutionPlan BuildSessionExecutionPlan(
            string hostOrIp = "",
            bool requestNoConsentPrompt = false,
            bool promptForCredentials = true)
        {
            var readiness = EvaluateRemoteDesktopReadiness();
            var sessions = GetRemoteSessions();
            return BuildSessionExecutionPlan(
                readiness,
                sessions,
                hostOrIp,
                requestNoConsentPrompt,
                promptForCredentials);
        }

        public static RemoteSessionExecutionPlan BuildSessionExecutionPlan(
            RemoteDesktopReadinessReport readiness,
            IReadOnlyList<RemoteSessionDescriptor> sessions,
            string hostOrIp = "",
            bool requestNoConsentPrompt = false,
            bool promptForCredentials = true)
        {
            var target = string.IsNullOrWhiteSpace(hostOrIp) ? Environment.MachineName : hostOrIp.Trim();
            var plan = new RemoteSessionExecutionPlan
            {
                Target = target,
                Ready = readiness != null && readiness.ReadyForSafeRemoteAccess,
                RequiresSessionCompanion = readiness != null &&
                                           readiness.RunningInSessionZero &&
                                           readiness.ActiveSessions > 0
            };

            if (readiness == null)
            {
                plan.Mode = "Blocked";
                plan.Reason = "Missing readiness snapshot.";
                return plan;
            }

            if (target.IndexOfAny(new[] { ' ', '\t', ';', '|', '&', '"' }) >= 0)
            {
                plan.Mode = "Blocked";
                plan.Reason = "Invalid target host/ip.";
                return plan;
            }

            if (!readiness.ReadyForSafeRemoteAccess)
            {
                plan.Mode = "Blocked";
                plan.Reason = string.IsNullOrWhiteSpace(readiness.WhyFailed)
                    ? "Remote desktop baseline is not ready."
                    : readiness.WhyFailed;
                return plan;
            }

            var selected = (sessions ?? Array.Empty<RemoteSessionDescriptor>())
                .OrderByDescending(item => item.IsActive)
                .ThenBy(item => item.SessionId)
                .FirstOrDefault();

            if (selected != null && selected.IsActive)
            {
                var allowNoConsent = false;
                plan.HasActiveSession = true;
                plan.Mode = "ShadowActiveSession";
                plan.SessionId = selected.SessionId;
                plan.UserName = selected.UserName;
                plan.NoConsentAllowed = allowNoConsent;
                plan.Command = BuildShadowAssistCommand(
                    target,
                    selected.SessionId,
                    control: true,
                    noConsentPrompt: false,
                    promptForCredentials: promptForCredentials);
                if (requestNoConsentPrompt)
                {
                    plan.Reason =
                        "Consent-bypass suppressed by policy. Shadow session requires user consent.";
                }
                else
                {
                    plan.Reason = "Shadow selected to avoid interrupting active ATM session.";
                }

                return plan;
            }

            plan.Mode = "AdminMaintenanceSession";
            plan.NoConsentAllowed = false;
            plan.Command = BuildAdminRdpCommand(target, readiness.RestrictedAdminEnabled, promptForCredentials);
            plan.Reason = "No active user session detected; maintenance admin session is preferred.";
            return plan;
        }

        public static string BuildAdminRdpCommand(
            string hostOrIp,
            bool restrictedAdminPreferred = true,
            bool promptForCredentials = true)
        {
            var target = string.IsNullOrWhiteSpace(hostOrIp) ? Environment.MachineName : hostOrIp.Trim();
            if (target.IndexOfAny(new[] { ' ', '\t', ';', '|', '&', '"' }) >= 0)
                return "RdpCommandBlocked: invalid target host/ip.";

            var args = new List<string> { $"/v:{target}", "/admin" };
            if (restrictedAdminPreferred)
                args.Add("/restrictedadmin");
            if (promptForCredentials)
                args.Add("/prompt");

            return "mstsc " + string.Join(" ", args);
        }

        public static string GenerateShadowCommandString(
            string hostOrIp,
            int sessionId,
            bool control = true,
            bool requestNoConsentPrompt = false,
            bool promptForCredentials = true,
            bool enforceNoConsentPolicy = true)
        {
            if (sessionId <= 0)
                return "ShadowCommandBlocked: invalid session id.";

            var target = string.IsNullOrWhiteSpace(hostOrIp) ? Environment.MachineName : hostOrIp.Trim();
            if (target.IndexOfAny(new[] { ' ', '\t', ';', '|', '&', '"' }) >= 0)
                return "ShadowCommandBlocked: invalid target host/ip.";

            var useNoConsentPrompt = false;

            var command = BuildShadowAssistCommand(
                target,
                sessionId,
                control,
                useNoConsentPrompt,
                promptForCredentials);

            if (requestNoConsentPrompt)
                return command + " ; note=consent bypass suppressed by policy";

            return command;
        }

        public static string BuildShadowAssistCommand(
            string hostOrIp,
            int sessionId,
            bool control = true,
            bool noConsentPrompt = true,
            bool promptForCredentials = true)
        {
            var target = string.IsNullOrWhiteSpace(hostOrIp) ? Environment.MachineName : hostOrIp.Trim();
            var args = new List<string> { $"/v:{target}" };
            if (promptForCredentials)
                args.Add("/prompt");
            args.Add($"/shadow:{sessionId}");
            if (control)
                args.Add("/control");
            _ = noConsentPrompt;
            return "mstsc " + string.Join(" ", args);
        }

        public static RemoteAccessResult EnsureRemoteAdministrationBaseline(
            string? allowedAccountsCsv,
            bool enforceNla = true,
            bool enableLocalAdminTokenPolicy = true,
            bool enableWinRm = true,
            bool enableRemoteRegistry = true)
        {
            if (!IsAdministrator())
            {
                return new RemoteAccessResult
                {
                    Success = false,
                    RequiresAdministrator = true,
                    Message = "Windows remote baseline requires Administrator privileges."
                };
            }

            var details = new List<string>();
            var before = EvaluateRemoteDesktopReadiness();
            details.Add("Before: " + before.ToSummary());

            var enableResult = SetRemoteDesktopEnabled(true);
            details.Add("EnableRdp: " + enableResult.Message);
            if (!enableResult.Success)
            {
                return new RemoteAccessResult
                {
                    Success = false,
                    Message = string.Join("; ", details)
                };
            }

            if (enforceNla)
                details.Add(EnsureNlaAndSessionSecurity());
            details.Add(EnsureTerminalServerPolicyDefaults());
            details.Add(EnsureShadowPolicyDefaults());
            details.Add(EnsureRestrictedAdminSupport());

            if (enableLocalAdminTokenPolicy)
                details.Add(EnsureLocalAdminTokenPolicy());

            details.Add(EnsureRemoteDesktopServiceAutoStart());
            details.Add(EnsureServiceAutoStartAndRunning("UmRdpService"));

            if (enableRemoteRegistry)
                details.Add(EnsureServiceAutoStartAndRunning("RemoteRegistry"));

            if (enableWinRm)
                details.Add(EnsureWinRmManagementChannel());

            foreach (var account in ParseAccountList(allowedAccountsCsv))
                details.Add(EnsureAccountCanUseRemoteDesktop(account));

            var after = EvaluateRemoteDesktopReadiness();
            details.Add("After: " + after.ToSummary());
            var success = after.RemoteExecutionReady;

            return new RemoteAccessResult
            {
                Success = success,
                Message = string.Join("; ", details)
            };
        }

        public static EndpointConfigurationReport ApplyScopedEndpointConfiguration(
            string serverIp,
            int serverPort,
            bool enforceScopedFirewallRule,
            int scopedFirewallPort,
            string? scopedFirewallRemoteAddresses,
            bool configureDefenderExclusions,
            string? defenderExclusionPaths,
            string? helpdeskAdGroup)
        {
            var report = new EndpointConfigurationReport();
            var details = report.Details;

            if (!IsAdministrator())
            {
                report.Success = false;
                report.RequiresAdministrator = true;
                report.Summary = "Endpoint hardening requires Administrator privileges.";
                details.Add(report.Summary);
                return report;
            }

            var effectivePort = ResolveScopedPort(serverPort, scopedFirewallPort);
            var remoteAddressList = BuildScopedRemoteAddressList(serverIp, scopedFirewallRemoteAddresses);
            var firewallDetail = string.Empty;
            var firewallApplied = !enforceScopedFirewallRule ||
                                  EnsureScopedFirewallRule(effectivePort, remoteAddressList, out firewallDetail);
            details.Add(firewallDetail);
            report.FirewallScoped = enforceScopedFirewallRule && firewallApplied;

            var exclusionPaths = BuildDefenderExclusionPathList(defenderExclusionPaths);
            var defenderDetail = string.Empty;
            var defenderApplied = !configureDefenderExclusions ||
                                  EnsureDefenderExclusions(exclusionPaths, out defenderDetail);
            details.Add(defenderDetail);
            report.DefenderScopedExclusionsApplied = configureDefenderExclusions && defenderApplied;

            var helpdeskAccount = ResolveHelpdeskAccountToken(helpdeskAdGroup);
            if (!string.IsNullOrWhiteSpace(helpdeskAccount))
                details.Add(EnsureAccountCanUseRemoteDesktop(helpdeskAccount));

            var readiness = EvaluateRemoteDesktopReadiness();
            var gpoGuidance = BuildDomainGpoGuidance(readiness, helpdeskAdGroup);
            if (!string.IsNullOrWhiteSpace(gpoGuidance))
                details.Add(gpoGuidance);

            report.DomainGpoRecommendationRequired =
                readiness.DomainPolicyOverrideLikely ||
                !readiness.RemoteInteractiveLogonRightGranted ||
                readiness.RemoteInteractiveLogonDenied ||
                (!readiness.LapsPolicyDetected && readiness.DomainJoinedLikely);

            report.Success = firewallApplied && defenderApplied;
            report.Summary = string.Join("; ", details.Where(value => !string.IsNullOrWhiteSpace(value)));
            return report;
        }

        public static RemoteAccessResult SetRemoteDesktopEnabled(bool enabled)
        {
            if (!IsAdministrator())
            {
                return new RemoteAccessResult
                {
                    Success = false,
                    RequiresAdministrator = true,
                    Message = "Windows Remote Access requires running EJLive Client as Administrator."
                };
            }

            var details = new List<string>();
            try
            {
                using (var key = OpenLocalMachineSubKey(TerminalServerKey, true))
                {
                    if (key == null)
                        throw new InvalidOperationException("Terminal Server registry key was not found.");
                    key.SetValue("fDenyTSConnections", enabled ? 0 : 1, RegistryValueKind.DWord);
                }
                details.Add(enabled ? "RDP registry enabled" : "RDP registry disabled");
                if (enabled)
                    details.Add(EnsureRdpPolicyOverrideEnabled());

                if (enabled)
                {
                    details.Add(RunHidden("sc.exe", "config TermService start= auto"));
                    details.Add(RunHidden("sc.exe", "start TermService"));
                    details.Add(RunHidden("sc.exe", "config UmRdpService start= demand"));
                    details.Add(RunHidden("sc.exe", "start UmRdpService"));
                    details.Add(RunHidden("netsh.exe", "advfirewall firewall set rule group=\"remote desktop\" new enable=Yes"));
                    details.Add(RunHidden("netsh.exe", "advfirewall firewall add rule name=\"EJLive Remote Desktop TCP 3389\" dir=in action=allow protocol=TCP localport=3389 profile=any"));
                    details.Add(RunHidden("netsh.exe", "advfirewall firewall add rule name=\"EJLive Remote Desktop UDP 3389\" dir=in action=allow protocol=UDP localport=3389 profile=any"));
                }
                else
                {
                    details.Add(RunHidden("netsh.exe", "advfirewall firewall set rule group=\"remote desktop\" new enable=No"));
                    details.Add(RunHidden("netsh.exe", "advfirewall firewall delete rule name=\"EJLive Remote Desktop TCP 3389\""));
                    details.Add(RunHidden("netsh.exe", "advfirewall firewall delete rule name=\"EJLive Remote Desktop UDP 3389\""));
                    details.Add(RunHidden("sc.exe", "stop TermService"));
                }

                return new RemoteAccessResult
                {
                    Success = true,
                    Message = string.Join("; ", details.ToArray())
                };
            }
            catch (Exception ex)
            {
                return new RemoteAccessResult
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        private static string EnsureRdpPolicyOverrideEnabled()
        {
            try
            {
                using (var policyRoot = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, Environment.Is64BitOperatingSystem ? RegistryView.Registry64 : RegistryView.Default))
                using (var key = policyRoot.CreateSubKey(@"SOFTWARE\Policies\Microsoft\Windows NT\Terminal Services", true))
                {
                    if (key == null)
                        return "RDP policy key not available.";

                    key.SetValue("fDenyTSConnections", 0, RegistryValueKind.DWord);
                    return "RDP policy override enabled (fDenyTSConnections=0).";
                }
            }
            catch (Exception ex)
            {
                return "RDP policy override failed: " + ex.Message;
            }
        }

        public static PasswordChangeResult SetLocalUserPassword(string userName, string newPassword)
        {
            if (string.IsNullOrWhiteSpace(userName))
            {
                return new PasswordChangeResult
                {
                    Success = false,
                    Message = "Username is required."
                };
            }

            if (string.IsNullOrWhiteSpace(newPassword))
            {
                return new PasswordChangeResult
                {
                    Success = false,
                    Message = "Password is required."
                };
            }

            if (!IsAdministrator())
            {
                return new PasswordChangeResult
                {
                    Success = false,
                    RequiresAdministrator = true,
                    Message = "Changing Windows user password requires Administrator privileges."
                };
            }

            if (userName.Contains("\"", StringComparison.Ordinal) ||
                newPassword.Contains("\"", StringComparison.Ordinal) ||
                userName.Contains(Environment.NewLine, StringComparison.Ordinal) ||
                newPassword.Contains(Environment.NewLine, StringComparison.Ordinal))
            {
                return new PasswordChangeResult
                {
                    Success = false,
                    Message = "Username/password contains unsupported characters for secure command execution."
                };
            }

            var safeUser = userName.Trim();
            var safePassword = newPassword.Trim();
            var run = RunHiddenDetailed("net.exe", $"user \"{safeUser}\" \"{safePassword}\"", timeoutMs: 15000, truncateOutput: false);
            if (run.ExitCode == 0)
            {
                return new PasswordChangeResult
                {
                    Success = true,
                    Message = $"Password updated for local account '{safeUser}'."
                };
            }

            return new PasswordChangeResult
            {
                Success = false,
                Message = $"Password change failed for '{safeUser}': {run.Output}"
            };
        }

        private static RegistryKey? OpenLocalMachineSubKey(string path, bool writable)
        {
            var view = Environment.Is64BitOperatingSystem ? RegistryView.Registry64 : RegistryView.Default;
            return RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, view).OpenSubKey(path, writable);
        }

        private static bool IsAdministrator()
        {
            try
            {
                using (var identity = WindowsIdentity.GetCurrent())
                {
                    return new WindowsPrincipal(identity).IsInRole(WindowsBuiltInRole.Administrator);
                }
            }
            catch
            {
                return false;
            }
        }

        private static bool IsRunningAsLocalSystem()
        {
            try
            {
                using var identity = WindowsIdentity.GetCurrent();
                var sid = identity?.User?.Value ?? string.Empty;
                return string.Equals(sid, "S-1-5-18", StringComparison.Ordinal);
            }
            catch
            {
                return false;
            }
        }

        private static bool ReadNlaEnabled()
        {
            try
            {
                using (var key = OpenLocalMachineSubKey(RdpTcpKey, false))
                {
                    var value = key?.GetValue("UserAuthentication");
                    return value != null && Convert.ToInt32(value) != 0;
                }
            }
            catch
            {
                return false;
            }
        }

        private static bool ReadRdpPolicyEnabled()
        {
            try
            {
                using (var key = OpenLocalMachineSubKey(@"SOFTWARE\Policies\Microsoft\Windows NT\Terminal Services", false))
                {
                    var value = key?.GetValue("fDenyTSConnections");
                    return value != null && Convert.ToInt32(value) == 0;
                }
            }
            catch
            {
                return false;
            }
        }

        private static bool ReadTsUserAuthenticationPolicyEnabled()
        {
            try
            {
                using (var policyKey = OpenLocalMachineSubKey(@"SOFTWARE\Policies\Microsoft\Windows NT\Terminal Services", false))
                {
                    var policyValue = policyKey?.GetValue("UserAuthentication");
                    if (policyValue != null)
                        return Convert.ToInt32(policyValue) == 1;
                }

                return ReadNlaEnabled();
            }
            catch
            {
                return false;
            }
        }

        private static bool ReadSingleSessionPolicyEnabled()
        {
            try
            {
                using (var policyKey = OpenLocalMachineSubKey(@"SOFTWARE\Policies\Microsoft\Windows NT\Terminal Services", false))
                {
                    var policyValue = policyKey?.GetValue("fSingleSessionPerUser");
                    if (policyValue != null)
                        return Convert.ToInt32(policyValue) == 1;
                }

                using (var key = OpenLocalMachineSubKey(TerminalServerKey, false))
                {
                    var value = key?.GetValue("fSingleSessionPerUser");
                    return value != null && Convert.ToInt32(value) == 1;
                }
            }
            catch
            {
                return false;
            }
        }

        private static bool ReadShadowPolicyNoConsentEnabled()
        {
            try
            {
                using (var key = OpenLocalMachineSubKey(@"SOFTWARE\Policies\Microsoft\Windows NT\Terminal Services", false))
                {
                    var value = key?.GetValue("Shadow");
                    // 2 = Full control without user permission.
                    return value != null && Convert.ToInt32(value) == 2;
                }
            }
            catch
            {
                return false;
            }
        }

        private static bool ReadShadowUnsolicitedPolicyEnabled()
        {
            try
            {
                using (var key = OpenLocalMachineSubKey(@"SOFTWARE\Policies\Microsoft\Windows NT\Terminal Services", false))
                {
                    var value = key?.GetValue("fAllowUnsolicited");
                    return value != null && Convert.ToInt32(value) == 1;
                }
            }
            catch
            {
                return false;
            }
        }

        private static bool ReadAllowRemoteRpcEnabled()
        {
            try
            {
                using (var key = OpenLocalMachineSubKey(TerminalServerKey, false))
                {
                    var value = key?.GetValue("AllowRemoteRPC");
                    return value != null && Convert.ToInt32(value) == 1;
                }
            }
            catch
            {
                return false;
            }
        }

        private static bool ReadRestrictedAdminEnabled()
        {
            try
            {
                using (var key = OpenLocalMachineSubKey(@"SYSTEM\CurrentControlSet\Control\Lsa", false))
                {
                    var value = key?.GetValue("DisableRestrictedAdmin");
                    // 0 = enabled, 1 = disabled. If missing, treat as disabled to keep strict precheck.
                    return value != null && Convert.ToInt32(value) == 0;
                }
            }
            catch
            {
                return false;
            }
        }

        private static bool IsDomainJoinedLikely()
        {
            try
            {
                var machine = (Environment.MachineName ?? string.Empty).Trim();
                var domain = (Environment.UserDomainName ?? string.Empty).Trim();
                if (machine.Length == 0 || domain.Length == 0)
                    return false;

                return !string.Equals(machine, domain, StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        private static RemoteInteractiveRightState EvaluateRemoteInteractiveLogonRights()
        {
            var state = new RemoteInteractiveRightState();
            var exportPath = Path.Combine(
                Path.GetTempPath(),
                "ejlive-rights-" + Guid.NewGuid().ToString("N") + ".inf");

            try
            {
                var run = RunHiddenDetailed(
                    "secedit.exe",
                    "/export /areas USER_RIGHTS /cfg \"" + exportPath + "\" /quiet",
                    timeoutMs: 15000,
                    truncateOutput: false);
                if (run.ExitCode != 0 || !File.Exists(exportPath))
                {
                    state.Error = "secedit export failed: " + run.Output;
                    return state;
                }

                var lines = File.ReadAllLines(exportPath);
                state.AllowRaw = ExtractSecurityPolicyAssignment(lines, "SeRemoteInteractiveLogonRight");
                state.DenyRaw = ExtractSecurityPolicyAssignment(lines, "SeDenyRemoteInteractiveLogonRight");
                state.AllowGranted = ContainsRemoteInteractivePrincipal(state.AllowRaw);
                state.DenyPresent = ContainsRemoteInteractivePrincipal(state.DenyRaw);
                state.Source = "Local security policy export (secedit)";
                return state;
            }
            catch (Exception ex)
            {
                state.Error = ex.Message;
                return state;
            }
            finally
            {
                try
                {
                    if (File.Exists(exportPath))
                        File.Delete(exportPath);
                }
                catch
                {
                }
            }
        }

        private static string ExtractSecurityPolicyAssignment(IEnumerable<string> lines, string keyName)
        {
            foreach (var rawLine in lines)
            {
                var line = (rawLine ?? string.Empty).Trim();
                if (!line.StartsWith(keyName + " ", StringComparison.OrdinalIgnoreCase) &&
                    !line.StartsWith(keyName + "=", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var index = line.IndexOf('=');
                if (index < 0 || index >= line.Length - 1)
                    return string.Empty;

                return line.Substring(index + 1).Trim();
            }

            return string.Empty;
        }

        private static bool ContainsRemoteInteractivePrincipal(string rawAssignment)
        {
            if (string.IsNullOrWhiteSpace(rawAssignment))
                return false;

            var normalized = rawAssignment.Replace("*", string.Empty, StringComparison.Ordinal);
            var tokens = normalized
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(item => item.Trim())
                .ToArray();

            return tokens.Any(token =>
                token.Equals("S-1-5-32-544", StringComparison.OrdinalIgnoreCase) ||
                token.Equals("S-1-5-32-555", StringComparison.OrdinalIgnoreCase) ||
                token.IndexOf("Administrators", StringComparison.OrdinalIgnoreCase) >= 0 ||
                token.IndexOf("Remote Desktop Users", StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private static void AppendPolicyConflictDiagnostics(RemoteDesktopReadinessReport report, RemoteInteractiveRightState rights)
        {
            var source = report.DomainJoinedLikely
                ? "Policies hive (likely domain/local GPO)"
                : "Policies hive (local policy)";

            if (!report.RdpPolicyEnabled)
            {
                var actual = DescribeValue(TryReadDwordValue(@"SOFTWARE\Policies\Microsoft\Windows NT\Terminal Services", "fDenyTSConnections"));
                AddPolicyDiagnostic(report,
                    @"HKLM\SOFTWARE\Policies\Microsoft\Windows NT\Terminal Services\fDenyTSConnections",
                    "0",
                    actual,
                    source,
                    "RDP can stay blocked by policy.");
            }

            if (!report.TsUserAuthenticationPolicyEnabled)
            {
                var actual = DescribeValue(TryReadDwordValue(@"SOFTWARE\Policies\Microsoft\Windows NT\Terminal Services", "UserAuthentication"));
                AddPolicyDiagnostic(report,
                    @"HKLM\SOFTWARE\Policies\Microsoft\Windows NT\Terminal Services\UserAuthentication",
                    "1",
                    actual,
                    source,
                    "NLA policy mismatch (TS_USER_AUTHENTICATION_POLICY).");
            }

            if (!report.ShadowPolicyNoConsentEnabled)
            {
                var actual = DescribeValue(TryReadDwordValue(@"SOFTWARE\Policies\Microsoft\Windows NT\Terminal Services", "Shadow"));
                AddPolicyDiagnostic(report,
                    @"HKLM\SOFTWARE\Policies\Microsoft\Windows NT\Terminal Services\Shadow",
                    "2",
                    actual,
                    source,
                    "Shadow without consent is blocked.");
            }

            if (!report.ShadowUnsolicitedPolicyEnabled)
            {
                var actual = DescribeValue(TryReadDwordValue(@"SOFTWARE\Policies\Microsoft\Windows NT\Terminal Services", "fAllowUnsolicited"));
                AddPolicyDiagnostic(report,
                    @"HKLM\SOFTWARE\Policies\Microsoft\Windows NT\Terminal Services\fAllowUnsolicited",
                    "1",
                    actual,
                    source,
                    "Unsolicited shadow attach policy is disabled.");
            }

            if (!report.AllowRemoteRpcEnabled)
            {
                var actual = DescribeValue(TryReadDwordValue(TerminalServerKey, "AllowRemoteRPC"));
                AddPolicyDiagnostic(report,
                    @"HKLM\SYSTEM\CurrentControlSet\Control\Terminal Server\AllowRemoteRPC",
                    "1",
                    actual,
                    "Local runtime key",
                    "Shadow attach may fail.");
            }

            if (!report.RestrictedAdminEnabled)
            {
                var actual = DescribeValue(TryReadDwordValue(@"SYSTEM\CurrentControlSet\Control\Lsa", "DisableRestrictedAdmin"));
                AddPolicyDiagnostic(report,
                    @"HKLM\SYSTEM\CurrentControlSet\Control\Lsa\DisableRestrictedAdmin",
                    "0",
                    actual,
                    "Local security policy",
                    "/restrictedadmin can be rejected.");
            }

            if (!report.RemoteInteractiveLogonRightGranted)
            {
                AddPolicyDiagnostic(report,
                    "SeRemoteInteractiveLogonRight",
                    "Contains Administrators or Remote Desktop Users",
                    string.IsNullOrWhiteSpace(rights.AllowRaw) ? "missing" : rights.AllowRaw,
                    rights.Source,
                    "Missing right blocks RDP logon.");
            }

            if (report.RemoteInteractiveLogonDenied)
            {
                AddPolicyDiagnostic(report,
                    "SeDenyRemoteInteractiveLogonRight",
                    "Must not contain Administrators or Remote Desktop Users",
                    string.IsNullOrWhiteSpace(rights.DenyRaw) ? "missing" : rights.DenyRaw,
                    rights.Source,
                    "Deny right overrides allow and blocks RDP.");
            }

            if (!report.LapsPolicyDetected)
            {
                AddPolicyDiagnostic(report,
                    @"HKLM\SOFTWARE\Microsoft\Policies\LAPS or HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\LAPS\Config",
                    "Present",
                    "Not detected",
                    report.DomainJoinedLikely ? "Likely domain policy missing on endpoint" : "Local policy missing",
                    "Password governance visibility is limited.");
            }

            if (report.RunningInSessionZero && report.ActiveSessions > 0)
            {
                AddPolicyDiagnostic(report,
                    "Session0Isolation",
                    "Interactive actions run from active user session",
                    "Service-only execution in Session 0",
                    "Windows session model",
                    "Screenshot/shadow operations can fail unless delegated to a user-session companion.");
            }

            if (!string.IsNullOrWhiteSpace(rights.Error))
            {
                AddPolicyDiagnostic(report,
                    "UserRightsExport",
                    "secedit export succeeds",
                    rights.Error,
                    "Local security subsystem",
                    "Cannot fully prove missing-right root cause.");
            }

            AppendPolicyOverrideDriftDiagnostics(report);
        }

        private static void AppendPolicyOverrideDriftDiagnostics(RemoteDesktopReadinessReport report)
        {
            AddPolicyOverrideDriftDiagnostic(
                report,
                @"SOFTWARE\Policies\Microsoft\Windows NT\Terminal Services",
                "fDenyTSConnections",
                TerminalServerKey,
                "fDenyTSConnections",
                "0",
                "Policy value diverges from local runtime RDP flag.");

            AddPolicyOverrideDriftDiagnostic(
                report,
                @"SOFTWARE\Policies\Microsoft\Windows NT\Terminal Services",
                "UserAuthentication",
                RdpTcpKey,
                "UserAuthentication",
                "1",
                "TS_USER_AUTHENTICATION_POLICY can override local NLA runtime value.");

            AddPolicyOverrideDriftDiagnostic(
                report,
                @"SOFTWARE\Policies\Microsoft\Windows NT\Terminal Services",
                "Shadow",
                @"SYSTEM\CurrentControlSet\Control\Terminal Server",
                "AllowRemoteRPC",
                "2",
                "Shadow policy may override no-consent assist flow.");
        }

        private static void AddPolicyOverrideDriftDiagnostic(
            RemoteDesktopReadinessReport report,
            string policyKeyPath,
            string policyValueName,
            string runtimeKeyPath,
            string runtimeValueName,
            string expected,
            string impact)
        {
            var policyValue = TryReadDwordValue(policyKeyPath, policyValueName);
            if (!policyValue.HasValue)
                return;

            var runtimeValue = TryReadDwordValue(runtimeKeyPath, runtimeValueName);
            var runtimeActual = DescribeValue(runtimeValue);
            var policyActual = DescribeValue(policyValue);
            if (runtimeActual.Equals(policyActual, StringComparison.OrdinalIgnoreCase))
                return;

            AddPolicyDiagnostic(
                report,
                $@"HKLM\{policyKeyPath}\{policyValueName}",
                expected,
                policyActual,
                report.DomainJoinedLikely ? "Likely domain GPO override" : "Local GPO override",
                impact + $" RuntimeKey={runtimeKeyPath}\\{runtimeValueName}={runtimeActual}");
        }

        private static int? TryReadDwordValue(string keyPath, string valueName)
        {
            try
            {
                using (var key = OpenLocalMachineSubKey(keyPath, false))
                {
                    var value = key?.GetValue(valueName);
                    if (value == null)
                        return null;

                    return Convert.ToInt32(value);
                }
            }
            catch
            {
                return null;
            }
        }

        private static string DescribeValue(int? value)
        {
            return value.HasValue ? value.Value.ToString() : "missing";
        }

        private static void AddPolicyDiagnostic(
            RemoteDesktopReadinessReport report,
            string key,
            string expected,
            string actual,
            string source,
            string impact)
        {
            if (report.PolicyConflictDetails.Any(existing =>
                    string.Equals(existing.Key, key, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(existing.Expected, expected, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(existing.Actual, actual, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(existing.Source, source, StringComparison.OrdinalIgnoreCase)))
            {
                return;
            }

            var diagnostic = new PolicyConflictDiagnostic
            {
                Key = key ?? string.Empty,
                Expected = expected ?? string.Empty,
                Actual = actual ?? string.Empty,
                Source = source ?? string.Empty,
                Impact = impact ?? string.Empty
            };
            report.PolicyConflictDetails.Add(diagnostic);
            report.PolicyDiagnostics.Add(diagnostic.ToWireFormat());
        }

        private static string BuildWhyFailedSummary(RemoteDesktopReadinessReport report)
        {
            if (report.RemoteExecutionReady)
                return "none";

            var blockers = new List<string>();
            if (!report.IsAdministrator)
                blockers.Add("Client is not running as Administrator.");
            if (!report.RemoteDesktopEnabled)
                blockers.Add("Remote Desktop runtime flag is disabled.");
            if (!report.RdpPolicyEnabled)
                blockers.Add("RDP denied by Terminal Services policy.");
            if (!report.TermServiceRunning)
                blockers.Add("TermService is not running.");
            if (!report.Port3389Listening)
                blockers.Add("TCP 3389 not listening.");
            if (!report.RemoteInteractiveLogonRightGranted)
                blockers.Add("Allow right missing: SeRemoteInteractiveLogonRight.");
            if (report.RemoteInteractiveLogonDenied)
                blockers.Add("Deny right present: SeDenyRemoteInteractiveLogonRight.");
            if (!report.TsUserAuthenticationPolicyEnabled)
                blockers.Add("TS_USER_AUTHENTICATION_POLICY mismatch.");
            if (!report.ShadowPolicyNoConsentEnabled)
                blockers.Add("Shadow policy is not full-control/no-consent.");
            if (!report.ShadowUnsolicitedPolicyEnabled)
                blockers.Add("Shadow unsolicited policy is disabled (fAllowUnsolicited).");
            if (!report.RestrictedAdminEnabled)
                blockers.Add("RestrictedAdmin disabled for /restrictedadmin flow.");
            if (report.RunningInSessionZero && report.ActiveSessions > 0)
                blockers.Add("Session 0 isolation blocks interactive operations from service context.");

            if (report.PolicyConflictDetails.Count > 0)
            {
                var top = report.PolicyConflictDetails
                    .Take(3)
                    .Select(diagnostic => $"{diagnostic.Key} (source={diagnostic.Source}, actual={diagnostic.Actual})");
                blockers.Add("PolicyConflicts: " + string.Join(" | ", top));
            }

            if (blockers.Count == 0 && report.Issues.Count > 0)
                blockers.AddRange(report.Issues.Take(3));

            if (blockers.Count == 0)
                blockers.Add("Readiness check failed without explicit blocker; inspect policy diagnostics.");

            var builder = new StringBuilder();
            builder.Append("RootCauses=");
            builder.Append(string.Join(" ; ", blockers.Distinct(StringComparer.OrdinalIgnoreCase)));
            return builder.ToString();
        }

        private static LapsPolicyState ReadLapsPolicyState()
        {
            var state = new LapsPolicyState();
            try
            {
                using (var key1 = OpenLocalMachineSubKey(@"SOFTWARE\Microsoft\Policies\LAPS", false))
                {
                    if (key1 != null)
                    {
                        state.Detected = true;
                        state.Source = @"HKLM\SOFTWARE\Microsoft\Policies\LAPS";
                        return state;
                    }
                }

                using (var key2 = OpenLocalMachineSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\LAPS\Config", false))
                {
                    if (key2 != null)
                    {
                        state.Detected = true;
                        state.Source = @"HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\LAPS\Config";
                        return state;
                    }
                }
            }
            catch
            {
                // Keep default "not detected" state.
            }

            state.Detected = false;
            state.Source = "Not detected";
            return state;
        }

        private static int GetCurrentProcessSessionId()
        {
            try
            {
                using var process = Process.GetCurrentProcess();
                return process.SessionId;
            }
            catch
            {
                return -1;
            }
        }

        private static bool IsUserInteractiveDesktop()
        {
            try
            {
                return Environment.UserInteractive;
            }
            catch
            {
                return false;
            }
        }

        private sealed class RemoteInteractiveRightState
        {
            public bool AllowGranted { get; set; }
            public bool DenyPresent { get; set; }
            public string AllowRaw { get; set; } = string.Empty;
            public string DenyRaw { get; set; } = string.Empty;
            public string Source { get; set; } = string.Empty;
            public string Error { get; set; } = string.Empty;
        }

        private sealed class LapsPolicyState
        {
            public bool Detected { get; set; }
            public string Source { get; set; } = string.Empty;
        }

        private static string EnsureNlaAndSessionSecurity()
        {
            try
            {
                using (var key = OpenLocalMachineSubKey(RdpTcpKey, true))
                {
                    if (key == null)
                        return "NLA: RDP-Tcp registry key missing.";

                    key.SetValue("UserAuthentication", 1, RegistryValueKind.DWord); // NLA required
                    key.SetValue("SecurityLayer", 2, RegistryValueKind.DWord); // Negotiate/TLS
                    key.SetValue("MinEncryptionLevel", 3, RegistryValueKind.DWord); // Client compatible
                }

                using (var terminalServerKey = OpenLocalMachineSubKey(TerminalServerKey, true))
                {
                    terminalServerKey?.SetValue("fSingleSessionPerUser", 1, RegistryValueKind.DWord);
                }

                return "NLA/Security policies applied.";
            }
            catch (Exception ex)
            {
                return "NLA/Security apply failed: " + ex.Message;
            }
        }

        private static string EnsureTerminalServerPolicyDefaults()
        {
            try
            {
                var view = Environment.Is64BitOperatingSystem ? RegistryView.Registry64 : RegistryView.Default;
                using (var root = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, view))
                using (var key = root.CreateSubKey(@"SOFTWARE\Policies\Microsoft\Windows NT\Terminal Services", true))
                {
                    if (key == null)
                        return "TerminalServerPolicy: policy key missing.";

                    key.SetValue("fDenyTSConnections", 0, RegistryValueKind.DWord); // allow RDP
                    key.SetValue("UserAuthentication", 1, RegistryValueKind.DWord); // TS_USER_AUTHENTICATION_POLICY
                    key.SetValue("fSingleSessionPerUser", 1, RegistryValueKind.DWord); // stable single-session policy
                    key.SetValue("MaxIdleTime", 0, RegistryValueKind.DWord); // disable forced idle timeout
                    key.SetValue("MaxDisconnectionTime", 0, RegistryValueKind.DWord); // disable forced disconnect timeout
                    key.SetValue("MaxConnectionTime", 0, RegistryValueKind.DWord); // disable forced total-connection timeout
                }

                return "TerminalServerPolicy: NLA/single-session/timeouts configured.";
            }
            catch (Exception ex)
            {
                return "TerminalServerPolicy apply failed: " + ex.Message;
            }
        }

        private static string EnsureShadowPolicyDefaults()
        {
            try
            {
                var view = Environment.Is64BitOperatingSystem ? RegistryView.Registry64 : RegistryView.Default;
                using (var root = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, view))
                using (var policyKey = root.CreateSubKey(@"SOFTWARE\Policies\Microsoft\Windows NT\Terminal Services", true))
                {
                    if (policyKey == null)
                        return "ShadowPolicy: policy key missing.";

                    // 2 = Full control without user permission.
                    policyKey.SetValue("Shadow", 2, RegistryValueKind.DWord);
                    policyKey.SetValue("fAllowUnsolicited", 1, RegistryValueKind.DWord);
                }

                using (var tsKey = OpenLocalMachineSubKey(TerminalServerKey, true))
                {
                    // Required by many environments for shadow attach compatibility.
                    tsKey?.SetValue("AllowRemoteRPC", 1, RegistryValueKind.DWord);
                }

                return "ShadowPolicy: full-control/no-consent + AllowRemoteRPC applied.";
            }
            catch (Exception ex)
            {
                return "ShadowPolicy apply failed: " + ex.Message;
            }
        }

        private static string EnsureRestrictedAdminSupport()
        {
            try
            {
                var view = Environment.Is64BitOperatingSystem ? RegistryView.Registry64 : RegistryView.Default;
                using (var root = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, view))
                using (var lsaKey = root.CreateSubKey(@"SYSTEM\CurrentControlSet\Control\Lsa", true))
                {
                    // 0 = allow /restrictedadmin mode for inbound RDP.
                    lsaKey?.SetValue("DisableRestrictedAdmin", 0, RegistryValueKind.DWord);
                }

                using (var root = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, view))
                using (var credPolicy = root.CreateSubKey(@"SOFTWARE\Policies\Microsoft\Windows\CredentialsDelegation", true))
                {
                    credPolicy?.SetValue("AllowProtectedCreds", 1, RegistryValueKind.DWord);
                    credPolicy?.SetValue("RestrictedRemoteAdministration", 1, RegistryValueKind.DWord);
                }

                return "RestrictedAdmin: inbound support and credential delegation policy applied.";
            }
            catch (Exception ex)
            {
                return "RestrictedAdmin apply failed: " + ex.Message;
            }
        }

        private static string EnsureRemoteDesktopServiceAutoStart()
        {
            return EnsureServiceAutoStartAndRunning("TermService");
        }

        private static string EnsureServiceAutoStartAndRunning(string serviceName)
        {
            if (string.IsNullOrWhiteSpace(serviceName))
                return "Service baseline skipped: empty service name.";

            var config = RunHidden("sc.exe", $"config \"{serviceName}\" start= auto");
            var start = RunHidden("sc.exe", $"start \"{serviceName}\"");
            return $"{serviceName}: {config} | {start}";
        }

        private static string EnsureWinRmManagementChannel()
        {
            var details = new List<string>
            {
                EnsureServiceAutoStartAndRunning("WinRM"),
                RunHidden("winrm.cmd", "quickconfig -quiet"),
                RunHidden(
                    "powershell.exe",
                    "-NoProfile -ExecutionPolicy Bypass -Command \"Enable-PSRemoting -Force -SkipNetworkProfileCheck\""),
                RunHidden(
                    "powershell.exe",
                    "-NoProfile -ExecutionPolicy Bypass -Command \"$rules = Get-NetFirewallRule -Name 'WINRM-HTTP-In-TCP*' -ErrorAction SilentlyContinue; if ($rules) { $rules | Set-NetFirewallRule -Enabled True -Action Allow -Profile Any -RemoteAddress Any }\"")
            };
            return "WinRM: " + string.Join(" | ", details.Where(value => !string.IsNullOrWhiteSpace(value)));
        }

        private static string EnsureLocalAdminTokenPolicy()
        {
            try
            {
                var view = Environment.Is64BitOperatingSystem ? RegistryView.Registry64 : RegistryView.Default;
                using (var root = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, view))
                using (var key = root.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", true))
                {
                    if (key == null)
                        return "LocalAccountTokenFilterPolicy: system policy key missing.";

                    var current = key.GetValue("LocalAccountTokenFilterPolicy");
                    var currentValue = current == null ? 0 : Convert.ToInt32(current);
                    if (currentValue == 1)
                        return "LocalAccountTokenFilterPolicy already enabled.";

                    key.SetValue("LocalAccountTokenFilterPolicy", 1, RegistryValueKind.DWord);
                    return "LocalAccountTokenFilterPolicy enabled for remote local-admin command execution.";
                }
            }
            catch (Exception ex)
            {
                return "LocalAccountTokenFilterPolicy apply failed: " + ex.Message;
            }
        }

        private static bool ReadLocalAdminTokenPolicyEnabled()
        {
            try
            {
                using (var key = OpenLocalMachineSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", false))
                {
                    var value = key?.GetValue("LocalAccountTokenFilterPolicy");
                    return value != null && Convert.ToInt32(value) == 1;
                }
            }
            catch
            {
                return false;
            }
        }

        private static string EnsureAccountCanUseRemoteDesktop(string account)
        {
            var safeAccount = NormalizeAccountToken(account);
            if (string.IsNullOrWhiteSpace(safeAccount))
                return "RdpUser: skipped empty account.";

            var run = RunHiddenDetailed(
                "net.exe",
                $"localgroup \"Remote Desktop Users\" \"{safeAccount}\" /add",
                timeoutMs: 12000,
                truncateOutput: false);
            if (run.ExitCode == 0)
                return $"RdpUser '{safeAccount}': allowed.";

            var output = run.Output ?? string.Empty;
            if (output.IndexOf("already a member", StringComparison.OrdinalIgnoreCase) >= 0)
                return $"RdpUser '{safeAccount}': already allowed.";

            return $"RdpUser '{safeAccount}': {output}";
        }

        private static IEnumerable<string> ParseAccountList(string? accountsCsv)
        {
            if (string.IsNullOrWhiteSpace(accountsCsv))
                return Array.Empty<string>();

            var parts = accountsCsv
                .Split(new[] { ',', ';', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(NormalizeAccountToken)
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
            return parts;
        }

        private static string NormalizeAccountToken(string account)
        {
            if (string.IsNullOrWhiteSpace(account))
                return string.Empty;

            var value = account.Trim();
            if (value.StartsWith(@".\", StringComparison.Ordinal))
                value = value.Substring(2);

            if (value.Contains("\"", StringComparison.Ordinal) ||
                value.Contains(Environment.NewLine, StringComparison.Ordinal))
            {
                return string.Empty;
            }

            return value;
        }

        private static bool IsTermServiceRunning()
        {
            return IsServiceRunning("TermService");
        }

        private static bool IsServiceRunning(string serviceName)
        {
            if (string.IsNullOrWhiteSpace(serviceName))
                return false;

            var output = RunHidden("sc.exe", $"query \"{serviceName}\"");
            return output.IndexOf("RUNNING", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static bool IsPortListening(int port)
        {
            try
            {
                return IPGlobalProperties.GetIPGlobalProperties()
                    .GetActiveTcpListeners()
                    .Any(endpoint => endpoint.Port == port);
            }
            catch
            {
                return false;
            }
        }

        private static int GetActiveSessionCount()
        {
            try
            {
                var run = RunHiddenDetailed("quser.exe", string.Empty, timeoutMs: 6000, truncateOutput: false);
                if (run.ExitCode != 0 || string.IsNullOrWhiteSpace(run.Output))
                    return 0;

                var lines = run.Output
                    .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(line => line.Trim())
                    .Where(line => line.Length > 0)
                    .ToArray();
                if (lines.Length <= 1)
                    return 0;

                // Skip header, count connected/active/disconnected sessions as existing user sessions.
                return lines
                    .Skip(1)
                    .Count(line =>
                        line.IndexOf("Active", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        line.IndexOf("Disc", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        line.IndexOf("Connected", StringComparison.OrdinalIgnoreCase) >= 0);
            }
            catch
            {
                return 0;
            }
        }

        private static IReadOnlyList<RemoteSessionDescriptor> ParseQuserSnapshot(string snapshot)
        {
            if (string.IsNullOrWhiteSpace(snapshot))
                return Array.Empty<RemoteSessionDescriptor>();

            var lines = snapshot
                .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(line => line.Trim())
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .ToArray();
            if (lines.Length == 0)
                return Array.Empty<RemoteSessionDescriptor>();

            var sessions = new List<RemoteSessionDescriptor>();
            foreach (var line in lines)
            {
                if (line.StartsWith("USERNAME", StringComparison.OrdinalIgnoreCase))
                    continue;

                var tokenLine = line.StartsWith(">", StringComparison.Ordinal) ? line[1..].TrimStart() : line;
                var tokens = Regex.Split(tokenLine, @"\s+")
                    .Where(token => !string.IsNullOrWhiteSpace(token))
                    .ToArray();
                if (tokens.Length < 3)
                    continue;

                var idIndex = -1;
                var sessionId = -1;
                for (var i = 1; i < tokens.Length; i++)
                {
                    if (int.TryParse(tokens[i], out sessionId))
                    {
                        idIndex = i;
                        break;
                    }
                }

                if (idIndex < 0 || sessionId < 0)
                    continue;

                var user = tokens[0];
                var sessionName = idIndex > 1 ? tokens[1] : string.Empty;
                var state = idIndex + 1 < tokens.Length ? tokens[idIndex + 1] : string.Empty;

                sessions.Add(new RemoteSessionDescriptor
                {
                    UserName = user,
                    SessionName = sessionName,
                    SessionId = sessionId,
                    State = state
                });
            }

            return sessions
                .OrderBy(item => item.SessionId)
                .ToArray();
        }

        private static int ResolveScopedPort(int serverPort, int scopedFirewallPort)
        {
            if (scopedFirewallPort > 0 && scopedFirewallPort <= 65535)
                return scopedFirewallPort;
            if (serverPort > 0 && serverPort <= 65535)
                return serverPort;
            return AppConstants.DefaultPort;
        }

        private static string BuildScopedRemoteAddressList(string? serverIp, string? additionalCsv)
        {
            var candidates = new List<string>();
            AddRemoteAddressCandidate(candidates, serverIp);
            if (!string.IsNullOrWhiteSpace(additionalCsv))
            {
                foreach (var token in additionalCsv.Split(new[] { ',', ';', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
                    AddRemoteAddressCandidate(candidates, token);
            }

            if (candidates.Count == 0)
                return "Any";

            return string.Join(",", candidates.Distinct(StringComparer.OrdinalIgnoreCase));
        }

        private static void AddRemoteAddressCandidate(ICollection<string> list, string? value)
        {
            var token = (value ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(token))
                return;

            if (token.Equals("any", StringComparison.OrdinalIgnoreCase))
            {
                list.Add("Any");
                return;
            }

            if (token.Equals("localsubnet", StringComparison.OrdinalIgnoreCase))
            {
                list.Add("LocalSubnet");
                return;
            }

            if (IPAddress.TryParse(token, out _))
            {
                list.Add(token);
                return;
            }

            var slashIndex = token.IndexOf('/');
            if (slashIndex > 0 && slashIndex < token.Length - 1)
            {
                var prefix = token.Substring(0, slashIndex).Trim();
                var cidrText = token.Substring(slashIndex + 1).Trim();
                if (IPAddress.TryParse(prefix, out _) &&
                    int.TryParse(cidrText, out var cidr) &&
                    cidr >= 0 &&
                    cidr <= 128)
                {
                    list.Add(prefix + "/" + cidr);
                }
            }
        }

        private static bool EnsureScopedFirewallRule(int port, string remoteAddresses, out string detail)
        {
            const string inboundRule = "EJLive Service Socket Inbound Scoped";
            const string outboundRule = "EJLive Service Socket Outbound Scoped";
            const string profiles = "domain,private,public";

            var deleteIn = RunHiddenDetailed("netsh.exe", $"advfirewall firewall delete rule name=\"{inboundRule}\"", 12000, truncateOutput: true);
            var deleteOut = RunHiddenDetailed("netsh.exe", $"advfirewall firewall delete rule name=\"{outboundRule}\"", 12000, truncateOutput: true);

            var addIn = RunHiddenDetailed(
                "netsh.exe",
                $"advfirewall firewall add rule name=\"{inboundRule}\" dir=in action=allow protocol=TCP localport={port} remoteip={remoteAddresses} profile={profiles}",
                15000,
                truncateOutput: false);
            var addOut = RunHiddenDetailed(
                "netsh.exe",
                $"advfirewall firewall add rule name=\"{outboundRule}\" dir=out action=allow protocol=TCP remoteport={port} remoteip={remoteAddresses} profile={profiles}",
                15000,
                truncateOutput: false);

            var success = addIn.ExitCode == 0 && addOut.ExitCode == 0;
            detail =
                $"ScopedFirewall: port={port}; remote={remoteAddresses}; deleteIn={deleteIn.ExitCode}; deleteOut={deleteOut.ExitCode}; addIn={addIn.ExitCode}; addOut={addOut.ExitCode}" +
                (success ? string.Empty : $"; out={Truncate(addIn.Output, 180)}; out2={Truncate(addOut.Output, 180)}");
            return success;
        }

        private static string[] BuildDefenderExclusionPathList(string? configuredPaths)
        {
            var paths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            AddExclusionPath(paths, AppContext.BaseDirectory);
            AddExclusionPath(paths, Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "EJLive"));
            AddExclusionPath(paths, Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "EJLive", "ClientService"));

            if (!string.IsNullOrWhiteSpace(configuredPaths))
            {
                foreach (var token in configuredPaths.Split(new[] { '|', ';', ',', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
                    AddExclusionPath(paths, token);
            }

            return paths.ToArray();
        }

        private static void AddExclusionPath(ISet<string> paths, string? candidate)
        {
            var value = (candidate ?? string.Empty).Trim().Trim('"');
            if (string.IsNullOrWhiteSpace(value))
                return;

            try
            {
                var fullPath = Path.GetFullPath(value);
                if (!string.IsNullOrWhiteSpace(fullPath))
                    paths.Add(fullPath);
            }
            catch
            {
                // Ignore invalid path token.
            }
        }

        private static bool EnsureDefenderExclusions(IEnumerable<string> paths, out string detail)
        {
            var pathList = paths
                .Where(path => !string.IsNullOrWhiteSpace(path))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            if (pathList.Length == 0)
            {
                detail = "DefenderExclusion: skipped (no paths).";
                return true;
            }

            var scriptBuilder = new StringBuilder();
            scriptBuilder.Append("$ErrorActionPreference='Stop'; ");
            scriptBuilder.Append("if (-not (Get-Command Add-MpPreference -ErrorAction SilentlyContinue)) { Write-Output 'Add-MpPreference unavailable'; exit 0 }; ");
            scriptBuilder.Append("$paths=@(");
            for (var i = 0; i < pathList.Length; i++)
            {
                if (i > 0)
                    scriptBuilder.Append(',');
                scriptBuilder.Append('\'');
                scriptBuilder.Append(pathList[i].Replace("'", "''", StringComparison.Ordinal));
                scriptBuilder.Append('\'');
            }
            scriptBuilder.Append("); ");
            scriptBuilder.Append("$added=0; foreach ($p in $paths) { if (Test-Path -LiteralPath $p) { Add-MpPreference -ExclusionPath $p -ErrorAction SilentlyContinue; $added++ } }; ");
            scriptBuilder.Append("Write-Output (\"Added=\" + $added)");

            var run = RunHiddenDetailed(
                "powershell.exe",
                "-NoProfile -ExecutionPolicy Bypass -Command \"" + scriptBuilder.ToString().Replace("\"", "`\"", StringComparison.Ordinal) + "\"",
                timeoutMs: 25000,
                truncateOutput: false);

            var success = run.ExitCode == 0;
            detail = $"DefenderExclusion: paths={pathList.Length}; exit={run.ExitCode}; {Truncate(run.Output, 220)}";
            return success;
        }

        private static string BuildDomainGpoGuidance(RemoteDesktopReadinessReport readiness, string? helpdeskAdGroup)
        {
            var needsGpoGuidance =
                readiness.DomainPolicyOverrideLikely ||
                !readiness.RemoteInteractiveLogonRightGranted ||
                readiness.RemoteInteractiveLogonDenied ||
                !readiness.ShadowPolicyNoConsentEnabled;
            if (!needsGpoGuidance)
                return string.Empty;

            var group = string.IsNullOrWhiteSpace(helpdeskAdGroup)
                ? "EJLive-Helpdesk"
                : helpdeskAdGroup.Trim();

            return string.Join(" | ", new[]
            {
                "DomainGpoGuidance: apply central GPO for RDS shadow + RDP rights.",
                "Set 'Set rules for remote control of Remote Desktop Services user sessions' to 'Full Control without user's permission'.",
                "Grant SeRemoteInteractiveLogonRight to Administrators/Remote Desktop Users and AD group '" + group + "'.",
                "Ensure deny-right policy does not include Administrators/Remote Desktop Users.",
                "Keep Windows LAPS managed by domain policy for password governance."
            });
        }

        private static string ResolveHelpdeskAccountToken(string? helpdeskAdGroup)
        {
            var group = (helpdeskAdGroup ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(group))
                return string.Empty;

            if (group.Contains("\\", StringComparison.Ordinal))
                return group;

            var domain = (Environment.UserDomainName ?? string.Empty).Trim();
            var machine = (Environment.MachineName ?? string.Empty).Trim();
            if (!string.IsNullOrWhiteSpace(domain) &&
                !string.Equals(domain, machine, StringComparison.OrdinalIgnoreCase))
            {
                return domain + "\\" + group;
            }

            return group;
        }

        private static string Truncate(string? value, int max)
        {
            var text = value ?? string.Empty;
            if (text.Length <= max)
                return text;
            return text.Substring(0, max);
        }

        private static string RunHidden(string fileName, string arguments)
        {
            var run = RunHiddenDetailed(fileName, arguments, timeoutMs: 10000, truncateOutput: true);
            return $"{fileName} exit={run.ExitCode}" + (string.IsNullOrWhiteSpace(run.Output) ? string.Empty : $" ({run.Output})");
        }

        private static CommandRun RunHiddenDetailed(string fileName, string arguments, int timeoutMs, bool truncateOutput)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = arguments,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                using (var process = Process.Start(psi))
                {
                    if (process == null)
                        return new CommandRun { ExitCode = -1, Output = $"{fileName}: not started" };

                    if (!process.WaitForExit(timeoutMs))
                    {
                        try { process.Kill(); } catch { }
                        return new CommandRun { ExitCode = -2, Output = $"{fileName}: timeout" };
                    }

                    var output = (process.StandardOutput.ReadToEnd() + " " + process.StandardError.ReadToEnd()).Trim();
                    if (truncateOutput && output.Length > 180)
                        output = output.Substring(0, 180);

                    return new CommandRun
                    {
                        ExitCode = process.ExitCode,
                        Output = output
                    };
                }
            }
            catch (Exception ex)
            {
                return new CommandRun { ExitCode = -3, Output = ex.Message };
            }
        }

        private sealed class CommandRun
        {
            public int ExitCode { get; set; }
            public string Output { get; set; } = string.Empty;
        }
    }
}
