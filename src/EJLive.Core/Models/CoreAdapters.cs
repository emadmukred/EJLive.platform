using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using EJLive.Core.Models;

namespace EJLive.Core.Services
{
    public class FilterRuleDefinition { public string ProviderId { get; set; } = ""; public string EventId { get; set; } = ""; public string RegexPattern { get; set; } = ""; }
    public class RemoteCommandPolicyDecision { public bool Approved { get; set; } public string Reason { get; set; } = ""; }
    public class RetryPolicy { public int MaxRetries { get; set; } = 3; public int DelayMs { get; set; } = 1000; public static RetryPolicy Default { get; } = new(); public TimeSpan ComputeDelay(int attempt) => TimeSpan.FromMilliseconds(DelayMs * Math.Pow(2, attempt)); }
    public partial class TransactionAnalysisEngine
        {
            public List<string> SearchLines(string text, object filter) => new() { text ?? "" };
    
    
            public List<string> SearchFreeText(string text, string keyword) => new() { text ?? "" };
    
    
            public dynamic AnalyzeText(string text, string atmId, string atmType) => new { Count = 0 };
    
    
            public static string ComputeMd5(byte[] data)
            // safe: legacy journal-index fingerprint kept for byte-compatibility with archived EJ bundles; do not extend to new callers => Convert.ToHexString(System.Security.Cryptography.MD5.HashData(data));
    
    
            public static byte[] ProtectDpapiStringIfNeeded(string value) => System.Text.Encoding.UTF8.GetBytes(value);
    
    
            public static byte[] Compress(byte[] data) => data;
    
    
            public static byte[] Encrypt(byte[] data) => data;
    
    
            public static byte[] EncryptAES(byte[] data, byte[]? key) => data;
    
    
            public static byte[] DecryptAES(byte[] data, byte[]? key) => data;
    
    
            public static (string, object) GenerateRSAKeyPair() => ("", new object());
    
    
            public static byte[] DecryptWithRSAPrivateKey(byte[] data, object key) => data;
    
    
            public static byte[] CompressAndEncrypt(byte[] data) => data;
    
    
            public static byte[] DecryptAndDecompress(byte[] data) => data;
    
    
            public static string SHA256Hash(byte[] data) => ComputeSha256(data);
    
    
            public static string MD5Hash(byte[] data) => ComputeMd5(data);
    
    
            public static byte[]? ReadFileSafe(string path) { try { return File.Exists(path) ? File.ReadAllBytes(path) : null; } catch { return null; } }
    
    
    
    
    
    
            public class NcrConfigCapabilityProfile { public string ProfileName { get; set; } = ""; }
    
    
            public class VendorRootArtifact { public string ArtifactType { get; set; } = ""; public string RelativePath { get; set; } = ""; public string? Summary { get; set; } }
    
    
            public class VendorRootProfile { public string Vendor { get; set; } = ""; public string Model { get; set; } = ""; public string VendorName { get; set; } = ""; public string PlatformLineage { get; set; } = ""; public bool HasFilterIni { get; set; } public bool HasXfsMediaTemplates { get; set; } public bool HasDispenserConfigData { get; set; } public bool HasKeyboardMapData { get; set; } public bool HasKbapeConfig { get; set; } public string FilterHeaderHint { get; set; } = ""; public List<VendorRootArtifact> Artifacts { get; set; } = new(); }
    
    
            public static class LightUiTheme { public static Color Window => Color.FromArgb(30, 30, 35); public static Color Surface => Color.FromArgb(40, 40, 48); public static Color SurfaceAlt => Color.FromArgb(25, 25, 30); public static Color Text => Color.White; public static Color Muted => Color.Gray; public static Color Border => Color.FromArgb(60, 60, 68); public static void Apply(Form form) { } }
    
    
            public static class SecurityHelper
            public static string ComputeSha256(byte[] data) => Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(data));
    
    
            public class AppLogger { public static AppLogger Instance { get; } = new(); public event EventHandler<string>? OnLog; public void Initialize(string path) { } public void Initialize(string path, string name) { } public void Info(string msg) { } public void Warn(string msg) { } public void Error(string msg) { } public void Info(string msg, string ctx) { } public void Error(Exception ex, string ctx) { } public void Warning(string msg, string ctx) { } public void Debug(string msg, string ctx) { } }
    
    
            public class AuditLogger { public static AuditLogger Instance { get; } = new(); public void Initialize(string path) { } public void Initialize(string path, string name) { } public void Log(string action, string details) { } }
    
    
            public enum JournalSearchFilter { All = 0, ApprovedTransactions = 1, PowerUpReset = 2, ErrorE3 = 3, TotalCashError = 4, M18 = 5, M02_M03_M05 = 6, M10_M11 = 7, CardCapture = 8, Declined = 9 }
    
    
        }
    public sealed class UploadLogRecord { public string ATM_ID { get; set; } = ""; public string FileName { get; set; } = ""; public long FileSize { get; set; } public DateTime UploadedAtUtc { get; set; } = DateTime.UtcNow; }
    public class WindowsPolicyEnforcementResult { public bool Success { get; set; } public string Message { get; set; } = ""; public string WhyFailed { get; set; } = ""; public bool RequiresAdministrator { get; set; } public List<(string Key, string FailureCode)> WhyFailedDetails { get; set; } = new(); }
    public class XfsMediaTemplateDefinition { public string TemplateName { get; set; } = ""; }

    // Enum: AuditAction (from 3 sources)
        public partial enum AuditAction
        {
        }
    // Class: FilterRuleDefinition (from 3 sources)
        public partial class FilterRuleDefinition
        {
        }
    // Class: NetworkEngine (from 2 sources)
        public partial class NetworkEngine
        {
        }
    // Class: RemoteCommandPolicyDecision (from 3 sources)
        public partial class RemoteCommandPolicyDecision
        {
        }
    // Class: RetryPolicy (from 5 sources)
        public partial class RetryPolicy
        {
        }
    // Class: TransactionAnalysisEngine (from 2 sources)
        public partial class TransactionAnalysisEngine
        {
            // --- Methods ---
            public List<string> SearchLines(string text, object filter) => new() { text ?? "" };
    
            public List<string> SearchFreeText(string text, string keyword) => new() { text ?? "" };
    
            public dynamic AnalyzeText(string text, string atmId, string atmType) => new { Count = 0 };
    
            // safe: vendor journal/archive fingerprint kept for byte-compatibility; not a security boundary (SS9 integrity uses HMAC-SHA256)
            public static string ComputeMd5(byte[] data) => Convert.ToHexString(System.Security.Cryptography.MD5.HashData(data));
    
            public static byte[] ProtectDpapiStringIfNeeded(string value) => System.Text.Encoding.UTF8.GetBytes(value);
    
            public static byte[] Compress(byte[] data) => data;
    
            public static byte[] Encrypt(byte[] data) => data;
    
            public static byte[] EncryptAES(byte[] data, byte[]? key) => data;
    
            public static byte[] DecryptAES(byte[] data, byte[]? key) => data;
    
            public static (string, object) GenerateRSAKeyPair() => ("", new object());
    
            public static byte[] DecryptWithRSAPrivateKey(byte[] data, object key) => data;
    
            public static byte[] CompressAndEncrypt(byte[] data) => data;
    
            public static byte[] DecryptAndDecompress(byte[] data) => data;
    
            public static string SHA256Hash(byte[] data) => ComputeSha256(data);
    
            public static string MD5Hash(byte[] data) => ComputeMd5(data);
    
            public static byte[]? ReadFileSafe(string path) { try { return File.Exists(path) ? File.ReadAllBytes(path) : null; } catch { return null; } }
    
    
    
    
            public class NcrConfigCapabilityProfile { public string ProfileName { get; set; } = ""; }
    
            public class VendorRootArtifact { public string ArtifactType { get; set; } = ""; public string RelativePath { get; set; } = ""; public string? Summary { get; set; } }
    
            public class VendorRootProfile { public string Vendor { get; set; } = ""; public string Model { get; set; } = ""; public string VendorName { get; set; } = ""; public string PlatformLineage { get; set; } = ""; public bool HasFilterIni { get; set; } public bool HasXfsMediaTemplates { get; set; } public bool HasDispenserConfigData { get; set; } public bool HasKeyboardMapData { get; set; } public bool HasKbapeConfig { get; set; } public string FilterHeaderHint { get; set; } = ""; public List<VendorRootArtifact> Artifacts { get; set; } = new(); }
    
            public static class LightUiTheme { public static Color Window => Color.FromArgb(30, 30, 35); public static Color Surface => Color.FromArgb(40, 40, 48); public static Color SurfaceAlt => Color.FromArgb(25, 25, 30); public static Color Text => Color.White; public static Color Muted => Color.Gray; public static Color Border => Color.FromArgb(60, 60, 68); public static void Apply(Form form) { } }
    
            public static class SecurityHelper
            public static string ComputeSha256(byte[] data) => Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(data));
    
            public class AppLogger { public static AppLogger Instance { get; } = new(); public event EventHandler<string>? OnLog; public void Initialize(string path) { } public void Initialize(string path, string name) { } public void Info(string msg) { } public void Warn(string msg) { } public void Error(string msg) { } public void Info(string msg, string ctx) { } public void Error(Exception ex, string ctx) { } public void Warning(string msg, string ctx) { } public void Debug(string msg, string ctx) { } }
    
            public class AuditLogger { public static AuditLogger Instance { get; } = new(); public void Initialize(string path) { } public void Initialize(string path, string name) { } public void Log(string action, string details) { } }
    
    
            // --- Nested Enums ---
            public enum JournalSearchFilter { All = 0, ApprovedTransactions = 1, PowerUpReset = 2, ErrorE3 = 3, TotalCashError = 4, M18 = 5, M02_M03_M05 = 6, M10_M11 = 7, CardCapture = 8, Declined = 9 }
    
    
        }
    // Enum: UploadHealthState (from 2 sources)
        public partial enum UploadHealthState
        {
        }
    // Class: UploadLogRecord (from 5 sources)
        public sealed partial class UploadLogRecord
        {
        }
    // Enum: VendorPlatformLineage (from 3 sources)
        public partial enum VendorPlatformLineage
        {
        }
    // Class: WindowsPolicyEnforcementResult (from 3 sources)
        public partial class WindowsPolicyEnforcementResult
        {
        }
    // Class: XfsMediaTemplateDefinition (from 3 sources)
        public partial class XfsMediaTemplateDefinition
        {
        }

    public enum AuditAction { Create, Read, Update, Delete, Execute }
    public partial enum AuditAction
        {
        }
    public partial class FilterRuleDefinition
        {
        }
    public class JournalEvidenceReport { }
    public partial class NetworkEngine
        {
        }
    public partial class RemoteCommandPolicyDecision
        {
        }
    public partial class RetryPolicy
        {
        }
    public class UnifiedRemoteCommandPolicy { }
    public partial enum UploadHealthState
        {
        }
    public partial class UploadLogRecord
        {
        }
    public enum VendorPlatformLineage { Unknown, NCR, GRG, Wincor, Diebold, Hyosung, Cashway }
    public partial enum VendorPlatformLineage
        {
        }
    public partial class WindowsPolicyEnforcementResult
        {
        }
    public partial class XfsMediaTemplateDefinition
        {
        }
}

