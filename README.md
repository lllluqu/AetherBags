# AetherBags

A Final Fantasy XIV Dalamud Plugin that enhances your inventory by organizing and displaying bag contents using native UI elements (made possible by KTK, KamiToolKit). 

It supports user-defined categories with custom names, ordering/priority, colors, and rule-based item filtering (e.g., by item ID, name patterns, UI category, rarity, level/item level, vendor price, and various flags).

![example](Images/example.png)

[![Download count](https://img.shields.io/endpoint?url=https://qzysathwfhebdai6xgauhz4q7m0mzmrf.lambda-url.us-east-1.on.aws/AetherBags)](https://github.com/Zeffuro/AetherBags)

## 简体中文汉化版 / Chinese localization

This fork (`lllluqu/AetherBags`) is a full Simplified Chinese localization of the plugin.
It ships as **AetherBags 汉化版** (`InternalName: AetherBagsCN`) and can be installed alongside the English original.

- Install repository: `https://raw.githubusercontent.com/lllluqu/AetherBags-Repo/main/pluginmaster.json`
- Language: follows the game client by default; can be switched in **设置 → 常规 → 语言**.
- Maintainers syncing with upstream: see [docs/localization-sync.md](docs/localization-sync.md) (中文).
- Localization entry point: `AetherBags/Localization/Loc.cs`, dictionary: `AetherBags/Localization/zh-CN.json`.

