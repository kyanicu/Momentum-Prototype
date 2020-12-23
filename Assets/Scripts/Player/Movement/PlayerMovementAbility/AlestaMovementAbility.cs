using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KinematicCharacterController;
using System;

[System.Serializable]
public class AlestaMovementAbilityValues : PlayerMovementOverridableValues
{
    [SerializeField]
    public float permeationPushAccel;
    [SerializeField]
    public float permeationResistFactor;
    [SerializeField]
    public float permeationGravityFactor;
    [SerializeField]
    public float permeationVelocityThreshold;

    public override void SetDefaultValues(PlayerMovementOverrideType overrideType)
    {
        permeationPushAccel = DefaultFloat(overrideType);
        permeationResistFactor = DefaultFloat(overrideType);
        permeationGravityFactor = DefaultFloat(overrideType);
        permeationVelocityThreshold = DefaultFloat(overrideType);
    }
    
    public override void AddBy(PlayerMovementOverridableValues ov) 
    {
        AlestaMovementAbilityValues v = ov as AlestaMovementAbilityValues;

        permeationPushAccel = Add(permeationPushAccel, v.permeationPushAccel);
        permeationResistFactor = Add(permeationResistFactor, v.permeationResistFactor);
        permeationGravityFactor = Add(permeationGravityFactor, v.permeationGravityFactor);
        permeationVelocityThreshold = Add(permeationVelocityThreshold, v.permeationVelocityThreshold);
    }

    public override void SubtractBy(PlayerMovementOverridableValues ov) 
    {
        AlestaMovementAbilityValues v = ov as AlestaMovementAbilityValues;

        permeationPushAccel = Subtract(permeationPushAccel, v.permeationPushAccel);
        permeationResistFactor = Subtract(permeationResistFactor, v.permeationResistFactor);
        permeationGravityFactor = Subtract(permeationGravityFactor, v.permeationGravityFactor);
        permeationVelocityThreshold = Subtract(permeationVelocityThreshold, v.permeationVelocityThreshold);
    }

    public override void MultiplyBy(PlayerMovementOverridableValues ov) 
    {
        AlestaMovementAbilityValues v = ov as AlestaMovementAbilityValues;

        permeationPushAccel = Multiply(permeationPushAccel, v.permeationPushAccel);
        permeationResistFactor = Multiply(permeationResistFactor, v.permeationResistFactor);
        permeationGravityFactor = Multiply(permeationGravityFactor, v.permeationGravityFactor);
        permeationVelocityThreshold = Multiply(permeationVelocityThreshold, v.permeationVelocityThreshold);
    }

    public override void DivideBy(PlayerMovementOverridableValues ov) 
    {
        AlestaMovementAbilityValues v = ov as AlestaMovementAbilityValues;

        permeationPushAccel = Divide(permeationPushAccel, v.permeationPushAccel);
        permeationResistFactor = Divide(permeationResistFactor, v.permeationResistFactor);
        permeationGravityFactor = Divide(permeationGravityFactor, v.permeationGravityFactor);
        permeationVelocityThreshold = Divide(permeationVelocityThreshold, v.permeationVelocityThreshold);
    }

    public override void OrBy(PlayerMovementOverridableValues ov) 
    {
        AlestaMovementAbilityValues v = ov as AlestaMovementAbilityValues;

        permeationPushAccel = Or(permeationPushAccel, v.permeationPushAccel);
        permeationResistFactor = Or(permeationResistFactor, v.permeationResistFactor);
        permeationGravityFactor = Or(permeationGravityFactor, v.permeationGravityFactor);
        permeationVelocityThreshold = Or(permeationVelocityThreshold, v.permeationVelocityThreshold);
    }

    public override void AndBy(PlayerMovementOverridableValues ov) 
    {
        AlestaMovementAbilityValues v = ov as AlestaMovementAbilityValues;

        permeationPushAccel = And(permeationPushAccel, v.permeationPushAccel);
        permeationResistFactor = And(permeationResistFactor, v.permeationResistFactor);
        permeationGravityFactor = And(permeationGravityFactor, v.permeationGravityFactor);
        permeationVelocityThreshold = And(permeationVelocityThreshold, v.permeationVelocityThreshold);
    }
}

