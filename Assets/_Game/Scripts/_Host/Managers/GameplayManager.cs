using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using TMPro;
using System.Linq;
using Control;

public class GameplayManager : SingletonMonoBehaviour<GameplayManager>
{
    [Header("Rounds")]
    public RoundBase[] rounds;

    [Header("Question Data")]
    public static int nextQuestionIndex = 0;

    public enum GameplayStage
    {
        RunTitles,
        OpenLobby,
        LockLobby,
        RevealInstructions,
        HideInstructions,
        RunQuestion,

        ResetPostQuestion,
        DisplayFinalLeaderboard,
        HideFinalLeaderboard,
        RollCredits,
        DoNothing
    };
    public GameplayStage currentStage = GameplayStage.DoNothing;

    public enum Round { None };
    public Round currentRound = Round.None;
    public int roundsPlayed = 0;

    [Button]
    public void ProgressGameplay()
    {
        switch (currentStage)
        {
            case GameplayStage.RunTitles:
                TitlesManager.Get.RunTitleSequence();
                //If in recovery mode, we need to call Restore Players to restore specific player data (client end should be handled by the reload host call)
                //Also need to call Restore gameplay state to bring us back to where we need to be (skipping titles along the way)
                //Reveal instructions would probably be a sensible place to go to, though check that doesn't iterate any game state data itself
                break;

            case GameplayStage.OpenLobby:
                LobbyManager.Get.OnOpenLobby();
                currentStage++;
                break;

            case GameplayStage.LockLobby:
                LobbyManager.Get.OnLockLobby();
                break;

            case GameplayStage.RevealInstructions:
                break;

            case GameplayStage.HideInstructions:
                break;

            case GameplayStage.RunQuestion:
                break;

            case GameplayStage.ResetPostQuestion:
                break;

            case GameplayStage.DisplayFinalLeaderboard:
                break;

            case GameplayStage.HideFinalLeaderboard:
                break;

            case GameplayStage.RollCredits:
                break;

            case GameplayStage.DoNothing:
                break;
        }
    }
}
