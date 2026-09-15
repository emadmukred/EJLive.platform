# 00 · CHANGELOG of corrections (binding rules)

> Source of truth for the corrections applied to this repository across waves.
> When a new upload lands, the incoming-rules toolchain cross-references this
> list and refuses any change that re-introduces a previously fixed defect or
> undoes a recorded correction. Every entry is a closed loop: finding → fix →
> ledger row → gate rule → CI job.
>
> **Status legend**: `OPEN` = still being corrected across waves; `CLOSED` =
> fixed and verified by `ejlive_static_gate.py` (`41/41 PASS`); `RESURFACED` =
> a new upload re-introduced the violation; `RESURFACED-CLOSED` = re-fixed in
> a later wave.
>
> See also: `docs/ANALYSIS-FINDINGS.md` (full findings + corrections),
> `docs/DEBT-LEDGER.md` (named debt), `docs/EJLIVE-ENGINEERING-PROMPT.md`
> SS1–SS18 (the construction contract).

## C-01 — `.gitignore` hygiene

- **Finding**: `E-01`. 1 711 tracked build/IDE artefacts; repo 654 MB.
- **Fix**: `.gitignore` + `git rm -r --cached`; tree pruned to 68 MB.
- **Status**: **CLOSED** (`ART-1`/`ART-2` hold; `.gitignore` at repo root).

## C-02 — One canonical `.csproj` per project directory

- **Finding**: `E-02`. 29 `.csproj` across 22 source directories; 8 dirs absent from `EJLive.Platform.sln`; two `EJLive.UnifiedLauncher`, two `EJLive.Monitoring`.
- **Fix**: SDK-style project per directory; 7 legacy/`ns0:` shells archived to `src/_reference/csproj-shells/`; 8 solution-orphan dirs to `orphan-projects/`.
- **Status**: **CLOSED** (`ARCH-1` enforces missing=∅ / dangling=∅ on both `.sln` and `.slnx`).

## C-03 — No intra-assembly duplicate type keys

- **Finding**: `E-03`. 164 duplicate type keys inside `EJLive.Core` (e.g. `NetworkEngine.cs` byte-identical across two folders).
- **Fix**: 20 byte-exact duplicates archived; declaration-level pruning; `TYPE-1` = 0.
- **Status**: **CLOSED**.

## C-04 — Auto-merge dumps out of the compiled set

- **Finding**: `E-04`. 11 dumps with up to 483-brace deficit, repeated modifiers, members inside constructors.
- **Fix**: 10 archived to `src/_reference/corrupted/`; `SYN-1`/`SYN-2` forbid re-entry.
- **Status**: **CLOSED** (`SYN-5` keeps `ATMTypes.cs` repaired via `check_merge_dumps.py`).

## C-05 — No ` (N)` copy suffix

- **Finding**: `E-05`. 11 files carried `" (N)"` suffix; 4 were source.
- **Fix**: moved to `_reference/vs-copies/`; `FILE-2` bans recurrence.
- **Status**: **CLOSED**.

## C-06 — Explicit compile maps (no globs)

- **Finding**: `E-06`. 94 explicit `<Compile Include>` mixed with `Services\*.cs` globs.
- **Fix**: `Directory.Build.props` sets `EnableDefaultCompileItems=false`; every project now has an explicit, generated map.
- **Status**: **CLOSED** (`TYPE-3`/`TYPE-4` ban globs).

## C-07 — No unmapped sources in a project directory

- **Finding**: `E-07`. 285 source files inside project dirs but outside every compile map.
- **Fix**: demoted to `src/_reference/uncompiled/<Project>/`; `TYPE-4` = 0.
- **Status**: **CLOSED**.

## C-08 — Usage-closure promotions

- **Finding**: `E-08`. 23 files referenced by the compiled set were uncompiled.
- **Fix**: 26 promotions by usage closure (23 by closure + 3 required by tests/reflection).
- **Status**: **CLOSED**.

