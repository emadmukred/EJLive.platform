// GhostRemoteEngine.cs
// Processed by Code Intelligence Scanner — Smart Merge & Restructure

using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Drawing;
using System.Globalization;
using System.IO.Compression;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System;
using EJLive.Core.Models;
using EJLive.Core.Services;
using EJLive.Shared;

namespace EJLive.Core.Engine
{
    /// <summary>
    /// محرك الوصول الشبحي الكامل — Ghost View (View-Only Remote Screen)
    /// لا يؤثر على الصراف ولا يُعيق العمليات
    /// لا يُسجل خروج المستخدم ولا يقفل شاشة الصراف أمام العملاء
    /// يضغط الشاشة JPEG ويُرسلها عبر NetworkEngine كل N ms
    /// </summary>
    public class GhostRemoteEngine : IDisposable
    {
        private Thread   _captureThread;
        private volatile bool _running;
        private int      _qualityPercent;
        private int      _captureIntervalMs;

        // قرارات الشاشة لتقليل الضغط على شبكات GSM/CDMA
        private readonly bool _lowBandwidth;

        public event EventHandler<byte[]>  OnFrameCaptured;   // JPEG bytes
        public event EventHandler<string>  OnError;
        public event EventHandler<string>  OnLog;

        // إحصائيات
        public int    FramesSent    { get; private set; }
        public long   BytesSent     { get; private set; }
        public double FPS           { get; private set; }
        public bool   IsActive      => _running;

        // WinAPI للتقاط الشاشة بدون التدخل في الجلسة الحالية
        [DllImport("user32.dll")] private static extern IntPtr GetDesktopWindow();
        [DllImport("user32.dll")] private static extern IntPtr GetWindowDC(IntPtr hWnd);
        [DllImport("gdi32.dll")]  private static extern bool BitBlt(IntPtr hdcDest, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hdcSrc, int nXSrc, int nYSrc, uint dwRop);
        [DllImport("user32.dll")] private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);
        [DllImport("gdi32.dll")]  private static extern IntPtr CreateCompatibleDC(IntPtr hdc);
        [DllImport("gdi32.dll")]  private static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int nWidth, int nHeight);
        [DllImport("gdi32.dll")]  private static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);
        [DllImport("gdi32.dll")]  private static extern bool DeleteDC(IntPtr hdc);
        [DllImport("gdi32.dll")]  private static extern bool DeleteObject(IntPtr hObject);

        private const uint SRCCOPY = 0x00CC0020;

        public GhostRemoteEngine(int qualityPercent = 75, bool lowBandwidth = false)
        {
            _qualityPercent    = Clamp(qualityPercent, 10, 100);
            _lowBandwidth      = lowBandwidth;
            _captureIntervalMs = lowBandwidth ? 2000 : 500; // GSM: 1 fps, LAN: 2 fps
        }

    public void SetQuality(int percent, int intervalMs = -1)
    {
        _qualityPercent    = Clamp(percent, 10, 100);
        if (intervalMs > 0) _captureIntervalMs = intervalMs;
    }

private static int Clamp(int value, int min, int max)
{
    return value < min ? min : (value > max ? max : value);
}

// ==========================================
// التشغيل
// ==========================================

public void Start()
{
    if (_running) return;
    _running = true;
    _captureThread = new Thread(CaptureLoop)
    {
        IsBackground = true,
        Name         = "EJLive.GhostCapture",
        Priority     = ThreadPriority.BelowNormal // لا يؤثر على الصراف
    };
_captureThread.Start();
OnLog?.Invoke(this, $"Ghost View started — quality={_qualityPercent}% interval={_captureIntervalMs}ms");
}

public void Stop()
{
    _running = false;
    OnLog?.Invoke(this, "Ghost View stopped.");
}

// ==========================================
// حلقة الالتقاط
// ==========================================

private void CaptureLoop()
{
    var lastFrame    = DateTime.UtcNow;
    var frameTracker = 0;
    var lastFpsCalc  = DateTime.UtcNow;

    while (_running)
    {
        try
        {
            var frame = CaptureScreenJpeg();
            if (frame != null && frame.Length > 0)
            {
                OnFrameCaptured?.Invoke(this, frame);
                FramesSent++;
                BytesSent += frame.Length;
                frameTracker++;
            }

        // حساب FPS كل ثانية
        var elapsed = (DateTime.UtcNow - lastFpsCalc).TotalSeconds;
        if (elapsed >= 1.0)
        {
            FPS          = frameTracker / elapsed;
            frameTracker = 0;
            lastFpsCalc  = DateTime.UtcNow;
        }

    Thread.Sleep(_captureIntervalMs);
}
catch (ThreadAbortException) { break; }
catch (Exception ex)
{
    OnError?.Invoke(this, $"Capture error: {ex.Message}");
    Thread.Sleep(2000);
}
}
}

// ==========================================
// التقاط الشاشة (WinAPI — لا يُعيق الجلسة)
// ==========================================

private byte[] CaptureScreenJpeg()
{
    var bounds = GetScreenBounds();
    var width  = bounds.Width;
    var height = bounds.Height;

    // تقليص الدقة للشبكات البطيئة
    if (_lowBandwidth)
    {
        width  = width  / 2;
        height = height / 2;
    }

var hWnd  = GetDesktopWindow();
var hDC   = GetWindowDC(hWnd);
var hMemDC = CreateCompatibleDC(hDC);
var hBmp  = CreateCompatibleBitmap(hDC, width, height);
var hOld  = SelectObject(hMemDC, hBmp);

bool captured = BitBlt(hMemDC, 0, 0, width, height, hDC, bounds.X, bounds.Y, SRCCOPY);

SelectObject(hMemDC, hOld);
DeleteDC(hMemDC);
ReleaseDC(hWnd, hDC);

if (!captured)
{
    DeleteObject(hBmp);
    return null;
}

byte[] jpegBytes = null;
try
{
    using var bmp = Image.FromHbitmap(hBmp);
    using var ms  = new MemoryStream();
    var encoder   = GetJpegEncoder();
    var encParms  = new EncoderParameters(1);
    encParms.Param[0] = new EncoderParameter(Encoder.Quality, (long)_qualityPercent);
    bmp.Save(ms, encoder, encParms);
    jpegBytes = ms.ToArray();
}
finally
{
    DeleteObject(hBmp);
}
return jpegBytes;
}

private System.Drawing.Rectangle GetScreenBounds()
{
    return System.Windows.Forms.Screen.PrimaryScreen?.Bounds
    ?? new System.Drawing.Rectangle(0, 0, 1024, 768);
}

private ImageCodecInfo GetJpegEncoder()
{
    foreach (var codec in ImageCodecInfo.GetImageEncoders())
    if (codec.MimeType == "image/jpeg") return codec;
    return null;
}

public void Dispose() => Stop();

/// <summary>
/// محرك الوصول عن بعد كشبح (Ghost/Shadow Remote Access)
/// الدخول كشبح بحيث:
/// 1. عدم تأثر الصراف
/// 2. عدم تسجيل الخروج
/// 3. عدم قفل شاشة الصراف على العميل
///
/// يعمل بنظام التقاط الشاشة المستمر (Screen Streaming) بدون تدخل
/// </summary>
public class GhostRemoteEngine
{
#region Events
    public event Action<string> OnLog;
    public event Action<byte[]> OnFrameReceived;      // Server side: frame from client
    public event Action<byte[]> OnFrameRequested;     // Client side: send frame to server
    public event Action<GhostSession> OnSessionStarted;
    public event Action<GhostSession> OnSessionEnded;
    public event Action<Exception> OnError;
#endregion

#region Fields
    private GhostSession _currentSession;
    private bool _isStreaming;
    private Thread _streamThread;
    private readonly object _lock = new object();
    private int _frameIntervalMs = 1000; // كل ثانية
    private int _jpegQuality = 40;       // جودة منخفضة للسرعة
    private Size _captureSize = new Size(1024, 768);
#endregion

#region Properties
    public bool IsActive => _currentSession != null && _currentSession.Status == GhostSessionStatus.Active;
    public GhostSession CurrentSession => _currentSession;
    public int FrameInterval { get => _frameIntervalMs; set => _frameIntervalMs = Math.Max(200, value); }
    public int JpegQuality { get => _jpegQuality; set => _jpegQuality = Math.Max(10, Math.Min(100, value)); }
#endregion

#region Client Side - Capture and Send
    /// <summary>
    /// بدء التقاط الشاشة وإرسالها (يعمل على الكلاينت)
    /// لا يؤثر على الصراف - فقط التقاط بدون تدخل
    /// </summary>
    public void StartClientStreaming(string sessionId)
    {
        if (_isStreaming) return;
        _isStreaming = true;

        _currentSession = new GhostSession
        {
            SessionID = sessionId,
            StartTime = DateTime.Now,
            Status = GhostSessionStatus.Active,
            IsViewOnly = true,
            ATMUnaffected = true,
            NoLogout = true,
            ScreenNotLocked = true
        };

    _streamThread = new Thread(CaptureLoop) { IsBackground = true, Name = "GhostCaptureThread" };
_streamThread.Start();

OnLog?.Invoke("[Ghost] Client streaming started - View Only mode (ATM unaffected)");
OnSessionStarted?.Invoke(_currentSession);
}

/// <summary>
/// إيقاف التقاط الشاشة
/// </summary>
public void StopClientStreaming()
{
    _isStreaming = false;
    if (_currentSession != null)
    {
        _currentSession.Status = GhostSessionStatus.Disconnected;
        _currentSession.EndTime = DateTime.Now;
        OnSessionEnded?.Invoke(_currentSession);
    }
OnLog?.Invoke("[Ghost] Client streaming stopped");
}

/// <summary>
/// التقاط إطار واحد من الشاشة
/// لا يقفل الشاشة ولا يؤثر على العميل
/// </summary>
public byte[] CaptureFrame()
{
    try
    {
        var bounds = System.Windows.Forms.Screen.PrimaryScreen.Bounds;
        using (var bmp = new Bitmap(bounds.Width, bounds.Height, PixelFormat.Format24bppRgb))
        {
            using (var g = Graphics.FromImage(bmp))
            {
                g.CopyFromScreen(bounds.Location, Point.Empty, bounds.Size, CopyPixelOperation.SourceCopy);
            }

        // تصغير الحجم للإرسال السريع
        using (var resized = new Bitmap(bmp, _captureSize))
        using (var ms = new MemoryStream())
        {
            var encoder = GetEncoder(ImageFormat.Jpeg);
            var encoderParams = new EncoderParameters(1);
            encoderParams.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, (long)_jpegQuality);
            resized.Save(ms, encoder, encoderParams);
            return ms.ToArray();
        }
}
}
catch (Exception ex)
{
    OnError?.Invoke(ex);
    return null;
}
}
#endregion

#region Server Side - Request and Display
/// <summary>
/// بدء جلسة مراقبة من السرفر
/// </summary>
public GhostSession StartServerSession(string atmId, string operatorName)
{
    _currentSession = new GhostSession
    {
        SessionID = Guid.NewGuid().ToString("N").Substring(0, 12),
        ATM_ID = atmId,
        OperatorName = operatorName,
        StartTime = DateTime.Now,
        Status = GhostSessionStatus.Connecting,
        IsViewOnly = true,
        ATMUnaffected = true,
        NoLogout = true,
        ScreenNotLocked = true
    };

OnLog?.Invoke($"[Ghost] Server session started for ATM: {atmId} by {operatorName}");
OnSessionStarted?.Invoke(_currentSession);
return _currentSession;
}

/// <summary>
/// إنهاء جلسة المراقبة
/// </summary>
public void EndServerSession()
{
    if (_currentSession != null)
    {
        _currentSession.Status = GhostSessionStatus.Disconnected;
        _currentSession.EndTime = DateTime.Now;
        _currentSession.ActivityLog.Add($"Session ended at {DateTime.Now:HH:mm:ss}");
        OnSessionEnded?.Invoke(_currentSession);
        OnLog?.Invoke($"[Ghost] Session ended: {_currentSession.SessionID}");
    }
_currentSession = null;
}

/// <summary>
/// معالجة إطار مستلم من الكلاينت
/// </summary>
public void ProcessReceivedFrame(byte[] frameData)
{
    if (frameData == null || frameData.Length == 0) return;
    if (_currentSession != null)
    {
        _currentSession.Status = GhostSessionStatus.Active;
        _currentSession.ActivityLog.Add($"Frame received: {frameData.Length} bytes at {DateTime.Now:HH:mm:ss}");
    }
OnFrameReceived?.Invoke(frameData);
}
#endregion

#region Protocol Messages
/// <summary>
/// بناء رسالة طلب بدء Ghost session
/// </summary>
public static byte[] BuildGhostStartRequest(string sessionId, string operatorName)
{
    string msg = $"GHOST_START|{sessionId}|{operatorName}|{DateTime.Now:yyyy-MM-dd HH:mm:ss}";
    return System.Text.Encoding.UTF8.GetBytes(msg);
}

/// <summary>
/// بناء رسالة إيقاف Ghost session
/// </summary>
public static byte[] BuildGhostStopRequest(string sessionId)
{
    string msg = $"GHOST_STOP|{sessionId}|{DateTime.Now:yyyy-MM-dd HH:mm:ss}";
    return System.Text.Encoding.UTF8.GetBytes(msg);
}

/// <summary>
/// بناء رسالة إطار (Frame)
/// </summary>
public static byte[] BuildFrameMessage(string sessionId, byte[] frameData)
{
    string header = $"GHOST_FRAME|{sessionId}|{frameData.Length}|";
    byte[] headerBytes = System.Text.Encoding.UTF8.GetBytes(header);
    byte[] message = new byte[headerBytes.Length + frameData.Length];
    Buffer.BlockCopy(headerBytes, 0, message, 0, headerBytes.Length);
    Buffer.BlockCopy(frameData, 0, message, headerBytes.Length, frameData.Length);
    return message;
}

/// <summary>
/// تحليل رسالة Ghost
/// </summary>
public static GhostMessage ParseGhostMessage(byte[] data)
{
    try
    {
        string text = System.Text.Encoding.UTF8.GetString(data);
        string[] parts = text.Split('|');
        if (parts.Length < 2) return null;

        var msg = new GhostMessage { Type = parts[0], SessionID = parts[1] };
        switch (parts[0])
        {
            case "GHOST_START":
            msg.OperatorName = parts.Length > 2 ? parts[2] : "";
            break;
            case "GHOST_FRAME":
            if (parts.Length > 2 && int.TryParse(parts[2], out int size))
            {
                msg.FrameSize = size;
                // Frame data follows after header
                int headerLen = System.Text.Encoding.UTF8.GetByteCount(string.Join("|", parts[0], parts[1], parts[2]) + "|");
                if (data.Length > headerLen)
                {
                    msg.FrameData = new byte[data.Length - headerLen];
                    Buffer.BlockCopy(data, headerLen, msg.FrameData, 0, msg.FrameData.Length);
                }
        }
    break;
}
return msg;
}
catch { return null; }
}
#endregion

#region Private Methods
private void CaptureLoop()
{
    while (_isStreaming)
    {
        try
        {
            byte[] frame = CaptureFrame();
            if (frame != null && frame.Length > 0)
            {
                OnFrameRequested?.Invoke(frame);
            }
    }
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
Thread.Sleep(_frameIntervalMs);
}
}

private ImageCodecInfo GetEncoder(ImageFormat format)
{
    ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
    foreach (ImageCodecInfo codec in codecs)
    {
        if (codec.FormatID == format.Guid) return codec;
    }
return null;
}
#endregion
}

/// <summary>
/// رسالة Ghost محللة
/// </summary>
public class GhostMessage
{
    public string Type { get; set; }
    public string SessionID { get; set; }
    public string OperatorName { get; set; }
    public int FrameSize { get; set; }
    public byte[] FrameData { get; set; }
}
}
/// <summary>
/// محرك الوصول الشبحي الكامل — Ghost View (View-Only Remote Screen)
/// لا يؤثر على الصراف ولا يُعيق العمليات
/// لا يُسجل خروج المستخدم ولا يقفل شاشة الصراف أمام العملاء
/// يضغط الشاشة JPEG ويُرسلها عبر NetworkEngine كل N ms
///
/// محرك الوصول عن بعد كشبح (Ghost/Shadow Remote Access)
/// الدخول كشبح بحيث:
/// 1. عدم تأثر الصراف
/// 2. عدم تسجيل الخروج
/// 3. عدم قفل شاشة الصراف على العميل
///
/// يعمل بنظام التقاط الشاشة المستمر (Screen Streaming) بدون تدخل
/// </summary>
public class GhostRemoteEngine : IDisposable
{
    // ==========================================
    // Fields / Events from First Class
    // ==========================================
    private Thread _captureThread;
    private volatile bool _running;
    private int _qualityPercent;
    private int _captureIntervalMs;

    // قرارات الشاشة لتقليل الضغط على شبكات GSM/CDMA
    private readonly bool _lowBandwidth;

    public event EventHandler<byte[]> OnFrameCaptured; // JPEG bytes
    public event EventHandler<string> OnError;
    public event EventHandler<string> OnLog;

    // إحصائيات
    public int FramesSent { get; private set; }
    public long BytesSent { get; private set; }
    public double FPS { get; private set; }
    public bool IsActive => _running;

    // WinAPI للتقاط الشاشة بدون التدخل في الجلسة الحالية
    [DllImport("user32.dll")] private static extern IntPtr GetDesktopWindow();
    [DllImport("user32.dll")] private static extern IntPtr GetWindowDC(IntPtr hWnd);
    [DllImport("gdi32.dll")] private static extern bool BitBlt(IntPtr hdcDest, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hdcSrc, int nXSrc, int nYSrc, uint dwRop);
    [DllImport("user32.dll")] private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);
    [DllImport("gdi32.dll")] private static extern IntPtr CreateCompatibleDC(IntPtr hdc);
    [DllImport("gdi32.dll")] private static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int nWidth, int nHeight);
    [DllImport("gdi32.dll")] private static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);
    [DllImport("gdi32.dll")] private static extern bool DeleteDC(IntPtr hdc);
    [DllImport("gdi32.dll")] private static extern bool DeleteObject(IntPtr hObject);

    private const uint SRCCOPY = 0x00CC0020;

    // ==========================================
    // Fields / Events from Second Class
    // ==========================================
    public event Action<string> OnLogAction;
    public event Action<byte[]> OnFrameReceived; // Server side: frame from client
    public event Action<byte[]> OnFrameRequested; // Client side: send frame to server
    public event Action<GhostSession> OnSessionStarted;
    public event Action<GhostSession> OnSessionEnded;
    public event Action<Exception> OnException;

    private GhostSession _currentSession;
    private bool _isStreaming;
    private Thread _streamThread;
    private readonly object _lock = new object();
    private int _frameIntervalMs = 1000; // كل ثانية
    private int _jpegQuality = 40; // جودة منخفضة للسرعة
    private Size _captureSize = new Size(1024, 768);

    public bool IsSessionActive => _currentSession != null && _currentSession.Status == GhostSessionStatus.Active;
    public GhostSession CurrentSession => _currentSession;
    public int FrameInterval
    {
        get => _frameIntervalMs;
        set => _frameIntervalMs = Math.Max(200, value);
    }
public int JpegQuality
{
    get => _jpegQuality;
    set => _jpegQuality = Math.Max(10, Math.Min(100, value));
}

// ==========================================
// Constructor / Helpers
// ==========================================
public GhostRemoteEngine(int qualityPercent = 75, bool lowBandwidth = false)
{
    _qualityPercent = Clamp(qualityPercent, 10, 100);
    _lowBandwidth = lowBandwidth;
    _captureIntervalMs = lowBandwidth ? 2000 : 500; // GSM: 1 fps, LAN: 2 fps
}

public void SetQuality(int percent, int intervalMs = -1)
{
    _qualityPercent = Clamp(percent, 10, 100);
    if (intervalMs > 0) _captureIntervalMs = intervalMs;
}

private static int Clamp(int value, int min, int max)
{
    return value < min ? min : (value > max ? max : value);
}

// ==========================================
// التشغيل (First Class)
// ==========================================
public void Start()
{
    if (_running) return;
    _running = true;
    _captureThread = new Thread(CaptureLoop)
    {
        IsBackground = true,
        Name = "EJLive.GhostCapture",
        Priority = ThreadPriority.BelowNormal // لا يؤثر على الصراف
    };
_captureThread.Start();
OnLog?.Invoke(this, $"Ghost View started — quality={_qualityPercent}% interval={_captureIntervalMs}ms");
}

public void Stop()
{
    _running = false;
    OnLog?.Invoke(this, "Ghost View stopped.");
}

// ==========================================
// حلقة الالتقاط (First Class)
// ==========================================
private void CaptureLoop()
{
    var lastFrame = DateTime.UtcNow;
    var frameTracker = 0;
    var lastFpsCalc = DateTime.UtcNow;

    while (_running)
    {
        try
        {
            var frame = CaptureScreenJpeg();
            if (frame != null && frame.Length > 0)
            {
                OnFrameCaptured?.Invoke(this, frame);
                FramesSent++;
                BytesSent += frame.Length;
                frameTracker++;
            }

        // حساب FPS كل ثانية
        var elapsed = (DateTime.UtcNow - lastFpsCalc).TotalSeconds;
        if (elapsed >= 1.0)
        {
            FPS = frameTracker / elapsed;
            frameTracker = 0;
            lastFpsCalc = DateTime.UtcNow;
        }

    Thread.Sleep(_captureIntervalMs);
}
catch (ThreadAbortException)
{
    break;
}
catch (Exception ex)
{
    OnError?.Invoke(this, $"Capture error: {ex.Message}");
    Thread.Sleep(2000);
}
}
}

// ==========================================
// التقاط الشاشة (WinAPI — لا يُعيق الجلسة) (First Class)
// ==========================================
private byte[] CaptureScreenJpeg()
{
    var bounds = GetScreenBounds();
    var width = bounds.Width;
    var height = bounds.Height;

    // تقليص الدقة للشبكات البطيئة
    if (_lowBandwidth)
    {
        width = width / 2;
        height = height / 2;
    }

var hWnd = GetDesktopWindow();
var hDC = GetWindowDC(hWnd);
var hMemDC = CreateCompatibleDC(hDC);
var hBmp = CreateCompatibleBitmap(hDC, width, height);
var hOld = SelectObject(hMemDC, hBmp);

bool captured = BitBlt(hMemDC, 0, 0, width, height, hDC, bounds.X, bounds.Y, SRCCOPY);

SelectObject(hMemDC, hOld);
DeleteDC(hMemDC);
ReleaseDC(hWnd, hDC);

if (!captured)
{
    DeleteObject(hBmp);
    return null;
}

byte[] jpegBytes = null;
try
{
    using var bmp = Image.FromHbitmap(hBmp);
    using var ms = new MemoryStream();
    var encoder = GetJpegEncoder();
    var encParms = new EncoderParameters(1);
    encParms.Param[0] = new EncoderParameter(Encoder.Quality, (long)_qualityPercent);
    bmp.Save(ms, encoder, encParms);
    jpegBytes = ms.ToArray();
}
finally
{
    DeleteObject(hBmp);
}
return jpegBytes;
}

private System.Drawing.Rectangle GetScreenBounds()
{
    return System.Windows.Forms.Screen.PrimaryScreen?.Bounds ??
    new System.Drawing.Rectangle(0, 0, 1024, 768);
}

private ImageCodecInfo GetJpegEncoder()
{
    foreach (var codec in ImageCodecInfo.GetImageEncoders())
    if (codec.MimeType == "image/jpeg") return codec;
    return null;
}

// ==========================================
// Client Side - Capture and Send (Second Class)
// ==========================================
/// <summary>
/// بدء التقاط الشاشة وإرسالها (يعمل على الكلاينت)
/// لا يؤثر على الصراف - فقط التقاط بدون تدخل
/// </summary>
public void StartClientStreaming(string sessionId)
{
    if (_isStreaming) return;
    _isStreaming = true;

    _currentSession = new GhostSession
    {
        SessionID = sessionId,
        StartTime = DateTime.Now,
        Status = GhostSessionStatus.Active,
        IsViewOnly = true,
        ATMUnaffected = true,
        NoLogout = true,
        ScreenNotLocked = true
    };

_streamThread = new Thread(CaptureLoopClient)
{
    IsBackground = true,
    Name = "GhostCaptureThread"
};
_streamThread.Start();

OnLogAction?.Invoke("[Ghost] Client streaming started - View Only mode (ATM unaffected)");
OnSessionStarted?.Invoke(_currentSession);
}

/// <summary>
/// إيقاف التقاط الشاشة
/// </summary>
public void StopClientStreaming()
{
    _isStreaming = false;
    if (_currentSession != null)
    {
        _currentSession.Status = GhostSessionStatus.Disconnected;
        _currentSession.EndTime = DateTime.Now;
        OnSessionEnded?.Invoke(_currentSession);
    }
OnLogAction?.Invoke("[Ghost] Client streaming stopped");
}

/// <summary>
/// التقاط إطار واحد من الشاشة
/// لا يقفل الشاشة ولا يؤثر على العميل
/// </summary>
public byte[] CaptureFrame()
{
    try
    {
        var bounds = System.Windows.Forms.Screen.PrimaryScreen.Bounds;
        using (var bmp = new Bitmap(bounds.Width, bounds.Height, PixelFormat.Format24bppRgb))
        {
            using (var g = Graphics.FromImage(bmp))
            {
                g.CopyFromScreen(bounds.Location, Point.Empty, bounds.Size, CopyPixelOperation.SourceCopy);
            }

        // تصغير الحجم للإرسال السريع
        using (var resized = new Bitmap(bmp, _captureSize))
        using (var ms = new MemoryStream())
        {
            var encoder = GetEncoder(ImageFormat.Jpeg);
            var encoderParams = new EncoderParameters(1);
            encoderParams.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, (long)_jpegQuality);
            resized.Save(ms, encoder, encoderParams);
            return ms.ToArray();
        }
}
}
catch (Exception ex)
{
    OnException?.Invoke(ex);
    return null;
}
}

// ==========================================
// Server Side - Request and Display (Second Class)
// ==========================================
/// <summary>
/// بدء جلسة مراقبة من السرفر
/// </summary>
public GhostSession StartServerSession(string atmId, string operatorName)
{
    _currentSession = new GhostSession
    {
        SessionID = Guid.NewGuid().ToString("N").Substring(0, 12),
        ATM_ID = atmId,
        OperatorName = operatorName,
        StartTime = DateTime.Now,
        Status = GhostSessionStatus.Connecting,
        IsViewOnly = true,
        ATMUnaffected = true,
        NoLogout = true,
        ScreenNotLocked = true
    };

OnLogAction?.Invoke($"[Ghost] Server session started for ATM: {atmId} by {operatorName}");
OnSessionStarted?.Invoke(_currentSession);
return _currentSession;
}

/// <summary>
/// إنهاء جلسة المراقبة
/// </summary>
public void EndServerSession()
{
    if (_currentSession != null)
    {
        _currentSession.Status = GhostSessionStatus.Disconnected;
        _currentSession.EndTime = DateTime.Now;
        _currentSession.ActivityLog.Add($"Session ended at {DateTime.Now:HH:mm:ss}");
        OnSessionEnded?.Invoke(_currentSession);
        OnLogAction?.Invoke($"[Ghost] Session ended: {_currentSession.SessionID}");
    }
_currentSession = null;
}

/// <summary>
/// معالجة إطار مستلم من الكلاينت
/// </summary>
public void ProcessReceivedFrame(byte[] frameData)
{
    if (frameData == null || frameData.Length == 0) return;
    if (_currentSession != null)
    {
        _currentSession.Status = GhostSessionStatus.Active;
        _currentSession.ActivityLog.Add($"Frame received: {frameData.Length} bytes at {DateTime.Now:HH:mm:ss}");
    }
OnFrameReceived?.Invoke(frameData);
}

// ==========================================
// Protocol Messages (Second Class)
// ==========================================
/// <summary>
/// بناء رسالة طلب بدء Ghost session
/// </summary>
public static byte[] BuildGhostStartRequest(string sessionId, string operatorName)
{
    string msg = $"GHOST_START|{sessionId}|{operatorName}|{DateTime.Now:yyyy-MM-dd HH:mm:ss}";
    return System.Text.Encoding.UTF8.GetBytes(msg);
}

/// <summary>
/// بناء رسالة إيقاف Ghost session
/// </summary>
public static byte[] BuildGhostStopRequest(string sessionId)
{
    string msg = $"GHOST_STOP|{sessionId}|{DateTime.Now:yyyy-MM-dd HH:mm:ss}";
    return System.Text.Encoding.UTF8.GetBytes(msg);
}

/// <summary>
/// بناء رسالة إطار (Frame)
/// </summary>
public static byte[] BuildFrameMessage(string sessionId, byte[] frameData)
{
    string header = $"GHOST_FRAME|{sessionId}|{frameData.Length}|";
    byte[] headerBytes = System.Text.Encoding.UTF8.GetBytes(header);
    byte[] message = new byte[headerBytes.Length + frameData.Length];
    Buffer.BlockCopy(headerBytes, 0, message, 0, headerBytes.Length);
    Buffer.BlockCopy(frameData, 0, message, headerBytes.Length, frameData.Length);
    return message;
}

/// <summary>
/// تحليل رسالة Ghost
/// </summary>
public static GhostMessage ParseGhostMessage(byte[] data)
{
    try
    {
        string text = System.Text.Encoding.UTF8.GetString(data);
        string[] parts = text.Split('|');
        if (parts.Length < 2) return null;

        var msg = new GhostMessage;
        {
            Type = parts[0],
            SessionID = parts[1]
        };
    switch (parts[0])
    {
        case "GHOST_START":
        msg.OperatorName = parts.Length > 2 ? parts[2] : "";
        break;
        case "GHOST_FRAME":
        if (parts.Length > 2 && int.TryParse(parts[2], out int size))
        {
            msg.FrameSize = size;
            // Frame data follows after header
            int headerLen = System.Text.Encoding.UTF8.GetByteCount(string.Join("|", parts[0], parts[1], parts[2]) + "|");
            if (data.Length > headerLen)
            {
                msg.FrameData = new byte[data.Length - headerLen];
                Buffer.BlockCopy(data, headerLen, msg.FrameData, 0, msg.FrameData.Length);
            }
    }
break;
}
return msg;
}
catch
{
    return null;
}
}

// ==========================================
// Private Methods (Second Class)
// ==========================================
private void CaptureLoopClient()
{
    while (_isStreaming)
    {
        try
        {
            byte[] frame = CaptureFrame();
            if (frame != null && frame.Length > 0)
            {
                OnFrameRequested?.Invoke(frame);
            }
    }
catch (Exception ex)
{
    OnException?.Invoke(ex);
}
Thread.Sleep(_frameIntervalMs);
}
}

private ImageCodecInfo GetEncoder(ImageFormat format)
{
    ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
    foreach (ImageCodecInfo codec in codecs)
    {
        if (codec.FormatID == format.Guid) return codec;
    }
return null;
}

public void Dispose() => Stop();
}
public partial public public class GhostRemoteEngine : IDisposable
{
    private Thread _captureThread;
    private volatile bool _running;
    private int _qualityPercent;
    private int _captureIntervalMs;
    private readonly bool _lowBandwidth;
    private GhostSession _currentSession;
    private bool _isStreaming;
    private Thread _streamThread;
    private int _frameIntervalMs = 1000;
    private int _jpegQuality = 40;
    public bool IsActive => _running;
    public bool IsSessionActive => _currentSession != null && _currentSession.Status == GhostSessionStatus.Active;
    public GhostSession CurrentSession => _currentSession;
    private readonly object _lock = new object();
    private Size _captureSize = new Size(1024, 768);
    private const uint SRCCOPY = 0x00CC0020;
    public GhostRemoteEngine(int qualityPercent = 75, bool lowBandwidth = false)
    {
        public int FrameInterval
        {
            get => _frameIntervalMs;
            set => _frameIntervalMs = Math.Max(200, value);
        }
    public int JpegQuality
    {
        get => _jpegQuality;
        set => _jpegQuality = Math.Max(10, Math.Min(100, value));
    }
public int FramesSent { get; private set; }
public long BytesSent { get; private set; }
public double FPS { get; private set; }
public class GhostRemoteEngine
{
#region Events
    public event Action<string> OnLog;
    public event Action<byte[]> OnFrameReceived;      // Server side: frame from client
    public event Action<byte[]> OnFrameRequested;     // Client side: send frame to server
    public event Action<GhostSession> OnSessionStarted;
    public event Action<GhostSession> OnSessionEnded;
    public event Action<Exception> OnError;
#endregion

#region Fields
    private GhostSession _currentSession;
    private bool _isStreaming;
    private Thread _streamThread;
    private readonly object _lock = new object();
    private int _frameIntervalMs = 1000; // كل ثانية
    private int _jpegQuality = 40;       // جودة منخفضة للسرعة
    private Size _captureSize = new Size(1024, 768);
#endregion

#region Properties
    public bool IsActive => _currentSession != null && _currentSession.Status == GhostSessionStatus.Active;
    public GhostSession CurrentSession => _currentSession;
    public int FrameInterval { get => _frameIntervalMs; set => _frameIntervalMs = Math.Max(200, value); }
    public class GhostMessage
    {
        public string Type { get; set; }
        public string SessionID { get; set; }
        public string OperatorName { get; set; }
        public int FrameSize { get; set; }
        public string Type { get; set; }
        [DllImport("user32.dll")] private static extern IntPtr GetDesktopWindow();


        // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\CodexMarege\CodexMarege\EJLive.Unified\src\EJLive.Core\Engine\GhostRemoteEngine.cs
        [DllImport("user32.dll")] private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);


        [DllImport("gdi32.dll")] private static extern bool BitBlt(IntPtr hdcDest, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hdcSrc, int nXSrc, int nYSrc, uint dwRop);


        // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\CodexMarege\CodexMarege\EJLive.Unified\src\EJLive.Core\Engine\GhostRemoteEngine.cs
        [DllImport("gdi32.dll")] private static extern bool DeleteObject(IntPtr hObject);


        private readonly object _lock = new object();


        private Size _captureSize = new Size(1024, 768);


        public void SetQuality(int percent, int intervalMs = -1)
        {
            private static int Clamp(int value, int min, int max)
            {
                public void Start()
                {
                    public void Stop()
                    {
                        private void CaptureLoop()
                        {
                            private byte[] CaptureScreenJpeg()
                            {
                                private ImageCodecInfo GetJpegEncoder()
                                {
                                    public void StartClientStreaming(string sessionId)
                                    {
                                        public void StopClientStreaming()
                                        {
                                            public byte[] CaptureFrame()
                                            {
                                                public GhostSession StartServerSession(string atmId, string operatorName)
                                                {
                                                    public void EndServerSession()
                                                    {
                                                        public void ProcessReceivedFrame(byte[] frameData)
                                                        {
                                                            public static byte[] BuildGhostStartRequest(string sessionId, string operatorName)
                                                            {
                                                                public static byte[] BuildGhostStopRequest(string sessionId)
                                                                {
                                                                    public static byte[] BuildFrameMessage(string sessionId, byte[] frameData)
                                                                    {
                                                                        public static GhostMessage ParseGhostMessage(byte[] data)
                                                                        {
                                                                            private void CaptureLoopClient()
                                                                            {
                                                                                private ImageCodecInfo GetEncoder(ImageFormat format)
                                                                                {
                                                                                    public void Dispose() =>
                                                                                    [DllImport("user32.dll")] private static extern IntPtr GetDesktopWindow();
                                                                                    [DllImport("user32.dll")] private static extern IntPtr GetWindowDC(IntPtr hWnd);
                                                                                    [DllImport("gdi32.dll")] private static extern bool BitBlt(IntPtr hdcDest, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hdcSrc, int nXSrc, int nYSrc, uint dwRop);
                                                                                    [DllImport("user32.dll")] private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);
                                                                                    [DllImport("gdi32.dll")] private static extern IntPtr CreateCompatibleDC(IntPtr hdc);
                                                                                    [DllImport("gdi32.dll")] private static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int nWidth, int nHeight);
                                                                                    [DllImport("gdi32.dll")] private static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);
                                                                                    [DllImport("gdi32.dll")] private static extern bool DeleteDC(IntPtr hdc);
                                                                                    [DllImport("gdi32.dll")] private static extern bool DeleteObject(IntPtr hObject);

                                                                                    private const uint SRCCOPY = 0x00CC0020;

                                                                                    // Fields / Events from Second Class
                                                                                    public event Action<string> OnLogAction;
                                                                                    public event Action<byte[]> OnFrameReceived; // Server side: frame from client
                                                                                    public event Action<byte[]> OnFrameRequested; // Client side: send frame to server
                                                                                    public event Action<GhostSession> OnSessionStarted;
                                                                                    public event Action<GhostSession> OnSessionEnded;
                                                                                    public event Action<Exception> OnException;

                                                                                    private GhostSession _currentSession;
                                                                                    private bool _isStreaming;
                                                                                    private Thread _streamThread;
                                                                                    private readonly object _lock = new object();
                                                                                    private int _frameIntervalMs = 1000; // كل ثانية
                                                                                    private int _jpegQuality = 40; // جودة منخفضة للسرعة
                                                                                    private Size _captureSize = new Size(1024, 768);

                                                                                    public bool IsSessionActive => _currentSession != null && _currentSession.Status == GhostSessionStatus.Active;
                                                                                    public GhostSession CurrentSession => _currentSession;
                                                                                    public int FrameInterval
                                                                                    {
                                                                                        get => _frameIntervalMs;
                                                                                        set => _frameIntervalMs = Math.Max(200, value);
                                                                                    }
                                                                                public int JpegQuality
                                                                                {
                                                                                    get => _jpegQuality;
                                                                                    set => _jpegQuality = Math.Max(10, Math.Min(100, value));
                                                                                }

                                                                            // Constructor / Helpers
                                                                            public GhostRemoteEngine(int qualityPercent = 75, bool lowBandwidth = false)
                                                                            {
                                                                                public void SetQuality(int percent, int intervalMs = -1)
                                                                                {
                                                                                    public event Action<string> OnLog;
                                                                                    public event Action<byte[]> OnFrameReceived;
                                                                                    public event Action<byte[]> OnFrameRequested;
                                                                                    public event Action<GhostSession> OnSessionStarted;
                                                                                    public event Action<GhostSession> OnSessionEnded;
                                                                                    public event Action<Exception> OnError;
                                                                                    public event Action<string> OnLogAction;
                                                                                    public event Action<Exception> OnException;
                                                                                    public event EventHandler<byte[]> OnFrameCaptured;
                                                                                    public event EventHandler<string> OnError;
                                                                                    public event EventHandler<string> OnLog;
                                                                                }

                                                                            public partial public class GhostMessage
                                                                            {
                                                                                private readonly object _lock = new object();
                                                                                private Size _captureSize = new Size(1024, 768);
                                                                                private GhostSession _currentSession;
                                                                                private bool _isStreaming;
                                                                                private Thread _streamThread;
                                                                                private int _frameIntervalMs = 1000;
                                                                                private int _jpegQuality = 40;
                                                                                public bool IsSessionActive => _currentSession != null && _currentSession.Status == GhostSessionStatus.Active;
                                                                                public GhostSession CurrentSession => _currentSession;
                                                                                private const uint SRCCOPY = 0x00CC0020;
                                                                                public string Type { get; set; }
                                                                                public string SessionID { get; set; }
                                                                                public string OperatorName { get; set; }
                                                                                public int FrameSize { get; set; }
                                                                                public int FrameInterval
                                                                                {
                                                                                    get => _frameIntervalMs;
                                                                                    set => _frameIntervalMs = Math.Max(200, value);
                                                                                }
                                                                            public int JpegQuality
                                                                            {
                                                                                get => _jpegQuality;
                                                                                set => _jpegQuality = Math.Max(10, Math.Min(100, value));
                                                                            }
                                                                        [DllImport("user32.dll")] private static extern IntPtr GetDesktopWindow();


                                                                        // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\CodexMarege\CodexMarege\EJLive.Unified\src\EJLive.Core\Engine\GhostRemoteEngine.cs
                                                                        [DllImport("user32.dll")] private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);


                                                                        [DllImport("gdi32.dll")] private static extern bool BitBlt(IntPtr hdcDest, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hdcSrc, int nXSrc, int nYSrc, uint dwRop);


                                                                        // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\CodexMarege\CodexMarege\EJLive.Unified\src\EJLive.Core\Engine\GhostRemoteEngine.cs
                                                                        [DllImport("gdi32.dll")] private static extern bool DeleteObject(IntPtr hObject);


                                                                        private readonly object _lock = new object();


                                                                        private Size _captureSize = new Size(1024, 768);


                                                                        public void SetQuality(int percent, int intervalMs = -1)
                                                                        {
                                                                            private static int Clamp(int value, int min, int max)
                                                                            {
                                                                                public void Start()
                                                                                {
                                                                                    public void Stop()
                                                                                    {
                                                                                        private void CaptureLoop()
                                                                                        {
                                                                                            private byte[] CaptureScreenJpeg()
                                                                                            {
                                                                                                private ImageCodecInfo GetJpegEncoder()
                                                                                                {
                                                                                                    public void StartClientStreaming(string sessionId)
                                                                                                    {
                                                                                                        public void StopClientStreaming()
                                                                                                        {
                                                                                                            public byte[] CaptureFrame()
                                                                                                            {
                                                                                                                public GhostSession StartServerSession(string atmId, string operatorName)
                                                                                                                {
                                                                                                                    public void EndServerSession()
                                                                                                                    {
                                                                                                                        public void ProcessReceivedFrame(byte[] frameData)
                                                                                                                        {
                                                                                                                            public static byte[] BuildGhostStartRequest(string sessionId, string operatorName)
                                                                                                                            {
                                                                                                                                public static byte[] BuildGhostStopRequest(string sessionId)
                                                                                                                                {
                                                                                                                                    public static byte[] BuildFrameMessage(string sessionId, byte[] frameData)
                                                                                                                                    {
                                                                                                                                        public static GhostMessage ParseGhostMessage(byte[] data)
                                                                                                                                        {
                                                                                                                                            private void CaptureLoopClient()
                                                                                                                                            {
                                                                                                                                                private ImageCodecInfo GetEncoder(ImageFormat format)
                                                                                                                                                {
                                                                                                                                                    public void Dispose() =>
                                                                                                                                                    [DllImport("user32.dll")] private static extern IntPtr GetDesktopWindow();
                                                                                                                                                    [DllImport("user32.dll")] private static extern IntPtr GetWindowDC(IntPtr hWnd);
                                                                                                                                                    [DllImport("gdi32.dll")] private static extern bool BitBlt(IntPtr hdcDest, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hdcSrc, int nXSrc, int nYSrc, uint dwRop);
                                                                                                                                                    [DllImport("user32.dll")] private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);
                                                                                                                                                    [DllImport("gdi32.dll")] private static extern IntPtr CreateCompatibleDC(IntPtr hdc);
                                                                                                                                                    [DllImport("gdi32.dll")] private static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int nWidth, int nHeight);
                                                                                                                                                    [DllImport("gdi32.dll")] private static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);
                                                                                                                                                    [DllImport("gdi32.dll")] private static extern bool DeleteDC(IntPtr hdc);
                                                                                                                                                    [DllImport("gdi32.dll")] private static extern bool DeleteObject(IntPtr hObject);

                                                                                                                                                    private const uint SRCCOPY = 0x00CC0020;

                                                                                                                                                    // Fields / Events from Second Class
                                                                                                                                                    public event Action<string> OnLogAction;
                                                                                                                                                    public event Action<byte[]> OnFrameReceived; // Server side: frame from client
                                                                                                                                                    public event Action<byte[]> OnFrameRequested; // Client side: send frame to server
                                                                                                                                                    public event Action<GhostSession> OnSessionStarted;
                                                                                                                                                    public event Action<GhostSession> OnSessionEnded;
                                                                                                                                                    public event Action<Exception> OnException;

                                                                                                                                                    private GhostSession _currentSession;
                                                                                                                                                    private bool _isStreaming;
                                                                                                                                                    private Thread _streamThread;
                                                                                                                                                    private readonly object _lock = new object();
                                                                                                                                                    private int _frameIntervalMs = 1000; // كل ثانية
                                                                                                                                                    private int _jpegQuality = 40; // جودة منخفضة للسرعة
                                                                                                                                                    private Size _captureSize = new Size(1024, 768);

                                                                                                                                                    public bool IsSessionActive => _currentSession != null && _currentSession.Status == GhostSessionStatus.Active;
                                                                                                                                                    public GhostSession CurrentSession => _currentSession;
                                                                                                                                                    public int FrameInterval
                                                                                                                                                    {
                                                                                                                                                        get => _frameIntervalMs;
                                                                                                                                                        set => _frameIntervalMs = Math.Max(200, value);
                                                                                                                                                    }
                                                                                                                                                public int JpegQuality
                                                                                                                                                {
                                                                                                                                                    get => _jpegQuality;
                                                                                                                                                    set => _jpegQuality = Math.Max(10, Math.Min(100, value));
                                                                                                                                                }

                                                                                                                                            // Constructor / Helpers
                                                                                                                                            public GhostRemoteEngine(int qualityPercent = 75, bool lowBandwidth = false)
                                                                                                                                            {
                                                                                                                                                public void SetQuality(int percent, int intervalMs = -1)
                                                                                                                                                {
                                                                                                                                                    public event Action<string> OnLogAction;
                                                                                                                                                    public event Action<byte[]> OnFrameReceived;
                                                                                                                                                    public event Action<byte[]> OnFrameRequested;
                                                                                                                                                    public event Action<GhostSession> OnSessionStarted;
                                                                                                                                                    public event Action<GhostSession> OnSessionEnded;
                                                                                                                                                    public event Action<Exception> OnException;
                                                                                                                                                    public event EventHandler<byte[]> OnFrameCaptured;
                                                                                                                                                    public event EventHandler<string> OnError;
                                                                                                                                                    public event EventHandler<string> OnLog;
                                                                                                                                                    public event Action<string> OnLog;
                                                                                                                                                    public event Action<Exception> OnError;
                                                                                                                                                }

                                                                                                                                        }
                                                                                                                                    public partial public class GhostRemoteEngine : IDisposable
                                                                                                                                    {
                                                                                                                                        private Thread _captureThread;
                                                                                                                                        private volatile bool _running;
                                                                                                                                        private int _qualityPercent;
                                                                                                                                        private int _captureIntervalMs;
                                                                                                                                        private readonly bool _lowBandwidth;
                                                                                                                                        private GhostSession _currentSession;
                                                                                                                                        private bool _isStreaming;
                                                                                                                                        private Thread _streamThread;
                                                                                                                                        private int _frameIntervalMs = 1000;
                                                                                                                                        private int _jpegQuality = 40;
                                                                                                                                        public bool IsActive => _running;
                                                                                                                                        public bool IsSessionActive => _currentSession != null && _currentSession.Status == GhostSessionStatus.Active;
                                                                                                                                        public GhostSession CurrentSession => _currentSession;
                                                                                                                                        private readonly object _lock = new object();
                                                                                                                                        private Size _captureSize = new Size(1024, 768);
                                                                                                                                        private const uint SRCCOPY = 0x00CC0020;
                                                                                                                                        public GhostRemoteEngine(int qualityPercent = 75, bool lowBandwidth = false)
                                                                                                                                        {
                                                                                                                                            public int FrameInterval
                                                                                                                                            {
                                                                                                                                                get => _frameIntervalMs;
                                                                                                                                                set => _frameIntervalMs = Math.Max(200, value);
                                                                                                                                            }
                                                                                                                                        public int JpegQuality
                                                                                                                                        {
                                                                                                                                            get => _jpegQuality;
                                                                                                                                            set => _jpegQuality = Math.Max(10, Math.Min(100, value));
                                                                                                                                        }
                                                                                                                                    public int FramesSent { get; private set; }
                                                                                                                                    public long BytesSent { get; private set; }
                                                                                                                                    public double FPS { get; private set; }
                                                                                                                                    public class GhostRemoteEngine
                                                                                                                                    {
#region Events
                                                                                                                                        public event Action<string> OnLog;
                                                                                                                                        public event Action<byte[]> OnFrameReceived;      // Server side: frame from client
                                                                                                                                        public event Action<byte[]> OnFrameRequested;     // Client side: send frame to server
                                                                                                                                        public event Action<GhostSession> OnSessionStarted;
                                                                                                                                        public event Action<GhostSession> OnSessionEnded;
                                                                                                                                        public event Action<Exception> OnError;
#endregion

#region Fields
                                                                                                                                        private GhostSession _currentSession;
                                                                                                                                        private bool _isStreaming;
                                                                                                                                        private Thread _streamThread;
                                                                                                                                        private readonly object _lock = new object();
                                                                                                                                        private int _frameIntervalMs = 1000; // كل ثانية
                                                                                                                                        private int _jpegQuality = 40;       // جودة منخفضة للسرعة
                                                                                                                                        private Size _captureSize = new Size(1024, 768);
#endregion

#region Properties
                                                                                                                                        public bool IsActive => _currentSession != null && _currentSession.Status == GhostSessionStatus.Active;
                                                                                                                                        public GhostSession CurrentSession => _currentSession;
                                                                                                                                        public int FrameInterval { get => _frameIntervalMs; set => _frameIntervalMs = Math.Max(200, value); }
                                                                                                                                        public class GhostMessage
                                                                                                                                        {
                                                                                                                                            public string Type { get; set; }
                                                                                                                                            public string SessionID { get; set; }
                                                                                                                                            public string OperatorName { get; set; }
                                                                                                                                            public int FrameSize { get; set; }
                                                                                                                                            [DllImport("user32.dll")] private static extern IntPtr GetDesktopWindow();


                                                                                                                                            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\CodexMarege\CodexMarege\EJLive.Unified\src\EJLive.Core\Engine\GhostRemoteEngine.cs
                                                                                                                                            [DllImport("user32.dll")] private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);


                                                                                                                                            [DllImport("gdi32.dll")] private static extern bool BitBlt(IntPtr hdcDest, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hdcSrc, int nXSrc, int nYSrc, uint dwRop);


                                                                                                                                            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\CodexMarege\CodexMarege\EJLive.Unified\src\EJLive.Core\Engine\GhostRemoteEngine.cs
                                                                                                                                            [DllImport("gdi32.dll")] private static extern bool DeleteObject(IntPtr hObject);


                                                                                                                                            private readonly object _lock = new object();


                                                                                                                                            private Size _captureSize = new Size(1024, 768);


                                                                                                                                            public void SetQuality(int percent, int intervalMs = -1)
                                                                                                                                            {
                                                                                                                                                private static int Clamp(int value, int min, int max)
                                                                                                                                                {
                                                                                                                                                    public void Start()
                                                                                                                                                    {
                                                                                                                                                        public void Stop()
                                                                                                                                                        {
                                                                                                                                                            private void CaptureLoop()
                                                                                                                                                            {
                                                                                                                                                                private byte[] CaptureScreenJpeg()
                                                                                                                                                                {
                                                                                                                                                                    private ImageCodecInfo GetJpegEncoder()
                                                                                                                                                                    {
                                                                                                                                                                        public void StartClientStreaming(string sessionId)
                                                                                                                                                                        {
                                                                                                                                                                            public void StopClientStreaming()
                                                                                                                                                                            {
                                                                                                                                                                                public byte[] CaptureFrame()
                                                                                                                                                                                {
                                                                                                                                                                                    public GhostSession StartServerSession(string atmId, string operatorName)
                                                                                                                                                                                    {
                                                                                                                                                                                        public void EndServerSession()
                                                                                                                                                                                        {
                                                                                                                                                                                            public void ProcessReceivedFrame(byte[] frameData)
                                                                                                                                                                                            {
                                                                                                                                                                                                public static byte[] BuildGhostStartRequest(string sessionId, string operatorName)
                                                                                                                                                                                                {
                                                                                                                                                                                                    public static byte[] BuildGhostStopRequest(string sessionId)
                                                                                                                                                                                                    {
                                                                                                                                                                                                        public static byte[] BuildFrameMessage(string sessionId, byte[] frameData)
                                                                                                                                                                                                        {
                                                                                                                                                                                                            public static GhostMessage ParseGhostMessage(byte[] data)
                                                                                                                                                                                                            {
                                                                                                                                                                                                                private void CaptureLoopClient()
                                                                                                                                                                                                                {
                                                                                                                                                                                                                    private ImageCodecInfo GetEncoder(ImageFormat format)
                                                                                                                                                                                                                    {
                                                                                                                                                                                                                        public void Dispose() =>
                                                                                                                                                                                                                        [DllImport("user32.dll")] private static extern IntPtr GetDesktopWindow();
                                                                                                                                                                                                                        [DllImport("user32.dll")] private static extern IntPtr GetWindowDC(IntPtr hWnd);
                                                                                                                                                                                                                        [DllImport("gdi32.dll")] private static extern bool BitBlt(IntPtr hdcDest, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hdcSrc, int nXSrc, int nYSrc, uint dwRop);
                                                                                                                                                                                                                        [DllImport("user32.dll")] private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);
                                                                                                                                                                                                                        [DllImport("gdi32.dll")] private static extern IntPtr CreateCompatibleDC(IntPtr hdc);
                                                                                                                                                                                                                        [DllImport("gdi32.dll")] private static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int nWidth, int nHeight);
                                                                                                                                                                                                                        [DllImport("gdi32.dll")] private static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);
                                                                                                                                                                                                                        [DllImport("gdi32.dll")] private static extern bool DeleteDC(IntPtr hdc);
                                                                                                                                                                                                                        [DllImport("gdi32.dll")] private static extern bool DeleteObject(IntPtr hObject);

                                                                                                                                                                                                                        private const uint SRCCOPY = 0x00CC0020;

                                                                                                                                                                                                                        // Fields / Events from Second Class
                                                                                                                                                                                                                        public event Action<string> OnLogAction;
                                                                                                                                                                                                                        public event Action<byte[]> OnFrameReceived; // Server side: frame from client
                                                                                                                                                                                                                        public event Action<byte[]> OnFrameRequested; // Client side: send frame to server
                                                                                                                                                                                                                        public event Action<GhostSession> OnSessionStarted;
                                                                                                                                                                                                                        public event Action<GhostSession> OnSessionEnded;
                                                                                                                                                                                                                        public event Action<Exception> OnException;

                                                                                                                                                                                                                        private GhostSession _currentSession;
                                                                                                                                                                                                                        private bool _isStreaming;
                                                                                                                                                                                                                        private Thread _streamThread;
                                                                                                                                                                                                                        private readonly object _lock = new object();
                                                                                                                                                                                                                        private int _frameIntervalMs = 1000; // كل ثانية
                                                                                                                                                                                                                        private int _jpegQuality = 40; // جودة منخفضة للسرعة
                                                                                                                                                                                                                        private Size _captureSize = new Size(1024, 768);

                                                                                                                                                                                                                        public bool IsSessionActive => _currentSession != null && _currentSession.Status == GhostSessionStatus.Active;
                                                                                                                                                                                                                        public GhostSession CurrentSession => _currentSession;
                                                                                                                                                                                                                        public int FrameInterval
                                                                                                                                                                                                                        {
                                                                                                                                                                                                                            get => _frameIntervalMs;
                                                                                                                                                                                                                            set => _frameIntervalMs = Math.Max(200, value);
                                                                                                                                                                                                                        }
                                                                                                                                                                                                                    public int JpegQuality
                                                                                                                                                                                                                    {
                                                                                                                                                                                                                        get => _jpegQuality;
                                                                                                                                                                                                                        set => _jpegQuality = Math.Max(10, Math.Min(100, value));
                                                                                                                                                                                                                    }

                                                                                                                                                                                                                // Constructor / Helpers
                                                                                                                                                                                                                public GhostRemoteEngine(int qualityPercent = 75, bool lowBandwidth = false)
                                                                                                                                                                                                                {
                                                                                                                                                                                                                    public void SetQuality(int percent, int intervalMs = -1)
                                                                                                                                                                                                                    {
                                                                                                                                                                                                                        public event EventHandler<byte[]> OnFrameCaptured;
                                                                                                                                                                                                                        public event EventHandler<string> OnError;
                                                                                                                                                                                                                        public event EventHandler<string> OnLog;
                                                                                                                                                                                                                        public event Action<string> OnLogAction;
                                                                                                                                                                                                                        public event Action<byte[]> OnFrameReceived;
                                                                                                                                                                                                                        public event Action<byte[]> OnFrameRequested;
                                                                                                                                                                                                                        public event Action<GhostSession> OnSessionStarted;
                                                                                                                                                                                                                        public event Action<GhostSession> OnSessionEnded;
                                                                                                                                                                                                                        public event Action<Exception> OnException;
                                                                                                                                                                                                                        public event Action<string> OnLog;
                                                                                                                                                                                                                        public event Action<Exception> OnError;
                                                                                                                                                                                                                    }

                                                                                                                                                                                                                public partial public class GhostMessage
                                                                                                                                                                                                                {
                                                                                                                                                                                                                    public string Type { get; set; }
                                                                                                                                                                                                                    public string SessionID { get; set; }
                                                                                                                                                                                                                    public string OperatorName { get; set; }
                                                                                                                                                                                                                    public int FrameSize { get; set; }
                                                                                                                                                                                                                }

                                                                                                                                                                                                        }
                                                                                                                                                                                                    public partial class GhostRemoteEngine : IDisposable
                                                                                                                                                                                                    {
                                                                                                                                                                                                        private Thread   _captureThread;


                                                                                                                                                                                                        private volatile bool _running;


                                                                                                                                                                                                        private int      _qualityPercent;


                                                                                                                                                                                                        private int      _captureIntervalMs;


                                                                                                                                                                                                        private readonly bool _lowBandwidth;


                                                                                                                                                                                                        private const uint SRCCOPY = 0x00CC0020;


                                                                                                                                                                                                        private GhostSession _currentSession;


                                                                                                                                                                                                        private bool _isStreaming;


                                                                                                                                                                                                        private Thread _streamThread;


                                                                                                                                                                                                        private int _frameIntervalMs = 1000; // كل ثانية


                                                                                                                                                                                                        private int _jpegQuality = 40;       // جودة منخفضة للسرعة


                                                                                                                                                                                                        public int FrameInterval
                                                                                                                                                                                                        {
                                                                                                                                                                                                            get => _frameIntervalMs;
                                                                                                                                                                                                            set => _frameIntervalMs = Math.Max(200, value);
                                                                                                                                                                                                        }


                                                                                                                                                                                                    // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\CodexMarege\CodexMarege\EJLive.Unified\src\EJLive.Core\Engine\GhostRemoteEngine.cs
                                                                                                                                                                                                    public int JpegQuality
                                                                                                                                                                                                    {
                                                                                                                                                                                                        get => _jpegQuality;
                                                                                                                                                                                                        set => _jpegQuality = Math.Max(10, Math.Min(100, value));
                                                                                                                                                                                                    }


                                                                                                                                                                                                // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\GhostRemoteEngine.cs
                                                                                                                                                                                                public int JpegQuality
                                                                                                                                                                                                {
                                                                                                                                                                                                    get => _jpegQuality;
                                                                                                                                                                                                    set => _jpegQuality = Math.Max(10, Math.Min(100, value));
                                                                                                                                                                                                }


                                                                                                                                                                                            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\CodexMarege\CodexMarege\EJLive.Unified\src\EJLive.Core\Engine\GhostRemoteEngine.cs
                                                                                                                                                                                            public int JpegQuality
                                                                                                                                                                                            {
                                                                                                                                                                                                get => _jpegQuality;
                                                                                                                                                                                                set => _jpegQuality = Math.Max(10, Math.Min(100, value));
                                                                                                                                                                                            }


                                                                                                                                                                                        // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-27\CodexMarege\CodexMarege\EJLive.Unified\src\EJLive.Core\Engine\GhostRemoteEngine.cs
                                                                                                                                                                                        public int JpegQuality
                                                                                                                                                                                        {
                                                                                                                                                                                            get => _jpegQuality;
                                                                                                                                                                                            set => _jpegQuality = Math.Max(10, Math.Min(100, value));
                                                                                                                                                                                        }


                                                                                                                                                                                    public int    FramesSent    { get; private set; }


                                                                                                                                                                                    public long   BytesSent     { get; private set; }


                                                                                                                                                                                    public double FPS           { get; private set; }


                                                                                                                                                                                    public bool   IsActive      => _running;


                                                                                                                                                                                    // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_36\EJLive.Core\Engine\GhostRemoteEngine.cs
                                                                                                                                                                                    public bool IsActive => _currentSession != null && _currentSession.Status == GhostSessionStatus.Active;


                                                                                                                                                                                    public GhostSession CurrentSession => _currentSession;


                                                                                                                                                                                    public int FrameInterval { get => _frameIntervalMs; set => _frameIntervalMs = Math.Max(200, value); }


                                                                                                                                                                                    public int JpegQuality { get => _jpegQuality; set => _jpegQuality = Math.Max(10, Math.Min(100, value)); }


                                                                                                                                                                                    // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_37\EJLive.Core\Engine\GhostRemoteEngine.cs
                                                                                                                                                                                    public bool IsActive => _currentSession != null && _currentSession.Status == GhostSessionStatus.Active;


                                                                                                                                                                                    public bool IsSessionActive => _currentSession != null && _currentSession.Status == GhostSessionStatus.Active;


                                                                                                                                                                                    // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\EJLive.Core\Engine\GhostRemoteEngine.cs
                                                                                                                                                                                    public bool IsActive => _currentSession != null && _currentSession.Status == GhostSessionStatus.Active;


                                                                                                                                                                                    // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\EJLiveWorkCoder\EJLive.Core\Engine\GhostRemoteEngine.cs
                                                                                                                                                                                    public bool IsActive => _currentSession != null && _currentSession.Status == GhostSessionStatus.Active;


                                                                                                                                                                                    // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\EJLive_Enterprise_Updated_20260512_build1\EJLive.Core\Engine\GhostRemoteEngine.cs
                                                                                                                                                                                    public bool IsActive => _currentSession != null && _currentSession.Status == GhostSessionStatus.Active;


                                                                                                                                                                                    // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\EJLive_Enterprise_operational_fixes_2026-05-12\EJLive.Core\Engine\GhostRemoteEngine.cs
                                                                                                                                                                                    public bool IsActive => _currentSession != null && _currentSession.Status == GhostSessionStatus.Active;


                                                                                                                                                                                    // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\EJLive_Enterprise_v3.4.0_Enhanced_Work\EJLive.Core\Engine\GhostRemoteEngine.cs
                                                                                                                                                                                    public bool IsActive => _currentSession != null && _currentSession.Status == GhostSessionStatus.Active;


                                                                                                                                                                                    // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\EJLive_replik\EJLive.Core\Engine\GhostRemoteEngine.cs
                                                                                                                                                                                    public bool IsActive => _currentSession != null && _currentSession.Status == GhostSessionStatus.Active;


                                                                                                                                                                                    // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\VBCode_local\CodexMarege\CodexMarege\EJLive.Core\Engine\GhostRemoteEngine.cs
                                                                                                                                                                                    public bool IsActive { get; private set; }


                                                                                                                                                                                    // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\src\EJLive.Core\Engine\GhostRemoteEngine.cs
                                                                                                                                                                                    public bool IsActive { get; private set; }


                                                                                                                                                                                    // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\GhostRemoteEngine.cs
                                                                                                                                                                                    public bool IsActive { get; private set; }


                                                                                                                                                                                    // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\EJLive.Core\Engine\GhostRemoteEngine.cs
                                                                                                                                                                                    public bool IsActive => _currentSession != null && _currentSession.Status == GhostSessionStatus.Active;


                                                                                                                                                                                    // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\EJLiveWorkCoder\EJLive.Core\Engine\GhostRemoteEngine.cs
                                                                                                                                                                                    public bool IsActive => _currentSession != null && _currentSession.Status == GhostSessionStatus.Active;


                                                                                                                                                                                    // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\EJLive_Enterprise_Updated_20260512_build1\EJLive.Core\Engine\GhostRemoteEngine.cs
                                                                                                                                                                                    public bool IsActive => _currentSession != null && _currentSession.Status == GhostSessionStatus.Active;


                                                                                                                                                                                    // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\EJLive_Enterprise_operational_fixes_2026-05-12\EJLive.Core\Engine\GhostRemoteEngine.cs
                                                                                                                                                                                    public bool IsActive => _currentSession != null && _currentSession.Status == GhostSessionStatus.Active;


                                                                                                                                                                                    // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\EJLive_Enterprise_v3.4.0_Enhanced_Work\EJLive.Core\Engine\GhostRemoteEngine.cs
                                                                                                                                                                                    public bool IsActive => _currentSession != null && _currentSession.Status == GhostSessionStatus.Active;


                                                                                                                                                                                    // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\EJLive_replik\EJLive.Core\Engine\GhostRemoteEngine.cs
                                                                                                                                                                                    public bool IsActive => _currentSession != null && _currentSession.Status == GhostSessionStatus.Active;


                                                                                                                                                                                    // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\VBCode_local\CodexMarege\CodexMarege\EJLive.Core\Engine\GhostRemoteEngine.cs
                                                                                                                                                                                    public bool IsActive { get; private set; }


                                                                                                                                                                                    // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\src\EJLive.Core\Engine\GhostRemoteEngine.cs
                                                                                                                                                                                    public bool IsActive { get; private set; }


                                                                                                                                                                                    // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-10\EJLive.Core\Engine\GhostRemoteEngine.cs
                                                                                                                                                                                    public bool IsActive => _currentSession != null && _currentSession.Status == GhostSessionStatus.Active;


                                                                                                                                                                                    // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-11\EJLive_Enterprise_operational_fixes_2026-05-12\EJLive.Core\Engine\GhostRemoteEngine.cs
                                                                                                                                                                                    public bool IsActive => _currentSession != null && _currentSession.Status == GhostSessionStatus.Active;


                                                                                                                                                                                    // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-13\EJLive_Enterprise_Updated_20260512_build1\EJLive.Core\Engine\GhostRemoteEngine.cs
                                                                                                                                                                                    public bool IsActive => _currentSession != null && _currentSession.Status == GhostSessionStatus.Active;


                                                                                                                                                                                    // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-17\EJLive_Enterprise_v3.4.0_Enhanced_Work\EJLive.Core\Engine\GhostRemoteEngine.cs
                                                                                                                                                                                    public bool IsActive => _currentSession != null && _currentSession.Status == GhostSessionStatus.Active;


                                                                                                                                                                                    // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-18\EJLive.Core\Engine\GhostRemoteEngine.cs
                                                                                                                                                                                    public bool IsActive => _currentSession != null && _currentSession.Status == GhostSessionStatus.Active;


                                                                                                                                                                                    // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-19\EJLive_replik\EJLive.Core\Engine\GhostRemoteEngine.cs
                                                                                                                                                                                    public bool IsActive => _currentSession != null && _currentSession.Status == GhostSessionStatus.Active;


                                                                                                                                                                                    // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-21\EJLive.Core\Engine\GhostRemoteEngine.cs
                                                                                                                                                                                    public bool IsActive => _currentSession != null && _currentSession.Status == GhostSessionStatus.Active;


                                                                                                                                                                                    // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-23\EJLiveWorkCoder\EJLive.Core\Engine\GhostRemoteEngine.cs
                                                                                                                                                                                    public bool IsActive => _currentSession != null && _currentSession.Status == GhostSessionStatus.Active;


                                                                                                                                                                                    // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-24\EJLiveWorkCoder\EJLive.Core\Engine\GhostRemoteEngine.cs
                                                                                                                                                                                    public bool IsActive => _currentSession != null && _currentSession.Status == GhostSessionStatus.Active;


                                                                                                                                                                                    // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-26\src\EJLive.Core\Engine\GhostRemoteEngine.cs
                                                                                                                                                                                    public bool IsActive { get; private set; }


                                                                                                                                                                                    // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-28\VBCode_local\CodexMarege\CodexMarege\EJLive.Core\Engine\GhostRemoteEngine.cs
                                                                                                                                                                                    public bool IsActive { get; private set; }


                                                                                                                                                                                    // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-2\EJLive.Core\Engine\GhostRemoteEngine.cs
                                                                                                                                                                                    public bool IsActive => _currentSession != null && _currentSession.Status == GhostSessionStatus.Active;


                                                                                                                                                                                    // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Engine\GhostRemoteEngine.cs.v15_bak
                                                                                                                                                                                    public bool IsActive => _currentSession != null && _currentSession.Status == GhostSessionStatus.Active;


                                                                                                                                                                                    // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Engine\GhostRemoteEngine.cs.v16_bak
                                                                                                                                                                                    public bool IsActive => _currentSession != null && _currentSession.Status == GhostSessionStatus.Active;


                                                                                                                                                                                    // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Engine\GhostRemoteEngine.cs.v17_bak
                                                                                                                                                                                    public bool IsActive => _currentSession != null && _currentSession.Status == GhostSessionStatus.Active;


                                                                                                                                                                                    // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Engine\GhostRemoteEngine.cs.v20_bak
                                                                                                                                                                                    public bool IsActive => _currentSession != null && _currentSession.Status == GhostSessionStatus.Active;


                                                                                                                                                                                    // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Engine\GhostRemoteEngine.cs.v22_bak
                                                                                                                                                                                    public bool IsActive => _currentSession != null && _currentSession.Status == GhostSessionStatus.Active;


                                                                                                                                                                                    public GhostRemoteEngine(int qualityPercent = 75, bool lowBandwidth = false)
                                                                                                                                                                                    {
                                                                                                                                                                                        _qualityPercent    = Clamp(qualityPercent, 10, 100);
                                                                                                                                                                                        _lowBandwidth      = lowBandwidth;
                                                                                                                                                                                        _captureIntervalMs = lowBandwidth ? 2000 : 500; // GSM: 1 fps, LAN: 2 fps
                                                                                                                                                                                    }


                                                                                                                                                                                [DllImport("user32.dll")] private static extern IntPtr GetDesktopWindow();


                                                                                                                                                                                // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_36\EJLive.Core\Engine\GhostRemoteEngine.cs
                                                                                                                                                                                [DllImport("user32.dll")] private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);


                                                                                                                                                                                [DllImport("gdi32.dll")]  private static extern bool BitBlt(IntPtr hdcDest, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hdcSrc, int nXSrc, int nYSrc, uint dwRop);


                                                                                                                                                                                // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_36\EJLive.Core\Engine\GhostRemoteEngine.cs
                                                                                                                                                                                [DllImport("gdi32.dll")]  private static extern bool DeleteObject(IntPtr hObject);


                                                                                                                                                                                public void SetQuality(int percent, int intervalMs = -1)
                                                                                                                                                                                {
                                                                                                                                                                                    _qualityPercent    = Clamp(percent, 10, 100);
                                                                                                                                                                                    if (intervalMs > 0) _captureIntervalMs = intervalMs;
                                                                                                                                                                                }


                                                                                                                                                                            private static int Clamp(int value, int min, int max)
                                                                                                                                                                            {
                                                                                                                                                                                return value < min ? min : (value > max ? max : value);
                                                                                                                                                                            }


                                                                                                                                                                        public void Start()
                                                                                                                                                                        {
                                                                                                                                                                            if (_running) return;
                                                                                                                                                                            _running = true;
                                                                                                                                                                            _captureThread = new Thread(CaptureLoop)
                                                                                                                                                                            {
                                                                                                                                                                                IsBackground = true,
                                                                                                                                                                                Name         = "EJLive.GhostCapture",
                                                                                                                                                                                Priority     = ThreadPriority.BelowNormal // لا يؤثر على الصراف
                                                                                                                                                                            };
                                                                                                                                                                        _captureThread.Start();
                                                                                                                                                                        OnLog?.Invoke(this, $"Ghost View started — quality={_qualityPercent}% interval={_captureIntervalMs}ms");
                                                                                                                                                                    }


                                                                                                                                                                public void Stop()
                                                                                                                                                                {
                                                                                                                                                                    _running = false;
                                                                                                                                                                    OnLog?.Invoke(this, "Ghost View stopped.");
                                                                                                                                                                }


                                                                                                                                                            private void CaptureLoop()
                                                                                                                                                            {
                                                                                                                                                                var lastFrame    = DateTime.UtcNow;
                                                                                                                                                                var frameTracker = 0;
                                                                                                                                                                var lastFpsCalc  = DateTime.UtcNow;

                                                                                                                                                                while (_running)
                                                                                                                                                                {
                                                                                                                                                                    try
                                                                                                                                                                    {
                                                                                                                                                                        var frame = CaptureScreenJpeg();
                                                                                                                                                                        if (frame != null && frame.Length > 0)
                                                                                                                                                                        {
                                                                                                                                                                            OnFrameCaptured?.Invoke(this, frame);
                                                                                                                                                                            FramesSent++;
                                                                                                                                                                            BytesSent += frame.Length;
                                                                                                                                                                            frameTracker++;
                                                                                                                                                                        }

                                                                                                                                                                    // حساب FPS كل ثانية
                                                                                                                                                                    var elapsed = (DateTime.UtcNow - lastFpsCalc).TotalSeconds;
                                                                                                                                                                    if (elapsed >= 1.0)
                                                                                                                                                                    {
                                                                                                                                                                        FPS          = frameTracker / elapsed;
                                                                                                                                                                        frameTracker = 0;
                                                                                                                                                                        lastFpsCalc  = DateTime.UtcNow;
                                                                                                                                                                    }

                                                                                                                                                                Thread.Sleep(_captureIntervalMs);
                                                                                                                                                            }
                                                                                                                                                        catch (ThreadAbortException) { break; }
                                                                                                                                                        catch (Exception ex)
                                                                                                                                                        {
                                                                                                                                                            OnError?.Invoke(this, $"Capture error: {ex.Message}");
                                                                                                                                                            Thread.Sleep(2000);
                                                                                                                                                        }
                                                                                                                                                }
                                                                                                                                        }


                                                                                                                                    private byte[] CaptureScreenJpeg()
                                                                                                                                    {
                                                                                                                                        var bounds = GetScreenBounds();
                                                                                                                                        var width  = bounds.Width;
                                                                                                                                        var height = bounds.Height;

                                                                                                                                        // تقليص الدقة للشبكات البطيئة
                                                                                                                                        if (_lowBandwidth)
                                                                                                                                        {
                                                                                                                                            width  = width  / 2;
                                                                                                                                            height = height / 2;
                                                                                                                                        }

                                                                                                                                    var hWnd  = GetDesktopWindow();
                                                                                                                                    var hDC   = GetWindowDC(hWnd);
                                                                                                                                    var hMemDC = CreateCompatibleDC(hDC);
                                                                                                                                    var hBmp  = CreateCompatibleBitmap(hDC, width, height);
                                                                                                                                    var hOld  = SelectObject(hMemDC, hBmp);

                                                                                                                                    bool captured = BitBlt(hMemDC, 0, 0, width, height, hDC, bounds.X, bounds.Y, SRCCOPY);

                                                                                                                                    SelectObject(hMemDC, hOld);
                                                                                                                                    DeleteDC(hMemDC);
                                                                                                                                    ReleaseDC(hWnd, hDC);

                                                                                                                                    if (!captured)
                                                                                                                                    {
                                                                                                                                        DeleteObject(hBmp);
                                                                                                                                        return null;
                                                                                                                                    }

                                                                                                                                byte[] jpegBytes = null;
                                                                                                                                try
                                                                                                                                {
                                                                                                                                    using var bmp = Image.FromHbitmap(hBmp);
                                                                                                                                    using var ms  = new MemoryStream();
                                                                                                                                    var encoder   = GetJpegEncoder();
                                                                                                                                    var encParms  = new EncoderParameters(1);
                                                                                                                                    encParms.Param[0] = new EncoderParameter(Encoder.Quality, (long)_qualityPercent);
                                                                                                                                    bmp.Save(ms, encoder, encParms);
                                                                                                                                    jpegBytes = ms.ToArray();
                                                                                                                                }
                                                                                                                            finally
                                                                                                                            {
                                                                                                                                DeleteObject(hBmp);
                                                                                                                            }
                                                                                                                        return jpegBytes;
                                                                                                                    }


                                                                                                                private System.Drawing.Rectangle GetScreenBounds()
                                                                                                                {
                                                                                                                    return System.Windows.Forms.Screen.PrimaryScreen?.Bounds
                                                                                                                    ?? new System.Drawing.Rectangle(0, 0, 1024, 768);
                                                                                                                }


                                                                                                            private ImageCodecInfo GetJpegEncoder()
                                                                                                            {
                                                                                                                foreach (var codec in ImageCodecInfo.GetImageEncoders())
                                                                                                                if (codec.MimeType == "image/jpeg") return codec;
                                                                                                                return null;
                                                                                                            }


                                                                                                        public void Dispose() => Stop();


                                                                                                        private readonly object _lock = new object();


                                                                                                        private Size _captureSize = new Size(1024, 768);


                                                                                                        public void StartClientStreaming(string sessionId)
                                                                                                        {
                                                                                                            if (_isStreaming) return;
                                                                                                            _isStreaming = true;

                                                                                                            _currentSession = new GhostSession
                                                                                                            {
                                                                                                                SessionID = sessionId,
                                                                                                                StartTime = DateTime.Now,
                                                                                                                Status = GhostSessionStatus.Active,
                                                                                                                IsViewOnly = true,
                                                                                                                ATMUnaffected = true,
                                                                                                                NoLogout = true,
                                                                                                                ScreenNotLocked = true
                                                                                                            };

                                                                                                        _streamThread = new Thread(CaptureLoop) { IsBackground = true, Name = "GhostCaptureThread" };
                                                                                                    _streamThread.Start();

                                                                                                    OnLog?.Invoke("[Ghost] Client streaming started - View Only mode (ATM unaffected)");
                                                                                                    OnSessionStarted?.Invoke(_currentSession);
                                                                                                }


                                                                                            public void StopClientStreaming()
                                                                                            {
                                                                                                _isStreaming = false;
                                                                                                if (_currentSession != null)
                                                                                                {
                                                                                                    _currentSession.Status = GhostSessionStatus.Disconnected;
                                                                                                    _currentSession.EndTime = DateTime.Now;
                                                                                                    OnSessionEnded?.Invoke(_currentSession);
                                                                                                }
                                                                                            OnLog?.Invoke("[Ghost] Client streaming stopped");
                                                                                        }


                                                                                    public byte[] CaptureFrame()
                                                                                    {
                                                                                        try
                                                                                        {
                                                                                            var bounds = System.Windows.Forms.Screen.PrimaryScreen.Bounds;
                                                                                            using (var bmp = new Bitmap(bounds.Width, bounds.Height, PixelFormat.Format24bppRgb))
                                                                                            {
                                                                                                using (var g = Graphics.FromImage(bmp))
                                                                                                {
                                                                                                    g.CopyFromScreen(bounds.Location, Point.Empty, bounds.Size, CopyPixelOperation.SourceCopy);
                                                                                                }

                                                                                            // تصغير الحجم للإرسال السريع
                                                                                            using (var resized = new Bitmap(bmp, _captureSize))
                                                                                            using (var ms = new MemoryStream())
                                                                                            {
                                                                                                var encoder = GetEncoder(ImageFormat.Jpeg);
                                                                                                var encoderParams = new EncoderParameters(1);
                                                                                                encoderParams.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, (long)_jpegQuality);
                                                                                                resized.Save(ms, encoder, encoderParams);
                                                                                                return ms.ToArray();
                                                                                            }
                                                                                    }
                                                                            }
                                                                        catch (Exception ex)
                                                                        {
                                                                            OnError?.Invoke(ex);
                                                                            return null;
                                                                        }
                                                                }


                                                            public GhostSession StartServerSession(string atmId, string operatorName)
                                                            {
                                                                _currentSession = new GhostSession
                                                                {
                                                                    SessionID = Guid.NewGuid().ToString("N").Substring(0, 12),
                                                                    ATM_ID = atmId,
                                                                    OperatorName = operatorName,
                                                                    StartTime = DateTime.Now,
                                                                    Status = GhostSessionStatus.Connecting,
                                                                    IsViewOnly = true,
                                                                    ATMUnaffected = true,
                                                                    NoLogout = true,
                                                                    ScreenNotLocked = true
                                                                };

                                                            OnLog?.Invoke($"[Ghost] Server session started for ATM: {atmId} by {operatorName}");
                                                            OnSessionStarted?.Invoke(_currentSession);
                                                            return _currentSession;
                                                        }


                                                    public void EndServerSession()
                                                    {
                                                        if (_currentSession != null)
                                                        {
                                                            _currentSession.Status = GhostSessionStatus.Disconnected;
                                                            _currentSession.EndTime = DateTime.Now;
                                                            _currentSession.ActivityLog.Add($"Session ended at {DateTime.Now:HH:mm:ss}");
                                                            OnSessionEnded?.Invoke(_currentSession);
                                                            OnLog?.Invoke($"[Ghost] Session ended: {_currentSession.SessionID}");
                                                        }
                                                    _currentSession = null;
                                                }


                                            public void ProcessReceivedFrame(byte[] frameData)
                                            {
                                                if (frameData == null || frameData.Length == 0) return;
                                                if (_currentSession != null)
                                                {
                                                    _currentSession.Status = GhostSessionStatus.Active;
                                                    _currentSession.ActivityLog.Add($"Frame received: {frameData.Length} bytes at {DateTime.Now:HH:mm:ss}");
                                                }
                                            OnFrameReceived?.Invoke(frameData);
                                        }


                                    public static byte[] BuildGhostStartRequest(string sessionId, string operatorName)
                                    {
                                        string msg = $"GHOST_START|{sessionId}|{operatorName}|{DateTime.Now:yyyy-MM-dd HH:mm:ss}";
                                        return System.Text.Encoding.UTF8.GetBytes(msg);
                                    }


                                public static byte[] BuildGhostStopRequest(string sessionId)
                                {
                                    string msg = $"GHOST_STOP|{sessionId}|{DateTime.Now:yyyy-MM-dd HH:mm:ss}";
                                    return System.Text.Encoding.UTF8.GetBytes(msg);
                                }


                            public static byte[] BuildFrameMessage(string sessionId, byte[] frameData)
                            {
                                string header = $"GHOST_FRAME|{sessionId}|{frameData.Length}|";
                                byte[] headerBytes = System.Text.Encoding.UTF8.GetBytes(header);
                                byte[] message = new byte[headerBytes.Length + frameData.Length];
                                Buffer.BlockCopy(headerBytes, 0, message, 0, headerBytes.Length);
                                Buffer.BlockCopy(frameData, 0, message, headerBytes.Length, frameData.Length);
                                return message;
                            }


                        public static GhostMessage ParseGhostMessage(byte[] data)
                        {
                            try
                            {
                                string text = System.Text.Encoding.UTF8.GetString(data);
                                string[] parts = text.Split('|');
                                if (parts.Length < 2) return null;

                                var msg = new GhostMessage { Type = parts[0], SessionID = parts[1] };
                                switch (parts[0])
                                {
                                    case "GHOST_START":
                                    msg.OperatorName = parts.Length > 2 ? parts[2] : "";
                                    break;
                                    case "GHOST_FRAME":
                                    if (parts.Length > 2 && int.TryParse(parts[2], out int size))
                                    {
                                        msg.FrameSize = size;
                                        // Frame data follows after header
                                        int headerLen = System.Text.Encoding.UTF8.GetByteCount(string.Join("|", parts[0], parts[1], parts[2]) + "|");
                                        if (data.Length > headerLen)
                                        {
                                            msg.FrameData = new byte[data.Length - headerLen];
                                            Buffer.BlockCopy(data, headerLen, msg.FrameData, 0, msg.FrameData.Length);
                                        }
                                }
                            break;
                        }
                    return msg;
                }
            catch { return null; }
        }


    // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_36\EJLive.Core\Engine\GhostRemoteEngine.cs
    private void CaptureLoop()
    {
        while (_isStreaming)
        {
            try
            {
                byte[] frame = CaptureFrame();
                if (frame != null && frame.Length > 0)
                {
                    OnFrameRequested?.Invoke(frame);
                }
        }
    catch (Exception ex)
    {
        OnError?.Invoke(ex);
    }
Thread.Sleep(_frameIntervalMs);
}
}


private ImageCodecInfo GetEncoder(ImageFormat format)
{
    ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
    foreach (ImageCodecInfo codec in codecs)
    {
        if (codec.FormatID == format.Guid) return codec;
    }
return null;
}


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_37\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("user32.dll")] private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_37\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("gdi32.dll")]  private static extern bool DeleteObject(IntPtr hObject);


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_37\EJLive.Core\Engine\GhostRemoteEngine.cs
private void CaptureLoop()
{
    while (_isStreaming)
    {
        try
        {
            byte[] frame = CaptureFrame();
            if (frame != null && frame.Length > 0)
            {
                OnFrameRequested?.Invoke(frame);
            }
    }
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
Thread.Sleep(_frameIntervalMs);
}
}


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\CodexMarege\CodexMarege\EJLive.Unified\src\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("user32.dll")] private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\CodexMarege\CodexMarege\EJLive.Unified\src\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("gdi32.dll")] private static extern bool DeleteObject(IntPtr hObject);


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\CodexMarege\CodexMarege\EJLive.Unified\src\EJLive.Core\Engine\GhostRemoteEngine.cs
public void StartClientStreaming(string sessionId)
{
    if (_isStreaming) return;
    _isStreaming = true;

    _currentSession = new GhostSession
    {
        SessionID = sessionId,
        StartTime = DateTime.Now,
        Status = GhostSessionStatus.Active,
        IsViewOnly = true,
        ATMUnaffected = true,
        NoLogout = true,
        ScreenNotLocked = true
    };

_streamThread = new Thread(CaptureLoopClient)
{
    IsBackground = true,
    Name = "GhostCaptureThread"
};
_streamThread.Start();

OnLogAction?.Invoke("[Ghost] Client streaming started - View Only mode (ATM unaffected)");
OnSessionStarted?.Invoke(_currentSession);
}


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\CodexMarege\CodexMarege\EJLive.Unified\src\EJLive.Core\Engine\GhostRemoteEngine.cs
public void StopClientStreaming()
{
    _isStreaming = false;
    if (_currentSession != null)
    {
        _currentSession.Status = GhostSessionStatus.Disconnected;
        _currentSession.EndTime = DateTime.Now;
        OnSessionEnded?.Invoke(_currentSession);
    }
OnLogAction?.Invoke("[Ghost] Client streaming stopped");
}


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\CodexMarege\CodexMarege\EJLive.Unified\src\EJLive.Core\Engine\GhostRemoteEngine.cs
public byte[] CaptureFrame()
{
    try
    {
        var bounds = System.Windows.Forms.Screen.PrimaryScreen.Bounds;
        using (var bmp = new Bitmap(bounds.Width, bounds.Height, PixelFormat.Format24bppRgb))
        {
            using (var g = Graphics.FromImage(bmp))
            {
                g.CopyFromScreen(bounds.Location, Point.Empty, bounds.Size, CopyPixelOperation.SourceCopy);
            }

        // تصغير الحجم للإرسال السريع
        using (var resized = new Bitmap(bmp, _captureSize))
        using (var ms = new MemoryStream())
        {
            var encoder = GetEncoder(ImageFormat.Jpeg);
            var encoderParams = new EncoderParameters(1);
            encoderParams.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, (long)_jpegQuality);
            resized.Save(ms, encoder, encoderParams);
            return ms.ToArray();
        }
}
}
catch (Exception ex)
{
    OnException?.Invoke(ex);
    return null;
}
}


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\CodexMarege\CodexMarege\EJLive.Unified\src\EJLive.Core\Engine\GhostRemoteEngine.cs
public GhostSession StartServerSession(string atmId, string operatorName)
{
    _currentSession = new GhostSession
    {
        SessionID = Guid.NewGuid().ToString("N").Substring(0, 12),
        ATM_ID = atmId,
        OperatorName = operatorName,
        StartTime = DateTime.Now,
        Status = GhostSessionStatus.Connecting,
        IsViewOnly = true,
        ATMUnaffected = true,
        NoLogout = true,
        ScreenNotLocked = true
    };

OnLogAction?.Invoke($"[Ghost] Server session started for ATM: {atmId} by {operatorName}");
OnSessionStarted?.Invoke(_currentSession);
return _currentSession;
}


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\CodexMarege\CodexMarege\EJLive.Unified\src\EJLive.Core\Engine\GhostRemoteEngine.cs
public void EndServerSession()
{
    if (_currentSession != null)
    {
        _currentSession.Status = GhostSessionStatus.Disconnected;
        _currentSession.EndTime = DateTime.Now;
        _currentSession.ActivityLog.Add($"Session ended at {DateTime.Now:HH:mm:ss}");
        OnSessionEnded?.Invoke(_currentSession);
        OnLogAction?.Invoke($"[Ghost] Session ended: {_currentSession.SessionID}");
    }
_currentSession = null;
}


private void CaptureLoopClient()
{
    while (_isStreaming)
    {
        try
        {
            byte[] frame = CaptureFrame();
            if (frame != null && frame.Length > 0)
            {
                OnFrameRequested?.Invoke(frame);
            }
    }
catch (Exception ex)
{
    OnException?.Invoke(ex);
}
Thread.Sleep(_frameIntervalMs);
}
}


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\CodexMarege\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("user32.dll")] private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\CodexMarege\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("gdi32.dll")]  private static extern bool DeleteObject(IntPtr hObject);


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("user32.dll")] private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("gdi32.dll")]  private static extern bool DeleteObject(IntPtr hObject);


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("user32.dll")] private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("gdi32.dll")]  private static extern bool DeleteObject(IntPtr hObject);


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\EJLive.Core\Engine\GhostRemoteEngine.cs
private void CaptureLoop()
{
    while (_isStreaming)
    {
        try
        {
            byte[] frame = CaptureFrame();
            if (frame != null && frame.Length > 0)
            {
                OnFrameRequested?.Invoke(frame);
            }
    }
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
Thread.Sleep(_frameIntervalMs);
}
}


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\EJLive.Unified\src\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("user32.dll")] private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\EJLive.Unified\src\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("gdi32.dll")]  private static extern bool DeleteObject(IntPtr hObject);


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\EJLiveEnterprisev4Unified\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("user32.dll")] private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\EJLiveEnterprisev4Unified\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("gdi32.dll")]  private static extern bool DeleteObject(IntPtr hObject);


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\EJLiveWorkCoder\EJLive.Core\Engine\GhostRemoteEngine.cs
private void CaptureLoop()
{
    while (_isStreaming)
    {
        try
        {
            byte[] frame = CaptureFrame();
            if (frame != null && frame.Length > 0)
            {
                OnFrameRequested?.Invoke(frame);
            }
    }
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
Thread.Sleep(_frameIntervalMs);
}
}


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\EJLive_Enterprise_Updated_20260512_build1\EJLive.Core\Engine\GhostRemoteEngine.cs
private void CaptureLoop()
{
    while (_isStreaming)
    {
        try
        {
            byte[] frame = CaptureFrame();
            if (frame != null && frame.Length > 0)
            {
                OnFrameRequested?.Invoke(frame);
            }
    }
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
Thread.Sleep(_frameIntervalMs);
}
}


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\EJLive_Enterprise_operational_fixes_2026-05-12\EJLive.Core\Engine\GhostRemoteEngine.cs
private void CaptureLoop()
{
    while (_isStreaming)
    {
        try
        {
            byte[] frame = CaptureFrame();
            if (frame != null && frame.Length > 0)
            {
                OnFrameRequested?.Invoke(frame);
            }
    }
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
Thread.Sleep(_frameIntervalMs);
}
}


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\EJLive_Enterprise_v3.4.0_Enhanced_Work\EJLive.Core\Engine\GhostRemoteEngine.cs
private void CaptureLoop()
{
    while (_isStreaming)
    {
        try
        {
            byte[] frame = CaptureFrame();
            if (frame != null && frame.Length > 0)
            {
                OnFrameRequested?.Invoke(frame);
            }
    }
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
Thread.Sleep(_frameIntervalMs);
}
}


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\EJLive_replik\EJLive.Core\Engine\GhostRemoteEngine.cs
private void CaptureLoop()
{
    while (_isStreaming)
    {
        try
        {
            byte[] frame = CaptureFrame();
            if (frame != null && frame.Length > 0)
            {
                OnFrameRequested?.Invoke(frame);
            }
    }
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
Thread.Sleep(_frameIntervalMs);
}
}


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\VBCode_local\CodexMarege\CodexMarege\EJLive.Core\Engine\GhostRemoteEngine.cs
public void Start() => IsActive = true;


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\VBCode_local\CodexMarege\CodexMarege\EJLive.Core\Engine\GhostRemoteEngine.cs
public void Stop() => IsActive = false;


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\src\EJLive.Core\Engine\GhostRemoteEngine.cs
private void CaptureLoop()
{
    while (_isStreaming)
    {
        try
        {
            byte[] frame = CaptureFrame();
            if (frame != null && frame.Length > 0)
            {
                OnFrameRequested?.Invoke(frame);
            }
    }
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
Thread.Sleep(_frameIntervalMs);
}
}


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\src\EJLive.Core\Engine\GhostRemoteEngine.cs
public void Start() => IsActive = true;


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\src\EJLive.Core\Engine\GhostRemoteEngine.cs
public void Stop() => IsActive = false;


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\src\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("user32.dll")] private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\src\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("gdi32.dll")]  private static extern bool DeleteObject(IntPtr hObject);


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\GhostRemoteEngine.cs
private void CaptureLoop()
{
    while (_isStreaming)
    {
        try
        {
            byte[] frame = CaptureFrame();
            if (frame != null && frame.Length > 0)
            {
                OnFrameRequested?.Invoke(frame);
            }
    }
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
Thread.Sleep(_frameIntervalMs);
}
}


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\GhostRemoteEngine.cs
public void Start() => IsActive = true;


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\GhostRemoteEngine.cs
public void Stop() => IsActive = false;


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\GhostRemoteEngine.cs
[DllImport("user32.dll")] private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\GhostRemoteEngine.cs
[DllImport("gdi32.dll")]  private static extern bool DeleteObject(IntPtr hObject);


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\GhostRemoteEngine.cs
public void StartClientStreaming(string sessionId)
{
    if (_isStreaming) return;
    _isStreaming = true;

    _currentSession = new GhostSession
    {
        SessionID = sessionId,
        StartTime = DateTime.Now,
        Status = GhostSessionStatus.Active,
        IsViewOnly = true,
        ATMUnaffected = true,
        NoLogout = true,
        ScreenNotLocked = true
    };

_streamThread = new Thread(CaptureLoopClient)
{
    IsBackground = true,
    Name = "GhostCaptureThread"
};
_streamThread.Start();

OnLogAction?.Invoke("[Ghost] Client streaming started - View Only mode (ATM unaffected)");
OnSessionStarted?.Invoke(_currentSession);
}


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\GhostRemoteEngine.cs
public void StopClientStreaming()
{
    _isStreaming = false;
    if (_currentSession != null)
    {
        _currentSession.Status = GhostSessionStatus.Disconnected;
        _currentSession.EndTime = DateTime.Now;
        OnSessionEnded?.Invoke(_currentSession);
    }
OnLogAction?.Invoke("[Ghost] Client streaming stopped");
}


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\GhostRemoteEngine.cs
public byte[] CaptureFrame()
{
    try
    {
        var bounds = System.Windows.Forms.Screen.PrimaryScreen.Bounds;
        using (var bmp = new Bitmap(bounds.Width, bounds.Height, PixelFormat.Format24bppRgb))
        {
            using (var g = Graphics.FromImage(bmp))
            {
                g.CopyFromScreen(bounds.Location, Point.Empty, bounds.Size, CopyPixelOperation.SourceCopy);
            }

        // تصغير الحجم للإرسال السريع
        using (var resized = new Bitmap(bmp, _captureSize))
        using (var ms = new MemoryStream())
        {
            var encoder = GetEncoder(ImageFormat.Jpeg);
            var encoderParams = new EncoderParameters(1);
            encoderParams.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, (long)_jpegQuality);
            resized.Save(ms, encoder, encoderParams);
            return ms.ToArray();
        }
}
}
catch (Exception ex)
{
    OnException?.Invoke(ex);
    return null;
}
}


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\GhostRemoteEngine.cs
public GhostSession StartServerSession(string atmId, string operatorName)
{
    _currentSession = new GhostSession
    {
        SessionID = Guid.NewGuid().ToString("N").Substring(0, 12),
        ATM_ID = atmId,
        OperatorName = operatorName,
        StartTime = DateTime.Now,
        Status = GhostSessionStatus.Connecting,
        IsViewOnly = true,
        ATMUnaffected = true,
        NoLogout = true,
        ScreenNotLocked = true
    };

OnLogAction?.Invoke($"[Ghost] Server session started for ATM: {atmId} by {operatorName}");
OnSessionStarted?.Invoke(_currentSession);
return _currentSession;
}


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\GhostRemoteEngine.cs
public void EndServerSession()
{
    if (_currentSession != null)
    {
        _currentSession.Status = GhostSessionStatus.Disconnected;
        _currentSession.EndTime = DateTime.Now;
        _currentSession.ActivityLog.Add($"Session ended at {DateTime.Now:HH:mm:ss}");
        OnSessionEnded?.Invoke(_currentSession);
        OnLogAction?.Invoke($"[Ghost] Session ended: {_currentSession.SessionID}");
    }
_currentSession = null;
}


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\CodexMarege\CodexMarege\EJLive.Unified\src\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("user32.dll")] private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\CodexMarege\CodexMarege\EJLive.Unified\src\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("gdi32.dll")] private static extern bool DeleteObject(IntPtr hObject);


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\CodexMarege\CodexMarege\EJLive.Unified\src\EJLive.Core\Engine\GhostRemoteEngine.cs
public void StartClientStreaming(string sessionId)
{
    if (_isStreaming) return;
    _isStreaming = true;

    _currentSession = new GhostSession
    {
        SessionID = sessionId,
        StartTime = DateTime.Now,
        Status = GhostSessionStatus.Active,
        IsViewOnly = true,
        ATMUnaffected = true,
        NoLogout = true,
        ScreenNotLocked = true
    };

_streamThread = new Thread(CaptureLoopClient)
{
    IsBackground = true,
    Name = "GhostCaptureThread"
};
_streamThread.Start();

OnLogAction?.Invoke("[Ghost] Client streaming started - View Only mode (ATM unaffected)");
OnSessionStarted?.Invoke(_currentSession);
}


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\CodexMarege\CodexMarege\EJLive.Unified\src\EJLive.Core\Engine\GhostRemoteEngine.cs
public void StopClientStreaming()
{
    _isStreaming = false;
    if (_currentSession != null)
    {
        _currentSession.Status = GhostSessionStatus.Disconnected;
        _currentSession.EndTime = DateTime.Now;
        OnSessionEnded?.Invoke(_currentSession);
    }
OnLogAction?.Invoke("[Ghost] Client streaming stopped");
}


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\CodexMarege\CodexMarege\EJLive.Unified\src\EJLive.Core\Engine\GhostRemoteEngine.cs
public byte[] CaptureFrame()
{
    try
    {
        var bounds = System.Windows.Forms.Screen.PrimaryScreen.Bounds;
        using (var bmp = new Bitmap(bounds.Width, bounds.Height, PixelFormat.Format24bppRgb))
        {
            using (var g = Graphics.FromImage(bmp))
            {
                g.CopyFromScreen(bounds.Location, Point.Empty, bounds.Size, CopyPixelOperation.SourceCopy);
            }

        // تصغير الحجم للإرسال السريع
        using (var resized = new Bitmap(bmp, _captureSize))
        using (var ms = new MemoryStream())
        {
            var encoder = GetEncoder(ImageFormat.Jpeg);
            var encoderParams = new EncoderParameters(1);
            encoderParams.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, (long)_jpegQuality);
            resized.Save(ms, encoder, encoderParams);
            return ms.ToArray();
        }
}
}
catch (Exception ex)
{
    OnException?.Invoke(ex);
    return null;
}
}


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\CodexMarege\CodexMarege\EJLive.Unified\src\EJLive.Core\Engine\GhostRemoteEngine.cs
public GhostSession StartServerSession(string atmId, string operatorName)
{
    _currentSession = new GhostSession
    {
        SessionID = Guid.NewGuid().ToString("N").Substring(0, 12),
        ATM_ID = atmId,
        OperatorName = operatorName,
        StartTime = DateTime.Now,
        Status = GhostSessionStatus.Connecting,
        IsViewOnly = true,
        ATMUnaffected = true,
        NoLogout = true,
        ScreenNotLocked = true
    };

OnLogAction?.Invoke($"[Ghost] Server session started for ATM: {atmId} by {operatorName}");
OnSessionStarted?.Invoke(_currentSession);
return _currentSession;
}


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\CodexMarege\CodexMarege\EJLive.Unified\src\EJLive.Core\Engine\GhostRemoteEngine.cs
public void EndServerSession()
{
    if (_currentSession != null)
    {
        _currentSession.Status = GhostSessionStatus.Disconnected;
        _currentSession.EndTime = DateTime.Now;
        _currentSession.ActivityLog.Add($"Session ended at {DateTime.Now:HH:mm:ss}");
        OnSessionEnded?.Invoke(_currentSession);
        OnLogAction?.Invoke($"[Ghost] Session ended: {_currentSession.SessionID}");
    }
_currentSession = null;
}


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\CodexMarege\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("user32.dll")] private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\CodexMarege\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("gdi32.dll")]  private static extern bool DeleteObject(IntPtr hObject);


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("user32.dll")] private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("gdi32.dll")]  private static extern bool DeleteObject(IntPtr hObject);


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("user32.dll")] private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("gdi32.dll")]  private static extern bool DeleteObject(IntPtr hObject);


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\EJLive.Core\Engine\GhostRemoteEngine.cs
private void CaptureLoop()
{
    while (_isStreaming)
    {
        try
        {
            byte[] frame = CaptureFrame();
            if (frame != null && frame.Length > 0)
            {
                OnFrameRequested?.Invoke(frame);
            }
    }
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
Thread.Sleep(_frameIntervalMs);
}
}


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\EJLive.Unified\src\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("user32.dll")] private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\EJLive.Unified\src\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("gdi32.dll")]  private static extern bool DeleteObject(IntPtr hObject);


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\EJLiveEnterprisev4Unified\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("user32.dll")] private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\EJLiveEnterprisev4Unified\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("gdi32.dll")]  private static extern bool DeleteObject(IntPtr hObject);


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\EJLiveWorkCoder\EJLive.Core\Engine\GhostRemoteEngine.cs
private void CaptureLoop()
{
    while (_isStreaming)
    {
        try
        {
            byte[] frame = CaptureFrame();
            if (frame != null && frame.Length > 0)
            {
                OnFrameRequested?.Invoke(frame);
            }
    }
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
Thread.Sleep(_frameIntervalMs);
}
}


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\EJLive_Enterprise_Updated_20260512_build1\EJLive.Core\Engine\GhostRemoteEngine.cs
private void CaptureLoop()
{
    while (_isStreaming)
    {
        try
        {
            byte[] frame = CaptureFrame();
            if (frame != null && frame.Length > 0)
            {
                OnFrameRequested?.Invoke(frame);
            }
    }
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
Thread.Sleep(_frameIntervalMs);
}
}


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\EJLive_Enterprise_operational_fixes_2026-05-12\EJLive.Core\Engine\GhostRemoteEngine.cs
private void CaptureLoop()
{
    while (_isStreaming)
    {
        try
        {
            byte[] frame = CaptureFrame();
            if (frame != null && frame.Length > 0)
            {
                OnFrameRequested?.Invoke(frame);
            }
    }
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
Thread.Sleep(_frameIntervalMs);
}
}


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\EJLive_Enterprise_v3.4.0_Enhanced_Work\EJLive.Core\Engine\GhostRemoteEngine.cs
private void CaptureLoop()
{
    while (_isStreaming)
    {
        try
        {
            byte[] frame = CaptureFrame();
            if (frame != null && frame.Length > 0)
            {
                OnFrameRequested?.Invoke(frame);
            }
    }
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
Thread.Sleep(_frameIntervalMs);
}
}


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\EJLive_replik\EJLive.Core\Engine\GhostRemoteEngine.cs
private void CaptureLoop()
{
    while (_isStreaming)
    {
        try
        {
            byte[] frame = CaptureFrame();
            if (frame != null && frame.Length > 0)
            {
                OnFrameRequested?.Invoke(frame);
            }
    }
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
Thread.Sleep(_frameIntervalMs);
}
}


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\VBCode_local\CodexMarege\CodexMarege\EJLive.Core\Engine\GhostRemoteEngine.cs
public void Start() => IsActive = true;


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\VBCode_local\CodexMarege\CodexMarege\EJLive.Core\Engine\GhostRemoteEngine.cs
public void Stop() => IsActive = false;


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\src\EJLive.Core\Engine\GhostRemoteEngine.cs
private void CaptureLoop()
{
    while (_isStreaming)
    {
        try
        {
            byte[] frame = CaptureFrame();
            if (frame != null && frame.Length > 0)
            {
                OnFrameRequested?.Invoke(frame);
            }
    }
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
Thread.Sleep(_frameIntervalMs);
}
}


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\src\EJLive.Core\Engine\GhostRemoteEngine.cs
public void Start() => IsActive = true;


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\src\EJLive.Core\Engine\GhostRemoteEngine.cs
public void Stop() => IsActive = false;


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\src\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("user32.dll")] private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\src\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("gdi32.dll")]  private static extern bool DeleteObject(IntPtr hObject);


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_41\CodexMarege\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("user32.dll")] private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_41\CodexMarege\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("gdi32.dll")]  private static extern bool DeleteObject(IntPtr hObject);


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_41\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("user32.dll")] private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_41\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("gdi32.dll")]  private static extern bool DeleteObject(IntPtr hObject);


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-10\EJLive.Core\Engine\GhostRemoteEngine.cs
private void CaptureLoop()
{
    while (_isStreaming)
    {
        try
        {
            byte[] frame = CaptureFrame();
            if (frame != null && frame.Length > 0)
            {
                OnFrameRequested?.Invoke(frame);
            }
    }
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
Thread.Sleep(_frameIntervalMs);
}
}


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-11\EJLive_Enterprise_operational_fixes_2026-05-12\EJLive.Core\Engine\GhostRemoteEngine.cs
private void CaptureLoop()
{
    while (_isStreaming)
    {
        try
        {
            byte[] frame = CaptureFrame();
            if (frame != null && frame.Length > 0)
            {
                OnFrameRequested?.Invoke(frame);
            }
    }
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
Thread.Sleep(_frameIntervalMs);
}
}


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-12\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("user32.dll")] private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-12\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("gdi32.dll")]  private static extern bool DeleteObject(IntPtr hObject);


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-13\EJLive_Enterprise_Updated_20260512_build1\EJLive.Core\Engine\GhostRemoteEngine.cs
private void CaptureLoop()
{
    while (_isStreaming)
    {
        try
        {
            byte[] frame = CaptureFrame();
            if (frame != null && frame.Length > 0)
            {
                OnFrameRequested?.Invoke(frame);
            }
    }
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
Thread.Sleep(_frameIntervalMs);
}
}


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-14\CodexMarege\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("user32.dll")] private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-14\CodexMarege\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("gdi32.dll")]  private static extern bool DeleteObject(IntPtr hObject);


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-15\CodexMarege\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("user32.dll")] private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-15\CodexMarege\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("gdi32.dll")]  private static extern bool DeleteObject(IntPtr hObject);


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-16\CodexMarege\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("user32.dll")] private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-16\CodexMarege\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("gdi32.dll")]  private static extern bool DeleteObject(IntPtr hObject);


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-17\EJLive_Enterprise_v3.4.0_Enhanced_Work\EJLive.Core\Engine\GhostRemoteEngine.cs
private void CaptureLoop()
{
    while (_isStreaming)
    {
        try
        {
            byte[] frame = CaptureFrame();
            if (frame != null && frame.Length > 0)
            {
                OnFrameRequested?.Invoke(frame);
            }
    }
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
Thread.Sleep(_frameIntervalMs);
}
}


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-18\EJLive.Core\Engine\GhostRemoteEngine.cs
private void CaptureLoop()
{
    while (_isStreaming)
    {
        try
        {
            byte[] frame = CaptureFrame();
            if (frame != null && frame.Length > 0)
            {
                OnFrameRequested?.Invoke(frame);
            }
    }
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
Thread.Sleep(_frameIntervalMs);
}
}


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-19\EJLive_replik\EJLive.Core\Engine\GhostRemoteEngine.cs
private void CaptureLoop()
{
    while (_isStreaming)
    {
        try
        {
            byte[] frame = CaptureFrame();
            if (frame != null && frame.Length > 0)
            {
                OnFrameRequested?.Invoke(frame);
            }
    }
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
Thread.Sleep(_frameIntervalMs);
}
}


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-1\CodexMarege\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("user32.dll")] private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-1\CodexMarege\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("gdi32.dll")]  private static extern bool DeleteObject(IntPtr hObject);


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-21\EJLive.Core\Engine\GhostRemoteEngine.cs
private void CaptureLoop()
{
    while (_isStreaming)
    {
        try
        {
            byte[] frame = CaptureFrame();
            if (frame != null && frame.Length > 0)
            {
                OnFrameRequested?.Invoke(frame);
            }
    }
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
Thread.Sleep(_frameIntervalMs);
}
}


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-22\EJLiveEnterprisev4Unified\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("user32.dll")] private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-22\EJLiveEnterprisev4Unified\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("gdi32.dll")]  private static extern bool DeleteObject(IntPtr hObject);


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-23\EJLiveWorkCoder\EJLive.Core\Engine\GhostRemoteEngine.cs
private void CaptureLoop()
{
    while (_isStreaming)
    {
        try
        {
            byte[] frame = CaptureFrame();
            if (frame != null && frame.Length > 0)
            {
                OnFrameRequested?.Invoke(frame);
            }
    }
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
Thread.Sleep(_frameIntervalMs);
}
}


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-24\EJLiveWorkCoder\EJLive.Core\Engine\GhostRemoteEngine.cs
private void CaptureLoop()
{
    while (_isStreaming)
    {
        try
        {
            byte[] frame = CaptureFrame();
            if (frame != null && frame.Length > 0)
            {
                OnFrameRequested?.Invoke(frame);
            }
    }
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
Thread.Sleep(_frameIntervalMs);
}
}


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-26\src\EJLive.Core\Engine\GhostRemoteEngine.cs
public void Start() => IsActive = true;


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-26\src\EJLive.Core\Engine\GhostRemoteEngine.cs
public void Stop() => IsActive = false;


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-27\CodexMarege\CodexMarege\EJLive.Unified\src\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("user32.dll")] private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-27\CodexMarege\CodexMarege\EJLive.Unified\src\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("gdi32.dll")] private static extern bool DeleteObject(IntPtr hObject);


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-27\CodexMarege\CodexMarege\EJLive.Unified\src\EJLive.Core\Engine\GhostRemoteEngine.cs
public void StartClientStreaming(string sessionId)
{
    if (_isStreaming) return;
    _isStreaming = true;

    _currentSession = new GhostSession
    {
        SessionID = sessionId,
        StartTime = DateTime.Now,
        Status = GhostSessionStatus.Active,
        IsViewOnly = true,
        ATMUnaffected = true,
        NoLogout = true,
        ScreenNotLocked = true
    };

_streamThread = new Thread(CaptureLoopClient)
{
    IsBackground = true,
    Name = "GhostCaptureThread"
};
_streamThread.Start();

OnLogAction?.Invoke("[Ghost] Client streaming started - View Only mode (ATM unaffected)");
OnSessionStarted?.Invoke(_currentSession);
}


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-27\CodexMarege\CodexMarege\EJLive.Unified\src\EJLive.Core\Engine\GhostRemoteEngine.cs
public void StopClientStreaming()
{
    _isStreaming = false;
    if (_currentSession != null)
    {
        _currentSession.Status = GhostSessionStatus.Disconnected;
        _currentSession.EndTime = DateTime.Now;
        OnSessionEnded?.Invoke(_currentSession);
    }
OnLogAction?.Invoke("[Ghost] Client streaming stopped");
}


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-27\CodexMarege\CodexMarege\EJLive.Unified\src\EJLive.Core\Engine\GhostRemoteEngine.cs
public byte[] CaptureFrame()
{
    try
    {
        var bounds = System.Windows.Forms.Screen.PrimaryScreen.Bounds;
        using (var bmp = new Bitmap(bounds.Width, bounds.Height, PixelFormat.Format24bppRgb))
        {
            using (var g = Graphics.FromImage(bmp))
            {
                g.CopyFromScreen(bounds.Location, Point.Empty, bounds.Size, CopyPixelOperation.SourceCopy);
            }

        // تصغير الحجم للإرسال السريع
        using (var resized = new Bitmap(bmp, _captureSize))
        using (var ms = new MemoryStream())
        {
            var encoder = GetEncoder(ImageFormat.Jpeg);
            var encoderParams = new EncoderParameters(1);
            encoderParams.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, (long)_jpegQuality);
            resized.Save(ms, encoder, encoderParams);
            return ms.ToArray();
        }
}
}
catch (Exception ex)
{
    OnException?.Invoke(ex);
    return null;
}
}


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-27\CodexMarege\CodexMarege\EJLive.Unified\src\EJLive.Core\Engine\GhostRemoteEngine.cs
public GhostSession StartServerSession(string atmId, string operatorName)
{
    _currentSession = new GhostSession
    {
        SessionID = Guid.NewGuid().ToString("N").Substring(0, 12),
        ATM_ID = atmId,
        OperatorName = operatorName,
        StartTime = DateTime.Now,
        Status = GhostSessionStatus.Connecting,
        IsViewOnly = true,
        ATMUnaffected = true,
        NoLogout = true,
        ScreenNotLocked = true
    };

OnLogAction?.Invoke($"[Ghost] Server session started for ATM: {atmId} by {operatorName}");
OnSessionStarted?.Invoke(_currentSession);
return _currentSession;
}


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-27\CodexMarege\CodexMarege\EJLive.Unified\src\EJLive.Core\Engine\GhostRemoteEngine.cs
public void EndServerSession()
{
    if (_currentSession != null)
    {
        _currentSession.Status = GhostSessionStatus.Disconnected;
        _currentSession.EndTime = DateTime.Now;
        _currentSession.ActivityLog.Add($"Session ended at {DateTime.Now:HH:mm:ss}");
        OnSessionEnded?.Invoke(_currentSession);
        OnLogAction?.Invoke($"[Ghost] Session ended: {_currentSession.SessionID}");
    }
_currentSession = null;
}


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-28\VBCode_local\CodexMarege\CodexMarege\EJLive.Core\Engine\GhostRemoteEngine.cs
public void Start() => IsActive = true;


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-28\VBCode_local\CodexMarege\CodexMarege\EJLive.Core\Engine\GhostRemoteEngine.cs
public void Stop() => IsActive = false;


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-29\CodexMarege\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("user32.dll")] private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-29\CodexMarege\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("gdi32.dll")]  private static extern bool DeleteObject(IntPtr hObject);


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-2\EJLive.Core\Engine\GhostRemoteEngine.cs
private void CaptureLoop()
{
    while (_isStreaming)
    {
        try
        {
            byte[] frame = CaptureFrame();
            if (frame != null && frame.Length > 0)
            {
                OnFrameRequested?.Invoke(frame);
            }
    }
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
Thread.Sleep(_frameIntervalMs);
}
}


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Engine\GhostRemoteEngine.cs.v15_bak
private void CaptureLoop()
{
    while (_isStreaming)
    {
        try
        {
            byte[] frame = CaptureFrame();
            if (frame != null && frame.Length > 0)
            {
                OnFrameRequested?.Invoke(frame);
            }
    }
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
Thread.Sleep(_frameIntervalMs);
}
}


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Engine\GhostRemoteEngine.cs.v16_bak
private void CaptureLoop()
{
    while (_isStreaming)
    {
        try
        {
            byte[] frame = CaptureFrame();
            if (frame != null && frame.Length > 0)
            {
                OnFrameRequested?.Invoke(frame);
            }
    }
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
Thread.Sleep(_frameIntervalMs);
}
}


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Engine\GhostRemoteEngine.cs.v17_bak
private void CaptureLoop()
{
    while (_isStreaming)
    {
        try
        {
            byte[] frame = CaptureFrame();
            if (frame != null && frame.Length > 0)
            {
                OnFrameRequested?.Invoke(frame);
            }
    }
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
Thread.Sleep(_frameIntervalMs);
}
}


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Engine\GhostRemoteEngine.cs.v20_bak
private void CaptureLoop()
{
    while (_isStreaming)
    {
        try
        {
            byte[] frame = CaptureFrame();
            if (frame != null && frame.Length > 0)
            {
                OnFrameRequested?.Invoke(frame);
            }
    }
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
Thread.Sleep(_frameIntervalMs);
}
}


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Engine\GhostRemoteEngine.cs.v22_bak
private void CaptureLoop()
{
    while (_isStreaming)
    {
        try
        {
            byte[] frame = CaptureFrame();
            if (frame != null && frame.Length > 0)
            {
                OnFrameRequested?.Invoke(frame);
            }
    }
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
Thread.Sleep(_frameIntervalMs);
}
}


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Engine\GhostRemoteEngine.cs.v23_bak
[DllImport("user32.dll")] private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Engine\GhostRemoteEngine.cs.v23_bak
[DllImport("gdi32.dll")]  private static extern bool DeleteObject(IntPtr hObject);


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-33\CodexMarege\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("user32.dll")] private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-33\CodexMarege\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("gdi32.dll")]  private static extern bool DeleteObject(IntPtr hObject);


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-34\CodexMarege\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("user32.dll")] private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-34\CodexMarege\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("gdi32.dll")]  private static extern bool DeleteObject(IntPtr hObject);


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-4\EJLive.Unified\src\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("user32.dll")] private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-4\EJLive.Unified\src\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("gdi32.dll")]  private static extern bool DeleteObject(IntPtr hObject);


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-5\EJLive.Unified\src\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("user32.dll")] private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-5\EJLive.Unified\src\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("gdi32.dll")]  private static extern bool DeleteObject(IntPtr hObject);


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-6\CodexMarege\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("user32.dll")] private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-6\CodexMarege\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("gdi32.dll")]  private static extern bool DeleteObject(IntPtr hObject);


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-7\CodexMarege\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("user32.dll")] private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-7\CodexMarege\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("gdi32.dll")]  private static extern bool DeleteObject(IntPtr hObject);


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-8\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("user32.dll")] private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-8\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("gdi32.dll")]  private static extern bool DeleteObject(IntPtr hObject);


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-9\EJLive.Unified\src\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("user32.dll")] private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-9\EJLive.Unified\src\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("gdi32.dll")]  private static extern bool DeleteObject(IntPtr hObject);


public event EventHandler<byte[]>  OnFrameCaptured;   // JPEG bytes


public event EventHandler<string>  OnError;


public event EventHandler<string>  OnLog;


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_36\EJLive.Core\Engine\GhostRemoteEngine.cs
public event Action<string> OnLog;


public event Action<byte[]> OnFrameReceived;      // Server side: frame from client


public event Action<byte[]> OnFrameRequested;     // Client side: send frame to server


public event Action<GhostSession> OnSessionStarted;


public event Action<GhostSession> OnSessionEnded;


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_36\EJLive.Core\Engine\GhostRemoteEngine.cs
public event Action<Exception> OnError;


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_37\EJLive.Core\Engine\GhostRemoteEngine.cs
public event Action<string> OnLog;


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_37\EJLive.Core\Engine\GhostRemoteEngine.cs
public event Action<Exception> OnError;


public event Action<string> OnLogAction;


public event Action<Exception> OnException;


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\EJLive.Core\Engine\GhostRemoteEngine.cs
public event Action<string> OnLog;


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\EJLive.Core\Engine\GhostRemoteEngine.cs
public event Action<Exception> OnError;


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\EJLiveWorkCoder\EJLive.Core\Engine\GhostRemoteEngine.cs
public event Action<string> OnLog;


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\EJLiveWorkCoder\EJLive.Core\Engine\GhostRemoteEngine.cs
public event Action<Exception> OnError;


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\EJLive_Enterprise_Updated_20260512_build1\EJLive.Core\Engine\GhostRemoteEngine.cs
public event Action<string> OnLog;


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\EJLive_Enterprise_Updated_20260512_build1\EJLive.Core\Engine\GhostRemoteEngine.cs
public event Action<Exception> OnError;


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\EJLive_Enterprise_operational_fixes_2026-05-12\EJLive.Core\Engine\GhostRemoteEngine.cs
public event Action<string> OnLog;


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\EJLive_Enterprise_operational_fixes_2026-05-12\EJLive.Core\Engine\GhostRemoteEngine.cs
public event Action<Exception> OnError;


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\EJLive_Enterprise_v3.4.0_Enhanced_Work\EJLive.Core\Engine\GhostRemoteEngine.cs
public event Action<string> OnLog;


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\EJLive_Enterprise_v3.4.0_Enhanced_Work\EJLive.Core\Engine\GhostRemoteEngine.cs
public event Action<Exception> OnError;


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\EJLive_replik\EJLive.Core\Engine\GhostRemoteEngine.cs
public event Action<string> OnLog;


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\EJLive_replik\EJLive.Core\Engine\GhostRemoteEngine.cs
public event Action<Exception> OnError;


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\src\EJLive.Core\Engine\GhostRemoteEngine.cs
public event Action<string> OnLog;


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\src\EJLive.Core\Engine\GhostRemoteEngine.cs
public event Action<Exception> OnError;


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\GhostRemoteEngine.cs
public event Action<string> OnLog;


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\GhostRemoteEngine.cs
public event Action<Exception> OnError;


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\EJLive.Core\Engine\GhostRemoteEngine.cs
public event Action<string> OnLog;


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\EJLive.Core\Engine\GhostRemoteEngine.cs
public event Action<Exception> OnError;


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\EJLiveWorkCoder\EJLive.Core\Engine\GhostRemoteEngine.cs
public event Action<string> OnLog;


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\EJLiveWorkCoder\EJLive.Core\Engine\GhostRemoteEngine.cs
public event Action<Exception> OnError;


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\EJLive_Enterprise_Updated_20260512_build1\EJLive.Core\Engine\GhostRemoteEngine.cs
public event Action<string> OnLog;


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\EJLive_Enterprise_Updated_20260512_build1\EJLive.Core\Engine\GhostRemoteEngine.cs
public event Action<Exception> OnError;


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\EJLive_Enterprise_operational_fixes_2026-05-12\EJLive.Core\Engine\GhostRemoteEngine.cs
public event Action<string> OnLog;


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\EJLive_Enterprise_operational_fixes_2026-05-12\EJLive.Core\Engine\GhostRemoteEngine.cs
public event Action<Exception> OnError;


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\EJLive_Enterprise_v3.4.0_Enhanced_Work\EJLive.Core\Engine\GhostRemoteEngine.cs
public event Action<string> OnLog;


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\EJLive_Enterprise_v3.4.0_Enhanced_Work\EJLive.Core\Engine\GhostRemoteEngine.cs
public event Action<Exception> OnError;


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\EJLive_replik\EJLive.Core\Engine\GhostRemoteEngine.cs
public event Action<string> OnLog;


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\EJLive_replik\EJLive.Core\Engine\GhostRemoteEngine.cs
public event Action<Exception> OnError;


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\src\EJLive.Core\Engine\GhostRemoteEngine.cs
public event Action<string> OnLog;


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\src\EJLive.Core\Engine\GhostRemoteEngine.cs
public event Action<Exception> OnError;


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-10\EJLive.Core\Engine\GhostRemoteEngine.cs
public event Action<string> OnLog;


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-10\EJLive.Core\Engine\GhostRemoteEngine.cs
public event Action<Exception> OnError;


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-11\EJLive_Enterprise_operational_fixes_2026-05-12\EJLive.Core\Engine\GhostRemoteEngine.cs
public event Action<string> OnLog;


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-11\EJLive_Enterprise_operational_fixes_2026-05-12\EJLive.Core\Engine\GhostRemoteEngine.cs
public event Action<Exception> OnError;


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-13\EJLive_Enterprise_Updated_20260512_build1\EJLive.Core\Engine\GhostRemoteEngine.cs
public event Action<string> OnLog;


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-13\EJLive_Enterprise_Updated_20260512_build1\EJLive.Core\Engine\GhostRemoteEngine.cs
public event Action<Exception> OnError;


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-17\EJLive_Enterprise_v3.4.0_Enhanced_Work\EJLive.Core\Engine\GhostRemoteEngine.cs
public event Action<string> OnLog;


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-17\EJLive_Enterprise_v3.4.0_Enhanced_Work\EJLive.Core\Engine\GhostRemoteEngine.cs
public event Action<Exception> OnError;


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-18\EJLive.Core\Engine\GhostRemoteEngine.cs
public event Action<string> OnLog;


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-18\EJLive.Core\Engine\GhostRemoteEngine.cs
public event Action<Exception> OnError;


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-19\EJLive_replik\EJLive.Core\Engine\GhostRemoteEngine.cs
public event Action<string> OnLog;


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-19\EJLive_replik\EJLive.Core\Engine\GhostRemoteEngine.cs
public event Action<Exception> OnError;


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-21\EJLive.Core\Engine\GhostRemoteEngine.cs
public event Action<string> OnLog;


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-21\EJLive.Core\Engine\GhostRemoteEngine.cs
public event Action<Exception> OnError;


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-23\EJLiveWorkCoder\EJLive.Core\Engine\GhostRemoteEngine.cs
public event Action<string> OnLog;


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-23\EJLiveWorkCoder\EJLive.Core\Engine\GhostRemoteEngine.cs
public event Action<Exception> OnError;


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-24\EJLiveWorkCoder\EJLive.Core\Engine\GhostRemoteEngine.cs
public event Action<string> OnLog;


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-24\EJLiveWorkCoder\EJLive.Core\Engine\GhostRemoteEngine.cs
public event Action<Exception> OnError;


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-2\EJLive.Core\Engine\GhostRemoteEngine.cs
public event Action<string> OnLog;


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-2\EJLive.Core\Engine\GhostRemoteEngine.cs
public event Action<Exception> OnError;


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Engine\GhostRemoteEngine.cs.v15_bak
public event Action<string> OnLog;


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Engine\GhostRemoteEngine.cs.v15_bak
public event Action<Exception> OnError;


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Engine\GhostRemoteEngine.cs.v16_bak
public event Action<string> OnLog;


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Engine\GhostRemoteEngine.cs.v16_bak
public event Action<Exception> OnError;


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Engine\GhostRemoteEngine.cs.v17_bak
public event Action<string> OnLog;


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Engine\GhostRemoteEngine.cs.v17_bak
public event Action<Exception> OnError;


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Engine\GhostRemoteEngine.cs.v20_bak
public event Action<string> OnLog;


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Engine\GhostRemoteEngine.cs.v20_bak
public event Action<Exception> OnError;


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Engine\GhostRemoteEngine.cs.v22_bak
public event Action<string> OnLog;


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Engine\GhostRemoteEngine.cs.v22_bak
public event Action<Exception> OnError;


public class GhostRemoteEngine
{
#region Events
    public event Action<string> OnLog;
    public event Action<byte[]> OnFrameReceived;      // Server side: frame from client
    public event Action<byte[]> OnFrameRequested;     // Client side: send frame to server
    public event Action<GhostSession> OnSessionStarted;
    public event Action<GhostSession> OnSessionEnded;
    public event Action<Exception> OnError;
#endregion

#region Fields
    private GhostSession _currentSession;
    private bool _isStreaming;
    private Thread _streamThread;
    private readonly object _lock = new object();
    private int _frameIntervalMs = 1000; // كل ثانية
    private int _jpegQuality = 40;       // جودة منخفضة للسرعة
    private Size _captureSize = new Size(1024, 768);
#endregion

#region Properties
    public bool IsActive => _currentSession != null && _currentSession.Status == GhostSessionStatus.Active;
    public GhostSession CurrentSession => _currentSession;
    public int FrameInterval { get => _frameIntervalMs; set => _frameIntervalMs = Math.Max(200, value); }
    public int JpegQuality { get => _jpegQuality; set => _jpegQuality = Math.Max(10, Math.Min(100, value)); }
#endregion

#region Client Side - Capture and Send
    /// <summary>
    /// بدء التقاط الشاشة وإرسالها (يعمل على الكلاينت)
    /// لا يؤثر على الصراف - فقط التقاط بدون تدخل
    /// </summary>
    public void StartClientStreaming(string sessionId)
    {
        if (_isStreaming) return;
        _isStreaming = true;

        _currentSession = new GhostSession
        {
            SessionID = sessionId,
            StartTime = DateTime.Now,
            Status = GhostSessionStatus.Active,
            IsViewOnly = true,
            ATMUnaffected = true,
            NoLogout = true,
            ScreenNotLocked = true
        };

    _streamThread = new Thread(CaptureLoop) { IsBackground = true, Name = "GhostCaptureThread" };
_streamThread.Start();

OnLog?.Invoke("[Ghost] Client streaming started - View Only mode (ATM unaffected)");
OnSessionStarted?.Invoke(_currentSession);
}

/// <summary>
/// إيقاف التقاط الشاشة
/// </summary>
public void StopClientStreaming()
{
    _isStreaming = false;
    if (_currentSession != null)
    {
        _currentSession.Status = GhostSessionStatus.Disconnected;
        _currentSession.EndTime = DateTime.Now;
        OnSessionEnded?.Invoke(_currentSession);
    }
OnLog?.Invoke("[Ghost] Client streaming stopped");
}

/// <summary>
/// التقاط إطار واحد من الشاشة
/// لا يقفل الشاشة ولا يؤثر على العميل
/// </summary>
public byte[] CaptureFrame()
{
    try
    {
        var bounds = System.Windows.Forms.Screen.PrimaryScreen.Bounds;
        using (var bmp = new Bitmap(bounds.Width, bounds.Height, PixelFormat.Format24bppRgb))
        {
            using (var g = Graphics.FromImage(bmp))
            {
                g.CopyFromScreen(bounds.Location, Point.Empty, bounds.Size, CopyPixelOperation.SourceCopy);
            }

        // تصغير الحجم للإرسال السريع
        using (var resized = new Bitmap(bmp, _captureSize))
        using (var ms = new MemoryStream())
        {
            var encoder = GetEncoder(ImageFormat.Jpeg);
            var encoderParams = new EncoderParameters(1);
            encoderParams.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, (long)_jpegQuality);
            resized.Save(ms, encoder, encoderParams);
            return ms.ToArray();
        }
}
}
catch (Exception ex)
{
    OnError?.Invoke(ex);
    return null;
}
}
#endregion

#region Server Side - Request and Display
/// <summary>
/// بدء جلسة مراقبة من السرفر
/// </summary>
public GhostSession StartServerSession(string atmId, string operatorName)
{
    _currentSession = new GhostSession
    {
        SessionID = Guid.NewGuid().ToString("N").Substring(0, 12),
        ATM_ID = atmId,
        OperatorName = operatorName,
        StartTime = DateTime.Now,
        Status = GhostSessionStatus.Connecting,
        IsViewOnly = true,
        ATMUnaffected = true,
        NoLogout = true,
        ScreenNotLocked = true
    };

OnLog?.Invoke($"[Ghost] Server session started for ATM: {atmId} by {operatorName}");
OnSessionStarted?.Invoke(_currentSession);
return _currentSession;
}

/// <summary>
/// إنهاء جلسة المراقبة
/// </summary>
public void EndServerSession()
{
    if (_currentSession != null)
    {
        _currentSession.Status = GhostSessionStatus.Disconnected;
        _currentSession.EndTime = DateTime.Now;
        _currentSession.ActivityLog.Add($"Session ended at {DateTime.Now:HH:mm:ss}");
        OnSessionEnded?.Invoke(_currentSession);
        OnLog?.Invoke($"[Ghost] Session ended: {_currentSession.SessionID}");
    }
_currentSession = null;
}

/// <summary>
/// معالجة إطار مستلم من الكلاينت
/// </summary>
public void ProcessReceivedFrame(byte[] frameData)
{
    if (frameData == null || frameData.Length == 0) return;
    if (_currentSession != null)
    {
        _currentSession.Status = GhostSessionStatus.Active;
        _currentSession.ActivityLog.Add($"Frame received: {frameData.Length} bytes at {DateTime.Now:HH:mm:ss}");
    }
OnFrameReceived?.Invoke(frameData);
}
#endregion

#region Protocol Messages
/// <summary>
/// بناء رسالة طلب بدء Ghost session
/// </summary>
public static byte[] BuildGhostStartRequest(string sessionId, string operatorName)
{
    string msg = $"GHOST_START|{sessionId}|{operatorName}|{DateTime.Now:yyyy-MM-dd HH:mm:ss}";
    return System.Text.Encoding.UTF8.GetBytes(msg);
}

/// <summary>
/// بناء رسالة إيقاف Ghost session
/// </summary>
public static byte[] BuildGhostStopRequest(string sessionId)
{
    string msg = $"GHOST_STOP|{sessionId}|{DateTime.Now:yyyy-MM-dd HH:mm:ss}";
    return System.Text.Encoding.UTF8.GetBytes(msg);
}

/// <summary>
/// بناء رسالة إطار (Frame)
/// </summary>
public static byte[] BuildFrameMessage(string sessionId, byte[] frameData)
{
    string header = $"GHOST_FRAME|{sessionId}|{frameData.Length}|";
    byte[] headerBytes = System.Text.Encoding.UTF8.GetBytes(header);
    byte[] message = new byte[headerBytes.Length + frameData.Length];
    Buffer.BlockCopy(headerBytes, 0, message, 0, headerBytes.Length);
    Buffer.BlockCopy(frameData, 0, message, headerBytes.Length, frameData.Length);
    return message;
}

/// <summary>
/// تحليل رسالة Ghost
/// </summary>
public static GhostMessage ParseGhostMessage(byte[] data)
{
    try
    {
        string text = System.Text.Encoding.UTF8.GetString(data);
        string[] parts = text.Split('|');
        if (parts.Length < 2) return null;

        var msg = new GhostMessage { Type = parts[0], SessionID = parts[1] };
        switch (parts[0])
        {
            case "GHOST_START":
            msg.OperatorName = parts.Length > 2 ? parts[2] : "";
            break;
            case "GHOST_FRAME":
            if (parts.Length > 2 && int.TryParse(parts[2], out int size))
            {
                msg.FrameSize = size;
                // Frame data follows after header
                int headerLen = System.Text.Encoding.UTF8.GetByteCount(string.Join("|", parts[0], parts[1], parts[2]) + "|");
                if (data.Length > headerLen)
                {
                    msg.FrameData = new byte[data.Length - headerLen];
                    Buffer.BlockCopy(data, headerLen, msg.FrameData, 0, msg.FrameData.Length);
                }
        }
    break;
}
return msg;
}
catch { return null; }
}
#endregion

#region Private Methods
private void CaptureLoop()
{
    while (_isStreaming)
    {
        try
        {
            byte[] frame = CaptureFrame();
            if (frame != null && frame.Length > 0)
            {
                OnFrameRequested?.Invoke(frame);
            }
    }
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
Thread.Sleep(_frameIntervalMs);
}
}

private ImageCodecInfo GetEncoder(ImageFormat format)
{
    ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
    foreach (ImageCodecInfo codec in codecs)
    {
        if (codec.FormatID == format.Guid) return codec;
    }
return null;
}
#endregion
}


public class GhostMessage
{
    public string Type { get; set; }
    public string SessionID { get; set; }
    public string OperatorName { get; set; }
    public int FrameSize { get; set; }
    public byte[] FrameData { get; set; }
}

}
public partial class GhostRemoteEngine : IDisposable
{
    private GhostSession _currentSession;


    private bool _isStreaming;


    private Thread _streamThread;


    private int _frameIntervalMs = 1000; // كل ثانية


    private int _jpegQuality = 40;       // جودة منخفضة للسرعة


    private Thread   _captureThread;


    private volatile bool _running;


    private int      _qualityPercent;


    private int      _captureIntervalMs;


    private readonly bool _lowBandwidth;


    private const uint SRCCOPY = 0x00CC0020;


    public bool IsActive => _currentSession != null && _currentSession.Status == GhostSessionStatus.Active;


    public GhostSession CurrentSession => _currentSession;


    public int FrameInterval { get => _frameIntervalMs; set => _frameIntervalMs = Math.Max(200, value); }


    public int JpegQuality { get => _jpegQuality; set => _jpegQuality = Math.Max(10, Math.Min(100, value)); }


    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\src\EJLive.Core\Engine\GhostRemoteEngine.cs
    public bool   IsActive      => _running;


    public int    FramesSent    { get; private set; }


    public long   BytesSent     { get; private set; }


    public double FPS           { get; private set; }


    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\UnifiedEJLiveProject\src\EJLive.Core\Engine\GhostRemoteEngine.cs
    public bool IsActive { get; private set; }


    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\GhostRemoteEngine.cs.v23_bak
    public bool   IsActive      => _running;


    public GhostRemoteEngine(int qualityPercent = 75, bool lowBandwidth = false)
    {
        _qualityPercent    = Clamp(qualityPercent, 10, 100);
        _lowBandwidth      = lowBandwidth;
        _captureIntervalMs = lowBandwidth ? 2000 : 500; // GSM: 1 fps, LAN: 2 fps
    }


private readonly object _lock = new object();


private Size _captureSize = new Size(1024, 768);


public void StartClientStreaming(string sessionId)
{
    if (_isStreaming) return;
    _isStreaming = true;

    _currentSession = new GhostSession
    {
        SessionID = sessionId,
        StartTime = DateTime.Now,
        Status = GhostSessionStatus.Active,
        IsViewOnly = true,
        ATMUnaffected = true,
        NoLogout = true,
        ScreenNotLocked = true
    };

_streamThread = new Thread(CaptureLoop) { IsBackground = true, Name = "GhostCaptureThread" };
_streamThread.Start();

OnLog?.Invoke("[Ghost] Client streaming started - View Only mode (ATM unaffected)");
OnSessionStarted?.Invoke(_currentSession);
}


public void StopClientStreaming()
{
    _isStreaming = false;
    if (_currentSession != null)
    {
        _currentSession.Status = GhostSessionStatus.Disconnected;
        _currentSession.EndTime = DateTime.Now;
        OnSessionEnded?.Invoke(_currentSession);
    }
OnLog?.Invoke("[Ghost] Client streaming stopped");
}


public byte[] CaptureFrame()
{
    try
    {
        var bounds = System.Windows.Forms.Screen.PrimaryScreen.Bounds;
        using (var bmp = new Bitmap(bounds.Width, bounds.Height, PixelFormat.Format24bppRgb))
        {
            using (var g = Graphics.FromImage(bmp))
            {
                g.CopyFromScreen(bounds.Location, Point.Empty, bounds.Size, CopyPixelOperation.SourceCopy);
            }

        // تصغير الحجم للإرسال السريع
        using (var resized = new Bitmap(bmp, _captureSize))
        using (var ms = new MemoryStream())
        {
            var encoder = GetEncoder(ImageFormat.Jpeg);
            var encoderParams = new EncoderParameters(1);
            encoderParams.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, (long)_jpegQuality);
            resized.Save(ms, encoder, encoderParams);
            return ms.ToArray();
        }
}
}
catch (Exception ex)
{
    OnError?.Invoke(ex);
    return null;
}
}


public GhostSession StartServerSession(string atmId, string operatorName)
{
    _currentSession = new GhostSession
    {
        SessionID = Guid.NewGuid().ToString("N").Substring(0, 12),
        ATM_ID = atmId,
        OperatorName = operatorName,
        StartTime = DateTime.Now,
        Status = GhostSessionStatus.Connecting,
        IsViewOnly = true,
        ATMUnaffected = true,
        NoLogout = true,
        ScreenNotLocked = true
    };

OnLog?.Invoke($"[Ghost] Server session started for ATM: {atmId} by {operatorName}");
OnSessionStarted?.Invoke(_currentSession);
return _currentSession;
}


public void EndServerSession()
{
    if (_currentSession != null)
    {
        _currentSession.Status = GhostSessionStatus.Disconnected;
        _currentSession.EndTime = DateTime.Now;
        _currentSession.ActivityLog.Add($"Session ended at {DateTime.Now:HH:mm:ss}");
        OnSessionEnded?.Invoke(_currentSession);
        OnLog?.Invoke($"[Ghost] Session ended: {_currentSession.SessionID}");
    }
_currentSession = null;
}


public void ProcessReceivedFrame(byte[] frameData)
{
    if (frameData == null || frameData.Length == 0) return;
    if (_currentSession != null)
    {
        _currentSession.Status = GhostSessionStatus.Active;
        _currentSession.ActivityLog.Add($"Frame received: {frameData.Length} bytes at {DateTime.Now:HH:mm:ss}");
    }
OnFrameReceived?.Invoke(frameData);
}


public static byte[] BuildGhostStartRequest(string sessionId, string operatorName)
{
    string msg = $"GHOST_START|{sessionId}|{operatorName}|{DateTime.Now:yyyy-MM-dd HH:mm:ss}";
    return System.Text.Encoding.UTF8.GetBytes(msg);
}


public static byte[] BuildGhostStopRequest(string sessionId)
{
    string msg = $"GHOST_STOP|{sessionId}|{DateTime.Now:yyyy-MM-dd HH:mm:ss}";
    return System.Text.Encoding.UTF8.GetBytes(msg);
}


public static byte[] BuildFrameMessage(string sessionId, byte[] frameData)
{
    string header = $"GHOST_FRAME|{sessionId}|{frameData.Length}|";
    byte[] headerBytes = System.Text.Encoding.UTF8.GetBytes(header);
    byte[] message = new byte[headerBytes.Length + frameData.Length];
    Buffer.BlockCopy(headerBytes, 0, message, 0, headerBytes.Length);
    Buffer.BlockCopy(frameData, 0, message, headerBytes.Length, frameData.Length);
    return message;
}


public static GhostMessage ParseGhostMessage(byte[] data)
{
    try
    {
        string text = System.Text.Encoding.UTF8.GetString(data);
        string[] parts = text.Split('|');
        if (parts.Length < 2) return null;

        var msg = new GhostMessage { Type = parts[0], SessionID = parts[1] };
        switch (parts[0])
        {
            case "GHOST_START":
            msg.OperatorName = parts.Length > 2 ? parts[2] : "";
            break;
            case "GHOST_FRAME":
            if (parts.Length > 2 && int.TryParse(parts[2], out int size))
            {
                msg.FrameSize = size;
                // Frame data follows after header
                int headerLen = System.Text.Encoding.UTF8.GetByteCount(string.Join("|", parts[0], parts[1], parts[2]) + "|");
                if (data.Length > headerLen)
                {
                    msg.FrameData = new byte[data.Length - headerLen];
                    Buffer.BlockCopy(data, headerLen, msg.FrameData, 0, msg.FrameData.Length);
                }
        }
    break;
}
return msg;
}
catch { return null; }
}


private void CaptureLoop()
{
    while (_isStreaming)
    {
        try
        {
            byte[] frame = CaptureFrame();
            if (frame != null && frame.Length > 0)
            {
                OnFrameRequested?.Invoke(frame);
            }
    }
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
Thread.Sleep(_frameIntervalMs);
}
}


private ImageCodecInfo GetEncoder(ImageFormat format)
{
    ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
    foreach (ImageCodecInfo codec in codecs)
    {
        if (codec.FormatID == format.Guid) return codec;
    }
return null;
}


public void Start() => IsActive = true;


public void Stop() => IsActive = false;


public void Dispose() => Stop();


[DllImport("user32.dll")] private static extern IntPtr GetDesktopWindow();


// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\src\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("user32.dll")] private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);


[DllImport("gdi32.dll")]  private static extern bool BitBlt(IntPtr hdcDest, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hdcSrc, int nXSrc, int nYSrc, uint dwRop);


// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\src\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("gdi32.dll")]  private static extern bool DeleteObject(IntPtr hObject);


public void SetQuality(int percent, int intervalMs = -1)
{
    _qualityPercent    = Clamp(percent, 10, 100);
    if (intervalMs > 0) _captureIntervalMs = intervalMs;
}


private static int Clamp(int value, int min, int max)
{
    return value < min ? min : (value > max ? max : value);
}


// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\src\EJLive.Core\Engine\GhostRemoteEngine.cs
public void Start()
{
    if (_running) return;
    _running = true;
    _captureThread = new Thread(CaptureLoop)
    {
        IsBackground = true,
        Name         = "EJLive.GhostCapture",
        Priority     = ThreadPriority.BelowNormal // لا يؤثر على الصراف
    };
_captureThread.Start();
OnLog?.Invoke(this, $"Ghost View started — quality={_qualityPercent}% interval={_captureIntervalMs}ms");
}


// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\src\EJLive.Core\Engine\GhostRemoteEngine.cs
public void Stop()
{
    _running = false;
    OnLog?.Invoke(this, "Ghost View stopped.");
}


// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\src\EJLive.Core\Engine\GhostRemoteEngine.cs
private void CaptureLoop()
{
    var lastFrame    = DateTime.UtcNow;
    var frameTracker = 0;
    var lastFpsCalc  = DateTime.UtcNow;

    while (_running)
    {
        try
        {
            var frame = CaptureScreenJpeg();
            if (frame != null && frame.Length > 0)
            {
                OnFrameCaptured?.Invoke(this, frame);
                FramesSent++;
                BytesSent += frame.Length;
                frameTracker++;
            }

        // حساب FPS كل ثانية
        var elapsed = (DateTime.UtcNow - lastFpsCalc).TotalSeconds;
        if (elapsed >= 1.0)
        {
            FPS          = frameTracker / elapsed;
            frameTracker = 0;
            lastFpsCalc  = DateTime.UtcNow;
        }

    Thread.Sleep(_captureIntervalMs);
}
catch (ThreadAbortException) { break; }
catch (Exception ex)
{
    OnError?.Invoke(this, $"Capture error: {ex.Message}");
    Thread.Sleep(2000);
}
}
}


private byte[] CaptureScreenJpeg()
{
    var bounds = GetScreenBounds();
    var width  = bounds.Width;
    var height = bounds.Height;

    // تقليص الدقة للشبكات البطيئة
    if (_lowBandwidth)
    {
        width  = width  / 2;
        height = height / 2;
    }

var hWnd  = GetDesktopWindow();
var hDC   = GetWindowDC(hWnd);
var hMemDC = CreateCompatibleDC(hDC);
var hBmp  = CreateCompatibleBitmap(hDC, width, height);
var hOld  = SelectObject(hMemDC, hBmp);

bool captured = BitBlt(hMemDC, 0, 0, width, height, hDC, bounds.X, bounds.Y, SRCCOPY);

SelectObject(hMemDC, hOld);
DeleteDC(hMemDC);
ReleaseDC(hWnd, hDC);

if (!captured)
{
    DeleteObject(hBmp);
    return null;
}

byte[] jpegBytes = null;
try
{
    using var bmp = Image.FromHbitmap(hBmp);
    using var ms  = new MemoryStream();
    var encoder   = GetJpegEncoder();
    var encParms  = new EncoderParameters(1);
    encParms.Param[0] = new EncoderParameter(Encoder.Quality, (long)_qualityPercent);
    bmp.Save(ms, encoder, encParms);
    jpegBytes = ms.ToArray();
}
finally
{
    DeleteObject(hBmp);
}
return jpegBytes;
}


private System.Drawing.Rectangle GetScreenBounds()
{
    return System.Windows.Forms.Screen.PrimaryScreen?.Bounds
    ?? new System.Drawing.Rectangle(0, 0, 1024, 768);
}


private ImageCodecInfo GetJpegEncoder()
{
    foreach (var codec in ImageCodecInfo.GetImageEncoders())
    if (codec.MimeType == "image/jpeg") return codec;
    return null;
}


// Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\GhostRemoteEngine.cs.v23_bak
[DllImport("user32.dll")] private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);


// Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\GhostRemoteEngine.cs.v23_bak
[DllImport("gdi32.dll")]  private static extern bool DeleteObject(IntPtr hObject);


// Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\GhostRemoteEngine.cs.v23_bak
public void Start()
{
    if (_running) return;
    _running = true;
    _captureThread = new Thread(CaptureLoop)
    {
        IsBackground = true,
        Name         = "EJLive.GhostCapture",
        Priority     = ThreadPriority.BelowNormal // لا يؤثر على الصراف
    };
_captureThread.Start();
OnLog?.Invoke(this, $"Ghost View started — quality={_qualityPercent}% interval={_captureIntervalMs}ms");
}


// Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\GhostRemoteEngine.cs.v23_bak
public void Stop()
{
    _running = false;
    OnLog?.Invoke(this, "Ghost View stopped.");
}


// Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\GhostRemoteEngine.cs.v23_bak
private void CaptureLoop()
{
    var lastFrame    = DateTime.UtcNow;
    var frameTracker = 0;
    var lastFpsCalc  = DateTime.UtcNow;

    while (_running)
    {
        try
        {
            var frame = CaptureScreenJpeg();
            if (frame != null && frame.Length > 0)
            {
                OnFrameCaptured?.Invoke(this, frame);
                FramesSent++;
                BytesSent += frame.Length;
                frameTracker++;
            }

        // حساب FPS كل ثانية
        var elapsed = (DateTime.UtcNow - lastFpsCalc).TotalSeconds;
        if (elapsed >= 1.0)
        {
            FPS          = frameTracker / elapsed;
            frameTracker = 0;
            lastFpsCalc  = DateTime.UtcNow;
        }

    Thread.Sleep(_captureIntervalMs);
}
catch (ThreadAbortException) { break; }
catch (Exception ex)
{
    OnError?.Invoke(this, $"Capture error: {ex.Message}");
    Thread.Sleep(2000);
}
}
}


public event Action<string> OnLog;


public event Action<byte[]> OnFrameReceived;      // Server side: frame from client


public event Action<byte[]> OnFrameRequested;     // Client side: send frame to server


public event Action<GhostSession> OnSessionStarted;


public event Action<GhostSession> OnSessionEnded;


public event Action<Exception> OnError;


public event EventHandler<byte[]>  OnFrameCaptured;   // JPEG bytes


// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\src\EJLive.Core\Engine\GhostRemoteEngine.cs
public event EventHandler<string>  OnError;


// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\src\EJLive.Core\Engine\GhostRemoteEngine.cs
public event EventHandler<string>  OnLog;


// Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\GhostRemoteEngine.cs.v23_bak
public event EventHandler<string>  OnError;


// Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\GhostRemoteEngine.cs.v23_bak
public event EventHandler<string>  OnLog;


public class GhostRemoteEngine
{
#region Events
    public event Action<string> OnLog;
    public event Action<byte[]> OnFrameReceived;      // Server side: frame from client
    public event Action<byte[]> OnFrameRequested;     // Client side: send frame to server
    public event Action<GhostSession> OnSessionStarted;
    public event Action<GhostSession> OnSessionEnded;
    public event Action<Exception> OnError;
#endregion

#region Fields
    private GhostSession _currentSession;
    private bool _isStreaming;
    private Thread _streamThread;
    private readonly object _lock = new object();
    private int _frameIntervalMs = 1000; // كل ثانية
    private int _jpegQuality = 40;       // جودة منخفضة للسرعة
    private Size _captureSize = new Size(1024, 768);
#endregion

#region Properties
    public bool IsActive => _currentSession != null && _currentSession.Status == GhostSessionStatus.Active;
    public GhostSession CurrentSession => _currentSession;
    public int FrameInterval { get => _frameIntervalMs; set => _frameIntervalMs = Math.Max(200, value); }
    public int JpegQuality { get => _jpegQuality; set => _jpegQuality = Math.Max(10, Math.Min(100, value)); }
#endregion

#region Client Side - Capture and Send
    /// <summary>
    /// بدء التقاط الشاشة وإرسالها (يعمل على الكلاينت)
    /// لا يؤثر على الصراف - فقط التقاط بدون تدخل
    /// </summary>
    public void StartClientStreaming(string sessionId)
    {
        if (_isStreaming) return;
        _isStreaming = true;

        _currentSession = new GhostSession
        {
            SessionID = sessionId,
            StartTime = DateTime.Now,
            Status = GhostSessionStatus.Active,
            IsViewOnly = true,
            ATMUnaffected = true,
            NoLogout = true,
            ScreenNotLocked = true
        };

    _streamThread = new Thread(CaptureLoop) { IsBackground = true, Name = "GhostCaptureThread" };
_streamThread.Start();

OnLog?.Invoke("[Ghost] Client streaming started - View Only mode (ATM unaffected)");
OnSessionStarted?.Invoke(_currentSession);
}

/// <summary>
/// إيقاف التقاط الشاشة
/// </summary>
public void StopClientStreaming()
{
    _isStreaming = false;
    if (_currentSession != null)
    {
        _currentSession.Status = GhostSessionStatus.Disconnected;
        _currentSession.EndTime = DateTime.Now;
        OnSessionEnded?.Invoke(_currentSession);
    }
OnLog?.Invoke("[Ghost] Client streaming stopped");
}

/// <summary>
/// التقاط إطار واحد من الشاشة
/// لا يقفل الشاشة ولا يؤثر على العميل
/// </summary>
public byte[] CaptureFrame()
{
    try
    {
        var bounds = System.Windows.Forms.Screen.PrimaryScreen.Bounds;
        using (var bmp = new Bitmap(bounds.Width, bounds.Height, PixelFormat.Format24bppRgb))
        {
            using (var g = Graphics.FromImage(bmp))
            {
                g.CopyFromScreen(bounds.Location, Point.Empty, bounds.Size, CopyPixelOperation.SourceCopy);
            }

        // تصغير الحجم للإرسال السريع
        using (var resized = new Bitmap(bmp, _captureSize))
        using (var ms = new MemoryStream())
        {
            var encoder = GetEncoder(ImageFormat.Jpeg);
            var encoderParams = new EncoderParameters(1);
            encoderParams.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, (long)_jpegQuality);
            resized.Save(ms, encoder, encoderParams);
            return ms.ToArray();
        }
}
}
catch (Exception ex)
{
    OnError?.Invoke(ex);
    return null;
}
}
#endregion

#region Server Side - Request and Display
/// <summary>
/// بدء جلسة مراقبة من السرفر
/// </summary>
public GhostSession StartServerSession(string atmId, string operatorName)
{
    _currentSession = new GhostSession
    {
        SessionID = Guid.NewGuid().ToString("N").Substring(0, 12),
        ATM_ID = atmId,
        OperatorName = operatorName,
        StartTime = DateTime.Now,
        Status = GhostSessionStatus.Connecting,
        IsViewOnly = true,
        ATMUnaffected = true,
        NoLogout = true,
        ScreenNotLocked = true
    };

OnLog?.Invoke($"[Ghost] Server session started for ATM: {atmId} by {operatorName}");
OnSessionStarted?.Invoke(_currentSession);
return _currentSession;
}

/// <summary>
/// إنهاء جلسة المراقبة
/// </summary>
public void EndServerSession()
{
    if (_currentSession != null)
    {
        _currentSession.Status = GhostSessionStatus.Disconnected;
        _currentSession.EndTime = DateTime.Now;
        _currentSession.ActivityLog.Add($"Session ended at {DateTime.Now:HH:mm:ss}");
        OnSessionEnded?.Invoke(_currentSession);
        OnLog?.Invoke($"[Ghost] Session ended: {_currentSession.SessionID}");
    }
_currentSession = null;
}

/// <summary>
/// معالجة إطار مستلم من الكلاينت
/// </summary>
public void ProcessReceivedFrame(byte[] frameData)
{
    if (frameData == null || frameData.Length == 0) return;
    if (_currentSession != null)
    {
        _currentSession.Status = GhostSessionStatus.Active;
        _currentSession.ActivityLog.Add($"Frame received: {frameData.Length} bytes at {DateTime.Now:HH:mm:ss}");
    }
OnFrameReceived?.Invoke(frameData);
}
#endregion

#region Protocol Messages
/// <summary>
/// بناء رسالة طلب بدء Ghost session
/// </summary>
public static byte[] BuildGhostStartRequest(string sessionId, string operatorName)
{
    string msg = $"GHOST_START|{sessionId}|{operatorName}|{DateTime.Now:yyyy-MM-dd HH:mm:ss}";
    return System.Text.Encoding.UTF8.GetBytes(msg);
}

/// <summary>
/// بناء رسالة إيقاف Ghost session
/// </summary>
public static byte[] BuildGhostStopRequest(string sessionId)
{
    string msg = $"GHOST_STOP|{sessionId}|{DateTime.Now:yyyy-MM-dd HH:mm:ss}";
    return System.Text.Encoding.UTF8.GetBytes(msg);
}

/// <summary>
/// بناء رسالة إطار (Frame)
/// </summary>
public static byte[] BuildFrameMessage(string sessionId, byte[] frameData)
{
    string header = $"GHOST_FRAME|{sessionId}|{frameData.Length}|";
    byte[] headerBytes = System.Text.Encoding.UTF8.GetBytes(header);
    byte[] message = new byte[headerBytes.Length + frameData.Length];
    Buffer.BlockCopy(headerBytes, 0, message, 0, headerBytes.Length);
    Buffer.BlockCopy(frameData, 0, message, headerBytes.Length, frameData.Length);
    return message;
}

/// <summary>
/// تحليل رسالة Ghost
/// </summary>
public static GhostMessage ParseGhostMessage(byte[] data)
{
    try
    {
        string text = System.Text.Encoding.UTF8.GetString(data);
        string[] parts = text.Split('|');
        if (parts.Length < 2) return null;

        var msg = new GhostMessage { Type = parts[0], SessionID = parts[1] };
        switch (parts[0])
        {
            case "GHOST_START":
            msg.OperatorName = parts.Length > 2 ? parts[2] : "";
            break;
            case "GHOST_FRAME":
            if (parts.Length > 2 && int.TryParse(parts[2], out int size))
            {
                msg.FrameSize = size;
                // Frame data follows after header
                int headerLen = System.Text.Encoding.UTF8.GetByteCount(string.Join("|", parts[0], parts[1], parts[2]) + "|");
                if (data.Length > headerLen)
                {
                    msg.FrameData = new byte[data.Length - headerLen];
                    Buffer.BlockCopy(data, headerLen, msg.FrameData, 0, msg.FrameData.Length);
                }
        }
    break;
}
return msg;
}
catch { return null; }
}
#endregion

#region Private Methods
private void CaptureLoop()
{
    while (_isStreaming)
    {
        try
        {
            byte[] frame = CaptureFrame();
            if (frame != null && frame.Length > 0)
            {
                OnFrameRequested?.Invoke(frame);
            }
    }
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
Thread.Sleep(_frameIntervalMs);
}
}

private ImageCodecInfo GetEncoder(ImageFormat format)
{
    ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
    foreach (ImageCodecInfo codec in codecs)
    {
        if (codec.FormatID == format.Guid) return codec;
    }
return null;
}
#endregion
}


public class GhostMessage
{
    public string Type { get; set; }
    public string SessionID { get; set; }
    public string OperatorName { get; set; }
    public int FrameSize { get; set; }
    public byte[] FrameData { get; set; }
}

}
public partial class GhostRemoteEngine : IDisposable
{
    private Thread   _captureThread;


    private volatile bool _running;


    private int      _qualityPercent;


    private int      _captureIntervalMs;


    private readonly bool _lowBandwidth;


    private const uint SRCCOPY = 0x00CC0020;


    public int    FramesSent    { get; private set; }


    public long   BytesSent     { get; private set; }


    public double FPS           { get; private set; }


    public bool   IsActive      => _running;


    public GhostRemoteEngine(int qualityPercent = 75, bool lowBandwidth = false)
    {
        _qualityPercent    = Clamp(qualityPercent, 10, 100);
        _lowBandwidth      = lowBandwidth;
        _captureIntervalMs = lowBandwidth ? 2000 : 500; // GSM: 1 fps, LAN: 2 fps
    }


[DllImport("user32.dll")] private static extern IntPtr GetDesktopWindow();


// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLiveEnterprisev4Unified\EJLiveEnterprisev4Unified\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("user32.dll")] private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);


[DllImport("gdi32.dll")]  private static extern bool BitBlt(IntPtr hdcDest, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hdcSrc, int nXSrc, int nYSrc, uint dwRop);


// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLiveEnterprisev4Unified\EJLiveEnterprisev4Unified\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("gdi32.dll")]  private static extern bool DeleteObject(IntPtr hObject);


public void SetQuality(int percent, int intervalMs = -1)
{
    _qualityPercent    = Clamp(percent, 10, 100);
    if (intervalMs > 0) _captureIntervalMs = intervalMs;
}


private static int Clamp(int value, int min, int max)
{
    return value < min ? min : (value > max ? max : value);
}


public void Start()
{
    if (_running) return;
    _running = true;
    _captureThread = new Thread(CaptureLoop)
    {
        IsBackground = true,
        Name         = "EJLive.GhostCapture",
        Priority     = ThreadPriority.BelowNormal // لا يؤثر على الصراف
    };
_captureThread.Start();
OnLog?.Invoke(this, $"Ghost View started — quality={_qualityPercent}% interval={_captureIntervalMs}ms");
}


public void Stop()
{
    _running = false;
    OnLog?.Invoke(this, "Ghost View stopped.");
}


private void CaptureLoop()
{
    var lastFrame    = DateTime.UtcNow;
    var frameTracker = 0;
    var lastFpsCalc  = DateTime.UtcNow;

    while (_running)
    {
        try
        {
            var frame = CaptureScreenJpeg();
            if (frame != null && frame.Length > 0)
            {
                OnFrameCaptured?.Invoke(this, frame);
                FramesSent++;
                BytesSent += frame.Length;
                frameTracker++;
            }

        // حساب FPS كل ثانية
        var elapsed = (DateTime.UtcNow - lastFpsCalc).TotalSeconds;
        if (elapsed >= 1.0)
        {
            FPS          = frameTracker / elapsed;
            frameTracker = 0;
            lastFpsCalc  = DateTime.UtcNow;
        }

    Thread.Sleep(_captureIntervalMs);
}
catch (ThreadAbortException) { break; }
catch (Exception ex)
{
    OnError?.Invoke(this, $"Capture error: {ex.Message}");
    Thread.Sleep(2000);
}
}
}


private byte[] CaptureScreenJpeg()
{
    var bounds = GetScreenBounds();
    var width  = bounds.Width;
    var height = bounds.Height;

    // تقليص الدقة للشبكات البطيئة
    if (_lowBandwidth)
    {
        width  = width  / 2;
        height = height / 2;
    }

var hWnd  = GetDesktopWindow();
var hDC   = GetWindowDC(hWnd);
var hMemDC = CreateCompatibleDC(hDC);
var hBmp  = CreateCompatibleBitmap(hDC, width, height);
var hOld  = SelectObject(hMemDC, hBmp);

bool captured = BitBlt(hMemDC, 0, 0, width, height, hDC, bounds.X, bounds.Y, SRCCOPY);

SelectObject(hMemDC, hOld);
DeleteDC(hMemDC);
ReleaseDC(hWnd, hDC);

if (!captured)
{
    DeleteObject(hBmp);
    return null;
}

byte[] jpegBytes = null;
try
{
    using var bmp = Image.FromHbitmap(hBmp);
    using var ms  = new MemoryStream();
    var encoder   = GetJpegEncoder();
    var encParms  = new EncoderParameters(1);
    encParms.Param[0] = new EncoderParameter(Encoder.Quality, (long)_qualityPercent);
    bmp.Save(ms, encoder, encParms);
    jpegBytes = ms.ToArray();
}
finally
{
    DeleteObject(hBmp);
}
return jpegBytes;
}


private System.Drawing.Rectangle GetScreenBounds()
{
    return System.Windows.Forms.Screen.PrimaryScreen?.Bounds
    ?? new System.Drawing.Rectangle(0, 0, 1024, 768);
}


private ImageCodecInfo GetJpegEncoder()
{
    foreach (var codec in ImageCodecInfo.GetImageEncoders())
    if (codec.MimeType == "image/jpeg") return codec;
    return null;
}


public void Dispose() => Stop();


// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\EJLiveEnterprisev4Unified\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("user32.dll")] private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);


// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\EJLiveEnterprisev4Unified\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("gdi32.dll")]  private static extern bool DeleteObject(IntPtr hObject);


public event EventHandler<byte[]>  OnFrameCaptured;   // JPEG bytes


public event EventHandler<string>  OnError;


public event EventHandler<string>  OnLog;


public class GhostRemoteEngine
{
#region Events
    public event Action<string> OnLog;
    public event Action<byte[]> OnFrameReceived;      // Server side: frame from client
    public event Action<byte[]> OnFrameRequested;     // Client side: send frame to server
    public event Action<GhostSession> OnSessionStarted;
    public event Action<GhostSession> OnSessionEnded;
    public event Action<Exception> OnError;
#endregion

#region Fields
    private GhostSession _currentSession;
    private bool _isStreaming;
    private Thread _streamThread;
    private readonly object _lock = new object();
    private int _frameIntervalMs = 1000; // كل ثانية
    private int _jpegQuality = 40;       // جودة منخفضة للسرعة
    private Size _captureSize = new Size(1024, 768);
#endregion

#region Properties
    public bool IsActive => _currentSession != null && _currentSession.Status == GhostSessionStatus.Active;
    public GhostSession CurrentSession => _currentSession;
    public int FrameInterval { get => _frameIntervalMs; set => _frameIntervalMs = Math.Max(200, value); }
    public int JpegQuality { get => _jpegQuality; set => _jpegQuality = Math.Max(10, Math.Min(100, value)); }
#endregion

#region Client Side - Capture and Send
    /// <summary>
    /// بدء التقاط الشاشة وإرسالها (يعمل على الكلاينت)
    /// لا يؤثر على الصراف - فقط التقاط بدون تدخل
    /// </summary>
    public void StartClientStreaming(string sessionId)
    {
        if (_isStreaming) return;
        _isStreaming = true;

        _currentSession = new GhostSession
        {
            SessionID = sessionId,
            StartTime = DateTime.Now,
            Status = GhostSessionStatus.Active,
            IsViewOnly = true,
            ATMUnaffected = true,
            NoLogout = true,
            ScreenNotLocked = true
        };

    _streamThread = new Thread(CaptureLoop) { IsBackground = true, Name = "GhostCaptureThread" };
_streamThread.Start();

OnLog?.Invoke("[Ghost] Client streaming started - View Only mode (ATM unaffected)");
OnSessionStarted?.Invoke(_currentSession);
}

/// <summary>
/// إيقاف التقاط الشاشة
/// </summary>
public void StopClientStreaming()
{
    _isStreaming = false;
    if (_currentSession != null)
    {
        _currentSession.Status = GhostSessionStatus.Disconnected;
        _currentSession.EndTime = DateTime.Now;
        OnSessionEnded?.Invoke(_currentSession);
    }
OnLog?.Invoke("[Ghost] Client streaming stopped");
}

/// <summary>
/// التقاط إطار واحد من الشاشة
/// لا يقفل الشاشة ولا يؤثر على العميل
/// </summary>
public byte[] CaptureFrame()
{
    try
    {
        var bounds = System.Windows.Forms.Screen.PrimaryScreen.Bounds;
        using (var bmp = new Bitmap(bounds.Width, bounds.Height, PixelFormat.Format24bppRgb))
        {
            using (var g = Graphics.FromImage(bmp))
            {
                g.CopyFromScreen(bounds.Location, Point.Empty, bounds.Size, CopyPixelOperation.SourceCopy);
            }

        // تصغير الحجم للإرسال السريع
        using (var resized = new Bitmap(bmp, _captureSize))
        using (var ms = new MemoryStream())
        {
            var encoder = GetEncoder(ImageFormat.Jpeg);
            var encoderParams = new EncoderParameters(1);
            encoderParams.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, (long)_jpegQuality);
            resized.Save(ms, encoder, encoderParams);
            return ms.ToArray();
        }
}
}
catch (Exception ex)
{
    OnError?.Invoke(ex);
    return null;
}
}
#endregion

#region Server Side - Request and Display
/// <summary>
/// بدء جلسة مراقبة من السرفر
/// </summary>
public GhostSession StartServerSession(string atmId, string operatorName)
{
    _currentSession = new GhostSession
    {
        SessionID = Guid.NewGuid().ToString("N").Substring(0, 12),
        ATM_ID = atmId,
        OperatorName = operatorName,
        StartTime = DateTime.Now,
        Status = GhostSessionStatus.Connecting,
        IsViewOnly = true,
        ATMUnaffected = true,
        NoLogout = true,
        ScreenNotLocked = true
    };

OnLog?.Invoke($"[Ghost] Server session started for ATM: {atmId} by {operatorName}");
OnSessionStarted?.Invoke(_currentSession);
return _currentSession;
}

/// <summary>
/// إنهاء جلسة المراقبة
/// </summary>
public void EndServerSession()
{
    if (_currentSession != null)
    {
        _currentSession.Status = GhostSessionStatus.Disconnected;
        _currentSession.EndTime = DateTime.Now;
        _currentSession.ActivityLog.Add($"Session ended at {DateTime.Now:HH:mm:ss}");
        OnSessionEnded?.Invoke(_currentSession);
        OnLog?.Invoke($"[Ghost] Session ended: {_currentSession.SessionID}");
    }
_currentSession = null;
}

/// <summary>
/// معالجة إطار مستلم من الكلاينت
/// </summary>
public void ProcessReceivedFrame(byte[] frameData)
{
    if (frameData == null || frameData.Length == 0) return;
    if (_currentSession != null)
    {
        _currentSession.Status = GhostSessionStatus.Active;
        _currentSession.ActivityLog.Add($"Frame received: {frameData.Length} bytes at {DateTime.Now:HH:mm:ss}");
    }
OnFrameReceived?.Invoke(frameData);
}
#endregion

#region Protocol Messages
/// <summary>
/// بناء رسالة طلب بدء Ghost session
/// </summary>
public static byte[] BuildGhostStartRequest(string sessionId, string operatorName)
{
    string msg = $"GHOST_START|{sessionId}|{operatorName}|{DateTime.Now:yyyy-MM-dd HH:mm:ss}";
    return System.Text.Encoding.UTF8.GetBytes(msg);
}

/// <summary>
/// بناء رسالة إيقاف Ghost session
/// </summary>
public static byte[] BuildGhostStopRequest(string sessionId)
{
    string msg = $"GHOST_STOP|{sessionId}|{DateTime.Now:yyyy-MM-dd HH:mm:ss}";
    return System.Text.Encoding.UTF8.GetBytes(msg);
}

/// <summary>
/// بناء رسالة إطار (Frame)
/// </summary>
public static byte[] BuildFrameMessage(string sessionId, byte[] frameData)
{
    string header = $"GHOST_FRAME|{sessionId}|{frameData.Length}|";
    byte[] headerBytes = System.Text.Encoding.UTF8.GetBytes(header);
    byte[] message = new byte[headerBytes.Length + frameData.Length];
    Buffer.BlockCopy(headerBytes, 0, message, 0, headerBytes.Length);
    Buffer.BlockCopy(frameData, 0, message, headerBytes.Length, frameData.Length);
    return message;
}

/// <summary>
/// تحليل رسالة Ghost
/// </summary>
public static GhostMessage ParseGhostMessage(byte[] data)
{
    try
    {
        string text = System.Text.Encoding.UTF8.GetString(data);
        string[] parts = text.Split('|');
        if (parts.Length < 2) return null;

        var msg = new GhostMessage { Type = parts[0], SessionID = parts[1] };
        switch (parts[0])
        {
            case "GHOST_START":
            msg.OperatorName = parts.Length > 2 ? parts[2] : "";
            break;
            case "GHOST_FRAME":
            if (parts.Length > 2 && int.TryParse(parts[2], out int size))
            {
                msg.FrameSize = size;
                // Frame data follows after header
                int headerLen = System.Text.Encoding.UTF8.GetByteCount(string.Join("|", parts[0], parts[1], parts[2]) + "|");
                if (data.Length > headerLen)
                {
                    msg.FrameData = new byte[data.Length - headerLen];
                    Buffer.BlockCopy(data, headerLen, msg.FrameData, 0, msg.FrameData.Length);
                }
        }
    break;
}
return msg;
}
catch { return null; }
}
#endregion

#region Private Methods
private void CaptureLoop()
{
    while (_isStreaming)
    {
        try
        {
            byte[] frame = CaptureFrame();
            if (frame != null && frame.Length > 0)
            {
                OnFrameRequested?.Invoke(frame);
            }
    }
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
Thread.Sleep(_frameIntervalMs);
}
}

private ImageCodecInfo GetEncoder(ImageFormat format)
{
    ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
    foreach (ImageCodecInfo codec in codecs)
    {
        if (codec.FormatID == format.Guid) return codec;
    }
return null;
}
#endregion
}


public class GhostMessage
{
    public string Type { get; set; }
    public string SessionID { get; set; }
    public string OperatorName { get; set; }
    public int FrameSize { get; set; }
    public byte[] FrameData { get; set; }
}

}
public partial class GhostRemoteEngine : IDisposable
{
    private Thread   _captureThread;


    private volatile bool _running;


    private int      _qualityPercent;


    private int      _captureIntervalMs;


    private readonly bool _lowBandwidth;


    private const uint SRCCOPY = 0x00CC0020;


    public int    FramesSent    { get; private set; }


    public long   BytesSent     { get; private set; }


    public double FPS           { get; private set; }


    public bool   IsActive      => _running;


    public GhostRemoteEngine(int qualityPercent = 75, bool lowBandwidth = false)
    {
        _qualityPercent    = Clamp(qualityPercent, 10, 100);
        _lowBandwidth      = lowBandwidth;
        _captureIntervalMs = lowBandwidth ? 2000 : 500; // GSM: 1 fps, LAN: 2 fps
    }


[DllImport("user32.dll")] private static extern IntPtr GetDesktopWindow();


// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\EJLive.Unified\src\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("user32.dll")] private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);


[DllImport("gdi32.dll")]  private static extern bool BitBlt(IntPtr hdcDest, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hdcSrc, int nXSrc, int nYSrc, uint dwRop);


// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\EJLive.Unified\src\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("gdi32.dll")]  private static extern bool DeleteObject(IntPtr hObject);


public void SetQuality(int percent, int intervalMs = -1)
{
    _qualityPercent    = Clamp(percent, 10, 100);
    if (intervalMs > 0) _captureIntervalMs = intervalMs;
}


private static int Clamp(int value, int min, int max)
{
    return value < min ? min : (value > max ? max : value);
}


public void Start()
{
    if (_running) return;
    _running = true;
    _captureThread = new Thread(CaptureLoop)
    {
        IsBackground = true,
        Name         = "EJLive.GhostCapture",
        Priority     = ThreadPriority.BelowNormal // لا يؤثر على الصراف
    };
_captureThread.Start();
OnLog?.Invoke(this, $"Ghost View started — quality={_qualityPercent}% interval={_captureIntervalMs}ms");
}


public void Stop()
{
    _running = false;
    OnLog?.Invoke(this, "Ghost View stopped.");
}


private void CaptureLoop()
{
    var lastFrame    = DateTime.UtcNow;
    var frameTracker = 0;
    var lastFpsCalc  = DateTime.UtcNow;

    while (_running)
    {
        try
        {
            var frame = CaptureScreenJpeg();
            if (frame != null && frame.Length > 0)
            {
                OnFrameCaptured?.Invoke(this, frame);
                FramesSent++;
                BytesSent += frame.Length;
                frameTracker++;
            }

        // حساب FPS كل ثانية
        var elapsed = (DateTime.UtcNow - lastFpsCalc).TotalSeconds;
        if (elapsed >= 1.0)
        {
            FPS          = frameTracker / elapsed;
            frameTracker = 0;
            lastFpsCalc  = DateTime.UtcNow;
        }

    Thread.Sleep(_captureIntervalMs);
}
catch (ThreadAbortException) { break; }
catch (Exception ex)
{
    OnError?.Invoke(this, $"Capture error: {ex.Message}");
    Thread.Sleep(2000);
}
}
}


private byte[] CaptureScreenJpeg()
{
    var bounds = GetScreenBounds();
    var width  = bounds.Width;
    var height = bounds.Height;

    // تقليص الدقة للشبكات البطيئة
    if (_lowBandwidth)
    {
        width  = width  / 2;
        height = height / 2;
    }

var hWnd  = GetDesktopWindow();
var hDC   = GetWindowDC(hWnd);
var hMemDC = CreateCompatibleDC(hDC);
var hBmp  = CreateCompatibleBitmap(hDC, width, height);
var hOld  = SelectObject(hMemDC, hBmp);

bool captured = BitBlt(hMemDC, 0, 0, width, height, hDC, bounds.X, bounds.Y, SRCCOPY);

SelectObject(hMemDC, hOld);
DeleteDC(hMemDC);
ReleaseDC(hWnd, hDC);

if (!captured)
{
    DeleteObject(hBmp);
    return null;
}

byte[] jpegBytes = null;
try
{
    using var bmp = Image.FromHbitmap(hBmp);
    using var ms  = new MemoryStream();
    var encoder   = GetJpegEncoder();
    var encParms  = new EncoderParameters(1);
    encParms.Param[0] = new EncoderParameter(Encoder.Quality, (long)_qualityPercent);
    bmp.Save(ms, encoder, encParms);
    jpegBytes = ms.ToArray();
}
finally
{
    DeleteObject(hBmp);
}
return jpegBytes;
}


private System.Drawing.Rectangle GetScreenBounds()
{
    return System.Windows.Forms.Screen.PrimaryScreen?.Bounds
    ?? new System.Drawing.Rectangle(0, 0, 1024, 768);
}


private ImageCodecInfo GetJpegEncoder()
{
    foreach (var codec in ImageCodecInfo.GetImageEncoders())
    if (codec.MimeType == "image/jpeg") return codec;
    return null;
}


public void Dispose() => Stop();


// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive.Unified\EJLive.Unified\src\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("user32.dll")] private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);


// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive.Unified\EJLive.Unified\src\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("gdi32.dll")]  private static extern bool DeleteObject(IntPtr hObject);


// Variant from: d:\EJLIVE\EJlive_Reference_Projects\Ejlive\EJLive.Unified\src\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("user32.dll")] private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);


// Variant from: d:\EJLIVE\EJlive_Reference_Projects\Ejlive\EJLive.Unified\src\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("gdi32.dll")]  private static extern bool DeleteObject(IntPtr hObject);


// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive.Unified.Enhanced\EJLive.Unified\src\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("user32.dll")] private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);


// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive.Unified.Enhanced\EJLive.Unified\src\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("gdi32.dll")]  private static extern bool DeleteObject(IntPtr hObject);


public event EventHandler<byte[]>  OnFrameCaptured;   // JPEG bytes


public event EventHandler<string>  OnError;


public event EventHandler<string>  OnLog;


public class GhostRemoteEngine
{
#region Events
    public event Action<string> OnLog;
    public event Action<byte[]> OnFrameReceived;      // Server side: frame from client
    public event Action<byte[]> OnFrameRequested;     // Client side: send frame to server
    public event Action<GhostSession> OnSessionStarted;
    public event Action<GhostSession> OnSessionEnded;
    public event Action<Exception> OnError;
#endregion

#region Fields
    private GhostSession _currentSession;
    private bool _isStreaming;
    private Thread _streamThread;
    private readonly object _lock = new object();
    private int _frameIntervalMs = 1000; // كل ثانية
    private int _jpegQuality = 40;       // جودة منخفضة للسرعة
    private Size _captureSize = new Size(1024, 768);
#endregion

#region Properties
    public bool IsActive => _currentSession != null && _currentSession.Status == GhostSessionStatus.Active;
    public GhostSession CurrentSession => _currentSession;
    public int FrameInterval { get => _frameIntervalMs; set => _frameIntervalMs = Math.Max(200, value); }
    public int JpegQuality { get => _jpegQuality; set => _jpegQuality = Math.Max(10, Math.Min(100, value)); }
#endregion

#region Client Side - Capture and Send
    /// <summary>
    /// بدء التقاط الشاشة وإرسالها (يعمل على الكلاينت)
    /// لا يؤثر على الصراف - فقط التقاط بدون تدخل
    /// </summary>
    public void StartClientStreaming(string sessionId)
    {
        if (_isStreaming) return;
        _isStreaming = true;

        _currentSession = new GhostSession
        {
            SessionID = sessionId,
            StartTime = DateTime.Now,
            Status = GhostSessionStatus.Active,
            IsViewOnly = true,
            ATMUnaffected = true,
            NoLogout = true,
            ScreenNotLocked = true
        };

    _streamThread = new Thread(CaptureLoop) { IsBackground = true, Name = "GhostCaptureThread" };
_streamThread.Start();

OnLog?.Invoke("[Ghost] Client streaming started - View Only mode (ATM unaffected)");
OnSessionStarted?.Invoke(_currentSession);
}

/// <summary>
/// إيقاف التقاط الشاشة
/// </summary>
public void StopClientStreaming()
{
    _isStreaming = false;
    if (_currentSession != null)
    {
        _currentSession.Status = GhostSessionStatus.Disconnected;
        _currentSession.EndTime = DateTime.Now;
        OnSessionEnded?.Invoke(_currentSession);
    }
OnLog?.Invoke("[Ghost] Client streaming stopped");
}

/// <summary>
/// التقاط إطار واحد من الشاشة
/// لا يقفل الشاشة ولا يؤثر على العميل
/// </summary>
public byte[] CaptureFrame()
{
    try
    {
        var bounds = System.Windows.Forms.Screen.PrimaryScreen.Bounds;
        using (var bmp = new Bitmap(bounds.Width, bounds.Height, PixelFormat.Format24bppRgb))
        {
            using (var g = Graphics.FromImage(bmp))
            {
                g.CopyFromScreen(bounds.Location, Point.Empty, bounds.Size, CopyPixelOperation.SourceCopy);
            }

        // تصغير الحجم للإرسال السريع
        using (var resized = new Bitmap(bmp, _captureSize))
        using (var ms = new MemoryStream())
        {
            var encoder = GetEncoder(ImageFormat.Jpeg);
            var encoderParams = new EncoderParameters(1);
            encoderParams.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, (long)_jpegQuality);
            resized.Save(ms, encoder, encoderParams);
            return ms.ToArray();
        }
}
}
catch (Exception ex)
{
    OnError?.Invoke(ex);
    return null;
}
}
#endregion

#region Server Side - Request and Display
/// <summary>
/// بدء جلسة مراقبة من السرفر
/// </summary>
public GhostSession StartServerSession(string atmId, string operatorName)
{
    _currentSession = new GhostSession
    {
        SessionID = Guid.NewGuid().ToString("N").Substring(0, 12),
        ATM_ID = atmId,
        OperatorName = operatorName,
        StartTime = DateTime.Now,
        Status = GhostSessionStatus.Connecting,
        IsViewOnly = true,
        ATMUnaffected = true,
        NoLogout = true,
        ScreenNotLocked = true
    };

OnLog?.Invoke($"[Ghost] Server session started for ATM: {atmId} by {operatorName}");
OnSessionStarted?.Invoke(_currentSession);
return _currentSession;
}

/// <summary>
/// إنهاء جلسة المراقبة
/// </summary>
public void EndServerSession()
{
    if (_currentSession != null)
    {
        _currentSession.Status = GhostSessionStatus.Disconnected;
        _currentSession.EndTime = DateTime.Now;
        _currentSession.ActivityLog.Add($"Session ended at {DateTime.Now:HH:mm:ss}");
        OnSessionEnded?.Invoke(_currentSession);
        OnLog?.Invoke($"[Ghost] Session ended: {_currentSession.SessionID}");
    }
_currentSession = null;
}

/// <summary>
/// معالجة إطار مستلم من الكلاينت
/// </summary>
public void ProcessReceivedFrame(byte[] frameData)
{
    if (frameData == null || frameData.Length == 0) return;
    if (_currentSession != null)
    {
        _currentSession.Status = GhostSessionStatus.Active;
        _currentSession.ActivityLog.Add($"Frame received: {frameData.Length} bytes at {DateTime.Now:HH:mm:ss}");
    }
OnFrameReceived?.Invoke(frameData);
}
#endregion

#region Protocol Messages
/// <summary>
/// بناء رسالة طلب بدء Ghost session
/// </summary>
public static byte[] BuildGhostStartRequest(string sessionId, string operatorName)
{
    string msg = $"GHOST_START|{sessionId}|{operatorName}|{DateTime.Now:yyyy-MM-dd HH:mm:ss}";
    return System.Text.Encoding.UTF8.GetBytes(msg);
}

/// <summary>
/// بناء رسالة إيقاف Ghost session
/// </summary>
public static byte[] BuildGhostStopRequest(string sessionId)
{
    string msg = $"GHOST_STOP|{sessionId}|{DateTime.Now:yyyy-MM-dd HH:mm:ss}";
    return System.Text.Encoding.UTF8.GetBytes(msg);
}

/// <summary>
/// بناء رسالة إطار (Frame)
/// </summary>
public static byte[] BuildFrameMessage(string sessionId, byte[] frameData)
{
    string header = $"GHOST_FRAME|{sessionId}|{frameData.Length}|";
    byte[] headerBytes = System.Text.Encoding.UTF8.GetBytes(header);
    byte[] message = new byte[headerBytes.Length + frameData.Length];
    Buffer.BlockCopy(headerBytes, 0, message, 0, headerBytes.Length);
    Buffer.BlockCopy(frameData, 0, message, headerBytes.Length, frameData.Length);
    return message;
}

/// <summary>
/// تحليل رسالة Ghost
/// </summary>
public static GhostMessage ParseGhostMessage(byte[] data)
{
    try
    {
        string text = System.Text.Encoding.UTF8.GetString(data);
        string[] parts = text.Split('|');
        if (parts.Length < 2) return null;

        var msg = new GhostMessage { Type = parts[0], SessionID = parts[1] };
        switch (parts[0])
        {
            case "GHOST_START":
            msg.OperatorName = parts.Length > 2 ? parts[2] : "";
            break;
            case "GHOST_FRAME":
            if (parts.Length > 2 && int.TryParse(parts[2], out int size))
            {
                msg.FrameSize = size;
                // Frame data follows after header
                int headerLen = System.Text.Encoding.UTF8.GetByteCount(string.Join("|", parts[0], parts[1], parts[2]) + "|");
                if (data.Length > headerLen)
                {
                    msg.FrameData = new byte[data.Length - headerLen];
                    Buffer.BlockCopy(data, headerLen, msg.FrameData, 0, msg.FrameData.Length);
                }
        }
    break;
}
return msg;
}
catch { return null; }
}
#endregion

#region Private Methods
private void CaptureLoop()
{
    while (_isStreaming)
    {
        try
        {
            byte[] frame = CaptureFrame();
            if (frame != null && frame.Length > 0)
            {
                OnFrameRequested?.Invoke(frame);
            }
    }
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
Thread.Sleep(_frameIntervalMs);
}
}

private ImageCodecInfo GetEncoder(ImageFormat format)
{
    ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
    foreach (ImageCodecInfo codec in codecs)
    {
        if (codec.FormatID == format.Guid) return codec;
    }
return null;
}
#endregion
}


public class GhostMessage
{
    public string Type { get; set; }
    public string SessionID { get; set; }
    public string OperatorName { get; set; }
    public int FrameSize { get; set; }
    public byte[] FrameData { get; set; }
}

}
public partial class GhostRemoteEngine : IDisposable
{
    private Thread   _captureThread;


    private volatile bool _running;


    private int      _qualityPercent;


    private int      _captureIntervalMs;


    private readonly bool _lowBandwidth;


    private const uint SRCCOPY = 0x00CC0020;


    private GhostSession _currentSession;


    private bool _isStreaming;


    private Thread _streamThread;


    private int _frameIntervalMs = 1000; // كل ثانية


    private int _jpegQuality = 40;       // جودة منخفضة للسرعة


    public int    FramesSent    { get; private set; }


    public long   BytesSent     { get; private set; }


    public double FPS           { get; private set; }


    public bool   IsActive      => _running;


    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\EJLive.Core\Engine\GhostRemoteEngine.cs
    public bool IsActive => _currentSession != null && _currentSession.Status == GhostSessionStatus.Active;


    public GhostSession CurrentSession => _currentSession;


    public int FrameInterval { get => _frameIntervalMs; set => _frameIntervalMs = Math.Max(200, value); }


    public int JpegQuality { get => _jpegQuality; set => _jpegQuality = Math.Max(10, Math.Min(100, value)); }


    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Unified_Master\EJLive.Core\Engine\GhostRemoteEngine.cs
    public bool IsActive => _currentSession != null && _currentSession.Status == GhostSessionStatus.Active;


    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Enterprise_v3.4.0\EJLive.Core\Engine\GhostRemoteEngine.cs
    public bool IsActive => _currentSession != null && _currentSession.Status == GhostSessionStatus.Active;


    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Winform\EJLive.Core\Engine\GhostRemoteEngine.cs
    public bool IsActive => _currentSession != null && _currentSession.Status == GhostSessionStatus.Active;


    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_menusai\EJLive.Core\Engine\GhostRemoteEngine.cs
    public bool IsActive => _currentSession != null && _currentSession.Status == GhostSessionStatus.Active;


    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Current_2026-06-10\EJLive.Core\Engine\GhostRemoteEngine.cs
    public bool IsActive => _currentSession != null && _currentSession.Status == GhostSessionStatus.Active;


    public GhostRemoteEngine(int qualityPercent = 75, bool lowBandwidth = false)
    {
        _qualityPercent    = Clamp(qualityPercent, 10, 100);
        _lowBandwidth      = lowBandwidth;
        _captureIntervalMs = lowBandwidth ? 2000 : 500; // GSM: 1 fps, LAN: 2 fps
    }


[DllImport("user32.dll")] private static extern IntPtr GetDesktopWindow();


// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("user32.dll")] private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);


[DllImport("gdi32.dll")]  private static extern bool BitBlt(IntPtr hdcDest, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hdcSrc, int nXSrc, int nYSrc, uint dwRop);


// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("gdi32.dll")]  private static extern bool DeleteObject(IntPtr hObject);


public void SetQuality(int percent, int intervalMs = -1)
{
    _qualityPercent    = Clamp(percent, 10, 100);
    if (intervalMs > 0) _captureIntervalMs = intervalMs;
}


private static int Clamp(int value, int min, int max)
{
    return value < min ? min : (value > max ? max : value);
}


public void Start()
{
    if (_running) return;
    _running = true;
    _captureThread = new Thread(CaptureLoop)
    {
        IsBackground = true,
        Name         = "EJLive.GhostCapture",
        Priority     = ThreadPriority.BelowNormal // لا يؤثر على الصراف
    };
_captureThread.Start();
OnLog?.Invoke(this, $"Ghost View started — quality={_qualityPercent}% interval={_captureIntervalMs}ms");
}


public void Stop()
{
    _running = false;
    OnLog?.Invoke(this, "Ghost View stopped.");
}


private void CaptureLoop()
{
    var lastFrame    = DateTime.UtcNow;
    var frameTracker = 0;
    var lastFpsCalc  = DateTime.UtcNow;

    while (_running)
    {
        try
        {
            var frame = CaptureScreenJpeg();
            if (frame != null && frame.Length > 0)
            {
                OnFrameCaptured?.Invoke(this, frame);
                FramesSent++;
                BytesSent += frame.Length;
                frameTracker++;
            }

        // حساب FPS كل ثانية
        var elapsed = (DateTime.UtcNow - lastFpsCalc).TotalSeconds;
        if (elapsed >= 1.0)
        {
            FPS          = frameTracker / elapsed;
            frameTracker = 0;
            lastFpsCalc  = DateTime.UtcNow;
        }

    Thread.Sleep(_captureIntervalMs);
}
catch (ThreadAbortException) { break; }
catch (Exception ex)
{
    OnError?.Invoke(this, $"Capture error: {ex.Message}");
    Thread.Sleep(2000);
}
}
}


private byte[] CaptureScreenJpeg()
{
    var bounds = GetScreenBounds();
    var width  = bounds.Width;
    var height = bounds.Height;

    // تقليص الدقة للشبكات البطيئة
    if (_lowBandwidth)
    {
        width  = width  / 2;
        height = height / 2;
    }

var hWnd  = GetDesktopWindow();
var hDC   = GetWindowDC(hWnd);
var hMemDC = CreateCompatibleDC(hDC);
var hBmp  = CreateCompatibleBitmap(hDC, width, height);
var hOld  = SelectObject(hMemDC, hBmp);

bool captured = BitBlt(hMemDC, 0, 0, width, height, hDC, bounds.X, bounds.Y, SRCCOPY);

SelectObject(hMemDC, hOld);
DeleteDC(hMemDC);
ReleaseDC(hWnd, hDC);

if (!captured)
{
    DeleteObject(hBmp);
    return null;
}

byte[] jpegBytes = null;
try
{
    using var bmp = Image.FromHbitmap(hBmp);
    using var ms  = new MemoryStream();
    var encoder   = GetJpegEncoder();
    var encParms  = new EncoderParameters(1);
    encParms.Param[0] = new EncoderParameter(Encoder.Quality, (long)_qualityPercent);
    bmp.Save(ms, encoder, encParms);
    jpegBytes = ms.ToArray();
}
finally
{
    DeleteObject(hBmp);
}
return jpegBytes;
}


private System.Drawing.Rectangle GetScreenBounds()
{
    return System.Windows.Forms.Screen.PrimaryScreen?.Bounds
    ?? new System.Drawing.Rectangle(0, 0, 1024, 768);
}


private ImageCodecInfo GetJpegEncoder()
{
    foreach (var codec in ImageCodecInfo.GetImageEncoders())
    if (codec.MimeType == "image/jpeg") return codec;
    return null;
}


public void Dispose() => Stop();


private readonly object _lock = new object();


private Size _captureSize = new Size(1024, 768);


public void StartClientStreaming(string sessionId)
{
    if (_isStreaming) return;
    _isStreaming = true;

    _currentSession = new GhostSession
    {
        SessionID = sessionId,
        StartTime = DateTime.Now,
        Status = GhostSessionStatus.Active,
        IsViewOnly = true,
        ATMUnaffected = true,
        NoLogout = true,
        ScreenNotLocked = true
    };

_streamThread = new Thread(CaptureLoop) { IsBackground = true, Name = "GhostCaptureThread" };
_streamThread.Start();

OnLog?.Invoke("[Ghost] Client streaming started - View Only mode (ATM unaffected)");
OnSessionStarted?.Invoke(_currentSession);
}


public void StopClientStreaming()
{
    _isStreaming = false;
    if (_currentSession != null)
    {
        _currentSession.Status = GhostSessionStatus.Disconnected;
        _currentSession.EndTime = DateTime.Now;
        OnSessionEnded?.Invoke(_currentSession);
    }
OnLog?.Invoke("[Ghost] Client streaming stopped");
}


public byte[] CaptureFrame()
{
    try
    {
        var bounds = System.Windows.Forms.Screen.PrimaryScreen.Bounds;
        using (var bmp = new Bitmap(bounds.Width, bounds.Height, PixelFormat.Format24bppRgb))
        {
            using (var g = Graphics.FromImage(bmp))
            {
                g.CopyFromScreen(bounds.Location, Point.Empty, bounds.Size, CopyPixelOperation.SourceCopy);
            }

        // تصغير الحجم للإرسال السريع
        using (var resized = new Bitmap(bmp, _captureSize))
        using (var ms = new MemoryStream())
        {
            var encoder = GetEncoder(ImageFormat.Jpeg);
            var encoderParams = new EncoderParameters(1);
            encoderParams.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, (long)_jpegQuality);
            resized.Save(ms, encoder, encoderParams);
            return ms.ToArray();
        }
}
}
catch (Exception ex)
{
    OnError?.Invoke(ex);
    return null;
}
}


public GhostSession StartServerSession(string atmId, string operatorName)
{
    _currentSession = new GhostSession
    {
        SessionID = Guid.NewGuid().ToString("N").Substring(0, 12),
        ATM_ID = atmId,
        OperatorName = operatorName,
        StartTime = DateTime.Now,
        Status = GhostSessionStatus.Connecting,
        IsViewOnly = true,
        ATMUnaffected = true,
        NoLogout = true,
        ScreenNotLocked = true
    };

OnLog?.Invoke($"[Ghost] Server session started for ATM: {atmId} by {operatorName}");
OnSessionStarted?.Invoke(_currentSession);
return _currentSession;
}


public void EndServerSession()
{
    if (_currentSession != null)
    {
        _currentSession.Status = GhostSessionStatus.Disconnected;
        _currentSession.EndTime = DateTime.Now;
        _currentSession.ActivityLog.Add($"Session ended at {DateTime.Now:HH:mm:ss}");
        OnSessionEnded?.Invoke(_currentSession);
        OnLog?.Invoke($"[Ghost] Session ended: {_currentSession.SessionID}");
    }
_currentSession = null;
}


public void ProcessReceivedFrame(byte[] frameData)
{
    if (frameData == null || frameData.Length == 0) return;
    if (_currentSession != null)
    {
        _currentSession.Status = GhostSessionStatus.Active;
        _currentSession.ActivityLog.Add($"Frame received: {frameData.Length} bytes at {DateTime.Now:HH:mm:ss}");
    }
OnFrameReceived?.Invoke(frameData);
}


public static byte[] BuildGhostStartRequest(string sessionId, string operatorName)
{
    string msg = $"GHOST_START|{sessionId}|{operatorName}|{DateTime.Now:yyyy-MM-dd HH:mm:ss}";
    return System.Text.Encoding.UTF8.GetBytes(msg);
}


public static byte[] BuildGhostStopRequest(string sessionId)
{
    string msg = $"GHOST_STOP|{sessionId}|{DateTime.Now:yyyy-MM-dd HH:mm:ss}";
    return System.Text.Encoding.UTF8.GetBytes(msg);
}


public static byte[] BuildFrameMessage(string sessionId, byte[] frameData)
{
    string header = $"GHOST_FRAME|{sessionId}|{frameData.Length}|";
    byte[] headerBytes = System.Text.Encoding.UTF8.GetBytes(header);
    byte[] message = new byte[headerBytes.Length + frameData.Length];
    Buffer.BlockCopy(headerBytes, 0, message, 0, headerBytes.Length);
    Buffer.BlockCopy(frameData, 0, message, headerBytes.Length, frameData.Length);
    return message;
}


public static GhostMessage ParseGhostMessage(byte[] data)
{
    try
    {
        string text = System.Text.Encoding.UTF8.GetString(data);
        string[] parts = text.Split('|');
        if (parts.Length < 2) return null;

        var msg = new GhostMessage { Type = parts[0], SessionID = parts[1] };
        switch (parts[0])
        {
            case "GHOST_START":
            msg.OperatorName = parts.Length > 2 ? parts[2] : "";
            break;
            case "GHOST_FRAME":
            if (parts.Length > 2 && int.TryParse(parts[2], out int size))
            {
                msg.FrameSize = size;
                // Frame data follows after header
                int headerLen = System.Text.Encoding.UTF8.GetByteCount(string.Join("|", parts[0], parts[1], parts[2]) + "|");
                if (data.Length > headerLen)
                {
                    msg.FrameData = new byte[data.Length - headerLen];
                    Buffer.BlockCopy(data, headerLen, msg.FrameData, 0, msg.FrameData.Length);
                }
        }
    break;
}
return msg;
}
catch { return null; }
}


// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\EJLive.Core\Engine\GhostRemoteEngine.cs
private void CaptureLoop()
{
    while (_isStreaming)
    {
        try
        {
            byte[] frame = CaptureFrame();
            if (frame != null && frame.Length > 0)
            {
                OnFrameRequested?.Invoke(frame);
            }
    }
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
Thread.Sleep(_frameIntervalMs);
}
}


private ImageCodecInfo GetEncoder(ImageFormat format)
{
    ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
    foreach (ImageCodecInfo codec in codecs)
    {
        if (codec.FormatID == format.Guid) return codec;
    }
return null;
}


// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Unified_Master\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("user32.dll")] private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);


// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Unified_Master\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("gdi32.dll")]  private static extern bool DeleteObject(IntPtr hObject);


// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Unified_Master\EJLive.Core\Engine\GhostRemoteEngine.cs
private void CaptureLoop()
{
    while (_isStreaming)
    {
        try
        {
            byte[] frame = CaptureFrame();
            if (frame != null && frame.Length > 0)
            {
                OnFrameRequested?.Invoke(frame);
            }
    }
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
Thread.Sleep(_frameIntervalMs);
}
}


// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Enterprise_Ultimate\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("user32.dll")] private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);


// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Enterprise_Ultimate\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("gdi32.dll")]  private static extern bool DeleteObject(IntPtr hObject);


// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Enterprise_v3.4.0\EJLive.Core\Engine\GhostRemoteEngine.cs
private void CaptureLoop()
{
    while (_isStreaming)
    {
        try
        {
            byte[] frame = CaptureFrame();
            if (frame != null && frame.Length > 0)
            {
                OnFrameRequested?.Invoke(frame);
            }
    }
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
Thread.Sleep(_frameIntervalMs);
}
}


// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Winform\EJLive.Core\Engine\GhostRemoteEngine.cs
private void CaptureLoop()
{
    while (_isStreaming)
    {
        try
        {
            byte[] frame = CaptureFrame();
            if (frame != null && frame.Length > 0)
            {
                OnFrameRequested?.Invoke(frame);
            }
    }
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
Thread.Sleep(_frameIntervalMs);
}
}


// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_menusai\EJLive.Core\Engine\GhostRemoteEngine.cs
private void CaptureLoop()
{
    while (_isStreaming)
    {
        try
        {
            byte[] frame = CaptureFrame();
            if (frame != null && frame.Length > 0)
            {
                OnFrameRequested?.Invoke(frame);
            }
    }
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
Thread.Sleep(_frameIntervalMs);
}
}


// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Current_2026-06-10\EJLive.Core\Engine\GhostRemoteEngine.cs
private void CaptureLoop()
{
    while (_isStreaming)
    {
        try
        {
            byte[] frame = CaptureFrame();
            if (frame != null && frame.Length > 0)
            {
                OnFrameRequested?.Invoke(frame);
            }
    }
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
Thread.Sleep(_frameIntervalMs);
}
}


public event EventHandler<byte[]>  OnFrameCaptured;   // JPEG bytes


public event EventHandler<string>  OnError;


public event EventHandler<string>  OnLog;


// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\EJLive.Core\Engine\GhostRemoteEngine.cs
public event Action<string> OnLog;


public event Action<byte[]> OnFrameReceived;      // Server side: frame from client


public event Action<byte[]> OnFrameRequested;     // Client side: send frame to server


public event Action<GhostSession> OnSessionStarted;


public event Action<GhostSession> OnSessionEnded;


// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\EJLive.Core\Engine\GhostRemoteEngine.cs
public event Action<Exception> OnError;


// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Unified_Master\EJLive.Core\Engine\GhostRemoteEngine.cs
public event Action<string> OnLog;


// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Unified_Master\EJLive.Core\Engine\GhostRemoteEngine.cs
public event Action<Exception> OnError;


// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Enterprise_v3.4.0\EJLive.Core\Engine\GhostRemoteEngine.cs
public event Action<string> OnLog;


// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Enterprise_v3.4.0\EJLive.Core\Engine\GhostRemoteEngine.cs
public event Action<Exception> OnError;


// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Winform\EJLive.Core\Engine\GhostRemoteEngine.cs
public event Action<string> OnLog;


// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Winform\EJLive.Core\Engine\GhostRemoteEngine.cs
public event Action<Exception> OnError;


// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_menusai\EJLive.Core\Engine\GhostRemoteEngine.cs
public event Action<string> OnLog;


// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_menusai\EJLive.Core\Engine\GhostRemoteEngine.cs
public event Action<Exception> OnError;


// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Current_2026-06-10\EJLive.Core\Engine\GhostRemoteEngine.cs
public event Action<string> OnLog;


// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Current_2026-06-10\EJLive.Core\Engine\GhostRemoteEngine.cs
public event Action<Exception> OnError;

}
public partial class GhostRemoteEngine : IDisposable
{
    private Thread   _captureThread;


    private volatile bool _running;


    private int      _qualityPercent;


    private int      _captureIntervalMs;


    private readonly bool _lowBandwidth;


    private const uint SRCCOPY = 0x00CC0020;


    public int    FramesSent    { get; private set; }


    public long   BytesSent     { get; private set; }


    public double FPS           { get; private set; }


    public bool   IsActive      => _running;


    public GhostRemoteEngine(int qualityPercent = 75, bool lowBandwidth = false)
    {
        _qualityPercent    = Clamp(qualityPercent, 10, 100);
        _lowBandwidth      = lowBandwidth;
        _captureIntervalMs = lowBandwidth ? 2000 : 500; // GSM: 1 fps, LAN: 2 fps
    }


[DllImport("user32.dll")] private static extern IntPtr GetDesktopWindow();


// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("user32.dll")] private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);


[DllImport("gdi32.dll")]  private static extern bool BitBlt(IntPtr hdcDest, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hdcSrc, int nXSrc, int nYSrc, uint dwRop);


// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("gdi32.dll")]  private static extern bool DeleteObject(IntPtr hObject);


public void SetQuality(int percent, int intervalMs = -1)
{
    _qualityPercent    = Clamp(percent, 10, 100);
    if (intervalMs > 0) _captureIntervalMs = intervalMs;
}


private static int Clamp(int value, int min, int max)
{
    return value < min ? min : (value > max ? max : value);
}


public void Start()
{
    if (_running) return;
    _running = true;
    _captureThread = new Thread(CaptureLoop)
    {
        IsBackground = true,
        Name         = "EJLive.GhostCapture",
        Priority     = ThreadPriority.BelowNormal // لا يؤثر على الصراف
    };
_captureThread.Start();
OnLog?.Invoke(this, $"Ghost View started — quality={_qualityPercent}% interval={_captureIntervalMs}ms");
}


public void Stop()
{
    _running = false;
    OnLog?.Invoke(this, "Ghost View stopped.");
}


private void CaptureLoop()
{
    var lastFrame    = DateTime.UtcNow;
    var frameTracker = 0;
    var lastFpsCalc  = DateTime.UtcNow;

    while (_running)
    {
        try
        {
            var frame = CaptureScreenJpeg();
            if (frame != null && frame.Length > 0)
            {
                OnFrameCaptured?.Invoke(this, frame);
                FramesSent++;
                BytesSent += frame.Length;
                frameTracker++;
            }

        // حساب FPS كل ثانية
        var elapsed = (DateTime.UtcNow - lastFpsCalc).TotalSeconds;
        if (elapsed >= 1.0)
        {
            FPS          = frameTracker / elapsed;
            frameTracker = 0;
            lastFpsCalc  = DateTime.UtcNow;
        }

    Thread.Sleep(_captureIntervalMs);
}
catch (ThreadAbortException) { break; }
catch (Exception ex)
{
    OnError?.Invoke(this, $"Capture error: {ex.Message}");
    Thread.Sleep(2000);
}
}
}


private byte[] CaptureScreenJpeg()
{
    var bounds = GetScreenBounds();
    var width  = bounds.Width;
    var height = bounds.Height;

    // تقليص الدقة للشبكات البطيئة
    if (_lowBandwidth)
    {
        width  = width  / 2;
        height = height / 2;
    }

var hWnd  = GetDesktopWindow();
var hDC   = GetWindowDC(hWnd);
var hMemDC = CreateCompatibleDC(hDC);
var hBmp  = CreateCompatibleBitmap(hDC, width, height);
var hOld  = SelectObject(hMemDC, hBmp);

bool captured = BitBlt(hMemDC, 0, 0, width, height, hDC, bounds.X, bounds.Y, SRCCOPY);

SelectObject(hMemDC, hOld);
DeleteDC(hMemDC);
ReleaseDC(hWnd, hDC);

if (!captured)
{
    DeleteObject(hBmp);
    return null;
}

byte[] jpegBytes = null;
try
{
    using var bmp = Image.FromHbitmap(hBmp);
    using var ms  = new MemoryStream();
    var encoder   = GetJpegEncoder();
    var encParms  = new EncoderParameters(1);
    encParms.Param[0] = new EncoderParameter(Encoder.Quality, (long)_qualityPercent);
    bmp.Save(ms, encoder, encParms);
    jpegBytes = ms.ToArray();
}
finally
{
    DeleteObject(hBmp);
}
return jpegBytes;
}


private System.Drawing.Rectangle GetScreenBounds()
{
    return System.Windows.Forms.Screen.PrimaryScreen?.Bounds
    ?? new System.Drawing.Rectangle(0, 0, 1024, 768);
}


private ImageCodecInfo GetJpegEncoder()
{
    foreach (var codec in ImageCodecInfo.GetImageEncoders())
    if (codec.MimeType == "image/jpeg") return codec;
    return null;
}


public void Dispose() => Stop();


// Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_Unified\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("user32.dll")] private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);


// Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_Unified\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("gdi32.dll")]  private static extern bool DeleteObject(IntPtr hObject);


// Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMaregerestructured\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("user32.dll")] private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);


// Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMaregerestructured\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("gdi32.dll")]  private static extern bool DeleteObject(IntPtr hObject);


public event EventHandler<byte[]>  OnFrameCaptured;   // JPEG bytes


public event EventHandler<string>  OnError;


public event EventHandler<string>  OnLog;


public class GhostRemoteEngine
{
#region Events
    public event Action<string> OnLog;
    public event Action<byte[]> OnFrameReceived;      // Server side: frame from client
    public event Action<byte[]> OnFrameRequested;     // Client side: send frame to server
    public event Action<GhostSession> OnSessionStarted;
    public event Action<GhostSession> OnSessionEnded;
    public event Action<Exception> OnError;
#endregion

#region Fields
    private GhostSession _currentSession;
    private bool _isStreaming;
    private Thread _streamThread;
    private readonly object _lock = new object();
    private int _frameIntervalMs = 1000; // كل ثانية
    private int _jpegQuality = 40;       // جودة منخفضة للسرعة
    private Size _captureSize = new Size(1024, 768);
#endregion

#region Properties
    public bool IsActive => _currentSession != null && _currentSession.Status == GhostSessionStatus.Active;
    public GhostSession CurrentSession => _currentSession;
    public int FrameInterval { get => _frameIntervalMs; set => _frameIntervalMs = Math.Max(200, value); }
    public int JpegQuality { get => _jpegQuality; set => _jpegQuality = Math.Max(10, Math.Min(100, value)); }
#endregion

#region Client Side - Capture and Send
    /// <summary>
    /// بدء التقاط الشاشة وإرسالها (يعمل على الكلاينت)
    /// لا يؤثر على الصراف - فقط التقاط بدون تدخل
    /// </summary>
    public void StartClientStreaming(string sessionId)
    {
        if (_isStreaming) return;
        _isStreaming = true;

        _currentSession = new GhostSession
        {
            SessionID = sessionId,
            StartTime = DateTime.Now,
            Status = GhostSessionStatus.Active,
            IsViewOnly = true,
            ATMUnaffected = true,
            NoLogout = true,
            ScreenNotLocked = true
        };

    _streamThread = new Thread(CaptureLoop) { IsBackground = true, Name = "GhostCaptureThread" };
_streamThread.Start();

OnLog?.Invoke("[Ghost] Client streaming started - View Only mode (ATM unaffected)");
OnSessionStarted?.Invoke(_currentSession);
}

/// <summary>
/// إيقاف التقاط الشاشة
/// </summary>
public void StopClientStreaming()
{
    _isStreaming = false;
    if (_currentSession != null)
    {
        _currentSession.Status = GhostSessionStatus.Disconnected;
        _currentSession.EndTime = DateTime.Now;
        OnSessionEnded?.Invoke(_currentSession);
    }
OnLog?.Invoke("[Ghost] Client streaming stopped");
}

/// <summary>
/// التقاط إطار واحد من الشاشة
/// لا يقفل الشاشة ولا يؤثر على العميل
/// </summary>
public byte[] CaptureFrame()
{
    try
    {
        var bounds = System.Windows.Forms.Screen.PrimaryScreen.Bounds;
        using (var bmp = new Bitmap(bounds.Width, bounds.Height, PixelFormat.Format24bppRgb))
        {
            using (var g = Graphics.FromImage(bmp))
            {
                g.CopyFromScreen(bounds.Location, Point.Empty, bounds.Size, CopyPixelOperation.SourceCopy);
            }

        // تصغير الحجم للإرسال السريع
        using (var resized = new Bitmap(bmp, _captureSize))
        using (var ms = new MemoryStream())
        {
            var encoder = GetEncoder(ImageFormat.Jpeg);
            var encoderParams = new EncoderParameters(1);
            encoderParams.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, (long)_jpegQuality);
            resized.Save(ms, encoder, encoderParams);
            return ms.ToArray();
        }
}
}
catch (Exception ex)
{
    OnError?.Invoke(ex);
    return null;
}
}
#endregion

#region Server Side - Request and Display
/// <summary>
/// بدء جلسة مراقبة من السرفر
/// </summary>
public GhostSession StartServerSession(string atmId, string operatorName)
{
    _currentSession = new GhostSession
    {
        SessionID = Guid.NewGuid().ToString("N").Substring(0, 12),
        ATM_ID = atmId,
        OperatorName = operatorName,
        StartTime = DateTime.Now,
        Status = GhostSessionStatus.Connecting,
        IsViewOnly = true,
        ATMUnaffected = true,
        NoLogout = true,
        ScreenNotLocked = true
    };

OnLog?.Invoke($"[Ghost] Server session started for ATM: {atmId} by {operatorName}");
OnSessionStarted?.Invoke(_currentSession);
return _currentSession;
}

/// <summary>
/// إنهاء جلسة المراقبة
/// </summary>
public void EndServerSession()
{
    if (_currentSession != null)
    {
        _currentSession.Status = GhostSessionStatus.Disconnected;
        _currentSession.EndTime = DateTime.Now;
        _currentSession.ActivityLog.Add($"Session ended at {DateTime.Now:HH:mm:ss}");
        OnSessionEnded?.Invoke(_currentSession);
        OnLog?.Invoke($"[Ghost] Session ended: {_currentSession.SessionID}");
    }
_currentSession = null;
}

/// <summary>
/// معالجة إطار مستلم من الكلاينت
/// </summary>
public void ProcessReceivedFrame(byte[] frameData)
{
    if (frameData == null || frameData.Length == 0) return;
    if (_currentSession != null)
    {
        _currentSession.Status = GhostSessionStatus.Active;
        _currentSession.ActivityLog.Add($"Frame received: {frameData.Length} bytes at {DateTime.Now:HH:mm:ss}");
    }
OnFrameReceived?.Invoke(frameData);
}
#endregion

#region Protocol Messages
/// <summary>
/// بناء رسالة طلب بدء Ghost session
/// </summary>
public static byte[] BuildGhostStartRequest(string sessionId, string operatorName)
{
    string msg = $"GHOST_START|{sessionId}|{operatorName}|{DateTime.Now:yyyy-MM-dd HH:mm:ss}";
    return System.Text.Encoding.UTF8.GetBytes(msg);
}

/// <summary>
/// بناء رسالة إيقاف Ghost session
/// </summary>
public static byte[] BuildGhostStopRequest(string sessionId)
{
    string msg = $"GHOST_STOP|{sessionId}|{DateTime.Now:yyyy-MM-dd HH:mm:ss}";
    return System.Text.Encoding.UTF8.GetBytes(msg);
}

/// <summary>
/// بناء رسالة إطار (Frame)
/// </summary>
public static byte[] BuildFrameMessage(string sessionId, byte[] frameData)
{
    string header = $"GHOST_FRAME|{sessionId}|{frameData.Length}|";
    byte[] headerBytes = System.Text.Encoding.UTF8.GetBytes(header);
    byte[] message = new byte[headerBytes.Length + frameData.Length];
    Buffer.BlockCopy(headerBytes, 0, message, 0, headerBytes.Length);
    Buffer.BlockCopy(frameData, 0, message, headerBytes.Length, frameData.Length);
    return message;
}

/// <summary>
/// تحليل رسالة Ghost
/// </summary>
public static GhostMessage ParseGhostMessage(byte[] data)
{
    try
    {
        string text = System.Text.Encoding.UTF8.GetString(data);
        string[] parts = text.Split('|');
        if (parts.Length < 2) return null;

        var msg = new GhostMessage { Type = parts[0], SessionID = parts[1] };
        switch (parts[0])
        {
            case "GHOST_START":
            msg.OperatorName = parts.Length > 2 ? parts[2] : "";
            break;
            case "GHOST_FRAME":
            if (parts.Length > 2 && int.TryParse(parts[2], out int size))
            {
                msg.FrameSize = size;
                // Frame data follows after header
                int headerLen = System.Text.Encoding.UTF8.GetByteCount(string.Join("|", parts[0], parts[1], parts[2]) + "|");
                if (data.Length > headerLen)
                {
                    msg.FrameData = new byte[data.Length - headerLen];
                    Buffer.BlockCopy(data, headerLen, msg.FrameData, 0, msg.FrameData.Length);
                }
        }
    break;
}
return msg;
}
catch { return null; }
}
#endregion

#region Private Methods
private void CaptureLoop()
{
    while (_isStreaming)
    {
        try
        {
            byte[] frame = CaptureFrame();
            if (frame != null && frame.Length > 0)
            {
                OnFrameRequested?.Invoke(frame);
            }
    }
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
Thread.Sleep(_frameIntervalMs);
}
}

private ImageCodecInfo GetEncoder(ImageFormat format)
{
    ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
    foreach (ImageCodecInfo codec in codecs)
    {
        if (codec.FormatID == format.Guid) return codec;
    }
return null;
}
#endregion
}


public class GhostMessage
{
    public string Type { get; set; }
    public string SessionID { get; set; }
    public string OperatorName { get; set; }
    public int FrameSize { get; set; }
    public byte[] FrameData { get; set; }
}

}
public partial class GhostRemoteEngine : IDisposable
{
    private Thread   _captureThread;


    private volatile bool _running;


    private int      _qualityPercent;


    private int      _captureIntervalMs;


    private readonly bool _lowBandwidth;


    private const uint SRCCOPY = 0x00CC0020;


    public int    FramesSent    { get; private set; }


    public long   BytesSent     { get; private set; }


    public double FPS           { get; private set; }


    public bool   IsActive      => _running;


    public GhostRemoteEngine(int qualityPercent = 75, bool lowBandwidth = false)
    {
        _qualityPercent    = Clamp(qualityPercent, 10, 100);
        _lowBandwidth      = lowBandwidth;
        _captureIntervalMs = lowBandwidth ? 2000 : 500; // GSM: 1 fps, LAN: 2 fps
    }


[DllImport("user32.dll")] private static extern IntPtr GetDesktopWindow();


// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\CodexMarege\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("user32.dll")] private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);


[DllImport("gdi32.dll")]  private static extern bool BitBlt(IntPtr hdcDest, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hdcSrc, int nXSrc, int nYSrc, uint dwRop);


// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\CodexMarege\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("gdi32.dll")]  private static extern bool DeleteObject(IntPtr hObject);


public void SetQuality(int percent, int intervalMs = -1)
{
    _qualityPercent    = Clamp(percent, 10, 100);
    if (intervalMs > 0) _captureIntervalMs = intervalMs;
}


private static int Clamp(int value, int min, int max)
{
    return value < min ? min : (value > max ? max : value);
}


public void Start()
{
    if (_running) return;
    _running = true;
    _captureThread = new Thread(CaptureLoop)
    {
        IsBackground = true,
        Name         = "EJLive.GhostCapture",
        Priority     = ThreadPriority.BelowNormal // لا يؤثر على الصراف
    };
_captureThread.Start();
OnLog?.Invoke(this, $"Ghost View started — quality={_qualityPercent}% interval={_captureIntervalMs}ms");
}


public void Stop()
{
    _running = false;
    OnLog?.Invoke(this, "Ghost View stopped.");
}


private void CaptureLoop()
{
    var lastFrame    = DateTime.UtcNow;
    var frameTracker = 0;
    var lastFpsCalc  = DateTime.UtcNow;

    while (_running)
    {
        try
        {
            var frame = CaptureScreenJpeg();
            if (frame != null && frame.Length > 0)
            {
                OnFrameCaptured?.Invoke(this, frame);
                FramesSent++;
                BytesSent += frame.Length;
                frameTracker++;
            }

        // حساب FPS كل ثانية
        var elapsed = (DateTime.UtcNow - lastFpsCalc).TotalSeconds;
        if (elapsed >= 1.0)
        {
            FPS          = frameTracker / elapsed;
            frameTracker = 0;
            lastFpsCalc  = DateTime.UtcNow;
        }

    Thread.Sleep(_captureIntervalMs);
}
catch (ThreadAbortException) { break; }
catch (Exception ex)
{
    OnError?.Invoke(this, $"Capture error: {ex.Message}");
    Thread.Sleep(2000);
}
}
}


private byte[] CaptureScreenJpeg()
{
    var bounds = GetScreenBounds();
    var width  = bounds.Width;
    var height = bounds.Height;

    // تقليص الدقة للشبكات البطيئة
    if (_lowBandwidth)
    {
        width  = width  / 2;
        height = height / 2;
    }

var hWnd  = GetDesktopWindow();
var hDC   = GetWindowDC(hWnd);
var hMemDC = CreateCompatibleDC(hDC);
var hBmp  = CreateCompatibleBitmap(hDC, width, height);
var hOld  = SelectObject(hMemDC, hBmp);

bool captured = BitBlt(hMemDC, 0, 0, width, height, hDC, bounds.X, bounds.Y, SRCCOPY);

SelectObject(hMemDC, hOld);
DeleteDC(hMemDC);
ReleaseDC(hWnd, hDC);

if (!captured)
{
    DeleteObject(hBmp);
    return null;
}

byte[] jpegBytes = null;
try
{
    using var bmp = Image.FromHbitmap(hBmp);
    using var ms  = new MemoryStream();
    var encoder   = GetJpegEncoder();
    var encParms  = new EncoderParameters(1);
    encParms.Param[0] = new EncoderParameter(Encoder.Quality, (long)_qualityPercent);
    bmp.Save(ms, encoder, encParms);
    jpegBytes = ms.ToArray();
}
finally
{
    DeleteObject(hBmp);
}
return jpegBytes;
}


private System.Drawing.Rectangle GetScreenBounds()
{
    return System.Windows.Forms.Screen.PrimaryScreen?.Bounds
    ?? new System.Drawing.Rectangle(0, 0, 1024, 768);
}


private ImageCodecInfo GetJpegEncoder()
{
    foreach (var codec in ImageCodecInfo.GetImageEncoders())
    if (codec.MimeType == "image/jpeg") return codec;
    return null;
}


public void Dispose() => Stop();


// Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_Unified\CodexMarege\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("user32.dll")] private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);


// Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_Unified\CodexMarege\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("gdi32.dll")]  private static extern bool DeleteObject(IntPtr hObject);


// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Enterprise_v4\CodexMarege\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("user32.dll")] private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);


// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Enterprise_v4\CodexMarege\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("gdi32.dll")]  private static extern bool DeleteObject(IntPtr hObject);


// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Enterprise_v4_Unified\CodexMarege\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("user32.dll")] private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);


// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Enterprise_v4_Unified\CodexMarege\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("gdi32.dll")]  private static extern bool DeleteObject(IntPtr hObject);


// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Final\CodexMarege\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("user32.dll")] private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);


// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Final\CodexMarege\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("gdi32.dll")]  private static extern bool DeleteObject(IntPtr hObject);


// Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_new_src\CodexMarege\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("user32.dll")] private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);


// Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_new_src\CodexMarege\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("gdi32.dll")]  private static extern bool DeleteObject(IntPtr hObject);


// Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_restructured\CodexMarege\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("user32.dll")] private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);


// Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_restructured\CodexMarege\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("gdi32.dll")]  private static extern bool DeleteObject(IntPtr hObject);


// Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_1778703792493\CodexMarege\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("user32.dll")] private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);


// Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_1778703792493\CodexMarege\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("gdi32.dll")]  private static extern bool DeleteObject(IntPtr hObject);


// Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_1778791921744\CodexMarege\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("user32.dll")] private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);


// Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_1778791921744\CodexMarege\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("gdi32.dll")]  private static extern bool DeleteObject(IntPtr hObject);


// Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_extracted\CodexMarege\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("user32.dll")] private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);


// Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_extracted\CodexMarege\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("gdi32.dll")]  private static extern bool DeleteObject(IntPtr hObject);


public event EventHandler<byte[]>  OnFrameCaptured;   // JPEG bytes


public event EventHandler<string>  OnError;


public event EventHandler<string>  OnLog;


public class GhostRemoteEngine
{
#region Events
    public event Action<string> OnLog;
    public event Action<byte[]> OnFrameReceived;      // Server side: frame from client
    public event Action<byte[]> OnFrameRequested;     // Client side: send frame to server
    public event Action<GhostSession> OnSessionStarted;
    public event Action<GhostSession> OnSessionEnded;
    public event Action<Exception> OnError;
#endregion

#region Fields
    private GhostSession _currentSession;
    private bool _isStreaming;
    private Thread _streamThread;
    private readonly object _lock = new object();
    private int _frameIntervalMs = 1000; // كل ثانية
    private int _jpegQuality = 40;       // جودة منخفضة للسرعة
    private Size _captureSize = new Size(1024, 768);
#endregion

#region Properties
    public bool IsActive => _currentSession != null && _currentSession.Status == GhostSessionStatus.Active;
    public GhostSession CurrentSession => _currentSession;
    public int FrameInterval { get => _frameIntervalMs; set => _frameIntervalMs = Math.Max(200, value); }
    public int JpegQuality { get => _jpegQuality; set => _jpegQuality = Math.Max(10, Math.Min(100, value)); }
#endregion

#region Client Side - Capture and Send
    /// <summary>
    /// بدء التقاط الشاشة وإرسالها (يعمل على الكلاينت)
    /// لا يؤثر على الصراف - فقط التقاط بدون تدخل
    /// </summary>
    public void StartClientStreaming(string sessionId)
    {
        if (_isStreaming) return;
        _isStreaming = true;

        _currentSession = new GhostSession
        {
            SessionID = sessionId,
            StartTime = DateTime.Now,
            Status = GhostSessionStatus.Active,
            IsViewOnly = true,
            ATMUnaffected = true,
            NoLogout = true,
            ScreenNotLocked = true
        };

    _streamThread = new Thread(CaptureLoop) { IsBackground = true, Name = "GhostCaptureThread" };
_streamThread.Start();

OnLog?.Invoke("[Ghost] Client streaming started - View Only mode (ATM unaffected)");
OnSessionStarted?.Invoke(_currentSession);
}

/// <summary>
/// إيقاف التقاط الشاشة
/// </summary>
public void StopClientStreaming()
{
    _isStreaming = false;
    if (_currentSession != null)
    {
        _currentSession.Status = GhostSessionStatus.Disconnected;
        _currentSession.EndTime = DateTime.Now;
        OnSessionEnded?.Invoke(_currentSession);
    }
OnLog?.Invoke("[Ghost] Client streaming stopped");
}

/// <summary>
/// التقاط إطار واحد من الشاشة
/// لا يقفل الشاشة ولا يؤثر على العميل
/// </summary>
public byte[] CaptureFrame()
{
    try
    {
        var bounds = System.Windows.Forms.Screen.PrimaryScreen.Bounds;
        using (var bmp = new Bitmap(bounds.Width, bounds.Height, PixelFormat.Format24bppRgb))
        {
            using (var g = Graphics.FromImage(bmp))
            {
                g.CopyFromScreen(bounds.Location, Point.Empty, bounds.Size, CopyPixelOperation.SourceCopy);
            }

        // تصغير الحجم للإرسال السريع
        using (var resized = new Bitmap(bmp, _captureSize))
        using (var ms = new MemoryStream())
        {
            var encoder = GetEncoder(ImageFormat.Jpeg);
            var encoderParams = new EncoderParameters(1);
            encoderParams.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, (long)_jpegQuality);
            resized.Save(ms, encoder, encoderParams);
            return ms.ToArray();
        }
}
}
catch (Exception ex)
{
    OnError?.Invoke(ex);
    return null;
}
}
#endregion

#region Server Side - Request and Display
/// <summary>
/// بدء جلسة مراقبة من السرفر
/// </summary>
public GhostSession StartServerSession(string atmId, string operatorName)
{
    _currentSession = new GhostSession
    {
        SessionID = Guid.NewGuid().ToString("N").Substring(0, 12),
        ATM_ID = atmId,
        OperatorName = operatorName,
        StartTime = DateTime.Now,
        Status = GhostSessionStatus.Connecting,
        IsViewOnly = true,
        ATMUnaffected = true,
        NoLogout = true,
        ScreenNotLocked = true
    };

OnLog?.Invoke($"[Ghost] Server session started for ATM: {atmId} by {operatorName}");
OnSessionStarted?.Invoke(_currentSession);
return _currentSession;
}

/// <summary>
/// إنهاء جلسة المراقبة
/// </summary>
public void EndServerSession()
{
    if (_currentSession != null)
    {
        _currentSession.Status = GhostSessionStatus.Disconnected;
        _currentSession.EndTime = DateTime.Now;
        _currentSession.ActivityLog.Add($"Session ended at {DateTime.Now:HH:mm:ss}");
        OnSessionEnded?.Invoke(_currentSession);
        OnLog?.Invoke($"[Ghost] Session ended: {_currentSession.SessionID}");
    }
_currentSession = null;
}

/// <summary>
/// معالجة إطار مستلم من الكلاينت
/// </summary>
public void ProcessReceivedFrame(byte[] frameData)
{
    if (frameData == null || frameData.Length == 0) return;
    if (_currentSession != null)
    {
        _currentSession.Status = GhostSessionStatus.Active;
        _currentSession.ActivityLog.Add($"Frame received: {frameData.Length} bytes at {DateTime.Now:HH:mm:ss}");
    }
OnFrameReceived?.Invoke(frameData);
}
#endregion

#region Protocol Messages
/// <summary>
/// بناء رسالة طلب بدء Ghost session
/// </summary>
public static byte[] BuildGhostStartRequest(string sessionId, string operatorName)
{
    string msg = $"GHOST_START|{sessionId}|{operatorName}|{DateTime.Now:yyyy-MM-dd HH:mm:ss}";
    return System.Text.Encoding.UTF8.GetBytes(msg);
}

/// <summary>
/// بناء رسالة إيقاف Ghost session
/// </summary>
public static byte[] BuildGhostStopRequest(string sessionId)
{
    string msg = $"GHOST_STOP|{sessionId}|{DateTime.Now:yyyy-MM-dd HH:mm:ss}";
    return System.Text.Encoding.UTF8.GetBytes(msg);
}

/// <summary>
/// بناء رسالة إطار (Frame)
/// </summary>
public static byte[] BuildFrameMessage(string sessionId, byte[] frameData)
{
    string header = $"GHOST_FRAME|{sessionId}|{frameData.Length}|";
    byte[] headerBytes = System.Text.Encoding.UTF8.GetBytes(header);
    byte[] message = new byte[headerBytes.Length + frameData.Length];
    Buffer.BlockCopy(headerBytes, 0, message, 0, headerBytes.Length);
    Buffer.BlockCopy(frameData, 0, message, headerBytes.Length, frameData.Length);
    return message;
}

/// <summary>
/// تحليل رسالة Ghost
/// </summary>
public static GhostMessage ParseGhostMessage(byte[] data)
{
    try
    {
        string text = System.Text.Encoding.UTF8.GetString(data);
        string[] parts = text.Split('|');
        if (parts.Length < 2) return null;

        var msg = new GhostMessage { Type = parts[0], SessionID = parts[1] };
        switch (parts[0])
        {
            case "GHOST_START":
            msg.OperatorName = parts.Length > 2 ? parts[2] : "";
            break;
            case "GHOST_FRAME":
            if (parts.Length > 2 && int.TryParse(parts[2], out int size))
            {
                msg.FrameSize = size;
                // Frame data follows after header
                int headerLen = System.Text.Encoding.UTF8.GetByteCount(string.Join("|", parts[0], parts[1], parts[2]) + "|");
                if (data.Length > headerLen)
                {
                    msg.FrameData = new byte[data.Length - headerLen];
                    Buffer.BlockCopy(data, headerLen, msg.FrameData, 0, msg.FrameData.Length);
                }
        }
    break;
}
return msg;
}
catch { return null; }
}
#endregion

#region Private Methods
private void CaptureLoop()
{
    while (_isStreaming)
    {
        try
        {
            byte[] frame = CaptureFrame();
            if (frame != null && frame.Length > 0)
            {
                OnFrameRequested?.Invoke(frame);
            }
    }
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
Thread.Sleep(_frameIntervalMs);
}
}

private ImageCodecInfo GetEncoder(ImageFormat format)
{
    ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
    foreach (ImageCodecInfo codec in codecs)
    {
        if (codec.FormatID == format.Guid) return codec;
    }
return null;
}
#endregion
}


public class GhostMessage
{
    public string Type { get; set; }
    public string SessionID { get; set; }
    public string OperatorName { get; set; }
    public int FrameSize { get; set; }
    public byte[] FrameData { get; set; }
}

}
public partial class GhostRemoteEngine : IDisposable
{
    private Thread _captureThread;


    private volatile bool _running;


    private int _qualityPercent;


    private int _captureIntervalMs;


    private readonly bool _lowBandwidth;


    private const uint SRCCOPY = 0x00CC0020;


    private GhostSession _currentSession;


    private bool _isStreaming;


    private Thread _streamThread;


    private int _frameIntervalMs = 1000; // كل ثانية


    private int _jpegQuality = 40; // جودة منخفضة للسرعة


    public int FrameInterval
    {
        get => _frameIntervalMs;
        set => _frameIntervalMs = Math.Max(200, value);
    }


// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\CodexMarege\CodexMarege\EJLive.Unified\src\EJLive.Core\Engine\GhostRemoteEngine.cs
public int JpegQuality
{
    get => _jpegQuality;
    set => _jpegQuality = Math.Max(10, Math.Min(100, value));
}


// Variant from: d:\EJLIVE\EJlive_Reference_Projects\VBCode_local\CodexMarege\CodexMarege\EJLive.Unified\src\EJLive.Core\Engine\GhostRemoteEngine.cs
public int JpegQuality
{
    get => _jpegQuality;
    set => _jpegQuality = Math.Max(10, Math.Min(100, value));
}


public int FramesSent { get; private set; }


public long BytesSent { get; private set; }


public double FPS { get; private set; }


public bool IsActive => _running;


public bool IsSessionActive => _currentSession != null && _currentSession.Status == GhostSessionStatus.Active;


public GhostSession CurrentSession => _currentSession;


public GhostRemoteEngine(int qualityPercent = 75, bool lowBandwidth = false)
{
    _qualityPercent = Clamp(qualityPercent, 10, 100);
    _lowBandwidth = lowBandwidth;
    _captureIntervalMs = lowBandwidth ? 2000 : 500; // GSM: 1 fps, LAN: 2 fps
}


[DllImport("user32.dll")] private static extern IntPtr GetDesktopWindow();


// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\CodexMarege\CodexMarege\EJLive.Unified\src\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("user32.dll")] private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);


[DllImport("gdi32.dll")] private static extern bool BitBlt(IntPtr hdcDest, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hdcSrc, int nXSrc, int nYSrc, uint dwRop);


// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\CodexMarege\CodexMarege\EJLive.Unified\src\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("gdi32.dll")] private static extern bool DeleteObject(IntPtr hObject);


private readonly object _lock = new object();


private Size _captureSize = new Size(1024, 768);


public void SetQuality(int percent, int intervalMs = -1)
{
    _qualityPercent = Clamp(percent, 10, 100);
    if (intervalMs > 0) _captureIntervalMs = intervalMs;
}


private static int Clamp(int value, int min, int max)
{
    return value < min ? min : (value > max ? max : value);
}


public void Start()
{
    if (_running) return;
    _running = true;
    _captureThread = new Thread(CaptureLoop)
    {
        IsBackground = true,
        Name = "EJLive.GhostCapture",
        Priority = ThreadPriority.BelowNormal // لا يؤثر على الصراف
    };
_captureThread.Start();
OnLog?.Invoke(this, $"Ghost View started — quality={_qualityPercent}% interval={_captureIntervalMs}ms");
}


public void Stop()
{
    _running = false;
    OnLog?.Invoke(this, "Ghost View stopped.");
}


private void CaptureLoop()
{
    var lastFrame = DateTime.UtcNow;
    var frameTracker = 0;
    var lastFpsCalc = DateTime.UtcNow;

    while (_running)
    {
        try
        {
            var frame = CaptureScreenJpeg();
            if (frame != null && frame.Length > 0)
            {
                OnFrameCaptured?.Invoke(this, frame);
                FramesSent++;
                BytesSent += frame.Length;
                frameTracker++;
            }

        // حساب FPS كل ثانية
        var elapsed = (DateTime.UtcNow - lastFpsCalc).TotalSeconds;
        if (elapsed >= 1.0)
        {
            FPS = frameTracker / elapsed;
            frameTracker = 0;
            lastFpsCalc = DateTime.UtcNow;
        }

    Thread.Sleep(_captureIntervalMs);
}
catch (ThreadAbortException)
{
    break;
}
catch (Exception ex)
{
    OnError?.Invoke(this, $"Capture error: {ex.Message}");
    Thread.Sleep(2000);
}
}
}


private byte[] CaptureScreenJpeg()
{
    var bounds = GetScreenBounds();
    var width = bounds.Width;
    var height = bounds.Height;

    // تقليص الدقة للشبكات البطيئة
    if (_lowBandwidth)
    {
        width = width / 2;
        height = height / 2;
    }

var hWnd = GetDesktopWindow();
var hDC = GetWindowDC(hWnd);
var hMemDC = CreateCompatibleDC(hDC);
var hBmp = CreateCompatibleBitmap(hDC, width, height);
var hOld = SelectObject(hMemDC, hBmp);

bool captured = BitBlt(hMemDC, 0, 0, width, height, hDC, bounds.X, bounds.Y, SRCCOPY);

SelectObject(hMemDC, hOld);
DeleteDC(hMemDC);
ReleaseDC(hWnd, hDC);

if (!captured)
{
    DeleteObject(hBmp);
    return null;
}

byte[] jpegBytes = null;
try
{
    using var bmp = Image.FromHbitmap(hBmp);
    using var ms = new MemoryStream();
    var encoder = GetJpegEncoder();
    var encParms = new EncoderParameters(1);
    encParms.Param[0] = new EncoderParameter(Encoder.Quality, (long)_qualityPercent);
    bmp.Save(ms, encoder, encParms);
    jpegBytes = ms.ToArray();
}
finally
{
    DeleteObject(hBmp);
}
return jpegBytes;
}


private System.Drawing.Rectangle GetScreenBounds()
{
    return System.Windows.Forms.Screen.PrimaryScreen?.Bounds ??
    new System.Drawing.Rectangle(0, 0, 1024, 768);
}


private ImageCodecInfo GetJpegEncoder()
{
    foreach (var codec in ImageCodecInfo.GetImageEncoders())
    if (codec.MimeType == "image/jpeg") return codec;
    return null;
}


public void StartClientStreaming(string sessionId)
{
    if (_isStreaming) return;
    _isStreaming = true;

    _currentSession = new GhostSession
    {
        SessionID = sessionId,
        StartTime = DateTime.Now,
        Status = GhostSessionStatus.Active,
        IsViewOnly = true,
        ATMUnaffected = true,
        NoLogout = true,
        ScreenNotLocked = true
    };

_streamThread = new Thread(CaptureLoopClient)
{
    IsBackground = true,
    Name = "GhostCaptureThread"
};
_streamThread.Start();

OnLogAction?.Invoke("[Ghost] Client streaming started - View Only mode (ATM unaffected)");
OnSessionStarted?.Invoke(_currentSession);
}


public void StopClientStreaming()
{
    _isStreaming = false;
    if (_currentSession != null)
    {
        _currentSession.Status = GhostSessionStatus.Disconnected;
        _currentSession.EndTime = DateTime.Now;
        OnSessionEnded?.Invoke(_currentSession);
    }
OnLogAction?.Invoke("[Ghost] Client streaming stopped");
}


public byte[] CaptureFrame()
{
    try
    {
        var bounds = System.Windows.Forms.Screen.PrimaryScreen.Bounds;
        using (var bmp = new Bitmap(bounds.Width, bounds.Height, PixelFormat.Format24bppRgb))
        {
            using (var g = Graphics.FromImage(bmp))
            {
                g.CopyFromScreen(bounds.Location, Point.Empty, bounds.Size, CopyPixelOperation.SourceCopy);
            }

        // تصغير الحجم للإرسال السريع
        using (var resized = new Bitmap(bmp, _captureSize))
        using (var ms = new MemoryStream())
        {
            var encoder = GetEncoder(ImageFormat.Jpeg);
            var encoderParams = new EncoderParameters(1);
            encoderParams.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, (long)_jpegQuality);
            resized.Save(ms, encoder, encoderParams);
            return ms.ToArray();
        }
}
}
catch (Exception ex)
{
    OnException?.Invoke(ex);
    return null;
}
}


public GhostSession StartServerSession(string atmId, string operatorName)
{
    _currentSession = new GhostSession
    {
        SessionID = Guid.NewGuid().ToString("N").Substring(0, 12),
        ATM_ID = atmId,
        OperatorName = operatorName,
        StartTime = DateTime.Now,
        Status = GhostSessionStatus.Connecting,
        IsViewOnly = true,
        ATMUnaffected = true,
        NoLogout = true,
        ScreenNotLocked = true
    };

OnLogAction?.Invoke($"[Ghost] Server session started for ATM: {atmId} by {operatorName}");
OnSessionStarted?.Invoke(_currentSession);
return _currentSession;
}


public void EndServerSession()
{
    if (_currentSession != null)
    {
        _currentSession.Status = GhostSessionStatus.Disconnected;
        _currentSession.EndTime = DateTime.Now;
        _currentSession.ActivityLog.Add($"Session ended at {DateTime.Now:HH:mm:ss}");
        OnSessionEnded?.Invoke(_currentSession);
        OnLogAction?.Invoke($"[Ghost] Session ended: {_currentSession.SessionID}");
    }
_currentSession = null;
}


public void ProcessReceivedFrame(byte[] frameData)
{
    if (frameData == null || frameData.Length == 0) return;
    if (_currentSession != null)
    {
        _currentSession.Status = GhostSessionStatus.Active;
        _currentSession.ActivityLog.Add($"Frame received: {frameData.Length} bytes at {DateTime.Now:HH:mm:ss}");
    }
OnFrameReceived?.Invoke(frameData);
}


public static byte[] BuildGhostStartRequest(string sessionId, string operatorName)
{
    string msg = $"GHOST_START|{sessionId}|{operatorName}|{DateTime.Now:yyyy-MM-dd HH:mm:ss}";
    return System.Text.Encoding.UTF8.GetBytes(msg);
}


public static byte[] BuildGhostStopRequest(string sessionId)
{
    string msg = $"GHOST_STOP|{sessionId}|{DateTime.Now:yyyy-MM-dd HH:mm:ss}";
    return System.Text.Encoding.UTF8.GetBytes(msg);
}


public static byte[] BuildFrameMessage(string sessionId, byte[] frameData)
{
    string header = $"GHOST_FRAME|{sessionId}|{frameData.Length}|";
    byte[] headerBytes = System.Text.Encoding.UTF8.GetBytes(header);
    byte[] message = new byte[headerBytes.Length + frameData.Length];
    Buffer.BlockCopy(headerBytes, 0, message, 0, headerBytes.Length);
    Buffer.BlockCopy(frameData, 0, message, headerBytes.Length, frameData.Length);
    return message;
}


public static GhostMessage ParseGhostMessage(byte[] data)
{
    try
    {
        string text = System.Text.Encoding.UTF8.GetString(data);
        string[] parts = text.Split('|');
        if (parts.Length < 2) return null;

        var msg = new GhostMessage;
        {
            Type = parts[0],
            SessionID = parts[1]
        };
    switch (parts[0])
    {
        case "GHOST_START":
        msg.OperatorName = parts.Length > 2 ? parts[2] : "";
        break;
        case "GHOST_FRAME":
        if (parts.Length > 2 && int.TryParse(parts[2], out int size))
        {
            msg.FrameSize = size;
            // Frame data follows after header
            int headerLen = System.Text.Encoding.UTF8.GetByteCount(string.Join("|", parts[0], parts[1], parts[2]) + "|");
            if (data.Length > headerLen)
            {
                msg.FrameData = new byte[data.Length - headerLen];
                Buffer.BlockCopy(data, headerLen, msg.FrameData, 0, msg.FrameData.Length);
            }
    }
break;
}
return msg;
}
catch
{
    return null;
}
}


private void CaptureLoopClient()
{
    while (_isStreaming)
    {
        try
        {
            byte[] frame = CaptureFrame();
            if (frame != null && frame.Length > 0)
            {
                OnFrameRequested?.Invoke(frame);
            }
    }
catch (Exception ex)
{
    OnException?.Invoke(ex);
}
Thread.Sleep(_frameIntervalMs);
}
}


private ImageCodecInfo GetEncoder(ImageFormat format)
{
    ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
    foreach (ImageCodecInfo codec in codecs)
    {
        if (codec.FormatID == format.Guid) return codec;
    }
return null;
}


public void Dispose() => Stop();


// Variant from: d:\EJLIVE\EJlive_Reference_Projects\VBCode_local\CodexMarege\CodexMarege\EJLive.Unified\src\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("user32.dll")] private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);


// Variant from: d:\EJLIVE\EJlive_Reference_Projects\VBCode_local\CodexMarege\CodexMarege\EJLive.Unified\src\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("gdi32.dll")] private static extern bool DeleteObject(IntPtr hObject);


public event EventHandler<byte[]> OnFrameCaptured; // JPEG bytes


public event EventHandler<string> OnError;


public event EventHandler<string> OnLog;


public event Action<string> OnLogAction;


public event Action<byte[]> OnFrameReceived; // Server side: frame from client


public event Action<byte[]> OnFrameRequested; // Client side: send frame to server


public event Action<GhostSession> OnSessionStarted;


public event Action<GhostSession> OnSessionEnded;


public event Action<Exception> OnException;

}
// Class: GhostRemoteEngine (from 7 sources)
public partial class GhostRemoteEngine : IDisposable
{
    // --- Constants & Fields ---
    private GhostSession _currentSession;

    private bool _isStreaming;

    private Thread _streamThread;

    private int _frameIntervalMs = 1000; // كل ثانية

    private int _jpegQuality = 40;       // جودة منخفضة للسرعة

    private Thread   _captureThread;

    private volatile bool _running;

    private int      _qualityPercent;

    private int      _captureIntervalMs;

    private readonly bool _lowBandwidth;

    private const uint SRCCOPY = 0x00CC0020;


    // --- Properties ---
    public bool IsActive => _currentSession != null && _currentSession.Status == GhostSessionStatus.Active;

    public GhostSession CurrentSession => _currentSession;

    public int FrameInterval { get => _frameIntervalMs; set => _frameIntervalMs = Math.Max(200, value); }

    public int JpegQuality { get => _jpegQuality; set => _jpegQuality = Math.Max(10, Math.Min(100, value)); }

    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\UnifiedEJLiveProject\src\EJLive.Core\Engine\GhostRemoteEngine.cs
    public bool IsActive { get; private set; }

    public int    FramesSent    { get; private set; }

    public long   BytesSent     { get; private set; }

    public double FPS           { get; private set; }

    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\GhostRemoteEngine.cs.v23_bak
    public bool   IsActive      => _running;


    // --- Constructors ---
    public GhostRemoteEngine(int qualityPercent = 75, bool lowBandwidth = false)
    {
        _qualityPercent    = Clamp(qualityPercent, 10, 100);
        _lowBandwidth      = lowBandwidth;
        _captureIntervalMs = lowBandwidth ? 2000 : 500; // GSM: 1 fps, LAN: 2 fps
    }


// --- Methods ---
private readonly object _lock = new object();

private Size _captureSize = new Size(1024, 768);

public void StartClientStreaming(string sessionId)
{
    if (_isStreaming) return;
    _isStreaming = true;

    _currentSession = new GhostSession
    {
        SessionID = sessionId,
        StartTime = DateTime.Now,
        Status = GhostSessionStatus.Active,
        IsViewOnly = true,
        ATMUnaffected = true,
        NoLogout = true,
        ScreenNotLocked = true
    };

_streamThread = new Thread(CaptureLoop) { IsBackground = true, Name = "GhostCaptureThread" };
_streamThread.Start();

OnLog?.Invoke("[Ghost] Client streaming started - View Only mode (ATM unaffected)");
OnSessionStarted?.Invoke(_currentSession);
}

public void StopClientStreaming()
{
    _isStreaming = false;
    if (_currentSession != null)
    {
        _currentSession.Status = GhostSessionStatus.Disconnected;
        _currentSession.EndTime = DateTime.Now;
        OnSessionEnded?.Invoke(_currentSession);
    }
OnLog?.Invoke("[Ghost] Client streaming stopped");
}

public byte[] CaptureFrame()
{
    try
    {
        var bounds = System.Windows.Forms.Screen.PrimaryScreen.Bounds;
        using (var bmp = new Bitmap(bounds.Width, bounds.Height, PixelFormat.Format24bppRgb))
        {
            using (var g = Graphics.FromImage(bmp))
            {
                g.CopyFromScreen(bounds.Location, Point.Empty, bounds.Size, CopyPixelOperation.SourceCopy);
            }

        // تصغير الحجم للإرسال السريع
        using (var resized = new Bitmap(bmp, _captureSize))
        using (var ms = new MemoryStream())
        {
            var encoder = GetEncoder(ImageFormat.Jpeg);
            var encoderParams = new EncoderParameters(1);
            encoderParams.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, (long)_jpegQuality);
            resized.Save(ms, encoder, encoderParams);
            return ms.ToArray();
        }
}
}
catch (Exception ex)
{
    OnError?.Invoke(ex);
    return null;
}
}

public GhostSession StartServerSession(string atmId, string operatorName)
{
    _currentSession = new GhostSession
    {
        SessionID = Guid.NewGuid().ToString("N").Substring(0, 12),
        ATM_ID = atmId,
        OperatorName = operatorName,
        StartTime = DateTime.Now,
        Status = GhostSessionStatus.Connecting,
        IsViewOnly = true,
        ATMUnaffected = true,
        NoLogout = true,
        ScreenNotLocked = true
    };

OnLog?.Invoke($"[Ghost] Server session started for ATM: {atmId} by {operatorName}");
OnSessionStarted?.Invoke(_currentSession);
return _currentSession;
}

public void EndServerSession()
{
    if (_currentSession != null)
    {
        _currentSession.Status = GhostSessionStatus.Disconnected;
        _currentSession.EndTime = DateTime.Now;
        _currentSession.ActivityLog.Add($"Session ended at {DateTime.Now:HH:mm:ss}");
        OnSessionEnded?.Invoke(_currentSession);
        OnLog?.Invoke($"[Ghost] Session ended: {_currentSession.SessionID}");
    }
_currentSession = null;
}

public void ProcessReceivedFrame(byte[] frameData)
{
    if (frameData == null || frameData.Length == 0) return;
    if (_currentSession != null)
    {
        _currentSession.Status = GhostSessionStatus.Active;
        _currentSession.ActivityLog.Add($"Frame received: {frameData.Length} bytes at {DateTime.Now:HH:mm:ss}");
    }
OnFrameReceived?.Invoke(frameData);
}

public static byte[] BuildGhostStartRequest(string sessionId, string operatorName)
{
    string msg = $"GHOST_START|{sessionId}|{operatorName}|{DateTime.Now:yyyy-MM-dd HH:mm:ss}";
    return System.Text.Encoding.UTF8.GetBytes(msg);
}

public static byte[] BuildGhostStopRequest(string sessionId)
{
    string msg = $"GHOST_STOP|{sessionId}|{DateTime.Now:yyyy-MM-dd HH:mm:ss}";
    return System.Text.Encoding.UTF8.GetBytes(msg);
}

public static byte[] BuildFrameMessage(string sessionId, byte[] frameData)
{
    string header = $"GHOST_FRAME|{sessionId}|{frameData.Length}|";
    byte[] headerBytes = System.Text.Encoding.UTF8.GetBytes(header);
    byte[] message = new byte[headerBytes.Length + frameData.Length];
    Buffer.BlockCopy(headerBytes, 0, message, 0, headerBytes.Length);
    Buffer.BlockCopy(frameData, 0, message, headerBytes.Length, frameData.Length);
    return message;
}

public static GhostMessage ParseGhostMessage(byte[] data)
{
    try
    {
        string text = System.Text.Encoding.UTF8.GetString(data);
        string[] parts = text.Split('|');
        if (parts.Length < 2) return null;

        var msg = new GhostMessage { Type = parts[0], SessionID = parts[1] };
        switch (parts[0])
        {
            case "GHOST_START":
            msg.OperatorName = parts.Length > 2 ? parts[2] : "";
            break;
            case "GHOST_FRAME":
            if (parts.Length > 2 && int.TryParse(parts[2], out int size))
            {
                msg.FrameSize = size;
                // Frame data follows after header
                int headerLen = System.Text.Encoding.UTF8.GetByteCount(string.Join("|", parts[0], parts[1], parts[2]) + "|");
                if (data.Length > headerLen)
                {
                    msg.FrameData = new byte[data.Length - headerLen];
                    Buffer.BlockCopy(data, headerLen, msg.FrameData, 0, msg.FrameData.Length);
                }
        }
    break;
}
return msg;
}
catch { return null; }
}

private void CaptureLoop()
{
    while (_isStreaming)
    {
        try
        {
            byte[] frame = CaptureFrame();
            if (frame != null && frame.Length > 0)
            {
                OnFrameRequested?.Invoke(frame);
            }
    }
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
Thread.Sleep(_frameIntervalMs);
}
}

private ImageCodecInfo GetEncoder(ImageFormat format)
{
    ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
    foreach (ImageCodecInfo codec in codecs)
    {
        if (codec.FormatID == format.Guid) return codec;
    }
return null;
}

public void Start() => IsActive = true;

public void Stop() => IsActive = false;

public void Dispose() => Stop();

[DllImport("user32.dll")] private static extern IntPtr GetDesktopWindow();

// Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\GhostRemoteEngine.cs.v23_bak
[DllImport("user32.dll")] private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

[DllImport("gdi32.dll")]  private static extern bool BitBlt(IntPtr hdcDest, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hdcSrc, int nXSrc, int nYSrc, uint dwRop);

// Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\GhostRemoteEngine.cs.v23_bak
[DllImport("gdi32.dll")]  private static extern bool DeleteObject(IntPtr hObject);

public void SetQuality(int percent, int intervalMs = -1)
{
    _qualityPercent    = Clamp(percent, 10, 100);
    if (intervalMs > 0) _captureIntervalMs = intervalMs;
}

private static int Clamp(int value, int min, int max)
{
    return value < min ? min : (value > max ? max : value);
}

// Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\GhostRemoteEngine.cs.v23_bak
public void Start()
{
    if (_running) return;
    _running = true;
    _captureThread = new Thread(CaptureLoop)
    {
        IsBackground = true,
        Name         = "EJLive.GhostCapture",
        Priority     = ThreadPriority.BelowNormal // لا يؤثر على الصراف
    };
_captureThread.Start();
OnLog?.Invoke(this, $"Ghost View started — quality={_qualityPercent}% interval={_captureIntervalMs}ms");
}

// Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\GhostRemoteEngine.cs.v23_bak
public void Stop()
{
    _running = false;
    OnLog?.Invoke(this, "Ghost View stopped.");
}

// Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\GhostRemoteEngine.cs.v23_bak
private void CaptureLoop()
{
    var lastFrame    = DateTime.UtcNow;
    var frameTracker = 0;
    var lastFpsCalc  = DateTime.UtcNow;

    while (_running)
    {
        try
        {
            var frame = CaptureScreenJpeg();
            if (frame != null && frame.Length > 0)
            {
                OnFrameCaptured?.Invoke(this, frame);
                FramesSent++;
                BytesSent += frame.Length;
                frameTracker++;
            }

        // حساب FPS كل ثانية
        var elapsed = (DateTime.UtcNow - lastFpsCalc).TotalSeconds;
        if (elapsed >= 1.0)
        {
            FPS          = frameTracker / elapsed;
            frameTracker = 0;
            lastFpsCalc  = DateTime.UtcNow;
        }

    Thread.Sleep(_captureIntervalMs);
}
catch (ThreadAbortException) { break; }
catch (Exception ex)
{
    OnError?.Invoke(this, $"Capture error: {ex.Message}");
    Thread.Sleep(2000);
}
}
}

private byte[] CaptureScreenJpeg()
{
    var bounds = GetScreenBounds();
    var width  = bounds.Width;
    var height = bounds.Height;

    // تقليص الدقة للشبكات البطيئة
    if (_lowBandwidth)
    {
        width  = width  / 2;
        height = height / 2;
    }

var hWnd  = GetDesktopWindow();
var hDC   = GetWindowDC(hWnd);
var hMemDC = CreateCompatibleDC(hDC);
var hBmp  = CreateCompatibleBitmap(hDC, width, height);
var hOld  = SelectObject(hMemDC, hBmp);

bool captured = BitBlt(hMemDC, 0, 0, width, height, hDC, bounds.X, bounds.Y, SRCCOPY);

SelectObject(hMemDC, hOld);
DeleteDC(hMemDC);
ReleaseDC(hWnd, hDC);

if (!captured)
{
    DeleteObject(hBmp);
    return null;
}

byte[] jpegBytes = null;
try
{
    using var bmp = Image.FromHbitmap(hBmp);
    using var ms  = new MemoryStream();
    var encoder   = GetJpegEncoder();
    var encParms  = new EncoderParameters(1);
    encParms.Param[0] = new EncoderParameter(Encoder.Quality, (long)_qualityPercent);
    bmp.Save(ms, encoder, encParms);
    jpegBytes = ms.ToArray();
}
finally
{
    DeleteObject(hBmp);
}
return jpegBytes;
}

private System.Drawing.Rectangle GetScreenBounds()
{
    return System.Windows.Forms.Screen.PrimaryScreen?.Bounds
    ?? new System.Drawing.Rectangle(0, 0, 1024, 768);
}

private ImageCodecInfo GetJpegEncoder()
{
    foreach (var codec in ImageCodecInfo.GetImageEncoders())
    if (codec.MimeType == "image/jpeg") return codec;
    return null;
}


// --- Events ---
public event Action<string> OnLog;

public event Action<byte[]> OnFrameReceived;      // Server side: frame from client

public event Action<byte[]> OnFrameRequested;     // Client side: send frame to server

public event Action<GhostSession> OnSessionStarted;

public event Action<GhostSession> OnSessionEnded;

public event Action<Exception> OnError;

public event EventHandler<byte[]>  OnFrameCaptured;   // JPEG bytes

// Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\GhostRemoteEngine.cs.v23_bak
public event EventHandler<string>  OnError;

// Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\GhostRemoteEngine.cs.v23_bak
public event EventHandler<string>  OnLog;


// --- Nested Classes ---
public class GhostRemoteEngine
{
#region Events
    public event Action<string> OnLog;
    public event Action<byte[]> OnFrameReceived;      // Server side: frame from client
    public event Action<byte[]> OnFrameRequested;     // Client side: send frame to server
    public event Action<GhostSession> OnSessionStarted;
    public event Action<GhostSession> OnSessionEnded;
    public event Action<Exception> OnError;
#endregion

#region Fields
    private GhostSession _currentSession;
    private bool _isStreaming;
    private Thread _streamThread;
    private readonly object _lock = new object();
    private int _frameIntervalMs = 1000; // كل ثانية
    private int _jpegQuality = 40;       // جودة منخفضة للسرعة
    private Size _captureSize = new Size(1024, 768);
#endregion

#region Properties
    public bool IsActive => _currentSession != null && _currentSession.Status == GhostSessionStatus.Active;
    public GhostSession CurrentSession => _currentSession;
    public int FrameInterval { get => _frameIntervalMs; set => _frameIntervalMs = Math.Max(200, value); }
    public int JpegQuality { get => _jpegQuality; set => _jpegQuality = Math.Max(10, Math.Min(100, value)); }
#endregion

#region Client Side - Capture and Send
    /// <summary>
    /// بدء التقاط الشاشة وإرسالها (يعمل على الكلاينت)
    /// لا يؤثر على الصراف - فقط التقاط بدون تدخل
    /// </summary>
    public void StartClientStreaming(string sessionId)
    {
        if (_isStreaming) return;
        _isStreaming = true;

        _currentSession = new GhostSession
        {
            SessionID = sessionId,
            StartTime = DateTime.Now,
            Status = GhostSessionStatus.Active,
            IsViewOnly = true,
            ATMUnaffected = true,
            NoLogout = true,
            ScreenNotLocked = true
        };

    _streamThread = new Thread(CaptureLoop) { IsBackground = true, Name = "GhostCaptureThread" };
_streamThread.Start();

OnLog?.Invoke("[Ghost] Client streaming started - View Only mode (ATM unaffected)");
OnSessionStarted?.Invoke(_currentSession);
}

/// <summary>
/// إيقاف التقاط الشاشة
/// </summary>
public void StopClientStreaming()
{
    _isStreaming = false;
    if (_currentSession != null)
    {
        _currentSession.Status = GhostSessionStatus.Disconnected;
        _currentSession.EndTime = DateTime.Now;
        OnSessionEnded?.Invoke(_currentSession);
    }
OnLog?.Invoke("[Ghost] Client streaming stopped");
}

/// <summary>
/// التقاط إطار واحد من الشاشة
/// لا يقفل الشاشة ولا يؤثر على العميل
/// </summary>
public byte[] CaptureFrame()
{
    try
    {
        var bounds = System.Windows.Forms.Screen.PrimaryScreen.Bounds;
        using (var bmp = new Bitmap(bounds.Width, bounds.Height, PixelFormat.Format24bppRgb))
        {
            using (var g = Graphics.FromImage(bmp))
            {
                g.CopyFromScreen(bounds.Location, Point.Empty, bounds.Size, CopyPixelOperation.SourceCopy);
            }

        // تصغير الحجم للإرسال السريع
        using (var resized = new Bitmap(bmp, _captureSize))
        using (var ms = new MemoryStream())
        {
            var encoder = GetEncoder(ImageFormat.Jpeg);
            var encoderParams = new EncoderParameters(1);
            encoderParams.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, (long)_jpegQuality);
            resized.Save(ms, encoder, encoderParams);
            return ms.ToArray();
        }
}
}
catch (Exception ex)
{
    OnError?.Invoke(ex);
    return null;
}
}
#endregion

#region Server Side - Request and Display
/// <summary>
/// بدء جلسة مراقبة من السرفر
/// </summary>
public GhostSession StartServerSession(string atmId, string operatorName)
{
    _currentSession = new GhostSession
    {
        SessionID = Guid.NewGuid().ToString("N").Substring(0, 12),
        ATM_ID = atmId,
        OperatorName = operatorName,
        StartTime = DateTime.Now,
        Status = GhostSessionStatus.Connecting,
        IsViewOnly = true,
        ATMUnaffected = true,
        NoLogout = true,
        ScreenNotLocked = true
    };

OnLog?.Invoke($"[Ghost] Server session started for ATM: {atmId} by {operatorName}");
OnSessionStarted?.Invoke(_currentSession);
return _currentSession;
}

/// <summary>
/// إنهاء جلسة المراقبة
/// </summary>
public void EndServerSession()
{
    if (_currentSession != null)
    {
        _currentSession.Status = GhostSessionStatus.Disconnected;
        _currentSession.EndTime = DateTime.Now;
        _currentSession.ActivityLog.Add($"Session ended at {DateTime.Now:HH:mm:ss}");
        OnSessionEnded?.Invoke(_currentSession);
        OnLog?.Invoke($"[Ghost] Session ended: {_currentSession.SessionID}");
    }
_currentSession = null;
}

/// <summary>
/// معالجة إطار مستلم من الكلاينت
/// </summary>
public void ProcessReceivedFrame(byte[] frameData)
{
    if (frameData == null || frameData.Length == 0) return;
    if (_currentSession != null)
    {
        _currentSession.Status = GhostSessionStatus.Active;
        _currentSession.ActivityLog.Add($"Frame received: {frameData.Length} bytes at {DateTime.Now:HH:mm:ss}");
    }
OnFrameReceived?.Invoke(frameData);
}
#endregion

#region Protocol Messages
/// <summary>
/// بناء رسالة طلب بدء Ghost session
/// </summary>
public static byte[] BuildGhostStartRequest(string sessionId, string operatorName)
{
    string msg = $"GHOST_START|{sessionId}|{operatorName}|{DateTime.Now:yyyy-MM-dd HH:mm:ss}";
    return System.Text.Encoding.UTF8.GetBytes(msg);
}

/// <summary>
/// بناء رسالة إيقاف Ghost session
/// </summary>
public static byte[] BuildGhostStopRequest(string sessionId)
{
    string msg = $"GHOST_STOP|{sessionId}|{DateTime.Now:yyyy-MM-dd HH:mm:ss}";
    return System.Text.Encoding.UTF8.GetBytes(msg);
}

/// <summary>
/// بناء رسالة إطار (Frame)
/// </summary>
public static byte[] BuildFrameMessage(string sessionId, byte[] frameData)
{
    string header = $"GHOST_FRAME|{sessionId}|{frameData.Length}|";
    byte[] headerBytes = System.Text.Encoding.UTF8.GetBytes(header);
    byte[] message = new byte[headerBytes.Length + frameData.Length];
    Buffer.BlockCopy(headerBytes, 0, message, 0, headerBytes.Length);
    Buffer.BlockCopy(frameData, 0, message, headerBytes.Length, frameData.Length);
    return message;
}

/// <summary>
/// تحليل رسالة Ghost
/// </summary>
public static GhostMessage ParseGhostMessage(byte[] data)
{
    try
    {
        string text = System.Text.Encoding.UTF8.GetString(data);
        string[] parts = text.Split('|');
        if (parts.Length < 2) return null;

        var msg = new GhostMessage { Type = parts[0], SessionID = parts[1] };
        switch (parts[0])
        {
            case "GHOST_START":
            msg.OperatorName = parts.Length > 2 ? parts[2] : "";
            break;
            case "GHOST_FRAME":
            if (parts.Length > 2 && int.TryParse(parts[2], out int size))
            {
                msg.FrameSize = size;
                // Frame data follows after header
                int headerLen = System.Text.Encoding.UTF8.GetByteCount(string.Join("|", parts[0], parts[1], parts[2]) + "|");
                if (data.Length > headerLen)
                {
                    msg.FrameData = new byte[data.Length - headerLen];
                    Buffer.BlockCopy(data, headerLen, msg.FrameData, 0, msg.FrameData.Length);
                }
        }
    break;
}
return msg;
}
catch { return null; }
}
#endregion

#region Private Methods
private void CaptureLoop()
{
    while (_isStreaming)
    {
        try
        {
            byte[] frame = CaptureFrame();
            if (frame != null && frame.Length > 0)
            {
                OnFrameRequested?.Invoke(frame);
            }
    }
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
Thread.Sleep(_frameIntervalMs);
}
}

private ImageCodecInfo GetEncoder(ImageFormat format)
{
    ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
    foreach (ImageCodecInfo codec in codecs)
    {
        if (codec.FormatID == format.Guid) return codec;
    }
return null;
}
#endregion
}

public class GhostMessage
{
    public string Type { get; set; }
    public string SessionID { get; set; }
    public string OperatorName { get; set; }
    public int FrameSize { get; set; }
    public byte[] FrameData { get; set; }
}

}
// Class: GhostRemoteEngine (from 3 sources)
public partial class GhostRemoteEngine : IDisposable
{
    // --- Constants & Fields ---
    private Thread   _captureThread;

    private volatile bool _running;

    private int      _qualityPercent;

    private int      _captureIntervalMs;

    private readonly bool _lowBandwidth;

    private const uint SRCCOPY = 0x00CC0020;


    // --- Properties ---
    public int    FramesSent    { get; private set; }

    public long   BytesSent     { get; private set; }

    public double FPS           { get; private set; }

    public bool   IsActive      => _running;


    // --- Constructors ---
    public GhostRemoteEngine(int qualityPercent = 75, bool lowBandwidth = false)
    {
        _qualityPercent    = Clamp(qualityPercent, 10, 100);
        _lowBandwidth      = lowBandwidth;
        _captureIntervalMs = lowBandwidth ? 2000 : 500; // GSM: 1 fps, LAN: 2 fps
    }


// --- Methods ---
[DllImport("user32.dll")] private static extern IntPtr GetDesktopWindow();

// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive.Unified\EJLive.Unified\src\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("user32.dll")] private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

[DllImport("gdi32.dll")]  private static extern bool BitBlt(IntPtr hdcDest, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hdcSrc, int nXSrc, int nYSrc, uint dwRop);

// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive.Unified\EJLive.Unified\src\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("gdi32.dll")]  private static extern bool DeleteObject(IntPtr hObject);

public void SetQuality(int percent, int intervalMs = -1)
{
    _qualityPercent    = Clamp(percent, 10, 100);
    if (intervalMs > 0) _captureIntervalMs = intervalMs;
}

private static int Clamp(int value, int min, int max)
{
    return value < min ? min : (value > max ? max : value);
}

public void Start()
{
    if (_running) return;
    _running = true;
    _captureThread = new Thread(CaptureLoop)
    {
        IsBackground = true,
        Name         = "EJLive.GhostCapture",
        Priority     = ThreadPriority.BelowNormal // لا يؤثر على الصراف
    };
_captureThread.Start();
OnLog?.Invoke(this, $"Ghost View started — quality={_qualityPercent}% interval={_captureIntervalMs}ms");
}

public void Stop()
{
    _running = false;
    OnLog?.Invoke(this, "Ghost View stopped.");
}

private void CaptureLoop()
{
    var lastFrame    = DateTime.UtcNow;
    var frameTracker = 0;
    var lastFpsCalc  = DateTime.UtcNow;

    while (_running)
    {
        try
        {
            var frame = CaptureScreenJpeg();
            if (frame != null && frame.Length > 0)
            {
                OnFrameCaptured?.Invoke(this, frame);
                FramesSent++;
                BytesSent += frame.Length;
                frameTracker++;
            }

        // حساب FPS كل ثانية
        var elapsed = (DateTime.UtcNow - lastFpsCalc).TotalSeconds;
        if (elapsed >= 1.0)
        {
            FPS          = frameTracker / elapsed;
            frameTracker = 0;
            lastFpsCalc  = DateTime.UtcNow;
        }

    Thread.Sleep(_captureIntervalMs);
}
catch (ThreadAbortException) { break; }
catch (Exception ex)
{
    OnError?.Invoke(this, $"Capture error: {ex.Message}");
    Thread.Sleep(2000);
}
}
}

private byte[] CaptureScreenJpeg()
{
    var bounds = GetScreenBounds();
    var width  = bounds.Width;
    var height = bounds.Height;

    // تقليص الدقة للشبكات البطيئة
    if (_lowBandwidth)
    {
        width  = width  / 2;
        height = height / 2;
    }

var hWnd  = GetDesktopWindow();
var hDC   = GetWindowDC(hWnd);
var hMemDC = CreateCompatibleDC(hDC);
var hBmp  = CreateCompatibleBitmap(hDC, width, height);
var hOld  = SelectObject(hMemDC, hBmp);

bool captured = BitBlt(hMemDC, 0, 0, width, height, hDC, bounds.X, bounds.Y, SRCCOPY);

SelectObject(hMemDC, hOld);
DeleteDC(hMemDC);
ReleaseDC(hWnd, hDC);

if (!captured)
{
    DeleteObject(hBmp);
    return null;
}

byte[] jpegBytes = null;
try
{
    using var bmp = Image.FromHbitmap(hBmp);
    using var ms  = new MemoryStream();
    var encoder   = GetJpegEncoder();
    var encParms  = new EncoderParameters(1);
    encParms.Param[0] = new EncoderParameter(Encoder.Quality, (long)_qualityPercent);
    bmp.Save(ms, encoder, encParms);
    jpegBytes = ms.ToArray();
}
finally
{
    DeleteObject(hBmp);
}
return jpegBytes;
}

private System.Drawing.Rectangle GetScreenBounds()
{
    return System.Windows.Forms.Screen.PrimaryScreen?.Bounds
    ?? new System.Drawing.Rectangle(0, 0, 1024, 768);
}

private ImageCodecInfo GetJpegEncoder()
{
    foreach (var codec in ImageCodecInfo.GetImageEncoders())
    if (codec.MimeType == "image/jpeg") return codec;
    return null;
}

public void Dispose() => Stop();

// Variant from: d:\EJLIVE\EJlive_Reference_Projects\Ejlive\EJLive.Unified\src\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("user32.dll")] private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

// Variant from: d:\EJLIVE\EJlive_Reference_Projects\Ejlive\EJLive.Unified\src\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("gdi32.dll")]  private static extern bool DeleteObject(IntPtr hObject);

// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive.Unified.Enhanced\EJLive.Unified\src\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("user32.dll")] private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive.Unified.Enhanced\EJLive.Unified\src\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("gdi32.dll")]  private static extern bool DeleteObject(IntPtr hObject);


// --- Events ---
public event EventHandler<byte[]>  OnFrameCaptured;   // JPEG bytes

public event EventHandler<string>  OnError;

public event EventHandler<string>  OnLog;


// --- Nested Classes ---
public class GhostRemoteEngine
{
#region Events
    public event Action<string> OnLog;
    public event Action<byte[]> OnFrameReceived;      // Server side: frame from client
    public event Action<byte[]> OnFrameRequested;     // Client side: send frame to server
    public event Action<GhostSession> OnSessionStarted;
    public event Action<GhostSession> OnSessionEnded;
    public event Action<Exception> OnError;
#endregion

#region Fields
    private GhostSession _currentSession;
    private bool _isStreaming;
    private Thread _streamThread;
    private readonly object _lock = new object();
    private int _frameIntervalMs = 1000; // كل ثانية
    private int _jpegQuality = 40;       // جودة منخفضة للسرعة
    private Size _captureSize = new Size(1024, 768);
#endregion

#region Properties
    public bool IsActive => _currentSession != null && _currentSession.Status == GhostSessionStatus.Active;
    public GhostSession CurrentSession => _currentSession;
    public int FrameInterval { get => _frameIntervalMs; set => _frameIntervalMs = Math.Max(200, value); }
    public int JpegQuality { get => _jpegQuality; set => _jpegQuality = Math.Max(10, Math.Min(100, value)); }
#endregion

#region Client Side - Capture and Send
    /// <summary>
    /// بدء التقاط الشاشة وإرسالها (يعمل على الكلاينت)
    /// لا يؤثر على الصراف - فقط التقاط بدون تدخل
    /// </summary>
    public void StartClientStreaming(string sessionId)
    {
        if (_isStreaming) return;
        _isStreaming = true;

        _currentSession = new GhostSession
        {
            SessionID = sessionId,
            StartTime = DateTime.Now,
            Status = GhostSessionStatus.Active,
            IsViewOnly = true,
            ATMUnaffected = true,
            NoLogout = true,
            ScreenNotLocked = true
        };

    _streamThread = new Thread(CaptureLoop) { IsBackground = true, Name = "GhostCaptureThread" };
_streamThread.Start();

OnLog?.Invoke("[Ghost] Client streaming started - View Only mode (ATM unaffected)");
OnSessionStarted?.Invoke(_currentSession);
}

/// <summary>
/// إيقاف التقاط الشاشة
/// </summary>
public void StopClientStreaming()
{
    _isStreaming = false;
    if (_currentSession != null)
    {
        _currentSession.Status = GhostSessionStatus.Disconnected;
        _currentSession.EndTime = DateTime.Now;
        OnSessionEnded?.Invoke(_currentSession);
    }
OnLog?.Invoke("[Ghost] Client streaming stopped");
}

/// <summary>
/// التقاط إطار واحد من الشاشة
/// لا يقفل الشاشة ولا يؤثر على العميل
/// </summary>
public byte[] CaptureFrame()
{
    try
    {
        var bounds = System.Windows.Forms.Screen.PrimaryScreen.Bounds;
        using (var bmp = new Bitmap(bounds.Width, bounds.Height, PixelFormat.Format24bppRgb))
        {
            using (var g = Graphics.FromImage(bmp))
            {
                g.CopyFromScreen(bounds.Location, Point.Empty, bounds.Size, CopyPixelOperation.SourceCopy);
            }

        // تصغير الحجم للإرسال السريع
        using (var resized = new Bitmap(bmp, _captureSize))
        using (var ms = new MemoryStream())
        {
            var encoder = GetEncoder(ImageFormat.Jpeg);
            var encoderParams = new EncoderParameters(1);
            encoderParams.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, (long)_jpegQuality);
            resized.Save(ms, encoder, encoderParams);
            return ms.ToArray();
        }
}
}
catch (Exception ex)
{
    OnError?.Invoke(ex);
    return null;
}
}
#endregion

#region Server Side - Request and Display
/// <summary>
/// بدء جلسة مراقبة من السرفر
/// </summary>
public GhostSession StartServerSession(string atmId, string operatorName)
{
    _currentSession = new GhostSession
    {
        SessionID = Guid.NewGuid().ToString("N").Substring(0, 12),
        ATM_ID = atmId,
        OperatorName = operatorName,
        StartTime = DateTime.Now,
        Status = GhostSessionStatus.Connecting,
        IsViewOnly = true,
        ATMUnaffected = true,
        NoLogout = true,
        ScreenNotLocked = true
    };

OnLog?.Invoke($"[Ghost] Server session started for ATM: {atmId} by {operatorName}");
OnSessionStarted?.Invoke(_currentSession);
return _currentSession;
}

/// <summary>
/// إنهاء جلسة المراقبة
/// </summary>
public void EndServerSession()
{
    if (_currentSession != null)
    {
        _currentSession.Status = GhostSessionStatus.Disconnected;
        _currentSession.EndTime = DateTime.Now;
        _currentSession.ActivityLog.Add($"Session ended at {DateTime.Now:HH:mm:ss}");
        OnSessionEnded?.Invoke(_currentSession);
        OnLog?.Invoke($"[Ghost] Session ended: {_currentSession.SessionID}");
    }
_currentSession = null;
}

/// <summary>
/// معالجة إطار مستلم من الكلاينت
/// </summary>
public void ProcessReceivedFrame(byte[] frameData)
{
    if (frameData == null || frameData.Length == 0) return;
    if (_currentSession != null)
    {
        _currentSession.Status = GhostSessionStatus.Active;
        _currentSession.ActivityLog.Add($"Frame received: {frameData.Length} bytes at {DateTime.Now:HH:mm:ss}");
    }
OnFrameReceived?.Invoke(frameData);
}
#endregion

#region Protocol Messages
/// <summary>
/// بناء رسالة طلب بدء Ghost session
/// </summary>
public static byte[] BuildGhostStartRequest(string sessionId, string operatorName)
{
    string msg = $"GHOST_START|{sessionId}|{operatorName}|{DateTime.Now:yyyy-MM-dd HH:mm:ss}";
    return System.Text.Encoding.UTF8.GetBytes(msg);
}

/// <summary>
/// بناء رسالة إيقاف Ghost session
/// </summary>
public static byte[] BuildGhostStopRequest(string sessionId)
{
    string msg = $"GHOST_STOP|{sessionId}|{DateTime.Now:yyyy-MM-dd HH:mm:ss}";
    return System.Text.Encoding.UTF8.GetBytes(msg);
}

/// <summary>
/// بناء رسالة إطار (Frame)
/// </summary>
public static byte[] BuildFrameMessage(string sessionId, byte[] frameData)
{
    string header = $"GHOST_FRAME|{sessionId}|{frameData.Length}|";
    byte[] headerBytes = System.Text.Encoding.UTF8.GetBytes(header);
    byte[] message = new byte[headerBytes.Length + frameData.Length];
    Buffer.BlockCopy(headerBytes, 0, message, 0, headerBytes.Length);
    Buffer.BlockCopy(frameData, 0, message, headerBytes.Length, frameData.Length);
    return message;
}

/// <summary>
/// تحليل رسالة Ghost
/// </summary>
public static GhostMessage ParseGhostMessage(byte[] data)
{
    try
    {
        string text = System.Text.Encoding.UTF8.GetString(data);
        string[] parts = text.Split('|');
        if (parts.Length < 2) return null;

        var msg = new GhostMessage { Type = parts[0], SessionID = parts[1] };
        switch (parts[0])
        {
            case "GHOST_START":
            msg.OperatorName = parts.Length > 2 ? parts[2] : "";
            break;
            case "GHOST_FRAME":
            if (parts.Length > 2 && int.TryParse(parts[2], out int size))
            {
                msg.FrameSize = size;
                // Frame data follows after header
                int headerLen = System.Text.Encoding.UTF8.GetByteCount(string.Join("|", parts[0], parts[1], parts[2]) + "|");
                if (data.Length > headerLen)
                {
                    msg.FrameData = new byte[data.Length - headerLen];
                    Buffer.BlockCopy(data, headerLen, msg.FrameData, 0, msg.FrameData.Length);
                }
        }
    break;
}
return msg;
}
catch { return null; }
}
#endregion

#region Private Methods
private void CaptureLoop()
{
    while (_isStreaming)
    {
        try
        {
            byte[] frame = CaptureFrame();
            if (frame != null && frame.Length > 0)
            {
                OnFrameRequested?.Invoke(frame);
            }
    }
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
Thread.Sleep(_frameIntervalMs);
}
}

private ImageCodecInfo GetEncoder(ImageFormat format)
{
    ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
    foreach (ImageCodecInfo codec in codecs)
    {
        if (codec.FormatID == format.Guid) return codec;
    }
return null;
}
#endregion
}

public class GhostMessage
{
    public string Type { get; set; }
    public string SessionID { get; set; }
    public string OperatorName { get; set; }
    public int FrameSize { get; set; }
    public byte[] FrameData { get; set; }
}

}
// Class: GhostRemoteEngine (from 6 sources)
public partial class GhostRemoteEngine : IDisposable
{
    // --- Constants & Fields ---
    private Thread   _captureThread;

    private volatile bool _running;

    private int      _qualityPercent;

    private int      _captureIntervalMs;

    private readonly bool _lowBandwidth;

    private const uint SRCCOPY = 0x00CC0020;

    private GhostSession _currentSession;

    private bool _isStreaming;

    private Thread _streamThread;

    private int _frameIntervalMs = 1000; // كل ثانية

    private int _jpegQuality = 40;       // جودة منخفضة للسرعة


    // --- Properties ---
    public int    FramesSent    { get; private set; }

    public long   BytesSent     { get; private set; }

    public double FPS           { get; private set; }

    public bool   IsActive      => _running;

    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Unified_Master\EJLive.Core\Engine\GhostRemoteEngine.cs
    public bool IsActive => _currentSession != null && _currentSession.Status == GhostSessionStatus.Active;

    public GhostSession CurrentSession => _currentSession;

    public int FrameInterval { get => _frameIntervalMs; set => _frameIntervalMs = Math.Max(200, value); }

    public int JpegQuality { get => _jpegQuality; set => _jpegQuality = Math.Max(10, Math.Min(100, value)); }

    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Enterprise_v3.4.0\EJLive.Core\Engine\GhostRemoteEngine.cs
    public bool IsActive => _currentSession != null && _currentSession.Status == GhostSessionStatus.Active;

    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Winform\EJLive.Core\Engine\GhostRemoteEngine.cs
    public bool IsActive => _currentSession != null && _currentSession.Status == GhostSessionStatus.Active;

    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_menusai\EJLive.Core\Engine\GhostRemoteEngine.cs
    public bool IsActive => _currentSession != null && _currentSession.Status == GhostSessionStatus.Active;

    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Current_2026-06-10\EJLive.Core\Engine\GhostRemoteEngine.cs
    public bool IsActive => _currentSession != null && _currentSession.Status == GhostSessionStatus.Active;


    // --- Constructors ---
    public GhostRemoteEngine(int qualityPercent = 75, bool lowBandwidth = false)
    {
        _qualityPercent    = Clamp(qualityPercent, 10, 100);
        _lowBandwidth      = lowBandwidth;
        _captureIntervalMs = lowBandwidth ? 2000 : 500; // GSM: 1 fps, LAN: 2 fps
    }


// --- Methods ---
[DllImport("user32.dll")] private static extern IntPtr GetDesktopWindow();

// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Unified_Master\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("user32.dll")] private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

[DllImport("gdi32.dll")]  private static extern bool BitBlt(IntPtr hdcDest, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hdcSrc, int nXSrc, int nYSrc, uint dwRop);

// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Unified_Master\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("gdi32.dll")]  private static extern bool DeleteObject(IntPtr hObject);

public void SetQuality(int percent, int intervalMs = -1)
{
    _qualityPercent    = Clamp(percent, 10, 100);
    if (intervalMs > 0) _captureIntervalMs = intervalMs;
}

private static int Clamp(int value, int min, int max)
{
    return value < min ? min : (value > max ? max : value);
}

public void Start()
{
    if (_running) return;
    _running = true;
    _captureThread = new Thread(CaptureLoop)
    {
        IsBackground = true,
        Name         = "EJLive.GhostCapture",
        Priority     = ThreadPriority.BelowNormal // لا يؤثر على الصراف
    };
_captureThread.Start();
OnLog?.Invoke(this, $"Ghost View started — quality={_qualityPercent}% interval={_captureIntervalMs}ms");
}

public void Stop()
{
    _running = false;
    OnLog?.Invoke(this, "Ghost View stopped.");
}

private void CaptureLoop()
{
    var lastFrame    = DateTime.UtcNow;
    var frameTracker = 0;
    var lastFpsCalc  = DateTime.UtcNow;

    while (_running)
    {
        try
        {
            var frame = CaptureScreenJpeg();
            if (frame != null && frame.Length > 0)
            {
                OnFrameCaptured?.Invoke(this, frame);
                FramesSent++;
                BytesSent += frame.Length;
                frameTracker++;
            }

        // حساب FPS كل ثانية
        var elapsed = (DateTime.UtcNow - lastFpsCalc).TotalSeconds;
        if (elapsed >= 1.0)
        {
            FPS          = frameTracker / elapsed;
            frameTracker = 0;
            lastFpsCalc  = DateTime.UtcNow;
        }

    Thread.Sleep(_captureIntervalMs);
}
catch (ThreadAbortException) { break; }
catch (Exception ex)
{
    OnError?.Invoke(this, $"Capture error: {ex.Message}");
    Thread.Sleep(2000);
}
}
}

private byte[] CaptureScreenJpeg()
{
    var bounds = GetScreenBounds();
    var width  = bounds.Width;
    var height = bounds.Height;

    // تقليص الدقة للشبكات البطيئة
    if (_lowBandwidth)
    {
        width  = width  / 2;
        height = height / 2;
    }

var hWnd  = GetDesktopWindow();
var hDC   = GetWindowDC(hWnd);
var hMemDC = CreateCompatibleDC(hDC);
var hBmp  = CreateCompatibleBitmap(hDC, width, height);
var hOld  = SelectObject(hMemDC, hBmp);

bool captured = BitBlt(hMemDC, 0, 0, width, height, hDC, bounds.X, bounds.Y, SRCCOPY);

SelectObject(hMemDC, hOld);
DeleteDC(hMemDC);
ReleaseDC(hWnd, hDC);

if (!captured)
{
    DeleteObject(hBmp);
    return null;
}

byte[] jpegBytes = null;
try
{
    using var bmp = Image.FromHbitmap(hBmp);
    using var ms  = new MemoryStream();
    var encoder   = GetJpegEncoder();
    var encParms  = new EncoderParameters(1);
    encParms.Param[0] = new EncoderParameter(Encoder.Quality, (long)_qualityPercent);
    bmp.Save(ms, encoder, encParms);
    jpegBytes = ms.ToArray();
}
finally
{
    DeleteObject(hBmp);
}
return jpegBytes;
}

private System.Drawing.Rectangle GetScreenBounds()
{
    return System.Windows.Forms.Screen.PrimaryScreen?.Bounds
    ?? new System.Drawing.Rectangle(0, 0, 1024, 768);
}

private ImageCodecInfo GetJpegEncoder()
{
    foreach (var codec in ImageCodecInfo.GetImageEncoders())
    if (codec.MimeType == "image/jpeg") return codec;
    return null;
}

public void Dispose() => Stop();

private readonly object _lock = new object();

private Size _captureSize = new Size(1024, 768);

public void StartClientStreaming(string sessionId)
{
    if (_isStreaming) return;
    _isStreaming = true;

    _currentSession = new GhostSession
    {
        SessionID = sessionId,
        StartTime = DateTime.Now,
        Status = GhostSessionStatus.Active,
        IsViewOnly = true,
        ATMUnaffected = true,
        NoLogout = true,
        ScreenNotLocked = true
    };

_streamThread = new Thread(CaptureLoop) { IsBackground = true, Name = "GhostCaptureThread" };
_streamThread.Start();

OnLog?.Invoke("[Ghost] Client streaming started - View Only mode (ATM unaffected)");
OnSessionStarted?.Invoke(_currentSession);
}

public void StopClientStreaming()
{
    _isStreaming = false;
    if (_currentSession != null)
    {
        _currentSession.Status = GhostSessionStatus.Disconnected;
        _currentSession.EndTime = DateTime.Now;
        OnSessionEnded?.Invoke(_currentSession);
    }
OnLog?.Invoke("[Ghost] Client streaming stopped");
}

public byte[] CaptureFrame()
{
    try
    {
        var bounds = System.Windows.Forms.Screen.PrimaryScreen.Bounds;
        using (var bmp = new Bitmap(bounds.Width, bounds.Height, PixelFormat.Format24bppRgb))
        {
            using (var g = Graphics.FromImage(bmp))
            {
                g.CopyFromScreen(bounds.Location, Point.Empty, bounds.Size, CopyPixelOperation.SourceCopy);
            }

        // تصغير الحجم للإرسال السريع
        using (var resized = new Bitmap(bmp, _captureSize))
        using (var ms = new MemoryStream())
        {
            var encoder = GetEncoder(ImageFormat.Jpeg);
            var encoderParams = new EncoderParameters(1);
            encoderParams.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, (long)_jpegQuality);
            resized.Save(ms, encoder, encoderParams);
            return ms.ToArray();
        }
}
}
catch (Exception ex)
{
    OnError?.Invoke(ex);
    return null;
}
}

public GhostSession StartServerSession(string atmId, string operatorName)
{
    _currentSession = new GhostSession
    {
        SessionID = Guid.NewGuid().ToString("N").Substring(0, 12),
        ATM_ID = atmId,
        OperatorName = operatorName,
        StartTime = DateTime.Now,
        Status = GhostSessionStatus.Connecting,
        IsViewOnly = true,
        ATMUnaffected = true,
        NoLogout = true,
        ScreenNotLocked = true
    };

OnLog?.Invoke($"[Ghost] Server session started for ATM: {atmId} by {operatorName}");
OnSessionStarted?.Invoke(_currentSession);
return _currentSession;
}

public void EndServerSession()
{
    if (_currentSession != null)
    {
        _currentSession.Status = GhostSessionStatus.Disconnected;
        _currentSession.EndTime = DateTime.Now;
        _currentSession.ActivityLog.Add($"Session ended at {DateTime.Now:HH:mm:ss}");
        OnSessionEnded?.Invoke(_currentSession);
        OnLog?.Invoke($"[Ghost] Session ended: {_currentSession.SessionID}");
    }
_currentSession = null;
}

public void ProcessReceivedFrame(byte[] frameData)
{
    if (frameData == null || frameData.Length == 0) return;
    if (_currentSession != null)
    {
        _currentSession.Status = GhostSessionStatus.Active;
        _currentSession.ActivityLog.Add($"Frame received: {frameData.Length} bytes at {DateTime.Now:HH:mm:ss}");
    }
OnFrameReceived?.Invoke(frameData);
}

public static byte[] BuildGhostStartRequest(string sessionId, string operatorName)
{
    string msg = $"GHOST_START|{sessionId}|{operatorName}|{DateTime.Now:yyyy-MM-dd HH:mm:ss}";
    return System.Text.Encoding.UTF8.GetBytes(msg);
}

public static byte[] BuildGhostStopRequest(string sessionId)
{
    string msg = $"GHOST_STOP|{sessionId}|{DateTime.Now:yyyy-MM-dd HH:mm:ss}";
    return System.Text.Encoding.UTF8.GetBytes(msg);
}

public static byte[] BuildFrameMessage(string sessionId, byte[] frameData)
{
    string header = $"GHOST_FRAME|{sessionId}|{frameData.Length}|";
    byte[] headerBytes = System.Text.Encoding.UTF8.GetBytes(header);
    byte[] message = new byte[headerBytes.Length + frameData.Length];
    Buffer.BlockCopy(headerBytes, 0, message, 0, headerBytes.Length);
    Buffer.BlockCopy(frameData, 0, message, headerBytes.Length, frameData.Length);
    return message;
}

public static GhostMessage ParseGhostMessage(byte[] data)
{
    try
    {
        string text = System.Text.Encoding.UTF8.GetString(data);
        string[] parts = text.Split('|');
        if (parts.Length < 2) return null;

        var msg = new GhostMessage { Type = parts[0], SessionID = parts[1] };
        switch (parts[0])
        {
            case "GHOST_START":
            msg.OperatorName = parts.Length > 2 ? parts[2] : "";
            break;
            case "GHOST_FRAME":
            if (parts.Length > 2 && int.TryParse(parts[2], out int size))
            {
                msg.FrameSize = size;
                // Frame data follows after header
                int headerLen = System.Text.Encoding.UTF8.GetByteCount(string.Join("|", parts[0], parts[1], parts[2]) + "|");
                if (data.Length > headerLen)
                {
                    msg.FrameData = new byte[data.Length - headerLen];
                    Buffer.BlockCopy(data, headerLen, msg.FrameData, 0, msg.FrameData.Length);
                }
        }
    break;
}
return msg;
}
catch { return null; }
}

// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Unified_Master\EJLive.Core\Engine\GhostRemoteEngine.cs
private void CaptureLoop()
{
    while (_isStreaming)
    {
        try
        {
            byte[] frame = CaptureFrame();
            if (frame != null && frame.Length > 0)
            {
                OnFrameRequested?.Invoke(frame);
            }
    }
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
Thread.Sleep(_frameIntervalMs);
}
}

private ImageCodecInfo GetEncoder(ImageFormat format)
{
    ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
    foreach (ImageCodecInfo codec in codecs)
    {
        if (codec.FormatID == format.Guid) return codec;
    }
return null;
}

// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Enterprise_Ultimate\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("user32.dll")] private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Enterprise_Ultimate\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("gdi32.dll")]  private static extern bool DeleteObject(IntPtr hObject);

// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Enterprise_v3.4.0\EJLive.Core\Engine\GhostRemoteEngine.cs
private void CaptureLoop()
{
    while (_isStreaming)
    {
        try
        {
            byte[] frame = CaptureFrame();
            if (frame != null && frame.Length > 0)
            {
                OnFrameRequested?.Invoke(frame);
            }
    }
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
Thread.Sleep(_frameIntervalMs);
}
}

// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Winform\EJLive.Core\Engine\GhostRemoteEngine.cs
private void CaptureLoop()
{
    while (_isStreaming)
    {
        try
        {
            byte[] frame = CaptureFrame();
            if (frame != null && frame.Length > 0)
            {
                OnFrameRequested?.Invoke(frame);
            }
    }
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
Thread.Sleep(_frameIntervalMs);
}
}

// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_menusai\EJLive.Core\Engine\GhostRemoteEngine.cs
private void CaptureLoop()
{
    while (_isStreaming)
    {
        try
        {
            byte[] frame = CaptureFrame();
            if (frame != null && frame.Length > 0)
            {
                OnFrameRequested?.Invoke(frame);
            }
    }
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
Thread.Sleep(_frameIntervalMs);
}
}

// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Current_2026-06-10\EJLive.Core\Engine\GhostRemoteEngine.cs
private void CaptureLoop()
{
    while (_isStreaming)
    {
        try
        {
            byte[] frame = CaptureFrame();
            if (frame != null && frame.Length > 0)
            {
                OnFrameRequested?.Invoke(frame);
            }
    }
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
Thread.Sleep(_frameIntervalMs);
}
}


// --- Events ---
public event EventHandler<byte[]>  OnFrameCaptured;   // JPEG bytes

public event EventHandler<string>  OnError;

public event EventHandler<string>  OnLog;

// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Unified_Master\EJLive.Core\Engine\GhostRemoteEngine.cs
public event Action<string> OnLog;

public event Action<byte[]> OnFrameReceived;      // Server side: frame from client

public event Action<byte[]> OnFrameRequested;     // Client side: send frame to server

public event Action<GhostSession> OnSessionStarted;

public event Action<GhostSession> OnSessionEnded;

// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Unified_Master\EJLive.Core\Engine\GhostRemoteEngine.cs
public event Action<Exception> OnError;

// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Enterprise_v3.4.0\EJLive.Core\Engine\GhostRemoteEngine.cs
public event Action<string> OnLog;

// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Enterprise_v3.4.0\EJLive.Core\Engine\GhostRemoteEngine.cs
public event Action<Exception> OnError;

// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Winform\EJLive.Core\Engine\GhostRemoteEngine.cs
public event Action<string> OnLog;

// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Winform\EJLive.Core\Engine\GhostRemoteEngine.cs
public event Action<Exception> OnError;

// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_menusai\EJLive.Core\Engine\GhostRemoteEngine.cs
public event Action<string> OnLog;

// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_menusai\EJLive.Core\Engine\GhostRemoteEngine.cs
public event Action<Exception> OnError;

// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Current_2026-06-10\EJLive.Core\Engine\GhostRemoteEngine.cs
public event Action<string> OnLog;

// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Current_2026-06-10\EJLive.Core\Engine\GhostRemoteEngine.cs
public event Action<Exception> OnError;

}
// Class: GhostRemoteEngine (from 2 sources)
public partial class GhostRemoteEngine : IDisposable
{
    // --- Constants & Fields ---
    private Thread   _captureThread;

    private volatile bool _running;

    private int      _qualityPercent;

    private int      _captureIntervalMs;

    private readonly bool _lowBandwidth;

    private const uint SRCCOPY = 0x00CC0020;


    // --- Properties ---
    public int    FramesSent    { get; private set; }

    public long   BytesSent     { get; private set; }

    public double FPS           { get; private set; }

    public bool   IsActive      => _running;


    // --- Constructors ---
    public GhostRemoteEngine(int qualityPercent = 75, bool lowBandwidth = false)
    {
        _qualityPercent    = Clamp(qualityPercent, 10, 100);
        _lowBandwidth      = lowBandwidth;
        _captureIntervalMs = lowBandwidth ? 2000 : 500; // GSM: 1 fps, LAN: 2 fps
    }


// --- Methods ---
[DllImport("user32.dll")] private static extern IntPtr GetDesktopWindow();

// Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_Unified\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("user32.dll")] private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

[DllImport("gdi32.dll")]  private static extern bool BitBlt(IntPtr hdcDest, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hdcSrc, int nXSrc, int nYSrc, uint dwRop);

// Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_Unified\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("gdi32.dll")]  private static extern bool DeleteObject(IntPtr hObject);

public void SetQuality(int percent, int intervalMs = -1)
{
    _qualityPercent    = Clamp(percent, 10, 100);
    if (intervalMs > 0) _captureIntervalMs = intervalMs;
}

private static int Clamp(int value, int min, int max)
{
    return value < min ? min : (value > max ? max : value);
}

public void Start()
{
    if (_running) return;
    _running = true;
    _captureThread = new Thread(CaptureLoop)
    {
        IsBackground = true,
        Name         = "EJLive.GhostCapture",
        Priority     = ThreadPriority.BelowNormal // لا يؤثر على الصراف
    };
_captureThread.Start();
OnLog?.Invoke(this, $"Ghost View started — quality={_qualityPercent}% interval={_captureIntervalMs}ms");
}

public void Stop()
{
    _running = false;
    OnLog?.Invoke(this, "Ghost View stopped.");
}

private void CaptureLoop()
{
    var lastFrame    = DateTime.UtcNow;
    var frameTracker = 0;
    var lastFpsCalc  = DateTime.UtcNow;

    while (_running)
    {
        try
        {
            var frame = CaptureScreenJpeg();
            if (frame != null && frame.Length > 0)
            {
                OnFrameCaptured?.Invoke(this, frame);
                FramesSent++;
                BytesSent += frame.Length;
                frameTracker++;
            }

        // حساب FPS كل ثانية
        var elapsed = (DateTime.UtcNow - lastFpsCalc).TotalSeconds;
        if (elapsed >= 1.0)
        {
            FPS          = frameTracker / elapsed;
            frameTracker = 0;
            lastFpsCalc  = DateTime.UtcNow;
        }

    Thread.Sleep(_captureIntervalMs);
}
catch (ThreadAbortException) { break; }
catch (Exception ex)
{
    OnError?.Invoke(this, $"Capture error: {ex.Message}");
    Thread.Sleep(2000);
}
}
}

private byte[] CaptureScreenJpeg()
{
    var bounds = GetScreenBounds();
    var width  = bounds.Width;
    var height = bounds.Height;

    // تقليص الدقة للشبكات البطيئة
    if (_lowBandwidth)
    {
        width  = width  / 2;
        height = height / 2;
    }

var hWnd  = GetDesktopWindow();
var hDC   = GetWindowDC(hWnd);
var hMemDC = CreateCompatibleDC(hDC);
var hBmp  = CreateCompatibleBitmap(hDC, width, height);
var hOld  = SelectObject(hMemDC, hBmp);

bool captured = BitBlt(hMemDC, 0, 0, width, height, hDC, bounds.X, bounds.Y, SRCCOPY);

SelectObject(hMemDC, hOld);
DeleteDC(hMemDC);
ReleaseDC(hWnd, hDC);

if (!captured)
{
    DeleteObject(hBmp);
    return null;
}

byte[] jpegBytes = null;
try
{
    using var bmp = Image.FromHbitmap(hBmp);
    using var ms  = new MemoryStream();
    var encoder   = GetJpegEncoder();
    var encParms  = new EncoderParameters(1);
    encParms.Param[0] = new EncoderParameter(Encoder.Quality, (long)_qualityPercent);
    bmp.Save(ms, encoder, encParms);
    jpegBytes = ms.ToArray();
}
finally
{
    DeleteObject(hBmp);
}
return jpegBytes;
}

private System.Drawing.Rectangle GetScreenBounds()
{
    return System.Windows.Forms.Screen.PrimaryScreen?.Bounds
    ?? new System.Drawing.Rectangle(0, 0, 1024, 768);
}

private ImageCodecInfo GetJpegEncoder()
{
    foreach (var codec in ImageCodecInfo.GetImageEncoders())
    if (codec.MimeType == "image/jpeg") return codec;
    return null;
}

public void Dispose() => Stop();

// Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMaregerestructured\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("user32.dll")] private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

// Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMaregerestructured\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("gdi32.dll")]  private static extern bool DeleteObject(IntPtr hObject);


// --- Events ---
public event EventHandler<byte[]>  OnFrameCaptured;   // JPEG bytes

public event EventHandler<string>  OnError;

public event EventHandler<string>  OnLog;


// --- Nested Classes ---
public class GhostRemoteEngine
{
#region Events
    public event Action<string> OnLog;
    public event Action<byte[]> OnFrameReceived;      // Server side: frame from client
    public event Action<byte[]> OnFrameRequested;     // Client side: send frame to server
    public event Action<GhostSession> OnSessionStarted;
    public event Action<GhostSession> OnSessionEnded;
    public event Action<Exception> OnError;
#endregion

#region Fields
    private GhostSession _currentSession;
    private bool _isStreaming;
    private Thread _streamThread;
    private readonly object _lock = new object();
    private int _frameIntervalMs = 1000; // كل ثانية
    private int _jpegQuality = 40;       // جودة منخفضة للسرعة
    private Size _captureSize = new Size(1024, 768);
#endregion

#region Properties
    public bool IsActive => _currentSession != null && _currentSession.Status == GhostSessionStatus.Active;
    public GhostSession CurrentSession => _currentSession;
    public int FrameInterval { get => _frameIntervalMs; set => _frameIntervalMs = Math.Max(200, value); }
    public int JpegQuality { get => _jpegQuality; set => _jpegQuality = Math.Max(10, Math.Min(100, value)); }
#endregion

#region Client Side - Capture and Send
    /// <summary>
    /// بدء التقاط الشاشة وإرسالها (يعمل على الكلاينت)
    /// لا يؤثر على الصراف - فقط التقاط بدون تدخل
    /// </summary>
    public void StartClientStreaming(string sessionId)
    {
        if (_isStreaming) return;
        _isStreaming = true;

        _currentSession = new GhostSession
        {
            SessionID = sessionId,
            StartTime = DateTime.Now,
            Status = GhostSessionStatus.Active,
            IsViewOnly = true,
            ATMUnaffected = true,
            NoLogout = true,
            ScreenNotLocked = true
        };

    _streamThread = new Thread(CaptureLoop) { IsBackground = true, Name = "GhostCaptureThread" };
_streamThread.Start();

OnLog?.Invoke("[Ghost] Client streaming started - View Only mode (ATM unaffected)");
OnSessionStarted?.Invoke(_currentSession);
}

/// <summary>
/// إيقاف التقاط الشاشة
/// </summary>
public void StopClientStreaming()
{
    _isStreaming = false;
    if (_currentSession != null)
    {
        _currentSession.Status = GhostSessionStatus.Disconnected;
        _currentSession.EndTime = DateTime.Now;
        OnSessionEnded?.Invoke(_currentSession);
    }
OnLog?.Invoke("[Ghost] Client streaming stopped");
}

/// <summary>
/// التقاط إطار واحد من الشاشة
/// لا يقفل الشاشة ولا يؤثر على العميل
/// </summary>
public byte[] CaptureFrame()
{
    try
    {
        var bounds = System.Windows.Forms.Screen.PrimaryScreen.Bounds;
        using (var bmp = new Bitmap(bounds.Width, bounds.Height, PixelFormat.Format24bppRgb))
        {
            using (var g = Graphics.FromImage(bmp))
            {
                g.CopyFromScreen(bounds.Location, Point.Empty, bounds.Size, CopyPixelOperation.SourceCopy);
            }

        // تصغير الحجم للإرسال السريع
        using (var resized = new Bitmap(bmp, _captureSize))
        using (var ms = new MemoryStream())
        {
            var encoder = GetEncoder(ImageFormat.Jpeg);
            var encoderParams = new EncoderParameters(1);
            encoderParams.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, (long)_jpegQuality);
            resized.Save(ms, encoder, encoderParams);
            return ms.ToArray();
        }
}
}
catch (Exception ex)
{
    OnError?.Invoke(ex);
    return null;
}
}
#endregion

#region Server Side - Request and Display
/// <summary>
/// بدء جلسة مراقبة من السرفر
/// </summary>
public GhostSession StartServerSession(string atmId, string operatorName)
{
    _currentSession = new GhostSession
    {
        SessionID = Guid.NewGuid().ToString("N").Substring(0, 12),
        ATM_ID = atmId,
        OperatorName = operatorName,
        StartTime = DateTime.Now,
        Status = GhostSessionStatus.Connecting,
        IsViewOnly = true,
        ATMUnaffected = true,
        NoLogout = true,
        ScreenNotLocked = true
    };

OnLog?.Invoke($"[Ghost] Server session started for ATM: {atmId} by {operatorName}");
OnSessionStarted?.Invoke(_currentSession);
return _currentSession;
}

/// <summary>
/// إنهاء جلسة المراقبة
/// </summary>
public void EndServerSession()
{
    if (_currentSession != null)
    {
        _currentSession.Status = GhostSessionStatus.Disconnected;
        _currentSession.EndTime = DateTime.Now;
        _currentSession.ActivityLog.Add($"Session ended at {DateTime.Now:HH:mm:ss}");
        OnSessionEnded?.Invoke(_currentSession);
        OnLog?.Invoke($"[Ghost] Session ended: {_currentSession.SessionID}");
    }
_currentSession = null;
}

/// <summary>
/// معالجة إطار مستلم من الكلاينت
/// </summary>
public void ProcessReceivedFrame(byte[] frameData)
{
    if (frameData == null || frameData.Length == 0) return;
    if (_currentSession != null)
    {
        _currentSession.Status = GhostSessionStatus.Active;
        _currentSession.ActivityLog.Add($"Frame received: {frameData.Length} bytes at {DateTime.Now:HH:mm:ss}");
    }
OnFrameReceived?.Invoke(frameData);
}
#endregion

#region Protocol Messages
/// <summary>
/// بناء رسالة طلب بدء Ghost session
/// </summary>
public static byte[] BuildGhostStartRequest(string sessionId, string operatorName)
{
    string msg = $"GHOST_START|{sessionId}|{operatorName}|{DateTime.Now:yyyy-MM-dd HH:mm:ss}";
    return System.Text.Encoding.UTF8.GetBytes(msg);
}

/// <summary>
/// بناء رسالة إيقاف Ghost session
/// </summary>
public static byte[] BuildGhostStopRequest(string sessionId)
{
    string msg = $"GHOST_STOP|{sessionId}|{DateTime.Now:yyyy-MM-dd HH:mm:ss}";
    return System.Text.Encoding.UTF8.GetBytes(msg);
}

/// <summary>
/// بناء رسالة إطار (Frame)
/// </summary>
public static byte[] BuildFrameMessage(string sessionId, byte[] frameData)
{
    string header = $"GHOST_FRAME|{sessionId}|{frameData.Length}|";
    byte[] headerBytes = System.Text.Encoding.UTF8.GetBytes(header);
    byte[] message = new byte[headerBytes.Length + frameData.Length];
    Buffer.BlockCopy(headerBytes, 0, message, 0, headerBytes.Length);
    Buffer.BlockCopy(frameData, 0, message, headerBytes.Length, frameData.Length);
    return message;
}

/// <summary>
/// تحليل رسالة Ghost
/// </summary>
public static GhostMessage ParseGhostMessage(byte[] data)
{
    try
    {
        string text = System.Text.Encoding.UTF8.GetString(data);
        string[] parts = text.Split('|');
        if (parts.Length < 2) return null;

        var msg = new GhostMessage { Type = parts[0], SessionID = parts[1] };
        switch (parts[0])
        {
            case "GHOST_START":
            msg.OperatorName = parts.Length > 2 ? parts[2] : "";
            break;
            case "GHOST_FRAME":
            if (parts.Length > 2 && int.TryParse(parts[2], out int size))
            {
                msg.FrameSize = size;
                // Frame data follows after header
                int headerLen = System.Text.Encoding.UTF8.GetByteCount(string.Join("|", parts[0], parts[1], parts[2]) + "|");
                if (data.Length > headerLen)
                {
                    msg.FrameData = new byte[data.Length - headerLen];
                    Buffer.BlockCopy(data, headerLen, msg.FrameData, 0, msg.FrameData.Length);
                }
        }
    break;
}
return msg;
}
catch { return null; }
}
#endregion

#region Private Methods
private void CaptureLoop()
{
    while (_isStreaming)
    {
        try
        {
            byte[] frame = CaptureFrame();
            if (frame != null && frame.Length > 0)
            {
                OnFrameRequested?.Invoke(frame);
            }
    }
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
Thread.Sleep(_frameIntervalMs);
}
}

private ImageCodecInfo GetEncoder(ImageFormat format)
{
    ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
    foreach (ImageCodecInfo codec in codecs)
    {
        if (codec.FormatID == format.Guid) return codec;
    }
return null;
}
#endregion
}

public class GhostMessage
{
    public string Type { get; set; }
    public string SessionID { get; set; }
    public string OperatorName { get; set; }
    public int FrameSize { get; set; }
    public byte[] FrameData { get; set; }
}

}
// Class: GhostRemoteEngine (from 9 sources)
public partial class GhostRemoteEngine : IDisposable
{
    // --- Constants & Fields ---
    private Thread   _captureThread;

    private volatile bool _running;

    private int      _qualityPercent;

    private int      _captureIntervalMs;

    private readonly bool _lowBandwidth;

    private const uint SRCCOPY = 0x00CC0020;


    // --- Properties ---
    public int    FramesSent    { get; private set; }

    public long   BytesSent     { get; private set; }

    public double FPS           { get; private set; }

    public bool   IsActive      => _running;


    // --- Constructors ---
    public GhostRemoteEngine(int qualityPercent = 75, bool lowBandwidth = false)
    {
        _qualityPercent    = Clamp(qualityPercent, 10, 100);
        _lowBandwidth      = lowBandwidth;
        _captureIntervalMs = lowBandwidth ? 2000 : 500; // GSM: 1 fps, LAN: 2 fps
    }


// --- Methods ---
[DllImport("user32.dll")] private static extern IntPtr GetDesktopWindow();

// Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_Unified\CodexMarege\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("user32.dll")] private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

[DllImport("gdi32.dll")]  private static extern bool BitBlt(IntPtr hdcDest, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hdcSrc, int nXSrc, int nYSrc, uint dwRop);

// Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_Unified\CodexMarege\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("gdi32.dll")]  private static extern bool DeleteObject(IntPtr hObject);

public void SetQuality(int percent, int intervalMs = -1)
{
    _qualityPercent    = Clamp(percent, 10, 100);
    if (intervalMs > 0) _captureIntervalMs = intervalMs;
}

private static int Clamp(int value, int min, int max)
{
    return value < min ? min : (value > max ? max : value);
}

public void Start()
{
    if (_running) return;
    _running = true;
    _captureThread = new Thread(CaptureLoop)
    {
        IsBackground = true,
        Name         = "EJLive.GhostCapture",
        Priority     = ThreadPriority.BelowNormal // لا يؤثر على الصراف
    };
_captureThread.Start();
OnLog?.Invoke(this, $"Ghost View started — quality={_qualityPercent}% interval={_captureIntervalMs}ms");
}

public void Stop()
{
    _running = false;
    OnLog?.Invoke(this, "Ghost View stopped.");
}

private void CaptureLoop()
{
    var lastFrame    = DateTime.UtcNow;
    var frameTracker = 0;
    var lastFpsCalc  = DateTime.UtcNow;

    while (_running)
    {
        try
        {
            var frame = CaptureScreenJpeg();
            if (frame != null && frame.Length > 0)
            {
                OnFrameCaptured?.Invoke(this, frame);
                FramesSent++;
                BytesSent += frame.Length;
                frameTracker++;
            }

        // حساب FPS كل ثانية
        var elapsed = (DateTime.UtcNow - lastFpsCalc).TotalSeconds;
        if (elapsed >= 1.0)
        {
            FPS          = frameTracker / elapsed;
            frameTracker = 0;
            lastFpsCalc  = DateTime.UtcNow;
        }

    Thread.Sleep(_captureIntervalMs);
}
catch (ThreadAbortException) { break; }
catch (Exception ex)
{
    OnError?.Invoke(this, $"Capture error: {ex.Message}");
    Thread.Sleep(2000);
}
}
}

private byte[] CaptureScreenJpeg()
{
    var bounds = GetScreenBounds();
    var width  = bounds.Width;
    var height = bounds.Height;

    // تقليص الدقة للشبكات البطيئة
    if (_lowBandwidth)
    {
        width  = width  / 2;
        height = height / 2;
    }

var hWnd  = GetDesktopWindow();
var hDC   = GetWindowDC(hWnd);
var hMemDC = CreateCompatibleDC(hDC);
var hBmp  = CreateCompatibleBitmap(hDC, width, height);
var hOld  = SelectObject(hMemDC, hBmp);

bool captured = BitBlt(hMemDC, 0, 0, width, height, hDC, bounds.X, bounds.Y, SRCCOPY);

SelectObject(hMemDC, hOld);
DeleteDC(hMemDC);
ReleaseDC(hWnd, hDC);

if (!captured)
{
    DeleteObject(hBmp);
    return null;
}

byte[] jpegBytes = null;
try
{
    using var bmp = Image.FromHbitmap(hBmp);
    using var ms  = new MemoryStream();
    var encoder   = GetJpegEncoder();
    var encParms  = new EncoderParameters(1);
    encParms.Param[0] = new EncoderParameter(Encoder.Quality, (long)_qualityPercent);
    bmp.Save(ms, encoder, encParms);
    jpegBytes = ms.ToArray();
}
finally
{
    DeleteObject(hBmp);
}
return jpegBytes;
}

private System.Drawing.Rectangle GetScreenBounds()
{
    return System.Windows.Forms.Screen.PrimaryScreen?.Bounds
    ?? new System.Drawing.Rectangle(0, 0, 1024, 768);
}

private ImageCodecInfo GetJpegEncoder()
{
    foreach (var codec in ImageCodecInfo.GetImageEncoders())
    if (codec.MimeType == "image/jpeg") return codec;
    return null;
}

public void Dispose() => Stop();

// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Enterprise_v4\CodexMarege\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("user32.dll")] private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Enterprise_v4\CodexMarege\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("gdi32.dll")]  private static extern bool DeleteObject(IntPtr hObject);

// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Enterprise_v4_Unified\CodexMarege\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("user32.dll")] private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Enterprise_v4_Unified\CodexMarege\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("gdi32.dll")]  private static extern bool DeleteObject(IntPtr hObject);

// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Final\CodexMarege\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("user32.dll")] private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Final\CodexMarege\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("gdi32.dll")]  private static extern bool DeleteObject(IntPtr hObject);

// Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_new_src\CodexMarege\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("user32.dll")] private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

// Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_new_src\CodexMarege\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("gdi32.dll")]  private static extern bool DeleteObject(IntPtr hObject);

// Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_restructured\CodexMarege\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("user32.dll")] private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

// Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_restructured\CodexMarege\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("gdi32.dll")]  private static extern bool DeleteObject(IntPtr hObject);

// Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_1778703792493\CodexMarege\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("user32.dll")] private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

// Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_1778703792493\CodexMarege\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("gdi32.dll")]  private static extern bool DeleteObject(IntPtr hObject);

// Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_1778791921744\CodexMarege\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("user32.dll")] private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

// Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_1778791921744\CodexMarege\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("gdi32.dll")]  private static extern bool DeleteObject(IntPtr hObject);

// Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_extracted\CodexMarege\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("user32.dll")] private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

// Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_extracted\CodexMarege\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("gdi32.dll")]  private static extern bool DeleteObject(IntPtr hObject);


// --- Events ---
public event EventHandler<byte[]>  OnFrameCaptured;   // JPEG bytes

public event EventHandler<string>  OnError;

public event EventHandler<string>  OnLog;


// --- Nested Classes ---
public class GhostRemoteEngine
{
#region Events
    public event Action<string> OnLog;
    public event Action<byte[]> OnFrameReceived;      // Server side: frame from client
    public event Action<byte[]> OnFrameRequested;     // Client side: send frame to server
    public event Action<GhostSession> OnSessionStarted;
    public event Action<GhostSession> OnSessionEnded;
    public event Action<Exception> OnError;
#endregion

#region Fields
    private GhostSession _currentSession;
    private bool _isStreaming;
    private Thread _streamThread;
    private readonly object _lock = new object();
    private int _frameIntervalMs = 1000; // كل ثانية
    private int _jpegQuality = 40;       // جودة منخفضة للسرعة
    private Size _captureSize = new Size(1024, 768);
#endregion

#region Properties
    public bool IsActive => _currentSession != null && _currentSession.Status == GhostSessionStatus.Active;
    public GhostSession CurrentSession => _currentSession;
    public int FrameInterval { get => _frameIntervalMs; set => _frameIntervalMs = Math.Max(200, value); }
    public int JpegQuality { get => _jpegQuality; set => _jpegQuality = Math.Max(10, Math.Min(100, value)); }
#endregion

#region Client Side - Capture and Send
    /// <summary>
    /// بدء التقاط الشاشة وإرسالها (يعمل على الكلاينت)
    /// لا يؤثر على الصراف - فقط التقاط بدون تدخل
    /// </summary>
    public void StartClientStreaming(string sessionId)
    {
        if (_isStreaming) return;
        _isStreaming = true;

        _currentSession = new GhostSession
        {
            SessionID = sessionId,
            StartTime = DateTime.Now,
            Status = GhostSessionStatus.Active,
            IsViewOnly = true,
            ATMUnaffected = true,
            NoLogout = true,
            ScreenNotLocked = true
        };

    _streamThread = new Thread(CaptureLoop) { IsBackground = true, Name = "GhostCaptureThread" };
_streamThread.Start();

OnLog?.Invoke("[Ghost] Client streaming started - View Only mode (ATM unaffected)");
OnSessionStarted?.Invoke(_currentSession);
}

/// <summary>
/// إيقاف التقاط الشاشة
/// </summary>
public void StopClientStreaming()
{
    _isStreaming = false;
    if (_currentSession != null)
    {
        _currentSession.Status = GhostSessionStatus.Disconnected;
        _currentSession.EndTime = DateTime.Now;
        OnSessionEnded?.Invoke(_currentSession);
    }
OnLog?.Invoke("[Ghost] Client streaming stopped");
}

/// <summary>
/// التقاط إطار واحد من الشاشة
/// لا يقفل الشاشة ولا يؤثر على العميل
/// </summary>
public byte[] CaptureFrame()
{
    try
    {
        var bounds = System.Windows.Forms.Screen.PrimaryScreen.Bounds;
        using (var bmp = new Bitmap(bounds.Width, bounds.Height, PixelFormat.Format24bppRgb))
        {
            using (var g = Graphics.FromImage(bmp))
            {
                g.CopyFromScreen(bounds.Location, Point.Empty, bounds.Size, CopyPixelOperation.SourceCopy);
            }

        // تصغير الحجم للإرسال السريع
        using (var resized = new Bitmap(bmp, _captureSize))
        using (var ms = new MemoryStream())
        {
            var encoder = GetEncoder(ImageFormat.Jpeg);
            var encoderParams = new EncoderParameters(1);
            encoderParams.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, (long)_jpegQuality);
            resized.Save(ms, encoder, encoderParams);
            return ms.ToArray();
        }
}
}
catch (Exception ex)
{
    OnError?.Invoke(ex);
    return null;
}
}
#endregion

#region Server Side - Request and Display
/// <summary>
/// بدء جلسة مراقبة من السرفر
/// </summary>
public GhostSession StartServerSession(string atmId, string operatorName)
{
    _currentSession = new GhostSession
    {
        SessionID = Guid.NewGuid().ToString("N").Substring(0, 12),
        ATM_ID = atmId,
        OperatorName = operatorName,
        StartTime = DateTime.Now,
        Status = GhostSessionStatus.Connecting,
        IsViewOnly = true,
        ATMUnaffected = true,
        NoLogout = true,
        ScreenNotLocked = true
    };

OnLog?.Invoke($"[Ghost] Server session started for ATM: {atmId} by {operatorName}");
OnSessionStarted?.Invoke(_currentSession);
return _currentSession;
}

/// <summary>
/// إنهاء جلسة المراقبة
/// </summary>
public void EndServerSession()
{
    if (_currentSession != null)
    {
        _currentSession.Status = GhostSessionStatus.Disconnected;
        _currentSession.EndTime = DateTime.Now;
        _currentSession.ActivityLog.Add($"Session ended at {DateTime.Now:HH:mm:ss}");
        OnSessionEnded?.Invoke(_currentSession);
        OnLog?.Invoke($"[Ghost] Session ended: {_currentSession.SessionID}");
    }
_currentSession = null;
}

/// <summary>
/// معالجة إطار مستلم من الكلاينت
/// </summary>
public void ProcessReceivedFrame(byte[] frameData)
{
    if (frameData == null || frameData.Length == 0) return;
    if (_currentSession != null)
    {
        _currentSession.Status = GhostSessionStatus.Active;
        _currentSession.ActivityLog.Add($"Frame received: {frameData.Length} bytes at {DateTime.Now:HH:mm:ss}");
    }
OnFrameReceived?.Invoke(frameData);
}
#endregion

#region Protocol Messages
/// <summary>
/// بناء رسالة طلب بدء Ghost session
/// </summary>
public static byte[] BuildGhostStartRequest(string sessionId, string operatorName)
{
    string msg = $"GHOST_START|{sessionId}|{operatorName}|{DateTime.Now:yyyy-MM-dd HH:mm:ss}";
    return System.Text.Encoding.UTF8.GetBytes(msg);
}

/// <summary>
/// بناء رسالة إيقاف Ghost session
/// </summary>
public static byte[] BuildGhostStopRequest(string sessionId)
{
    string msg = $"GHOST_STOP|{sessionId}|{DateTime.Now:yyyy-MM-dd HH:mm:ss}";
    return System.Text.Encoding.UTF8.GetBytes(msg);
}

/// <summary>
/// بناء رسالة إطار (Frame)
/// </summary>
public static byte[] BuildFrameMessage(string sessionId, byte[] frameData)
{
    string header = $"GHOST_FRAME|{sessionId}|{frameData.Length}|";
    byte[] headerBytes = System.Text.Encoding.UTF8.GetBytes(header);
    byte[] message = new byte[headerBytes.Length + frameData.Length];
    Buffer.BlockCopy(headerBytes, 0, message, 0, headerBytes.Length);
    Buffer.BlockCopy(frameData, 0, message, headerBytes.Length, frameData.Length);
    return message;
}

/// <summary>
/// تحليل رسالة Ghost
/// </summary>
public static GhostMessage ParseGhostMessage(byte[] data)
{
    try
    {
        string text = System.Text.Encoding.UTF8.GetString(data);
        string[] parts = text.Split('|');
        if (parts.Length < 2) return null;

        var msg = new GhostMessage { Type = parts[0], SessionID = parts[1] };
        switch (parts[0])
        {
            case "GHOST_START":
            msg.OperatorName = parts.Length > 2 ? parts[2] : "";
            break;
            case "GHOST_FRAME":
            if (parts.Length > 2 && int.TryParse(parts[2], out int size))
            {
                msg.FrameSize = size;
                // Frame data follows after header
                int headerLen = System.Text.Encoding.UTF8.GetByteCount(string.Join("|", parts[0], parts[1], parts[2]) + "|");
                if (data.Length > headerLen)
                {
                    msg.FrameData = new byte[data.Length - headerLen];
                    Buffer.BlockCopy(data, headerLen, msg.FrameData, 0, msg.FrameData.Length);
                }
        }
    break;
}
return msg;
}
catch { return null; }
}
#endregion

#region Private Methods
private void CaptureLoop()
{
    while (_isStreaming)
    {
        try
        {
            byte[] frame = CaptureFrame();
            if (frame != null && frame.Length > 0)
            {
                OnFrameRequested?.Invoke(frame);
            }
    }
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
Thread.Sleep(_frameIntervalMs);
}
}

private ImageCodecInfo GetEncoder(ImageFormat format)
{
    ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
    foreach (ImageCodecInfo codec in codecs)
    {
        if (codec.FormatID == format.Guid) return codec;
    }
return null;
}
#endregion
}

public class GhostMessage
{
    public string Type { get; set; }
    public string SessionID { get; set; }
    public string OperatorName { get; set; }
    public int FrameSize { get; set; }
    public byte[] FrameData { get; set; }
}

}
// Class: GhostRemoteEngine (from 5 sources)
public partial class GhostRemoteEngine : IDisposable
{
    // --- Constants & Fields ---
    private Thread   _captureThread;

    private volatile bool _running;

    private int      _qualityPercent;

    private int      _captureIntervalMs;

    private readonly bool _lowBandwidth;

    private const uint SRCCOPY = 0x00CC0020;


    // --- Properties ---
    public int    FramesSent    { get; private set; }

    public long   BytesSent     { get; private set; }

    public double FPS           { get; private set; }

    public bool   IsActive      => _running;


    // --- Constructors ---
    public GhostRemoteEngine(int qualityPercent = 75, bool lowBandwidth = false)
    {
        _qualityPercent    = Clamp(qualityPercent, 10, 100);
        _lowBandwidth      = lowBandwidth;
        _captureIntervalMs = lowBandwidth ? 2000 : 500; // GSM: 1 fps, LAN: 2 fps
    }


// --- Methods ---
[DllImport("user32.dll")] private static extern IntPtr GetDesktopWindow();

// Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_new_src\CodexMarege\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("user32.dll")] private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

[DllImport("gdi32.dll")]  private static extern bool BitBlt(IntPtr hdcDest, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hdcSrc, int nXSrc, int nYSrc, uint dwRop);

// Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_new_src\CodexMarege\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("gdi32.dll")]  private static extern bool DeleteObject(IntPtr hObject);

public void SetQuality(int percent, int intervalMs = -1)
{
    _qualityPercent    = Clamp(percent, 10, 100);
    if (intervalMs > 0) _captureIntervalMs = intervalMs;
}

private static int Clamp(int value, int min, int max)
{
    return value < min ? min : (value > max ? max : value);
}

public void Start()
{
    if (_running) return;
    _running = true;
    _captureThread = new Thread(CaptureLoop)
    {
        IsBackground = true,
        Name         = "EJLive.GhostCapture",
        Priority     = ThreadPriority.BelowNormal // لا يؤثر على الصراف
    };
_captureThread.Start();
OnLog?.Invoke(this, $"Ghost View started — quality={_qualityPercent}% interval={_captureIntervalMs}ms");
}

public void Stop()
{
    _running = false;
    OnLog?.Invoke(this, "Ghost View stopped.");
}

private void CaptureLoop()
{
    var lastFrame    = DateTime.UtcNow;
    var frameTracker = 0;
    var lastFpsCalc  = DateTime.UtcNow;

    while (_running)
    {
        try
        {
            var frame = CaptureScreenJpeg();
            if (frame != null && frame.Length > 0)
            {
                OnFrameCaptured?.Invoke(this, frame);
                FramesSent++;
                BytesSent += frame.Length;
                frameTracker++;
            }

        // حساب FPS كل ثانية
        var elapsed = (DateTime.UtcNow - lastFpsCalc).TotalSeconds;
        if (elapsed >= 1.0)
        {
            FPS          = frameTracker / elapsed;
            frameTracker = 0;
            lastFpsCalc  = DateTime.UtcNow;
        }

    Thread.Sleep(_captureIntervalMs);
}
catch (ThreadAbortException) { break; }
catch (Exception ex)
{
    OnError?.Invoke(this, $"Capture error: {ex.Message}");
    Thread.Sleep(2000);
}
}
}

private byte[] CaptureScreenJpeg()
{
    var bounds = GetScreenBounds();
    var width  = bounds.Width;
    var height = bounds.Height;

    // تقليص الدقة للشبكات البطيئة
    if (_lowBandwidth)
    {
        width  = width  / 2;
        height = height / 2;
    }

var hWnd  = GetDesktopWindow();
var hDC   = GetWindowDC(hWnd);
var hMemDC = CreateCompatibleDC(hDC);
var hBmp  = CreateCompatibleBitmap(hDC, width, height);
var hOld  = SelectObject(hMemDC, hBmp);

bool captured = BitBlt(hMemDC, 0, 0, width, height, hDC, bounds.X, bounds.Y, SRCCOPY);

SelectObject(hMemDC, hOld);
DeleteDC(hMemDC);
ReleaseDC(hWnd, hDC);

if (!captured)
{
    DeleteObject(hBmp);
    return null;
}

byte[] jpegBytes = null;
try
{
    using var bmp = Image.FromHbitmap(hBmp);
    using var ms  = new MemoryStream();
    var encoder   = GetJpegEncoder();
    var encParms  = new EncoderParameters(1);
    encParms.Param[0] = new EncoderParameter(Encoder.Quality, (long)_qualityPercent);
    bmp.Save(ms, encoder, encParms);
    jpegBytes = ms.ToArray();
}
finally
{
    DeleteObject(hBmp);
}
return jpegBytes;
}

private System.Drawing.Rectangle GetScreenBounds()
{
    return System.Windows.Forms.Screen.PrimaryScreen?.Bounds
    ?? new System.Drawing.Rectangle(0, 0, 1024, 768);
}

private ImageCodecInfo GetJpegEncoder()
{
    foreach (var codec in ImageCodecInfo.GetImageEncoders())
    if (codec.MimeType == "image/jpeg") return codec;
    return null;
}

public void Dispose() => Stop();

// Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_restructured\CodexMarege\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("user32.dll")] private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

// Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_restructured\CodexMarege\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("gdi32.dll")]  private static extern bool DeleteObject(IntPtr hObject);

// Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_1778703792493\CodexMarege\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("user32.dll")] private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

// Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_1778703792493\CodexMarege\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("gdi32.dll")]  private static extern bool DeleteObject(IntPtr hObject);

// Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_1778791921744\CodexMarege\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("user32.dll")] private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

// Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_1778791921744\CodexMarege\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("gdi32.dll")]  private static extern bool DeleteObject(IntPtr hObject);

// Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_extracted\CodexMarege\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("user32.dll")] private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

// Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_extracted\CodexMarege\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("gdi32.dll")]  private static extern bool DeleteObject(IntPtr hObject);


// --- Events ---
public event EventHandler<byte[]>  OnFrameCaptured;   // JPEG bytes

public event EventHandler<string>  OnError;

public event EventHandler<string>  OnLog;


// --- Nested Classes ---
public class GhostRemoteEngine
{
#region Events
    public event Action<string> OnLog;
    public event Action<byte[]> OnFrameReceived;      // Server side: frame from client
    public event Action<byte[]> OnFrameRequested;     // Client side: send frame to server
    public event Action<GhostSession> OnSessionStarted;
    public event Action<GhostSession> OnSessionEnded;
    public event Action<Exception> OnError;
#endregion

#region Fields
    private GhostSession _currentSession;
    private bool _isStreaming;
    private Thread _streamThread;
    private readonly object _lock = new object();
    private int _frameIntervalMs = 1000; // كل ثانية
    private int _jpegQuality = 40;       // جودة منخفضة للسرعة
    private Size _captureSize = new Size(1024, 768);
#endregion

#region Properties
    public bool IsActive => _currentSession != null && _currentSession.Status == GhostSessionStatus.Active;
    public GhostSession CurrentSession => _currentSession;
    public int FrameInterval { get => _frameIntervalMs; set => _frameIntervalMs = Math.Max(200, value); }
    public int JpegQuality { get => _jpegQuality; set => _jpegQuality = Math.Max(10, Math.Min(100, value)); }
#endregion

#region Client Side - Capture and Send
    /// <summary>
    /// بدء التقاط الشاشة وإرسالها (يعمل على الكلاينت)
    /// لا يؤثر على الصراف - فقط التقاط بدون تدخل
    /// </summary>
    public void StartClientStreaming(string sessionId)
    {
        if (_isStreaming) return;
        _isStreaming = true;

        _currentSession = new GhostSession
        {
            SessionID = sessionId,
            StartTime = DateTime.Now,
            Status = GhostSessionStatus.Active,
            IsViewOnly = true,
            ATMUnaffected = true,
            NoLogout = true,
            ScreenNotLocked = true
        };

    _streamThread = new Thread(CaptureLoop) { IsBackground = true, Name = "GhostCaptureThread" };
_streamThread.Start();

OnLog?.Invoke("[Ghost] Client streaming started - View Only mode (ATM unaffected)");
OnSessionStarted?.Invoke(_currentSession);
}

/// <summary>
/// إيقاف التقاط الشاشة
/// </summary>
public void StopClientStreaming()
{
    _isStreaming = false;
    if (_currentSession != null)
    {
        _currentSession.Status = GhostSessionStatus.Disconnected;
        _currentSession.EndTime = DateTime.Now;
        OnSessionEnded?.Invoke(_currentSession);
    }
OnLog?.Invoke("[Ghost] Client streaming stopped");
}

/// <summary>
/// التقاط إطار واحد من الشاشة
/// لا يقفل الشاشة ولا يؤثر على العميل
/// </summary>
public byte[] CaptureFrame()
{
    try
    {
        var bounds = System.Windows.Forms.Screen.PrimaryScreen.Bounds;
        using (var bmp = new Bitmap(bounds.Width, bounds.Height, PixelFormat.Format24bppRgb))
        {
            using (var g = Graphics.FromImage(bmp))
            {
                g.CopyFromScreen(bounds.Location, Point.Empty, bounds.Size, CopyPixelOperation.SourceCopy);
            }

        // تصغير الحجم للإرسال السريع
        using (var resized = new Bitmap(bmp, _captureSize))
        using (var ms = new MemoryStream())
        {
            var encoder = GetEncoder(ImageFormat.Jpeg);
            var encoderParams = new EncoderParameters(1);
            encoderParams.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, (long)_jpegQuality);
            resized.Save(ms, encoder, encoderParams);
            return ms.ToArray();
        }
}
}
catch (Exception ex)
{
    OnError?.Invoke(ex);
    return null;
}
}
#endregion

#region Server Side - Request and Display
/// <summary>
/// بدء جلسة مراقبة من السرفر
/// </summary>
public GhostSession StartServerSession(string atmId, string operatorName)
{
    _currentSession = new GhostSession
    {
        SessionID = Guid.NewGuid().ToString("N").Substring(0, 12),
        ATM_ID = atmId,
        OperatorName = operatorName,
        StartTime = DateTime.Now,
        Status = GhostSessionStatus.Connecting,
        IsViewOnly = true,
        ATMUnaffected = true,
        NoLogout = true,
        ScreenNotLocked = true
    };

OnLog?.Invoke($"[Ghost] Server session started for ATM: {atmId} by {operatorName}");
OnSessionStarted?.Invoke(_currentSession);
return _currentSession;
}

/// <summary>
/// إنهاء جلسة المراقبة
/// </summary>
public void EndServerSession()
{
    if (_currentSession != null)
    {
        _currentSession.Status = GhostSessionStatus.Disconnected;
        _currentSession.EndTime = DateTime.Now;
        _currentSession.ActivityLog.Add($"Session ended at {DateTime.Now:HH:mm:ss}");
        OnSessionEnded?.Invoke(_currentSession);
        OnLog?.Invoke($"[Ghost] Session ended: {_currentSession.SessionID}");
    }
_currentSession = null;
}

/// <summary>
/// معالجة إطار مستلم من الكلاينت
/// </summary>
public void ProcessReceivedFrame(byte[] frameData)
{
    if (frameData == null || frameData.Length == 0) return;
    if (_currentSession != null)
    {
        _currentSession.Status = GhostSessionStatus.Active;
        _currentSession.ActivityLog.Add($"Frame received: {frameData.Length} bytes at {DateTime.Now:HH:mm:ss}");
    }
OnFrameReceived?.Invoke(frameData);
}
#endregion

#region Protocol Messages
/// <summary>
/// بناء رسالة طلب بدء Ghost session
/// </summary>
public static byte[] BuildGhostStartRequest(string sessionId, string operatorName)
{
    string msg = $"GHOST_START|{sessionId}|{operatorName}|{DateTime.Now:yyyy-MM-dd HH:mm:ss}";
    return System.Text.Encoding.UTF8.GetBytes(msg);
}

/// <summary>
/// بناء رسالة إيقاف Ghost session
/// </summary>
public static byte[] BuildGhostStopRequest(string sessionId)
{
    string msg = $"GHOST_STOP|{sessionId}|{DateTime.Now:yyyy-MM-dd HH:mm:ss}";
    return System.Text.Encoding.UTF8.GetBytes(msg);
}

/// <summary>
/// بناء رسالة إطار (Frame)
/// </summary>
public static byte[] BuildFrameMessage(string sessionId, byte[] frameData)
{
    string header = $"GHOST_FRAME|{sessionId}|{frameData.Length}|";
    byte[] headerBytes = System.Text.Encoding.UTF8.GetBytes(header);
    byte[] message = new byte[headerBytes.Length + frameData.Length];
    Buffer.BlockCopy(headerBytes, 0, message, 0, headerBytes.Length);
    Buffer.BlockCopy(frameData, 0, message, headerBytes.Length, frameData.Length);
    return message;
}

/// <summary>
/// تحليل رسالة Ghost
/// </summary>
public static GhostMessage ParseGhostMessage(byte[] data)
{
    try
    {
        string text = System.Text.Encoding.UTF8.GetString(data);
        string[] parts = text.Split('|');
        if (parts.Length < 2) return null;

        var msg = new GhostMessage { Type = parts[0], SessionID = parts[1] };
        switch (parts[0])
        {
            case "GHOST_START":
            msg.OperatorName = parts.Length > 2 ? parts[2] : "";
            break;
            case "GHOST_FRAME":
            if (parts.Length > 2 && int.TryParse(parts[2], out int size))
            {
                msg.FrameSize = size;
                // Frame data follows after header
                int headerLen = System.Text.Encoding.UTF8.GetByteCount(string.Join("|", parts[0], parts[1], parts[2]) + "|");
                if (data.Length > headerLen)
                {
                    msg.FrameData = new byte[data.Length - headerLen];
                    Buffer.BlockCopy(data, headerLen, msg.FrameData, 0, msg.FrameData.Length);
                }
        }
    break;
}
return msg;
}
catch { return null; }
}
#endregion

#region Private Methods
private void CaptureLoop()
{
    while (_isStreaming)
    {
        try
        {
            byte[] frame = CaptureFrame();
            if (frame != null && frame.Length > 0)
            {
                OnFrameRequested?.Invoke(frame);
            }
    }
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
Thread.Sleep(_frameIntervalMs);
}
}

private ImageCodecInfo GetEncoder(ImageFormat format)
{
    ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
    foreach (ImageCodecInfo codec in codecs)
    {
        if (codec.FormatID == format.Guid) return codec;
    }
return null;
}
#endregion
}

public class GhostMessage
{
    public string Type { get; set; }
    public string SessionID { get; set; }
    public string OperatorName { get; set; }
    public int FrameSize { get; set; }
    public byte[] FrameData { get; set; }
}

}
// ═══ Class: GhostRemoteEngine (from 5 sources) ═══
public partial class GhostRemoteEngine : IDisposable
{
    // --- Constants & Fields ---
    private Thread   _captureThread;

    private volatile bool _running;

    private int      _qualityPercent;

    private int      _captureIntervalMs;

    private readonly bool _lowBandwidth;

    private const uint SRCCOPY = 0x00CC0020;

    private GhostSession _currentSession;

    private bool _isStreaming;

    private Thread _streamThread;

    private int _frameIntervalMs = 1000; // كل ثانية

    private int _jpegQuality = 40;       // جودة منخفضة للسرعة


    // --- Properties ---
    public int    FramesSent    { get; private set; }

    public long   BytesSent     { get; private set; }

    public double FPS           { get; private set; }

    public bool   IsActive      => _running;

    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Enterprise_v3.4.0\EJLive.Core\Engine\GhostRemoteEngine.cs
    public bool IsActive => _currentSession != null && _currentSession.Status == GhostSessionStatus.Active;

    public GhostSession CurrentSession => _currentSession;

    public int FrameInterval { get => _frameIntervalMs; set => _frameIntervalMs = Math.Max(200, value); }

    public int JpegQuality { get => _jpegQuality; set => _jpegQuality = Math.Max(10, Math.Min(100, value)); }

    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Winform\EJLive.Core\Engine\GhostRemoteEngine.cs
    public bool IsActive => _currentSession != null && _currentSession.Status == GhostSessionStatus.Active;

    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_menusai\EJLive.Core\Engine\GhostRemoteEngine.cs
    public bool IsActive => _currentSession != null && _currentSession.Status == GhostSessionStatus.Active;

    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Current_2026-06-10\EJLive.Core\Engine\GhostRemoteEngine.cs
    public bool IsActive => _currentSession != null && _currentSession.Status == GhostSessionStatus.Active;


    // --- Constructors ---
    public GhostRemoteEngine(int qualityPercent = 75, bool lowBandwidth = false)
    {
        _qualityPercent    = Clamp(qualityPercent, 10, 100);
        _lowBandwidth      = lowBandwidth;
        _captureIntervalMs = lowBandwidth ? 2000 : 500; // GSM: 1 fps, LAN: 2 fps
    }


// --- Methods ---
[DllImport("user32.dll")] private static extern IntPtr GetDesktopWindow();

// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Enterprise_Ultimate\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("user32.dll")] private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

[DllImport("gdi32.dll")]  private static extern bool BitBlt(IntPtr hdcDest, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hdcSrc, int nXSrc, int nYSrc, uint dwRop);

// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Enterprise_Ultimate\EJLive.Core\Engine\GhostRemoteEngine.cs
[DllImport("gdi32.dll")]  private static extern bool DeleteObject(IntPtr hObject);

public void SetQuality(int percent, int intervalMs = -1)
{
    _qualityPercent    = Clamp(percent, 10, 100);
    if (intervalMs > 0) _captureIntervalMs = intervalMs;
}

private static int Clamp(int value, int min, int max)
{
    return value < min ? min : (value > max ? max : value);
}

public void Start()
{
    if (_running) return;
    _running = true;
    _captureThread = new Thread(CaptureLoop)
    {
        IsBackground = true,
        Name         = "EJLive.GhostCapture",
        Priority     = ThreadPriority.BelowNormal // لا يؤثر على الصراف
    };
_captureThread.Start();
OnLog?.Invoke(this, $"Ghost View started — quality={_qualityPercent}% interval={_captureIntervalMs}ms");
}

public void Stop()
{
    _running = false;
    OnLog?.Invoke(this, "Ghost View stopped.");
}

private void CaptureLoop()
{
    var lastFrame    = DateTime.UtcNow;
    var frameTracker = 0;
    var lastFpsCalc  = DateTime.UtcNow;

    while (_running)
    {
        try
        {
            var frame = CaptureScreenJpeg();
            if (frame != null && frame.Length > 0)
            {
                OnFrameCaptured?.Invoke(this, frame);
                FramesSent++;
                BytesSent += frame.Length;
                frameTracker++;
            }

        // حساب FPS كل ثانية
        var elapsed = (DateTime.UtcNow - lastFpsCalc).TotalSeconds;
        if (elapsed >= 1.0)
        {
            FPS          = frameTracker / elapsed;
            frameTracker = 0;
            lastFpsCalc  = DateTime.UtcNow;
        }

    Thread.Sleep(_captureIntervalMs);
}
catch (ThreadAbortException) { break; }
catch (Exception ex)
{
    OnError?.Invoke(this, $"Capture error: {ex.Message}");
    Thread.Sleep(2000);
}
}
}

private byte[] CaptureScreenJpeg()
{
    var bounds = GetScreenBounds();
    var width  = bounds.Width;
    var height = bounds.Height;

    // تقليص الدقة للشبكات البطيئة
    if (_lowBandwidth)
    {
        width  = width  / 2;
        height = height / 2;
    }

var hWnd  = GetDesktopWindow();
var hDC   = GetWindowDC(hWnd);
var hMemDC = CreateCompatibleDC(hDC);
var hBmp  = CreateCompatibleBitmap(hDC, width, height);
var hOld  = SelectObject(hMemDC, hBmp);

bool captured = BitBlt(hMemDC, 0, 0, width, height, hDC, bounds.X, bounds.Y, SRCCOPY);

SelectObject(hMemDC, hOld);
DeleteDC(hMemDC);
ReleaseDC(hWnd, hDC);

if (!captured)
{
    DeleteObject(hBmp);
    return null;
}

byte[] jpegBytes = null;
try
{
    using var bmp = Image.FromHbitmap(hBmp);
    using var ms  = new MemoryStream();
    var encoder   = GetJpegEncoder();
    var encParms  = new EncoderParameters(1);
    encParms.Param[0] = new EncoderParameter(Encoder.Quality, (long)_qualityPercent);
    bmp.Save(ms, encoder, encParms);
    jpegBytes = ms.ToArray();
}
finally
{
    DeleteObject(hBmp);
}
return jpegBytes;
}

private System.Drawing.Rectangle GetScreenBounds()
{
    return System.Windows.Forms.Screen.PrimaryScreen?.Bounds
    ?? new System.Drawing.Rectangle(0, 0, 1024, 768);
}

private ImageCodecInfo GetJpegEncoder()
{
    foreach (var codec in ImageCodecInfo.GetImageEncoders())
    if (codec.MimeType == "image/jpeg") return codec;
    return null;
}

public void Dispose() => Stop();

private readonly object _lock = new object();

private Size _captureSize = new Size(1024, 768);

public void StartClientStreaming(string sessionId)
{
    if (_isStreaming) return;
    _isStreaming = true;

    _currentSession = new GhostSession
    {
        SessionID = sessionId,
        StartTime = DateTime.Now,
        Status = GhostSessionStatus.Active,
        IsViewOnly = true,
        ATMUnaffected = true,
        NoLogout = true,
        ScreenNotLocked = true
    };

_streamThread = new Thread(CaptureLoop) { IsBackground = true, Name = "GhostCaptureThread" };
_streamThread.Start();

OnLog?.Invoke("[Ghost] Client streaming started - View Only mode (ATM unaffected)");
OnSessionStarted?.Invoke(_currentSession);
}

public void StopClientStreaming()
{
    _isStreaming = false;
    if (_currentSession != null)
    {
        _currentSession.Status = GhostSessionStatus.Disconnected;
        _currentSession.EndTime = DateTime.Now;
        OnSessionEnded?.Invoke(_currentSession);
    }
OnLog?.Invoke("[Ghost] Client streaming stopped");
}

public byte[] CaptureFrame()
{
    try
    {
        var bounds = System.Windows.Forms.Screen.PrimaryScreen.Bounds;
        using (var bmp = new Bitmap(bounds.Width, bounds.Height, PixelFormat.Format24bppRgb))
        {
            using (var g = Graphics.FromImage(bmp))
            {
                g.CopyFromScreen(bounds.Location, Point.Empty, bounds.Size, CopyPixelOperation.SourceCopy);
            }

        // تصغير الحجم للإرسال السريع
        using (var resized = new Bitmap(bmp, _captureSize))
        using (var ms = new MemoryStream())
        {
            var encoder = GetEncoder(ImageFormat.Jpeg);
            var encoderParams = new EncoderParameters(1);
            encoderParams.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, (long)_jpegQuality);
            resized.Save(ms, encoder, encoderParams);
            return ms.ToArray();
        }
}
}
catch (Exception ex)
{
    OnError?.Invoke(ex);
    return null;
}
}

public GhostSession StartServerSession(string atmId, string operatorName)
{
    _currentSession = new GhostSession
    {
        SessionID = Guid.NewGuid().ToString("N").Substring(0, 12),
        ATM_ID = atmId,
        OperatorName = operatorName,
        StartTime = DateTime.Now,
        Status = GhostSessionStatus.Connecting,
        IsViewOnly = true,
        ATMUnaffected = true,
        NoLogout = true,
        ScreenNotLocked = true
    };

OnLog?.Invoke($"[Ghost] Server session started for ATM: {atmId} by {operatorName}");
OnSessionStarted?.Invoke(_currentSession);
return _currentSession;
}

public void EndServerSession()
{
    if (_currentSession != null)
    {
        _currentSession.Status = GhostSessionStatus.Disconnected;
        _currentSession.EndTime = DateTime.Now;
        _currentSession.ActivityLog.Add($"Session ended at {DateTime.Now:HH:mm:ss}");
        OnSessionEnded?.Invoke(_currentSession);
        OnLog?.Invoke($"[Ghost] Session ended: {_currentSession.SessionID}");
    }
_currentSession = null;
}

public void ProcessReceivedFrame(byte[] frameData)
{
    if (frameData == null || frameData.Length == 0) return;
    if (_currentSession != null)
    {
        _currentSession.Status = GhostSessionStatus.Active;
        _currentSession.ActivityLog.Add($"Frame received: {frameData.Length} bytes at {DateTime.Now:HH:mm:ss}");
    }
OnFrameReceived?.Invoke(frameData);
}

public static byte[] BuildGhostStartRequest(string sessionId, string operatorName)
{
    string msg = $"GHOST_START|{sessionId}|{operatorName}|{DateTime.Now:yyyy-MM-dd HH:mm:ss}";
    return System.Text.Encoding.UTF8.GetBytes(msg);
}

public static byte[] BuildGhostStopRequest(string sessionId)
{
    string msg = $"GHOST_STOP|{sessionId}|{DateTime.Now:yyyy-MM-dd HH:mm:ss}";
    return System.Text.Encoding.UTF8.GetBytes(msg);
}

public static byte[] BuildFrameMessage(string sessionId, byte[] frameData)
{
    string header = $"GHOST_FRAME|{sessionId}|{frameData.Length}|";
    byte[] headerBytes = System.Text.Encoding.UTF8.GetBytes(header);
    byte[] message = new byte[headerBytes.Length + frameData.Length];
    Buffer.BlockCopy(headerBytes, 0, message, 0, headerBytes.Length);
    Buffer.BlockCopy(frameData, 0, message, headerBytes.Length, frameData.Length);
    return message;
}

public static GhostMessage ParseGhostMessage(byte[] data)
{
    try
    {
        string text = System.Text.Encoding.UTF8.GetString(data);
        string[] parts = text.Split('|');
        if (parts.Length < 2) return null;

        var msg = new GhostMessage { Type = parts[0], SessionID = parts[1] };
        switch (parts[0])
        {
            case "GHOST_START":
            msg.OperatorName = parts.Length > 2 ? parts[2] : "";
            break;
            case "GHOST_FRAME":
            if (parts.Length > 2 && int.TryParse(parts[2], out int size))
            {
                msg.FrameSize = size;
                // Frame data follows after header
                int headerLen = System.Text.Encoding.UTF8.GetByteCount(string.Join("|", parts[0], parts[1], parts[2]) + "|");
                if (data.Length > headerLen)
                {
                    msg.FrameData = new byte[data.Length - headerLen];
                    Buffer.BlockCopy(data, headerLen, msg.FrameData, 0, msg.FrameData.Length);
                }
        }
    break;
}
return msg;
}
catch { return null; }
}

// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Enterprise_v3.4.0\EJLive.Core\Engine\GhostRemoteEngine.cs
private void CaptureLoop()
{
    while (_isStreaming)
    {
        try
        {
            byte[] frame = CaptureFrame();
            if (frame != null && frame.Length > 0)
            {
                OnFrameRequested?.Invoke(frame);
            }
    }
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
Thread.Sleep(_frameIntervalMs);
}
}

private ImageCodecInfo GetEncoder(ImageFormat format)
{
    ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
    foreach (ImageCodecInfo codec in codecs)
    {
        if (codec.FormatID == format.Guid) return codec;
    }
return null;
}

// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Winform\EJLive.Core\Engine\GhostRemoteEngine.cs
private void CaptureLoop()
{
    while (_isStreaming)
    {
        try
        {
            byte[] frame = CaptureFrame();
            if (frame != null && frame.Length > 0)
            {
                OnFrameRequested?.Invoke(frame);
            }
    }
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
Thread.Sleep(_frameIntervalMs);
}
}

// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_menusai\EJLive.Core\Engine\GhostRemoteEngine.cs
private void CaptureLoop()
{
    while (_isStreaming)
    {
        try
        {
            byte[] frame = CaptureFrame();
            if (frame != null && frame.Length > 0)
            {
                OnFrameRequested?.Invoke(frame);
            }
    }
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
Thread.Sleep(_frameIntervalMs);
}
}

// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Current_2026-06-10\EJLive.Core\Engine\GhostRemoteEngine.cs
private void CaptureLoop()
{
    while (_isStreaming)
    {
        try
        {
            byte[] frame = CaptureFrame();
            if (frame != null && frame.Length > 0)
            {
                OnFrameRequested?.Invoke(frame);
            }
    }
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
Thread.Sleep(_frameIntervalMs);
}
}


// --- Events ---
public event EventHandler<byte[]>  OnFrameCaptured;   // JPEG bytes

public event EventHandler<string>  OnError;

public event EventHandler<string>  OnLog;

// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Enterprise_v3.4.0\EJLive.Core\Engine\GhostRemoteEngine.cs
public event Action<string> OnLog;

public event Action<byte[]> OnFrameReceived;      // Server side: frame from client

public event Action<byte[]> OnFrameRequested;     // Client side: send frame to server

public event Action<GhostSession> OnSessionStarted;

public event Action<GhostSession> OnSessionEnded;

// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Enterprise_v3.4.0\EJLive.Core\Engine\GhostRemoteEngine.cs
public event Action<Exception> OnError;

// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Winform\EJLive.Core\Engine\GhostRemoteEngine.cs
public event Action<string> OnLog;

// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Winform\EJLive.Core\Engine\GhostRemoteEngine.cs
public event Action<Exception> OnError;

// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_menusai\EJLive.Core\Engine\GhostRemoteEngine.cs
public event Action<string> OnLog;

// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_menusai\EJLive.Core\Engine\GhostRemoteEngine.cs
public event Action<Exception> OnError;

// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Current_2026-06-10\EJLive.Core\Engine\GhostRemoteEngine.cs
public event Action<string> OnLog;

// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Current_2026-06-10\EJLive.Core\Engine\GhostRemoteEngine.cs
public event Action<Exception> OnError;

}
/// <summary>
/// محرك الوصول الشبحي الكامل — Ghost View (View-Only Remote Screen)
/// لا يؤثر على الصراف ولا يُعيق العمليات
/// لا يُسجل خروج المستخدم ولا يقفل شاشة الصراف أمام العملاء
/// يضغط الشاشة JPEG ويُرسلها عبر NetworkEngine كل N ms
/// </summary>
public class GhostRemoteEngine : IDisposable
{
    private Thread   _captureThread;
    private volatile bool _running;
    private int      _qualityPercent;
    private int      _captureIntervalMs;

    // قرارات الشاشة لتقليل الضغط على شبكات GSM/CDMA
    private readonly bool _lowBandwidth;

    public event EventHandler<byte[]>  OnFrameCaptured;   // JPEG bytes
    public event EventHandler<string>  OnError;
    public event EventHandler<string>  OnLog;

    // إحصائيات
    public int    FramesSent    { get; private set; }
    public long   BytesSent     { get; private set; }
    public double FPS           { get; private set; }
    public bool   IsActive      => _running;

    // WinAPI للتقاط الشاشة بدون التدخل في الجلسة الحالية
    [DllImport("user32.dll")] private static extern IntPtr GetDesktopWindow();
    [DllImport("user32.dll")] private static extern IntPtr GetWindowDC(IntPtr hWnd);
    [DllImport("gdi32.dll")]  private static extern bool BitBlt(IntPtr hdcDest, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hdcSrc, int nXSrc, int nYSrc, uint dwRop);
    [DllImport("user32.dll")] private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);
    [DllImport("gdi32.dll")]  private static extern IntPtr CreateCompatibleDC(IntPtr hdc);
    [DllImport("gdi32.dll")]  private static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int nWidth, int nHeight);
    [DllImport("gdi32.dll")]  private static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);
    [DllImport("gdi32.dll")]  private static extern bool DeleteDC(IntPtr hdc);
    [DllImport("gdi32.dll")]  private static extern bool DeleteObject(IntPtr hObject);

    private const uint SRCCOPY = 0x00CC0020;

    public GhostRemoteEngine(int qualityPercent = 75, bool lowBandwidth = false)
    {
        _qualityPercent    = Clamp(qualityPercent, 10, 100);
        _lowBandwidth      = lowBandwidth;
        _captureIntervalMs = lowBandwidth ? 2000 : 500; // GSM: 1 fps, LAN: 2 fps
    }

public void SetQuality(int percent, int intervalMs = -1)
{
    _qualityPercent    = Clamp(percent, 10, 100);
    if (intervalMs > 0) _captureIntervalMs = intervalMs;
}

private static int Clamp(int value, int min, int max)
{
    return value < min ? min : (value > max ? max : value);
}

// ==========================================
// التشغيل
// ==========================================

public void Start()
{
    if (_running) return;
    _running = true;
    _captureThread = new Thread(CaptureLoop)
    {
        IsBackground = true,
        Name         = "EJLive.GhostCapture",
        Priority     = ThreadPriority.BelowNormal // لا يؤثر على الصراف
    };
_captureThread.Start();
OnLog?.Invoke(this, $"Ghost View started — quality={_qualityPercent}% interval={_captureIntervalMs}ms");
}

public void Stop()
{
    _running = false;
    OnLog?.Invoke(this, "Ghost View stopped.");
}

// ==========================================
// حلقة الالتقاط
// ==========================================

private void CaptureLoop()
{
    var lastFrame    = DateTime.UtcNow;
    var frameTracker = 0;
    var lastFpsCalc  = DateTime.UtcNow;

    while (_running)
    {
        try
        {
            var frame = CaptureScreenJpeg();
            if (frame != null && frame.Length > 0)
            {
                OnFrameCaptured?.Invoke(this, frame);
                FramesSent++;
                BytesSent += frame.Length;
                frameTracker++;
            }

        // حساب FPS كل ثانية
        var elapsed = (DateTime.UtcNow - lastFpsCalc).TotalSeconds;
        if (elapsed >= 1.0)
        {
            FPS          = frameTracker / elapsed;
            frameTracker = 0;
            lastFpsCalc  = DateTime.UtcNow;
        }

    Thread.Sleep(_captureIntervalMs);
}
catch (ThreadAbortException) { break; }
catch (Exception ex)
{
    OnError?.Invoke(this, $"Capture error: {ex.Message}");
    Thread.Sleep(2000);
}
}
}

// ==========================================
// التقاط الشاشة (WinAPI — لا يُعيق الجلسة)
// ==========================================

private byte[] CaptureScreenJpeg()
{
    var bounds = GetScreenBounds();
    var width  = bounds.Width;
    var height = bounds.Height;

    // تقليص الدقة للشبكات البطيئة
    if (_lowBandwidth)
    {
        width  = width  / 2;
        height = height / 2;
    }

var hWnd  = GetDesktopWindow();
var hDC   = GetWindowDC(hWnd);
var hMemDC = CreateCompatibleDC(hDC);
var hBmp  = CreateCompatibleBitmap(hDC, width, height);
var hOld  = SelectObject(hMemDC, hBmp);

bool captured = BitBlt(hMemDC, 0, 0, width, height, hDC, bounds.X, bounds.Y, SRCCOPY);

SelectObject(hMemDC, hOld);
DeleteDC(hMemDC);
ReleaseDC(hWnd, hDC);

if (!captured)
{
    DeleteObject(hBmp);
    return null;
}

byte[] jpegBytes = null;
try
{
    using var bmp = Image.FromHbitmap(hBmp);
    using var ms  = new MemoryStream();
    var encoder   = GetJpegEncoder();
    var encParms  = new EncoderParameters(1);
    encParms.Param[0] = new EncoderParameter(Encoder.Quality, (long)_qualityPercent);
    bmp.Save(ms, encoder, encParms);
    jpegBytes = ms.ToArray();
}
finally
{
    DeleteObject(hBmp);
}
return jpegBytes;
}

private System.Drawing.Rectangle GetScreenBounds()
{
    return System.Windows.Forms.Screen.PrimaryScreen?.Bounds
    ?? new System.Drawing.Rectangle(0, 0, 1024, 768);
}

private ImageCodecInfo GetJpegEncoder()
{
    foreach (var codec in ImageCodecInfo.GetImageEncoders())
    if (codec.MimeType == "image/jpeg") return codec;
    return null;
}

public void Dispose() => Stop();
}
public partial class GhostRemoteEngine : IDisposable
{
    private GhostSession _currentSession;
    private bool _isStreaming;
    private Thread _streamThread;
    private int _frameIntervalMs = 1000; // كل ثانية
    private int _jpegQuality = 40;       // جودة منخفضة للسرعة
    private Thread   _captureThread;
    private volatile bool _running;
    private int      _qualityPercent;
    private int      _captureIntervalMs;
    private readonly bool _lowBandwidth;
    private const uint SRCCOPY = 0x00CC0020;
    public int FrameInterval
    {
        get => _frameIntervalMs;
        set => _frameIntervalMs = Math.Max(200, value);
    }
public int JpegQuality
{
    get => _jpegQuality;
    set => _jpegQuality = Math.Max(10, Math.Min(100, value));
}
public bool IsActive => _currentSession != null && _currentSession.Status == GhostSessionStatus.Active;
public GhostSession CurrentSession => _currentSession;
public int FrameInterval { get => _frameIntervalMs; set => _frameIntervalMs = Math.Max(200, value); }
public int JpegQuality { get => _jpegQuality; set => _jpegQuality = Math.Max(10, Math.Min(100, value)); }
public bool   IsActive      => _running;
public int    FramesSent    { get; private set; }
public long   BytesSent     { get; private set; }
public double FPS           { get; private set; }
public bool IsSessionActive => _currentSession != null && _currentSession.Status == GhostSessionStatus.Active;
public GhostRemoteEngine(int qualityPercent = 75, bool lowBandwidth = false)
{
    _qualityPercent    = Clamp(qualityPercent, 10, 100);
    _lowBandwidth      = lowBandwidth;
    _captureIntervalMs = lowBandwidth ? 2000 : 500; // GSM: 1 fps, LAN: 2 fps
}
private readonly object _lock = new object();
private Size _captureSize = new Size(1024, 768);
public void StartClientStreaming(string sessionId)
{
    if (_isStreaming) return;
    _isStreaming = true;
    _currentSession = new GhostSession
    {
        SessionID = sessionId,
        StartTime = DateTime.Now,
        Status = GhostSessionStatus.Active,
        IsViewOnly = true,
        ATMUnaffected = true,
        NoLogout = true,
        ScreenNotLocked = true
    };
_streamThread = new Thread(CaptureLoop) { IsBackground = true, Name = "GhostCaptureThread" };
_streamThread.Start();
OnLog?.Invoke("[Ghost] Client streaming started - View Only mode (ATM unaffected)");
OnSessionStarted?.Invoke(_currentSession);
}
public void StopClientStreaming()
{
    _isStreaming = false;
    if (_currentSession != null)
    {
        _currentSession.Status = GhostSessionStatus.Disconnected;
        _currentSession.EndTime = DateTime.Now;
        OnSessionEnded?.Invoke(_currentSession);
    }
OnLog?.Invoke("[Ghost] Client streaming stopped");
}
public byte[] CaptureFrame()
{
    try
    {
        var bounds = System.Windows.Forms.Screen.PrimaryScreen.Bounds;
        using (var bmp = new Bitmap(bounds.Width, bounds.Height, PixelFormat.Format24bppRgb))
        {
            using (var g = Graphics.FromImage(bmp))
            {
                g.CopyFromScreen(bounds.Location, Point.Empty, bounds.Size, CopyPixelOperation.SourceCopy);
            }
        // تصغير الحجم للإرسال السريع
        using (var resized = new Bitmap(bmp, _captureSize))
        using (var ms = new MemoryStream())
        {
            var encoder = GetEncoder(ImageFormat.Jpeg);
            var encoderParams = new EncoderParameters(1);
            encoderParams.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, (long)_jpegQuality);
            resized.Save(ms, encoder, encoderParams);
            return ms.ToArray();
        }
}
}
catch (Exception ex)
{
    OnError?.Invoke(ex);
    return null;
}
}
public GhostSession StartServerSession(string atmId, string operatorName)
{
    _currentSession = new GhostSession
    {
        SessionID = Guid.NewGuid().ToString("N").Substring(0, 12),
        ATM_ID = atmId,
        OperatorName = operatorName,
        StartTime = DateTime.Now,
        Status = GhostSessionStatus.Connecting,
        IsViewOnly = true,
        ATMUnaffected = true,
        NoLogout = true,
        ScreenNotLocked = true
    };
OnLog?.Invoke($"[Ghost] Server session started for ATM: {atmId} by {operatorName}");
OnSessionStarted?.Invoke(_currentSession);
return _currentSession;
}
public void EndServerSession()
{
    if (_currentSession != null)
    {
        _currentSession.Status = GhostSessionStatus.Disconnected;
        _currentSession.EndTime = DateTime.Now;
        _currentSession.ActivityLog.Add($"Session ended at {DateTime.Now:HH:mm:ss}");
        OnSessionEnded?.Invoke(_currentSession);
        OnLog?.Invoke($"[Ghost] Session ended: {_currentSession.SessionID}");
    }
_currentSession = null;
}
public void ProcessReceivedFrame(byte[] frameData)
{
    if (frameData == null || frameData.Length == 0) return;
    if (_currentSession != null)
    {
        _currentSession.Status = GhostSessionStatus.Active;
        _currentSession.ActivityLog.Add($"Frame received: {frameData.Length} bytes at {DateTime.Now:HH:mm:ss}");
    }
OnFrameReceived?.Invoke(frameData);
}
public static byte[] BuildGhostStartRequest(string sessionId, string operatorName)
{
    string msg = $"GHOST_START|{sessionId}|{operatorName}|{DateTime.Now:yyyy-MM-dd HH:mm:ss}";
    return System.Text.Encoding.UTF8.GetBytes(msg);
}
public static byte[] BuildGhostStopRequest(string sessionId)
{
    string msg = $"GHOST_STOP|{sessionId}|{DateTime.Now:yyyy-MM-dd HH:mm:ss}";
    return System.Text.Encoding.UTF8.GetBytes(msg);
}
public static byte[] BuildFrameMessage(string sessionId, byte[] frameData)
{
    string header = $"GHOST_FRAME|{sessionId}|{frameData.Length}|";
    byte[] headerBytes = System.Text.Encoding.UTF8.GetBytes(header);
    byte[] message = new byte[headerBytes.Length + frameData.Length];
    Buffer.BlockCopy(headerBytes, 0, message, 0, headerBytes.Length);
    Buffer.BlockCopy(frameData, 0, message, headerBytes.Length, frameData.Length);
    return message;
}
public static GhostMessage ParseGhostMessage(byte[] data)
{
    try
    {
        string text = System.Text.Encoding.UTF8.GetString(data);
        string[] parts = text.Split('|');
        if (parts.Length < 2) return null;
        var msg = new GhostMessage { Type = parts[0], SessionID = parts[1] };
        switch (parts[0])
        {
            case "GHOST_START":
            msg.OperatorName = parts.Length > 2 ? parts[2] : "";
            break;
            case "GHOST_FRAME":
            if (parts.Length > 2 && int.TryParse(parts[2], out int size))
            {
                msg.FrameSize = size;
                // Frame data follows after header
                int headerLen = System.Text.Encoding.UTF8.GetByteCount(string.Join("|", parts[0], parts[1], parts[2]) + "|");
                if (data.Length > headerLen)
                {
                    msg.FrameData = new byte[data.Length - headerLen];
                    Buffer.BlockCopy(data, headerLen, msg.FrameData, 0, msg.FrameData.Length);
                }
        }
    break;
}
return msg;
}
catch { return null; }
}
private void CaptureLoop()
{
    while (_isStreaming)
    {
        try
        {
            byte[] frame = CaptureFrame();
            if (frame != null && frame.Length > 0)
            {
                OnFrameRequested?.Invoke(frame);
            }
    }
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
Thread.Sleep(_frameIntervalMs);
}
}
private ImageCodecInfo GetEncoder(ImageFormat format)
{
    ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
    foreach (ImageCodecInfo codec in codecs)
    {
        if (codec.FormatID == format.Guid) return codec;
    }
return null;
}
public void Start() => IsActive = true;
public void Stop() => IsActive = false;
public void Dispose() => Stop();
[DllImport("user32.dll")] private static extern IntPtr GetDesktopWindow();
[DllImport("user32.dll")] private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);
[DllImport("gdi32.dll")]  private static extern bool BitBlt(IntPtr hdcDest, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hdcSrc, int nXSrc, int nYSrc, uint dwRop);
[DllImport("gdi32.dll")]  private static extern bool DeleteObject(IntPtr hObject);
public void SetQuality(int percent, int intervalMs = -1)
{
    _qualityPercent    = Clamp(percent, 10, 100);
    if (intervalMs > 0) _captureIntervalMs = intervalMs;
}
private static int Clamp(int value, int min, int max)
{
    return value < min ? min : (value > max ? max : value);
}
public void Start()
{
    if (_running) return;
    _running = true;
    _captureThread = new Thread(CaptureLoop)
    {
        IsBackground = true,
        Name         = "EJLive.GhostCapture",
        Priority     = ThreadPriority.BelowNormal // لا يؤثر على الصراف
    };
_captureThread.Start();
OnLog?.Invoke(this, $"Ghost View started — quality={_qualityPercent}% interval={_captureIntervalMs}ms");
}
public void Stop()
{
    _running = false;
    OnLog?.Invoke(this, "Ghost View stopped.");
}
private void CaptureLoop()
{
    var lastFrame    = DateTime.UtcNow;
    var frameTracker = 0;
    var lastFpsCalc  = DateTime.UtcNow;
    while (_running)
    {
        try
        {
            var frame = CaptureScreenJpeg();
            if (frame != null && frame.Length > 0)
            {
                OnFrameCaptured?.Invoke(this, frame);
                FramesSent++;
                BytesSent += frame.Length;
                frameTracker++;
            }
        // حساب FPS كل ثانية
        var elapsed = (DateTime.UtcNow - lastFpsCalc).TotalSeconds;
        if (elapsed >= 1.0)
        {
            FPS          = frameTracker / elapsed;
            frameTracker = 0;
            lastFpsCalc  = DateTime.UtcNow;
        }
    Thread.Sleep(_captureIntervalMs);
}
catch (ThreadAbortException) { break; }
catch (Exception ex)
{
    OnError?.Invoke(this, $"Capture error: {ex.Message}");
    Thread.Sleep(2000);
}
}
}
private byte[] CaptureScreenJpeg()
{
    var bounds = GetScreenBounds();
    var width  = bounds.Width;
    var height = bounds.Height;
    // تقليص الدقة للشبكات البطيئة
    if (_lowBandwidth)
    {
        width  = width  / 2;
        height = height / 2;
    }
var hWnd  = GetDesktopWindow();
var hDC   = GetWindowDC(hWnd);
var hMemDC = CreateCompatibleDC(hDC);
var hBmp  = CreateCompatibleBitmap(hDC, width, height);
var hOld  = SelectObject(hMemDC, hBmp);
bool captured = BitBlt(hMemDC, 0, 0, width, height, hDC, bounds.X, bounds.Y, SRCCOPY);
SelectObject(hMemDC, hOld);
DeleteDC(hMemDC);
ReleaseDC(hWnd, hDC);
if (!captured)
{
    DeleteObject(hBmp);
    return null;
}
byte[] jpegBytes = null;
try
{
    using var bmp = Image.FromHbitmap(hBmp);
    using var ms  = new MemoryStream();
    var encoder   = GetJpegEncoder();
    var encParms  = new EncoderParameters(1);
    encParms.Param[0] = new EncoderParameter(Encoder.Quality, (long)_qualityPercent);
    bmp.Save(ms, encoder, encParms);
    jpegBytes = ms.ToArray();
}
finally
{
    DeleteObject(hBmp);
}
return jpegBytes;
}
private System.Drawing.Rectangle GetScreenBounds()
{
    return System.Windows.Forms.Screen.PrimaryScreen?.Bounds
    ?? new System.Drawing.Rectangle(0, 0, 1024, 768);
}
private ImageCodecInfo GetJpegEncoder()
{
    foreach (var codec in ImageCodecInfo.GetImageEncoders())
    if (codec.MimeType == "image/jpeg") return codec;
    return null;
}
public void StartClientStreaming(string sessionId)
{
    if (_isStreaming) return;
    _isStreaming = true;
    _currentSession = new GhostSession
    {
        SessionID = sessionId,
        StartTime = DateTime.Now,
        Status = GhostSessionStatus.Active,
        IsViewOnly = true,
        ATMUnaffected = true,
        NoLogout = true,
        ScreenNotLocked = true
    };
_streamThread = new Thread(CaptureLoopClient)
{
    IsBackground = true,
    Name = "GhostCaptureThread"
};
_streamThread.Start();
OnLogAction?.Invoke("[Ghost] Client streaming started - View Only mode (ATM unaffected)");
OnSessionStarted?.Invoke(_currentSession);
}
public void StopClientStreaming()
{
    _isStreaming = false;
    if (_currentSession != null)
    {
        _currentSession.Status = GhostSessionStatus.Disconnected;
        _currentSession.EndTime = DateTime.Now;
        OnSessionEnded?.Invoke(_currentSession);
    }
OnLogAction?.Invoke("[Ghost] Client streaming stopped");
}
public byte[] CaptureFrame()
{
    try
    {
        var bounds = System.Windows.Forms.Screen.PrimaryScreen.Bounds;
        using (var bmp = new Bitmap(bounds.Width, bounds.Height, PixelFormat.Format24bppRgb))
        {
            using (var g = Graphics.FromImage(bmp))
            {
                g.CopyFromScreen(bounds.Location, Point.Empty, bounds.Size, CopyPixelOperation.SourceCopy);
            }
        // تصغير الحجم للإرسال السريع
        using (var resized = new Bitmap(bmp, _captureSize))
        using (var ms = new MemoryStream())
        {
            var encoder = GetEncoder(ImageFormat.Jpeg);
            var encoderParams = new EncoderParameters(1);
            encoderParams.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, (long)_jpegQuality);
            resized.Save(ms, encoder, encoderParams);
            return ms.ToArray();
        }
}
}
catch (Exception ex)
{
    OnException?.Invoke(ex);
    return null;
}
}
public GhostSession StartServerSession(string atmId, string operatorName)
{
    _currentSession = new GhostSession
    {
        SessionID = Guid.NewGuid().ToString("N").Substring(0, 12),
        ATM_ID = atmId,
        OperatorName = operatorName,
        StartTime = DateTime.Now,
        Status = GhostSessionStatus.Connecting,
        IsViewOnly = true,
        ATMUnaffected = true,
        NoLogout = true,
        ScreenNotLocked = true
    };
OnLogAction?.Invoke($"[Ghost] Server session started for ATM: {atmId} by {operatorName}");
OnSessionStarted?.Invoke(_currentSession);
return _currentSession;
}
public void EndServerSession()
{
    if (_currentSession != null)
    {
        _currentSession.Status = GhostSessionStatus.Disconnected;
        _currentSession.EndTime = DateTime.Now;
        _currentSession.ActivityLog.Add($"Session ended at {DateTime.Now:HH:mm:ss}");
        OnSessionEnded?.Invoke(_currentSession);
        OnLogAction?.Invoke($"[Ghost] Session ended: {_currentSession.SessionID}");
    }
_currentSession = null;
}
private void CaptureLoopClient()
{
    while (_isStreaming)
    {
        try
        {
            byte[] frame = CaptureFrame();
            if (frame != null && frame.Length > 0)
            {
                OnFrameRequested?.Invoke(frame);
            }
    }
catch (Exception ex)
{
    OnException?.Invoke(ex);
}
Thread.Sleep(_frameIntervalMs);
}
}
public event Action<string> OnLog;
public event Action<byte[]> OnFrameReceived;      // Server side: frame from client
public event Action<byte[]> OnFrameRequested;     // Client side: send frame to server
public event Action<GhostSession> OnSessionStarted;
public event Action<GhostSession> OnSessionEnded;
public event Action<Exception> OnError;
public event EventHandler<byte[]>  OnFrameCaptured;   // JPEG bytes
public event EventHandler<string>  OnError;
public event EventHandler<string>  OnLog;
public event Action<string> OnLogAction;
public event Action<Exception> OnException;
public class GhostRemoteEngine
{
#region Events
    public event Action<string> OnLog;
    public event Action<byte[]> OnFrameReceived;      // Server side: frame from client
    public event Action<byte[]> OnFrameRequested;     // Client side: send frame to server
    public event Action<GhostSession> OnSessionStarted;
    public event Action<GhostSession> OnSessionEnded;
    public event Action<Exception> OnError;
#endregion
#region Fields
    private GhostSession _currentSession;
    private bool _isStreaming;
    private Thread _streamThread;
    private readonly object _lock = new object();
    private int _frameIntervalMs = 1000; // كل ثانية
    private int _jpegQuality = 40;       // جودة منخفضة للسرعة
    private Size _captureSize = new Size(1024, 768);
#endregion
#region Properties
    public bool IsActive => _currentSession != null && _currentSession.Status == GhostSessionStatus.Active;
    public GhostSession CurrentSession => _currentSession;
    public int FrameInterval { get => _frameIntervalMs; set => _frameIntervalMs = Math.Max(200, value); }
    public int JpegQuality { get => _jpegQuality; set => _jpegQuality = Math.Max(10, Math.Min(100, value)); }
#endregion
#region Client Side - Capture and Send
    /// <summary>
    /// بدء التقاط الشاشة وإرسالها (يعمل على الكلاينت)
    /// لا يؤثر على الصراف - فقط التقاط بدون تدخل
    /// </summary>
    public void StartClientStreaming(string sessionId)
    {
        if (_isStreaming) return;
        _isStreaming = true;
        _currentSession = new GhostSession
        {
            SessionID = sessionId,
            StartTime = DateTime.Now,
            Status = GhostSessionStatus.Active,
            IsViewOnly = true,
            ATMUnaffected = true,
            NoLogout = true,
            ScreenNotLocked = true
        };
    _streamThread = new Thread(CaptureLoop) { IsBackground = true, Name = "GhostCaptureThread" };
_streamThread.Start();
OnLog?.Invoke("[Ghost] Client streaming started - View Only mode (ATM unaffected)");
OnSessionStarted?.Invoke(_currentSession);
}
/// <summary>
/// إيقاف التقاط الشاشة
/// </summary>
public void StopClientStreaming()
{
    _isStreaming = false;
    if (_currentSession != null)
    {
        _currentSession.Status = GhostSessionStatus.Disconnected;
        _currentSession.EndTime = DateTime.Now;
        OnSessionEnded?.Invoke(_currentSession);
    }
OnLog?.Invoke("[Ghost] Client streaming stopped");
}
/// <summary>
/// التقاط إطار واحد من الشاشة
/// لا يقفل الشاشة ولا يؤثر على العميل
/// </summary>
public byte[] CaptureFrame()
{
    try
    {
        var bounds = System.Windows.Forms.Screen.PrimaryScreen.Bounds;
        using (var bmp = new Bitmap(bounds.Width, bounds.Height, PixelFormat.Format24bppRgb))
        {
            using (var g = Graphics.FromImage(bmp))
            {
                g.CopyFromScreen(bounds.Location, Point.Empty, bounds.Size, CopyPixelOperation.SourceCopy);
            }
        // تصغير الحجم للإرسال السريع
        using (var resized = new Bitmap(bmp, _captureSize))
        using (var ms = new MemoryStream())
        {
            var encoder = GetEncoder(ImageFormat.Jpeg);
            var encoderParams = new EncoderParameters(1);
            encoderParams.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, (long)_jpegQuality);
            resized.Save(ms, encoder, encoderParams);
            return ms.ToArray();
        }
}
}
catch (Exception ex)
{
    OnError?.Invoke(ex);
    return null;
}
}
#endregion
#region Server Side - Request and Display
/// <summary>
/// بدء جلسة مراقبة من السرفر
/// </summary>
public GhostSession StartServerSession(string atmId, string operatorName)
{
    _currentSession = new GhostSession
    {
        SessionID = Guid.NewGuid().ToString("N").Substring(0, 12),
        ATM_ID = atmId,
        OperatorName = operatorName,
        StartTime = DateTime.Now,
        Status = GhostSessionStatus.Connecting,
        IsViewOnly = true,
        ATMUnaffected = true,
        NoLogout = true,
        ScreenNotLocked = true
    };
OnLog?.Invoke($"[Ghost] Server session started for ATM: {atmId} by {operatorName}");
OnSessionStarted?.Invoke(_currentSession);
return _currentSession;
}
/// <summary>
/// إنهاء جلسة المراقبة
/// </summary>
public void EndServerSession()
{
    if (_currentSession != null)
    {
        _currentSession.Status = GhostSessionStatus.Disconnected;
        _currentSession.EndTime = DateTime.Now;
        _currentSession.ActivityLog.Add($"Session ended at {DateTime.Now:HH:mm:ss}");
        OnSessionEnded?.Invoke(_currentSession);
        OnLog?.Invoke($"[Ghost] Session ended: {_currentSession.SessionID}");
    }
_currentSession = null;
}
/// <summary>
/// معالجة إطار مستلم من الكلاينت
/// </summary>
public void ProcessReceivedFrame(byte[] frameData)
{
    if (frameData == null || frameData.Length == 0) return;
    if (_currentSession != null)
    {
        _currentSession.Status = GhostSessionStatus.Active;
        _currentSession.ActivityLog.Add($"Frame received: {frameData.Length} bytes at {DateTime.Now:HH:mm:ss}");
    }
OnFrameReceived?.Invoke(frameData);
}
#endregion
#region Protocol Messages
/// <summary>
/// بناء رسالة طلب بدء Ghost session
/// </summary>
public static byte[] BuildGhostStartRequest(string sessionId, string operatorName)
{
    string msg = $"GHOST_START|{sessionId}|{operatorName}|{DateTime.Now:yyyy-MM-dd HH:mm:ss}";
    return System.Text.Encoding.UTF8.GetBytes(msg);
}
/// <summary>
/// بناء رسالة إيقاف Ghost session
/// </summary>
public static byte[] BuildGhostStopRequest(string sessionId)
{
    string msg = $"GHOST_STOP|{sessionId}|{DateTime.Now:yyyy-MM-dd HH:mm:ss}";
    return System.Text.Encoding.UTF8.GetBytes(msg);
}
/// <summary>
/// بناء رسالة إطار (Frame)
/// </summary>
public static byte[] BuildFrameMessage(string sessionId, byte[] frameData)
{
    string header = $"GHOST_FRAME|{sessionId}|{frameData.Length}|";
    byte[] headerBytes = System.Text.Encoding.UTF8.GetBytes(header);
    byte[] message = new byte[headerBytes.Length + frameData.Length];
    Buffer.BlockCopy(headerBytes, 0, message, 0, headerBytes.Length);
    Buffer.BlockCopy(frameData, 0, message, headerBytes.Length, frameData.Length);
    return message;
}
/// <summary>
/// تحليل رسالة Ghost
/// </summary>
public static GhostMessage ParseGhostMessage(byte[] data)
{
    try
    {
        string text = System.Text.Encoding.UTF8.GetString(data);
        string[] parts = text.Split('|');
        if (parts.Length < 2) return null;
        var msg = new GhostMessage { Type = parts[0], SessionID = parts[1] };
        switch (parts[0])
        {
            case "GHOST_START":
            msg.OperatorName = parts.Length > 2 ? parts[2] : "";
            break;
            case "GHOST_FRAME":
            if (parts.Length > 2 && int.TryParse(parts[2], out int size))
            {
                msg.FrameSize = size;
                // Frame data follows after header
                int headerLen = System.Text.Encoding.UTF8.GetByteCount(string.Join("|", parts[0], parts[1], parts[2]) + "|");
                if (data.Length > headerLen)
                {
                    msg.FrameData = new byte[data.Length - headerLen];
                    Buffer.BlockCopy(data, headerLen, msg.FrameData, 0, msg.FrameData.Length);
                }
        }
    break;
}
return msg;
}
catch { return null; }
}
#endregion
#region Private Methods
private void CaptureLoop()
{
    while (_isStreaming)
    {
        try
        {
            byte[] frame = CaptureFrame();
            if (frame != null && frame.Length > 0)
            {
                OnFrameRequested?.Invoke(frame);
            }
    }
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
Thread.Sleep(_frameIntervalMs);
}
}
private ImageCodecInfo GetEncoder(ImageFormat format)
{
    ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
    foreach (ImageCodecInfo codec in codecs)
    {
        if (codec.FormatID == format.Guid) return codec;
    }
return null;
}
#endregion
}
public class GhostMessage
{
    public string Type { get; set; }
    public string SessionID { get; set; }
    public string OperatorName { get; set; }
    public int FrameSize { get; set; }
    public byte[] FrameData { get; set; }
}
}
public interface IRemoteSessionPolicy
{

    public interface IRemoteSessionAuditStore
    {

        public interface IRemoteSessionNotifier
        {

            public partial public class GhostRemoteEngine : IDisposable
            {
                private Thread   _captureThread;
                private volatile bool _running;
                private int      _qualityPercent;
                private int      _captureIntervalMs;
                private readonly bool _lowBandwidth;
                public bool   IsActive      => _running;
                private GhostSession _currentSession;
                private bool _isStreaming;
                private Thread _streamThread;
                private readonly object _lock = new object();
                private int _frameIntervalMs = 1000;
                private int _jpegQuality = 40;
                private Size _captureSize = new Size(1024, 768);
                public GhostSession CurrentSession => _currentSession;
                public bool IsSessionActive => _currentSession != null && _currentSession.Status == GhostSessionStatus.Active;
                private readonly List<RemoteAssistanceSession> _sessions = new();
                private const uint SRCCOPY = 0x00CC0020;
                public GhostRemoteEngine(int qualityPercent = 75, bool lowBandwidth = false)
                {
                    public int    FramesSent    { get; private set; }
                    public long   BytesSent     { get; private set; }
                    public double FPS           { get; private set; }
                    public class GhostRemoteEngine
                    {
#region Events
                        public event Action<string> OnLog;
                        public event Action<byte[]> OnFrameReceived;      // Server side: frame from client
                        public event Action<byte[]> OnFrameRequested;     // Client side: send frame to server
                        public event Action<GhostSession> OnSessionStarted;
                        public event Action<GhostSession> OnSessionEnded;
                        public event Action<Exception> OnError;
#endregion

#region Fields
                        private GhostSession _currentSession;
                        private bool _isStreaming;
                        private Thread _streamThread;
                        private readonly object _lock = new object();
                        private int _frameIntervalMs = 1000; // كل ثانية
                        private int _jpegQuality = 40;       // جودة منخفضة للسرعة
                        private Size _captureSize = new Size(1024, 768);
#endregion

#region Properties
                        public bool IsActive => _currentSession != null && _currentSession.Status == GhostSessionStatus.Active;
                        public GhostSession CurrentSession => _currentSession;
                        public int FrameInterval { get => _frameIntervalMs; set => _frameIntervalMs = Math.Max(200, value); }
                        public int JpegQuality { get => _jpegQuality; set => _jpegQuality = Math.Max(10, Math.Min(100, value)); }
                        public class GhostMessage
                        {
                            public string Type { get; set; }
                            public string SessionID { get; set; }
                            public string OperatorName { get; set; }
                            public int FrameSize { get; set; }
                            public int FrameInterval { get => _frameIntervalMs; set => _frameIntervalMs = Math.Max(200, value); }
                            public bool IsActive { get; private set; }
                            public string Type { get; set; }
                            public string AtmId { get; set; }
                            public bool ViewOnly { get; set; }
                            public DateTime StartedAtUtc { get; set; }
                            public DateTime? EndedAtUtc { get; set; }
                            [DllImport("user32.dll")] private static extern IntPtr GetDesktopWindow();
                            [DllImport("user32.dll")] private static extern IntPtr GetWindowDC(IntPtr hWnd);
                            [DllImport("gdi32.dll")]  private static extern bool BitBlt(IntPtr hdcDest, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hdcSrc, int nXSrc, int nYSrc, uint dwRop);
                            [DllImport("user32.dll")] private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);
                            [DllImport("gdi32.dll")]  private static extern IntPtr CreateCompatibleDC(IntPtr hdc);
                            [DllImport("gdi32.dll")]  private static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int nWidth, int nHeight);
                            [DllImport("gdi32.dll")]  private static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);
                            [DllImport("gdi32.dll")]  private static extern bool DeleteDC(IntPtr hdc);
                            [DllImport("gdi32.dll")]  private static extern bool DeleteObject(IntPtr hObject);

                            private const uint SRCCOPY = 0x00CC0020;

                            public GhostRemoteEngine(int qualityPercent = 75, bool lowBandwidth = false)
                            {
                                public void SetQuality(int percent, int intervalMs = -1)
                                {
                                    private static int Clamp(int value, int min, int max)
                                    {
                                        public void Start()
                                        {
                                            public void Stop()
                                            {
                                                private void CaptureLoop()
                                                {
                                                    private byte[] CaptureScreenJpeg()
                                                    {
                                                        private ImageCodecInfo GetJpegEncoder()
                                                        {
                                                            public void Dispose() =>
                                                            public void StartClientStreaming(string sessionId)
                                                            {
                                                                public void StopClientStreaming()
                                                                {
                                                                    public byte[] CaptureFrame()
                                                                    {
                                                                        public GhostSession StartServerSession(string atmId, string operatorName)
                                                                        {
                                                                            public void EndServerSession()
                                                                            {
                                                                                public void ProcessReceivedFrame(byte[] frameData)
                                                                                {
                                                                                    public static byte[] BuildGhostStartRequest(string sessionId, string operatorName)
                                                                                    {
                                                                                        public static byte[] BuildGhostStopRequest(string sessionId)
                                                                                        {
                                                                                            public static byte[] BuildFrameMessage(string sessionId, byte[] frameData)
                                                                                            {
                                                                                                public static GhostMessage ParseGhostMessage(byte[] data)
                                                                                                {
                                                                                                    private ImageCodecInfo GetEncoder(ImageFormat format)
                                                                                                    {
                                                                                                        [DllImport("user32.dll")] private static extern IntPtr GetDesktopWindow();

                                                                                                        // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Enterprise_Ultimate\EJLive.Core\Engine\GhostRemoteEngine.cs
                                                                                                        [DllImport("user32.dll")] private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

                                                                                                        [DllImport("gdi32.dll")]  private static extern bool BitBlt(IntPtr hdcDest, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hdcSrc, int nXSrc, int nYSrc, uint dwRop);

                                                                                                        // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Enterprise_Ultimate\EJLive.Core\Engine\GhostRemoteEngine.cs
                                                                                                        [DllImport("gdi32.dll")]  private static extern bool DeleteObject(IntPtr hObject);

                                                                                                        public void SetQuality(int percent, int intervalMs = -1)
                                                                                                        {
                                                                                                            private void CaptureLoopClient()
                                                                                                            {
                                                                                                                [DllImport("user32.dll")] private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

                                                                                                                // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_restructured\CodexMarege\EJLive.Core\Engine\GhostRemoteEngine.cs
                                                                                                                [DllImport("gdi32.dll")]  private static extern bool DeleteObject(IntPtr hObject);

                                                                                                                // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_1778703792493\CodexMarege\EJLive.Core\Engine\GhostRemoteEngine.cs
                                                                                                                [DllImport("user32.dll")] private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

                                                                                                                // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_1778703792493\CodexMarege\EJLive.Core\Engine\GhostRemoteEngine.cs
                                                                                                                [DllImport("gdi32.dll")]  private static extern bool DeleteObject(IntPtr hObject);

                                                                                                                // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_1778791921744\CodexMarege\EJLive.Core\Engine\GhostRemoteEngine.cs
                                                                                                                [DllImport("user32.dll")] private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

                                                                                                                // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_1778791921744\CodexMarege\EJLive.Core\Engine\GhostRemoteEngine.cs
                                                                                                                [DllImport("gdi32.dll")]  private static extern bool DeleteObject(IntPtr hObject);

                                                                                                                // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_extracted\CodexMarege\EJLive.Core\Engine\GhostRemoteEngine.cs
                                                                                                                [DllImport("user32.dll")] private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

                                                                                                                // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_extracted\CodexMarege\EJLive.Core\Engine\GhostRemoteEngine.cs
                                                                                                                [DllImport("gdi32.dll")]  private static extern bool DeleteObject(IntPtr hObject);


                                                                                                                // --- Events ---
                                                                                                                public event EventHandler<byte[]>  OnFrameCaptured;   // JPEG bytes

                                                                                                                public event EventHandler<string>  OnError;

                                                                                                                public event EventHandler<string>  OnLog;


                                                                                                                // --- Nested Classes ---
                                                                                                                public class GhostRemoteEngine
                                                                                                                {
#region Events
                                                                                                                    public event Action<string> OnLog;
                                                                                                                    public event Action<byte[]> OnFrameReceived;      // Server side: frame from client
                                                                                                                    public event Action<byte[]> OnFrameRequested;     // Client side: send frame to server
                                                                                                                    public event Action<GhostSession> OnSessionStarted;
                                                                                                                    public event Action<GhostSession> OnSessionEnded;
                                                                                                                    public event Action<Exception> OnError;
#endregion

#region Fields
                                                                                                                    private GhostSession _currentSession;
                                                                                                                    private bool _isStreaming;
                                                                                                                    private Thread _streamThread;
                                                                                                                    private readonly object _lock = new object();
                                                                                                                    private int _frameIntervalMs = 1000; // كل ثانية
                                                                                                                    private int _jpegQuality = 40;       // جودة منخفضة للسرعة
                                                                                                                    private Size _captureSize = new Size(1024, 768);
#endregion

#region Properties
                                                                                                                    public bool IsActive => _currentSession != null && _currentSession.Status == GhostSessionStatus.Active;
                                                                                                                    public GhostSession CurrentSession => _currentSession;
                                                                                                                    public int FrameInterval { get => _frameIntervalMs; set => _frameIntervalMs = Math.Max(200, value); }
                                                                                                                    public int JpegQuality { get => _jpegQuality; set => _jpegQuality = Math.Max(10, Math.Min(100, value)); }
#endregion

#region Client Side - Capture and Send
                                                                                                                    /// <summary>
                                                                                                                    /// بدء التقاط الشاشة وإرسالها (يعمل على الكلاينت)
                                                                                                                    /// لا يؤثر على الصراف - فقط التقاط بدون تدخل
                                                                                                                    /// </summary>
                                                                                                                    public void StartClientStreaming(string sessionId)
                                                                                                                    {
                                                                                                                        public RemoteAssistanceSession? StartSession(string atmId, string operatorName, bool viewOnly)
                                                                                                                        {
                                                                                                                            public void EndSession(string sessionId)
                                                                                                                            {
                                                                                                                                public IReadOnlyList<RemoteAssistanceSession> GetActiveSessions()
                                                                                                                                {
                                                                                                                                    private void Log(string msg) =>
                                                                                                                                    private void StopAllSessions()
                                                                                                                                    {
                                                                                                                                        public event EventHandler<byte[]>  OnFrameCaptured;
                                                                                                                                        public event EventHandler<string>  OnError;
                                                                                                                                        public event EventHandler<string>  OnLog;
                                                                                                                                        public event Action<string> OnLog;
                                                                                                                                        public event Action<byte[]> OnFrameReceived;
                                                                                                                                        public event Action<byte[]> OnFrameRequested;
                                                                                                                                        public event Action<GhostSession> OnSessionStarted;
                                                                                                                                        public event Action<GhostSession> OnSessionEnded;
                                                                                                                                        public event Action<Exception> OnError;
                                                                                                                                        public event Action<string> OnLogAction;
                                                                                                                                        public event Action<Exception> OnException;
                                                                                                                                    }

                                                                                                                                public partial public class GhostMessage
                                                                                                                                {
                                                                                                                                    private GhostSession _currentSession;
                                                                                                                                    private bool _isStreaming;
                                                                                                                                    private Thread _streamThread;
                                                                                                                                    private readonly object _lock = new object();
                                                                                                                                    private int _frameIntervalMs = 1000;
                                                                                                                                    private int _jpegQuality = 40;
                                                                                                                                    private Size _captureSize = new Size(1024, 768);
                                                                                                                                    public bool IsActive => _currentSession != null && _currentSession.Status == GhostSessionStatus.Active;
                                                                                                                                    public GhostSession CurrentSession => _currentSession;
                                                                                                                                    private readonly List<RemoteAssistanceSession> _sessions = new();
                                                                                                                                    private const uint SRCCOPY = 0x00CC0020;
                                                                                                                                    public string Type { get; set; }
                                                                                                                                    public string SessionID { get; set; }
                                                                                                                                    public string OperatorName { get; set; }
                                                                                                                                    public int FrameSize { get; set; }
                                                                                                                                    public int FrameInterval { get => _frameIntervalMs; set => _frameIntervalMs = Math.Max(200, value); }
                                                                                                                                    public bool IsActive { get; private set; }
                                                                                                                                    public class GhostRemoteEngine
                                                                                                                                    {
#region Events
                                                                                                                                        public event Action<string> OnLog;
                                                                                                                                        public event Action<byte[]> OnFrameReceived;      // Server side: frame from client
                                                                                                                                        public event Action<byte[]> OnFrameRequested;     // Client side: send frame to server
                                                                                                                                        public event Action<GhostSession> OnSessionStarted;
                                                                                                                                        public event Action<GhostSession> OnSessionEnded;
                                                                                                                                        public event Action<Exception> OnError;
#endregion

#region Fields
                                                                                                                                        private GhostSession _currentSession;
                                                                                                                                        private bool _isStreaming;
                                                                                                                                        private Thread _streamThread;
                                                                                                                                        private readonly object _lock = new object();
                                                                                                                                        private int _frameIntervalMs = 1000; // كل ثانية
                                                                                                                                        private int _jpegQuality = 40;       // جودة منخفضة للسرعة
                                                                                                                                        private Size _captureSize = new Size(1024, 768);
#endregion

#region Properties
                                                                                                                                        public bool IsActive => _currentSession != null && _currentSession.Status == GhostSessionStatus.Active;
                                                                                                                                        public GhostSession CurrentSession => _currentSession;
                                                                                                                                        public int FrameInterval { get => _frameIntervalMs; set => _frameIntervalMs = Math.Max(200, value); }
                                                                                                                                        public int JpegQuality { get => _jpegQuality; set => _jpegQuality = Math.Max(10, Math.Min(100, value)); }
                                                                                                                                        public string AtmId { get; set; }
                                                                                                                                        public bool ViewOnly { get; set; }
                                                                                                                                        public DateTime StartedAtUtc { get; set; }
                                                                                                                                        public DateTime? EndedAtUtc { get; set; }
                                                                                                                                        [DllImport("user32.dll")] private static extern IntPtr GetDesktopWindow();
                                                                                                                                        [DllImport("user32.dll")] private static extern IntPtr GetWindowDC(IntPtr hWnd);
                                                                                                                                        [DllImport("gdi32.dll")]  private static extern bool BitBlt(IntPtr hdcDest, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hdcSrc, int nXSrc, int nYSrc, uint dwRop);
                                                                                                                                        [DllImport("user32.dll")] private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);
                                                                                                                                        [DllImport("gdi32.dll")]  private static extern IntPtr CreateCompatibleDC(IntPtr hdc);
                                                                                                                                        [DllImport("gdi32.dll")]  private static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int nWidth, int nHeight);
                                                                                                                                        [DllImport("gdi32.dll")]  private static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);
                                                                                                                                        [DllImport("gdi32.dll")]  private static extern bool DeleteDC(IntPtr hdc);
                                                                                                                                        [DllImport("gdi32.dll")]  private static extern bool DeleteObject(IntPtr hObject);

                                                                                                                                        private const uint SRCCOPY = 0x00CC0020;

                                                                                                                                        public GhostRemoteEngine(int qualityPercent = 75, bool lowBandwidth = false)
                                                                                                                                        {
                                                                                                                                            public void SetQuality(int percent, int intervalMs = -1)
                                                                                                                                            {
                                                                                                                                                private static int Clamp(int value, int min, int max)
                                                                                                                                                {
                                                                                                                                                    public void Start()
                                                                                                                                                    {
                                                                                                                                                        public void Stop()
                                                                                                                                                        {
                                                                                                                                                            private void CaptureLoop()
                                                                                                                                                            {
                                                                                                                                                                private byte[] CaptureScreenJpeg()
                                                                                                                                                                {
                                                                                                                                                                    private ImageCodecInfo GetJpegEncoder()
                                                                                                                                                                    {
                                                                                                                                                                        public void Dispose() =>
                                                                                                                                                                        public void StartClientStreaming(string sessionId)
                                                                                                                                                                        {
                                                                                                                                                                            public void StopClientStreaming()
                                                                                                                                                                            {
                                                                                                                                                                                public byte[] CaptureFrame()
                                                                                                                                                                                {
                                                                                                                                                                                    public GhostSession StartServerSession(string atmId, string operatorName)
                                                                                                                                                                                    {
                                                                                                                                                                                        public void EndServerSession()
                                                                                                                                                                                        {
                                                                                                                                                                                            public void ProcessReceivedFrame(byte[] frameData)
                                                                                                                                                                                            {
                                                                                                                                                                                                public static byte[] BuildGhostStartRequest(string sessionId, string operatorName)
                                                                                                                                                                                                {
                                                                                                                                                                                                    public static byte[] BuildGhostStopRequest(string sessionId)
                                                                                                                                                                                                    {
                                                                                                                                                                                                        public static byte[] BuildFrameMessage(string sessionId, byte[] frameData)
                                                                                                                                                                                                        {
                                                                                                                                                                                                            public static GhostMessage ParseGhostMessage(byte[] data)
                                                                                                                                                                                                            {
                                                                                                                                                                                                                private ImageCodecInfo GetEncoder(ImageFormat format)
                                                                                                                                                                                                                {
                                                                                                                                                                                                                    [DllImport("user32.dll")] private static extern IntPtr GetDesktopWindow();

                                                                                                                                                                                                                    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Enterprise_Ultimate\EJLive.Core\Engine\GhostRemoteEngine.cs
                                                                                                                                                                                                                    [DllImport("user32.dll")] private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

                                                                                                                                                                                                                    [DllImport("gdi32.dll")]  private static extern bool BitBlt(IntPtr hdcDest, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hdcSrc, int nXSrc, int nYSrc, uint dwRop);

                                                                                                                                                                                                                    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Enterprise_Ultimate\EJLive.Core\Engine\GhostRemoteEngine.cs
                                                                                                                                                                                                                    [DllImport("gdi32.dll")]  private static extern bool DeleteObject(IntPtr hObject);

                                                                                                                                                                                                                    public void SetQuality(int percent, int intervalMs = -1)
                                                                                                                                                                                                                    {
                                                                                                                                                                                                                        private void CaptureLoopClient()
                                                                                                                                                                                                                        {
                                                                                                                                                                                                                            [DllImport("user32.dll")] private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

                                                                                                                                                                                                                            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_restructured\CodexMarege\EJLive.Core\Engine\GhostRemoteEngine.cs
                                                                                                                                                                                                                            [DllImport("gdi32.dll")]  private static extern bool DeleteObject(IntPtr hObject);

                                                                                                                                                                                                                            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_1778703792493\CodexMarege\EJLive.Core\Engine\GhostRemoteEngine.cs
                                                                                                                                                                                                                            [DllImport("user32.dll")] private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

                                                                                                                                                                                                                            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_1778703792493\CodexMarege\EJLive.Core\Engine\GhostRemoteEngine.cs
                                                                                                                                                                                                                            [DllImport("gdi32.dll")]  private static extern bool DeleteObject(IntPtr hObject);

                                                                                                                                                                                                                            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_1778791921744\CodexMarege\EJLive.Core\Engine\GhostRemoteEngine.cs
                                                                                                                                                                                                                            [DllImport("user32.dll")] private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

                                                                                                                                                                                                                            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_1778791921744\CodexMarege\EJLive.Core\Engine\GhostRemoteEngine.cs
                                                                                                                                                                                                                            [DllImport("gdi32.dll")]  private static extern bool DeleteObject(IntPtr hObject);

                                                                                                                                                                                                                            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_extracted\CodexMarege\EJLive.Core\Engine\GhostRemoteEngine.cs
                                                                                                                                                                                                                            [DllImport("user32.dll")] private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

                                                                                                                                                                                                                            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_extracted\CodexMarege\EJLive.Core\Engine\GhostRemoteEngine.cs
                                                                                                                                                                                                                            [DllImport("gdi32.dll")]  private static extern bool DeleteObject(IntPtr hObject);


                                                                                                                                                                                                                            // --- Events ---
                                                                                                                                                                                                                            public event EventHandler<byte[]>  OnFrameCaptured;   // JPEG bytes

                                                                                                                                                                                                                            public event EventHandler<string>  OnError;

                                                                                                                                                                                                                            public event EventHandler<string>  OnLog;


                                                                                                                                                                                                                            // --- Nested Classes ---
                                                                                                                                                                                                                            public class GhostRemoteEngine
                                                                                                                                                                                                                            {
#region Events
                                                                                                                                                                                                                                public event Action<string> OnLog;
                                                                                                                                                                                                                                public event Action<byte[]> OnFrameReceived;      // Server side: frame from client
                                                                                                                                                                                                                                public event Action<byte[]> OnFrameRequested;     // Client side: send frame to server
                                                                                                                                                                                                                                public event Action<GhostSession> OnSessionStarted;
                                                                                                                                                                                                                                public event Action<GhostSession> OnSessionEnded;
                                                                                                                                                                                                                                public event Action<Exception> OnError;
#endregion

#region Fields
                                                                                                                                                                                                                                private GhostSession _currentSession;
                                                                                                                                                                                                                                private bool _isStreaming;
                                                                                                                                                                                                                                private Thread _streamThread;
                                                                                                                                                                                                                                private readonly object _lock = new object();
                                                                                                                                                                                                                                private int _frameIntervalMs = 1000; // كل ثانية
                                                                                                                                                                                                                                private int _jpegQuality = 40;       // جودة منخفضة للسرعة
                                                                                                                                                                                                                                private Size _captureSize = new Size(1024, 768);
#endregion

#region Properties
                                                                                                                                                                                                                                public bool IsActive => _currentSession != null && _currentSession.Status == GhostSessionStatus.Active;
                                                                                                                                                                                                                                public GhostSession CurrentSession => _currentSession;
                                                                                                                                                                                                                                public int FrameInterval { get => _frameIntervalMs; set => _frameIntervalMs = Math.Max(200, value); }
                                                                                                                                                                                                                                public int JpegQuality { get => _jpegQuality; set => _jpegQuality = Math.Max(10, Math.Min(100, value)); }
#endregion

#region Client Side - Capture and Send
                                                                                                                                                                                                                                /// <summary>
                                                                                                                                                                                                                                /// بدء التقاط الشاشة وإرسالها (يعمل على الكلاينت)
                                                                                                                                                                                                                                /// لا يؤثر على الصراف - فقط التقاط بدون تدخل
                                                                                                                                                                                                                                /// </summary>
                                                                                                                                                                                                                                public void StartClientStreaming(string sessionId)
                                                                                                                                                                                                                                {
                                                                                                                                                                                                                                    public RemoteAssistanceSession? StartSession(string atmId, string operatorName, bool viewOnly)
                                                                                                                                                                                                                                    {
                                                                                                                                                                                                                                        public void EndSession(string sessionId)
                                                                                                                                                                                                                                        {
                                                                                                                                                                                                                                            public IReadOnlyList<RemoteAssistanceSession> GetActiveSessions()
                                                                                                                                                                                                                                            {
                                                                                                                                                                                                                                                private void Log(string msg) =>
                                                                                                                                                                                                                                                private void StopAllSessions()
                                                                                                                                                                                                                                                {
                                                                                                                                                                                                                                                    public event EventHandler<byte[]>  OnFrameCaptured;
                                                                                                                                                                                                                                                    public event EventHandler<string>  OnError;
                                                                                                                                                                                                                                                    public event EventHandler<string>  OnLog;
                                                                                                                                                                                                                                                    public event Action<string> OnLog;
                                                                                                                                                                                                                                                    public event Action<byte[]> OnFrameReceived;
                                                                                                                                                                                                                                                    public event Action<byte[]> OnFrameRequested;
                                                                                                                                                                                                                                                    public event Action<GhostSession> OnSessionStarted;
                                                                                                                                                                                                                                                    public event Action<GhostSession> OnSessionEnded;
                                                                                                                                                                                                                                                    public event Action<Exception> OnError;
                                                                                                                                                                                                                                                    public event Action<string> OnLogAction;
                                                                                                                                                                                                                                                    public event Action<Exception> OnException;
                                                                                                                                                                                                                                                }

                                                                                                                                                                                                                                            public partial public public sealed class RemoteAssistanceSession
                                                                                                                                                                                                                                            {
                                                                                                                                                                                                                                                public string SessionID { get; set; }
                                                                                                                                                                                                                                                public string AtmId { get; set; }
                                                                                                                                                                                                                                                public string OperatorName { get; set; }
                                                                                                                                                                                                                                                public bool ViewOnly { get; set; }
                                                                                                                                                                                                                                                public DateTime StartedAtUtc { get; set; }
                                                                                                                                                                                                                                                public DateTime? EndedAtUtc { get; set; }
                                                                                                                                                                                                                                            }

                                                                                                                                                                                                                                        public partial public public sealed class RemoteAssistanceEngine : IDisposable
                                                                                                                                                                                                                                        {
                                                                                                                                                                                                                                            private readonly List<RemoteAssistanceSession> _sessions = new();
                                                                                                                                                                                                                                            private readonly object _lock = new();
                                                                                                                                                                                                                                            public bool IsActive => _sessions.Count > 0;
                                                                                                                                                                                                                                            public RemoteAssistanceSession? StartSession(string atmId, string operatorName, bool viewOnly)
                                                                                                                                                                                                                                            {
                                                                                                                                                                                                                                                public void EndSession(string sessionId)
                                                                                                                                                                                                                                                {
                                                                                                                                                                                                                                                    public IReadOnlyList<RemoteAssistanceSession> GetActiveSessions()
                                                                                                                                                                                                                                                    {
                                                                                                                                                                                                                                                        private void Log(string msg) =>
                                                                                                                                                                                                                                                        public void Dispose() {
                                                                                                                                                                                                                                                            private void StopAllSessions()
                                                                                                                                                                                                                                                            {
                                                                                                                                                                                                                                                            }

                                                                                                                                                                                                                                                    }
                                                                                                                                                                                                                                                public interface IRemoteSessionPolicy
                                                                                                                                                                                                                                                {

                                                                                                                                                                                                                                                    public interface IRemoteSessionAuditStore
                                                                                                                                                                                                                                                    {

                                                                                                                                                                                                                                                        public interface IRemoteSessionNotifier
                                                                                                                                                                                                                                                        {

                                                                                                                                                                                                                                                            public partial public class GhostRemoteEngine : IDisposable
                                                                                                                                                                                                                                                            {
                                                                                                                                                                                                                                                                private Thread   _captureThread;
                                                                                                                                                                                                                                                                private volatile bool _running;
                                                                                                                                                                                                                                                                private int      _qualityPercent;
                                                                                                                                                                                                                                                                private int      _captureIntervalMs;
                                                                                                                                                                                                                                                                private readonly bool _lowBandwidth;
                                                                                                                                                                                                                                                                public bool   IsActive      => _running;
                                                                                                                                                                                                                                                                private GhostSession _currentSession;
                                                                                                                                                                                                                                                                private bool _isStreaming;
                                                                                                                                                                                                                                                                private Thread _streamThread;
                                                                                                                                                                                                                                                                private readonly object _lock = new object();
                                                                                                                                                                                                                                                                private int _frameIntervalMs = 1000;
                                                                                                                                                                                                                                                                private int _jpegQuality = 40;
                                                                                                                                                                                                                                                                private Size _captureSize = new Size(1024, 768);
                                                                                                                                                                                                                                                                public GhostSession CurrentSession => _currentSession;
                                                                                                                                                                                                                                                                public bool IsSessionActive => _currentSession != null && _currentSession.Status == GhostSessionStatus.Active;
                                                                                                                                                                                                                                                                private const uint SRCCOPY = 0x00CC0020;
                                                                                                                                                                                                                                                                public GhostRemoteEngine(int qualityPercent = 75, bool lowBandwidth = false)
                                                                                                                                                                                                                                                                {
                                                                                                                                                                                                                                                                    public int    FramesSent    { get; private set; }
                                                                                                                                                                                                                                                                    public long   BytesSent     { get; private set; }
                                                                                                                                                                                                                                                                    public double FPS           { get; private set; }
                                                                                                                                                                                                                                                                    public class GhostRemoteEngine
                                                                                                                                                                                                                                                                    {
#region Events
                                                                                                                                                                                                                                                                        public event Action<string> OnLog;
                                                                                                                                                                                                                                                                        public event Action<byte[]> OnFrameReceived;      // Server side: frame from client
                                                                                                                                                                                                                                                                        public event Action<byte[]> OnFrameRequested;     // Client side: send frame to server
                                                                                                                                                                                                                                                                        public event Action<GhostSession> OnSessionStarted;
                                                                                                                                                                                                                                                                        public event Action<GhostSession> OnSessionEnded;
                                                                                                                                                                                                                                                                        public event Action<Exception> OnError;
#endregion

#region Fields
                                                                                                                                                                                                                                                                        private GhostSession _currentSession;
                                                                                                                                                                                                                                                                        private bool _isStreaming;
                                                                                                                                                                                                                                                                        private Thread _streamThread;
                                                                                                                                                                                                                                                                        private readonly object _lock = new object();
                                                                                                                                                                                                                                                                        private int _frameIntervalMs = 1000; // كل ثانية
                                                                                                                                                                                                                                                                        private int _jpegQuality = 40;       // جودة منخفضة للسرعة
                                                                                                                                                                                                                                                                        private Size _captureSize = new Size(1024, 768);
#endregion

#region Properties
                                                                                                                                                                                                                                                                        public bool IsActive => _currentSession != null && _currentSession.Status == GhostSessionStatus.Active;
                                                                                                                                                                                                                                                                        public GhostSession CurrentSession => _currentSession;
                                                                                                                                                                                                                                                                        public int FrameInterval { get => _frameIntervalMs; set => _frameIntervalMs = Math.Max(200, value); }
                                                                                                                                                                                                                                                                        public int JpegQuality { get => _jpegQuality; set => _jpegQuality = Math.Max(10, Math.Min(100, value)); }
                                                                                                                                                                                                                                                                        public class GhostMessage
                                                                                                                                                                                                                                                                        {
                                                                                                                                                                                                                                                                            public string Type { get; set; }
                                                                                                                                                                                                                                                                            public string SessionID { get; set; }
                                                                                                                                                                                                                                                                            public string OperatorName { get; set; }
                                                                                                                                                                                                                                                                            public int FrameSize { get; set; }
                                                                                                                                                                                                                                                                            public int FrameInterval { get => _frameIntervalMs; set => _frameIntervalMs = Math.Max(200, value); }
                                                                                                                                                                                                                                                                            public bool IsActive { get; private set; }
                                                                                                                                                                                                                                                                            [DllImport("user32.dll")] private static extern IntPtr GetDesktopWindow();
                                                                                                                                                                                                                                                                            [DllImport("user32.dll")] private static extern IntPtr GetWindowDC(IntPtr hWnd);
                                                                                                                                                                                                                                                                            [DllImport("gdi32.dll")]  private static extern bool BitBlt(IntPtr hdcDest, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hdcSrc, int nXSrc, int nYSrc, uint dwRop);
                                                                                                                                                                                                                                                                            [DllImport("user32.dll")] private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);
                                                                                                                                                                                                                                                                            [DllImport("gdi32.dll")]  private static extern IntPtr CreateCompatibleDC(IntPtr hdc);
                                                                                                                                                                                                                                                                            [DllImport("gdi32.dll")]  private static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int nWidth, int nHeight);
                                                                                                                                                                                                                                                                            [DllImport("gdi32.dll")]  private static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);
                                                                                                                                                                                                                                                                            [DllImport("gdi32.dll")]  private static extern bool DeleteDC(IntPtr hdc);
                                                                                                                                                                                                                                                                            [DllImport("gdi32.dll")]  private static extern bool DeleteObject(IntPtr hObject);

                                                                                                                                                                                                                                                                            private const uint SRCCOPY = 0x00CC0020;

                                                                                                                                                                                                                                                                            public GhostRemoteEngine(int qualityPercent = 75, bool lowBandwidth = false)
                                                                                                                                                                                                                                                                            {
                                                                                                                                                                                                                                                                                public void SetQuality(int percent, int intervalMs = -1)
                                                                                                                                                                                                                                                                                {
                                                                                                                                                                                                                                                                                    private static int Clamp(int value, int min, int max)
                                                                                                                                                                                                                                                                                    {
                                                                                                                                                                                                                                                                                        public void Start()
                                                                                                                                                                                                                                                                                        {
                                                                                                                                                                                                                                                                                            public void Stop()
                                                                                                                                                                                                                                                                                            {
                                                                                                                                                                                                                                                                                                private void CaptureLoop()
                                                                                                                                                                                                                                                                                                {
                                                                                                                                                                                                                                                                                                    private byte[] CaptureScreenJpeg()
                                                                                                                                                                                                                                                                                                    {
                                                                                                                                                                                                                                                                                                        private ImageCodecInfo GetJpegEncoder()
                                                                                                                                                                                                                                                                                                        {
                                                                                                                                                                                                                                                                                                            public void Dispose() =>
                                                                                                                                                                                                                                                                                                            public void StartClientStreaming(string sessionId)
                                                                                                                                                                                                                                                                                                            {
                                                                                                                                                                                                                                                                                                                public void StopClientStreaming()
                                                                                                                                                                                                                                                                                                                {
                                                                                                                                                                                                                                                                                                                    public byte[] CaptureFrame()
                                                                                                                                                                                                                                                                                                                    {
                                                                                                                                                                                                                                                                                                                        public GhostSession StartServerSession(string atmId, string operatorName)
                                                                                                                                                                                                                                                                                                                        {
                                                                                                                                                                                                                                                                                                                            public void EndServerSession()
                                                                                                                                                                                                                                                                                                                            {
                                                                                                                                                                                                                                                                                                                                public void ProcessReceivedFrame(byte[] frameData)
                                                                                                                                                                                                                                                                                                                                {
                                                                                                                                                                                                                                                                                                                                    public static byte[] BuildGhostStartRequest(string sessionId, string operatorName)
                                                                                                                                                                                                                                                                                                                                    {
                                                                                                                                                                                                                                                                                                                                        public static byte[] BuildGhostStopRequest(string sessionId)
                                                                                                                                                                                                                                                                                                                                        {
                                                                                                                                                                                                                                                                                                                                            public static byte[] BuildFrameMessage(string sessionId, byte[] frameData)
                                                                                                                                                                                                                                                                                                                                            {
                                                                                                                                                                                                                                                                                                                                                public static GhostMessage ParseGhostMessage(byte[] data)
                                                                                                                                                                                                                                                                                                                                                {
                                                                                                                                                                                                                                                                                                                                                    private ImageCodecInfo GetEncoder(ImageFormat format)
                                                                                                                                                                                                                                                                                                                                                    {
                                                                                                                                                                                                                                                                                                                                                        [DllImport("user32.dll")] private static extern IntPtr GetDesktopWindow();

                                                                                                                                                                                                                                                                                                                                                        // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Enterprise_Ultimate\EJLive.Core\Engine\GhostRemoteEngine.cs
                                                                                                                                                                                                                                                                                                                                                        [DllImport("user32.dll")] private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

                                                                                                                                                                                                                                                                                                                                                        [DllImport("gdi32.dll")]  private static extern bool BitBlt(IntPtr hdcDest, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hdcSrc, int nXSrc, int nYSrc, uint dwRop);

                                                                                                                                                                                                                                                                                                                                                        // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Enterprise_Ultimate\EJLive.Core\Engine\GhostRemoteEngine.cs
                                                                                                                                                                                                                                                                                                                                                        [DllImport("gdi32.dll")]  private static extern bool DeleteObject(IntPtr hObject);

                                                                                                                                                                                                                                                                                                                                                        public void SetQuality(int percent, int intervalMs = -1)
                                                                                                                                                                                                                                                                                                                                                        {
                                                                                                                                                                                                                                                                                                                                                            private void CaptureLoopClient()
                                                                                                                                                                                                                                                                                                                                                            {
                                                                                                                                                                                                                                                                                                                                                                [DllImport("user32.dll")] private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

                                                                                                                                                                                                                                                                                                                                                                // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_restructured\CodexMarege\EJLive.Core\Engine\GhostRemoteEngine.cs
                                                                                                                                                                                                                                                                                                                                                                [DllImport("gdi32.dll")]  private static extern bool DeleteObject(IntPtr hObject);

                                                                                                                                                                                                                                                                                                                                                                // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_1778703792493\CodexMarege\EJLive.Core\Engine\GhostRemoteEngine.cs
                                                                                                                                                                                                                                                                                                                                                                [DllImport("user32.dll")] private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

                                                                                                                                                                                                                                                                                                                                                                // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_1778703792493\CodexMarege\EJLive.Core\Engine\GhostRemoteEngine.cs
                                                                                                                                                                                                                                                                                                                                                                [DllImport("gdi32.dll")]  private static extern bool DeleteObject(IntPtr hObject);

                                                                                                                                                                                                                                                                                                                                                                // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_1778791921744\CodexMarege\EJLive.Core\Engine\GhostRemoteEngine.cs
                                                                                                                                                                                                                                                                                                                                                                [DllImport("user32.dll")] private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

                                                                                                                                                                                                                                                                                                                                                                // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_1778791921744\CodexMarege\EJLive.Core\Engine\GhostRemoteEngine.cs
                                                                                                                                                                                                                                                                                                                                                                [DllImport("gdi32.dll")]  private static extern bool DeleteObject(IntPtr hObject);

                                                                                                                                                                                                                                                                                                                                                                // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_extracted\CodexMarege\EJLive.Core\Engine\GhostRemoteEngine.cs
                                                                                                                                                                                                                                                                                                                                                                [DllImport("user32.dll")] private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

                                                                                                                                                                                                                                                                                                                                                                // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_extracted\CodexMarege\EJLive.Core\Engine\GhostRemoteEngine.cs
                                                                                                                                                                                                                                                                                                                                                                [DllImport("gdi32.dll")]  private static extern bool DeleteObject(IntPtr hObject);


                                                                                                                                                                                                                                                                                                                                                                // --- Events ---
                                                                                                                                                                                                                                                                                                                                                                public event EventHandler<byte[]>  OnFrameCaptured;   // JPEG bytes

                                                                                                                                                                                                                                                                                                                                                                public event EventHandler<string>  OnError;

                                                                                                                                                                                                                                                                                                                                                                public event EventHandler<string>  OnLog;


                                                                                                                                                                                                                                                                                                                                                                // --- Nested Classes ---
                                                                                                                                                                                                                                                                                                                                                                public class GhostRemoteEngine
                                                                                                                                                                                                                                                                                                                                                                {
#region Events
                                                                                                                                                                                                                                                                                                                                                                    public event Action<string> OnLog;
                                                                                                                                                                                                                                                                                                                                                                    public event Action<byte[]> OnFrameReceived;      // Server side: frame from client
                                                                                                                                                                                                                                                                                                                                                                    public event Action<byte[]> OnFrameRequested;     // Client side: send frame to server
                                                                                                                                                                                                                                                                                                                                                                    public event Action<GhostSession> OnSessionStarted;
                                                                                                                                                                                                                                                                                                                                                                    public event Action<GhostSession> OnSessionEnded;
                                                                                                                                                                                                                                                                                                                                                                    public event Action<Exception> OnError;
#endregion

#region Fields
                                                                                                                                                                                                                                                                                                                                                                    private GhostSession _currentSession;
                                                                                                                                                                                                                                                                                                                                                                    private bool _isStreaming;
                                                                                                                                                                                                                                                                                                                                                                    private Thread _streamThread;
                                                                                                                                                                                                                                                                                                                                                                    private readonly object _lock = new object();
                                                                                                                                                                                                                                                                                                                                                                    private int _frameIntervalMs = 1000; // كل ثانية
                                                                                                                                                                                                                                                                                                                                                                    private int _jpegQuality = 40;       // جودة منخفضة للسرعة
                                                                                                                                                                                                                                                                                                                                                                    private Size _captureSize = new Size(1024, 768);
#endregion

#region Properties
                                                                                                                                                                                                                                                                                                                                                                    public bool IsActive => _currentSession != null && _currentSession.Status == GhostSessionStatus.Active;
                                                                                                                                                                                                                                                                                                                                                                    public GhostSession CurrentSession => _currentSession;
                                                                                                                                                                                                                                                                                                                                                                    public int FrameInterval { get => _frameIntervalMs; set => _frameIntervalMs = Math.Max(200, value); }
                                                                                                                                                                                                                                                                                                                                                                    public int JpegQuality { get => _jpegQuality; set => _jpegQuality = Math.Max(10, Math.Min(100, value)); }
#endregion

#region Client Side - Capture and Send
                                                                                                                                                                                                                                                                                                                                                                    /// <summary>
                                                                                                                                                                                                                                                                                                                                                                    /// بدء التقاط الشاشة وإرسالها (يعمل على الكلاينت)
                                                                                                                                                                                                                                                                                                                                                                    /// لا يؤثر على الصراف - فقط التقاط بدون تدخل
                                                                                                                                                                                                                                                                                                                                                                    /// </summary>
                                                                                                                                                                                                                                                                                                                                                                    public void StartClientStreaming(string sessionId)
                                                                                                                                                                                                                                                                                                                                                                    {
                                                                                                                                                                                                                                                                                                                                                                        public event EventHandler<byte[]>  OnFrameCaptured;
                                                                                                                                                                                                                                                                                                                                                                        public event EventHandler<string>  OnError;
                                                                                                                                                                                                                                                                                                                                                                        public event EventHandler<string>  OnLog;
                                                                                                                                                                                                                                                                                                                                                                        public event Action<string> OnLog;
                                                                                                                                                                                                                                                                                                                                                                        public event Action<byte[]> OnFrameReceived;
                                                                                                                                                                                                                                                                                                                                                                        public event Action<byte[]> OnFrameRequested;
                                                                                                                                                                                                                                                                                                                                                                        public event Action<GhostSession> OnSessionStarted;
                                                                                                                                                                                                                                                                                                                                                                        public event Action<GhostSession> OnSessionEnded;
                                                                                                                                                                                                                                                                                                                                                                        public event Action<Exception> OnError;
                                                                                                                                                                                                                                                                                                                                                                        public event Action<string> OnLogAction;
                                                                                                                                                                                                                                                                                                                                                                        public event Action<Exception> OnException;
                                                                                                                                                                                                                                                                                                                                                                    }

                                                                                                                                                                                                                                                                                                                                                                public partial public class GhostMessage
                                                                                                                                                                                                                                                                                                                                                                {
                                                                                                                                                                                                                                                                                                                                                                    public string Type { get; set; }
                                                                                                                                                                                                                                                                                                                                                                    public string SessionID { get; set; }
                                                                                                                                                                                                                                                                                                                                                                    public string OperatorName { get; set; }
                                                                                                                                                                                                                                                                                                                                                                    public int FrameSize { get; set; }
                                                                                                                                                                                                                                                                                                                                                                }

                                                                                                                                                                                                                                                                                                                                                            public partial public sealed class RemoteAssistanceSession
                                                                                                                                                                                                                                                                                                                                                            {
                                                                                                                                                                                                                                                                                                                                                                public string SessionID { get; set; }
                                                                                                                                                                                                                                                                                                                                                                public string AtmId { get; set; }
                                                                                                                                                                                                                                                                                                                                                                public string OperatorName { get; set; }
                                                                                                                                                                                                                                                                                                                                                                public bool ViewOnly { get; set; }
                                                                                                                                                                                                                                                                                                                                                                public DateTime StartedAtUtc { get; set; }
                                                                                                                                                                                                                                                                                                                                                                public DateTime? EndedAtUtc { get; set; }
                                                                                                                                                                                                                                                                                                                                                            }

                                                                                                                                                                                                                                                                                                                                                        public partial public sealed class RemoteAssistanceEngine : IDisposable
                                                                                                                                                                                                                                                                                                                                                        {
                                                                                                                                                                                                                                                                                                                                                            private readonly List<RemoteAssistanceSession> _sessions = new();
                                                                                                                                                                                                                                                                                                                                                            private readonly object _lock = new();
                                                                                                                                                                                                                                                                                                                                                            public bool IsActive => _sessions.Count > 0;
                                                                                                                                                                                                                                                                                                                                                            public RemoteAssistanceSession? StartSession(string atmId, string operatorName, bool viewOnly)
                                                                                                                                                                                                                                                                                                                                                            {
                                                                                                                                                                                                                                                                                                                                                                public void EndSession(string sessionId)
                                                                                                                                                                                                                                                                                                                                                                {
                                                                                                                                                                                                                                                                                                                                                                    public IReadOnlyList<RemoteAssistanceSession> GetActiveSessions()
                                                                                                                                                                                                                                                                                                                                                                    {
                                                                                                                                                                                                                                                                                                                                                                        private void Log(string msg) =>
                                                                                                                                                                                                                                                                                                                                                                        public void Dispose() {
                                                                                                                                                                                                                                                                                                                                                                            private void StopAllSessions()
                                                                                                                                                                                                                                                                                                                                                                            {
                                                                                                                                                                                                                                                                                                                                                                            }

                                                                                                                                                                                                                                                                                                                                                                    }

                                                                                                                                                                                                                                                                                                                                                                );

                                                                                                                                                                                                                                                                                                                                                                public partial class EJMessage
                                                                                                                                                                                                                                                                                                                                                                {
                                                                                                                                                                                                                                                                                                                                                                    public CommunicationProtocol.MsgType Type { get; set; }


                                                                                                                                                                                                                                                                                                                                                                    public string Text { get; set; } = string.Empty;


                                                                                                                                                                                                                                                                                                                                                                    public byte[] Payload { get; set; } = Array.Empty<byte>();


                                                                                                                                                                                                                                                                                                                                                                    public DateTime ReceivedAtUtc { get; set; } = DateTime.UtcNow;


                                                                                                                                                                                                                                                                                                                                                                }
                                                                                                                                                                                                                                                                                                                                                            /// <summary>
                                                                                                                                                                                                                                                                                                                                                            /// رسالة Ghost محللة
                                                                                                                                                                                                                                                                                                                                                            /// </summary>
                                                                                                                                                                                                                                                                                                                                                            public class GhostMessage
                                                                                                                                                                                                                                                                                                                                                            {
                                                                                                                                                                                                                                                                                                                                                                public string Type { get; set; }
                                                                                                                                                                                                                                                                                                                                                                public string SessionID { get; set; }
                                                                                                                                                                                                                                                                                                                                                                public string OperatorName { get; set; }
                                                                                                                                                                                                                                                                                                                                                                public int FrameSize { get; set; }
                                                                                                                                                                                                                                                                                                                                                                public byte[] FrameData { get; set; }
                                                                                                                                                                                                                                                                                                                                                            }
                                                                                                                                                                                                                                                                                                                                                        public partial class GhostMessage
                                                                                                                                                                                                                                                                                                                                                        {
                                                                                                                                                                                                                                                                                                                                                            public string Type { get; set; }


                                                                                                                                                                                                                                                                                                                                                            public string SessionID { get; set; }


                                                                                                                                                                                                                                                                                                                                                            public string OperatorName { get; set; }


                                                                                                                                                                                                                                                                                                                                                            public int FrameSize { get; set; }


                                                                                                                                                                                                                                                                                                                                                            public byte[] FrameData { get; set; }


                                                                                                                                                                                                                                                                                                                                                        }
                                                                                                                                                                                                                                                                                                                                                    public partial class GhostRemoteEngine : IDisposable
                                                                                                                                                                                                                                                                                                                                                    {
                                                                                                                                                                                                                                                                                                                                                        public bool IsActive { get; private set; }


                                                                                                                                                                                                                                                                                                                                                        public void Start() => IsActive = true;


                                                                                                                                                                                                                                                                                                                                                        public void Stop() => IsActive = false;


                                                                                                                                                                                                                                                                                                                                                        public void Dispose() => Stop();


                                                                                                                                                                                                                                                                                                                                                    }
                                                                                                                                                                                                                                                                                                                                                public partial class GhostRemoteEngine
                                                                                                                                                                                                                                                                                                                                                {
                                                                                                                                                                                                                                                                                                                                                    private GhostSession _currentSession;


                                                                                                                                                                                                                                                                                                                                                    private bool _isStreaming;


                                                                                                                                                                                                                                                                                                                                                    private Thread _streamThread;


                                                                                                                                                                                                                                                                                                                                                    private int _frameIntervalMs = 1000; // كل ثانية


                                                                                                                                                                                                                                                                                                                                                    private int _jpegQuality = 40;       // جودة منخفضة للسرعة


                                                                                                                                                                                                                                                                                                                                                    public bool IsActive => _currentSession != null && _currentSession.Status == GhostSessionStatus.Active;


                                                                                                                                                                                                                                                                                                                                                    public GhostSession CurrentSession => _currentSession;


                                                                                                                                                                                                                                                                                                                                                    public int FrameInterval { get => _frameIntervalMs; set => _frameIntervalMs = Math.Max(200, value); }


                                                                                                                                                                                                                                                                                                                                                    public int JpegQuality { get => _jpegQuality; set => _jpegQuality = Math.Max(10, Math.Min(100, value)); }


                                                                                                                                                                                                                                                                                                                                                    private readonly object _lock = new object();


                                                                                                                                                                                                                                                                                                                                                    private Size _captureSize = new Size(1024, 768);


                                                                                                                                                                                                                                                                                                                                                    public void StartClientStreaming(string sessionId)
                                                                                                                                                                                                                                                                                                                                                    {
                                                                                                                                                                                                                                                                                                                                                        if (_isStreaming) return;
                                                                                                                                                                                                                                                                                                                                                        _isStreaming = true;

                                                                                                                                                                                                                                                                                                                                                        _currentSession = new GhostSession
                                                                                                                                                                                                                                                                                                                                                        {
                                                                                                                                                                                                                                                                                                                                                            SessionID = sessionId,
                                                                                                                                                                                                                                                                                                                                                            StartTime = DateTime.Now,
                                                                                                                                                                                                                                                                                                                                                            Status = GhostSessionStatus.Active,
                                                                                                                                                                                                                                                                                                                                                            IsViewOnly = true,
                                                                                                                                                                                                                                                                                                                                                            ATMUnaffected = true,
                                                                                                                                                                                                                                                                                                                                                            NoLogout = true,
                                                                                                                                                                                                                                                                                                                                                            ScreenNotLocked = true
                                                                                                                                                                                                                                                                                                                                                        };

                                                                                                                                                                                                                                                                                                                                                    _streamThread = new Thread(CaptureLoop) { IsBackground = true, Name = "GhostCaptureThread" };
                                                                                                                                                                                                                                                                                                                                                _streamThread.Start();

                                                                                                                                                                                                                                                                                                                                                OnLog?.Invoke("[Ghost] Client streaming started - View Only mode (ATM unaffected)");
                                                                                                                                                                                                                                                                                                                                                OnSessionStarted?.Invoke(_currentSession);
                                                                                                                                                                                                                                                                                                                                            }


                                                                                                                                                                                                                                                                                                                                        public void StopClientStreaming()
                                                                                                                                                                                                                                                                                                                                        {
                                                                                                                                                                                                                                                                                                                                            _isStreaming = false;
                                                                                                                                                                                                                                                                                                                                            if (_currentSession != null)
                                                                                                                                                                                                                                                                                                                                            {
                                                                                                                                                                                                                                                                                                                                                _currentSession.Status = GhostSessionStatus.Disconnected;
                                                                                                                                                                                                                                                                                                                                                _currentSession.EndTime = DateTime.Now;
                                                                                                                                                                                                                                                                                                                                                OnSessionEnded?.Invoke(_currentSession);
                                                                                                                                                                                                                                                                                                                                            }
                                                                                                                                                                                                                                                                                                                                        OnLog?.Invoke("[Ghost] Client streaming stopped");
                                                                                                                                                                                                                                                                                                                                    }


                                                                                                                                                                                                                                                                                                                                public byte[] CaptureFrame()
                                                                                                                                                                                                                                                                                                                                {
                                                                                                                                                                                                                                                                                                                                    try
                                                                                                                                                                                                                                                                                                                                    {
                                                                                                                                                                                                                                                                                                                                        var bounds = System.Windows.Forms.Screen.PrimaryScreen.Bounds;
                                                                                                                                                                                                                                                                                                                                        using (var bmp = new Bitmap(bounds.Width, bounds.Height, PixelFormat.Format24bppRgb))
                                                                                                                                                                                                                                                                                                                                        {
                                                                                                                                                                                                                                                                                                                                            using (var g = Graphics.FromImage(bmp))
                                                                                                                                                                                                                                                                                                                                            {
                                                                                                                                                                                                                                                                                                                                                g.CopyFromScreen(bounds.Location, Point.Empty, bounds.Size, CopyPixelOperation.SourceCopy);
                                                                                                                                                                                                                                                                                                                                            }

                                                                                                                                                                                                                                                                                                                                        // تصغير الحجم للإرسال السريع
                                                                                                                                                                                                                                                                                                                                        using (var resized = new Bitmap(bmp, _captureSize))
                                                                                                                                                                                                                                                                                                                                        using (var ms = new MemoryStream())
                                                                                                                                                                                                                                                                                                                                        {
                                                                                                                                                                                                                                                                                                                                            var encoder = GetEncoder(ImageFormat.Jpeg);
                                                                                                                                                                                                                                                                                                                                            var encoderParams = new EncoderParameters(1);
                                                                                                                                                                                                                                                                                                                                            encoderParams.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, (long)_jpegQuality);
                                                                                                                                                                                                                                                                                                                                            resized.Save(ms, encoder, encoderParams);
                                                                                                                                                                                                                                                                                                                                            return ms.ToArray();
                                                                                                                                                                                                                                                                                                                                        }
                                                                                                                                                                                                                                                                                                                                }
                                                                                                                                                                                                                                                                                                                        }
                                                                                                                                                                                                                                                                                                                    catch (Exception ex)
                                                                                                                                                                                                                                                                                                                    {
                                                                                                                                                                                                                                                                                                                        OnError?.Invoke(ex);
                                                                                                                                                                                                                                                                                                                        return null;
                                                                                                                                                                                                                                                                                                                    }
                                                                                                                                                                                                                                                                                                            }


                                                                                                                                                                                                                                                                                                        public GhostSession StartServerSession(string atmId, string operatorName)
                                                                                                                                                                                                                                                                                                        {
                                                                                                                                                                                                                                                                                                            _currentSession = new GhostSession
                                                                                                                                                                                                                                                                                                            {
                                                                                                                                                                                                                                                                                                                SessionID = Guid.NewGuid().ToString("N").Substring(0, 12),
                                                                                                                                                                                                                                                                                                                ATM_ID = atmId,
                                                                                                                                                                                                                                                                                                                OperatorName = operatorName,
                                                                                                                                                                                                                                                                                                                StartTime = DateTime.Now,
                                                                                                                                                                                                                                                                                                                Status = GhostSessionStatus.Connecting,
                                                                                                                                                                                                                                                                                                                IsViewOnly = true,
                                                                                                                                                                                                                                                                                                                ATMUnaffected = true,
                                                                                                                                                                                                                                                                                                                NoLogout = true,
                                                                                                                                                                                                                                                                                                                ScreenNotLocked = true
                                                                                                                                                                                                                                                                                                            };

                                                                                                                                                                                                                                                                                                        OnLog?.Invoke($"[Ghost] Server session started for ATM: {atmId} by {operatorName}");
                                                                                                                                                                                                                                                                                                        OnSessionStarted?.Invoke(_currentSession);
                                                                                                                                                                                                                                                                                                        return _currentSession;
                                                                                                                                                                                                                                                                                                    }


                                                                                                                                                                                                                                                                                                public void EndServerSession()
                                                                                                                                                                                                                                                                                                {
                                                                                                                                                                                                                                                                                                    if (_currentSession != null)
                                                                                                                                                                                                                                                                                                    {
                                                                                                                                                                                                                                                                                                        _currentSession.Status = GhostSessionStatus.Disconnected;
                                                                                                                                                                                                                                                                                                        _currentSession.EndTime = DateTime.Now;
                                                                                                                                                                                                                                                                                                        _currentSession.ActivityLog.Add($"Session ended at {DateTime.Now:HH:mm:ss}");
                                                                                                                                                                                                                                                                                                        OnSessionEnded?.Invoke(_currentSession);
                                                                                                                                                                                                                                                                                                        OnLog?.Invoke($"[Ghost] Session ended: {_currentSession.SessionID}");
                                                                                                                                                                                                                                                                                                    }
                                                                                                                                                                                                                                                                                                _currentSession = null;
                                                                                                                                                                                                                                                                                            }


                                                                                                                                                                                                                                                                                        public void ProcessReceivedFrame(byte[] frameData)
                                                                                                                                                                                                                                                                                        {
                                                                                                                                                                                                                                                                                            if (frameData == null || frameData.Length == 0) return;
                                                                                                                                                                                                                                                                                            if (_currentSession != null)
                                                                                                                                                                                                                                                                                            {
                                                                                                                                                                                                                                                                                                _currentSession.Status = GhostSessionStatus.Active;
                                                                                                                                                                                                                                                                                                _currentSession.ActivityLog.Add($"Frame received: {frameData.Length} bytes at {DateTime.Now:HH:mm:ss}");
                                                                                                                                                                                                                                                                                            }
                                                                                                                                                                                                                                                                                        OnFrameReceived?.Invoke(frameData);
                                                                                                                                                                                                                                                                                    }


                                                                                                                                                                                                                                                                                public static byte[] BuildGhostStartRequest(string sessionId, string operatorName)
                                                                                                                                                                                                                                                                                {
                                                                                                                                                                                                                                                                                    string msg = $"GHOST_START|{sessionId}|{operatorName}|{DateTime.Now:yyyy-MM-dd HH:mm:ss}";
                                                                                                                                                                                                                                                                                    return System.Text.Encoding.UTF8.GetBytes(msg);
                                                                                                                                                                                                                                                                                }


                                                                                                                                                                                                                                                                            public static byte[] BuildGhostStopRequest(string sessionId)
                                                                                                                                                                                                                                                                            {
                                                                                                                                                                                                                                                                                string msg = $"GHOST_STOP|{sessionId}|{DateTime.Now:yyyy-MM-dd HH:mm:ss}";
                                                                                                                                                                                                                                                                                return System.Text.Encoding.UTF8.GetBytes(msg);
                                                                                                                                                                                                                                                                            }


                                                                                                                                                                                                                                                                        public static byte[] BuildFrameMessage(string sessionId, byte[] frameData)
                                                                                                                                                                                                                                                                        {
                                                                                                                                                                                                                                                                            string header = $"GHOST_FRAME|{sessionId}|{frameData.Length}|";
                                                                                                                                                                                                                                                                            byte[] headerBytes = System.Text.Encoding.UTF8.GetBytes(header);
                                                                                                                                                                                                                                                                            byte[] message = new byte[headerBytes.Length + frameData.Length];
                                                                                                                                                                                                                                                                            Buffer.BlockCopy(headerBytes, 0, message, 0, headerBytes.Length);
                                                                                                                                                                                                                                                                            Buffer.BlockCopy(frameData, 0, message, headerBytes.Length, frameData.Length);
                                                                                                                                                                                                                                                                            return message;
                                                                                                                                                                                                                                                                        }


                                                                                                                                                                                                                                                                    public static GhostMessage ParseGhostMessage(byte[] data)
                                                                                                                                                                                                                                                                    {
                                                                                                                                                                                                                                                                        try
                                                                                                                                                                                                                                                                        {
                                                                                                                                                                                                                                                                            string text = System.Text.Encoding.UTF8.GetString(data);
                                                                                                                                                                                                                                                                            string[] parts = text.Split('|');
                                                                                                                                                                                                                                                                            if (parts.Length < 2) return null;

                                                                                                                                                                                                                                                                            var msg = new GhostMessage { Type = parts[0], SessionID = parts[1] };
                                                                                                                                                                                                                                                                            switch (parts[0])
                                                                                                                                                                                                                                                                            {
                                                                                                                                                                                                                                                                                case "GHOST_START":
                                                                                                                                                                                                                                                                                msg.OperatorName = parts.Length > 2 ? parts[2] : "";
                                                                                                                                                                                                                                                                                break;
                                                                                                                                                                                                                                                                                case "GHOST_FRAME":
                                                                                                                                                                                                                                                                                if (parts.Length > 2 && int.TryParse(parts[2], out int size))
                                                                                                                                                                                                                                                                                {
                                                                                                                                                                                                                                                                                    msg.FrameSize = size;
                                                                                                                                                                                                                                                                                    // Frame data follows after header
                                                                                                                                                                                                                                                                                    int headerLen = System.Text.Encoding.UTF8.GetByteCount(string.Join("|", parts[0], parts[1], parts[2]) + "|");
                                                                                                                                                                                                                                                                                    if (data.Length > headerLen)
                                                                                                                                                                                                                                                                                    {
                                                                                                                                                                                                                                                                                        msg.FrameData = new byte[data.Length - headerLen];
                                                                                                                                                                                                                                                                                        Buffer.BlockCopy(data, headerLen, msg.FrameData, 0, msg.FrameData.Length);
                                                                                                                                                                                                                                                                                    }
                                                                                                                                                                                                                                                                            }
                                                                                                                                                                                                                                                                        break;
                                                                                                                                                                                                                                                                    }
                                                                                                                                                                                                                                                                return msg;
                                                                                                                                                                                                                                                            }
                                                                                                                                                                                                                                                        catch { return null; }
                                                                                                                                                                                                                                                    }


                                                                                                                                                                                                                                                private void CaptureLoop()
                                                                                                                                                                                                                                                {
                                                                                                                                                                                                                                                    while (_isStreaming)
                                                                                                                                                                                                                                                    {
                                                                                                                                                                                                                                                        try
                                                                                                                                                                                                                                                        {
                                                                                                                                                                                                                                                            byte[] frame = CaptureFrame();
                                                                                                                                                                                                                                                            if (frame != null && frame.Length > 0)
                                                                                                                                                                                                                                                            {
                                                                                                                                                                                                                                                                OnFrameRequested?.Invoke(frame);
                                                                                                                                                                                                                                                            }
                                                                                                                                                                                                                                                    }
                                                                                                                                                                                                                                                catch (Exception ex)
                                                                                                                                                                                                                                                {
                                                                                                                                                                                                                                                    OnError?.Invoke(ex);
                                                                                                                                                                                                                                                }
                                                                                                                                                                                                                                            Thread.Sleep(_frameIntervalMs);
                                                                                                                                                                                                                                        }
                                                                                                                                                                                                                                }


                                                                                                                                                                                                                            private ImageCodecInfo GetEncoder(ImageFormat format)
                                                                                                                                                                                                                            {
                                                                                                                                                                                                                                ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
                                                                                                                                                                                                                                foreach (ImageCodecInfo codec in codecs)
                                                                                                                                                                                                                                {
                                                                                                                                                                                                                                    if (codec.FormatID == format.Guid) return codec;
                                                                                                                                                                                                                                }
                                                                                                                                                                                                                            return null;
                                                                                                                                                                                                                        }


                                                                                                                                                                                                                    public event Action<string> OnLog;


                                                                                                                                                                                                                    public event Action<byte[]> OnFrameReceived;      // Server side: frame from client


                                                                                                                                                                                                                    public event Action<byte[]> OnFrameRequested;     // Client side: send frame to server


                                                                                                                                                                                                                    public event Action<GhostSession> OnSessionStarted;


                                                                                                                                                                                                                    public event Action<GhostSession> OnSessionEnded;


                                                                                                                                                                                                                    public event Action<Exception> OnError;


                                                                                                                                                                                                                }
                                                                                                                                                                                                            /// <summary>Original: ReferenceOnly/GhostRemoteEngine.cs.original</summary>
                                                                                                                                                                                                            public class GhostRemoteEngine : IDisposable
                                                                                                                                                                                                            {
                                                                                                                                                                                                                public bool IsActive { get; private set; }
                                                                                                                                                                                                                public void Start() => IsActive = true;
                                                                                                                                                                                                                public void Stop() => IsActive = false;
                                                                                                                                                                                                                public void Dispose() => Stop();
                                                                                                                                                                                                            }
                                                                                                                                                                                                        public partial enum MsgType
                                                                                                                                                                                                        {
                                                                                                                                                                                                            payload = string.Empty;


                                                                                                                                                                                                            if (string.IsNullOrWhiteSpace(value))
                                                                                                                                                                                                            return false;


                                                                                                                                                                                                            if (separator < 0)
                                                                                                                                                                                                            atmId = value;


                                                                                                                                                                                                            if (separator >= 0)
                                                                                                                                                                                                            var tail = value[(separator + 1)..].Trim();


                                                                                                                                                                                                            if (tail.StartsWith("{", StringComparison.Ordinal))
                                                                                                                                                                                                            jsonCandidate = tail;


                                                                                                                                                                                                            if (!jsonCandidate.StartsWith("{", StringComparison.Ordinal))
                                                                                                                                                                                                            try
                                                                                                                                                                                                            if (document.RootElement.ValueKind != JsonValueKind.Object)
                                                                                                                                                                                                            var root = document.RootElement;


                                                                                                                                                                                                            var serverTime = DateTime.UtcNow;


                                                                                                                                                                                                            var pendingCount = 0;


                                                                                                                                                                                                            var requestImmediateSync = false;


                                                                                                                                                                                                            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\src\EJLive.Core\Engine\GhostRemoteEngine.cs
                                                                                                                                                                                                            if (DateTime.TryParse(serverTimeText, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var parsedServerTime))
                                                                                                                                                                                                            serverTime = parsedServerTime.ToUniversalTime();


                                                                                                                                                                                                            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\src\EJLive.Core\Engine\GhostRemoteEngine.cs
                                                                                                                                                                                                            if (int.TryParse(pendingText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedPending))
                                                                                                                                                                                                            pendingCount = Math.Max(0, parsedPending);


                                                                                                                                                                                                            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\src\EJLive.Core\Engine\GhostRemoteEngine.cs
                                                                                                                                                                                                            bool requestImmediateSync = false,


                                                                                                                                                                                                            catch (JsonException)
                                                                                                                                                                                                            public static byte[] BuildStartFile(string atmId, string fileName, long length, long offset, string checksum) => BuildFrame(MsgType.StartFile, $"{atmId}|{fileName}|{length}|{offset}|{checksum}");


                                                                                                                                                                                                            ```

                                                                                                                                                                                                            var frame = new byte[header.Length + body.Length];


                                                                                                                                                                                                            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\src\EJLive.Core\Engine\GhostRemoteEngine.cs
                                                                                                                                                                                                            return frame;


                                                                                                                                                                                                            var typeName = header[..separator];


                                                                                                                                                                                                            var lengthText = header[(separator + 1)..];


                                                                                                                                                                                                            if (!Enum.TryParse<MsgType>(typeName, out var type))
                                                                                                                                                                                                            type = MsgType.Unknown;


                                                                                                                                                                                                            return null;


                                                                                                                                                                                                            value = default;


                                                                                                                                                                                                            while (true)
                                                                                                                                                                                                            var next = stream.ReadByte();


                                                                                                                                                                                                            if (next == '\n')
                                                                                                                                                                                                            break;


                                                                                                                                                                                                            var offset = 0;


                                                                                                                                                                                                            while (offset < length)
                                                                                                                                                                                                            var read = stream.Read(buffer, offset, length - offset);


                                                                                                                                                                                                            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\src\EJLive.Core\Engine\GhostRemoteEngine.cs
                                                                                                                                                                                                            offset += read;


                                                                                                                                                                                                            return buffer;


                                                                                                                                                                                                            if (string.IsNullOrWhiteSpace(IssuedAtUtc))
                                                                                                                                                                                                            IssuedAtUtc = DateTime.UtcNow.ToString("O");


                                                                                                                                                                                                            if (string.IsNullOrWhiteSpace(Nonce))
                                                                                                                                                                                                            Nonce = Guid.NewGuid().ToString("N");


                                                                                                                                                                                                            SignatureVersion = CommandSigningEngine.SignatureVersion;


                                                                                                                                                                                                            var payloadBase64 = string.Empty;


                                                                                                                                                                                                            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\src\EJLive.Core\Engine\GhostRemoteEngine.cs
                                                                                                                                                                                                            catch (FormatException)
                                                                                                                                                                                                            command.Payload = payloadBase64;


                                                                                                                                                                                                            if (!CommandSigningEngine.IsFresh(command.IssuedAtUtc, out var freshnessReason))
                                                                                                                                                                                                            command.SignatureFailureReason = freshnessReason;


                                                                                                                                                                                                            if (!CommandSigningEngine.VerifyCanonical(canonical, command.Signature, out var verifyReason))
                                                                                                                                                                                                            command.SignatureFailureReason = verifyReason;


                                                                                                                                                                                                            command.SignatureVerified = true;


                                                                                                                                                                                                            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\src\EJLive.Core\Engine\GhostRemoteEngine.cs
                                                                                                                                                                                                            command.SignatureVerified = false;


                                                                                                                                                                                                            private readonly string _encryptionIV;


                                                                                                                                                                                                            private readonly bool _useEncryption;


                                                                                                                                                                                                            private readonly bool _useCompression;


                                                                                                                                                                                                            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\src\EJLive.Core\Engine\GhostRemoteEngine.cs
                                                                                                                                                                                                            _encryptionIV = SecurityConfig.DEFAULT_IV;


                                                                                                                                                                                                            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\src\EJLive.Core\Engine\GhostRemoteEngine.cs
                                                                                                                                                                                                            _useEncryption = useEncryption;


                                                                                                                                                                                                            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\src\EJLive.Core\Engine\GhostRemoteEngine.cs
                                                                                                                                                                                                            _useCompression = useCompression;


                                                                                                                                                                                                            string fullMessage = header + "\n" + data + Protocol.DATA_END;


                                                                                                                                                                                                            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\src\EJLive.Core\Engine\GhostRemoteEngine.cs
                                                                                                                                                                                                            string fullMessage = header + "\n" + base64Data + Protocol.DATA_END;


                                                                                                                                                                                                            if (_useEncryption && data.Length > 0)
                                                                                                                                                                                                            data = Decrypt(data);


                                                                                                                                                                                                            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\src\EJLive.Core\Engine\GhostRemoteEngine.cs
                                                                                                                                                                                                            return data;


                                                                                                                                                                                                            if (string.IsNullOrEmpty(message)) return info;


                                                                                                                                                                                                            if (body.EndsWith(Protocol.DATA_END.Trim()))
                                                                                                                                                                                                            body = body.Substring(0, body.Length - Protocol.DATA_END.Trim().Length);


                                                                                                                                                                                                            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\src\EJLive.Core\Engine\GhostRemoteEngine.cs
                                                                                                                                                                                                            return info;


                                                                                                                                                                                                            info.RawParts = parts;


                                                                                                                                                                                                            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\src\EJLive.Core\Engine\GhostRemoteEngine.cs
                                                                                                                                                                                                            info.Body = body;


                                                                                                                                                                                                            for (int i = 0; i < data.Length; i++)


                                                                                                                                                                                                            int pendingCommandCount,


                                                                                                                                                                                                            string? serverMessage = null)


                                                                                                                                                                                                            ATM_ID = atmId,


                                                                                                                                                                                                            RequestImmediateSync = requestImmediateSync,


                                                                                                                                                                                                            ServerMessage = serverMessage


                                                                                                                                                                                                            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\GhostRemoteEngine.cs.v21_bak
                                                                                                                                                                                                            if (DateTime.TryParse(serverTimeText, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var parsedServerTime))
                                                                                                                                                                                                            serverTime = parsedServerTime.ToUniversalTime();


                                                                                                                                                                                                            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\src\EJLive.Core\Engine\GhostRemoteEngine.cs
                                                                                                                                                                                                            int pendingCommandCount,


                                                                                                                                                                                                            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\src\EJLive.Core\Engine\GhostRemoteEngine.cs
                                                                                                                                                                                                            string? serverMessage = null)


                                                                                                                                                                                                            ATM_ID = atmId,


                                                                                                                                                                                                            RequestImmediateSync = requestImmediateSync,


                                                                                                                                                                                                            ServerMessage = serverMessage


                                                                                                                                                                                                            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\GhostRemoteEngine.cs
                                                                                                                                                                                                            if (DateTime.TryParse(serverTimeText, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var parsedServerTime))
                                                                                                                                                                                                            serverTime = parsedServerTime.ToUniversalTime();


                                                                                                                                                                                                            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\GhostRemoteEngine.cs
                                                                                                                                                                                                            if (int.TryParse(pendingText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedPending))
                                                                                                                                                                                                            pendingCount = Math.Max(0, parsedPending);


                                                                                                                                                                                                            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\GhostRemoteEngine.cs
                                                                                                                                                                                                            bool requestImmediateSync = false,


                                                                                                                                                                                                            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\GhostRemoteEngine.cs
                                                                                                                                                                                                            return frame;


                                                                                                                                                                                                            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\GhostRemoteEngine.cs
                                                                                                                                                                                                            offset += read;


                                                                                                                                                                                                            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\GhostRemoteEngine.cs
                                                                                                                                                                                                            catch (FormatException)
                                                                                                                                                                                                            command.Payload = payloadBase64;


                                                                                                                                                                                                            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\GhostRemoteEngine.cs
                                                                                                                                                                                                            command.SignatureVerified = false;


                                                                                                                                                                                                            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\GhostRemoteEngine.cs
                                                                                                                                                                                                            _encryptionIV = SecurityConfig.DEFAULT_IV;


                                                                                                                                                                                                            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\GhostRemoteEngine.cs
                                                                                                                                                                                                            _useEncryption = useEncryption;


                                                                                                                                                                                                            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\GhostRemoteEngine.cs
                                                                                                                                                                                                            _useCompression = useCompression;


                                                                                                                                                                                                            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\GhostRemoteEngine.cs
                                                                                                                                                                                                            string fullMessage = header + "\n" + base64Data + Protocol.DATA_END;


                                                                                                                                                                                                            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\GhostRemoteEngine.cs
                                                                                                                                                                                                            return data;


                                                                                                                                                                                                            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\GhostRemoteEngine.cs
                                                                                                                                                                                                            return info;


                                                                                                                                                                                                            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\GhostRemoteEngine.cs
                                                                                                                                                                                                            info.Body = body;


                                                                                                                                                                                                            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\GhostRemoteEngine.cs
                                                                                                                                                                                                            string? serverMessage = null)


                                                                                                                                                                                                            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\GhostRemoteEngine.cs
                                                                                                                                                                                                            return true;


                                                                                                                                                                                                            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\GhostRemoteEngine.cs
                                                                                                                                                                                                            int pendingCommandCount,


                                                                                                                                                                                                            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\src\EJLive.Core\Engine\GhostRemoteEngine.cs
                                                                                                                                                                                                            if (DateTime.TryParse(serverTimeText, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var parsedServerTime))
                                                                                                                                                                                                            serverTime = parsedServerTime.ToUniversalTime();


                                                                                                                                                                                                            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\src\EJLive.Core\Engine\GhostRemoteEngine.cs
                                                                                                                                                                                                            if (int.TryParse(pendingText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedPending))
                                                                                                                                                                                                            pendingCount = Math.Max(0, parsedPending);


                                                                                                                                                                                                            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\src\EJLive.Core\Engine\GhostRemoteEngine.cs
                                                                                                                                                                                                            bool requestImmediateSync = false,


                                                                                                                                                                                                            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\src\EJLive.Core\Engine\GhostRemoteEngine.cs
                                                                                                                                                                                                            return true;


                                                                                                                                                                                                            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\src\EJLive.Core\Engine\GhostRemoteEngine.cs
                                                                                                                                                                                                            return frame;


                                                                                                                                                                                                            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\src\EJLive.Core\Engine\GhostRemoteEngine.cs
                                                                                                                                                                                                            offset += read;


                                                                                                                                                                                                            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\src\EJLive.Core\Engine\GhostRemoteEngine.cs
                                                                                                                                                                                                            catch (FormatException)
                                                                                                                                                                                                            command.Payload = payloadBase64;


                                                                                                                                                                                                            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\src\EJLive.Core\Engine\GhostRemoteEngine.cs
                                                                                                                                                                                                            command.SignatureVerified = false;


                                                                                                                                                                                                            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\src\EJLive.Core\Engine\GhostRemoteEngine.cs
                                                                                                                                                                                                            _encryptionIV = SecurityConfig.DEFAULT_IV;


                                                                                                                                                                                                            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\src\EJLive.Core\Engine\GhostRemoteEngine.cs
                                                                                                                                                                                                            _useEncryption = useEncryption;


                                                                                                                                                                                                            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\src\EJLive.Core\Engine\GhostRemoteEngine.cs
                                                                                                                                                                                                            _useCompression = useCompression;


                                                                                                                                                                                                            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\src\EJLive.Core\Engine\GhostRemoteEngine.cs
                                                                                                                                                                                                            string fullMessage = header + "\n" + base64Data + Protocol.DATA_END;


                                                                                                                                                                                                            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\src\EJLive.Core\Engine\GhostRemoteEngine.cs
                                                                                                                                                                                                            return data;


                                                                                                                                                                                                            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\src\EJLive.Core\Engine\GhostRemoteEngine.cs
                                                                                                                                                                                                            return info;


                                                                                                                                                                                                            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\src\EJLive.Core\Engine\GhostRemoteEngine.cs
                                                                                                                                                                                                            info.Body = body;


                                                                                                                                                                                                            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\src\EJLive.Core\Engine\GhostRemoteEngine.cs
                                                                                                                                                                                                            int pendingCommandCount,


                                                                                                                                                                                                            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\src\EJLive.Core\Engine\GhostRemoteEngine.cs
                                                                                                                                                                                                            string? serverMessage = null)


                                                                                                                                                                                                            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Engine\GhostRemoteEngine.cs.before_unify
                                                                                                                                                                                                            if (DateTime.TryParse(serverTimeText, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var parsedServerTime))
                                                                                                                                                                                                            serverTime = parsedServerTime.ToUniversalTime();


                                                                                                                                                                                                            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Engine\GhostRemoteEngine.cs.before_unify
                                                                                                                                                                                                            if (int.TryParse(pendingText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedPending))
                                                                                                                                                                                                            pendingCount = Math.Max(0, parsedPending);


                                                                                                                                                                                                            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Engine\GhostRemoteEngine.cs.before_unify
                                                                                                                                                                                                            if (bool.TryParse(immediateText, out var parsedImmediate))
                                                                                                                                                                                                            requestImmediateSync = parsedImmediate;


                                                                                                                                                                                                            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Engine\GhostRemoteEngine.cs.before_unify
                                                                                                                                                                                                            return true;


                                                                                                                                                                                                            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Engine\GhostRemoteEngine.cs.before_unify
                                                                                                                                                                                                            checksum ^= data[i];


                                                                                                                                                                                                            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Engine\GhostRemoteEngine.cs.before_unify
                                                                                                                                                                                                            return frame;


                                                                                                                                                                                                            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Engine\GhostRemoteEngine.cs.before_unify
                                                                                                                                                                                                            offset += read;


                                                                                                                                                                                                            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Engine\GhostRemoteEngine.cs.before_unify
                                                                                                                                                                                                            catch (FormatException)
                                                                                                                                                                                                            command.Payload = payloadBase64;


                                                                                                                                                                                                            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Engine\GhostRemoteEngine.cs.before_unify
                                                                                                                                                                                                            command.SignatureVerified = false;


                                                                                                                                                                                                            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Engine\GhostRemoteEngine.cs.before_unify
                                                                                                                                                                                                            _encryptionIV = SecurityConfig.DEFAULT_IV;


                                                                                                                                                                                                            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Engine\GhostRemoteEngine.cs.before_unify
                                                                                                                                                                                                            _useEncryption = useEncryption;


                                                                                                                                                                                                            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Engine\GhostRemoteEngine.cs.before_unify
                                                                                                                                                                                                            _useCompression = useCompression;


                                                                                                                                                                                                            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Engine\GhostRemoteEngine.cs.before_unify
                                                                                                                                                                                                            string fullMessage = header + "\n" + base64Data + Protocol.DATA_END;


                                                                                                                                                                                                            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Engine\GhostRemoteEngine.cs.before_unify
                                                                                                                                                                                                            return data;


                                                                                                                                                                                                            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Engine\GhostRemoteEngine.cs.before_unify
                                                                                                                                                                                                            return info;


                                                                                                                                                                                                            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Engine\GhostRemoteEngine.cs.before_unify
                                                                                                                                                                                                            info.Body = body;


                                                                                                                                                                                                            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Engine\GhostRemoteEngine.cs.v21_bak
                                                                                                                                                                                                            if (DateTime.TryParse(serverTimeText, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var parsedServerTime))
                                                                                                                                                                                                            serverTime = parsedServerTime.ToUniversalTime();


                                                                                                                                                                                                            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Engine\GhostRemoteEngine.cs.v21_bak
                                                                                                                                                                                                            if (int.TryParse(pendingText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedPending))
                                                                                                                                                                                                            pendingCount = Math.Max(0, parsedPending);


                                                                                                                                                                                                            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Engine\GhostRemoteEngine.cs.v21_bak
                                                                                                                                                                                                            if (bool.TryParse(immediateText, out var parsedImmediate))
                                                                                                                                                                                                            requestImmediateSync = parsedImmediate;


                                                                                                                                                                                                            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Engine\GhostRemoteEngine.cs.v21_bak
                                                                                                                                                                                                            return true;


                                                                                                                                                                                                            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Engine\GhostRemoteEngine.cs.v21_bak
                                                                                                                                                                                                            ```


                                                                                                                                                                                                            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Engine\GhostRemoteEngine.cs.v21_bak
                                                                                                                                                                                                            return frame;


                                                                                                                                                                                                            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Engine\GhostRemoteEngine.cs.v21_bak
                                                                                                                                                                                                            offset += read;


                                                                                                                                                                                                            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Engine\GhostRemoteEngine.cs.v21_bak
                                                                                                                                                                                                            catch (FormatException)
                                                                                                                                                                                                            command.Payload = payloadBase64;


                                                                                                                                                                                                            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Engine\GhostRemoteEngine.cs.v21_bak
                                                                                                                                                                                                            command.SignatureVerified = false;


                                                                                                                                                                                                            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Engine\GhostRemoteEngine.cs.v21_bak
                                                                                                                                                                                                            _encryptionIV = SecurityConfig.DEFAULT_IV;


                                                                                                                                                                                                            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Engine\GhostRemoteEngine.cs.v21_bak
                                                                                                                                                                                                            _useEncryption = useEncryption;


                                                                                                                                                                                                            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Engine\GhostRemoteEngine.cs.v21_bak
                                                                                                                                                                                                            _useCompression = useCompression;


                                                                                                                                                                                                            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Engine\GhostRemoteEngine.cs.v21_bak
                                                                                                                                                                                                            string fullMessage = header + "\n" + base64Data + Protocol.DATA_END;


                                                                                                                                                                                                            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Engine\GhostRemoteEngine.cs.v21_bak
                                                                                                                                                                                                            return data;


                                                                                                                                                                                                            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Engine\GhostRemoteEngine.cs.v21_bak
                                                                                                                                                                                                            return info;


                                                                                                                                                                                                            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Engine\GhostRemoteEngine.cs.v21_bak
                                                                                                                                                                                                            info.Body = body;


                                                                                                                                                                                                            public string CommandType { get; set; } = string.Empty;


                                                                                                                                                                                                            public string Payload { get; set; } = string.Empty;


                                                                                                                                                                                                            public bool RequiresConfirmation { get; set; }


                                                                                                                                                                                                            public string IssuedAtUtc { get; set; } = DateTime.UtcNow.ToString("O");


                                                                                                                                                                                                            public string Nonce { get; set; } = Guid.NewGuid().ToString("N");


                                                                                                                                                                                                            public string Signature { get; set; } = string.Empty;


                                                                                                                                                                                                            public string SignatureVersion { get; set; } = CommandSigningEngine.SignatureVersion;


                                                                                                                                                                                                            public bool SignatureVerified { get; private set; }


                                                                                                                                                                                                            public string SignatureFailureReason { get; private set; } = string.Empty;


                                                                                                                                                                                                            public string ATM_ID { get; set; }


                                                                                                                                                                                                            public string ATM_Type { get; set; }


                                                                                                                                                                                                            public string Version { get; set; }


                                                                                                                                                                                                            public string FileName { get; set; }


                                                                                                                                                                                                            public int DataLength { get; set; }


                                                                                                                                                                                                            public string Checksum { get; set; }


                                                                                                                                                                                                            public string Body { get; set; }


                                                                                                                                                                                                            public byte[] BinaryData { get; set; }


                                                                                                                                                                                                            public string CommandId { get; set; }


                                                                                                                                                                                                            public bool Success { get; set; }


                                                                                                                                                                                                            public string ResultData { get; set; }


                                                                                                                                                                                                            public string Status { get; set; }


                                                                                                                                                                                                            public string CSCStatus { get; set; }


                                                                                                                                                                                                            public string[] RawParts { get; set; }


                                                                                                                                                                                                            public static byte[] BuildHandshakeAck(string sessionId) => BuildFrame(MsgType.HandshakeAck, $"OK|{sessionId}");


                                                                                                                                                                                                            public static byte[] BuildHeartbeat(string atmId, string payload = "") => BuildFrame(MsgType.Heartbeat, $"{atmId}|{payload}");


                                                                                                                                                                                                            public static byte[] BuildHeartbeatAck(string atmId = "") => BuildFrame(MsgType.HeartbeatAck, atmId);


                                                                                                                                                                                                            public static byte[] BuildHeartbeatAck(


                                                                                                                                                                                                            var payload = JsonSerializer.Serialize(new;


                                                                                                                                                                                                            ServerTimeUtc = serverTimeUtc.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture),


                                                                                                                                                                                                            PendingCommandCount = Math.Max(0, pendingCommandCount),


                                                                                                                                                                                                            public static byte[] BuildHeartbeatAck(
                                                                                                                                                                                                            string atmId,
                                                                                                                                                                                                            int pendingCommandCount,
                                                                                                                                                                                                            bool requestImmediateSync = false,
                                                                                                                                                                                                            string? serverMessage = null)
                                                                                                                                                                                                            var payload = JsonSerializer.Serialize(new;
                                                                                                                                                                                                            ATM_ID = atmId,
                                                                                                                                                                                                            ServerTimeUtc = serverTimeUtc.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture),
                                                                                                                                                                                                            PendingCommandCount = Math.Max(0, pendingCommandCount),
                                                                                                                                                                                                            RequestImmediateSync = requestImmediateSync,
                                                                                                                                                                                                            ServerMessage = serverMessage
                                                                                                                                                                                                        }
                                                                                                                                                                                                    public partial enum MsgType
                                                                                                                                                                                                    {
                                                                                                                                                                                                        payload = string.Empty;


                                                                                                                                                                                                        if (string.IsNullOrWhiteSpace(value))
                                                                                                                                                                                                        return false;


                                                                                                                                                                                                        if (separator < 0)
                                                                                                                                                                                                        atmId = value;


                                                                                                                                                                                                        if (separator >= 0)
                                                                                                                                                                                                        var tail = value[(separator + 1)..].Trim();


                                                                                                                                                                                                        if (tail.StartsWith("{", StringComparison.Ordinal))
                                                                                                                                                                                                        jsonCandidate = tail;


                                                                                                                                                                                                        if (!jsonCandidate.StartsWith("{", StringComparison.Ordinal))
                                                                                                                                                                                                        try
                                                                                                                                                                                                        if (document.RootElement.ValueKind != JsonValueKind.Object)
                                                                                                                                                                                                        var root = document.RootElement;


                                                                                                                                                                                                        var serverTime = DateTime.UtcNow;


                                                                                                                                                                                                        var pendingCount = 0;


                                                                                                                                                                                                        var requestImmediateSync = false;


                                                                                                                                                                                                        // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\src\EJLive.Core\Engine\GhostRemoteEngine.cs
                                                                                                                                                                                                        if (DateTime.TryParse(serverTimeText, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var parsedServerTime))
                                                                                                                                                                                                        serverTime = parsedServerTime.ToUniversalTime();


                                                                                                                                                                                                        // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\src\EJLive.Core\Engine\GhostRemoteEngine.cs
                                                                                                                                                                                                        if (int.TryParse(pendingText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedPending))
                                                                                                                                                                                                        pendingCount = Math.Max(0, parsedPending);


                                                                                                                                                                                                        // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\src\EJLive.Core\Engine\GhostRemoteEngine.cs
                                                                                                                                                                                                        bool requestImmediateSync = false,


                                                                                                                                                                                                        return true;


                                                                                                                                                                                                        catch (JsonException)
                                                                                                                                                                                                        public static byte[] BuildStartFile(string atmId, string fileName, long length, long offset, string checksum) => BuildFrame(MsgType.StartFile, $"{atmId}|{fileName}|{length}|{offset}|{checksum}");


                                                                                                                                                                                                        ```

                                                                                                                                                                                                        var frame = new byte[header.Length + body.Length];


                                                                                                                                                                                                        // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\src\EJLive.Core\Engine\GhostRemoteEngine.cs
                                                                                                                                                                                                        return frame;


                                                                                                                                                                                                        var typeName = header[..separator];


                                                                                                                                                                                                        var lengthText = header[(separator + 1)..];


                                                                                                                                                                                                        if (!Enum.TryParse<MsgType>(typeName, out var type))
                                                                                                                                                                                                        type = MsgType.Unknown;


                                                                                                                                                                                                        return null;


                                                                                                                                                                                                        value = default;


                                                                                                                                                                                                        while (true)
                                                                                                                                                                                                        var next = stream.ReadByte();


                                                                                                                                                                                                        if (next == '\n')
                                                                                                                                                                                                        break;


                                                                                                                                                                                                        var offset = 0;


                                                                                                                                                                                                        while (offset < length)
                                                                                                                                                                                                        var read = stream.Read(buffer, offset, length - offset);


                                                                                                                                                                                                        // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\src\EJLive.Core\Engine\GhostRemoteEngine.cs
                                                                                                                                                                                                        offset += read;


                                                                                                                                                                                                        return buffer;


                                                                                                                                                                                                        if (string.IsNullOrWhiteSpace(IssuedAtUtc))
                                                                                                                                                                                                        IssuedAtUtc = DateTime.UtcNow.ToString("O");


                                                                                                                                                                                                        if (string.IsNullOrWhiteSpace(Nonce))
                                                                                                                                                                                                        Nonce = Guid.NewGuid().ToString("N");


                                                                                                                                                                                                        SignatureVersion = CommandSigningEngine.SignatureVersion;


                                                                                                                                                                                                        var payloadBase64 = string.Empty;


                                                                                                                                                                                                        // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\src\EJLive.Core\Engine\GhostRemoteEngine.cs
                                                                                                                                                                                                        catch (FormatException)
                                                                                                                                                                                                        command.Payload = payloadBase64;


                                                                                                                                                                                                        if (!CommandSigningEngine.IsFresh(command.IssuedAtUtc, out var freshnessReason))
                                                                                                                                                                                                        command.SignatureFailureReason = freshnessReason;


                                                                                                                                                                                                        if (!CommandSigningEngine.VerifyCanonical(canonical, command.Signature, out var verifyReason))
                                                                                                                                                                                                        command.SignatureFailureReason = verifyReason;


                                                                                                                                                                                                        // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\src\EJLive.Core\Engine\GhostRemoteEngine.cs
                                                                                                                                                                                                        command.SignatureVerified = true;


                                                                                                                                                                                                        // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\src\EJLive.Core\Engine\GhostRemoteEngine.cs
                                                                                                                                                                                                        command.SignatureVerified = false;


                                                                                                                                                                                                        private readonly string _encryptionIV;


                                                                                                                                                                                                        private readonly bool _useEncryption;


                                                                                                                                                                                                        private readonly bool _useCompression;


                                                                                                                                                                                                        // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\src\EJLive.Core\Engine\GhostRemoteEngine.cs
                                                                                                                                                                                                        _encryptionIV = SecurityConfig.DEFAULT_IV;


                                                                                                                                                                                                        // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\src\EJLive.Core\Engine\GhostRemoteEngine.cs
                                                                                                                                                                                                        _useEncryption = useEncryption;


                                                                                                                                                                                                        // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\src\EJLive.Core\Engine\GhostRemoteEngine.cs
                                                                                                                                                                                                        _useCompression = useCompression;


                                                                                                                                                                                                        string fullMessage = header + "\n" + data + Protocol.DATA_END;


                                                                                                                                                                                                        // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\src\EJLive.Core\Engine\GhostRemoteEngine.cs
                                                                                                                                                                                                        string fullMessage = header + "\n" + base64Data + Protocol.DATA_END;


                                                                                                                                                                                                        if (_useEncryption && data.Length > 0)
                                                                                                                                                                                                        data = Decrypt(data);


                                                                                                                                                                                                        // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\src\EJLive.Core\Engine\GhostRemoteEngine.cs
                                                                                                                                                                                                        return data;


                                                                                                                                                                                                        if (string.IsNullOrEmpty(message)) return info;


                                                                                                                                                                                                        if (body.EndsWith(Protocol.DATA_END.Trim()))
                                                                                                                                                                                                        body = body.Substring(0, body.Length - Protocol.DATA_END.Trim().Length);


                                                                                                                                                                                                        // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\src\EJLive.Core\Engine\GhostRemoteEngine.cs
                                                                                                                                                                                                        return info;


                                                                                                                                                                                                        info.RawParts = parts;


                                                                                                                                                                                                        // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\src\EJLive.Core\Engine\GhostRemoteEngine.cs
                                                                                                                                                                                                        info.Body = body;


                                                                                                                                                                                                        for (int i = 0; i < data.Length; i++)


                                                                                                                                                                                                        // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\src\EJLive.Core\Engine\GhostRemoteEngine.cs
                                                                                                                                                                                                        int pendingCommandCount,


                                                                                                                                                                                                        string? serverMessage = null)


                                                                                                                                                                                                        ATM_ID = atmId,


                                                                                                                                                                                                        RequestImmediateSync = requestImmediateSync,


                                                                                                                                                                                                        ServerMessage = serverMessage


                                                                                                                                                                                                        // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\GhostRemoteEngine.cs.v21_bak
                                                                                                                                                                                                        if (DateTime.TryParse(serverTimeText, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var parsedServerTime))
                                                                                                                                                                                                        serverTime = parsedServerTime.ToUniversalTime();


                                                                                                                                                                                                        // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\GhostRemoteEngine.cs.v21_bak
                                                                                                                                                                                                        if (int.TryParse(pendingText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedPending))
                                                                                                                                                                                                        pendingCount = Math.Max(0, parsedPending);


                                                                                                                                                                                                        // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\GhostRemoteEngine.cs.v21_bak
                                                                                                                                                                                                        if (bool.TryParse(immediateText, out var parsedImmediate))
                                                                                                                                                                                                        requestImmediateSync = parsedImmediate;


                                                                                                                                                                                                        // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\GhostRemoteEngine.cs.v21_bak
                                                                                                                                                                                                        ```


                                                                                                                                                                                                        // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\GhostRemoteEngine.cs.v21_bak
                                                                                                                                                                                                        return frame;


                                                                                                                                                                                                        // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\GhostRemoteEngine.cs.v21_bak
                                                                                                                                                                                                        offset += read;


                                                                                                                                                                                                        // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\GhostRemoteEngine.cs.v21_bak
                                                                                                                                                                                                        catch (FormatException)
                                                                                                                                                                                                        command.Payload = payloadBase64;


                                                                                                                                                                                                        // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\GhostRemoteEngine.cs.v21_bak
                                                                                                                                                                                                        command.SignatureVerified = true;


                                                                                                                                                                                                        // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\GhostRemoteEngine.cs.v21_bak
                                                                                                                                                                                                        command.SignatureVerified = false;


                                                                                                                                                                                                        // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\GhostRemoteEngine.cs.v21_bak
                                                                                                                                                                                                        _encryptionIV = SecurityConfig.DEFAULT_IV;


                                                                                                                                                                                                        // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\GhostRemoteEngine.cs.v21_bak
                                                                                                                                                                                                        _useEncryption = useEncryption;


                                                                                                                                                                                                        // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\GhostRemoteEngine.cs.v21_bak
                                                                                                                                                                                                        _useCompression = useCompression;


                                                                                                                                                                                                        // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\GhostRemoteEngine.cs.v21_bak
                                                                                                                                                                                                        string fullMessage = header + "\n" + base64Data + Protocol.DATA_END;


                                                                                                                                                                                                        // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\GhostRemoteEngine.cs.v21_bak
                                                                                                                                                                                                        return data;


                                                                                                                                                                                                        // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\GhostRemoteEngine.cs.v21_bak
                                                                                                                                                                                                        return info;


                                                                                                                                                                                                        // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\GhostRemoteEngine.cs.v21_bak
                                                                                                                                                                                                        info.Body = body;


                                                                                                                                                                                                        // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\GhostRemoteEngine.cs.before_unify
                                                                                                                                                                                                        if (DateTime.TryParse(serverTimeText, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var parsedServerTime))
                                                                                                                                                                                                        serverTime = parsedServerTime.ToUniversalTime();


                                                                                                                                                                                                        // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\GhostRemoteEngine.cs.before_unify
                                                                                                                                                                                                        if (int.TryParse(pendingText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedPending))
                                                                                                                                                                                                        pendingCount = Math.Max(0, parsedPending);


                                                                                                                                                                                                        // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\GhostRemoteEngine.cs.before_unify
                                                                                                                                                                                                        if (bool.TryParse(immediateText, out var parsedImmediate))
                                                                                                                                                                                                        requestImmediateSync = parsedImmediate;


                                                                                                                                                                                                        // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\GhostRemoteEngine.cs.before_unify
                                                                                                                                                                                                        checksum ^= data[i];


                                                                                                                                                                                                        // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\GhostRemoteEngine.cs.before_unify
                                                                                                                                                                                                        return frame;


                                                                                                                                                                                                        // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\GhostRemoteEngine.cs.before_unify
                                                                                                                                                                                                        offset += read;


                                                                                                                                                                                                        // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\GhostRemoteEngine.cs.before_unify
                                                                                                                                                                                                        catch (FormatException)
                                                                                                                                                                                                        command.Payload = payloadBase64;


                                                                                                                                                                                                        // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\GhostRemoteEngine.cs.before_unify
                                                                                                                                                                                                        command.SignatureVerified = true;


                                                                                                                                                                                                        // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\GhostRemoteEngine.cs.before_unify
                                                                                                                                                                                                        command.SignatureVerified = false;


                                                                                                                                                                                                        // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\GhostRemoteEngine.cs.before_unify
                                                                                                                                                                                                        _encryptionIV = SecurityConfig.DEFAULT_IV;


                                                                                                                                                                                                        // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\GhostRemoteEngine.cs.before_unify
                                                                                                                                                                                                        _useEncryption = useEncryption;


                                                                                                                                                                                                        // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\GhostRemoteEngine.cs.before_unify
                                                                                                                                                                                                        _useCompression = useCompression;


                                                                                                                                                                                                        // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\GhostRemoteEngine.cs.before_unify
                                                                                                                                                                                                        string fullMessage = header + "\n" + base64Data + Protocol.DATA_END;


                                                                                                                                                                                                        // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\GhostRemoteEngine.cs.before_unify
                                                                                                                                                                                                        return data;


                                                                                                                                                                                                        // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\GhostRemoteEngine.cs.before_unify
                                                                                                                                                                                                        return info;


                                                                                                                                                                                                        // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\GhostRemoteEngine.cs.before_unify
                                                                                                                                                                                                        info.Body = body;


                                                                                                                                                                                                        public string CommandType { get; set; } = string.Empty;


                                                                                                                                                                                                        public string Payload { get; set; } = string.Empty;


                                                                                                                                                                                                        public bool RequiresConfirmation { get; set; }


                                                                                                                                                                                                        public string IssuedAtUtc { get; set; } = DateTime.UtcNow.ToString("O");


                                                                                                                                                                                                        public string Nonce { get; set; } = Guid.NewGuid().ToString("N");


                                                                                                                                                                                                        public string Signature { get; set; } = string.Empty;


                                                                                                                                                                                                        public string SignatureVersion { get; set; } = CommandSigningEngine.SignatureVersion;


                                                                                                                                                                                                        public bool SignatureVerified { get; private set; }


                                                                                                                                                                                                        public string SignatureFailureReason { get; private set; } = string.Empty;


                                                                                                                                                                                                        public string ATM_ID { get; set; }


                                                                                                                                                                                                        public string ATM_Type { get; set; }


                                                                                                                                                                                                        public string Version { get; set; }


                                                                                                                                                                                                        public string FileName { get; set; }


                                                                                                                                                                                                        public int DataLength { get; set; }


                                                                                                                                                                                                        public string Checksum { get; set; }


                                                                                                                                                                                                        public string Body { get; set; }


                                                                                                                                                                                                        public byte[] BinaryData { get; set; }


                                                                                                                                                                                                        public string CommandId { get; set; }


                                                                                                                                                                                                        public bool Success { get; set; }


                                                                                                                                                                                                        public string ResultData { get; set; }


                                                                                                                                                                                                        public string Status { get; set; }


                                                                                                                                                                                                        public string CSCStatus { get; set; }


                                                                                                                                                                                                        public string[] RawParts { get; set; }


                                                                                                                                                                                                        public static byte[] BuildHandshakeAck(string sessionId) => BuildFrame(MsgType.HandshakeAck, $"OK|{sessionId}");


                                                                                                                                                                                                        public static byte[] BuildHeartbeat(string atmId, string payload = "") => BuildFrame(MsgType.Heartbeat, $"{atmId}|{payload}");


                                                                                                                                                                                                        public static byte[] BuildHeartbeatAck(string atmId = "") => BuildFrame(MsgType.HeartbeatAck, atmId);


                                                                                                                                                                                                        public static byte[] BuildHeartbeatAck(


                                                                                                                                                                                                        var payload = JsonSerializer.Serialize(new;


                                                                                                                                                                                                        ServerTimeUtc = serverTimeUtc.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture),


                                                                                                                                                                                                        PendingCommandCount = Math.Max(0, pendingCommandCount),


                                                                                                                                                                                                        public static byte[] BuildHeartbeatAck(
                                                                                                                                                                                                        string atmId,
                                                                                                                                                                                                        int pendingCommandCount,
                                                                                                                                                                                                        bool requestImmediateSync = false,
                                                                                                                                                                                                        string? serverMessage = null)
                                                                                                                                                                                                        var payload = JsonSerializer.Serialize(new;
                                                                                                                                                                                                        ATM_ID = atmId,
                                                                                                                                                                                                        ServerTimeUtc = serverTimeUtc.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture),
                                                                                                                                                                                                        PendingCommandCount = Math.Max(0, pendingCommandCount),
                                                                                                                                                                                                        RequestImmediateSync = requestImmediateSync,
                                                                                                                                                                                                        ServerMessage = serverMessage
                                                                                                                                                                                                    }
                                                                                                                                                                                                public partial enum MsgType
                                                                                                                                                                                                {
                                                                                                                                                                                                    payload = string.Empty;
                                                                                                                                                                                                    if (string.IsNullOrWhiteSpace(value))
                                                                                                                                                                                                    return false;
                                                                                                                                                                                                    if (separator < 0)
                                                                                                                                                                                                    atmId = value;
                                                                                                                                                                                                    if (separator >= 0)
                                                                                                                                                                                                    var tail = value[(separator + 1)..].Trim();
                                                                                                                                                                                                    if (tail.StartsWith("{", StringComparison.Ordinal))
                                                                                                                                                                                                    jsonCandidate = tail;
                                                                                                                                                                                                    if (!jsonCandidate.StartsWith("{", StringComparison.Ordinal))
                                                                                                                                                                                                    try
                                                                                                                                                                                                    if (document.RootElement.ValueKind != JsonValueKind.Object)
                                                                                                                                                                                                    var root = document.RootElement;
                                                                                                                                                                                                    var serverTime = DateTime.UtcNow;
                                                                                                                                                                                                    var pendingCount = 0;
                                                                                                                                                                                                    var requestImmediateSync = false;
                                                                                                                                                                                                    if (DateTime.TryParse(serverTimeText, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var parsedServerTime))
                                                                                                                                                                                                    serverTime = parsedServerTime.ToUniversalTime();
                                                                                                                                                                                                    if (int.TryParse(pendingText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedPending))
                                                                                                                                                                                                    pendingCount = Math.Max(0, parsedPending);
                                                                                                                                                                                                    bool requestImmediateSync = false,
                                                                                                                                                                                                    return true;
                                                                                                                                                                                                    catch (JsonException)
                                                                                                                                                                                                    public static byte[] BuildStartFile(string atmId, string fileName, long length, long offset, string checksum) => BuildFrame(MsgType.StartFile, $"{atmId}|{fileName}|{length}|{offset}|{checksum}");
                                                                                                                                                                                                    ```
                                                                                                                                                                                                    var frame = new byte[header.Length + body.Length];
                                                                                                                                                                                                    return frame;
                                                                                                                                                                                                    var typeName = header[..separator];
                                                                                                                                                                                                    var lengthText = header[(separator + 1)..];
                                                                                                                                                                                                    if (!Enum.TryParse<MsgType>(typeName, out var type))
                                                                                                                                                                                                    type = MsgType.Unknown;
                                                                                                                                                                                                    return null;
                                                                                                                                                                                                    value = default;
                                                                                                                                                                                                    while (true)
                                                                                                                                                                                                    var next = stream.ReadByte();
                                                                                                                                                                                                    if (next == '\n')
                                                                                                                                                                                                    break;
                                                                                                                                                                                                    var offset = 0;
                                                                                                                                                                                                    while (offset < length)
                                                                                                                                                                                                    var read = stream.Read(buffer, offset, length - offset);
                                                                                                                                                                                                    offset += read;
                                                                                                                                                                                                    return buffer;
                                                                                                                                                                                                    if (string.IsNullOrWhiteSpace(IssuedAtUtc))
                                                                                                                                                                                                    IssuedAtUtc = DateTime.UtcNow.ToString("O");
                                                                                                                                                                                                    if (string.IsNullOrWhiteSpace(Nonce))
                                                                                                                                                                                                    Nonce = Guid.NewGuid().ToString("N");
                                                                                                                                                                                                    SignatureVersion = CommandSigningEngine.SignatureVersion;
                                                                                                                                                                                                    var payloadBase64 = string.Empty;
                                                                                                                                                                                                    catch (FormatException)
                                                                                                                                                                                                    command.Payload = payloadBase64;
                                                                                                                                                                                                    if (!CommandSigningEngine.IsFresh(command.IssuedAtUtc, out var freshnessReason))
                                                                                                                                                                                                    command.SignatureFailureReason = freshnessReason;
                                                                                                                                                                                                    if (!CommandSigningEngine.VerifyCanonical(canonical, command.Signature, out var verifyReason))
                                                                                                                                                                                                    command.SignatureFailureReason = verifyReason;
                                                                                                                                                                                                    command.SignatureVerified = true;
                                                                                                                                                                                                    command.SignatureVerified = false;
                                                                                                                                                                                                    private readonly string _encryptionIV;
                                                                                                                                                                                                    private readonly bool _useEncryption;
                                                                                                                                                                                                    private readonly bool _useCompression;
                                                                                                                                                                                                    _encryptionIV = SecurityConfig.DEFAULT_IV;
                                                                                                                                                                                                    _useEncryption = useEncryption;
                                                                                                                                                                                                    _useCompression = useCompression;
                                                                                                                                                                                                    string fullMessage = header + "\n" + data + Protocol.DATA_END;
                                                                                                                                                                                                    string fullMessage = header + "\n" + base64Data + Protocol.DATA_END;
                                                                                                                                                                                                    if (_useEncryption && data.Length > 0)
                                                                                                                                                                                                    data = Decrypt(data);
                                                                                                                                                                                                    return data;
                                                                                                                                                                                                    if (string.IsNullOrEmpty(message)) return info;
                                                                                                                                                                                                    if (body.EndsWith(Protocol.DATA_END.Trim()))
                                                                                                                                                                                                    body = body.Substring(0, body.Length - Protocol.DATA_END.Trim().Length);
                                                                                                                                                                                                    return info;
                                                                                                                                                                                                    info.RawParts = parts;
                                                                                                                                                                                                    info.Body = body;
                                                                                                                                                                                                    for (int i = 0; i < data.Length; i++)
                                                                                                                                                                                                    int pendingCommandCount,
                                                                                                                                                                                                    string? serverMessage = null)
                                                                                                                                                                                                    ATM_ID = atmId,
                                                                                                                                                                                                    RequestImmediateSync = requestImmediateSync,
                                                                                                                                                                                                    ServerMessage = serverMessage
                                                                                                                                                                                                    int pendingCommandCount,
                                                                                                                                                                                                    string? serverMessage = null)
                                                                                                                                                                                                    ATM_ID = atmId,
                                                                                                                                                                                                    RequestImmediateSync = requestImmediateSync,
                                                                                                                                                                                                    ServerMessage = serverMessage
                                                                                                                                                                                                    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\GhostRemoteEngine.cs.v21_bak
                                                                                                                                                                                                    if (DateTime.TryParse(serverTimeText, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var parsedServerTime))
                                                                                                                                                                                                    serverTime = parsedServerTime.ToUniversalTime();
                                                                                                                                                                                                    public string CommandType { get; set; } = string.Empty;
                                                                                                                                                                                                    public string Payload { get; set; } = string.Empty;
                                                                                                                                                                                                    public bool RequiresConfirmation { get; set; }
                                                                                                                                                                                                    public string IssuedAtUtc { get; set; } = DateTime.UtcNow.ToString("O");
                                                                                                                                                                                                    public string Nonce { get; set; } = Guid.NewGuid().ToString("N");
                                                                                                                                                                                                    public string Signature { get; set; } = string.Empty;
                                                                                                                                                                                                    public string SignatureVersion { get; set; } = CommandSigningEngine.SignatureVersion;
                                                                                                                                                                                                    public bool SignatureVerified { get; private set; }
                                                                                                                                                                                                    public string SignatureFailureReason { get; private set; } = string.Empty;
                                                                                                                                                                                                    public string ATM_ID { get; set; }
                                                                                                                                                                                                    public string ATM_Type { get; set; }
                                                                                                                                                                                                    public string Version { get; set; }
                                                                                                                                                                                                    public string FileName { get; set; }
                                                                                                                                                                                                    public int DataLength { get; set; }
                                                                                                                                                                                                    public string Checksum { get; set; }
                                                                                                                                                                                                    public string Body { get; set; }
                                                                                                                                                                                                    public byte[] BinaryData { get; set; }
                                                                                                                                                                                                    public string CommandId { get; set; }
                                                                                                                                                                                                    public bool Success { get; set; }
                                                                                                                                                                                                    public string ResultData { get; set; }
                                                                                                                                                                                                    public string Status { get; set; }
                                                                                                                                                                                                    public string CSCStatus { get; set; }
                                                                                                                                                                                                    public string[] RawParts { get; set; }
                                                                                                                                                                                                    public static byte[] BuildHandshakeAck(string sessionId) => BuildFrame(MsgType.HandshakeAck, $"OK|{sessionId}");
                                                                                                                                                                                                    public static byte[] BuildHeartbeat(string atmId, string payload = "") => BuildFrame(MsgType.Heartbeat, $"{atmId}|{payload}");
                                                                                                                                                                                                    public static byte[] BuildHeartbeatAck(string atmId = "") => BuildFrame(MsgType.HeartbeatAck, atmId);
                                                                                                                                                                                                    public static byte[] BuildHeartbeatAck(
                                                                                                                                                                                                    var payload = JsonSerializer.Serialize(new;
                                                                                                                                                                                                    ServerTimeUtc = serverTimeUtc.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture),
                                                                                                                                                                                                    PendingCommandCount = Math.Max(0, pendingCommandCount),
                                                                                                                                                                                                }
                                                                                                                                                                                            public sealed class RemoteAssistanceSession
                                                                                                                                                                                            {
                                                                                                                                                                                                public string SessionID { get; set; } = string.Empty;
                                                                                                                                                                                                public string AtmId { get; set; } = string.Empty;
                                                                                                                                                                                                public string OperatorName { get; set; } = string.Empty;
                                                                                                                                                                                                public bool ViewOnly { get; set; } = true;
                                                                                                                                                                                                public DateTime StartedAtUtc { get; set; } = DateTime.UtcNow;
                                                                                                                                                                                                public DateTime? EndedAtUtc { get; set; }
                                                                                                                                                                                            }
                                                                                                                                                                                        public partial class RemoteAssistanceSession
                                                                                                                                                                                        {
                                                                                                                                                                                            public string SessionID { get; set; } = string.Empty;
                                                                                                                                                                                            public string AtmId { get; set; } = string.Empty;
                                                                                                                                                                                            public string OperatorName { get; set; } = string.Empty;
                                                                                                                                                                                            public bool ViewOnly { get; set; } = true;
                                                                                                                                                                                            public DateTime StartedAtUtc { get; set; } = DateTime.UtcNow;
                                                                                                                                                                                            public DateTime? EndedAtUtc { get; set; }
                                                                                                                                                                                        }
                                                                                                                                                                                    /// <summary>
                                                                                                                                                                                    /// Represents the planned execution parameters for a remote session.
                                                                                                                                                                                    /// </summary>
                                                                                                                                                                                    public sealed record RemoteSessionExecutionPlan
                                                                                                                                                                                    {
                                                                                                                                                                                        public required Guid RequestId { get; init; }
                                                                                                                                                                                        public required bool IsAllowed { get; init; }
                                                                                                                                                                                        public bool RequiresConsentPrompt { get; init; }
                                                                                                                                                                                        public bool NoConsentAllowed { get; init; }
                                                                                                                                                                                        public int MaxDurationMinutes { get; init; }
                                                                                                                                                                                        public DateTime ScheduledStartUtc { get; init; }
                                                                                                                                                                                        public DateTime ScheduledEndUtc { get; init; }
                                                                                                                                                                                        public string? DenialReason { get; init; }

                                                                                                                                                                                        public static RemoteSessionExecutionPlan Denied(string reason) => new()
                                                                                                                                                                                        {
                                                                                                                                                                                            RequestId = Guid.Empty,
                                                                                                                                                                                            IsAllowed = false,
                                                                                                                                                                                            DenialReason = reason
                                                                                                                                                                                        };
                                                                                                                                                                                }

                                                                                                                                                                            Buffer.BlockCopy(header, 0, frame, 0, header.Length);
                                                                                                                                                                            Buffer.BlockCopy(body, 0, frame, header.Length, body.Length);

                                                                                                                                                                            public static byte[] BuildChunkAck(int sequence) => BuildFrame(MsgType.ChunkAck, sequence.ToString(), BitConverter.GetBytes(sequence));
                                                                                                                                                                            public static byte[] BuildCommand(EJLive.Core.Models.RemoteCommandEnvelope command) => BuildFrame(MsgType.Command, command.ToWireText());
                                                                                                                                                                            public static byte[] BuildCommandResult(string commandId, bool ok, string message) => BuildFrame(MsgType.CommandResult, $"{commandId}|{(ok ? "OK" : "FAIL")}|{Convert.ToBase64String(Encoding.UTF8.GetBytes(message ?? string.Empty))}");
                                                                                                                                                                            public static byte[] BuildComplete(string fileName, string checksum, string sha256) => BuildFrame(MsgType.Complete, $"{fileName}|{checksum}|{sha256}");
                                                                                                                                                                            public static byte[] BuildDisconnect(string reason = "") => BuildFrame(MsgType.Disconnect, reason);
                                                                                                                                                                            public static byte[] BuildError(string message) => BuildFrame(MsgType.Error, message);
                                                                                                                                                                            return BuildFrame(MsgType.HeartbeatAck, payload);
                                                                                                                                                                            public static byte[] BuildFrame(MsgType type, string text, byte[]? payload = null, byte[]? sessionKey = null)
                                                                                                                                                                            var body = payload is { Length: > 0 }
                                                                                                                                                                            public static byte[] BuildGhostFrame(byte[] jpegFrame) => BuildFrame(MsgType.GhostFrame, "image/jpeg", jpegFrame);
                                                                                                                                                                            public static byte[] BuildGhostStart(string atmId) => BuildFrame(MsgType.GhostStart, atmId);
                                                                                                                                                                            public static byte[] BuildGhostStop(string atmId) => BuildFrame(MsgType.GhostStop, atmId);
                                                                                                                                                                            public static byte[] BuildJournalAck(string fileName, bool ok, string message = "") => BuildFrame(MsgType.JournalAck, $"{fileName}|{(ok ? "OK" : "FAIL")}|{message}");
                                                                                                                                                                            body = SecurityHelper.DecryptAES(body, sessionKey);
                                                                                                                                                                            // Class: EJMessage (from 2 sources)
                                                                                                                                                                            public sealed partial class EJMessage
                                                                                                                                                                            {
                                                                                                                                                                                // --- Properties ---
                                                                                                                                                                                public CommunicationProtocol.MsgType Type { get; set; }

                                                                                                                                                                                public string Text { get; set; } = string.Empty;

                                                                                                                                                                                public byte[] Payload { get; set; } = Array.Empty<byte>();

                                                                                                                                                                                public DateTime ReceivedAtUtc { get; set; } = DateTime.UtcNow;


                                                                                                                                                                            }
                                                                                                                                                                        body = SecurityHelper.EncryptAES(body, sessionKey);
                                                                                                                                                                        ? payload : WireEncoding.GetBytes(text ?? string.Empty);
                                                                                                                                                                        var header = WireEncoding.GetBytes($"{type}:{body.Length}\n");
                                                                                                                                                                        // Class: GhostMessage (from 5 sources)
                                                                                                                                                                        public partial class GhostMessage
                                                                                                                                                                        {
                                                                                                                                                                            // --- Properties ---
                                                                                                                                                                            public string Type { get; set; }

                                                                                                                                                                            public string SessionID { get; set; }

                                                                                                                                                                            public string OperatorName { get; set; }

                                                                                                                                                                            public int FrameSize { get; set; }

                                                                                                                                                                            public byte[] FrameData { get; set; }


                                                                                                                                                                        }
                                                                                                                                                                    // Class: GhostMessage (from 2 sources)
                                                                                                                                                                    public partial class GhostMessage
                                                                                                                                                                    {
                                                                                                                                                                        // --- Properties ---
                                                                                                                                                                        public string Type { get; set; }

                                                                                                                                                                        public string SessionID { get; set; }

                                                                                                                                                                        public string OperatorName { get; set; }

                                                                                                                                                                        public int FrameSize { get; set; }

                                                                                                                                                                        public byte[] FrameData { get; set; }


                                                                                                                                                                    }
                                                                                                                                                                // ═══ Class: GhostMessage (from 4 sources) ═══
                                                                                                                                                                public partial class GhostMessage
                                                                                                                                                                {
                                                                                                                                                                    // --- Properties ---
                                                                                                                                                                    public string Type { get; set; }

                                                                                                                                                                    public string SessionID { get; set; }

                                                                                                                                                                    public string OperatorName { get; set; }

                                                                                                                                                                    public int FrameSize { get; set; }

                                                                                                                                                                    public byte[] FrameData { get; set; }


                                                                                                                                                                }
                                                                                                                                                            // Class: GhostRemoteEngine (from 2 sources)
                                                                                                                                                            public partial class GhostRemoteEngine
                                                                                                                                                            {
                                                                                                                                                                // --- Constants & Fields ---
                                                                                                                                                                private GhostSession _currentSession;

                                                                                                                                                                private bool _isStreaming;

                                                                                                                                                                private Thread _streamThread;

                                                                                                                                                                private int _frameIntervalMs = 1000; // كل ثانية

                                                                                                                                                                private int _jpegQuality = 40;       // جودة منخفضة للسرعة


                                                                                                                                                                // --- Properties ---
                                                                                                                                                                public bool IsActive => _currentSession != null && _currentSession.Status == GhostSessionStatus.Active;

                                                                                                                                                                public GhostSession CurrentSession => _currentSession;

                                                                                                                                                                public int FrameInterval { get => _frameIntervalMs; set => _frameIntervalMs = Math.Max(200, value); }

                                                                                                                                                                public int JpegQuality { get => _jpegQuality; set => _jpegQuality = Math.Max(10, Math.Min(100, value)); }


                                                                                                                                                                // --- Methods ---
                                                                                                                                                                private readonly object _lock = new object();

                                                                                                                                                                private Size _captureSize = new Size(1024, 768);

                                                                                                                                                                public void StartClientStreaming(string sessionId)
                                                                                                                                                                {
                                                                                                                                                                    if (_isStreaming) return;
                                                                                                                                                                    _isStreaming = true;

                                                                                                                                                                    _currentSession = new GhostSession
                                                                                                                                                                    {
                                                                                                                                                                        SessionID = sessionId,
                                                                                                                                                                        StartTime = DateTime.Now,
                                                                                                                                                                        Status = GhostSessionStatus.Active,
                                                                                                                                                                        IsViewOnly = true,
                                                                                                                                                                        ATMUnaffected = true,
                                                                                                                                                                        NoLogout = true,
                                                                                                                                                                        ScreenNotLocked = true
                                                                                                                                                                    };

                                                                                                                                                                _streamThread = new Thread(CaptureLoop) { IsBackground = true, Name = "GhostCaptureThread" };
                                                                                                                                                            _streamThread.Start();

                                                                                                                                                            OnLog?.Invoke("[Ghost] Client streaming started - View Only mode (ATM unaffected)");
                                                                                                                                                            OnSessionStarted?.Invoke(_currentSession);
                                                                                                                                                        }

                                                                                                                                                    public void StopClientStreaming()
                                                                                                                                                    {
                                                                                                                                                        _isStreaming = false;
                                                                                                                                                        if (_currentSession != null)
                                                                                                                                                        {
                                                                                                                                                            _currentSession.Status = GhostSessionStatus.Disconnected;
                                                                                                                                                            _currentSession.EndTime = DateTime.Now;
                                                                                                                                                            OnSessionEnded?.Invoke(_currentSession);
                                                                                                                                                        }
                                                                                                                                                    OnLog?.Invoke("[Ghost] Client streaming stopped");
                                                                                                                                                }

                                                                                                                                            public byte[] CaptureFrame()
                                                                                                                                            {
                                                                                                                                                try
                                                                                                                                                {
                                                                                                                                                    var bounds = System.Windows.Forms.Screen.PrimaryScreen.Bounds;
                                                                                                                                                    using (var bmp = new Bitmap(bounds.Width, bounds.Height, PixelFormat.Format24bppRgb))
                                                                                                                                                    {
                                                                                                                                                        using (var g = Graphics.FromImage(bmp))
                                                                                                                                                        {
                                                                                                                                                            g.CopyFromScreen(bounds.Location, Point.Empty, bounds.Size, CopyPixelOperation.SourceCopy);
                                                                                                                                                        }

                                                                                                                                                    // تصغير الحجم للإرسال السريع
                                                                                                                                                    using (var resized = new Bitmap(bmp, _captureSize))
                                                                                                                                                    using (var ms = new MemoryStream())
                                                                                                                                                    {
                                                                                                                                                        var encoder = GetEncoder(ImageFormat.Jpeg);
                                                                                                                                                        var encoderParams = new EncoderParameters(1);
                                                                                                                                                        encoderParams.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, (long)_jpegQuality);
                                                                                                                                                        resized.Save(ms, encoder, encoderParams);
                                                                                                                                                        return ms.ToArray();
                                                                                                                                                    }
                                                                                                                                            }
                                                                                                                                    }
                                                                                                                                catch (Exception ex)
                                                                                                                                {
                                                                                                                                    OnError?.Invoke(ex);
                                                                                                                                    return null;
                                                                                                                                }
                                                                                                                        }

                                                                                                                    public GhostSession StartServerSession(string atmId, string operatorName)
                                                                                                                    {
                                                                                                                        _currentSession = new GhostSession
                                                                                                                        {
                                                                                                                            SessionID = Guid.NewGuid().ToString("N").Substring(0, 12),
                                                                                                                            ATM_ID = atmId,
                                                                                                                            OperatorName = operatorName,
                                                                                                                            StartTime = DateTime.Now,
                                                                                                                            Status = GhostSessionStatus.Connecting,
                                                                                                                            IsViewOnly = true,
                                                                                                                            ATMUnaffected = true,
                                                                                                                            NoLogout = true,
                                                                                                                            ScreenNotLocked = true
                                                                                                                        };

                                                                                                                    OnLog?.Invoke($"[Ghost] Server session started for ATM: {atmId} by {operatorName}");
                                                                                                                    OnSessionStarted?.Invoke(_currentSession);
                                                                                                                    return _currentSession;
                                                                                                                }

                                                                                                            public void EndServerSession()
                                                                                                            {
                                                                                                                if (_currentSession != null)
                                                                                                                {
                                                                                                                    _currentSession.Status = GhostSessionStatus.Disconnected;
                                                                                                                    _currentSession.EndTime = DateTime.Now;
                                                                                                                    _currentSession.ActivityLog.Add($"Session ended at {DateTime.Now:HH:mm:ss}");
                                                                                                                    OnSessionEnded?.Invoke(_currentSession);
                                                                                                                    OnLog?.Invoke($"[Ghost] Session ended: {_currentSession.SessionID}");
                                                                                                                }
                                                                                                            _currentSession = null;
                                                                                                        }

                                                                                                    public void ProcessReceivedFrame(byte[] frameData)
                                                                                                    {
                                                                                                        if (frameData == null || frameData.Length == 0) return;
                                                                                                        if (_currentSession != null)
                                                                                                        {
                                                                                                            _currentSession.Status = GhostSessionStatus.Active;
                                                                                                            _currentSession.ActivityLog.Add($"Frame received: {frameData.Length} bytes at {DateTime.Now:HH:mm:ss}");
                                                                                                        }
                                                                                                    OnFrameReceived?.Invoke(frameData);
                                                                                                }

                                                                                            public static byte[] BuildGhostStartRequest(string sessionId, string operatorName)
                                                                                            {
                                                                                                string msg = $"GHOST_START|{sessionId}|{operatorName}|{DateTime.Now:yyyy-MM-dd HH:mm:ss}";
                                                                                                return System.Text.Encoding.UTF8.GetBytes(msg);
                                                                                            }

                                                                                        public static byte[] BuildGhostStopRequest(string sessionId)
                                                                                        {
                                                                                            string msg = $"GHOST_STOP|{sessionId}|{DateTime.Now:yyyy-MM-dd HH:mm:ss}";
                                                                                            return System.Text.Encoding.UTF8.GetBytes(msg);
                                                                                        }

                                                                                    public static byte[] BuildFrameMessage(string sessionId, byte[] frameData)
                                                                                    {
                                                                                        string header = $"GHOST_FRAME|{sessionId}|{frameData.Length}|";
                                                                                        byte[] headerBytes = System.Text.Encoding.UTF8.GetBytes(header);
                                                                                        byte[] message = new byte[headerBytes.Length + frameData.Length];
                                                                                        Buffer.BlockCopy(headerBytes, 0, message, 0, headerBytes.Length);
                                                                                        Buffer.BlockCopy(frameData, 0, message, headerBytes.Length, frameData.Length);
                                                                                        return message;
                                                                                    }

                                                                                public static GhostMessage ParseGhostMessage(byte[] data)
                                                                                {
                                                                                    try
                                                                                    {
                                                                                        string text = System.Text.Encoding.UTF8.GetString(data);
                                                                                        string[] parts = text.Split('|');
                                                                                        if (parts.Length < 2) return null;

                                                                                        var msg = new GhostMessage { Type = parts[0], SessionID = parts[1] };
                                                                                        switch (parts[0])
                                                                                        {
                                                                                            case "GHOST_START":
                                                                                            msg.OperatorName = parts.Length > 2 ? parts[2] : "";
                                                                                            break;
                                                                                            case "GHOST_FRAME":
                                                                                            if (parts.Length > 2 && int.TryParse(parts[2], out int size))
                                                                                            {
                                                                                                msg.FrameSize = size;
                                                                                                // Frame data follows after header
                                                                                                int headerLen = System.Text.Encoding.UTF8.GetByteCount(string.Join("|", parts[0], parts[1], parts[2]) + "|");
                                                                                                if (data.Length > headerLen)
                                                                                                {
                                                                                                    msg.FrameData = new byte[data.Length - headerLen];
                                                                                                    Buffer.BlockCopy(data, headerLen, msg.FrameData, 0, msg.FrameData.Length);
                                                                                                }
                                                                                        }
                                                                                    break;
                                                                                }
                                                                            return msg;
                                                                        }
                                                                    catch { return null; }
                                                                }

                                                            private void CaptureLoop()
                                                            {
                                                                while (_isStreaming)
                                                                {
                                                                    try
                                                                    {
                                                                        byte[] frame = CaptureFrame();
                                                                        if (frame != null && frame.Length > 0)
                                                                        {
                                                                            OnFrameRequested?.Invoke(frame);
                                                                        }
                                                                }
                                                            catch (Exception ex)
                                                            {
                                                                OnError?.Invoke(ex);
                                                            }
                                                        Thread.Sleep(_frameIntervalMs);
                                                    }
                                            }

                                        private ImageCodecInfo GetEncoder(ImageFormat format)
                                        {
                                            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
                                            foreach (ImageCodecInfo codec in codecs)
                                            {
                                                if (codec.FormatID == format.Guid) return codec;
                                            }
                                        return null;
                                    }


                                // --- Events ---
                                public event Action<string> OnLog;

                                public event Action<byte[]> OnFrameReceived;      // Server side: frame from client

                                public event Action<byte[]> OnFrameRequested;     // Client side: send frame to server

                                public event Action<GhostSession> OnSessionStarted;

                                public event Action<GhostSession> OnSessionEnded;

                                public event Action<Exception> OnError;


                            }
                        /// <summary>
                        /// محرك الوصول عن بعد كشبح (Ghost/Shadow Remote Access)
                        /// الدخول كشبح بحيث:
                        /// 1. عدم تأثر الصراف
                        /// 2. عدم تسجيل الخروج
                        /// 3. عدم قفل شاشة الصراف على العميل
                        ///
                        /// يعمل بنظام التقاط الشاشة المستمر (Screen Streaming) بدون تدخل
                        /// </summary>
                        public class GhostRemoteEngine
                        {
#region Events
                            public event Action<string> OnLog;
                            public event Action<byte[]> OnFrameReceived;      // Server side: frame from client
                            public event Action<byte[]> OnFrameRequested;     // Client side: send frame to server
                            public event Action<GhostSession> OnSessionStarted;
                            public event Action<GhostSession> OnSessionEnded;
                            public event Action<Exception> OnError;
#endregion

#region Fields
                            private GhostSession _currentSession;
                            private bool _isStreaming;
                            private Thread _streamThread;
                            private readonly object _lock = new object();
                            private int _frameIntervalMs = 1000; // كل ثانية
                            private int _jpegQuality = 40;       // جودة منخفضة للسرعة
                            private Size _captureSize = new Size(1024, 768);
#endregion

#region Properties
                            public bool IsActive => _currentSession != null && _currentSession.Status == GhostSessionStatus.Active;
                            public GhostSession CurrentSession => _currentSession;
                            public int FrameInterval { get => _frameIntervalMs; set => _frameIntervalMs = Math.Max(200, value); }
                            public int JpegQuality { get => _jpegQuality; set => _jpegQuality = Math.Max(10, Math.Min(100, value)); }
#endregion

#region Client Side - Capture and Send
                            /// <summary>
                            /// بدء التقاط الشاشة وإرسالها (يعمل على الكلاينت)
                            /// لا يؤثر على الصراف - فقط التقاط بدون تدخل
                            /// </summary>
                            public void StartClientStreaming(string sessionId)
                            {
                                if (_isStreaming) return;
                                _isStreaming = true;

                                _currentSession = new GhostSession
                                {
                                    SessionID = sessionId,
                                    StartTime = DateTime.Now,
                                    Status = GhostSessionStatus.Active,
                                    IsViewOnly = true,
                                    ATMUnaffected = true,
                                    NoLogout = true,
                                    ScreenNotLocked = true
                                };

                            _streamThread = new Thread(CaptureLoop) { IsBackground = true, Name = "GhostCaptureThread" };
                        _streamThread.Start();

                        OnLog?.Invoke("[Ghost] Client streaming started - View Only mode (ATM unaffected)");
                        OnSessionStarted?.Invoke(_currentSession);
                    }

                /// <summary>
                /// إيقاف التقاط الشاشة
                /// </summary>
                public void StopClientStreaming()
                {
                    _isStreaming = false;
                    if (_currentSession != null)
                    {
                        _currentSession.Status = GhostSessionStatus.Disconnected;
                        _currentSession.EndTime = DateTime.Now;
                        OnSessionEnded?.Invoke(_currentSession);
                    }
                OnLog?.Invoke("[Ghost] Client streaming stopped");
            }

        /// <summary>
        /// التقاط إطار واحد من الشاشة
        /// لا يقفل الشاشة ولا يؤثر على العميل
        /// </summary>
        public byte[] CaptureFrame()
        {
            try
            {
                var bounds = System.Windows.Forms.Screen.PrimaryScreen.Bounds;
                using (var bmp = new Bitmap(bounds.Width, bounds.Height, PixelFormat.Format24bppRgb))
                {
                    using (var g = Graphics.FromImage(bmp))
                    {
                        g.CopyFromScreen(bounds.Location, Point.Empty, bounds.Size, CopyPixelOperation.SourceCopy);
                    }

                // تصغير الحجم للإرسال السريع
                using (var resized = new Bitmap(bmp, _captureSize))
                using (var ms = new MemoryStream())
                {
                    var encoder = GetEncoder(ImageFormat.Jpeg);
                    var encoderParams = new EncoderParameters(1);
                    encoderParams.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, (long)_jpegQuality);
                    resized.Save(ms, encoder, encoderParams);
                    return ms.ToArray();
                }
        }
}
catch (Exception ex)
{
    OnError?.Invoke(ex);
    return null;
}
}
#endregion

#region Server Side - Request and Display
/// <summary>
/// بدء جلسة مراقبة من السرفر
/// </summary>
public GhostSession StartServerSession(string atmId, string operatorName)
{
    _currentSession = new GhostSession
    {
        SessionID = Guid.NewGuid().ToString("N").Substring(0, 12),
        ATM_ID = atmId,
        OperatorName = operatorName,
        StartTime = DateTime.Now,
        Status = GhostSessionStatus.Connecting,
        IsViewOnly = true,
        ATMUnaffected = true,
        NoLogout = true,
        ScreenNotLocked = true
    };

OnLog?.Invoke($"[Ghost] Server session started for ATM: {atmId} by {operatorName}");
OnSessionStarted?.Invoke(_currentSession);
return _currentSession;
}

/// <summary>
/// إنهاء جلسة المراقبة
/// </summary>
public void EndServerSession()
{
    if (_currentSession != null)
    {
        _currentSession.Status = GhostSessionStatus.Disconnected;
        _currentSession.EndTime = DateTime.Now;
        _currentSession.ActivityLog.Add($"Session ended at {DateTime.Now:HH:mm:ss}");
        OnSessionEnded?.Invoke(_currentSession);
        OnLog?.Invoke($"[Ghost] Session ended: {_currentSession.SessionID}");
    }
_currentSession = null;
}

/// <summary>
/// معالجة إطار مستلم من الكلاينت
/// </summary>
public void ProcessReceivedFrame(byte[] frameData)
{
    if (frameData == null || frameData.Length == 0) return;
    if (_currentSession != null)
    {
        _currentSession.Status = GhostSessionStatus.Active;
        _currentSession.ActivityLog.Add($"Frame received: {frameData.Length} bytes at {DateTime.Now:HH:mm:ss}");
    }
OnFrameReceived?.Invoke(frameData);
}
#endregion

#region Protocol Messages
/// <summary>
/// بناء رسالة طلب بدء Ghost session
/// </summary>
public static byte[] BuildGhostStartRequest(string sessionId, string operatorName)
{
    string msg = $"GHOST_START|{sessionId}|{operatorName}|{DateTime.Now:yyyy-MM-dd HH:mm:ss}";
    return System.Text.Encoding.UTF8.GetBytes(msg);
}

/// <summary>
/// بناء رسالة إيقاف Ghost session
/// </summary>
public static byte[] BuildGhostStopRequest(string sessionId)
{
    string msg = $"GHOST_STOP|{sessionId}|{DateTime.Now:yyyy-MM-dd HH:mm:ss}";
    return System.Text.Encoding.UTF8.GetBytes(msg);
}

/// <summary>
/// بناء رسالة إطار (Frame)
/// </summary>
public static byte[] BuildFrameMessage(string sessionId, byte[] frameData)
{
    string header = $"GHOST_FRAME|{sessionId}|{frameData.Length}|";
    byte[] headerBytes = System.Text.Encoding.UTF8.GetBytes(header);
    byte[] message = new byte[headerBytes.Length + frameData.Length];
    Buffer.BlockCopy(headerBytes, 0, message, 0, headerBytes.Length);
    Buffer.BlockCopy(frameData, 0, message, headerBytes.Length, frameData.Length);
    return message;
}

/// <summary>
/// تحليل رسالة Ghost
/// </summary>
public static GhostMessage ParseGhostMessage(byte[] data)
{
    try
    {
        string text = System.Text.Encoding.UTF8.GetString(data);
        string[] parts = text.Split('|');
        if (parts.Length < 2) return null;

        var msg = new GhostMessage { Type = parts[0], SessionID = parts[1] };
        switch (parts[0])
        {
            case "GHOST_START":
            msg.OperatorName = parts.Length > 2 ? parts[2] : "";
            break;
            case "GHOST_FRAME":
            if (parts.Length > 2 && int.TryParse(parts[2], out int size))
            {
                msg.FrameSize = size;
                // Frame data follows after header
                int headerLen = System.Text.Encoding.UTF8.GetByteCount(string.Join("|", parts[0], parts[1], parts[2]) + "|");
                if (data.Length > headerLen)
                {
                    msg.FrameData = new byte[data.Length - headerLen];
                    Buffer.BlockCopy(data, headerLen, msg.FrameData, 0, msg.FrameData.Length);
                }
        }
    break;
}
return msg;
}
catch { return null; }
}
#endregion

#region Private Methods
private void CaptureLoop()
{
    while (_isStreaming)
    {
        try
        {
            byte[] frame = CaptureFrame();
            if (frame != null && frame.Length > 0)
            {
                OnFrameRequested?.Invoke(frame);
            }
    }
catch (Exception ex)
{
    OnError?.Invoke(ex);
}
Thread.Sleep(_frameIntervalMs);
}
}

private ImageCodecInfo GetEncoder(ImageFormat format)
{
    ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
    foreach (ImageCodecInfo codec in codecs)
    {
        if (codec.FormatID == format.Guid) return codec;
    }
return null;
}
#endregion
}
var separator = value.IndexOf('|');
var separator = header.IndexOf(':');
throw new InvalidDataException("Invalid EJLive frame header.");
/// <summary>
/// Stores and retrieves remote session audit records.
/// </summary>
public interface IRemoteSessionAuditStore
{
    Task RecordStartAsync(RemoteSessionAudit audit, CancellationToken cancellationToken = default);
    Task RecordStopAsync(RemoteSessionAudit audit, CancellationToken cancellationToken = default);
    bool HasExplicitConsentWaiver(RemoteSessionRequest request);
}
public partial interface IRemoteSessionAuditStore
{
    Task RecordStartAsync(RemoteSessionAudit audit, CancellationToken cancellationToken = default);
    Task RecordStopAsync(RemoteSessionAudit audit, CancellationToken cancellationToken = default);
    bool HasExplicitConsentWaiver(RemoteSessionRequest request);
}
/// <summary>
/// Notifies stakeholders of remote session lifecycle events.
/// </summary>
public interface IRemoteSessionNotifier
{
    Task NotifySessionStartedAsync(RemoteSessionAudit audit, CancellationToken cancellationToken = default);
    Task NotifySessionStoppedAsync(RemoteSessionAudit audit, CancellationToken cancellationToken = default);
}
public partial interface IRemoteSessionNotifier
{
    Task NotifySessionStartedAsync(RemoteSessionAudit audit, CancellationToken cancellationToken = default);
    Task NotifySessionStoppedAsync(RemoteSessionAudit audit, CancellationToken cancellationToken = default);
}
/// <summary>
/// Defines policy rules for remote sessions.
/// </summary>
public interface IRemoteSessionPolicy
{
    bool IsSessionTypeAllowed(RemoteSessionType sessionType);
    bool IsOperatorAllowed(string operatorId);
    bool AllowNoConsentPrompt(RemoteSessionType sessionType);
}
public partial interface IRemoteSessionPolicy
{
    bool IsSessionTypeAllowed(RemoteSessionType sessionType);
    bool IsOperatorAllowed(string operatorId);
    bool AllowNoConsentPrompt(RemoteSessionType sessionType);
}
return !string.IsNullOrWhiteSpace(atmId);
// Enum: MsgType (from 2 sources)
public partial enum MsgType
{
    // --- Constants & Fields ---
    payload = string.Empty;

    if (string.IsNullOrWhiteSpace(value))
    return false;

    if (separator < 0)
    atmId = value;

    if (separator >= 0)
    var tail = value[(separator + 1)..].Trim();

    if (tail.StartsWith("{", StringComparison.Ordinal))
    jsonCandidate = tail;

    if (!jsonCandidate.StartsWith("{", StringComparison.Ordinal))
    try
    if (document.RootElement.ValueKind != JsonValueKind.Object)
    var root = document.RootElement;

    var serverTime = DateTime.UtcNow;

    var pendingCount = 0;

    var requestImmediateSync = false;

    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\GhostRemoteEngine.cs.v21_bak
    if (DateTime.TryParse(serverTimeText, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var parsedServerTime))
    serverTime = parsedServerTime.ToUniversalTime();

    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\GhostRemoteEngine.cs.v21_bak
    if (int.TryParse(pendingText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedPending))
    pendingCount = Math.Max(0, parsedPending);

    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\GhostRemoteEngine.cs.v21_bak
    if (bool.TryParse(immediateText, out var parsedImmediate))
    requestImmediateSync = parsedImmediate;

    return true;

    catch (JsonException)
    public static byte[] BuildStartFile(string atmId, string fileName, long length, long offset, string checksum) => BuildFrame(MsgType.StartFile, $"{atmId}|{fileName}|{length}|{offset}|{checksum}");

    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\GhostRemoteEngine.cs.v21_bak
    ```

    var frame = new byte[header.Length + body.Length];

    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\GhostRemoteEngine.cs.v21_bak
    return frame;

    var typeName = header[..separator];

    var lengthText = header[(separator + 1)..];

    if (!Enum.TryParse<MsgType>(typeName, out var type))
    type = MsgType.Unknown;

    return null;

    value = default;

    while (true)
    var next = stream.ReadByte();

    if (next == '\n')
    break;

    var offset = 0;

    while (offset < length)
    var read = stream.Read(buffer, offset, length - offset);

    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\GhostRemoteEngine.cs.v21_bak
    offset += read;

    return buffer;

    if (string.IsNullOrWhiteSpace(IssuedAtUtc))
    IssuedAtUtc = DateTime.UtcNow.ToString("O");

    if (string.IsNullOrWhiteSpace(Nonce))
    Nonce = Guid.NewGuid().ToString("N");

    SignatureVersion = CommandSigningEngine.SignatureVersion;

    var payloadBase64 = string.Empty;

    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\GhostRemoteEngine.cs.v21_bak
    catch (FormatException)
    command.Payload = payloadBase64;

    if (!CommandSigningEngine.IsFresh(command.IssuedAtUtc, out var freshnessReason))
    command.SignatureFailureReason = freshnessReason;

    if (!CommandSigningEngine.VerifyCanonical(canonical, command.Signature, out var verifyReason))
    command.SignatureFailureReason = verifyReason;

    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\GhostRemoteEngine.cs.v21_bak
    command.SignatureVerified = true;

    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\GhostRemoteEngine.cs.v21_bak
    command.SignatureVerified = false;

    private readonly string _encryptionIV;

    private readonly bool _useEncryption;

    private readonly bool _useCompression;

    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\GhostRemoteEngine.cs.v21_bak
    _encryptionIV = SecurityConfig.DEFAULT_IV;

    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\GhostRemoteEngine.cs.v21_bak
    _useEncryption = useEncryption;

    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\GhostRemoteEngine.cs.v21_bak
    _useCompression = useCompression;

    string fullMessage = header + "\n" + data + Protocol.DATA_END;

    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\GhostRemoteEngine.cs.v21_bak
    string fullMessage = header + "\n" + base64Data + Protocol.DATA_END;

    if (_useEncryption && data.Length > 0)
    data = Decrypt(data);

    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\GhostRemoteEngine.cs.v21_bak
    return data;

    if (string.IsNullOrEmpty(message)) return info;

    if (body.EndsWith(Protocol.DATA_END.Trim()))
    body = body.Substring(0, body.Length - Protocol.DATA_END.Trim().Length);

    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\GhostRemoteEngine.cs.v21_bak
    return info;

    info.RawParts = parts;

    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\GhostRemoteEngine.cs.v21_bak
    info.Body = body;

    for (int i = 0; i < data.Length; i++)

    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\GhostRemoteEngine.cs.before_unify
    if (DateTime.TryParse(serverTimeText, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var parsedServerTime))
    serverTime = parsedServerTime.ToUniversalTime();

    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\GhostRemoteEngine.cs.before_unify
    if (int.TryParse(pendingText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedPending))
    pendingCount = Math.Max(0, parsedPending);

    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\GhostRemoteEngine.cs.before_unify
    if (bool.TryParse(immediateText, out var parsedImmediate))
    requestImmediateSync = parsedImmediate;

    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\GhostRemoteEngine.cs.before_unify
    checksum ^= data[i];

    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\GhostRemoteEngine.cs.before_unify
    return frame;

    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\GhostRemoteEngine.cs.before_unify
    offset += read;

    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\GhostRemoteEngine.cs.before_unify
    catch (FormatException)
    command.Payload = payloadBase64;

    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\GhostRemoteEngine.cs.before_unify
    command.SignatureVerified = true;

    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\GhostRemoteEngine.cs.before_unify
    command.SignatureVerified = false;

    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\GhostRemoteEngine.cs.before_unify
    _encryptionIV = SecurityConfig.DEFAULT_IV;

    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\GhostRemoteEngine.cs.before_unify
    _useEncryption = useEncryption;

    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\GhostRemoteEngine.cs.before_unify
    _useCompression = useCompression;

    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\GhostRemoteEngine.cs.before_unify
    string fullMessage = header + "\n" + base64Data + Protocol.DATA_END;

    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\GhostRemoteEngine.cs.before_unify
    return data;

    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\GhostRemoteEngine.cs.before_unify
    return info;

    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Engine\GhostRemoteEngine.cs.before_unify
    info.Body = body;


    // --- Properties ---
    public string CommandType { get; set; } = string.Empty;

    public string Payload { get; set; } = string.Empty;

    public bool RequiresConfirmation { get; set; }

    public string IssuedAtUtc { get; set; } = DateTime.UtcNow.ToString("O");

    public string Nonce { get; set; } = Guid.NewGuid().ToString("N");

    public string Signature { get; set; } = string.Empty;

    public string SignatureVersion { get; set; } = CommandSigningEngine.SignatureVersion;

    public bool SignatureVerified { get; private set; }

    public string SignatureFailureReason { get; private set; } = string.Empty;

    public string ATM_ID { get; set; }

    public string ATM_Type { get; set; }

    public string Version { get; set; }

    public string FileName { get; set; }

    public int DataLength { get; set; }

    public string Checksum { get; set; }

    public string Body { get; set; }

    public byte[] BinaryData { get; set; }

    public string CommandId { get; set; }

    public bool Success { get; set; }

    public string ResultData { get; set; }

    public string Status { get; set; }

    public string CSCStatus { get; set; }

    public string[] RawParts { get; set; }


    // --- Methods ---
    public static byte[] BuildHandshakeAck(string sessionId) => BuildFrame(MsgType.HandshakeAck, $"OK|{sessionId}");

    public static byte[] BuildHeartbeat(string atmId, string payload = "") => BuildFrame(MsgType.Heartbeat, $"{atmId}|{payload}");

    public static byte[] BuildHeartbeatAck(string atmId = "") => BuildFrame(MsgType.HeartbeatAck, atmId);

    public static byte[] BuildHeartbeatAck(
    string atmId,
    int pendingCommandCount,
    bool requestImmediateSync = false,
    string? serverMessage = null)
    var payload = JsonSerializer.Serialize(new;
    ATM_ID = atmId,
    ServerTimeUtc = serverTimeUtc.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture),
    PendingCommandCount = Math.Max(0, pendingCommandCount),
    RequestImmediateSync = requestImmediateSync,
    ServerMessage = serverMessage
}
var body = ReadExact(stream, length);
public static EJMessage ReadMessage(Stream stream, byte[]? sessionKey = null)
var header = ReadHeader(stream);
public sealed class RemoteAssistanceEngine : IDisposable
{
    private readonly List<RemoteAssistanceSession> _sessions = new();
    private readonly object _lock = new();

    public event EventHandler<RemoteAssistanceSession>? SessionStarted;
    public event EventHandler<RemoteAssistanceSession>? SessionEnded;
    public event EventHandler<string>? OnLog;

    public bool IsActive => _sessions.Count > 0;

    public RemoteAssistanceSession? StartSession(string atmId, string operatorName, bool viewOnly)
    {
        var session = new RemoteAssistanceSession;
        {
            SessionID = Guid.NewGuid().ToString("N"),
            AtmId = atmId,
            OperatorName = operatorName,
            ViewOnly = viewOnly,
            StartedAtUtc = DateTime.UtcNow
        };

    lock (_lock) { _sessions.Add(session); }
    SessionStarted?.Invoke(this, session);
    Log($"Remote session started: {session.SessionID} ({atmId})");
    return session;
}

public void EndSession(string sessionId)
{
    lock (_lock)
    {
        var session = _sessions.FirstOrDefault(s => s.SessionID == sessionId);
        if (session != null)
        {
            session.EndedAtUtc = DateTime.UtcNow;
            _sessions.Remove(session);
            SessionEnded?.Invoke(this, session);
            Log($"Remote session ended: {sessionId}");
        }
}
}

public IReadOnlyList<RemoteAssistanceSession> GetActiveSessions()
{
    lock (_lock) { return _sessions.ToArray(); }
}

private void Log(string msg) => OnLog?.Invoke(this, $"[GhostRemote] {msg}");
public void Dispose() { StopAllSessions(); }

private void StopAllSessions()
{
    lock (_lock)
    {
        foreach (var s in _sessions)
        s.EndedAtUtc = DateTime.UtcNow;
        _sessions.Clear();
    }
}
}
public partial class RemoteAssistanceEngine : IDisposable
{
    private readonly List<RemoteAssistanceSession> _sessions = new();
    private readonly object _lock = new();
    public bool IsActive => _sessions.Count > 0;
    public RemoteAssistanceSession? StartSession(string atmId, string operatorName, bool viewOnly)
    {
        var session = new RemoteAssistanceSession;
        {
            SessionID = Guid.NewGuid().ToString("N"),
            AtmId = atmId,
            OperatorName = operatorName,
            ViewOnly = viewOnly,
            StartedAtUtc = DateTime.UtcNow
        };
    lock (_lock) { _sessions.Add(session); }
    SessionStarted?.Invoke(this, session);
    Log($"Remote session started: {session.SessionID} ({atmId})");
    return session;
}
public void EndSession(string sessionId)
{
    lock (_lock)
    {
        var session = _sessions.FirstOrDefault(s => s.SessionID == sessionId);
        if (session != null)
        {
            session.EndedAtUtc = DateTime.UtcNow;
            _sessions.Remove(session);
            SessionEnded?.Invoke(this, session);
            Log($"Remote session ended: {sessionId}");
        }
}
}
public IReadOnlyList<RemoteAssistanceSession> GetActiveSessions()
{
    lock (_lock) { return _sessions.ToArray(); }
}
private void Log(string msg) => OnLog?.Invoke(this, $"[GhostRemote] {msg}");
public void Dispose() { StopAllSessions(); }
private void StopAllSessions()
{
    lock (_lock)
    {
        foreach (var s in _sessions)
        s.EndedAtUtc = DateTime.UtcNow;
        _sessions.Clear();
    }
}
public event EventHandler<RemoteAssistanceSession>? SessionStarted;
public event EventHandler<RemoteAssistanceSession>? SessionEnded;
public event EventHandler<string>? OnLog;
}
var value = (text ?? string.Empty).Trim();
atmId = value[..separator].Trim();
payload = value[(separator + 1)..].Trim();
public static bool TryParseHeartbeatAck(string text, out HeartbeatAck ack)
ack = new HeartbeatAck(
var jsonCandidate = value;


var serverMessage = ReadJsonValue(root, "ServerMessage", "message", "detail");


var serverTimeText = ReadJsonValue(root, "ServerTimeUtc", "serverTimeUtc", "serverTime", "timestampUtc");


var pendingText = ReadJsonValue(root, "PendingCommandCount", "CommandsPendingCount", "PendingCommands", "pending");


var immediateText = ReadJsonValue(root, "RequestImmediateSync", "ImmediateSync", "requestImmediateSync");


public static byte[] BuildChunk(int sequence, byte[] chunk, byte[]? sessionKey = null) => BuildFrame(MsgType.Chunk, sequence.ToString(), chunk, sessionKey);
public static bool TryParseHeartbeatMessage(string text, out string atmId, out string payload)
atmId = string.Empty;
}


{
    // Class: GhostRemote2Service (from 3 sources)
    public sealed partial class GhostRemote2Service
    {
        // --- Properties ---
        private static readonly Dictionary<string, (string FileName, string Arguments)> PresetCommands =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["PING_LOCAL"] = ("cmd.exe", "/c ping -n 1 127.0.0.1"),
            ["QUSER"] = ("quser.exe", string.Empty),
            ["WHOAMI"] = ("whoami.exe", "/all"),
            ["HOSTNAME"] = ("hostname.exe", string.Empty),
            ["W32TM_STATUS"] = ("w32tm.exe", "/query /status"),
            ["TERMSERVICE_STATE"] = ("sc.exe", "query TermService"),
            ["FIREWALL_STATE"] = ("netsh.exe", "advfirewall show allprofiles state")
        };

    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\Ejlive\EJLive.Unified\src\EJLive.Core\Services\GhostRemote2Service.cs
    public GhostRemoteCommandResult ExecuteSilentShell(string preset, int timeoutMs = 12000)
    {
        if (string.IsNullOrWhiteSpace(preset))
        return GhostRemoteCommandResult.Rejected("Preset command is required.");

        if (!PresetCommands.TryGetValue(preset.Trim(), out var command))
        return GhostRemoteCommandResult.Rejected("Preset is not allowed by policy allowlist.");

        return ExecuteHidden(command.FileName, command.Arguments, timeoutMs);
    }

// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive.Unified\EJLive.Unified\src\EJLive.Core\Services\GhostRemote2Service.cs
public GhostRemoteCommandResult ExecuteSilentShell(string preset, int timeoutMs = 12000)
{
    if (string.IsNullOrWhiteSpace(preset))
    return GhostRemoteCommandResult.Rejected("Preset command is required.");

    if (!PresetCommands.TryGetValue(preset.Trim(), out var command))
    return GhostRemoteCommandResult.Rejected("Preset is not allowed by policy allowlist.");

    return ExecuteHidden(command.FileName, command.Arguments, timeoutMs);
}

// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive.Unified.Enhanced\EJLive.Unified\src\EJLive.Core\Services\GhostRemote2Service.cs
public GhostRemoteCommandResult ExecuteSilentShell(string preset, int timeoutMs = 12000)
{
    if (string.IsNullOrWhiteSpace(preset))
    return GhostRemoteCommandResult.Rejected("Preset command is required.");

    if (!PresetCommands.TryGetValue(preset.Trim(), out var command))
    return GhostRemoteCommandResult.Rejected("Preset is not allowed by policy allowlist.");

    return ExecuteHidden(command.FileName, command.Arguments, timeoutMs);
}


// --- Methods ---
private static GhostRemoteCommandResult ExecuteHidden(string fileName, string arguments, int timeoutMs)
{
    try
    {
        var psi = new ProcessStartInfo;
        {
            FileName = fileName,
            Arguments = arguments,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

    using var process = Process.Start(psi);
    if (process == null)
    return GhostRemoteCommandResult.Failed("Process failed to start.");

    if (!process.WaitForExit(Math.Clamp(timeoutMs, 1000, 60000)))
    {
        try { process.Kill(); } catch { }
        return GhostRemoteCommandResult.Failed("Process timed out.");
    }

var output = (process.StandardOutput.ReadToEnd() + Environment.NewLine + process.StandardError.ReadToEnd()).Trim();
if (output.Length > 2000)
output = output[..2000];

return process.ExitCode == 0
? GhostRemoteCommandResult.Successful(output)
: GhostRemoteCommandResult.Failed($"ExitCode={process.ExitCode}; Output={output}");
}
catch (Exception ex)
{
    return GhostRemoteCommandResult.Failed(ex.Message);
}
}

}
/// <summary>
/// Compatibility-safe replacement for legacy "hidden terminal" helpers.
/// Executes only approved diagnostic commands in hidden mode with output capture.
/// </summary>
public sealed class GhostRemote2Service
{
    private static readonly Dictionary<string, (string FileName, string Arguments)> PresetCommands =
    new(StringComparer.OrdinalIgnoreCase)
    {
        ["PING_LOCAL"] = ("cmd.exe", "/c ping -n 1 127.0.0.1"),
        ["QUSER"] = ("quser.exe", string.Empty),
        ["WHOAMI"] = ("whoami.exe", "/all"),
        ["HOSTNAME"] = ("hostname.exe", string.Empty),
        ["W32TM_STATUS"] = ("w32tm.exe", "/query /status"),
        ["TERMSERVICE_STATE"] = ("sc.exe", "query TermService"),
        ["FIREWALL_STATE"] = ("netsh.exe", "advfirewall show allprofiles state")
    };

public GhostRemoteCommandResult ExecuteSilentShell(string preset, int timeoutMs = 12000)
{
    if (string.IsNullOrWhiteSpace(preset))
    return GhostRemoteCommandResult.Rejected("Preset command is required.");

    if (!PresetCommands.TryGetValue(preset.Trim(), out var command))
    return GhostRemoteCommandResult.Rejected("Preset is not allowed by policy allowlist.");

    return ExecuteHidden(command.FileName, command.Arguments, timeoutMs);
}

private static GhostRemoteCommandResult ExecuteHidden(string fileName, string arguments, int timeoutMs)
{
    try
    {
        var psi = new ProcessStartInfo;
        {
            FileName = fileName,
            Arguments = arguments,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

    using var process = Process.Start(psi);
    if (process == null)
    return GhostRemoteCommandResult.Failed("Process failed to start.");

    if (!process.WaitForExit(Math.Clamp(timeoutMs, 1000, 60000)))
    {
        try { process.Kill(); } catch { }
        return GhostRemoteCommandResult.Failed("Process timed out.");
    }

var output = (process.StandardOutput.ReadToEnd() + Environment.NewLine + process.StandardError.ReadToEnd()).Trim();
if (output.Length > 2000)
output = output[..2000];

return process.ExitCode == 0
? GhostRemoteCommandResult.Successful(output)
: GhostRemoteCommandResult.Failed($"ExitCode={process.ExitCode}; Output={output}");
}
catch (Exception ex)
{
    return GhostRemoteCommandResult.Failed(ex.Message);
}
}
}
public partial public public sealed class GhostRemote2Service
{
    private static readonly Dictionary<string, (string FileName, string Arguments)> PresetCommands =
    new(StringComparer.OrdinalIgnoreCase)
    {
        ["PING_LOCAL"] = ("cmd.exe", "/c ping -n 1 127.0.0.1"),
        ["QUSER"] = ("quser.exe", string.Empty),
        ["WHOAMI"] = ("whoami.exe", "/all"),
        ["HOSTNAME"] = ("hostname.exe", string.Empty),
        ["W32TM_STATUS"] = ("w32tm.exe", "/query /status"),
        ["TERMSERVICE_STATE"] = ("sc.exe", "query TermService"),
        ["FIREWALL_STATE"] = ("netsh.exe", "advfirewall show allprofiles state")
    };
public GhostRemoteCommandResult ExecuteSilentShell(string preset, int timeoutMs = 12000)
{
    private static GhostRemoteCommandResult ExecuteHidden(string fileName, string arguments, int timeoutMs)
    {
    }

}
public partial public class GhostRemote2Service
{
    private static readonly Dictionary<string, (string FileName, string Arguments)> PresetCommands =
    new(StringComparer.OrdinalIgnoreCase)
    {
        ["PING_LOCAL"] = ("cmd.exe", "/c ping -n 1 127.0.0.1"),
        ["QUSER"] = ("quser.exe", string.Empty),
        ["WHOAMI"] = ("whoami.exe", "/all"),
        ["HOSTNAME"] = ("hostname.exe", string.Empty),
        ["W32TM_STATUS"] = ("w32tm.exe", "/query /status"),
        ["TERMSERVICE_STATE"] = ("sc.exe", "query TermService"),
        ["FIREWALL_STATE"] = ("netsh.exe", "advfirewall show allprofiles state")
    };
public GhostRemoteCommandResult ExecuteSilentShell(string preset, int timeoutMs = 12000)
{
    private static GhostRemoteCommandResult ExecuteHidden(string fileName, string arguments, int timeoutMs)
    {
    }

}
public partial public sealed class GhostRemote2Service
{
    private static readonly Dictionary<string, (string FileName, string Arguments)> PresetCommands =
    new(StringComparer.OrdinalIgnoreCase)
    {
        ["PING_LOCAL"] = ("cmd.exe", "/c ping -n 1 127.0.0.1"),
        ["QUSER"] = ("quser.exe", string.Empty),
        ["WHOAMI"] = ("whoami.exe", "/all"),
        ["HOSTNAME"] = ("hostname.exe", string.Empty),
        ["W32TM_STATUS"] = ("w32tm.exe", "/query /status"),
        ["TERMSERVICE_STATE"] = ("sc.exe", "query TermService"),
        ["FIREWALL_STATE"] = ("netsh.exe", "advfirewall show allprofiles state")
    };
public GhostRemoteCommandResult ExecuteSilentShell(string preset, int timeoutMs = 12000)
{
    private static GhostRemoteCommandResult ExecuteHidden(string fileName, string arguments, int timeoutMs)
    {
    }

}
public partial class GhostRemote2Service
{
    private static readonly Dictionary<string, (string FileName, string Arguments)> PresetCommands =


    new(StringComparer.OrdinalIgnoreCase)


    // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Services\GhostRemote2Service.cs.before_unify
    ["FIREWALL_STATE"] = ("netsh.exe", "advfirewall show allprofiles state")


    // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Services\GhostRemote2Service.cs.v22_bak
    ["FIREWALL_STATE"] = ("netsh.exe", "advfirewall show allprofiles state")


    private static readonly Dictionary<string, (string FileName, string Arguments)> PresetCommands =
    new(StringComparer.OrdinalIgnoreCase)
    {
        ["PING_LOCAL"] = ("cmd.exe", "/c ping -n 1 127.0.0.1"),
        ["QUSER"] = ("quser.exe", string.Empty),
        ["WHOAMI"] = ("whoami.exe", "/all"),
        ["HOSTNAME"] = ("hostname.exe", string.Empty),
        ["W32TM_STATUS"] = ("w32tm.exe", "/query /status"),
        ["TERMSERVICE_STATE"] = ("sc.exe", "query TermService"),
        ["FIREWALL_STATE"] = ("netsh.exe", "advfirewall show allprofiles state")
    };


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\EJLive.Unified\src\EJLive.Core\Services\GhostRemote2Service.cs
public GhostRemoteCommandResult ExecuteSilentShell(string preset, int timeoutMs = 12000)
{
    if (string.IsNullOrWhiteSpace(preset))
    return GhostRemoteCommandResult.Rejected("Preset command is required.");

    if (!PresetCommands.TryGetValue(preset.Trim(), out var command))
    return GhostRemoteCommandResult.Rejected("Preset is not allowed by policy allowlist.");

    return ExecuteHidden(command.FileName, command.Arguments, timeoutMs);
}


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\src\EJLive.Core\Services\GhostRemote2Service.cs
public GhostRemoteCommandResult ExecuteSilentShell(string preset, int timeoutMs = 12000)
{
    if (string.IsNullOrWhiteSpace(preset))
    return GhostRemoteCommandResult.Rejected("Preset command is required.");

    if (!PresetCommands.TryGetValue(preset.Trim(), out var command))
    return GhostRemoteCommandResult.Rejected("Preset is not allowed by policy allowlist.");

    return ExecuteHidden(command.FileName, command.Arguments, timeoutMs);
}


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\GhostRemote2Service.cs
public GhostRemoteCommandResult ExecuteSilentShell(string preset, int timeoutMs = 12000)
{
    if (string.IsNullOrWhiteSpace(preset))
    return GhostRemoteCommandResult.Rejected("Preset command is required.");

    if (!PresetCommands.TryGetValue(preset.Trim(), out var command))
    return GhostRemoteCommandResult.Rejected("Preset is not allowed by policy allowlist.");

    return ExecuteHidden(command.FileName, command.Arguments, timeoutMs);
}


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\EJLive.Unified\src\EJLive.Core\Services\GhostRemote2Service.cs
public GhostRemoteCommandResult ExecuteSilentShell(string preset, int timeoutMs = 12000)
{
    if (string.IsNullOrWhiteSpace(preset))
    return GhostRemoteCommandResult.Rejected("Preset command is required.");

    if (!PresetCommands.TryGetValue(preset.Trim(), out var command))
    return GhostRemoteCommandResult.Rejected("Preset is not allowed by policy allowlist.");

    return ExecuteHidden(command.FileName, command.Arguments, timeoutMs);
}


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\src\EJLive.Core\Services\GhostRemote2Service.cs
public GhostRemoteCommandResult ExecuteSilentShell(string preset, int timeoutMs = 12000)
{
    if (string.IsNullOrWhiteSpace(preset))
    return GhostRemoteCommandResult.Rejected("Preset command is required.");

    if (!PresetCommands.TryGetValue(preset.Trim(), out var command))
    return GhostRemoteCommandResult.Rejected("Preset is not allowed by policy allowlist.");

    return ExecuteHidden(command.FileName, command.Arguments, timeoutMs);
}


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Services\GhostRemote2Service.cs
public GhostRemoteCommandResult ExecuteSilentShell(string preset, int timeoutMs = 12000)
{
    if (string.IsNullOrWhiteSpace(preset))
    return GhostRemoteCommandResult.Rejected("Preset command is required.");

    if (!PresetCommands.TryGetValue(preset.Trim(), out var command))
    return GhostRemoteCommandResult.Rejected("Preset is not allowed by policy allowlist.");

    return ExecuteHidden(command.FileName, command.Arguments, timeoutMs);
}


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Services\GhostRemote2Service.cs.v17_bak
public GhostRemoteCommandResult ExecuteSilentShell(string preset, int timeoutMs = 12000)
{
    if (string.IsNullOrWhiteSpace(preset))
    return GhostRemoteCommandResult.Rejected("Preset command is required.");

    if (!PresetCommands.TryGetValue(preset.Trim(), out var command))
    return GhostRemoteCommandResult.Rejected("Preset is not allowed by policy allowlist.");

    return ExecuteHidden(command.FileName, command.Arguments, timeoutMs);
}


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Services\GhostRemote2Service.cs.v20_bak
public GhostRemoteCommandResult ExecuteSilentShell(string preset, int timeoutMs = 12000)
{
    if (string.IsNullOrWhiteSpace(preset))
    return GhostRemoteCommandResult.Rejected("Preset command is required.");

    if (!PresetCommands.TryGetValue(preset.Trim(), out var command))
    return GhostRemoteCommandResult.Rejected("Preset is not allowed by policy allowlist.");

    return ExecuteHidden(command.FileName, command.Arguments, timeoutMs);
}


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-4\EJLive.Unified\src\EJLive.Core\Services\GhostRemote2Service.cs
public GhostRemoteCommandResult ExecuteSilentShell(string preset, int timeoutMs = 12000)
{
    if (string.IsNullOrWhiteSpace(preset))
    return GhostRemoteCommandResult.Rejected("Preset command is required.");

    if (!PresetCommands.TryGetValue(preset.Trim(), out var command))
    return GhostRemoteCommandResult.Rejected("Preset is not allowed by policy allowlist.");

    return ExecuteHidden(command.FileName, command.Arguments, timeoutMs);
}


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-5\EJLive.Unified\src\EJLive.Core\Services\GhostRemote2Service.cs
public GhostRemoteCommandResult ExecuteSilentShell(string preset, int timeoutMs = 12000)
{
    if (string.IsNullOrWhiteSpace(preset))
    return GhostRemoteCommandResult.Rejected("Preset command is required.");

    if (!PresetCommands.TryGetValue(preset.Trim(), out var command))
    return GhostRemoteCommandResult.Rejected("Preset is not allowed by policy allowlist.");

    return ExecuteHidden(command.FileName, command.Arguments, timeoutMs);
}


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-9\EJLive.Unified\src\EJLive.Core\Services\GhostRemote2Service.cs
public GhostRemoteCommandResult ExecuteSilentShell(string preset, int timeoutMs = 12000)
{
    if (string.IsNullOrWhiteSpace(preset))
    return GhostRemoteCommandResult.Rejected("Preset command is required.");

    if (!PresetCommands.TryGetValue(preset.Trim(), out var command))
    return GhostRemoteCommandResult.Rejected("Preset is not allowed by policy allowlist.");

    return ExecuteHidden(command.FileName, command.Arguments, timeoutMs);
}


private static GhostRemoteCommandResult ExecuteHidden(string fileName, string arguments, int timeoutMs)
{
    try
    {
        var psi = new ProcessStartInfo;
        {
            FileName = fileName,
            Arguments = arguments,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

    using var process = Process.Start(psi);
    if (process == null)
    return GhostRemoteCommandResult.Failed("Process failed to start.");

    if (!process.WaitForExit(Math.Clamp(timeoutMs, 1000, 60000)))
    {
        try { process.Kill(); } catch { }
        return GhostRemoteCommandResult.Failed("Process timed out.");
    }

var output = (process.StandardOutput.ReadToEnd() + Environment.NewLine + process.StandardError.ReadToEnd()).Trim();
if (output.Length > 2000)
output = output[..2000];

return process.ExitCode == 0
? GhostRemoteCommandResult.Successful(output)
: GhostRemoteCommandResult.Failed($"ExitCode={process.ExitCode}; Output={output}");
}
catch (Exception ex)
{
    return GhostRemoteCommandResult.Failed(ex.Message);
}
}


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\src\EJLive.Core\Services\GhostRemote2Service.cs
private static GhostRemoteCommandResult ExecuteHidden(string fileName, string arguments, int timeoutMs)
{
    try
    {
        var psi = new ProcessStartInfo;
        {
            FileName = fileName,
            Arguments = arguments,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

    if (process == null)
    return GhostRemoteCommandResult.Failed("Process failed to start.");

    if (!process.WaitForExit(Math.Clamp(timeoutMs, 1000, 60000)))
    {
        try { process.Kill(); } catch { }
        return GhostRemoteCommandResult.Failed("Process timed out.");
    }

var output = (process.StandardOutput.ReadToEnd() + Environment.NewLine + process.StandardError.ReadToEnd()).Trim();
if (output.Length > 2000)
output = output[..2000];

return process.ExitCode == 0
? GhostRemoteCommandResult.Successful(output)
: GhostRemoteCommandResult.Failed($"ExitCode={process.ExitCode}; Output={output}");
}
catch (Exception ex)
{
    return GhostRemoteCommandResult.Failed(ex.Message);
}
}


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\GhostRemote2Service.cs
private static GhostRemoteCommandResult ExecuteHidden(string fileName, string arguments, int timeoutMs)
{
    try
    {
        var psi = new ProcessStartInfo;
        {
            FileName = fileName,
            Arguments = arguments,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

    if (process == null)
    return GhostRemoteCommandResult.Failed("Process failed to start.");

    if (!process.WaitForExit(Math.Clamp(timeoutMs, 1000, 60000)))
    {
        try { process.Kill(); } catch { }
        return GhostRemoteCommandResult.Failed("Process timed out.");
    }

var output = (process.StandardOutput.ReadToEnd() + Environment.NewLine + process.StandardError.ReadToEnd()).Trim();
if (output.Length > 2000)
output = output[..2000];

return process.ExitCode == 0
? GhostRemoteCommandResult.Successful(output)
: GhostRemoteCommandResult.Failed($"ExitCode={process.ExitCode}; Output={output}");
}
catch (Exception ex)
{
    return GhostRemoteCommandResult.Failed(ex.Message);
}
}


// Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\src\EJLive.Core\Services\GhostRemote2Service.cs
private static GhostRemoteCommandResult ExecuteHidden(string fileName, string arguments, int timeoutMs)
{
    try
    {
        var psi = new ProcessStartInfo;
        {
            FileName = fileName,
            Arguments = arguments,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

    if (process == null)
    return GhostRemoteCommandResult.Failed("Process failed to start.");

    if (!process.WaitForExit(Math.Clamp(timeoutMs, 1000, 60000)))
    {
        try { process.Kill(); } catch { }
        return GhostRemoteCommandResult.Failed("Process timed out.");
    }

var output = (process.StandardOutput.ReadToEnd() + Environment.NewLine + process.StandardError.ReadToEnd()).Trim();
if (output.Length > 2000)
output = output[..2000];

return process.ExitCode == 0
? GhostRemoteCommandResult.Successful(output)
: GhostRemoteCommandResult.Failed($"ExitCode={process.ExitCode}; Output={output}");
}
catch (Exception ex)
{
    return GhostRemoteCommandResult.Failed(ex.Message);
}
}


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Services\GhostRemote2Service.cs.v20_bak
private static GhostRemoteCommandResult ExecuteHidden(string fileName, string arguments, int timeoutMs)
{
    try
    {
        var psi = new ProcessStartInfo;
        {
            FileName = fileName,
            Arguments = arguments,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

    if (process == null)
    return GhostRemoteCommandResult.Failed("Process failed to start.");

    if (!process.WaitForExit(Math.Clamp(timeoutMs, 1000, 60000)))
    {
        try { process.Kill(); } catch { }
        return GhostRemoteCommandResult.Failed("Process timed out.");
    }

var output = (process.StandardOutput.ReadToEnd() + Environment.NewLine + process.StandardError.ReadToEnd()).Trim();
if (output.Length > 2000)
output = output[..2000];

return process.ExitCode == 0
? GhostRemoteCommandResult.Successful(output)
: GhostRemoteCommandResult.Failed($"ExitCode={process.ExitCode}; Output={output}");
}
catch (Exception ex)
{
    return GhostRemoteCommandResult.Failed(ex.Message);
}
}

}
public partial class GhostRemote2Service
{
    private static readonly Dictionary<string, (string FileName, string Arguments)> PresetCommands =


    new(StringComparer.OrdinalIgnoreCase)


    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Services\GhostRemote2Service.cs.v22_bak
    ["FIREWALL_STATE"] = ("netsh.exe", "advfirewall show allprofiles state")


    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Services\GhostRemote2Service.cs.before_unify
    ["FIREWALL_STATE"] = ("netsh.exe", "advfirewall show allprofiles state")


    private static readonly Dictionary<string, (string FileName, string Arguments)> PresetCommands =

    new(StringComparer.OrdinalIgnoreCase)

    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Services\GhostRemote2Service.cs.v22_bak
    ["FIREWALL_STATE"] = ("netsh.exe", "advfirewall show allprofiles state")

    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Services\GhostRemote2Service.cs.before_unify
    ["FIREWALL_STATE"] = ("netsh.exe", "advfirewall show allprofiles state")


    // --- Properties ---
    private static readonly Dictionary<string, (string FileName, string Arguments)> PresetCommands =
    new(StringComparer.OrdinalIgnoreCase)
    {
        ["PING_LOCAL"] = ("cmd.exe", "/c ping -n 1 127.0.0.1"),
        ["QUSER"] = ("quser.exe", string.Empty),
        ["WHOAMI"] = ("whoami.exe", "/all"),
        ["HOSTNAME"] = ("hostname.exe", string.Empty),
        ["W32TM_STATUS"] = ("w32tm.exe", "/query /status"),
        ["TERMSERVICE_STATE"] = ("sc.exe", "query TermService"),
        ["FIREWALL_STATE"] = ("netsh.exe", "advfirewall show allprofiles state")
    };


// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\src\EJLive.Core\Services\GhostRemote2Service.cs
public GhostRemoteCommandResult ExecuteSilentShell(string preset, int timeoutMs = 12000)
{
    if (string.IsNullOrWhiteSpace(preset))
    return GhostRemoteCommandResult.Rejected("Preset command is required.");

    if (!PresetCommands.TryGetValue(preset.Trim(), out var command))
    return GhostRemoteCommandResult.Rejected("Preset is not allowed by policy allowlist.");

    return ExecuteHidden(command.FileName, command.Arguments, timeoutMs);
}


// Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Services\GhostRemote2Service.cs.v20_bak
public GhostRemoteCommandResult ExecuteSilentShell(string preset, int timeoutMs = 12000)
{
    if (string.IsNullOrWhiteSpace(preset))
    return GhostRemoteCommandResult.Rejected("Preset command is required.");

    if (!PresetCommands.TryGetValue(preset.Trim(), out var command))
    return GhostRemoteCommandResult.Rejected("Preset is not allowed by policy allowlist.");

    return ExecuteHidden(command.FileName, command.Arguments, timeoutMs);
}


// Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Services\GhostRemote2Service.cs
public GhostRemoteCommandResult ExecuteSilentShell(string preset, int timeoutMs = 12000)
{
    if (string.IsNullOrWhiteSpace(preset))
    return GhostRemoteCommandResult.Rejected("Preset command is required.");

    if (!PresetCommands.TryGetValue(preset.Trim(), out var command))
    return GhostRemoteCommandResult.Rejected("Preset is not allowed by policy allowlist.");

    return ExecuteHidden(command.FileName, command.Arguments, timeoutMs);
}


// Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Services\GhostRemote2Service.cs.v17_bak
public GhostRemoteCommandResult ExecuteSilentShell(string preset, int timeoutMs = 12000)
{
    if (string.IsNullOrWhiteSpace(preset))
    return GhostRemoteCommandResult.Rejected("Preset command is required.");

    if (!PresetCommands.TryGetValue(preset.Trim(), out var command))
    return GhostRemoteCommandResult.Rejected("Preset is not allowed by policy allowlist.");

    return ExecuteHidden(command.FileName, command.Arguments, timeoutMs);
}


private static GhostRemoteCommandResult ExecuteHidden(string fileName, string arguments, int timeoutMs)
{
    try
    {
        var psi = new ProcessStartInfo;
        {
            FileName = fileName,
            Arguments = arguments,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

    if (process == null)
    return GhostRemoteCommandResult.Failed("Process failed to start.");

    if (!process.WaitForExit(Math.Clamp(timeoutMs, 1000, 60000)))
    {
        try { process.Kill(); } catch { }
        return GhostRemoteCommandResult.Failed("Process timed out.");
    }

var output = (process.StandardOutput.ReadToEnd() + Environment.NewLine + process.StandardError.ReadToEnd()).Trim();
if (output.Length > 2000)
output = output[..2000];

return process.ExitCode == 0
? GhostRemoteCommandResult.Successful(output)
: GhostRemoteCommandResult.Failed($"ExitCode={process.ExitCode}; Output={output}");
}
catch (Exception ex)
{
    return GhostRemoteCommandResult.Failed(ex.Message);
}
}


// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\src\EJLive.Core\Services\GhostRemote2Service.cs
private static GhostRemoteCommandResult ExecuteHidden(string fileName, string arguments, int timeoutMs)
{
    try
    {
        var psi = new ProcessStartInfo;
        {
            FileName = fileName,
            Arguments = arguments,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

    using var process = Process.Start(psi);
    if (process == null)
    return GhostRemoteCommandResult.Failed("Process failed to start.");

    if (!process.WaitForExit(Math.Clamp(timeoutMs, 1000, 60000)))
    {
        try { process.Kill(); } catch { }
        return GhostRemoteCommandResult.Failed("Process timed out.");
    }

var output = (process.StandardOutput.ReadToEnd() + Environment.NewLine + process.StandardError.ReadToEnd()).Trim();
if (output.Length > 2000)
output = output[..2000];

return process.ExitCode == 0
? GhostRemoteCommandResult.Successful(output)
: GhostRemoteCommandResult.Failed($"ExitCode={process.ExitCode}; Output={output}");
}
catch (Exception ex)
{
    return GhostRemoteCommandResult.Failed(ex.Message);
}
}


// Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Services\GhostRemote2Service.cs
private static GhostRemoteCommandResult ExecuteHidden(string fileName, string arguments, int timeoutMs)
{
    try
    {
        var psi = new ProcessStartInfo;
        {
            FileName = fileName,
            Arguments = arguments,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

    using var process = Process.Start(psi);
    if (process == null)
    return GhostRemoteCommandResult.Failed("Process failed to start.");

    if (!process.WaitForExit(Math.Clamp(timeoutMs, 1000, 60000)))
    {
        try { process.Kill(); } catch { }
        return GhostRemoteCommandResult.Failed("Process timed out.");
    }

var output = (process.StandardOutput.ReadToEnd() + Environment.NewLine + process.StandardError.ReadToEnd()).Trim();
if (output.Length > 2000)
output = output[..2000];

return process.ExitCode == 0
? GhostRemoteCommandResult.Successful(output)
: GhostRemoteCommandResult.Failed($"ExitCode={process.ExitCode}; Output={output}");
}
catch (Exception ex)
{
    return GhostRemoteCommandResult.Failed(ex.Message);
}
}


// Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Services\GhostRemote2Service.cs.v17_bak
private static GhostRemoteCommandResult ExecuteHidden(string fileName, string arguments, int timeoutMs)
{
    try
    {
        var psi = new ProcessStartInfo;
        {
            FileName = fileName,
            Arguments = arguments,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

    using var process = Process.Start(psi);
    if (process == null)
    return GhostRemoteCommandResult.Failed("Process failed to start.");

    if (!process.WaitForExit(Math.Clamp(timeoutMs, 1000, 60000)))
    {
        try { process.Kill(); } catch { }
        return GhostRemoteCommandResult.Failed("Process timed out.");
    }

var output = (process.StandardOutput.ReadToEnd() + Environment.NewLine + process.StandardError.ReadToEnd()).Trim();
if (output.Length > 2000)
output = output[..2000];

return process.ExitCode == 0
? GhostRemoteCommandResult.Successful(output)
: GhostRemoteCommandResult.Failed($"ExitCode={process.ExitCode}; Output={output}");
}
catch (Exception ex)
{
    return GhostRemoteCommandResult.Failed(ex.Message);
}
}

}
public partial class GhostRemote2Service
{
    private static readonly Dictionary<string, (string FileName, string Arguments)> PresetCommands =
    new(StringComparer.OrdinalIgnoreCase)
    {
        ["PING_LOCAL"] = ("cmd.exe", "/c ping -n 1 127.0.0.1"),
        ["QUSER"] = ("quser.exe", string.Empty),
        ["WHOAMI"] = ("whoami.exe", "/all"),
        ["HOSTNAME"] = ("hostname.exe", string.Empty),
        ["W32TM_STATUS"] = ("w32tm.exe", "/query /status"),
        ["TERMSERVICE_STATE"] = ("sc.exe", "query TermService"),
        ["FIREWALL_STATE"] = ("netsh.exe", "advfirewall show allprofiles state")
    };


// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\EJLive.Unified\src\EJLive.Core\Services\GhostRemote2Service.cs
public GhostRemoteCommandResult ExecuteSilentShell(string preset, int timeoutMs = 12000)
{
    if (string.IsNullOrWhiteSpace(preset))
    return GhostRemoteCommandResult.Rejected("Preset command is required.");

    if (!PresetCommands.TryGetValue(preset.Trim(), out var command))
    return GhostRemoteCommandResult.Rejected("Preset is not allowed by policy allowlist.");

    return ExecuteHidden(command.FileName, command.Arguments, timeoutMs);
}


// Variant from: d:\EJLIVE\EJlive_Reference_Projects\Ejlive\EJLive.Unified\src\EJLive.Core\Services\GhostRemote2Service.cs
public GhostRemoteCommandResult ExecuteSilentShell(string preset, int timeoutMs = 12000)
{
    if (string.IsNullOrWhiteSpace(preset))
    return GhostRemoteCommandResult.Rejected("Preset command is required.");

    if (!PresetCommands.TryGetValue(preset.Trim(), out var command))
    return GhostRemoteCommandResult.Rejected("Preset is not allowed by policy allowlist.");

    return ExecuteHidden(command.FileName, command.Arguments, timeoutMs);
}


// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive.Unified\EJLive.Unified\src\EJLive.Core\Services\GhostRemote2Service.cs
public GhostRemoteCommandResult ExecuteSilentShell(string preset, int timeoutMs = 12000)
{
    if (string.IsNullOrWhiteSpace(preset))
    return GhostRemoteCommandResult.Rejected("Preset command is required.");

    if (!PresetCommands.TryGetValue(preset.Trim(), out var command))
    return GhostRemoteCommandResult.Rejected("Preset is not allowed by policy allowlist.");

    return ExecuteHidden(command.FileName, command.Arguments, timeoutMs);
}


// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive.Unified.Enhanced\EJLive.Unified\src\EJLive.Core\Services\GhostRemote2Service.cs
public GhostRemoteCommandResult ExecuteSilentShell(string preset, int timeoutMs = 12000)
{
    if (string.IsNullOrWhiteSpace(preset))
    return GhostRemoteCommandResult.Rejected("Preset command is required.");

    if (!PresetCommands.TryGetValue(preset.Trim(), out var command))
    return GhostRemoteCommandResult.Rejected("Preset is not allowed by policy allowlist.");

    return ExecuteHidden(command.FileName, command.Arguments, timeoutMs);
}


private static GhostRemoteCommandResult ExecuteHidden(string fileName, string arguments, int timeoutMs)
{
    try
    {
        var psi = new ProcessStartInfo;
        {
            FileName = fileName,
            Arguments = arguments,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

    using var process = Process.Start(psi);
    if (process == null)
    return GhostRemoteCommandResult.Failed("Process failed to start.");

    if (!process.WaitForExit(Math.Clamp(timeoutMs, 1000, 60000)))
    {
        try { process.Kill(); } catch { }
        return GhostRemoteCommandResult.Failed("Process timed out.");
    }

var output = (process.StandardOutput.ReadToEnd() + Environment.NewLine + process.StandardError.ReadToEnd()).Trim();
if (output.Length > 2000)
output = output[..2000];

return process.ExitCode == 0
? GhostRemoteCommandResult.Successful(output)
: GhostRemoteCommandResult.Failed($"ExitCode={process.ExitCode}; Output={output}");
}
catch (Exception ex)
{
    return GhostRemoteCommandResult.Failed(ex.Message);
}
}

}
// Class: GhostRemote2Service (from 5 sources)
public sealed partial class GhostRemote2Service
{
    // --- Constants & Fields ---
    private static readonly Dictionary<string, (string FileName, string Arguments)> PresetCommands =

    new(StringComparer.OrdinalIgnoreCase)

    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Services\GhostRemote2Service.cs.v22_bak
    ["FIREWALL_STATE"] = ("netsh.exe", "advfirewall show allprofiles state")

    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Services\GhostRemote2Service.cs.before_unify
    ["FIREWALL_STATE"] = ("netsh.exe", "advfirewall show allprofiles state")


    // --- Properties ---
    private static readonly Dictionary<string, (string FileName, string Arguments)> PresetCommands =
    new(StringComparer.OrdinalIgnoreCase)
    {
        ["PING_LOCAL"] = ("cmd.exe", "/c ping -n 1 127.0.0.1"),
        ["QUSER"] = ("quser.exe", string.Empty),
        ["WHOAMI"] = ("whoami.exe", "/all"),
        ["HOSTNAME"] = ("hostname.exe", string.Empty),
        ["W32TM_STATUS"] = ("w32tm.exe", "/query /status"),
        ["TERMSERVICE_STATE"] = ("sc.exe", "query TermService"),
        ["FIREWALL_STATE"] = ("netsh.exe", "advfirewall show allprofiles state")
    };

// Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Services\GhostRemote2Service.cs.v20_bak
public GhostRemoteCommandResult ExecuteSilentShell(string preset, int timeoutMs = 12000)
{
    if (string.IsNullOrWhiteSpace(preset))
    return GhostRemoteCommandResult.Rejected("Preset command is required.");

    if (!PresetCommands.TryGetValue(preset.Trim(), out var command))
    return GhostRemoteCommandResult.Rejected("Preset is not allowed by policy allowlist.");

    return ExecuteHidden(command.FileName, command.Arguments, timeoutMs);
}

// Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Services\GhostRemote2Service.cs
public GhostRemoteCommandResult ExecuteSilentShell(string preset, int timeoutMs = 12000)
{
    if (string.IsNullOrWhiteSpace(preset))
    return GhostRemoteCommandResult.Rejected("Preset command is required.");

    if (!PresetCommands.TryGetValue(preset.Trim(), out var command))
    return GhostRemoteCommandResult.Rejected("Preset is not allowed by policy allowlist.");

    return ExecuteHidden(command.FileName, command.Arguments, timeoutMs);
}

// Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Services\GhostRemote2Service.cs.v17_bak
public GhostRemoteCommandResult ExecuteSilentShell(string preset, int timeoutMs = 12000)
{
    if (string.IsNullOrWhiteSpace(preset))
    return GhostRemoteCommandResult.Rejected("Preset command is required.");

    if (!PresetCommands.TryGetValue(preset.Trim(), out var command))
    return GhostRemoteCommandResult.Rejected("Preset is not allowed by policy allowlist.");

    return ExecuteHidden(command.FileName, command.Arguments, timeoutMs);
}


// --- Methods ---
private static GhostRemoteCommandResult ExecuteHidden(string fileName, string arguments, int timeoutMs)
{
    try
    {
        var psi = new ProcessStartInfo;
        {
            FileName = fileName,
            Arguments = arguments,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

    if (process == null)
    return GhostRemoteCommandResult.Failed("Process failed to start.");

    if (!process.WaitForExit(Math.Clamp(timeoutMs, 1000, 60000)))
    {
        try { process.Kill(); } catch { }
        return GhostRemoteCommandResult.Failed("Process timed out.");
    }

var output = (process.StandardOutput.ReadToEnd() + Environment.NewLine + process.StandardError.ReadToEnd()).Trim();
if (output.Length > 2000)
output = output[..2000];

return process.ExitCode == 0
? GhostRemoteCommandResult.Successful(output)
: GhostRemoteCommandResult.Failed($"ExitCode={process.ExitCode}; Output={output}");
}
catch (Exception ex)
{
    return GhostRemoteCommandResult.Failed(ex.Message);
}
}

// Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Services\GhostRemote2Service.cs
private static GhostRemoteCommandResult ExecuteHidden(string fileName, string arguments, int timeoutMs)
{
    try
    {
        var psi = new ProcessStartInfo;
        {
            FileName = fileName,
            Arguments = arguments,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

    using var process = Process.Start(psi);
    if (process == null)
    return GhostRemoteCommandResult.Failed("Process failed to start.");

    if (!process.WaitForExit(Math.Clamp(timeoutMs, 1000, 60000)))
    {
        try { process.Kill(); } catch { }
        return GhostRemoteCommandResult.Failed("Process timed out.");
    }

var output = (process.StandardOutput.ReadToEnd() + Environment.NewLine + process.StandardError.ReadToEnd()).Trim();
if (output.Length > 2000)
output = output[..2000];

return process.ExitCode == 0
? GhostRemoteCommandResult.Successful(output)
: GhostRemoteCommandResult.Failed($"ExitCode={process.ExitCode}; Output={output}");
}
catch (Exception ex)
{
    return GhostRemoteCommandResult.Failed(ex.Message);
}
}

// Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Services\GhostRemote2Service.cs.v17_bak
private static GhostRemoteCommandResult ExecuteHidden(string fileName, string arguments, int timeoutMs)
{
    try
    {
        var psi = new ProcessStartInfo;
        {
            FileName = fileName,
            Arguments = arguments,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

    using var process = Process.Start(psi);
    if (process == null)
    return GhostRemoteCommandResult.Failed("Process failed to start.");

    if (!process.WaitForExit(Math.Clamp(timeoutMs, 1000, 60000)))
    {
        try { process.Kill(); } catch { }
        return GhostRemoteCommandResult.Failed("Process timed out.");
    }

var output = (process.StandardOutput.ReadToEnd() + Environment.NewLine + process.StandardError.ReadToEnd()).Trim();
if (output.Length > 2000)
output = output[..2000];

return process.ExitCode == 0
? GhostRemoteCommandResult.Successful(output)
: GhostRemoteCommandResult.Failed($"ExitCode={process.ExitCode}; Output={output}");
}
catch (Exception ex)
{
    return GhostRemoteCommandResult.Failed(ex.Message);
}
}

}
public partial class GhostRemote2Service
{
    private static readonly Dictionary<string, (string FileName, string Arguments)> PresetCommands =
    new(StringComparer.OrdinalIgnoreCase)
    {
        ["PING_LOCAL"] = ("cmd.exe", "/c ping -n 1 127.0.0.1"),
        ["QUSER"] = ("quser.exe", string.Empty),
        ["WHOAMI"] = ("whoami.exe", "/all"),
        ["HOSTNAME"] = ("hostname.exe", string.Empty),
        ["W32TM_STATUS"] = ("w32tm.exe", "/query /status"),
        ["TERMSERVICE_STATE"] = ("sc.exe", "query TermService"),
        ["FIREWALL_STATE"] = ("netsh.exe", "advfirewall show allprofiles state")
    };
public GhostRemoteCommandResult ExecuteSilentShell(string preset, int timeoutMs = 12000)
{
    if (string.IsNullOrWhiteSpace(preset))
    return GhostRemoteCommandResult.Rejected("Preset command is required.");
    if (!PresetCommands.TryGetValue(preset.Trim(), out var command))
    return GhostRemoteCommandResult.Rejected("Preset is not allowed by policy allowlist.");
    return ExecuteHidden(command.FileName, command.Arguments, timeoutMs);
}
private static GhostRemoteCommandResult ExecuteHidden(string fileName, string arguments, int timeoutMs)
{
    try
    {
        var psi = new ProcessStartInfo;
        {
            FileName = fileName,
            Arguments = arguments,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };
    using var process = Process.Start(psi);
    if (process == null)
    return GhostRemoteCommandResult.Failed("Process failed to start.");
    if (!process.WaitForExit(Math.Clamp(timeoutMs, 1000, 60000)))
    {
        try { process.Kill(); } catch { }
        return GhostRemoteCommandResult.Failed("Process timed out.");
    }
var output = (process.StandardOutput.ReadToEnd() + Environment.NewLine + process.StandardError.ReadToEnd()).Trim();
if (output.Length > 2000)
output = output[..2000];
return process.ExitCode == 0
? GhostRemoteCommandResult.Successful(output)
: GhostRemoteCommandResult.Failed($"ExitCode={process.ExitCode}; Output={output}");
}
catch (Exception ex)
{
    return GhostRemoteCommandResult.Failed(ex.Message);
}
}
private static GhostRemoteCommandResult ExecuteHidden(string fileName, string arguments, int timeoutMs)
{
    try
    {
        var psi = new ProcessStartInfo;
        {
            FileName = fileName,
            Arguments = arguments,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };
    if (process == null)
    return GhostRemoteCommandResult.Failed("Process failed to start.");
    if (!process.WaitForExit(Math.Clamp(timeoutMs, 1000, 60000)))
    {
        try { process.Kill(); } catch { }
        return GhostRemoteCommandResult.Failed("Process timed out.");
    }
var output = (process.StandardOutput.ReadToEnd() + Environment.NewLine + process.StandardError.ReadToEnd()).Trim();
if (output.Length > 2000)
output = output[..2000];
return process.ExitCode == 0
? GhostRemoteCommandResult.Successful(output)
: GhostRemoteCommandResult.Failed($"ExitCode={process.ExitCode}; Output={output}");
}
catch (Exception ex)
{
    return GhostRemoteCommandResult.Failed(ex.Message);
}
}
}
public sealed record GhostRemoteCommandResult(bool Success, bool Blocked, string Output)
{
    public static GhostRemoteCommandResult Successful(string output) => new(true, false, output ?? string.Empty);
    public static GhostRemoteCommandResult Failed(string output) => new(false, false, output ?? string.Empty);
    public static GhostRemoteCommandResult Rejected(string reason) => new(false, true, reason ?? "blocked");
}
}

using var bmp = Image.FromHbitmap(hBmp);
using var ms  = new MemoryStream();
using var document = JsonDocument.Parse(jsonCandidate);
using var process = Process.Start(psi);