#!/usr/bin/env python3
"""Wrap known UI strings in Loc.T("...") calls.

Idempotent: lines that already contain Loc.T( are skipped, and only literals whose
runtime value exists in AetherBags/Localization/zh-CN.json are wrapped.

Usage:
    python tools/localization/wrap_strings.py --dry
    python tools/localization/wrap_strings.py
"""
import ast
import json
import os
import re
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
SRC = ROOT / "AetherBags"
DICT = SRC / "Localization" / "zh-CN.json"
LOG = ROOT / "wrap-log.csv"
DRY = "--dry" in sys.argv

with open(DICT, encoding="utf-8") as f:
    trans = json.load(f)
keys = set(trans)
print(f"dictionary keys: {len(keys)}")

LITERAL = re.compile(r'"((?:[^"\\]|\\.)*)"')

SKIP_SUBSTRINGS = (
    "[Description(", "[System.ComponentModel.Description",
    "const string", 'case "', "using ", "Loc.T(",
    ".Contains(", ".StartsWith(", ".EndsWith(", ".Equals(",
    "AddonName", "InternalName", "GetAddonByName",
    "nameof(", "DllDirectory", "ResourceName",
)

changed_files = 0
total = 0
log_rows = []

for dirpath, dirnames, filenames in os.walk(SRC):
    dirnames[:] = [d for d in dirnames if d not in ("obj", "bin", "Localization", "KamiToolKit")]
    for fn in filenames:
        if not fn.endswith(".cs"):
            continue
        path = Path(dirpath) / fn
        rel = str(path.relative_to(SRC))
        text = path.read_text(encoding="utf-8")
        lines = text.split("\n")
        file_changes = 0

        for i, line in enumerate(lines):
            stripped = line.lstrip()
            if stripped.startswith("//") or stripped.startswith("*"):
                continue
            if any(tok in line for tok in SKIP_SUBSTRINGS):
                continue

            counter = [0]

            def repl(m, line=line):
                content = m.group(1)
                try:
                    runtime = ast.literal_eval('"' + content + '"')
                except Exception:
                    runtime = content
                if runtime not in keys:
                    return m.group(0)
                start = m.start()
                prev = line[start - 1] if start > 0 else ""
                if prev in ("$", "@", "\\"):
                    return m.group(0)
                after = line[m.end():].lstrip()
                before = line[:start].rstrip()
                if after.startswith("==") or after.startswith("!="):
                    return m.group(0)
                if before.endswith("==") or before.endswith("!="):
                    return m.group(0)
                counter[0] += 1
                return 'Loc.T("' + content + '")'

            new_line = LITERAL.sub(repl, line)
            if new_line != line:
                log_rows.append((rel, i + 1, line.strip(), new_line.strip()))
                lines[i] = new_line
                file_changes += counter[0]

        if file_changes:
            changed_files += 1
            total += file_changes
            if not DRY:
                path.write_text("\n".join(lines), encoding="utf-8", newline="")

if log_rows:
    with open(LOG, "w", encoding="utf-8-sig", newline="") as f:
        f.write("file,line,before,after\n")
        for r in log_rows:
            f.write('"{}",{},"{}","{}"\n'.format(r[0], r[1], r[2].replace('"', '""'), r[3].replace('"', '""')))

print(f"files_changed={changed_files} replacements={total} dry={DRY}")
if log_rows:
    print(f"log: {LOG}")
