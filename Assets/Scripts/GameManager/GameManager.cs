using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{

    #region Helpers
    /*
    public static bool IsValid(GameObject obj)
    {
        return obj != null && obj.activeInHierarchy;
    }

    public static bool IsValid(Behaviour be, bool CheckGameObject = false)
    {
        return be != null && be.isActiveAndEnabled && CheckGameObject ? IsValid(be.gameObject) : true;
    }
    */
    #endregion

    private int[] layerMasks = new int[32];

    private HashSet<CharacterDirector> controlledCharacters;

    protected GameManager() {}
    
    void Awake()
    {
        controlledCharacters = new HashSet<CharacterDirector>();

        SetLayerMasks();
    }

   
    void Update()
    {
        foreach(CharacterDirector c in controlledCharacters)
        {
            c.HandleControl();
        }
    }

    private void SetLayerMasks()
    {
        for (int i = 0; i < 32; i++)
        {
            layerMasks[i] = 0;
            for (int j = 0; j < 32; j++)
            {
                if (!Physics.GetIgnoreLayerCollision(i, j))
                {
                    layerMasks[i] = layerMasks[i] | 1 << j;
                }
            }
        }

    }

    public int GetLayerMask(int layer)
    {
        return layerMasks[layer];
    }

    public bool CheckLayerIsInMask(LayerMask layerMask, int layer)
    {
        return layerMask == (layerMask | (1 << layer));
    }

    public void RegisterCharacterControl(CharacterDirector c)
    {
        if(!controlledCharacters.Contains(c))
            controlledCharacters.Add(c);
    }

    public void DeregisterCharacterControl(CharacterDirector c)
    {
        if(controlledCharacters.Contains(c))
            controlledCharacters.Add(c);
    }

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
