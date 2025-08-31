using System;
using System.Globalization;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace StatPeak;


public static class Stat
{
    public readonly static string TotalDeaths = "Died this run";
    public readonly static string TotalFaints = "Passed out this run";
    public readonly static string TotalRevives = "Been revived this run";
    public readonly static string TotalJumps = "Jumped this run";
    public readonly static string LuggagesOpened = "Luggages opened by you";
    public readonly static string ItemsThrown = "Items thrown by you";
    public readonly static string ItemsGrabbed = "Items picked up by you";
    public readonly static string ItemsCooked = "Items cooked by you";

    public readonly static string DistanceWalked = "Distance traveled (in m)";
    public readonly static string DistanceClimbed = "Distance climbed (in m)";
    public readonly static string DistanceClimbedOnVines = "Distance traveled on vines (in m)";
    public readonly static string DistanceClimbedOnRopes = "Distance climbed on the rope (in m)";
    public readonly static string DistanceWhileAirborne = "Distance while airborne (in m)";
    public readonly static string GreatestContinuousClimb = "Greatest Continuous Climb (in m)";
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
        _harmony.PatchAll(typeof(Plugin.PlayerMovementPatch));
        Logger.LogInfo($"All player state patches applied successfully");

        _harmony.PatchAll(typeof(Plugin.PlayerActionPatch));
        _harmony.PatchAll(typeof(Plugin.RunPatch));
        Logger.LogInfo($"All run specific patches applied successfully");

        _harmony.PatchAll(typeof(Plugin.InitializationPatch));
        Logger.LogInfo($"All initialization patches applied successfully");

        RemoteServerBaseURL = Config.Bind(
            "RemoteServer",
            "BaseURL",
            "api.hacktix.dev",
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

    public class InitializationPatch
    {
        [HarmonyPatch(typeof(GameHandler), nameof(GameHandler.Initialize))]
        [HarmonyPostfix]
        public static void InitializeSessionTicket()
        {
            SteamUtil.Init();
        }
    }

    public class RunPatch
    {
        public readonly static string AIRPORT_SCENE = "Airport";

        [HarmonyPatch(typeof(RunManager), nameof(RunManager.StartRun))]
        [HarmonyPostfix]
        public static void ResetStatistics()
        {
            Plugin.Logger.LogDebug("Run started. Resetting all accumulated statistics...");
            PlayerStats.Reset();
        }

        [HarmonyPatch(typeof(RunManager), nameof(RunManager.EndGame))]
        [HarmonyPostfix]
        public static void SendRunStatistics()
        {
            Plugin.Logger.LogDebug("Run ended. Sending all accumulated statistics to remote server...");
            StatPeakServerUtil.SendStats(PlayerStats.GetAll());
        }

        [HarmonyPatch(typeof(GlobalEvents), nameof(GlobalEvents.TriggerPlayerDisconnected))]
        [HarmonyPostfix]
        public static void SendRunStatisticsOnDisconnect(Photon.Realtime.Player player)
        {
            // Ignore disconnect events from other players
            if (!player.IsLocal) return;

            SendRunStatistics();
        }

        [HarmonyPatch(typeof(PauseMenuMainPage), nameof(PauseMenuMainPage.OnQuitClicked))]
        [HarmonyPostfix]
        public static void SendRunStatisticsOnQuit()
        {
            // Only local player should click this button.
            // If not, something far worse is happening

            // Dont send stats when in airport
            if (SceneManager.GetActiveScene().name == AIRPORT_SCENE)
            {
                Plugin.Logger.LogDebug("Player quit in airport, no data will be sent");
                return;
            }

            SendRunStatistics();
        }
    }

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

    public class PlayerStatePatch
    {
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

            Plugin.Logger.LogDebug($"Local player died, incrementing 'deaths'");
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

            Plugin.Logger.LogDebug($"Local player revived, incrementing 'revives'");
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

            Plugin.Logger.LogDebug($"Local player passed out, incrementing 'faints'");
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

            Plugin.Logger.LogDebug($"Local player jumped, incrementing 'jumps'");
            PlayerStats.Increment(Stat.TotalJumps);
        }
    }

    public class PlayerMovementPatch
    {
        private static Vector3 PreviousPosition = Vector3.zero;

        private static void IncrementWithoutY(string statName, Vector3 updatedPosition, Vector3 previousPosition)
        {
            var tempY = updatedPosition.y;
            var tempPrevY = previousPosition.y;

            updatedPosition.y = 0f;
            previousPosition.y = 0f;
            
            var distance = Vector3.Distance(updatedPosition, previousPosition);
            // Because position uses units rather than meters, we need to convert before incrementing
            var distanceInMeters = distance * CharacterStats.unitsToMeters;

            PlayerStats.Increment(statName, distanceInMeters);

            updatedPosition.y = tempY;
            previousPosition.y = tempPrevY;
        }

