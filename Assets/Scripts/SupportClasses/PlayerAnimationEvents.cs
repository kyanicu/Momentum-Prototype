using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AttackAnimationState { FINISHED, STARTUP, CONTACT, RECOVERY, BUFFER }

public class PlayerAnimationEvents : MonoBehaviour
{

    public event Action<AttackAnimationState> attackStateTransition;
    

    public void AttackStartup()
    {
        attackStateTransition?.Invoke(AttackAnimationState.STARTUP);
    }

    public void AttackContact()
    {
        attackStateTransition?.Invoke(AttackAnimationState.CONTACT);
    }

    public void AttackRecovery()
    {
        attackStateTransition?.Invoke(AttackAnimationState.RECOVERY);
    }

    public void AttackBufferable()
    {
        attackStateTransition?.Invoke(AttackAnimationState.BUFFER);
    }

    public void AttackFinished()
    {
        attackStateTransition?.Invoke(AttackAnimationState.FINISHED);
    }
}
