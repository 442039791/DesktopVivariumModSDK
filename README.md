# DesktopVivarium 模组开发 SDK

本仓库是 **DesktopVivarium（桌面生态缸）** 的模组开发资料合集，面向想要为游戏制作模组的开发者。

- 想**使用**模组的普通玩家：请先看 [docs/玩家模组使用说明.md](docs/玩家模组使用说明.md)
- 想**制作**模组的开发者：从本 README 开始，配合 [docs/模组开发者文档.md](docs/模组开发者文档.md)

---

## 目录结构

```text
ModSDK/
├── README.md                # 本文件：入口 + 快速开始
├── LICENSE                  # 本 SDK 的许可证
├── THIRD_PARTY_NOTICES.md   # 第三方组件声明
├── docs/                    # 三份核心文档
│   ├── 模组开发者文档.md      # 模组能力边界、目录规范、加载规则
│   ├── DLL模组开发指南.md     # C# DLL 模组编译与 Harmony 补丁开发
│   └── 玩家模组使用说明.md     # 面向玩家的安装/排查说明
├── api/                     # 模组 API 接口源码（只读参考，无需编译）
├── templates/
│   └── ServerModTemplate/   # 最小 DLL 模组模板（改名字即可开工）
└── examples/
    ├── SpeedBoostMod/          # Harmony 补丁示例（C# DLL + 源码）
    ├── LocalizationExampleMod/ # 本地化/新语言示例
    └── ExampleFishMod/         # 资源覆盖 + 配置追加示例
```

---

## 快速开始：做一个最简单的模组

### 第 1 步：选模组类型

| 你想做的事 | 推荐类型 | 难度 | 参考示例 |
|------------|----------|------|----------|
| 改贴图 / 模型 / 音效 | 资源覆盖模组 | ⭐ | ExampleFishMod |
| 改数值（价格、速度等） | 配置覆盖模组（JSON） | ⭐ | ExampleFishMod |
| 加新语言 / 翻译 | 本地化模组 | ⭐ | LocalizationExampleMod |
| 改游戏逻辑（C#） | Server DLL 模组 | ⭐⭐⭐ | SpeedBoostMod |
| 改游戏逻辑（高级） | Harmony 补丁模组 | ⭐⭐⭐⭐ | SpeedBoostMod |
| 替换界面 | UI 模组 | ⭐⭐⭐ | 暂无完整示例，谨慎评估 |

**建议的上手顺序**：配置模组 → 本地化模组 → 最小 DLL 模组 → Harmony 模组。

### 第 2 步：搭目录

每个模组一个文件夹，放到游戏目录的 `Mods/` 下：

```text
Mods/MyMod/
├── manifest.json        # 必需，唯一必须项
├── ConfigData/          # 可选：覆盖游戏 JSON 配置
├── Image/               # 可选：覆盖贴图（路径需与游戏内一致）
├── Mesh/                # 可选：覆盖模型
├── Localization/        # 可选：本地化文本
└── Plugins/
    └── Server/          # 可选：Server 端 DLL
```

`manifest.json` 最小示例：

```json
{
  "Id": "com.你的名字.mymod",
  "Name": "我的模组",
  "Version": "1.0.0"
}
```

### 第 3 步：启用并重启

1. 把模组文件夹放进 `Mods/`
2. 启动游戏，打开游戏内模组管理界面，启用它
3. **重启游戏**（模组更改需要重启才生效）

详细规则（加载顺序、覆盖优先级、依赖写法）见 [docs/模组开发者文档.md](docs/模组开发者文档.md)。

---

## 快速开始：做 C# DLL 模组

1. 复制 `templates/ServerModTemplate/` 为你的模组名
2. 编辑 `Source/MyFirstMod.csproj`，把 `GameInstallDir` 指向你的游戏安装目录
3. 实现 `IModEntry` 接口（模板里已有最小实现）
4. `dotnet build -c Release`，DLL 自动输出到 `Plugins/Server/`
5. 启动游戏测试

> ⚠️ 公开版注意：编译 DLL 引用的 `Assembly-CSharp.dll` 来自**已安装游戏**的数据目录
> （`游戏名_Data/Managed/Assembly-CSharp.dll`），不需要游戏源码。
> 具体路径与 Unity 版本要求见 [docs/DLL模组开发指南.md](docs/DLL模组开发指南.md)。

---

## 当前能力边界（务必先读）

模组系统会随游戏版本演进，当前版本的能力边界与已知限制（如加载顺序以游戏内设置为准、
依赖不做强校验等）都在 [docs/模组开发者文档.md](docs/模组开发者文档.md) 第 1、17 节有明确说明。
**做模组前请先接受这些事实，避免做完不生效来回排查。**

## API 一览

接口定义在 `api/` 目录（只读参考）：

| 接口 | 作用 |
|------|------|
| `IModEntry` | 模组入口，所有 DLL 模组必须实现 |
| `IModContext` | 模组能力入口：日志 / 资源 / 配置 / 事件 / 钩子 / Harmony / UI |
| `IModGameEvents` | 游戏事件订阅 |
| `IModHooks` | 游戏行为钩子注册 |
| `IModHarmony` | Harmony 补丁管理 |
| `IModUIServices` | 模组 UI 注册 |

---

## 反馈与交流

- 模组问题排查：[docs/玩家模组使用说明.md](docs/玩家模组使用说明.md) 第 15 节（反馈问题时建议提供的信息）
- Steam 创意工坊：游戏发售后开放（模组可通过创意工坊订阅分发）
- （可选）模组作者交流群 / Discord：待补充

## 许可证

- 本 SDK 中的示例代码、模板：见 [LICENSE](LICENSE)
- 文档、游戏内接口的版权归属：见 [THIRD_PARTY_NOTICES.md](THIRD_PARTY_NOTICES.md)
