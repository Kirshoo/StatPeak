using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

namespace StatPeak.Patches;

public class ItemUsePatch
{
    private static bool IsItemNameEqual(Item item, string toCompare)
    {
        return LocalizedText.GetNameIndex(item.UIData.itemName).ToUpperInvariant() == toCompare;
    }

    [HarmonyPatch(typeof(ScoutCannonFuse), nameof(ScoutCannonFuse.Interact_CastFinished))]
    [HarmonyPostfix]
    public static void IncrementCannonUses(Character interactor)
    {
        // Ignore non local character
        if (!interactor.IsLocal)
        {
            return;
        }

        Plugin.Logger.LogDebug("Local player lit the fuse. Incrementing scout cannon uses.");
        PlayerStats.Increment(ItemName.ScoutCannon);
    }

    #region Magic Bean Patches

    // Because GrowVineRPC is called twice, the OnMagicBeanGrow is called twice
    // This serves as a way to fix this issue and disable double copy of Vines

    [HarmonyPatch(typeof(MagicBean), nameof(MagicBean.Update))]
    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> FixDoubleVineCreation(IEnumerable<CodeInstruction> instructions)
    {
        int GrowVineCallStart = -1;
        int GrowVineCallEnd = -1;

        List<CodeInstruction> code = new List<CodeInstruction>(instructions);

        for (int i = 0; i < code.Count; i++)
        {
            Plugin.Logger.LogDebug($"{i}: {code[i]}");

            if (GrowVineCallEnd == -1 && code[i].opcode == OpCodes.Call &&
                (MethodInfo)code[i].operand == AccessTools.Method(typeof(MagicBean), nameof(MagicBean.GrowVineRPC)))
            {
                Plugin.Logger.LogDebug($"Found call to GrowVineRPC at line {i}");
                GrowVineCallEnd = i;
            }

            if (GrowVineCallStart == -1 && 
                (code[i].opcode == OpCodes.Call || code[i].opcode == OpCodes.Callvirt) &&
                (MethodInfo)code[i].operand == AccessTools.Method(typeof(Photon.Pun.PhotonView), nameof(Photon.Pun.PhotonView.RPC), new[] { typeof(string), typeof(Photon.Pun.RpcTarget), typeof(object[]) }))
            {
                Plugin.Logger.LogDebug($"Found RPC call to for GrowVineRPC at line {i + 1}");
                GrowVineCallStart = i + 1;
            }
        }

        if (GrowVineCallStart < 0 || GrowVineCallEnd < 0)
        {
            Plugin.Logger.LogDebug($"Unable to find second RPC call. ({GrowVineCallStart} : {GrowVineCallEnd}) Aborting transpilation...");
            return instructions;
        }

        code.RemoveRange(GrowVineCallStart, GrowVineCallEnd - GrowVineCallStart + 1);

        Plugin.Logger.LogDebug("--- UPDATED INSTRUCTIONS ---");
        for (int i = 0; i < code.Count; i++)
        {
            Plugin.Logger.LogDebug($"{i}: {code[i]}");
        }

        return code;
    }

    [HarmonyPatch(typeof(MagicBean), nameof(MagicBean.GrowVineRPC))]
    [HarmonyPostfix]
    public static void OnMagicBeanGrow(MagicBean __instance)
    {
        if (__instance.item.lastThrownCharacter == null || !__instance.item.lastThrownCharacter.IsLocal)
        {
            return;
        }

        Plugin.Logger.LogDebug($"Magic bean, last held by local player, started to grow");
        PlayerStats.Increment(ItemName.MagicBean);
    }

    #endregion

    [HarmonyPatch(typeof(ShelfShroom), nameof(ShelfShroom.Break))]
    [HarmonyPostfix]
    public static void OnThrowShroomActivation(ShelfShroom __instance)
    {
        if (__instance.item.lastThrownCharacter == null || !__instance.item.lastThrownCharacter.IsLocal) 
        { 
            return; 
        }

        Plugin.Logger.LogDebug($"{__instance.item.GetName()}, last held by local player, got activated");
        PlayerStats.Increment(LocalizedText.GetNameIndex(__instance.item.UIData.itemName).ToUpperInvariant());
    }

    #region Bugle Patches

    private class BugleContext
    {
        [ThreadStatic]
        public static float StartedAt = 0f;

        [ThreadStatic]
        public static BugleSFX? bugleInstance;
    }

