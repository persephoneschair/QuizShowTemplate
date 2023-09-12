using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LobbyManager : SingletonMonoBehaviour<LobbyManager>
{
    public bool lateEntry;

    public TextMeshProUGUI welcomeMessageMesh;
    public Animator lobbyCodeAnim;
    private const string welcomeMessage = "";

    //"Welcome to <font=RiskyBusiness><size=200%>GAME NAME</size></font>\n\n" +
    //  "" +
    //"Playing on a mobile device? Scan the QR code!\n\n\n\n\n\n\n" +
    //"" +
    //"Desktop or laptop? Please visit:\n<color=yellow>https://persephoneschair.itch.io/gamenight</color>\n" +
    //"<size=300%><color=#F8A3A3>[ABCD]</color>";

    private const string permaMessage = "To join the game, please visit <color=yellow>https://persephoneschair.itch.io/gamenight</color> and join with the room code <color=#F8A3A3>[ABCD]</color>";

    public Animator permaCodeAnim;
    public TextMeshProUGUI permaCodeMesh;

    [Button]
    public void OnOpenLobby()
    {
        lobbyCodeAnim.SetTrigger("toggle");
        welcomeMessageMesh.text = welcomeMessage.Replace("[ABCD]", HostManager.Get.host.RoomCode.ToUpperInvariant());
    }

    [Button]
    public void OnLockLobby()
    {
        lateEntry = true;
        lobbyCodeAnim.SetTrigger("toggle");
        permaCodeMesh.text = permaMessage.Replace("[ABCD]", HostManager.Get.host.RoomCode.ToUpperInvariant());
        Invoke("TogglePermaCode", 1f);
    }

    public void TogglePermaCode()
    {
        permaCodeAnim.SetTrigger("toggle");
    }
}
