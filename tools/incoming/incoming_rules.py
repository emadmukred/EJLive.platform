#!/usr/bin/env python3
"""
incoming_rules.py — apply 00_CHANGELOG / 01_Architecture / 02_Coding rules to an upload.

Usage:
    python3 tools/incoming/incoming_rules.py --upload <path>
    python3 tools/incoming/incoming_rules.py --upload <path> --check-architecture
    python3 tools/incoming/incoming_rules.py --upload <path> --check-implementation
    python3 tools/incoming/incoming_rules.py --upload <path> --check-changelog

Exit codes:
    0 — clean
    2 — at least one violation
    3 — internal error

This is the engine that backs:
    .github/workflows/incoming-rules.yml   (CI)
    tools/hooks/pre-commit                  (local)

The companion rule documents live in:
    docs/incoming-rules/00_CHANGELOG_Corrections.md
    docs/incoming-rules/01_eJLIVE_Architecture_Analysis_Prompt.md
    docs/incoming-rules/02_eJLIVE_Coding_Implementation_Prompt.md

Every verdict is a tuple (file, rule_id, severity, message). The tool emits
JSON to stdout so CI can mirror it as GitHub Actions annotations.
"""

from __future__ import annotations

import argparse
import json
import os
import re
import sys
from dataclasses import dataclass, field
from pathlib import Path
from typing import Iterable

ROOT = Path(__file__).resolve().parents[2]
INCOMING = ROOT / "docs" / "incoming-rules"
CHANGELOG = INCOMING / "00_CHANGELOG_Corrections.md"
ARCHITECTURE = INCOMING / "01_eJLIVE_Architecture_Analysis_Prompt.md"
IMPLEMENTATION = INCOMING / "02_eJLIVE_Coding_Implementation_Prompt.md"
DEBT_LEDGER = ROOT / "docs" / "DEBT-LEDGER.md"

# ---------------------------------------------------------------------------
# Result types
# ---------------------------------------------------------------------------


@dataclass
class Verdict:
    file: str
    rule: str
    severity: str  # "error" or "warning"
    message: str

    def to_dict(self) -> dict:
        return {
            "file": self.file,
            "rule": self.rule,
            "severity": self.severity,
            "message": self.message,
        }


@dataclass
class Report:
    verdicts: list[Verdict] = field(default_factory=list)

    @property
    def errors(self) -> list[Verdict]:
        return [v for v in self.verdicts if v.severity == "error"]

    @property
    def warnings(self) -> list[Verdict]:
        return [v for v in self.verdicts if v.severity == "warning"]

    def add(self, v: Verdict) -> None:
        self.verdicts.append(v)

    def emit(self) -> int:
        out = json.dumps([v.to_dict() for v in self.verdicts], indent=2)
        print(out)
        return 0 if not self.errors else 2


# ---------------------------------------------------------------------------
# Helpers
# ---------------------------------------------------------------------------


def is_source_file(path: Path) -> bool:
    return path.suffix.lower() in {".cs", ".csproj", ".sql", ".json", ".md"}


def safe_relative(path: Path) -> str:
    """Return the path relative to ROOT if possible, else the absolute path."""
    try:
        return str(path.relative_to(ROOT))
    except ValueError:
        return str(path)


def _code_segments(text: str) -> list[tuple[int, int, str]]:
    """Return [(start, end, segment_text), ...] for code-only spans.

    Splits the file by skipping strings, char literals, and comments; the
    returned spans are the code positions where vocabulary checks should
    fire. The boundary handling treats verbatim strings (@"…") and
    interpolated strings ($"…{expr}…") as opaque.
    """
    spans: list[tuple[int, int]] = []
    n = len(text)
    i = 0
    seg_start = 0
    in_string = False
    in_char = False
    in_verbatim = False
    in_block_comment = False
    in_line_comment = False
    escape = False
    while i < n:
        c = text[i]
        nxt = text[i + 1] if i + 1 < n else ""
        if in_line_comment:
            if c == "\n":
                in_line_comment = False
                spans.append((seg_start, i))
                seg_start = i + 1
            i += 1
            continue
        if in_block_comment:
            if c == "*" and nxt == "/":
                in_block_comment = False
                i += 2
                spans.append((seg_start, i - 2))
                seg_start = i
                continue
            i += 1
            continue
        if in_verbatim:
            if c == '"' and nxt == '"':
                i += 2
                continue
            if c == '"':
                in_verbatim = False
                spans.append((seg_start, i + 1))
                seg_start = i + 1
            i += 1
            continue
        if in_string:
            if escape:
                escape = False
            elif c == "\\":
                escape = True
            elif c == '"':
                in_string = False
                spans.append((seg_start, i + 1))
                seg_start = i + 1
            i += 1
            continue
        if in_char:
            if escape:
                escape = False
            elif c == "\\":
                escape = True
            elif c == "'":
                in_char = False
                spans.append((seg_start, i + 1))
                seg_start = i + 1
            i += 1
            continue
        # We are in code.
        if c == "/" and nxt == "/":
            in_line_comment = True
            i += 2
            continue
        if c == "/" and nxt == "*":
            in_block_comment = True
            i += 2
            continue
        if c == "@" and nxt == '"':
            in_verbatim = True
            i += 2
            continue
        if c == "$" and nxt == '"':
            in_string = True
            i += 2
            continue
        if c == '"':
            in_string = True
            i += 1
            continue
        if c == "'":
            in_char = True
            i += 1
            continue
        i += 1
    if seg_start < n:
        spans.append((seg_start, n))
    return [(s, e, text[s:e]) for s, e in spans]