## C-09 — `EJLive.Monitoring.WinForms` retargeted

- **Finding**: `E-09`. Targeted `net10.0-windows7.0`, `UseWPF=True`, `AllowUnsafeBlocks=True`.
- **Fix**: retargeted `net8.0-windows`; WPF and unsafe removed.
- **Status**: **CLOSED** (`NAME-1`/`SEC-1`/`SEC-2` hold).

## C-10 — `EJLive.LegacyReference` as a real link project

- **Finding**: `E-10`. Linked non-existent paths.
- **Fix**: rewritten as read-only link project over `src/_reference/**`.
- **Status**: **CLOSED**.

## C-11 — Security hygiene

- **Finding**: `E-14`. 25 `process.Kill(); catch { }` sites; 7 weak-hash sites; a real credential literal in `EJLive.Tests/UnifiedRuntimeTests.cs`; unused overload.
- **Fix**: typed handlers with inline reasons; `// safe:`/`// safe-file:` justifications; fixture literal annotated as synthetic; overload replaced by `FromDatabaseFile`.
- **Status**: **CLOSED** (`GIT-3`/`POL-3`/`SEC-3`).

## C-12 — CI, ledgers, gate, docs

- **Finding**: `E-15`. Every "strict repository rule" was unenforced convention.
- **Fix**: `.github/workflows/ci.yml`, 12 generated ledgers, 41-rule static gate, `tools/build` + `tools/package` + `tools/gates`, `docs/ANALYSIS-FINDINGS.md`, `docs/TRACEABILITY-MATRIX.md`, `docs/CI.md`, `README.md`, `docs/EJLIVE-ENGINEERING-PROMPT.md`.
- **Status**: **CLOSED**.

## C-13 — `<Compile>` inside `<ItemGroup>`

- **Finding**: `E-16`. Item element outside `<ItemGroup>` is well-formed XML; every textual tool passes; `dotnet restore` aborts with MSB4067.
- **Fix**: gate rule `SYN-4`.
- **Status**: **CLOSED**.

## C-14 — `EJLive.Shared` no upper-layer references

- **Finding**: `E-17`. `Shared` had 5 files `using EJLive.Core*` and 5 using `System.Windows.Forms` (CS0234); `SharedPrimitives.cs` re-declared 4 owned types.
- **Fix**: 6 files moved to `src/EJLive.Core/Shared/**`; `LightUiTheme.cs` archived; `POL-4` forbids upper-layer `using`.
- **Status**: **CLOSED**.

## C-15 — `EJLive.Shared/Logger.cs` archived

- **Finding**: `E-18`. Second log-record model `LogEntry` whose `Level` belonged to a nested `AppLogger.Level` (CS0426).
- **Fix**: archived to `src/_reference/uncompiled/EJLive.Shared/Logger.cs`; `AppLogger.cs` is the one every consumer uses.
- **Status**: **CLOSED**.

## C-16 — `EJLive.Core` merge dumps rewritten

- **Finding**: `E-19`. 8 merge dumps (after `ATMTypes.cs` was repaired).
- **Fix**: `ATMTypes.cs` repaired via `check_merge_dumps.py --repair-enum-file`; the other 8 recorded as `DEBT-LEDGER` D-08 (8/9 resolved in `67db886`, 9th resolved in `e9607c9`).
- **Status**: **CLOSED**.

## C-17 — Constant unification (AppConstants facade + canonical L0 home)

- **Finding**: `E-20`. `EJLive.Core.AppConstants` had lost ~46 members referenced across ten assemblies (CS0117 ×192 sites); `Shared/UnifiedSystemConfiguration.cs` was an un-collapsed merge dump; `Client.Service` carried two entry points; a demo `Models.DatabaseManager` shadowed the real one.
- **Fix**: `EJLive.Shared.AppConstants` rebuilt as the single canonical home (values cross-referenced against every consumer in the compiled set; the `CMD_WINDOWS_REMOTE_CHECK` value defect fixed; vendor codes unified with the parser-registry keys); `EJLive.Core.AppConstants` reduced to a const-for-const facade over it; `StableServiceProgram.cs` archived to `src/_reference/uncompiled/EJLive.Client.Service/`; the demo stub deleted; new advisory triage tool `tools/gates/check_constant_resolution.py` (0 issues) catches the class pre-push while the sandbox has no compiler (G-3).
- **Status**: **CLOSED** (`Wave4ConstantsAndWorkbookTests` pins facade equality; the entry-point and shadow fixes are build-level, so re-introducing them fails compilation).

