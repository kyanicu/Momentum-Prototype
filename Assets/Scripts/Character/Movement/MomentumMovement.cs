﻿/*********************************
* Copyright: © 2020 Katherine Leitao <kathleitao13@gmail.com> <github.com/kyanicu>
* All Rights Reserved
**********************************/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KinematicCharacterController;
using PathCreation;

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

/// <summary>
/// The overrideable values for PlayerMovement 
/// </summary>
[System.Serializable]
public class PlayerMovementValues : CharacterOverridableValues
{
    /// <summary>
    /// The absolute max speed the player can ever achieve
    /// </summary>
    [SerializeField]
    public float maxSpeed;
    /// <summary>
    /// The speed needed for the player to attach to a surface and reorient along it 
    /// </summary>
    [SerializeField]
    public float attachThreshold;
    //// <summary>
    //// The acceleration off the current slope needed to force unground the character
    //// </summary>
    ////[SerializeField]
    ////public float pushOffGroundThreshold;
    /// <summary>
    /// The max time for a slope to stay registered in the slope tracking for determining rotational momentum
    /// </summary>
    [SerializeField]
    public float maxSlopeTrackTime;
    /// <summary>
    /// THe scalar factor for the kept speed of rotational momentum when ungrounding
    /// </summary>
    [SerializeField]
    public float ungroundRotationFactor;
    /// <summary>
    /// The minimum speed of rotational momentum for it to be kept when ungrounding
    /// </summary>
    [SerializeField]
    public float ungroundRotationMinSpeed;
    /// <summary>
    /// The maximum speed of rotational momentum the player can keep when ungrounding
    /// </summary>
    [SerializeField]
    public float ungroundRotationMaxSpeed;

    protected override float[] floatValues 
    {
        get 
        { 
            return new float[]
            {
                maxSpeed,
                attachThreshold,
                maxSlopeTrackTime,
                ungroundRotationFactor,
                ungroundRotationMinSpeed,
                ungroundRotationMaxSpeed,
            };
        }

        set
        {
            maxSpeed = value[0];
            attachThreshold = value[1];
            maxSlopeTrackTime = value[2];
            ungroundRotationFactor = value[3];
            ungroundRotationMinSpeed = value[4];
            ungroundRotationMaxSpeed = value[5];
        }
    }

}

/// <summary>
/// Represents and handles all aspects of Player Movement
/// Uses the KinematicCharacterMotor to determine logicstics and calculations for how the player moves.
/// KinematicCharacterMotor Unity Component (3rd Party Extension from Asset Store) uses a kinematic rigidbody to handle lower level velocity/rotation/slope calculation, ground probing, collision, rigidbody interactions, etc.
/// Movement Component, itself, directly controls the kinematic motor and uses it's info to create the game's character controller, based on this game's requirements and mechanics, such as Dynamic Plane Shifting, linear and rotational momentum conservation/usage, slope handling, 360 degree movement, collision effects, etc. Uses Physics, Action, and Ability to calculate Acceleration each physics tick
/// Physics component handles environmental effects on velocity and rotation
/// Action component handles player input effects on velocity and rotation
/// Ability component handles special actions that can act as an extension to the Movement component itself, allowing it to handle and control Physics, Action, and core Movement component aspects
/// Overridable Attribute to allow for temporarily modified movement
/// Implements ICharacterController to allow control over the players KinematicCharacterMotor Unity Component
/// Implements IPlayerMovementCmmunication to allow communication between other Player components
/// </summary>
[RequireComponent(typeof(KinematicCharacterMotor))]
public class MomentumMovement : CharacterMovement, ICharacterController
{

    public override Vector3 position { get { return motor.TransientPosition; } set { motor.SetPosition(value); } }
    public override Quaternion rotation { get { return motor.TransientRotation; } set { motor.SetRotation(value); } }
    public override Vector3 velocity { get { return motor.BaseVelocity; } set { externalVelocity += -motor.BaseVelocity + value; } }
    public override Vector3 angularVelocity { get { return internalAngularVelocity; } set { if (!motor.IsGroundedThisUpdate) internalAngularVelocity = value; } }
    public override Vector3 groundNormal { get { return motor.GetEffectiveGroundNormal(); } }
    public override Vector3 lastGroundNormal { get { return motor.GetLastEffectiveGroundNormal(); } }
    public override bool isGroundedThisUpdate { get { return motor.IsGroundedThisUpdate; } }
    public override bool wasGroundedLastUpdate { get { return motor.WasGroundedLastUpdate; } }

