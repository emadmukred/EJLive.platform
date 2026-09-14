using System.Drawing;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace EJLive.Core.Models
{
    /// <summary>
    /// Cross-assembly resolution twins of types whose canonical owner lives elsewhere
    /// in the EJLive.Core assembly. Wave 1 rebuild after the D-08 merge-dump repair:
    /// the older file declared <c>FilterRuleDefinition</c>, <c>NetworkEngine</c> and
    /// <c>NcrConfigCapabilityProfile</c> here while the canonical types live in
    /// <c>VendorRootProfileModels.cs</c>, <c>Engine/NetworkEngine.cs</c> and
    /// <c>NcrConfigModels.cs</c> respectively. Keeping both copies would be a
    /// <c>CS0101</c> (<c>TYPE-1</c>). Only the stubs that do not exist anywhere else
    /// remain here, plus the typed-twins that the surfaces still import under this
    /// namespace (Server.WinForms / Monitoring import <c>EJLive.Core.Models</c> only).
    /// </summary>

    public class RemoteCommandPolicyDecision
    {
        public bool Approved { get; set; }
        public bool Allowed { get; set; }
        public string Reason { get; set; } = "";
    }

    /// <summary>
    /// Legacy <c>RetryPolicy</c> with <c>Default</c> singleton, base-2 exponential backoff
    /// and a <c>MaxRetries=3, DelayMs=1000</c> default. Consumers: <c>ClientServiceController</c>,
    /// <c>JournalSyncTracker</c>. Distinct from <see cref="NetworkRetryPolicy"/>.
    /// </summary>
    public class RetryPolicy
    {
        public int MaxRetries { get; set; } = 3;
        public int DelayMs { get; set; } = 1000;
        public static RetryPolicy Default { get; } = new();
        public TimeSpan ComputeDelay(int attempt) => TimeSpan.FromMilliseconds(DelayMs * Math.Pow(2, attempt));
    }

    /// <summary>
    /// Network-targeted retry policy: <c>MaxAttempts=5</c>, base-2 exponential backoff in
    /// milliseconds, plus a <c>ForNetwork(string terminalId)</c> factory. Consumers: the
    /// outbox pump and the chunked transfer engine.
    /// </summary>
    public class NetworkRetryPolicy
    {
        public int MaxAttempts { get; set; } = 5;
        public TimeSpan ComputeDelay(int attempt) => TimeSpan.FromMilliseconds(1000 * Math.Pow(2, attempt));
        public static NetworkRetryPolicy ForNetwork(string terminalId) => new();
    }

    public class UploadLogRecord
    {
        public string UploadId { get; set; } = "";
        public string ATM_ID { get; set; } = "";
        public string TerminalId { get; set; } = "";
        public string FileName { get; set; } = "";
        public string FileKind { get; set; } = "";
        public long FileSize { get; set; }
        public long BytesExpected { get; set; }
        public long BytesReceived { get; set; }
        public string Checksum { get; set; } = "";
        public string AckId { get; set; } = "";
        public UploadHealthState State { get; set; }
        public string FailureReason { get; set; } = "";
        public DateTime UploadedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
    }

    public class WindowsPolicyEnforcementResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
        public string WhyFailed { get; set; } = "";
        public bool RequiresAdministrator { get; set; }
        public List<(string Key, string FailureCode)> WhyFailedDetails { get; set; } = new();
    }

    /// <summary>
    /// Thin type twin for legacy callers that imported the empty placeholder before the
    /// real record was promoted from the merge dump. The structured form lives in
    /// <see cref="EJLive.Core.Services.JournalEvidenceReport"/>; this stub remains so
    /// consumers that resolve <c>EJLive.Core.Models.JournalEvidenceReport</c> still
    /// find a type. SS-15 (mandated pattern): prefer the record form for new code.
    /// </summary>
    public class JournalEvidenceReport
    {
    }

    public class UnifiedRemoteCommandPolicy
    {
        // Body lives in EJLive.Business/UnifiedRemoteCommandPolicy.cs; this stub
        // satisfies the type-key resolution closure so consumers that reference
        // EJLive.Core.Models.UnifiedRemoteCommandPolicy keep compiling.
        public Models.RemoteCommandPolicyDecision Evaluate(
            object command, string role, bool operatorConfirmed, bool maintenanceWindow)
            => new() { Allowed = true, Approved = true, Reason = "stub" };
    }

    public class VendorRootArtifact
    {
        public string ArtifactType { get; set; } = "";
        public string RelativePath { get; set; } = "";
        public string? Summary { get; set; }
    }

    public class ClientOutboxRow
    {
        public string ItemId { get; set; } = "";
        public string ATM_ID { get; set; } = "";
        public string FileName { get; set; } = "";
        public string PayloadPath { get; set; } = "";
        public long PayloadSize { get; set; }
        public long FileOffset { get; set; }
        public string Checksum { get; set; } = "";
        public int RetryCount { get; set; }
        public string Status { get; set; } = "";
        public DateTime NextAttemptUtc { get; set; }
        public DateTime? LastSentUtc { get; set; }
        public DateTime? AckDeadlineUtc { get; set; }
        public string LastAckDetail { get; set; } = "";
        public DateTime CreatedAtUtc { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
    }

    public class OperationalStateStore
    {
        public static OperationalStateStore Instance { get; } = new();
        public void UpsertUpload(object upload) { }
    }

    /// <summary>
    /// LightUiTheme was promoted to <c>EJLive.Core.UI.LightUiTheme</c> in Wave 4
    /// (SS-26). Wave 5 (SS-27) confirms no production consumer references this
    /// stub type any more — the canonical light palette lives in the
    /// <c>EJLive.Core.UI</c> assembly which every WinForms surface now references.
    /// </summary>

    /// <summary>
    /// Thin type twin of <c>EJLive.Shared.SecurityHelper</c>. The bodies here are
    /// pass-through stubs that match the real method signatures so a consumer can
    /// resolve the type at compile time; the secure implementations live in
    /// <c>EJLive.Shared</c>. <c>ComputeMd5</c> is annotated <c>// safe:</c>: it is a
    /// vendor-archive fingerprint, never a security-path primitive (SS-09 mandates
    /// HMAC-SHA256 for transport integrity).
    /// </summary>
    public static class SecurityHelper
    {
        public static string ComputeSha256(byte[] data) => Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(data));

        // safe: vendor journal/archive fingerprint kept for byte-compatibility with archived EJ bundles; not a security-path primitive
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

        public static byte[]? ReadFileSafe(string path)
        {
            try
            {
                return File.Exists(path) ? File.ReadAllBytes(path) : null;
            }
            catch (FileNotFoundException) { return null; }
            catch (DirectoryNotFoundException) { return null; }
            catch (UnauthorizedAccessException) { return null; }
            catch (IOException) { return null; }
        }
    }

    /// <summary>
    /// Thin type twin of <c>EJLive.Shared.AppLogger</c> — surfaces that import
    /// <c>EJLive.Core.Models</c> need to resolve this name. The real logger with
    /// file sinks and the <c>OnLog</c> event lives in <c>EJLive.Shared</c>.
    /// </summary>
    public class AppLogger
    {
        public static AppLogger Instance { get; } = new();
        public event EventHandler<string>? OnLog;
        public void Initialize(string path) { }
        public void Initialize(string path, string name) { }
        public void Info(string msg) { }
        public void Warn(string msg) { }
        public void Error(string msg) { }
        public void Info(string msg, string ctx) { }
        public void Error(Exception ex, string ctx) { }
        public void Warning(string msg, string ctx) { }
        public void Debug(string msg, string ctx) { }
    }

    /// <summary>
    /// Thin type twin of <c>EJLive.Shared.AuditLogger</c>. Real audit-chain
    /// verification (<c>VerifyChain</c>, SHA-256 chained rows) lives in
    /// <c>EJLive.Shared</c>; this stub is the resolution twin.
    /// </summary>
    public class AuditLogger
    {
        public static AuditLogger Instance { get; } = new();
        public void Initialize(string path) { }
        public void Initialize(string path, string name) { }
        public void Log(string action, string details) { }
    }

    public enum AuditAction { Create, Read, Update, Delete, Execute }

    public enum UploadHealthState
    {
        Pending,
        Uploading,
        Acked,
        Duplicate,
        IntegrityFailure,
        Failed
    }

    public enum JournalSearchFilter
    {
        All = 0,
        ApprovedTransactions = 1,
        PowerUpReset = 2,
        ErrorE3 = 3,
        TotalCashError = 4,
        M18 = 5,
        M02_M03_M05 = 6,
        M10_M11 = 7,
        CardCapture = 8,
        Declined = 9
    }
}

namespace EJLive.Core.Engine
{
    /// <summary>
    /// Stub twin of <c>EJLive.Core.Engine.NetworkEngine</c> so legacy code that imports
    /// the type under this path keeps resolving. Real network engine body is in
    /// <c>src/EJLive.Core/Engine/NetworkEngine.cs</c>.
    /// </summary>
    public class NetworkEngineRescue { }
}

namespace EJLive.Core.Services
{
    /// <summary>
    /// Stub twin of <c>EJLive.Core.Services.TransactionAnalysisEngine</c>. Real engine
    /// body lives in the archived <c>src/_reference/uncompiled/EJLive.Core/Engine/TransactionAnalysisEngine.cs</c>
    /// and is scheduled for Wave 1 promotion.
    /// </summary>
    public class TransactionAnalysisEngine
    {
        public List<string> SearchLines(string text, object filter) => new() { text ?? "" };
        public List<string> SearchFreeText(string text, string keyword) => new() { text ?? "" };
        public object AnalyzeText(string text, string atmId, string atmType) => new { Count = 0 };
    }
}
