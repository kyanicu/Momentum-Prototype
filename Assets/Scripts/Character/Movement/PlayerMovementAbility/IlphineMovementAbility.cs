using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KinematicCharacterController;
using System;

[System.Serializable]
public class IlphineMovementAbilityValues : CharacterOverridableValues
{
    [SerializeField]
    public float test;

    protected override float[] floatValues { get { return new float[] { test }; } set { test = value[0]; } }
}

struct IlphineMovementAbilityInput : IPlayerMovementInput
{
    public void RegisterInput(PlayerController.PlayerActions controllerActions)
    {

    }

    public void Reset()
    {
        
    }
}

[System.Serializable]
public class IlphineMovementAbility : PlayerMovementAbility, IIlphineMovementAbilityCommunication
{

    public override event Action<AbilityOverrideArgs> addingMovementOverrides;
    public override event Action<AbilityOverrideArgs> removingMovementOverrides;

    private IlphineMovementAbilityInput input;

    [SerializeField]
    CharacterOverridableAttribute<IlphineMovementAbilityValues> overridableAttribute = new CharacterOverridableAttribute<IlphineMovementAbilityValues>();

    void Awake()
    {
        input = new IlphineMovementAbilityInput();
    }

    void Reset()
    {
        overridableAttribute.baseValues.test = 7;
    }
    
    #region Ability Implementation

    public override void EnterMovementEffector(MovementEffector effector)
    {
        for (int i = 0; i < effector.ilphineAbilityOverrides.Count; i++)
        {
            overridableAttribute.ApplyOverride(effector.ilphineAbilityOverrides[i].item1, effector.ilphineAbilityOverrides[i].item2);
        }
    }

    public override void ExitMovementEffector(MovementEffector effector)
    {
        for (int i = 0; i < effector.ilphineAbilityOverrides.Count; i++)
        {
            overridableAttribute.RemoveOverride(effector.ilphineAbilityOverrides[i].item1, effector.ilphineAbilityOverrides[i].item2);
        }
    }

    public override void Flinch()
    {
        
    }

    public override void RegisterInput(PlayerController.PlayerActions controllerActions)
    {
        input.RegisterInput(controllerActions);
    }

    /// <summary>
    /// Resets the input state
    /// </summary>
    public override void ResetInput()
    {
        input.Reset();
    }
#endregion
}