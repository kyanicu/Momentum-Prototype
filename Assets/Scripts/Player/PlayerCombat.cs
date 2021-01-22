using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerCombat : IPlayerCombatCommunication, IAttacker
{

    [SerializeField, HideInInspector]
    private Hitbox[] hitboxes;

    AttackAnimationState attackAnimationState = AttackAnimationState.FINISHED;

    public event Action neutralAttack;
    public event Action downAttack;
    public event Action upAttack;
    public event Action runningAttack;
    public event Action brakingAttack;
    public event Action neutralAerialAttack;
    public event Action backAerialAttack;
    public event Action downAerialAttack;
    public event Action upAerialAttack;

    [SerializeField]
    private AttackInfo[] neutralAttackHitboxInfo; 
    [SerializeField]
    private AttackInfo[] downAttackHitboxInfo;
    [SerializeField]
    private AttackInfo[] upAttackHitboxInfo;
    [SerializeField]
    private AttackInfo[] runningAttackHitboxInfo;
    [SerializeField]
    private float runningAttackMinSpeed;
    [SerializeField]
    private AttackInfo[] brakingAttackHitboxInfo;
    [SerializeField]
    private float brakingAttackMinSpeed;
    [SerializeField]
    private AttackInfo[] neutralAerialAttackHitboxInfo;
    [SerializeField]
    private AttackInfo[] backAerialAttackHitboxInfo;
    [SerializeField]
    private AttackInfo[] downAerialAttackHitboxInfo;
    [SerializeField]
    private AttackInfo[] upAerialAttackHitboxInfo;

    private AttackInfo[] settingHitBoxInfo;

    private bool attackBuffered;

    private ReadOnlyKinematicMotor playerMotorReference;

    private ReadOnlyPlayerMovementAction playerMovementActionState;

    public PlayerCombat(GameObject _hitboxes)
    {
        hitboxes = new Hitbox[_hitboxes.transform.childCount];
        for (int i = 0; i < _hitboxes.transform.childCount; i++)
        {
            Debug.Log("hIt");
            hitboxes[i] = _hitboxes.transform.GetChild(i).GetComponent<Hitbox>();
            hitboxes[i].SetAttacker(this);
        }
        SetDefaultValues();
    }

    private void SetDefaultValues()
    {
        neutralAttackHitboxInfo = new AttackInfo[1];
        neutralAttackHitboxInfo[0].baseDamage = 5;

        downAttackHitboxInfo = new AttackInfo[1];
        downAttackHitboxInfo[0].baseDamage = 5;

        upAttackHitboxInfo = new AttackInfo[1];
        upAttackHitboxInfo[0].baseDamage = 5;

        runningAttackHitboxInfo = new AttackInfo[2];
        runningAttackHitboxInfo[0].baseDamage = 5;
        runningAttackHitboxInfo[1].baseDamage = 5;
        runningAttackMinSpeed = 14;

        brakingAttackHitboxInfo = new AttackInfo[2];
        brakingAttackHitboxInfo[0].baseDamage = 5;
        brakingAttackHitboxInfo[1].baseDamage = 5;
        brakingAttackMinSpeed = 10;

        neutralAerialAttackHitboxInfo = new AttackInfo[1];
        neutralAerialAttackHitboxInfo[0].baseDamage = 5;

        backAerialAttackHitboxInfo = new AttackInfo[1];
        backAerialAttackHitboxInfo[0].baseDamage = 5;

        downAerialAttackHitboxInfo = new AttackInfo[1];
        downAerialAttackHitboxInfo[0].baseDamage = 5;

        upAerialAttackHitboxInfo = new AttackInfo[1];
        upAerialAttackHitboxInfo[0].baseDamage = 5;
    }

    public void SetCommunicationInterface(PlayerInternalCommunicator communicator)
    {
        communicator.SetCommunication(this);
    }

    public void SetReadOnlyReferences(ReadOnlyKinematicMotor motor, ReadOnlyPlayerMovementAction action)
    {
        playerMotorReference = motor;
        playerMovementActionState = action;
    }

    public void AttackAnimationStateTransition(AttackAnimationState newState)
    {
        attackAnimationState = newState;

        if (newState == AttackAnimationState.STARTUP)
        {
            attackBuffered = false;
            SetHitboxAttackInfo(settingHitBoxInfo);
        }
    }

    private void NeutralAttack()
    {
        settingHitBoxInfo = neutralAttackHitboxInfo;
        neutralAttack?.Invoke();
    }

    private void DownAttack()
    {
        settingHitBoxInfo = downAttackHitboxInfo;
        downAttack?.Invoke();
    }

    private void UpAttack()
    {
        settingHitBoxInfo = upAttackHitboxInfo;
        upAttack?.Invoke();
    }

    private void RunningAttack()
    {
        settingHitBoxInfo = runningAttackHitboxInfo;
        runningAttack?.Invoke();
    }

    private void BrakingAttack()
    {
        settingHitBoxInfo = brakingAttackHitboxInfo;
        brakingAttack?.Invoke();
    }

    private void NeutralAerialAttack()
    {
        settingHitBoxInfo = neutralAerialAttackHitboxInfo;
        neutralAerialAttack?.Invoke();
    }

    private void UpAerialAttack()
    {
        settingHitBoxInfo = upAerialAttackHitboxInfo;
        upAerialAttack?.Invoke();
    }

    private void DownAerialAttack()
    {
        settingHitBoxInfo = downAerialAttackHitboxInfo;
        downAerialAttack?.Invoke();
    }

    private void BackAerialAttack()
    {
        settingHitBoxInfo = backAerialAttackHitboxInfo;
        backAerialAttack?.Invoke();
    }


    private void SetHitboxAttackInfo(AttackInfo[] attackInfo)
    {
        int len = attackInfo.Length;
        for (int i = 0; i < len; i++)
        {
            hitboxes[i].SetAttackInfo(attackInfo[i]);
        }
    }

    public void HandleOutgoingAttack(AttackInfo attackInfo)
    {
        
    }

    public AttackerInfo GetAttackerInfo()
    {
        AttackerInfo info = new AttackerInfo();

        return info;
    }

    public void HandleInput(PlayerController.PlayerActions controllerActions)
    {
        if (controllerActions.NeutralAttack.triggered && !attackBuffered &&
            (attackAnimationState == AttackAnimationState.FINISHED || (attackBuffered = (attackAnimationState == AttackAnimationState.BUFFER))))
        {
            bool grounded = playerMotorReference.isGroundedThisUpdate;
            float sqrSpeed = playerMotorReference.velocity.sqrMagnitude;

            float horizDir = controllerActions.Run.ReadValue<float>();
            float vertDir = controllerActions.VerticalDirection.ReadValue<float>();

            if (grounded)
            {
                if (playerMovementActionState.isBraking && sqrSpeed >= brakingAttackMinSpeed * brakingAttackMinSpeed)
                    BrakingAttack();
                else if (sqrSpeed >= runningAttackMinSpeed * runningAttackMinSpeed && horizDir == playerMovementActionState.facingDirection)
                    RunningAttack();
                else if (vertDir == +1)
                    UpAttack();
                else if (vertDir == -1)
                    DownAttack();
                else
                    NeutralAttack();
            }
            else
            {
                if (vertDir == +1)
                    UpAerialAttack();
                else if (vertDir == -1)
                    DownAerialAttack();
                else if (horizDir == -playerMovementActionState.facingDirection)
                    BackAerialAttack();
                else
                    NeutralAerialAttack();
            } 
        }
    }
}
