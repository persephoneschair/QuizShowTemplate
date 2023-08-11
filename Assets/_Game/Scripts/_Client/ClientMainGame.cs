using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;

public class ClientMainGame : MonoBehaviour
{
    #region Init

    public static ClientMainGame Get { get; private set; }
    private void Awake()
    {
        if (Get != null && Get != this)
            Destroy(this);
        else
            Get = this;
    }

    #endregion

    [Header("Leaderboard")]
    public ClientLeaderboardManager leaderboardManager;

    [Header("Fixed Message")]
    public GameObject fixedMessageObj;
    public TextMeshProUGUI fixedMessageMesh;

    private void Update()
    {
        /*submitButton.interactable = answerInput.text.Length <= 0 ? false : true;

        if (enterSubmits && answerInput.text.Length > 0 && (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
            OnSubmitAnswer();*/
    }

    public void Initialise()
    {

    }

    public void DisplayCountdown()
    {

    }

    public void DisplayQuestion()
    {

    }

    public void OnSubmitAnswer()
    {
        ClientManager.Get.SendPayloadToHost("ANSWER GOES HERE", EventLibrary.ClientEventType.Answer);
    }

    public void DisplayResponse(string[] data)
    {

    }

    public void DisplayAnswer(string data)
    {

    }

    public void UpdateCurrentScore(string data)
    {

    }

    public void ResetForNewQuestion()
    {
        
    }

    public void NewInstanceOpened()
    {
        fixedMessageObj.SetActive(true);
        fixedMessageMesh.text = "THE TWITCH ACCOUNT ASSOCIATED WITH THIS CONTROLLER HAS BEEN ASSOCIATED WITH ANOTHER CONTROLLER.\n\n" +
            "THIS CONTROLLER WILL NOT RECEIVE FURTHER DATA FROM THE GAME AND CAN NOW BE CLOSED.\n\n" +
            "IF YOU DID  NOT VALIDATE A NEW CONTROLLER, PLEASE CONTACT THE HOST.";
    }

    public void EndOfGameAlert(string pennyValue)
    {
        fixedMessageObj.SetActive(true);
        fixedMessageMesh.text = "THE GAME HAS NOW CONCLUDED AND THIS CONTROLLER CAN BE CLOSED.";
    }
}
