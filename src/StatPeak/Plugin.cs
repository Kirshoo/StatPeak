using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;

namespace StatPeak;

[BepInAutoPlugin]
public partial class Plugin : BaseUnityPlugin
{
    internal static ManualLogSource Log { get; private set; } = null!;

    private void Awake()
    {
        Log = Logger;
        Log.LogInfo($"Plugin {Name} is loaded!");
    }
}
