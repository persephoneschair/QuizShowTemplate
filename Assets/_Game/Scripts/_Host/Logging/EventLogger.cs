using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public static class EventLogger
{
    private static List<LoggableEvent> log = new List<LoggableEvent>();
    public static string formattedLog;

    private static List<LoggableEvent> pasteLog = new List<LoggableEvent>();
    public static string formattedPasteLog;

    public static void StoreEvent(LoggableEvent l)
    {
        log.Add(l);
        formattedLog = JsonConvert.SerializeObject(log, Formatting.Indented);
    }

    public static void StorePasteDetection(LoggableEvent l)
    {
        pasteLog.Add(l);
        formattedPasteLog = JsonConvert.SerializeObject(pasteLog, Formatting.Indented);
    }

    public static void PrintLog()
    {
        DataStorage.SaveFile("Event Log", formattedLog);
    }

    public static void PrintPasteLog()
    {
        DataStorage.SaveFile("Paste Log", formattedPasteLog);
    }
}
