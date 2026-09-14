# 01 · eJLIVE Architecture Analysis Prompt (binding)

> Architectural analysis rules. Every new upload is checked against these
> rules before it lands in a compile map. The companion tool
> `tools/incoming/incoming_rules.py --check-architecture <upload>` reports
> per-rule verdicts; `PASS` means the upload preserves the invariant, `FAIL`
> blocks the merge.

## A. Project topology

**A-1.** Exactly **14 projects** in `EJLive.Platform.sln` and `EJLive.Platform.slnx`
(mirrored). Layers L0–L5. A new project is allowed only with a recorded
ledger row in `docs/inventory/` and an entry in `EJLive.Platform.sln[x]`
*and* an explicit compile map.

**A-2.** Layer reference rule (ARCH-4): a project may reference a strictly
lower layer only. Forbidden upward references:
- `EJLive.Shared` ← `EJLive.Core` or higher
- `EJLive.Core` ← `EJLive.Business` or higher
- `EJLive.Business` ← `EJLive.Application`, `*.Client.Service`, `*.Server`
- `EJLive.Client.WinForms` ← `EJLive.Server.WinForms` (sibling, not parent)
- `EJLive.Server.WinForms` ← `EJLive.Client.WinForms`

**A-3.** Runtime projects never reference `src/_reference/` (ARCH-3). Only
`EJLive.LegacyReference` links the archive, and only as `Compile Include`
of files in `src/_reference/**` (no `ProjectReference`).

**A-4.** Dependency graph is acyclic (ARCH-2). A new `ProjectReference` that
introduces a cycle fails the gate.

## B. Compile maps

**B-1.** Every project directory carries an explicit compile map (`Compile`
items only inside `<ItemGroup>`, no globs, no `Services\*.cs` patterns).
`EnableDefaultCompileItems=false` in `Directory.Build.props` is mandatory
(FILE-4).

**B-2.** Every file under `src/<Project>/**` is either:
  (a) listed in the project's `<Compile Include>`;
  (b) under `src/_reference/` (archive);
  (c) flagged as a non-source artefact (`.resx`, `.config`, `.sql`, `.json` schema).
Anything else fails `TYPE-4`.

**B-3.** Every `<Compile Include>` resolves to a real file (TYPE-3). Stale
includes fail the gate.

## C. Type ownership

**C-1.** Exactly one compiled owner per type key per assembly (TYPE-1).
A re-introduced `JournalSyncTrackingService` in two assemblies, for
example, fails TYPE-1 *or* TYPE-2 depending on the shape (sealed vs
partial).

**C-2.** No cross-assembly partial split unless recorded in
`docs/DEBT-LEDGER.md` (TYPE-2). A new upload that creates a cross-assembly
partial *and* does not record it fails the gate.

**C-3.** Public types: one per file unless they are partials in the *same
assembly*. Sealed by default. `record` for DTOs.

## D. Wire protocol

**D-1.** Single source for `MsgType`: `EJLive.Core.Engine.CommunicationProtocol`
(22 members). Any second `MsgType` enum in a compile map fails the gate
once a corresponding probe is enabled (the protocol probe is
`RunNetworkProbeAsync` in `EJLive.Verification`).

**D-2.** Envelope grammar `<MsgType>:<byteLength>\n<body>` is the only
framing. UTF-8 JSON for control messages, raw bytes for `Chunk` /
`RemoteSessionFrame` / `ImageSync`.

**D-3.** Ports: server 5656 (`AppConstants.DefaultPort`), no UDP, no
multicast, no port scanning.

## E. Data layer

**E-1.** Single ADO.NET provider per assembly (DEP-1). `EJLive.Core` is on
`Microsoft.Data.Sqlite` 8.0.5; any new SQLite package must either
replace this one or be added to a *different* assembly.

**E-2.** One package version across the graph (DEP-2). Re-pinning the
same package to a different version in another project fails the gate.

**E-3.** Every table has a `CREATE` in `DatabaseSchema.cs` / a migration
*and* a DML consumer (DB-1 ledger). A migration that creates a table
without a corresponding `INSERT`/`SELECT` site fails `DB-1`.

## F. UI surface

**F-1.** Windows Forms only (FILE-3). No WPF, no WinUI, no Blazor, no web
host, no external UI framework.

**F-2.** Every form binds at least one handler and exposes at least one
command method (UI-1 ledger). A new form with zero handlers fails the
gate.

**F-3.** No synchronous wait on a UI thread (POL-2): no `.Result`,
`.Wait()`, `Thread.Sleep` on a UI path.

## G. Security

**G-1.** No loaded vocabulary (SEC-1). `Kill`, `PasswordDeriveBytes`,
`MD5`, `SHA-1`, `DES`, ECB, `TripleDES`, `RC4` are flagged unless annotated
`// safe:` / `// safe-file:` with a one-line reason.

**G-2.** No `unsafe` (SEC-2). `AllowUnsafeBlocks` must be `false` in every
project.

**G-3.** No weak crypto on a security path (SEC-3). Allowed only for
vendor archive fingerprints with an explicit reason.

**G-4.** No P/Invoke without `SetLastError = true`, `CharSet.Unicode`, and
a tested failure path (SEC-4).

**G-5.** No real credentials in tests or fixtures (GIT-3). Synthetic
fixtures must be annotated `// synthetic`.

## H. Concurrency

**H-1.** One `Channel<T>` per pipeline stage (`BoundedChannelOptions`
capacity 1024, `FullMode = Wait`); no shared mutable collections between
stages.

**H-2.** UI updates only via `IProgress<T>` posted to the message loop or
`Control.Invoke`; never `.Result`, `.Wait()`, `Thread.Sleep` on a UI
thread.

**H-3.** Every loop owns a `CancellationToken`; shutdown drains for ≤ 5 s
then records `ShutdownAbandoned` (event name in
`docs/EJLIVE-ENGINEERING-PROMPT.md` SS14).

## I. Patterns (mandated)

**I-1.** Result-carrying API — exceptions for faults, refusals are data.

**I-2.** Idempotent ingest — `INSERT … ON CONFLICT DO NOTHING` keyed by
the unique index, never a re-read of the vendor file.

**I-3.** Parser shape — one vendor per file, sniff then stream, no
allocation per line. POL-1 holds.

**I-4.** Fail-closed security check — deny on unknown, never on
default-allow.

**I-5.** UI handler — thin, async, cancellable, no business logic.

## J. Delivery / waves

**J-1.** Every commit carries an `SS-nn` reference (commit message
trailer `SS-nn:`).

**J-2.** Wave resolutions are listed in `00_CHANGELOG_Corrections.md`.
Re-opening a wave resolution requires a new entry with a `RESURFACED`
status; the incoming-rules toolchain will refuse the upload otherwise.

**J-3.** `docs/DEBT-LEDGER.md` is the *only* tolerated violation list.
Removing a row without fixing the underlying issue fails the gate; adding
a new entry requires a one-line justification in the PR body.

## K. Acceptance (SS18)

The full Definition of Done is in `docs/EJLIVE-ENGINEERING-PROMPT.md`
SS18. Summary: `dotnet build` 0 errors, `dotnet test` 371+ cases, 23/23
verification probes, `python3 tools/inventory/ejlive_inventory.py`
followed by `python3 tools/gates/ejlive_static_gate.py` returning PASS
(41 rules, 0 advisory).
