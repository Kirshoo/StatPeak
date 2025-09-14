using HarmonyLib;

namespace StatPeak.Patches;

public class PlayerActionPatch
{
    [HarmonyPatch(typeof(GlobalEvents), nameof(GlobalEvents.TriggerLuggageOpened))]
    [HarmonyPostfix]
    public static void IncrementOpenedLuggages(Character character)
    {
        if (!character.IsLocal)
        {
            // Ignore calls that are not about local player
            return;
        }

        Plugin.Logger.LogDebug($"Local player opened luggage, incrementing 'luggages'");
        PlayerStats.Increment(Stat.LuggagesOpened);
    }

    [HarmonyPatch(typeof(Item), nameof(Item.RPC_SetThrownData))]
    [HarmonyPostfix]
    public static void IncrementItemsThrown(Item __instance, int characterID)
    {
        if (!Character.GetCharacterWithPhotonID(characterID, out Character throwCharacter))
        {
            Plugin.Logger.LogError($"Cannot find view by viewID {characterID}");
            return;
        }

        if (!throwCharacter.IsLocal) return;

        Plugin.Logger.LogDebug($"Local player got rid of {__instance.GetItemName()}, incrementing 'items_thrown'...");
        PlayerStats.Increment(Stat.ItemsThrown);
    }

    [HarmonyPatch(typeof(CharacterItems), nameof(CharacterItems.OnPickupAccepted))]
    [HarmonyPostfix]
    public static void IncrementItemsPickedUp(CharacterItems __instance)
    {
        if (!__instance.character.IsLocal) return;

        Plugin.Logger.LogDebug($"Local player picked up an item, incrementing 'items_grabbed'...");
        PlayerStats.Increment(Stat.ItemsGrabbed);
    }

    [HarmonyPatch(typeof(ItemCooking), nameof(ItemCooking.FinishCooking))]
    [HarmonyPostfix]
    public static void IncrementItemsCooked(ItemCooking __instance)
    {
        if (!__instance.item.holderCharacter.IsLocal) return;

        Plugin.Logger.LogDebug($"Local player cooked {__instance.item.GetItemName()}, incrementing 'items_cooked'...");
        PlayerStats.Increment(Stat.ItemsCooked);
    }

    [HarmonyPatch(typeof(GlobalEvents), nameof(GlobalEvents.TriggerItemConsumed))]
    [HarmonyPostfix]
    public static void IncrementEatenItem(Item item, Character character)
    {
        if (!character.IsLocal) return;

        Plugin.Logger.LogDebug($"Local player consumed {item.GetName()}, incrementing '{LocalizedText.GetNameIndex(item.UIData.itemName)}'...");
        PlayerStats.Increment(LocalizedText.GetNameIndex(item.UIData.itemName).ToUpperInvariant());
    }

    [HarmonyPatch(typeof(Bugfix), nameof(Bugfix.Interact))]
    [HarmonyPostfix]
    public static void IncrementTicksPicked(Character interactor)
    {
        if (!interactor.IsLocal)
        {
            return;
        }

        Plugin.Logger.LogDebug($"Local player removed tick! Incrementing '{Stat.TicksRemoved}'...");
        PlayerStats.Increment(Stat.TicksRemoved);
    }

    [HarmonyPatch(typeof(Campfire), nameof(Campfire.Interact_CastFinished))]
    [HarmonyPostfix]
    public static void IncrementCampfiresLit(Campfire __instance, Character interactor)
    {
        if (__instance.Lit || !__instance.EveryoneInRange(out _))
        {
            return;
        }

        if (!interactor.IsLocal)
        {
            return;
        }

        Plugin.Logger.LogDebug($"Local player lit the campfire! Incrementing '{Stat.CampfiresLit}'...");
        PlayerStats.Increment(Stat.CampfiresLit);
    }
}
