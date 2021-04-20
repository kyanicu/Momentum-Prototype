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

public struct IlphineMovementAbilityControl : IAbilityControl
{

    public void Reset()
    {
        
    }
}

public class IlphineMovementAbility : PlayerMovementAbility
{

    public IlphineMovementAbilityControl control;

    [SerializeField]
    CharacterOverridableAttribute<IlphineMovementAbilityValues> overridableAttribute = new CharacterOverridableAttribute<IlphineMovementAbilityValues>();

    void Reset()
    {
        overridableAttribute.baseValues.test = 7;
    }

    void Awake()
    {
        control = new IlphineMovementAbilityControl();
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