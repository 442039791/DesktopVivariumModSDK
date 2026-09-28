# DLL模组开发指南

本文档说明如何为桌面生态箱游戏开发 DLL 脚本模组。

> 📌 **公开 SDK 版说明**
>
> 本文档同步自游戏工程内部。文中出现的 `C:\1\server\...`、`C:\1\client\...` 是**开发者工程路径**，
> 作为普通模组作者你不需要它们 —— 请引用你**已安装游戏**目录下的程序集：
> `游戏安装目录\DesktopVivarium_Data\Managed\Assembly-CSharp.dll`。
> 开箱即用的编译模板见本仓库 `templates/ServerModTemplate/`。

> 说明
>
> 本文档已按当前代码实现修订。
> 如果本文档与旧设计文档冲突，以 `MOD_SYSTEM_CURRENT_STATE.md` 和当前代码为准。

---

## 1. 模组目录结构

```
Mods/
└── YourModName/
    ├── manifest.json           # 模组配置文件（必需）
    ├── Plugins/
    │   ├── Server/             # Server端DLL
    │   │   └── YourMod.dll
    │   └── Client/             # Client端DLL
    │       └── YourClientMod.dll
    ├── ConfigData/             # 配置文件覆盖（可选）
    ├── Image/                  # 图片资源（可选）
    └── Source/                 # 源代码（建议保留，方便维护）
        ├── YourMod.cs
        └── YourMod.csproj      # 项目文件（可选）
```

---

## 2. Server端 vs Client端

| 类型 | 放置目录 | 加载项目 | 用途 |
|------|----------|----------|------|
| **Server** | `Plugins/Server/` | server项目 | 游戏逻辑、存档、配置、生态箱管理、数值计算 |
| **Client** | `Plugins/Client/` | client项目 | 渲染效果、客户端UI、视觉特效、粒子系统 |

### 选择依据

- 修改**游戏逻辑**（如加速、自动喂食、繁殖规则）→ **Server端**
- 修改**视觉效果**（如天气、光照、特效）→ **Client端**
- 大多数模组只需要 **Server端** DLL

---

## 3. manifest.json 配置

```json
{
  "Id": "com.yourname.modname",
  "Name": "模组显示名称",
  "Version": "1.0.0",
  "Author": "作者名",
  "Description": "模组描述",
  "MinGameVersion": "0.1.0",
  "Dependencies": ["com.lib.harmony"],
  "Scripts": {
    "Server": ["YourServerMod.dll"],
    "Client": ["YourClientMod.dll"]
  }
}
```

### 字段说明

| 字段 | 必需 | 说明 |
|------|------|------|
| `Id` | ✅ | 模组唯一标识符，建议格式：`com.作者.模组名` |
| `Name` | ✅ | 模组显示名称 |
| `Version` | ✅ | 版本号，格式：`主版本.次版本.修订号` |
| `Author` | ❌ | 作者名 |
| `Description` | ❌ | 模组描述 |
| `MinGameVersion` | ❌ | 最低游戏版本要求 |
| `Dependencies` | ❌ | 依赖的其他模组ID列表 |
| `Scripts.Server` | ❌ | Server端DLL文件名列表 |
| `Scripts.Client` | ❌ | Client端DLL文件名列表 |

### 3.1 当前实现中的实际生效范围

当前代码中，`manifest.json` 会被直接读取到 `ModManifest`，但真正稳定生效的字段主要只有：

- `Id`
- `Name`
- `Version`
- `Author`
- `Description`
- `MinGameVersion`
- `Dependencies`
- `Scripts.Server`
- `Scripts.Client`

当前代码不会直接从 manifest 中读取并应用这些常见字段：

- `LoadOrder`
- `GameVersion`
- `MaxGameVersion`
- `Tags`
- 复杂结构的依赖对象

当前实际的启用状态和加载顺序以 `ModSettings.json` 为准，而不是 manifest。

### 3.2 当前实现中的加载顺序来源

当前代码的加载顺序来自：

- `server/Assets/StreamingAssets/Save/ModSettings.json`

也就是说：

- manifest 里即使写了 `LoadOrder`
- 当前运行时代码也不会直接把它应用到 `ModInfo.LoadOrder`

如果你需要稳定控制先后顺序，当前应以游戏内保存的模组设置为准。

---

## 4. 模组接口

