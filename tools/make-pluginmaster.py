#!/usr/bin/env python3
"""Generate a Dalamud pluginmaster.json entry for the localized build.

Usage:
    python tools/make-pluginmaster.py --output ../MyDalamudRepo/pluginmaster.json

The version is read from AetherBags/AetherBags.csproj, so it always matches the
assembly that DalamudPackager just built.
"""
import argparse
import json
import re
import time
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
CSPROJ = ROOT / "AetherBags" / "AetherBags.csproj"

parser = argparse.ArgumentParser()
parser.add_argument("--owner", default="lllluqu")
parser.add_argument("--repo", default="AetherBags")
parser.add_argument("--output", required=True, help="Where to write pluginmaster.json")
args = parser.parse_args()

csproj = CSPROJ.read_text(encoding="utf-8")
version = re.search(r"<Version>([^<]+)</Version>", csproj).group(1)
tag = f"v{version}"
asset = f"AetherBagsCN-{version}.zip"
download = f"https://github.com/{args.owner}/{args.repo}/releases/download/{tag}/{asset}"

entry = {
    "Author": "Zeffuro, Pie Lover; 汉化: llluqu",
    "Name": "AetherBags 汉化版",
    "Description": "用游戏原生 UI 按自定义分类整理并显示背包物品，支持筛选、排序、固定分类与货币显示。基于 Zeffuro 的 AetherBags 全面汉化。",
    "InternalName": "AetherBagsCN",
    "AssemblyVersion": version,
    "RepoUrl": f"https://github.com/{args.owner}/{args.repo}",
    "ApplicableVersion": "any",
    "Tags": ["inventory", "bags", "organization", "categories", "filters", "sorting", "qol", "ui", "chinese", "汉化"],
    "CategoryTags": ["ui", "inventory"],
    "DalamudApiLevel": 15,
    "Punchline": "再也不用对着背包发愁！",
    "IconUrl": f"https://raw.githubusercontent.com/{args.owner}/{args.repo}/master/Images/icon.png",
    "ImageUrls": [f"https://raw.githubusercontent.com/{args.owner}/{args.repo}/master/Images/example.png"],
    "DownloadLinkInstall": download,
    "DownloadLinkUpdate": download,
    "DownloadLinkTesting": download,
    "IsHide": False,
    "IsTestingExclusive": False,
    "DownloadCount": 0,
    "LastUpdated": int(time.time()),
}

out = Path(args.output)
out.parent.mkdir(parents=True, exist_ok=True)
out.write_text(json.dumps([entry], ensure_ascii=False, indent=4) + "\n", encoding="utf-8", newline="\n")
print(f"version={version}\nasset={asset}\nwritten={out}")
