using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KinematicCharacterController;
using System;

[System.Serializable]
public class AbilityOverrideArgs : EventArgs
{
    [SerializeField]
    public List<MutableTuple<PlayerMovementValues, PlayerMovementOverrideType>> movementOverrides = new List<MutableTuple<PlayerMovementValues, PlayerMovementOverrideType>>();

    [SerializeField]
    public List<MutableTuple<PlayerMovementPhysicsValues, PlayerMovementOverrideType>> physicsOverrides = new List<MutableTuple<PlayerMovementPhysicsValues, PlayerMovementOverrideType>>();

    [SerializeField]
    public List<MutableTuple<PlayerMovementActionValues, PlayerMovementOverrideType>> actionOverrides = new List<MutableTuple<PlayerMovementActionValues, PlayerMovementOverrideType>>();

}

public interface IPlayerMovementAbility : IPlayerCommunication
{
    /// <summary>
    /// This is called when the motor wants to know what its rotation should be right now
    /// </summary>
    /// <param name="currentRotation"> Reference to the player's rotation </param>
    /// <param name="currentAngularVelocity"> Reference to the player's angular velocity</param>
    /// <param name="motor"> The player's kinematic motor</param>
    /// <param name="deltaTime"> Motor update time </param>
    void UpdateRotation(ref Quaternion currentRotation, ref Vector3 currentAngularVelocity, KinematicCharacterMotor motor, float deltaTime);
    
    /// <summary>
    /// This is called when the motor wants to know what its rotation should be right now
    /// </summary>
    /// <param name="currentVelocity"> Reference to the player's velocity</param>
    /// <param name="motor"> The player's kinematic motor</param>
    /// <param name="gravityDirection"> The direction of gravity </param>
    /// <param name="physicsOverride"> Determines overrides to player physics values </param>
    /// <param name="deltaTime"> Motor update time</param>
    void UpdateVelocity(ref Vector3 currentVelocity, KinematicCharacterMotor motor, Vector3 gravityDirection, ref PlayerMovementPhysics.PhysicsNegations physicsNegations, float deltaTime);

    /// <summary>
    /// This is called before the motor does anything
    /// </summary>
    /// <param name="motor"> The player's kinematic motor</param>
    /// <param name="deltaTime"> Motor update time </param>
    void BeforeCharacterUpdate(KinematicCharacterMotor motor, float deltaTime);

    /// <summary>
    /// This is called after the motor has finished its ground probing, but before PhysicsMover/Velocity/etc.... handling
    /// Primarily used currently to handle the slope tracking for the ungrounding angular momentum mechanic
    /// </summary>
    /// <param name="motor"> The player's kinematic motor</param>
    /// <param name="deltaTime"> Motor update time </param>
    void PostGroundingUpdate(KinematicCharacterMotor motor, float deltaTime);

    /// <summary>
    /// This is called after the motor has finished everything in its update
    /// </summary>
    /// <param name="motor"> The player's kinematic motor</param>
    /// <param name="deltaTime"> Motor update time </param>
    void AfterCharacterUpdate(KinematicCharacterMotor motor, float deltaTime);

    /// <summary>
    /// This is called after when the motor wants to know if the collider can be collided with (or if we just go through it)
    /// </summary>
    /// <param name="motor"> The player's kinematic motor</param>
    /// <param name="coll"> The collider being checked </param>
    bool IsColliderValidForCollisions(KinematicCharacterMotor motor, Collider coll);

    /// <summary>
    /// This is called when the motor's ground probing detects a ground hit
    /// </summary>
    /// <param name="motor"> The player's kinematic motor</param>
    /// <param name="hitCollider">The ground collider </param>
    /// <param name="hitNormal"> The ground normal </param>
    /// <param name="hitPoint"> The ground point </param>
    /// <param name="hitStabilityReport"> The ground stability </param>
    void OnGroundHit(KinematicCharacterMotor motor, Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport);

    /// <summary>
    /// This is called when the motor's movement logic detects a hit
    /// </summary>
    /// <param name="motor"> The player's kinematic motor</param>
    /// <param name="hitCollider"> The hit collider </param>
    /// <param name="hitNormal"> The hit normal </param>
    /// <param name="hitPoint"> The hit point </param>
    /// <param name="hitStabilityReport"> The hit stability </param>
    void OnMovementHit(KinematicCharacterMotor motor, Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport);

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
    void ProcessHitStabilityReport(KinematicCharacterMotor motor, Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport);

    /// <summary>
    /// This is called when the character detects discrete collisions (collisions that don't result from the motor's capsuleCasts when moving)
    /// </summary>
    /// <param name="motor"> The player's kinematic motor</param>
    /// <param name="hitCollider"> The detected collider </param>
    void OnDiscreteCollisionDetected(KinematicCharacterMotor motor, Collider hitCollider);

    void RegisterInput(PlayerController.PlayerActions controllerActions);

    /// <summary>
    /// Resets the input state
    /// </summary>
    void ResetInput();

    void EnterMovementEffector(MovementEffector effector);

