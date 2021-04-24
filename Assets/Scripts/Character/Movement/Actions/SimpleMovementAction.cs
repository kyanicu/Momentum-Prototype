using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KinematicCharacterController;
using System;

[System.Serializable]
public class SimpleMovementActionValues : CharacterOverridableValues
{
    /// <summary>
    /// The acceleration of the player when actively running on the ground
    /// </summary>
    public float groundAccel;
    /// <summary>
    /// The deceleration of the player when actively running on the ground against their velocity
    /// </summary>
    public float groundDecel;
    /// <summary>
    /// The max speed a player can achieve by actively running on the ground alone
    /// </summary>
    public float groundMaxSpeed;
    /// <summary>
    /// The acceleration of the player when actively moving horizontally in the air over a minimum speed
    /// </summary>
    public float airAccel;
    /// <summary>
    /// The deceleration of the player when actively moving horizontally in the air over a minimum speed against their velocity
    /// </summary>
    public float airDecel;
    /// <summary>
    /// The max horizontal speed a player can achieve by actively moving in the air alone
    /// </summary>
    public float airMaxSpeed;
    /// <summary>
    /// The "up" speed at which the player jumps off the ground
    /// </summary>
    public float jumpSpeed;
    /// <summary>
    /// The "up" speed at which the player jumps off the ground
    /// </summary>
    public float dynamicDashSpeed;
    /// <summary>
    /// The "up" speed at which the player jumps off the ground
    /// </summary>
    public float kinematicDashSpeed;
    /// <summary>
    /// The "up" speed at which the player jumps off the ground
    /// </summary>
    public float kinematicDashTime;
    /// <summary>
    /// The "up" speed at which the player jumps off the ground
    /// </summary>
    public float rawAccel;
    /// <summary>
    /// The "up" speed at which the player jumps off the ground
    /// </summary>
    public float rawMaxSpeed;

    /// <summary>
    /// Should right be inverted for player actions?
    /// Uses int as a count of true bools as bool is not supported in overridable values
    /// Use invertRight > 0 == true
    /// </summary>
    public int invertRight;

    protected override float[] floatValues
    {
        get
        {
            return new float[]
            {
                groundAccel,
                groundMaxSpeed,
                groundDecel,
                airAccel,
                airDecel,
                airMaxSpeed,
                jumpSpeed,
                dynamicDashSpeed,
                kinematicDashSpeed,
                kinematicDashTime,
                rawAccel,
                rawMaxSpeed,
            };
        }
        set
        {
            groundAccel = value[0];
            groundMaxSpeed = value[1];
            groundDecel = value[2];
            airAccel = value[3];
            airDecel = value[4];
            airMaxSpeed = value[5];
            jumpSpeed = value[6];
            dynamicDashSpeed = value[7];
            kinematicDashSpeed = value[8];
            kinematicDashTime = value[9];
            rawAccel = value[10];
            rawMaxSpeed = value[11];
        }
    }

    protected override int[] intValues
    {
        get
        {
            return new int[]
            {
                invertRight,
            };
        }
        set
        {
            invertRight = value[0];
        }
    }

}

/// <summary>
/// Struct that holds information on player input
/// Ensures any value found on Monobehavior.Update() will be handled when appropriate for the motor without being overwritten by a zeroed value
/// </summary>
public class SimpleMovementActionControl : MovementActionControl
{
    // Properties encapsulate their respective variable to prevent overwriting by a zeroed value

    /// <summary>
    /// accelerate input using GetAxis()
    /// -1 for "left", +1 for "right" (with respect to current orientation of motor.CharacterRight)
    /// </summary>
    private float _accelerate;
    public float accelerate { get { return _accelerate; } set { if (resetAccelerate || _accelerate == 0) _accelerate = value; resetAccelerate = false; } }

    /// <summary>
    /// Ensures that run input uses previous input if physics tick runs more than once in a single frame
    /// </summary>
    private bool resetAccelerate;

    /// <summary>
    /// Player run input using GetAxis()
    /// -1 for "left", +1 for "right" (with respect to current orientation of motor.CharacterRight)
    /// </summary>
    private float _maxMove;
    public float maxMove { get { return _maxMove; } set { if (_maxMove == 0) _maxMove = value; } }

    /// <summary>
    /// Player jump input using GetButtonDown()
    /// </summary>
    private bool _jump;
    public bool jump { get { return _jump; } set { if (!_jump) _jump = value; } }

