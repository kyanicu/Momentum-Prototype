﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KinematicCharacterController;
using System;

[System.Serializable]
public class AlestaMovementAbilityValues : CharacterOverridableValues
{
    [SerializeField]
    public float permeationPushAccel;
    [SerializeField]
    public float permeationResistFactor;
    [SerializeField]
    public float permeationGravityFactor;
    [SerializeField]
    public float permeationVelocityThreshold;

    protected override float[] floatValues 
    {
        get 
        { 
            return new float[]
            {
                permeationPushAccel,
                permeationResistFactor,
                permeationGravityFactor,
                permeationVelocityThreshold,
            };
        }

        set
        {
            permeationPushAccel = value[0];
            permeationResistFactor = value[1];
            permeationGravityFactor = value[2];
            permeationVelocityThreshold = value[3];
        }
    }
}

public struct AlestaMovementAbilityControl : IAbilityControl
{

    /// <summary>
    /// Alesta permeation input using GetButtonDown()
    /// </summary>
    private bool _permeation;
    public bool permeation { get { return _permeation; } set {  if (!_permeation) _permeation = value; } }

    public void Reset()
    {
        _permeation = false;
    }
}

[System.Serializable]
public class AlestaMovementAbility : PlayerMovementAbility
{

    [SerializeField]
	private CharacterOverridableAttribute<AlestaMovementAbilityValues> overridableAttribute = new CharacterOverridableAttribute<AlestaMovementAbilityValues>();

    public AlestaMovementAbilityControl control;

    private bool isPermeating;
    private bool inSurfaceCheck;
    private Vector3 permeationEnterVelocity;
    private FullMovementOverride currentPermeationOverride;
    private Vector3 currentPermeationNormal;
    private float currentPermeationAngularAccel;
    private Vector3 tempCurrentAngularVelocity;

    private CharacterValueOverridability overridability;
    
    protected override void Awake()
    {
        base.Awake();
        currentPermeationOverride = new FullMovementOverride();
        currentPermeationOverride.movementOverrides = new List<MutableTuple<CharacterMovementValues, ValueOverrideType>>();
        CharacterMovementValues moveOverrides = new CharacterMovementValues();
        moveOverrides.SetDefaultValues(ValueOverrideType.Addition);
        //moveOverrides.negateAction = 1;
        //moveOverrides.negatePhysics = 1;
        Debug.Assert(currentPermeationOverride.movementOverrides != null);
        currentPermeationOverride.movementOverrides.Add(new MutableTuple<CharacterMovementValues, ValueOverrideType>(moveOverrides, ValueOverrideType.Addition));
        
        control = new AlestaMovementAbilityControl();
        controlInterface = control;
    }

    void Start()
    {
        overridability = GetComponent<CharacterValueOverridability>();
        overridability.RegisterOverridability(overridableAttribute);
    }

    void Reset()
    {
        overridableAttribute.baseValues.permeationPushAccel = 80;
        overridableAttribute.baseValues.permeationResistFactor = 0.5f;
        overridableAttribute.baseValues.permeationGravityFactor = 0.25f;
        overridableAttribute.baseValues.permeationVelocityThreshold = 10;
    }

    #region AbilityInterface

    /// <summary>
    /// This is called when the motor wants to know what its rotation should be right now
    /// </summary>
    /// <param name="currentRotation"> Reference to the player's rotation </param>
    /// <param name="currentAngularVelocity"> Reference to the player's angular velocity</param>
    /// <param name="motor"> The player's kinematic motor</param>
    /// <param name="deltaTime"> Motor update time </param>
    public override void UpdateRotation(ref Quaternion currentRotation, ref Vector3 currentAngularVelocity, float deltaTime)
    {
        if (isPermeating)
            currentAngularVelocity += motor.PlanarConstraintAxis * currentPermeationAngularAccel * ((control.permeation) ? overridableAttribute.values.permeationResistFactor : 1) * deltaTime; 
        tempCurrentAngularVelocity = currentAngularVelocity;
    }
    
