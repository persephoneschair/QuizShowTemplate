using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ClientLandingPageManager : SingletonMonoBehaviour<ClientLandingPageManager>
{
    [Header("Landing Page")]
    public GameObject joinBoxObj;
    public TextMeshProUGUI versionMesh;
    public TMP_InputField nameInput;
    public TMP_InputField roomCodeInput;
    public Button joinButton;

    [Header("Loading Box")]
    public GameObject loadingBox;
    public TextMeshProUGUI loadingText;

    [Header("Failed To Connect")]
    public GameObject alertBox;
    public TextMeshProUGUI alertText;
    public TextMeshProUGUI alertButtonText;

    [Header("OTP Fields")]
    private readonly string otpMessage = "Please whisper [ABCD] to <color=yellow>persephones_twitch_bot</color> from the Twitch chat to validate your account";
    public GameObject otpAlert;
    public TextMeshProUGUI otpMesh;

    private void Update()
    {
        joinButton.interactable = (nameInput.text == "" || roomCodeInput.text.Length != 4) ? false : true;

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (nameInput.isFocused)
                roomCodeInput.ActivateInputField();
            else if(roomCodeInput.isFocused)
                nameInput.ActivateInputField();
        }

        if (joinButton.interactable && (Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Return)))
            OnPressJoinRoomButton();
    }

    private void Start()
    {
        versionMesh.text = versionMesh.text.Replace("[##]", Application.version);
    }

    public void OnPressJoinRoomButton()
    {
        ClientManager.Get.AttemptToConnectToRoom(nameInput.text.ToUpperInvariant(), roomCodeInput.text.ToUpperInvariant());
        roomCodeInput.text = "";
        nameInput.text = "";
        joinButton.interactable = false;
        OnAttemptToConnect();
        joinBoxObj.SetActive(false);
    }

    public void OnAttemptToConnect()
    {
        loadingBox.SetActive(true);
        loadingText.text = "Attempting connection to host server...";
    }

    public void OnCouldNotConnectToRoom()
    {
        loadingBox.SetActive(false);
        alertButtonText.text = "OK";
        alertBox.SetActive(true);
        alertText.text = "<color=red>Connection could not be established.\n\nPlease check the room code and try again.";
    }

    public void OnCouldNotConnectOK()
    {
        alertBox.SetActive(false);
        RefreshLandingPage();
    }

    public void RefreshLandingPage()
    {
        joinBoxObj.SetActive(true);
        this.gameObject.SetActive(true);
        nameInput.text = "";
        roomCodeInput.text = "";
    }

    public void OnValidateAccount(string otp)
    {
        loadingBox.SetActive(false);
        otpMesh.text = otpMessage.Replace("[ABCD]", otp);
        otpAlert.SetActive(true);
    }

    public void OnValidated(string[] otpArr)
    {
        //Default [0] is player name
        //Default [1] is player points
        otpAlert.SetActive(false);
        ClientMainGame.Get.Initialise();
        this.gameObject.SetActive(false);
    }
}
