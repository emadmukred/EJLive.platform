using System;
using System.Collections.Generic;

namespace EJLive.Core.Data.Repositories;

// ---------------------------------------------------------------------------
// Contracts + entities for the data access layer (SS-11, wave 4).
//
// Before this batch the platform wrote its operational tables through raw SQL
// embedded in service classes, and the Phase-2 tables (parser_transactions,
// command_audit, client_health_snapshots, transfer_sessions, atm_registry, …)
// were created by migrations but had no DML consumer at all — the DB-1 backlog.
// These interfaces are the typed owners: one table (or a join the read path
// needs), one implementation, all parameters bound. The implementations are
// synchronous by contract: every caller is a background pipeline stage (SS-12),
// and the UI never touches a repository directly.
// ---------------------------------------------------------------------------

/// <summary>
/// Journal archive ledger — the durable receipt of "what the terminal sent and what
/// we kept". Idempotent on <c>(atm_id, file_name, sha256_hash)</c>, the pair the server
/// ingest rule requires: a resend must never create a second row.
/// </summary>
public interface IJournalArchiveRepository
{
    /// <summary>Insert (or verify-existing). Returns true when a new row was written.</summary>
    bool TryInsert(JournalArchiveRecord record);

    /// <summary>Count of archived files for a terminal in an (inclusive) UTC window.</summary>
    long CountFiles(string atmId, DateTime? fromUtc = null, DateTime? toUtc = null);

    /// <summary>Sum of <c>transaction_count</c> for a terminal — the "archive total" side of the
    /// Journal Studio reconciliation (SS-10.5). Null window means "all history".</summary>
    long SumTransactions(string atmId, DateTime? fromUtc = null, DateTime? toUtc = null);

    /// <summary>Backfills <c>transaction_count</c> once the parser stage knows it.
    /// The archive row is written at ingest, the count at analysis (SS8 pipeline order).</summary>
    void UpdateTransactionCount(string entryId, int count);

    /// <summary>Retention job entry point (SS-11: retention is an explicit job, never a trigger).
    /// Returns the number of pruned rows; files on disk are owned by the caller.</summary>
    int PruneBefore(DateTime olderThanUtc);
}

/// <summary>
/// Command intake ledger (SS-9 chokepoint 1 + 2): <see cref="Append"/>d for every decision —
/// allow AND deny — with the args hash and, when the command completed, the outcome and latency.
/// </summary>
public interface ICommandAuditRepository
{
    void Append(CommandAuditRecord record);

    /// <summary>Most recent decisions, optionally filtered by operator. Rows already carry the outcome.</summary>
    IReadOnlyList<CommandAuditRecord> QueryRecent(string? operatorId = null, int maxRows = 200);

    /// <summary>Fills outcome/latency when a queued command settles. Returns rows updated.</summary>
    int RecordOutcome(string commandId, string outcome, int latencyMs);
}

/// <summary>
/// Client heartbeat/health projection (SS8 telemetry lane:
/// <c>telemetry → client_health_snapshots</c>). Newest-per-terminal is the only read shape the
/// NOC and the server fleet grid need.
/// </summary>
public interface IClientHealthSnapshotRepository
{
    void Save(ClientHealthSnapshotRecord snapshot);

    /// <summary>Latest snapshot per terminal, newest first, at most <paramref name="maxResults"/> terminals.</summary>
    IReadOnlyList<ClientHealthSnapshotRecord> LoadLatestPerTerminal(int maxResults = 500);
}

/// <summary>
/// Parser output ledger (SS8 ingest lane: <c>journal archive → parser → parser_transactions</c>).
/// Batch-inserts the transactions parsed for one archived file, keyed by the archive ingestion id.
/// </summary>
public interface IParserTransactionRepository
{
    void InsertBatch(string ingestionId, IReadOnlyList<ParserTransactionRecord> rows);

    IReadOnlyList<ParserTransactionRecord> QueryByIngestion(string ingestionId, int maxRows = 5_000);

