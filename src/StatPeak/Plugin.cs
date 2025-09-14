using System;
using System.Globalization;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zorro.Core;

namespace StatPeak;


public class Stat
{
    public const string TotalDeaths = "Died this run";
    public const string TotalFaints = "Passed out this run";
    public const string TotalRevives = "Been revived this run";
    public const string TotalJumps = "Jumped this run";
    public const string HasEscaped = "You have escaped";
    public const string HasFailed = "You have failed to escaped";

    public const string LuggagesOpened = "Luggages opened by you";
    public const string ItemsThrown = "Items thrown by you";
    public const string ItemsGrabbed = "Items picked up by you";
    public const string ItemsCooked = "Items cooked by you";
    public const string TicksRemoved = "Ticks removed by you";
    public const string CampfiresLit = "Campfires lit by you";

    public const string DistanceWalked = "Distance traveled (in m)";
    public const string DistanceClimbed = "Distance climbed (in m)";
    public const string DistanceClimbedOnVines = "Distance traveled on vines (in m)";
    public const string DistanceClimbedOnRopes = "Distance climbed on the rope (in m)";
    public const string DistanceWhileAirborne = "Distance while airborne (in m)";
    public const string GreatestContinuousClimb = "Greatest Continuous Climb (in m)";

    public const string RopePlaced = "Rope placed (in m)";
    public const string AntiropePlaced = "Antirope placed (in m)";
    public const string ChainPlaced = "Chain length placed (in m)";
}

public class ItemName
{
    // Crispberries
    public const string GreenCrispberry = "NAME_GREEN CRISPBERRY";
    public const string RedCrispberry = "NAME_RED CRISPBERRY";
    public const string YellowCrispberry = "NAME_YELLOW CRISPBERRY";

    // Clusterberries
    public const string BlackClusterberry = "NAME_BLACK CLUSTERBERRY";
    public const string RedClusterberry = "NAME_RED CLUSTERBERRY";
    public const string YellowClusterberry = "NAME_YELLOW CLUSTERBERRY";
    public const string GreenClusterberry = "NAME_GREEN CLUSTERBERRY";

    // Berrynanas
    public const string BlueBerrynana = "NAME_BLUE BERRYNANA";
    public const string BrownBerrynana = "NAME_BROWN BERRYNANA";
    public const string PinkBerrynana = "NAME_PINK BERRYNANA";
    public const string YellowBerrynana = "NAME_YELLOW BERRYNANA";
    public const string BerrynanaPeel = "NAME_BERRYNANA PEEL";

    // Kingberries
    public const string GreenKingberry = "NAME_GREEN KINGBERRY";
    public const string PurpleKingberry = "NAME_PURPLE KINGBERRY";
    public const string YellowKingberry = "NAME_YELLOW KINGBERRY";

    // Winterberries
    public const string OrangeWinterberry = "NAME_ORANGE WINTERBERRY";
    public const string YellowWinterberry = "NAME_YELLOW WINTERBERRY";

    // Mushrooms
    public const string ChubbyShroom = "NAME_CHUBBY SHROOM";
    public const string ClusterShroom = "NAME_CLUSTER SHROOM";
    public const string WeirdShroom = "NAME_WEIRD SHROOM";
    public const string BugleShroom = "NAME_BUGLE SHROOM";
    public const string ButtonShroom = "NAME_BUTTON SHROOM";

    // Special mushrooms
    public const string RemedyFungus = "NAME_REMEDY FUNGUS";
    public const string ShelfFungus = "NAME_SHELF FUNGUS";
    public const string BounceFungus = "NAME_BOUNCE FUNGUS";
    public const string MagicBean = "NAME_MAGIC BEAN";

    // Rope related
    public const string ChainLauncher = "NAME_CHAIN LAUNCHER";
    public const string RopeCannon = "NAME_ROPE CANNON";
    public const string AntiropeCannon = "NAME_ANTI-ROPE CANNON";
    public const string RopeSpool = "NAME_ROPE SPOOL";
    public const string AntiropeSpool = "NAME_ANTI-ROPE SPOOL";

    // Area specific items
    public const string Beehive = "NAME_BEEHIVE";
    public const string Honeycomb = "NAME_HONEYCOMB";
    public const string Coconut = "NAME_COCONUT";
    public const string CoconutHalf = "NAME_COCONUT HALF";
    public const string Scorchberry = "NAME_SCORCHBERRY";

    // Packaged food
    public const string AirlineFood = "NAME_AIRLINE FOOD";
    public const string EnergyDrink = "NAME_ENERGY DRINK";
    public const string GranolaBar = "NAME_GRANOLA BAR";
    public const string BigLollipop = "NAME_BIG LOLLIPOP";
    public const string ScoutCookies = "NAME_SCOUT COOKIES";
    public const string SportsDrink = "NAME_SPORTS DRINK";
    public const string TrailMix = "NAME_TRAIL MIX";

    // Mystical items
    public const string CureAll = "NAME_CURE-ALL";
    public const string PandorasLunchbox = "NAME_PANDORA'S LUNCHBOX";
    public const string ScoutEffigy = "NAME_SCOUT EFFIGY";
    public const string CursedSkull = "NAME_CURSED SKULL";

    // Healing items
    public const string Bandages = "NAME_BANDAGES";
    public const string Antidote = "NAME_ANTIDOTE";
    public const string MedicinalRoot = "NAME_MEDICINAL ROOT";
    public const string FirstAidKit = "NAME_FIRST AID KIT";

