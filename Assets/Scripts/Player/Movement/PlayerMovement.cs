using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KinematicCharacterController;

public class PlayerMovement : MonoBehaviour, ICharacterController
{

    private KinematicCharacterMotor motor;
    private PlayerMovementPhysics physics;
    private PlayerMovementAction action;
    private PlayerMovementAbility ability;

    private Vector3 internalAngularVelocity = Vector3.zero;

    public Vector3 externalVelocityAddition { private get; set; }

    [SerializeField]
    private float attachThreshold;
    [SerializeField]
    private float pushOffGroundThreshold;

    private Vector3 previousStableGroundNormal;
    [SerializeField]
    private float previousStableGroundTime;
    private float previousStableGroundTimer;

    private bool foundFloorToReorientTo;

    void Reset()
    {
        attachThreshold = 8;
        pushOffGroundThreshold = 1;
        previousStableGroundTime = 0.5f;
    }

    void Awake()
    {
        motor = GetComponentInParent<KinematicCharacterMotor>();
    }

    void OnValidate()
    {
        if (motor != null && motor.CharacterController == null)
            motor.CharacterController = this;
    }

    KinematicCharacterMotorState startState;

    // Start is called before the first frame update
    void Start()
    {
        motor.CharacterController = this;
        
        // Debug
        #region debug
        startState = motor.GetState();
        #endregion

        action = GetComponent<PlayerMovementAction>();
        physics = GetComponent<PlayerMovementPhysics>();
    }

    public void HandleInput()
    {
        action.RegisterInput();
    }

    /// <summary>
    /// Sets the tracked previous ground normal and resets the timer
    /// </summary>
    /// <param name="groundNormal"> The ground normal </param>
    private void SetPreviousStableGroundNormal(Vector3 groundNormal)
    {
        /// Sets the previous ground normal
        previousStableGroundNormal = groundNormal;

        // Force stop time if no ground given
        if(groundNormal == Vector3.zero)
            previousStableGroundTimer = 0;
        else
            // Start/restart timer
            previousStableGroundTimer = previousStableGroundTime;
    }

#region CharacterControllerInterface

    /// <summary>
    /// This is called when the motor wants to know what its rotation should be right now
    /// </summary>
    /// <param name="currentVelocity"> Reference to the player's velocity </param>
    /// <param name="deltaTime"> Motor update time </param>
    public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
    {
        if (motor.CharacterForward != motor.PlanarConstraintAxis)
        {
            Vector3 smoothedForward;
                if(Vector3.Dot(motor.CharacterForward, -motor.PlanarConstraintAxis) > 0.95f)
                    smoothedForward = Vector3.Slerp((motor.CharacterUp - motor.CharacterRight * 0.05f).normalized, -physics.gravityDirection, 1 - Mathf.Exp(-10 * deltaTime));
                else 
                    smoothedForward = Vector3.Slerp(motor.CharacterForward, motor.PlanarConstraintAxis, 1 - Mathf.Exp(-30 * deltaTime));
                currentRotation = Quaternion.FromToRotation(motor.CharacterForward, smoothedForward) * currentRotation;
        }

        bool reorient = false;
        if(foundFloorToReorientTo)
        {
            internalAngularVelocity = Vector3.zero;
            foundFloorToReorientTo = false;
            reorient = true;
        }
        
        float slerpFactor = 30;
        if(motor.GroundingStatus.IsStableOnGround && !motor.MustUnground())
        {
            internalAngularVelocity = Vector3.zero;

            if(motor.BaseVelocity.magnitude > attachThreshold)
            {
                Vector3 initialCharacterBottomHemiCenter = motor.TransientPosition + (motor.CharacterUp * motor.Capsule.radius);

                Vector3 smoothedGroundNormal = Vector3.Slerp(motor.CharacterUp, motor.GetEffectiveGroundNormal(), 1 - Mathf.Exp(-slerpFactor * deltaTime));
                currentRotation = Quaternion.FromToRotation(motor.CharacterUp, smoothedGroundNormal) * currentRotation;

                // Move the position to create a rotation around the bottom hemi center instead of around the pivot
                motor.SetTransientPosition(initialCharacterBottomHemiCenter + (currentRotation * Vector3.down * motor.Capsule.radius));
            }
            else
            {
                slerpFactor = 3;
                Vector3 smoothedUp = Vector3.Slerp(motor.CharacterUp, -physics.gravityDirection, 1 - Mathf.Exp(-slerpFactor * deltaTime));
                currentRotation = Quaternion.FromToRotation(motor.CharacterUp, smoothedUp) * currentRotation;
            }
        }
        else
        {
            if(internalAngularVelocity == Vector3.zero)
            {
                if(!reorient)
                    slerpFactor = 3;

                Vector3 smoothedUp;
                if(Vector3.Dot(motor.CharacterUp, physics.gravityDirection) > 0.95f)
                    smoothedUp = Vector3.Slerp((motor.CharacterUp - motor.CharacterRight * 0.05f).normalized, -physics.gravityDirection, 1 - Mathf.Exp(-slerpFactor * deltaTime));
                else
                    smoothedUp = Vector3.Slerp(motor.CharacterUp, -physics.gravityDirection, 1 - Mathf.Exp(-slerpFactor * deltaTime));
                
                currentRotation = Quaternion.FromToRotation(motor.CharacterUp, smoothedUp) * currentRotation;
            }
        }

        if(internalAngularVelocity != Vector3.zero)
        {
            internalAngularVelocity = Vector3.Project(internalAngularVelocity, motor.PlanarConstraintAxis);
            currentRotation *= Quaternion.Euler(internalAngularVelocity * deltaTime);
        }
        action.UpdateRotation(ref currentRotation, motor, deltaTime);
        physics.UpdateRotation(ref currentRotation, motor, deltaTime);
    }
    
