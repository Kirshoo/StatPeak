using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;

namespace StatPeak;

internal class StatPeakServerUtil
{
    internal class Stats_DTO
    {
        [JsonProperty("ticket")]
        public string Ticket { get; set; }

        [JsonProperty("stats")]
        public Dictionary<string, double> Stats { get; set; }
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
            Stats = stats,
        };

        string requestUrl = CreateRequestUrl("stats");
        string stringifiedBody = JsonConvert.SerializeObject(dto);

        using (HttpClient client = new HttpClient())
        {
            Plugin.Logger.LogDebug($"Request body: {JsonConvert.SerializeObject(stats)}");
            HttpContent content = new StringContent(stringifiedBody, Encoding.UTF8, "application/json");

            Plugin.Logger.LogDebug($"Sending stats to {requestUrl}");
            var response = client.PostAsync(requestUrl, content).Result;

            if (!response.IsSuccessStatusCode)
            {
                Plugin.Logger.LogError($"Failed to post stats: {response.StatusCode}");
            }
        }
    }
}

