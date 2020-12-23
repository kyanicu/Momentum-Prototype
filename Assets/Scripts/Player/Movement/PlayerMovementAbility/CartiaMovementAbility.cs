using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KinematicCharacterController;
using System;

[System.Serializable]
public class CartiaMovementAbilityValues : PlayerMovementOverridableValues
{
    [SerializeField]
    public float test;

    public override void SetDefaultValues(PlayerMovementOverrideType overrideType)
    {
        test = DefaultFloat(overrideType);
    }
    
    public override void AddBy(PlayerMovementOverridableValues ov) 
    {
        CartiaMovementAbilityValues v = ov as CartiaMovementAbilityValues;

        test = Add(test, v.test);
    }

    public override void SubtractBy(PlayerMovementOverridableValues ov) 
    {
        CartiaMovementAbilityValues v = ov as CartiaMovementAbilityValues;

        test = Subtract(test, v.test);
    }

    public override void MultiplyBy(PlayerMovementOverridableValues ov) 
    {
        CartiaMovementAbilityValues v = ov as CartiaMovementAbilityValues;

        test = Multiply(test, v.test);
    }

    public override void DivideBy(PlayerMovementOverridableValues ov) 
    {
        CartiaMovementAbilityValues v = ov as CartiaMovementAbilityValues;

        test = Divide(test, v.test);
    }

    public override void OrBy(PlayerMovementOverridableValues ov) 
    {
        CartiaMovementAbilityValues v = ov as CartiaMovementAbilityValues;

        test = Or(test, v.test);
    }

    public override void AndBy(PlayerMovementOverridableValues ov) 
    {
        CartiaMovementAbilityValues v = ov as CartiaMovementAbilityValues;

        test = And(test, v.test);
    }
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
public class CartiaMovementAbility : PlayerMovementAbility<CartiaMovementAbilityValues> ,ICartiaMovementAbilityCommunication
{

    public override event Action<AbilityOverrideArgs> addingMovementOverrides;
    public override event Action<AbilityOverrideArgs> removingMovementOverrides;

    private CartiaMovementAbilityInput input;

    public CartiaMovementAbility()
    {
        input = new CartiaMovementAbilityInput();
    }

    protected override void SetDefaultBaseValues()
    {
        baseValues.test = 7;
    }

    protected override void ValidateBaseValues()
    {
        
    }

    public override void SetCommunicationInterface(PlayerInternalCommunicator communicator)
    {
        (communicator as CartiaInternalCommunicator).SetCommunication(this);
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
        for (int i = 0; i < effector.cartiaAbilityOverrides.Count; i++)
        {
            AddOverride(effector.cartiaAbilityOverrides[i].item1, effector.cartiaAbilityOverrides[i].item2);
        }
    }

    public override void ExitMovementEffector(MovementEffector effector)
    {
        for (int i = 0; i < effector.cartiaAbilityOverrides.Count; i++)
        {
            RemoveOverride(effector.cartiaAbilityOverrides[i].item1, effector.cartiaAbilityOverrides[i].item2);
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