    /// <summary>
    /// Hold information on recently changed slopes
    /// Used primarily to handle calculating rotational momentum on ungrounding
    /// </summary>
    private struct SlopeList
    {
        /// <summary>
        /// The list of registered slopes
        /// </summary>
        private (Vector3, float)[] slopeList;
        /// <summary>
        /// The maximum ammount of slopes that can be registered
        /// </summary>
        private int capacity;
        /// <summary>
        /// The current ammount of registered slopes
        /// </summary>
        private int count;

        /// <summary>
        /// The time the slope has been registered as the current slope
        /// </summary>
        private float slopeTimer;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="size"> The max capacity of registered slopes </param>
        public SlopeList(int size)
        {
            slopeList = new (Vector3, float)[size];
            capacity = size;
            count = 0;
            slopeTimer = 0;
        }

        /// <summary>
        /// Resets the list to empty
        /// </summary>
        public void Reset()
        {
            this = new SlopeList(capacity);
        }

        /// <summary>
        /// Increment timer
        /// </summary>
        /// <param name="deltaTime"> Difference in time since last increment </param>
        /// <param name="maxTime"> The max time allowed to stay on a single slope</param>
        public void IncrementTimer(float deltaTime, float maxTime)
        {
            slopeTimer += deltaTime;

            // If timer exeeds max time, deregister all slopes
            if (slopeTimer > maxTime && count != 0)
            {
                Reset();
            }
        }

        /// <summary>
        /// Registers a new slope as the current slope
        /// </summary>
        /// <param name="slopeNormal"> The slope normal </param>
        public void Add(Vector3 slopeNormal)
        {
            // If slope can simply be added
            if (count < capacity)
            {
                slopeList[count] = (slopeNormal,slopeTimer);
                count++;
            }
            // If max size exceeded
            else 
            {
                // Shift all slopes back an index, Loosing index 0
                for(int i = 0; i < count-1; i++)
                {
                    slopeList[i] = slopeList[i+1];
                }
                // Add new slope at top
                slopeList[count-1] = (slopeNormal, slopeTimer);
            }
            // Reset the timer to track the new current slope
            slopeTimer = 0;
        }

        /// <summary>
        /// Calculate (heavily estimated) the angular velocity based on the slopes and current timer
        /// </summary>
        /// <param name="linearVelocity"> The Player's current Linear velocity</param>
        /// <param name="axis"> The player's current plane normal </param>
        /// <param name="rotationFactor"> The rotation factor for scaling the calculated rotation </param>
        /// <param name="minRotationSpeed"> The minimum speed allowed for rotational momentum to not be set to zero </param>
        /// <param name="maxRotationSpeed"> The maximum speed for rotational momentum to be capped at </param>
        /// <returns> The calculated angular velocity </returns>
        public Vector3 GetAngularVelocity(Vector3 linearVelocity, Vector3 axis, float rotationFactor, float minRotationSpeed, float maxRotationSpeed)
        {
            // No rotational momentum
            if(count <= 1) 
                return Vector3.zero;

            Vector3 angularVelocity = Vector3.zero;

            float rotationSpeed = 0;

            // Estimate rotational speed via kinematic equations
            for (int i = 0; i < count-1; i++)
            {
                rotationSpeed += Vector3.SignedAngle(slopeList[i].Item1, slopeList[i+1].Item1, axis) / (slopeList[i].Item2 + slopeList[i+1].Item2);
            }
            rotationSpeed /= count - 1;

            // Calculate final rotational momentum
            angularVelocity = rotationSpeed * axis * rotationFactor;

            // Ensure max and min
            if (Mathf.Abs(rotationSpeed) > maxRotationSpeed)
                rotationSpeed = maxRotationSpeed * Mathf.Sign(rotationSpeed);
            else if (Mathf.Abs(rotationSpeed) < minRotationSpeed)
                rotationSpeed = 0;
            
            return angularVelocity;
        }
    }

#region Extra Ground Stability Values
    /// <summary>
    /// The current multiplied distance for probing for a ground
    /// Increases with player speed
    /// </summary>
    [SerializeField]
    float extraGroundProbingDistanceFactor = 0.012f;
    /// <summary>
    /// The current extra multiplied slope differece for staying on ground
    /// Increases with player speed
    /// </summary>
    [SerializeField]
    float extraMaxStableSlopeFactor = 0.005f;
    /// <summary>
    /// Current total max slope difference for staying on ground
    /// </summary>
    float currentMaxStableSlopeAngle;
    #endregion

#region BufferingInfo
    /// <summary>
    /// Buffer used to allow for grounded within a buffered time before grounding and after ungroundiong
    /// </summary>
    [SerializeField]
    float groundingActionBuffer = 0.1f;
    /// <summary>
    /// The buffered ground slope during buffer time after ungrounding
    /// </summary>
    Vector3 bufferedUngroundedNormal = Vector3.zero;
#endregion