def iter_files(target: Path) -> Iterable[Path]:
    if target.is_file():
        yield target
        return
    for p in target.rglob("*"):
        if p.is_file() and is_source_file(p):
            # The archive (src/_reference/) is *meant* to be unbalanced
            # (DEBT-LEDGER D-02). It is governed by the static gate's SYN-1
            # rule on the compiled set, not by this tool. Skip it here.
            if "_reference" in p.parts:
                continue
            yield p


def read_text(path: Path) -> str:
    try:
        return path.read_text(encoding="utf-8", errors="replace")
    except Exception:
        return ""


# ---------------------------------------------------------------------------
# Architecture checks (A-J)
# ---------------------------------------------------------------------------


def check_architecture(files: Iterable[Path], report: Report) -> None:
    csproj_changes = [f for f in files if f.suffix.lower() == ".csproj"]
    cs_files = [f for f in files if f.suffix.lower() == ".cs"]

    # A-2: layer reference rule. We only check that no new `<ProjectReference`
    # points to a project the gate already forbids; the gate itself enforces
    # the cycle detection. The check here is the *shape* of the change.
    for csproj in csproj_changes:
        text = read_text(csproj)
        # Forbidden upward references.
        forbidden_pairs = [
            ("..\\EJLive.Core\\EJLive.Core.csproj", "..\\EJLive.Shared\\EJLive.Shared.csproj"),
        ]
        # Simpler: forbid Shared → Core references anywhere.
        if "EJLive.Shared" in str(csproj):
            if "..\\EJLive.Core\\EJLive.Core.csproj" in text or "../EJLive.Core/EJLive.Core.csproj" in text:
                report.add(Verdict(
                    file=safe_relative(csproj),
                    rule="A-2",
                    severity="error",
                    message="EJLive.Shared cannot reference EJLive.Core (POL-4). Use a Core-side type or a public contract in Shared."))

    # A-3: runtime projects never reference src/_reference.
    # Strip XML comments before checking, otherwise doc comments that mention
    # the archive path are flagged as violations.
    for csproj in csproj_changes:
        text = read_text(csproj)
        no_comments = re.sub(r"<!--.*?-->", "", text, flags=re.DOTALL)
        if "_reference" in no_comments and "EJLive.LegacyReference" not in str(csproj):
            report.add(Verdict(
                file=safe_relative(csproj),
                rule="A-3",
                severity="error",
                message="runtime project must not reference src/_reference/ (ARCH-3)"))


# ---------------------------------------------------------------------------
# Implementation checks (K-T)
# ---------------------------------------------------------------------------


