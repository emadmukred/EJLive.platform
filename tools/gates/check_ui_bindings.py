#!/usr/bin/env python3
"""WinForms event-binding / designer-field resolution checker (compile-gap triage; advisory).

The sandbox has no .NET SDK, so this checks the two failure classes the Wave-5
Designer-partial split (SS-10 / C-28...C-30) actually produced and that no existing
rule covered -- they reached the Windows CI job and turned it red for a whole wave
(C-33):

  * CS0123 -- an event bound to a *bare method group* whose only declaration takes
    zero parameters:

        _startServerMenuItem.Click += StartServer;      // private void StartServer()

    `EventHandler` is `void (object?, EventArgs)`, and C# does not drop parameters
    during method-group conversion, so no overload matches. The legal shapes are a
    discard lambda (`+= (_, _) => StartServer();`) or a two-parameter handler.

  * CS0103 -- an identifier used in one partial of a designer-split type and declared
    in neither partial:

        _bulkExportCsvButton.Click += ...               // designer field is
                                                        // _exportBulkCsvButton

    Word-order drift between the behaviour partial and the designer partial is the
    characteristic mistake of the split, and it is invisible to every other rule.

Neither class is visible to SYN-1 (braces still balance), TYPE-1/TYPE-4 (the files
are owned and unique) or POL-2 (no sync-over-async), which is why they survived the
static gate. This tool closes that gap; SYN-1 remains the authority on syntax.

    python3 tools/gates/check_ui_bindings.py            # human report
    python3 tools/gates/check_ui_bindings.py --quiet    # exit code only

Exit code: 0 = clean, 1 = findings. Advisory by design (like
`check_constant_resolution.py`): it reasons about a subset of C# name resolution,
so a finding is a strong signal and a clean run is not a proof of compilation.
"""
from __future__ import annotations

import os
import re
import sys

ROOT = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
sys.path.insert(0, os.path.join(ROOT, "tools", "inventory"))
import importlib  # noqa: E402

inv = importlib.import_module("ejlive_inventory")

# WinForms events whose delegate is EventHandler / EventHandler<T> with the
# conventional (object?, EventArgs) shape. A bare method group bound to one of these
# must resolve to a two-parameter overload.
EVENT_NAMES = (
    "Click", "DoubleClick", "MouseClick", "MouseDoubleClick", "MouseDown", "MouseUp",
    "MouseEnter", "MouseLeave", "MouseMove", "MouseWheel", "KeyPress", "KeyDown", "KeyUp",
    "TextChanged", "CheckedChanged", "SelectedIndexChanged", "SelectedItemChanged",
    "SelectedCellsChanged", "CurrentCellChanged", "SelectionChanged", "ValueChanged",
    "Scroll", "ScrollStateChanged", "Tick", "Load", "Shown", "FormClosing", "FormClosed",
    "Closed", "Closing", "Opening", "Open", "Paint", "Enter", "Leave", "DragDrop",
    "DragEnter", "DragOver", "DragLeave", "CellClick", "CellDoubleClick",
    "CellContentClick", "CellFormatting", "CellValueChanged", "CellBeginEdit",
    "CellEndEdit", "RowEnter", "RowLeave", "RowPrePaint", "UserDeletingRow",
    "DataError", "ButtonClicked", "ButtonClick", "DropDownClosed", "LinkClicked",
    "SplitterMoved", "AutoSizeChanged", "EnabledChanged", "VisibleChanged",
    "ParentChanged", "TabIndexChanged", "RightToLeftChanged", "Layout",
)

WIRE_RE = re.compile(
    r"\.\s*(?:" + "|".join(EVENT_NAMES) + r")\s*\+=\s*([A-Za-z_][A-Za-z0-9_]*)\s*;"
)

# Members of System.Windows.Forms.Form / Control that are parameterless and are
# therefore CS0123 when used as a bare handler (the classic one is `Close`).
INHERITED_PARAMETERLESS = {
    "Close", "Hide", "Show", "Refresh", "Focus", "Activate", "Select", "Update",
    "Invalidate", "BringToFront", "SendToBack", "PerformLayout", "ResetText",
    "ResetBackColor", "ResetFont", "CreateControl", "DoDragDrop",
}

DECL_RE = re.compile(
    r"(?:private|public|internal|protected)\s+"
    r"(?:readonly\s+|static\s+|const\s+|volatile\s+|async\s+|override\s+|virtual\s+|sealed\s+)*"
    r"[^=;()\r\n]+?\s([A-Za-z_][A-Za-z0-9_]*)\s*\(([^)]*)\)\s*(?:=>|\{)"
)
# Field declarations. Unlike DECL_RE the type portion allows parentheses, because
# tuple-typed fields are legal and common here (`List<(string Label, double Value)>
# _data`, `Dictionary<string, (string PasswordHash, UserRole Role)> _users`); a field
# has no parameter list of its own, and the `=`/`;` terminator keeps the match honest.
FIELD_RE = re.compile(
    r"^\s*(?:private|public|internal|protected)\s+"
    r"(?:readonly\s+|static\s+|const\s+|volatile\s+)*"
    r"[^=;\r\n]+?\s(_[A-Za-z0-9_]+)\s*(?:=[^=]|;)",
    re.M,
)
FIELD_USE_RE = re.compile(r"\b(_[A-Za-z][A-Za-z0-9_]+)\b")
CLASS_RE = re.compile(r"\b(?:partial\s+)?(?:class|record)\s+([A-Za-z_][A-Za-z0-9_]*)")


