using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;

namespace StatPeak;

[BepInAutoPlugin]
public partial class Plugin : BaseUnityPlugin
{
    internal static ManualLogSource Log { get; private set; } = null!;

    internal static ConfigEntry<string> RemoteServerBaseURL { get; private set; }
    internal static ConfigEntry<string> RemoteServerBasePath { get; private set; }
    internal static ConfigEntry<bool> RemoteServerHttpsProtocol { get; private set; }

    private void Awake()
    {
        Log = base.Logger;
        Log.LogInfo($"Plugin {Name} is loaded!");

        RemoteServerBaseURL = Config.Bind(
            "RemoteServer",
            "BaseURL",
            "api.hactix.dev",
            "Base URL string of the remote server to which POST requests with stats will be sent to."
        );

        RemoteServerBasePath = Config.Bind(
            "RemoteServer",
            "BasePath",
            "/statpeak/v1",
            "Base path to endpoints on remote server. Has to start with '/'"
        );

        RemoteServerHttpsProtocol = Config.Bind(
            "RemoteServer",
            "UsesHttpsProtocol",
            true,
            "Toggle to specify whether server communicates with HTTP or HTTPS protocol. 'true' signifies HTTPS, 'false' - HTTP"
        );
    }
}
