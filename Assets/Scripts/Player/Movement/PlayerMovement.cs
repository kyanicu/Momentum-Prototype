using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KinematicCharacterController;
using PathCreation;

public class PlayerMovement : MonoBehaviour, ICharacterController
{

    private struct SlopeList
    {
        private (Vector3, float)[] slopeList;
        private int capacity;
        private int count;

        private float slopeTimer;

        public SlopeList(int size)
        {
            slopeList = new (Vector3, float)[size];
            capacity = size;
            count = 0;
            slopeTimer = 0;
        }

        public void Reset()
        {
            this = new SlopeList(capacity);
        }

        public void IncrementTimer(float deltaTime, float maxTime)
        {
            slopeTimer += deltaTime;
        
            if (slopeTimer > maxTime && count != 0)
            {
                Reset();
            }
        }

        public void Add(Vector3 slopeNormal)
        {
            if (count < capacity)
            {
                slopeList[count] = (slopeNormal,slopeTimer);
                count++;
            }
            else 
            {
                for(int i = 0; i < count-1; i++)
                {
                    slopeList[i] = slopeList[i+1];
                }
                slopeList[count-1] = (slopeNormal, slopeTimer);
            }
            slopeTimer = 0;
        }

        public Vector3 GetAngularVelocity(Vector3 linearVelocity, Vector3 axis, float rotationFactor, float minRotationSpeed, float maxRotationSpeed)
        {
            if(count <= 1) 
                return Vector3.zero;

            Vector3 angularVelocity = Vector3.zero;

            float rotationSpeed = 0;

            for (int i = 0; i < count-1; i++)
            {
                rotationSpeed += Vector3.SignedAngle(slopeList[i].Item1, slopeList[i+1].Item1, axis) / (slopeList[i].Item2 + slopeList[i+1].Item2);
            }
            rotationSpeed /= count - 1;

            angularVelocity = rotationSpeed * axis * rotationFactor;

            if (Mathf.Abs(rotationSpeed) > maxRotationSpeed)
                rotationSpeed = maxRotationSpeed * Mathf.Sign(rotationSpeed);
            else if (Mathf.Abs(rotationSpeed) < minRotationSpeed)
                rotationSpeed = 0;
            
            return angularVelocity;
        }
    }

    private KinematicCharacterMotor motor;
    private PlayerMovementPhysics physics;
    private PlayerMovementAction action;
    private PlayerMovementAbility ability;

    [SerializeField]
    private float attachThreshold;
    [SerializeField]
    private float pushOffGroundThreshold;
    [SerializeField]
    private float maxSlopeTrackTime;
    [SerializeField]
    private float ungroundRotationFactor;
    [SerializeField]
    private float ungroundRotationMinSpeed;
    [SerializeField]
    private float ungroundRotationMaxSpeed;

    private SlopeList slopeList;
    private bool foundFloorToReorientTo;

    private Vector3 internalAngularVelocity = Vector3.zero;
    public Vector3 externalVelocity { private get; set; }

    private Plane currentPlane;
    private PathCreator currentDynamicPlane;
    private PathCreator currentPlaneBreaker;

    // Debug
    #region debug
    KinematicCharacterMotorState startState;
    Plane startPlane;
    #endregion

    /// <summary>
    /// Resets default Values
    /// </summary>
    void Reset()
    {
        attachThreshold = 8;
        pushOffGroundThreshold = 1;
        maxSlopeTrackTime = 0.5f;
        ungroundRotationFactor = 1.25f;
        ungroundRotationMinSpeed = 15;
        ungroundRotationMaxSpeed = 1000;
    }

    /// <summary>
    ///  Initialize Script
    /// </summary>
    void Awake()
    {
        motor = GetComponentInParent<KinematicCharacterMotor>();
        action = GetComponent<PlayerMovementAction>();
        physics = GetComponent<PlayerMovementPhysics>();

        slopeList = new SlopeList(5);
    }

    // Used to make sure script is set up properly
    void OnValidate()
    {
        if (motor != null && motor.CharacterController == null)
            motor.CharacterController = this;
    }

    /// <summary>
    /// Used to initialize motor's reference to this script
    /// Also initializes state info for debugging
    /// </summary>
    void Start()
    {
        // Set motor's reference to this Character Controller Interface
        motor.CharacterController = this;
        SetCurrentPlane(new Plane(motor.CharacterForward, motor.Transform.position));
        motor.PlanarConstraintAxis = currentPlane.normal;
        
        // Debug
        #region debug
        startState = motor.GetState();
        startPlane = new Plane(motor.PlanarConstraintAxis, motor.Transform.position);
        physics.SetDebugGravity(-motor.CharacterUp);
        #endregion
    }

