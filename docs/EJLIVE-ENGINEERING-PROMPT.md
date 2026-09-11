# EJLIVE.PLATFORM — Standalone Engineering Prompt (Rev 3)

> Self-contained construction contract. A team holding only this file, the ledgers
> under `docs/inventory/`, `artifacts/ActiveCompileMap.csv` and the gate in
> `tools/gates/` can build, extend, package and certify EJLIVE.PLATFORM without
> opening any legacy source. Every normative statement below is either (a) verified
> against the compiled tree, or (b) marked `TARGET` (to be built) or `DEBT` (documented
> deviation, see `docs/DEBT-LEDGER.md`).
>
> Build/verify commands (Windows, .NET 8 SDK pinned in `global.json`):
>
> ```
> powershell tools/build/build.ps1 -Configuration Release -WithTests -WithGate
> python3 tools/inventory/ejlive_inventory.py            # ledgers (INV-1, per push)
> python3 tools/gates/ejlive_static_gate.py              # 40 rules, exit code = verdict
> ```
>
> **SS = section. Reference SS-nn in commit messages when a change implements it.**
>
> Measured state of the tree this contract was written against (regenerated, not typed):
> 14 projects · 303 compiled files / 57 551 lines · 237 linked-reference files / 724 249
> lines · 0 stale includes · 0 sources outside a compile map · 0 intra-assembly duplicate
> type keys · 18 cross-assembly partial splits (D-01) · 10 unparsable dumps archived (D-02)
> · 19 database tables · 22 wire message types · 23 verification probes · 371 test cases
> · 99 service-activation rows · 40 gate rules, all PASS.

---

## SS1 · Identity, scope, hard rules

| item | value |
|---|---|
| product | EJLIVE.PLATFORM — ATM electronic-journal (EJ) capture, archive, correlation, remote operations |
| topology | distributed client–server, Windows-only, no web tier |
| runtime | .NET 8 (`net8.0-windows`), SDK `8.0.404`, `rollForward: latestFeature` |
| UI | **Windows Forms only** — client, server, NOC, installer. No WPF, no WinUI, no Blazor, no web host, no external UI framework |
| assemblies | 14 projects (SS2); `EJLive.Platform.sln` is the build entry point (`global.json` pins the .NET 8 SDK), `EJLive.Platform.slnx` mirrors it for VS 17.13+ / SDK 9.0.2xx+ |
| data | SQLite file database, code-owned schema, forward-only migrations |
| language of artefacts | identifiers, comments, ledgers, commit messages: English. Operator-visible strings: `LanguageManager` resources (en + ar) — prose in code never mixes scripts |

Hard rules (gate-enforced; violating one is a build failure, not a review comment):

1. **FILE-4 / ARCH-1 / ARCH-3** one canonical `.csproj` per project directory, every project listed in both
   solution files, and `src/_reference/` never referenced by a runtime project. **ARCH-4** keeps references
   strictly downward through L0-L5.
2. **POL-1** one compiled journal parser per vendor. Auxiliary readers (config capability, distribution, trace) must not claim parser ownership; adapters feed the parser.
3. **SEC-1** no loaded vocabulary in code or prose; annotate framework names with `// safe:` only where the API is external (`.Kill()` is exempt by rule, not by annotation).
4. **SEC-2/SEC-3** no `unsafe`, no `AllowUnsafeBlocks`, no weak crypto on a security path (`MD5`/`SHA-1`/`DES`/ECB allowed only for vendor archive fingerprints, with a `// safe:` or `// safe-file:` reason).
5. **SYN-1/SYN-2/SYN-3** every file in a compile map must be structurally parsable, single-modifier, non-empty. Auto-merge dumps are archive material, never source. **SYN-4** every csproj must parse *and* nest its items inside an `<ItemGroup>`: a stray `<Compile>` under `<Project>` is valid XML, passes every textual tool, and aborts `restore` for the whole solution with MSB4067.
6. **TYPE-1/TYPE-2** exactly one compiled owner per type key per assembly; cross-assembly partial splits are illegal unless recorded as debt.
7. **INV-1 (LED-1/LED-2/LED-3)** ledgers regenerate on every push; a hand-edited ledger fails the gate.
8. **POL-2/POL-3** no synchronous wait on a UI thread; no silent `catch { }` — an empty catch carries an inline reason.
9. **DEP-1/DEP-2** one ADO.NET provider per assembly; one version per package across the graph.
10. Ledger rules (checked in `docs/inventory/`, not by the compiler): **DB-1** every table has a `CREATE` *and* a
    DML consumer; **UI-1** every form binds at least one handler and exposes at least one command method;
    **POL-1** status is per-vendor in `docs/inventory/VENDORS.md`. Both ledgers are generated, so a violation is a
    row, not an opinion.

## SS2 · Project topology (authoritative)

```
EJLive.Shared              L0  primitives, logging, redaction, UI resources, config model
EJLive.Core                L1  models, wire protocol, transport, parsers, engine, XFS adapters, data layer
EJLive.Business            L2  service composition, gateways, journal storage/analytics, command policy
EJLive.Application         L3  installation automation, platform services, operational security
EJLive.Client.Service      L4  endpoint Windows service (worker host, headless agent)
EJLive.Server             L4  headless server services (library; hosted by the WinForms surface)
EJLive.Client.WinForms     L4  Endpoint Console  -> ships as EJLive.Client.exe
EJLive.Server.WinForms     L4  Enterprise Server -> EJLive.Server.WinForms.exe
EJLive.Monitoring.WinForms L4  NOC / Windows Operations Console -> EJLive.Monitoring.exe
EJLive.Installer.WinForms  L4  installer UI      -> EJLive.Installer.exe
EJLive.UnifiedLauncher     L5  single entry point: `EJLive.UnifiedLauncher.exe [client|server]`
EJLive.Tests               L5  42 fixtures, 371 cases (MSTest + xUnit)
EJLive.Verification        L5  23 in-process probes; CI gate; `--no-build` after build
EJLive.LegacyReference     --  read-only link project over src/_reference (never compiled content)
```

