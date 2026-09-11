#!/usr/bin/env python3
"""EJLIVE.PLATFORM inventory generator.

Single source of truth for the repository ledgers. Derives every ledger from
the on-disk compile maps of the active solution -- no hand-maintained table is
authoritative. Regenerate on every push (repo rule INV-1); CI runs
`--check` and fails on drift.

    python3 tools/inventory/ejlive_inventory.py          # regenerate
    python3 tools/inventory/ejlive_inventory.py --check  # verify freshness

Outputs (all generated, all committed):
    artifacts/ActiveCompileMap.csv
    artifacts/ProjectDependencyGraph.md
    artifacts/DuplicateTypeReport.md
    artifacts/InventorySummary.json
    docs/inventory/UI-SURFACES.md
    docs/inventory/PROTOCOL.md
    docs/inventory/VENDORS.md
    docs/inventory/PERMISSIONS.md
    docs/inventory/CONFIGURATION.md
    docs/inventory/DATABASE.md
    docs/inventory/TESTS.md
"""
from __future__ import annotations

import csv
import hashlib
import json
import os
import re
import sys
from collections import defaultdict
from xml.etree import ElementTree as ET

MS = "{http://schemas.microsoft.com/developer/msbuild/2003}"
ROOT = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
SRC = os.path.join(ROOT, "src")
REFERENCE_DIR = os.path.join("src", "_reference")

SKIP_DIR_PARTS = {"bin", "obj", ".vs", ".git", "packages", "TestResults", "node_modules"}
DECL_RE = re.compile(
    r"^\s*(?:(?:public|internal|private|protected|static|sealed|abstract|partial|readonly|file|new)\s+)*"
    r"(class|struct|record(?:\s+struct|\s+class)?|interface|enum)\s+([A-Za-z_][A-Za-z0-9_]*)"
)
NS_RE = re.compile(r"^\s*namespace\s+([A-Za-z0-9_.]+)")
GENERATED_MARKERS = ("designer.cs", ".g.cs", ".generated.cs", "assemblyinfo.cs")


def norm(p: str) -> str:
    return p.replace("\\", "/")


def rel(p: str) -> str:
    return norm(os.path.relpath(p, ROOT))


def tracked_paths() -> list[str]:
    import subprocess

    out = subprocess.run(["git", "ls-files"], cwd=ROOT, capture_output=True, text=True, check=True).stdout
    return [norm(x) for x in out.splitlines() if x]


def is_build_output(p: str) -> bool:
    parts = p.split("/")
    return any(x in SKIP_DIR_PARTS for x in parts[:-1]) or p.endswith(
        (".dll", ".pdb", ".cache", ".lscache", ".binlog", ".log", ".exe", ".so", ".dylib", ".a", ".o", ".vsidx", ".up2date")
    )


def read(path: str) -> str:
    try:
        with open(os.path.join(ROOT, path), encoding="utf-8-sig", errors="replace") as fh:
            return fh.read()
    except OSError:
        return ""


def count_lines(text: str) -> int:
    return text.count("\n") + (1 if text and not text.endswith("\n") else 0)


# --------------------------------------------------------------------------
# project model
# --------------------------------------------------------------------------
class Project:
    def __init__(self, name: str, csproj: str):
        self.name = name
        self.csproj = csproj                       # repo-relative
        self.dir = os.path.dirname(csproj)
        self.legacy_shells: list[str] = []
        self.props: dict[str, str] = {}
        self.compiled: list[str] = []               # repo-relative .cs paths
        self.stale: list[str] = []                  # includes that resolve to nothing
        self.refs: list[str] = []                   # ProjectReference names
        self.packages: list[tuple[str, str]] = []
        self.none_items: list[str] = []
        self.linked: list[str] = []                 # Reference (assembly) items

    @property
    def lines(self) -> int:
        return sum(count_lines(read(p)) for p in self.compiled)

    @property
    def identity_issues(self) -> list[str]:
        out = []
        asm = self.props.get("AssemblyName", self.name)
        if asm != self.name:
            out.append(f"AssemblyName={asm}")
        if self.props.get("OutputType") == "Library" and self.props.get("EntryPoint"):
            out.append("Library declares EntryPoint")
        if self.props.get("AllowUnsafeBlocks", "").lower() == "true":
            out.append("AllowUnsafeBlocks=true")
        if self.props.get("UseWPF", "").lower() == "true":
            out.append("UseWPF=true (WinForms-only rule)")
        tfm = self.props.get("TargetFramework", "")
        if tfm and not tfm.startswith("net8.0"):
            out.append(f"TargetFramework={tfm}")
        if "WinForms" in self.name and self.props.get("UseWindowsForms", "").lower() != "true":
            out.append("WinForms project without UseWindowsForms")
        if "WinForms" not in self.name and "Launcher" not in self.name and \
                self.props.get("OutputType") == "WinExe" and self.props.get("UseWindowsForms", "").lower() != "true":
            out.append("WinExe outside a WinForms surface")
        return out


