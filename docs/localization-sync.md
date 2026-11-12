# 上游更新时如何维护汉化版

本仓库是 [Zeffuro/AetherBags](https://github.com/Zeffuro/AetherBags) 的简体中文汉化分支。
上游发版后，按下面的流程把新代码合进来、补翻译、再发布。

## 0. 仓库结构

```
AetherBags/                     ← 插件工程（本地化代码都在这里）
  Localization/Loc.cs           ← 运行时翻译入口（Loc.T / Loc.TryGet）
  Localization/zh-CN.json       ← 汉化词典（英文原文 → 中文）
  Localization/LocResourceManager.cs  ← 给 KamiToolKit 枚举下拉框用的翻译桥
  Localization/KtkLocalizer.cs  ← 对 KTK 生成的文字节点做二次翻译
  Addons/Localized*.cs          ← KTK 选择器/取色器的本地化子类
tools/                          ← 构建、翻译校验、插件源生成脚本
docs/                           ← 文档（本文件）
.github/workflows/release.yml   ← 推 v* 标签自动构建 Release
```

汉化相关的改动分两类，冲突处理方式不同：

| 类型 | 文件 | 上游更新时 |
| --- | --- | --- |
| 本地化基础设施 | `Localization/*`、`Addons/Localized*.cs`、`tools/`、`docs/` | 保留本仓库版本，一般不会冲突 |
| 被 `Loc.T(...)` 包裹的业务代码 | 其余 `AetherBags/**/*.cs` | **以上游为准**，重新跑包装脚本 |

## 1. 一次性准备

```powershell
git clone --recurse-submodules https://github.com/lllluqu/AetherBags.git
cd AetherBags
git remote add upstream https://github.com/Zeffuro/AetherBags.git

# 构建依赖：.NET 10 SDK + Dalamud 开发库
# Dalamud 开发库默认放在 %AppData%\XIVLauncher\addon\Hooks\dev
# 下载 https://goatcorp.github.io/dalamud-distrib/latest.zip 解压到该目录
```

## 2. 每次上游更新

### 2.1 同步代码

```powershell
git fetch upstream
git merge upstream/master        # 或 git rebase upstream/master
git submodule update --init --recursive   # 上游可能更新了 KTK 子模块
```

### 2.2 解决冲突（重点）

绝大多数冲突来自我们给字符串加的 `Loc.T(...)` 包装。推荐做法：

```powershell
# 对冲突的“业务代码”文件，直接取上游版本
git checkout --theirs -- AetherBags/Nodes AetherBags/Addons AetherBags/Commands
# 或者逐个文件：git checkout --theirs -- <file>

# 保留我们自己的本地化文件（如果有冲突）
git checkout --ours -- AetherBags/Localization AetherBags/Addons/LocalizedSearchAddon.cs ...
```

> merge 时 `--theirs` = 上游（upstream），`--ours` = 当前分支（汉化）。rebase 时相反，注意别弄反。

然后重新把翻译套回去：

```powershell
python tools/localization/wrap_strings.py --dry   # 先看会改多少
python tools/localization/wrap_strings.py         # 实际写入
```

这个脚本是幂等的：只包裹词典里已有的英文，已经包过 `Loc.T(` 的行会跳过。

### 2.3 处理新出现的字符串

上游新增的 UI 文案不会自动翻译，先找出来：

```powershell
python tools/localization/find_untranslated.py
```

- 普通字面量：把英文加进 `AetherBags/Localization/zh-CN.json`，再跑一次 `wrap_strings.py`。
- 插值字符串（`$"..."`）和跨行拼接：脚本包不了，手工改成
  `Loc.T("... {0} ...", 参数)` 的形式，键写进词典（占位符顺序保持一致）。
- `[Description("...")]` 的枚举选项会被 `EnumExtensions` / KTK 的 ResourceManager 自动翻译，只要词典里有对应键即可。
- 日志、配置键、文件名、IPC 名、正则不要翻译。

### 2.4 校验

```powershell
python tools/localization/verify_keys.py     # 必须 missing=0
dotnet build -c Release                      # 或 powershell -File tools/build.ps1
```

`verify_keys.py` 会列出所有静态 `Loc.T("...")` 里没有译文的键，缺失时退出码为 1。

### 2.5 版本号

版本号规则：**上游版本 + 汉化修订号**，拼成第 4 段（Dalamud 只接受 4 段版本号）。

| 场景 | 版本号 |
| --- | --- |
| 上游 `1.0.0.5` | 汉化修订 1 = `1.0.0.51` |
| 同一上游的第二次汉化修订 | `1.0.0.52` |
| 上游升到 `1.0.0.6` | 汉化修订 1 = `1.0.0.61` |

修改 `AetherBags/AetherBags.csproj` 里的 `<Version>`，同时在 `AetherBags/changelog.md` 顶部加一节说明。

### 2.6 构建

```powershell
powershell -File tools/build.ps1            # 默认 Release
# 产物：AetherBags/bin/x64/Release/AetherBagsCN/latest.zip
```

### 2.7 发布

方式 A：推送标签，让 GitHub Actions 自动构建并创建 Release（推荐）

```powershell
git add -A
git commit -m "feat: 同步上游 1.0.0.6 并更新汉化"
git push origin master
git tag v1.0.0.61
git push origin v1.0.0.61
```

方式 B：本地构建后手动发布

1. 把 `latest.zip` 重命名为 `AetherBagsCN-<版本>.zip`（带版本号，避免 GitHub 下载缓存命中旧文件）。
2. 在 GitHub 创建 `v<版本>` 标签与 Release，上传这个 zip。
3. 更新聚合仓库 `MyDalamudRepo` 的 `pluginmaster.json`：

```powershell
python tools/make-pluginmaster.py --output ../MyDalamudRepo/pluginmaster.json
```

它会从 `csproj` 读版本号，生成正确的 `AssemblyVersion` 与下载链接，然后提交到 `MyDalamudRepo` 即可。

## 3. 常见问题

- **装不上 / 版本解析失败**：版本号必须是 4 段，`1.0.0.5.1` 这种 5 段是非法的。
- **下载到的 zip 损坏或还是旧版本**：GitHub Release 资产按 URL 缓存，务必使用带版本号的文件名。
- **下拉框又变英文**：确认 `Localization/zh-CN.json` 里有对应 `[Description]` 文本；KTK 枚举项走 `LocResourceManager`，只要键存在就会翻译。
- **取色器/物品选择器按钮变英文**：确认 `Addons/Localized*.cs` 仍然存在，且对应的 picker 继承自 `LocalizedSearchAddon` / `LocalizedItemSearchAddon` / `LocalizedColorPickerAddon`。
- **插件配置丢失**：汉化版内部名是 `AetherBagsCN`，配置目录为 `PluginConfigs/AetherBagsCN`，与英文原版互不影响。

## 4. 汉化新增文件清单（同步上游时不要删）

```
AetherBags/Localization/Loc.cs
AetherBags/Localization/LocResourceManager.cs
AetherBags/Localization/KtkLocalizer.cs
AetherBags/Localization/zh-CN.json
AetherBags/Addons/LocalizedSearchAddon.cs
AetherBags/Addons/LocalizedItemSearchAddon.cs
AetherBags/Addons/LocalizedColorPickerAddon.cs
tools/
docs/
.github/workflows/release.yml
```

`AetherBags.csproj` 里需要保留的汉化相关配置：

```xml
<Version>1.0.0.51</Version>
<AssemblyName>AetherBagsCN</AssemblyName>
<InternalName>AetherBagsCN</InternalName>
<Name>AetherBags 汉化版</Name>
<EmbeddedResource Include="Localization\zh-CN.json" LogicalName="AetherBags.Localization.zh-CN.json" />
```
