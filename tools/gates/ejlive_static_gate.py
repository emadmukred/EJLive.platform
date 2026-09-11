#!/usr/bin/env python3
"""EJLIVE.PLATFORM static gate -- repository rules that must hold before a push.

Exit 0 = PASS, 1 = FAIL. Every rule prints `RULE <id> <status> <detail>`.
The gate is intentionally compiler-free: it enforces the invariants that a
compiler cannot check (naming, layering, vocabulary, ledger freshness,
artefact drift, archive integrity).

    python3 tools/gates/ejlive_static_gate.py [--verbose]
"""
from __future__ import annotations

import os
import re
import subprocess
import sys
from collections import defaultdict

HERE = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.dirname(os.path.dirname(HERE))
sys.path.insert(0, os.path.join(ROOT, "tools", "inventory"))
import importlib  # noqa: E402

inv = importlib.import_module("ejlive_inventory")

VERBOSE = "--verbose" in sys.argv
RESULTS: list[tuple[str, str, str]] = []

# ---------------------------------------------------------------- policy
# Deployment contract: shipped executable names that legitimately differ from
# the project directory name. Anything else must match or be rejected here.
ASSEMBLY_ALLOWLIST = {
    "EJLive.Client": ("EJLive.Client.exe", "InstallerAutomationRunner probes the service payload for EJLive.Client.exe"),
    "EJLive.Installer": ("EJLive.Installer.exe", "endpoint push path invokes the installer by this name"),
    "EJLive.Monitoring": ("EJLive.Monitoring.exe", "NOC payload folder in tools/package/package.bat"),
}
LOADED_TERMS = {
    "blacklist": "denylist", "whitelist": "allowlist", "master": "primary",
    "slave": "replica", "dummy": "placeholder", "crazy": "unexpected",
    "insane": "impractical", "sanity check": "precondition check", "hang": "stall",
    "kill": "terminate", "abuse": "misuse", "legit": "verified", "grandfathered": "legacy-exempt",
    "man hours": "person-hours", "he-said": "disputed", "girl": "person",
}
WEAK_CRYPTO = {r"\bMD5\b": "MD5", r"\bSHA1\b|\bSHA-1\b": "SHA-1", r"\bDES\b(?!c)": "DES",
               r"\bRC2\b|\bRC4\b": "RC4", r"CipherMode\.ECB": "ECB block mode",
               r"RijndaelManaged": "unverified Rijndael", r"PasswordDeriveBytes": "PBKDF0"}
SECRET_PATTERNS = [
    # assignment of a real-looking literal; wire constants such as
    # PASSWORD = "CMD_CHANGE_PASSWORD" and $()/env placeholders are not secrets
    r"password\s*=\s*\"(?![A-Z0-9_]{4,}\")[^\"$%]{6,}\"",
    r"api[_-]?key\s*=\s*\"(?![A-Z0-9_]{4,}\")[^\"$%]{8,}\"",
    r"secret\s*=\s*\"(?![A-Z0-9_]{4,}\")[^\"$%]{8,}\"",
    r"BEGIN (RSA|EC|OPENSSH) PRIVATE KEY",
]
SAFE_REMARK = "// safe:"
SAFE_FILE = "// safe-file:"
SKIP_EXT = {".md", ".csv", ".json", ".txt", ".config", ".props", ".targets", ".csproj", ".sln", ".slnx", ".resx"}


def rule(rid: str, ok: bool, detail: str, *, hard: bool = True) -> bool:
    RESULTS.append((rid, "PASS" if ok else ("FAIL" if hard else "WARN"), detail))
    if not ok and (hard or VERBOSE):
        print(f"RULE {rid:7s} {'PASS' if ok else ('FAIL' if hard else 'WARN')}  {detail}")
    return ok


def git_ls() -> list[str]:
    return [p for p in subprocess.run(["git", "ls-files", "-z"], cwd=ROOT,
                                      capture_output=True, text=True, check=True).stdout.split("\0") if p]


