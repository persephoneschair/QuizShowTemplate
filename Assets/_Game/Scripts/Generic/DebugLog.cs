using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class DebugLog
{
    public enum ColorOption { Green, Blue, Red, Yellow, Orange, Default };
    public enum StyleOption { Bold, Italic, BoldItalic, Default };

    public static void Print(string message, StyleOption style = StyleOption.Default, ColorOption col = ColorOption.Default)
    {
        switch(style)
        {
            case StyleOption.Default:
                PrintRegular(col, message);
                break;

            case StyleOption.Bold:
                PrintBold(col, message);
                break;

            case StyleOption.Italic:
                PrintItalic(col, message);
                break;

            case StyleOption.BoldItalic:
                PrintBoldItalic(col, message);
                break;
        }
    }

    private static void PrintBoldItalic(ColorOption col, string message)
    {
        string color = GetColor(col);
        Debug.Log("<color=" + color + "><b><i>" + message + "</i></b></color>");
    }

    private static void PrintBold(ColorOption col, string message)
    {
        string color = GetColor(col);
        Debug.Log("<color=" + color + "><b>" + message + "</b></color>");
    }

    private static void PrintItalic(ColorOption col, string message)
    {
        string color = GetColor(col);
        Debug.Log("<color=" + color + "><i>" + message + "</i></color>");
    }

    private static void PrintRegular(ColorOption col, string message)
    {
        string color = GetColor(col);
        Debug.Log("<color=" + color + ">" + message + "</color>");
    }

    private static string GetColor(ColorOption col)
    {
        switch (col)
        {
            case ColorOption.Green:
                return "#68FF00";

            case ColorOption.Blue:
                return "#00DBFF";

            case ColorOption.Red:
                return "#FF0000";

            case ColorOption.Yellow:
                return "#FFFF00";

            case ColorOption.Orange:
                return "#FFA500";

            case ColorOption.Default:
                return "#FFFFFF";

            default:
                return "#FFFFFF";
        }
    }
}
