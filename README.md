# AetherBags

A Final Fantasy XIV Dalamud Plugin that enhances your inventory by organizing and displaying bag contents using native UI elements (made possible by KTK, KamiToolKit). 

It supports user-defined categories with custom names, ordering/priority, colors, and rule-based item filtering (e.g., by item ID, name patterns, UI category, rarity, level/item level, vendor price, and various flags).

![example](Images/example.png)

[![Download count](https://img.shields.io/endpoint?url=https://qzysathwfhebdai6xgauhz4q7m0mzmrf.lambda-url.us-east-1.on.aws/AetherBags)](https://github.com/Zeffuro/AetherBags)

## 简体中文汉化版 / Chinese localization

This fork (`lllluqu/AetherBags`) is a full Simplified Chinese localization of the plugin.
It ships as **AetherBags 汉化版** (`InternalName: AetherBagsCN`) and can be installed alongside the English original.

- 安装源（聚合仓库，收录多个汉化插件）：`https://raw.githubusercontent.com/lllluqu/MyDalamudRepo/main/pluginmaster.json`
- 语言：默认跟随客户端，可在 **设置 → 常规 → 语言** 切换「自动 / 简体中文 / English」。
- 汉化范围：界面、右键菜单、提示、聊天命令、DTR 服务器信息栏、更新日志、内置分类名，以及 KTK 生成的取色器 / 物品选择器 / 下拉框文字。
- 版本号：原版基线 + 汉化修订，例如原版 `1.0.0.5` 的汉化修订 1 为 `1.0.0.51`（Dalamud 只接受 4 段版本号）。
- 与英文原版的差异：`InternalName`、addon 名、IPC 前缀均为 `AetherBagsCN`，配置目录为 `PluginConfigs/AetherBagsCN`，两者互不冲突。
- 维护者同步上游：见 [docs/localization-sync.md](docs/localization-sync.md)（中文）。
- 本地化代码入口：`AetherBags/Localization/Loc.cs`，词典：`AetherBags/Localization/zh-CN.json`。

