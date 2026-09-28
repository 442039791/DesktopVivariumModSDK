using System;

/// <summary>
/// 最小 DLL 模组模板。
/// 注意：
/// 1. ModId 必须与 manifest.json 中的 Id 完全一致
/// 2. 不要在 Initialize 里做重活，真正的功能放到 OnEnable
/// </summary>
public class MyFirstMod : IModEntry
{
    private IModContext _context;

    public string ModId => "com.yourname.myfirstmod";
    public string ModName => "我的第一个模组";
    public string Version => "1.0.0";

    public void Initialize(IModContext context)
    {
        // 只做轻量初始化：缓存上下文
        _context = context;
    }

    public void OnEnable()
    {
        // 真正的功能在这里启用
        _context.Logger.Log("Hello DesktopVivarium! 我的第一个模组已启用");

        // 示例：把游戏速度设为 2 倍
        // _context.GameServices.SetBioTankSpeedMultiplier(2.0f);
    }

    public void OnDisable()
    {
        // 与 OnEnable 对称：取消订阅、撤销修改
        _context.Logger.Log("我的第一个模组已禁用");
    }

    public void OnUnload()
    {
        // 清理资源
    }

    public void OnUpdate()
    {
        // 每帧调用：只放必须逐帧处理的逻辑，通常留空
    }
}
