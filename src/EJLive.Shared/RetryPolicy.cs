namespace EJLive.Shared;

public sealed class RetryPolicy
{
    public static readonly RetryPolicy LAN = new("LAN", 5, 1_000, 15_000, 1.8);
    public static readonly RetryPolicy ADSL = new("ADSL", 8, 2_000, 30_000, 2.0);
    public static readonly RetryPolicy GSM = new("GSM", 12, 5_000, 90_000, 2.2);
    public static readonly RetryPolicy CDMA = new("CDMA", 10, 4_000, 60_000, 2.0);

    public RetryPolicy(string name, int maxAttempts, int baseDelayMs, int maxDelayMs, double multiplier)
    {
        Name = name;
        MaxAttempts = Math.Max(1, maxAttempts);
        BaseDelayMs = Math.Max(1, baseDelayMs);
        MaxDelayMs = Math.Max(BaseDelayMs, maxDelayMs);
        Multiplier = Math.Max(1.0, multiplier);
    }

    public string Name { get; }
    public int MaxAttempts { get; }
    public int BaseDelayMs { get; }
    public int MaxDelayMs { get; }
    public double Multiplier { get; }

    public int ComputeDelay(int attempt)
    {
        var safeAttempt = Math.Max(1, attempt);
        var delay = BaseDelayMs * Math.Pow(Multiplier, safeAttempt - 1);
        var jitter = Random.Shared.Next(0, Math.Max(50, BaseDelayMs / 2));
        return Math.Min(MaxDelayMs, checked((int)Math.Min(int.MaxValue, delay + jitter)));
    }

    public static RetryPolicy ForNetwork(string? networkType)
    {
        return (networkType ?? string.Empty).Trim().ToUpperInvariant() switch
        {
            "GSM" => GSM,
            "CDMA" => CDMA,
            "ADSL" => ADSL,
            _ => LAN
        };
    }
}