    void ExitMovementEffector(MovementEffector effector);
    
    event EventHandler<AbilityOverrideArgs> addingMovementOverrides;
    event EventHandler<AbilityOverrideArgs> removingMovementOverrides;

}

[System.Serializable]
public abstract class PlayerMovementAbility<Values> : PlayerMovementOverridableAttribute<Values>, IPlayerMovementAbility where Values : PlayerMovementOverridableValues, new()
{
    public abstract event EventHandler<AbilityOverrideArgs> addingMovementOverrides;
    public abstract event EventHandler<AbilityOverrideArgs> removingMovementOverrides;

    public abstract void SetCommunication(PlayerInternalCommunicator communicator);

    protected abstract override void SetDefaultBaseValues();

    /// <summary>
    /// This is called when the motor wants to know what its rotation should be right now
    /// </summary>
    /// <param name="currentRotation"> Reference to the player's rotation </param>
    /// <param name="currentAngularVelocity"> Reference to the player's angular velocity</param>
    /// <param name="motor"> The player's kinematic motor</param>
    /// <param name="deltaTime"> Motor update time </param>
    public abstract void UpdateRotation(ref Quaternion currentRotation, ref Vector3 currentAngularVelocity, KinematicCharacterMotor motor, float deltaTime);
    
    /// <summary>
    /// This is called when the motor wants to know what its rotation should be right now
    /// </summary>
    /// <param name="currentVelocity"> Reference to the player's velocity</param>
    /// <param name="motor"> The player's kinematic motor</param>
    /// <param name="gravityDirection"> The direction of gravity </param>
    /// <param name="physicsOverride"> Determines overrides to player physics values </param>
    /// <param name="deltaTime"> Motor update time</param>
    public abstract void UpdateVelocity(ref Vector3 currentVelocity, KinematicCharacterMotor motor, Vector3 gravityDirection, ref PlayerMovementPhysics.PhysicsNegations physicsNegations, float deltaTime);

    /// <summary>
    /// This is called before the motor does anything
    /// </summary>
    /// <param name="motor"> The player's kinematic motor</param>
    /// <param name="deltaTime"> Motor update time </param>
    public abstract void BeforeCharacterUpdate(KinematicCharacterMotor motor, float deltaTime);

    /// <summary>
    /// This is called after the motor has finished its ground probing, but before PhysicsMover/Velocity/etc.... handling
    /// Primarily used currently to handle the slope tracking for the ungrounding angular momentum mechanic
    /// </summary>
    /// <param name="motor"> The player's kinematic motor</param>
    /// <param name="deltaTime"> Motor update time </param>
    public abstract void PostGroundingUpdate(KinematicCharacterMotor motor, float deltaTime);

    /// <summary>
    /// This is called after the motor has finished everything in its update
    /// </summary>
    /// <param name="motor"> The player's kinematic motor</param>
    /// <param name="deltaTime"> Motor update time </param>
    public abstract void AfterCharacterUpdate(KinematicCharacterMotor motor, float deltaTime);

    /// <summary>
    /// This is called after when the motor wants to know if the collider can be collided with (or if we just go through it)
    /// </summary>
    /// <param name="motor"> The player's kinematic motor</param>
    /// <param name="coll"> The collider being checked </param>
    public abstract bool IsColliderValidForCollisions(KinematicCharacterMotor motor, Collider coll);

    /// <summary>
    /// This is called when the motor's ground probing detects a ground hit
    /// </summary>
    /// <param name="motor"> The player's kinematic motor</param>
    /// <param name="hitCollider">The ground collider </param>
    /// <param name="hitNormal"> The ground normal </param>
    /// <param name="hitPoint"> The ground point </param>
    /// <param name="hitStabilityReport"> The ground stability </param>
    public abstract void OnGroundHit(KinematicCharacterMotor motor, Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport);

    /// <summary>
    /// This is called when the motor's movement logic detects a hit
    /// </summary>
    /// <param name="motor"> The player's kinematic motor</param>
    /// <param name="hitCollider"> The hit collider </param>
    /// <param name="hitNormal"> The hit normal </param>
    /// <param name="hitPoint"> The hit point </param>
    /// <param name="hitStabilityReport"> The hit stability </param>
    public abstract void OnMovementHit(KinematicCharacterMotor motor, Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport);

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
    public abstract void ProcessHitStabilityReport(KinematicCharacterMotor motor, Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport);

    /// <summary>
    /// This is called when the character detects discrete collisions (collisions that don't result from the motor's capsuleCasts when moving)
    /// </summary>
    /// <param name="motor"> The player's kinematic motor</param>
    /// <param name="hitCollider"> The detected collider </param>
    public abstract void OnDiscreteCollisionDetected(KinematicCharacterMotor motor, Collider hitCollider);

    public abstract void RegisterInput(PlayerController.PlayerActions controllerActions);

    /// <summary>
    /// Resets the input state
    /// </summary>
    public  abstract void ResetInput();

    public abstract void EnterMovementEffector(MovementEffector effector);

    public abstract void ExitMovementEffector(MovementEffector effector);

}