def _prop(root_el, name: str) -> str:
    for el in root_el.iter():
        tag = el.tag.replace(MS, "")
        if tag == name and (el.text or "").strip():
            return el.text.strip()
    return ""


def directory_props() -> dict[str, str]:
    path = os.path.join(ROOT, "Directory.Build.props")
    if not os.path.isfile(path):
        return {}
    try:
        root_el = ET.parse(path).getroot()
    except ET.ParseError:
        return {}
    return {el.tag.replace(MS, ""): (el.text or "").strip()
            for el in root_el.iter() if el.tag.replace(MS, "") in
            ("TargetFramework", "Nullable", "ImplicitUsings", "EnableDefaultCompileItems",
             "EnableDefaultItems", "AllowUnsafeBlocks", "UseWPF", "LangVersion", "GenerateAssemblyInfo")}


def load_projects() -> dict[str, Project]:
    inherited = directory_props()
    projects: dict[str, Project] = {}
    for entry in sorted(os.listdir(SRC)):
        d = os.path.join(SRC, entry)
        if not os.path.isdir(d) or entry.startswith("_"):
            continue
        shells = []
        sdk_shells = []
        for fn in sorted(os.listdir(d)):
            if not fn.endswith(".csproj"):
                continue
            path = os.path.join(d, fn)
            text = read(rel(path))
            try:
                root_el = ET.fromstring(text)
            except ET.ParseError:
                root_el = ET.Element("Project")
            sdk = root_el.tag.replace(MS, "") == "Project" and bool(root_el.get("Sdk"))
            is_ns0 = "ns0:Project" in text or (MS + "Project") in text and not root_el.get("Sdk")
            proj = Project(entry, rel(path))
            for key in ("TargetFramework", "OutputType", "UseWindowsForms", "UseWPF",
                        "EnableDefaultItems", "AssemblyName", "RootNamespace", "Nullable",
                        "AllowUnsafeBlocks", "ImplicitUsings", "LangVersion", "GenerateAssemblyInfo",
                        "EntryPoint"):
                v = _prop(root_el, key)
                if v:
                    proj.props[key] = v
            for k, v in inherited.items():
                proj.props.setdefault(k, v)
            for el in root_el.iter():
                tag = el.tag.replace(MS, "")
                inc = (el.get("Include") or "").strip()
                if tag == "ProjectReference" and inc:
                    target = norm(os.path.relpath(os.path.normpath(os.path.join(d, inc.replace("\\", "/"))), SRC))
                    proj.refs.append(target.split("/")[0])
                elif tag == "PackageReference" and inc:
                    proj.packages.append((inc, el.get("Version") or ""))
                elif tag == "Reference" and inc:
                    proj.linked.append(inc)
                elif tag in ("Compile", "None") and inc:
                    (proj.compiled if tag == "Compile" else proj.none_items).append(inc)
            if sdk and not is_ns0:
                sdk_shells.append(proj)
            else:
                shells.append(proj)
        # non-project dirs with no SDK-style csproj: still expose them as pseudo-projects
        if not sdk_shells and not shells:
            continue
        chosen = None
        if sdk_shells:
            chosen = sdk_shells[0]
            chosen.legacy_shells = [rel(os.path.join(d, s)) for s in shells] if False else [p.csproj for p in shells] + [p.csproj for p in sdk_shells[1:]]
        if chosen is None:                      # legacy-only directory
            chosen = shells[0]
            chosen.legacy_shells = [p.csproj for p in shells[1:]]
        resolve_compile_items(chosen)
        projects[entry] = chosen
    return projects