    [HarmonyPatch(typeof(BugleSFX), nameof(BugleSFX.RPC_StartToot))]
    [HarmonyPostfix]
    public static void SetTootingTime(BugleSFX __instance)
    {
        if (!__instance.item.holderCharacter.IsLocal)
        {
            return;
        }

        BugleContext.bugleInstance = __instance;
        BugleContext.StartedAt = Time.time;

        Plugin.Logger.LogDebug($"{__instance.item.GetName()}, currently held by local player, started tooting");
        PlayerStats.Increment(LocalizedText.GetNameIndex(__instance.item.UIData.itemName).ToUpperInvariant());
    }

    [HarmonyPatch(typeof(BugleSFX), nameof(BugleSFX.RPC_EndToot))]
    [HarmonyPostfix]
    public static void RecordTootingDuration(BugleSFX __instance)
    {
        if (!__instance.item.holderCharacter.IsLocal || __instance != BugleContext.bugleInstance)
        {
            return;
        }

        float stopTime = Time.time;
        Plugin.Logger.LogDebug($"{__instance.item.GetName()}, currently held by local player, stopped tooting. Incrementing duration of tooting by '{stopTime - BugleContext.StartedAt}'");

        string statToIncrement = IsItemNameEqual(__instance.item, ItemName.Bugle) ? Stat.BugleTootDuration : Stat.MagicBugleTootDuration;
        PlayerStats.Increment(statToIncrement, stopTime - BugleContext.StartedAt);
    }

    #endregion

    #region Piton Patch

    private class PitonContext
    {
        [ThreadStatic]
        public static CharacterItems? pitonInstance;
    }

    [HarmonyPatch(typeof(CharacterItems), nameof(CharacterItems.HammerClimbingSpike))]
    [HarmonyPrefix]
    public static void SetContextInstance(CharacterItems __instance)
    {
        if (!__instance.character.IsLocal) { return; }

        PitonContext.pitonInstance = __instance;
    }

    [HarmonyPatch(typeof(AchievementManager), nameof(AchievementManager.IncrementSteamStat))]
    [HarmonyPostfix]
    public static void IncrementPitonUsed(STEAMSTATTYPE steamStatType)
    {
        if (steamStatType != STEAMSTATTYPE.PitonsPlaced || PitonContext.pitonInstance == null) { return; }

        PlayerStats.Increment(ItemName.Piton);
    }

    [HarmonyPatch(typeof(CharacterItems), nameof(CharacterItems.HammerClimbingSpike))]
    [HarmonyPostfix]
    public static void RemoveInstanceFromContext()
    {
        PitonContext.pitonInstance = null;
    }

    #endregion

    [HarmonyPatch(typeof(Lantern), nameof(Lantern.UpdateFuel))]
    [HarmonyPostfix]
    public static void UpdateLanternUsedDuration(Lantern __instance)
    {
        if (!__instance.lit || !__instance.item.lastHolderCharacter.IsLocal)
        {
            return;
        }

        string lanternDurationStat = IsItemNameEqual(__instance.item, ItemName.Lantern) ? Stat.LanternLitDuration : Stat.FairyLanternLitDuration;
        PlayerStats.Increment(lanternDurationStat, Time.deltaTime);
    }

    [HarmonyPatch(typeof(Item), nameof(Item.FinishCastPrimary))]
    [HarmonyPostfix]
    public static void OnPrimaryUse(Item __instance)
    {
        if (!__instance.lastHolderCharacter.IsLocal || __instance.OnPrimaryFinishedCast == null)
        {
            return;
        }

        // Dont count placements of the cannon
        if (IsItemNameEqual(__instance, ItemName.ScoutCannon))
        {
            return;
        }

        Plugin.Logger.LogDebug($"Local player finished primary action of {__instance.GetName()}");
        PlayerStats.Increment(LocalizedText.GetNameIndex(__instance.UIData.itemName).ToUpperInvariant());
    }

    [HarmonyPatch(typeof(Item), nameof(Item.FinishCastSecondary))]
    [HarmonyPostfix]
    public static void OnSecondaryUse(Item __instance)
    {
        if (!__instance.lastHolderCharacter.IsLocal || __instance.OnSecondaryFinishedCast == null)
        {
            return;
        }

        Plugin.Logger.LogDebug($"Local player finished secondary action of {__instance.GetName()}");
        PlayerStats.Increment(LocalizedText.GetNameIndex(__instance.UIData.itemName).ToUpperInvariant());
    }
}
