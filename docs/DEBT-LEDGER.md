# DEBT LEDGER -- reviewed, machine-checked deviations

Every entry below is a *known* violation that the static gate would otherwise
fail (`TYPE-2`, `GIT-2`, `FILE-6`). Debt is only legal when it is named here:
the gate reads this file, so an unlisted violation fails CI and a fixed
violation fails CI until the row is removed. Format: `id | what | why-now | exit`.

## D-01 cross-assembly partial split (Core &harr; Business) &mdash; RESOLVED

`EJLive.Business/BusinessAdapters.cs` and `EJLive.Business/UnifiedServiceGateway.cs`
extended `EJLive.Core.Services.*` types with `partial` bodies that lived in a
*different assembly*. C# composes partial types per assembly, so the Business
half was invisible to every other consumer, while any project that referenced
both assemblies (Tests, Verification, Installer.WinForms, UnifiedLauncher)
resolved the type name ambiguously (CS0433).

`src/EJLive.Server.WinForms/Models/ServerModels.cs` carried the same problem for
`EJLive.Core.Engine.ServerEngine` and `EJLive.Core.Engine.ReportExportEngine`,
and was additionally unparsable (missing `{` on the trailing
`TerminalLiveSummaryCanonical` / `TerminalCashStatusCanonical` declarations).

Resolution (Wave 2 / D-01, one commit, gate-verified):
- `BusinessAdapters.cs` archived to `src/_reference/uncompiled/EJLive.Business/`;
- `UnifiedServiceGateway.cs` archived to `src/_reference/uncompiled/EJLive.Business/`;
  its trailing orphan statements (`_remoteCommands = remoteCommands …`) were
  already outside any method body and would have been CS1519 at compile time.
- `Models/ServerModels.cs` archived to
  `src/_reference/uncompiled/EJLive.Server.WinForms/Models/`.
- All 14 keys now have exactly one owner (in `EJLive.Core`).
  `gate TYPE-2` reports `0 cross-assembly partial splits`.

Re-introducing a cross-assembly partial (or adding any new one) fails
`TYPE-2`; Business-side needs for Core types must be re-declared as extension
methods or adapters in namespace `EJLive.Business`, never as `partial`.

## D-02 unparsable auto-merge dumps (archived, not compiled) -- 10 files — OPEN (Wave-5 rewrite backlog)

Each file below was produced by an "auto merge / code-intelligence" pass: brace
blocks do not balance, modifiers repeat (`public partial public class`), and
bodies are indented 70+ columns. They cannot compile, so they are archived
under `src/_reference/corrupted/` and their types are *unowned* in the build
graph. Wave 1 rewrites them from the specification in
`docs/EJLIVE-ENGINEERING-PROMPT.md` (SS7 parsers, SS15 patterns) instead of
repairing the dump.

| file | depth deficit | exit |
|---|---|---|
| `src/EJLive.Core/Services/AdvancedMonitoringService.cs` | see `artifacts/InventorySummary.json` | rewrite from SS7/SS15, add tests, promote to the owning project map |
| `src/EJLive.Core/Services/EnhancedServerService.cs` | see `artifacts/InventorySummary.json` | rewrite from SS7/SS15, add tests, promote to the owning project map |
| `src/EJLive.Core/Services/FileTransferManager.cs` | see `artifacts/InventorySummary.json` | rewrite from SS7/SS15, add tests, promote to the owning project map |
| `src/EJLive.Core/Services/GhostRemoteEngine.cs` | see `artifacts/InventorySummary.json` | rewrite from SS7/SS15, add tests, promote to the owning project map |
| `src/EJLive.Core/Services/ImageSyncEngine.cs` | see `artifacts/InventorySummary.json` | rewrite from SS7/SS15, add tests, promote to the owning project map |
| `src/EJLive.Core/Services/JournalSyncHub.cs` | see `artifacts/InventorySummary.json` | rewrite from SS7/SS15, add tests, promote to the owning project map |
| `src/EJLive.Core/Services/RemoteCommandHandler.cs` | see `artifacts/InventorySummary.json` | rewrite from SS7/SS15, add tests, promote to the owning project map |
| `src/EJLive.Core/Services/UnifiedJournalService.cs` | see `artifacts/InventorySummary.json` | rewrite from SS7/SS15, add tests, promote to the owning project map |
| `src/EJLive.Core/Utils/DateTimeHelper.cs` | see `artifacts/InventorySummary.json` | rewrite from SS7/SS15, add tests, promote to the owning project map |
| `src/EJLive.Core/Utils/Logger.cs` | see `artifacts/InventorySummary.json` | rewrite from SS7/SS15, add tests, promote to the owning project map |