def resolve_compile_items(proj: Project) -> None:
    raw = [norm(x.replace("\\", "/")) for x in proj.compiled]
    default_on = (proj.props.get("EnableDefaultItems", "true").lower() != "false"
                  and proj.props.get("EnableDefaultCompileItems", "true").lower() != "false")
    explicit = [x for x in raw if "**" not in x]
    globs = [x for x in raw if "**" in x]
    resolved: list[str] = []
    for x in explicit:
        if "*" in x:
            head = x.split("*")[0].rstrip("/").replace("\\", "/")
            base = os.path.normpath(os.path.join(ROOT, proj.dir, head))
            if os.path.isdir(base):
                for fn in sorted(os.listdir(base)):
                    if fn.endswith(".cs"):
                        rp = rel(os.path.join(base, fn))
                        if rp not in resolved:
                            resolved.append(rp)
            else:
                proj.stale.append(norm(os.path.join(proj.dir, x)))
            continue
        p = os.path.normpath(os.path.join(ROOT, proj.dir, x))
        if os.path.isfile(p):
            resolved.append(rel(p))
        else:
            proj.stale.append(norm(os.path.join(proj.dir, x)))
    if default_on or globs:
        for dirpath, dirnames, filenames in os.walk(os.path.join(ROOT, proj.dir)):
            dirnames[:] = [d for d in dirnames if d not in SKIP_DIR_PARTS]
            for fn in filenames:
                if not fn.endswith(".cs"):
                    continue
                rp = rel(os.path.join(dirpath, fn))
                if glob_match(rp, proj.dir, globs) or (default_on and not globs and not explicit):
                    if rp not in resolved:
                        resolved.append(rp)
    # globs that were explicit also honour explicit includes
    resolved.sort()
    proj.compiled = resolved


def glob_match(repo_path: str, proj_dir: str, globs: list[str]) -> bool:
    if not globs:
        return False
    base = norm(proj_dir)
    if not repo_path.startswith(base + "/"):
        return False
    tail = repo_path[len(base) + 1:]
    for g in globs:
        g = norm(g)
        pat = g.split("/")[-1] if "**" not in g else g.split("**/")[-1]
        rx = "^" + re.escape(pat).replace(r"\*\*/", r"(.*\/)?").replace(r"\*\*", ".*").replace(r"\*", "[^/]*").replace(r"\?", ".") + "$"
        target = tail if g.startswith("**") or "/" not in g else tail
        if re.match(rx, target):
            return True
    return False


# --------------------------------------------------------------------------
# declarations
# --------------------------------------------------------------------------
DECL_LINE = re.compile(
    r"^(?P<ind>\s*)(?:(?:public|internal|private|protected|static|sealed|abstract|partial|readonly|file|new|nested)\s+)*"
    r"(?P<kind>class|struct|record(?:\s+struct|\s+class)?|interface|enum)\s+(?P<name>[A-Za-z_][A-Za-z0-9_]*)")


def declarations(path: str) -> list[tuple[str, str, int, bool]]:
    """[(namespace, qualified type, line, is_partial)] -- nested types qualified."""
    text = read(path)
    if not text:
        return []
    out: list[tuple[str, str, int, bool]] = []
    ns = ""
    pending_ns = None
    stack: list[tuple[int, str]] = []
    awaiting: list[tuple[int, str]] = []      # (depth when the declaring line ended, name)
    depth = 0
    for i, line in enumerate(text.splitlines(), 1):
        stripped = line.strip()
        if stripped.startswith("//") or stripped.startswith("*") or stripped.startswith("/*"):
            continue
        m = NS_RE.match(line)
        if m:
            if "{" in line:
                ns = m.group(1)
            else:
                pending_ns = m.group(1)
        elif pending_ns and stripped:
            ns = pending_ns
            pending_ns = None
        d = DECL_LINE.match(line)
        partial = bool(d and re.search(r"\bpartial\b", line))
        if d:
            qual = ".".join(([ns] if ns else []) + [s[1] for s in stack] + [d.group("name")])
            out.append((ns, qual, i, partial))
            awaiting.append((depth, d.group("name")))
        opens, closes = line.count("{"), line.count("}")
        for _ in range(opens):
            depth += 1
            if awaiting and awaiting[0][0] == depth - 1:
                stack.append((depth, awaiting.pop(0)[1]))
        for _ in range(closes):
            depth -= 1
            while stack and stack[-1][0] > depth:
                stack.pop()
        depth = max(depth, 0)
    return out


def type_index(projects: dict[str, Project]) -> dict[str, list[tuple[str, str, int, bool]]]:
    idx: dict[str, list[tuple[str, str, int, bool]]] = defaultdict(list)
    for name, proj in sorted(projects.items()):
        for f in proj.compiled:
            for ns, ty, ln, partial in declarations(f):
                idx[ty].append((name, f, ln, partial))
    return idx


