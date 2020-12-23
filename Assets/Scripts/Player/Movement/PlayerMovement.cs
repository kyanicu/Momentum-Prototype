﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KinematicCharacterController;
using PathCreation;

/// <summary>
/// Interface for input relating to PlayerMovement
/// </summary>
public interface IPlayerMovementInput
{
    /// <summary>
    /// Register input for use
    /// </summary>
    /// <param name="controllerActions">The controller actions</param>
    void RegisterInput(PlayerController.PlayerActions controllerActions);
    /// <summary>
    /// Reset input registration
    /// </summary>
    void Reset();
}

/// <summary>
/// Holds wall hit info
/// </summary>
public struct WallHits
{
    /// <summary>
    /// Normal of a ceiling hit
    /// Vector3.Zero if none hit
    /// </summary>
    public Vector3 hitCeiling;
    /// <summary>
    /// Normal of a left wall hit
    /// Vector3.Zero if none hit
    /// </summary>
    public Vector3 hitLeftWall;
    /// <summary>
    /// Normal of a right wall hit
    /// Vector3.Zero if none hit
    /// </summary>
    public Vector3 hitRightWall;
}

[System.Serializable]
public class PlayerMovementValues : PlayerOverridableValues
{

    [SerializeField]
    public float maxSpeed;
    [SerializeField]
    public float attachThreshold;
    [SerializeField]
    public float pushOffGroundThreshold;
    [SerializeField]
    public float maxSlopeTrackTime;
    [SerializeField]
    public float ungroundRotationFactor;
    [SerializeField]
    public float ungroundRotationMinSpeed;
    [SerializeField]
    public float ungroundRotationMaxSpeed;

    public int negateAction;
    public int negatePhysics;

    protected override void SetValueCounts()
    {
        floatValuesCount = 7;
        intValuesCount = 2;
        vector3ValuesCount = 0;
    }

    protected override float GetFloatValue(int i)
    {
        switch (i) 
        {
            case (0) :
                return maxSpeed;
            case (1) :
                return attachThreshold;
            case (2) :
                return pushOffGroundThreshold;
            case (3) :
                return maxSlopeTrackTime;
            case (4) :
                return ungroundRotationFactor;
            case (5) :
                return ungroundRotationMinSpeed;
            case (6) :
                return ungroundRotationMaxSpeed;
            default :
                return 0;
        }
    }
    protected override void SetFloatValue(int i, float value)
    {
        switch (i) 
        {
            case (0) :
                maxSpeed = value;
                break;
            case (1) :
                attachThreshold = value;
                break;
            case (2) :
                pushOffGroundThreshold = value;
                break;
            case (3) :
                maxSlopeTrackTime = value;
                break;
            case (4) :
                ungroundRotationFactor = value;
                break;
            case (5) :
                ungroundRotationMinSpeed = value;
                break;
            case (6) :
                ungroundRotationMaxSpeed = value;
                break;
            default :
                break;
        }
    }
    protected override int GetIntValue(int i)
    {
        switch (i) 
        {
            case (0) :
                return negateAction;
            case (1) :
                return negatePhysics;
            default :
                return 0;
        }
    }
    protected override void SetIntValue(int i, int value)
    {
        switch (i) 
        {
            case (0) :
                negateAction = value;
                break;
            case (1) :
                negatePhysics = value;
                break;
            default :
                break;
        }
    }
    protected override Vector3 GetVector3Value(int i)
    {
        return Vector3.zero;
    }
    protected override void SetVector3Value(int i, Vector3 value) {}

}

[System.Serializable]
public class PlayerMovement : PlayerOverridableAttribute<PlayerMovementValues>, ICharacterController, IPlayerMovementCommunication
{

#region MovementEvents
    public event Action<PlaneChangeArgs> planeChanged;
    public event Action<KinematicCharacterMotorState> stateUpdated;
#endregion

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

    [SerializeField]
    float extraGroundProbingDistanceFactor = 0.012f;
    [SerializeField]
    float extraMaxStableSlopeFactor = 0.005f;
    float currentMaxStableSlopeAngle;

    [SerializeField]
    float groundingActionBuffer = 0.1f;
    Vector3 bufferedUngroundedNormal = Vector3.zero;

    [SerializeField]
    private PlayerMovementPhysics physics;
    [SerializeField]
    private PlayerMovementAction action;
    [SerializeField]
    private IPlayerMovementAbility ability;

    private SlopeList slopeList;
    private bool foundFloorToReorientTo;

    WallHits wallHits = new WallHits();

    private Vector3 internalAngularVelocity = Vector3.zero;
    public Vector3 externalVelocity { private get; set; }

