using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public static class EventLogger
{
    private static List<LoggableEvent> log = new List<LoggableEvent>();
    public static string formattedLog;

    public static void StoreEvent(LoggableEvent l)
    {
        log.Add(l);
        formattedLog = JsonConvert.SerializeObject(log, Formatting.Indented);
    }

    public static void PrintLog()
    {
        DataStorage.SaveFile("Event Log", formattedLog);
    }
}
