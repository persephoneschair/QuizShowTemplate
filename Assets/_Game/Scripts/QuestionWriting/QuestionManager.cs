using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System;
using UnityEngine.PlayerLoop;

public static class QuestionManager
{
    public static Pack currentPack = null;

    public static void DecompilePack(TextAsset tx)
    {
        currentPack = JsonConvert.DeserializeObject<Pack>(tx.text);
    }

    public static int GetRoundQCount()
    {
        switch (GameplayManager.Get.currentRound)
        {
            default:
                return 0;
        }
    }

    public static Question GetQuestion(int qNum)
    {
        switch (GameplayManager.Get.currentRound)
        {
            default:
                return null;
        }
    }
}
