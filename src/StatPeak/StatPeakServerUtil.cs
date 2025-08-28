using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using Newtonsoft.Json;

namespace StatPeak;

internal class StatPeakServerUtil
{
    internal static Dictionary<string, string> ExternalKeyMap = new Dictionary<string, string>()
    {
        // { IncomingKey, OutgoingKey }

        { "walked", "walked" },
        { "climbed", "climbed" },
        { "fallen", "fallen" },
        { "slid", "slid" },
        { "jumps", "jumps" },
        { "hunger", "hunger" },
        { "poison", "poison" },
        { "cold", "cold" },
        { "hot", "heat" },
        { "sleep", "sleep" },
        { "curse", "curse" },
        { "injury", "injury" },
        { "faints", "faints" },
        { "deaths", "deaths" },
        { "revives", "revives" },
        { "luggages", "luggages" },
        { "items_thrown", "items_thrown" },
        { "items_grabbed", "items_grabbed" },
        { "items_cooked", "items_cooked" },
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

    internal class Stats_DTO
    {
        [JsonProperty("ticket")]
        public string Ticket { get; set; }

        [JsonProperty("stats")]
        public Dictionary<string, double> Stats { get; set; }

        public Stats_DTO GetTruncatedCopy(int TruncatedTicketLength = 16)
        {
            return new Stats_DTO()
            {
                Ticket = Ticket.Substring(0, TruncatedTicketLength),
                Stats = Stats,
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

    public static void SendStats(Dictionary<string, double> stats)
    {
        Stats_DTO dto = new Stats_DTO()
        {
            Ticket = SteamUtil.GetSessionTicket(),
            Stats = RemapKeys(stats),
        };

        string requestUrl = CreateRequestUrl("stats");
        string stringifiedBody = JsonConvert.SerializeObject(dto);

        new Thread(() =>
        {
            using (HttpClient client = new HttpClient())
            {
                Plugin.Logger.LogDebug($"Request body: {JsonConvert.SerializeObject(dto.GetTruncatedCopy())}");
                HttpContent content = new StringContent(stringifiedBody, Encoding.UTF8, "application/json");

                Plugin.Logger.LogDebug($"Sending stats to {requestUrl}");
                var response = client.PostAsync(requestUrl, content).Result;

                if (!response.IsSuccessStatusCode)
                {
                    Plugin.Logger.LogError($"Failed to post stats: {response.StatusCode}");
                }
            }
        }).Start();
    }
}

