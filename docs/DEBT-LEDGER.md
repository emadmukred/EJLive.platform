# DEBT LEDGER -- reviewed, machine-checked deviations

Every entry below is a *known* violation that the static gate would otherwise
fail (`TYPE-2`, `GIT-2`, `FILE-6`). Debt is only legal when it is named here:
the gate reads this file, so an unlisted violation fails CI and a fixed
violation fails CI until the row is removed. Format: `id | what | why-now | exit`.

## D-01 cross-assembly partial split (Core &harr; Business) &mdash; 18 keys

`EJLive.Business/BusinessAdapters.cs` and `EJLive.Business/UnifiedServiceGateway.cs`
extend `EJLive.Core.Services.*` types with `partial` bodies that live in a
*different assembly*. C# composes partial types per assembly, so the Business
half is invisible to every other consumer, while any project that references
both assemblies (Tests, Verification, Installer.WinForms, UnifiedLauncher)
resolves the type name ambiguously (CS0433).

Exit condition (Wave 2, one commit, compiler-verified):
1. move each member body into the owning Core file (`src/EJLive.Core/Services/*`);
2. where the member genuinely belongs to Business, re-declare it as an
   extension method or an adapter in namespace `EJLive.Business`;
3. delete `BusinessAdapters.cs` from the Business compile map; re-run
   `python3 tools/inventory/ejlive_inventory.py` and require 0 in this section.

Keys:

- `EJLive.Core.Engine.OperationalStateStore`
- `EJLive.Core.Engine.ReportExportEngine`
- `EJLive.Core.Engine.ServerEngine`
- `EJLive.Core.Services.AlertManager`
- `EJLive.Core.Services.DatabaseManager`
- `EJLive.Core.Services.JournalSyncService`
- `EJLive.Core.Services.JournalSyncTrackingService`
- `EJLive.Core.Services.OperationalStateStore`
- `EJLive.Core.Services.RoleBasedAccess`
- `EJLive.Core.Services.TransactionAnalysisEngine`
- `EJLive.Core.Services.UnifiedGatewayActivationBatchResult`
- `EJLive.Core.Services.UnifiedGatewayReferenceCoverage`
- `EJLive.Core.Services.UnifiedServiceGateway`
- `EJLive.Core.Services.VendorRootCapabilityService`
- `EJLive.Core.Services.XfsLogAnalysisService`
- `EJLive.Shared.AppLogger`
- `EJLive.Shared.LightUiTheme`
- `EJLive.Shared.SecurityHelper`

## D-02 unparsable auto-merge dumps (archived, not compiled) -- 10 files

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

## D-05 headless service host missing a published executable identity

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

## D-07 two wire-protocol definitions in one assembly

`src/EJLive.Core/Communication/Protocol.cs` declares the canonical `MsgType` (22 members:
`RsaPublicKey`, `AesSessionKey`, `Handshake`, `HandshakeAck`, `Heartbeat`, `HeartbeatAck`,
`StartFile`, `Chunk`, `ChunkAck`, `Complete`, `JournalAck`, `Command`, `CommandResult`,
`RemoteSessionStart`, `RemoteSessionFrame`, `RemoteSessionStop`, `ImageSync`, `ImageAck`,
`Broadcast`, `Disconnect`, `Error`, `Unknown`) while
`src/EJLive.Core/Communication/MessageTypes.cs` declares a second, older `MsgType`
(10 members) in a sibling namespace. Both compile; a caller that imports both namespaces
gets an ambiguous `MsgType`, and the 10-member set contains no `ImageSync`/`RemoteSession*`
values, so a downgrade path silently mislabels frames.

Exit condition (Wave 2): delete `MessageTypes.cs`'s enum, move the surviving 22-member enum
into `EJLive.Shared` (the protocol becomes an L0 contract shared by endpoint, server and NOC),
re-point every `Protocol`/`CommunicationProtocol` framing helper at it, and keep the header
grammar `<MsgType>:<byteLength>\n` asserted by `RunNetworkProbeAsync` plus a new probe that
fails if a second `MsgType` enum appears anywhere in a compile map.

