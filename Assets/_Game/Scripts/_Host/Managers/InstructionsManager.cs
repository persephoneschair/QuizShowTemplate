using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InstructionsManager : SingletonMonoBehaviour<InstructionsManager>
{
    public TextMeshProUGUI instructionsMesh;
    public Animator instructionsAnim;
    private readonly string[] instructions = new string[4]
    {
        "",
        "",
        "",
        ""
    };

    [Button]
    public void OnShowInstructions()
    {
        instructionsAnim.SetTrigger("toggle");
        instructionsMesh.text = instructions[(int)GameplayManager.Get.currentRound].Replace("[###]", Extensions.NumberToWords(QuestionManager.GetRoundQCount()));
    }

    [Button]
    public void OnHideInstructions()
    {
        instructionsAnim.SetTrigger("toggle");
    }
}
