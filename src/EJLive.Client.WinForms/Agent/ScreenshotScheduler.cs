using System.Drawing.Imaging;
using EJLive.Client.WinForms.Services;
using EJLive.Core.Engine;
using CoreNetworkEngine = EJLive.Core.Engine.NetworkEngine;

namespace EJLive.Client.WinForms.Agent;

/// <summary>
/// Captures periodic screenshots, stores local copies, and optionally streams frames.
/// </summary>
public sealed class ScreenshotScheduler : IDisposable
{
    public sealed record ScreenshotCaptureResult(string LocalPath, long SizeBytes, DateTime CapturedAtUtc);

    private readonly Func<bool> _isConnected;
    private readonly Action<byte[]> _sendFrame;
    private readonly string _atmId;
    private readonly int _intervalMin;
    private readonly string _logPath;
    private System.Threading.Timer? _timer;

    public event Action<string>? OnLog;
    public event Action<ScreenshotCaptureResult>? OnScreenshotCaptured;
    public int ShotCount { get; private set; }
    public DateTime LastShot { get; private set; }

    public ScreenshotScheduler(NetworkManager? network, string atmId, int intervalMin, string logPath)
    {
        _isConnected = () => network?.IsConnected == true;
        _sendFrame = frame => network?.SendMessage(CommunicationProtocol.BuildGhostFrame(frame));
        _atmId = string.IsNullOrWhiteSpace(atmId) ? "UNKNOWN" : atmId.Trim();
        _intervalMin = Math.Max(1, intervalMin);
        _logPath = string.IsNullOrWhiteSpace(logPath) ? AppDomain.CurrentDomain.BaseDirectory : logPath;
    }

    public ScreenshotScheduler(CoreNetworkEngine? network, string atmId, int intervalMin, string logPath)
    {
        _isConnected = () => network?.IsConnected == true;
        _sendFrame = frame => network?.SendMessage(CommunicationProtocol.BuildGhostFrame(frame));
        _atmId = string.IsNullOrWhiteSpace(atmId) ? "UNKNOWN" : atmId.Trim();
        _intervalMin = Math.Max(1, intervalMin);
        _logPath = string.IsNullOrWhiteSpace(logPath) ? AppDomain.CurrentDomain.BaseDirectory : logPath;
    }

    public void Start()
    {
        var interval = TimeSpan.FromMinutes(_intervalMin);
        _timer = new System.Threading.Timer(_ => TakeAndSend(), null, TimeSpan.FromSeconds(30), interval);
    }

    public void Stop()
    {
        _timer?.Dispose();
        _timer = null;
    }

    public void TakeNow()
    {
        _ = Task.Run(TakeAndSend);
    }

    private void TakeAndSend()
    {
        try
        {
            var bounds = System.Windows.Forms.SystemInformation.VirtualScreen;
            if (bounds.Width <= 0 || bounds.Height <= 0)
                return;

            using var bitmap = new Bitmap(bounds.Width, bounds.Height, PixelFormat.Format32bppArgb);
            using (var graphics = Graphics.FromImage(bitmap))
                graphics.CopyFromScreen(bounds.Location, Point.Empty, bounds.Size);

            using var stream = new MemoryStream();
            var jpegInfo = ImageCodecInfo.GetImageEncoders().First(codec => codec.FormatID == ImageFormat.Jpeg.Guid);
            using var parameters = new EncoderParameters(1);
            parameters.Param[0] = new EncoderParameter(Encoder.Quality, 75L);
            bitmap.Save(stream, jpegInfo, parameters);
            var image = stream.ToArray();

            var folder = Path.Combine(_logPath, "screenshots", DateTime.Now.ToString("yyyy-MM-dd"));
            Directory.CreateDirectory(folder);
            var localFile = Path.Combine(folder, $"shot_{DateTime.Now:HHmmss}_{_atmId}.jpg");
            File.WriteAllBytes(localFile, image);

            if (_isConnected())
                _sendFrame(image);

            ShotCount++;
            LastShot = DateTime.UtcNow;
            OnScreenshotCaptured?.Invoke(new ScreenshotCaptureResult(localFile, image.LongLength, LastShot));
            OnLog?.Invoke($"Screenshot #{ShotCount}: {Math.Max(1, image.Length / 1024)} KB -> {localFile}");
        }
        catch (Exception ex)
        {
            OnLog?.Invoke("Screenshot error: " + ex.Message);
        }
    }

    public void Dispose()
    {
        Stop();
    }
}