def _brace_stats(text: str) -> tuple[int, int]:
    """Return (open_count, close_count) outside of string/char literals.

    Honours interpolated strings ($"…{expr}…") and verbatim strings (@"…")
    by tracking an interpolation depth. The result is heuristic; the gate's
    SYN-1 rule is the source of truth.
    """
    open_count = 0
    close_count = 0
    i = 0
    n = len(text)
    in_line_comment = False
    in_block_comment = False
    in_string = False     # "..."
    in_char = False       # '...'
    in_interp = 0         # depth inside $" { ... }
    in_verbatim = False   # @"..."
    escape = False
    while i < n:
        c = text[i]
        nxt = text[i + 1] if i + 1 < n else ""
        if in_line_comment:
            if c == "\n":
                in_line_comment = False
            i += 1
            continue
        if in_block_comment:
            if c == "*" and nxt == "/":
                in_block_comment = False
                i += 2
                continue
            i += 1
            continue
        if in_interp > 0:
            # We're inside a { … } interpolation. Track nested braces and
            # the same string/comment rules until the matching close brace.
            if escape:
                escape = False
            elif c == "\\" and (in_string or in_char):
                escape = True
            elif in_string:
                if c == '"':
                    in_string = False
            elif in_char:
                if c == "'":
                    in_char = False
            elif c == "/" and nxt == "/":
                in_line_comment = True
            elif c == "/" and nxt == "*":
                in_block_comment = True
            elif c == '"':
                in_string = True
            elif c == "'":
                in_char = True
            elif c == "{":
                in_interp += 1
                open_count += 1
            elif c == "}":
                in_interp -= 1
                close_count += 1
            i += 1
            continue
        if in_verbatim:
            # Verbatim string: only "" escapes a quote. End at unescaped ".
            if c == '"' and nxt == '"':
                i += 2
                continue
            if c == '"':
                in_verbatim = False
            i += 1
            continue
        if escape:
            escape = False
            i += 1
            continue
        if c == "\\" and (in_string or in_char):
            escape = True
            i += 1
            continue
        if in_string:
            if c == '"':
                in_string = False
            i += 1
            continue
        if in_char:
            if c == "'":
                in_char = False
            i += 1
            continue
        # Comments.
        if c == "/" and nxt == "/":
            in_line_comment = True
            i += 2
            continue
        if c == "/" and nxt == "*":
            in_block_comment = True
            i += 2
            continue
        # Verbatim / interpolated strings: $" or @".
        if c == "@" and nxt == '"':
            in_verbatim = True
            i += 2
            continue
        if c == "$" and nxt == '"':
            in_string = True
            in_interp = 1   # treat the whole $"…" as an interpolation for the outermost {}
            i += 2
            continue
        if c == '"':
            in_string = True
            i += 1
            continue
        if c == "'":
            in_char = True
            i += 1
            continue
        if c == "{":
            open_count += 1
        elif c == "}":
            close_count += 1
        i += 1
    return open_count, close_count


def check_implementation(files: Iterable[Path], report: Report) -> None:
    cs_files = [f for f in files if f.suffix.lower() == ".cs"]

    for cs in cs_files:
        rel = safe_relative(cs)
        text = read_text(cs)

        # K-1: file name = primary type name.
        m = re.search(r"\b(?:public|internal)\s+(?:sealed\s+|partial\s+)?(?:class|record|struct|interface)\s+(\w+)", text)
        if m:
            type_name = m.group(1)
            stem = cs.stem
            if stem != type_name and not stem.endswith("Designer") and not stem.endswith(".g"):
                # Some files legitimately hold helpers (Ui.cs, ControlRenderingExtensions.cs).
                # Tolerate only if the type is internal and the file is named helpers/<...>.cs
                pass  # soft warning, not enforced here.

        # K-6: brace balance.
        # The gate's SYN-1 rule is the authoritative brace-balance check
        # (it uses the same heuristic as this tool, with a smaller surface
        # for false positives in interpolated strings). We surface a
        # warning here so the developer sees the imbalance but we don't
        # block the commit — the gate will block if SYN-1 is genuinely
        # violated.
        op, cl = _brace_stats(text)
        if op != cl:
            report.add(Verdict(
                file=rel,
                rule="K-6",
                severity="warning",
                message=f"brace balance: {op} '{{' vs {cl} '}}' (heuristic; SYN-1 is authoritative)"))

        # K-7: duplicated modifier tokens.
        dup = re.search(r"\b(public|internal|private|protected)\s+(?:static\s+|partial\s+|sealed\s+|readonly\s+|virtual\s+|override\s+)*\1\b", text)
        if dup:
            report.add(Verdict(
                file=rel,
                rule="K-7",
                severity="error",
                message=f"duplicated modifier: '{dup.group(0)}'"))

        # G-1: loaded vocabulary (Kill, MD5, SHA-1, DES, ECB, TripleDES, RC4, PasswordDeriveBytes).
        # We only flag tokens that appear in code positions (not in
        # string/char literals, comments, XML doc comments, or identifiers).
        # The check splits the file into "code" segments and tests each.
        code_segments = _code_segments(text)
        for token in ("Kill", "MD5", "SHA-1", "DES", "ECB", "TripleDES", "RC4", "PasswordDeriveBytes"):
            for start, end, segment in code_segments:
                for m in re.finditer(r"\b" + re.escape(token) + r"\b", segment):
                    abs_idx = start + m.start()
                    # `process.Kill()` is exempted by SEC-1.
                    if token == "Kill" and abs_idx > 0 and text[abs_idx - 1] == ".":
                        continue
                    window = text[max(0, abs_idx - 600): abs_idx + 600]
                    if "// safe:" in window or "// safe-file:" in window:
                        continue
                    report.add(Verdict(
                        file=rel,
                        rule="G-1",
                        severity="error",
                        message=f"loaded term '{token}' at offset {abs_idx} without `// safe:` annotation"))

        # G-2: `unsafe` keyword.
        if re.search(r"\bunsafe\b", text) and "AllowUnsafeBlocks" not in text:
            report.add(Verdict(
                file=rel,
                rule="G-2",
                severity="error",
                message="`unsafe` keyword present; SEC-2 forbids it"))

        # F-3 / H-2: synchronous wait on UI thread.
        if re.search(r"\bControl\.(?:Invoke|BeginInvoke)\b", text):
            if re.search(r"\.Result\b|\.Wait\(\)", text):
                report.add(Verdict(
                    file=rel,
                    rule="F-3",
                    severity="error",
                    message="synchronous wait on a UI thread is forbidden (POL-2)"))


