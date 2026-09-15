namespace EJLive.Shared;

/// <summary>
/// Canonical retry policy for the whole platform (Wave-4 fix-up, C-27).
/// Before this revision two divergent copies existed — <c>EJLive.Core.Models.RetryPolicy</c>
/// (MaxRetries/DelayMs) in <c>CoreAdapters.cs</c> and an <c>EJLive.Shared</c> copy
/// (MaxAttempts/BaseDelay) inside <c>Core/Shared/SharedPrimitives.cs</c>. The divergence
/// produced CS0104 in <c>NetworkEngine</c> and left <c>JournalOutbox</c> callers without the
/// named-constructor shape they already used. This type is the single owner:
/// bounded exponential backoff, a stable <see cref="Name"/> for diagnostics, a
/// <see cref="ForNetwork"/> factory for the reconnect loop, and <see cref="Default"/>
/// for legacy call sites.
/// </summary>
public sealed class RetryPolicy
{
    /// <summary>Diagnostic label carried into logs and audit entries.</summary>
    public string Name { get; }

    /// <summary>Maximum attempts before the operation is failed over to the outbox/dead-letter path.</summary>
    public int MaxAttempts { get; init; } = 3;

    /// <summary>Delay for the first attempt; later attempts multiply from here.</summary>
    public int BaseDelayMs { get; init; } = 250;

    /// <summary>Hard cap so a long outage cannot produce an unbounded sleep.</summary>
    public int MaxDelayMs { get; init; } = 30_000;

    /// <summary>Exponential growth factor between attempts.</summary>
    public double Multiplier { get; init; } = 2.0;

    public RetryPolicy(
        string name = "default",
        int maxAttempts = 3,
        int baseDelayMs = 250,
        int maxDelayMs = 30_000,
        double multiplier = 2.0)
    {
        Name = string.IsNullOrWhiteSpace(name) ? "default" : name;
        MaxAttempts = Math.Max(1, maxAttempts);
        BaseDelayMs = Math.Max(1, baseDelayMs);
        MaxDelayMs = Math.Max(BaseDelayMs, maxDelayMs);
        Multiplier = multiplier <= 0 ? 1.0 : multiplier;
    }

    /// <summary>Shared fallback instance — never mutate; construct a policy per call site instead.</summary>
    public static RetryPolicy Default { get; } = new("default");

    /// <summary>
    /// Reconnect-loop profile: five attempts, one-second base, doubling up to the cap.
    /// Consumed by <c>NetworkEngine</c> exactly as the retired <c>NetworkRetryPolicy</c> promised.
    /// </summary>
    public static RetryPolicy ForNetwork(string networkType)
        => new($"network:{networkType}", maxAttempts: 5, baseDelayMs: 1_000, maxDelayMs: 30_000, multiplier: 2.0);

    /// <summary>Bounded exponential delay for a 1-based attempt number.</summary>
    public TimeSpan ComputeDelay(int attempt)
    {
        var bounded = Math.Max(1, attempt);
        var ms = BaseDelayMs * Math.Pow(Multiplier, bounded - 1);
        return TimeSpan.FromMilliseconds(Math.Min(ms, MaxDelayMs));
    }

    /// <summary>Alias kept for call sites migrated from the retired Shared copy.</summary>
    public TimeSpan GetDelay(int attempt) => ComputeDelay(attempt);
}
