using System.Collections;
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
    [SerializeField]
    private IDamageable _damageable;

    public IDamageable damageable { get { return _damageable; } private set { _damageable = value; } }

    public void SetDamageable(IDamageable d)
    {
        damageable = d;
    }

    public void HandleIncommingAttack(Hitbox hitbox)
    {
        Hitbox.HandleAttackInfo(hitbox.attackInfo, damageable, hitbox.transform, this.transform);
    }
}