    long CountByIngestion(string ingestionId);
}

/// <summary>
/// Chunked-transfer session persistence (SS4: "add explicit <c>BitArray ReceivedChunksBitmap</c>
/// + persist transfer_sessions"). This is what lets a 64 MB transfer interrupted at 50 % resume
/// with &lt; 5 % retransmit (SS-13) across an agent restart.
/// </summary>
public interface ITransferSessionRepository
{
    void Save(TransferSessionRecord session);

    TransferSessionRecord? Load(string transferId);

    void MarkCompleted(string transferId, DateTime completedUtc);
}

/// <summary>
/// Terminal discovery/registry ledger. Upserted from the server when telemetry arrives for an
/// unknown terminal, refreshed on every heartbeat — the durable half of the fleet view.
/// </summary>
public interface IATMRegistryRepository
{
    void Upsert(AtmRegistrationRecord registration);

    IReadOnlyList<AtmRegistrationRecord> Snapshot(int maxRows = 1_000);
}

// ---------------------------------------------------------------------------
// Entities (records — SS-15 file shape: DTOs are records).
// ---------------------------------------------------------------------------

/// <summary>One archived journal file as recorded by the ingest pipeline.</summary>
public sealed record JournalArchiveRecord(
    string EntryId,
    string AtmId,
    string FileName,
    long OriginalSize,
    long CompressedSize,
    long EncryptedSize,
    bool IsEncrypted,
    bool IsCompressed,
    string Checksum,
    string Sha256Hash,
    int TransactionCount,
    string ArchivePath,
    DateTime ReceivedAtUtc)
{
    /// <summary>Month partition key the archive tree uses (<c>yyyy-MM</c>, UTC).</summary>
    public string MonthPartition => ReceivedAtUtc.ToString("yyyy-MM");
}

/// <summary>One command-policy decision (allow or deny) or command result.</summary>
public sealed record CommandAuditRecord(
    string AuditId,
    string CommandId,
    string OperatorId,
    string Action,
    string? DetailsJson,
    DateTime TimestampUtc,
    string? AtmId = null,
    string? ArgsHash = null,
    string Outcome = "Pending",
    int? LatencyMs = null);

/// <summary>Projected client health (what the service writes every 30 s batch).</summary>
public sealed record ClientHealthSnapshotRecord(
    string SnapshotId,
    string AtmId,
    string AgentState,
    bool NetworkConnected,
    string? SessionId,
    DateTime? LastHeartbeatUtc,
    DateTime? LastSyncUtc,
    int OutboxCount,
    int ErrorCount,
    string? LastError,
    DateTime SnapshotUtc);

/// <summary>One parsed transaction projected for one ingestion run.</summary>
public sealed record ParserTransactionRecord(
    int? TransactionNumber,
    DateTime? DateUtc,
    string AtmId,
    string Vendor,
    string? CardMasked,
    decimal? Amount,
    string? Currency,
    string? Stan,
    string? Rrn,
    string? MCode,
    string? RCode,
    string? HostResponse,
    string Status,
    string Confidence,
    int? RawStartLine,
    int? RawEndLine,
    string? Evidence);

/// <summary>Persisted transfer session with its resume bitmap. Bitmap indices are stored as a
/// comma-separated ascending list of received chunk indexes — human-inspectable in the raw file.</summary>
public sealed record TransferSessionRecord(
    string TransferId,
    string AtmId,
    string FileName,
    long FileLength,
    int ChunkSize,
    int TotalChunks,
    string ReceivedChunksCsv,
    string? FileSha256,
    long NextExpectedOffset,
    DateTime CreatedUtc,
    DateTime? CompletedUtc);

/// <summary>Registry row for one terminal.</summary>
public sealed record AtmRegistrationRecord(
    string AtmId,
    string AtmName,
    string AtmType,
    string IpAddress,
    DateTime RegisteredAtUtc,
    DateTime? LastHeartbeatUtc,
    DateTime? LastDataReceivedUtc);