def duplicate_keys(idx: dict) -> tuple[dict, dict]:
    """(local, cross_assembly)

    local          -- >1 owner inside one assembly: CS0101/CS0260, always fatal.
    cross_assembly -- same fully-qualified key owned by two assemblies: a partial
                      split across assemblies; it compiles per-assembly but makes
                      every consumer that references both ambiguous (CS0433).
                      Only tolerated when recorded in docs/DEBT-LEDGER.md.
    """
    local, cross = {}, {}
    for key, occ in idx.items():
        owners = {f for _, f, _, _ in occ}
        if len(owners) < 2:
            continue
        projects = {pr for pr, _, _, _ in occ}
        if all(p for _, _, _, p in occ) and len(projects) > 1:
            cross[key] = sorted({pr for pr, _, _, _ in occ}) + sorted(owners)[:4]
            continue
        if len(projects) == 1:
            local[key] = [(f, ln) for _, f, ln, _ in sorted(occ)]
        else:
            cross[key] = sorted(projects) + sorted(owners)[:4]
    return local, cross


# --------------------------------------------------------------------------
# ledgers
# --------------------------------------------------------------------------
VENDOR_TOKENS = ["NCR", "GRG", "Diebold", "Hyosung", "Wincor", "Hitachi", "Olympia",
                 "Cashway", "Talaris", "Prommtec", "NCRAPTX", "Genmega"]
VENDOR_PAT = re.compile(r"\b(" + "|".join(VENDOR_TOKENS) + r")", re.I)


