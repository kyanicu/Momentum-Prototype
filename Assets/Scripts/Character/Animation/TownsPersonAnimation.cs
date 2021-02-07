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
        animator.SetBool(animatorParameterNameToID["Walking"], true);
    }

    public void StopWalking() 
    {
        animator.SetBool(animatorParameterNameToID["Walking"], false);
    }

    public void StartTalking() 
    {
        animator.SetBool(animatorParameterNameToID["Talking"], true);
    }

    public void StopTalking() 
    {
        animator.SetBool(animatorParameterNameToID["Talking"], false);
    }

}