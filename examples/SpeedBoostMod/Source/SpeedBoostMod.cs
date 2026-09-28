using System;
using System.Reflection;
using UnityEngine;

/// <summary>
/// 生态箱加速模组 - 使用 Harmony 将生态箱运行速度加快60倍
///
/// 原理：通过 Harmony Prefix 补丁修改 BioTankManager.StateUpdate 方法的 IntervalTime 参数
/// 将传入的时间间隔扩大60倍，从而实现加速效果
///
/// 依赖：需要先加载 HarmonyLib 模组（com.lib.harmony）
/// </summary>
public class SpeedBoostMod : IModEntry
{
    private IModContext _context;
    private IHarmonyInstance _harmony;
    private bool _isEnabled = false;

    /// <summary>
    /// 加速倍率 - 60倍速
    /// </summary>
    public static float SpeedMultiplier = 60.0f;

    public string ModId => "com.example.speedboost";
    public string ModName => "生态箱加速模组";
    public string Version => "1.0.0";

    /// <summary>
    /// 初始化模组
    /// </summary>
    public void Initialize(IModContext context)
    {
        _context = context;
        _context.Logger.Log("生态箱加速模组初始化中...");
    }

    /// <summary>
    /// 启用模组 - 应用 Harmony 补丁
    /// </summary>
    public void OnEnable()
    {
        if (_isEnabled) return;

        // 检查 Harmony 是否可用
        if (!_context.HarmonyManager.IsHarmonyAvailable)
        {
            _context.Logger.LogError("Harmony 不可用！请确保已加载 HarmonyLib 模组（com.lib.harmony）");
            return;
        }

        // 创建 Harmony 实例
        _harmony = _context.HarmonyManager.CreateHarmony(ModId);
        if (_harmony == null)
        {
            _context.Logger.LogError("无法创建 Harmony 实例");
            return;
        }

        // 获取目标方法：BioTankManager.StateUpdate(float)
        var targetType = typeof(BioTankManager);
        var targetMethod = targetType.GetMethod("StateUpdate", BindingFlags.Public | BindingFlags.Instance, null, new Type[] { typeof(float) }, null);

        if (targetMethod == null)
        {
            _context.Logger.LogError("找不到 BioTankManager.StateUpdate 方法");
            return;
        }

        // 获取补丁方法
        var prefixMethod = typeof(SpeedBoostMod).GetMethod("StateUpdatePrefix", BindingFlags.Public | BindingFlags.Static);

        if (prefixMethod == null)
        {
            _context.Logger.LogError("找不到补丁方法 StateUpdatePrefix");
            return;
        }

        // 应用 Prefix 补丁
        _harmony.Prefix(targetMethod, prefixMethod);

        _isEnabled = true;
        _context.Logger.Log($"生态箱加速模组已启用！速度倍率: {SpeedMultiplier}x");
        _context.Logger.Log("通过 Harmony 补丁修改 BioTankManager.StateUpdate 方法");
    }

    /// <summary>
    /// Harmony Prefix 补丁 - 修改 IntervalTime 参数
    /// </summary>
    /// <param name="IntervalTime">原始时间间隔（引用参数，可修改）</param>
    public static void StateUpdatePrefix(ref float IntervalTime)
    {
        // 将时间间隔扩大60倍
        IntervalTime *= SpeedMultiplier;
    }

    /// <summary>
    /// 禁用模组 - 移除 Harmony 补丁
    /// </summary>
    public void OnDisable()
    {
        if (!_isEnabled) return;

        if (_harmony != null)
        {
            _harmony.UnpatchAll();
            _harmony = null;
        }

        _isEnabled = false;
        _context.Logger.Log("生态箱加速模组已禁用，Harmony 补丁已移除");
    }

    /// <summary>
    /// 卸载模组
    /// </summary>
    public void OnUnload()
    {
        if (_isEnabled)
        {
            OnDisable();
        }
        _context.Logger.Log("生态箱加速模组已卸载");
    }

    /// <summary>
    /// 每帧更新（可选）
    /// </summary>
    public void OnUpdate()
    {
        // 此模组不需要每帧更新逻辑
    }
}
