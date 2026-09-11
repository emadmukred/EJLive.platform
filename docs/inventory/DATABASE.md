# DATABASE ledger (generated -- do not hand-edit)

Source: `tools/inventory/ejlive_inventory.py`. Regenerate with `python3 tools/inventory/ejlive_inventory.py`.

| object | evidence | map |
|---|---|---|
| AtmDevice | table | reference |
| AuditLog | table | reference |
| ClientSession | table | reference |
| CommandAudit | table | reference |
| CommandQueue | table | reference |
| FileManifest | table | reference |
| HealthSnapshot | dml, table | reference |
| JournalFile | table | reference |
| ParserEvidence | table | reference |
| ParserRun | table | reference |
| SchemaVersion | table | reference |
| UserEntity | table | reference |
| __migrations | dml, table | active |
| active_compile_map | table | reference |
| agent_events | table | reference |
| agent_heartbeats | table | reference |
| alerts | table | reference |
| atm_agents | table | reference |
| atm_registry | table | active |
| atms | table | reference |
| audit_log | dml, table | active, reference |
| client_health_snapshots | table | active, reference |
| client_outbox | dml, table | active |
| command_audit | table | active, reference |
| command_queue | table | active, reference |
| commands | table | reference |
| correlation_events | table | active, reference |
| daily_stats | dml, table | active, reference |
| file_transfers | table | reference |
| idx_alerts_created | index | reference |
| idx_alerts_severity | index | reference |
| idx_atm_status | index | reference |
| idx_audit_log_created | index | reference |
| idx_command_status | index | reference |
| idx_commands_atm | index | reference |
| idx_commands_status | index | reference |
| idx_events_agent | index | reference |
| idx_hb_agent | index | reference |
| idx_health_atm | index | reference |
| idx_journal_sync_atm | index | reference |
| idx_journal_sync_state | index | reference |
| idx_offsets_atm | index | active |
| idx_offsets_file | index | active |
| idx_parser_atm | index | reference |
| idx_session_req | index | reference |
| idx_telemetry_atm | index | reference |
| idx_telemetry_corr | index | reference |
| idx_transactions_atm | index | reference |
| idx_transactions_timestamp | index | reference |
| idx_vendor_atm | index | reference |
| ix_archive_atm | index | active, reference |
| ix_archive_month | index | active, reference |
| ix_audit_action | index | active, reference |
| ix_audit_log_created_at | index | active |
| ix_client_outbox_atm | index | active |
| ix_client_outbox_ready | index | active |
| ix_command_queue_atm | index | active |
| ix_command_queue_state | index | active |
| ix_journal_archive_atm | index | active |
| ix_screenshot_atm | index | active |
| ix_stats_atm_date | index | active, reference |
| ix_sync_atm_state | index | active, reference |
| ix_sync_records_atm | index | active |
| ix_sync_records_updated | index | active |
| ix_telemetry_atm | index | active |
| ix_telemetry_atm_time | index | active |
| ix_telemetry_type | index | active |
| ix_telemetry_type_time | index | active |
| journal_archive | table | active, reference |
| journal_offsets | dml, table | active |
| journal_sync | table | reference |
| network_disconnects | table | reference |
| outbox_dead_letters | table | reference |
| parser_transactions | table | active, reference |
| remote_session_audit | table | reference |
| reports | table | reference |
| schema_migrations | table | active |
| screenshot_history | table | active |
| screenshots | table | reference |
| security_policy_snapshots | table | reference |
| source_truth_records | table | reference |
| sync_records | dml, table | active, reference |
| telemetry_events | dml, table | active, reference |
| transactions | table | reference |
| transfer_sessions | table | active, reference |
| users | table | active |
| ux_daily_stats_atm_date | index | active, reference |
| ux_sync_idempotency | index | active, reference |
| vendor_events | table | active, reference |

Schema is code-owned: a table without a CREATE in `DatabaseSchema.cs`/migration *or* without a DML consumer is a defect (DB-1). Migrations are forward-only and numbered `NNNN_Name.sql`.
