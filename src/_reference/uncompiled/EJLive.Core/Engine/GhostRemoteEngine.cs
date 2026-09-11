using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using EJLive.Core.Models;

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
