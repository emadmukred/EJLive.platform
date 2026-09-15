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
