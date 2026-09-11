using System;
using System.Diagnostics;

namespace EJLive.Core.Utils
{
    public partial class RetryPolicy
        {
            private readonly int _maxAttempts;
            private readonly int _baseDelayMs;
            private readonly bool _useExponentialBackoff;
            private readonly bool _useJitter;
            private readonly Random _random = new();
            _baseDelayMs = baseDelayMs;
            _useExponentialBackoff = useExponentialBackoff;
            _useJitter = useJitter;
            public static readonly RetryPolicy LAN = new RetryPolicy
            {
                Name        = "LAN",
                MaxAttempts = 20,
                BaseDelayMs = 500,
                MaxDelayMs  = 30_000,
                Multiplier  = 1.5,
                JitterRatio = 0.1
            };
            public static readonly RetryPolicy ADSL = new RetryPolicy
            {
                Name        = "ADSL",
                MaxAttempts = 15,
                BaseDelayMs = 2_000,
                MaxDelayMs  = 120_000,
                Multiplier  = 2.0,
                JitterRatio = 0.25
            };
            public static readonly RetryPolicy GSM = new RetryPolicy
            {
                Name        = "GSM",
                MaxAttempts = 10,
                BaseDelayMs = 5_000,
                MaxDelayMs  = 300_000,   // 5 دقائق
                Multiplier  = 2.5,
                JitterRatio = 0.3
            };
            public static readonly RetryPolicy CDMA = new RetryPolicy
            {
                Name        = "CDMA",
                MaxAttempts = 12,
                BaseDelayMs = 3_000,
                MaxDelayMs  = 180_000,
                Multiplier  = 2.0,
                JitterRatio = 0.25
            };
            public static readonly RetryPolicy LAN = new("LAN", 5, 1_000, 15_000, 1.8);
            public static readonly RetryPolicy ADSL = new("ADSL", 8, 2_000, 30_000, 2.0);
            public static readonly RetryPolicy GSM = new("GSM", 12, 5_000, 90_000, 2.2);
            public static readonly RetryPolicy CDMA = new("CDMA", 10, 4_000, 60_000, 2.0);
            public string Name        { get; set; }
            public int    MaxAttempts { get; set; }
            public int    BaseDelayMs { get; set; }
            public int    MaxDelayMs  { get; set; }
            public double Multiplier  { get; set; } = 2.0;
            public double JitterRatio { get; set; } = 0.2;    // ±20% jitter
            public RetryPolicy(int maxAttempts = 10, int baseDelayMs = 5000,
            bool useExponentialBackoff = true, bool useJitter = true)
            _maxAttempts = maxAttempts;
            public RetryPolicy(int maxAttempts = 10, int baseDelayMs = 5000,
                                bool useExponentialBackoff = true, bool useJitter = true)
            {
                _maxAttempts = maxAttempts;
                _baseDelayMs = baseDelayMs;
                _useExponentialBackoff = useExponentialBackoff;
                _useJitter = useJitter;
            }
            public RetryPolicy(string name, int maxAttempts, int baseDelayMs, int maxDelayMs, double multiplier)
            Name = name;
            public async Task<T> ExecuteAsync<T>(Func<Task<T>> action, Func<Exception, bool>? shouldRetry = null,
                                                    CancellationToken cancellationToken = default)
            {
                for (int attempt = 1; attempt <= _maxAttempts; attempt++)
                {
                    try
                    {
                        return await action();
                    }
                    catch (Exception ex) when (attempt < _maxAttempts && (shouldRetry?.Invoke(ex) ?? true))
                    {
                        var delay = CalculateDelay(attempt);
                        Debug.WriteLine($"[Retry] Attempt {attempt} failed: {ex.Message}. Retrying in {delay}ms...");
                        await Task.Delay(delay, cancellationToken);
                    }
                }
                throw new InvalidOperationException($"Failed after {_maxAttempts} attempts");
            }
            public async Task ExecuteAsync(Func<Task> action, Func<Exception, bool>? shouldRetry = null,
                                            CancellationToken cancellationToken = default)
            {
                await ExecuteAsync(async () => { await action(); return true; }, shouldRetry, cancellationToken);
            }
            private int CalculateDelay(int attempt)
            {
                double delay = _baseDelayMs;
                if (_useExponentialBackoff)
                {
                    delay *= Math.Pow(2, attempt - 1);
                    delay = Math.Min(delay, 60000); // Cap at 60 seconds
                }
                if (_useJitter)
                {
                    var jitter = _random.NextDouble() * 0.3 * delay; // Up to 30% jitter
                    delay += jitter;
                }
                return (int)delay;
            }
            private static readonly Random _jitter = new Random();
            public int ComputeDelay(int attempt)
            {
                if (attempt <= 0) attempt = 1;
                var exp = Math.Pow(Multiplier, attempt - 1);
                var raw = (int)(BaseDelayMs * exp);
                raw = Math.Min(raw, MaxDelayMs);
                // جيتر عشوائي ± JitterRatio
                var jitter = (int)(raw * JitterRatio * (2.0 * _jitter.NextDouble() - 1.0));
                return Math.Max(100, raw + jitter);
            }
            public bool ShouldGiveUp(int attempt) => attempt > MaxAttempts;
            public override string ToString()
                => $"RetryPolicy[{Name}] max={MaxAttempts} base={BaseDelayMs}ms max={MaxDelayMs}ms x{Multiplier} jitter={JitterRatio:P0}";
            MaxAttempts = Math.Max(1, maxAttempts);
            BaseDelayMs = Math.Max(1, baseDelayMs);
            MaxDelayMs = Math.Max(BaseDelayMs, maxDelayMs);
            Multiplier = Math.Max(1.0, multiplier);
        }

