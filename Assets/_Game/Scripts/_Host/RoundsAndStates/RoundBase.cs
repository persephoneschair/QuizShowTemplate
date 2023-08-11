using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class RoundBase : MonoBehaviour
{
    public Question currentQuestion = null;
    public Animator questionLozengeAnim;
    public TextMeshProUGUI questionMesh;

    public virtual void LoadQuestion(int qNum)
    {
        
    }

    public virtual void RunQuestion()
    {
        
    }

    public virtual void QuestionRunning()
    {
        
    }

    public virtual void OnQuestionEnded()
    {
        
    }

    public virtual void DisplayAnswer()
    {
                   
    }

    public virtual void ResetForNewQuestion()
    {
        
    }

    public virtual void BespokeEndOfRoundLogic()
    {

    }

    public virtual void ResetPlayerVariables()
    {
        foreach (PlayerObject po in PlayerManager.Get.players)
        {
            po.submission = "";
            po.submissionTime = 0;
            po.flagForCondone = false;
            po.wasCorrect = false;
        }
    }
}
