# ANALYSIS FINDINGS — what was wrong and what was corrected

Method: first-principles pass over the repository as its own executable
contract (compile maps, csproj/LegacyReference link lists, 23 `EJLive.Verification`
probes, `RegressionVerificationGate`, `ReleaseReadinessChecker`, 371 test cases).
No claim below is asserted from file names alone: each was produced by a
resolver (`tools/inventory/ejlive_inventory.py`) reading the actual maps, and each
correction is re-verified by `tools/gates/ejlive_static_gate.py`.

`E-nn` = finding, `C-nn` = correction applied in this branch,
`D-nn` = residual debt with an exit condition (see `DEBT-LEDGER.md`).

| id | finding (measured) | severity | correction / disposition |
|---|---|---|---|
| E-01 | No `.gitignore`: 1 711 tracked build/IDE artefacts (`.dll`, `.pdb`, `.so`, `obj/`, `.vs/`, `.lscache`, `build.binlog`), repo 654 MB with only 663 authored files | blocker (repo unusable to review) | **C-01** `.gitignore` + `git rm -r --cached`, tree pruned → 68 MB; journal fixtures under `Samples/` explicitly preserved as source |
| E-02 | 29 `.csproj` across 22 source directories (23 including `packages`); 8 directories absent from `EJLive.Platform.sln`; two projects both producing `EJLive.UnifiedLauncher`, two `EJLive.Monitoring` | blocker | **C-02** one canonical SDK-style project per directory; 7 legacy/`ns0:` shells archived to `src/_reference/csproj-shells/`; 8 solution-orphan directories + the csproj-less `EJLive.Vendors` archived to `orphan-projects/` (40 files) |
| E-03 | 164 duplicate type keys inside `EJLive.Core` (measured before curation)'s resolved compile map (e.g. `Core/Network/NetworkEngine.cs` ≡ `Core/Transport/NetworkEngine.cs`, 97 001 lines, byte-identical) | blocker (CS0101/CS0433) | **C-03** 20 byte-exact duplicate files archived; declaration-level pruning (one owner per key) with brace-delta verification; `TYPE-1` gate rule now 0 |
| E-04 | 11 auto-merge dumps in the compiled set: unbalanced blocks (up to 483-brace deficit, 3.3 MB), repeated modifiers (`public partial public class ImageSyncEngine`), 3 539 lines indented >70 columns, members declared inside constructors, header `Processed by Code Intelligence Scanner — Smart Merge` | blocker (unparsable) | **C-04** archived to `src/_reference/corrupted/` (10 files); `SYN-1/SYN-2` rules forbid re-entry; rewrite prescribed in prompt SS17 Wave 1 (D-02) |
| E-05 | 11 files carried a ` " (N)" ` copy suffix; 4 were source (`JournalSyncTrackerService (91).cs`, `MergedTraceCorrelationService (51).cs`, `NcrConfigCapabilityParser (72).cs`, `RemoteDiagnosticCommandService (8).cs`) and sat inside compile maps beside their originals | high | **C-05** moved to `_reference/vs-copies/`; `FILE-2` bans recurrence |
| E-06 | Compile maps mixed 94 explicit `<Compile Include>` entries with `Services\*.cs` and `Xfs\Adapters\*.cs` globs — a hidden, growing inclusion set | high | **C-06** `Directory.Build.props` sets `EnableDefaultCompileItems=false`; every project now carries an explicit, generated map; globs banned by `TYPE-3`/`TYPE-4` |
| E-07 | 285 source files sat inside project directories but outside every compile map — invisible to build, review and ledgers | high | **C-07** demoted to `src/_reference/uncompiled/<Project>/` (177 files); `TYPE-4` now requires 0 (map completeness is enforced, not documented) |
| E-08 | 23 files referenced by the compiled set were uncompiled (usage closure broken): `Core/ActiveCore.cs`, models, `Logger`, `Protocol`, plus 3 `Client.WinForms` types that `EJLive.Tests` and `Core/Services/ServiceLocator.cs` resolve | high | **C-08** 26 promotions by usage closure (23 by closure + 3 required by tests/reflection) (parsable files only); the 3 archived-but-required files (`ClientStartupPlanner`, `ServiceRegistry`, `FileDeliveryConfirmationTracker`) promoted back into `EJLive.Client.WinForms` |
| E-09 | `EJLive.Monitoring.WinForms` targeted `net10.0-windows7.0`, enabled `UseWPF=True` and `AllowUnsafeBlocks=True` in a Windows-Forms-only, no-unsafe platform | blocker vs repo rules | **C-09** retargeted `net8.0-windows`, WPF and unsafe removed; `NAME-1`/`SEC-1` hold it |
| E-10 | `EJLive.LegacyReference.csproj` linked non-existent paths (`src_update.zip`, `legacy/original/**`, whole project folders) and had no real reference globs | medium | **C-10** rewritten as the read-only link project over `src/_reference/**` with a fresh `AssemblyInfo` |
| E-11 | Cross-assembly `partial` split: `EJLive.Business/BusinessAdapters.cs` and `UnifiedServiceGateway.cs` extend `EJLive.Core.Services.*` types from another assembly (18 keys) — invisible to other consumers, ambiguous to any project referencing both | high | **D-01** retained as named debt with a mechanical exit (fold members into Core, or re-declare in `EJLive.Business`); `TYPE-2` fails if the row is removed before the fix or a new key appears |
| E-12 | Two wire-protocol definitions in one assembly: `Core/Communication/Protocol.cs` `MsgType` (22 members, canonical) and `Core/Communication/MessageTypes.cs` `MsgType` (10) | high | **D-07** canonical set fixed in prompt SS5; legacy enum archived in Wave 2; protocol ledger (`docs/inventory/PROTOCOL.md`) maps every literal |
| E-13 | Two ADO.NET providers pinned in `EJLive.Core` (`System.Data.SQLite.Core` 1.0.118 + `Microsoft.Data.Sqlite` 8.0.5) and used by different files in the same assembly | medium | **D-06** `DEP-1` rule + exit condition; `DEP-2` keeps the rest of the package graph version-coherent |
| E-14 | Security hygiene: 25 `process.Kill(); catch { }` teardown sites swallowing failures; 7 weak-hash call sites without a stated reason; a real credential literal in `EJLive.Tests/UnifiedRuntimeTests.cs`; an unused `(string, bool dummy)` constructor overload | medium | **C-11** typed handlers with inline reasons; `// safe:`/`// safe-file:` justifications recorded; fixture literal annotated as synthetic; overload replaced by `FromDatabaseFile` factory; `GIT-3`/`POL-3`/`SEC-3` now guard all four classes |
| E-15 | No CI, no ledgers, no gate, no docs: every "strict repository rule" (one parser per vendor, no unsafe vocabulary, inventory regeneration per push) was unenforced convention | blocker (drift) | **C-12** `.github/workflows/ci.yml`, 12 generated ledgers, 39-rule static gate, `tools/build` + `tools/package` + `tools/gates` scripts, this findings file, `TRACEABILITY-MATRIX.md`, `CI.md`, `README.md`, `EJLIVE-ENGINEERING-PROMPT.md` |
| E-16 | A `<Compile>` item outside `<ItemGroup>` in `EJLive.Client.WinForms.csproj` (introduced while promoting three archived files) is well-formed XML, so the inventory resolver, the ledgers and every earlier gate rule stayed green while `dotnet restore` aborted the whole solution with MSB4067 — a red CI with zero project diagnostics | blocker (tooling blind spot) | **C-13** gate rule `SYN-4` (csproj parses *and* items nest inside `<ItemGroup>`), verified by breaking the file and watching the gate fail; the promoted items moved into the existing `<ItemGroup>` |
| E-17 | `EJLive.Shared` compiled five files that `using EJLive.Core*` and five that `using System.Windows.Forms`; the L0 primitives assembly has neither reference, so every one of them was CS0234 at compile time, and `SharedPrimitives.cs` additionally re-declared four types that Shared already owns in other files | blocker (build) | **C-14** the six files that need Core/WinForms moved to `src/EJLive.Core/Shared/**` with their namespaces kept (consumers unaffected); `LightUiTheme.cs` archived instead, because `Models/CoreAdapters.cs` already declares `EJLive.Shared.LightUiTheme` for that assembly and a second copy is CS0101; `SharedPrimitives.cs` moved for the same reason it was a cross-assembly twin of Logger/RetryPolicy/MonitoringState*; new gate rule `POL-4` forbids upper-layer `using` inside `EJLive.Shared` |

