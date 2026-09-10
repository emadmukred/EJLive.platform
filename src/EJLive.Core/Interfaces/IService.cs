namespace EJLive.Core.Interfaces
{
    public interface IService
    {
        string ServiceName { get; }
        bool IsRunning { get; }
        Task StartAsync(CancellationToken cancellationToken = default);
        Task StopAsync(CancellationToken cancellationToken = default);
        ServiceHealth GetHealth();
    }
    public partial interface IService
        {
            string ServiceName { get; }
    
    
            bool IsRunning { get; }
    
    
            Task StartAsync(CancellationToken cancellationToken = default);
    
    
            Task StopAsync(CancellationToken cancellationToken = default);
    
    
            ServiceHealth GetHealth();
    
    
        }
    /// <summary>
    /// Base service lifecycle contract for all Core services.
    /// Provides unified Start/Stop/Health semantics across the system.
    /// </summary>
    public interface IService
    {
        /// <summary>Unique human-readable identifier for this service.</summary>
        string ServiceName { get; }
    
        /// <summary>Whether the service is currently running.</summary>
        bool IsRunning { get; }
    
        /// <summary>Starts the service asynchronously with graceful cancellation support.</summary>
        Task StartAsync(CancellationToken cancellationToken = default);
    
        /// <summary>Stops the service asynchronously with graceful cancellation support.</summary>
        Task StopAsync(CancellationToken cancellationToken = default);
    
        /// <summary>Returns a snapshot of the service's current health status.</summary>
        ServiceHealth GetHealth();
    }
    public class ServiceHealth
    {
        public string ServiceName { get; set; } = string.Empty;
        public bool IsHealthy { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime LastCheck { get; set; } = DateTime.UtcNow;
        public string? Message { get; set; }
        public TimeSpan Uptime { get; set; }
    }
    public partial class ServiceHealth
        {
            public string ServiceName { get; set; } = string.Empty;
    
    
            public bool IsHealthy { get; set; }
    
    
            public string Status { get; set; } = string.Empty;
    
    
            public DateTime LastCheck { get; set; } = DateTime.UtcNow;
    
    
            public string? Message { get; set; }
    
    
            public TimeSpan Uptime { get; set; }
    
    
        }
    /// <summary>
    /// Health snapshot for a service, used by monitoring dashboards and alerting.
    /// </summary>
    public class ServiceHealth
    {
        /// <summary>Service identifier.</summary>
        public string ServiceName { get; set; } = string.Empty;
    
        /// <summary>Whether the service is in a healthy operational state.</summary>
        public bool IsHealthy { get; set; }
    
        /// <summary>Human-readable status description.</summary>
        public string Status { get; set; } = string.Empty;
    
        /// <summary>UTC timestamp of the last health check.</summary>
        public DateTime LastCheck { get; set; } = DateTime.UtcNow;
    
        /// <summary>Optional diagnostic message (e.g., error details).</summary>
        public string? Message { get; set; }
    
        /// <summary>Continuous uptime duration of the service.</summary>
        public TimeSpan Uptime { get; set; }
    }

    // Interface: IService (from 3 sources)
        public partial interface IService
        {
            // --- Properties ---
            string ServiceName { get; }
    
            bool IsRunning { get; }
    
    
            // --- Methods ---
                Task StartAsync(CancellationToken cancellationToken = default);
    
                Task StopAsync(CancellationToken cancellationToken = default);
    
                ServiceHealth GetHealth();
    
    
        }
    // Class: ServiceHealth (from 3 sources)
        public partial class ServiceHealth
        {
            // --- Properties ---
                public string ServiceName { get; set; } = string.Empty;
    
                public bool IsHealthy { get; set; }
    
                public string Status { get; set; } = string.Empty;
    
                public DateTime LastCheck { get; set; } = DateTime.UtcNow;
    
                public string? Message { get; set; }
    
                public TimeSpan Uptime { get; set; }
    
    
        }
}
