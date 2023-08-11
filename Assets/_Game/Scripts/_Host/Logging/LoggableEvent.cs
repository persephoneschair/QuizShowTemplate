using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoggableEvent
{
    public enum EventType { AudiencePlayerEvent, HotseatPlayerEvent, GameplayEvent };

    public string timestamp;
    public string currentRound;
    public string eventType;

    public LoggableEvent()
    {
        timestamp = DateTime.Now.ToLongTimeString();
        currentRound = GameplayManager.Get.currentRound.ToString();
    }
}
