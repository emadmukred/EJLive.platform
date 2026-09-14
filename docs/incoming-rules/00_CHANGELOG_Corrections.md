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
