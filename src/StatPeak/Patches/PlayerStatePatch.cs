using System;
using HarmonyLib;

namespace StatPeak.Patches;

public class PlayerStatePatch
{
    [ThreadStatic]
    private static bool EndGameStatusTracked = false;

    [HarmonyPatch(typeof(Character), nameof(Character.RPCA_Die))]
    // Has to be Prefix because it calls RunManager.Instance.EndGame, which sends all the stats to server
    // and information about death is lost since it runs AFTER the data is sent.
    [HarmonyPrefix]
    public static void IncrementDeaths(Character __instance)
    {
        if (!__instance.IsLocal)
        {
            // Ignore calls that are not about local player
            return;
        }

        Plugin.Logger.LogDebug($"Local player died, incrementing '{Stat.TotalDeaths}'");
        PlayerStats.Increment(Stat.TotalDeaths);
    }

    [HarmonyPatch(typeof(Character), nameof(Character.RPCA_Revive))]
    [HarmonyPostfix]
    public static void IncrementRevives(Character __instance)
    {
        if (!__instance.IsLocal)
        {
            // Ignore calls that are not about local player
            return;
        }

        Plugin.Logger.LogDebug($"Local player revived, incrementing '{Stat.TotalRevives}'");
        PlayerStats.Increment(Stat.TotalRevives);
    }

    [HarmonyPatch(typeof(Character), nameof(Character.RPCA_PassOut))]
    [HarmonyPostfix]
    public static void IncrementFaints(Character __instance)
    {
        if (!__instance.IsLocal)
        {
            // Ignore calls that are not about local player
            return;
        }

        Plugin.Logger.LogDebug($"Local player passed out, incrementing '{Stat.TotalFaints}'");
        PlayerStats.Increment(Stat.TotalFaints);
    }

    [HarmonyPatch(typeof(Character), nameof(Character.OnJump))]
    [HarmonyPostfix]
    public static void IncrementJumps(Character __instance)
    {
        if (!__instance.IsLocal)
        {
            // Ignore calls that are not about local player
            return;
        }

        Plugin.Logger.LogDebug($"Local player jumped, incrementing '{Stat.TotalJumps}'");
        PlayerStats.Increment(Stat.TotalJumps);
    }

    [HarmonyPatch(typeof(RunManager), nameof(RunManager.StartRun))]
    [HarmonyPostfix]
    public static void OnRunStart()
    {
        EndGameStatusTracked = false;
    }

    [HarmonyPatch(typeof(Character), nameof(Character.CheckWinCondition))]
    [HarmonyPrefix]
    public static void IncrementEndGameStatus(bool __result, Character c)
    {
        if (!c.IsLocal || EndGameStatusTracked) { return; }

        string statToIncrement = __result ? Stat.HasEscaped : Stat.HasFailed;
        PlayerStats.Increment(statToIncrement);

        EndGameStatusTracked = true;
    }
}
