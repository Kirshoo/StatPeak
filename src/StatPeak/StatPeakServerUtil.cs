using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using Newtonsoft.Json;

namespace StatPeak;

internal class StatPeakServerUtil
{
    // Acts as a filter and a translator. All stats listed here, will be sent to remote server
    internal static Dictionary<string, string> ExternalKeyMap = new Dictionary<string, string>()
    {
        // { IncomingKey, OutgoingKey }

        { Stat.DistanceWalked, "walked" },
        { Stat.DistanceClimbed, "climbed" },
        { Stat.DistanceWhileAirborne, "fallen" },
        { Stat.DistanceClimbedOnVines, "slid" },
        { Stat.TotalJumps, "jumps" },
        { Stat.GreatestContinuousClimb, "greatest_climb" },

        { "Hunger", "hunger" },
        { "Poison", "poison" },
        { "Cold", "cold" },
        { "Hot", "heat" },
        { "Drowsy", "sleep" },
        { "Curse", "curse" },
        { "Injury", "injury" },

        { Stat.TotalFaints, "faints" },
        { Stat.TotalDeaths, "deaths" },
        { Stat.TotalRevives, "revives" },
        { Stat.LuggagesOpened, "luggages" },
        { Stat.ItemsThrown, "items_thrown" },
        { Stat.ItemsGrabbed, "items_grabbed" },
        { Stat.ItemsCooked, "items_cooked" },

        { ItemName.YellowCrispberry, "eaten_y_crispberry" },
        { ItemName.RedCrispberry, "eaten_r_crispberry" },
        { ItemName.GreenCrispberry, "eaten_g_crispberry" },
        { ItemName.YellowClusterberry, "eaten_y_clusterberry" },
        { ItemName.BlackClusterberry, "eaten_b_clusterberry" },
        { ItemName.RedClusterberry, "eaten_r_clusterberry" },
        { ItemName.GreenClusterberry, "eaten_g_clusterberry" },
        { ItemName.YellowKingberry, "eaten_y_kingberry" },
        { ItemName.GreenKingberry, "eaten_g_kingberry" },
        { ItemName.PurpleKingberry, "eaten_p_kingberry" },
        { ItemName.BrownBerrynana, "eaten_br_berrynana" },
        { ItemName.BlueBerrynana, "eaten_bl_berrynana" },
        { ItemName.PinkBerrynana, "eaten_p_berrynana" },
        { ItemName.YellowBerrynana, "eaten_y_berrynana" },
        { ItemName.YellowWinterberry, "eaten_y_winterberry" },
        { ItemName.OrangeWinterberry, "eaten_o_winterberry" },
        { ItemName.Napberry, "eaten_napberry" },
        { ItemName.MedicinalRoot, "eaten_medroot" },
        { ItemName.Egg, "eaten_egg" },
        { ItemName.Bird, "eaten_chicken" },
        { ItemName.Marshmallow, "eaten_marshmallow" },
        { ItemName.CoconutHalf, "eaten_coconut" },
        { ItemName.Honeycomb, "eaten_honeycomb" },
        { ItemName.ButtonShroom, "eaten_buttonshroom" },
        { ItemName.BugleShroom, "eaten_bugleshroom" },
        { ItemName.ClusterShroom, "eaten_clustershroom" },
        { ItemName.ChubbyShroom, "eaten_chubbyshroom" },
        { ItemName.Tick, "eaten_tick" },
        { ItemName.TrailMix, "eaten_trailmix" },
        { ItemName.GranolaBar, "eaten_granola" },
        { ItemName.SportsDrink, "eaten_sportsdrink" },
        { ItemName.EnergyDrink, "eaten_energy" },
        { ItemName.AirlineFood, "eaten_airlinefood" },
        { ItemName.ScoutCookies, "eaten_cookies" },
        { ItemName.BigLollipop, "eaten_lollipop" },

        // TODO: Check tracking for all these
        { ItemName.PandorasLunchbox, "used_lunchbox" },
        { ItemName.CureAll, "used_cureall" },
        { ItemName.CursedSkull, "used_cursedskull" },
        { ItemName.ScoutEffigy, "used_scouteffigy" },
        { ItemName.Balloon, "used_balloon" },
        { ItemName.Sunscreen, "used_sunscreen" },
        { ItemName.ScoutCannon, "used_cannon" },
        { ItemName.Blowgun, "used_blowgun" },
        { ItemName.Piton, "used_piton" },
        { ItemName.FirstAidKit, "used_medkit" },
        { ItemName.HeatPack, "used_heatpack" },
        { ItemName.Antidote, "used_antidote" },
        { ItemName.Bandages, "used_bandages" },
        { ItemName.FriendshipBugle, "used_friendshipbugle" },
        { ItemName.Bugle, "used_bugle" },
        { ItemName.PortableStove, "used_stove" },
        { ItemName.Binoculars, "used_binoculars" },
        { ItemName.Flare, "used_flares" },
        { ItemName.RopeSpool, "used_ropespool" },
        { ItemName.AntiropeSpool, "used_antiropespool" },
        { ItemName.RopeCannon, "used_ropegun" },
        { ItemName.AntiropeCannon, "used_antiropegun" },
        { ItemName.ChainLauncher, "used_chaingun" },

        { Stat.RopePlaced, "placed_rope" },
        { Stat.ChainPlaced, "placed_chain" },
        { Stat.AntiropePlaced, "placed_antirope" },

        { Stat.HasEscaped, "escaped" },
        { Stat.HasFailed, "failed" },

        { Stat.CampfiresLit, "campfires_lit" },
        { Stat.TicksRemoved, "ticks_removed" },
    };