Layer rule (ARCH-4): a project may reference a strictly lower layer only. Deployed exe names differ from project
names for the four shipped surfaces; that mapping is the allowlist in the gate (`ASSEMBLY_ALLOWLIST`) and must
stay in sync with `tools/package/package.bat`.

## SS3 · Runtime processes

| process | owns | loop | shuts down on |
|---|---|---|---|
| `EJLive.Client.Service.exe` | agent: file watchers, EJ capture, outbox, XFS poll, telemetry | worker `ExecuteAsync`, cancellation token, 1 Hz health pulse | `SIGTERM`-equivalent service stop, ≤ 5 s drain |
| `EJLive.Client.exe` (Endpoint Console) | operator view, session companion, elevation prompt, local health read | message pump; **no** business logic | user close (tray-resident, `NotifyIcon`) |
| `EJLive.Server.WinForms.exe` (Enterprise Server) | ingest, archive, correlation, command dispatch, journal query | accept loop + `System.Threading.Channels` fan-out | admin stop, WAL checkpoint |
| `EJLive.Monitoring.exe` (NOC) | fleet dashboard, alert triage, report export | snapshot poll 2 s + push updates | user close |
| `EJLive.Installer.exe` | payload staging, service registration, companion probe | step machine (SS16) | any step failure → rollback |

Endpoint pairing is a contract: the installer starts `EJLive.Client.exe` as a **session companion** of the service
(`InstallerAutomationRunner` probes the service payload for that exact file name). The service is headless; any
UI dependency in it is a defect the `RunUiInServicePathProbe` probe rejects.

## SS4 · Domain model (`EJLive.Core`, namespace `EJLive.Core.Models`)

Reconciliation first — the tree already owns most of this vocabulary, and a wave must rename
*with* the ledgers, never around them. Left column is the normative name (this contract); right
column is what compiles today.

| normative | current owner (verified) | disposition |
|---|---|---|
| `ATMInfo` | `Core/ATMInfo.cs` | keep; add `ATMStatus`→`ATMState` alias removal in Wave 2 (`ATMStatus`, `ATMOperationalState`, `ATMState` are three enums for one idea) |
| `ATMTransaction` / `Transaction` | `Core/Models/UnifiedModels.cs:444`, `Core/Models/Transaction.cs` (four partials) | collapse to one `EjTransaction` record; keep `ATMTransaction` as a deprecated alias for one release |
| `TransactionKind` | `CommandType`, `ATMType`, `JournalFileType`, `FraudType` fragments | introduce one `TransactionKind` (Deposit, Withdrawal, Balance, Transfer, Reversal, Inquiry, Maintenance, Other) + `ToLegacy` mapper |
| `JournalEntry` + offsets | `Core/Data/JournalOffsetStore.cs`, `Core/Engine/JournalOutbox.cs` | keep |
| `TransferSession` + bitmap | `Core/Engine/ChunkedTransferEngine.cs` (`Session.ChunkSize`, `StartFile/Chunk/ChunkAck/Complete`) | add explicit `BitArray ReceivedChunksBitmap` + persist `transfer_sessions` |
| `RemoteCommandEnvelope` + signing | `Core/Engine/CommandSigningEngine.cs`, `Core/Engine/SecurityPolicy.cs` | keep; version field is mandatory (SS5) |
| `UserRole`, `Permission`, `CommandRiskLevel` | `Core/Services/RoleBasedAccess.cs:449`, `Core/Engine/CommandRiskLevel.cs` | keep; matrix in SS9 is the evaluator's truth table |
| redaction / masking | `Core/Engine/LogRedactionEngine.cs`, `Core/Security/SecurityHelper.cs`, `Shared/SecurityConfig.cs` | keep; sink-side guarantee (SS14) |

Target record shapes (build these; names are binding, the legacy spellings retire in Wave 2):

```csharp
public sealed record ATMInfo(string TerminalId, string Vendor, string Model, string Location,
                             string IpAddress, int Port, ATMState State, DateTimeOffset LastSeenUtc);
public enum ATMState { Unknown, Online, Offline, InService, OutOfService, Suspended }
public enum JournalKind { VendorJournal, VendorTrace, XfsLog, Supplement }
public sealed record JournalEntry(string TerminalId, DateTimeOffset TimestampUtc, long OffsetBytes,
                                  int LengthBytes, string VendorRaw, JournalKind Kind);
public sealed record EjTransaction(string TerminalId, string TraceId, DateTimeOffset TimestampUtc,
                                   TransactionKind Kind, decimal Amount, string Currency,
                                   string ReceiptNumber, IReadOnlyList<string> Lines,
                                   TransactionOutcome Outcome);
public sealed record TransferSession(string SessionId, string PeerId, string FileName, long TotalBytes,
                                     int ChunkSize, BitArray ReceivedChunksBitmap, TransferState State);
public sealed record RemoteCommandEnvelope(Guid CommandId, int Version, string Verb,
                                           IReadOnlyDictionary<string, string> Args,
                                           CommandRiskLevel Risk, string Actor,
                                           DateTimeOffset IssuedUtc, byte[] Signature);
```

Identity rules: `TerminalId` is the primary key of the fleet (string affinity, never renumbered,
vendor prefix allowed); `TraceId` is `terminal:yyyymmdd:receipt` and is the correlation join key; journal
offsets are byte offsets into the vendor log file, monotone, never reconstructed from `LastWriteTime`.
All timestamps are UTC `DateTimeOffset`; local display happens only inside a control binding, never in a
persisted value. `VendorRaw` keeps the original line for audit replay and is redacted before display to
`Observer`/`Auditor` roles (SS9).