def build_ledgers(projects: dict[str, Project], tracked: list[str]) -> dict[str, str]:
    """Ledgers are derived from source: compiled files carry map=`active`,
    every other tracked .cs/.xaml/.sql/.config carries map=`reference`."""
    active = {f for p in projects.values() for f in p.compiled}
    scanned: dict[str, str] = {}

    def src_of(f: str) -> str:
        if f not in scanned:
            scanned[f] = read(f)
        return scanned[f]

    def sources(pred=lambda f: True, exts=(".cs",)):
        pool = sorted(f for f in active if f.endswith(exts))
        extra = sorted(f for f in tracked if f.endswith(exts) and f not in active and not is_build_output(f))
        return [f for f in pool + extra if pred(f)]

    def tag(f: str) -> str:
        return "active" if f in active else "reference"

    # ---- UI surfaces -----------------------------------------------------
    rows = []
    for f in sources():
        text = src_of(f)
        if "Form" not in text and "Control" not in text:
            continue
        proj = next((n for n, p in projects.items() if f in p.compiled), "reference")
        for m in re.finditer(r"[ \t]*class[ \t]+(\w*(?:Form|Panel|Control|Console|Dashboard|Studio|Browser|Viewer))(?![\w])", text):
            body = text[m.start():m.start() + 40000]
            controls = len(re.findall(r"new (Button|TextBox|Label|DataGridView|ToolStrip|MenuStrip|TabControl|Panel|ListView|SplitContainer|GroupBox|CheckBox|ComboBox|NumericUpDown|StatusStrip|ToolStripButton|TreeView|ProgressBar|Timer|NotifyIcon|MenuItem|FlowLayoutPanel|TableLayoutPanel)\b", body))
            handlers = len(re.findall(r"\.(Click|DoubleClick|TextChanged|CheckedChanged|SelectedIndexChanged|CellContentClick|FormClosing|Load|KeyDown|MouseDown|MouseDown)\s*[+]?=", body))
            public_api = len(re.findall(r"public (?:async )?(?:Task|void|int|bool|string|\w+) (?:Bind|Update|Refresh|Apply|Show|Load|Save|Open|Run|Start|Stop|Toggle|Export|Navigate)[A-Z]\w*\(", body))
            rows.append((proj, m.group(1), rel(f), controls, handlers, public_api))
    ui = table(["project", "form / control", "file", "controls", "wired handlers", "command methods"],
               sorted(set(rows)),
               note="WinForms-only surface ledger. Every form must bind >=1 handler and expose >=1 command method; "
                    "0-handler rows are orphan UI and are gate violations (UI-1).")

    # ---- protocol --------------------------------------------------------
    msgs, frames, ports = [], [], set()
    for f in sources():
        text = src_of(f)
        if '"' not in text:
            continue
        for m in re.finditer(r'"([A-Z][A-Z0-9_]{2,}(?:_[A-Z0-9_]+)+)"', text):
            lit = m.group(1)
            if re.match(r"^(AU|EJ|CMD|RSP|ACK|NAK|HELLO|AUTH|HEARTBEAT|STATUS|UPLOAD|DOWNLOAD|BROADCAST|EXEC|RESULT|ERR|PING|PONG|JOIN|LEAVE|SESSION|CAPABILITY)", lit):
                msgs.append((lit, tag(f), rel(f)))
        for m in re.finditer(r":\{?0\}?[^\n]{0,40}|MsgType[^\n]{0,60}|:\\n", text):
            if "Length" in m.group(0) or "MsgType" in m.group(0) or "\\n" in m.group(0):
                frames.append((m.group(0).strip()[:90], rel(f)))
        for m in re.finditer(r"(?:DefaultPort|port)[ \t]*[=:][ \t]*(\d{4,5})", text):
            ports.add(m.group(1))
    mrows = sorted({(a, b) for a, b, _ in msgs})
    proto = ("| wire literal | map |\n|---|---|\n" +
             "\n".join(f"| `{a}` | {b} |" for a, b in mrows) +
             f"\n\n{len(mrows)} message literals. Envelope grammar, chunking, signing and resume semantics are normative in "
             f"`docs/EJLIVE-ENGINEERING-PROMPT.md` SS5. Ports observed: {', '.join(sorted(ports)) or 'none'}.\n\n"
             "## frame and header probes\n\n" +
             "\n".join(f"- `{a}` <- `{b}`" for a, b in sorted(set(frames))[:60]))

    # ---- vendors ---------------------------------------------------------
    vrows = []
    for f in sources():
        base = os.path.basename(f)
        m = VENDOR_PAT.search(base)
        if not m:
            head = src_of(f)[:6000]
            m = VENDOR_PAT.search(head) if "pars" in head.lower() else None
        if not m:
            continue
        vendor = m.group(1).upper()
        low = base.lower()
        kind = ("parser" if "parser" in low else "adapter" if "adapter" in low
                else "strategy" if "strategy" in low else "profile" if "profile" in low
                else "frame" if "frame" in low or "record" in low else "auxiliary")
        vrows.append((vendor, kind, tag(f), rel(f)))
    owners = defaultdict(set)
    for v, k, mp, f in vrows:
        if k == "parser" and mp == "active":
            owners[v].add(f)
    body = ["| vendor | artefacts | compiled parser owners | POL-1 status |", "|---|---|---|---|"]
    allv = defaultdict(list)
    for v, k, mp, f in vrows:
        allv[v].append((k, mp, f))
    for v in sorted(allv):
        parsers = sorted(set(f for k, mp, f in allv[v] if k == "parser"))
        compiled = sorted(owners.get(v, []))
        status = "OK" if len(compiled) <= 1 else f"VIOLATION ({len(compiled)} compiled parsers)"
        body.append(f"| {v} | {len(allv[v])} | {len(parsers)} ({', '.join('`'+os.path.basename(x)+'`' for x in parsers[:4])}) | {status} |")
    for v in sorted(allv):
        body.append(f"\n### {v}")
        for k, mp, f in sorted(set(allv[v])):
            body.append(f"- `{k}` [{mp}] `{f}`")
    body.append("\nRule POL-1: one compiled parser per vendor. Additional vendor-specific parsers may exist only as "
                "linked reference (map=`reference`) and must not enter a compile map.")
    vend = "\n".join(body)

    # ---- permissions -----------------------------------------------------
    prows = []
    for f in sources():
        text = src_of(f)
        if "enum " not in text and "Can[A-Z]" not in text and "Permission" not in text:
            continue
        for m in re.finditer(r"enum (Role|EJLiveRole|Permission|PermissionTier|CommandRiskLevel|RiskLevel|AccessLevel|AuthorizationTier|UserRole)\b[^\n]*\{([^}]*)\}", text, re.S):
            members = [x.strip().split("=")[0].strip() for x in m.group(2).split(",")
                       if x.strip().split("=")[0].strip() and not x.strip().startswith("//")]
            prows.append((m.group(1), "; ".join(members[:24]), tag(f), rel(f)))
        for m in re.finditer(r"[ \t]*(?:public|internal|private|static)[^\n;]*bool (Can[A-Z]\w*)\b[^\n]*=>[^\n]+", text):
            line = text[m.start():text.index("\n", m.end())] if "\n" in text[m.end():] else text[m.start():m.end()]
            prows.append(("gate:" + m.group(1), line.strip()[:150], tag(f), rel(f)))
        for m in re.finditer(r"[ \t]*(?:\[[^\]]*(Require|Authorize|Permission)[^\]]*\])[^\n]*", text):
            frag = m.group(0).strip()
            if len(frag) < 150 and ("Risk" in frag or "Require" in frag or "Authoriz" in frag):
                prows.append(("attribute", frag, tag(f), rel(f)))
    perms = table(["enumerable / gate / attribute", "definition", "map", "file"], sorted(set(prows)),
                  note="Authorization is enforced at three chokepoints: intake (message -> risk tier), executor "
                       "(role x risk matrix), UI (control binding mirrors the matrix, never the inverse). See SS9.")

    # ---- configuration ---------------------------------------------------
    crows = []
    for f in sources(lambda x: True, (".cs", ".config", ".json", ".csproj", ".props", ".targets")):
        text = src_of(f)
        if "<add " not in text and '"' not in text and "ReadSetting" not in text:
            continue
        for m in re.finditer(r'<add key="([^"]+)" value="([^"]*)"\s*/>', text):
            crows.append((m.group(1), m.group(2)[:60], tag(f), rel(f)))
        for m in re.finditer(r'ReadSetting\("([A-Za-z0-9_.]+)"[^)]*\)', text):
            crows.append((m.group(1), "(read by code, default at call site)", tag(f), rel(f)))
        for m in re.finditer(r'"(EJLive[A-Za-z0-9_.]+|Max[A-Za-z0-9_]+|[A-Za-z0-9_]+(?:Port|Path|Url|Uri|Timeout|Interval|Retries|ChunkSize|Secret|Thumbprint))" *: *"?([^,\n}"]{0,60})', text):
            crows.append((m.group(1), m.group(2).strip() or "-", tag(f), rel(f)))
    cfg = table(["key", "value / default", "map", "source"], sorted(set(crows)),
                note="Configuration is file-backed (no env-var magic): every key above has a documented default, a "
                     "validator and a redaction class. Secrets never appear here (SEC-3).")

    # ---- database --------------------------------------------------------
    drows = []
    for f in sources(lambda x: True, (".cs", ".sql")):
        text = src_of(f)
        up = text.upper()
        if "CREATE TABLE" not in up and "INSERT INTO" not in up and "CREATE INDEX" not in up:
            continue
        for m in re.finditer(r"CREATE TABLE (?:IF NOT EXISTS )?\[?(?:dbo\]?\.)?\[?([A-Za-z_][A-Za-z0-9_]*)\]?", text, re.I):
            drows.append(("table", m.group(1), tag(f), rel(f)))
        for m in re.finditer(r"CREATE (?:UNIQUE )?INDEX (?:IF NOT EXISTS )?\[?([A-Za-z_][A-Za-z0-9_]*)\]?", text, re.I):
            drows.append(("index", m.group(1), tag(f), rel(f)))
        for m in re.finditer(r"(?:INSERT INTO|UPDATE|DELETE FROM)[ \t]+\[?([A-Za-z_][A-Za-z0-9_]{2,})\]?", text, re.I):
            name = m.group(1)
            if name.upper() not in ("SET", "VALUES", "SELECT", "INTO"):
                drows.append(("dml", name, tag(f), rel(f)))
    agg = defaultdict(lambda: [set(), set()])
    for kind, name, mp, f in drows:
        agg[name][0].add(kind)
        agg[name][1].add(mp)
    db = table(["object", "evidence", "map"],
               sorted((n, ", ".join(sorted(k)), ", ".join(sorted(m))) for n, (k, m) in agg.items()),
               note="Schema is code-owned: a table without a CREATE in `DatabaseSchema.cs`/migration *or* without a "
                    "DML consumer is a defect (DB-1). Migrations are forward-only and numbered `NNNN_Name.sql`.")

    # ---- tests -----------------------------------------------------------
    trows = []
    for name, proj in sorted(projects.items()):
        for f in proj.compiled:
            text = src_of(f)
            if "[TestMethod]" not in text and "[Fact]" not in text and "[Theory]" not in text:
                continue
            cls = re.findall(r"class (\w+)", text)
            n = len(re.findall(r"\[(TestMethod|Fact|Theory)\]", text))
            trows.append((name, cls[0] if cls else os.path.basename(f), n, rel(f)))
    total = sum(r[2] for r in trows)
    tests = table(["project", "class", "cases", "file"], sorted(trows),
                  note=f"{total} executable cases in {len(trows)} files. Acceptance gate: `dotnet test` green AND "
                       f"`EJLive.Verification` probes green (see docs/CI.md).")
    return {"UI-SURFACES": ui, "PROTOCOL": proto, "VENDORS": vend, "PERMISSIONS": perms,
            "CONFIGURATION": cfg, "DATABASE": db, "TESTS": tests}


