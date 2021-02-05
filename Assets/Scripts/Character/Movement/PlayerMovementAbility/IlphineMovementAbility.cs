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
public class IlphineMovementAbility : PlayerMovementAbility
{
    private IlphineMovementAbilityInput input;

    [SerializeField]
    CharacterOverridableAttribute<IlphineMovementAbilityValues> overridableAttribute = new CharacterOverridableAttribute<IlphineMovementAbilityValues>();

    void Reset()
    {
        overridableAttribute.baseValues.test = 7;
    }

    void Awake()
    {
        input = new IlphineMovementAbilityInput();
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