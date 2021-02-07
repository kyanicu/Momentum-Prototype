using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AttackAnimationState { FINISHED, STARTUP, CONTACT, RECOVERY, BUFFER }

public class PlayerAnimationEvents : MonoBehaviour
{
    new private PlayerAnimation animation;

    void Start()
    {
        animation = transform.parent.GetComponent<PlayerAnimation>();
    }

    public void AttackStartup()
    {
        animation.AttackStateTransition(AttackAnimationState.STARTUP);
    }

    public void AttackContact()
    {
        animation.AttackStateTransition(AttackAnimationState.CONTACT);
    }

    public void AttackRecovery()
    {
        animation.AttackStateTransition(AttackAnimationState.RECOVERY);
    }

    public void AttackBufferable()
    {
        animation.AttackStateTransition(AttackAnimationState.BUFFER);
    }

    public void AttackFinished()
    {
        animation.AttackStateTransition(AttackAnimationState.FINISHED);
    }
}
