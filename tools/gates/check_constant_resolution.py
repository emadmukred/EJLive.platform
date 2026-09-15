#!/usr/bin/env python3
"""Constant-class resolution checker (compile-gap triage tool; advisory).

The sandbox has no .NET SDK; this checks the exact CS0117/CS0104 failure class for
the duplicated constant classes of the tree (`AppConstants`, `NetworkConfig`) so the
Windows CI job is never the place where they surface. Resolution follows C#:

  * a file whose namespace chain contains the declaring namespace sees that
    declaration first (enclosing-namespace preference);
  * otherwise the name may come from the file's own `using` directives — two
    candidates is CS0104; a `using X = ...;` alias resolves it;
  * members are collected per declaration; a use of a member missing from the
    resolved class is CS0117.

    python3 tools/gates/check_constant_resolution.py            # human report
    python3 tools/gates/check_constant_resolution.py --quiet     # exit code only
"""
from __future__ import annotations
import os, re, sys
from collections import defaultdict

ROOT = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
sys.path.insert(0, os.path.join(ROOT, "tools", "inventory"))
import importlib
inv = importlib.import_module("ejlive_inventory")

CLASSES = ("AppConstants", "NetworkConfig")

def strip(text: str) -> str:
    text = re.sub(r"/\*.*?\*/", " ", text, flags=re.S)
    text = re.sub(r"//[^\n]*", " ", text)
    prev = None
    while prev != text:
        prev = text
        text = re.sub(r'@\"(?:[^\"]|\"\")*\"|\"\"\"[\s\S]*?\"\"\"|\"(?:\\.|[^\"\\])*\"|\'(?:\\.|[^\'\\\n])*\'', " ", text)
    return text

def class_body(text: str, cname: str):
    out = []
    for m in re.finditer(r"\bclass\s+" + cname + r"\b", text):
        i = text.find("{", m.end())
        if i < 0:
            continue
        depth, j = 0, i
        while j < len(text):
            if text[j] == "{": depth += 1
            elif text[j] == "}":
                depth -= 1
                if depth == 0: break
            j += 1
        out.append(text[i + 1:j])
    return "\n".join(out)

MEMBER = re.compile(r"\b(?:const|static|readonly|public|internal)\b[^=;{}()]*?\b([A-Za-z_]\w*)\s*(?:=|\(|;|\{\s*get)", re.M)

def main() -> int:
    quiet = "--quiet" in sys.argv
    projects = inv.load_projects()

    # (namespace, class) -> member set ; and namespace of every declaration
    decls: dict[tuple[str, str], set[str]] = {}
    for proj in projects.values():
        for f in proj.compiled:
            text = strip(inv.read(f))
            # (position, namespace) anchors so each class belongs to the namespace that
            # precedes it in the file (file-scoped namespace => one anchor at -1).
            anchors = [(-1, "")] + [(m.start(), m.group(1))
                                    for m in re.finditer(r"^\s*namespace\s+([\w.]+)", text, re.M)]
            for cname in CLASSES:
                for cm in re.finditer(r"\bclass\s+" + cname + r"\b", text):
                    ns = ""
                    for pos, cand in anchors:
                        if pos < cm.start():
                            ns = cand
                        else:
                            break
                    body = class_body(text, cname)
                    key = (ns, cname)
                    acc = decls.setdefault(key, set())
                    for mm in MEMBER.finditer(body):
                        acc.add(mm.group(1))
                    for mm in re.finditer(r"\benum\s+(\w+)", body):
                        acc.add(mm.group(1))

    ns_of_class = defaultdict(list)
    for (ns, cname) in decls:
        ns_of_class[cname].append(ns)

    problems = []
    for proj in projects.values():
        for f in proj.compiled:
            raw = inv.read(f)
            text = strip(raw)
            m = re.search(r"^\s*namespace\s+([\w.]+)", text, re.M)
            ns = m.group(1) if m else ""
            chain = []
            if ns:
                parts = ns.split(".")
                for k in range(len(parts), 0, -1):
                    chain.append(".".join(parts[:k]))
            using_ns = set(re.findall(r"^\s*using\s+(?:static\s+)?([\w.]+)\s*;", text, re.M))
            aliases = dict(re.findall(r"^\s*using\s+(\w+)\s*=\s*([\w.]+)\s*;", text, re.M))
            for cname in CLASSES:
                for mm in re.finditer(r"(?<![.\w])" + cname + r"\.([A-Za-z_]\w*)", text):
                    member = mm.group(1)
                    # alias wins over everything
                    if cname in aliases:
                        fq = aliases[cname]
                        owner_ns = None
                        for (ns2, c2) in decls:
                            if c2 == cname and fq.endswith(f"{ns2}.{cname}"):
                                owner_ns = ns2
                        if owner_ns is None:
                            continue
                        candidates = [(owner_ns, cname)]
                    else:
                        candidates = [((e, cname)) for e in chain if (e, cname) in decls]
                        if not candidates:
                            candidates = [((u, cname)) for u in using_ns if (u, cname) in decls]
                            if cname in using_ns or any(u.endswith(cname) for u in using_ns):
                                pass
                    if not candidates:
                        continue  # simple name may resolve to a local type; not our concern
                    if len(candidates) > 1:
                        problems.append(f"AMBIG {f}: {cname} resolves to {candidates} (CS0104)")
                        continue
                    owner = candidates[0]
                    if member not in decls[owner]:
                        problems.append(f"MISSING {f}: {member} not declared in {owner[0] or '<global>'}.{cname} (CS0117)")

    seen, out = set(), []
    for p in problems:
        if p not in seen:
            seen.add(p); out.append(p)
    if not quiet:
        for p in sorted(out):
            print(p)
    print(f"constant-resolution scan: {len(out)} issue(s) across {sum(len(p.compiled) for p in projects.values())} compiled files")
    return 1 if out else 0

if __name__ == "__main__":
    raise SystemExit(main())