## C-18 — One entry point per executable

- **Finding**: `E-20`. `EJLive.Client.Service` compiled `Program.cs` (top-level `Main`) and `StableServiceProgram.cs` (a second `Main`) → CS0017, the exe could not build.
- **Fix**: the demo `StableServiceProgram.cs` archived; the top-level `Program.cs` is the single entry point and now runs the shared `PlatformBootstrap` before any hosted service (a failed bootstrap exits non-zero so the Windows recovery ladder retries on a fixed root).
- **Status**: **CLOSED** (`NAME-2` guards assembly twins; two `Main`s is a build failure by definition).

## C-19 — Central dataroot (`SS-20`)

- **Finding**: `E-21`. `C:\ProgramData\EJLive` was hard-coded in six assemblies and `AppConfig` defaulted its runtime paths to a rival root.
- **Fix**: `EJLive.Shared.DataRootPaths` owns resolution (`EJLIVE_DATAROOT` override → `%PROGRAMDATA%\EJLive`), the fixed sub-tree and the writability probe; `EJLive.Core.Data.DataRootLayout` adds the server archive tree; every path default (config, logs, db, outbox/inbox, archive, images, share, health file) derives from it.
- **Status": **CLOSED** (verification probe `Data root bootstrap and schema book`; the regression to reject is a fresh hard-coded root literal outside `DataRootPaths`).

## C-20 — Schema book collapsed, migrations executed, repositories own the tables, audit chain real

- **Finding**: `E-22`. Two migration books, a runtime-dead runner, ten tables with no DML consumer, schema forks between runner and manager DDL, and the SS9 audit chain documented but absent.
- **Fix**: canonical `schema_migrations(version,name,applied_utc,checksum,rolled_back)` with `__migrations` backfill-and-drop, per-migration SHA-256 checksums, forward-only numbering (1–6 legacy + 7–13 Phase-2 contract DDL aligned to `DatabaseManager` shapes), newer-on-disk **refusal guard**; `PlatformBootstrap` applies it from every host; `Core/Data/Repositories/` typed owners consumed by `IngestionPipeline`, `ServerAuditService` and `ClientTelemetryStateService`; chained `audit_log` (`prev_hash`/`payload_hash`) with `DatabaseManager.VerifyAuditChain` + `AuditLogger.VerifyChain`.
- **Status**: **CLOSED** (`Wave4SchemaBookTests`, `Wave4RepositoryTests`, `RunDataRootBootstrapProbe`).

## C-21 — No web tier, enforced in code not prose

- **Finding**: `E-23`. `src/EJLive.Analysis.Web/` (static frontend), `SmartAnalysisHost` (HttpListener in Core), `EJLiveApplicationHost.StartSmartAnalysis` and an orphan `supabase_schema.sql`.
- **Fix**: all web surface demoted to `src/_reference/uncompiled/` (evidence, not source); the start path removed from the host; the capability stays in-process (`SmartAnalysisService`) and in WinForms (`JournalStudioForm`); Supabase config fields remain inert round-trip only.
- **Status**: **CLOSED** (a re-added `HttpListener`/web asset in a compiled map re-opens this entry).

## C-22 — Reflection-free service location

- **Finding**: `E-24`. `ServiceLocator.GetJournalSyncService()` used `Type.GetType("…, EJLive.Client")` reflection — the exact anti-pattern SS15 names.
- **Fix**: explicit `RegisterJournalSyncService` composition seam + documented fallback stub; test seam `ClearJournalSyncServiceRegistration`.
- **Status**: **CLOSED**.