### 4.1 IModEntry 接口（必须实现）

所有DLL模组必须包含一个实现 `IModEntry` 接口的类：

```csharp
public interface IModEntry
{
    /// <summary>模组ID，必须与manifest.json中的Id一致</summary>
    string ModId { get; }

    /// <summary>模组显示名称</summary>
    string ModName { get; }

    /// <summary>模组版本</summary>
    string Version { get; }

    /// <summary>模组初始化时调用</summary>
    void Initialize(IModContext context);

    /// <summary>模组启用时调用</summary>
    void OnEnable();

    /// <summary>模组禁用时调用</summary>
    void OnDisable();

    /// <summary>模组卸载时调用</summary>
    void OnUnload();

    /// <summary>每帧更新（可选实现）</summary>
    void OnUpdate();
}
```

### 4.2 IModContext 接口（模组上下文）

通过 `Initialize` 方法传入，提供对游戏功能的访问：

```csharp
public interface IModContext
{
    /// <summary>当前模组ID</summary>
    string ModId { get; }

    /// <summary>模组根目录路径</summary>
    string ModPath { get; }

    /// <summary>日志记录器</summary>
    IModLogger Logger { get; }

    /// <summary>资源加载器</summary>
    IModAssetLoader AssetLoader { get; }

    /// <summary>配置加载器</summary>
    IModConfigLoader ConfigLoader { get; }

    /// <summary>事件总线</summary>
    IModEventBus EventBus { get; }

    /// <summary>游戏服务访问</summary>
    IModGameServices GameServices { get; }

    /// <summary>游戏事件订阅</summary>
    IModGameEvents GameEvents { get; }

    /// <summary>游戏钩子注册</summary>
    IModHooks Hooks { get; }

    /// <summary>Harmony 补丁管理器</summary>
    IModHarmonyManager HarmonyManager { get; }

    /// <summary>UI 服务</summary>
    IModUIServices UIServices { get; }
}
```

### 4.3 IModGameServices 接口（游戏服务）

提供对游戏核心功能的访问：

```csharp
public interface IModGameServices
{
    /// <summary>获取所有生态箱</summary>
    IReadOnlyList<BioTankManager> GetAllBioTanks();

    /// <summary>获取当前活动的生态箱</summary>
    BioTankManager GetActiveBioTank();

    /// <summary>设置生态箱更新速度倍率</summary>
    void SetBioTankSpeedMultiplier(float multiplier);

    /// <summary>获取当前速度倍率</summary>
    float GetBioTankSpeedMultiplier();
}
```

### 4.4 IModHarmonyManager 接口（Harmony补丁）

提供 Harmony 运行时代码修补功能：

```csharp
public interface IModHarmonyManager
{
    /// <summary>Harmony 是否可用（需要先加载 HarmonyLib 模组）</summary>
    bool IsHarmonyAvailable { get; }

    /// <summary>创建 Harmony 实例</summary>
    IHarmonyInstance CreateHarmony(string harmonyId);

    /// <summary>卸载指定模组的所有补丁</summary>
    void UnpatchAll(string harmonyId);
}

public interface IHarmonyInstance
{
    string Id { get; }

    /// <summary>添加前缀补丁（在原方法之前执行）</summary>
    void Prefix(MethodInfo original, MethodInfo prefix);

    /// <summary>添加后缀补丁（在原方法之后执行）</summary>
    void Postfix(MethodInfo original, MethodInfo postfix);

    /// <summary>添加转译器补丁（修改IL代码）</summary>
    void Transpiler(MethodInfo original, MethodInfo transpiler);

    /// <summary>自动应用带有 Harmony 特性的补丁类</summary>
    void PatchAll(Type patchType);

    /// <summary>移除此实例的所有补丁</summary>
    void UnpatchAll();
}
```

---

## 5. 编译环境配置

### 5.1 必需的程序集引用

编译DLL模组需要引用以下程序集：

| 程序集 | 位置 | 说明 |
|--------|------|------|
| `UnityEngine.dll` | Unity编辑器目录 | Unity核心 |
| `UnityEngine.CoreModule.dll` | Unity编辑器目录 | Unity核心模块 |
| `Assembly-CSharp.dll` | 项目Library目录 | 游戏代码（包含IModEntry等接口） |

### 5.2 程序集路径

**Unity编辑器程序集**（根据你安装的Unity版本调整路径）：