    /// <summary>
    /// Player jump input using GetButtonDown()
    /// </summary>
    private (Vector3, bool) _dynamicDash;
    /// <summary>
    /// dash using a velocity addition with set speed (Vector3 direction, bool force unground)
    /// </summary>
    public (Vector3, bool) dynamicDash { get { return _dynamicDash; } set { if (_dynamicDash.Item1 == Vector3.zero) _dynamicDash = value; } }

    /// <summary>
    /// Player jump input using GetButtonDown()
    /// </summary>
    private (Vector3, bool) _kinematicDash;
    /// <summary>
    /// dash using a kinematic path with set speed (Vector3 direction, bool force unground)
    /// </summary>
    public (Vector3, bool) kinematicDash { get { return _kinematicDash; } set { if (_kinematicDash.Item1 == Vector3.zero) _kinematicDash = value; } }

    /// <summary>
    /// Player jump input using GetButtonDown()
    /// </summary>
    private Vector3 _rawAccelerate;
    /// <summary>
    /// dash using a kinematic path with set speed (Vector3 direction, bool force unground)
    /// </summary>
    public Vector3 rawAccelerate { get { return _rawAccelerate; } set { if (resetRawAccelerate || _rawAccelerate == Vector3.zero) _rawAccelerate = value; resetRawAccelerate = false; } }

    /// <summary>
    /// Ensures that run input uses previous input if physics tick runs more than once in a single frame
    /// </summary>
    private bool resetRawAccelerate;

    /// <summary>
    /// Player jump input using GetButtonDown()
    /// </summary>
    private Vector3 _rawMaxMove;
    /// <summary>
    /// dash using a kinematic path with set speed (Vector3 direction, bool force unground)
    /// </summary>
    public Vector3 rawMaxMove { get { return _rawMaxMove; } set { if (_rawMaxMove == Vector3.zero) _rawMaxMove = value; } }

    /// <summary>
    /// Player jump input using GetButtonDown()
    /// </summary>
    private bool _halt;
    public bool halt { get { return _halt; } set { if (!_halt) _halt = value; } }

    /// <summary>
    /// Reset to default values
    /// </summary>
    public void Reset()
    {
        resetAccelerate = true;
        resetRawAccelerate = true;
        _maxMove = 0;
        _jump = false;
        _dynamicDash = (Vector3.zero, false);
        _kinematicDash = (Vector3.zero, false);
        _rawMaxMove = Vector3.zero;
        _halt = false;
    }
}

/// <summary>
/// Handles application of internally intended actions focused on moving the player
/// </summary>
public class SimpleMovementAction : CharacterMovementAction
{
    [SerializeField]
    public CharacterOverridableAttribute<SimpleMovementActionValues> overridableAttribute = new CharacterOverridableAttribute<SimpleMovementActionValues>();

    public new SimpleMovementActionControl control;

    [SerializeField]
    bool jumpPerpendicularToSlope;

    [SerializeField]
    bool haltIfNoInput;

    [SerializeField]
    bool turnWithDeceleration;
    [SerializeField]
    bool turnWithVelocity;
    [SerializeField]
    bool turnWithAcceleration;
    [SerializeField]
    bool turnInAir;

    private void Reset()
    {
        // Set default values
        /*
        overridableAttribute.baseValues.groundAccel = 10;
        overridableAttribute.baseValues.groundMaxSpeed = 18;
        overridableAttribute.baseValues.groundDecel = 20;
        overridableAttribute.baseValues.airAccel = 10;
        overridableAttribute.baseValues.airDecel = 20;
        overridableAttribute.baseValues.airMaxSpeed = 18;
        overridableAttribute.baseValues.jumpSpeed = 20;
        overridableAttribute.baseValues.dynamicDashSpeed = 20;
        overridableAttribute.baseValues.kinematicDashSpeed = 20;
        overridableAttribute.baseValues.kinematicDashTime = 0.5f;
        overridableAttribute.baseValues.rawAccel = 10;
        overridableAttribute.baseValues.rawMaxSpeed = 18;

        overridableAttribute.baseValues.invertRight = 0;

        jumpPerpendicularToSlope = false;
        */
    }

