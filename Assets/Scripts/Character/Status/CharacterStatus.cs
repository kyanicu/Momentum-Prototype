using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterStatus : MonoBehaviour, IDamageable
{

    [SerializeField]
    private float maxHealth;

    private float _health;
    protected float health { get { return _health; } set { _health = (value > maxHealth) ? maxHealth : (value < 0) ? 0 : value; if( health == 0) Down(); } }

    [SerializeField]
    private float iFrameTime = 1f;
    private bool _iFramesActive;
    public bool iFramesActive { get { return _iFramesActive; } private set { _iFramesActive = value; } }

    #region Sibling References
    new private PlayerAnimation animation;
    private MomentumMovement movement;
    private PlayerCharacterDirector director;
    private PlayerCombat combat;
    #endregion

    protected virtual void Start()
    {
        animation = GetComponent<PlayerAnimation>();
        movement = GetComponent<MomentumMovement>();
        director = GetComponent<PlayerCharacterDirector>();
        combat = GetComponent<PlayerCombat>();
    }

    protected virtual void Down()
    {

    }

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
        animation.AnimateFlinch();
    }

    public virtual void Halt()
    {
        movement.ZeroVelocity();
    }

    public virtual void ForceUnground()
    {
        movement.ForceUnground();
    }

    public virtual void Stun(float stunTime)
    {
        director.TempLockControl(stunTime);
    }

    public virtual void TakeKinematicKnockback(Vector3 knockback, float time)
    {
        movement.SetKinematicPath(knockback, time);
    }

    public virtual void TakeDynamicKnockback(Vector3 knockback)
    {
        movement.AddImpulse(knockback);
    }

    public virtual void TakeDynamicKnockbackWithTorque(Vector3 knockback, Vector3 atPoint)
    {
        movement.AddImpulseAtPoint(knockback, atPoint);
    }

}
