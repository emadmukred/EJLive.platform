# TRACEABILITY MATRIX — specification artefacts vs repository

Cross-map of every artefact class the engineering brief names, against the
curated tree. `exists` = present, compiled, ledger-covered; `partial` = present
but not fully wired into a compile map or not exercised by a probe/test;
`missing` = no owner in the compiled set (built in a wave); `debt` = present and
deliberately tolerated under `docs/DEBT-LEDGER.md`.

## Gaps in the inputs themselves (must be resolved by the requester)

| id | gap | effect on this pass | what closes it |
|---|---|---|---|
| G-1 | The 9 specification files named in the brief (`EJLive Platform — Full Technical Build.md`, `EJLIVE.PLATFORM — MASTER ENGINEERING ANALYSIS & SPECIFICATION (Rev 2)`, `… SPECIFICATION (CONTINUED)` × 2, `… SPECIFICATION`, `EJLive-Engineering-Implementation-Prompt-v3.0.md`, `EJLive-Platform-Engineering-Prompt (1)/(2).md`, `Wave46-Certification-Report.md`) were **not present in the workspace** (`/home/user/uploads/` does not exist in this sandbox; nothing on disk matches those names) | Steps "read the document fully / extract every artefact it specifies / execute all provided markdown prompt files" cannot be performed against those bodies. The pass therefore used the repository's own executable contract as the specification: 14 compile maps, 12 ledgers, 23 `EJLive.Verification` probes, `RegressionVerificationGate`, `ReleaseReadinessChecker`, 371 test cases | Re-attach the files (or paste them into the workspace). Then this matrix gains a `spec ref` column and prompt `SS17` gains the spec-clause ↔ SS-nn mapping; nothing else about the repo work is invalidated |
| G-2 | Brief says implement on branch `/ejlive-platform`; this session is bound to `arena/01a08d42-ejlive-platform` (Arena tracks the session by that branch) | Work landed on the session branch instead of the literal name; content is identical | Branch rename or merge into the target branch on request (the session cannot switch branches) |
| G-3 | No .NET SDK and no NuGet egress in the sandbox | Nothing is compile-verified; `dotnet build/test/probes` remain red until the first Windows CI run | CI job `build` (docs/CI.md); Wave 1 promotes archived files only behind a build gate |

## Artefact cross-map