    public partial public public class RetryPolicy
        {
            private readonly int _maxAttempts;
            private readonly int _baseDelayMs;
            private readonly bool _useExponentialBackoff;
            private readonly bool _useJitter;
            private readonly Random _random = new();
            public RetryPolicy(int maxAttempts = 10, int baseDelayMs = 5000,
            bool useExponentialBackoff = true, bool useJitter = true)
            {
            public async Task<T> ExecuteAsync<T>(Func<Task<T>> action, Func<Exception, bool>? shouldRetry = null,
            CancellationToken cancellationToken = default)
            {
            public async Task ExecuteAsync(Func<Task> action, Func<Exception, bool>? shouldRetry = null,
            CancellationToken cancellationToken = default)
            {
            private int CalculateDelay(int attempt)
            {
        }
    
    }
    public partial public class RetryPolicy
        {
            private readonly int _maxAttempts;
            private readonly int _baseDelayMs;
            private readonly bool _useExponentialBackoff;
            private readonly bool _useJitter;
            private readonly Random _random = new();
            public RetryPolicy(int maxAttempts = 10, int baseDelayMs = 5000,
            bool useExponentialBackoff = true, bool useJitter = true)
            {
            public async Task<T> ExecuteAsync<T>(Func<Task<T>> action, Func<Exception, bool>? shouldRetry = null,
            CancellationToken cancellationToken = default)
            {
            public async Task ExecuteAsync(Func<Task> action, Func<Exception, bool>? shouldRetry = null,
            CancellationToken cancellationToken = default)
            {
            private int CalculateDelay(int attempt)
            {
        }
    
    }
    public partial class RetryPolicy
        {
            private readonly int _maxAttempts;
    
    
            private readonly int _baseDelayMs;
    
    
            private readonly bool _useExponentialBackoff;
    
    
            private readonly bool _useJitter;
    
    
            private readonly Random _random = new();
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\src\EJLive.Core\Utils\RetryPolicy.cs
            _baseDelayMs = baseDelayMs;
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\src\EJLive.Core\Utils\RetryPolicy.cs
            _useExponentialBackoff = useExponentialBackoff;
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\src\EJLive.Core\Utils\RetryPolicy.cs
            _useJitter = useJitter;
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Utils\RetryPolicy.cs.v22_bak
            _baseDelayMs = baseDelayMs;
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Utils\RetryPolicy.cs.v22_bak
            _useExponentialBackoff = useExponentialBackoff;
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Utils\RetryPolicy.cs.v22_bak
            _useJitter = useJitter;
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Utils\RetryPolicy.cs.v17_bak
            _baseDelayMs = baseDelayMs;
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Utils\RetryPolicy.cs.v17_bak
            _useExponentialBackoff = useExponentialBackoff;
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Utils\RetryPolicy.cs.v17_bak
            _useJitter = useJitter;
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Utils\RetryPolicy.cs.before_unify
            _baseDelayMs = baseDelayMs;
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Utils\RetryPolicy.cs.before_unify
            _useExponentialBackoff = useExponentialBackoff;
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Utils\RetryPolicy.cs.before_unify
            _useJitter = useJitter;
    
    
            public RetryPolicy(int maxAttempts = 10, int baseDelayMs = 5000,
            bool useExponentialBackoff = true, bool useJitter = true)
            _maxAttempts = maxAttempts;
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\src\EJLive.Core\Utils\RetryPolicy.cs
            public RetryPolicy(int maxAttempts = 10, int baseDelayMs = 5000,
                                bool useExponentialBackoff = true, bool useJitter = true)
            {
                _maxAttempts = maxAttempts;
                _baseDelayMs = baseDelayMs;
                _useExponentialBackoff = useExponentialBackoff;
                _useJitter = useJitter;
            }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Utils\RetryPolicy.cs
            public RetryPolicy(int maxAttempts = 10, int baseDelayMs = 5000,
                                bool useExponentialBackoff = true, bool useJitter = true)
            {
                _maxAttempts = maxAttempts;
                _baseDelayMs = baseDelayMs;
                _useExponentialBackoff = useExponentialBackoff;
                _useJitter = useJitter;
            }
    
    
            public async Task<T> ExecuteAsync<T>(Func<Task<T>> action, Func<Exception, bool>? shouldRetry = null,
                                                    CancellationToken cancellationToken = default)
            {
                for (int attempt = 1; attempt <= _maxAttempts; attempt++)
                {
                    try
                    {
                        return await action();
                    }
                    catch (Exception ex) when (attempt < _maxAttempts && (shouldRetry?.Invoke(ex) ?? true))
                    {
                        var delay = CalculateDelay(attempt);
                        Debug.WriteLine($"[Retry] Attempt {attempt} failed: {ex.Message}. Retrying in {delay}ms...");
                        await Task.Delay(delay, cancellationToken);
                    }
                }
                throw new InvalidOperationException($"Failed after {_maxAttempts} attempts");
            }
    
    
            public async Task ExecuteAsync(Func<Task> action, Func<Exception, bool>? shouldRetry = null,
                                            CancellationToken cancellationToken = default)
            {
                await ExecuteAsync(async () => { await action(); return true; }, shouldRetry, cancellationToken);
            }
    
    
            private int CalculateDelay(int attempt)
            {
                double delay = _baseDelayMs;
                if (_useExponentialBackoff)
                {
                    delay *= Math.Pow(2, attempt - 1);
                    delay = Math.Min(delay, 60000); // Cap at 60 seconds
                }
                if (_useJitter)
                {
                    var jitter = _random.NextDouble() * 0.3 * delay; // Up to 30% jitter
                    delay += jitter;
                }
                return (int)delay;
            }
    
    
        }
    // Class: RetryPolicy (from 4 sources)
        public partial class RetryPolicy
        {
            // --- Constants & Fields ---
            private readonly int _maxAttempts;
    
            private readonly int _baseDelayMs;
    
            private readonly bool _useExponentialBackoff;
    
            private readonly bool _useJitter;
    
            private readonly Random _random = new();
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Utils\RetryPolicy.cs.v22_bak
            _baseDelayMs = baseDelayMs;
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Utils\RetryPolicy.cs.v22_bak
            _useExponentialBackoff = useExponentialBackoff;
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Utils\RetryPolicy.cs.v22_bak
            _useJitter = useJitter;
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Utils\RetryPolicy.cs.v17_bak
            _baseDelayMs = baseDelayMs;
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Utils\RetryPolicy.cs.v17_bak
            _useExponentialBackoff = useExponentialBackoff;
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Utils\RetryPolicy.cs.v17_bak
            _useJitter = useJitter;
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Utils\RetryPolicy.cs.before_unify
            _baseDelayMs = baseDelayMs;
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Utils\RetryPolicy.cs.before_unify
            _useExponentialBackoff = useExponentialBackoff;
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Utils\RetryPolicy.cs.before_unify
            _useJitter = useJitter;
    
    
            // --- Constructors ---
            public RetryPolicy(int maxAttempts = 10, int baseDelayMs = 5000,
            bool useExponentialBackoff = true, bool useJitter = true)
            _maxAttempts = maxAttempts;
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Utils\RetryPolicy.cs
                public RetryPolicy(int maxAttempts = 10, int baseDelayMs = 5000,
                                    bool useExponentialBackoff = true, bool useJitter = true)
                {
                    _maxAttempts = maxAttempts;
                    _baseDelayMs = baseDelayMs;
                    _useExponentialBackoff = useExponentialBackoff;
                    _useJitter = useJitter;
                }
    
    
            // --- Methods ---
                public async Task<T> ExecuteAsync<T>(Func<Task<T>> action, Func<Exception, bool>? shouldRetry = null,
                                                        CancellationToken cancellationToken = default)
                {
                    for (int attempt = 1; attempt <= _maxAttempts; attempt++)
                    {
                        try
                        {
                            return await action();
                        }
                        catch (Exception ex) when (attempt < _maxAttempts && (shouldRetry?.Invoke(ex) ?? true))
                        {
                            var delay = CalculateDelay(attempt);
                            Debug.WriteLine($"[Retry] Attempt {attempt} failed: {ex.Message}. Retrying in {delay}ms...");
                            await Task.Delay(delay, cancellationToken);
                        }
                    }
                    throw new InvalidOperationException($"Failed after {_maxAttempts} attempts");
                }
    
                public async Task ExecuteAsync(Func<Task> action, Func<Exception, bool>? shouldRetry = null,
                                                CancellationToken cancellationToken = default)
                {
                    await ExecuteAsync(async () => { await action(); return true; }, shouldRetry, cancellationToken);
                }
    
                private int CalculateDelay(int attempt)
                {
                    double delay = _baseDelayMs;
                    if (_useExponentialBackoff)
                    {
                        delay *= Math.Pow(2, attempt - 1);
                        delay = Math.Min(delay, 60000); // Cap at 60 seconds
                    }
                    if (_useJitter)
                    {
                        var jitter = _random.NextDouble() * 0.3 * delay; // Up to 30% jitter
                        delay += jitter;
                    }
                    return (int)delay;
                }
    
    
        }
    /// <summary>
    /// A configurable retry policy with exponential backoff and jitter support.
    /// Used for resilient network operations, file transfers, and database calls.
    /// </summary>
    public class RetryPolicy
    {
        private readonly int _maxAttempts;
        private readonly int _baseDelayMs;
        private readonly bool _useExponentialBackoff;
        private readonly bool _useJitter;
        private readonly Random _random = new();
    
        /// <summary>
        /// Creates a new retry policy with the specified parameters.
        /// </summary>
        /// <param name="maxAttempts">Maximum number of retry attempts (default 10).</param>
        /// <param name="baseDelayMs">Base delay in milliseconds between attempts (default 5000).</param>
        /// <param name="useExponentialBackoff">Whether to exponentially increase delay (default true).</param>
        /// <param name="useJitter">Whether to add random jitter to delay (default true).</param>
        public RetryPolicy(int maxAttempts = 10, int baseDelayMs = 5000,
                            bool useExponentialBackoff = true, bool useJitter = true)
        {
            _maxAttempts = maxAttempts;
            _baseDelayMs = baseDelayMs;
            _useExponentialBackoff = useExponentialBackoff;
            _useJitter = useJitter;
        }
    
        /// <summary>
        /// Executes an asynchronous action with automatic retry on failure.
        /// </summary>
        /// <typeparam name="T">The return type of the action.</typeparam>
        /// <param name="action">The async function to execute.</param>
        /// <param name="shouldRetry">Optional predicate to determine if retry is appropriate for a given exception.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The result of the action.</returns>
        /// <exception cref="InvalidOperationException">Thrown when all retry attempts are exhausted.</exception>
        public async Task<T> ExecuteAsync<T>(Func<Task<T>> action, Func<Exception, bool>? shouldRetry = null,
                                                CancellationToken cancellationToken = default)
        {
            for (int attempt = 1; attempt <= _maxAttempts; attempt++)
            {
                try
                {
                    return await action();
                }
                catch (Exception ex) when (attempt < _maxAttempts && (shouldRetry?.Invoke(ex) ?? true))
                {
                    var delay = CalculateDelay(attempt);
                    Debug.WriteLine($"[Retry] Attempt {attempt} failed: {ex.Message}. Retrying in {delay}ms...");
                    await Task.Delay(delay, cancellationToken);
                }
            }
            throw new InvalidOperationException($"Failed after {_maxAttempts} attempts");
        }
    
        /// <summary>
        /// Executes a void asynchronous action with automatic retry on failure.
        /// </summary>
        /// <param name="action">The async void function to execute.</param>
        /// <param name="shouldRetry">Optional predicate to determine if retry is appropriate.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        public async Task ExecuteAsync(Func<Task> action, Func<Exception, bool>? shouldRetry = null,
                                        CancellationToken cancellationToken = default)
        {
            await ExecuteAsync(async () => { await action(); return true; }, shouldRetry, cancellationToken);
        }
    
        /// <summary>
        /// Calculates the delay for a given attempt using exponential backoff and optional jitter.
        /// </summary>
        private int CalculateDelay(int attempt)
        {
            double delay = _baseDelayMs;
            if (_useExponentialBackoff)
            {
                delay *= Math.Pow(2, attempt - 1);
                delay = Math.Min(delay, 60000); // Cap at 60 seconds
            }
            if (_useJitter)
            {
                var jitter = _random.NextDouble() * 0.3 * delay; // Up to 30% jitter
                delay += jitter;
            }
            return (int)delay;
        }
    }
    public class RetryPolicy
    {
        private readonly int _maxAttempts;
        private readonly int _baseDelayMs;
        private readonly bool _useExponentialBackoff;
        private readonly bool _useJitter;
        private readonly Random _random = new();
    
        public RetryPolicy(int maxAttempts = 10, int baseDelayMs = 5000,
                            bool useExponentialBackoff = true, bool useJitter = true)
        {
            _maxAttempts = maxAttempts;
            _baseDelayMs = baseDelayMs;
            _useExponentialBackoff = useExponentialBackoff;
            _useJitter = useJitter;
        }
    
        public async Task<T> ExecuteAsync<T>(Func<Task<T>> action, Func<Exception, bool>? shouldRetry = null,
                                                CancellationToken cancellationToken = default)
        {
            for (int attempt = 1; attempt <= _maxAttempts; attempt++)
            {
                try
                {
                    return await action();
                }
                catch (Exception ex) when (attempt < _maxAttempts && (shouldRetry?.Invoke(ex) ?? true))
                {
                    var delay = CalculateDelay(attempt);
                    Debug.WriteLine($"[Retry] Attempt {attempt} failed: {ex.Message}. Retrying in {delay}ms...");
                    await Task.Delay(delay, cancellationToken);
                }
            }
            throw new InvalidOperationException($"Failed after {_maxAttempts} attempts");
        }
    
        public async Task ExecuteAsync(Func<Task> action, Func<Exception, bool>? shouldRetry = null,
                                        CancellationToken cancellationToken = default)
        {
            await ExecuteAsync(async () => { await action(); return true; }, shouldRetry, cancellationToken);
        }
    
        private int CalculateDelay(int attempt)
        {
            double delay = _baseDelayMs;
            if (_useExponentialBackoff)
            {
                delay *= Math.Pow(2, attempt - 1);
                delay = Math.Min(delay, 60000); // Cap at 60 seconds
            }
            if (_useJitter)
            {
                var jitter = _random.NextDouble() * 0.3 * delay; // Up to 30% jitter
                delay += jitter;
            }
            return (int)delay;
        }
    }
}

namespace EJLive.Shared
{
    public partial public public class RetryPolicy
        {
            public static readonly RetryPolicy LAN = new RetryPolicy
            {
            Name        = "LAN",
            MaxAttempts = 20,
            BaseDelayMs = 500,
            MaxDelayMs  = 30_000,
            Multiplier  = 1.5,
            JitterRatio = 0.1
            };
            public static readonly RetryPolicy ADSL = new RetryPolicy
            {
            Name        = "ADSL",
            MaxAttempts = 15,
            BaseDelayMs = 2_000,
            MaxDelayMs  = 120_000,
            Multiplier  = 2.0,
            JitterRatio = 0.25
            };
            public static readonly RetryPolicy GSM = new RetryPolicy
            {
            Name        = "GSM",
            MaxAttempts = 10,
            BaseDelayMs = 5_000,
            MaxDelayMs  = 300_000,   // 5 دقائق
            Multiplier  = 2.5,
            JitterRatio = 0.3
            };
            public static readonly RetryPolicy CDMA = new RetryPolicy
            {
            Name        = "CDMA",
            MaxAttempts = 12,
            BaseDelayMs = 3_000,
            MaxDelayMs  = 180_000,
            Multiplier  = 2.0,
            JitterRatio = 0.25
            };
            private static readonly Random _jitter = new Random();
            public RetryPolicy(string name, int maxAttempts, int baseDelayMs, int maxDelayMs, double multiplier)
            {
            public string Name        { get; set; }
            public int    MaxAttempts { get; set; }
            public int    BaseDelayMs { get; set; }
            public int    MaxDelayMs  { get; set; }
            public double Multiplier  { get; set; }
            public double JitterRatio { get; set; }
            public int ComputeDelay(int attempt)
            {
            public bool ShouldGiveUp(int attempt) =>
            public override string ToString()
            =>
            public static RetryPolicy ForNetwork(string? networkType)
            {
        }
    
    }
    public partial public class RetryPolicy
        {
            private static readonly Random _jitter = new Random();
            public static readonly RetryPolicy LAN = new RetryPolicy
            {
            Name        = "LAN",
            MaxAttempts = 20,
            BaseDelayMs = 500,
            MaxDelayMs  = 30_000,
            Multiplier  = 1.5,
            JitterRatio = 0.1
            };
            public static readonly RetryPolicy ADSL = new RetryPolicy
            {
            Name        = "ADSL",
            MaxAttempts = 15,
            BaseDelayMs = 2_000,
            MaxDelayMs  = 120_000,
            Multiplier  = 2.0,
            JitterRatio = 0.25
            };
            public static readonly RetryPolicy GSM = new RetryPolicy
            {
            Name        = "GSM",
            MaxAttempts = 10,
            BaseDelayMs = 5_000,
            MaxDelayMs  = 300_000,   // 5 دقائق
            Multiplier  = 2.5,
            JitterRatio = 0.3
            };
            public static readonly RetryPolicy CDMA = new RetryPolicy
            {
            Name        = "CDMA",
            MaxAttempts = 12,
            BaseDelayMs = 3_000,
            MaxDelayMs  = 180_000,
            Multiplier  = 2.0,
            JitterRatio = 0.25
            };
            private readonly int _maxAttempts;
            private readonly int _baseDelayMs;
            private readonly bool _useExponentialBackoff;
            private readonly bool _useJitter;
            private readonly Random _random = new();
            public RetryPolicy(int maxAttempts = 10, int baseDelayMs = 5000,
            bool useExponentialBackoff = true, bool useJitter = true)
            _maxAttempts = maxAttempts;
    
    
            // Variant from:
            public RetryPolicy(int maxAttempts = 10, int baseDelayMs = 5000,
            bool useExponentialBackoff = true, bool useJitter = true)
            {
            public RetryPolicy(int maxAttempts = 10, int baseDelayMs = 5000,
            bool useExponentialBackoff = true, bool useJitter = true)
            {
            public RetryPolicy(string name, int maxAttempts, int baseDelayMs, int maxDelayMs, double multiplier)
            Name = name;
    
    
            MaxAttempts = Math.Max(1, maxAttempts);
    
    
            BaseDelayMs = Math.Max(1, baseDelayMs);
    
    
            MaxDelayMs = Math.Max(BaseDelayMs, maxDelayMs);
    
    
            Multiplier = Math.Max(1.0, multiplier);
    
    
            private static readonly Random _jitter = new Random();
    
    
            public int ComputeDelay(int attempt)
            {
            public RetryPolicy(string name, int maxAttempts, int baseDelayMs, int maxDelayMs, double multiplier)
            Name = name;
    
    
            public RetryPolicy(int maxAttempts = 10, int baseDelayMs = 5000,
            bool useExponentialBackoff = true, bool useJitter = true)
            _maxAttempts = maxAttempts;
    
    
            // Variant from:
            public RetryPolicy(int maxAttempts = 10, int baseDelayMs = 5000,
            bool useExponentialBackoff = true, bool useJitter = true)
            {
            public RetryPolicy(int maxAttempts = 10, int baseDelayMs = 5000,
            bool useExponentialBackoff = true, bool useJitter = true)
            _maxAttempts = maxAttempts;
    
    
            // Variant from:
            public RetryPolicy(int maxAttempts = 10, int baseDelayMs = 5000,
            bool useExponentialBackoff = true, bool useJitter = true)
            _maxAttempts = maxAttempts;
    
    
            // Variant from:
            public RetryPolicy(int maxAttempts = 10, int baseDelayMs = 5000,
            bool useExponentialBackoff = true, bool useJitter = true)
            _maxAttempts = maxAttempts;
    
    
            public RetryPolicy(string name, int maxAttempts, int baseDelayMs, int maxDelayMs, double multiplier)
            Name = name;
    
    
            public async Task<T> ExecuteAsync<T>(Func<Task<T>> action, Func<Exception, bool>? shouldRetry = null,
            CancellationToken cancellationToken = default)
            {
            public RetryPolicy(string name, int maxAttempts, int baseDelayMs, int maxDelayMs, double multiplier)
            Name = name;
    
    
            // Variant:
            public RetryPolicy(int maxAttempts = 10, int baseDelayMs = 5000,
            bool useExponentialBackoff = true, bool useJitter = true)
            {
            public string Name        { get; set; }
            public int    MaxAttempts { get; set; }
            public int    BaseDelayMs { get; set; }
            public int    MaxDelayMs  { get; set; }
            public double Multiplier  { get; set; }
            public double JitterRatio { get; set; }
            public int ComputeDelay(int attempt)
            {
            public bool ShouldGiveUp(int attempt) =>
            public override string ToString()
            =>
            public async Task<T> ExecuteAsync<T>(Func<Task<T>> action, Func<Exception, bool>? shouldRetry = null,
            CancellationToken cancellationToken = default)
            {
            public async Task ExecuteAsync(Func<Task> action, Func<Exception, bool>? shouldRetry = null,
            CancellationToken cancellationToken = default)
            {
            private int CalculateDelay(int attempt)
            {
            public static RetryPolicy ForNetwork(string? networkType)
            {
        }
    
    }
    public partial public class RetryPolicy
        {
            public static readonly RetryPolicy LAN = new RetryPolicy
            {
            Name        = "LAN",
            MaxAttempts = 20,
            BaseDelayMs = 500,
            MaxDelayMs  = 30_000,
            Multiplier  = 1.5,
            JitterRatio = 0.1
            };
            public static readonly RetryPolicy ADSL = new RetryPolicy
            {
            Name        = "ADSL",
            MaxAttempts = 15,
            BaseDelayMs = 2_000,
            MaxDelayMs  = 120_000,
            Multiplier  = 2.0,
            JitterRatio = 0.25
            };
            public static readonly RetryPolicy GSM = new RetryPolicy
            {
            Name        = "GSM",
            MaxAttempts = 10,
            BaseDelayMs = 5_000,
            MaxDelayMs  = 300_000,   // 5 دقائق
            Multiplier  = 2.5,
            JitterRatio = 0.3
            };
            public static readonly RetryPolicy CDMA = new RetryPolicy
            {
            Name        = "CDMA",
            MaxAttempts = 12,
            BaseDelayMs = 3_000,
            MaxDelayMs  = 180_000,
            Multiplier  = 2.0,
            JitterRatio = 0.25
            };
            private static readonly Random _jitter = new Random();
            public RetryPolicy(string name, int maxAttempts, int baseDelayMs, int maxDelayMs, double multiplier)
            {
            public string Name        { get; set; }
            public int    MaxAttempts { get; set; }
            public int    BaseDelayMs { get; set; }
            public int    MaxDelayMs  { get; set; }
            public double Multiplier  { get; set; }
            public double JitterRatio { get; set; }
            public int ComputeDelay(int attempt)
            {
            public bool ShouldGiveUp(int attempt) =>
            public override string ToString()
            =>
            public static RetryPolicy ForNetwork(string? networkType)
            {
        }
    
    }
    public partial class RetryPolicy
        {
            public static readonly RetryPolicy LAN = new("LAN", 5, 1_000, 15_000, 1.8);
    
    
            public static readonly RetryPolicy ADSL = new("ADSL", 8, 2_000, 30_000, 2.0);
    
    
            public static readonly RetryPolicy GSM = new("GSM", 12, 5_000, 90_000, 2.2);
    
    
            public static readonly RetryPolicy CDMA = new("CDMA", 10, 4_000, 60_000, 2.0);
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\src\EJLive.Shared\RetryPolicy.cs
            public static readonly RetryPolicy LAN = new RetryPolicy
            {
                Name        = "LAN",
                MaxAttempts = 20,
                BaseDelayMs = 500,
                MaxDelayMs  = 30_000,
                Multiplier  = 1.5,
                JitterRatio = 0.1
            };
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\src\EJLive.Shared\RetryPolicy.cs
            public static readonly RetryPolicy ADSL = new RetryPolicy
            {
                Name        = "ADSL",
                MaxAttempts = 15,
                BaseDelayMs = 2_000,
                MaxDelayMs  = 120_000,
                Multiplier  = 2.0,
                JitterRatio = 0.25
            };
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\src\EJLive.Shared\RetryPolicy.cs
            public static readonly RetryPolicy GSM = new RetryPolicy
            {
                Name        = "GSM",
                MaxAttempts = 10,
                BaseDelayMs = 5_000,
                MaxDelayMs  = 300_000,   // 5 دقائق
                Multiplier  = 2.5,
                JitterRatio = 0.3
            };
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\src\EJLive.Shared\RetryPolicy.cs
            public static readonly RetryPolicy CDMA = new RetryPolicy
            {
                Name        = "CDMA",
                MaxAttempts = 12,
                BaseDelayMs = 3_000,
                MaxDelayMs  = 180_000,
                Multiplier  = 2.0,
                JitterRatio = 0.25
            };
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\UnifiedEJLiveProject\src\EJLive.Shared\RetryPolicy.cs
            public static readonly RetryPolicy LAN = new RetryPolicy
            {
                Name        = "LAN",
                MaxAttempts = 20,
                BaseDelayMs = 500,
                MaxDelayMs  = 30_000,
                Multiplier  = 1.5,
                JitterRatio = 0.1
            };
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\UnifiedEJLiveProject\src\EJLive.Shared\RetryPolicy.cs
            public static readonly RetryPolicy ADSL = new RetryPolicy
            {
                Name        = "ADSL",
                MaxAttempts = 15,
                BaseDelayMs = 2_000,
                MaxDelayMs  = 120_000,
                Multiplier  = 2.0,
                JitterRatio = 0.25
            };
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\UnifiedEJLiveProject\src\EJLive.Shared\RetryPolicy.cs
            public static readonly RetryPolicy GSM = new RetryPolicy
            {
                Name        = "GSM",
                MaxAttempts = 10,
                BaseDelayMs = 5_000,
                MaxDelayMs  = 300_000,   // 5 دقائق
                Multiplier  = 2.5,
                JitterRatio = 0.3
            };
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\UnifiedEJLiveProject\src\EJLive.Shared\RetryPolicy.cs
            public static readonly RetryPolicy CDMA = new RetryPolicy
            {
                Name        = "CDMA",
                MaxAttempts = 12,
                BaseDelayMs = 3_000,
                MaxDelayMs  = 180_000,
                Multiplier  = 2.0,
                JitterRatio = 0.25
            };
    
    
            public string Name        { get; set; }
    
    
            public int    MaxAttempts { get; set; }
    
    
            public int    BaseDelayMs { get; set; }
    
    
            public int    MaxDelayMs  { get; set; }
    
    
            public double Multiplier  { get; set; } = 2.0;
    
    
            public double JitterRatio { get; set; } = 0.2;    // ±20% jitter
    
    
            public RetryPolicy(string name, int maxAttempts, int baseDelayMs, int maxDelayMs, double multiplier)
            Name = name;
    
    
            MaxAttempts = Math.Max(1, maxAttempts);
    
    
            BaseDelayMs = Math.Max(1, baseDelayMs);
    
    
            MaxDelayMs = Math.Max(BaseDelayMs, maxDelayMs);
    
    
            Multiplier = Math.Max(1.0, multiplier);
    
    
            private static readonly Random _jitter = new Random();
    
    
            public int ComputeDelay(int attempt)
            {
                if (attempt <= 0) attempt = 1;
                var exp = Math.Pow(Multiplier, attempt - 1);
                var raw = (int)(BaseDelayMs * exp);
                raw = Math.Min(raw, MaxDelayMs);
    
                // جيتر عشوائي ± JitterRatio
                var jitter = (int)(raw * JitterRatio * (2.0 * _jitter.NextDouble() - 1.0));
                return Math.Max(100, raw + jitter);
            }
    
    
            public bool ShouldGiveUp(int attempt) => attempt > MaxAttempts;
    
    
            public override string ToString()
                => $"RetryPolicy[{Name}] max={MaxAttempts} base={BaseDelayMs}ms max={MaxDelayMs}ms x{Multiplier} jitter={JitterRatio:P0}";
    
    
        }
    public partial class RetryPolicy
        {
            public static readonly RetryPolicy LAN = new RetryPolicy
            {
                Name        = "LAN",
                MaxAttempts = 20,
                BaseDelayMs = 500,
                MaxDelayMs  = 30_000,
                Multiplier  = 1.5,
                JitterRatio = 0.1
            };
    
    
            public static readonly RetryPolicy ADSL = new RetryPolicy
            {
                Name        = "ADSL",
                MaxAttempts = 15,
                BaseDelayMs = 2_000,
                MaxDelayMs  = 120_000,
                Multiplier  = 2.0,
                JitterRatio = 0.25
            };
    
    
            public static readonly RetryPolicy GSM = new RetryPolicy
            {
                Name        = "GSM",
                MaxAttempts = 10,
                BaseDelayMs = 5_000,
                MaxDelayMs  = 300_000,   // 5 دقائق
                Multiplier  = 2.5,
                JitterRatio = 0.3
            };
    
    
            public static readonly RetryPolicy CDMA = new RetryPolicy
            {
                Name        = "CDMA",
                MaxAttempts = 12,
                BaseDelayMs = 3_000,
                MaxDelayMs  = 180_000,
                Multiplier  = 2.0,
                JitterRatio = 0.25
            };
    
    
            public string Name        { get; set; }
    
    
            public int    MaxAttempts { get; set; }
    
    
            public int    BaseDelayMs { get; set; }
    
    
            public int    MaxDelayMs  { get; set; }
    
    
            public double Multiplier  { get; set; } = 2.0;
    
    
            public double JitterRatio { get; set; } = 0.2;    // ±20% jitter
    
    
            private static readonly Random _jitter = new Random();
    
    
            public int ComputeDelay(int attempt)
            {
                if (attempt <= 0) attempt = 1;
                var exp = Math.Pow(Multiplier, attempt - 1);
                var raw = (int)(BaseDelayMs * exp);
                raw = Math.Min(raw, MaxDelayMs);
    
                // جيتر عشوائي ± JitterRatio
                var jitter = (int)(raw * JitterRatio * (2.0 * _jitter.NextDouble() - 1.0));
                return Math.Max(100, raw + jitter);
            }
    
    
            public bool ShouldGiveUp(int attempt) => attempt > MaxAttempts;
    
    
            public override string ToString()
                => $"RetryPolicy[{Name}] max={MaxAttempts} base={BaseDelayMs}ms max={MaxDelayMs}ms x{Multiplier} jitter={JitterRatio:P0}";
    
    
        }
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

    // Class: RetryPolicy (from 9 sources)
        public sealed partial class RetryPolicy
        {
            // --- Constants & Fields ---
            public static readonly RetryPolicy LAN = new("LAN", 5, 1_000, 15_000, 1.8);
    
            public static readonly RetryPolicy ADSL = new("ADSL", 8, 2_000, 30_000, 2.0);
    
            public static readonly RetryPolicy GSM = new("GSM", 12, 5_000, 90_000, 2.2);
    
            public static readonly RetryPolicy CDMA = new("CDMA", 10, 4_000, 60_000, 2.0);
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\UnifiedEJLiveProject\src\EJLive.Shared\RetryPolicy.cs
                    public static readonly RetryPolicy LAN = new RetryPolicy
                    {
                        Name        = "LAN",
                        MaxAttempts = 20,
                        BaseDelayMs = 500,
                        MaxDelayMs  = 30_000,
                        Multiplier  = 1.5,
                        JitterRatio = 0.1
                    };
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\UnifiedEJLiveProject\src\EJLive.Shared\RetryPolicy.cs
                    public static readonly RetryPolicy ADSL = new RetryPolicy
                    {
                        Name        = "ADSL",
                        MaxAttempts = 15,
                        BaseDelayMs = 2_000,
                        MaxDelayMs  = 120_000,
                        Multiplier  = 2.0,
                        JitterRatio = 0.25
                    };
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\UnifiedEJLiveProject\src\EJLive.Shared\RetryPolicy.cs
                    public static readonly RetryPolicy GSM = new RetryPolicy
                    {
                        Name        = "GSM",
                        MaxAttempts = 10,
                        BaseDelayMs = 5_000,
                        MaxDelayMs  = 300_000,   // 5 دقائق
                        Multiplier  = 2.5,
                        JitterRatio = 0.3
                    };
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\UnifiedEJLiveProject\src\EJLive.Shared\RetryPolicy.cs
                    public static readonly RetryPolicy CDMA = new RetryPolicy
                    {
                        Name        = "CDMA",
                        MaxAttempts = 12,
                        BaseDelayMs = 3_000,
                        MaxDelayMs  = 180_000,
                        Multiplier  = 2.0,
                        JitterRatio = 0.25
                    };
    
    
            // --- Properties ---
                    public string Name        { get; set; }
    
                    public int    MaxAttempts { get; set; }
    
                    public int    BaseDelayMs { get; set; }
    
                    public int    MaxDelayMs  { get; set; }
    
                    public double Multiplier  { get; set; } = 2.0;
    
                    public double JitterRatio { get; set; } = 0.2;    // ±20% jitter
    
    
            // --- Constructors ---
            public RetryPolicy(string name, int maxAttempts, int baseDelayMs, int maxDelayMs, double multiplier)
            Name = name;
    
    
            // --- Methods ---
            MaxAttempts = Math.Max(1, maxAttempts);
    
            BaseDelayMs = Math.Max(1, baseDelayMs);
    
            MaxDelayMs = Math.Max(BaseDelayMs, maxDelayMs);
    
            Multiplier = Math.Max(1.0, multiplier);
    
                    private static readonly Random _jitter = new Random();
    
                    public int ComputeDelay(int attempt)
                    {
                        if (attempt <= 0) attempt = 1;
                        var exp = Math.Pow(Multiplier, attempt - 1);
                        var raw = (int)(BaseDelayMs * exp);
                        raw = Math.Min(raw, MaxDelayMs);
    
                        // جيتر عشوائي ± JitterRatio
                        var jitter = (int)(raw * JitterRatio * (2.0 * _jitter.NextDouble() - 1.0));
                        return Math.Max(100, raw + jitter);
                    }
    
                    public bool ShouldGiveUp(int attempt) => attempt > MaxAttempts;
    
                    public override string ToString()
                        => $"RetryPolicy[{Name}] max={MaxAttempts} base={BaseDelayMs}ms max={MaxDelayMs}ms x{Multiplier} jitter={JitterRatio:P0}";
    
    
        }
    // Class: RetryPolicy (from 2 sources)
        public partial class RetryPolicy
        {
            // --- Constants & Fields ---
                    public static readonly RetryPolicy LAN = new RetryPolicy
                    {
                        Name        = "LAN",
                        MaxAttempts = 20,
                        BaseDelayMs = 500,
                        MaxDelayMs  = 30_000,
                        Multiplier  = 1.5,
                        JitterRatio = 0.1
                    };
    
                    public static readonly RetryPolicy ADSL = new RetryPolicy
                    {
                        Name        = "ADSL",
                        MaxAttempts = 15,
                        BaseDelayMs = 2_000,
                        MaxDelayMs  = 120_000,
                        Multiplier  = 2.0,
                        JitterRatio = 0.25
                    };
    
                    public static readonly RetryPolicy GSM = new RetryPolicy
                    {
                        Name        = "GSM",
                        MaxAttempts = 10,
                        BaseDelayMs = 5_000,
                        MaxDelayMs  = 300_000,   // 5 دقائق
                        Multiplier  = 2.5,
                        JitterRatio = 0.3
                    };
    
                    public static readonly RetryPolicy CDMA = new RetryPolicy
                    {
                        Name        = "CDMA",
                        MaxAttempts = 12,
                        BaseDelayMs = 3_000,
                        MaxDelayMs  = 180_000,
                        Multiplier  = 2.0,
                        JitterRatio = 0.25
                    };
    
    
            // --- Properties ---
                    public string Name        { get; set; }
    
                    public int    MaxAttempts { get; set; }
    
                    public int    BaseDelayMs { get; set; }
    
                    public int    MaxDelayMs  { get; set; }
    
                    public double Multiplier  { get; set; } = 2.0;
    
                    public double JitterRatio { get; set; } = 0.2;    // ±20% jitter
    
    
            // --- Methods ---
                    private static readonly Random _jitter = new Random();
    
                    public int ComputeDelay(int attempt)
                    {
                        if (attempt <= 0) attempt = 1;
                        var exp = Math.Pow(Multiplier, attempt - 1);
                        var raw = (int)(BaseDelayMs * exp);
                        raw = Math.Min(raw, MaxDelayMs);
    
                        // جيتر عشوائي ± JitterRatio
                        var jitter = (int)(raw * JitterRatio * (2.0 * _jitter.NextDouble() - 1.0));
                        return Math.Max(100, raw + jitter);
                    }
    
                    public bool ShouldGiveUp(int attempt) => attempt > MaxAttempts;
    
                    public override string ToString()
                        => $"RetryPolicy[{Name}] max={MaxAttempts} base={BaseDelayMs}ms max={MaxDelayMs}ms x{Multiplier} jitter={JitterRatio:P0}";
    
    
        }
    // Class: RetryPolicy (from 9 sources)
        public partial class RetryPolicy
        {
            // --- Constants & Fields ---
                            public static readonly RetryPolicy LAN = new RetryPolicy
                            {
                                Name        = "LAN",
                                MaxAttempts = 20,
                                BaseDelayMs = 500,
                                MaxDelayMs  = 30_000,
                                Multiplier  = 1.5,
                                JitterRatio = 0.1
                            };
    
                            public static readonly RetryPolicy ADSL = new RetryPolicy
                            {
                                Name        = "ADSL",
                                MaxAttempts = 15,
                                BaseDelayMs = 2_000,
                                MaxDelayMs  = 120_000,
                                Multiplier  = 2.0,
                                JitterRatio = 0.25
                            };
    
                            public static readonly RetryPolicy GSM = new RetryPolicy
                            {
                                Name        = "GSM",
                                MaxAttempts = 10,
                                BaseDelayMs = 5_000,
                                MaxDelayMs  = 300_000,   // 5 دقائق
                                Multiplier  = 2.5,
                                JitterRatio = 0.3
                            };
    
                            public static readonly RetryPolicy CDMA = new RetryPolicy
                            {
                                Name        = "CDMA",
                                MaxAttempts = 12,
                                BaseDelayMs = 3_000,
                                MaxDelayMs  = 180_000,
                                Multiplier  = 2.0,
                                JitterRatio = 0.25
                            };
    
    
            // --- Properties ---
                            public string Name        { get; set; }
    
                            public int    MaxAttempts { get; set; }
    
                            public int    BaseDelayMs { get; set; }
    
                            public int    MaxDelayMs  { get; set; }
    
                            public double Multiplier  { get; set; } = 2.0;
    
                            public double JitterRatio { get; set; } = 0.2;    // ±20% jitter
    
    
            // --- Methods ---
                            private static readonly Random _jitter = new Random();
    
                            public int ComputeDelay(int attempt)
                            {
                                if (attempt <= 0) attempt = 1;
                                var exp = Math.Pow(Multiplier, attempt - 1);
                                var raw = (int)(BaseDelayMs * exp);
                                raw = Math.Min(raw, MaxDelayMs);
    
                                // جيتر عشوائي ± JitterRatio
                                var jitter = (int)(raw * JitterRatio * (2.0 * _jitter.NextDouble() - 1.0));
                                return Math.Max(100, raw + jitter);
                            }
    
                            public bool ShouldGiveUp(int attempt) => attempt > MaxAttempts;
    
                            public override string ToString()
                                => $"RetryPolicy[{Name}] max={MaxAttempts} base={BaseDelayMs}ms max={MaxDelayMs}ms x{Multiplier} jitter={JitterRatio:P0}";
    
    
        }
    // Class: RetryPolicy (from 5 sources)
        public partial class RetryPolicy
        {
            // --- Constants & Fields ---
                    public static readonly RetryPolicy LAN = new RetryPolicy
                    {
                        Name        = "LAN",
                        MaxAttempts = 20,
                        BaseDelayMs = 500,
                        MaxDelayMs  = 30_000,
                        Multiplier  = 1.5,
                        JitterRatio = 0.1
                    };
    
                    public static readonly RetryPolicy ADSL = new RetryPolicy
                    {
                        Name        = "ADSL",
                        MaxAttempts = 15,
                        BaseDelayMs = 2_000,
                        MaxDelayMs  = 120_000,
                        Multiplier  = 2.0,
                        JitterRatio = 0.25
                    };
    
                    public static readonly RetryPolicy GSM = new RetryPolicy
                    {
                        Name        = "GSM",
                        MaxAttempts = 10,
                        BaseDelayMs = 5_000,
                        MaxDelayMs  = 300_000,   // 5 دقائق
                        Multiplier  = 2.5,
                        JitterRatio = 0.3
                    };
    
                    public static readonly RetryPolicy CDMA = new RetryPolicy
                    {
                        Name        = "CDMA",
                        MaxAttempts = 12,
                        BaseDelayMs = 3_000,
                        MaxDelayMs  = 180_000,
                        Multiplier  = 2.0,
                        JitterRatio = 0.25
                    };
    
    
            // --- Properties ---
                    public string Name        { get; set; }
    
                    public int    MaxAttempts { get; set; }
    
                    public int    BaseDelayMs { get; set; }
    
                    public int    MaxDelayMs  { get; set; }
    
                    public double Multiplier  { get; set; } = 2.0;
    
                    public double JitterRatio { get; set; } = 0.2;    // ±20% jitter
    
    
            // --- Methods ---
                    private static readonly Random _jitter = new Random();
    
                    public int ComputeDelay(int attempt)
                    {
                        if (attempt <= 0) attempt = 1;
                        var exp = Math.Pow(Multiplier, attempt - 1);
                        var raw = (int)(BaseDelayMs * exp);
                        raw = Math.Min(raw, MaxDelayMs);
    
                        // جيتر عشوائي ± JitterRatio
                        var jitter = (int)(raw * JitterRatio * (2.0 * _jitter.NextDouble() - 1.0));
                        return Math.Max(100, raw + jitter);
                    }
    
                    public bool ShouldGiveUp(int attempt) => attempt > MaxAttempts;
    
                    public override string ToString()
                        => $"RetryPolicy[{Name}] max={MaxAttempts} base={BaseDelayMs}ms max={MaxDelayMs}ms x{Multiplier} jitter={JitterRatio:P0}";
    
    
        }
    /// <summary>
        /// سياسة إعادة المحاولة الكاملة — Exponential Backoff with Jitter (L-05)
        /// 4 سياسات جاهزة: LAN | ADSL | GSM | CDMA
        /// يطبق: أسي مع ضوضاء عشوائية لتجنب "قطيع الرعد"
        /// </summary>
        public class RetryPolicy
        {
            public string Name        { get; set; }
            public int    MaxAttempts { get; set; }
            public int    BaseDelayMs { get; set; }
            public int    MaxDelayMs  { get; set; }
            public double Multiplier  { get; set; } = 2.0;
            public double JitterRatio { get; set; } = 0.2;    // ±20% jitter
    
            private static readonly Random _jitter = new Random();
    
            // ==========================================
            // سياسات جاهزة
            // ==========================================
    
            /// <summary>شبكة LAN داخلية — استجابة سريعة</summary>
            public static readonly RetryPolicy LAN = new RetryPolicy
            {
                Name        = "LAN",
                MaxAttempts = 20,
                BaseDelayMs = 500,
                MaxDelayMs  = 30_000,
                Multiplier  = 1.5,
                JitterRatio = 0.1
            };
    
            /// <summary>خط ADSL — تأخير معتدل</summary>
            public static readonly RetryPolicy ADSL = new RetryPolicy
            {
                Name        = "ADSL",
                MaxAttempts = 15,
                BaseDelayMs = 2_000,
                MaxDelayMs  = 120_000,
                Multiplier  = 2.0,
                JitterRatio = 0.25
            };
    
            /// <summary>GSM/GPRS — شبكة بطيئة وغير مستقرة</summary>
            public static readonly RetryPolicy GSM = new RetryPolicy
            {
                Name        = "GSM",
                MaxAttempts = 10,
                BaseDelayMs = 5_000,
                MaxDelayMs  = 300_000,   // 5 دقائق
                Multiplier  = 2.5,
                JitterRatio = 0.3
            };
    
            /// <summary>CDMA/3G — متوسط بين GSM وADSL</summary>
            public static readonly RetryPolicy CDMA = new RetryPolicy
            {
                Name        = "CDMA",
                MaxAttempts = 12,
                BaseDelayMs = 3_000,
                MaxDelayMs  = 180_000,
                Multiplier  = 2.0,
                JitterRatio = 0.25
            };
    
            // ==========================================
            // حساب التأخير
            // ==========================================
    
            /// <summary>
            /// يحسب تأخير المحاولة رقم <paramref name="attempt"/> (يبدأ من 1)
            /// يستخدم: base * multiplier^(attempt-1) + jitter
            /// </summary>
            public int ComputeDelay(int attempt)
            {
                if (attempt <= 0) attempt = 1;
                var exp = Math.Pow(Multiplier, attempt - 1);
                var raw = (int)(BaseDelayMs * exp);
                raw = Math.Min(raw, MaxDelayMs);
    
                // جيتر عشوائي ± JitterRatio
                var jitter = (int)(raw * JitterRatio * (2.0 * _jitter.NextDouble() - 1.0));
                return Math.Max(100, raw + jitter);
            }
    
            /// <summary>هل وصلنا للحد الأقصى؟</summary>
            public bool ShouldGiveUp(int attempt) => attempt > MaxAttempts;
    
            /// <summary>خلاصة السياسة للتسجيل</summary>
            public override string ToString()
                => $"RetryPolicy[{Name}] max={MaxAttempts} base={BaseDelayMs}ms max={MaxDelayMs}ms x{Multiplier} jitter={JitterRatio:P0}";
        }
}
