using System.Reflection;

namespace EJLive.Client.Service.Compatibility;

/// <summary>
/// Compatibility adapter around EJLive.Core.Engine.JournalOutbox.
/// It allows the service layer to build on old and new outbox APIs without
/// deleting existing runtime code or forcing a blind replacement.
/// </summary>
internal sealed class JournalOutboxAdapter : IDisposable
{
    private readonly object _inner;

    public JournalOutboxAdapter(object inner)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
    }

    public object Inner => _inner;

    public int PendingCount =>
        ReflectionSafe.GetPropertyValue<int>(_inner, "PendingCount", "Count");

    public void EnqueueFile(string atmId, string fileName, byte[] data, string checksum)
    {
        // Newer shape: Enqueue(fileName, data, checksum)
        if (TryInvoke("Enqueue", fileName, data, checksum))
            return;

        // Older/alternate shape: Enqueue(atmId, fileName, data, offset, checksum)
        if (TryInvoke("Enqueue", atmId, fileName, data, 0L, checksum))
            return;

        // Alternate offset int shape
        if (TryInvoke("Enqueue", atmId, fileName, data, 0, checksum))
            return;

        throw new MissingMethodException(
            _inner.GetType().FullName,
            "Supported Enqueue overloads were not found.");
    }

    public void EnqueueForceSyncMarker(string atmId)
    {
        var marker = Array.Empty<byte>();

        if (TryInvoke("Enqueue", atmId, "_forcesync.marker", marker, 0L, string.Empty))
            return;

        if (TryInvoke("Enqueue", "_forcesync.marker", marker, string.Empty))
            return;
    }

    public void EnqueuePendingForImmediateDispatch()
    {
        TryInvoke("EnqueuePendingForImmediateDispatch");
    }

    private bool TryInvoke(string methodName, params object?[] args)
    {
        var methods = _inner.GetType()
            .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .Where(m => m.Name == methodName)
            .ToArray();

        foreach (var method in methods)
        {
            var parameters = method.GetParameters();
            if (parameters.Length != args.Length)
                continue;

            var compatible = true;

            for (var i = 0; i < parameters.Length; i++)
            {
                if (args[i] == null)
                    continue;

                var parameterType = parameters[i].ParameterType;
                var argType = args[i]!.GetType();

                if (parameterType.IsAssignableFrom(argType))
                    continue;

                try
                {
                    _ = Convert.ChangeType(args[i], parameterType);
                }
                catch
                {
                    compatible = false;
                    break;
                }
            }

            if (!compatible)
                continue;

            var normalized = new object?[args.Length];
            for (var i = 0; i < args.Length; i++)
            {
                var parameterType = parameters[i].ParameterType;
                normalized[i] = args[i] == null || parameterType.IsAssignableFrom(args[i]!.GetType())
                    ? args[i]
                    : Convert.ChangeType(args[i], parameterType);
            }

            method.Invoke(_inner, normalized);
            return true;
        }

        return false;
    }

    public void Dispose()
    {
        if (_inner is IDisposable disposable)
            disposable.Dispose();
    }
}
