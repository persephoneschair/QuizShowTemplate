using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TwitchLib.Unity;
using System.IO;

public static class Secrets
{
    private const string directory = @"D:\Unity Projects\GAME NIGHT\Secrets.txt";

    public static string client_id;
    public static string client_secret;
    public static string bot_access_token;
    public static string bot_refresh_token;
    public static string twitch_user_id;

    public static void ReadSecrets()
    {
        string[] sc = File.ReadAllLines(directory);
        client_id = sc[0].Trim();
        client_secret = sc[1].Trim();
        bot_access_token = sc[2].Trim();
        bot_refresh_token = sc[3].Trim();
        twitch_user_id = sc[4].Trim();
    }
}
