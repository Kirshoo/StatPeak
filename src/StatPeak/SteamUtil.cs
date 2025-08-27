using System;
using System.Threading;
using Steamworks;

namespace StatPeak;

internal class SteamUtil
{
    public static readonly int STEAM_API_DELAY = 500;

    private static string SessionTicket = string.Empty;
    private static Callback<GetTicketForWebApiResponse_t> GetTicketForWebApiResponseCallback;

    public static void Init()
    {
        if (GetTicketForWebApiResponseCallback == null)
        {
            RequestSessionTicket();
        }
    }

    private static void OnAuthCallback(GetTicketForWebApiResponse_t callback)
    {
        Plugin.Logger.LogDebug($"Ticket Callback is called: {callback.m_eResult}");
        SessionTicket = BitConverter.ToString(callback.m_rgubTicket).Replace("-", string.Empty);

        GetTicketForWebApiResponseCallback.Dispose();
        GetTicketForWebApiResponseCallback = null;
        Plugin.Logger.LogDebug($"Received session token: {SessionTicket}");
    }

    public static void RequestSessionTicket()
    {
        GetTicketForWebApiResponseCallback = Callback<GetTicketForWebApiResponse_t>.Create(OnAuthCallback);
        HAuthTicket ticket = SteamUser.GetAuthTicketForWebApi(null);
        Plugin.Logger.LogDebug($"Requesting steam for auth ticket: {ticket.m_HAuthTicket}");

        new Thread(() =>
        {
            Thread.Sleep(STEAM_API_DELAY);

            while (string.IsNullOrEmpty(SessionTicket))
            {
                Plugin.Logger.LogWarning($"No callback was run yet. Running callbacks manually...");
                Thread.Sleep(STEAM_API_DELAY);
                SteamAPI.RunCallbacks();
            }
        }).Start();
    }

    public static string GetSessionTicket()
    {
        return SessionTicket;
    }
}
