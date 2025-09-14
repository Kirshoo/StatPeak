using System;
using HarmonyLib;

namespace StatPeak.Patches;

public class RopePatch
{
    public class RopeStatContext
    {
        [ThreadStatic]
        public static RopeShooter? ropeShooter;

        [ThreadStatic]
        public static bool ropeShooterShot;
    }

    private static void IncrementRopePlacedLength(float amount, bool isAntiRope)
    {
        string StatToIncrement = isAntiRope ? Stat.AntiropePlaced : Stat.RopePlaced;

        Plugin.Logger.LogDebug($"Incrementing '{StatToIncrement}' by {amount}");
        PlayerStats.Increment(StatToIncrement, amount);
    }

    [HarmonyPatch(typeof(AchievementManager), nameof(AchievementManager.AddToRunBasedFloat))]
    [HarmonyPrefix]
    public static void SetSuccessfulShot(RUNBASEDVALUETYPE type)
    {
        if (RopeStatContext.ropeShooter == null || type != RUNBASEDVALUETYPE.RopePlaced)
        {
            return;
        }

        RopeStatContext.ropeShooterShot = true;
    }

    [HarmonyPatch(typeof(RopeShooter), nameof(RopeShooter.OnPrimaryFinishedCast))]
    [HarmonyPrefix]
    public static void InitiateRopeShooterShot(RopeShooter __instance)
    {
        // Ignore non-local player rope shooters
        if (!__instance.photonView.IsMine)
        {
            Plugin.Logger.LogDebug("Someone else shot the rope cannon!");
            return;
        }

        RopeStatContext.ropeShooter = __instance;
    }

    [HarmonyPatch(typeof(RopeShooter), nameof(RopeShooter.OnPrimaryFinishedCast))]
    [HarmonyPostfix]
    public static void DisposeOfInstanceReference()
    {
        RopeStatContext.ropeShooter = null;
    }

    // Because SpawnRope is called via RPC, we cannot dispose of ropeShooter after its being shot.
    // This is a workaround for the time being.
    [HarmonyPatch(typeof(RopeAnchorWithRope), nameof(RopeAnchorWithRope.SpawnRope))]
    [HarmonyPostfix]
    public static void SpawnedRope(RopeAnchorWithRope __instance, Rope __result)
    {
        // Make sure we dont assign instance of stray ropes that lay on the map
        if (!RopeStatContext.ropeShooterShot)
        {
            Plugin.Logger.LogDebug("Spawning the rope before rope shooter was instantiated.");
            return;
        }

        IncrementRopePlacedLength(Rope.GetLengthInMeters(__instance.ropeSegmentLength), __result.antigrav);
        RopeStatContext.ropeShooterShot = false;
    }

    // Called by both Rope Cannon and Rope Spool.
    // Unlike Rope Spool, Rope Cannon calls with length 0f.
    // It could be explained by RopeAnchorWithRope.SpawnRope.SpoolOut, which changes length of segements (from 0f to 20f over period of time)
    [HarmonyPatch(typeof(Rope), nameof(Rope.AttachToAnchor_Rpc))]
    [HarmonyPostfix]
    public static void IncrementRopeSpoolPlaced(Rope __instance)
    {
        if (!__instance.view.IsMine)
        {
            Plugin.Logger.LogDebug("Non-local player's spool was placed.");
            return;
        }

        // Temporary check to avoid incrementing on rope cannon usage
        if (RopeStatContext.ropeShooterShot)
        {
            Plugin.Logger.LogDebug("AttachToAnchor: Rope Spool placed by rope cannon shot. Ignoring...");
            return;
        }

        IncrementRopePlacedLength(__instance.GetLengthInMeters(), __instance.antigrav);

        string RopePlaced = __instance.antigrav ? ItemName.AntiropeSpool : ItemName.RopeSpool;
        Plugin.Logger.LogDebug($"Rope spool placed. Incrementing '{RopePlaced}'");
        PlayerStats.Increment(RopePlaced);
    }
}