        private static void IncrementOnlyWithY(string statName, Vector3 updatedPosition, Vector3 previousPosition)
        {
            var distance = Math.Sqrt(Math.Pow(updatedPosition.y - previousPosition.y, 2));
            // Because position uses units rather than meters, we need to convert before incrementing
            var distanceInMeters = distance * CharacterStats.unitsToMeters;

            PlayerStats.Increment(statName, distanceInMeters);
        }

        private static void IncrementDistanceTraveled(Vector3 currentPosition, Vector3 previousPosition)
        {
            IncrementWithoutY(Stat.DistanceWalked, currentPosition, previousPosition);
        }

        private static void IncrementDistanceClimbed(Vector3 currentPosition, Vector3 previousPosition)
        {
            IncrementOnlyWithY(Stat.DistanceClimbed, currentPosition, previousPosition);
        }

        private static void IncrementDistanceVineClimbed(Vector3 currentPosition, Vector3 previousPosition)
        {
            IncrementWithoutY(Stat.DistanceClimbedOnVines, currentPosition, previousPosition);
        }

        private static void IncrementDistanceInAir(Vector3 currentPosition, Vector3 previousPosition)
        {
            IncrementOnlyWithY(Stat.DistanceWhileAirborne, currentPosition, previousPosition);
        }

        private static void IncrementDistanceRopeClimbed(Vector3 currentPosition, Vector3 previousPosition)
        {
            IncrementOnlyWithY(Stat.DistanceClimbedOnRopes, currentPosition, previousPosition);
        }

        [HarmonyPatch(typeof(CharacterMovement), nameof(CharacterMovement.FixedUpdate))]
        [HarmonyPostfix]
        public static void IncrementCharacterMovement(CharacterMovement __instance)
        {
            if (!__instance.character.IsLocal) return;

            Vector3 currentPosition = __instance.character.Center;

            if (PlayerMovementPatch.PreviousPosition == Vector3.zero)
            {
                PlayerMovementPatch.PreviousPosition = currentPosition;
                return;
            }

            if (__instance.character.data.isGrounded)
            {
                IncrementDistanceTraveled(currentPosition, PlayerMovementPatch.PreviousPosition);
            }
            else if (__instance.character.data.isClimbing)
            {
                IncrementDistanceClimbed(currentPosition, PlayerMovementPatch.PreviousPosition);
            }
            else if (__instance.character.data.isVineClimbing)
            {
                IncrementDistanceVineClimbed(currentPosition, PlayerMovementPatch.PreviousPosition);
            }
            else if (__instance.character.data.isRopeClimbing)
            {
                IncrementDistanceRopeClimbed(currentPosition, PlayerMovementPatch.PreviousPosition);
            }
            else if (!__instance.character.data.isGrounded && !__instance.character.data.isClimbingAnything)
            {
                IncrementDistanceInAir(currentPosition, PlayerMovementPatch.PreviousPosition);
            }

            PreviousPosition = currentPosition;
        }

        [HarmonyPatch(typeof(Character), nameof(Character.UpdateVariablesFixed))]
        [HarmonyPostfix]
        public static void IncrementCharacterGreatestContinuousClimb(Character __instance)
        {
            if (!__instance.IsLocal) { return; }

            if (!__instance.data.isClimbing) { return; }

            if (__instance.data.sinceGrounded > __instance.data.sinceClimb + 1f)
            {
                // This is a debug to show when endurance achivement can be thrown
                Plugin.Logger.LogDebug($"[GreatestContinuousClimb] {{ SinceGrounded: {__instance.data.sinceGrounded} }} > {{ SinceClimb + 1f: {__instance.data.sinceClimb + 1f} }}");
            }

            var currentClimb = __instance.Center.y - __instance.data.lastGroundedHeight;
            var currentClimbInMeters = currentClimb * CharacterStats.unitsToMeters;

            PlayerStats.Set(
                Stat.GreatestContinuousClimb,
                // Only record the biggest of the two!
                Mathf.Max((float)PlayerStats.Get(Stat.GreatestContinuousClimb), currentClimbInMeters)
            );
        }
    }

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

            // Using GetName() should ensure that any staged of cooked item with the same type
            // will increment this stat
            Plugin.Logger.LogDebug($"Local player consumed {item.GetName()}, incrementing '{item.GetName()}'...");
            PlayerStats.Increment(item.GetName());
        }
    }
}
