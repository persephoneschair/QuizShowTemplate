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
        string[] name = joinedPlayer.Name.Split('¬');
        if (name[1] != gameName)
        {
            SendPayloadToClient(joinedPlayer, EventLibrary.HostEventType.WrongApp, "");
            return;
        }

        if (PlayerManager.Get.players.Count >= Operator.Get.playerLimit && Operator.Get.playerLimit != 0)
        {
            //Do something slightly better than this
            return;
        }
        PlayerObject pl = new PlayerObject(joinedPlayer, name[0].Trim());
        pl.playerClientID = joinedPlayer.UserID;
        PlayerManager.Get.players.Add(pl);
        
        if(Operator.Get.recoveryMode)
        {
            DebugLog.Print($"{joinedPlayer.Name} HAS BEEN RECOVERED", DebugLog.StyleOption.Bold, DebugLog.ColorOption.Orange);
            SaveManager.RestorePlayer(pl);
            if (pl.twitchName != null)
                StartCoroutine(RecoveryValidation(pl));
            else
            {
                pl.otp = "";
                pl.podium.containedPlayer = null;
                pl.podium = null;
                pl.playerClientRef = null;
                pl.playerName = "";
                PlayerManager.Get.players.Remove(pl);
                DebugLog.Print($"{joinedPlayer.Name} HAS BEEN CLEARED", DebugLog.StyleOption.Bold, DebugLog.ColorOption.Red);
                return;
            }
        }
        else if (Operator.Get.fastValidation)
            StartCoroutine(FastValidation(pl));

        DebugLog.Print($"{name[0].Trim()} HAS JOINED THE LOBBY", DebugLog.StyleOption.Bold, DebugLog.ColorOption.Green);
        SendPayloadToClient(joinedPlayer, EventLibrary.HostEventType.Validate, $"{pl.otp}");
    }

    private IEnumerator FastValidation(PlayerObject pl)
    {
        yield return new WaitForSeconds(1f);
        TwitchManager.Get.testUsername = pl.playerName;
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
            case EventLibrary.ClientEventType.Answer:
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
