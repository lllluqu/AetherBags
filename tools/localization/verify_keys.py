#!/usr/bin/env python3
"""Verify every static Loc.T("...") key has a zh-CN translation.

Exit code 1 when something is missing, so it can gate a release.
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

with open(DICT, encoding="utf-8") as f:
    trans = json.load(f)

CALL = re.compile(r'Loc\.T\(\s*"((?:[^"\\]|\\.)*)"')
DYNAMIC = re.compile(r'Loc\.T\(\s*\$')

missing = {}
used = set()
dynamic = []

for dirpath, dirnames, filenames in os.walk(SRC):
    dirnames[:] = [d for d in dirnames if d not in ("obj", "bin", "KamiToolKit")]
    for fn in filenames:
        if not fn.endswith(".cs"):
            continue
        path = Path(dirpath) / fn
        rel = str(path.relative_to(SRC))
        text = path.read_text(encoding="utf-8")
        for m in CALL.finditer(text):
            try:
                runtime = ast.literal_eval('"' + m.group(1) + '"')
            except Exception:
                runtime = m.group(1)
            used.add(runtime)
            if runtime not in trans:
                line = text.count("\n", 0, m.start()) + 1
                missing.setdefault(runtime, []).append(f"{rel}:{line}")
        for m in DYNAMIC.finditer(text):
            line = text.count("\n", 0, m.start()) + 1
            dynamic.append(f"{rel}:{line}")

print(f"static_keys={len(used)} translated={len(used) - len(missing)} missing={len(missing)} dynamic={len(dynamic)}")
for key, locs in sorted(missing.items()):
    print(f"  MISSING {key!r} <- {', '.join(locs[:3])}")
for d in dynamic:
    print(f"  DYNAMIC {d}")

sys.exit(1 if missing else 0)
