using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerStatus : IPlayerStatusCommunication, IDamageable
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

    public event Action flinch;
    public event Action halt;
    public event Action forceUnground;
    public event Action<float> stun;
    public event Action<Vector3, float> takeKinematicKnockback;
    public event Action<Vector3> takeDynamicKnockback;
    public event Action<Vector3, Vector3> takeDynamicKnockbackWithTorque;

    public event Action iFramesStarted;
    public event Action iFramesEnded;

    public PlayerStatus(GameObject _hurtboxes)
    {
        hurtboxes = _hurtboxes;

        Hurtbox[] hbs = hurtboxes.GetComponentsInChildren<Hurtbox>();
        foreach (Hurtbox hb in hbs)
        {
            hb.SetDamageable(this);
        }
    }

    public void SetCommunicationInterface(PlayerInternalCommunicator communicator)
    {
        communicator.SetCommunication(this);
    }

    public void HandleTriggerEnter(Collider col)
    {

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
        iFramesStarted?.Invoke();
    }

    public void DeactivateIFrames()
    {
        iFramesActive = false;
        iFramesEnded?.Invoke();
    }

    public void Flinch()
    {
        Debug.Log("Flinched");
        flinch?.Invoke();
    }

    public void Halt()
    {
        Debug.Log("Halted");
        halt?.Invoke();
    }

    public void ForceUnground()
    {
        Debug.Log("Force Ungrounded");
        forceUnground?.Invoke();
    }

    public void Stun(float stunTime)
    {
        Debug.Log("Stunned");
        stun?.Invoke(stunTime);
    }

    public void TakeKinematicKnockback(Vector3 knockback, float time)
    {
        Debug.Log("Took Kinematic Knockback for " + time + "Seconds");
        Debug.DrawRay(GameObject.Find("Alesta").transform.position, knockback, Color.yellow, 5);
        takeKinematicKnockback?.Invoke(knockback, time);
    }

    public void TakeDynamicKnockback(Vector3 knockback)
    {
        Debug.Log("Took Dynamic Knockback");
        Debug.DrawRay(GameObject.Find("Alesta").transform.position, knockback, Color.yellow + Color.red, 5);
        takeDynamicKnockback?.Invoke(knockback);
    }

    public void TakeDynamicKnockbackWithTorque(Vector3 knockback, Vector3 atPoint)
    {
        Debug.Log("Took Dynamic Knockback With Torque");
        Debug.DrawRay(atPoint, knockback, Color.red, 5);
        takeDynamicKnockbackWithTorque?.Invoke(knockback, atPoint);
    }
}
