using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameplayEvent : LoggableEvent
{
    public string eventDetails;

    public GameplayEvent(string response)
    {
        eventType = EventType.GameplayEvent.ToString();
        eventDetails = response;
    }

    public static void Log(string response)
    {
        EventLogger.StoreEvent(new GameplayEvent(response));
    }
}
