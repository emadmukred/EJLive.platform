namespace EJLive.Client.WinForms.Supabase;

public record AtmAgentRow(
    string agent_id, string atm_type, string? branch_name, string? region,
    string? server_ip, int server_port, string? app_version,
    bool is_online, string? last_seen_utc,
    string? machine_name, string? os_user, string? windows_build);

public record AgentEventRow(
    string agent_id, string event_type, string? details,
    string severity, string occurred_at);

public record AgentHeartbeatRow(
    string agent_id, string beat_at_utc,
    bool is_connected, int pending_files, double uptime_minutes);

public record FileTransferRow(
    string agent_id, string filename,
    long file_size, string status, string started_at);