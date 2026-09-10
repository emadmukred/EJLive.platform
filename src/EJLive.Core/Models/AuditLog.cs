namespace EJLive.Core.Models
{
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
    public partial class AuditLog
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
    /// <summary>
    /// Represents a single audit log entry for tracking user actions and system events.
    /// </summary>
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

    // Class: AuditLog (from 4 sources)
        public partial class AuditLog
        {
            // --- Properties ---
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