    [SerializeField]
    private CharacterOverridableAttribute<PlayerMovementValues> overridableAttribute;

#region Helper/Extension Components
    /// <summary>
    /// The component for handling special player actions that can have a more direct effect on the player's Movement itself, allowing for more than just adding acceleration each physics tick
    /// </summary>
    private PlayerMovementAbility ability;
#endregion

    /// <summary>
    /// The list of tracked slopes for calculating rotational momentum
    /// </summary>
    private SlopeList slopeList;

    /// <summary>
    /// Flag for if the player is spinning midair, yet finds a floor inline with gravity that it can snap on to, even if it's not oriented towards it
    /// </summary>
    private bool foundFloorToReorientTo;

    /// <summary>
    /// Info on the non floor surfaces hit each physics tick
    /// </summary>
    WallHits wallHits = new WallHits();

    /// <summary>
    /// The internally calculated angular velocity
    /// </summary>
    private Vector3 internalAngularVelocity = Vector3.zero;
    /// <summary>
    /// Allows for external additions to velocity without effecting the core internally calculated velocity 
    /// </summary>
    /// <value></value>
    public Vector3 externalVelocity { private get; set; }
#region flags
////    private int velocityLocked = 0;
////    private Vector3 preLockedVel;
////    private bool revertLockedVelAfter;
    private bool zeroingVel;
    ////private int angularVelocityLocked = 0;
    private bool forceUngrounding = false;
    /// <summary>
    /// Set to true on new plane so that UpdateRotation can properly align the player with the new plane
    /// </summary>
    private bool setOrientationAlongPlane = false;

#endregion

    // Debug
#region debug
    /// <summary>
    /// Holds the initial kinematic motor state to be applied on a debug reset
    /// Essentially a forced respawn
    /// </summary>
    KinematicCharacterMotorState startState;
    /// <summary>
    /// Holds the initial plane to be applied on a debug reset
    /// </summary>
    Plane startPlane;
    #endregion

#region References
    /// <summary>
    /// Unity Component that handles collision and kinematic movement
    /// Used by PlayerMovement to handle player movement mechanics
    /// </summary>
    protected KinematicCharacterMotor motor;
#endregion

    public override bool usePlaneBreakers { get { return true; } }
    public override bool reaffirmCurrentPlane { get { return false; } }

#region MonoBehavior Messages Handling
    
    /// <summary>
    /// Sets the default base overridable values
    /// </summary>
    void Reset()
    {
        // Set default field values
        overridableAttribute.baseValues.maxSpeed = 125;
        overridableAttribute.baseValues.attachThreshold = 8;
        ////overridableAttribute.baseValues.pushOffGroundThreshold = 1;
        overridableAttribute.baseValues.maxSlopeTrackTime = 0.5f;
        overridableAttribute.baseValues.ungroundRotationFactor = 1.25f;
        overridableAttribute.baseValues.ungroundRotationMinSpeed = 15;
        overridableAttribute.baseValues.ungroundRotationMaxSpeed = 1000;
    }

    protected override void Awake()
    {
        base.Awake();

        motor = GetComponent<KinematicCharacterMotor>();
        // Instantiate helper components
        ability = GetComponent<PlayerMovementAbility>();

        GetComponent<ICharacterValueOverridabilityCommunication>()?.RegisterOverridability(overridableAttribute);
    }

    /// <summary>
    /// Setup the class to be ready for gameplay
    /// Also initializes state info for debugging
    /// </summary>
    protected void Start()
    {
        motor.CharacterController = this;

        slopeList = new SlopeList(5);

        
        GetComponent<DynamicPlaneConstraint>()?.SetCurrentPlane(new Plane(motor.CharacterForward, motor.Transform.position));
        Plane currentPlane = new Plane(motor.CharacterForward, motor.transform.position);
        motor.PlanarConstraintAxis = currentPlane.normal;

        // Debug
        #region debug
        startState = motor.GetState();
        startPlane = currentPlane;
        #endregion
    }
    
