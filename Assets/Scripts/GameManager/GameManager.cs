using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    protected GameManager() {}

    private IEnumerator TimerViaRealTimeCoroutine(float t, Action functionToCall)
    {
        yield return new WaitForSecondsRealtime(t);
        functionToCall();
    }

    public void TimerViaRealTime(float t, Action functionToCall)
    {
        StartCoroutine(TimerViaRealTimeCoroutine(t, functionToCall));
    }

    private IEnumerator TimedConditionalCheckViaRealTimeCoroutine(float t, Func<bool> conditional, Action functionToCall)
    {
        for (float timer = 0; timer < t; timer += Time.deltaTime)
        {
            if (conditional())
            {
                functionToCall();
                break;
            }
            yield return null;
        }
    }

    public void TimedConditionalCheckViaRealTime(float t, Func<bool> conditional, Action functionToCall)
    {
        StartCoroutine(TimedConditionalCheckViaRealTimeCoroutine(t, conditional, functionToCall));
    }

}
