using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
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

/// <summary>
/// Communication interface for player combat
/// </summary>
public interface IPlayerCombatCommunication
{
    void AttackAnimationStateTransition(AttackAnimationState newState);

    void Flinch();
} 

public class PlayerCombat : MonoBehaviour, IPlayerCombatCommunication, IAttacker
{
    private IDamageable _damageable;
    public IDamageable damageable { get { return _damageable; } private set { _damageable = value; } }

    [SerializeField, HideInInspector]
    private Hitbox[] hitboxes;

    AttackAnimationState attackAnimationState = AttackAnimationState.FINISHED;

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

    private bool stunned;

#region Communications
    private IPlayerAnimationCommunication animationCommunication;
    private IPlayerMovementCommunication movementCommunication;
    private IPlayerMovementActionCommunication movementActionCommunication;
    private ICharacterValueOverridabilityCommunication overridabilityCommunication;
    
#endregion

    void Awake()
    {

        GameObject _hitboxes = transform.GetChild(0).GetChild(1).gameObject;

        hitboxes = new Hitbox[_hitboxes.transform.childCount];
        for (int i = 0; i < _hitboxes.transform.childCount; i++)
        {
            hitboxes[i] = _hitboxes.transform.GetChild(i).GetComponent<Hitbox>();
            hitboxes[i].SetAttacker(this);
        }
    }

    void Start()
    {
        animationCommunication = GetComponent<IPlayerAnimationCommunication>();
        movementCommunication = GetComponent<IPlayerMovementCommunication>();
        movementActionCommunication = GetComponent<IPlayerMovementActionCommunication>();
        overridabilityCommunication = GetComponent<ICharacterValueOverridabilityCommunication>();

        damageable = GetComponent<IDamageable>();
    }

    private void Reset()
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
            settingAttackInitInfo = new AttackInitInfo();
        }
    }

    public void StartStun()
    {
        stunned = true;
    }

    public void EndStun()
    {
        stunned = false;
    }

    public void Flinch()
    {
        if (attackAnimationState != AttackAnimationState.FINISHED)
            AttackAnimationStateTransition(AttackAnimationState.FINISHED);
        attackBuffered = false;
        foreach (Hitbox hb in hitboxes)
            hb.enabled = false;
    }

    private void NeutralAttack()
    {
        settingAttackInitInfo = neutralAttackInitInfo;
        animationCommunication.AnimateNeutralAttack();
    }

    private void DownAttack()
    {
        settingAttackInitInfo = downAttackInitInfo;
        animationCommunication.AnimateDownAttack();
    }

    private void UpAttack()
    {
        settingAttackInitInfo = upAttackInitInfo;
        animationCommunication.AnimateUpAttack();
    }

    private void RunningAttack()
    {
        settingAttackInitInfo = runningAttackInitInfo;
        animationCommunication.AnimateRunningAttack();
    }

    private void BrakingAttack()
    {
        settingAttackInitInfo = brakingAttackInitInfo;
        animationCommunication.AnimateBrakingAttack();
    }

    private void NeutralAerialAttack()
    {
        settingAttackInitInfo = neutralAerialAttackInitInfo;
        animationCommunication.AnimateNeutralAerialAttack();
    }

    private void UpAerialAttack()
    {
        settingAttackInitInfo = upAerialAttackInitInfo;
        animationCommunication.AnimateUpAerialAttack();
    }

    private void DownAerialAttack()
    {
        settingAttackInitInfo = downAerialAttackInitInfo;
        animationCommunication.AnimateDownAerialAttack();
    }

    private void BackAerialAttack()
    {
        settingAttackInitInfo = backAerialAttackInitInfo;
        animationCommunication.AnimateBackAerialAttack();
    }

    private void ApplyAttackInitInfo(AttackInitInfo info)
    {
        overridabilityCommunication.ApplyFullMovementOverride(info.movementOverride);
    }

    private void ResetAttackInitInfo(AttackInitInfo info)
    {
        overridabilityCommunication.RemoveFullMovementOverride(info.movementOverride);
    }

    public AttackerInfo GetAttackerInfo()
    {
        AttackerInfo info = new AttackerInfo();

        return info;
    }

    public void HandleInput(PlayerController.PlayerActions controllerActions)
    {
        if(stunned)
            return;
        if (controllerActions.NeutralAttack.triggered && !attackBuffered &&
            (attackAnimationState == AttackAnimationState.FINISHED || (attackBuffered = (attackAnimationState == AttackAnimationState.BUFFER))))
        {
            bool grounded = movementCommunication.isGroundedThisUpdate;
            float sqrSpeed = movementCommunication.velocity.sqrMagnitude;

            float horizDir = controllerActions.Run.ReadValue<float>();
            float vertDir = controllerActions.VerticalDirection.ReadValue<float>();

            if (grounded)
            {
                if (movementActionCommunication.isBraking && sqrSpeed >= brakingAttackMinSpeed * brakingAttackMinSpeed)
                    BrakingAttack();
                else if (sqrSpeed >= runningAttackMinSpeed * runningAttackMinSpeed && horizDir == movementActionCommunication.facingDirection)
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
                else if (horizDir == -movementActionCommunication.facingDirection)
                    BackAerialAttack();
                else
                    NeutralAerialAttack();
            } 
        }
    }
}