    /// <summary>
    /// Appropriately handle entering a trigger
    /// </summary>
    /// <param name="col"> The trigger collider</param>
    void OnTriggerEnter(Collider col)
    {
        switch (col.tag)
        {
            case ("Checkpoint") :
                startState = motor.GetState();
                startPlane = new Plane(motor.PlanarConstraintAxis, motor.Transform.position);
                break;
        }
    } 
    #endregion

#region Communication Methods

    public override void ZeroVelocity()
    {
        if (kinematicPath != null)
            return;

        zeroingVel = true;
        externalVelocity = Vector3.zero;
        internalAngularVelocity = Vector3.zero;
    }

    public override void SetKinematicPath(Vector3 vel, float time)
    {   
        if (kinematicPath != null)
            EndKinematicPath();

        KinematicPath kp = new KinematicPath 
        { 
            velocity = vel,
            velocityAfter = (zeroingVel ? Vector3.zero : motor.BaseVelocity + externalVelocity),
            timer = GameManager.Instance.TimerViaGameTime(time, EndKinematicPath)
        };
        
        kinematicPath = kp;

        externalVelocity += vel;
    }

    ////public override void EndKinematicPath()
    ////{
    ////    GameManager.Instance.StopCoroutine(kinematicPath.Value.timer);

    ////    externalVelocity = -motor.BaseVelocity + kinematicPath.Value.velocityAfter;

    ////    kinematicPath = null;
    ////}

    public override void Flinch()
    {
        base.Flinch();

        ability.Flinch();
    }

    public override void ForceUnground()
    {
        forceUngrounding = true;
    }

    ////public override void AddImpulse(Vector3 impulse)
    ////{
    ////    if (kinematicPath != null)
    ////        return;

    ////    externalVelocity += impulse;
    ////}

    public override void AddImpulseAtPoint(Vector3 impulse, Vector3 point)
    {
        if (kinematicPath != null)
            return;

        AddImpulse(impulse);
        internalAngularVelocity += Vector3.Cross(point - motor.TransientPosition, impulse);
    }
#endregion

#region CharacterControllerInterface

    /// <summary>
    /// Sets the rotation of the kinematic motor
    /// ? Is this being done efficiently with how it uses Vector and Quaternion math?
    /// </summary>
    /// <param name="currentRotation"> Reference to the motor's rotation </param>
    /// <param name="motor"> The player's kinematic motor</param>
    /// <param name="deltaTime"> Motor update time </param>
    public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
    {
        //Hold the previous rotation info
        Quaternion prevRot = currentRotation;
        Vector3 initialCharacterBottomHemiCenter = motor.TransientPosition + (currentRotation * motor.CharacterTransformToCapsuleBottomHemi); 
        
        // Handles reorienting the player onto the proper plane if not already
        if (setOrientationAlongPlane)
            // Set forward to match the current plane
            currentRotation = Quaternion.FromToRotation(motor.CharacterForward, motor.PlanarConstraintAxis) * currentRotation;

        // See if the player should reorient itself back to the gravity-based ground
        bool reorient = false;
        if(foundFloorToReorientTo)
        {
            internalAngularVelocity = Vector3.zero;
            foundFloorToReorientTo = false;
            reorient = true;
        }
        
        if(motor.IsGroundedThisUpdate)
        {
            // If player is attached to ground orientation
            if(motor.BaseVelocity.sqrMagnitude >= overridableAttribute.values.attachThreshold * overridableAttribute.values.attachThreshold)
                currentRotation = Quaternion.FromToRotation(motor.CharacterUp, motor.GetEffectiveGroundNormal()) * currentRotation;
            else
            {
                currentRotation = Quaternion.FromToRotation(motor.CharacterUp, -physics.gravityDirection) * currentRotation;
            }
        }
        // If the player does not have angular momentum midair
        // ? Can this be moved to be an if/else with the directly negated if statement a few lines below?
        else if(internalAngularVelocity == Vector3.zero)
        {
            if(reorient)
                // Instantly set orientation to the floor if there is a ground to reorient onto 
                currentRotation = Quaternion.FromToRotation(motor.CharacterUp, -physics.gravityDirection) * currentRotation;
            else
            {
                // Smoothly reorient player along gravity 
                float slerpFactor = 1;////3;//0;
                Vector3 smoothedUp;
                if(Vector3.Dot(motor.CharacterUp, physics.gravityDirection) > 0.95f)
                    smoothedUp = Vector3.Slerp((motor.CharacterUp - motor.CharacterRight * 0.05f).normalized, -physics.gravityDirection, 1 - Mathf.Exp(-slerpFactor * deltaTime));
                else
                    smoothedUp = Vector3.Slerp(motor.CharacterUp, -physics.gravityDirection, 1 - Mathf.Exp(-slerpFactor * deltaTime));
                    
                currentRotation = Quaternion.FromToRotation(motor.CharacterUp, smoothedUp) * currentRotation;
            }
        }

        // Handle Action related rotation updates, if active
        action.UpdateRotation(ref currentRotation, ref internalAngularVelocity, motor, deltaTime);
        
        // Handle Ability related rotation updates
        ability.UpdateRotation(ref currentRotation, ref internalAngularVelocity, motor, deltaTime);
        
        // Handle Physics related rotation updates, if active
        physics.UpdateRotation(ref currentRotation, ref internalAngularVelocity, motor, deltaTime);

        // Handle angular momentum 
        if(internalAngularVelocity != Vector3.zero)
        {
            // Reorient angluar momentum along plane
            if (setOrientationAlongPlane) 
                internalAngularVelocity = Quaternion.FromToRotation(internalAngularVelocity, motor.PlanarConstraintAxis) * internalAngularVelocity;

            // Add angular velocity to the rotation
            currentRotation = Quaternion.Euler(internalAngularVelocity * deltaTime) * currentRotation;
        }

        // If rotated along the ground, rotate along the bottom sphere of the capsule instead of it's center so that the player doesn't rotate off the point on the ground they are currently on 
        if(motor.IsGroundedThisUpdate)
        {
            Vector3 newCharacterBottomHemiCenter = motor.TransientPosition + (currentRotation * motor.CharacterTransformToCapsuleBottomHemi);

            motor.SetTransientPosition((initialCharacterBottomHemiCenter - newCharacterBottomHemiCenter) + motor.TransientPosition);
        }

        setOrientationAlongPlane = false;
    }
    