    // Misc. items
    public const string Backpack = "NAME_BACKPACK";
    public const string BingBong = "NAME_BING BONG";
    public const string Binoculars = "NAME_BINOCULARS";
    public const string Tick = "NAME_TICK";
    public const string Bugle = "NAME_BUGLE";
    public const string FriendshipBugle = "NAME_BUGLE OF FRIENDSHIP";
    public const string ScoutMasterBugle = "NAME_SCOUTMASTER'S BUGLE";
    public const string Piton = "NAME_PITON";
    public const string Compass = "NAME_COMPASS";
    public const string Egg = "NAME_EGG";
    public const string Stick = "NAME_STICK";
    public const string Flare = "NAME_FLARE";
    public const string GuideBook = "NAME_GUIDEBOOK";
    public const string TornPage = "NAME_TORN PAGE";
    public const string Scroll = "NAME_SCROLL";
    public const string Blowgun = "NAME_BLOWGUN";
    public const string HeatPack = "NAME_HEAT PACK";
    public const string Lantern = "NAME_LANTERN";
    public const string FaerieLantern = "NAME_FAERIE LANTERN";
    public const string Marshmallow = "NAME_MARSHMALLOW";
    public const string Megaphone = "NAME_MEGAPHONE";
    public const string Napberry = "NAME_NAPBERRY";
    public const string BigEgg = "NAME_BIG EGG";
    public const string Passport = "NAME_PASSPORT";
    public const string PiratesCompass = "NAME_PIRATE'S COMPASS";
    public const string PortableStove = "NAME_PORTABLE STOVE";
    public const string Conch = "NAME_CONCH";
    public const string Stone = "NAME_STONE";
    public const string WarpCompass = "NAME_WARP COMPASS";
    public const string FlyingDisc = "NAME_FLYING DISC";
    public const string Parasol = "NAME_PARASOL";
    public const string RedPrickleberry = "NAME_RED PRICKLEBERRY";
    public const string GoldPrickleberry = "NAME_GOLD PRICKLEBERRY";
    public const string Sunscreen = "NAME_SUNSCREEN";
    public const string AloeVera = "NAME_ALOE VERA";
    public const string Cactus = "NAME_CACTUS";
    public const string Balloon = "NAME_BALLOON";
    public const string ScoutCannon = "NAME_SCOUT CANNON";
    public const string Torch = "NAME_TORCH";
    public const string BalloonBunch = "NAME_BALLOON BUNCH";
    public const string AncientIdol = "NAME_ANCIENT IDOL";
    public const string Dynamite = "NAME_DYNAMITE";
    public const string Scorpion = "NAME_SCORPION";
    public const string Bird = "NAME_BIRD";
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
        _harmony.PatchAll(typeof(Plugin.RopePatches));
        _harmony.PatchAll(typeof(Plugin.ChainPatches));
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

    #region Communication with Remote server

    public class RunPatch
    {
        public readonly static string AIRPORT_SCENE = "Airport";

        private static bool IsInAirport()
        {
            return SceneManager.GetActiveScene().name == AIRPORT_SCENE;
        }

        private static void ResetStatistics()
        {
            Plugin.Logger.LogDebug("Resetting all accumulated statistics...");
            PlayerStats.Reset();
        }

        private static void SendRunStatistics()
        {
            Plugin.Logger.LogDebug("Sending all accumulated statistics to remote server...");
            StatPeakServerUtil.SendStats(PlayerStats.GetAll());
        }

        private static CharacterCustomizationData GetCharacterLooks()
        {
            return CharacterCustomization.GetCustomizationData(PhotonNetwork.LocalPlayer);
        }

        private static void SendCharacterCustomizations()
        {
            CharacterCustomizationData data = GetCharacterLooks();

            Plugin.Logger.LogDebug("Sending current character customizations to remote server...");
            StatPeakServerUtil.SendLooks(StatPeakServerUtil.CharacterLooks.FromCustomizationData(data));
        }

        [HarmonyPatch(typeof(RunManager), nameof(RunManager.StartRun))]
        [HarmonyPostfix]
        public static void OnRunStart()
        {
            ResetStatistics();

            if (IsInAirport())
            {
                Plugin.Logger.LogDebug("Player is in airport, no customization data will be sent");
                return;
            }

            SendCharacterCustomizations();
        }

        [HarmonyPatch(typeof(RunManager), nameof(RunManager.EndGame))]
        [HarmonyPostfix]
        public static void OnRunEnd()
        {
            SendRunStatistics();
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
            if (IsInAirport())
            {
                Plugin.Logger.LogDebug("Player quit in airport, no data will be sent");
                return;
            }

            SendRunStatistics();
        }
    }

    #endregion

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

    #region Player state stat tracking

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

    #endregion

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

            // Using if statements to track different movement types within one patch.
            // This ensures that exactly one stat is incremeneted at the same time.
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

    public class ChainPatches
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

    public class RopePatches
    {

        #region Rope Cannon stat tracking

        // In short, this is a mess and i wonder if there is a better way to track this statistic

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

        #endregion

        // Called by both Rope Cannon and Rope Spool.
        // Unlike Rope Spool, Rope Cannon calls with length 0f.
        // It could be explained by RopeAnchorWithRope.SpawnRope.SpoolOut, which changes length of segements (from 0f to 20f over period of time)
        [HarmonyPatch(typeof(Rope), nameof(Rope.AttachToAnchor_Rpc))]
        [HarmonyPostfix]
        public static void IncrementRopeSpoolPlaced(Rope __instance)
        {
            if (!__instance.view.IsMine) {
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
}
