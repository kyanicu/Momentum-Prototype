using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TownsPersonAnimation : CharacterAnimation
{

    protected override void Awake()
    {
        base.Awake();
    }

    public void StartWalking() 
    {
        //modelAnimator.SetBool(animatorParameterNameToID["Walking"], true);
    }

    public void StopWalking() 
    {
        //modelAnimator.SetBool(animatorParameterNameToID["Walking"], false);
    }

    public void StartTalking() 
    {
        //modelAnimator.SetBool(animatorParameterNameToID["Talking"], true);
    }

    public void StopTalking() 
    {
        //modelAnimator.SetBool(animatorParameterNameToID["Talking"], false);
    }

}