    /// <summary>
    /// Sets the velocity of the kinematic motor
    /// ? Is this being done efficiently with how it uses Vector and Quaternion math?
    /// ? Is max move even necessary?
    /// </summary>
    /// <param name="currentVelocity"> Reference to the motor's velocity </param>
    /// <param name="maxMove"> The max distance the player can move this update</param>
    /// <param name="motor"> The player's kinematic motor</param>
    /// <param name="deltaTime"> Motor update time </param>
    public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
    {

        if (zeroingVel)
        {
            currentVelocity = Vector3.zero;
            zeroingVel = false;
        }

        // Handle velocity projection on a surface if grounded
        if (motor.IsGroundedThisUpdate)
        {
            ////// The dot between the ground normal and the external velocity addition
            ////float dot = Vector3.Dot(externalVelocity, motor.GetEffectiveGroundNormal());
            ////// The velocity off the ground
            ////Vector3 projection = dot * motor.GetEffectiveGroundNormal();
            ////// If external velocity off ground is strong enough
            ////if(dot > 0  && projection.sqrMagnitude >= overridableAttribute.values.pushOffGroundThreshold * overridableAttribute.values.////pushOffGroundThreshold)
            ////{
            ////    motor.ForceUnground();
            ////}
            ////else
            ////{
                // if just landed or slope hasn't changed
                if (!motor.WasGroundedLastUpdate || motor.GetEffectiveGroundNormal() == motor.GetLastEffectiveGroundNormal())
                    // Project velocity onto ground
                    currentVelocity = Vector3.ProjectOnPlane(currentVelocity, motor.GetEffectiveGroundNormal());
                else
                    // Reorient without losing momentum
                    currentVelocity = Vector3.ProjectOnPlane(currentVelocity, motor.GetEffectiveGroundNormal()).normalized * currentVelocity.magnitude;

                // Snap external velocity to ground
                externalVelocity = Vector3.ProjectOnPlane(externalVelocity, motor.GetEffectiveGroundNormal()); ////-= projection;
            ////}
        }

        // Add external velocity and reset back to zero
        currentVelocity += externalVelocity;
        externalVelocity = Vector3.zero;

        if(kinematicPath == null)
        {
            action.UpdateVelocity(ref currentVelocity, motor, physics.gravityDirection, wallHits, ref physics.negations, groundingActionBuffer, bufferedUngroundedNormal, deltaTime);
            ability.UpdateVelocity(ref currentVelocity, motor, physics.gravityDirection, ref physics.negations, deltaTime);
            physics.UpdateVelocity (ref currentVelocity, motor, deltaTime);
        }

        // Ensure velocity is locked to current 2.5D plane
        currentVelocity = Vector3.ProjectOnPlane(currentVelocity, motor.PlanarConstraintAxis);
        
        // Cap the velocity if over max speed
        if (currentVelocity.sqrMagnitude > overridableAttribute.values.maxSpeed * overridableAttribute.values.maxSpeed)
            currentVelocity = currentVelocity.normalized * overridableAttribute.values.maxSpeed;

        // Handle ground stability calculation changes   
        if (motor.IsGroundedThisUpdate)
        {
            //Scale according to velocity so that it's less likely to fly off the ground due to innacuracies cause by higher speeds
            motor.GroundDetectionExtraDistance = currentVelocity.magnitude * extraGroundProbingDistanceFactor;
            currentMaxStableSlopeAngle = motor.MaxStableSlopeAngle + (motor.MaxStableSlopeAngle * motor.BaseVelocity.magnitude * extraMaxStableSlopeFactor);
        }
        else 
        {
            // No need for extra stability measures since there's no ground to try to stay grounded to
            motor.GroundDetectionExtraDistance = 0;
            currentMaxStableSlopeAngle = motor.MaxStableSlopeAngle;
        }
        motor.MaxStableDenivelationAngle = currentMaxStableSlopeAngle;

        // Reset wall hit info since current info is no longer in use
        wallHits.hitCeiling = Vector3.zero;
        wallHits.hitLeftWall = Vector3.zero;
        wallHits.hitRightWall = Vector3.zero;
    }

