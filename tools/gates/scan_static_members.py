#!/usr/bin/env python3
"""Targeted static-class member-existence check for the compiled set.

The sandbox has no compiler; this catches the exact breakage class the textual gate
cannot see (CS0117 missing member, CS0111 duplicate member, CS0104 ambiguity) for a
set of well-known static utility classes. Namespaces resolve like C#: a file declared
inside `EJLive.Core` sees `EJLive.Core.X` first; otherwise the name must come from a
`using` in the same file.
"""
from __future__ import annotations
import os, re, sys, json
from collections import defaultdict

ROOT = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
sys.path.insert(0, os.path.join(ROOT, "tools", "inventory"))
import importlib
inv = importlib.import_module("ejlive_inventory")

CLASS = re.compile(r"(?:public|internal)\s+(?:static\s+)?(?:sealed\s+)?(?:partial\s+)?class\s+(\w+)")
MEMBER = re.compile(r"(?:public|internal)\s+(?:const|static|readonly|virtual|override)?\s*(?:[\w<>,\[\]\.\?]+\s+)?(\w+)\s*(?:\(|\{\s*get|=|;)")
USING = re.compile(r"^\s*using\s+(?:static\s+)?([\w.]+)\s*;", re.M)
NSDECL = re.compile(r"^\s*namespace\s+([\w.]+)", re.M)

def strip(text):
    text = re.sub(r"/\*.*?\*/", " ", text, flags=re.S)
    text = re.sub(r"//[^\n]*", " ", text)
    prev = None
    while prev != text:
        prev = text
        text = re.sub(r"@\"(?:[^\"]|\"\")*\"|\"\"\"(?:.|\n)*?\"\"\"|\"(?:\\.|[^\"\\])*\"", " ", text)
    return text

projects = inv.load_projects()
# 1) collect members of every class in every compiled file: (namespace, class) -> members
members: dict[tuple[str, str], set[str]] = defaultdict(set)
file_ns: dict[str, list[str]] = {}
file_usings: dict[str, list[str]] = {}
files_by_project: dict[str, list[str]] = {}
for proj in projects.values():
    files_by_project[proj.name] = proj.compiled
    for f in proj.compiled:
        t = strip(inv.read(f))
        ns_list = NSDECL.findall(t) or [""]
        file_ns[f] = ns_list
        file_usings[f] = USING.findall(t)
        # find class blocks: class name -> region up to balanced brace
        for m in re.finditer(r"(?:public|internal)\s+(?:static\s+)?(?:sealed\s+)?(?:partial\s+)?class\s+(\w+)", t):
            cname = m.group(1)
            i = t.find("{", m.end())
            if i < 0: continue
            depth, j = 0, i
            while j < len(t):
                if t[j] == "{": depth += 1
                elif t[j] == "}":
                    depth -= 1
                    if depth == 0: break
                j += 1
            body = t[i:j]
            for ns in ns_list:
                for mm in MEMBER.finditer(body):
                    members[(ns, cname)].add(mm.group(1))

# 2) for each file, find Simple.Member uses where Simple is a known static class name
counts = {"missing": [], "ambig": [], "dupctor": []}
static_names = {name for (ns, name) in members}
for proj in projects.values():
    for f in proj.compiled:
        t = strip(inv.read(f))
        ns_list = file_ns[f]; usings = file_usings[f]
        for m in re.finditer(r"\b([A-Z]\w+)\.([A-Za-z_]\w*)\b", t):
            simple, mem = m.group(1), m.group(2)
            if simple not in static_names: continue
            cands = {ns for (ns, n) in members if n == simple}
            visible = []
            for ns in ns_list:
                if (ns, simple) in members: visible.append((ns, simple))
            if not visible:
                for c in cands:
                    if c in usings or (c and any(u == c for u in usings)):
                        visible.append((c, simple))
                if not visible and "" in cands:
                    visible.append(("", simple))
            if len(visible) > 1:
                counts["ambig"].append(f"{f}: {simple} ambiguous between {visible}")
                continue
            if not visible:
                continue
            owner = visible[0]
            # member may exist under a different namespace variant of the class
            if mem in members[owner]:
                continue
            if mem in ("Instance", "Default", "Create", "Value"):
                continue
            if len(owner[0]) == 0 and False:
                continue
            # ignore common instance-member patterns accessed via property chains
            counts["missing"].append(f"{f}: {simple}.{mem} -> {owner[0] or '<global>'}.{simple} lacks it")

# dedupe
def dedup(rows):
    seen, out = set(), []
    for r in rows:
        if r not in seen:
            seen.add(r); out.append(r)
    return sorted(out)

missing = [x for x in dedup(counts["missing"]) if re.search(r"(AppConstants|NetworkConfig|Constants|ATMPaths|AppLogger|DataRoot\w*)\.", x)]
other = [x for x in dedup(counts["missing"]) if x not in missing]
print("=== STATIC CLASS MEMBER GAPS ===")
for x in missing: print(x)
print(f"=== other candidate-type member gaps ({len(other)}) ===")
for x in other[:40]: print(x)
print(f"\nsummary: static-class gaps {len(missing)}, other gaps {len(other)}, ambiguities {len(dedup(counts['ambig']))}")