## SS5 · Wire protocol (single source: `EJLive.Shared` — TARGET of Wave 1, see DEBT D-07)

**Envelope.** One frame per `NetworkStream`, no nesting:

```
<MsgType>:<byteLength>\n<body: byteLength bytes>
```

`byteLength` counts body bytes only, ASCII decimal, no padding, terminated by `\n` (0x0A). A reader must not
assume the header fits one `Read`; it accumulates to `\n` with a hard cap (64 bytes) then reads exactly
`byteLength`. Unknown `MsgType`, oversized header, or truncated body ⇒ close + `Error` frame (protocol, not
transport, faults are distinguished in logs).

**Message set (22).** `Unknown`, `RsaPublicKey`, `AesSessionKey`, `Handshake`, `HandshakeAck`, `Heartbeat`,
`HeartbeatAck`, `StartFile`, `Chunk`, `ChunkAck`, `Complete`, `JournalAck`, `Command`, `CommandResult`,
`RemoteSessionStart`, `RemoteSessionFrame`, `RemoteSessionStop`, `ImageSync`, `ImageAck`, `Broadcast`,
`Disconnect`, `Error`. Bodies are UTF-8 JSON for control messages (`System.Text.Json`, camelCase,
`JsonStringEnumConverter`) and raw bytes for `Chunk`/`RemoteSessionFrame`/`ImageSync`.

**Ports.** Server listener `5656` (`AppConstants.DefaultPort`), configurable via `client.config`/`server.config`;
no port scanning, no UDP, no multicast.

**Session security.** (1) client requests `RsaPublicKey`; server answers with a 2048-bit RSA public key
(`RSA.Create(2048)`) plus a self-signed X-509 whose SHA-256 thumbprint is pinned in configuration
(`TrustedThumbprints` — configuration key introduced by this contract); (2) client generates a 256-bit AES session key + IV, sends `AesSessionKey` encrypted with
`RSAEncryptionPadding.OaepSHA256`; (3) both sides switch to `AES-CBC` (PKCS#7) for bodies and keep a
per-direction sequence number; (4) each subsequent control message carries an HMAC-SHA256 signature over
`version|msgType|sequence|sha256(body)` keyed by the session key's derived MAC key (HKDF-style split:
`enc`/`mac` labels). Replays fail on sequence monotonicity, not on timing.

**Command signing is versioned.** `CommandEnvelopePolicy.Sign` embeds `Version`; a verifier accepts only the
version advertised at handshake time. `DEBT`: `CommandSigningEngine` must reject `Version <= 0`.

**Transfer + resume.** `StartFile{ fileName, totalBytes, chunkSize, sha256 }`; server replies `ChunkAck` per
chunk, adaptive `chunkSize` between 16 KiB and 256 KiB (default 64 KiB; grow on RTT < 40 ms and no NAK, halve on
timeout, floor 16 KiB). A session is resumable: `transfer_sessions` persists the received-chunk bitmap, so a
reconnect sends `StartFile` again and the server responds with the missing-chunk list. Completion is accepted
only when SHA-256 of the reassembled file matches.

**Heartbeat.** 15 s client / 45 s server timeout; `Heartbeat{ uptimeS, queueDepth, diskFreeKb }`,
`HeartbeatAck{ serverTimeUtc }` used for clock-skew display (never for scheduling).

## SS6 · Endpoint agent (`EJLive.Client.Service`)

Verified boot path today (`Program.cs`, 21 compiled files):

```csharp
var builder = Host.CreateApplicationBuilder(args);
builder.Logging.AddEventLog(s => { s.SourceName = "EJLive-Client-Source"; s.LogName = "Application"; });
builder.Services.AddWindowsService(o => o.ServiceName = "EJLive Client Agent Service");
builder.Services.AddSingleton<RuntimeAgentConfigResolver>();
builder.Services.AddHostedService<ClientAgentWindowsService>();
await builder.Build().RunAsync();
```

Target shape (adds are `TARGET`: implement in Wave 1, each behind a build step):

```
RuntimeAgentConfigResolver  ->  ClientAgentWindowsService (ExecuteAsync)
   ├─ FileWatcherEngine            (Core/Engine)     per-vendor journal paths
   ├─ SafeLiveFileReader          (Compatibility)   share-safe tail + CompleteLineReadWindow
   ├─ JournalOffsetStore          (Core/Data)       offset commit, unique index
   ├─ JournalOutbox               (Core/Engine)     spool pump -> client_outbox
   ├─ SecureHandshakeService      (Core/Engine)     RSA-2048 + AES-256-CBC + HMAC framing
   ├─ ChunkedTransferEngine       (Core/Engine)     adaptive 16-256 KiB chunks, bitmap resume
   ├─ AgentHealthReporter         (project)         1 s pulse -> telemetry_events
   └─ ClientServiceRegistry       (TARGET, from _reference)  replaces reflection in
                                                            Core/Services/ServiceLocator.cs
```

Responsibilities and failure posture:

| duty | mechanism (verified owner) | on failure |
|---|---|---|
| journal capture | `SafeLiveFileReader` (`FileShare.ReadWrite`) + `FileWatcherEngine`; offsets through `JournalOffsetStore` | keep offset, retry 1 s→60 s exponential, never advance past a partial line |
| spool & ship | `JournalOutbox` pump over `client_outbox`, ≤ 200 rows per transaction | row stays; after 10 attempts move to `outbox_dead_letters` with fault text (table: `TARGET`, SS11) |
| vendor trace | `XfsAdapterRegistry` + `Core/Xfs/Adapters/{NcrCardReaderTraceAdapter,NcrDebugTraceAdapter,NcrHostMessageInAdapter,NcrHostMessageOutAdapter,GrgJournalAdapter,DieboldMdsAdapter}` | disable adapter for the session, surface a `vendor_events` row |
| remote ops | `SafeRemoteCommandExecutor` + `GhostRemote2Service` (view-only), risk-tiered by `RoleBasedAccess` | explicit reject + `CommandResult{Denied}`; never silent, never unlogged |
| config | `AgentConfigurationXmlService` (`client.config`), DPAPI-protected secrets via `SecretProtector` | last-known-good kept, `ConfigValidationFailed` broadcast (`TARGET` event name) |
| elevation / autostart | `AutoAdminSetup` (`HKLM ...\Run`, elevated-only) archived at `_reference/uncompiled/EJLive.Client.WinForms/Agent/` | refuse to write `HKCU`, report `ElevationRefused` (`TARGET` event name) |
| log retention | `LogBackupScheduler` (2 owners: agent + companion) | 14 daily files, 8 MB cap, never delete the active file |

Hard rule: the service is headless. `RunUiInServicePathProbe` (verification) fails the
build if a `System.Windows.Forms` control type appears on the service path; UI lives in
`EJLive.Client.WinForms` and talks to the service through `ClientCompanionGateway`.

## SS7 · Vendor parsing (`EJLive.Core/Engine`, `EJLive.Core/Xfs/Adapters`)

`IEjTransactionParser` is the only parsing contract:

```csharp
public interface IEjTransactionParser
{
    string Vendor { get; }                                        // "NCR", "GRG", ...
    bool CanParse(ReadOnlySpan<byte> head);                       // sniff <= 4 KiB, no allocation
    IEnumerable<EjTransaction> Parse(Stream journal, JournalParseContext ctx, CancellationToken ct);
}
```

Owners (one per vendor, POL-1): `NcrEjTransactionParser`, `GrgEjTransactionParser`,
`DieboldEjTransactionParser`, `HyosungEjTransactionParser`, `WincorEjTransactionParser`,
`CashwayEjTransactionParser`. Registration is `EjParserRegistry.Resolve(stream)` — sniff first, then parse;
unmatched ⇒ `VendorRaw` journal stored, `parser_transactions` row with `Outcome=Unmatched` (never dropped).

Parser invariants (every vendor, no exceptions):

1. **Line-oriented, offset-exact.** A parser consumes only complete lines; the registry tracks a byte offset and
   re-emit of the same offset must be idempotent.
2. **No cross-line recovery.** An unterminated trailing line is held for the next window, never truncated.
3. **Amounts** are parsed with `decimal.TryParse(..., NumberStyles.Currency, vendorCulture)`, negative amounts
   kept (reversals are first-class), currency from the vendor's own token, never inferred from locale default.
4. **Trace correlation.** `TraceId` = `terminal:yyyymmdd:receipt`; where a vendor emits no receipt number,
   synthesize `receipt = sha256(line)[..10]` and set `Synthetic=true` on the row.
5. **Anonymisation.** PAN/track data is redacted at parse time through `LogRedactionEngine.Redact` — masked to
   `first6 + '*'×n + last4` (or dropped entirely for `Observer` role) and never persisted raw.
6. **Vocabulary.** NCR terms (`EJDATA`, `TRX`, `CFG`), GRG (`[TXN]`, `[BALANCE]`), Diebold MDS, Hyosung, Wincor,
   Cashway markers stay as parsed tokens in `VendorRaw` and are mapped to `TransactionKind`, never renamed inline.
7. **Fixture tests.** Each vendor ships at least one `.LOG` fixture under `src/EJLive.Tests/Samples/` and one test
   asserting: line count, first/last `EjTransaction`, total amount per `TransactionKind`, redaction hit-count.
   Fixtures are source files, never build output; the gate exempts `Samples/` deliberately.

## SS8 · Server (`EJLive.Server` + `EJLive.Server.WinForms`)

`EJLive.Server` is a library (`EJLive.Server.dll`) exposing `ServerEngine`, `IngestionPipeline`,
`JournalAnalyticsService`, `ClientTelemetryStateService`, `RemoteControlService`, `ServerAuditService`.
The WinForms surface is the process host; `DEBT D-05`: the retired `EJLive.Server.exe` name must not be revived
by packaging — copy `EJLive.Server.WinForms.exe` as the server entry point.

Server control loop (target shape, all on background threads, no UI affinity):

```
accept ─> handshake ─> decode frame ─> ingress channel ─┬─> journal archive ─> parser ─> parser_transactions
                                                          ├─> telemetry ─> client_health_snapshots, telemetry_events
                                                          ├─> transfer assembly ─> files + transfer_sessions
                                                          └─> command dispatch ─> policy ─> queue ─> ack path
```

Ingest rules: `JournalAck` only after the archive write commits **and** the offset store advances;
correlation runs after ingest (never inline in the socket loop); the outbox pump on the endpoint is the retry
authority, so the server must be idempotent on `(terminalId, offsetBytes)` pairs (unique index
`ux_journal_offsets_terminal_offset`).

## SS9 · Security, permissions, risk (three chokepoints)

Tiers: `CommandRiskLevel { Low, Medium, High, Critical }`. Roles:
`UserRole { Observer, Support, Auditor, Admin }` (`Permission` enumerates 12+ atoms: `ViewDashboard`,
`ViewATMStatus`, `ViewJournal`, `ViewAlerts`, `ViewLogs`, `ExportReports`, `ViewAuditLog`, `TakeScreenshot`,
`RemoteSessionView`, `SendCommands`, `RestartATM`, `ChangePassword`, ...).

Matrix (server-authoritative; `RoleBasedAccess` is the only evaluator; UI mirrors it, never the inverse):