def strip(text: str) -> str:
    """Remove comments and string/char literals so only code tokens are scanned."""
    text = re.sub(r"/\*.*?\*/", " ", text, flags=re.S)
    text = re.sub(r"//[^\n]*", " ", text)
    prev = None
    while prev != text:
        prev = text
        text = re.sub(
            r'@\"(?:[^\"]|\"\")*\"|\"\"\"[\s\S]*?\"\"\"|\"(?:\\.|[^\"\\])*\"|\'(?:\\.|[^\'\\\n])*\'',
            " ",
            text,
        )
    return text


def param_count(params: str) -> int:
    params = params.strip()
    if not params:
        return 0
    return len([p for p in params.split(",") if p.strip()])


def declarations(text: str) -> dict[str, set[int]]:
    """method/property name -> set of declared parameter counts."""
    out: dict[str, set[int]] = {}
    for name, params in DECL_RE.findall(text):
        out.setdefault(name, set()).add(param_count(params))
    return out


def fields(text: str) -> set[str]:
    return set(FIELD_RE.findall(text))


def scan_type(file_texts: dict[str, str]) -> list[str]:
    """Scan one designer-split type (all partial files merged) for both classes.

    `file_texts` maps repo-relative path -> *stripped* code text. Every partial of
    the type contributes declarations, because C# composes a partial type across all
    of its files in the same assembly.
    """
    findings: list[str] = []
    merged_decls: dict[str, set[int]] = {}
    merged_fields: set[str] = set()
    for text in file_texts.values():
        for name, counts in declarations(text).items():
            merged_decls.setdefault(name, set()).update(counts)
        merged_fields |= fields(text)

    for path, text in file_texts.items():
        for handler in WIRE_RE.findall(text):
            if handler in merged_decls:
                if 2 not in merged_decls[handler]:
                    counts = ",".join(str(c) for c in sorted(merged_decls[handler]))
                    findings.append(
                        f"CS0123 {path}: `+= {handler}` binds a method group whose only "
                        f"declaration takes {counts} parameter(s); EventHandler needs "
                        f"(object?, EventArgs) - write `+= (_, _) => {handler}();`"
                    )
            elif handler in INHERITED_PARAMETERLESS:
                findings.append(
                    f"CS0123 {path}: `+= {handler}` binds the inherited parameterless "
                    f"Form/Control member {handler}(); write `+= (_, _) => {handler}();`"
                )

        used = set(FIELD_USE_RE.findall(text))
        for name in sorted(used - merged_fields):
            if name in merged_decls:
                continue  # a method named with a leading underscore: legal, not a field
            findings.append(
                f"CS0103 {path}: `{name}` is used but declared in no partial of this type"
            )
    return findings


def group_partials(compiled: list[str]) -> dict[tuple[str, str], list[str]]:
    """(directory, declared type name) -> the partial files that declare it."""
    groups: dict[tuple[str, str], list[str]] = {}
    for path in compiled:
        full = os.path.join(ROOT, path)
        if not os.path.isfile(full):
            continue
        text = inv.read(path)
        names = set(CLASS_RE.findall(strip(text)))
        if not names:
            continue
        # a designer-split file declares exactly one type; a merge dump declares many
        # and is already SYN-5's business, so only single-type files are grouped here.
        if len(names) != 1:
            names = {os.path.basename(path).replace(".Designer.cs", "").replace(".cs", "")}
        key = (os.path.dirname(path), next(iter(names)))
        groups.setdefault(key, []).append(path)
    return groups


def main() -> int:
    quiet = "--quiet" in sys.argv
    projects = inv.load_projects()
    compiled = sorted({f for p in projects.values() for f in p.compiled})

    findings: list[str] = []
    scanned = 0
    for _key, paths in sorted(group_partials(compiled).items()):
        texts = {}
        for path in paths:
            texts[path] = strip(inv.read(path))
        scanned += len(paths)
        findings.extend(scan_type(texts))

    findings = sorted(set(findings))
    if not quiet:
        for f in findings:
            print(f)
    print(
        f"ui-binding scan: {len(findings)} issue(s) across {scanned} compiled "
        f"single-type files ({len(compiled)} in the compile maps)"
    )
    return 1 if findings else 0


if __name__ == "__main__":
    raise SystemExit(main())
