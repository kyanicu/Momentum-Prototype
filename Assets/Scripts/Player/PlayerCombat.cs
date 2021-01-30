using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct AttackInitInfo
{

    public AttackInitInfo(bool aerial)
    {
        movementOverride.movementOverrides = new List<MutableTuple<PlayerMovementValues, PlayerOverrideType>>();
        movementOverride.movementOverrides.Add(new MutableTuple<PlayerMovementValues, PlayerOverrideType> (new PlayerMovementValues() , PlayerOverrideType.Set));
        movementOverride.movementOverrides[0].item1.SetDefaultValues(movementOverride.movementOverrides[0].item2);
        if(!aerial)
            movementOverride.movementOverrides[0].item1.negateAction = 1;

        movementOverride.physicsOverrides = new List<MutableTuple<PlayerMovementPhysicsValues, PlayerOverrideType>>();
        movementOverride.physicsOverrides.Add(new MutableTuple<PlayerMovementPhysicsValues, PlayerOverrideType> (new PlayerMovementPhysicsValues() , PlayerOverrideType.Set));
        movementOverride.physicsOverrides[0].item1.SetDefaultValues(movementOverride.physicsOverrides[0].item2);
        if(!aerial)
        {
            movementOverride.physicsOverrides[0].item1.kineticFriction = 100;
            movementOverride.physicsOverrides[0].item1.gravityAccel = 0;
        }
        movementOverride.actionOverrides = new List<MutableTuple<PlayerMovementActionValues, PlayerOverrideType>>();
    
        movementOverride.alestaAbilityOverrides = new List<MutableTuple<AlestaMovementAbilityValues, PlayerOverrideType>>();
        movementOverride.nephuiAbilityOverrides = new List<MutableTuple<NephuiMovementAbilityValues, PlayerOverrideType>>();
        movementOverride.ilphineAbilityOverrides = new List<MutableTuple<IlphineMovementAbilityValues, PlayerOverrideType>>();
        movementOverride.cartiaAbilityOverrides = new List<MutableTuple<CartiaMovementAbilityValues, PlayerOverrideType>>();

    }
    
    public FullMovementOverride movementOverride;

}

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

    public event Action<FullMovementOverride> ApplyMovementOverride;
    public event Action<FullMovementOverride> RemoveMovementOverride;
    

    [SerializeField]
    private AttackInitInfo neutralAttackInitInfo; 
    [SerializeField]
    private AttackInitInfo downAttackInitInfo;
    [SerializeField]
    private AttackInitInfo upAttackInitInfo;
    [SerializeField]
    private AttackInitInfo runningAttackInitInfo;
    [SerializeField]
    private float runningAttackMinSpeed;
    [SerializeField]
    private AttackInitInfo brakingAttackInitInfo;
    [SerializeField]
    private float brakingAttackMinSpeed;
    [SerializeField]
    private AttackInitInfo neutralAerialAttackInitInfo;
    [SerializeField]
    private AttackInitInfo backAerialAttackInitInfo;
    [SerializeField]
    private AttackInitInfo downAerialAttackInitInfo;
    [SerializeField]
    private AttackInitInfo upAerialAttackInitInfo;

    private AttackInitInfo settingAttackInitInfo;

    private bool attackBuffered;

    private ReadOnlyKinematicMotor playerMotorReference;

    private ReadOnlyPlayerMovementAction playerMovementActionState;

    public PlayerCombat(GameObject _hitboxes)
    {
        hitboxes = new Hitbox[_hitboxes.transform.childCount];
        for (int i = 0; i < _hitboxes.transform.childCount; i++)
        {
            hitboxes[i] = _hitboxes.transform.GetChild(i).GetComponent<Hitbox>();
            hitboxes[i].SetAttacker(this);
        }
        SetDefaultValues();
    }

    private void SetDefaultValues()
    {
        neutralAttackInitInfo = new AttackInitInfo(false);

        downAttackInitInfo = new AttackInitInfo(false);

        upAttackInitInfo = new AttackInitInfo(false);

        runningAttackInitInfo = new AttackInitInfo(false);
        runningAttackInitInfo.movementOverride.physicsOverrides[0].item1.kineticFriction = 0;
        runningAttackMinSpeed = 14;

        brakingAttackInitInfo = new AttackInitInfo(false);
        brakingAttackMinSpeed = 10;

        neutralAerialAttackInitInfo = new AttackInitInfo(true);

        backAerialAttackInitInfo = new AttackInitInfo(true);

        downAerialAttackInitInfo = new AttackInitInfo(true);

        upAerialAttackInitInfo = new AttackInitInfo(true);
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
            ApplyAttackInitInfo(settingAttackInitInfo);
        }
        else if (newState == AttackAnimationState.FINISHED)
        {
            ResetAttackInitInfo(settingAttackInitInfo);
        }
    }

    private void NeutralAttack()
    {
        settingAttackInitInfo = neutralAttackInitInfo;
        neutralAttack?.Invoke();
    }

    private void DownAttack()
    {
        settingAttackInitInfo = downAttackInitInfo;
        downAttack?.Invoke();
    }

    private void UpAttack()
    {
        settingAttackInitInfo = upAttackInitInfo;
        upAttack?.Invoke();
    }

    private void RunningAttack()
    {
        settingAttackInitInfo = runningAttackInitInfo;
        runningAttack?.Invoke();
    }

    private void BrakingAttack()
    {
        settingAttackInitInfo = brakingAttackInitInfo;
        brakingAttack?.Invoke();
    }

    private void NeutralAerialAttack()
    {
        settingAttackInitInfo = neutralAerialAttackInitInfo;
        neutralAerialAttack?.Invoke();
    }

    private void UpAerialAttack()
    {
        settingAttackInitInfo = upAerialAttackInitInfo;
        upAerialAttack?.Invoke();
    }

    private void DownAerialAttack()
    {
        settingAttackInitInfo = downAerialAttackInitInfo;
        downAerialAttack?.Invoke();
    }

    private void BackAerialAttack()
    {
        settingAttackInitInfo = backAerialAttackInitInfo;
        backAerialAttack?.Invoke();
    }


    private void ApplyAttackInitInfo(AttackInitInfo info)
    {
        ApplyMovementOverride?.Invoke(info.movementOverride);
    }

    private void ResetAttackInitInfo(AttackInitInfo info)
    {
        RemoveMovementOverride?.Invoke(info.movementOverride);
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

    public void TakeKinematicRecoil(Vector3 knockback, float time)
    {
        throw new NotImplementedException();
    }

    public void TakeDynamicRecoil(Vector3 knockback, bool withTorque = false)
    {
        throw new NotImplementedException();
    }
}
