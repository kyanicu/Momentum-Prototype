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

}