    private Plane currentPlane;
    private Plane brokenPlane;
    private DynamicPlane[] currentDynamicPlanes;
    private PlaneBreaker[] currentPlaneBreakers;

    // Debug
    #region debug
    KinematicCharacterMotorState startState;
    Plane startPlane;
    #endregion

    /// <summary>
    ///  Constructor
    /// </summary>
    private PlayerMovement()
    {
        physics = new PlayerMovementPhysics();
        action = new PlayerMovementAction();
    } 

    public PlayerMovement(IPlayerMovementAbility _ability) : this()
    {
        ability = _ability;

        ability.addingMovementOverrides += AddAbilityOverride;
        ability.removingMovementOverrides += RemoveAbilityOverride;
    }
    
    protected override void SetDefaultBaseValues()
    {
        // Set default field values
        baseValues.maxSpeed = 125;
        baseValues.attachThreshold = 8;
        baseValues.pushOffGroundThreshold = 1;
        baseValues.maxSlopeTrackTime = 0.5f;
        baseValues.ungroundRotationFactor = 1.25f;
        baseValues.ungroundRotationMinSpeed = 15;
        baseValues.ungroundRotationMaxSpeed = 1000;
        baseValues.negateAction = 0;
        baseValues.negatePhysics = 0;
    }

    protected override void ValidateBaseValues()
    {
        action.OnValidate();
        physics.OnValidate();
        ////ability.OnValidate();
    }

    public void SetCommunicationInterface(PlayerInternalCommunicator communicator)
    {
        communicator.SetCommunication(this);

        action.SetCommunicationInterface(communicator);
        ability.SetCommunicationInterface(communicator);
    }

    /// <summary>
    /// Used to initialize motor's reference to this script
    /// Also initializes state info for debugging
    /// </summary>
    public void InitializeForPlay(KinematicCharacterMotor motor)
    {
        currentDynamicPlanes = new DynamicPlane[2];
        currentPlaneBreakers = new PlaneBreaker[2];
        slopeList = new SlopeList(5);

        SetCurrentPlane(motor, new Plane(motor.CharacterForward, motor.Transform.position));
        motor.PlanarConstraintAxis = currentPlane.normal;
        
        // Debug
        #region debug
        startState = motor.GetState();
        startPlane = new Plane(motor.PlanarConstraintAxis, motor.Transform.position);
        #endregion
    }

    /// <summary>
    /// Handles Input when valid
    /// </summary>
    public void HandleInput(PlayerController.PlayerActions controllerActions)
    {
        if(values.negateAction == 0)
            action.RegisterInput(controllerActions);
            ability.RegisterInput(controllerActions);
    }

    /// <summary>
    /// Appropriately handle entering a trigger
    /// </summary>
    /// <param name="motor"> The Character's Kinematic Motor</param>
    /// <param name="col"> The trigger collider</param>
    public void HandleTriggerEnter(KinematicCharacterMotor motor, Collider col)
    {
        switch (col.tag)
        {
            case ("Plane") :
                EnterDynamicPlane(motor, col.GetComponent<DynamicPlane>());
                break;
            case ("Plane Breaker") :
                EnterPlaneBreaker(motor, col.GetComponent<PlaneBreaker>());
                break;
            case ("Movement Effector") :
                EnterMovementEffector(col.GetComponent<MovementEffector>());
                break;
            case ("Checkpoint") :
                startState = motor.GetState();
                startPlane = new Plane(motor.PlanarConstraintAxis, motor.Transform.position);
                break;
        }
    } 

    /// <summary>
    /// Appropriately handle Exiting a trigger
    /// </summary>
    /// <param name="motor"> The Character's Kinematic Motor</param>
    /// <param name="col"> The trigger collider</param>
    public void HandleTriggerExit(Collider col)
    {
        switch (col.tag)
        {
            case ("Plane") :
                ExitDynamicPlane(col);
                break;
            case ("Plane Breaker") :
                ExitPlaneBreaker(col);
                break;
            case ("Movement Effector") :
                ExitMovementEffector(col.GetComponent<MovementEffector>());
                break;
        }
    } 

#region CharacterControllerInterface

