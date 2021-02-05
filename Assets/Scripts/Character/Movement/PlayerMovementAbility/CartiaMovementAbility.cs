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
public class CartiaMovementAbility : PlayerMovementAbility
{
    private CartiaMovementAbilityInput input;

    [SerializeField]
    CharacterOverridableAttribute<CartiaMovementAbilityValues> overridableAttribute = new CharacterOverridableAttribute<CartiaMovementAbilityValues>();

    void Reset()
    {
        overridableAttribute.baseValues.test = 7;
    }

    void Awake()
    {
        input = new CartiaMovementAbilityInput();
    }

    void Start()
    {
        GetComponent<ICharacterValueOverridabilityCommunication>()?.RegisterOverridability(overridableAttribute);
    }

    #region Ability Implementation

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