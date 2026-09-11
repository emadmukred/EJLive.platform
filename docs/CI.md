# CI — pipeline contract (`.github/workflows/ci.yml`)

Two jobs, one runner each. Both must be green to merge; the gate job is the
repository-rule authority, the build job is the compiler authority.

## Job `structure` — `ubuntu-latest` (no .NET required)

| step | command | fails when |
|---|---|---|
| ledgers | `python3 tools/inventory/ejlive_inventory.py --check` | any drift between committed ledgers and the compile maps (repo rule INV-1: regenerate per push) |
| activation ledger | `python3 tools/inventory/service_activation.py --check` | `docs/12-service-activation-status.csv` stale |
| static gate | `python3 tools/gates/ejlive_static_gate.py` | any of 38 rules fail (`GIT`, `NAME`, `FILE`, `TYPE`, `ARCH`, `SEC`, `SYN`, `POL`, `DEP`, `ART`, `LED`) |
| cross-check | `python3 tools/gates/ejlive_static_gate.py --verbose` in the log | advisory output only |

The gate runs on Linux because every rule it checks is structural, not
Windows-specific: `dotnet` is not invoked, and `artifacts/*.csv|json|md` are
committed so a reviewer can read the same ledgers CI read.

## Job `build` — `windows-latest` (the only place a compiler exists)

```
actions/setup-dotnet@v4  dotnet-version: 8.0.x
dotnet restore EJLive.Platform.slnx --configfile NuGet.Config
dotnet build EJLive.Platform.slnx -c Release -m:1 /p:BuildInParallel=false
dotnet test  src/EJLive.Tests/EJLive.Tests.csproj -c Release --no-build
dotnet run   --project src/EJLive.Verification/EJLive.Verification.csproj -c Release --no-build
powershell   tools/package/package.bat Release      # artefacts: 3 payload zips
```

Why `-m:1 /p:BuildInParallel=false`: `EJLive.Core` and `EJLive.Shared` share
generated sources, and MSBuild node reuse races on them — a parallel build
produces spurious `CS2001`/`MSB3021` failures that a serial build never shows.
The flag is not a performance knob; it is a correctness requirement.

`EJLive.Verification` is a build step, not a test: its 23 probes assert the
composition contract (protocol reachability, journal-ack metadata, UI-free
service path, unsafe-term scan, duplicate-type scan, source-truth/file-linkage
checks) against the *published* assemblies, which `dotnet test` cannot cover.

## Failure triage

| symptom | cause | fix |
|---|---|---|
| `ledger drift` in `structure` | source moved but ledgers not regenerated | run the generator, commit its output in the same commit |
| `TYPE-4` source outside a map | file added to a project directory without editing the map | add the `<Compile Include>` or move the file to `src/_reference/` |
| `SYN-1` unbalanced blocks | an editor/merge tool produced a dump | archive it; write the type properly (prompt SS15) |
| `NAME-1` identity deviation | new project, or a renamed shipped exe | fix, or add the exe to `ASSEMBLY_ALLOWLIST` **and** `tools/package/package.bat` in the same commit |
| `DEP-1` provider mix | second ADO.NET provider introduced | use the canonical provider; see `DEBT-LEDGER.md` D-06 |
| `build` fails, `structure` green | compile-only error (missing type from an archived file) | promote the archived file for that type, per Wave 1 rules |
| `structure` fails, `build` green | rule violation the compiler cannot see | never weaken the rule to pass; fix the tree or record reviewed debt |

## Local pre-push

```
powershell tools/gates/run-gate.ps1                 # ledgers + gate (+ build/test when an SDK exists)
powershell tools/build/build.ps1 -WithTests -WithGate
```

`run-gate.ps1` degrades to ledgers+gate only when `dotnet` is absent, and reports
that it skipped the compiler steps rather than pretending success.
