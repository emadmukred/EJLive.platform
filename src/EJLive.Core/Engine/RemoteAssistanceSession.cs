using EJLive.Core.Models;

namespace EJLive.Core.Engine;

/// <summary>
/// Defines policy rules for remote sessions.
/// </summary>
public interface IRemoteSessionPolicy
{
    /// <summary>Determines whether the specified session type is allowed.</summary>
    bool IsSessionTypeAllowed(RemoteSessionType sessionType);

    /// <summary>Determines whether the specified operator is allowed.</summary>
    bool IsOperatorAllowed(string operatorId);

    /// <summary>Determines whether no-consent prompts are allowed for the session type.</summary>
    bool AllowNoConsentPrompt(RemoteSessionType sessionType);
}

/// <summary>
/// Stores and retrieves remote session audit records.
/// </summary>
public interface IRemoteSessionAuditStore
{
    /// <summary>Records the start of a session.</summary>
    Task RecordStartAsync(RemoteSessionAudit audit, CancellationToken cancellationToken = default);

    /// <summary>Records the stop of a session.</summary>
    Task RecordStopAsync(RemoteSessionAudit audit, CancellationToken cancellationToken = default);

    /// <summary>Checks whether an explicit consent waiver exists for the request.</summary>
    bool HasExplicitConsentWaiver(RemoteSessionRequest request);
}

/// <summary>
/// Notifies stakeholders of remote session lifecycle events.
/// </summary>
public interface IRemoteSessionNotifier
{
    /// <summary>Notifies that a session has started.</summary>
    Task NotifySessionStartedAsync(RemoteSessionAudit audit, CancellationToken cancellationToken = default);