struct AlestaMovementAbilityInput : IPlayerMovementInput
{

    /// <summary>
    /// Alesta permeation input using GetButtonDown()
    /// </summary>
    private bool _permeation;
    public bool permeation { get { return _permeation; } set {  if (!_permeation) _permeation = value; } }

    public void RegisterInput(PlayerController.PlayerActions controllerActions)
    {
        permeation = controllerActions.Permeation.ReadValue<float>() > 0;
    }

    public void Reset()
    {
        _permeation = false;
    }
}

[System.Serializable]
public class AlestaMovementAbility : PlayerMovementAbility<AlestaMovementAbilityValues>, IAlestaMovementAbilityCommunication
{

    public override event Action<AbilityOverrideArgs> addingMovementOverrides;
    public override event Action<AbilityOverrideArgs> removingMovementOverrides;

    private AlestaMovementAbilityInput input;

    private bool isPermeating;
    private bool inSurfaceCheck;
    private Vector3 permeationEnterVelocity;
    private AbilityOverrideArgs currentPermeationOverride;
    private Vector3 currentPermeationNormal;
    private float currentPermeationAngularAccel;
    private Vector3 tempCurrentAngularVelocity;

    public AlestaMovementAbility()
    {
        currentPermeationOverride = new AbilityOverrideArgs();
        currentPermeationOverride.movementOverrides = new List<MutableTuple<PlayerMovementValues, PlayerMovementOverrideType>>();
        PlayerMovementValues moveOverrides = new PlayerMovementValues();
        moveOverrides.SetDefaultValues(PlayerMovementOverrideType.Addition);
        moveOverrides.negateAction = 1;
        moveOverrides.negatePhysics = 1;
        Debug.Assert(currentPermeationOverride.movementOverrides != null);
        currentPermeationOverride.movementOverrides.Add(new MutableTuple<PlayerMovementValues, PlayerMovementOverrideType>(moveOverrides, PlayerMovementOverrideType.Addition));
        
        input = new AlestaMovementAbilityInput();
    }

    protected override void SetDefaultBaseValues()
    {
        baseValues.permeationPushAccel = 80;
        baseValues.permeationResistFactor = 0.5f;
        baseValues.permeationGravityFactor = 0.25f;
        baseValues.permeationVelocityThreshold = 10;
    }

    protected override void ValidateBaseValues()
    {
        
    }

    public override void SetCommunicationInterface(PlayerInternalCommunicator communicator)
    {
        (communicator as AlestaInternalCommunicator).SetCommunication(this);
    }

    #region AbilityInterface

