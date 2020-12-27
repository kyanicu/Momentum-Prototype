using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KinematicCharacterController;
using PathCreation;

/// <summary>
/// Interface for input relating to PlayerMovement
/// TODO: Should be deprecated or restructured once input is handled by actions/events
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

/// <summary>
/// The overrideable values for PlayerMovement 
/// </summary>
[System.Serializable]
public class PlayerMovementValues : PlayerOverridableValues
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
    /// <summary>
    /// The acceleration off the current slope needed to force unground the character
    /// </summary>
    [SerializeField]
    public float pushOffGroundThreshold;
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

    /// <summary>
    /// Is the action component negated?
    /// Uses int as a count of true bools as bool is not supported in overridable values
    /// Use negateAction > 0 == true
    /// </summary>
    public int negateAction;
    /// <summary>
    /// Is the physics component negated?
    /// Uses int as a count of true bools as bool is not supported in overridable values
    /// Use negatePhysics > 0 == true
    public int negatePhysics;

    /// <summary>
    /// Overridden to set the counts for the overridable values
    /// </summary>
    protected override void SetValueCounts()
    {
        floatValuesCount = 7;
        intValuesCount = 2;
        vector3ValuesCount = 0;
    }

    /// <summary>
    /// Overridden to return the appropriate float value
    /// </summary>
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
    /// <summary>
    /// Overridden to set the appropriate float value
    /// </summary>
    /// <param name="i"> The float value index </param>
    /// <param name="value"> The value to be set to </param>
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
    /// <summary>
    /// Overridden to return the appropriate int value
    /// </summary>
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
    /// <summary>
    /// Overridden to set the appropriate int value
    /// </summary>
    /// <param name="i"> The int value index </param>
    /// <param name="value"> The value to be set to </param>
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
    /// <summary>
    /// Overridden to return the appropriate Vector3 value
    /// </summary>
    protected override Vector3 GetVector3Value(int i)
    {
        return Vector3.zero;
    }
    /// <summary>
    /// Overridden to set the appropriate Vector3 value
    /// </summary>
    /// <param name="i"> The Vector3 value index </param>
    /// <param name="value"> The value to be set to </param>
    protected override void SetVector3Value(int i, Vector3 value) {}

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
[System.Serializable]
public class PlayerMovement : PlayerOverridableAttribute<PlayerMovementValues>, ICharacterController, IPlayerMovementCommunication
{

#region MovementEvents
    /// <summary>
    /// Triggered on Plane Change
    /// </summary>
    public event Action<PlaneChangeArgs> planeChanged;
#endregion

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

#region Helper/Extension Components
    /// <summary>
    /// The component for handling player physics that occur as a result of environment
    /// Adds acceleration to velocity each physics tick
    /// </summary>
    [SerializeField]
    private PlayerMovementPhysics physics;
    /// <summary>
    /// The component for handling player actions directly related to movement as a result of player input
    /// Adds acceleration to velocity each physics tick
    /// </summary>
    [SerializeField]
    private PlayerMovementAction action;
    /// <summary>
    /// The component for handling special player actions that can have a more direct effect on the player's Movement itself, allowing for more than just adding acceleration each physics tick
    /// </summary>
    [SerializeField]
    private IPlayerMovementAbility ability;
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

#region Dynamic Plane Info
    /// <summary>
    /// The current plane that the player is on
    /// In regards to the 2.5D Dynamic Plane Shifting mechanic
    /// </summary>
    private Plane currentPlane;
    /// <summary>
    /// The plane a player broke off of when on a plane breaker
    /// Tracked so that if the player leaves a plane breaker without being in a dynamic plane, it can reorient itself back to this as it's last stable plane
    /// </summary>
    private Plane brokenPlane;
    /// <summary>
    /// The current dynamic plane the player is on
    /// Holds up to 2 incase of overlap
    /// </summary>
    private DynamicPlane[] currentDynamicPlanes = new DynamicPlane[2];
    /// <summary>
    /// The current Plane breaker the player is on
    /// Holds up to 2 incase of overlap 
    /// </summary>
    private PlaneBreaker[] currentPlaneBreakers = new PlaneBreaker[2];
    /// <summary>
    /// Current stored plane waiting to be set at the appropriate time in the update cycle
    /// </summary>
    public Plane settingPlane = new Plane(Vector3.zero, Vector3.zero);
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

#region ClassSetup
    /// <summary>
    /// Default constructor
    /// </summary>
    private PlayerMovement()
    {
        // Instantiate helper components
        physics = new PlayerMovementPhysics();
        action = new PlayerMovementAction();
    } 

