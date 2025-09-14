using System;
using HarmonyLib;
using UnityEngine;

namespace StatPeak.Patches;

public class FrisbeePatch
{
    public static class FrisbeeStatContext
    {
        [ThreadStatic]
        public static bool ThrownByMe = false;

        [ThreadStatic]
        public static float CurrentDistance = 0;

        [ThreadStatic]
        public static Frisbee? Instance;

        public static bool IsComplete()
        {
            return Instance != null;
        }

        public static void Reset()
        {
            Instance = null;
            CurrentDistance = 0;
            ThrownByMe = false;
        }
    }

    private static void RecordDistanceFlown()
    {
        Plugin.Logger.LogDebug($"Incrementing distance frisbee has flown by {FrisbeeStatContext.CurrentDistance}");
        PlayerStats.Increment(Stat.FrisbeeDistanceFlown, FrisbeeStatContext.CurrentDistance);
    }

    private static void RecordDistanceCaught()
    {
        Plugin.Logger.LogDebug($"Recording distance frisbee has flown before caught: {FrisbeeStatContext.CurrentDistance}");
        PlayerStats.Set(Stat.FrisbeeGreatestThrow, 
            Mathf.Max((float)PlayerStats.Get(Stat.FrisbeeGreatestThrow), FrisbeeStatContext.CurrentDistance)
        );
    }

    [HarmonyPatch(typeof(Frisbee), nameof(Frisbee.OnItemThrown))]
    [HarmonyPostfix]
    public static void OnThrown(Frisbee __instance, Item obj)
    {
        if (FrisbeeStatContext.IsComplete())
        {
            Plugin.Logger.LogWarning("Frisbee thrown, while tracking another one. Ignoring new frisbee...");
            return;
        }

        if (obj != __instance.item)
        {
            return;
        }

        if (__instance.item.lastThrownCharacter == Character.localCharacter)
        {
            FrisbeeStatContext.ThrownByMe = true;
        }

        FrisbeeStatContext.Instance = __instance;
    }

    [HarmonyPatch(typeof(Frisbee), nameof(Frisbee.OnCollisionEnter))]
    [HarmonyPostfix]
    public static void OnCollision(Frisbee __instance)
    {
        if (!FrisbeeStatContext.IsComplete() || !FrisbeeStatContext.ThrownByMe || __instance != FrisbeeStatContext.Instance)
        {
            return;
        }

        RecordDistanceFlown();
        FrisbeeStatContext.Reset();
    }

    [HarmonyPatch(typeof(Frisbee), nameof(Frisbee.TestRequestedItem))]
    [HarmonyPostfix]
    public static void OnPickUp(Frisbee __instance, Character character)
    {
        if (!FrisbeeStatContext.IsComplete() || __instance != FrisbeeStatContext.Instance)
        {
            return;
        }

        if (character.IsLocal)
        {
            RecordDistanceCaught();
        }

        if (FrisbeeStatContext.ThrownByMe)
        {
            RecordDistanceFlown();
        }

        FrisbeeStatContext.Reset();
    }

    [HarmonyPatch(typeof(Frisbee), nameof(Frisbee.FixedUpdate))]
    [HarmonyPostfix]
    public static void UpdateCurrentDistanceFlown(Frisbee __instance)
    {
        if (!FrisbeeStatContext.IsComplete() || __instance != FrisbeeStatContext.Instance)
        {
            return;
        }

        FrisbeeStatContext.CurrentDistance = FrisbeeStatContext.Instance.throwDistance;
    }
}
