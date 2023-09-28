using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using NaughtyAttributes;
using TwitchLib.Api.Helix.Models.Users;
using TwitchLib.Client.Models;
using TwitchLib.Unity;
using UnityEngine.Networking;

public class TwitchManager : SingletonMonoBehaviour<TwitchManager>
{
    public string testUsername;
    public string testMessage;

    #region Init
    public Api api;
    public Client client;
    private string channelName = "royal_flush";
    private object userImageLock = new object();

    void Start()
    {
        Application.runInBackground = true;

        Secrets.ReadSecrets();

        ConnectionCredentials credentials = new ConnectionCredentials("persephones_twitch_bot", Secrets.bot_access_token);
        client = new Client();
        client.Initialize(credentials, channelName);

        //Events
        client.OnMessageReceived += PublicChatMessageReceived;
        client.OnWhisperReceived += WhisperReceived;

        client.Connect();

        Application.runInBackground = true;
        api = new Api();
        api.Settings.AccessToken = Secrets.bot_access_token;
        api.Settings.ClientId = Secrets.client_id;
        List<string> fakeNames = new List<string>();
    }

    #endregion

    #region Public Functions

    public void MessageReceived(Identity id, string message, bool whisper)
    {
        //Check to see if the typed message matches any of the OTPs of the players in the lobby and that their Twitch name is not already populated
        //Check that any of the Twitch names are not not empty and that Twitch name is the same as the username
        if(PlayerManager.Get.pendingPlayers.Any(x => x.otp.ToUpperInvariant() == message.ToUpperInvariant()))
        {
            var pl = PlayerManager.Get.pendingPlayers.FirstOrDefault(x => x.otp.ToUpperInvariant() == message.ToUpperInvariant() && string.IsNullOrEmpty(x.twitchName));
            DebugLog.Print(pl.playerName + " (" + id.UserName + ") has validated their account.", DebugLog.StyleOption.Bold, DebugLog.ColorOption.Yellow);
            SendBotMessage(id.UserName + ", you have validated your account.");
            List<string> l = new List<string> { id.UserName };
            StartCoroutine(RequestUserImagesCoroutine(l, pl));
            return;
        }
    }

    public void RecoveryValidation(string twitchName, string otp)
    {
        if (PlayerManager.Get.pendingPlayers.Any(x => x.otp.ToUpperInvariant() == otp.ToUpperInvariant()))
        {
            var pl = PlayerManager.Get.pendingPlayers.FirstOrDefault(x => x.otp.ToUpperInvariant() == otp.ToUpperInvariant());
            DebugLog.Print(pl.playerName + " (" + twitchName + ") has validated their account.", DebugLog.StyleOption.Bold, DebugLog.ColorOption.Yellow);
            SendBotMessage(twitchName + ", you have validated your account.");
            List<string> l = new List<string> { twitchName };
            StartCoroutine(RequestUserImagesCoroutine(l, pl, true));
            return;
        }
    }

    [Button]
    public void SendTwitchWhisper()
    {
        if (testUsername == "" || testMessage == "")
        {
            return;
        }
        Identity id = new Identity(testUsername, testUsername, "0", "000000");
        MessageReceived(id, testMessage, true);
    }

    #endregion

    #region Private Functions

    private void SendBotMessage(string message)
    {
        if(!Operator.Get.testMode)
            client.SendMessage(client.JoinedChannels[0], message);
        else
        {
            DebugLog.Print("MESSAGE TO CHAT: " + message, DebugLog.StyleOption.Italic, DebugLog.ColorOption.Blue);
        }
    }

    private void PublicChatMessageReceived(object sender, TwitchLib.Client.Events.OnMessageReceivedArgs e)
    {
        Identity id = new Identity(e.ChatMessage.DisplayName, e.ChatMessage.Username, e.ChatMessage.UserId, e.ChatMessage.ColorHex);
        MessageReceived(id, e.ChatMessage.Message, false);
    }

    private void WhisperReceived(object sender, TwitchLib.Client.Events.OnWhisperReceivedArgs e)
    {
        Identity id = new Identity(e.WhisperMessage.DisplayName, e.WhisperMessage.Username, e.WhisperMessage.UserId, e.WhisperMessage.ColorHex);
        MessageReceived(id, e.WhisperMessage.Message, true);
    }

    private IEnumerator RequestUserImagesCoroutine(List<string> users, PlayerObject p, bool bypassSwitchAccount = false)
    {
        GetUsersResponse returnedResponse = null;

        yield return api.InvokeAsync(api.Helix.Users.GetUsersAsync(logins: users), (response) => returnedResponse = response);

        foreach (User user in returnedResponse.Users)
        {
            using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(user.ProfileImageUrl, false))
            {
                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.ConnectionError && request.result != UnityWebRequest.Result.ProtocolError)
                {
                    lock (userImageLock)
                    {
                        Texture2D texture = DownloadHandlerTexture.GetContent(request);
                        texture.filterMode = FilterMode.Bilinear;
                        texture.Apply(true, true);

                        p.ApplyProfilePicture(users[0], texture, bypassSwitchAccount);
                    }
                }
                else
                    Debug.LogError($"Failed to get profile image for {user.DisplayName}; reason: {request.error}");
            }
        }
    }

    #endregion

}
