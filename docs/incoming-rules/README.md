# Incoming rules — applied automatically to every upload

> Three binding documents that the toolchain enforces on every new upload
> to this repository. The companion tool
> `tools/incoming/incoming_rules.py` reads them, and the workflow
> `.github/workflows/incoming-rules.yml` runs the same checks on every
> push and pull request.

## Files

| file | role |
|---|---|
| `00_CHANGELOG_Corrections.md` | every correction applied to the repo, its finding, its fix, and its current status (CLOSED / RESURFACED). The incoming toolchain refuses any upload that re-introduces a CLOSED entry. |
| `01_eJLIVE_Architecture_Analysis_Prompt.md` | the architectural rules (project topology, compile maps, type ownership, wire protocol, data layer, UI surface, security, concurrency, patterns, waves, acceptance). |
| `02_eJLIVE_Coding_Implementation_Prompt.md` | the implementation rules (file shape, SQL, parsing, network, observability, code patterns, anti-patterns, language, tests, definition of done). |

## How it works

```
   upload lands (PR, push, attach, …)
                 │
                 ▼
   ┌───────────────────────────────┐
   │  tools/incoming/              │
   │    incoming_rules.py          │   1. parse every file in the upload
   │                               │   2. cross-reference 00/01/02
   │                               │   3. compare against active compile map
   │                               │   4. compare against docs/DEBT-LEDGER.md
   └───────────────────────────────┘
                 │
       ┌─────────┴─────────┐
       │                   │
   no violations      violation(s)
       │                   │
       ▼                   ▼
    merge proceeds     merge blocked, report uploaded as PR check
```

The CI job `.github/workflows/incoming-rules.yml` runs the tool on every
push. The pre-commit hook (`tools/hooks/pre-commit`) runs the same check
locally so a violation is caught before the commit ever reaches GitHub.

## Running it locally

```bash
# Check a single file against all three rule sets
python3 tools/incoming/incoming_rules.py \
    --upload src/EJLive.Server.WinForms/NewForm.cs

# Check every file in a folder (the way CI does)
python3 tools/incoming/incoming_rules.py \
    --upload src/ \
    --check-architecture \
    --check-implementation \
    --check-changelog

# Run inside a pre-commit check
./tools/hooks/pre-commit
```

Exit codes:
- `0` — upload is consistent with the rules and the existing tree.
- `2` — at least one rule violation; per-rule JSON written to stdout and
  the same JSON is published as a GitHub Actions annotation when run
  from CI.
- `3` — internal error (the toolchain itself is broken; file an issue).

## When the rules disagree with the upload

1. The CI job prints the offending file, the rule it violates, and a one-line
   explanation. Example:
   ```
   FAIL [C-2] src/EJLive.Business/Adapters/SomeAdapter.cs
       cross-assembly partial for EJLive.Core.Services.AlertManager
       → no row in docs/DEBT-LEDGER.md for this key
       → re-declare in EJLive.Business as extension methods, not partial
   ```
2. Fix the upload (the toolchain never edits the upload; it only reports).
3. Re-run the check; once the verdict is clean, the merge unblocks.

## When the rules disagree with the upload on purpose

There is no override. The repository rule is "fix the tree or record named
debt in `docs/DEBT-LEDGER.md`". A new debt row is added with a one-line
justification in the PR body; the gate reads `docs/DEBT-LEDGER.md` and
accepts the entry without a code fix.

## Adding a new rule

1. Add the rule to one of the three files above with a unique letter/number
   (A.1, B.2, …, K.1, K.2, …, T.1, …).
2. Add the rule implementation to `tools/incoming/incoming_rules.py`.
3. Add a static-gate rule in `tools/gates/ejlive_static_gate.py` if it can be
   checked without parsing the upload (the gate runs on every push).
4. Commit with an `SS-nn` trailer; the CI job picks up the change on the
   next push.