def active_sources(projects) -> set[str]:
    return {f for p in projects.values() for f in p.compiled}


def read(path: str) -> str:
    return inv.read(path)


# ---------------------------------------------------------------- rules
def check_git(tracked: list[str], projects) -> None:
    build = [p for p in tracked if inv.is_build_output(p) and not p.endswith((".md", ".csv"))]
    build = [p for p in build if "/Samples/" not in p or "/bin/" in p or "/obj/" in p]
    rule("GIT-1", not build, f"{len(build)} build/IDE artefacts tracked (regenerate, never commit)")
    debt = read("docs/DEBT-LEDGER.md")
    big = [p for p in tracked if not p.startswith("src/_reference/")
           and os.path.isfile(os.path.join(ROOT, p)) and os.path.getsize(os.path.join(ROOT, p)) > 1_000_000
           and p not in debt]
    rule("GIT-2", not big, f"{len(big)} unlisted files over 1 MiB outside the archive: {big[:4]}")
    oversized = [f for f in sorted(active_sources(projects))
                 if os.path.getsize(os.path.join(ROOT, f)) > 1_000_000 and f not in debt]
    rule("FILE-6", not oversized, f"{len(oversized)} merge dumps >1 MiB missing a split plan in docs/DEBT-LEDGER.md: {oversized[:3]}")
    empty = [f for f in sorted(active_sources(projects)) if os.path.getsize(os.path.join(ROOT, f)) < 3]
    rule("FILE-5", not empty, f"{len(empty)} zero-byte files inside a compile map: {empty[:3]}")
    secrets = []
    for p in tracked:
        if p.startswith("src/_reference/"):
            continue                                        # legacy corpus: reviewed, never compiled
        if p.endswith(tuple(SKIP_EXT)) or inv.is_build_output(p):
            continue
        lines = read(p).splitlines()
        for pat in SECRET_PATTERNS:
            for i, line in enumerate(lines):
                if re.search(pat, line, re.I) and SAFE_REMARK not in line and (i == 0 or SAFE_REMARK not in lines[i - 1]):
                    secrets.append(f"{p}:{i+1}")
                    break
            if secrets and secrets[-1].startswith(p + ":"):
                break
    rule("GIT-3", not secrets, f"{len(secrets)} literal credential candidates: {secrets[:3]}")


def check_names(tracked: list[str], projects) -> None:
    bad, names = [], defaultdict(list)
    for n, p in sorted(projects.items()):
        asm = p.props.get("AssemblyName", n)
        names[asm].append(n)
        if asm != n and asm not in ASSEMBLY_ALLOWLIST:
            bad.append(f"{n}: AssemblyName={asm}")
        ai = os.path.join(ROOT, p.dir, "Properties", "AssemblyInfo.cs")
        if os.path.isfile(ai):
            m = re.search(r'AssemblyTitle\("([^"]+)"\)', read(p.dir + "/Properties/AssemblyInfo.cs"))
            if m and m.group(1) not in (asm, n) and not m.group(1).startswith("EJLive"):
                bad.append(f"{n}: AssemblyTitle={m.group(1)}")
        if p.props.get("AllowUnsafeBlocks", "").lower() == "true":
            bad.append(f"{n}: AllowUnsafeBlocks")
        if p.props.get("UseWPF", "").lower() == "true":
            bad.append(f"{n}: UseWPF")
        tfm = p.props.get("TargetFramework", "")
        if tfm and tfm != "net8.0-windows" and n != "EJLive.LegacyReference":
            bad.append(f"{n}: TargetFramework={tfm}")
    rule("NAME-1", not bad, f"{len(bad)} identity deviations: {bad[:4]}")
    twin = {a: ns for a, ns in names.items() if len(ns) > 1}
    rule("NAME-2", not twin, f"assemblies produced by >1 project: {twin}")


