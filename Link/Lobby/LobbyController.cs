using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json.Nodes;

using PCL.Core.Link.EasyTier;
using PCL.Core.Logging;
using PCL.Core.Utils.Secret;
using PCL.Core.Net;
using static PCL.Core.Link.Natayark.NatayarkProfileManager;
using static PCL.Core.Link.Lobby.LobbyInfoProvider;
using static PCL.Core.Link.EasyTier.ETInfoProvider;
using System.Threading.Tasks;
using PCL.Core.App;
using PCL.Core.Utils.OS;

namespace PCL.Core.Link.Lobby;

public static class LobbyController
{
    public static int Launch(bool isHost, string? playerName = null)
    {
        if (TargetLobby == null) { return 1; }
        LogWrapper.Info("Link", "联机数据发送行为已被移除。");

        var etResult = ETController.Launch(isHost, hostname: playerName);
        if (etResult == 1)
        {
            return 1;
        }
            
        while (CheckETStatusAsync().GetAwaiter().GetResult() != 0)
        {
            Task.Delay(800).GetAwaiter().GetResult();
        }

        if (isHost || TargetLobby.Ip is null) return 0;
        string desc;
        var hostInfo = GetPlayerList().Item1?[0];
        if (hostInfo == null)
        {
            desc = string.Empty;
        }
        else
        {
            desc = " - " + (hostInfo.Username ?? hostInfo.Hostname);
        }

        var tcpPortForForward = NetworkHelper.NewTcpPort();
        McForward = new TcpForward(IPAddress.Loopback, tcpPortForForward, IPAddress.Loopback,
            JoinerLocalPort);
        McBroadcast = new Broadcast($"§ePCL CE 大厅{desc}", tcpPortForForward);
        McForward.Start();
        McBroadcast.Start();
        
        return 0;
    }

    /// <summary>
    /// 检查主机的 MC 实例是否可用。
    /// </summary>
    public static bool IsHostInstanceAvailable(int port)
    {
        var ping = new McPing("127.0.0.1", port);
        var info = ping.PingAsync().GetAwaiter().GetResult();
        if (info != null) return true;
        LogWrapper.Warn("Link", $"本地 MC 局域网实例 ({port}) 疑似已关闭");
        return false;
    }

    /// <summary>
    /// 退出大厅。这将同时关闭 EasyTier 和 MC 端口转发，需要自行清理 UI。
    /// </summary>
    public static int Close()
    {
        TargetLobby = null;
        ETController.Exit();
        McForward?.Stop();
        McBroadcast?.Stop();
        return 0;
    }
}