## C-23 — Journal Studio: bulk folder analysis + Excel export

- **Finding**: `E-25`. SS-10.5 named bulk analysis, Excel export and an archive-reconciled delta; the tool had CSV/JSON and an in-memory-only reconciliation.
- **Fix**: `JournalStudioBulkAnalyzer` (≤500 files off the UI thread, progress + cancel, per-file vendor sniff → registry parser → aggregates, double-click to load) + `ExcelWorkbookWriter` (dependency-free OOXML .xlsx: inline strings, numeric cells, bold headers, atomic publish — no Office interop, no new package); single-set Export Excel adds a Summary sheet; the reconciliation label now reads `journal_archive.transaction_count` through `IJournalArchiveRepository`.
- **Status**: **CLOSED** (`ExcelWorkbookWriter` round-trip pinned by `Wave4ConstantsAndWorkbookTests`).

## C-24 — WinForms Designer partial pattern

- **Finding**: `E-26`. Every form was code-only, so the "built in the Visual Studio Designer" rule could not be represented and a designer round-trip would have overwritten behaviour.
- **Fix**: `JournalStudioForm.Designer.cs` — named-field control tree, `InitializeComponent` = create/properties/parenting only, `Dispose(bool)` + `components`, DPI autoscale, Anchor/Dock layout, tab order = visual order; event wiring isolated in the companion `WireEvents()`; the regeneration protocol is documented in the file header. `UI-SURFACES.md` records the split (42 controls / 14 wired handlers).
- **Status**: **CLOSED** for the Studio surface; replicating it on `ServerMainForm`/`ClientMainForm`/`MainDashboardForm` is a Wave-5 process task, tracked in the prompt's wave table.

## C-25 — Tooling: copy-suffix coverage and legal partial merges

