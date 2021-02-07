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

public struct NephuiMovementAbilityControl : IAbilityControl
{

    public void Reset()
    {
        
    }
}

public class NephuiMovementAbility : PlayerMovementAbility
{

    public NephuiMovementAbilityControl control;

    [SerializeField]
    CharacterOverridableAttribute<NephuiMovementAbilityValues> overridableAttribute = new CharacterOverridableAttribute<NephuiMovementAbilityValues>();

    void Reset()
    {
        overridableAttribute.baseValues.test = 7;
    }

    void Awake()
    {
        control = new NephuiMovementAbilityControl();
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