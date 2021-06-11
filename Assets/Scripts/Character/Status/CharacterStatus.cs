using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;
using System;

public enum DamageArmor { None, Intangibility, Imovability, Invulnerability, }

public abstract class CharacterStatus : MonoBehaviour, IDamageable
{

    [SerializeField]
    private float maxHealth;

    private float _health;
    protected float health { get { return _health; } set { _health = (value > maxHealth) ? maxHealth : (value < 0) ? 0 : value; if (health == 0) Down(); } }

    public DamageArmor armor { get; set; }

    [SerializeField]
    private float iFrameTime = 1f;
    private bool _iFramesActive;
    public bool iFramesActive { get { return _iFramesActive; } private set { _iFramesActive = value; } }

    [SerializeField]
    TimelineAsset flinchPlayable;

    #region Sibling References
    new private CharacterAnimation animation;
    private CharacterMovement movement;
    private CharacterDirector director;
    private CharacterCombat combat;
    #endregion

    private Hurtbox[] hurtboxes;

    protected virtual void Awake()
    {
        hurtboxes = GetComponentsInChildren<Hurtbox>();
        foreach (Hurtbox hb in hurtboxes)
        {
            hb.SetDamageable(this);
        }
    }

    protected virtual void Start()
    {
        animation = GetComponent<CharacterAnimation>();
        movement = GetComponent<CharacterMovement>();
        director = GetComponent<CharacterDirector>();
        combat = GetComponent<CharacterCombat>();
    }

    protected abstract void Down();

    public virtual HitValidity ValidHit(Hitbox hitbox, Hurtbox hurtbox)
    {
        if (iFramesActive)
            return HitValidity.IFRAME;
        else
            return HitValidity.VALID;
    }

    public virtual void TakeDamage(float damage)
    {
        Debug.Log("Took " + damage + "Damage");
        health -= damage;
    }

    public virtual void ActivateIFrames(float iFrameTimeOverride = 0)
    {
        Debug.Log("iFramesActive for " + ((iFrameTimeOverride != 0) ? iFrameTimeOverride : iFrameTime) + " Seconds");
        GameManager.Instance.TimerViaGameTime((iFrameTimeOverride != 0) ? iFrameTimeOverride : iFrameTime, DeactivateIFrames);
        iFramesActive = true;
        animation.StartIFrames();
    }

    public virtual void DeactivateIFrames()
    {
        iFramesActive = false;
        animation.EndIFrames();
    }

    public virtual void Flinch()
    {
        Debug.Log("Flinched");
        movement.Flinch();
        combat.Flinch();
        animation.PlayTimelinePlayable(flinchPlayable);
    }

    public virtual void Halt()
    {
        Debug.Log("Halted");
        movement.ZeroVelocity();
    }

    public virtual void ForceUnground()
    {
        Debug.Log("Force Ungrounded");
        movement.ForceUnground();
    }

    public virtual void Stun(float stunTime)
    {
        Debug.Log("Stunned for: " + stunTime + " seconds");
        director.TempLockControl(stunTime);
    }

    public virtual void TakeKinematicKnockback(Vector3 knockback, float time)
    {
        Debug.Log("Took Kinematic Knockback");
        movement.SetKinematicPath(knockback, time);
    }

    public virtual void TakeDynamicKnockback(Vector3 knockback)
    {
        Debug.Log("Took Dynamic Knockback");
        movement.AddImpulse(knockback);
    }

    public virtual void TakeDynamicKnockbackWithTorque(Vector3 knockback, Vector3 atPoint)
    {
        Debug.Log("Took Dynamic Knockback With Torque");
        movement.AddImpulseAtPoint(knockback, atPoint);
    }

}