    public static Dictionary<string, double> RemapKeys(Dictionary<string, double> map)
    {
        return map
            // Only pass the valid (i.e. tracked) stat keys
            .Where(entry => ExternalKeyMap.ContainsKey(entry.Key))
            .ToDictionary(
                entry => ExternalKeyMap[entry.Key],
                entry => entry.Value
            );
    }

    internal abstract class ServerPayloadBase
    {
        [JsonProperty("ticket")]
        public string Ticket { get; set; }

        public abstract ServerPayloadBase GetTruncatedCopy(int TruncatedTicketLength = 16);
    }

    internal class Stats_DTO : ServerPayloadBase
    {
        [JsonProperty("stats")]
        public Dictionary<string, double> Stats { get; set; }

        public override ServerPayloadBase GetTruncatedCopy(int TruncatedTicketLength = 16)
        {
            return new Stats_DTO()
            {
                Ticket = Ticket.Substring(0, TruncatedTicketLength),
                Stats = Stats,
            };
        }
    }

    internal class CharacterLooks
    {
        [JsonProperty("color")]
        public int Color { get; set; }

        [JsonProperty("accessory")]
        public int Accessory { get; set; }

        [JsonProperty("eye")]
        public int Eye { get; set; }

        [JsonProperty("mouth")]
        public int Mouth { get; set; }

        [JsonProperty("hat")]
        public int Hat { get; set; }

        [JsonProperty("outfit")]
        public int Outfit { get; set; }

        public static CharacterLooks FromCustomizationData(CharacterCustomizationData data)
        {
            return new CharacterLooks()
            {
                Color = data.currentSkin,
                Accessory = data.currentAccessory,
                Eye = data.currentEyes,
                Mouth = data.currentMouth,
                Hat = data.currentHat,
                Outfit = data.currentOutfit,
            };
        }
    }

    internal class Looks_DTO : ServerPayloadBase
    {
        [JsonProperty("looks")]
        public CharacterLooks Looks { get; set; }

        public override ServerPayloadBase GetTruncatedCopy(int TruncatedTicketLength = 16)
        {
            return new Looks_DTO()
            {
                Ticket = Ticket.Substring(0, TruncatedTicketLength),
                Looks = Looks,
            };
        }
    }

    private static string CreateRequestUrl(string endpoint)
    {
        StringBuilder url = new StringBuilder();

        url.Append(
            Plugin.RemoteServerHttpsProtocol.Value ? "https://" : "http://"
        );
        url.Append(Plugin.RemoteServerBaseURL.Value);
        url.Append(Plugin.RemoteServerBasePath.Value);

        url.Append(string.Format("/{0}", endpoint));

        return url.ToString();
    }

    private static void PostData(string url, ServerPayloadBase payload)
    {
        string stringifiedBody = JsonConvert.SerializeObject(payload);

        new Thread(() =>
        {
            using HttpClient client = new HttpClient();
            Plugin.Logger.LogDebug($"Request body: {JsonConvert.SerializeObject(payload.GetTruncatedCopy())}");
            HttpContent content = new StringContent(stringifiedBody, Encoding.UTF8, "application/json");

            Plugin.Logger.LogDebug($"Sending stats to {url}");
            var response = client.PostAsync(url, content).Result;

            if (!response.IsSuccessStatusCode)
            {
                Plugin.Logger.LogError($"Failed to post stats: {response.StatusCode}");
            }
        }).Start();
    }

    public static void SendStats(Dictionary<string, double> stats)
    {
        Stats_DTO dto = new Stats_DTO()
        {
            Ticket = SteamUtil.GetSessionTicket(),
            Stats = RemapKeys(stats),
        };

        string requestUrl = CreateRequestUrl("stats");
        PostData(requestUrl, dto);
    }

    public static void SendLooks(CharacterLooks looks)
    {
        Looks_DTO dto = new Looks_DTO()
        {
            Ticket = SteamUtil.GetSessionTicket(),
            Looks = looks,
        };

        string requestUrl = CreateRequestUrl("looks");
        PostData(requestUrl, dto);
    }
}

