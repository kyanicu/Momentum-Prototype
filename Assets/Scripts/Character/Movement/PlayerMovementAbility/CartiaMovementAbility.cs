using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KinematicCharacterController;
using System;

[System.Serializable]
public class CartiaMovementAbilityValues : CharacterOverridableValues
{
    [SerializeField]
    public float test;

    protected override float[] floatValues { get { return new float[] { test }; } set { test = value[0]; } }
}

struct CartiaMovementAbilityInput : IPlayerMovementInput
{
    public void RegisterInput(PlayerController.PlayerActions controllerActions)
    {

    }

    public void Reset()
    {
        
    }
}

[System.Serializable]
public class CartiaMovementAbility : PlayerMovementAbility, ICartiaMovementAbilityCommunication
{

    public override event Action<AbilityOverrideArgs> addingMovementOverrides;
    public override event Action<AbilityOverrideArgs> removingMovementOverrides;

    private CartiaMovementAbilityInput input;

    [SerializeField]
    CharacterOverridableAttribute<CartiaMovementAbilityValues> overridableAttribute = new CharacterOverridableAttribute<CartiaMovementAbilityValues>();

    void Awake()
    {
        input = new CartiaMovementAbilityInput();
    }

    void Reset()
    {
        overridableAttribute.baseValues.test = 7;
    }

    #region Ability Implementation

    public override void EnterMovementEffector(MovementEffector effector)
    {
        for (int i = 0; i < effector.cartiaAbilityOverrides.Count; i++)
        {
            overridableAttribute.ApplyOverride(effector.cartiaAbilityOverrides[i].item1, effector.cartiaAbilityOverrides[i].item2);
        }
    }

    public override void ExitMovementEffector(MovementEffector effector)
    {
        for (int i = 0; i < effector.cartiaAbilityOverrides.Count; i++)
        {
            overridableAttribute.RemoveOverride(effector.cartiaAbilityOverrides[i].item1, effector.cartiaAbilityOverrides[i].item2);
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