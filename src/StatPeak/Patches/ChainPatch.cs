using System;
using UnityEngine;
using Zorro.Core;
using HarmonyLib;

namespace StatPeak.Patches;

public class ChainPatch
{
    internal const int DefaultAmountOfSamples = 50;

    public class ChainStatContext
    {
        [ThreadStatic]
        public static VineShooter? shooterInstance;
    }

    [HarmonyPatch(typeof(VineShooter), nameof(VineShooter.OnPrimaryFinishedCast))]
    [HarmonyPrefix]
    public static void SetShooterInstance(VineShooter __instance)
    {
        if (!__instance.photonView.IsMine) { return; }

        ChainStatContext.shooterInstance = __instance;
    }

    [HarmonyPatch(typeof(VineShooter), nameof(VineShooter.OnPrimaryFinishedCast))]
    [HarmonyPostfix]
    public static void DisposeShooterInstance()
    {
        ChainStatContext.shooterInstance = null;
    }

    private static float CalculateArcLength(Vector3 from, Vector3 mid, Vector3 to, int samples = DefaultAmountOfSamples)
    {
        float length = 0f;
        Vector3 previous = from;

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / (samples - 1);
            Vector3 point = BezierCurve.QuadraticBezier(from, mid, to, t);
            length += Vector3.Distance(previous, point);
            previous = point;
        }

        return length;
    }

    [HarmonyPatch(typeof(JungleVine), nameof(JungleVine.CheckVinePath))]
    [HarmonyPostfix]
    public static void IncrementVineLength(bool __result, Vector3 from, Vector3 to, Vector3 mid)
    {
        // Vine is not valid or chain launcher never shot
        if (!__result || ChainStatContext.shooterInstance == null)
        {
            return;
        }

        // Debug output to compare with results of original function
        // Plugin.Logger.LogDebug($"from: {from}, to: {to}, mid: {mid}, hang: {Vector3.Distance(Vector3.Lerp(from, to, 0.5f), mid)}");

        float valueToAdd = CalculateArcLength(from, to, mid);
        float distanceInMeters = valueToAdd * CharacterStats.unitsToMeters;

        Plugin.Logger.LogDebug($"Incrementing '{Stat.ChainPlaced}' by {distanceInMeters}m");
        PlayerStats.Increment(Stat.ChainPlaced, distanceInMeters);
    }
}
