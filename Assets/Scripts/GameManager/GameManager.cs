﻿using System.Collections;
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

    private IEnumerator TimerViaGameTimeCoroutine(float t, Action functionToCall)
    {
        yield return new WaitForSeconds(t);
        functionToCall();
    }

    public Coroutine TimerViaRealTime(float t, Action functionToCall)
    {
        return StartCoroutine(TimerViaRealTimeCoroutine(t, functionToCall));
    }
    public Coroutine TimerViaGameTime(float t, Action functionToCall)
    {
        return StartCoroutine(TimerViaGameTimeCoroutine(t, functionToCall));
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


    public Coroutine TimedConditionalCheckViaRealTime(float t, Func<bool> conditional, Action functionToCall)
    {
        return StartCoroutine(TimedConditionalCheckViaRealTimeCoroutine(t, conditional, functionToCall));
    }
    

}
