using System.Globalization;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;
using StatPeak.Patches;

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

        PatchAll();

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

    private void PatchAll()
    {
        _harmony.PatchAll(typeof(AfflictionPatch));
        Logger.LogInfo($"All affliction patches applied successfully.");

        _harmony.PatchAll(typeof(PlayerStatePatch));
        _harmony.PatchAll(typeof(PlayerMovementPatch));
        Logger.LogInfo($"All player state patches applied successfully");

        _harmony.PatchAll(typeof(PlayerActionPatch));
        _harmony.PatchAll(typeof(Plugin.RunPatch));
        _harmony.PatchAll(typeof(RopePatch));
        _harmony.PatchAll(typeof(ChainPatch));
        Logger.LogInfo($"All run specific patches applied successfully");

        _harmony.PatchAll(typeof(Plugin.InitializationPatch));
        Logger.LogInfo($"All initialization patches applied successfully");
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
}
