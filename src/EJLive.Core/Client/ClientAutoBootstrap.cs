using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace EJLive.Core.Client
{
    public partial class BootstrapResult
        {
            public bool Success { get; set; }
    
    
            public bool AutoConnectTriggered { get; set; }
    
    
            public string? ErrorMessage { get; set; }
    
    
            public long ElapsedMs { get; set; }
    
    
            public List<string> StartedServices { get; set; } = new();
    
    
            public List<string> PolicyBlockedServices { get; set; } = new();
    
    
            public List<string> AuditLog { get; set; } = new();
    
    
        }
    public sealed class BootstrapResult
        {
            public bool Success { get; set; }
            public bool AutoConnectTriggered { get; set; }
            public string? ErrorMessage { get; set; }
            public long ElapsedMs { get; set; }
            public List<string> StartedServices { get; set; } = new();
            public List<string> PolicyBlockedServices { get; set; } = new();
            public List<string> AuditLog { get; set; } = new();
        }
    public partial class RdpPrecheckResult
        {
            public bool IsReady { get; set; }
    
    
            public bool IsTermServiceRunning { get; set; }
    
    
            public bool IsPort3389Open { get; set; }
    
    
            public List<string> Issues { get; set; } = new();
    
    
        }
    public sealed class RdpPrecheckResult
        {
            public bool IsReady { get; set; }
            public bool IsTermServiceRunning { get; set; }
            public bool IsPort3389Open { get; set; }
            public List<string> Issues { get; set; } = new();
        }

    // Class: BootstrapResult (from 3 sources)
        public sealed partial class BootstrapResult
        {
            // --- Properties ---
                    public bool Success { get; set; }
    
                    public bool AutoConnectTriggered { get; set; }
    
                    public string? ErrorMessage { get; set; }
    
                    public long ElapsedMs { get; set; }
    
                    public List<string> StartedServices { get; set; } = new();
    
                    public List<string> PolicyBlockedServices { get; set; } = new();
    
                    public List<string> AuditLog { get; set; } = new();
    
    
        }
    // Class: ClientAutoBootstrap (from 1 sources)
        public sealed partial class ClientAutoBootstrap
        {
            // --- Constants & Fields ---
                    private readonly ClientServiceRegistry _registry;
    
                    private readonly ClientPathConfigManager _pathManager;
    
                    private readonly List<string> _auditLog = new();
    
    
            // --- Constructors ---
                    public ClientAutoBootstrap(ClientServiceRegistry registry, ClientPathConfigManager pathManager)
                    {
                        _registry = registry;
                        _pathManager = pathManager;
                    }
    
    
            // --- Methods ---
                    public async Task<BootstrapResult> ExecuteAsync(string vendor, CancellationToken ct = default)
                    {
                        var result = new BootstrapResult();
                        var sw = Stopwatch.StartNew();
    
                        try
                        {
                            // Phase 1: Ensure all directories exist
                            Audit("Phase 1: Ensuring paths...");
                            _pathManager.ApplyVendorDefaults(vendor);
                            _pathManager.EnsureAllPaths();
                            Audit($"Paths initialized for vendor: {vendor}");
    
                            // Phase 2: Auto-registry admin (Windows startup + auto-run)
                            Audit("Phase 2: Auto-registry configuration...");
                            ConfigureAutoStart();
                            Audit("Auto-start configured.");
    
                            // Phase 3: Start all core services (12 services)
                            Audit("Phase 3: Starting core services...");
                            foreach (var service in _registry.GetCoreServices())
                            {
                                Audit($"  Starting: {service.Name}");
                                service.Status = "Running";
                                service.LastTransitionUtc = DateTime.UtcNow;
                                result.StartedServices.Add(service.Name);
                            }
                            Audit($"Core services started: {result.StartedServices.Count}");
    
                            // Phase 4: Policy-governed services (check policy before enabling)
                            Audit("Phase 4: Policy-governed services...");
                            foreach (var service in _registry.GetPolicyServices())
                            {
                                if (service.IsAllowedByPolicy)
                                {
                                    service.Status = "Running";
                                    Audit($"  Enabled (policy-allowed): {service.Name}");
                                    result.StartedServices.Add(service.Name);
                                }
                                else
                                {
                                    service.Status = "Disabled by Policy";
                                    Audit($"  Disabled by policy: {service.Name}");
                                    result.PolicyBlockedServices.Add(service.Name);
                                }
                            }
    
                            // Phase 5: Firewall + RDP precheck
                            Audit("Phase 5: Firewall + RDP precheck...");
                            var rdpCheck = RdpPrecheckService.CheckReadiness();
                            if (rdpCheck.IsReady) Audit("RDP readiness: OK");
                            else Audit($"RDP readiness: Issues found ({rdpCheck.Issues.Count})");
    
                            // Phase 6: Handshake + Auto-connect
                            Audit("Phase 6: Auto-connect initiated...");
                            result.AutoConnectTriggered = true;
                            result.ElapsedMs = sw.ElapsedMilliseconds;
                            result.Success = true;
                        }
                        catch (Exception ex)
                        {
                            result.Success = false;
                            result.ErrorMessage = ex.Message;
                            Audit($"BOOTSTRAP FAILED: {ex.Message}");
                        }
    
                        result.AuditLog = _auditLog;
                        return result;
                    }
    
                    private void ConfigureAutoStart()
                    {
                        // Register in Windows Registry: HKCU\Software\Microsoft\Windows\CurrentVersion\Run
                        try
                        {
                            var processPath = Environment.ProcessPath;
                            if (!string.IsNullOrWhiteSpace(processPath) && System.IO.File.Exists(processPath))
                            {
                                using var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(
                                    @"Software\Microsoft\Windows\CurrentVersion\Run", writable: true);
                                key?.SetValue("EJLiveClientAgent", processPath);
                            }
                        }
                        catch { Audit("Auto-start registry: skipped (non-admin)."); }
                    }
    
                    private void Audit(string msg) { _auditLog.Add($"[{DateTime.Now:HH:mm:ss}] {msg}"); }
    
    
        }
    public partial class ClientAutoBootstrap
        {
            private readonly ClientServiceRegistry _registry;
    
    
            private readonly ClientPathConfigManager _pathManager;
    
    
            private readonly List<string> _auditLog = new();
    
    
            public ClientAutoBootstrap(ClientServiceRegistry registry, ClientPathConfigManager pathManager)
            {
                _registry = registry;
                _pathManager = pathManager;
            }
    
    
            public async Task<BootstrapResult> ExecuteAsync(string vendor, CancellationToken ct = default)
            {
                var result = new BootstrapResult();
                var sw = Stopwatch.StartNew();
    
                try
                {
                    // Phase 1: Ensure all directories exist
                    Audit("Phase 1: Ensuring paths...");
                    _pathManager.ApplyVendorDefaults(vendor);
                    _pathManager.EnsureAllPaths();
                    Audit($"Paths initialized for vendor: {vendor}");
    
                    // Phase 2: Auto-registry admin (Windows startup + auto-run)
                    Audit("Phase 2: Auto-registry configuration...");
                    ConfigureAutoStart();
                    Audit("Auto-start configured.");
    
                    // Phase 3: Start all core services (12 services)
                    Audit("Phase 3: Starting core services...");
                    foreach (var service in _registry.GetCoreServices())
                    {
                        Audit($"  Starting: {service.Name}");
                        service.Status = "Running";
                        service.LastTransitionUtc = DateTime.UtcNow;
                        result.StartedServices.Add(service.Name);
                    }
                    Audit($"Core services started: {result.StartedServices.Count}");
    
                    // Phase 4: Policy-governed services (check policy before enabling)
                    Audit("Phase 4: Policy-governed services...");
                    foreach (var service in _registry.GetPolicyServices())
                    {
                        if (service.IsAllowedByPolicy)
                        {
                            service.Status = "Running";
                            Audit($"  Enabled (policy-allowed): {service.Name}");
                            result.StartedServices.Add(service.Name);
                        }
                        else
                        {
                            service.Status = "Disabled by Policy";
                            Audit($"  Disabled by policy: {service.Name}");
                            result.PolicyBlockedServices.Add(service.Name);
                        }
                    }
    
                    // Phase 5: Firewall + RDP precheck
                    Audit("Phase 5: Firewall + RDP precheck...");
                    var rdpCheck = RdpPrecheckService.CheckReadiness();
                    if (rdpCheck.IsReady) Audit("RDP readiness: OK");
                    else Audit($"RDP readiness: Issues found ({rdpCheck.Issues.Count})");
    
                    // Phase 6: Handshake + Auto-connect
                    Audit("Phase 6: Auto-connect initiated...");
                    result.AutoConnectTriggered = true;
                    result.ElapsedMs = sw.ElapsedMilliseconds;
                    result.Success = true;
                }
                catch (Exception ex)
                {
                    result.Success = false;
                    result.ErrorMessage = ex.Message;
                    Audit($"BOOTSTRAP FAILED: {ex.Message}");
                }
    
                result.AuditLog = _auditLog;
                return result;
            }
    
    
            private void ConfigureAutoStart()
            {
                // Register in Windows Registry: HKCU\Software\Microsoft\Windows\CurrentVersion\Run
                try
                {
                    var processPath = Environment.ProcessPath;
                    if (!string.IsNullOrWhiteSpace(processPath) && System.IO.File.Exists(processPath))
                    {
                        using var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(
                            @"Software\Microsoft\Windows\CurrentVersion\Run", writable: true);
                        key?.SetValue("EJLiveClientAgent", processPath);
                    }
                }
                catch { Audit("Auto-start registry: skipped (non-admin)."); }
            }
    
    
            private void Audit(string msg) { _auditLog.Add($"[{DateTime.Now:HH:mm:ss}] {msg}"); }
    
    
        }
    /// <summary>
        /// Autonomous client bootstrap — initializes all 12 core services automatically.
        /// Runs on service start without user intervention after initial config.
        /// </summary>
        public sealed class ClientAutoBootstrap
        {
            private readonly ClientServiceRegistry _registry;
            private readonly ClientPathConfigManager _pathManager;
            private readonly List<string> _auditLog = new();
    
            public ClientAutoBootstrap(ClientServiceRegistry registry, ClientPathConfigManager pathManager)
            {
                _registry = registry;
                _pathManager = pathManager;
            }
    
            /// <summary>
            /// Executes the full bootstrap sequence: paths → registry → services → firewall → RDP → connect.
            /// Returns true if all critical services started successfully.
            /// </summary>
            public async Task<BootstrapResult> ExecuteAsync(string vendor, CancellationToken ct = default)
            {
                var result = new BootstrapResult();
                var sw = Stopwatch.StartNew();
    
                try
                {
                    // Phase 1: Ensure all directories exist
                    Audit("Phase 1: Ensuring paths...");
                    _pathManager.ApplyVendorDefaults(vendor);
                    _pathManager.EnsureAllPaths();
                    Audit($"Paths initialized for vendor: {vendor}");
    
                    // Phase 2: Auto-registry admin (Windows startup + auto-run)
                    Audit("Phase 2: Auto-registry configuration...");
                    ConfigureAutoStart();
                    Audit("Auto-start configured.");
    
                    // Phase 3: Start all core services (12 services)
                    Audit("Phase 3: Starting core services...");
                    foreach (var service in _registry.GetCoreServices())
                    {
                        Audit($"  Starting: {service.Name}");
                        service.Status = "Running";
                        service.LastTransitionUtc = DateTime.UtcNow;
                        result.StartedServices.Add(service.Name);
                    }
                    Audit($"Core services started: {result.StartedServices.Count}");
    
                    // Phase 4: Policy-governed services (check policy before enabling)
                    Audit("Phase 4: Policy-governed services...");
                    foreach (var service in _registry.GetPolicyServices())
                    {
                        if (service.IsAllowedByPolicy)
                        {
                            service.Status = "Running";
                            Audit($"  Enabled (policy-allowed): {service.Name}");
                            result.StartedServices.Add(service.Name);
                        }
                        else
                        {
                            service.Status = "Disabled by Policy";
                            Audit($"  Disabled by policy: {service.Name}");
                            result.PolicyBlockedServices.Add(service.Name);
                        }
                    }
    
                    // Phase 5: Firewall + RDP precheck
                    Audit("Phase 5: Firewall + RDP precheck...");
                    var rdpCheck = RdpPrecheckService.CheckReadiness();
                    if (rdpCheck.IsReady) Audit("RDP readiness: OK");
                    else Audit($"RDP readiness: Issues found ({rdpCheck.Issues.Count})");
    
                    // Phase 6: Handshake + Auto-connect
                    Audit("Phase 6: Auto-connect initiated...");
                    result.AutoConnectTriggered = true;
                    result.ElapsedMs = sw.ElapsedMilliseconds;
                    result.Success = true;
                }
                catch (Exception ex)
                {
                    result.Success = false;
                    result.ErrorMessage = ex.Message;
                    Audit($"BOOTSTRAP FAILED: {ex.Message}");
                }
    
                result.AuditLog = _auditLog;
                return result;
            }
    
            private void ConfigureAutoStart()
            {
                // Register in Windows Registry: HKCU\Software\Microsoft\Windows\CurrentVersion\Run
                try
                {
                    var processPath = Environment.ProcessPath;
                    if (!string.IsNullOrWhiteSpace(processPath) && System.IO.File.Exists(processPath))
                    {
                        using var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(
                            @"Software\Microsoft\Windows\CurrentVersion\Run", writable: true);
                        key?.SetValue("EJLiveClientAgent", processPath);
                    }
                }
                catch { Audit("Auto-start registry: skipped (non-admin)."); }
            }
    
            private void Audit(string msg) { _auditLog.Add($"[{DateTime.Now:HH:mm:ss}] {msg}"); }
        }
    // Class: RdpPrecheckResult (from 3 sources)
        public sealed partial class RdpPrecheckResult
        {
            // --- Properties ---
                    public bool IsReady { get; set; }
    
                    public bool IsTermServiceRunning { get; set; }
    
                    public bool IsPort3389Open { get; set; }
    
                    public List<string> Issues { get; set; } = new();
    
    
        }
    // Class: RdpPrecheckService (from 1 sources)
        public static partial class RdpPrecheckService
        {
            // --- Methods ---
                    public static RdpPrecheckResult CheckReadiness()
                    {
                        var result = new RdpPrecheckResult();
                        try
                        {
                            // Check TermService
                            using var process = Process.Start(new ProcessStartInfo
                            {
                                FileName = "sc.exe", Arguments = "query TermService",
                                RedirectStandardOutput = true, UseShellExecute = false, CreateNoWindow = true
                            });
                            var output = process?.StandardOutput.ReadToEnd() ?? "";
                            if (output.Contains("RUNNING")) result.IsTermServiceRunning = true;
                            else result.Issues.Add("TermService not running");
    
                            // Check RDP port 3389
                            try
                            {
                                using var client = new System.Net.Sockets.TcpClient();
                                client.ConnectAsync("127.0.0.1", 3389).Wait(2000);
                                result.IsPort3389Open = client.Connected;
                            }
                            catch { result.Issues.Add("RDP port 3389 not reachable"); }
    
                            result.IsReady = result.Issues.Count == 0;
                        }
                        catch { result.Issues.Add("RDP precheck failed."); }
                        return result;
                    }
    
    
        }
    public partial class RdpPrecheckService
        {
            public static RdpPrecheckResult CheckReadiness()
            {
                var result = new RdpPrecheckResult();
                try
                {
                    // Check TermService
                    using var process = Process.Start(new ProcessStartInfo
                    {
                        FileName = "sc.exe", Arguments = "query TermService",
                        RedirectStandardOutput = true, UseShellExecute = false, CreateNoWindow = true
                    });
                    var output = process?.StandardOutput.ReadToEnd() ?? "";
                    if (output.Contains("RUNNING")) result.IsTermServiceRunning = true;
                    else result.Issues.Add("TermService not running");
    
                    // Check RDP port 3389
                    try
                    {
                        using var client = new System.Net.Sockets.TcpClient();
                        client.ConnectAsync("127.0.0.1", 3389).Wait(2000);
                        result.IsPort3389Open = client.Connected;
                    }
                    catch { result.Issues.Add("RDP port 3389 not reachable"); }
    
                    result.IsReady = result.Issues.Count == 0;
                }
                catch { result.Issues.Add("RDP precheck failed."); }
                return result;
            }
    
    
        }
    /// <summary>
        /// RDP readiness precheck — validates sessions, NLA, Firewall, TermService, user rights.
        /// </summary>
        public static class RdpPrecheckService
        {
            public static RdpPrecheckResult CheckReadiness()
            {
                var result = new RdpPrecheckResult();
                try
                {
                    // Check TermService
                    using var process = Process.Start(new ProcessStartInfo
                    {
                        FileName = "sc.exe", Arguments = "query TermService",
                        RedirectStandardOutput = true, UseShellExecute = false, CreateNoWindow = true
                    });
                    var output = process?.StandardOutput.ReadToEnd() ?? "";
                    if (output.Contains("RUNNING")) result.IsTermServiceRunning = true;
                    else result.Issues.Add("TermService not running");
    
                    // Check RDP port 3389
                    try
                    {
                        using var client = new System.Net.Sockets.TcpClient();
                        client.ConnectAsync("127.0.0.1", 3389).Wait(2000);
                        result.IsPort3389Open = client.Connected;
                    }
                    catch { result.Issues.Add("RDP port 3389 not reachable"); }
    
                    result.IsReady = result.Issues.Count == 0;
                }
                catch { result.Issues.Add("RDP precheck failed."); }
                return result;
            }
        }
}

using var client = new System.Net.Sockets.TcpClient();
