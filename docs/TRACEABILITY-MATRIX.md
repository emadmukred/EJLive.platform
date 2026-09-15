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
| Class hierarchy / ownership | — | `artifacts/ActiveCompileMap.csv` (301 compiled, 239 archived rows) | exists — one owner per type key (`TYPE-1` = 0) |
| Toolbox / WinForms controls | "UI Elements, TOOLBOX [VS.NET]" | `docs/inventory/UI-SURFACES.md` (generated rows: form → controls → handlers → command methods) | partial — Wave 4 introduced the Designer-partial pattern (`JournalStudioForm.Designer.cs`: 42 controls; wiring in the companion `WireEvents()`, 14 handlers — both rows non-empty); replicating the pattern on the three consoles is queued Wave 5; every surface is Forms-only (`FILE-3` bans XAML) |
| Endpoint Console | "Client / Terminals" | `src/EJLive.Client.WinForms/` (`ClientMainForm`, 4 map entries + 21 archived) | partial — mapped surface exists, archived forms are Wave 1 |
| Enterprise Server console | "Central Server Controls" | `src/EJLive.Server.WinForms/` (`ServerMainForm`, `ATMCardPanel`, `ATMDetailDrawerForm`, `SyncDashboardForm`) | partial — 5 compiled / 44 archived |
| Windows Operations Console | "NOC Dashboard" | `src/EJLive.Monitoring.WinForms/` (`MainDashboardForm`) | partial — 4 compiled, retargeted from `net10.0-windows7.0`, WPF/unsafe removed |
| Installer UI | "installation, deployment" | `src/EJLive.Installer.WinForms/` (`InstallerForm`, `InstallerAutomationRunner`) | exists (unattended contract `--install --service … --silent` in prompt SS16) |
| Journal Analysis Log Studio | "EJ Analysis Log Studio with GUI + analytics" | `EJLive.Server.WinForms/JournalStudioForm.cs` (Wave 3 / SS-10.5, commit `0f00399`) | exists — vendor-aware load (`EjParserRegistry` × `UnifiedJournalEvidenceAnalyzer.DetectVendor`), virtual-mode grid + raw + ladder, anomaly tab (non-monotone offsets, duplicate ids, missing ids), analytics tab (classification histogram + anomaly Pareto + throughput/hr + reconciliation delta), CSV/JSON export |
| Client bootstrap pairing | "session companion of the service" | `EJLive.Client.Service/Program.cs` (single entry point after C-18; bootstrap-gated startup), `ClientAgentWindowsService` (health file at `DataRootPaths.HealthSnapshotFile`), `EJLive.Client.WinForms/Program.cs` (warning-not-crash) | exists - Wave 4 |
| Protocols / messaging | "distributed communication patterns" | `Core/Engine/CommunicationProtocol.cs` (22 `MsgType`), frame `{type}:{len}\n`, port 5656; `docs/inventory/PROTOCOL.md` | exists — `D-07` resolved in Wave 2 (legacy 10-member `MessageTypes.cs` removed during L0 curation E-17; the only sibling `MsgType` lives in `Client.Service.Compatibility` namespace and does not collide) |
| Encryption / signing | "security" | `RSA.Create(2048)`, `Aes.Create()`, `HMACSHA256`, `SecretRedactor`, `LogRedactionEngine`, DPAPI in `EJLive.Shared` | exists — prompt SS5/SS9 |
| Permissions & risk | "logic, permissions, authorization flows" | `Core/Services/RoleBasedAccess.cs` (`UserRole`×4 + `Permission` enum), `Core/Engine/CommandRiskLevel.cs` (Low/Medium/High/Critical), `Core/Security/SecurityHelper.cs`, `EJLive.Shared/Security/` (`PermissionEvaluator`, `CredentialRotationService`, `AuditChainVerifier`, `SensitiveDataMasker`); `docs/inventory/PERMISSIONS.md` | exists — matrix fixed in prompt SS9 (4×4, three chokepoints, deny-on-unknown) |
| Database schema + repositories | "complete database layer" | `Core/Services/DatabaseManager.cs` (canonical DDL + audit chain), `Core/Data/DatabaseMigrationsRunner.cs` (single `schema_migrations` book, v1-13, checksums, refusal guard), `Core/Data/Repositories/` (6 interface+impl pairs), `JournalOffsetStore`; `docs/inventory/DATABASE.md` (19 tables) | exists - Wave 4 (C-20): books collapsed, Phase-2 migrations applied at startup by `PlatformBootstrap`, repositories give every Phase-2 table a DML owner; `0001_Phase2_RequiredTables (2).sql` deduped (C-25) |
| Dataroot + bootstrap | "fixed central data paths, bootstrap" | `EJLive.Shared/DataRootPaths.cs` (resolution + fixed layout + probe), `EJLive.Core/Data/DataRootLayout.cs` (server archive tree), `EJLive.Core/Data/PlatformBootstrap.cs` (one machine, wired into the four host Programs) | exists - Wave 4 (C-19); pinned by `Wave4BootstrapTests` + verification probe `Data root bootstrap and schema book` |
| Central server controls | "business logic, orchestration, state" | `EJLive.Business` (13 files: gateway, journal storage/analytics, command policy/orchestrator), `EJLive.Server/Services/*` | exists — D-01 resolved in Wave 2 (three cross-assembly partial files archived; 14 keys now have one owner each in `EJLive.Core`) |
| Clients / agents / terminals | "client and terminals" | `EJLive.Client.Service` (21 files: `ClientAgentWindowsService`, `ClientServiceHost`, `AgentHeadlessController`, `AgentHealthReporter`, `RuntimeAgentConfigResolver`, `SystemBackupService`, `TimeSyncScheduler`, `LogBackupScheduler`, `Compatibility/{SafeLiveFileReader,ReflectionSafe,ServiceStubs,JournalOutboxAdapter}`) | exists — service is UI-free (`RunUiInServicePathProbe`) |
| Vendor parsers | "NCR/GRG electronic journal parsing" | `Core/Engine/{Ncr,Grg,Diebold,Hyosung,Wincor,Cashway}EjTransactionParser.cs` + `EjParserRegistry` + `IEjTransactionParser`; `Core/Xfs/Adapters/*`; `docs/inventory/VENDORS.md` | exists — one compiled parser per vendor (`POL-1` PASS) |
| Analysis Log Studio (full) | "EJ Analysis Log Studio with bulk analysis and Excel export" | `EJLive.Server.WinForms/JournalStudioForm.cs` + `JournalStudioForm.Designer.cs` (C-24 split) + `JournalStudioBulkAnalyzer.cs`; `EJLive.Core/Engine/ExcelWorkbookWriter.cs` (dependency-free OOXML); reconciliation via `IJournalArchiveRepository` | exists - Wave 4 (C-23/C-24): bulk <=500 files off-UI-thread with progress/cancel, Export Excel (Summary+Transactions), bulk Files+ByVendor sheets, archive-side reconciliation |
| Journal analysis / correlation | "EJ analysis" | `Core/Engine/CorrelationEngine.cs`, `Core/Services/MergedTraceCorrelationService.cs`, `Core/Xfs/Adapters/Ncr*TraceAdapter.cs`, `Core/Models/AnalysisRuntimeModels.cs`, `Core/Services/JournalSync*` services | partial — `MergedTraceCorrelationService` archived copies (11) deduped to one owner; `ReferenceCoverage` probe in verification |
| Tests | "all test specifications" | `src/EJLive.Tests/` 42 fixtures / 371 cases + 8 vendored journal fixtures (`src/EJLive.Tests/Samples/*.LOG`) | exists — `docs/inventory/TESTS.md` |
| Verification harness | "certification / readiness" | `src/EJLive.Verification/Program.cs` (23 probes) + `RegressionVerificationGate` (`Core/Engine`) + `ReleaseReadinessChecker` (10 checks: solution builds, core compiles, tests present, documentation, artefacts generated, all parsers present, XFS adapters present, network engines, security infrastructure, installer ready) | exists — wired into CI |
| Certification report | `Wave46-Certification-Report.md` | no owner in repo; not present in workspace | missing — G-1 |
| Security hardening (web removal, reflection ban, audit chain) | "OWASP, isolation, secure paths" | `EJLive.Core/Services/ServiceLocator.cs` (composition seam, no `Type.GetType` — C-22); web surface demoted (C-21); `audit_log` hash chain + `VerifyAuditChain` (C-20); `GIT-3`/`SEC-1..4` unchanged | exists — Wave 4 |
| Docs | "operational procedures" | `README.md`, `docs/CI.md`, `docs/ANALYSIS-FINDINGS.md`, `docs/DEBT-LEDGER.md`, `docs/inventory/*` (12 ledgers), `docs/12-service-activation-status.csv` (99 rows), prompt itself | exists |
| Configuration | "configurations" | `Directory.Build.props`, `.editorconfig`, `global.json`, `NuGet.Config`, `App.config`/`client.config`/`server.config`; `docs/inventory/CONFIGURATION.md` | exists |
| Build / package scripts | "deployment procedures" | `tools/build/build.ps1`, `tools/gates/run-gate.ps1`, `tools/package/package.bat` (was `src/Package.bat`, paths repaired), `tools/package/Package.bat` (legacy archive) | exists |
| Inventory tooling | "inventory regeneration per push" | `tools/inventory/ejlive_inventory.py` (12 artefacts) + `tools/inventory/service_activation.py`; CI enforces `--check` | exists |
| Safety policy | "no unsafe vocabulary" | `tools/gates/ejlive_static_gate.py` `SEC-1` (loaded terms), `SEC-2` (`unsafe`), `SEC-3` (weak crypto), `GIT-3` (literal credentials); `RunUnsafeTermScanProbe` | exists — 0 violations, 3 recorded `// safe:`/`// safe-file:` reasons |
| Incoming rules (auto-applied) | "every upload checked against binding rules" | `docs/incoming-rules/{00_CHANGELOG_Corrections.md, 01_eJLIVE_Architecture_Analysis_Prompt.md, 02_eJLIVE_Coding_Implementation_Prompt.md}`, `tools/incoming/incoming_rules.py`, `.github/workflows/incoming-rules.yml`, `tools/hooks/pre-commit` | exists — runs on every push and on every pre-commit; per-rule verdicts published as GitHub Actions annotations |

## Deliberate deviations from "promote everything now"

- **Archived ≠ deleted.** 237 files / 724 k lines sit in `src/_reference/` and stay
  linked (`EJLive.LegacyReference`). Promoting them blind would convert a known
  `CS0101` into unknown failures with no compiler in reach — Wave 1 promotes per file group.
- **Compile maps stay explicit** (`EnableDefaultCompileItems=false`): a glob would
  silently re-admit archived code, which is the exact failure mode this branch removed.
- **Gate never weakens to pass.** Where a violation is architectural (D-01…D-07) it is
  named in `docs/DEBT-LEDGER.md`; unlisted violations fail CI, and a fixed-but-still-listed
  violation also fails, so the ledger cannot rot.