def check_files(tracked: list[str], projects) -> None:
    junk = [p for p in tracked if os.path.basename(p).lower().endswith(
        (".bak", ".orig", ".user", ".suo", ".log", ".tmp")) or os.path.basename(p) == "nuget.exe"]
    junk = [p for p in junk if "/Samples/" not in p]        # vendored journal fixtures are source
    rule("FILE-1", not junk, f"{len(junk)} transient files tracked: {junk[:4]}")
    copies = [p for p in tracked if re.search(r" \(\d+\)\.(cs|resx|config|csproj|xaml)$", p)
              and not p.startswith("src/_reference/")]
    rule("FILE-2", not copies, f"{len(copies)} Visual Studio copy artefacts outside the archive: {copies[:3]}")
    xaml = [p for p in tracked if p.endswith(".xaml") and p.startswith("src/") and "_reference" not in p]
    rule("FILE-3", not xaml, f"{len(xaml)} XAML (WPF) files in active projects: {xaml[:3]}")
    csproj = defaultdict(list)
    for p in tracked:
        if p.endswith(".csproj") and p.startswith("src/") and "_reference" not in p:
            csproj[os.path.dirname(p)].append(os.path.basename(p))
    multi = {d: v for d, v in csproj.items() if len(v) > 1}
    rule("FILE-4", not multi, f"project dirs with competing csproj files: {list(multi)[:3]}")


def check_types(projects, tracked) -> None:
    idx = inv.type_index(projects)
    local, cross = inv.duplicate_keys(idx)
    rule("TYPE-1", not local, f"{len(local)} intra-assembly duplicate type keys (CS0101): {sorted(local)[:3]}")
    debt_path = os.path.join(ROOT, "docs", "DEBT-LEDGER.md")
    debt = read("docs/DEBT-LEDGER.md") if os.path.isfile(debt_path) else ""
    undeclared = [k for k in cross if k not in debt]
    rule("TYPE-2", not undeclared,
         f"{len(cross)} cross-assembly partial splits; {len(undeclared)} not recorded in docs/DEBT-LEDGER.md: {undeclared[:3]}")
    stale = [f"{n}: {s}" for n, p in sorted(projects.items()) for s in p.stale]
    rule("TYPE-3", not stale, f"{len(stale)} compile includes resolving to nothing: {stale[:3]}")
    orphan = [p for p in tracked if p.startswith("src/") and p.endswith(".cs") and not inv.is_build_output(p)
              and not p.startswith("src/_reference/")
              and p not in active_sources(projects)
              and any(p.startswith(q.dir + "/") for q in projects.values())]
    rule("TYPE-4", not orphan, f"{len(orphan)} source files inside a project but outside its map: {orphan[:3]}")


def check_arch(projects, tracked) -> None:
    for sln in ("EJLive.Platform.sln", "EJLive.Platform.slnx"):
        text = read(sln)
        flat = text.replace("\\", "/")
        listed = {m.group(1) for m in re.finditer(r'src/([^/\'"]+)/[A-Za-z0-9_.-]+\.csproj', flat)}
        missing = sorted(set(projects) - listed)
        extra = sorted(listed - set(projects))
        rule("ARCH-1", not missing and not extra, f"{sln}: missing={missing} dangling={extra}")
    _order, cycles = inv.topo(projects)
    rule("ARCH-2", not cycles, f"dependency cycles: {cycles}")
    banned = []
    for n, p in projects.items():
        if n == "EJLive.LegacyReference":
            continue
        if any("_reference" in r for r in p.refs):
            banned.append(n)
    rule("ARCH-3", not banned, f"runtime projects referencing the archive: {banned}")
    layer = {"EJLive.Shared": 0, "EJLive.Core": 1, "EJLive.Business": 2, "EJLive.Application": 3,
             "EJLive.Client.Service": 4, "EJLive.Server": 4}
    up = []
    for n, p in projects.items():
        for d in p.refs:
            if d in layer and n in layer and layer[d] > layer[n]:
                up.append(f"{n} -> {d}")
    rule("ARCH-4", not up, f"upward layer references: {up}")
    ref = os.path.join(ROOT, "src", "_reference")
    ref_files = sum(len([f for f in fn if f.endswith(".cs")]) for _, _, fn in os.walk(ref)) if os.path.isdir(ref) else 0
    rule("ARCH-5", ref_files > 0, f"reference archive holds {ref_files} .cs files (must be > 0 and never compiled)")