    /// <summary>
    /// Constructor
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        base.control = new SimpleMovementActionControl();
        control = base.control as SimpleMovementActionControl;
    }

    protected override void Start()
    {
        base.Start();

        GetComponent<CharacterValueOverridability>()?.RegisterOverridability(overridableAttribute);
    }

    protected override void OnDisable()
    {
        base.OnDisable();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
    }

    /// <summary>
    /// Adds appropriate run acceleration to the player 
    /// </summary>
    /// <param name="currentVelocity"> Reference to the player's velocity</param>
    /// <param name="motor"> The player's kinematic motor</param>
    /// <param name="gravityDirection"> The direction of gravity</param>
    /// <param name="physicsOverride"> Determines overrides to player physics values </param>
    /// <param name="deltaTime"> Motor update time</param>
    private void GroundAccelerate(ref Vector3 currentVelocity, float deltaTime)
    {
        // Ensure friction does not activate
        if (physics) physics.negations.kineticAndStaticFrictionNegated = true;

        // Calculate current square speed
        float sqrSpeed = currentVelocity.sqrMagnitude;
        // Direction attempting to run in
        Vector3 runDirection = control.accelerate * Vector3.ProjectOnPlane(movement.right, movement.groundNormal).normalized * (overridableAttribute.values.invertRight > 0 ? -1 : +1);

        // If running against velocity
        if (Vector3.Dot(currentVelocity, runDirection) < 0)
        {
            // Brake via deceleration
            currentVelocity += runDirection * overridableAttribute.values.groundDecel * deltaTime;
            if (turnWithDeceleration)
                facingDirection = Mathf.Sign(control.accelerate) * (overridableAttribute.values.invertRight > 0 ? -1 : +1);
        }
        else
        {
            // If possible run along ground via acceleration
            if (sqrSpeed < overridableAttribute.values.groundMaxSpeed * overridableAttribute.values.groundMaxSpeed)
                currentVelocity += runDirection * overridableAttribute.values.groundAccel * deltaTime;
            if (turnWithAcceleration)
                facingDirection = Mathf.Sign(control.accelerate) * (overridableAttribute.values.invertRight > 0 ? -1 : +1);
        }
    }

    /// <summary>
    /// Adds appropriate air move acceleration to the player 
    /// </summary>
    /// <param name="currentVelocity"> Reference to the player's velocity </param>
    /// <param name="motor"> The player's kinematic motor </param>
    /// <param name="gravityDirection"> The direction of gravity </param>
    /// <param name="physicsNegations"> Determines overrides to player physics values </param>
    /// <param name="deltaTime"> Motor update time</param>
    private void AirAccelerate(ref Vector3 currentVelocity, float deltaTime)
    {
        // The velocity perpendicular to gravity 
        Vector3 flattenedVelocity = Vector3.ProjectOnPlane(currentVelocity, -movement.upWorldOrientation);
        // The squared speed perpendicular to gravity 
        float flattenedSqrSpeed = flattenedVelocity.sqrMagnitude;
        // Direction attempting to move in
        Vector3 airMoveDirection = Vector3.Cross(movement.upWorldOrientation, movement.forward) * control.accelerate * (overridableAttribute.values.invertRight > 0 ? -1 : +1);

        // Ensure drag does not activate
        if (physics) physics.negations.airDragNegated = true;

        Vector3 intoWallNormal = Vector3.zero;
        float intoWallDot = 0;
        float dotChecking;
        if (movement.wallHits.hitCeiling != Vector3.zero && (dotChecking = Vector3.Dot(movement.wallHits.hitCeiling, airMoveDirection)) < intoWallDot)
        {
            intoWallNormal = movement.wallHits.hitCeiling;
            intoWallDot = dotChecking;
        }
        if (movement.wallHits.hitRightWall != Vector3.zero && (dotChecking = Vector3.Dot(movement.wallHits.hitRightWall, airMoveDirection)) < intoWallDot)
        {
            intoWallNormal = movement.wallHits.hitRightWall;
            intoWallDot = dotChecking;
        }
        if (movement.wallHits.hitLeftWall != Vector3.zero && (dotChecking = Vector3.Dot(movement.wallHits.hitLeftWall, airMoveDirection)) < intoWallDot)
        {
            intoWallNormal = movement.wallHits.hitLeftWall;
            intoWallDot = dotChecking;
        }

        if (intoWallDot == 0 || Vector3.Dot(intoWallNormal, -movement.upWorldOrientation) >= 0)
        {
            // If moving against horizontal velocity
            if (Vector3.Dot(flattenedVelocity, airMoveDirection) < 0)
            {
                // Brake via deceleration
                currentVelocity += airMoveDirection * overridableAttribute.values.airDecel * deltaTime;

                if (turnWithDeceleration)
                    facingDirection = Mathf.Sign(control.accelerate) * (overridableAttribute.values.invertRight > 0 ? -1 : +1);
            }
            // If possible move in air via acceleration
            else if (flattenedSqrSpeed < overridableAttribute.values.airMaxSpeed * overridableAttribute.values.airMaxSpeed)
            {
                currentVelocity += airMoveDirection * overridableAttribute.values.airAccel * deltaTime;

                if (turnWithAcceleration && turnInAir)
                    facingDirection = Mathf.Sign(control.accelerate) * (overridableAttribute.values.invertRight > 0 ? -1 : +1);
            }
        }
    }

    /// <summary>
    /// Adds appropriate run acceleration to the player 
    /// </summary>
    /// <param name="currentVelocity"> Reference to the player's velocity</param>
    /// <param name="motor"> The player's kinematic motor</param>
    /// <param name="gravityDirection"> The direction of gravity</param>
    /// <param name="physicsOverride"> Determines overrides to player physics values </param>
    /// <param name="deltaTime"> Motor update time</param>
    private void GroundMaxMove(ref Vector3 currentVelocity, float deltaTime)
    {
        // Ensure friction does not activate
        if (physics) physics.negations.kineticAndStaticFrictionNegated = true;
        if (physics) physics.negations.gravityNegated = true;

        // Calculate current square speed
        float sqrSpeed = currentVelocity.sqrMagnitude;
        // Direction attempting to run in
        Vector3 runDirection = control.maxMove * Vector3.ProjectOnPlane(movement.right, movement.groundNormal).normalized * (overridableAttribute.values.invertRight > 0 ? -1 : +1);

        currentVelocity = runDirection * overridableAttribute.values.groundMaxSpeed;

        if (turnWithVelocity)
            facingDirection = Mathf.Sign(control.maxMove) * (overridableAttribute.values.invertRight > 0 ? -1 : +1);
    }

    /// <summary>
    /// Adds appropriate air move acceleration to the player 
    /// </summary>
    /// <param name="currentVelocity"> Reference to the player's velocity </param>
    /// <param name="motor"> The player's kinematic motor </param>
    /// <param name="gravityDirection"> The direction of gravity </param>
    /// <param name="physicsNegations"> Determines overrides to player physics values </param>
    /// <param name="deltaTime"> Motor update time</param>
    private void AirMaxMove(ref Vector3 currentVelocity, float deltaTime)
    {
        // The velocity perpendicular to gravity 
        Vector3 flattenedVelocity = Vector3.ProjectOnPlane(currentVelocity, -movement.upWorldOrientation);
        // Direction attempting to move in
        Vector3 airMoveDirection = Vector3.Cross(movement.upWorldOrientation, movement.forward) * control.accelerate * (overridableAttribute.values.invertRight > 0 ? -1 : +1);

        // Ensure drag does not activate
        if (physics) physics.negations.airDragNegated = true;

        Vector3 intoWallNormal = Vector3.zero;
        float intoWallDot = 0;
        float dotChecking;
        if (movement.wallHits.hitCeiling != Vector3.zero && (dotChecking = Vector3.Dot(movement.wallHits.hitCeiling, airMoveDirection)) < intoWallDot)
        {
            intoWallNormal = movement.wallHits.hitCeiling;
            intoWallDot = dotChecking;
        }
        if (movement.wallHits.hitRightWall != Vector3.zero && (dotChecking = Vector3.Dot(movement.wallHits.hitRightWall, airMoveDirection)) < intoWallDot)
        {
            intoWallNormal = movement.wallHits.hitRightWall;
            intoWallDot = dotChecking;
        }
        if (movement.wallHits.hitLeftWall != Vector3.zero && (dotChecking = Vector3.Dot(movement.wallHits.hitLeftWall, airMoveDirection)) < intoWallDot)
        {
            intoWallNormal = movement.wallHits.hitLeftWall;
            intoWallDot = dotChecking;
        }

        if (intoWallDot == 0 || Vector3.Dot(intoWallNormal, -movement.upWorldOrientation) >= 0)
        {
            currentVelocity = Vector3.Project(currentVelocity, movement.upWorldOrientation) + airMoveDirection * overridableAttribute.values.airAccel;

            if (turnWithVelocity)
                facingDirection = Mathf.Sign(control.maxMove) * (overridableAttribute.values.invertRight > 0 ? -1 : +1);
        }
    }

    /// <summary>
    /// Adds velocity from activated jump 
    /// </summary>
    /// <param name="currentVelocity"> Reference to the player's velocity </param>
    /// <param name="motor"> The player's kinematic motor </param>
    private void Jump(ref Vector3 currentVelocity, Vector3 groundNormal)
    {
        // Jump perpendicular to the current slope
        currentVelocity += Vector3.ProjectOnPlane(overridableAttribute.values.jumpSpeed * groundNormal, movement.forward);

        // Unground the motor
        movement.ForceUnground();
    }

    /// <summary>
    /// Updates the reference velocity based on intended player actions
    /// </summary>
    /// <param name="currentVelocity"> Reference to the player's velocity</param>
    /// <param name="movement"> The player's kinematic motor</param>
    /// <param name="gravityDirection"> The direction of gravity </param>
    /// <param name="physicsOverride"> Determines overrides to player physics values </param>
    /// <param name="deltaTime"> Motor update time</param>
    public override void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
    {
        if (haltIfNoInput &&
            control.accelerate == 0 &&
            control.maxMove == 0 &&
            control.jump == false &&
            control.dynamicDash.Item1 == Vector3.zero &&
            control.kinematicDash.Item1 == Vector3.zero &&
            control.rawAccelerate == Vector3.zero &&
            control.rawMaxMove == Vector3.zero &&
            control.halt == false
            )
        {
            currentVelocity = Vector3.zero;
            return;
        }

        if (control.halt)
        {
            currentVelocity = Vector3.zero;
        }

        if (control.kinematicDash.Item1 != Vector3.zero)
        {
            if (control.kinematicDash.Item2)
                movement.ForceUnground();

            movement.SetKinematicPath(control.kinematicDash.Item1 * overridableAttribute.values.kinematicDashSpeed, overridableAttribute.values.kinematicDashTime);
        }
        else 
        {
            if (control.rawMaxMove != Vector3.zero)
            {
                if (physics)
                    physics.negations.airDragNegated = true;

                if (physics)
                    physics.negations.airDragNegated = true;

                currentVelocity = control.rawMaxMove * overridableAttribute.values.rawMaxSpeed;
                if (movement.isGroundedThisUpdate)
                    movement.ForceUnground();

                if (turnWithVelocity)
                    facingDirection = Mathf.Sign(Vector3.Dot(control.rawMaxMove, movement.right)) * (overridableAttribute.values.invertRight > 0 ? -1 : +1);
            }
            else
            {
                if (control.maxMove != 0)
                {
                    if (movement.isGroundedThisUpdate)
                        GroundMaxMove(ref currentVelocity, deltaTime);
                    else
                        AirMaxMove(ref currentVelocity, deltaTime);
                }
                else if (control.accelerate != 0)
                {
                    if (movement.isGroundedThisUpdate)
                        GroundAccelerate(ref currentVelocity, deltaTime);
                    else
                        AirAccelerate(ref currentVelocity, deltaTime);
                }

                if (control.dynamicDash.Item1 != Vector3.zero)
                {
                    if (control.kinematicDash.Item2)
                        movement.ForceUnground();

                    currentVelocity += control.dynamicDash.Item1 * overridableAttribute.values.dynamicDashSpeed;
                }
                if (control.rawAccelerate != Vector3.zero)
                {
                    if (physics)
                        physics.negations.airDragNegated = true;

                    currentVelocity = Vector3.ClampMagnitude(currentVelocity + control.rawAccelerate * overridableAttribute.values.rawAccel * deltaTime, overridableAttribute.values.rawMaxSpeed);
                    
                    if (movement.isGroundedThisUpdate)
                        movement.ForceUnground();

                    if(turnWithDeceleration || (turnWithAcceleration && Vector3.Dot(currentVelocity, movement.right * (overridableAttribute.values.invertRight > 0 ? -1 : +1)) > 0))
                        facingDirection = Mathf.Sign(Vector3.Dot(control.rawAccelerate, movement.right)) * (overridableAttribute.values.invertRight > 0 ? -1 : +1);
                }

                // if the player is trying to and able to jump
                if (control.jump)
                {
                    if (movement.isGroundedThisUpdate)
                        Jump(ref currentVelocity, (jumpPerpendicularToSlope) ? movement.groundNormal : movement.upWorldOrientation);
                    else if (bufferedUngroundedNormal != Vector3.zero)
                    {
                        Jump(ref currentVelocity, bufferedUngroundedNormal);
                        bufferedUngroundedNormal = Vector3.zero;
                    }
                }
            }
        }
    }
}