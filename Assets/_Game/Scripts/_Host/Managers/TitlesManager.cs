using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TitlesManager : SingletonMonoBehaviour<TitlesManager>
{
    [Button]
    public void RunTitleSequence()
    {
        if (Operator.Get.skipOpeningTitles)
            EndOfTitleSequence();
        else
        {
            GameplayManager.Get.currentStage = GameplayManager.GameplayStage.DoNothing;
            StartCoroutine(TitleSequence());
        }           
    }

    IEnumerator TitleSequence()
    {        
        yield return new WaitForSeconds(0f);
        EndOfTitleSequence();
    }

    void EndOfTitleSequence()
    {
        this.gameObject.SetActive(false);
    }
}
