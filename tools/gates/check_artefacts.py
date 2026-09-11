#!/usr/bin/env python3
"""Cross-consistency checks over the committed artefacts (CI: 'artefact integrity').

Lives in a file rather than as an inline `python3 - <<PY` heredoc in the workflow: a heredoc written
inside a YAML *plain* scalar is folded onto one line by the parser, so CI would execute something
other than what the file appears to say. Checked here, it runs identically on a workstation.

What it defends: the ledgers are generated together, so a partially regenerated set (map rewritten,
summary stale) is a lie about the repository. The counts must agree with each other, not merely be
non-empty.
"""
from __future__ import annotations

import csv
import json
import pathlib
import re
import sys

ROOT = pathlib.Path(__file__).resolve().parents[2]
MAP_HEADER = ["project", "file", "lines", "md5", "status"]
ALLOWED_STATUS = {"compiled", "linked-reference"}
REQUIRED_SUMMARY_KEYS = {
    "projects", "compiled_files", "compiled_lines", "reference_files", "reference_lines",
    "ledger_rows", "stale_includes", "duplicate_type_keys", "cycles", "build_order",
}


def main() -> int:
    problems: list[str] = []

    map_path = ROOT / "artifacts/ActiveCompileMap.csv"
    with map_path.open(newline="", encoding="utf-8-sig") as handle:
        reader = csv.reader(handle)
        header = next(reader, [])
        rows = [row for row in reader if row]
    if header != MAP_HEADER:
        problems.append(f"{map_path.name} header {header} != {MAP_HEADER}")
    compiled = 0
    for index, row in enumerate(rows, start=2):
        record = dict(zip(header, row))
        if not record.get("project") or not record.get("file"):
            problems.append(f"{map_path.name}:{index} has a blank project/file cell")
            break
        if record.get("status") not in ALLOWED_STATUS:
            problems.append(f"{map_path.name}:{index} status '{record.get('status')}' not in {sorted(ALLOWED_STATUS)}")
            break
        if not (ROOT / record["file"]).is_file():
            problems.append(f"{map_path.name}:{index} points at a missing file {record['file']}")
            break
        compiled += record.get("status") == "compiled"
    print(f"compile map: {len(rows)} rows ({compiled} compiled, {len(rows) - compiled} linked-reference)")

    summary = json.loads((ROOT / "artifacts/InventorySummary.json").read_text(encoding="utf-8"))
    missing = sorted(REQUIRED_SUMMARY_KEYS - summary.keys())
    if missing:
        problems.append(f"InventorySummary.json missing {missing}")
    else:
        linked = len(rows) - compiled
        for key, expected in (("compiled_files", compiled), ("reference_files", linked)):
            if int(summary[key]) != expected:
                problems.append(f"InventorySummary.json {key}={summary[key]} disagrees with ActiveCompileMap.csv ({expected})")
        for key in ("stale_includes", "duplicate_type_keys"):
            if int(summary[key]) != 0:
                problems.append(f"InventorySummary.json {key}={summary[key]}, expected 0 (see docs/DEBT-LEDGER.md)")
        for key in ("cycles", "stale_include_paths"):
            if summary[key]:
                problems.append(f"InventorySummary.json {key} is not empty: {summary[key]}")
        if len(summary["build_order"]) != int(summary["projects"]):
            problems.append("build_order does not cover every project")
        print(f"inventory summary: {summary['compiled_files']} files / {summary['compiled_lines']} lines, "
              f"{summary['reference_files']} linked files, {summary['projects']} projects")

    graph = (ROOT / "artifacts/ProjectDependencyGraph.md").read_text(encoding="utf-8")
    if "acyclic OK" not in graph:
        problems.append("ProjectDependencyGraph.md does not certify the layering as acyclic")

    probes = ROOT / "src/EJLive.Verification/Program.cs"
    text = probes.read_text(encoding="utf-8") if probes.is_file() else ""
    # Probes are the entries of the `checks` list literal, i.e. `Run<Name>Probe[Async]()` calls.
    # Counting inside that literal is what keeps the number honest: `Probe(` also appears in the
    # method definitions below it.
    block = re.search(r"var checks = new List<.*?>\s*\{(.*?)\n\};", text, re.DOTALL)
    probe_count = len(re.findall(r"Run\w+Probe(?:Async)?\(", block.group(1))) if block else 0
    if "bool Passed" not in text:
        problems.append("src/EJLive.Verification/Program.cs is missing or lost its (Name, Passed, Detail) harness")
    elif probe_count == 0:
        problems.append("src/EJLive.Verification/Program.cs registers no probes")
    else:
        print(f"verification probes: {probe_count}")

    tests = ROOT / "docs/inventory/TESTS.md"
    fixtures = 0
    if tests.is_file():
        fixtures = sum(1 for line in tests.read_text(encoding="utf-8").splitlines()
                       if line.startswith("| EJLive.Tests"))
    if fixtures == 0:
        problems.append("docs/inventory/TESTS.md lists no test fixtures")
    else:
        print(f"test fixtures: {fixtures}")

    for problem in problems:
        print(f"error: {problem}")
    print("artefact check:", "FAIL" if problems else "PASS")
    return 1 if problems else 0


if __name__ == "__main__":
    sys.exit(main())
