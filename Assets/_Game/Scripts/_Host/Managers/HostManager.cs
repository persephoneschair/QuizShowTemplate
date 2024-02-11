using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Control;
using Newtonsoft.Json;
using System.Linq;

public class HostManager : SingletonMonoBehaviour<HostManager>
{
    [Header("Controlling Class")]
    public Host host;
    public string gameName;

    #region Join Room & Validation

    public void OnRoomConnected(string roomCode)
    {
        DebugLog.Print($"PLAYERS MAY NOW JOIN THE ROOM WITH THE CODE {host.RoomCode}", DebugLog.StyleOption.Bold, DebugLog.ColorOption.Blue);
    }

    public void OnPlayerJoins(Player joinedPlayer)
    {
        //This will handle any player joining with a valid Twitch name stored inside their client app
        if (joinedPlayer.Name.Contains('|'))
        {
            string[] name = joinedPlayer.Name.Split('|');
            PlayerObject pla = new PlayerObject(joinedPlayer, name[0]);
            pla.playerClientID = joinedPlayer.UserID;
            PlayerManager.Get.pendingPlayers.Add(pla);
            SendPayloadToClient(joinedPlayer, EventLibrary.HostEventType.Validate, $"{pla.otp}");
            StartCoroutine(FastValidation(pla, name[1]));
        }

        /*string[] name = joinedPlayer.Name.Split('¬');
        if (name[1] != gameName)
        {
            SendPayloadToClient(joinedPlayer, EventLibrary.HostEventType.WrongApp, "");
            return;
        }*/

        if (PlayerManager.Get.players.Count >= Operator.Get.playerLimit && Operator.Get.playerLimit != 0)
        {
            //Do something slightly better than this
            return;
        }

        PlayerObject pl = new PlayerObject(joinedPlayer, joinedPlayer.Name);
        pl.playerClientID = joinedPlayer.UserID;
        PlayerManager.Get.pendingPlayers.Add(pl);
        
        if(Operator.Get.recoveryMode)
        {
            DebugLog.Print($"{joinedPlayer.Name} HAS BEEN RECOVERED", DebugLog.StyleOption.Bold, DebugLog.ColorOption.Orange);
            SaveManager.RestorePlayer(pl);
            if (pl.twitchName != null)
                StartCoroutine(RecoveryValidation(pl));
            else
            {
                pl.otp = "";
                //pl.podium.containedPlayer = null;
                //pl.podium = null;
                pl.playerClientRef = null;
                pl.playerName = "";
                PlayerManager.Get.players.Remove(pl);
                DebugLog.Print($"{joinedPlayer.Name} HAS BEEN CLEARED", DebugLog.StyleOption.Bold, DebugLog.ColorOption.Red);
                return;
            }
        }
        else if (Operator.Get.fastValidation)
            StartCoroutine(FastValidation(pl));

        DebugLog.Print($"{joinedPlayer.Name} HAS JOINED THE LOBBY", DebugLog.StyleOption.Bold, DebugLog.ColorOption.Green);
        SendPayloadToClient(joinedPlayer, EventLibrary.HostEventType.Validate, $"{pl.otp}");
    }

    private IEnumerator FastValidation(PlayerObject pl, string overrideName = "")
    {
        yield return new WaitForSeconds(1f);
        TwitchManager.Get.testUsername = string.IsNullOrEmpty(overrideName) ? pl.playerName : overrideName;
        TwitchManager.Get.testMessage = pl.otp;
        TwitchManager.Get.SendTwitchWhisper();
        TwitchManager.Get.testUsername = "";
        TwitchManager.Get.testMessage = "";
    }

    private IEnumerator RecoveryValidation(PlayerObject pl)
    {
        yield return new WaitForSeconds(1f);
        TwitchManager.Get.RecoveryValidation(pl.twitchName, pl.otp);
    }

    #endregion

    #region Payload Management

