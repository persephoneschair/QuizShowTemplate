using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PasteAlertEvent : LoggableEvent
{
    public string twitchName;
    public string playerName;
    public string currentQuestion;
    public string pastedData;

    public PasteAlertEvent(PlayerObject p, string response, string currentQ = "")
    {
        eventType = EventType.PasteAlertEvent.ToString();
        GetPlayerBaseDetails(p);
        currentQuestion = currentQ;
        pastedData = response;
    }

    public static void Log(PlayerObject p, string response, string currentQ = "currentQ")
    {
        EventLogger.StorePasteDetection(new PasteAlertEvent(p, response, currentQ));
    }

    #region Internal Calls

    private void GetPlayerBaseDetails(PlayerObject p)
    {
        twitchName = p.twitchName;
        playerName = p.playerName;
    }

    #endregion
}