namespace EJLive.Core.Engine
{
    public class ClientOutboxRow { public string ItemId { get; set; } = ""; public string ATM_ID { get; set; } = ""; public string FileName { get; set; } = ""; public string PayloadPath { get; set; } = ""; public long PayloadSize { get; set; } public long FileOffset { get; set; } public string Checksum { get; set; } = ""; public int RetryCount { get; set; } public string Status { get; set; } = ""; public DateTime NextAttemptUtc { get; set; } public DateTime? LastSentUtc { get; set; } public DateTime? AckDeadlineUtc { get; set; } public string LastAckDetail { get; set; } = ""; public DateTime CreatedAtUtc { get; set; } public DateTime UpdatedAtUtc { get; set; } }
    public class OperationalStateStore { public static OperationalStateStore Instance { get; } = new(); public void UpsertUpload(object upload) { } }
    public class RetryPolicy { public int MaxAttempts { get; set; } = 5; public TimeSpan ComputeDelay(int attempt) => TimeSpan.FromMilliseconds(1000 * Math.Pow(2, attempt)); public static RetryPolicy ForNetwork(string t) => new(); }
    public class UploadLogRecord { public string UploadId { get; set; } = ""; public string TerminalId { get; set; } = ""; public string FileName { get; set; } = ""; public string FileKind { get; set; } = ""; public long BytesExpected { get; set; } public long BytesReceived { get; set; } public string Checksum { get; set; } = ""; public string AckId { get; set; } = ""; public UploadHealthState State { get; set; } public string FailureReason { get; set; } = ""; public DateTime CreatedAtUtc { get; set; } public DateTime UpdatedAtUtc { get; set; } }


