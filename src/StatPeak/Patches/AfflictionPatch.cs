using System;
using HarmonyLib;

namespace StatPeak.Patches;

public class AfflictionPatch
{
    private static CharacterAfflictions.STATUSTYPE lastAffliction = CharacterAfflictions.STATUSTYPE.Hunger;
    private static float AccumulatedAmount = 0;
    private static DateTime lastLogTime = DateTime.MinValue;

    [HarmonyPatch(typeof(CharacterAfflictions), nameof(CharacterAfflictions.AddStatus))]
    [HarmonyPostfix]
    public static void IncrementStatus(CharacterAfflictions __instance, bool __result, CharacterAfflictions.STATUSTYPE statusType, float amount)
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
        PlayerStats.Increment(statusType.ToString(), amount);

        if (lastAffliction != statusType || DateTime.Now > lastLogTime.AddMinutes(1))
        {
            // Only log when new affliction type has called AddStatus
            // or when you are accumulating for more than 1 minute
            //
            // Otherwise, will flood console with lots of "Added X.XXXXXX of hunger"
            Plugin.Logger.LogDebug($"Adding {AccumulatedAmount} of {lastAffliction}");

            lastLogTime = DateTime.Now;
            AccumulatedAmount = 0;
            lastAffliction = statusType;
        }
    }
}