    /// <summary>
    /// This is called when the motor wants to know what its rotation should be right now
    /// </summary>
    /// <param name="currentVelocity"> Reference to the player's velocity</param>
    /// <param name="motor"> The player's kinematic motor</param>
    /// <param name="gravityDirection"> The direction of gravity </param>
    /// <param name="physicsOverride"> Determines overrides to player physics values </param>
    /// <param name="deltaTime"> Motor update time</param>
    public override void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
    {
        if (isPermeating)
            currentVelocity += currentPermeationNormal * overridableAttribute.values.permeationPushAccel * ((control.permeation) ? overridableAttribute.values.permeationResistFactor : 1) * deltaTime; 
    }

    /// <summary>
    /// This is called after the motor has finished everything in its update
    /// </summary>
    /// <param name="motor"> The player's kinematic motor</param>
    /// <param name="deltaTime"> Motor update time </param>
    public override void AfterCharacterUpdate(float deltaTime)
    {
        if (isPermeating)
            {
            if (permeationEnterVelocity != Vector3.zero)
            {
                motor.BaseVelocity = permeationEnterVelocity;
                permeationEnterVelocity = Vector3.zero;
            }
            if (inSurfaceCheck)
                inSurfaceCheck = false;
            else
                ExitPermeation();
        }

    }

    /// <summary>
    /// This is called after when the motor wants to know if the collider can be collided with (or if we just go through it)
    /// </summary>
    /// <param name="motor"> The player's kinematic motor</param>
    /// <param name="coll"> The collider being checked </param>
    public override bool IsColliderValidForCollisions(Collider coll)
    {
        if (isPermeating)
        {
            inSurfaceCheck = true;
            return false;
        }
        else
            return true;
    }

    /// <summary>
    /// This is called when the motor's movement logic detects a hit
    /// </summary>
    /// <param name="motor"> The player's kinematic motor</param>
    /// <param name="hitCollider"> The hit collider </param>
    /// <param name="hitNormal"> The hit normal </param>
    /// <param name="hitPoint"> The hit point </param>
    /// <param name="hitStabilityReport"> The hit stability </param>
    public override void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
    {   
        if (control.permeation && CanPermeate(motor.BaseVelocity))
        {
            EnterPermeation(motor.BaseVelocity, tempCurrentAngularVelocity, hitNormal, motor);
        }
    }

    /// <summary>
    /// This is called after every move hit, to give you an opportunity to modify the HitStabilityReport to your liking
    /// </summary>
    /// <param name="motor"> The player's kinematic motor</param>
    /// <param name="hitCollider"> The hit collider </param>
    /// <param name="hitNormal"> The hit normal </param>
    /// <param name="hitPoint"> The hit point </param>
    /// <param name="hitPoint"></param>
    /// <param name="atCharacterPosition"> The character position on hit </param>
    /// <param name="atCharacterRotation"> The character rotation on hit </param>
    /// <param name="hitStabilityReport"> The hit stability </param>
    public override void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport)
    {
        if (isPermeating)
            hitStabilityReport.IsStable = false;
    }

    public override void Flinch()
    {
        controlInterface.Reset();
    }

    #endregion

    private bool CanPermeate(Vector3 currentVelocity)
    {
        return currentVelocity.sqrMagnitude >= overridableAttribute.values.permeationVelocityThreshold * overridableAttribute.values.permeationVelocityThreshold; 
    }

    private void EnterPermeation(Vector3 velocity, Vector3 angularVelocity, Vector3 hitNormal, KinematicCharacterMotor motor)
    {
        isPermeating = true;
        inSurfaceCheck = true;
        permeationEnterVelocity = velocity;
        currentPermeationNormal = Vector3.ProjectOnPlane(hitNormal, motor.PlanarConstraintAxis).normalized;
        float angularAccelSign = Mathf.Sign(Vector3.Dot(motor.PlanarConstraintAxis, angularVelocity));
        currentPermeationAngularAccel = (angularVelocity.magnitude / (velocity.magnitude/overridableAttribute.values.permeationPushAccel)) * angularAccelSign;
        overridability.ApplyFullMovementOverride(currentPermeationOverride);
    }

    private void ExitPermeation()
    {
        isPermeating = false;
        inSurfaceCheck = false;
        currentPermeationAngularAccel = 0;
        currentPermeationNormal = Vector3.zero;
        overridability.RemoveFullMovementOverride(currentPermeationOverride);
    }

}