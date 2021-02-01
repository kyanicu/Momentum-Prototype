﻿using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public enum HitValidity
{
    VALID,
    IFRAME,
    BLOCK,
    PHASE,
    CLANK,
    TEMPORARY_IMMUNITY,
    PERMANENT_IMMUNITY,

}

public interface IDamageable
{
    HitValidity ValidHit(Hitbox hitbox, Hurtbox hurtbox);

    void TakeDamage(float damage);
    void ActivateIFrames(float iFrameTimeOverride = 0);
    void Flinch();
    void Halt();
    void ForceUnground();
    void Stun(float stunTime);

    void TakeKinematicKnockback(Vector3 knockback, float time);
    void TakeDynamicKnockback(Vector3 knockback);
    void TakeDynamicKnockbackWithTorque(Vector3 knockback, Vector3 atPoint);

}

public class Hurtbox : MonoBehaviour
{
    private IDamageable _damageable;

    public IDamageable damageable { get { return _damageable; } private set { _damageable = value; } }

    public void SetDamageable(IDamageable d)
    {
        damageable = d;
    }

    public void HandleIncommingAttack(Hitbox hitbox)
    {
        AttackInfo attackInfo = hitbox.attackInfo;
        
        if(attackInfo.baseDamage != 0)
            damageable.TakeDamage(attackInfo.baseDamage);
        
        if (attackInfo.activateIFrames)
            damageable.ActivateIFrames(attackInfo.iFrameTimeOverride);

        if (attackInfo.flinch)
            damageable.Flinch();
        if (attackInfo.halt)
            damageable.Halt();
        if (attackInfo.forceUnground)
            damageable.ForceUnground();
        if (attackInfo.stunTime != 0)
            damageable.Stun(attackInfo.stunTime);
        
        if (attackInfo.knockbackType != KnockbackType.STATIC)
        {

            Vector3 calculatedKnockback = attackInfo.baseKnockbackDirection * attackInfo.baseKnockbackSpeed;
            switch (attackInfo.knockbackDirectionCalculation)
            {
                case (KnockbackDirectionCalculation.LOCAL_HITBOX) :
                    calculatedKnockback = hitbox.transform.TransformDirection(calculatedKnockback);
                    break;
                case (KnockbackDirectionCalculation.LOCAL_HURTBOX) :
                    calculatedKnockback = transform.TransformDirection(calculatedKnockback);
                    break;
                case (KnockbackDirectionCalculation.RADIAL) :
                    calculatedKnockback = Quaternion.FromToRotation(Vector3.right, (transform.position - hitbox.transform.position).normalized) * calculatedKnockback;
                    break;
            }

            switch (attackInfo.knockbackType)
            {
                case (KnockbackType.KINEMATIC) :
                    damageable.TakeKinematicKnockback(calculatedKnockback, attackInfo.kinematicKnockbackTime);
                    break;
                case (KnockbackType.DYNAMIC) :
                    damageable.TakeDynamicKnockback(calculatedKnockback);
                    break;
                case (KnockbackType.DYNAMIC_WITH_TORQUE) :
                    damageable.TakeDynamicKnockbackWithTorque(calculatedKnockback, hitbox.transform.position);
                    break;
            }
        }
    }
}