| risk \ role | Observer | Support | Auditor | Admin |
|---|---|---|---|---|
| Low (status, telemetry) | allow | allow | allow | allow |
| Medium (screenshot, config read, journal query) | deny | allow | allow | allow |
| High (service restart, ATM restart, config write) | deny | deny | deny | allow, 1 confirm |
| Critical (shutdown, password change, firewall, purge) | deny | deny | deny | allow, 2 confirms + re-auth ≤ 60 s |

Enforcement points:
1. **Intake** — signature + version + sequence validity; expired or replayed ⇒ `Error`, logged to `command_audit`.
2. **Executor** — `RoleBasedAccess.HasPermission(role, risk)`; every decision (allow *and* deny) writes
   `command_audit` with actor, command id, args hash, outcome, latency.
3. **UI** — a control for a denied command is **disabled with a tooltip** naming the missing permission, never
   hidden (operators must see what exists and why it is inert).

Credential handling: DPAPI (`ProtectedData`) per machine+user for stored endpoint credentials; no plaintext in
config or logs (gate GIT-3 + `SecretRedactor`); transport secrets live only in memory; key material rotation =
re-issue the pinned thumbprint and restart the agent (no dual-key window).
Audit chain: `audit_log` rows carry `prev_hash` = SHA-256 over the previous row's canonical JSON; a verification
pass (`AuditLogger.VerifyChain`) reports the first broken link — this is the tamper-evidence contract.

## SS10 · Windows Forms UX specification

Design language (identical across surfaces; implemented once in `EJLive.Shared/UI`):
`ThemeColors` + `ToolStrip`-based ribbon-lite headers, `TableLayoutPanel` skeletons, `Anchor`+`Dock` only
(no absolute pixel layout), minimum client area 1280×720, DPI-aware `PerMonitorV2` (declared in `app.manifest`),
`AutoScaleMode.Dpi`, keyboard-first: `Ctrl+F` filter, `F5` refresh, `Esc` close dialog, `Ctrl+Enter` submit,
tab order = visual order, mnemonics on every command label, `AccessibleName` on every data cell control.
Status discipline: every long operation shows progress + elapsed + cancel; failures show the *actionable* line
(the same text the log records) with a `Copy details` affordance.

**Shared shell pattern** — each surface composes:
`MenuStrip(File · View · Operations · Help)` + `ToolStrip(refresh | filter | export | theme | about)` +
central `SplitContainer(list ⇄ detail)` + `StatusStrip(state · peer · latency · lastRefresh · version)` +
`NotifyIcon` (client/NOC only) with `Show/Hide/Exit`.

### SS10.1 Endpoint Console — `EJLive.Client.WinForms` (`ClientMainForm`, ships `EJLive.Client.exe`)

| control | function | binding (code path) |
|---|---|---|
| `serviceStateLabel` + `stateDot` | headless service state (Running/Stopped/Degraded) | `ClientCompanionGateway.QueryHealth()` → `AgentRuntimeSnapshot` |
| `configPathTextBox` | last-known-good config path, read-only | `ClientPathConfigManager` |
| `journalTailTextBox` (virtual) | live EJ tail, 500 lines, redacted | `SafeLiveFileReader` + `LogRedactionEngine` |
| `offsetGrid` | per-file offset, last commit, lag bytes | `JournalOffsetStore` |
| `outboxGrid` + `retryButton` | spool depth, retry-now (Low risk) | `JournalOutbox.FlushNow()` |
| `linkPictureBox` + `linkProbeButton` | server link state, RTT, thumbprint match | `NetworkEngine` + `SecureHandshakeService` |
| `startupModeComboBox` | Full / Background / Diagnostics | `ClientStartupPlanner.Create(args, isAdministrator)` |
| `elevateButton` | relaunch elevated with `AutoStartArgument` | `AutoAdminSetup` |
| `trayMenu` | show/hide, pause capture, log folder | `AgentTrayContext` |
| `selfTestButton` | run vendor fixture parse against `Samples/*.LOG`, report | `EjParserRegistry.Resolve` |

Startup state machine: `ClientStartupPlanner` decides `Interactive | Background | ElevatedRelaunch | Reject`;
the mutex (`AgentMutexName`) guarantees one companion per session; `--background` never opens a window and never
blocks the service.

### SS10.2 Enterprise Server — `EJLive.Server.WinForms` (`ServerMainForm`)

| control | function | binding |
|---|---|---|
| `listenerToggle` + `portNumeric` | start/stop accept loop | `ServerEngine.StartAsync/StopAsync` |
| `atmCardsPanel` (`ATMCardPanel`) | fleet cards: state, last EJ, lag, vendor badge | `ATMRealTimeStatusReducer` snapshot |
| `atmDetailDrawer` (`ATMDetailDrawerForm`) | per-ATM journal, offsets, XFS trace, files | `UnifiedJournalStorageService` |
| `ingestGrid` | `parser_transactions` feed with `Outcome`/`VendorRaw` filter | `IngestionPipeline` query |
| `correlationPanel` | trace ⇄ archive ⇄ vendor-event join view | `CorrelationEngine` + `MergedTraceCorrelationService` |
| `syncDashboard` (`SyncDashboardForm`) | transfer sessions, chunk progress, resume actions | `ChunkedTransferEngine` |
| `commandBar` + `riskBanner` | dispatch commands; banner states risk tier + required confirms | `SafeRemoteCommandExecutor` + `RoleBasedAccess` |
| `archiveGrid` + `exportButton` | archive browser, CSV/JSON export | `ReportingExportEngine` |
| `settingsForm` (`ServerSettingsForm`) | port, retention, thumbprints, migration run | `ServerConfigStore` + `DatabaseMigrationsRunner` |
| `auditViewer` | `audit_log` + chain verification status | `AuditLogger.VerifyChain` |
| `themeToggle`, `languageComboBox` | light/dark, en/ar | `ThemeColors`, `LanguageManager` |

### SS10.3 Windows Operations Console — `EJLive.Monitoring.WinForms` (`MainDashboardForm`)