    /// <summary>Notifies that a session has stopped.</summary>
    Task NotifySessionStoppedAsync(RemoteSessionAudit audit, CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents the planned execution parameters for a remote session.
/// </summary>
public sealed record RemoteSessionExecutionPlan
{
    /// <summary>Reference to the original request.</summary>
    public required Guid RequestId { get; init; }

    /// <summary>Whether the session is permitted to start.</summary>
    public required bool IsAllowed { get; init; }

    /// <summary>Whether the session requires a consent prompt on the ATM.</summary>
    public bool RequiresConsentPrompt { get; init; }

    /// <summary>Whether no-consent operation is allowed for this session.</summary>
    public bool NoConsentAllowed { get; init; }

    /// <summary>Maximum allowed duration in minutes.</summary>
    public int MaxDurationMinutes { get; init; }

    /// <summary>Scheduled start time in UTC.</summary>
    public DateTime ScheduledStartUtc { get; init; }

    /// <summary>Scheduled end time in UTC.</summary>
    public DateTime ScheduledEndUtc { get; init; }

    /// <summary>Reason for denial, if applicable.</summary>
    public string? DenialReason { get; init; }

    /// <summary>Creates a denied execution plan.</summary>
    public static RemoteSessionExecutionPlan Denied(string reason) => new()
    {
        RequestId = Guid.Empty,
        IsAllowed = false,
        DenialReason = reason
    };
}

/// <summary>
/// Plans and executes remote assistance sessions with governance controls.
/// </summary>
public class RemoteAssistanceSession
{
    private readonly IRemoteSessionPolicy _policy;
    private readonly IRemoteSessionAuditStore _auditStore;
    private readonly IRemoteSessionNotifier _notifier;
    private readonly TimeProvider _timeProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="RemoteAssistanceSession"/> class.
    /// </summary>
    public RemoteAssistanceSession(
        IRemoteSessionPolicy policy,
        IRemoteSessionAuditStore auditStore,
        IRemoteSessionNotifier notifier,
        TimeProvider? timeProvider = null)
    {
        _policy = policy ?? throw new ArgumentNullException(nameof(policy));
        _auditStore = auditStore ?? throw new ArgumentNullException(nameof(auditStore));
        _notifier = notifier ?? throw new ArgumentNullException(nameof(notifier));
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    /// <summary>
    /// Creates an execution plan for a remote session request.
    /// </summary>
    public RemoteSessionExecutionPlan CreateExecutionPlan(
        RemoteSessionRequest request,
        IReadOnlyList<RemoteSessionAudit> activeSessions)
    {
        if (request is null) throw new ArgumentNullException(nameof(request));
        if (activeSessions is null) throw new ArgumentNullException(nameof(activeSessions));

        var now = _timeProvider.GetUtcNow().UtcDateTime;

        // Check if request is approved and within duration
        if (!request.ApprovedUtc.HasValue)
        {
            return RemoteSessionExecutionPlan.Denied("Request has not been approved.");
        }

        if (request.ApprovedUtc.Value.AddMinutes(request.RequestedDurationMinutes) < now)
        {
            return RemoteSessionExecutionPlan.Denied("Approval has expired.");
        }

        // Check policy
        if (!_policy.IsSessionTypeAllowed(request.SessionType))
        {
            return RemoteSessionExecutionPlan.Denied("Session type is not permitted by policy.");
        }

        if (!_policy.IsOperatorAllowed(request.OperatorId))
        {
            return RemoteSessionExecutionPlan.Denied("Operator is not permitted by policy.");
        }

        // Check active sessions - do not allow multiple active sessions on same ATM
        var hasActiveSession = activeSessions.Any(s => s.EndUtc is null);
        if (hasActiveSession)
        {
            return RemoteSessionExecutionPlan.Denied("An active session already exists on this ATM.");
        }

        // Determine consent requirement
        bool noConsentAllowed = _policy.AllowNoConsentPrompt(request.SessionType)
            && _auditStore.HasExplicitConsentWaiver(request);

        var plan = new RemoteSessionExecutionPlan
        {
            RequestId = request.RequestId,
            IsAllowed = true,
            RequiresConsentPrompt = !noConsentAllowed,
            NoConsentAllowed = noConsentAllowed,
            MaxDurationMinutes = request.RequestedDurationMinutes,
            ScheduledStartUtc = now,
            ScheduledEndUtc = now.AddMinutes(request.RequestedDurationMinutes)
        };

        return plan;
    }

    /// <summary>
    /// Starts the remote session according to the execution plan.
    /// </summary>
    public async Task<RemoteSessionAudit> StartAsync(
        RemoteSessionExecutionPlan plan,
        CancellationToken cancellationToken = default)
    {
        if (plan is null) throw new ArgumentNullException(nameof(plan));
        if (!plan.IsAllowed) throw new InvalidOperationException("Cannot start a denied session.");

        var sessionId = Guid.NewGuid();
        var now = _timeProvider.GetUtcNow().UtcDateTime;

        var audit = new RemoteSessionAudit
        {
            RequestId = plan.RequestId,
            SessionId = sessionId,
            StartUtc = now,
            Outcome = RemoteSessionOutcome.Success
        };

        await _auditStore.RecordStartAsync(audit, cancellationToken).ConfigureAwait(false);
        await _notifier.NotifySessionStartedAsync(audit, cancellationToken).ConfigureAwait(false);

        return audit;
    }

    /// <summary>
    /// Stops an active remote session.
    /// </summary>
    public async Task<RemoteSessionAudit> StopAsync(
        RemoteSessionAudit activeAudit,
        string reason,
        CancellationToken cancellationToken = default)
    {
        if (activeAudit is null) throw new ArgumentNullException(nameof(activeAudit));
        if (string.IsNullOrWhiteSpace(reason)) throw new ArgumentException("Stop reason is required.", nameof(reason));

        var now = _timeProvider.GetUtcNow().UtcDateTime;

        var completedAudit = activeAudit with
        {
            EndUtc = now,
            Outcome = RemoteSessionOutcome.Stopped,
            StopReason = reason
        };

        await _auditStore.RecordStopAsync(completedAudit, cancellationToken).ConfigureAwait(false);
        await _notifier.NotifySessionStoppedAsync(completedAudit, cancellationToken).ConfigureAwait(false);

        return completedAudit;
    }

    /// <summary>
    /// Checks for timed-out sessions and terminates them.
    /// </summary>
    public async Task<IReadOnlyList<RemoteSessionAudit>> EnforceTimeoutsAsync(
        IReadOnlyList<RemoteSessionAudit> activeAudits,
        RemoteSessionRequest request,
        CancellationToken cancellationToken = default)
    {
        if (activeAudits is null) throw new ArgumentNullException(nameof(activeAudits));
        if (request is null) throw new ArgumentNullException(nameof(request));

        var now = _timeProvider.GetUtcNow().UtcDateTime;
        var expired = new List<RemoteSessionAudit>();

        foreach (var audit in activeAudits.Where(a => a.EndUtc is null))
        {
            var maxEnd = audit.StartUtc.AddMinutes(request.RequestedDurationMinutes);
            if (now > maxEnd)
            {
                var timedOut = audit with
                {
                    EndUtc = now,
                    Outcome = RemoteSessionOutcome.TimedOut,
                    StopReason = "Session exceeded requested duration."
                };

                await _auditStore.RecordStopAsync(timedOut, cancellationToken).ConfigureAwait(false);
                await _notifier.NotifySessionStoppedAsync(timedOut, cancellationToken).ConfigureAwait(false);
                expired.Add(timedOut);
            }
        }

        return expired;
    }
}
