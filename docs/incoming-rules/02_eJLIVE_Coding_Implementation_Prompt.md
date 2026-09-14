# 02 · eJLIVE Coding Implementation Prompt (binding)

> Code-level implementation rules. Every new upload is checked against these
> rules before it lands in a compile map. The companion tool
> `tools/incoming/incoming_rules.py --check-implementation <upload>` reports
> per-rule verdicts.

## K. File shape

**K-1.** One public type per file (partials only inside the owning
assembly). File name = type name (`JournalSyncTrackingService.cs` →
`class JournalSyncTrackingService`).

**K-2.** `sealed` by default. Use `partial` only when splitting a class
inside the same assembly.

**K-3.** `record` for DTOs. `internal` until a consumer exists. No
`public static` mutable state except documented registries
(`EjParserRegistry`, `XfsAdapterRegistry`, `ServiceRegistry`).

**K-4.** `nullable` reference types are on (`Directory.Build.props`).
`string?` is the only "optional" marker; no `[Optional]` attributes,
no `bool HasValue + T Value` twins.

**K-5.** No `async void` except event handlers. No `catch { }` without
an inline reason. Every P/Invoke declares `SetLastError = true`,
`CharSet.Unicode` and a tested failure path.

**K-6.** Brace balance: `{` count equals `}` count; minimum depth never
goes negative (gate SYN-1). No file under 3 non-whitespace characters
(SYN-3).

**K-7.** Modifiers: no `public partial public class …`. No duplicated
modifier tokens on a single declaration (gate SYN-2).

## L. SQL

**L-1.** Engine: SQLite, WAL, `synchronous=NORMAL`, `cache_size=4000`,
`temp_store=MEMORY` — set as one PRAGMA per statement (merged PRAGMA list
is a syntax error).

**L-2.** Constraints (mandatory, not optional polish):
```sql
CREATE UNIQUE INDEX ux_journal_offsets_terminal_offset ON journal_offsets(terminal_id, offset_bytes);
CREATE UNIQUE INDEX ux_archive_terminal_offset        ON journal_archive(terminal_id, offset_bytes);
CREATE        INDEX ix_outbox_due                     ON client_outbox(next_attempt_utc, attempts);
CREATE UNIQUE INDEX ux_transfer_chunk                 ON transfer_sessions(session_id, chunk_index);
CREATE        INDEX ix_cmd_audit_actor_time           ON command_audit(actor, issued_utc);
CREATE UNIQUE INDEX ux_correlation_trace              ON correlation_events(trace_id);
```

**L-3.** Idempotent upserts keyed by `(terminal_id, offset_bytes)`;
retries use the outbox, never a re-read of the vendor file. Retention is
an explicit job (`daily_stats` + `prune`), never a trigger.

**L-4.** `users.password_hash` is `PBKDF2-SHA256` (≥ 210 000 iterations,
per-user salt). `PasswordDeriveBytes` is rejected by the gate.

**L-5.** Migration runner: `DatabaseMigrationsRunner.FromDatabaseFile(path)`
or `new DatabaseMigrationsRunner(connString)`. Forward-only, numbered
`NNNN_Name.sql`, applied in one transaction with
`PRAGMA foreign_keys=OFF` during the swap. `RunAll()` returns applied
count and logs each name to `schema_migrations` with its checksum.

## M. Parsing

**M-1.** `IEjTransactionParser` is the only parsing contract:
```csharp
public interface IEjTransactionParser
{
    string Vendor { get; }                                        // "NCR", "GRG", ...
    bool CanParse(ReadOnlySpan<byte> head);                       // sniff <= 4 KiB, no allocation
    IEnumerable<EjTransaction> Parse(Stream journal, JournalParseContext ctx, CancellationToken ct);
}
```

**M-2.** One compiled parser per vendor (POL-1). Auxiliary readers
(config capability, distribution, trace) must not claim parser ownership;
adapters feed the parser.

**M-3.** Parser invariants (every vendor, no exceptions):
1. Line-oriented, offset-exact; same offset re-emit is idempotent.
2. No cross-line recovery; an unterminated trailing line is held for the
   next window.
3. Amounts parsed with `decimal.TryParse(..., NumberStyles.Currency, vendorCulture)`;
   negative amounts kept (reversals first-class); currency from the
   vendor's own token.
4. `TraceId` = `terminal:yyyymmdd:receipt`; missing receipt →
   `receipt = sha256(line)[..10]` and `Synthetic=true`.
5. PAN/track data redacted at parse time through `LogRedactionEngine.Redact`
   → `first6 + '*'×n + last4` (or dropped for `Observer`).
6. Vendor vocabulary preserved as parsed tokens in `VendorRaw`, mapped to
   `TransactionKind` in the registry, never renamed inline.
7. Each vendor ships at least one `.LOG` fixture under
   `src/EJLive.Tests/Samples/` and one test asserting line count,
   first/last `EjTransaction`, total amount per `TransactionKind`, and
   redaction hit-count.

## N. Network / transport

**N-1.** One frame per `NetworkStream`, no nesting. Header is
`<MsgType>:<byteLength>\n`. Reader accumulates to `\n` with a hard cap
(64 bytes) then reads exactly `byteLength`.

**N-2.** Unknown `MsgType`, oversized header, truncated body ⇒ close +
`Error` frame (protocol, not transport, faults are distinguished in logs).