KPI strip (online/offline/degraded, EJ lag p95, outbox depth, dead letters) → `DashboardSnapshotService`;
fleet grid with column sort + `Ctrl+F` filter → `OperationalStateStore`; alert rail with acknowledge/resolve
(→ `AlertManager`, Medium risk) → writes `audit_log`; journal studio launcher (SS10.5) →
`UnifiedJournalEvidenceAnalyzer`; report launcher → `ReportingExportEngine`; NOC-only `Broadcast` composer
(High risk) → `ServerEngine.BroadcastAsync`.

### SS10.4 Installer — `EJLive.Installer.WinForms` (`InstallerForm`)

Steps: payload select → target dir + free-space check → service registration (elevated) → firewall rule (single
rule scoped to 5656, `Private` profile) → companion probe (`EJLive.Client.exe` present) → config write → self-test
→ activate. Every step is idempotent and reports a machine-readable line (`InstallerAutomationRunner` output),
and any failure triggers the recorded rollback set. `--install --service EJLive.Client.Service --silent` is the
unattended contract used by `tools/package/package.bat`.

### SS10.5 Electronic Journal Analysis Log Studio (TARGET — build in Wave 1)

Single tool window (`JournalStudioForm`, host: `EJLive.Server.WinForms`, menu *View → Journal Studio*):

- `openFileDialog` (vendor journal / `.LOG` fixture) → `loadSummaryLabel`: bytes, lines, detected vendor, parser,
  parse errors.
- `kindFilterCheckedListBox`, `amountRangeNumeric` pair, `dateRangePicker`, `terminalComboBox`, `searchTextBox`
  → all four feed one `BindingList<EjTransaction>` (no re-query per keystroke: 250 ms debounce).
- `transactionsGrid` (virtual mode, ≤ 5 000 rows/page) → `rawLineView` shows the exact source line with the
  byte offset highlighted; `receiptDiagramView` renders the parsed receipt ladder (kind, amount, balance,
  trace id, outcome) side-by-side with raw text.
- `anomalyPanel`: gap detection (missing sequence), duplicate receipt, impossible balance jump, unclosed
  session, non-monotone offsets — each anomaly is one row with `explainButton` (rule + evidence).
- `correlationStrip`: matched XFS trace ⇄ journal ⇄ server archive, with `unmatched` counts per side.
- `exportButton` (CSV, JSON, PDF-less print via `PrintDocument`), `redactPreviewCheckBox` (shows the
  `Observer`-safe view), `reparseButton` (re-runs the parser at the current offset window).
- analytics tabs: throughput/hr, kind histogram, reconciliation delta (terminal total vs archive total),
  vendor error Pareto. Every chart is a light owner-drawn `Panel` (no external chart framework — WinForms-only),
  fed by LINQ over the parsed set, and each exposes `Copy data` for the ledger.

Acceptance: parses all vendored fixtures with 0 exceptions; total per kind equals `journal_archive` totals;
studio never renders an unredacted PAN; every anomaly row carries offset evidence the operator can re-open.

## SS11 · Data layer

Engine: SQLite, WAL, `synchronous=NORMAL`, `cache_size=4000`, `temp_store=MEMORY` — set as one PRAGMA per
statement (a merged PRAGMA list is a syntax error). **DEBT D-06**: single provider (`Microsoft.Data.Sqlite`).

Tables (19, code-owned; every one needs a `CREATE` in `DatabaseSchema.cs`/a migration **and** a DML consumer —
`DB-1`):
`__migrations`, `schema_migrations`, `atm_registry`, `audit_log`, `client_health_snapshots`, `client_outbox`,
`outbox_dead_letters`, `command_audit`, `command_queue`, `correlation_events`, `daily_stats`, `journal_archive`,
`journal_offsets`, `parser_transactions`, `screenshot_history`, `sync_records`, `telemetry_events`,
`transfer_sessions`, `users`, `vendor_events`.
(`DEBT`: `__migrations` and `schema_migrations` are two books for one concern — collapse into
`schema_migrations(version INTEGER PRIMARY KEY, name, applied_utc, checksum, rolled_back INTEGER)`.)

Constraints that must exist (not optional polish; ledger rule **DB-1**):

```sql
CREATE UNIQUE INDEX ux_journal_offsets_terminal_offset ON journal_offsets(terminal_id, offset_bytes);
CREATE UNIQUE INDEX ux_archive_terminal_offset        ON journal_archive(terminal_id, offset_bytes);
CREATE        INDEX ix_outbox_due                     ON client_outbox(next_attempt_utc, attempts);
CREATE UNIQUE INDEX ux_transfer_chunk                 ON transfer_sessions(session_id, chunk_index);
CREATE        INDEX ix_cmd_audit_actor_time           ON command_audit(actor, issued_utc);
CREATE UNIQUE INDEX ux_correlation_trace              ON correlation_events(trace_id);
```

Rules: idempotent upserts keyed by `(terminal_id, offset_bytes)`; retries use the outbox, never a re-read of the
vendor file; retention is an explicit job (`daily_stats` + `prune`), never a trigger; `users.password_hash` is
`PBKDF2-SHA256` (≥ 210 000 iterations, per-user salt) — the gate rejects `PasswordDeriveBytes`; the schema
version is verified at service start and a newer-on-disk version **refuses to run** rather than downgrading.

Migration runner: `DatabaseMigrationsRunner.FromDatabaseFile(path)` or `new DatabaseMigrationsRunner(connString)`;
forward-only, numbered `NNNN_Name.sql`, applied inside one transaction with `PRAGMA foreign_keys=OFF` during the
swap; `RunAll()` returns applied count and logs each name to `schema_migrations` with its checksum.

## SS12 · Concurrency & threading