    public void SendPayloadToClient(PlayerObject pl, EventLibrary.HostEventType e, string data)
    {
        host.UpdatePlayerData(pl.playerClientRef, EventLibrary.GetHostEventTypeString(e), data);

        //Simple Question
        //[0] = question
        //[1] = (int)time in seconds

        //Numerical Question
        //[0] = question
        //[1] = (int)time in seconds

        //Multiple Choice / Multi-select
        //[0] = question
        //[1] = (int)time in seconds
        //[2]-[n] = options

        //Multi-select limited
        //[0] = question
        //[1] = (int)time in seconds
        //[2] = limiter (X answers must be selected before submit activates)
        //[3]-[n] = options

        //Single & Multi Result
        //[0] = answer message (simply include list as concatenated string)
        //[1] = feedbackBox colorstyle enum (DEFAULT|CORRECT|INCORRECT - AS STRING)

        //Double Information
        //Used almost exclusively in Distinction!
        //Two data points in a single string, separated by |
        //[0] = box #1
        //[1] = box #2
    }

    public void SendPayloadToClient(Control.Player pl, EventLibrary.HostEventType e, string data)
    {
        host.UpdatePlayerData(pl, EventLibrary.GetHostEventTypeString(e), data);
    }

    public void OnReceivePayloadFromClient(EventMessage e)
    {
        PlayerObject p = GetPlayerFromEvent(e);
        EventLibrary.ClientEventType eventType = EventLibrary.GetClientEventType(e.EventName);

        string s = (string)e.Data[e.EventName];
        var data = JsonConvert.DeserializeObject<string>(s);

        switch (eventType)
        {
            case EventLibrary.ClientEventType.StoredValidation:
                string[] str = data.Split('|').ToArray();
                TwitchManager.Get.testUsername = str[0];
                TwitchManager.Get.testMessage = str[1];
                TwitchManager.Get.SendTwitchWhisper();
                TwitchManager.Get.testUsername = "";
                TwitchManager.Get.testMessage = "";
                break;

            case EventLibrary.ClientEventType.NumericalQuestion:
                //This will have an array length of [1]
                p.HandlePlayerScoring(data.Split('|'));
                SendPayloadToClient(p, EventLibrary.HostEventType.Information, "Answer received");
                break;

            case EventLibrary.ClientEventType.SimpleQuestion:
                //This will have an array length of [1]
                p.HandlePlayerScoring(data.Split('|'));
                SendPayloadToClient(p, EventLibrary.HostEventType.Information, "Answer received");
                break;

            case EventLibrary.ClientEventType.MultipleChoiceQuestion:
                //This will have an array length of [1]
                p.HandlePlayerScoring(data.Split('|'));
                SendPayloadToClient(p, EventLibrary.HostEventType.Information, "Answer received");
                break;

            case EventLibrary.ClientEventType.MultiSelectQuestion:
                //This will have a variable array length
                p.HandlePlayerScoring(data.Split('|'));
                SendPayloadToClient(p, EventLibrary.HostEventType.Information, "Answer received");
                break;

            case EventLibrary.ClientEventType.DangerZoneQuestion:
                //This will have an array length of [1] or [2]
                p.HandlePlayerScoring(data.Split('|'));
                SendPayloadToClient(p, EventLibrary.HostEventType.Information, "Answer received");
                break;

            case EventLibrary.ClientEventType.PasteAlert:
                //Silent alarm indicating some text has been pasted into an answer box
                DebugLog.Print($"A PASTE ALERT WAS RAISED BY {p.playerName} ({p.twitchName}): {data}", DebugLog.StyleOption.Bold, DebugLog.ColorOption.Purple);

                string currentQ = "";
                //Populate on game by game basis depending on format
                switch (GameplayManager.Get.currentRound)
                {
                    case GameplayManager.Round.None:
                        currentQ = "No live question";
                        break;
                }
                PasteAlertEvent.Log(p, data, currentQ);
                EventLogger.PrintPasteLog();
                break;

            default:
                break;
        }
    }

    #endregion

    #region Helpers

    public PlayerObject GetPlayerFromEvent(EventMessage e)
    {
        return PlayerManager.Get.players.FirstOrDefault(x => x.playerClientRef == e.Player);
    }

    #endregion
}