    /// <summary>
    /// This is called when the motor wants to know what its velocity should be right now
    /// </summary>
    /// <param name="currentVelocity"> Reference to the player's velocity </param>
    /// <param name="deltaTime"> Motor update time </param>
    public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
    {
        // Handle velocity projection if grounded
        if (motor.IsGroundedThisUpdate)
        {
            // The dot between the ground normal and the external velocity addition
            float dot = Vector3.Dot(externalVelocityAddition, motor.GetEffectiveGroundNormal());
            // The velocity off the ground
            Vector3 projection = dot * motor.GetEffectiveGroundNormal();
            // If external velocity off ground is strong enough
            if(dot > 0  && projection.sqrMagnitude >= pushOffGroundThreshold * pushOffGroundThreshold)
                motor.ForceUnground();
            else
            {
                // if just landed
                if (motor.LastGroundingStatus.IsStableOnGround)
                    // Project velocity onto ground
                    currentVelocity = Vector3.ProjectOnPlane(currentVelocity, motor.GetEffectiveGroundNormal()).normalized * currentVelocity.magnitude;
                else
                    // Reorient without losing momentum
                    currentVelocity = Vector3.ProjectOnPlane(currentVelocity, motor.GetEffectiveGroundNormal());

                // Snap external velocity to ground
                 externalVelocityAddition -= projection;
            }
        }

        // Add external velocity and reset back to zero
        currentVelocity += externalVelocityAddition;
        externalVelocityAddition = Vector3.zero;

        // Update velocity from components 
        //ability.UpdateVelocity();
        action.UpdateVelocity(ref currentVelocity, motor, physics.gravityDirection, ref physics.negations, deltaTime);
        physics.UpdateVelocity (ref currentVelocity, motor, deltaTime);
    }

    /// <summary>
    /// This is called before the motor does anything
    /// </summary>
    /// <param name="deltaTime"> Motor update time </param>
    public void BeforeCharacterUpdate(float deltaTime)
    {

    }