## D-03 oversized single-owner dumps still compiled

- `src/EJLive.Core/Services/CoreServices.cs` (merged service dump; the only
  owner of several service types) -- split per concern in Wave 3, one type per
  file, keeping the compile map explicit.
- `src/EJLive.Core/Models/UnifiedModels.cs` -- split into `ATM*`, `Journal*`,
  `Transfer*` model files when Wave 1 touches the model layer.

## D-04 uncompiled legacy volume in project-adjacent archives

`src/_reference/` holds every demoted file (see `artifacts/ActiveCompileMap.csv`,
rows with `linked-reference`). `docs/12-service-activation-status.csv` counts
`orphan` and `reference-only` rows; that count is the Wave-1 promotion backlog
and must shrink monotonically. No archive file may be added to a compile map
without a compiler run and a ledger refresh in the same commit.

## D-05 headless service host missing a published executable identity — RESOLVED (Wave 4)

`EJLive.Server` builds as a library (`EJLive.Server.dll`) and the WinForms host
`EJLive.Server.WinForms` publishes `EJLive.Server.WinForms.exe`. Archived
`EJLive.Setup/InstallationManager.cs` still probes for the legacy
`EJLive.Server.exe`; the packaging contract is therefore `EJLive.Server.WinForms.exe`
and `tools/package/package.bat` must copy it under that name (SS18).

## D-06 two SQLite providers in one assembly

`src/EJLive.Core/EJLive.Core.csproj` pins both `System.Data.SQLite.Core` 1.0.118 and
`Microsoft.Data.Sqlite` 8.0.5; `Data/JournalOffsetStore.cs` opens the second while
`Services/DatabaseManager.cs` opens the first. Consequences: two native
`SQLite.Interop.dll` loads per process, different `busy_timeout` defaults, and
`JournalOffsetStore` cannot share a transaction with the archive writer.

Exit condition (Wave 1, compiler-verified): migrate `DatabaseManager.cs` to
`Microsoft.Data.Sqlite`, drop `System.Data.SQLite.Core`, re-run
`EJLive.Verification` probe 15 (archive writer) and `dotnet test`. The gate rule
DEP-1 fails as soon as this row exists without the project name in this file.

## D-07 two wire-protocol definitions in one assembly &mdash; RESOLVED

The canonical 22-member `MsgType` lives in
`src/EJLive.Core/Engine/CommunicationProtocol.cs`
(`RsaPublicKey`, `AesSessionKey`, `Handshake`, `HandshakeAck`, `Heartbeat`,
`HeartbeatAck`, `StartFile`, `Chunk`, `ChunkAck`, `Complete`, `JournalAck`,
`Command`, `CommandResult`, `RemoteSessionStart`, `RemoteSessionFrame`,
`RemoteSessionStop`, `ImageSync`, `ImageAck`, `Broadcast`, `Disconnect`,
`Error`, `Unknown`).

The second `MsgType` declared in `src/EJLive.Core/Communication/MessageTypes.cs`
was removed when the L0 assembly was curated (E-17); the file no longer
exists in the compiled set. The only other `MsgType` in the repo lives in
`src/EJLive.Client.Service/Compatibility/ServiceStubs.cs`, but that enum is
in `namespace EJLive.Client.Service.Compatibility` — a sibling, not an
overlap — so no CS0433 is reachable and a single protocol decoder continues
to map every wire frame through `CommunicationProtocol.MsgType`.

The protocol is intentionally an L1 (Core) contract, not L0 (Shared):
`EJLive.Shared` has no `ProjectReference` to `EJLive.Core` (POL-4 forbids
upper-layer references inside L0), and lifting the enum to `Shared` would
require breaking that invariant for a single type. The header grammar
`<MsgType>:<byteLength>\n` is asserted by `RunNetworkProbeAsync` in
`EJLive.Verification`.

## D-08 merge dumps still in the compiled set (`EJLive.Core`) &mdash; RESOLVED (Wave 4)

Exit condition met: `check_merge_dumps.py --report` returns
`merge dumps: 0 files in the compiled set (0 in the debt ledger, 0 unlisted)`. The nine files
were rebuilt in Wave 1 (`67db886`); the last three SYN-5 hits were the phrase
*partial enum* inside Wave-1 repair notes, reworded in Wave 4 without touching code. The
section stays as history; `SYN-5` keeps guarding the compiled set against new dumps.


