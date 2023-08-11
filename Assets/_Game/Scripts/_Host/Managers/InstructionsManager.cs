using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InstructionsManager : MonoBehaviour
{
    #region Init

    public static InstructionsManager Get { get; private set; }
    private void Awake()
    {
        if (Get != null && Get != this)
            Destroy(this);
        else
            Get = this;
    }

    #endregion

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