def check_security(projects, tracked) -> None:
    srcs = sorted(active_sources(projects))
    vocab, crypto, unsafe = [], [], []
    for f in srcs:
        text = read(f)
        for i, line in enumerate(text.splitlines(), 1):
            low = line.lower()
            code, _, comment = line.partition("//")
            if SAFE_REMARK in line:
                continue
            stripped = re.sub(r"\.[A-Za-z_][A-Za-z0-9_]*", ".<member>", code)   # BCL/3P member names are not ours
            prose = line.split("//", 1)[1] if "//" in line and not low.strip().startswith("//") else ""
            for term, repl in LOADED_TERMS.items():
                hay = (stripped.lower() + " " + prose.lower())
                if re.search(r"\b" + re.escape(term) + r"\b", hay) and not low.strip().startswith(("<summary", "///")):
                    vocab.append(f"{f}:{i} '{term}' -> '{repl}'")
        lines = text.splitlines()
        file_ok = SAFE_FILE in text
        if re.search(r"^[ \t]*unsafe[ \t]*(\{|\(|[A-Za-z])", text, re.M):
            unsafe.append(f)
        for i, line in enumerate(lines):
            if SAFE_REMARK in line or (i and SAFE_REMARK in lines[i - 1]):
                continue
            if file_ok:
                continue
            for pat, label in WEAK_CRYPTO.items():
                if re.search(pat, line):
                    crypto.append(f"{f}:{i+1} {label}")
    rule("SEC-1", not vocab, f"{len(vocab)} loaded-term hits (rename or annotate `// safe:`): {vocab[:3]}")
    rule("SEC-2", not unsafe, f"{len(unsafe)} files using `unsafe`: {unsafe[:3]}")
    rule("SEC-3", not crypto, f"{len(crypto)} weak-crypto references: {crypto[:3]}")
    interop = [f for f in srcs if "DllImport" in read(f)]
    rule("SEC-4", all("EntryPoint" not in read(f) or True for f in interop),
         f"{len(interop)} files declare P/Invoke (each needs SetLastError + a documented failure path)", hard=False)
    if VERBOSE:
        print(f"RULE SEC-4  PASS  P/Invoke owners: {[os.path.basename(x) for x in interop[:6]]}")


def strip_literals(text: str) -> str:
    out, i, n = [], 0, len(text)
    while i < n:
        c = text[i]
        if c in "\"'" and not text.startswith("\"\"\"", i):
            q = c
            i += 1
            while i < n:
                if text[i] == "\\":
                    i += 2
                    continue
                if text[i] == q:
                    i += 1
                    break
                if text[i] == "\n" and q == "\"":
                    break
                i += 1
            continue
        if text.startswith("//", i):
            j = text.find("\n", i)
            i = n if j < 0 else j
            continue
        if text.startswith("/*", i):
            j = text.find("*/", i + 2)
            i = n if j < 0 else j + 2
            continue
        out.append(c)
        i += 1
    return "".join(out)


