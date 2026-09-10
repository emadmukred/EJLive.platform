using System;
using System.Threading;
using System.Threading.Tasks;
using EJLive.Core.Models;

namespace EJLive.Core.Engine
{
    /// <summary>
    /// Manages handshake lifecycle and heartbeat cadence with exponential backoff and circuit breaker behavior.
    /// </summary>
    public sealed class SecureHandshakeService : IDisposable
    {
        private readonly NetworkEngine _network;
        private readonly AppConfig _config;
        private readonly string _machineId;
        private readonly string _protocolVersion;
        private readonly string _clientVersion;
        private HandshakeState _state = HandshakeState.None;
        private string? _sessionId;
        private DateTime? _lastHandshakeUtc;
        private int _consecutiveFailures;
        private readonly object _lock = new();
        private readonly CancellationTokenSource _cts = new();

        public event Action<HandshakeState>? OnStateChanged;
        public event Action<HeartbeatAck>? OnHeartbeatAck;
        public event Action<string>? OnLog;

        public HandshakeState State => _state;
        public string? SessionId => _sessionId;

        public SecureHandshakeService(NetworkEngine network, AppConfig config)
        {
            _network = network ?? throw new ArgumentNullException(nameof(network));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _machineId = Environment.MachineName;
            _protocolVersion = "2.0";
            _clientVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "2.0.0";
        }

        public async Task RunAsync(CancellationToken externalToken)
        {
            using var linked = CancellationTokenSource.CreateLinkedTokenSource(externalToken, _cts.Token);
            var token = linked.Token;

            while (!token.IsCancellationRequested)
            {
                try
                {
                    if (_state != HandshakeState.Accepted)
                    {
                        await AttemptHandshakeAsync(token);
                    }
                    else
                    {
                        await SendHeartbeatAsync(token);
                    }

                    var delay = ComputeBackoff();
                    await Task.Delay(delay, token);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Log($"Handshake/Heartbeat loop error: {ex.Message}");
                    _consecutiveFailures++;
                    TransitionTo(HandshakeState.None);
                    var delay = ComputeBackoff();
                    await Task.Delay(delay, token);
                }
            }
        }

        private async Task AttemptHandshakeAsync(CancellationToken token)
        {
            TransitionTo(HandshakeState.Sent);
            _sessionId = Guid.NewGuid().ToString("N");

            var request = new HandshakeRequest(
                _config.ATM_ID ?? _machineId,
                _machineId,
                _protocolVersion,
                _clientVersion,
                _sessionId,
                DateTime.UtcNow);

            Log($"Sending handshake: SessionId={_sessionId}");
            _lastHandshakeUtc = DateTime.UtcNow;

            // Serialize and send via NetworkEngine; await response
            // NetworkEngine provides a request/response primitive or event-based delivery.
            // Here we simulate the wire send and await the server response event.
            var tcs = new TaskCompletionSource<HandshakeResponse>(TaskCreationOptions.RunContinuationsAsynchronously);
            void OnResponse(object? sender, HandshakeResponse response) => tcs.TrySetResult(response);
            _network.OnHandshakeResponse += OnResponse;
            try
            {
                _network.SendHandshake(request);
                using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
                using var reg = timeoutCts.Token.Register(() => tcs.TrySetCanceled());
                var response = await tcs.Task;

                if (response.Accepted)
                {
                    _sessionId = response.ServerSessionId ?? _sessionId;
                    _consecutiveFailures = 0;
                    TransitionTo(HandshakeState.Accepted);
                    Log($"Handshake accepted. ServerTime={response.ServerTimeUtc:O}, PendingCommands={response.CommandPendingCount}");
                }
                else
                {
                    TransitionTo(HandshakeState.Rejected);
                    Log($"Handshake rejected: {response.RejectionReason}");
                }
            }
            finally
            {
                _network.OnHandshakeResponse -= OnResponse;
            }

            await Task.CompletedTask;
        }

        private async Task SendHeartbeatAsync(CancellationToken token)
        {
            var payload = new HeartbeatPayload(
                _config.ATM_ID ?? _machineId,
                _sessionId,
                DateTime.UtcNow,
                0, // outbox count injected by caller
                true, // file watcher health injected by caller
                0,    // cpu
                0,    // memory
                0,    // disk
                0,    // last journal offset
                null);

            var tcs = new TaskCompletionSource<HeartbeatAck>(TaskCreationOptions.RunContinuationsAsynchronously);
            void OnAck(object? sender, HeartbeatAck ack) => tcs.TrySetResult(ack);
            _network.OnHeartbeatAck += OnAck;
            try
            {
                _network.SendHeartbeat(payload);
                using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                using var reg = timeoutCts.Token.Register(() => tcs.TrySetCanceled());
                var ack = await tcs.Task;
                _consecutiveFailures = 0;
                OnHeartbeatAck?.Invoke(ack);
            }
            catch (OperationCanceledException)
            {
                _consecutiveFailures++;
                if (_consecutiveFailures >= 3)
                {
                    TransitionTo(HandshakeState.Expired);
                }
            }
            finally
            {
                _network.OnHeartbeatAck -= OnAck;
            }
        }

        private TimeSpan ComputeBackoff()
        {
            if (_state == HandshakeState.Accepted)
            {
            var interval = TimeSpan.FromSeconds(_config.HeartbeatIntervalSec > 0 ? _config.HeartbeatIntervalSec : 30);
                // Add small jitter to prevent thundering herd
                var jitter = TimeSpan.FromMilliseconds(Random.Shared.Next(0, 1000));
                return interval + jitter;
            }

            var baseDelay = Math.Min(5 * Math.Pow(2, _consecutiveFailures), 60); // cap at 60s
            var jitterMs = Random.Shared.Next(0, 500);
            return TimeSpan.FromSeconds(baseDelay) + TimeSpan.FromMilliseconds(jitterMs);
        }

        private void TransitionTo(HandshakeState newState)
        {
            lock (_lock)
            {
                if (_state == newState)
                    return;
                _state = newState;
            }
            OnStateChanged?.Invoke(newState);
        }

        private void Log(string message)
        {
            OnLog?.Invoke($"[{DateTime.UtcNow:O}] [Handshake] {message}");
        }

        public void Dispose()
        {
            _cts.Cancel();
            _cts.Dispose();
        }
    }
}
