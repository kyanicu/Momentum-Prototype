using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KinematicCharacterController;
using System;

[System.Serializable]
public class NephuiMovementAbilityValues : CharacterOverridableValues
{
    [SerializeField]
    public float test;

    protected override float[] floatValues { get { return new float[] { test }; } set { test = value[0]; } }
}

struct NephuiMovementAbilityInput : IPlayerMovementInput
{
    public void RegisterInput(PlayerController.PlayerActions controllerActions)
    {

    }

    public void Reset()
    {
        
    }
}

[System.Serializable]
public class NephuiMovementAbility : PlayerMovementAbility, INephuiMovementAbilityCommunication
{

    public override event Action<AbilityOverrideArgs> addingMovementOverrides;
    public override event Action<AbilityOverrideArgs> removingMovementOverrides;

    private NephuiMovementAbilityInput input;

    [SerializeField]
    CharacterOverridableAttribute<NephuiMovementAbilityValues> overridableAttribute = new CharacterOverridableAttribute<NephuiMovementAbilityValues>();

    void Awake()
    {
        input = new NephuiMovementAbilityInput();
    }

    void Reset()
    {
        overridableAttribute.baseValues.test = 7;
    }

    #region Ability Implementation

    public override void EnterMovementEffector(MovementEffector effector)
    {
        for (int i = 0; i < effector.nephuiAbilityOverrides.Count; i++)
        {
            overridableAttribute.ApplyOverride(effector.nephuiAbilityOverrides[i].item1, effector.nephuiAbilityOverrides[i].item2);
        }
    }

    public override void ExitMovementEffector(MovementEffector effector)
    {
        for (int i = 0; i < effector.nephuiAbilityOverrides.Count; i++)
        {
            overridableAttribute.RemoveOverride(effector.nephuiAbilityOverrides[i].item1, effector.nephuiAbilityOverrides[i].item2);
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