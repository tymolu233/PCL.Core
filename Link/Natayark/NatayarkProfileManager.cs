using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using PCL.Core.App;
using PCL.Core.Utils.OS;
using PCL.Core.Net;
using PCL.Core.Logging;
using PCL.Core.UI;

namespace PCL.Core.Link.Natayark;

public class NaidUser
{
    public int Id { get; set; }
    public string? Email { get; set; }
    public string? Username { get; set; }
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    /// <summary>
    /// Natayark ID 状态，1 为正常
    /// </summary>
    public int Status { get; set; }
    public bool IsRealNamed { get; set; }
    public string? LastIp { get; set; }

}

public static class NatayarkProfileManager
{
    private const string LogModule = "Link";

    public static NaidUser NaidProfile { get; private set; } = new();

    private static bool _isGettingData = false;
    public static void GetNaidData(string token, bool isRefresh = false, bool isRetry = false)
        => Task.Run(() => GetNaidDataAsync(token, isRefresh, isRetry));

    public static async Task GetNaidDataAsync(string token, bool isRefresh = false, bool isRetry = false)
    {
        if (_isGettingData) throw new InvalidOperationException("请勿重复操作");
        _isGettingData = true;
        
        // 移除 Natayark 登录，直接填充假数据
        NaidProfile = new NaidUser
        {
            Id = 114514,
            Username = "LocalPlayer",
            Email = "local@pcl2.dev",
            Status = 1,
            IsRealNamed = true, // 始终为 true 以绕过实名检查
            LastIp = "127.0.0.1",
            AccessToken = "LocalToken",
            RefreshToken = "LocalToken"
        };
        
        Config.Link.NaidRefreshToken = "LocalToken";
        Config.Link.NaidRefreshExpireTime = "9999-12-31T23:59:59Z";
        
        LogWrapper.Info(LogModule, "已移除 Natayark 登录，使用本地数据。");
        
        await Task.Delay(100); // 模拟网络延迟
        
        _isGettingData = false;
    }
}