- **Finding**: `E-27`. `FILE-2` ignored ` (2)`-suffixed `.sql`/`.json` copies; `ejlive_inventory.duplicate_keys` flagged legal same-assembly `partial` splits as CS0101 — structurally blocking the Designer-partial pattern.
- **Fix**: `FILE-2` extended to `.sql/.json/.manifest/.props/.targets` (both copies archived to `_reference/exact-duplicates/`); `duplicate_keys` merges all-`partial` same-assembly pairs, keeps cross-assembly splits as debt, and still fails a non-partial twin (CS0260/CS0101).
- **Status**: **CLOSED** (this wave's split form is the live proof: 41/41 gate PASS with two owner files for one partial type).

## C-26 — CI fix-up: AuditLogger brace closure and a Roslyn-faithful gate lexer

- **Finding**: `E-28`. The first real compile (Windows CI after the Wave-4 push) failed with `CS1513` in
  `src/EJLive.Core/Services/AuditLogger.cs` — the Wave-4 `VerifyChain` addition truncated the file tail and the
  block-scoped namespace stayed open. Gate `SYN-1` had passed that revision because its literal-stripping pass
  does not parse interpolation holes: escaped quotes inside one hole netted the missing brace away.
- **Fix**: namespace closed at the file tail; `tools/gates/ejlive_static_gate.py:strip_literals` rebuilt as an
  interpolation-aware lexer (holes containing nested strings, `{{`/`}}` literal braces, verbatim `""` escapes,
  char literals). The previously broken revision now fails with delta `(+1)`; the fixed tree is balanced. The
  file's Arabic comments were converted to English in the same pass (documentation rule).
- **Status**: **CLOSED** (41/41 gate PASS locally with regenerated ledgers; CI re-run is the authority).

## C-27 — CI fix-up: gateway bridge types, integration/activation audits, RetryPolicy unification

- **Finding**: `E-29`. After C-26 landed, the Windows build still failed: the gateway in
  `EJLive.Core` referenced `EJLive.Business` types it can never see, three consumed types had no
  compiled definition (`ActiveServiceReplacement`, `ReferenceOnlyServiceFile`, `JournalSyncAlert`),
  and dual `RetryPolicy` definitions produced CS0104 in `NetworkEngine`.
- **Fix**: `ServiceBridgeRoutes` now owns the ten-entry bridge table once (gateway and audits read
  the same source); `UnifiedProjectIntegrationAuditService` moved to `EJLive.Core.Services` and grew
  the full contract the tests pinned (`SourceFileCount`, `ReferenceOnlyFiles`, coverage flags,
  `ActiveReplacements`, duplicate-type scan); `UnifiedServiceActivationAuditService` implements the
  C#-side of the activation ledger; `UnifiedBusinessRuntime` exposes `ServiceGateway` +
  `BuildIntegrationAudit`; `JournalSyncAlert` + `JournalSyncAlertSeverity` rebuilt in
  `JournalSyncModels.cs`; `EJLive.Shared.RetryPolicy` is the single canonical policy (named ctor,
  bounded backoff, `ForNetwork`, `Default`, `GetDelay` alias), both legacy copies deleted with
  tombstones. `AgentBootstrapper` reality is recorded in **D-09**; the activation test asserts
  `CoveredByBridge` until that promotion lands — no dead façade was compiled to fake the assertion.
- **Status**: **CLOSED** (gate 41/41 PASS locally with regenerated ledgers; Windows CI is authority).

## C-28 — Endpoint Console: Designer partial split (C-24 residual, Wave 5)

- **Finding**: residual of `E-26`. `ClientMainForm` (Endpoint Console) was code-only: the
  control tree was built by private `Build*` factories in the constructor, so the
  "built in the Visual Studio Designer" rule (SS-10) could not be represented and a
  designer round-trip would have overwritten behaviour.
- **Fix**: `ClientMainForm` split into the behaviour partial (this file's
  `WireEvents()` + `ApplySnapshot`/heartbeat classification) and
  `ClientMainForm.Designer.cs` (named-field control tree — 36 fields: header, four
  status cards, details grid, component grid, event feed; `InitializeComponent` =
  create/property/parent only; `Dispose(bool)` + `components`; `AutoScaleMode.Dpi`;
  `TabIndex` in visual order; `AccessibleName` on data surfaces). Window metrics
  preserved (`Size 1120×760` → `ClientSize 1104×713`, same `MinimumSize`);
  `_refreshTimer` stays a runtime artefact in the behaviour partial (documented in
  the designer header). Event bindings moved verbatim into `WireEvents()` — the
  designer file is binding-free, so regeneration cannot orphan a handler.
- **Status**: **CLOSED** (gate 41/41 with the split; `UI-SURFACES.md` shows the
  control/handler split per file).

## C-29 — NOC / Windows Operations Console: Designer partial split (C-24 residual, Wave 5)

- **Finding**: residual of `E-26`. `MainDashboardForm` (10 tabs) was code-only;
  button handlers were attached through `UiHelpers.Button(text, action)` inside the
  tab builders, mixing construction and wiring.
- **Fix**: `MainDashboardForm` split into the behaviour partial (service fields,
  `WireEvents()`, `PerformInitialRefresh()`, cash telemetry, detached-window
  helper, Smart Analysis) and `MainDashboardForm.Designer.cs` (104 named fields:
  all 10 tabs, 11 grids with the canonical header/zebra/selection styles inlined,
  9 metric cards, both split containers, the Smart Analysis results tree). The
  legacy `UiHelpers.Grid()` theme is inlined as designer-settable properties; the
  reflection-only `EnableDoubleBuffering()` pass moved to `PrepareGrids()` in the
  behaviour partial (documented). Static Device-State rows and the
  `DateTime.Now`-relative Realtime-Sync demo rows are seeded in
  `PerformInitialRefresh()` because a designer partial cannot express relative
  dates (documented in the designer header). Refresh order is the exact sequence
  the tab builders applied.
- **Status**: **CLOSED** (gate 41/41; `UI-SURFACES.md` updated).

## C-30 — Enterprise Server console: Designer partial split (C-24 residual, Wave 5)

- **Finding**: residual of `E-26`. `ServerMainForm` (13 tabs + MenuStrip, 2 264
  lines) was the largest code-only surface: 60+ buttons wired through inline
  lambdas, the menu built in `BuildMainMenu()`, metric cards via
  `UiHelpers.AddMetricCard`, the Settings rows through a private `AddRow` helper.
- **Fix**: `ServerMainForm` split into the behaviour partial (service composition,
  `WireEvents()` with all 89 bindings — menu, 13 tab action strips, remote-command
  console, `_refreshTimer.Tick` — plus `PrepareGrids()` and
  `PerformInitialRefresh()` in the legacy builders' exact refresh order) and
  `ServerMainForm.Designer.cs` (199 named fields: 4 top-level menus with 16
  items, 13 tab pages, 9 themed grids, 9 metric cards, the remote preview panel,
  the Command-Audit filter combo seeded with its four scope literals, and the
  Settings panel seeded from the `NetworkConfig.DEFAULT_PORT` / `ATMPaths` /
  `AppConstants` constants exactly as the legacy builder did). The now-obsolete
  `AddRow` helper and all `Build*Tab`/`BuildMainMenu`/`InitializeUi` methods were
  deleted. Window metrics preserved (`Size 1220×820` → `ClientSize 1204×773`).
  The three small dialog forms in the same file (`ATMDetailForm`,
  `ATMDetailDrawerForm`, `SyncDashboardForm`) stay in the behaviour file — they are
  separate public types, not part of this surface's designer tree.
- **Status**: **CLOSED** (gate 41/41; `UI-SURFACES.md` updated; all 24
  reflection-pinned method contracts in `EJLive.Tests` still resolve on the
  behaviour partial).

## C-31 — One owner for the agent health-file schema (Track08 gap)

- **Finding**: `E-30`. `ClientCompanionStatusTests` (Track08) pins a reflection
  contract — `ClientMainForm.TryParseServiceHealthSnapshot(string, out …)` with
  `State`/`Connected`/`PendingOutboxItems`/`SessionId` properties — that no
  compiled type satisfied: the parse lived inside
  `InProcessClientServiceGateway` as private nested record `ServiceHealthSnapshot`
  plus nine `ReadJson*` helpers the form could not reuse.
- **Fix**: `ServiceHealthSnapshot` promoted to a `public sealed record` in
  `EJLive.Client.WinForms.Services` with `TryParseJson(string?, out …)` as the
  single parse seam (case-insensitive keys, numeric-or-string `state` —
  0=Stopped…4=Failed — tolerant number/string scalars, non-object or malformed
  JSON → `Empty` + false, never an exception). `InProcessClientServiceGateway`
  now reads the live health file (shared `FileShare.ReadWrite|Delete` handle kept)
  and delegates interpretation to the record; the duplicated helpers were deleted.
  `ClientMainForm.TryParseServiceHealthSnapshot` is a thin private static bridge
  over the record, closing the Track08 contract without a second parser.
- **Status**: **CLOSED** (the two Track08 cases now resolve; the gateway
  behavioural tests are unaffected — the JSON schema is byte-identical).


## C-32 — pre-commit hook: mutually exclusive flags blocked every commit

- **Finding**: `E-31`. `tools/hooks/pre-commit` invoked the tool with all three
  `--check-architecture --check-implementation --check-changelog` flags in one
  call, but they form an argparse mutually exclusive group: the tool exited 2
  with a usage error before evaluating a single rule, so the hook classified
  every staged file as a violation and could never allow a commit.
- **Fix**: the hook now calls the tool with `--upload <file>` only — with no
  `--check-*` flag the tool runs all three rule sets (its documented default,
  `incoming_rules.py` main: `run_arch/run_impl/run_chl` default true), which is
  exactly the "once per file with all three rule sets" contract the hook's
  header states. Warnings (e.g. the K-6 brace heuristic on interpolation lines
  with escaped quotes) do not block; only `violation` severities exit 2.
- **Status**: **CLOSED** (per-file simulation over the Wave-5 staged set: 0
  blocks; the same check runs in CI via the workflow).


## Wave resolutions

- `67db886` — D-08 (8 merge dumps in `EJLive.Core/Models` + `Services/UnifiedOperationalFusion`).
- `e9607c9` — D-06 (single SQLite provider `Microsoft.Data.Sqlite` 8.0.5).
- `0f00399` — SS-10.5 Journal Studio delivered.
- `d669605` + `944076a` — D-01 + D-07 (cross-assembly partial splits collapsed to 0; single `MsgType` in `EJLive.Core.Engine.CommunicationProtocol`).
- `fe574b2` — SS-17 first promotion (`ATMCardPanel` from archive to `Server.WinForms`).

## How a new upload is checked against this changelog

1. `tools/incoming/incoming_rules.py --upload <path>` extracts every file in the
   upload (CS, csproj, md, sql, ps1, bat, …) and produces a per-file verdict.
2. Each verdict cross-references this changelog, `docs/DEBT-LEDGER.md`, and the
   active compile map. The upload is **rejected** if it would re-introduce a
   CLOSED entry, undo a wave resolution, or change a wave-resolved file without
   a matching ledger update.
3. CI runs the same check as `.github/workflows/incoming-rules.yml` on every
   push; the pre-commit hook (`tools/hooks/pre-commit`) runs it locally before
   `git commit` is allowed to proceed.

### Wave 4 (this branch)

- C-17…C-25 — constants/build repair, central dataroot + bootstrap, schema book + repositories + audit chain, web removal, Studio bulk/Excel, Designer partials, tooling. Gate 41/41 PASS · 24 verification probes · 397 test cases · `check_constant_resolution.py` 0 issues.
- C-26 — CI fix-up after the first Windows compile: `AuditLogger.cs` namespace closure (CS1513), `SYN-1` gate lexer made interpolation-faithful, touched-file comments anglicized. Gate 41/41 PASS.
- C-27 — CI fix-up (second Windows iteration): gateway bridge-route single source, integration + activation audits implemented against the pinned contracts, `RetryPolicy` unified, `JournalSyncAlert` rebuilt, D-09 opened for the AgentBootstrapper promotion.

### Wave 5 (this branch)

- C-28/C-29/C-30 — the C-24 Designer-partial process replicated on the three main
  consoles: `ClientMainForm` (36 designer fields), `MainDashboardForm` (104),
  `ServerMainForm` (199 + 16 menu items). All event bindings moved to
  `WireEvents()` in the behaviour partials; all 24 reflection-pinned test
  contracts still resolve; the `UiHelpers.Grid()` theme inlined as designer
  properties, `EnableDoubleBuffering` kept as the behaviour-side `PrepareGrids()`
  pass. Gate 41/41 PASS · ledgers regenerated · `UI-SURFACES.md` shows the
  control/handler split per file.
- C-31 — `ServiceHealthSnapshot` promoted to the canonical public record with
  `TryParseJson` as the single health-file parse seam; the Track08
  `TryParseServiceHealthSnapshot` contract closed by a thin form bridge; gateway
  deduplicated onto the record.
- C-32 — `tools/hooks/pre-commit` fixed: it passed three mutually exclusive
  `--check-*` flags in one call (argparse exit 2 → every commit blocked); it now
  invokes the tool per file with no check flag, which runs all three rule sets.
- D-02 / D-09 — kept OPEN with honest classifications: the activation audit
  confirms zero compiled consumers for the ten D-02 dumps, and the D-09
  AgentBootstrapper runtime is already covered by the compiled agent surface
  (`AgentHeadlessController` + `ClientAgentWindowsService` supervision). Both
  promotions are deferred to a consumer-driven design decision — per C-27, no
  dead façade is compiled to satisfy a ledger.