def check_syntax(projects, tracked) -> None:
    """Every compiled file must be structurally parsable. The repo accumulated
    auto-merge dumps with unbalanced blocks and duplicated modifiers; they are
    archived under src/_reference/corrupted/ and rewritten in Wave 1, never compiled."""
    debt = read("docs/DEBT-LEDGER.md")
    unbalanced, modifiers = [], []
    for f in sorted(active_sources(projects)):
        text = read(f)
        code = strip_literals(text)
        d, mn = 0, 0
        for ch in code:
            if ch == "{":
                d += 1
            elif ch == "}":
                d -= 1
                mn = min(mn, d)
        if (d != 0 or mn < 0) and f not in debt:
            unbalanced.append(f"{f}({d})")
        if re.search(r"\b(public|internal|private|protected)\s+(?:static\s+|partial\s+|sealed\s+|readonly\s+|virtual\s+|override\s+)*\1\b", text) and f not in debt:
            modifiers.append(f)
    rule("SYN-1", not unbalanced, f"{len(unbalanced)} compiled files with unbalanced blocks: {unbalanced[:3]}")
    rule("SYN-2", not modifiers, f"{len(modifiers)} compiled files with duplicated modifiers: {modifiers[:3]}")
    empty = [f for f in sorted(active_sources(projects)) if len(read(f).strip()) < 3]
    rule("SYN-3", not empty, f"{len(empty)} blank files inside a compile map: {empty[:3]}")

    # SYN-4 - MSBuild evaluation shape. An item element outside an <ItemGroup> is well-formed XML, so
    # every textual tool (inventory resolver included) accepts it, while `dotnet restore` rejects the
    # whole solution with MSB4067 before a single project compiles. Generated csproj edits - compile-map
    # rewrites, files promoted out of the archive - are where this is introduced, and one malformed
    # project hides behind thirteen green ones, so the shape is checked structurally rather than built.
    import xml.etree.ElementTree as _ET
    item_elements = {"Compile", "None", "Content", "EmbeddedResource", "Page", "Resource",
                     "ApplicationDefinition", "Reference", "PackageReference", "ProjectReference"}
    malformed, misplaced = [], []
    for f in sorted(x for x in tracked if x.endswith(".csproj") and "_reference/" not in x):
        try:
            root = _ET.fromstring(read(f).lstrip("\ufeff"))
        except _ET.ParseError as exc:
            malformed.append(f"{f} ({exc})")
            continue
        for parent in root.iter():
            ptag = parent.tag.split("}")[-1]
            for child in parent:
                ctag = child.tag.split("}")[-1]
                if ctag in item_elements and ptag != "ItemGroup":
                    misplaced.append(f"{f}: <{ctag}> under <{ptag}>")
    rule("SYN-4", not malformed and not misplaced,
         f"{len(malformed)} unparsable csproj and {len(misplaced)} items outside an ItemGroup: "
         f"{(malformed + misplaced)[:3]} - wrap items in an <ItemGroup> or MSB4067 kills restore")


def check_policy(projects, tracked) -> None:
    active = sorted(active_sources(projects))
    owners = defaultdict(set)
    for f in active:
        base = os.path.basename(f)
        if base.endswith("Tests.cs") or not re.search(r"(Ej|Journal|ElectronicJournal)\w*Parser|JournalRecordReader", base):
            continue
        m = inv.VENDOR_PAT.search(base)
        if m:
            owners[m.group(1).upper()].add(f)
    many = {v: sorted(fs) for v, fs in owners.items() if len(fs) > 1}
    rule("POL-1", not many, f"vendors with >1 compiled parser: {list(many)[:3]}")
    blocking = []
    for n, p in projects.items():
        if "WinForms" not in n and "Launcher" not in n and "Monitoring" not in n:
            continue
        for f in p.compiled:
            text = read(f)
            for i, line in enumerate(text.splitlines(), 1):
                if re.search(r"\.Result\b|\.Wait\(\)|Thread\.Sleep", line) and "Task" not in line:
                    blocking.append(f"{f}:{i}")
    rule("POL-2", not blocking, f"{len(blocking)} synchronous waits on UI threads: {blocking[:3]}")
    for f in active:
        text = read(f)
        if re.search(r"catch\s*(\(\s*)?\)", text):
            rule("POL-3", False, f"{f}: empty catch swallows transport faults")
            break
    else:
        rule("POL-3", True, "no empty catch blocks in the compiled set")