    /// <summary>
    /// This is called when the motor wants to know what its rotation should be right now
    /// </summary>
    /// <param name="currentRotation"> Reference to the player's </param>
    /// <param name="motor"> The player's kinematic motor</param>
    /// <param name="deltaTime"> Motor update time </param>
    public void UpdateRotation(ref Quaternion currentRotation, KinematicCharacterMotor motor, float deltaTime)
    {
        Quaternion prevRot = currentRotation;
        Vector3 initialCharacterBottomHemiCenter = motor.TransientPosition + (currentRotation * motor.CharacterTransformToCapsuleBottomHemi); //(currentRotation * Vector3.down * motor.Capsule.radius);

        float slerpFactor = 30;
        if (motor.CharacterForward != motor.PlanarConstraintAxis)
        {
            
            Vector3 smoothedForward;
                if(Vector3.Dot(motor.CharacterForward, -motor.PlanarConstraintAxis) > 0.95f)
                    smoothedForward = Vector3.Slerp((motor.CharacterUp - motor.CharacterRight * 0.05f).normalized, -physics.gravityDirection, 1 - Mathf.Exp(-10 * deltaTime));
                else 
                    smoothedForward = Vector3.Slerp(motor.CharacterForward, motor.PlanarConstraintAxis, 1 - Mathf.Exp(-slerpFactor * deltaTime));
            
            currentRotation = /*Quaternion.FromToRotation(motor.CharacterForward, motor.PlanarConstraintAxis) * currentRotation;*/Quaternion.FromToRotation(motor.CharacterForward, smoothedForward) * currentRotation;
        }

        bool reorient = false;
        if(foundFloorToReorientTo)
        {
            internalAngularVelocity = Vector3.zero;
            foundFloorToReorientTo = false;
            reorient = true;
        }
        
        if(motor.IsGroundedThisUpdate)
        {
            internalAngularVelocity = Vector3.zero;

            if(motor.BaseVelocity.magnitude >= values.attachThreshold)
            {
                slerpFactor = 1000;//30;
                Vector3 smoothedGroundNormal = motor.GetEffectiveGroundNormal(); //Vector3.Slerp(motor.CharacterUp, motor.GetEffectiveGroundNormal(), 1 - Mathf.Exp(-slerpFactor * deltaTime));
                currentRotation = /*Quaternion.FromToRotation(motor.CharacterUp, motor.GetEffectiveGroundNormal()) * currentRotation;*/Quaternion.FromToRotation(motor.CharacterUp, smoothedGroundNormal) * currentRotation;
            }
            else
            {
                slerpFactor = 1000;//1;//3;;
                Vector3 smoothedUp = Vector3.Slerp(motor.CharacterUp, -physics.gravityDirection, 1 - Mathf.Exp(-slerpFactor * deltaTime));
                
                currentRotation = Quaternion.FromToRotation(motor.CharacterUp, smoothedUp) * currentRotation;
            }
        }
        else if(internalAngularVelocity == Vector3.zero)
        {
            
            if(!reorient)
                slerpFactor = 1;//3;//0;
            else
                slerpFactor = 1000;//30;

            Vector3 smoothedUp;
            if(Vector3.Dot(motor.CharacterUp, physics.gravityDirection) > 0.95f)
                smoothedUp = Vector3.Slerp((motor.CharacterUp - motor.CharacterRight * 0.05f).normalized, -physics.gravityDirection, 1 - Mathf.Exp(-slerpFactor * deltaTime));
            else
                smoothedUp = Vector3.Slerp(motor.CharacterUp, -physics.gravityDirection, 1 - Mathf.Exp(-slerpFactor * deltaTime));
            currentRotation = Quaternion.FromToRotation(motor.CharacterUp, smoothedUp) * currentRotation;
        }

        if(values.negateAction == 0)
            action.UpdateRotation(ref currentRotation, ref internalAngularVelocity, motor, deltaTime);
            ability.UpdateRotation(ref currentRotation, ref internalAngularVelocity, motor, deltaTime);
        if(values.negatePhysics == 0)
            physics.UpdateRotation(ref currentRotation, ref internalAngularVelocity, motor, deltaTime);

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

        if(motor.IsGroundedThisUpdate)
        {
            Vector3 newCharacterBottomHemiCenter = motor.TransientPosition + (currentRotation * motor.CharacterTransformToCapsuleBottomHemi);
            // Move the position to create a rotation around the bottom hemi center instead of around the pivot
            motor.SetTransientPosition((initialCharacterBottomHemiCenter - newCharacterBottomHemiCenter) + motor.TransientPosition);
        }
    }
    