```
# Unity 6 (6000.0.49f1)
C:\Program Files\Unity\Hub\Editor\6000.0.49f1\Editor\Data\Managed\UnityEngine.dll
C:\Program Files\Unity\Hub\Editor\6000.0.49f1\Editor\Data\Managed\UnityEngine\UnityEngine.CoreModule.dll

# Unity 2022.3
C:\Program Files\Unity\Hub\Editor\2022.3.x\Editor\Data\Managed\UnityEngine.dll
C:\Program Files\Unity\Hub\Editor\2022.3.x\Editor\Data\Managed\UnityEngine\UnityEngine.CoreModule.dll
```

**游戏程序集**：

```
# Server端（编译Server端DLL时引用）
C:\1\server\Library\ScriptAssemblies\Assembly-CSharp.dll

# Client端（编译Client端DLL时引用）
C:\1\client\Library\ScriptAssemblies\Assembly-CSharp.dll
```

> **注意**：必须先在Unity编辑器中打开项目一次，让Unity编译生成 `Assembly-CSharp.dll`

---

## 6. 编译方法

### 6.1 使用 .NET SDK（推荐）

创建 `.csproj` 项目文件：

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>netstandard2.1</TargetFramework>
    <AssemblyName>YourModName</AssemblyName>
    <OutputPath>..\Plugins\Server\</OutputPath>
    <AppendTargetFrameworkToOutputPath>false</AppendTargetFrameworkToOutputPath>
    <GenerateAssemblyInfo>false</GenerateAssemblyInfo>
    <EnableDefaultCompileItems>false</EnableDefaultCompileItems>
  </PropertyGroup>

  <ItemGroup>
    <Compile Include="YourMod.cs" />
  </ItemGroup>

  <ItemGroup>
    <!-- Unity 程序集 - 根据你的Unity安装路径调整 -->
    <Reference Include="UnityEngine">
      <HintPath>C:\Program Files\Unity\Hub\Editor\6000.0.49f1\Editor\Data\Managed\UnityEngine.dll</HintPath>
      <Private>false</Private>
    </Reference>
    <Reference Include="UnityEngine.CoreModule">
      <HintPath>C:\Program Files\Unity\Hub\Editor\6000.0.49f1\Editor\Data\Managed\UnityEngine\UnityEngine.CoreModule.dll</HintPath>
      <Private>false</Private>
    </Reference>
    <!-- 游戏程序集 -->
    <Reference Include="Assembly-CSharp">
      <HintPath>C:\1\server\Library\ScriptAssemblies\Assembly-CSharp.dll</HintPath>
      <Private>false</Private>
    </Reference>
  </ItemGroup>

</Project>
```

编译命令：

```bash
# 进入源代码目录
cd Mods/YourModName/Source

# 编译
dotnet build -c Release

# DLL会输出到 ../Plugins/Server/ 目录
```

### 6.1.1 当前工程下的实际注意事项

当前项目里：

- Server 模组应输出到 `Plugins/Server/`
- Client 模组应输出到 `Plugins/Client/`
- 如果 manifest 写了 `Scripts.Server` 或 `Scripts.Client`，文件名必须和输出 DLL 一致

实际验证结果：

- `SpeedBoostMod` 当前可以成功编译
- `UIExampleMod` 当前工作区下没有现成 DLL，且其 `.csproj` 需要额外核对 Unity UI 引用路径后才能稳定编译

### 6.2 使用 Visual Studio

1. **创建项目**
   - 新建 → 类库（.NET Standard 2.1 或 .NET Framework 4.8）
   - 项目名称与模组名称一致

2. **添加引用**
   - 右键项目 → 添加 → 引用
   - 浏览到上述程序集路径，添加引用
   - 将引用的"复制本地"设为 `False`

3. **编写代码**
   - 实现 `IModEntry` 接口
   - 不需要复制接口定义，编译时会从 `Assembly-CSharp.dll` 引用

4. **编译**
   - 生成 → 生成解决方案
   - 输出的 DLL 放入 `Plugins/Server/` 或 `Plugins/Client/`

### 6.3 使用命令行（csc）

```bash
# Windows (需要 .NET Framework SDK)
csc /target:library /out:YourMod.dll ^
    /reference:"C:\Program Files\Unity\Hub\Editor\6000.0.49f1\Editor\Data\Managed\UnityEngine.dll" ^
    /reference:"C:\Program Files\Unity\Hub\Editor\6000.0.49f1\Editor\Data\Managed\UnityEngine\UnityEngine.CoreModule.dll" ^
    /reference:"C:\1\server\Library\ScriptAssemblies\Assembly-CSharp.dll" ^
    YourMod.cs
