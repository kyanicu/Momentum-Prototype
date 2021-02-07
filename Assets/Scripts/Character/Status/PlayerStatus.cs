using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStatus : CharacterStatus, IDamageable
{
    [SerializeField]
    private float maxVitality;

    [SerializeField]
    private float iFrameTime = 1f;
    private bool _iFramesActive;
    public bool iFramesActive { get { return _iFramesActive; } private set { _iFramesActive = value; } }

    [SerializeField, HideInInspector]
    private GameObject hurtboxes;

    #region Sibling References
    new private PlayerAnimation animation;
    private PlayerMovement movement;
    private PlayerCharacterDirector character;
    private PlayerCombat combat;
    #endregion

    void Awake()
    {
        hurtboxes = transform.GetChild(0).GetChild(2).gameObject;

        Hurtbox[] hbs = hurtboxes.GetComponentsInChildren<Hurtbox>();
        foreach (Hurtbox hb in hbs)
        {
            hb.SetDamageable(this);
        }
    }

    void Start()
    {
        animation = GetComponent<PlayerAnimation>();
        movement = GetComponent<PlayerMovement>();
        character = GetComponent<PlayerCharacterDirector>();
        combat = GetComponent<PlayerCombat>();
    }

    public HitValidity ValidHit(Hitbox hitbox, Hurtbox hurtbox)
    {
        if (iFramesActive)
            return HitValidity.IFRAME;
        else
            return HitValidity.VALID;
    }

    public void TakeDamage(float damage)
    {
        Debug.Log("Took " + damage + "Damage");
        health -= damage;
    }

    public void ActivateIFrames(float iFrameTimeOverride = 0)
    {
        Debug.Log("iFramesActive for " + ((iFrameTimeOverride != 0) ? iFrameTimeOverride : iFrameTime) + " Seconds");
        GameManager.Instance.TimerViaGameTime((iFrameTimeOverride != 0) ? iFrameTimeOverride : iFrameTime, DeactivateIFrames);
        iFramesActive = true;
        animation.StartIFrames();
    }

    public void DeactivateIFrames()
    {
        iFramesActive = false;
        animation.EndIFrames();
    }

    public void Flinch()
    {
        Debug.Log("Flinched");
        movement.Flinch();
        combat.Flinch();
        animation.AnimateFlinch();
    }

    public void Halt()
    {
        Debug.Log("Halted");
        movement.ZeroVelocity(true);
    }

    public void ForceUnground()
    {
        Debug.Log("Force Ungrounded");
        movement.ForceUnground();
    }

    public void Stun(float stunTime)
    {
        Debug.Log("Stunned");
        character.TempLockControl(stunTime);
    }

    public void TakeKinematicKnockback(Vector3 knockback, float time)
    {
        Debug.Log("Took Kinematic Knockback for " + time + "Seconds");
        Debug.DrawRay(GameObject.Find("Alesta").transform.position, knockback, Color.yellow, 5);
        movement.SetKinematicPath(knockback, time);
    }

    public void TakeDynamicKnockback(Vector3 knockback)
    {
        Debug.Log("Took Dynamic Knockback");
        Debug.DrawRay(GameObject.Find("Alesta").transform.position, knockback, Color.yellow + Color.red, 5);
        movement.AddImpulse(knockback);
    }

    public void TakeDynamicKnockbackWithTorque(Vector3 knockback, Vector3 atPoint)
    {
        Debug.Log("Took Dynamic Knockback With Torque");
        Debug.DrawRay(atPoint, knockback, Color.red, 5);
        movement.AddImpulseAtPoint(knockback, atPoint);
    }
}
