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

public struct CartiaMovementAbilityControl : IAbilityControl
{

    public void Reset()
    {
        
    }
}

public class CartiaMovementAbility : PlayerMovementAbility
{

    public CartiaMovementAbilityControl control;

    [SerializeField]
    CharacterOverridableAttribute<CartiaMovementAbilityValues> overridableAttribute = new CharacterOverridableAttribute<CartiaMovementAbilityValues>();

    void Reset()
    {
        overridableAttribute.baseValues.test = 7;
    }

    void Awake()
    {
        control = new CartiaMovementAbilityControl();
        controlInterface = control;
    }

    void Start()
    {
        GetComponent<ICharacterValueOverridabilityCommunication>()?.RegisterOverridability(overridableAttribute);
    }
    
#region Ability Implementation

    public override void Flinch()
    {
        controlInterface.Reset();
    }

#endregion
}