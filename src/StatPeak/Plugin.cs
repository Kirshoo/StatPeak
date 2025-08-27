using System;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;

namespace StatPeak;

[BepInAutoPlugin]
public partial class Plugin : BaseUnityPlugin
{
    internal new static ManualLogSource Logger;
    private readonly Harmony _harmony = new(Id);

    internal static ConfigEntry<string> RemoteServerBaseURL { get; private set; }
    internal static ConfigEntry<string> RemoteServerBasePath { get; private set; }
    internal static ConfigEntry<bool> RemoteServerHttpsProtocol { get; private set; }

    private void Awake()
    {
        Logger = base.Logger;
        Logger.LogInfo($"Plugin {Name} is loaded!");

        _harmony.PatchAll(typeof(Plugin.AfflictionPatch));
        Logger.LogInfo($"All affliction patches applied successfully.");

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

    public class AfflictionPatch
    {
        private static CharacterAfflictions.STATUSTYPE lastAffliction = CharacterAfflictions.STATUSTYPE.Hunger;
        private static float AccumulatedAmount = 0;
        private static DateTime lastLogTime = DateTime.MinValue;

        [HarmonyPatch(typeof(CharacterAfflictions), nameof(CharacterAfflictions.AddStatus))]
        [HarmonyPostfix]
        public static void IncrementStatus(ref CharacterAfflictions __instance, ref bool __result, CharacterAfflictions.STATUSTYPE statusType, float amount)
        {
            if (!__result)
            {
                // Ignore calls to AddStatus if they didn't add status to character
                return;
            }

            if (!__instance.character.IsLocal)
            {
                // Ignore calls that are not about local player
                return;
            }

            // Add before reseting to not lose information about last amount
            AccumulatedAmount += amount;
            PlayerStats.Increment(statusType.ToString().ToLower(), amount);

            if (lastAffliction != statusType || DateTime.Now > lastLogTime.AddMinutes(1))
            {
                // Only log when new affliction type has called AddStatus
                // or when you are accumulating for more than 1 minute
                //
                // Otherwise, will flood console with lots of "Added X.XXXXXX of hunger"
                Plugin.Logger.LogDebug($"Adding {AccumulatedAmount} of {lastAffliction.ToString().ToLower()}");

                lastLogTime = DateTime.Now;
                AccumulatedAmount = 0;
                lastAffliction = statusType;
            }
        }
    }

}
