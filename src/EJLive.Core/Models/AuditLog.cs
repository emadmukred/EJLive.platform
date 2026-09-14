namespace EJLive.Core.Models
{
    /// <summary>
    /// Represents a single audit log entry for tracking user actions and system events.
    /// </summary>
    /// <remarks>
    /// Wave 1 (SS-04 D-08): rewrote the file. The previous version had four copies of
    /// <c>AuditLog</c> interleaved with provenance comments and an empty trailing
    /// partial-class fragment. The body of every copy was identical, so the union set is
    /// the original single declaration. <c>Hash</c> carries the tamper-evidence chain
    /// value consumed by <c>AuditLogger.VerifyChain</c> (SS-09 / SS-14).
    /// </remarks>
    public class AuditLog
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string UserName { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string TargetDevice { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
        public string IPAddress { get; set; } = string.Empty;
        public bool Success { get; set; } = true;
        public string? ErrorMessage { get; set; }
        public string Hash { get; set; } = string.Empty;
    }
}
