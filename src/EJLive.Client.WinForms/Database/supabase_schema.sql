-- ============================================================
-- EJLive Enterprise v5 — Supabase PostgreSQL Schema
-- Run in Supabase SQL Editor (supabase.com → SQL Editor)
-- ============================================================

CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- ATM Agents registry
CREATE TABLE IF NOT EXISTS atm_agents (
    agent_id        TEXT PRIMARY KEY,
    atm_type        TEXT NOT NULL DEFAULT 'NCR',
    branch_name     TEXT,
    region          TEXT,
    server_ip       TEXT,
    server_port     INTEGER DEFAULT 5656,
    app_version     TEXT DEFAULT '5.0.0',
    is_online       BOOLEAN DEFAULT FALSE,
    last_seen_utc   TIMESTAMPTZ,
    machine_name    TEXT,
    os_user         TEXT,
    windows_build   TEXT,
    created_at      TIMESTAMPTZ DEFAULT NOW()
);

-- Events log (boot, restart, connect, disconnect, etc.)
CREATE TABLE IF NOT EXISTS agent_events (
    id          UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    agent_id    TEXT NOT NULL,
    event_type  TEXT NOT NULL,
    details     TEXT,
    severity    TEXT DEFAULT 'info',
    occurred_at TIMESTAMPTZ DEFAULT NOW()
);
CREATE INDEX IF NOT EXISTS idx_events_agent ON agent_events(agent_id, occurred_at DESC);

-- Heartbeats (every 30 seconds)
CREATE TABLE IF NOT EXISTS agent_heartbeats (
    id              UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    agent_id        TEXT NOT NULL,
    beat_at_utc     TIMESTAMPTZ DEFAULT NOW(),
    is_connected    BOOLEAN DEFAULT FALSE,
    pending_files   INTEGER DEFAULT 0,
    uptime_minutes  DECIMAL DEFAULT 0
);
CREATE INDEX IF NOT EXISTS idx_hb_agent ON agent_heartbeats(agent_id, beat_at_utc DESC);

-- File transfers
CREATE TABLE IF NOT EXISTS file_transfers (
    id          UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    agent_id    TEXT NOT NULL,
    filename    TEXT NOT NULL,
    file_size   BIGINT DEFAULT 0,
    status      TEXT DEFAULT 'pending',
    started_at  TIMESTAMPTZ DEFAULT NOW()
);

-- Screenshots metadata
CREATE TABLE IF NOT EXISTS screenshots (
    id            UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    agent_id      TEXT NOT NULL,
    storage_url   TEXT,
    taken_at_utc  TIMESTAMPTZ DEFAULT NOW(),
    file_size_kb  INTEGER DEFAULT 0
);

-- Network disconnects history
CREATE TABLE IF NOT EXISTS network_disconnects (
    id              UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    agent_id        TEXT NOT NULL,
    disconnected_at TIMESTAMPTZ NOT NULL,
    reconnected_at  TIMESTAMPTZ,
    duration_sec    INTEGER
);

-- Fleet summary view
CREATE OR REPLACE VIEW fleet_summary AS
SELECT
    a.agent_id, a.atm_type, a.branch_name, a.region, a.is_online, a.last_seen_utc,
    (SELECT COUNT(*) FROM file_transfers ft WHERE ft.agent_id = a.agent_id AND ft.status = 'done') AS files_sent,
    (SELECT COUNT(*) FROM network_disconnects nd WHERE nd.agent_id = a.agent_id) AS disconnects,
    (SELECT COUNT(*) FROM agent_events ae WHERE ae.agent_id = a.agent_id AND ae.event_type LIKE '%restart%') AS restarts
FROM atm_agents a;

-- Hint: Create storage bucket "atm-screenshots" for screenshot uploads