**N-3.** Session security:
  (1) client requests `RsaPublicKey`; server answers with a 2048-bit RSA
      public key + a self-signed X-509 whose SHA-256 thumbprint is pinned
      in configuration;
  (2) client generates 256-bit AES session key + IV, sends `AesSessionKey`
      encrypted with `RSAEncryptionPadding.OaepSHA256`;
  (3) both sides switch to `AES-CBC` (PKCS#7) for bodies and keep a
      per-direction sequence number;
  (4) each subsequent control message carries an HMAC-SHA256 signature
      over `version|msgType|sequence|sha256(body)` keyed by the session
      key's derived MAC key (HKDF-style split: `enc`/`mac` labels).
  Replays fail on sequence monotonicity, not on timing.

**N-4.** Adaptive chunking between 16 KiB and 256 KiB (default 64 KiB;
grow on RTT < 40 ms and no NAK; halve on timeout, floor 16 KiB). A
session is resumable: `transfer_sessions` persists the received-chunk
bitmap, so a reconnect sends `StartFile` again and the server responds
with the missing-chunk list.

## O. Observability

**O-1.** `StructuredLogger` (JSON lines) with: UTC timestamp, level,
`terminalId`, `sessionId`, `commandId`, `traceId`, `latencyMs`,
`outcome`, redacted `detail`.

**O-2.** Sinks: rolling file (`logs/ejlive-.log`, 14 files, 8 MB cap),
`telemetry_events` table (server), `EventLog` for service start/stop
only.

**O-3.** Levels: `Debug` off in Release, `Info` for state transitions,
`Warn` for retryable, `Error` for terminal per-op, `Critical` for host
loss. Telemetry cadence 1 s pulse, 30 s batch push.

**O-4.** Redaction is a *sink-side* guarantee: every write goes through
`LogRedactionEngine`. A forgotten call site cannot leak.

**O-5.** Health surface: snapshot object (no HTTP) consumed by the
companion, the NOC and `RunClientTelemetryProbeAsync`. Every shutdown
path emits `ShutdownReason` (event name in SS14); every teardown race
carries an inline comment.

## P. Code patterns (mandated, see `02_eJLIVE_Coding_Implementation_Prompt.md`)

**P-1.** Result-carrying API.
```csharp
public sealed record CommandOutcome(bool Accepted, string Reason, CommandRiskLevel Risk, int? LatencyMs = null);
```

**P-2.** Idempotent ingest.
```csharp
public async Task<IngestResult> IngestAsync(JournalEntry entry, CancellationToken ct)
{
    await using var tx = await connection.BeginTransactionAsync(ct);
    var applied = await InsertIfOffsetUnseenAsync(entry, ct);
    if (applied) { await AdvanceOffsetAsync(entry, ct); }
    await tx.CommitAsync(ct);
    return applied ? IngestResult.New : IngestResult.Duplicate;
}
```

**P-3.** Parser shape (see M-1 above).

**P-4.** Fail-closed security check.
```csharp
public static bool IsHashAlgorithmPermitted(string algorithm, string usage) =>
    usage.Equals("integrity", StringComparison.OrdinalIgnoreCase)
        ? PermittedIntegrity.Contains(Normalize(algorithm))
        : MigrationUsages.Contains((Normalize(algorithm), usage));
```

**P-5.** UI handler — thin, async, cancellable, no business logic.
```csharp
private async void retryButton_Click(object sender, EventArgs e)
{
    using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(2));
    retryButton.Enabled = false;
    try { var n = await outbox.FlushNowAsync(cbs: progress, cts.Token); statusLabel.Text = $"{n} spooled"; }
    catch (OperationCanceledException) { statusLabel.Text = "Retry cancelled"; }
    finally { retryButton.Enabled = true; }
}
```

## Q. Anti-patterns (banned, found in the original repo)

- Merged multi-type dumps > 1 MiB.
- `public partial public class …` scanner output.
- Per-project glob `Services\*.cs` beside an explicit list (double
  inclusion).
- A second assembly with the same `AssemblyName`.
- `AssemblyName` ≠ project name outside the shipped-exe allowlist.
- UI code in the Windows service.
- Two ADO.NET providers in one assembly.
- Reflection used to reach a type that could be a referenced contract.
- Arabic/English mixed prose inside identifiers or SQL literals.

## R. Operator-visible language

**R-1.** Operator-visible strings flow through `LanguageManager`
resources (en + ar). Source prose (comments, identifiers, commit
messages) is English-only.

## S. Tests and probes

**S-1.** `src/EJLive.Tests/` 42 fixtures / 371 cases. New tests must
carry an `SS-nn` reference.

**S-2.** `src/EJLive.Verification/Program.cs` 23 in-process probes.
New probes must carry an `SS-nn` reference and assert the gate
invariant they cover.

## T. Definition of Done (SS18)

1. `dotnet build EJLive.Platform.sln -c Release -m:1 /p:BuildInParallel=false` — 0 errors, 0 new warnings.
2. `dotnet test src/EJLive.Tests/EJLive.Tests.csproj` — 371+ cases, 0 failures, 0 skipped without a reason file.
3. `dotnet run --project src/EJLive.Verification/EJLive.Verification.csproj -c Release --no-build` — 23/23 PASS.
4. `python3 tools/inventory/ejlive_inventory.py` then `--check` → `ledgers fresh`; activation ledger fresh.
5. `python3 tools/gates/ejlive_static_gate.py` → `gate result: PASS` (41 rules), no new allowlist entries.
6. `artifacts/ActiveCompileMap.csv` — 0 stale includes, 0 unmapped sources, 0 intra-assembly duplicate keys.
7. `docs/DEBT-LEDGER.md` — every remaining entry has an owner and an exit condition.
8. Three payloads package, install on a clean Windows 10/11 + Server 2019+ box, self-test green against `Samples/*.LOG`, and a 64 MB transfer interrupted at 50 % resumes correctly.
9. No UI thread handler above 50 ms, no `.Result`, no `unsafe`, no WPF, no web UI, one parser per vendor.
10. PR merges only with CI green; commits carry `SS-nn` references for spec-traceable changes.