    /// <summary>
    /// Sets up the class and motor info for the upcoming motor update
    /// </summary>
    /// <param name="motor"> The player's kinematic motor</param>
    /// <param name="deltaTime"> Motor update time </param>
    public void BeforeCharacterUpdate(float deltaTime)
    {
        if (forceUngrounding)
        {
            motor.ForceUnground();
            forceUngrounding = false;
        }

        // Handle extra Ability update setup
        ability.BeforeCharacterUpdate(motor, deltaTime);
    }

    /// <summary>
    /// Handles grounding info after probing and grounding/ungrounding
    /// Primarily used to handle the slope tracking for the ungrounding angular momentum mechanic
    /// </summary>
    /// <param name="motor"> The player's kinematic motor</param>
    /// <param name="deltaTime"> Motor update time </param>
    public void PostGroundingUpdate(float deltaTime)
    {
        // Keep tracking time on current slope
        slopeList.IncrementTimer(deltaTime, overridableAttribute.values.maxSlopeTrackTime);
        
        // Used to see if angular velocity should be set for angular momentum when ungrounding
        bool setAngularVelocity = false;

        if (motor.IsGroundedThisUpdate)
        {
            // If just landed
            if (motor.WasGroundedLastUpdate)
            {
                internalAngularVelocity = Vector3.zero;
            }
            // If speed drops enough, detatch from slope
            // TODO: Simplify attach threshold so that a bool is set here so that "isAttached" doesn't have to be calculated every time it's needed in other parts of the class
            if(Vector3.ProjectOnPlane(motor.BaseVelocity, motor.GetEffectiveGroundNormal()).sqrMagnitude < overridableAttribute.values.attachThreshold * overridableAttribute.values.attachThreshold && Vector3.Angle(-physics.gravityDirection, motor.GetEffectiveGroundNormal()) > motor.MaxStableSlopeAngle)
            {
                motor.ForceUnground();
                setAngularVelocity = true;
            }
            // If slope changed
            else if(motor.WasGroundedLastUpdate && motor.GetEffectiveGroundNormal() != motor.GetLastEffectiveGroundNormal())
            {
                // Start tracking new slope
                slopeList.Add(motor.GetLastEffectiveGroundNormal());
            }
        }
        // If just ungrounded
        else if(motor.WasGroundedLastUpdate)
        {
            // Log new slope that was just ungrounded from
            slopeList.Add(motor.GetLastEffectiveGroundNormal());
            setAngularVelocity = true;

            // Handle ungrounded buffering
            if(!motor.MustUnground())
                StartUngroundedBuffering(motor.GetLastEffectiveGroundNormal());
        }

        // If ungrounding angular momentum mechanic was triggered
        if(setAngularVelocity)
        {
            // Set angular velocity (if any) using slope tracking info and reset the tracker
            internalAngularVelocity = slopeList.GetAngularVelocity(motor.BaseVelocity, motor.PlanarConstraintAxis, overridableAttribute.values.ungroundRotationFactor, overridableAttribute.values.ungroundRotationMinSpeed, overridableAttribute.values.ungroundRotationMaxSpeed);
            slopeList.Reset();
        }

        // Handle extra grounding update based Ability mechanics/info
        ability.PostGroundingUpdate(motor, deltaTime);
    }

