using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO.Pipes;
using System.Text;

namespace EJLive.Client.WinForms.Services;

public static class SessionCompanionIpc
{
    public const string PipeName = "EJLive.SessionCompanion.v1";
    public const string CaptureCommand = "CAPTURE_SCREENSHOT";
    public const string PingCommand = "PING";
}

/// <summary>
/// Runs inside user session companion process and provides interactive actions via IPC.
/// </summary>
public sealed class SessionCompanionIpcServer : IDisposable
{
    private readonly CancellationTokenSource _cts = new();
    private readonly Action<string>? _log;
    private Task? _loop;

    public SessionCompanionIpcServer(Action<string>? log = null)
    {
        _log = log;
    }

    public void Start()
    {
        if (_loop != null)
            return;

        _loop = Task.Run(() => RunAsync(_cts.Token));
    }

    private async Task RunAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                using var pipe = new NamedPipeServerStream(
                    SessionCompanionIpc.PipeName,
                    PipeDirection.InOut,
                    1,
                    PipeTransmissionMode.Byte,
                    PipeOptions.Asynchronous);

                await pipe.WaitForConnectionAsync(token).ConfigureAwait(false);

                using var reader = new StreamReader(pipe, Encoding.UTF8, false, 4096, leaveOpen: true);
                using var writer = new StreamWriter(pipe, Encoding.UTF8, 4096, leaveOpen: true) { AutoFlush = true };

                var command = (await reader.ReadLineAsync().ConfigureAwait(false) ?? string.Empty).Trim();
                if (command.Length == 0)
                {
                    await writer.WriteLineAsync("ERR|EMPTY_COMMAND").ConfigureAwait(false);
                    continue;
                }

                if (string.Equals(command, SessionCompanionIpc.PingCommand, StringComparison.OrdinalIgnoreCase))
                {
                    var sessionId = -1;
                    try
                    {
                        using var process = Process.GetCurrentProcess();
                        sessionId = process.SessionId;
                    }
                    catch
                    {
                    }

                    await writer.WriteLineAsync($"OK|PONG|Session={sessionId}|Interactive={Environment.UserInteractive}").ConfigureAwait(false);
                    continue;
                }

                if (string.Equals(command, SessionCompanionIpc.CaptureCommand, StringComparison.OrdinalIgnoreCase))
                {
                    var jpeg = TryCaptureJpeg();
                    if (jpeg == null || jpeg.Length == 0)
                    {
                        await writer.WriteLineAsync("ERR|CAPTURE_FAILED").ConfigureAwait(false);
                        continue;
                    }

                    await writer.WriteLineAsync("OK|" + Convert.ToBase64String(jpeg)).ConfigureAwait(false);
                    continue;
                }

                await writer.WriteLineAsync("ERR|UNKNOWN_COMMAND").ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _log?.Invoke("SessionCompanion IPC server error: " + ex.Message);
                await Task.Delay(TimeSpan.FromSeconds(1), token).ConfigureAwait(false);
            }
        }
    }

    private static byte[]? TryCaptureJpeg()
    {
        try
        {
            var bounds = Screen.PrimaryScreen?.Bounds ?? new Rectangle(0, 0, 1024, 768);
            using var bitmap = new Bitmap(bounds.Width, bounds.Height);
            using (var graphics = Graphics.FromImage(bitmap))
                graphics.CopyFromScreen(bounds.Location, Point.Empty, bounds.Size);

            using var stream = new MemoryStream();
            bitmap.Save(stream, ImageFormat.Jpeg);
            return stream.ToArray();
        }
        catch
        {
            return null;
        }
    }

    public void Dispose()
    {
        _cts.Cancel();
        try
        {
            _loop?.Wait(TimeSpan.FromSeconds(2));
        }
        catch
        {
        }
        _cts.Dispose();
    }
}

public static class SessionCompanionIpcClient
{
    public static bool IsSessionZeroLikely()
    {
        try
        {
            using var process = Process.GetCurrentProcess();
            return process.SessionId == 0 || !Environment.UserInteractive;
        }
        catch
        {
            return !Environment.UserInteractive;
        }
    }

    public static bool TryCaptureScreenshot(out byte[] data, out string detail, int timeoutMs = 3000)
    {
        data = Array.Empty<byte>();
        detail = string.Empty;

        try
        {
            using var client = new NamedPipeClientStream(
                ".",
                SessionCompanionIpc.PipeName,
                PipeDirection.InOut,
                PipeOptions.None);

            client.Connect(timeoutMs);
            using var writer = new StreamWriter(client, Encoding.UTF8, 4096, leaveOpen: true) { AutoFlush = true };
            using var reader = new StreamReader(client, Encoding.UTF8, false, 4096, leaveOpen: true);

            writer.WriteLine(SessionCompanionIpc.CaptureCommand);
            var response = reader.ReadLine() ?? string.Empty;
            if (!response.StartsWith("OK|", StringComparison.OrdinalIgnoreCase))
            {
                detail = response;
                return false;
            }

            var payload = response.Substring(3);
            data = Convert.FromBase64String(payload);
            detail = "Companion capture succeeded.";
            return data.Length > 0;
        }
        catch (Exception ex)
        {
            detail = ex.Message;
            return false;
        }
    }

    public static bool TryPing(out string detail, int timeoutMs = 1500)
    {
        detail = string.Empty;

        try
        {
            using var client = new NamedPipeClientStream(
                ".",
                SessionCompanionIpc.PipeName,
                PipeDirection.InOut,
                PipeOptions.None);

            client.Connect(timeoutMs);
            using var writer = new StreamWriter(client, Encoding.UTF8, 4096, leaveOpen: true) { AutoFlush = true };
            using var reader = new StreamReader(client, Encoding.UTF8, false, 4096, leaveOpen: true);

            writer.WriteLine(SessionCompanionIpc.PingCommand);
            var response = reader.ReadLine() ?? string.Empty;
            if (!response.StartsWith("OK|", StringComparison.OrdinalIgnoreCase))
            {
                detail = response;
                return false;
            }

            detail = response;
            return true;
        }
        catch (Exception ex)
        {
            detail = ex.Message;
            return false;
        }
    }
}