    /// <summary>
    /// This is called hen the motor wants to know what its velocity should be right now
    /// </summary>
    /// <param name="currentVelocity"> Reference to the player's velocity </param>
    /// <param name="maxMove"> The max distance the player can move this update</param>
    /// <param name="motor"> The player's kinematic motor</param>
    /// <param name="deltaTime"> Motor update time </param>
    public void UpdateVelocity(ref Vector3 currentVelocity, ref float maxMove, KinematicCharacterMotor motor, float deltaTime)
    {
        
        // Handle velocity projection if grounded
        if (motor.IsGroundedThisUpdate)
        {
            // The dot between the ground normal and the external velocity addition
            float dot = Vector3.Dot(externalVelocity, motor.GetEffectiveGroundNormal());
            // The velocity off the ground
            Vector3 projection = dot * motor.GetEffectiveGroundNormal();
            // If external velocity off ground is strong enough
            if(dot > 0  && projection.sqrMagnitude >= values.pushOffGroundThreshold * values.pushOffGroundThreshold)
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
        if(values.negateAction == 0)
            action.UpdateVelocity(ref currentVelocity, motor, physics.gravityDirection, wallHits, ref physics.negations, groundingActionBuffer, bufferedUngroundedNormal, deltaTime);
        ability.UpdateVelocity(ref currentVelocity, motor, physics.gravityDirection, ref physics.negations, deltaTime);
        if(values.negatePhysics == 0)
            physics.UpdateVelocity (ref currentVelocity, motor, deltaTime);

        currentVelocity = Vector3.ProjectOnPlane(currentVelocity, motor.PlanarConstraintAxis);
        if (currentVelocity.sqrMagnitude > values.maxSpeed * values.maxSpeed)
            currentVelocity = currentVelocity.normalized * values.maxSpeed;

        if (motor.IsGroundedThisUpdate)
        {
            motor.GroundDetectionExtraDistance = currentVelocity.magnitude * extraGroundProbingDistanceFactor;
            currentMaxStableSlopeAngle = motor.MaxStableSlopeAngle + (motor.MaxStableSlopeAngle * motor.BaseVelocity.magnitude * extraMaxStableSlopeFactor);
            motor.MaxStableDenivelationAngle = currentMaxStableSlopeAngle;
        }
        else 
        {
            motor.GroundDetectionExtraDistance = 0;
            currentMaxStableSlopeAngle = motor.MaxStableSlopeAngle;
            motor.MaxStableDenivelationAngle = currentMaxStableSlopeAngle;
        }


        wallHits.hitCeiling = Vector3.zero;
        wallHits.hitLeftWall = Vector3.zero;
        wallHits.hitRightWall = Vector3.zero;
    }

    /// <summary>
    /// This is called before the motor does anything
    /// </summary>
    /// <param name="motor"> The player's kinematic motor</param>
    /// <param name="deltaTime"> Motor update time </param>
    public void BeforeCharacterUpdate(KinematicCharacterMotor motor, float deltaTime)
    {

        // If active on PlaneBreaker
        if (currentPlaneBreakers[0] != null && motor.IsGroundedThisUpdate && motor.BaseVelocity.magnitude >= values.attachThreshold)
        {
            Vector3 planeRight = Vector3.Cross(currentPlaneBreakers[0].transform.up, currentPlane.normal);
            float dot = Vector3.Dot(motor.BaseVelocity, planeRight);
            bool movingRight = dot > 0;            
            /*
            Plane traversalPlane;
            Plane approachingPlaneTransition = currentPlaneBreaker.GetApproachingPlaneTransition(motor.Transform.position, currentVelocity.normalized, dot > 0, out traversalPlane);
            */    SetCurrentPlane(motor, currentPlaneBreakers[0].GetClosestPathPlane(/*motor.TransientPosition + (motor.CharacterUp * motor.Capsule.radius)*/motor.Transform.position, movingRight), true);//SetCurrentPlane(motor, currentPlaneBreaker.GetClosestPathPlane(motor.TransientPosition + (motor.CharacterUp * motor.Capsule.radius)/*motor.Transform.position*/, movingRight), true);
            //if (dot != 0 && approachingPlaneTransition.normal != Vector3.zero && approachingPlaneTransition.distance != float.PositiveInfinity)
            //{
            //    float dist;
                //if (approachingPlaneTransition.Raycast(new Ray(motor.Transform.position, currentVelocity), out dist))
                    //maxMove = dist;
            //}
            //Debug.Break();
        }
        // If on Dynamic Plane, handle movement  
        else if(currentDynamicPlanes[0] != null)
        {
            Vector3 planeRight = Vector3.Cross(currentDynamicPlanes[0].transform.up, currentPlane.normal);
            float dot = Vector3.Dot(motor.BaseVelocity, planeRight);
            bool movingRight = dot > 0;
            /*
            Plane traversalPlane;
            Plane approachingPlaneTransition = currentDynamicPlanes[0].GetApproachingPlaneTransition(motor.Transform.position, movingRight, out traversalPlane);
            */    SetCurrentPlane(motor, currentDynamicPlanes[0].GetClosestPathPlane(motor.Transform.position, movingRight));
            /*
            if (dot != 0 && approachingPlaneTransition.normal != Vector3.zero && approachingPlaneTransition.distance != float.PositiveInfinity)
            {
                float dist;
                if (approachingPlaneTransition.Raycast(new Ray(motor.Transform.position, currentVelocity), out dist))
                    maxMove = dist;
            }
            */
        }

        if (settingPlane.normal != Vector3.zero)
            SetCurrentPlane(motor, settingPlane, settingPlaneBreaker, true);

        ability.BeforeCharacterUpdate(motor, deltaTime);
    }

    /// <summary>
    /// This is called after the motor has finished its ground probing, but before PhysicsMover/Velocity/etc.... handling
    /// Primarily used currently to handle the slope tracking for the ungrounding angular momentum mechanic
    /// </summary>
    /// <param name="motor"> The player's kinematic motor</param>
    /// <param name="deltaTime"> Motor update time </param>
    public void PostGroundingUpdate(KinematicCharacterMotor motor, float deltaTime)
    {
        // kKep tracking time on current slope
        slopeList.IncrementTimer(deltaTime, values.maxSlopeTrackTime);
        
        // Used to see if angular velocity should be set for angular momentum when ungrounding
        bool setAngularVelocity = false;

        if(motor.IsGroundedThisUpdate)
        {
            // If speed drops enough, detatch from slope
            if(Vector3.ProjectOnPlane(motor.BaseVelocity, motor.GetEffectiveGroundNormal()).magnitude < values.attachThreshold && Vector3.Angle(-physics.gravityDirection, motor.GetEffectiveGroundNormal()) > motor.MaxStableSlopeAngle)
            {
                motor.ForceUnground();
                setAngularVelocity = true;
            }
            // If slope changed
            else if(motor.WasGroundedLastUpdate && motor.GetEffectiveGroundNormal() != motor.GetLastEffectiveGroundNormal())
                // Start tracking new slope
                slopeList.Add(motor.GetLastEffectiveGroundNormal());

            // If just grounded
            if(!motor.WasGroundedLastUpdate)
            {
                //grounded?.Invoke(this, EventArgs.Empty);
            }

        }
        // If just ungrounded
        else if(motor.WasGroundedLastUpdate)
        {
            // Log new slope that was just ungrounded from
            slopeList.Add(motor.GetLastEffectiveGroundNormal());
            setAngularVelocity = true;
            //ungrounded?.Invoke(this, EventArgs.Empty);
            if(!motor.MustUnground())
                StartUngroundedBuffering(motor.GetLastEffectiveGroundNormal());
        }

        // If ungrounding angular momentum mechanic was triggered
        if(setAngularVelocity)
        {
            // Set angular velocity (if any) using slope tracking info and reset the tracker
            internalAngularVelocity = slopeList.GetAngularVelocity(motor.BaseVelocity, motor.PlanarConstraintAxis, values.ungroundRotationFactor, values.ungroundRotationMinSpeed, values.ungroundRotationMaxSpeed);
            slopeList.Reset();
        }
        ability.PostGroundingUpdate(motor, deltaTime);
    }

    /// <summary>
    /// This is called after the motor has finished everything in its update
    /// </summary>
    /// <param name="motor"> The player's kinematic motor</param>
    /// <param name="deltaTime"> Motor update time </param>
    public void AfterCharacterUpdate(KinematicCharacterMotor motor, float deltaTime)
    {
        ability.AfterCharacterUpdate(motor, deltaTime);
        stateUpdated?.Invoke(motor.GetState());

        // Reset Ability and Action Input
        ability.ResetInput();
        action.ResetInput();

        //if(motor.WasGroundedLastUpdate != motor.IsGroundedThisUpdate)
            //Debug.Break();
    }

    /// <summary>
    /// This is called after when the motor wants to know if the collider can be collided with (or if we just go through it)
    /// </summary>
    /// <param name="motor"> The player's kinematic motor</param>
    /// <param name="coll"> The collider being checked </param>
    public bool IsColliderValidForCollisions(KinematicCharacterMotor motor, Collider coll)
    {
        // As of now all colliders are valid
        return ability.IsColliderValidForCollisions(motor, coll);
    }

    /// <summary>
    /// This is called when the motor's ground probing detects a ground hit
    /// </summary>
    /// <param name="motor"> The player's kinematic motor</param>
    /// <param name="hitCollider">The ground collider </param>
    /// <param name="hitNormal"> The ground normal </param>
    /// <param name="hitPoint"> The ground point </param>
    /// <param name="hitStabilityReport"> The ground stability </param>
    public void OnGroundHit(KinematicCharacterMotor motor, Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
    {

        //Vector3 initialCharacterBottomHemiCenter = motor.TransientPosition + (motor.CharacterUp * motor.Capsule.radius);
            /*
            slerpFactor = 300;
            Vector3 smoothedGroundNormal = Vector3.Slerp(motor.CharacterUp, motor.GetEffectiveGroundNormal(), 1 - Mathf.Exp(-slerpFactor * deltaTime));
            */
            //motor.SetRotation(Quaternion.FromToRotation(motor.CharacterUp, hitNormal) * motor.TransientRotation);//Quaternion.FromToRotation(motor.CharacterUp, smoothedGroundNormal) * currentRotation;
            //motor.SetTransientPosition(initialCharacterBottomHemiCenter + (motor.TransientRotation * Vector3.down * motor.Capsule.radius));   
             
        ability.OnGroundHit(motor, hitCollider, hitNormal, hitPoint, ref hitStabilityReport);
    }

    /// <summary>
    /// This is called when the motor's movement logic detects a hit
    /// </summary>
    /// <param name="motor"> The player's kinematic motor</param>
    /// <param name="hitCollider"> The hit collider </param>
    /// <param name="hitNormal"> The hit normal </param>
    /// <param name="hitPoint"> The hit point </param>
    /// <param name="hitStabilityReport"> The hit stability </param>
    public void OnMovementHit(KinematicCharacterMotor motor, Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
    {   
        /*if(motor.IsGroundedThisUpdate && hitStabilityReport.IsStable && motor.BaseVelocity.magnitude >= values.attachThreshold)
        {
            Vector3 initialCharacterBottomHemiCenter = motor.TransientPosition + (motor.CharacterUp * motor.Capsule.radius);
            /*
            slerpFactor = 300;
            Vector3 smoothedGroundNormal = Vector3.Slerp(motor.CharacterUp, motor.GetEffectiveGroundNormal(), 1 - Mathf.Exp(-slerpFactor * deltaTime));
            */
            /*motor.SetRotation(Quaternion.FromToRotation(motor.CharacterUp, hitNormal) * motor.TransientRotation);//Quaternion.FromToRotation(motor.CharacterUp, smoothedGroundNormal) * currentRotation;
             motor.SetTransientPosition(initialCharacterBottomHemiCenter + (motor.TransientRotation * Vector3.down * motor.Capsule.radius));           
        }
        // If floor it hit when mid air
        else */if (!motor.IsGroundedThisUpdate
        && motor.StableGroundLayers.value == (motor.StableGroundLayers.value | (1 << hitCollider.gameObject.layer))
        && Vector3.Angle(-physics.gravityDirection, hitNormal) <= motor.MaxStableSlopeAngle)
        { 
            foundFloorToReorientTo = true;
        }

        if (!hitStabilityReport.IsStable)
        {
            if (Vector3.Dot(physics.gravityDirection, hitNormal) >= 0.5)
                wallHits.hitCeiling = hitNormal;
            else if (Vector3.Dot(motor.CharacterRight, hitNormal) < 0)
                wallHits.hitRightWall = hitNormal;
            else
                wallHits.hitLeftWall = hitNormal;
        }

        ability.OnMovementHit(motor, hitCollider, hitNormal, hitPoint, ref hitStabilityReport);
    }

    /// <summary>
    /// This is called after every move hit, to give you an opportunity to modify the HitStabilityReport to your liking
    /// </summary>
    /// <param name="motor"> The player's kinematic motor </param>
    /// <param name="hitCollider"> The hit collider </param>
    /// <param name="hitNormal"> The hit normal </param>
    /// <param name="hitPoint"> The hit point </param>
    /// <param name="hitPoint"></param>
    /// <param name="atCharacterPosition"> The character position on hit </param>
    /// <param name="atCharacterRotation"> The character rotation on hit </param>
    /// <param name="hitStabilityReport"> The hit stability </param>
    public void ProcessHitStabilityReport(KinematicCharacterMotor motor, Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport)
    {
        if (motor.IsGroundedThisUpdate && !hitStabilityReport.IsStable && Vector3.Angle(motor.GetEffectiveGroundNormal(), hitNormal) < currentMaxStableSlopeAngle)
        {
            hitStabilityReport.IsStable = true;
        }

        float dotEpsilon = 0.998f;//99998f;
        float ledgeHitDot;
        float groundHitDot;
        if (hitStabilityReport.IsStable)//Vector3.Dot(hitStabilityReport.OuterNormal, hitNormal) < 1)
        {
            // Is ledge
            if (hitStabilityReport.LedgeDetected || Vector3.Angle(hitStabilityReport.OuterNormal, hitStabilityReport.InnerNormal) > motor.MaxStableSlopeAngle)//(Vector3.Dot(hitStabilityReport.InnerNormal, hitNormal) == 1 || Vector3.Dot(hitStabilityReport.InnerNormal, hitStabilityReport.OuterNormal) == 1 || Vector3.Angle(hitStabilityReport.OuterNormal, hitStabilityReport.InnerNormal) > motor.MaxStableSlopeAngle)
            {
                hitStabilityReport.IsStable = false;
            }
            else if (hitStabilityReport.InnerNormal == hitStabilityReport.OuterNormal && (groundHitDot = Vector3.Dot(motor.GroundingStatus.GroundNormal, hitNormal)) < dotEpsilon && (ledgeHitDot = Vector3.Dot(hitStabilityReport.OuterNormal, hitNormal)) < dotEpsilon && ledgeHitDot != groundHitDot)
            {
                Debug.DrawRay(hitPoint, hitStabilityReport.OuterNormal*5, Color.magenta, 5);
                Debug.DrawRay(hitPoint, hitStabilityReport.InnerNormal*4, Color.cyan, 5);
                Debug.DrawRay(hitPoint, hitNormal*3, Color.blue, 5);
                //Debug.Break();
                Debug.LogWarning("Faulty Ledge Detection, setting to stable to prevent flying off. Debug Rays drawn for 5 seconds. Likely caused by Mesh Collider issue. Try Replacing area with primitive/concave collider if grounding should not be allowed on this hit.");
                Debug.LogWarning("Dot between Ledge and Hit normals: " + ledgeHitDot);
                Debug.LogWarning    ("Dot between Ground and Hit normals: " + groundHitDot);
                hitStabilityReport.IsStable = true;
            }
        }

        ability.ProcessHitStabilityReport(motor, hitCollider, hitNormal, hitPoint, atCharacterPosition, atCharacterRotation, ref hitStabilityReport);
    }

    /// <summary>
    /// This is called when the character detects discrete collisions (collisions that don't result from the motor's capsuleCasts when moving)
    /// </summary>
    /// <param name="motor"> The player's kinematic motor</param>
    /// <param name="hitCollider"> The detected collider </param>
    public void OnDiscreteCollisionDetected(KinematicCharacterMotor motor, Collider hitCollider)
    {
        ability.OnDiscreteCollisionDetected(motor, hitCollider);
    }

#endregion

    private void StartUngroundedBuffering(Vector3 ungroundedNormal)
    {
        bufferedUngroundedNormal = ungroundedNormal;
        GameManager.Instance.TimerViaRealTime(groundingActionBuffer, EndUngroundedBuffering);
    }

    private void EndUngroundedBuffering()
    {
        bufferedUngroundedNormal = Vector3.zero;
    }

    Plane settingPlane = new Plane(Vector3.zero, Vector3.zero);
    bool settingPlaneBreaker = false;
    private void SetCurrentPlane(KinematicCharacterMotor motor, Plane plane, bool breaker = false, bool immediate = false)
    {
        
        if (plane.normal == Vector3.zero || (plane.normal == currentPlane.normal && plane.distance == currentPlane.distance))
            return; 
    
        if (immediate)
        {
            motor.BaseVelocity = Quaternion.FromToRotation(currentPlane.normal, plane.normal) * motor.BaseVelocity;//Vector3.ProjectOnPlane(motor.BaseVelocity, currentPlane.normal).normalized * motor.BaseVelocity.magnitude; 
            motor.PlanarConstraintAxis = plane.normal;
            motor.SetPosition(plane.ClosestPointOnPlane(motor.Transform.position));
            planeChanged?.Invoke(new PlaneChangeArgs(plane.normal, breaker));
            settingPlane = new Plane(Vector3.zero, Vector3.zero);
            settingPlaneBreaker = false;
        }
        else
        {
            settingPlane = plane;
            settingPlaneBreaker = breaker;
        }
    }

    private void EnterDynamicPlane(KinematicCharacterMotor motor, DynamicPlane dynamicPlane)
    {
        if (currentDynamicPlanes[0] != null && currentDynamicPlanes[0].prioritize)
        {
            currentDynamicPlanes[1] = dynamicPlane;
        }
        else
        {    
            if (currentDynamicPlanes[0] != null)
                currentDynamicPlanes[1] = currentDynamicPlanes[0];

            currentDynamicPlanes[0] = dynamicPlane;

            if (currentPlaneBreakers[0] == null || !motor.IsGroundedThisUpdate)
            {
                Vector3 planeRight = Vector3.Cross(currentDynamicPlanes[0].transform.up, currentPlane.normal);
                float dot = Vector3.Dot(motor.BaseVelocity, planeRight);
                bool movingRight = dot > 0;
                SetCurrentPlane(motor, dynamicPlane.GetClosestPathPlane(motor.Transform.position, movingRight));
            }
        }
    }

    private void ExitDynamicPlane(Collider col)
    {
        if (currentDynamicPlanes[0].gameObject == col.gameObject)
        {
            currentDynamicPlanes[0] = currentDynamicPlanes[1];
            currentDynamicPlanes[1] = null;
        }
        else if (currentDynamicPlanes[1].gameObject == col.gameObject)
        {
            currentDynamicPlanes[1] = null;
        }
    }

    private void EnterPlaneBreaker(KinematicCharacterMotor motor, PlaneBreaker planeBreaker)
    {
        if (currentPlaneBreakers[0] != null && currentPlaneBreakers[0].prioritize)
        {
            currentPlaneBreakers[1] = planeBreaker;
        }
        else
        {    
            if (currentPlaneBreakers[0] != null)
                currentPlaneBreakers[1] = currentPlaneBreakers[0];

            currentPlaneBreakers[0] = planeBreaker;
            brokenPlane = currentPlane;
            Vector3 planeRight = Vector3.Cross(Vector3.ProjectOnPlane(-physics.gravityDirection, motor.PlanarConstraintAxis).normalized, currentPlane.normal);
            float dot = Vector3.Dot(motor.BaseVelocity, planeRight);
            bool movingRight = dot > 0;
            if(motor.IsGroundedThisUpdate)
                SetCurrentPlane(motor, planeBreaker.GetClosestPathPlane(motor.Transform.position, movingRight), true);
        }
    }

    private void ExitPlaneBreaker(Collider col)
    {
        if (currentPlaneBreakers[0].gameObject == col.gameObject)
        {
            currentPlaneBreakers[0] = currentPlaneBreakers[1];
            currentPlaneBreakers[1] = null;
            if (currentPlaneBreakers[0] == null && currentDynamicPlanes[0] == null)
                currentPlane = brokenPlane;
        }
        else if (currentPlaneBreakers[1].gameObject == col.gameObject)
        {
            currentPlaneBreakers[1] = null;
        }
    }

    private void EnterMovementEffector(MovementEffector effector)
    {
        for (int i = 0; i < effector.movementOverrides.Count; i++)
        {
            ApplyOverride(effector.movementOverrides[i].item1, effector.movementOverrides[i].item2);
        }

        for (int i = 0; i < effector.physicsOverrides.Count; i++)
        {
            physics.ApplyOverride(effector.physicsOverrides[i].item1, effector.physicsOverrides[i].item2);
        }

        for (int i = 0; i < effector.actionOverrides.Count; i++)
        {
            action.ApplyOverride(effector.actionOverrides[i].item1, effector.actionOverrides[i].item2);
        }

        ability.EnterMovementEffector(effector);
    }

    private void ExitMovementEffector(MovementEffector effector)
    {
        for (int i = 0; i < effector.movementOverrides.Count; i++)
        {
            RemoveOverride(effector.movementOverrides[i].item1, effector.movementOverrides[i].item2);
        }

        for (int i = 0; i < effector.physicsOverrides.Count; i++)
        {
            physics.RemoveOverride(effector.physicsOverrides[i].item1, effector.physicsOverrides[i].item2);
        }

        for (int i = 0; i < effector.actionOverrides.Count; i++)
        {
            action.RemoveOverride(effector.actionOverrides[i].item1, effector.actionOverrides[i].item2);
        }

        ability.ExitMovementEffector(effector);
    }

    private void AddAbilityOverride(AbilityOverrideArgs args)
    {
        for (int i = 0; i < args.movementOverrides.Count; i++)
        {
            ApplyOverride(args.movementOverrides[i].item1, args.movementOverrides[i].item2);
        }

        for (int i = 0; i < args.physicsOverrides.Count; i++)
        {
            physics.ApplyOverride(args.physicsOverrides[i].item1, args.physicsOverrides[i].item2);
        }

        for (int i = 0; i < args.actionOverrides.Count; i++)
        {
            action.ApplyOverride(args.actionOverrides[i].item1, args.actionOverrides[i].item2);
        }
    }

    private void RemoveAbilityOverride(AbilityOverrideArgs args)
    {
        for (int i = 0; i < args.movementOverrides.Count; i++)
        {
            RemoveOverride(args.movementOverrides[i].item1, args.movementOverrides[i].item2);
        }

        for (int i = 0; i < args.physicsOverrides.Count; i++)
        {
            physics.RemoveOverride(args.physicsOverrides[i].item1, args.physicsOverrides[i].item2);
        }

        for (int i = 0; i < args.actionOverrides.Count; i++)
        {
            action.RemoveOverride(args.actionOverrides[i].item1, args.actionOverrides[i].item2);
        }
    }

    #region debug        
    /// <summary>
    /// Currently used for debugging inputs
    /// </summary>
    public void ResetState(KinematicCharacterMotor motor)
    {
        // Resets the the motor state (used as a makeshift "level restart")
        SetCurrentPlane(motor, startPlane);
        motor.ApplyState(startState);
        motor.BaseVelocity = Vector3.zero;
    }
    #endregion
}
