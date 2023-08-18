using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalTimeManager : SingletonMonoBehaviour<GlobalTimeManager>
{

    public bool questionClockRunning;
    [ShowOnly] public float elapsedTime;

    private void Update()
    {
        if (questionClockRunning)
            QuestionTimer();
        else
            elapsedTime = 0;
    }

    void QuestionTimer()
    {
        elapsedTime += (1f * Time.deltaTime);
    }

    public float GetRawTimestamp()
    {
        return elapsedTime;
    }

    public string GetFormattedTimestamp()
    {
        return elapsedTime.ToString("#0.00");
    }
}