```

---

## 7. Harmony 补丁开发

### 7.1 什么是 Harmony

Harmony 是一个运行时代码修补库，允许模组在不修改游戏源代码的情况下修改游戏行为。

### 7.2 使用 Harmony 的前提

1. 必须依赖 `com.lib.harmony` 模组
2. 在 `manifest.json` 中声明依赖：
   ```json
   {
     "Dependencies": ["com.lib.harmony"]
   }
   ```

### 7.2.1 当前实现中的限制

当前代码里，`Dependencies` 只是清单字段，不代表已经实现了严格的依赖解析和拓扑排序。

另外，当前 `Harmony` 初始化时序存在一个实现细节：

- `ModHarmonyManager.Initialize()` 会在模组真正开始加载前先执行
- 它会先检查当前 `AppDomain` 中是否已经存在 `0Harmony`
- 如果没有，才尝试从“已加载模组”的目录中查找 `0Harmony.dll`

这意味着：

- `com.lib.harmony` 虽然是推荐依赖写法
- 但当前 `Harmony` 是否真正可用，还会受到初始化时序影响

因此当前更准确的说法是：

- 依赖 `com.lib.harmony` 是正确写法
- 但不能把它理解成“只要写了依赖就一定能在当前实现里稳定生效”

### 7.3 补丁类型

| 类型 | 说明 | 用途 |
|------|------|------|
| **Prefix** | 在原方法之前执行 | 修改参数、跳过原方法 |
| **Postfix** | 在原方法之后执行 | 修改返回值、添加逻辑 |
| **Transpiler** | 修改IL代码 | 复杂的代码修改 |

### 7.4 Prefix 补丁示例

修改 `BioTankManager.StateUpdate` 方法的参数，实现60倍加速：

```csharp
using System;
using System.Reflection;
using UnityEngine;

public class SpeedBoostMod : IModEntry
{
    private IModContext _context;
    private IHarmonyInstance _harmony;

    public static float SpeedMultiplier = 60.0f;

    public string ModId => "com.example.speedboost";
    public string ModName => "生态箱加速模组";
    public string Version => "1.0.0";

    public void Initialize(IModContext context)
    {
        _context = context;
    }

    public void OnEnable()
    {
        // 检查 Harmony 是否可用
        if (!_context.HarmonyManager.IsHarmonyAvailable)
        {
            _context.Logger.LogError("Harmony 不可用！请先加载 HarmonyLib 模组");
            return;
        }

        // 创建 Harmony 实例
        _harmony = _context.HarmonyManager.CreateHarmony(ModId);

        // 获取目标方法
        var targetType = typeof(BioTankManager);
        var targetMethod = targetType.GetMethod("StateUpdate",
            BindingFlags.Public | BindingFlags.Instance,
            null,
            new Type[] { typeof(float) },
            null);

        // 获取补丁方法
        var prefixMethod = typeof(SpeedBoostMod).GetMethod("StateUpdatePrefix",
            BindingFlags.Public | BindingFlags.Static);

        // 应用补丁
        _harmony.Prefix(targetMethod, prefixMethod);

        _context.Logger.Log($"加速模组已启用，速度倍率: {SpeedMultiplier}x");
    }

    /// <summary>
    /// Prefix 补丁 - 修改 IntervalTime 参数
    /// 参数名必须与原方法一致，使用 ref 可以修改值
    /// </summary>
    public static void StateUpdatePrefix(ref float IntervalTime)
    {
        IntervalTime *= SpeedMultiplier;
    }

    public void OnDisable()
    {
        _harmony?.UnpatchAll();
        _context.Logger.Log("加速模组已禁用");
    }

    public void OnUnload()
    {
        _harmony?.UnpatchAll();
    }

    public void OnUpdate() { }
}
```

### 7.5 Postfix 补丁示例

在方法执行后修改结果：

```csharp
/// <summary>
/// Postfix 补丁 - 修改返回值
/// __result 是特殊参数，代表原方法的返回值
/// </summary>
public static void SomeMethodPostfix(ref int __result)
{
    __result *= 2;  // 将返回值翻倍
}

