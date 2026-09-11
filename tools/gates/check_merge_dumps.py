#!/usr/bin/env python3
"""Merge-dump signature detection (and the one deterministic repair that exists).

The tool that assembled this repository concatenated every variant of a type into a single file and
annotated each fragment with its provenance. The output is brace-balanced - so a brace-balance rule
passes it - and is not valid C#: object-initialiser fragments sit in class bodies, whole members are
declared three times, and enums are marked `partial`, which is not a C# construct.

`MARKER` and `ILLEGAL` are defined here because both this tool and the static gate (`SYN-5`) must agree
on what a dump is; a rule and a repair that disagree about their input are worse than either alone.

    python3 tools/gates/check_merge_dumps.py --report
    python3 tools/gates/check_merge_dumps.py --repair-enum-file src/EJLive.Core/Enums/ATMTypes.cs
"""
from __future__ import annotations

import argparse
import collections
import os
import re
import sys

ROOT = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
sys.path.insert(0, os.path.join(ROOT, "tools", "inventory"))
import importlib  # noqa: E402

inv = importlib.import_module("ejlive_inventory")

MARKER = re.compile(r"//\s*(?:Variant(?:\s+from)?\s*:|Class:\s*\w+\s*\(from \d+ sources\w*\))", re.I)
ILLEGAL = re.compile(r"\bpartial\s+enum\b")
FRAGMENT = re.compile(r"^\s*[A-Za-z_]\w*\s*=\s*[^\n=]*,\s*$", re.M)


def compiled_files() -> list[str]:
    return sorted(f for p in inv.load_projects().values() for f in p.compiled)


def scan(files: list[str] | None = None) -> list[tuple[str, int, int, int]]:
    """-> [(file, provenance fragments, illegal `partial enum`, bare initialiser fragments)]"""
    rows = []
    for f in files if files is not None else compiled_files():
        text = inv.read(f)
        marks, illegal, frag = len(MARKER.findall(text)), len(ILLEGAL.findall(text)), 0
        if marks:
            frag = len([m for m in FRAGMENT.finditer(text) if m.start() > 0])
        if marks or illegal:
            rows.append((f, marks, illegal, frag))
    return rows


def enum_spans(text: str) -> list[tuple[int, int, str, list[tuple[str, str | None]]]]:
    """Every `(public|internal) [partial] enum NAME { ... }` declaration, with span and members.

    Members are read from the brace-balanced body by splitting on commas, which is how C# reads them and
    the only way that covers both layouts the dumps contain: one-line shims (`public enum X { A, B }`)
    and multi-line members. A declaration whose closing brace the merge tool cut is skipped rather than
    half-parsed, and `partial` before `enum` is accepted here because removing the modifier is the only
    legal reading of it.
    """
    spans = []
    for match in re.finditer(r"(?:public|internal)\s+(?:partial\s+)?enum\s+(\w+)\s*\{", text):
        depth, end = 0, None
        for i in range(match.end() - 1, len(text)):
            if text[i] == "{":
                depth += 1
            elif text[i] == "}":
                depth -= 1
                if depth == 0:
                    end = i
                    break
        if end is None:
            continue
        body = re.sub(r"//[^\n]*", "", text[match.end():end])
        members = []
        for part in body.split(","):
            entry = re.match(r"\s*([A-Za-z_]\w*)\s*(?:=\s*([\w\-]+))?\s*$", part)
            if entry:
                members.append((entry.group(1), entry.group(2)))
        spans.append((match.start(), end + 1, match.group(1), members))
    return spans


def merge_enums(name: str, variants: list[list[tuple[str, str | None]]]) -> str:
    """One declaration carrying the union of the members its copies declared."""
    order, values = [], {}
    for members in sorted(variants, key=lambda v: -len(v)):   # richest copy decides member order
        for member, value in members:
            if member not in order:
                order.append(member)
            if value is not None:
                values.setdefault(member, collections.Counter())[value] += 1
    lines = []
    for index, member in enumerate(order):
        counter = values.get(member)
        value = "" if counter is None else f" = {counter.most_common(1)[0][0]}"
        lines.append(f"        {member}{value}{',' if index < len(order) - 1 else ''}")
    return f"public enum {name}\n    {{\n" + "\n".join(lines) + "\n    }"


def repair_enum_file(path: str) -> int:
    """Collapse duplicate copies of each enum *in place*.

    The first copy of an enum name is replaced by the merged declaration and every later copy is deleted;
    the text between them - classes, methods, comments - is untouched, because the dumps interleave
    several kinds of member. Idempotent: a file with one declaration per enum is left as it is (apart
    from the illegal `partial` modifier, which is removed wherever it precedes `enum`).
    """
    text = inv.read(path)
    out = ILLEGAL.sub("enum", text)
    spans = enum_spans(out)
    by_name: dict[str, list[tuple[int, int, str, list]]] = {}
    for span in spans:
        by_name.setdefault(span[2], []).append(span)
    log, edits = [], []
    for name, group in by_name.items():
        members = [m for g in group for m in g[3]]
        if len(group) == 1 and not ILLEGAL.search(text[max(0, group[0][0]):group[0][0] + 40]):
            continue
        if not members:
            # nothing parsed: rewriting would replace a one-line or truncated body with an empty enum,
            # which compiles and silently deletes the members every consumer names.
            print(f"  {name}: SKIPPED (no members parsed - body layout not understood, repair by hand)")
            continue
        merged = merge_enums(name, [g[3] for g in group])
        edits.append((group[0][0], group[0][1], merged))
        edits.extend((s0, e0, "") for _n, s0, e0 in [(0, g[0], g[1]) for g in group[1:]])
        log.append(f"  {name}: {len(group)} copies (members {[len(g[3]) for g in group]}) "
                   f"-> {len(merged.splitlines()) - 3} declared once")
    for start, end, replacement in sorted(edits, key=lambda e: -e[0]):
        out = out[:start] + replacement + out[end:]
    if not edits:
        print(f"{path}: one declaration per enum ({len(by_name)} enums)"
              + (", `partial` modifier removed" if out != text else ""))
        return _write(path, out) if out != text else 0
    return _write(path, out, log)


def _write(path: str, content: str, log: list[str] | None = None) -> int:
    with open(os.path.join(ROOT, path), "w", encoding="utf-8", newline="\r\n" if "\r\n" in content else "\n") as handle:
        handle.write(content)
    for line in log or []:
        print(line)
    print(f"{path}: rewritten")
    return 0


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--report", action="store_true", help="list dumps in the compiled set")
    parser.add_argument("--check", action="store_true", help="same as --report, exit 1 on an unlisted dump")
    parser.add_argument("--repair-enum-file", metavar="PATH", help="collapse duplicate enum copies in PATH")
    args = parser.parse_args()

    if args.repair_enum_file:
        return repair_enum_file(args.repair_enum_file)

    debt = ""
    debt_path = os.path.join(ROOT, "docs/DEBT-LEDGER.md")
    if os.path.isfile(debt_path):
        debt = open(debt_path, encoding="utf-8").read()
    rows = scan()
    listed = [r for r in rows if r[0] in debt]
    unlisted = [r for r in rows if r[0] not in debt]
    for path, marks, illegal, frag in rows:
        state = "debt" if path in debt else "UNLISTED"
        print(f"{state:8s} {path}  fragments={marks} partial-enum={illegal} initialiser-fragments={frag}")
    print(f"merge dumps: {len(rows)} files in the compiled set ({len(listed)} in the debt ledger, "
          f"{len(unlisted)} unlisted)")
    return 1 if unlisted and (args.check or args.report) else 0


if __name__ == "__main__":
    sys.exit(main())