| artefact class | specified in brief | repository owner | status |
|---|---|---|---|
| Projects / assemblies (14) | "all .NET projects, class hierarchies, namespaces" | `EJLive.Platform.sln` + `.slnx`, `artifacts/ProjectDependencyGraph.md` | exists — 29 csproj collapsed to 14, acyclic, L0–L5 |
| Namespaces | "harmonize naming" | every project map + `NAME-1` allowlist | partial — 3 shipped-exe names intentionally differ (prompt SS2, `ASSEMBLY_ALLOWLIST`) |
| Class hierarchy / ownership | — | `artifacts/ActiveCompileMap.csv` (303 compiled, 237 archived rows) | exists — one owner per type key (`TYPE-1` = 0) |
| Toolbox / WinForms controls | "UI Elements, TOOLBOX [VS.NET]" | `docs/inventory/UI-SURFACES.md` (93 rows: form → controls → handlers → command methods) | partial — 0-handler rows are the UI-1 backlog; every surface is Forms-only (`FILE-3` bans XAML) |
| Endpoint Console | "Client / Terminals" | `src/EJLive.Client.WinForms/` (`ClientMainForm`, 4 map entries + 21 archived) | partial — mapped surface exists, archived forms are Wave 1 |
| Enterprise Server console | "Central Server Controls" | `src/EJLive.Server.WinForms/` (`ServerMainForm`, `ATMCardPanel`, `ATMDetailDrawerForm`, `SyncDashboardForm`) | partial — 5 compiled / 44 archived |
| Windows Operations Console | "NOC Dashboard" | `src/EJLive.Monitoring.WinForms/` (`MainDashboardForm`) | partial — 4 compiled, retargeted from `net10.0-windows7.0`, WPF/unsafe removed |
| Installer UI | "installation, deployment" | `src/EJLive.Installer.WinForms/` (`InstallerForm`, `InstallerAutomationRunner`) | exists (unattended contract `--install --service … --silent` in prompt SS16) |
| Journal Analysis Log Studio | "EJ Analysis Log Studio with GUI + analytics" | none | missing — specified in prompt SS10.5, scheduled Wave 1/3 |
| Protocols / messaging | "distributed communication patterns" | `Core/Communication/Protocol.cs` (22 `MsgType`), frame `{type}:{len}\n`, port 5656; `docs/inventory/PROTOCOL.md` | partial — `debt` D-07 (a second 10-member `MsgType`), prompt SS5 normative |
| Encryption / signing | "security" | `RSA.Create(2048)`, `Aes.Create()`, `HMACSHA256`, `SecretRedactor`, `LogRedactionEngine`, DPAPI in `EJLive.Shared` | exists — prompt SS5/SS9 |
| Permissions & risk | "logic, permissions, authorization flows" | `Core/Services/RoleBasedAccess.cs` (`UserRole`×4 + `Permission` enum), `Core/Engine/CommandRiskLevel.cs` (Low/Medium/High/Critical), `Core/Security/SecurityHelper.cs`, `EJLive.Shared/Security/` (`PermissionEvaluator`, `CredentialRotationService`, `AuditChainVerifier`, `SensitiveDataMasker`); `docs/inventory/PERMISSIONS.md` | exists — matrix fixed in prompt SS9 (4×4, three chokepoints, deny-on-unknown) |
| Database schema | "complete database layer" | `Core/Data/DatabaseSchema.cs`, `Core/Services/DatabaseManager.cs`, `Data/Migrations/0001_Phase2_RequiredTables.sql`, `JournalOffsetStore`, `DatabaseMigrationsRunner`; `docs/inventory/DATABASE.md` (19 tables) | partial — `debt` D-06 (two SQLite providers), duplicate migration books to collapse (prompt SS11) |
| Central server controls | "business logic, orchestration, state" | `EJLive.Business` (15 files: gateway, journal storage/analytics, command policy/orchestrator), `EJLive.Server/Services/*` | exists — `partial` only in the sense that D-01 splits gateways across assemblies |
| Clients / agents / terminals | "client and terminals" | `EJLive.Client.Service` (21 files: `ClientAgentWindowsService`, `ClientServiceHost`, `AgentHeadlessController`, `AgentHealthReporter`, `RuntimeAgentConfigResolver`, `SystemBackupService`, `TimeSyncScheduler`, `LogBackupScheduler`, `Compatibility/{SafeLiveFileReader,ReflectionSafe,ServiceStubs,JournalOutboxAdapter}`) | exists — service is UI-free (`RunUiInServicePathProbe`) |
| Vendor parsers | "NCR/GRG electronic journal parsing" | `Core/Engine/{Ncr,Grg,Diebold,Hyosung,Wincor,Cashway}EjTransactionParser.cs` + `EjParserRegistry` + `IEjTransactionParser`; `Core/Xfs/Adapters/*`; `docs/inventory/VENDORS.md` | exists — one compiled parser per vendor (`POL-1` PASS) |
| Journal analysis / correlation | "EJ analysis" | `Core/Engine/CorrelationEngine.cs`, `Core/Services/MergedTraceCorrelationService.cs`, `Core/Xfs/Adapters/Ncr*TraceAdapter.cs`, `Core/Models/AnalysisRuntimeModels.cs`, `Core/Services/JournalSync*` services | partial — `MergedTraceCorrelationService` archived copies (11) deduped to one owner; `ReferenceCoverage` probe in verification |
| Tests | "all test specifications" | `src/EJLive.Tests/` 42 fixtures / 371 cases + 8 vendored journal fixtures (`src/EJLive.Tests/Samples/*.LOG`) | exists — `docs/inventory/TESTS.md` |
| Verification harness | "certification / readiness" | `src/EJLive.Verification/Program.cs` (23 probes) + `RegressionVerificationGate` (`Core/Engine`) + `ReleaseReadinessChecker` (10 checks: solution builds, core compiles, tests present, documentation, artefacts generated, all parsers present, XFS adapters present, network engines, security infrastructure, installer ready) | exists — wired into CI |
| Certification report | `Wave46-Certification-Report.md` | no owner in repo; not present in workspace | missing — G-1 |
| Docs | "operational procedures" | `README.md`, `docs/CI.md`, `docs/ANALYSIS-FINDINGS.md`, `docs/DEBT-LEDGER.md`, `docs/inventory/*` (12 ledgers), `docs/12-service-activation-status.csv` (99 rows), prompt itself | exists |
| Configuration | "configurations" | `Directory.Build.props`, `.editorconfig`, `global.json`, `NuGet.Config`, `App.config`/`client.config`/`server.config`; `docs/inventory/CONFIGURATION.md` | exists |
| Build / package scripts | "deployment procedures" | `tools/build/build.ps1`, `tools/gates/run-gate.ps1`, `tools/package/package.bat` (was `src/Package.bat`, paths repaired), `tools/package/Package.bat` (legacy archive) | exists |
| Inventory tooling | "inventory regeneration per push" | `tools/inventory/ejlive_inventory.py` (12 artefacts) + `tools/inventory/service_activation.py`; CI enforces `--check` | exists |
| Safety policy | "no unsafe vocabulary" | `tools/gates/ejlive_static_gate.py` `SEC-1` (loaded terms), `SEC-2` (`unsafe`), `SEC-3` (weak crypto), `GIT-3` (literal credentials); `RunUnsafeTermScanProbe` | exists — 0 violations, 3 recorded `// safe:`/`// safe-file:` reasons |

## Deliberate deviations from "promote everything now"

- **Archived ≠ deleted.** 237 files / 724 k lines sit in `src/_reference/` and stay
  linked (`EJLive.LegacyReference`). Promoting them blind would convert a known
  `CS0101` into unknown failures with no compiler in reach — Wave 1 promotes per file group.
- **Compile maps stay explicit** (`EnableDefaultCompileItems=false`): a glob would
  silently re-admit archived code, which is the exact failure mode this branch removed.
- **Gate never weakens to pass.** Where a violation is architectural (D-01…D-07) it is
  named in `docs/DEBT-LEDGER.md`; unlisted violations fail CI, and a fixed-but-still-listed
  violation also fails, so the ledger cannot rot.