/// <summary>
/// Postfix 补丁 - 访问实例
/// __instance 是特殊参数，代表调用方法的实例
/// </summary>
public static void SomeMethodPostfix(BioTankManager __instance)
{
    Debug.Log($"生态箱 {__instance.stateData.ID} 的方法执行完毕");
}
```

### 7.6 Harmony 特殊参数

| 参数 | 说明 |
|------|------|
| `ref 参数名` | 修改原方法的参数（参数名必须与原方法一致） |
| `__instance` | 原方法的调用实例（仅实例方法） |
| `__result` | 原方法的返回值（仅 Postfix） |
| `__state` | 在 Prefix 和 Postfix 间传递数据 |
| `返回 bool` | Prefix 返回 false 可跳过原方法 |

---

## 8. 游戏事件订阅

### 8.1 可订阅的事件

通过 `_context.GameEvents` 订阅游戏事件：

```csharp
public void OnEnable()
{
    // 生态箱创建事件
    _context.GameEvents.OnBioTankCreated += OnBioTankCreated;

    // 生物死亡事件
    _context.GameEvents.OnCreatureDied += OnCreatureDied;

    // 植物成长事件
    _context.GameEvents.OnPlantGrown += OnPlantGrown;
}

public void OnDisable()
{
    // 记得取消订阅
    _context.GameEvents.OnBioTankCreated -= OnBioTankCreated;
    _context.GameEvents.OnCreatureDied -= OnCreatureDied;
    _context.GameEvents.OnPlantGrown -= OnPlantGrown;
}

private void OnBioTankCreated(BioTankCreatedEventArgs args)
{
    _context.Logger.Log($"生态箱 {args.BioTankId} 已创建");
}

private void OnCreatureDied(CreatureDiedEventArgs args)
{
    _context.Logger.Log($"生物 {args.CreatureName} 死亡，原因: {args.DeathReason}");
}

private void OnPlantGrown(PlantGrownEventArgs args)
{
    _context.Logger.Log($"植物 {args.PlantName} 成长阶段: {args.GrowthStage}");
}
```

---

## 9. 钩子系统

### 9.1 注册钩子

通过 `_context.Hooks` 注册钩子，可以拦截和修改游戏行为：

```csharp
public void OnEnable()
{
    // 注册生物生成钩子（优先级100，数值越小越先执行）
    _context.Hooks.RegisterCreatureSpawnHook(OnCreatureSpawn, 100);

    // 注册伤害计算钩子
    _context.Hooks.RegisterDamageHook(OnDamageCalculation, 100);
}

public void OnDisable()
{
    // 移除钩子
    _context.Hooks.UnregisterCreatureSpawnHook(OnCreatureSpawn);
    _context.Hooks.UnregisterDamageHook(OnDamageCalculation);
}

private void OnCreatureSpawn(CreatureSpawnHookArgs args)
{
    // 修改生成的生物属性
    args.Health *= 2;  // 双倍生命值

    // 或者取消生成
    // args.Cancel = true;
}

private void OnDamageCalculation(DamageHookArgs args)
{
    // 修改伤害值
    args.Damage *= 0.5f;  // 伤害减半
}
```

---

## 10. 调试技巧

### 10.1 日志输出

```csharp
_context.Logger.Log("普通日志");
_context.Logger.LogWarning("警告日志");
_context.Logger.LogError("错误日志");
```

日志会显示在Unity Console中，格式：`[Mod:模组ID] 消息内容`

### 10.2 常见问题

| 问题 | 原因 | 解决方案 |
|------|------|----------|
| DLL未加载 | manifest.json配置错误 | 检查Scripts字段和文件名 |
| 找不到类型 | 缺少引用 | 添加UnityEngine和Assembly-CSharp引用 |
| 模组无效果 | OnEnable未调用 | 检查Initialize是否正常执行 |
| 空引用异常 | _context未初始化 | 确保在OnEnable中使用_context |
| Harmony不可用 | Harmony库未提前进入运行域，或初始化时序过早 | 检查 `com.lib.harmony` 是否存在，并结合当前实现确认 `0Harmony.dll` 是否已被实际加载 |
| 补丁无效果 | 方法签名不匹配 | 检查参数名称和类型 |

### 10.3 热重载

目前不支持热重载，修改DLL后需要重启游戏。

---

## 11. 完整示例：Harmony加速模组

### 目录结构

```
Mods/SpeedBoostMod/
├── manifest.json
├── README.txt
├── Plugins/
│   └── Server/
│       └── SpeedBoostMod.dll
└── Source/
    ├── SpeedBoostMod.cs
    └── SpeedBoostMod.csproj
