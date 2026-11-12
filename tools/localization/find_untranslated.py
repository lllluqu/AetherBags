#!/usr/bin/env python3
"""List UI-looking English literals that are not translated yet.

Heuristic: literals with 2+ English words on lines that don't already use Loc.T,
excluding logging / identifiers / paths. Review the output and add entries to
AetherBags/Localization/zh-CN.json, then run wrap_strings.py.
"""
import ast
import json
import os
import re
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
SRC = ROOT / "AetherBags"
DICT = SRC / "Localization" / "zh-CN.json"

with open(DICT, encoding="utf-8") as f:
    trans = json.load(f)

LITERAL = re.compile(r'"((?:[^"\\]|\\.)*)"')
WORDISH = re.compile(r"[A-Za-z]{2,}")
SKIP = (
    "Logger", "DebugOnly", "Log.", "Information(", "Warning(", "Error(", "Verbose(",
    "throw new", "Exception(", "InternalName", "TexturePath", "Path.Combine",
)

rows = []
for dirpath, dirnames, filenames in os.walk(SRC):
    dirnames[:] = [d for d in dirnames if d not in ("obj", "bin", "Localization", "KamiToolKit")]
    for fn in filenames:
        if not fn.endswith(".cs"):
            continue
        path = Path(dirpath) / fn
        rel = str(path.relative_to(SRC))
        for i, line in enumerate(path.read_text(encoding="utf-8").split("\n"), start=1):
            if "Loc.T(" in line or any(t in line for t in SKIP):
                continue
            for m in LITERAL.finditer(line):
                content = m.group(1)
                try:
                    runtime = ast.literal_eval('"' + content + '"')
                except Exception:
                    runtime = content
                if runtime in trans:
                    continue
                if len(WORDISH.findall(runtime)) < 2 or len(runtime) < 12:
                    continue
                rows.append((rel, i, runtime, line.strip()))

print(f"candidates={len(rows)}")
seen = set()
for rel, line, text, ctx in rows:
    if text in seen:
        continue
    seen.add(text)
    print(f"{rel}:{line}: {text}")
print(f"unique={len(seen)}")