def table(headers: list[str], rows: list[tuple], note: str = "") -> str:
    head = "| " + " | ".join(headers) + " |"
    sep = "|" + "|".join("---" for _ in headers) + "|"
    body = [head, sep] + ["| " + " | ".join(str(c) for c in r) + " |" for r in rows]
    if note:
        body += ["", note]
    return "\n".join(body)


# --------------------------------------------------------------------------
# emitters
# --------------------------------------------------------------------------
def render(projects: dict[str, Project], tracked: list[str], ledgers: dict[str, str]) -> dict[str, str]:
    """Return {repo-relative path: content} for every generated artefact."""
    idx = type_index(projects)
    dupes, cross_assembly = duplicate_keys(idx)
    partial_splits = {k: v for k, v in idx.items()
                      if len({f for _, f, _, _ in v}) > 1 and all(x[3] for x in v) and len({x[1] for x in v}) > 1}
    out: dict[str, str] = {}

    rows = []
    for name, proj in sorted(projects.items()):
        for f in proj.compiled:
            text = read(f)
            rows.append([name, f, count_lines(text), hashlib.md5(text.encode("utf-8", "replace")).hexdigest(), "compiled"])
    compiled_all = {f for p in projects.values() for f in p.compiled}
    for f in tracked:
        if f in compiled_all or is_build_output(f) or not f.endswith(".cs"):
            continue
        if f.startswith(REFERENCE_DIR + "/"):
            rows.append(["(reference)", f, count_lines(read(f)), "", "linked-reference"])
    out["artifacts/ActiveCompileMap.csv"] = "project,file,lines,md5,status\n" + "".join(
        ",".join(str(c) for c in r) + "\n" for r in rows)

    order, cycles = topo(projects)
    g = ["# Project dependency graph", "",
         "Build order (topological; `dotnet build -m:1` serialises on it):", ""]
    for i, n in enumerate(order, 1):
        pr = projects[n]
        deps = ", ".join(f"`{d}`" for d in sorted(pr.refs)) or "_none_"
        g.append(f"{i}. **{n}** -> {deps}")
    g += ["", "Layers: " + " -> ".join(layers(projects)), "",
          f"Cycles detected: {cycles if cycles else 'none (acyclic OK)'}", "",
          "## Project identity", "",
          "| project | tfm | output | forms | files | lines | identity issues |", "|---|---|---|---|---|---|---|"]
    for n, pr in sorted(projects.items()):
        g.append(f"| {n} | {pr.props.get('TargetFramework','-')} | {pr.props.get('OutputType','-')} | "
                 f"{pr.props.get('UseWindowsForms','-')} | {len(pr.compiled)} | {pr.lines} | "
                 f"{'; '.join(pr.identity_issues) or 'ok'} |")
    shells = [(s, pr.csproj, n) for n, pr in sorted(projects.items()) for s in pr.legacy_shells]
    g += ["", f"## Demoted csproj shells -- not built, kept for audit ({len(shells)})", ""]
    for s, keep, n in shells:
        g.append(f"- `{s}` superseded by `{keep}` ({n})")
    owned = {f for p in projects.values() for f in os.walk and []}
    dirs = [p.dir for p in projects.values()]
    orphans = sorted({f for f in tracked if f.startswith("src/") and f.endswith(".cs") and not is_build_output(f)
                      and not f.startswith(REFERENCE_DIR + "/")
                      and not any(f.startswith(d + "/") for d in dirs)})
    g += ["", f"## Unowned .cs under src/ ({len(orphans)})", ""]
    g += [f"- `{f}`" for f in orphans]
    out["artifacts/ProjectDependencyGraph.md"] = "\n".join(g) + "\n"

    d = ["# Duplicate type declarations in the active compile set", "",
         "## Intra-assembly duplicates (CS0101 / CS0260 -- fatal, must be zero)", ""]
    d.append("none -- rule ARCH-4 holds" if not dupes else f"{len(dupes)} keys need exactly one compiled owner; "
             "demote or delete the redundant owner, never rename it:")
    if dupes:
        d.append("")
        for k, v in sorted(dupes.items()):
            d.append(f"- `{k}`: " + ", ".join(f"`{f}:{ln}`" for f, ln in sorted(v)))
    d += ["", f"## Cross-assembly partial splits (CS0433 for dual consumers -- tracked debt)", "",
          f"{len(cross_assembly)} keys. Each must appear in `docs/DEBT-LEDGER.md` or the gate fails (ARCH-5).", ""]
    for k, v in sorted(cross_assembly.items()):
        d.append(f"- `{k}`: " + ", ".join(f"`{x}`" for x in v))
    if dupes:
        if partial_splits:
            d += ["", f"Legal partial splits (not defects): {len(partial_splits)} --", ""]
            for k, v in sorted(partial_splits.items()):
                d.append(f"- `{k}`: " + ", ".join(f"`{os.path.basename(f)}:{ln}`" for _, f, ln, _ in sorted(v)))
    out["artifacts/DuplicateTypeReport.md"] = "\n".join(d) + "\n"

    ref_root = os.path.join(ROOT, REFERENCE_DIR)
    ref_files = sorted(rel(os.path.join(dp, f)) for dp, _dn, fns in os.walk(ref_root) for f in fns
                       if f.endswith(".cs")) if os.path.isdir(ref_root) else []
    unmapped = len(orphans)
    summary = {
        "projects": len(projects),
        "compiled_files": sum(len(p.compiled) for p in projects.values()),
        "compiled_lines": sum(p.lines for p in projects.values()),
        "reference_files": len(ref_files),
        "reference_lines": sum(count_lines(read(f)) for f in ref_files),
        "stale_includes": sum(len(p.stale) for p in projects.values()),
        "unmapped_source_files": unmapped,
        "duplicate_type_keys": len(dupes),
        "cross_assembly_partial_splits": len(cross_assembly),
        "cross_assembly_keys": sorted(cross_assembly),
        "legal_partial_splits": len(partial_splits),
        "duplicate_type_list": sorted(dupes),
        "cycles": cycles,
        "build_order": order,
        "ledger_rows": {k: v.count("\n") for k, v in ledgers.items()},
        "identity_issues": {n: p.identity_issues for n, p in sorted(projects.items()) if p.identity_issues},
        "stale_include_paths": {n: p.stale for n, p in sorted(projects.items()) if p.stale},
    }
    out["artifacts/InventorySummary.json"] = json.dumps(summary, indent=1, sort_keys=True) + "\n"
    for name, body in ledgers.items():
        out[f"docs/inventory/{name}.md"] = (
            f"# {name} ledger (generated -- do not hand-edit)\n\n"
            "Source: `tools/inventory/ejlive_inventory.py`. Regenerate with "
            "`python3 tools/inventory/ejlive_inventory.py`.\n\n" + body + "\n")
    return out


