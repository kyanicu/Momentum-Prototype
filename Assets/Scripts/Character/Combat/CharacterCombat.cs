﻿using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public enum AttackState { FINISHED, STARTUP, COMMITAL, BUFFERABLE }

public struct CharacterCombatControl
{
    public string attackName { get; private set; }
    public bool attackSet { get; private set; }

    public void SetAttack(string name)
    {
        attackName = name;
        attackSet = true;
    }

    public void Reset()
    {
        attackName = "";
        attackSet = false;
    }
}

[System.Serializable]
public struct AttackInitInfo
{
    public string attackName;

    public TimelineAsset attackTimeline;
}

public class CharacterCombat : MonoBehaviour, IAttacker
{
    [SerializeField]
    protected List<AttackInitInfo> attackInitInfo = new List<AttackInitInfo>();

    private Dictionary<string, AttackInitInfo> attackInitMap = new Dictionary<string, AttackInitInfo>();

    public CharacterCombatControl control = new CharacterCombatControl();

    private IDamageable _damageable;
    public IDamageable damageable { get { return _damageable; } private set { _damageable = value; } }

    private Hitbox[] hitboxes;

    public string currentAttack { get; private set; }
    public AttackState attackState { get; private set; }

    private bool attackBuffered = false;
    private string bufferedAttack = "";

    public event Action AttackFinished;

    new protected CharacterAnimation animation;
    protected CharacterMovement movement;
    protected CharacterMovementAction movementAction;
    protected CharacterMovementPhysics movementPhysics;
    protected PlayerMovementAbility movementAbility;
    protected CharacterValueOverridability overridability;

    void Awake()
    {
        hitboxes = GetComponentsInChildren<Hitbox>();
        foreach (Hitbox hb in hitboxes)
        {
            hb.SetAttacker(this);
        }
        
        foreach (AttackInitInfo initInfo in attackInitInfo)
        {
            attackInitMap.Add(initInfo.attackName, initInfo);
        }

        GetComponent<PlayableDirector>().stopped += CharacterCombat_stopped;
    }

    private void CharacterCombat_stopped(PlayableDirector obj)
    {
        //Debug.Log("Aaaa?");
        if (attackState != AttackState.FINISHED)
            AttackStateTransition(AttackState.FINISHED);
    }

    protected virtual void Start()
    {
        animation = GetComponent<CharacterAnimation>();
        movement = GetComponent<CharacterMovement>();
        movementAction = GetComponent<CharacterMovementAction>();
        movementPhysics = GetComponent<CharacterMovementPhysics>();
        movementAbility = GetComponent<PlayerMovementAbility>();
        overridability = GetComponent<CharacterValueOverridability>();

        damageable = GetComponent<IDamageable>();
    }

    void Update()
    { 
        if (control.attackSet)
        {
            switch (attackState)
            {
                case (AttackState.STARTUP):
                    if (control.attackName != currentAttack)
                    {
                        attackBuffered = true;
                        bufferedAttack = control.attackName;
                        AttackStateTransition(AttackState.FINISHED);
                    }
                    break;
                case (AttackState.BUFFERABLE):
                    attackBuffered = true;
                    bufferedAttack = control.attackName;
                    break;
                case (AttackState.FINISHED):
                    HandleAttack(control.attackName);
                    break;
            }
        }

        control.Reset();
    }

    private void HandleAttack(string name)
    {
        currentAttack = name;
        AttackStateTransition(AttackState.STARTUP);
    }

    public void AttackAnimationCommitalSignalHandler()
    {
        AttackStateTransition(AttackState.COMMITAL);
    }

    public void AttackAnimationBufferableSignalHandler()
    {
        AttackStateTransition(AttackState.BUFFERABLE);
    }

    public void AttackAnimationFinishedSignalHandler()
    {
        AttackStateTransition(AttackState.FINISHED);
    }

    private void AttackStateTransition(AttackState newState)
    {
        attackState = newState;

        switch (newState)
        {
            case (AttackState.STARTUP):
                
                attackBuffered = false;
                bufferedAttack = "";
                animation.PlayTimelinePlayable(attackInitMap[currentAttack].attackTimeline);
                break;
            case (AttackState.COMMITAL):
                break;
            case (AttackState.BUFFERABLE):
                
                break;
            case (AttackState.FINISHED):
                AttackFinished?.Invoke();
                if (attackBuffered)
                {
                    HandleAttack(bufferedAttack);
                }
                else
                    currentAttack = "";
                break;
        }
    }

    public void Flinch()
    {
        if (attackState != AttackState.FINISHED)
        {
            attackBuffered = false;
            bufferedAttack = "";
            AttackStateTransition(AttackState.FINISHED);
            
            foreach (Hitbox hb in hitboxes)
                hb.enabled = false;
        }
    }

    public AttackerInfo GetAttackerInfo()
    {
        AttackerInfo info = new AttackerInfo();

        return info;
    }
}