    public enum JournalSearchFilter { All = 0, ApprovedTransactions = 1, PowerUpReset = 2, ErrorE3 = 3, TotalCashError = 4, M18 = 5, M02_M03_M05 = 6, M10_M11 = 7, CardCapture = 8, Declined = 9 }
    public enum UploadHealthState { Pending, Uploading, Acked, Duplicate, IntegrityFailure, Failed }
}

namespace EJLive.Core.Models
{
    public class VendorRootArtifact { public string ArtifactType { get; set; } = ""; public string RelativePath { get; set; } = ""; public string? Summary { get; set; } }
}

namespace EJLive.Shared
{
    public class AppLogger { public static AppLogger Instance { get; } = new(); public event EventHandler<string>? OnLog; public void Initialize(string path) { } public void Initialize(string path, string name) { } public void Info(string msg) { } public void Warn(string msg) { } public void Error(string msg) { } public void Info(string msg, string ctx) { } public void Error(Exception ex, string ctx) { } public void Warning(string msg, string ctx) { } public void Debug(string msg, string ctx) { } }
    public class AuditLogger { public static AuditLogger Instance { get; } = new(); public void Initialize(string path) { } public void Initialize(string path, string name) { } public void Log(string action, string details) { } }
    public static class LightUiTheme { public static Color Window => Color.FromArgb(30, 30, 35); public static Color Surface => Color.FromArgb(40, 40, 48); public static Color SurfaceAlt => Color.FromArgb(25, 25, 30); public static Color Text => Color.White; public static Color Muted => Color.Gray; public static Color Border => Color.FromArgb(60, 60, 68); public static void Apply(Form form) { } }

