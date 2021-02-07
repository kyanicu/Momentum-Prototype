using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStatus : CharacterStatus
{
    [SerializeField]
    private float maxVitality;

    [SerializeField, HideInInspector]
    private GameObject hurtboxes;

    void Awake()
    {
        hurtboxes = transform.GetChild(0).GetChild(2).gameObject;

        Hurtbox[] hbs = hurtboxes.GetComponentsInChildren<Hurtbox>();
        foreach (Hurtbox hb in hbs)
        {
            hb.SetDamageable(this);
        }
    }

    protected override void Start()
    {
        base.Start();
    }
}