    /// <summary>
    /// This is called when the motor wants to know what its rotation should be right now
    /// </summary>
    /// <param name="currentRotation"> Reference to the player's rotation </param>
    /// <param name="currentAngularVelocity"> Reference to the player's angular velocity</param>
    /// <param name="motor"> The player's kinematic motor</param>
    /// <param name="deltaTime"> Motor update time </param>
    public override void UpdateRotation(ref Quaternion currentRotation, ref Vector3 currentAngularVelocity, KinematicCharacterMotor motor, float deltaTime)
    {
        if (isPermeating)
            currentAngularVelocity += motor.PlanarConstraintAxis * currentPermeationAngularAccel * ((input.permeation) ? values.permeationResistFactor : 1) * deltaTime; 
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
    public override void UpdateVelocity(ref Vector3 currentVelocity, KinematicCharacterMotor motor, Vector3 gravityDirection, ref PlayerMovementPhysics.PhysicsNegations physicsNegations, float deltaTime)
    {
        if (isPermeating)
            currentVelocity += currentPermeationNormal * values.permeationPushAccel * ((input.permeation) ? values.permeationResistFactor : 1) * deltaTime; 
    }

    /// <summary>
    /// This is called before the motor does anything
    /// </summary>
    /// <param name="motor"> The player's kinematic motor</param>
    /// <param name="deltaTime"> Motor update time </param>
    public override void BeforeCharacterUpdate(KinematicCharacterMotor motor, float deltaTime)
    {

    }

    /// <summary>
    /// This is called after the motor has finished its ground probing, but before PhysicsMover/Velocity/etc.... handling
    /// Primarily used currently to handle the slope tracking for the ungrounding angular momentum mechanic
    /// </summary>
    /// <param name="motor"> The player's kinematic motor</param>
    /// <param name="deltaTime"> Motor update time </param>
    public override void PostGroundingUpdate(KinematicCharacterMotor motor, float deltaTime)
    {

    }

    /// <summary>
    /// This is called after the motor has finished everything in its update
    /// </summary>
    /// <param name="motor"> The player's kinematic motor</param>
    /// <param name="deltaTime"> Motor update time </param>
    public override void AfterCharacterUpdate(KinematicCharacterMotor motor, float deltaTime)
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
    public override bool IsColliderValidForCollisions(KinematicCharacterMotor motor, Collider coll)
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
    /// This is called when the motor's ground probing detects a ground hit
    /// </summary>
    /// <param name="motor"> The player's kinematic motor</param>
    /// <param name="hitCollider">The ground collider </param>
    /// <param name="hitNormal"> The ground normal </param>
    /// <param name="hitPoint"> The ground point </param>
    /// <param name="hitStabilityReport"> The ground stability </param>
    public override void OnGroundHit(KinematicCharacterMotor motor, Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
    {

    }

    /// <summary>
    /// This is called when the motor's movement logic detects a hit
    /// </summary>
    /// <param name="motor"> The player's kinematic motor</param>
    /// <param name="hitCollider"> The hit collider </param>
    /// <param name="hitNormal"> The hit normal </param>
    /// <param name="hitPoint"> The hit point </param>
    /// <param name="hitStabilityReport"> The hit stability </param>
    public override void OnMovementHit(KinematicCharacterMotor motor, Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
    {   
        if (input.permeation && CanPermeate(motor.BaseVelocity))
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
    public override void ProcessHitStabilityReport(KinematicCharacterMotor motor, Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport)
    {
        if (isPermeating)
            hitStabilityReport.IsStable = false;
    }

    /// <summary>
    /// This is called when the character detects discrete collisions (collisions that don't result from the motor's capsuleCasts when moving)
    /// </summary>
    /// <param name="motor"> The player's kinematic motor</param>
    /// <param name="hitCollider"> The detected collider </param>
    public override void OnDiscreteCollisionDetected(KinematicCharacterMotor motor, Collider hitCollider)
    {
        
    }

    public override void EnterMovementEffector(MovementEffector effector)
    {
        for (int i = 0; i < effector.alestaAbilityOverrides.Count; i++)
        {
            AddOverride(effector.alestaAbilityOverrides[i].item1, effector.alestaAbilityOverrides[i].item2);
        }
    }

    public override void ExitMovementEffector(MovementEffector effector)
    {
        for (int i = 0; i < effector.alestaAbilityOverrides.Count; i++)
        {
            RemoveOverride(effector.alestaAbilityOverrides[i].item1, effector.alestaAbilityOverrides[i].item2);
        }
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

    private bool CanPermeate(Vector3 currentVelocity)
    {
        return currentVelocity.sqrMagnitude >= values.permeationVelocityThreshold * values.permeationVelocityThreshold; 
    }

    private void EnterPermeation(Vector3 velocity, Vector3 angularVelocity, Vector3 hitNormal, KinematicCharacterMotor motor)
    {
        isPermeating = true;
        inSurfaceCheck = true;
        permeationEnterVelocity = velocity;
        currentPermeationNormal = Vector3.ProjectOnPlane(hitNormal, motor.PlanarConstraintAxis).normalized;
        float angularAccelSign = Mathf.Sign(Vector3.Dot(motor.PlanarConstraintAxis, angularVelocity));
        currentPermeationAngularAccel = (angularVelocity.magnitude / (velocity.magnitude/values.permeationPushAccel)) * angularAccelSign;
        addingMovementOverrides?.Invoke(currentPermeationOverride);
    }

    private void ExitPermeation()
    {
        isPermeating = false;
        inSurfaceCheck = false;
        currentPermeationAngularAccel = 0;
        currentPermeationNormal = Vector3.zero;
        removingMovementOverrides(currentPermeationOverride);
    }

}