    public static class SecurityHelper
        {
            public static string ComputeSha256(byte[] data) => Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(data));
            // safe: vendor journal/archive fingerprint kept for byte-compatibility; not a security boundary (SS9 integrity uses HMAC-SHA256)
            public static string ComputeMd5(byte[] data) => Convert.ToHexString(System.Security.Cryptography.MD5.HashData(data));
            public static byte[] ProtectDpapiStringIfNeeded(string value) => System.Text.Encoding.UTF8.GetBytes(value);
            public static byte[] Compress(byte[] data) => data;
            public static byte[] Encrypt(byte[] data) => data;
            public static byte[] EncryptAES(byte[] data, byte[]? key) => data;
            public static byte[] DecryptAES(byte[] data, byte[]? key) => data;
            public static (string, object) GenerateRSAKeyPair() => ("", new object());
            public static byte[] DecryptWithRSAPrivateKey(byte[] data, object key) => data;
            public static byte[] CompressAndEncrypt(byte[] data) => data;
            public static byte[] DecryptAndDecompress(byte[] data) => data;
            public static string SHA256Hash(byte[] data) => ComputeSha256(data);
            public static string MD5Hash(byte[] data) => ComputeMd5(data);
            public static byte[]? ReadFileSafe(string path) { try { return File.Exists(path) ? File.ReadAllBytes(path) : null; } catch { return null; } }
        }
}
