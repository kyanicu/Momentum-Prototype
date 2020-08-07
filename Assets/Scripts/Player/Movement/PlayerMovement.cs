using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KinematicCharacterController;

public class PlayerMovement
 : MonoBehaviour, ICharacterController
{

    private KinematicCharacterMotor motor;
    PlayerMovementPhysics physics;
    PlayerMovementAction action;

    Vector3 internalAngularVelocity = Vector3.zero;

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
        
        startState = motor.GetState();

        action = GetComponent<PlayerMovementAction>();
        physics = GetComponent<PlayerMovementPhysics>();
    }

    public void HandleInput()
    {
        action.RegisterInput();
    }

    private void SetPreviousStableGroundNormal(Vector3 groundNormal)
    {
        previousStableGroundNormal = groundNormal;

        if(groundNormal == Vector3.zero)
            previousStableGroundTimer = 0;
        else
           previousStableGroundTimer = previousStableGroundTime;
    }

#region CharacterControllerInterface

    /// <summary>
    /// This is called when the motor wants to know what its rotation should be right now
    /// </summary>
    public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
    {
        if (motor.CharacterForward != motor.PlanarConstraintAxis)
        {

            Vector3 smoothedForward;
                if(Vector3.Dot(motor.CharacterForward, -motor.PlanarConstraintAxis) > 0.95f)
                    smoothedForward = Vector3.Slerp((motor.CharacterUp - motor.CharacterRight * 0.05f).normalized, -physics.gravity.normalized, 1 - Mathf.Exp(-10 * deltaTime));
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
                Vector3 smoothedUp = Vector3.Slerp(motor.CharacterUp, -physics.gravity.normalized, 1 - Mathf.Exp(-slerpFactor * deltaTime));
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
                if(Vector3.Dot(motor.CharacterUp, physics.gravity.normalized) > 0.95f)
                    smoothedUp = Vector3.Slerp((motor.CharacterUp - motor.CharacterRight * 0.05f).normalized, -physics.gravity.normalized, 1 - Mathf.Exp(-slerpFactor * deltaTime));
                else
                    smoothedUp = Vector3.Slerp(motor.CharacterUp, -physics.gravity.normalized, 1 - Mathf.Exp(-slerpFactor * deltaTime));
                
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
    public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
    {

        if (motor.GroundingStatus.IsStableOnGround && !motor.MustUnground())
        {
            if (motor.LastGroundingStatus.IsStableOnGround)
                currentVelocity = Vector3.ProjectOnPlane(currentVelocity, motor.GetEffectiveGroundNormal()).normalized * currentVelocity.magnitude;
            else
                currentVelocity = Vector3.ProjectOnPlane(currentVelocity, motor.GetEffectiveGroundNormal());
        }
        /*
        #region debug
        float speed = 16;
        float direction = 0;
        if(Input.GetKey(KeyCode.A))
            direction = -1;
        else if(Input.GetKey(KeyCode.D))
            direction = +1;
        if (motor.GroundingStatus.IsStableOnGround && !motor.MustUnground())
        {
            currentVelocity = direction * speed * Vector3.ProjectOnPlane(motor.CharacterRight, motor.GetEffectiveGroundNormal()).normalized;
        }
        else 
        {
            currentVelocity = direction * speed * Vector3.right + currentVelocity.y * Vector3.up;
        }
        float jump = 15;
        if(motor.GroundingStatus.IsStableOnGround && !motor.MustUnground() && Input.GetKey(KeyCode.Space))
        {
            currentVelocity += jump * Vector3.up;
            motor.ForceUnground();
        }
        else if(!motor.GroundingStatus.IsStableOnGround)
            currentVelocity += physics.gravity * deltaTime;
        #endregion
        */

        action.UpdateVelocity(ref currentVelocity, motor, physics.gravity.normalized, ref physics.overrides, deltaTime);
        physics.UpdateVelocity (ref currentVelocity, motor, deltaTime);


    }

    /// <summary>
    /// This is called before the motor does anything
    /// </summary>
    public void BeforeCharacterUpdate(float deltaTime)
    {

    }

    /// <summary>
    /// This is called after the motor has finished its ground probing, but before PhysicsMover/Velocity/etc.... handling
    /// </summary>
    public void PostGroundingUpdate(float deltaTime)
    {
        if(motor.GroundingStatus.IsStableOnGround)
        {
            if(Vector3.ProjectOnPlane(motor.BaseVelocity, motor.GetEffectiveGroundNormal()).magnitude < attachThreshold && Vector3.Angle(-physics.gravity, motor.GetEffectiveGroundNormal()) > motor.MaxStableSlopeAngle)
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
    public void AfterCharacterUpdate(float deltaTime)
    {
        action.ResetInput();
    }

    /// <summary>
    /// This is called after when the motor wants to know if the collider can be collided with (or if we just go through it)
    /// </summary>
    public bool IsColliderValidForCollisions(Collider coll)
    {
        return true;
    }

    /// <summary>
    /// This is called when the motor's ground probing detects a ground hit
    /// </summary>
    public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
    {

    }

    /// <summary>
    /// This is called when the motor's movement logic detects a hit
    /// </summary>
    public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
    {   
        if (!motor.GroundingStatus.IsStableOnGround && motor.StableGroundLayers.value == (motor.StableGroundLayers.value | (1 << hitCollider.gameObject.layer)) && Vector3.Angle(-physics.gravity, hitNormal) <= motor.MaxStableSlopeAngle)
        {
            foundFloorToReorientTo = true;
        }
    }

    /// <summary>
    /// This is called after every move hit, to give you an opportunity to modify the HitStabilityReport to your liking
    /// </summary>
    public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport)
    {
        
    }

    /// <summary>
    /// This is called when the character detects discrete collisions (collisions that don't result from the motor's capsuleCasts when moving)
    /// </summary>
    public void OnDiscreteCollisionDetected(Collider hitCollider)
    {

    }


#endregion

    // Update is called once per frame
    void Update()
    {
        if(previousStableGroundTimer > 0)
        {
            previousStableGroundTimer -= Time.deltaTime;
            if(previousStableGroundTimer <= 0)
                SetPreviousStableGroundNormal(Vector3.zero);
        }

        // Debug
        if (Input.GetKeyDown(KeyCode.Return))
        {
            motor.ApplyState(startState);
        }
    }
}