```

### manifest.json

```json
{
  "Id": "com.example.speedboost",
  "Name": "生态箱加速模组",
  "Version": "1.0.0",
  "Author": "ModDeveloper",
  "Description": "使用 Harmony 补丁将生态箱运行速度加快60倍",
  "MinGameVersion": "0.1.0",
  "Dependencies": ["com.lib.harmony"],
  "Scripts": {
    "Server": ["SpeedBoostMod.dll"],
    "Client": []
  }
}
```

### SpeedBoostMod.csproj

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>netstandard2.1</TargetFramework>
    <AssemblyName>SpeedBoostMod</AssemblyName>
    <OutputPath>..\Plugins\Server\</OutputPath>
    <AppendTargetFrameworkToOutputPath>false</AppendTargetFrameworkToOutputPath>
    <GenerateAssemblyInfo>false</GenerateAssemblyInfo>
    <EnableDefaultCompileItems>false</EnableDefaultCompileItems>
  </PropertyGroup>

  <ItemGroup>
    <Compile Include="SpeedBoostMod.cs" />
  </ItemGroup>

  <ItemGroup>
    <Reference Include="UnityEngine">
      <HintPath>C:\Program Files\Unity\Hub\Editor\6000.0.49f1\Editor\Data\Managed\UnityEngine.dll</HintPath>
      <Private>false</Private>
    </Reference>
    <Reference Include="UnityEngine.CoreModule">
      <HintPath>C:\Program Files\Unity\Hub\Editor\6000.0.49f1\Editor\Data\Managed\UnityEngine\UnityEngine.CoreModule.dll</HintPath>
      <Private>false</Private>
    </Reference>
    <Reference Include="Assembly-CSharp">
      <HintPath>C:\1\server\Library\ScriptAssemblies\Assembly-CSharp.dll</HintPath>
      <Private>false</Private>
    </Reference>
  </ItemGroup>

</Project>
```

### SpeedBoostMod.cs

见第7.4节示例代码。

---

## 12. 注意事项

1. **ModId必须唯一** - 建议使用 `com.作者名.模组名` 格式
2. **Dependencies 目前不是严格依赖系统** - 当前实现会读取该字段，但不会像完整包管理器那样自动做依赖拓扑和阻断
3. **Server端DLL无命名空间限制** - 可以访问System.IO、System.Net等
4. **避免阻塞主线程** - 耗时操作请使用协程或异步
5. **正确清理资源** - 在OnDisable和OnUnload中释放资源、移除补丁
6. **版本兼容性** - 游戏更新后可能需要重新编译DLL
7. **Unity版本匹配** - 编译时引用的Unity版本应与游戏一致
8. **以当前实现为准** - 若与旧文档冲突，优先参考 `MOD_SYSTEM_CURRENT_STATE.md`

---

## 13. 附录：常用程序集路径

### Unity 6 (6000.0.49f1)

```
UnityEngine.dll:
C:\Program Files\Unity\Hub\Editor\6000.0.49f1\Editor\Data\Managed\UnityEngine.dll

UnityEngine.CoreModule.dll:
C:\Program Files\Unity\Hub\Editor\6000.0.49f1\Editor\Data\Managed\UnityEngine\UnityEngine.CoreModule.dll

UnityEngine.InputLegacyModule.dll (Input类):
C:\Program Files\Unity\Hub\Editor\6000.0.49f1\Editor\Data\Managed\UnityEngine\UnityEngine.InputLegacyModule.dll
```

### 游戏程序集

```
Server端:
C:\1\server\Library\ScriptAssemblies\Assembly-CSharp.dll

Client端:
C:\1\client\Library\ScriptAssemblies\Assembly-CSharp.dll
```

### 其他常用程序集

```
Newtonsoft.Json (JSON处理):
C:\1\server\Library\ScriptAssemblies\Newtonsoft.Json.dll

TextMeshPro (UI文本):
C:\1\server\Library\ScriptAssemblies\Unity.TextMeshPro.dll
```
