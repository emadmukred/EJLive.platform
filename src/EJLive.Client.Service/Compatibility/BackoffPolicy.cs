namespace EJLive.Client.Service.Compatibility;

/// <summary>
/// Exponential backoff with jitter to prevent reconnect storms when
/// many ATM agents regain network connectivity simultaneously.
/// </summary>
internal sealed class BackoffPolicy
{
    private readonly TimeSpan _minDelay;
    private readonly TimeSpan _maxDelay;
    private readonly Random _random = new();
    private int _attempt;

    public BackoffPolicy(TimeSpan? minDelay = null, TimeSpan? maxDelay = null)
    {
        _minDelay = minDelay ?? TimeSpan.FromSeconds(5);
        _maxDelay = maxDelay ?? TimeSpan.FromMinutes(2);
    }

    public TimeSpan NextDelay()
    {
        var exponent = Math.Min(_attempt++, 6);
        var baseSeconds = Math.Min(_minDelay.TotalSeconds * Math.Pow(2, exponent), _maxDelay.TotalSeconds);
        var jitter = _random.NextDouble() * Math.Min(5, baseSeconds * 0.20);
        return TimeSpan.FromSeconds(Math.Min(baseSeconds + jitter, _maxDelay.TotalSeconds));
    }

    public void Reset() => _attempt = 0;
}
