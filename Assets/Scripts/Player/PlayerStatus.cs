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
    private float _iFrameTimer;
    public float iFrameTimer { get { return _iFrameTimer; } private set { _iFrameTimer = value; } }

    [SerializeField, HideInInspector]
    private GameObject hurtboxes;

    public PlayerStatus(GameObject _hurtboxes)
    {
        hurtboxes = _hurtboxes;

        Hurtbox[] hbs = hurtboxes.GetComponentsInChildren<Hurtbox>();
        foreach (Hurtbox hb in hbs)
        {
            hb.SetDamageable(this);
        }
    }

    public void HandleIncommingAttack(AttackInfo attackInfo)
    {
        health -= attackInfo.baseDamage;
    }

    public void SetCommunicationInterface(PlayerInternalCommunicator communicator)
    {
        communicator.SetCommunication(this);
    }

    public void HandleTriggerEnter(Collider col)
    {
        if (col.tag == "Crystal")
        {
            GameObject.Destroy(col.gameObject);
        }
    }

    private void Down()
    {

    }

    public HitValidity ValidHit(Hitbox hitbox, Hurtbox hurtbox)
    {
        if (iFrameTimer > 0)
            return HitValidity.IFRAME;
        else
            return HitValidity.VALID;
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
    }

    public void ActivateIFrames(float iFrameTimeOverride = 0)
    {
        iFrameTimer = (iFrameTimeOverride != 0) ? iFrameTimeOverride : iFrameTime;
    }

    public void Flinch()
    {
        throw new System.NotImplementedException();
    }

    public void Halt()
    {
        throw new System.NotImplementedException();
    }

    public void ForceUnground()
    {
        throw new System.NotImplementedException();
    }

    public void Stun(float stunTime)
    {
        throw new System.NotImplementedException();
    }

    public void TakeKinematicKnockback(Vector3 knockback, float time)
    {
        throw new System.NotImplementedException();
    }

    public void TakeDynamicKnockback(Vector3 knockback, bool withTorque = false)
    {
        throw new System.NotImplementedException();
    }
}