    /// <summary>
    /// Handles post motor update actions such as resetting things for the next update
    /// </summary>
    /// <param name="motor"> The player's kinematic motor</param>
    /// <param name="deltaTime"> Motor update time </param>
    public void AfterCharacterUpdate(float deltaTime)
    {
        ability.AfterCharacterUpdate(motor, deltaTime);

        // Reset Ability and Action Input
        ability.controlInterface.Reset();
        action.control.Reset();
    }

    /// <summary>
    /// Lets the motor know if a collision should be ignored or not, used primarily but Ability for potential collision overriding mechanics
    /// </summary>
    /// <param name="motor"> The player's kinematic motor</param>
    /// <param name="coll"> The collider being checked </param>
    public bool IsColliderValidForCollisions(Collider coll)
    {
        // See if an Ability mechanic overrides a collision
        return ability.IsColliderValidForCollisions(motor, coll);
    }

    /// <summary>
    /// Used to handle (or even modify) ground info on probing
    /// Happens before grounding update so that modifications here are taken into account
    /// </summary>
    /// <param name="motor"> The player's kinematic motor</param>
    /// <param name="hitCollider">The ground collider </param>
    /// <param name="hitNormal"> The ground normal </param>
    /// <param name="hitPoint"> The ground point </param>
    /// <param name="hitStabilityReport"> The ground stability </param>
    public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
    {
        // Handles extra Ability handling and modififcation of grounding hit info 
        ability.OnGroundHit(motor, hitCollider, hitNormal, hitPoint, ref hitStabilityReport);
    }

    /// <summary>
    /// Used to handle (or even modify) hit info on sweeping 
    /// Happens before hit info is handled by the motor so that modifications here are taken into account
    /// </summary>
    /// <param name="motor"> The player's kinematic motor</param>
    /// <param name="hitCollider"> The hit collider </param>
    /// <param name="hitNormal"> The hit normal </param>
    /// <param name="hitPoint"> The hit point </param>
    /// <param name="hitStabilityReport"> The hit stability </param>
    public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
    {   
        // If floor (in line with gravity) is hit when mid air, but the player isn't oriented properly to land normally 
        if (!motor.IsGroundedThisUpdate
        && motor.StableGroundLayers.value == (motor.StableGroundLayers.value | (1 << hitCollider.gameObject.layer))
        && Vector3.Angle(-physics.gravityDirection, hitNormal) <= motor.MaxStableSlopeAngle)
        { 
            foundFloorToReorientTo = true;
        }
        // If hit is not valid ground for the player to land on
        else if (!hitStabilityReport.IsStable)
        {
            if (Vector3.Dot(physics.gravityDirection, hitNormal) >= 0.5)
                wallHits.hitCeiling = hitNormal;
            else if (Vector3.Dot(motor.CharacterRight, hitNormal) < 0)
                wallHits.hitRightWall = hitNormal;
            else
                wallHits.hitLeftWall = hitNormal;
        }

        // Handle extra movement hit handling/modification based on Ability
        ability.OnMovementHit(motor, hitCollider, hitNormal, hitPoint, ref hitStabilityReport);
    }