- One `Channel<T>` per pipeline stage (`BoundedChannelOptions(capacity: 1024, FullMode = Wait)`), no shared
  mutable collections between stages.
- UI updates only via `IProgress<T>` posted to the message loop or `Control.Invoke` guard; **never** `.Result`,
  `.Wait()`, `Thread.Sleep` on a UI thread (gate `POL-2`).
- File reading: `ArrayPool<byte>.Shared.Rent`, `FileOptions.SequentialScan`, `FileShare.ReadWrite`, and a
  `CompleteLineReadWindow` so a partial tail line is never surfaced or committed.
- Socket reads: one reader task per connection, `Stream.CopyToAsync`-style framing loop, `CancellationToken`
  propagated from the service host; a socket is disposed only by its reader.
- Timers: `System.Threading.Timer` in the service, `System.Windows.Forms.Timer` in the UI — the two are never
  interchangeable, and a `Forms.Timer` callback never touches a `Channel` writer without `TryWrite`.
- Locking: `_lock` around registry mutation only; collections returned to callers are snapshots
  (`ImmutableArray.ToImmutable()`), so a dashboard refresh cannot mutate the engine state.
- Every loop owns cancellation; shutdown drains for ≤ 5 s, then abandons and records `ShutdownAbandoned` (`TARGET` event name).

## SS13 · Performance & capacity targets

| metric | target | measurement |
|---|---|---|
| journal ingest | ≥ 12 MB/s per terminal, ≥ 200 terminals concurrently | server ingest counter, 60 s window |
| end-to-end EJ lag (write → archive) | p95 < 5 s at 4 MB file, < 15 s at 64 MB | `daily_stats` lag histogram |
| archive read | 5 000 rows filtered page < 200 ms | studio stopwatch test |
| parse throughput | ≥ 40 k lines/s per vendor parser | fixture benchmark, `Stopwatch` |
| memory | service RSS < 220 MB steady; UI < 180 MB | 30-min soak |
| UI frame budget | no handler > 50 ms on UI thread | `RunUiProbe` + manual profiler gate |
| resume | interrupted 64 MB transfer resumes with < 5 % retransmit | bitmap check in `transfer_sessions` |
| disk | WAL checkpoint < 2 s; archive growth bounded by retention job | maintenance log |

## SS14 · Observability

`StructuredLogger` (JSON lines) with: UTC timestamp, level, `terminalId`, `sessionId`, `commandId`, `traceId`,
`latencyMs`, `outcome`, redacted `detail`. Sinks: rolling file (`logs/ejlive-.log`, 14 files, 8 MB cap),
`telemetry_events` table (server), `EventLog` for service start/stop only. Levels:
`Debug` off in Release, `Info` for state transitions, `Warn` for retryable, `Error` for terminal per-op,
`Critical` for host loss. Telemetry cadence 1 s pulse, 30 s batch push. Redaction is a *sink-side* guarantee:
every write goes through `LogRedactionEngine`, so a forgotten call site cannot leak.
Health surface: `/status`-equivalent snapshot object (no HTTP) consumed by the companion, the NOC and
`RunClientTelemetryProbeAsync`. Every shutdown path emits `ShutdownReason` (`TARGET` event name), every teardown race carries an inline
comment (the `process.Kill` sites) so a swallow is a decision, not an accident.

## SS15 · Code patterns (mandated)

```csharp
// 1. Result-carrying API — exceptions are for faults, refusals are data.
public sealed record CommandOutcome(bool Accepted, string Reason, CommandRiskLevel Risk, int? LatencyMs = null);

// 2. Idempotent ingest — no re-read, no duplicate row.
public async Task<IngestResult> IngestAsync(JournalEntry entry, CancellationToken ct)
{
    await using var tx = await connection.BeginTransactionAsync(ct);
    var applied = await InsertIfOffsetUnseenAsync(entry, ct);          // unique index ux_archive_terminal_offset
    if (applied) { await AdvanceOffsetAsync(entry, ct); }              // same tx: archive + offset
    await tx.CommitAsync(ct);
    return applied ? IngestResult.New : IngestResult.Duplicate;
}

// 3. Parser shape — one vendor, one file, sniff then stream, no allocation per line.
public sealed class GrgEjTransactionParser : IEjTransactionParser
{
    public string Vendor => "GRG";
    public bool CanParse(ReadOnlySpan<byte> head) => head.IndexOf("[TXN]"u8) >= 0;
    public IEnumerable<EjTransaction> Parse(Stream journal, JournalParseContext ctx, CancellationToken ct) { ... }
}

// 4. Fail-closed security check — deny on unknown, never on default-allow.
public static bool IsHashAlgorithmPermitted(string algorithm, string usage) =>
    usage.Equals("integrity", StringComparison.OrdinalIgnoreCase)
        ? PermittedIntegrity.Contains(Normalize(algorithm))
        : MigrationUsages.Contains((Normalize(algorithm), usage));

// 5. UI handler — thin, async, cancellable, no business logic.
private async void retryButton_Click(object sender, EventArgs e)
{
    using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(2));
    retryButton.Enabled = false;
    try { var n = await outbox.FlushNowAsync(cbs: progress, cts.Token); statusLabel.Text = $"{n} spooled"; }
    catch (OperationCanceledException) { statusLabel.Text = "Retry cancelled"; }
    finally { retryButton.Enabled = true; }
}
```

File shape: one public type per file (partials allowed only inside the owning assembly); `sealed` by default;
`record` for DTOs; `internal` until a consumer exists; no `public static` mutable state except documented
registries (`EjParserRegistry`, `XfsAdapterRegistry`, `ServiceRegistry`); nullable reference types enabled
(`Directory.Build.props`), so `string?` is the only "optional" marker; no `async void` except event handlers;
no `catch { }` without an inline reason; every P/Invoke declares `SetLastError = true`, `CharSet.Unicode` and a
tested failure path.

