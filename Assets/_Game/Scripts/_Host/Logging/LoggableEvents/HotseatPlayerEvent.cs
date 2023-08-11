using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HotseatPlayerEvent : LoggableEvent
{
    public string twitchName;
    public string playerName;
    public string eventDetails;
    public int points;

    public HotseatPlayerEvent(PlayerObject p, string response)
    {
        eventType = EventType.HotseatPlayerEvent.ToString();
        GetPlayerBaseDetails(p);
        eventDetails = response;
    }

    public static void Log(PlayerObject p, string response)
    {
        EventLogger.StoreEvent(new HotseatPlayerEvent(p, response));
    }

    #region Internal Calls

    private void GetPlayerBaseDetails(PlayerObject p)
    {
        twitchName = p.twitchName;
        playerName = p.playerName;
        points = p.points;
    }

    #endregion
}