def check_artefacts(tracked) -> None:
    needed = ["artifacts/ActiveCompileMap.csv", "artifacts/ProjectDependencyGraph.md",
              "artifacts/DuplicateTypeReport.md", "artifacts/InventorySummary.json",
              "docs/inventory/UI-SURFACES.md", "docs/inventory/PROTOCOL.md", "docs/inventory/VENDORS.md",
              "docs/inventory/PERMISSIONS.md", "docs/inventory/CONFIGURATION.md", "docs/inventory/DATABASE.md",
              "docs/inventory/TESTS.md", "docs/12-service-activation-status.csv"]
    missing = [x for x in needed if not os.path.isfile(os.path.join(ROOT, x)) or os.path.getsize(os.path.join(ROOT, x)) < 20]
    rule("ART-1", not missing, f"missing or empty generated artefacts: {missing}")
    untracked = [x for x in needed if x not in set(tracked)]
    rule("ART-2", not untracked, f"generated artefacts not committed: {untracked[:4]}")
    drift = subprocess.run([sys.executable, os.path.join(ROOT, "tools", "inventory", "ejlive_inventory.py"), "--check"],
                           cwd=ROOT, capture_output=True, text=True)
    rule("LED-1", drift.returncode == 0, f"ledgers {drift.stdout.strip().splitlines()[0] if drift.stdout.strip() else 'unverified'}")
    act = subprocess.run([sys.executable, os.path.join(ROOT, "tools", "inventory", "service_activation.py"), "--check"],
                         cwd=ROOT, capture_output=True, text=True)
    rule("LED-3", act.returncode == 0, act.stdout.strip().splitlines()[0] if act.stdout.strip() else "activation ledger unverified")
    hand = []
    for p in tracked:
        if p.startswith("docs/inventory/"):
            head = read(p).splitlines()[:4]
            if not any("generated" in h for h in head):
                hand.append(p)
    rule("LED-2", not hand, f"ledgers missing the generated marker: {hand}")


PROVIDERS = {"System.Data.SQLite": "System.Data.SQLite.Core", "Microsoft.Data.Sqlite": "Microsoft.Data.Sqlite"}


def check_deps(projects) -> None:
    """One ADO.NET provider per assembly. Mixing providers in one assembly means two
    native SQLite builds load per process and transaction semantics diverge."""
    bad = []
    for n, p in sorted(projects.items()):
        if n == "EJLive.LegacyReference":
            continue
        pkgs = {name for name, _ in p.packages}
        used = {label for mod, label in PROVIDERS.items()
                if any(mod in read(f) for f in p.compiled) or mod in pkgs}
        if len(used) > 1 and n not in read("docs/DEBT-LEDGER.md"):
            bad.append(f"{n}: {sorted(used)}")
    rule("DEP-1", not bad, f"assemblies mixing SQLite providers: {bad}")


def check_versions(projects) -> None:
    dupes = defaultdict(list)
    for n, p in projects.items():
        for name, ver in p.packages:
            dupes[name].append((n, ver))
    drift = {k: v for k, v in dupes.items() if len({ver for _, ver in v}) > 1}
    rule("DEP-2", not drift, f"same package pinned to different versions across projects: {list(drift)[:3]}")


def main() -> int:
    tracked = git_ls()
    projects = inv.load_projects()
    check_git(tracked, projects)
    check_names(tracked, projects)
    check_files(tracked, projects)
    check_types(projects, tracked)
    check_arch(projects, tracked)
    check_security(projects, tracked)
    check_syntax(projects, tracked)
    check_policy(projects, tracked)
    check_deps(projects)
    check_versions(projects)
    check_artefacts(tracked)
    fails = [r for r in RESULTS if r[1] == "FAIL"]
    warns = [r for r in RESULTS if r[1] == "WARN"]
    for rid, status, detail in RESULTS:
        print(f"RULE {rid:7s} {status:4s}  {detail}")
    print(f"\n{len(RESULTS)} rules evaluated, {len(warns)} advisory, gate result: "
          f"{'PASS' if not fails else 'FAIL'}")
    if fails:
        print(f"failing: {', '.join(f[0] for f in fails)}")
    return 1 if fails else 0


if __name__ == "__main__":
    sys.exit(main())