    /// <summary>
    /// Detailed handling and modifying of HitStabilityReport to check if ground is valid or not
    /// Used primarily for ledge handling logic
    /// </summary>
    /// <param name="motor"> The player's kinematic motor </param>
    /// <param name="hitCollider"> The hit collider </param>
    /// <param name="hitNormal"> The hit normal </param>
    /// <param name="hitPoint"> The hit point </param>
    /// <param name="hitPoint"></param>
    /// <param name="atCharacterPosition"> The character position on hit </param>
    /// <param name="atCharacterRotation"> The character rotation on hit </param>
    /// <param name="hitStabilityReport"> The hit stability </param>
    public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport)
    {
        // Handles extra slope angle measurements to ensure valid ground slope difference is detected on faster velocities
        if (motor.IsGroundedThisUpdate && !hitStabilityReport.IsStable && Vector3.Angle(motor.GetEffectiveGroundNormal(), hitNormal) < currentMaxStableSlopeAngle)
        {
            hitStabilityReport.IsStable = true;
        }

        // Handles potentially innaccurate ledge detection possibly due to issues with concave mesh colliders
        // TODO: This is a hacky mess of a bug fix. PLEASE fix this. Commented out code is kept for later debugging
        float dotEpsilon = 0.998f;////99998f;
        float ledgeHitDot;
        float groundHitDot;
        if (hitStabilityReport.IsStable)////Vector3.Dot(hitStabilityReport.OuterNormal, hitNormal) < 1)
        {
            // Is ledge that was falsely detected
            if (hitStabilityReport.LedgeDetected || Vector3.Angle(hitStabilityReport.OuterNormal, hitStabilityReport.InnerNormal) > motor.MaxStableSlopeAngle)////(Vector3.Dot(hitStabilityReport.InnerNormal, hitNormal) == 1 || Vector3.Dot(hitStabilityReport.InnerNormal, hitStabilityReport.OuterNormal) == 1 || Vector3.Angle(hitStabilityReport.OuterNormal, hitStabilityReport.InnerNormal) > motor.MaxStableSlopeAngle)
            {
                hitStabilityReport.IsStable = false;
            }
            // Double check that this ledge isn't actually a ledge
            else if (hitStabilityReport.InnerNormal == hitStabilityReport.OuterNormal && (groundHitDot = Vector3.Dot(motor.GroundingStatus.GroundNormal, hitNormal)) < dotEpsilon && (ledgeHitDot = Vector3.Dot(hitStabilityReport.OuterNormal, hitNormal)) < dotEpsilon && ledgeHitDot != groundHitDot)
            {
                ////Debug.DrawRay(hitPoint, hitStabilityReport.OuterNormal*5, Color.magenta, 5);
                ////Debug.DrawRay(hitPoint, hitStabilityReport.InnerNormal*4, Color.cyan, 5);
                ////Debug.DrawRay(hitPoint, hitNormal*3, Color.blue, 5);
                ////Debug.Break();
                ////Debug.LogWarning("Faulty Ledge Detection, setting to stable to prevent flying off. Debug Rays drawn for 5 seconds. Likely caused by Mesh Collider issue. Try Replacing area with primitive/concave collider if grounding should not be allowed on this hit.");
                ////Debug.LogWarning("Dot between Ledge and Hit normals: " + ledgeHitDot);
                ////Debug.LogWarning    ("Dot between Ground and Hit normals: " + groundHitDot);
                hitStabilityReport.IsStable = true;
            }
        }

        // Handles extra Ability mechanics that can modify or use hit stability info
        ability.ProcessHitStabilityReport(motor, hitCollider, hitNormal, hitPoint, atCharacterPosition, atCharacterRotation, ref hitStabilityReport);
    }

    /// <summary>
    /// Handles discrete collision detection (not detected by sweeping movement logic), if on
    /// </summary>
    /// <param name="motor"> The player's kinematic motor</param>
    /// <param name="hitCollider"> The detected collider </param>
    public void OnDiscreteCollisionDetected(Collider hitCollider)
    {
        // Extra discrete collision handling by Ability
        ability.OnDiscreteCollisionDetected(motor, hitCollider);
    }

#endregion


#region Buffer Handling

    /// <summary>
    /// Starts ungrounded buffering by storing buffered info and beginning a timer for the info to be deleted
    /// </summary>
    /// <param name="ungroundedNormal"> The last ground normal before ungrounding</param>
    private void StartUngroundedBuffering(Vector3 ungroundedNormal)
    {
        bufferedUngroundedNormal = ungroundedNormal;
        GameManager.Instance.TimerViaRealTime(groundingActionBuffer, EndUngroundedBuffering);
    }

    /// <summary>
    /// Ends ungrounded bufferiong by deleting all buffered info
    /// </summary>
    private void EndUngroundedBuffering()
    {
        bufferedUngroundedNormal = Vector3.zero;
    }
    #endregion

#region IDynamicPlaneConstrainable Implementation
    public override void HandlePlaneSettingExtra(Plane plane)
    {
        // Lock player movement on plane
        motor.PlanarConstraintAxis = plane.normal;
        // Reorient player rotation
        setOrientationAlongPlane = true;
    }

    public override bool PlaneBreakValid()
    {
        return motor.IsGroundedThisUpdate && motor.BaseVelocity.sqrMagnitude >= overridableAttribute.values.attachThreshold * overridableAttribute.values.attachThreshold;
    }
#endregion

    #region debug        
    /// <summary>
    /// Currently used for debugging inputs
    /// </summary>
    public void ResetState()
    {
        // Resets the the motor state (used as a makeshift "level restart")
        GetComponent<DynamicPlaneConstraint>()?.SetCurrentPlane(startPlane);
        motor.ApplyState(startState);
        motor.BaseVelocity = Vector3.zero;
    }
    #endregion

}
