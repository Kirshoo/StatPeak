using HarmonyLib;

namespace StatPeak.Patches;

public class ItemUsePatch
{
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
}