    /// <summary>
    /// Constructor used to set the appropriate PlayerMovementAbility
    /// Calls default constructor first
    /// </summary>
    /// <param name="_ability"> The PlayerMovementAbility to be used</param>
    /// <returns></returns>
    public PlayerMovement(IPlayerMovementAbility _ability) : this()
    {
        ability = _ability;

        // Set Ability event handlers
        ability.addingMovementOverrides += AddAbilityOverride;
        ability.removingMovementOverrides += RemoveAbilityOverride;
    }
    
    /// <summary>
    /// Sets the default base overridable values
    /// </summary>
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

    /// <summary>
    /// Validates the base default values as valid
    /// </summary>
    protected override void ValidateBaseValues()
    {
        action.OnValidate();
        physics.OnValidate();
        ////ability.OnValidate();
    }

    /// <summary>
    /// Initializes communication with the Player's internal communcator
    /// </summary>
    /// <param name="communicator"> The internal commmunicator </param>
    public void SetCommunicationInterface(PlayerInternalCommunicator communicator)
    {
        // Set the communication
        communicator.SetCommunication(this);

        // Set the helper classes' communications
        action.SetCommunicationInterface(communicator);
        ability.SetCommunicationInterface(communicator);
    }

    /// <summary>
    /// Setup the class to be ready for gameplay
    /// Also initializes state info for debugging
    /// </summary>
    public void InitializeForPlay(KinematicCharacterMotor motor)
    {
        slopeList = new SlopeList(5);

        SetCurrentPlane(motor, new Plane(motor.CharacterForward, motor.Transform.position));
        currentPlane = new Plane(motor.CharacterForward, motor.transform.position);
        motor.PlanarConstraintAxis = currentPlane.normal;
        
        // Debug
        #region debug
        startState = motor.GetState();
        startPlane = currentPlane;
        #endregion
    }
    #endregion

    /// <summary>
    /// Handles Input when valid
    /// TODO: Remove when refactored to be handled by events 
    /// </summary>
    public void HandleInput(PlayerController.PlayerActions controllerActions)
    {
        // Make sure Action can take input
        if(values.negateAction == 0)
            action.RegisterInput(controllerActions);
        ability.RegisterInput(controllerActions);
    }

#region PlayerCharacter's MonoBehavior Messages Handling
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
                startPlane = new Plane(currentPlane.normal, motor.Transform.position);
                break;
        }
    } 

    /// <summary>
    /// Appropriately handle exiting a trigger
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
    #endregion

