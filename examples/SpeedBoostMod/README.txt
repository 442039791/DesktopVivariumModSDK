# 生态箱加速模组

这是一个使用 Harmony 补丁的 DLL 模组示例，演示如何直接修改游戏代码。

## 功能

通过 Harmony Prefix 补丁修改 `BioTankManager.StateUpdate` 方法，将传入的 `IntervalTime` 参数扩大60倍，从而实现生态箱60倍加速效果。

## 工作原理

```csharp
// Harmony Prefix 补丁 - 在原方法执行前修改参数
public static void StateUpdatePrefix(ref float IntervalTime)
{
    IntervalTime *= 60.0f;  // 将时间间隔扩大60倍
}
```

## 依赖

- **HarmonyLib 模组** (com.lib.harmony) - 必须先加载此模组

## 模组结构

```
SpeedBoostMod/
├── manifest.json           # 模组配置文件（声明依赖 HarmonyLib）
├── README.txt              # 说明文档
├── Plugins/
│   └── Server/
│       └── SpeedBoostMod.dll   ← 编译后放这里
└── Source/
    └── SpeedBoostMod.cs    # 模组源代码
```

## 如何使用

### 步骤1：确保 HarmonyLib 已启用

在游戏的模组管理器中确保 `Harmony Library` 模组已启用且排在本模组之前。

### 步骤2：编译 DLL

**Visual Studio:**
1. 创建类库项目（.NET Framework 4.8 或 .NET Standard 2.1）
2. 添加引用：
   - `UnityEngine.dll`（从 Unity 安装目录）
   - `UnityEngine.CoreModule.dll`
   - 游戏的 `Assembly-CSharp.dll`
3. 复制 `Source/SpeedBoostMod.cs` 到项目中
4. 编译生成 `SpeedBoostMod.dll`

**命令行（需要安装 .NET SDK）:**
```bash
csc /target:library /out:SpeedBoostMod.dll \
    /reference:UnityEngine.dll \
    /reference:UnityEngine.CoreModule.dll \
    /reference:Assembly-CSharp.dll \
    Source/SpeedBoostMod.cs
```

### 步骤3：安装 DLL

1. 创建 `Plugins/Server/` 目录
2. 将编译好的 `SpeedBoostMod.dll` 放入该目录

### 步骤4：更新 manifest.json

编译完成后，需要在 manifest.json 中添加 Scripts 配置：

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

## 修改加速倍率

编辑 `Source/SpeedBoostMod.cs` 中的常量：

```csharp
public static float SpeedMultiplier = 60.0f;  // 改成你想要的倍率
```

示例值：
- `30.0f` - 30倍速
- `60.0f` - 60倍速（默认）
- `120.0f` - 120倍速

## 与旧版本的区别

旧版本通过调用 `SetBioTankSpeedMultiplier` API 实现加速，新版本直接使用 Harmony 修改游戏代码：

| 特性 | 旧版本 (API) | 新版本 (Harmony) |
|------|-------------|-----------------|
| 实现方式 | 调用游戏提供的 API | 直接修补游戏方法 |
| 灵活性 | 受限于 API 功能 | 可修改任意代码 |
| 依赖 | 无 | 需要 HarmonyLib |
| 学习价值 | 基础模组开发 | 高级模组开发 |

## 注意事项

1. 必须先加载 HarmonyLib 模组
2. 高倍速可能影响游戏性能
3. 资源消耗也会加快，注意及时补充
4. 禁用模组后会自动移除补丁，恢复正常速度
5. 建议在测试环境使用