Anti-patterns (found here; banned going forward): merged multi-type dumps >1 MiB; `public partial public class`
scanner output; per-project glob `Services\*.cs` beside an explicit list (double inclusion); a second assembly
with the same `AssemblyName`; `AssemblyName` ≠ project name outside the shipped-exe allowlist; UI code in the
Windows service; two ADO.NET providers in one assembly; reflection used to reach a type that could be a
referenced contract (`ServiceLocator` → `Type.GetType("..., EJLive.Client")` must be replaced by an interface in
`EJLive.Core`); Arabic/English mixed prose inside identifiers or SQL literals.

## SS16 · Installation, packaging, operations

Build: `dotnet build EJLive.Platform.sln -c Release -m:1 /p:BuildInParallel=false` (serialised — parallel node
reuse races the shared generated files, see `docs/CI.md`).
CI (`docs/CI.md`) runs two jobs, in that order: `structure` (inventory `--check`, activation `--check`, the
39-rule gate, and `tools/gates/check_artefacts.py`, which proves the artefacts agree with each other) then
`build` on `windows-latest` (restore, serialised build, tests, 23 probes, packaging). Both jobs are green on
the branch; the Windows job is the first thing that proves the compile maps are MSBuild-evaluatable, which no
local tool can (no SDK), so a red `build` job is a plan input, not a surprise.
Package: `tools/package/package.bat Release` → three zips (client / server / NOC) staged from `bin/Release/
net8.0-windows`, journal fixtures copied for self-test, `install.cmd` / `start.cmd` generated.
Install (endpoint, elevated): payload → `Program Files\EJLive\Client` → service `EJLive.Client.Service`
(`start=auto`, `objtype=own`, recovery 1 s/5 s/60 s restart) → firewall (Private, 5656 out for endpoint, in for
server) → `client.config` (server host, port, thumbprints, retention) → companion probe → self-test.
Install (server): service optional; recommended as a scheduled task at startup plus the WinForms host, or
`sc create EJLive.ServerWin` when unattended operation is required.
Activation ledger: `docs/12-service-activation-status.csv` (99 service types; `host` = constructed by an entry
point, `registry` = constructed via composition root, `orphan`/`reference-only` = backlog).
Backup/restore: online backup API or `VACUUM INTO` for the archive, `sqlite3 backup` forbidden mid-migration;
restore validates `schema_migrations` version ≥ current binary expectation.
Rotation/retention: journal archive `daily_stats`-driven, default 400 days, redacted bundles kept 90 days,
logs 14 files × 8 MB.

## SS17 · Delivery plan (waves) and traceability

| wave | content | exit criteria |
|---|---|---|
| 0 — done (this branch) | build graph repair: 654 MB → 68 MB, 29 csproj → 14, curated explicit compile maps, archive `src/_reference/`, ledgers (12), static gate (40 rules), CI, this document | `gate: PASS`, `ledgers fresh`, 0 intra-assembly duplicate keys, 0 stale includes, 0 unparsable files in a map |
| 1 — restore lost capability | promote archived surface into the compiled tree behind a build gate: `Server.WinForms` 5 → its 44 archived files, `Monitoring`/`Installer`/`Client` companions, `UnifiedLauncher`, `Verification`; rewrite the 10 unparsable dumps (DEBT D-02) from SS5/SS7/SS9; **build after each file group** | `dotnet build` green; 371 + new tests green; 23 probes green; `orphan` + `reference-only` rows strictly decreasing |
| 2 — namespace & provider repair | DEBT D-01 (18 cross-assembly partial splits), D-06 (one SQLite provider), D-07 (single `MsgType` + protocol in `EJLive.Shared`), remove `ServiceLocator` reflection | `TYPE-2` empty, `DEP-1` clean, protocol ledger maps to one owner |
| 3 — build the new | Journal Studio (SS10.5), `outbox_dead_letters` + retention job UI, audit-chain verifier UI, adaptive chunking tuning, `active_compile_map` table replacing the CSV dependency | features covered by tests + probes, targets in SS13 measured |

Traceability: `docs/TRACEABILITY-MATRIX.md` maps every artefact class of the specification corpus to a file and a
status (`exists` / `partial` / `missing` / `debt`). Gaps that cannot be closed from the repository alone are
listed there with the exact input needed — G-1 (specification corpus not present in the workspace) and G-2
(branch-name mismatch) are open and must be resolved by the requester, not assumed.

## SS18 · Definition of done

1. `dotnet build EJLive.Platform.sln -c Release -m:1 /p:BuildInParallel=false` — 0 errors, 0 new warnings.
2. `dotnet test src/EJLive.Tests/EJLive.Tests.csproj` — 371+ cases, 0 failures, 0 skipped without a reason file.
3. `dotnet run --project src/EJLive.Verification/EJLive.Verification.csproj -c Release --no-build` — 23/23 PASS.
4. `python3 tools/inventory/ejlive_inventory.py` then `--check` → `ledgers fresh`; activation ledger fresh.
5. `python3 tools/gates/ejlive_static_gate.py` → `gate result: PASS` (40 rules), no new allowlist entries.
6. `artifacts/ActiveCompileMap.csv` — 0 stale includes, 0 unmapped sources, 0 intra-assembly duplicate keys.
7. `docs/DEBT-LEDGER.md` — every remaining entry has an owner and an exit condition; nothing new added without a
   one-line justification in the PR body.
8. Three payloads package, install on a clean Windows 10/11 + Server 2019+ box, self-test green against
   `Samples/*.LOG`, and a 64 MB transfer interrupted at 50 % resumes correctly.
9. No UI thread handler above 50 ms, no `.Result`, no `unsafe`, no WPF, no web UI, one parser per vendor.
10. PR merges only with CI green; commits carry `SS-nn` references for spec-traceable changes.