#region CharacterControllerInterface

    /// <summary>
    /// Sets the rotation of the kinematic motor
    /// ? Is this being done efficiently with how it uses Vector and Quaternion math?
    /// </summary>
    /// <param name="currentRotation"> Reference to the motor's rotation </param>
    /// <param name="motor"> The player's kinematic motor</param>
    /// <param name="deltaTime"> Motor update time </param>
    public void UpdateRotation(ref Quaternion currentRotation, KinematicCharacterMotor motor, float deltaTime)
    {
        //Hold the previous rotation info
        Quaternion prevRot = currentRotation;
        Vector3 initialCharacterBottomHemiCenter = motor.TransientPosition + (currentRotation * motor.CharacterTransformToCapsuleBottomHemi); 
        
        // Handles reorienting the player onto the proper plane if not already
        if (setOrientationAlongPlane)
            // Set forward to match the current plane
            currentRotation = Quaternion.FromToRotation(motor.CharacterForward, currentPlane.normal) * currentRotation;

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
            if(motor.BaseVelocity.sqrMagnitude >= values.attachThreshold * values.attachThreshold)
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
        if(values.negateAction == 0)
            action.UpdateRotation(ref currentRotation, ref internalAngularVelocity, motor, deltaTime);
        
        // Handle Ability related rotation updates
        ability.UpdateRotation(ref currentRotation, ref internalAngularVelocity, motor, deltaTime);
        
        // Handle Physics related rotation updates, if active
        if(values.negatePhysics == 0)
            physics.UpdateRotation(ref currentRotation, ref internalAngularVelocity, motor, deltaTime);

        // Handle angular momentum 
        if(internalAngularVelocity != Vector3.zero)
        {
            // Reorient angluar momentum along plane
            if (setOrientationAlongPlane) 
                internalAngularVelocity = Quaternion.FromToRotation(internalAngularVelocity, currentPlane.normal) * internalAngularVelocity;

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
    public void UpdateVelocity(ref Vector3 currentVelocity, ref float maxMove, KinematicCharacterMotor motor, float deltaTime)
    {
        // Handle velocity projection on a surface if grounded
        if (motor.IsGroundedThisUpdate)
        {
            // The dot between the ground normal and the external velocity addition
            float dot = Vector3.Dot(externalVelocity, motor.GetEffectiveGroundNormal());
            // The velocity off the ground
            Vector3 projection = dot * motor.GetEffectiveGroundNormal();
            // If external velocity off ground is strong enough
            if(dot > 0  && projection.sqrMagnitude >= values.pushOffGroundThreshold * values.pushOffGroundThreshold)
            {
                motor.ForceUnground();
            }
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

        // Ensure velocity is locked to current 2.5D plane
        currentVelocity = Vector3.ProjectOnPlane(currentVelocity, currentPlane.normal);
        
        // Cap the velocity if over max speed
        if (currentVelocity.sqrMagnitude > values.maxSpeed * values.maxSpeed)
            currentVelocity = currentVelocity.normalized * values.maxSpeed;

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
    public void BeforeCharacterUpdate(KinematicCharacterMotor motor, float deltaTime)
    {
        // Handle setting the current plane 
        // Set so that the update uses the proper plane that the player is currently on

        // If active on PlaneBreaker
        if (currentPlaneBreakers[0] != null && motor.IsGroundedThisUpdate && motor.BaseVelocity.sqrMagnitude >= values.attachThreshold * values.attachThreshold)
        {
            Vector3 planeRight = Vector3.Cross(currentPlaneBreakers[0].transform.up, currentPlane.normal);
            bool movingRight = Vector3.Dot(motor.BaseVelocity, planeRight) > 0;            
            
            // Update current plane to match the player's position on the PlaneBreaker
            SetCurrentPlane(motor, currentPlaneBreakers[0].GetClosestPathPlane(motor.Transform.position, movingRight), true);
        }
        // If on Dynamic Plane
        else if(currentDynamicPlanes[0] != null)
        {
            Vector3 planeRight = Vector3.Cross(currentDynamicPlanes[0].transform.up, currentPlane.normal);
            bool movingRight = Vector3.Dot(motor.BaseVelocity, planeRight) > 0;

            // Update current plane to match the player's position on the DynamicPlane
            SetCurrentPlane(motor, currentDynamicPlanes[0].GetClosestPathPlane(motor.Transform.position, movingRight), false);
        }
        // Set stored plane, if waiting to be set
        else if (settingPlane.normal != Vector3.zero)
            SetCurrentPlane(motor, settingPlane);

        // Reset stored plane
        settingPlane = new Plane(Vector3.zero, Vector3.zero);

        // Handle extra Ability update setup
        ability.BeforeCharacterUpdate(motor, deltaTime);
    }

    /// <summary>
    /// Handles grounding info after probing and grounding/ungrounding
    /// Primarily used to handle the slope tracking for the ungrounding angular momentum mechanic
    /// </summary>
    /// <param name="motor"> The player's kinematic motor</param>
    /// <param name="deltaTime"> Motor update time </param>
    public void PostGroundingUpdate(KinematicCharacterMotor motor, float deltaTime)
    {
        // Keep tracking time on current slope
        slopeList.IncrementTimer(deltaTime, values.maxSlopeTrackTime);
        
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
            if(Vector3.ProjectOnPlane(motor.BaseVelocity, motor.GetEffectiveGroundNormal()).sqrMagnitude < values.attachThreshold * values.attachThreshold && Vector3.Angle(-physics.gravityDirection, motor.GetEffectiveGroundNormal()) > motor.MaxStableSlopeAngle)
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
            internalAngularVelocity = slopeList.GetAngularVelocity(motor.BaseVelocity, currentPlane.normal, values.ungroundRotationFactor, values.ungroundRotationMinSpeed, values.ungroundRotationMaxSpeed);
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
    public void AfterCharacterUpdate(KinematicCharacterMotor motor, float deltaTime)
    {
        ability.AfterCharacterUpdate(motor, deltaTime);

        // Reset Ability and Action Input
        ability.ResetInput();
        action.ResetInput();
    }

    /// <summary>
    /// Lets the motor know if a collision should be ignored or not, used primarily but Ability for potential collision overriding mechanics
    /// </summary>
    /// <param name="motor"> The player's kinematic motor</param>
    /// <param name="coll"> The collider being checked </param>
    public bool IsColliderValidForCollisions(KinematicCharacterMotor motor, Collider coll)
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
    public void OnGroundHit(KinematicCharacterMotor motor, Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
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
    public void OnMovementHit(KinematicCharacterMotor motor, Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
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
    public void ProcessHitStabilityReport(KinematicCharacterMotor motor, Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport)
    {
        // Handles extra slope angle measurements to ensure valid ground slope difference is detected on faster velocities
        if (motor.IsGroundedThisUpdate && !hitStabilityReport.IsStable && Vector3.Angle(motor.GetEffectiveGroundNormal(), hitNormal) < currentMaxStableSlopeAngle)
        {
            hitStabilityReport.IsStable = true;
        }

        // Handles potentially innaccurate ledge detection possibly due to issues with concave mesh colliders
        // TODO: Jesus Fucking Christ this is a mess PLEASE fix this or (better yet) just DON'T use concave mesh colliders. Commented out code is kept for later debugging
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

        // Handles extra Ability mechanics thaqt can modify or use hit stability info
        ability.ProcessHitStabilityReport(motor, hitCollider, hitNormal, hitPoint, atCharacterPosition, atCharacterRotation, ref hitStabilityReport);
    }

    /// <summary>
    /// Handles discrete collision detection (not detected by sweeping movement logic), if on
    /// </summary>
    /// <param name="motor"> The player's kinematic motor</param>
    /// <param name="hitCollider"> The detected collider </param>
    public void OnDiscreteCollisionDetected(KinematicCharacterMotor motor, Collider hitCollider)
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

#region Dynamic Plane Shifting
    /// <summary>
    /// Sets the current plane and fixes the player and their movement on that plane
    /// </summary>
    /// <param name="motor"> The player's kinematic motor</param>
    /// <param name="plane"> The new plane to be set</param>
    /// <param name="breaker"> Is the plane change a result of a PlaneBreaker </param>
    private void SetCurrentPlane(KinematicCharacterMotor motor, Plane plane, bool breaker = false)
    {
        //If the plane is invalid or identical to the current plane
        if (plane.normal == Vector3.zero || (plane.normal == currentPlane.normal && plane.distance == currentPlane.distance))
            return; 

        // Set the currentPlane
        currentPlane = plane;
        // Rotate Velocity along new plane without losing momentum
        motor.BaseVelocity = Quaternion.FromToRotation(currentPlane.normal, plane.normal) * motor.BaseVelocity;
        // Lock player movement on plane
        motor.PlanarConstraintAxis = plane.normal;
        // Move player to plane
        motor.SetPosition(plane.ClosestPointOnPlane(motor.Transform.position));
        // Reorient player rotation
        setOrientationAlongPlane = true;

        // Trigger communication event on plane change
        planeChanged?.Invoke(new PlaneChangeArgs(plane.normal, breaker));
    }

    /// <summary>
    /// Begin tracking newly entered DynamicPlane
    /// </summary>
    /// <param name="motor"> The player's kinematic motor</param>
    /// <param name="dynamicPlane"> The DynamicPlane entered</param>
    private void EnterDynamicPlane(KinematicCharacterMotor motor, DynamicPlane dynamicPlane)
    {
        // If DynamicPlane overlaps with another the player is already in
        if (currentDynamicPlanes[0] != null)
        {
            // If the previous DynamicPlane is prioritized, keep new DynamicPlane tracked secondarily
            if (currentDynamicPlanes[0].prioritize)
                currentDynamicPlanes[1] = dynamicPlane;
            else
            {
                // Track previous DynamicPlane secondarily, and new one primarily, forgetting any other tracked DynamicPlane
                currentDynamicPlanes[1] = currentDynamicPlanes[0];
                currentDynamicPlanes[0] = dynamicPlane;
            }
        }
        else
            // Track new DynamicPlane primarily
            currentDynamicPlanes[0] = dynamicPlane;
    }

    /// <summary>
    /// End tracking previously entered DynamicPlane 
    /// </summary>
    /// <param name="col"> The collider of the DynamicPlane trigger exited</param>
    private void ExitDynamicPlane(Collider col)
    {
        // If exited DynamicPlane was tracked primarily
        if (currentDynamicPlanes[0].gameObject == col.gameObject)
        {
            // Shift over potential secondarily tracked DynamicPlane so that it is now tracked primarily
            // If there was no DynamicPlane, then there is no longer any tracked DynamicPlane
            currentDynamicPlanes[0] = currentDynamicPlanes[1];
            // Stop tracking secondary DynamicPlane
            currentDynamicPlanes[1] = null;
        }
        // If exited DynamicPlane was only tracked secondarily  
        else if (currentDynamicPlanes[1].gameObject == col.gameObject)
        {
            // Stop tracking it
            currentDynamicPlanes[1] = null;
        }
    }

    /// <summary>
    /// Begin tracking newly entered PlaneBreaker
    /// </summary>
    /// <param name="motor"> The player's kinematic motor</param>
    /// <param name="planeBreaker">The PlaneBreaker entered</param>
    private void EnterPlaneBreaker(KinematicCharacterMotor motor, PlaneBreaker planeBreaker)
    {
        // If PlaneBreaker overlaps with another the player is already in
        if (currentPlaneBreakers[0] != null)
        {
            // If the previous PlaneBreaker is prioritized, keep new PlaneBreaker tracked secondarily
            if (currentPlaneBreakers[0].prioritize)
                currentPlaneBreakers[1] = planeBreaker;
            else
            {
                // Track previous PlaneBreaker secondarily, and new one primarily, forgetting any other tracked PlaneBreaker
                currentPlaneBreakers[1] = currentPlaneBreakers[0];
                currentPlaneBreakers[0] = planeBreaker;

                // Remember the plane that was broken
                brokenPlane = currentPlane;
            }
        }
        else
        {
            // Track new PlaneBreaker primarily
            currentPlaneBreakers[0] = planeBreaker;

            // Remember the plane that was broken
            brokenPlane = currentPlane;
        }
    }

    /// <summary>
    /// End tracking previously entered PlaneBreaker 
    /// </summary>
    /// <param name="col"> The collider of the PlaneBreaker trigger exited</param>
    private void ExitPlaneBreaker(Collider col)
    {
        // If exited PlaneBreaker was tracked primarily
        if (currentPlaneBreakers[0].gameObject == col.gameObject)
        {
            // Shift over potential secondarily tracked PlaneBreaker so that it is now tracked primarily
            // If there was no PlaneBreaker, then there is no longer any tracked PlaneBreaker
            currentPlaneBreakers[0] = currentPlaneBreakers[1];
            // Stop tracking secondary PlaneBreaker
            currentPlaneBreakers[1] = null;

            // if There was no secondarily tracked PlaneBreaker
            if (currentPlaneBreakers[0] == null)
            {
                // Restore broken plane in the case that there is no current Dynamic Plane
                if (currentDynamicPlanes[0] == null)
                    settingPlane = brokenPlane; ////currentPlane = brokenPlane;
                
                // Stop Tracking the broken plane
                brokenPlane = new Plane(Vector3.zero, Vector3.zero);
            }
        }
        // If exited PlaneBreaker was only tracked secondarily  
        else if (currentPlaneBreakers[1].gameObject == col.gameObject)
        {
            // Stop tracking it
            currentPlaneBreakers[1] = null;
        }
    }
#endregion

#region Movement Effector Handling

    /// <summary>
    /// Apply any overrides from an entered MovementEffector
    /// </summary>
    /// <param name="effector"> The MovementEffector entered </param>
    private void EnterMovementEffector(MovementEffector effector)
    {
        // Apply base Movement value overrides
        for (int i = 0; i < effector.movementOverrides.Count; i++)
        {
            ApplyOverride(effector.movementOverrides[i].item1, effector.movementOverrides[i].item2);
        }

        // Apply Physics value overrides
        for (int i = 0; i < effector.physicsOverrides.Count; i++)
        {
            physics.ApplyOverride(effector.physicsOverrides[i].item1, effector.physicsOverrides[i].item2);
        }

        // Apply Action value overrides
        for (int i = 0; i < effector.actionOverrides.Count; i++)
        {
            action.ApplyOverride(effector.actionOverrides[i].item1, effector.actionOverrides[i].item2);
        }

        // Apply appropriate Ability value overrides 
        ability.EnterMovementEffector(effector);
    }

    /// <summary>
    /// Remove any overrides from an exited MovementEffector
    /// </summary>
    /// <param name="effector"> The MovementEffector exited </param>
    private void ExitMovementEffector(MovementEffector effector)
    {
        // Remove base Movement value overrides
        for (int i = 0; i < effector.movementOverrides.Count; i++)
        {
            RemoveOverride(effector.movementOverrides[i].item1, effector.movementOverrides[i].item2);
        }

        // Remove Physics value overrides
        for (int i = 0; i < effector.physicsOverrides.Count; i++)
        {
            physics.RemoveOverride(effector.physicsOverrides[i].item1, effector.physicsOverrides[i].item2);
        }

        // Remove Action value overrides
        for (int i = 0; i < effector.actionOverrides.Count; i++)
        {
            action.RemoveOverride(effector.actionOverrides[i].item1, effector.actionOverrides[i].item2);
        }

        // Remove appropriate Ability value overrides 
        ability.ExitMovementEffector(effector);
    }
#endregion

#region Ability Movement Override Handling
    /// <summary>
    /// Handle the event triggered by Ability wishing to apply an override
    /// ? Possibly find a way to combine this nicely with movement effector so there's less copy and paste?
    /// </summary>
    /// <param name="args"> The Abilty Overrides given </param>
    private void AddAbilityOverride(AbilityOverrideArgs args)
    {
        // Apply base Movement value overrides
        for (int i = 0; i < args.movementOverrides.Count; i++)
        {
            ApplyOverride(args.movementOverrides[i].item1, args.movementOverrides[i].item2);
        }

        // Apply Physics value overrides
        for (int i = 0; i < args.physicsOverrides.Count; i++)
        {
            physics.ApplyOverride(args.physicsOverrides[i].item1, args.physicsOverrides[i].item2);
        }

        // Apply Action value overrides
        for (int i = 0; i < args.actionOverrides.Count; i++)
        {
            action.ApplyOverride(args.actionOverrides[i].item1, args.actionOverrides[i].item2);
        }
    }

    /// <summary>
    /// Handle the event triggered by Ability wishing to remove an override
    /// ? Possibly find a way to combine this nicely with movement effector so there's less copy and paste?
    /// </summary>
    /// <param name="args"> The Abilty Overrides given </param>
    private void RemoveAbilityOverride(AbilityOverrideArgs args)
    {
         // Remove base Movement value overrides
        for (int i = 0; i < args.movementOverrides.Count; i++)
        {
            RemoveOverride(args.movementOverrides[i].item1, args.movementOverrides[i].item2);
        }

        // Remove Physics value overrides
        for (int i = 0; i < args.physicsOverrides.Count; i++)
        {
            physics.RemoveOverride(args.physicsOverrides[i].item1, args.physicsOverrides[i].item2);
        }

        // Remove Action value overrides
        for (int i = 0; i < args.actionOverrides.Count; i++)
        {
            action.RemoveOverride(args.actionOverrides[i].item1, args.actionOverrides[i].item2);
        }
    }
#endregion

    #region debug        
    /// <summary>
    /// Currently used for debugging inputs
    /// </summary>
    public void ResetState(KinematicCharacterMotor motor)
    {
        // Resets the the motor state (used as a makeshift "level restart")
        settingPlane = startPlane;
        motor.ApplyState(startState);
        motor.BaseVelocity = Vector3.zero;
    }
    #endregion

}