## Residual debt (summary)

| id | subject | exit |
|---|---|---|
| D-01 | cross-assembly partial splits (18 keys) | Wave 2 |
| D-02 | 10 unparsable merge dumps, archived | Wave 1 rewrite from prompt SS5/SS7/SS9 |
| D-03 | oversized single-owner dumps still compiled (`CoreServices.cs`, `UnifiedModels.cs`) | Wave 3 split |
| D-04 | 237 archived files = capability backlog (656 k lines) | Wave 1, per-file with build gate |
| D-05 | retired `EJLive.Server.exe` name still probed by archived setup code | packaging contract fixed in prompt SS16 |
| D-06 | two SQLite providers in one assembly | Wave 1 |
| D-07 | two `MsgType` enums, protocol split across two files | Wave 2 |

## Constraints on this pass (stated, not hidden)

- **No compiler.** The sandbox has no .NET SDK and no NuGet egress, so nothing here is
  compile-verified. Consequences were designed in: no blind promotion of unparsable
  legacy code, no glob flips, and each remaining risky step is explicitly gated on a
  Windows `dotnet build` in Wave 1. `tools/gates/run-gate.ps1` runs the compiler-verified
  sequence where an SDK exists.
- **Ledgers are the authority.** Numbers quoted anywhere (`303/57 551` compiled,
  `237/724 249` archived, 19 tables, 22 message types, 23 probes, 371 cases, 99 activation rows,
  39 gate rules) come from `artifacts/InventorySummary.json` + `docs/inventory/*`, regenerated in the
  same commit as the change that moved them.
