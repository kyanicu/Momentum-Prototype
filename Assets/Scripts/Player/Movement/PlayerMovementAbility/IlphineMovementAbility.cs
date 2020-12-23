using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KinematicCharacterController;
using System;

[System.Serializable]
public class IlphineMovementAbilityValues : PlayerOverridableValues
{
    [SerializeField]
    public float test;
    
    protected override void SetValueCounts()
    {
        floatValuesCount = 1;
        intValuesCount = 0;
        vector3ValuesCount = 0;
    }

    protected override float GetFloatValue(int i)
    {
        switch (i) 
        {
            case (0) :
                return test;
            default :
                return 0;
        }
    }
    protected override void SetFloatValue(int i, float value)
    {
        switch (i) 
        {
            case (0) :
                test = value;
                break;
            default :
                break;
        }
    }
    protected override int GetIntValue(int i)
    {
        return 0;
    }
    protected override void SetIntValue(int i, int value) { }
    protected override Vector3 GetVector3Value(int i)
    {
        return Vector3.zero;
    }
    protected override void SetVector3Value(int i, Vector3 value) {}
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
public class IlphineMovementAbility : PlayerMovementAbility<IlphineMovementAbilityValues>, IIlphineMovementAbilityCommunication
{
    
    public override event Action<AbilityOverrideArgs> addingMovementOverrides;
    public override event Action<AbilityOverrideArgs> removingMovementOverrides;

    private IlphineMovementAbilityInput input;

    public IlphineMovementAbility()
    {
        input = new IlphineMovementAbilityInput();
    }

    protected override void SetDefaultBaseValues()
    {
        baseValues.test = 1;
    }

    protected override void ValidateBaseValues()
    {
        
    }

    public override void SetCommunicationInterface(PlayerInternalCommunicator communicator)
    {
        (communicator as IlphineInternalCommunicator).SetCommunication(this);
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

    }

    /// <summary>
    /// This is called after when the motor wants to know if the collider can be collided with (or if we just go through it)
    /// </summary>
    /// <param name="motor"> The player's kinematic motor</param>
    /// <param name="coll"> The collider being checked </param>
    public override bool IsColliderValidForCollisions(KinematicCharacterMotor motor, Collider coll)
    {
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
        for (int i = 0; i < effector.movementOverrides.Count; i++)
        {
            ApplyOverride(effector.ilphineAbilityOverrides[i].item1, effector.movementOverrides[i].item2);
        }
    }

    public override void ExitMovementEffector(MovementEffector effector)
    {
        for (int i = 0; i < effector.movementOverrides.Count; i++)
        {
            RemoveOverride(effector.ilphineAbilityOverrides[i].item1, effector.movementOverrides[i].item2);
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

}