def main() -> int:
    check = "--check" in sys.argv
    tracked = tracked_paths()
    projects = load_projects()
    ledgers = build_ledgers(projects, tracked)
    files = render(projects, tracked, ledgers)
    if check:
        drift = []
        for path, content in sorted(files.items()):
            p = os.path.join(ROOT, path)
            cur = open(p, encoding="utf-8").read() if os.path.isfile(p) else None
            if cur != content:
                drift.append(path)
        if drift:
            print("ledger drift (regenerate: python3 tools/inventory/ejlive_inventory.py):")
            for x in drift:
                print(" -", x)
            return 1
        print(f"ledgers fresh ({len(files)} generated files)")
        return 0
    for path, content in files.items():
        p = os.path.join(ROOT, path)
        os.makedirs(os.path.dirname(p), exist_ok=True)
        with open(p, "w", encoding="utf-8", newline="\n") as fh:
            fh.write(content)
    s = json.loads(files["artifacts/InventorySummary.json"])
    print(json.dumps({k: v for k, v in s.items() if k not in ("stale_include_paths", "duplicate_type_list")},
                     indent=1, sort_keys=True))
    if s["duplicate_type_keys"]:
        print("first 20 duplicate keys:", s["duplicate_type_list"][:20])
    return 0


def topo(projects: dict[str, Project]) -> tuple[list[str], list[str]]:
    indeg = {n: 0 for n in projects}
    edges = defaultdict(set)
    for n, p in projects.items():
        for d in p.refs:
            if d in projects:
                edges[d].add(n)
                indeg[n] += 1
    ready = sorted(n for n, d in indeg.items() if d == 0)
    order = []
    while ready:
        n = ready.pop(0)
        order.append(n)
        for m in sorted(edges[n]):
            indeg[m] -= 1
            if indeg[m] == 0:
                ready.append(m)
                ready.sort()
    cycles = [] if len(order) == len(projects) else [f"unresolved: {sorted(set(projects) - set(order))}"]
    return order, cycles


def layers(projects: dict[str, Project]) -> list[str]:
    depth = {}
    def d(n):
        if n in depth:
            return depth[n]
        deps = [x for x in projects[n].refs if x in projects]
        depth[n] = 1 + max([d(x) for x in deps], default=-1)
        return depth[n]
    for n in projects:
        d(n)
    bucket = defaultdict(list)
    for n, v in depth.items():
        bucket[v].append(n)
    return [f"L{k}:{','.join(sorted(v))}" for k, v in sorted(bucket.items())]


if __name__ == "__main__":
    sys.exit(main())