    /// <summary>
    /// This is called after the motor has finished its ground probing, but before PhysicsMover/Velocity/etc.... handling
    /// </summary>
    /// <param name="deltaTime"> Motor update time </param>
    public void PostGroundingUpdate(float deltaTime)
    {
        if(motor.GroundingStatus.IsStableOnGround)
        {
            if(Vector3.ProjectOnPlane(motor.BaseVelocity, motor.GetEffectiveGroundNormal()).magnitude < attachThreshold && Vector3.Angle(-physics.gravityDirection, motor.GetEffectiveGroundNormal()) > motor.MaxStableSlopeAngle)
                motor.ForceUnground();

            if(!motor.MustUnground())
            {
                // Just Grounded
                if(!motor.LastGroundingStatus.IsStableOnGround)
                {
                    
                }
                else if(motor.GetEffectiveGroundNormal() != motor.GetLastEffectiveGroundNormal() )
                {
                    SetPreviousStableGroundNormal(motor.GetLastEffectiveGroundNormal());
                }
            }
        }
        
        if(!motor.GroundingStatus.IsStableOnGround)
        {
            // Just Ungrounded
            if(motor.LastGroundingStatus.IsStableOnGround || motor.MustUnground())
            {
                if(previousStableGroundNormal != Vector3.zero && Vector3.ProjectOnPlane(motor.BaseVelocity,motor.GetEffectiveGroundNormal()).magnitude > attachThreshold)
                {
                    Vector3 rotationDirection = Quaternion.FromToRotation(previousStableGroundNormal, motor.GetLastEffectiveGroundNormal()).eulerAngles.normalized;
                    float rotationSpeed = -Vector3.SignedAngle(previousStableGroundNormal, motor.GetLastEffectiveGroundNormal(), rotationDirection) / ((previousStableGroundTimer - previousStableGroundTime) + 0.05f);
                    float minRotationSpeed = 150;
                    float maxRotationSpeed = 1000;
                    if(Mathf.Abs(rotationSpeed) > maxRotationSpeed)
                        rotationSpeed = maxRotationSpeed * Mathf.Sign(rotationSpeed);
                    else if(Mathf.Abs(rotationSpeed) < minRotationSpeed)
                        rotationSpeed = minRotationSpeed * Mathf.Sign(rotationSpeed);
                    
                    internalAngularVelocity = rotationDirection * rotationSpeed;
                }
                SetPreviousStableGroundNormal(Vector3.zero);
            }
        }
    }

    /// <summary>
    /// This is called after the motor has finished everything in its update
    /// </summary>
    /// <param name="deltaTime"> Motor update time </param>
    public void AfterCharacterUpdate(float deltaTime)
    {
        // Reset Action Input
        //ability.ResetInput();
        action.ResetInput();
    }

    /// <summary>
    /// This is called after when the motor wants to know if the collider can be collided with (or if we just go through it)
    /// </summary>
    /// <param name="coll"> The collider being checked </param>
    public bool IsColliderValidForCollisions(Collider coll)
    {
        return true;
    }

    /// <summary>
    /// This is called when the motor's ground probing detects a ground hit
    /// </summary>
    /// <param name="hitCollider">The ground collider </param>
    /// <param name="hitNormal"> The ground normal </param>
    /// <param name="hitPoint"> The ground point </param>
    /// <param name="hitStabilityReport"> The ground stability </param>
    public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
    {

    }

    /// <summary>
    /// This is called when the motor's movement logic detects a hit
    /// </summary>
    /// <param name="hitCollider"> The hit collider </param>
    /// <param name="hitNormal"> The hit normal </param>
    /// <param name="hitPoint"> The hit point </param>
    /// <param name="hitStabilityReport"> The hit stability </param>
    public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
    {   
        if (!motor.GroundingStatus.IsStableOnGround && motor.StableGroundLayers.value == (motor.StableGroundLayers.value | (1 << hitCollider.gameObject.layer)) && Vector3.Angle(-physics.gravityDirection, hitNormal) <= motor.MaxStableSlopeAngle)
        {
            foundFloorToReorientTo = true;
        }
    }

    /// <summary>
    /// This is called after every move hit, to give you an opportunity to modify the HitStabilityReport to your liking
    /// </summary>
    /// <param name="hitCollider"> The hit collider </param>
    /// <param name="hitNormal"> The hit normal </param>
    /// <param name="hitPoint"> The hit point </param>
    /// <param name="hitPoint"></param>
    /// <param name="atCharacterPosition"> The character position on hit </param>
    /// <param name="atCharacterRotation"> The character rotation on hit </param>
    /// <param name="hitStabilityReport"> The hit stability </param>
    public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport)
    {
        
    }

    /// <summary>
    /// This is called when the character detects discrete collisions (collisions that don't result from the motor's capsuleCasts when moving)
    /// </summary>
    /// <param name="hitCollider"> The detected collider </param>
    public void OnDiscreteCollisionDetected(Collider hitCollider)
    {

    }

#endregion

    /// <summary>
    /// Manages slope change timer 
    /// Also currently used for debugging inputs
    /// </summary>
    void Update()
    {
        // If the timer is active 
        if(previousStableGroundTimer > 0)
        {
            // Handle timer and previous stable ground tracker
            previousStableGroundTimer -= Time.deltaTime;
            if(previousStableGroundTimer <= 0)
                SetPreviousStableGroundNormal(Vector3.zero);
        }

        // Debug
        #region debug
        // Resets the the motor state (used as a makeshift "level restart")
        if (Input.GetKeyDown(KeyCode.Return))
        {
            motor.ApplyState(startState);
        }

        #endregion
    }
}
