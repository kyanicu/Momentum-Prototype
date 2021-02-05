using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStatus : MonoBehaviour, IDamageable
{
    [SerializeField]
    private float maxHealth;
    [SerializeField]
    private float maxVitality;

    private float _health;
    private float health { get { return _health; } set { _health = (value > maxHealth) ? maxHealth : (value < 0) ? 0 : value; if( health == 0) Down(); } }

    [SerializeField]
    private float iFrameTime = 1f;
    private bool _iFramesActive;
    public bool iFramesActive { get { return _iFramesActive; } private set { _iFramesActive = value; } }

    [SerializeField, HideInInspector]
    private GameObject hurtboxes;

    #region Communications
    private IPlayerAnimationCommunication animationCommunication;
    private IPlayerMovementCommunication movementCommunication;
    private IPlayerCharacterCommunication characterCommunication;
    private IPlayerCombatCommunication combatCommunication;
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
        animationCommunication = GetComponent<IPlayerAnimationCommunication>();
        movementCommunication = GetComponent<IPlayerMovementCommunication>();
        characterCommunication = GetComponent<IPlayerCharacterCommunication>();
        combatCommunication = GetComponent<IPlayerCombatCommunication>();
    }

    private void Down()
    {

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
        animationCommunication.StartIFrames();
    }

    public void DeactivateIFrames()
    {
        iFramesActive = false;
        animationCommunication.EndIFrames();
    }

    public void Flinch()
    {
        Debug.Log("Flinched");
        movementCommunication.Flinch();
        combatCommunication.Flinch();
        animationCommunication.AnimateFlinch();
    }

    public void Halt()
    {
        Debug.Log("Halted");
        movementCommunication.ZeroVelocity(true);
    }

    public void ForceUnground()
    {
        Debug.Log("Force Ungrounded");
        movementCommunication.ForceUnground();
    }

    public void Stun(float stunTime)
    {
        Debug.Log("Stunned");
        characterCommunication.LockInput(stunTime);
    }

    public void TakeKinematicKnockback(Vector3 knockback, float time)
    {
        Debug.Log("Took Kinematic Knockback for " + time + "Seconds");
        Debug.DrawRay(GameObject.Find("Alesta").transform.position, knockback, Color.yellow, 5);
        movementCommunication.SetKinematicPath(knockback, time);
    }

    public void TakeDynamicKnockback(Vector3 knockback)
    {
        Debug.Log("Took Dynamic Knockback");
        Debug.DrawRay(GameObject.Find("Alesta").transform.position, knockback, Color.yellow + Color.red, 5);
        movementCommunication.AddImpulse(knockback);
    }

    public void TakeDynamicKnockbackWithTorque(Vector3 knockback, Vector3 atPoint)
    {
        Debug.Log("Took Dynamic Knockback With Torque");
        Debug.DrawRay(atPoint, knockback, Color.red, 5);
        movementCommunication.AddImpulseAtPoint(knockback, atPoint);
    }
}