The tool that assembled this repository concatenated every variant of a type into one file, annotated each
fragment with its provenance (`// Variant from: d:\EJLIVE\EJlive_Reference_Projects\...`,
`// Class: X (from 3 sources)`), and left the copies side by side. The result is brace-balanced &mdash;
`SYN-1` passes it &mdash; and is not C#: members are declared up to four times, fragments of a copied object
initialiser sit in class bodies (`CS1519`), and enums carry the `partial` modifier, which does not exist for
enums (`CS1001`/`CS1002`/`CS1003` at parse time). `SYN-5` fails the gate on any compiled file carrying the
signature unless it is listed here, so the pile is fixed-size and cannot be re-enlarged by promoting an
archived dump.

* `src/EJLive.Core/Models/ATMConfig.cs`
* `src/EJLive.Core/Models/ATMDevice.cs`
* `src/EJLive.Core/Models/AuditLog.cs`
* `src/EJLive.Core/Models/CoreAdapters.cs`
* `src/EJLive.Core/Models/FleetSummary.cs`
* `src/EJLive.Core/Models/NetworkMessage.cs`
* `src/EJLive.Core/Models/SyncProgress.cs`
* `src/EJLive.Core/Services/UnifiedOperationalFusion.cs`
* `src/EJLive.Core/Models/Transaction.cs`

Repaired and removed from this list already: `src/EJLive.Core/Enums/ATMTypes.cs`, where each of the ten
enums existed in three or four copies with identical member sets. `python3
tools/gates/check_merge_dumps.py --repair-enum-file <path>` collapsed them into one declaration per enum
carrying the *union* of the members with the values the copies agreed on, which is mechanical and loses
nothing (verified: 10 enums, every member and every explicit value preserved, `SyncStatus` 9 = superset of
its 8-member variant).

`src/EJLive.Core/Services/UnifiedOperationalFusion.cs` is the one entry without provenance comments, added
after CI localised it: two `public sealed record ...(` header lines were deleted by the merge tool, leaving
orphan parameter lists (`CS1001`/`CS1002` at 274,275). Restoring them is *not* mechanical: `JournalEvidenceReport`
is then declared twice in the assembly, because `Models/CoreAdapters.cs` carries an empty `class
JournalEvidenceReport { }` placeholder that cannot satisfy the 10-argument `new JournalEvidenceReport(...)`
call in this same file, and `RemoteCommandPolicyDecision` is declared three times in `CoreAdapters.cs` with a
member set (`Approved`) that the orphan list (`Allowed`) contradicts. It needs the call sites read and a
compiler, so it joins this list instead of being guessed at.

Exit condition (Wave 1, per file, compiler-verified). The remaining files are class dumps whose copies differ
in member *sets*, not just layout, so no lossless text transform exists: read the real call sites, write one
declaration per type, then delete the row above and require `--report` to come back empty. Two attempts to
shortcut this were rejected during this pass, and the reason is recorded here so it is not retried: stripping
every bare `Name = value,` line deletes the members of any enum that spells its values one per line, and
"keep the richest copy" is wrong when the copies disagree, because the merge tool had no semantics and
neither copy is authoritative.


## D-09 AgentBootstrapper promotion (agent decomposition) — OPEN (Wave 5)

`src/EJLive.Client.WinForms/Agent/AgentBootstrapper.cs` has no compiled owner: the 1 395-line
single-copy source sits in the reference archive (`src/_reference/uncompiled/EJLive.Client.WinForms/Agent/`)
and still imports `EJLive.Client.WinForms.Supabase` — a namespace the platform has retired. The
activation audit therefore classifies its nominal path as `CoveredByBridge`
(`UnifiedClientServiceSupervisor`), and `UnifiedRuntimeTests.ServiceActivationAudit_ClassifiesCandidatesAsCompiledOrCovered`
asserts exactly that status with a pointer back to this row.

Exit condition (Wave 5): decompose the archived bootstrapper against the compiled agent surface
(`ClientStartupPlanner`, `ClientAgentWindowsService`, `JournalSyncStateService`), promote it to
`src/EJLive.Client.WinForms/Agent/AgentBootstrapper.cs` in the project's compile map, re-point the
test assertion to `ActiveCompiled`, and land the duplicate-type scan with `AgentBootstrapper` absent
from `DuplicateTypeFindings` (already true — the name is declared nowhere in the compiled set).
Until then the expectation stays honest: no dead façade is compiled to satisfy an assertion.
