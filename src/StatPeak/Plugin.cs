using System;
using System.Globalization;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace StatPeak;

public enum Stat
{
    deaths,
    faints,
    revives,
    jumps,
    luggages,
}

[BepInAutoPlugin]
public partial class Plugin : BaseUnityPlugin
{
    internal new static ManualLogSource Logger;
    private readonly Harmony _harmony = new(Id);
    private bool showStats = false;

    internal static ConfigEntry<string> RemoteServerBaseURL { get; private set; }
    internal static ConfigEntry<string> RemoteServerBasePath { get; private set; }
    internal static ConfigEntry<bool> RemoteServerHttpsProtocol { get; private set; }

    private void Awake()
    {
        Logger = base.Logger;
        Logger.LogInfo($"Plugin {Name} is loaded!");

        _harmony.PatchAll(typeof(Plugin.AfflictionPatch));
        Logger.LogInfo($"All affliction patches applied successfully.");

        _harmony.PatchAll(typeof(Plugin.PlayerStatePatch));
        Logger.LogInfo($"All player state patches applied successfully");

        _harmony.PatchAll(typeof(Plugin.RunSpecificPatches));
        Logger.LogInfo($"All run specific patches applied successfully");

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

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F5))
        {
            showStats = !showStats;
        }
    }

    private void OnGUI()
    {
        if (!showStats) return;

        int i = 1;
        foreach (var entry in PlayerStats.GetAll())
        {
            GUI.Label(new Rect(20, 20 * i, 300, 20), string.Format("{0}: {1}", entry.Key, entry.Value.ToString("F4", CultureInfo.InvariantCulture)));
            i++;
        }
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

    public class PlayerStatePatch
    {
        [HarmonyPatch(typeof(Character), nameof(Character.RPCA_Die))]
        [HarmonyPostfix]
        public static void IncrementDeaths(ref Character __instance)
        {
            if (!__instance.IsLocal)
            {
                // Ignore calls that are not about local player
                return;
            }

            Plugin.Logger.LogDebug($"Local player died, incrementing '{Stat.deaths}'");
            PlayerStats.Increment(Stat.deaths.ToString());
        }

        [HarmonyPatch(typeof(Character), nameof(Character.RPCA_Revive))]
        [HarmonyPostfix]
        public static void IncrementRevives(ref Character __instance)
        {
            if (!__instance.IsLocal)
            {
                // Ignore calls that are not about local player
                return;
            }

            Plugin.Logger.LogDebug($"Local player revived, incrementing '{Stat.revives}'");
            PlayerStats.Increment(Stat.revives.ToString());
        }

        [HarmonyPatch(typeof(Character), nameof(Character.RPCA_PassOut))]
        [HarmonyPostfix]
        public static void IncrementFaints(ref Character __instance)
        {
            if (!__instance.IsLocal)
            {
                // Ignore calls that are not about local player
                return;
            }

            Plugin.Logger.LogDebug($"Local player passed out, incrementing '{Stat.faints}'");
            PlayerStats.Increment(Stat.faints.ToString());
        }

        [HarmonyPatch(typeof(Character), nameof(Character.OnJump))]
        [HarmonyPostfix]
        public static void IncrementJumps(ref Character __instance)
        {
            if (!__instance.IsLocal)
            {
                // Ignore calls that are not about local player
                return;
            }

            Plugin.Logger.LogDebug($"Local player jumped, incrementing '{Stat.jumps}'");
            PlayerStats.Increment(Stat.jumps.ToString());
        }
    }

    public class RunSpecificPatches
    {
        [HarmonyPatch(typeof(GlobalEvents), nameof(GlobalEvents.TriggerLuggageOpened))]
        [HarmonyPostfix]
        public static void IncrementOpenedLuggages(ref Character character)
        {
            if (!character.IsLocal)
            {
                // Ignore calls that are not about local player
                return;
            }

            Plugin.Logger.LogDebug($"Local player opened luggage, incrementing {Stat.luggages}");
            PlayerStats.Increment(Stat.luggages.ToString());
        }
    }
}