    /// <summary>
    /// Handles Input when valid
    /// </summary>
    public void HandleInput()
    {
        action.RegisterInput();
    }

#region CharacterControllerInterface

    /// <summary>
    /// This is called when the motor wants to know what its rotation should be right now
    /// </summary>
    /// <param name="currentRotation"> Reference to the player's </param>
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
        if(motor.IsGroundedThisUpdate)
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
        else if(internalAngularVelocity == Vector3.zero)
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

        action.UpdateRotation(ref currentRotation, motor, ref internalAngularVelocity, deltaTime);
        physics.UpdateRotation(ref currentRotation, motor, deltaTime);

        if(internalAngularVelocity != Vector3.zero)
        {
            if (Mathf.Abs(Vector3.Dot(internalAngularVelocity, motor.PlanarConstraintAxis)) < 0.99) 
            {
                // Project rotational velocity
                internalAngularVelocity = Vector3.Project(internalAngularVelocity, motor.PlanarConstraintAxis);
                // Maintain rotational momentum but reorient
            }
            //internalAngularVelocity = Vector3.project(internalAngularVelocity, motor.PlanarConstraintAxis).normalized * internalAngularVelocity.magnitude

            currentRotation = Quaternion.Euler(internalAngularVelocity * deltaTime) * currentRotation;
        }
    }
    
    /// <summary>
    /// This is called when the motor wants to know what its velocity should be right now
    /// </summary>
    /// <param name="currentVelocity"> Reference to the player's velocity </param>
    /// <param name="deltaTime"> Motor update time </param>
    public void UpdateVelocity(ref Vector3 currentVelocity, ref float maxMove, float deltaTime)
    {
        
        // Handle velocity projection if grounded
        if (motor.IsGroundedThisUpdate)
        {
            // The dot between the ground normal and the external velocity addition
            float dot = Vector3.Dot(externalVelocity, motor.GetEffectiveGroundNormal());
            // The velocity off the ground
            Vector3 projection = dot * motor.GetEffectiveGroundNormal();
            // If external velocity off ground is strong enough
            if(dot > 0  && projection.sqrMagnitude >= pushOffGroundThreshold * pushOffGroundThreshold)
                motor.ForceUnground();
            else
            {
                // if just landed or slope hasn't changed
                if (!motor.WasGroundedLastUpdate || motor.GetEffectiveGroundNormal() == motor.GetLastEffectiveGroundNormal())
                    // Project velocity onto ground
                    currentVelocity = Vector3.ProjectOnPlane(currentVelocity, motor.GetEffectiveGroundNormal());
                else
                    // Reorient without losing momentum
                    currentVelocity = Vector3.ProjectOnPlane(currentVelocity, motor.GetEffectiveGroundNormal()).normalized * currentVelocity.magnitude;

                // Snap external velocity to ground
                externalVelocity -= projection;
            }
        }

        // Add external velocity and reset back to zero
        currentVelocity += externalVelocity;
        externalVelocity = Vector3.zero;

        // Update velocity from components 
        //ability.UpdateVelocity();
        action.UpdateVelocity(ref currentVelocity, motor, physics.gravityDirection, ref physics.negations, deltaTime);
        physics.UpdateVelocity (ref currentVelocity, motor, deltaTime);

        currentVelocity = Vector3.ProjectOnPlane(currentVelocity, motor.PlanarConstraintAxis);

        // If on Dynamic Plane, handle movement  
        if(currentDynamicPlane != null)
        {
            VertexPath vertexPath = currentDynamicPlane.path;
            float closestPointTime = vertexPath.GetClosestTimeOnPath(motor.Transform.position);
            Vector3 closestPoint = vertexPath.GetPointAtTime(closestPointTime, EndOfPathInstruction.Stop);
            Vector3 closestPointNormal = vertexPath.GetNormal(closestPointTime, EndOfPathInstruction.Stop);

            int closestPointIndex = vertexPath.rotat

            Vector3 toPoint;
            Vector3 alongNormal; 
            maxMove = .magnitude;

        }
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
    /// Primarily used currently to handle the slope tracking for the ungrounding angular momentum mechanic
    /// </summary>
    /// <param name="deltaTime"> Motor update time </param>
    public void PostGroundingUpdate(float deltaTime)
    {
        // kKep tracking time on current slope
        slopeList.IncrementTimer(deltaTime, maxSlopeTrackTime);
        
        // Used to see if angular velocity should be set for angular momentum when ungrounding
        bool setAngularVelocity = false;

        if(motor.IsGroundedThisUpdate)
        {
            // If speed drops enough, detatch from slope
            if(Vector3.ProjectOnPlane(motor.BaseVelocity, motor.GetEffectiveGroundNormal()).magnitude < attachThreshold && Vector3.Angle(-physics.gravityDirection, motor.GetEffectiveGroundNormal()) > motor.MaxStableSlopeAngle)
            {
                motor.ForceUnground();
                setAngularVelocity = true;
            }
            // If slope changed
            else if(motor.WasGroundedLastUpdate && motor.GetEffectiveGroundNormal() != motor.GetLastEffectiveGroundNormal())
                // Start tracking new slope
                slopeList.Add(motor.GetLastEffectiveGroundNormal());
        }
        // If just ungrounded
        else if(motor.WasGroundedLastUpdate)
        {
            // Log new slope that was just ungrounded from
            slopeList.Add(motor.GetLastEffectiveGroundNormal());
            setAngularVelocity = true;
        }

        // If ungrounding angular momentum mechanic was triggered
        if(setAngularVelocity)
        {
            // Set angular velocity (if any) using slope tracking info and reset the tracker
            internalAngularVelocity = slopeList.GetAngularVelocity(motor.BaseVelocity, motor.PlanarConstraintAxis, ungroundRotationFactor, ungroundRotationMinSpeed, ungroundRotationMaxSpeed);
            slopeList.Reset();
        }
    }

    /// <summary>
    /// This is called after the motor has finished everything in its update
    /// </summary>
    /// <param name="deltaTime"> Motor update time </param>
    public void AfterCharacterUpdate(float deltaTime)
    {
        // Handle Dynamic Plane Shifting
        if (currentDynamicPlane != null)
        {
            
        }

        // Reset Ability and Action Input
        //ability.ResetInput();
        action.ResetInput();
    }

    /// <summary>
    /// This is called after when the motor wants to know if the collider can be collided with (or if we just go through it)
    /// </summary>
    /// <param name="coll"> The collider being checked </param>
    public bool IsColliderValidForCollisions(Collider coll)
    {
        // As of now all colliders are valid
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
        // If floor it hit when mid air
        if (!motor.IsGroundedThisUpdate
        && motor.StableGroundLayers.value == (motor.StableGroundLayers.value | (1 << hitCollider.gameObject.layer))
        && Vector3.Angle(-physics.gravityDirection, hitNormal) <= motor.MaxStableSlopeAngle)
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



    private void SetCurrentPlane(Plane plane)
    {
        currentPlane = plane;
        motor.BaseVelocity = Vector3.ProjectOnPlane(motor.BaseVelocity, currentPlane.normal).normalized * motor.BaseVelocity.magnitude; 
        motor.PlanarConstraintAxis = plane.normal;
        motor.SetTransientPosition(plane.ClosestPointOnPlane(motor.Transform.position));
    }


    private void EnterDynamicPlane(PathCreator pathCreator)
    {
        currentDynamicPlane = pathCreator;
        VertexPath vertexPath = currentDynamicPlane.path;
        Plane pathFloorPlane = new Plane(currentDynamicPlane.transform.up, currentDynamicPlane.transform.position);

        float closestPointTime = vertexPath.GetClosestTimeOnPath(pathFloorPlane.ClosestPointOnPlane(motor.Transform.position));
        Vector3 closestPoint = vertexPath.GetPointAtTime(closestPointTime, EndOfPathInstruction.Stop);
        Vector3 closestPointNormal = vertexPath.GetNormal(closestPointTime, EndOfPathInstruction.Stop);

        SetCurrentPlane(new Plane(closestPointNormal, closestPoint));

        Debug.DrawLine(motor.Transform.position, closestPoint, Color.yellow);
        Debug.DrawLine(motor.Transform.position, motor.TransientPosition, Color.red);
        Debug.DrawRay(closestPoint, closestPointNormal, Color.blue);
        Debug.DrawLine(closestPoint, pathCreator.GetComponent<Collider>().ClosestPoint(closestPoint + pathCreator.transform.up * 10000), Color.green);
        Debug.DrawLine(closestPoint, pathCreator.GetComponent<Collider>().ClosestPoint(closestPoint - pathCreator.transform.up * 10000), Color.green);
        
        Debug.Break();
    }

    private void ExitDynamicPlane(Collider col)
    {
        currentDynamicPlane = null;
    }

    public void HandleTriggerEnter(Collider col)
    {
        if (col.tag == "Plane")
            EnterDynamicPlane(col.GetComponent<PathCreator>());
    } 

    public void HandleTriggerExit(Collider col)
    {
        if (col.tag == "Plane")
            ExitDynamicPlane(col);
            
    } 

    /// <summary>
    /// Currently used for debugging inputs
    /// </summary>
    void Update()
    {
        // Debug
        #region debug
        // Resets the the motor state (used as a makeshift "level restart")
        if (Input.GetKeyDown(KeyCode.Return))
        {
            motor.ApplyState(startState);
            SetCurrentPlane(startPlane);
        }

        #endregion
    }
}
