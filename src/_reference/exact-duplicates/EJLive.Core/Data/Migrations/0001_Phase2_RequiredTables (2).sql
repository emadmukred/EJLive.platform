-- Migration: 0001_Phase2_RequiredTables
-- Description: Create tables required for Phase-2 tracks
-- Up

CREATE TABLE IF NOT EXISTS active_compile_map (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    project TEXT NOT NULL,
    file_path TEXT NOT NULL,
    compile_state TEXT NOT NULL,
    include_source TEXT,
    reason TEXT,
    last_verified_utc TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS transfer_sessions (
    transfer_id TEXT PRIMARY KEY,
    atm_id TEXT NOT NULL,
    file_name TEXT NOT NULL,
    length INTEGER NOT NULL,
    offset INTEGER NOT NULL DEFAULT 0,
    chunk_size INTEGER NOT NULL,
    total_chunks INTEGER NOT NULL,
    received_chunks_bitmap TEXT,
    expected_sha256 TEXT,
    computed_sha256 TEXT,
    state TEXT NOT NULL,
    retry_count INTEGER NOT NULL DEFAULT 0,
    created_utc TEXT NOT NULL,
    completed_utc TEXT,
    last_error TEXT
);

CREATE TABLE IF NOT EXISTS telemetry_events (
    event_id TEXT PRIMARY KEY,
    correlation_id TEXT NOT NULL,
    atm_id TEXT,
    event_type TEXT NOT NULL,
    severity TEXT NOT NULL,
    message TEXT NOT NULL,
    timestamp_utc TEXT NOT NULL,
    metadata_json TEXT
);

CREATE TABLE IF NOT EXISTS command_queue (
    command_id TEXT PRIMARY KEY,
    status TEXT NOT NULL,
    command_type TEXT NOT NULL,
    payload_json TEXT,
    signature TEXT,
    issuer_role TEXT,
    issuer_id TEXT,
    risk_level TEXT,
    created_utc TEXT NOT NULL,
    sent_utc TEXT,
    completed_utc TEXT,
    result_json TEXT,
    error TEXT
);

CREATE TABLE IF NOT EXISTS command_audit (
    audit_id TEXT PRIMARY KEY,
    command_id TEXT NOT NULL,
    action TEXT NOT NULL,
    operator_id TEXT,
    timestamp_utc TEXT NOT NULL,
    before_state_json TEXT,
    after_state_json TEXT,
    rollback_plan_json TEXT
);

CREATE TABLE IF NOT EXISTS parser_transactions (
    transaction_id TEXT PRIMARY KEY,
    atm_id TEXT NOT NULL,
    start_line INTEGER NOT NULL,
    end_line INTEGER NOT NULL,
    classification TEXT NOT NULL,
    confidence REAL,
    amount REAL,
    currency TEXT,
    stan TEXT,
    rrn TEXT,
    m_code TEXT,
    r_code TEXT,
    raw_lines_json TEXT,
    timestamp_utc TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS vendor_events (
    event_id TEXT PRIMARY KEY,
    atm_id TEXT NOT NULL,
    vendor TEXT NOT NULL,
    device_class TEXT,
    severity TEXT,
    code TEXT,
    message TEXT,
    timestamp_utc TEXT NOT NULL,
    raw_line TEXT,
    source_file TEXT
);

CREATE TABLE IF NOT EXISTS correlation_events (
    correlation_id TEXT PRIMARY KEY,
    event_id TEXT NOT NULL,
    transaction_id TEXT,
    match_strength TEXT NOT NULL,
    confidence_score REAL,
    correlation_reason TEXT,
    false_positive_risk TEXT,
    operator_explanation TEXT,
    created_utc TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS client_health_snapshots (
    snapshot_id TEXT PRIMARY KEY,
    atm_id TEXT NOT NULL,
    timestamp_utc TEXT NOT NULL,
    state TEXT,
    connected INTEGER,
    handshake_complete INTEGER,
    pending_outbox_items INTEGER,
    total_bytes_sent INTEGER,
    total_bytes_received INTEGER,
    last_heartbeat_utc TEXT,
    last_journal_sync_utc TEXT,
    session_id TEXT,
    last_error TEXT
);

CREATE TABLE IF NOT EXISTS source_truth_records (
    record_id TEXT PRIMARY KEY,
    baseline_timestamp TEXT NOT NULL,
    source_root_sha256 TEXT NOT NULL,
    dotnet_version TEXT,
    test_count INTEGER,
    probe_count INTEGER,
    log_paths_json TEXT,
    created_utc TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS outbox_dead_letters (
    dead_letter_id TEXT PRIMARY KEY,
    atm_id TEXT NOT NULL,
    original_file_name TEXT,
    payload_path TEXT,
    failure_reason TEXT,
    retry_count INTEGER,
    created_utc TEXT NOT NULL,
    dead_lettered_utc TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS remote_session_audit (
    audit_id TEXT PRIMARY KEY,
    request_id TEXT NOT NULL,
    session_id TEXT,
    operator_id TEXT,
    atm_id TEXT,
    session_type TEXT,
    start_utc TEXT,
    end_utc TEXT,
    outcome TEXT,
    stop_reason TEXT,
    created_utc TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS security_policy_snapshots (
    snapshot_id TEXT PRIMARY KEY,
    policy_type TEXT NOT NULL,
    before_json TEXT,
    after_json TEXT,
    rollback_json TEXT,
    operator_id TEXT,
    reason TEXT,
    timestamp_utc TEXT NOT NULL
);

-- Indexes
CREATE INDEX IF NOT EXISTS idx_telemetry_corr ON telemetry_events(correlation_id);
CREATE INDEX IF NOT EXISTS idx_telemetry_atm ON telemetry_events(atm_id);
CREATE INDEX IF NOT EXISTS idx_command_status ON command_queue(status);
CREATE INDEX IF NOT EXISTS idx_parser_atm ON parser_transactions(atm_id);
CREATE INDEX IF NOT EXISTS idx_vendor_atm ON vendor_events(atm_id);
CREATE INDEX IF NOT EXISTS idx_health_atm ON client_health_snapshots(atm_id);
CREATE INDEX IF NOT EXISTS idx_session_req ON remote_session_audit(request_id);

-- Down (rollback)
-- DROP TABLE IF EXISTS active_compile_map;
-- DROP TABLE IF EXISTS transfer_sessions;
-- DROP TABLE IF EXISTS telemetry_events;
-- DROP TABLE IF EXISTS command_queue;
-- DROP TABLE IF EXISTS command_audit;
-- DROP TABLE IF EXISTS parser_transactions;
-- DROP TABLE IF EXISTS vendor_events;
-- DROP TABLE IF EXISTS correlation_events;
-- DROP TABLE IF EXISTS client_health_snapshots;
-- DROP TABLE IF EXISTS source_truth_records;
-- DROP TABLE IF EXISTS outbox_dead_letters;
-- DROP TABLE IF EXISTS remote_session_audit;
-- DROP TABLE IF EXISTS security_policy_snapshots;