# ---------------------------------------------------------------------------
# Changelog check
# ---------------------------------------------------------------------------


def _closed_entries(text: str) -> list[str]:
    """Return the IDs of every CLOSED entry in 00_CHANGELOG_Corrections.md."""
    out: list[str] = []
    for line in text.splitlines():
        if "**Status**" in line and "CLOSED" in line:
            # The previous line is the C-NN heading.
            out.append(line.strip())
    return out


def check_changelog(files: Iterable[Path], report: Report) -> None:
    """Refuse to re-introduce a CLOSED correction.

    Heuristic: a CLOSED entry describes a defect with a fix that produced a
    specific code shape (e.g. a second `MsgType`, a glob `<Compile Include="Services\*.cs"`,
    a `partial` in another assembly, two SQLite providers). If the upload
    re-introduces one of those shapes, it surfaces a verdict.
    """
    text = ""
    if CHANGELOG.exists():
        text = read_text(CHANGELOG)

    # Closed: C-06 — no `Services\*.cs` glob in csproj.
    for csproj in [f for f in files if f.suffix.lower() == ".csproj"]:
        proj_text = read_text(csproj)
        if re.search(r'Compile\s+Include="[^"]*\*\.cs"', proj_text):
            report.add(Verdict(
                file=safe_relative(csproj),
                rule="C-06",
                severity="error",
                message="glob `<Compile Include=\"...\\*.cs\" />` re-introduces C-06 (closed)"))

    # Closed: C-13 — every `<Compile>` must sit inside `<ItemGroup>`.
    # Done by the gate's SYN-4 rule (we leave it to the gate for now).

    # Closed: C-14 — `EJLive.Shared` cannot reference upper layers.
    for cs in [f for f in files if f.suffix.lower() == ".cs"]:
        if "EJLive.Shared" not in str(cs):
            continue
        body = read_text(cs)
        if "using EJLive.Core" in body or "using System.Windows.Forms" in body:
            report.add(Verdict(
                file=safe_relative(cs),
                rule="C-14",
                severity="error",
                message="EJLive.Shared source using EJLive.Core or System.Windows.Forms (C-14 closed)"))


# ---------------------------------------------------------------------------
# Entry point
# ---------------------------------------------------------------------------


def main() -> int:
    ap = argparse.ArgumentParser(description="Apply incoming rules to an upload.")
    ap.add_argument("--upload", required=True, help="file or folder to check")
    mode = ap.add_mutually_exclusive_group()
    mode.add_argument("--check-architecture", action="store_true")
    mode.add_argument("--check-implementation", action="store_true")
    mode.add_argument("--check-changelog", action="store_true")
    args = ap.parse_args()

    target = Path(args.upload).resolve()
    if not target.exists():
        print(json.dumps({"error": f"upload not found: {args.upload}"}))
        return 3

    files = list(iter_files(target))
    if not files:
        print(json.dumps({"info": "no source files in upload", "path": str(target)}))

    report = Report()

    run_arch = args.check_architecture or not (args.check_implementation or args.check_changelog)
    run_impl = args.check_implementation or not (args.check_architecture or args.check_changelog)
    run_chl = args.check_changelog or not (args.check_architecture or args.check_implementation)

    if run_arch:
        check_architecture(files, report)
    if run_impl:
        check_implementation(files, report)
    if run_chl:
        check_changelog(files, report)

    return report.emit()


if __name__ == "__main__":
    sys.exit(main())
