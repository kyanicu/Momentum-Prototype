using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KinematicCharacterController;
using System;

[System.Serializable]
public class MomentumMovementActionValues : CharacterOverridableValues
{
    /// <summary>
    /// The acceleration of the player when actively running on the ground
    /// </summary>
    public float runAccel;
    /// <summary>
    /// The instantaneous speed the player achieves when manually performing a run kick off
    /// </summary>
    public float runKickOffSpeed;
    /// <summary>
    /// The instantaneous speed the player achieves when automatically performing a run kick off
    /// </summary>
    public float autoRunKickOffSpeed;
    /// <summary>
    /// The min slope required for auto run kick off to be triggered
    /// </summary>
    public float autoRunKickOffSlopeThreshold;
    /// <summary>
    /// The max speed a player can achieve by actively running on the ground alone
    /// </summary>
    public float runMaxSpeed;
    /// <summary>
    /// The deceleration of the player when actively running on the ground against their velocity
    /// </summary>
    public float brakeDecel;
    /// <summary>
    /// The acceleration of the player when actively moving horizontally in the air over a minimum speed
    /// </summary>
    public float looseAirMoveAccel;
    /// <summary>
    /// The deceleration of the player when actively moving horizontally in the air over a minimum speed against their velocity
    /// </summary>
    public float looseAirMoveBrakeDecel;
    /// <summary>
    /// The acceleration of the player when actively moving horizontally in the air under a minimum speed
    /// </summary>
    public float preciseAirMoveAccel;
    /// <summary>
    /// The deceleration of the player when actively moving horizontally in the air over a minimum speed against their velocity
    /// </summary>
    public float preciseAirMoveBrakeDecel;
    /// <summary>
    /// The threshold of horizontal air speed to determine which acceleration to use (precise if under, loose if over)
    /// </summary>
    public float airSpeedThreshold;
    /// <summary>
    /// The max horizontal speed a player can achieve by actively moving in the air alone
    /// </summary>
    public float airMoveMaxSpeed;
    /// <summary>
    /// The "up" (perpendicular to the current slope) speed at which the player jumps off the ground
    /// </summary>
    public float jumpSpeed;
    /// <summary>
    /// The factor on the player's angular velocity when jumping with internalAngularVelocity stored;
    /// </summary>
    public float jumpRotationFactor;
    /// <summary>
    /// The "up" (opposite to the direction of gravity) speed the the player is set at when jump canceling
    /// </summary>
    public float jumpCancelSpeed;
    /// <summary>
    /// The maximum "up" (opposite to the direction of gravity) speed allowed for jump canceling to activate 
    /// </summary>
    public float jumpCancelThreshold;

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
				runAccel,
				runKickOffSpeed,
				autoRunKickOffSpeed,
				autoRunKickOffSlopeThreshold,
				runMaxSpeed,
				brakeDecel,
				looseAirMoveAccel,
				looseAirMoveBrakeDecel,
				preciseAirMoveAccel,
				preciseAirMoveBrakeDecel,
				airSpeedThreshold,
				airMoveMaxSpeed,
				jumpSpeed,
				jumpRotationFactor,
				jumpCancelSpeed,
				jumpCancelThreshold,
				invertRight,
            };
        }
    	set
        {
			runAccel = value[0];
			runKickOffSpeed = value[1];
			autoRunKickOffSpeed = value[2];
			autoRunKickOffSlopeThreshold = value[3];
			runMaxSpeed = value[4];
			brakeDecel = value[5];
			looseAirMoveAccel = value[6];
			looseAirMoveBrakeDecel = value[7];
			preciseAirMoveAccel = value[8];
			preciseAirMoveBrakeDecel = value[9];
			airSpeedThreshold = value[10];
			airMoveMaxSpeed = value[11];
			jumpSpeed = value[12];
			jumpRotationFactor = value[13];
			jumpCancelSpeed = value[14];
 			jumpCancelThreshold = value[15];
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
public class MomentumMovementActionControl : MovementActionControl
{
    // Properties encapsulate their respective variable to prevent overwriting by a zeroed value

    /// <summary>
    /// Player run input using GetAxis()
    /// -1 for "left", +1 for "right" (with respect to current orientation of motor.CharacterRight)
    /// </summary>
    private float _run;
    public float run { get { return _run; } set { if (resetRun || _run == 0) _run = value; resetRun = false;} }

    /// <summary>
    /// Ensures that run input uses previous input if physics tick runs more than once in a single frame
    /// </summary>
    private bool resetRun;

    /// <summary>
    /// Player jump input using GetButtonDown()
    /// </summary>
    private bool _jump;
    public bool jump { get { return _jump; } set {  if (!_jump) _jump = value; } }

    /// <summary>
    /// Player jump cancel input using GetButtonUp
    /// </summary>
    private bool _jumpCancel;
    public bool jumpCancel { get { return _jumpCancel; } set {  if (!_jumpCancel) _jumpCancel = value; } }

    /// <summary>
    /// Player double tap input, true if player has double tapped the same run input this motor update
    /// </summary>
    private bool _doubleTapRun;
    public bool doubleTapRun { get { return _doubleTapRun; } set {  if (!_doubleTapRun) _doubleTapRun = value; } }

    /// <summary>
    /// Reset to default values
    /// </summary>
    public void Reset()
    {
        resetRun = true;
        _jump = false;
        _jumpCancel = false;
        _doubleTapRun = false;
    }
}

/// <summary>
/// Handles application of internally intended actions focused on moving the player
/// </summary>
public class MomentumMovementAction : CharacterMovementAction
{
    /// <summary>
    /// Is the player currently moving "up" due to an initiated jump?
    /// </summary>
    private bool isJumping;
    private bool bufferingJump;

    private bool bufferingJumpCancel;

    /// <summary>
    /// Is the player waiting for a jump cancel to activate ?
    /// </summary>
    private bool waitingToJumpCancel;

    public bool isBraking { get; private set; }

    [SerializeField]
    public CharacterOverridableAttribute<MomentumMovementActionValues> overridableAttribute = new CharacterOverridableAttribute<MomentumMovementActionValues>();

    public new MomentumMovementActionControl control;

    private void Reset()
    {
        // Set default values
        overridableAttribute.baseValues.runAccel = 15;
        overridableAttribute.baseValues.runKickOffSpeed = 15;
        overridableAttribute.baseValues.autoRunKickOffSpeed = 2;
        overridableAttribute.baseValues.autoRunKickOffSlopeThreshold = 14;
        overridableAttribute.baseValues.runMaxSpeed = 18;
        overridableAttribute.baseValues.brakeDecel = 50;
        overridableAttribute.baseValues.looseAirMoveAccel = 10;
        overridableAttribute.baseValues.looseAirMoveBrakeDecel = 15;
        overridableAttribute.baseValues.preciseAirMoveAccel = 30;
        overridableAttribute.baseValues.preciseAirMoveBrakeDecel = 70; 
        overridableAttribute.baseValues.airSpeedThreshold = 12;
        overridableAttribute.baseValues.airMoveMaxSpeed = 18;
        overridableAttribute.baseValues.jumpSpeed = 22;
        overridableAttribute.baseValues.jumpRotationFactor = 2; 
        overridableAttribute.baseValues.jumpCancelSpeed = 4;
        overridableAttribute.baseValues.jumpCancelThreshold = 20;
        overridableAttribute.baseValues.invertRight = 0;
    }

    /// <summary>
    /// Constructor
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        base.control = new MomentumMovementActionControl();
        control = base.control as MomentumMovementActionControl;
    }

    protected override void Start()
    {
        base.Start();

        GetComponent<CharacterValueOverridability>()?.RegisterOverridability(overridableAttribute);
    }

    /// <summary>
    /// Adds appropriate run acceleration to the player 
    /// </summary>
    /// <param name="currentVelocity"> Reference to the player's velocity</param>
    /// <param name="motor"> The player's kinematic motor</param>
    /// <param name="gravityDirection"> The direction of gravity</param>
    /// <param name="physicsOverride"> Determines overrides to player physics values </param>
    /// <param name="deltaTime"> Motor update time</param>
    private void Run(ref Vector3 currentVelocity, float deltaTime)
    {
        // Ensure friction does not activate
        if (physics) physics.negations.kineticAndStaticFrictionNegated = true;

        // Calculate current square speed
        float sqrSpeed = currentVelocity.sqrMagnitude;
        // Direction attempting to run in
        Vector3 runDirection = control.run * Vector3.ProjectOnPlane(movement.right, movement.groundNormal).normalized * (overridableAttribute.values.invertRight > 0 ? -1 : +1);
        
        float autoRunKickOffBuffer = 0.25f;
        // If manual run kick off was activated successfully
        if (control.doubleTapRun && sqrSpeed < (overridableAttribute.values.runKickOffSpeed-autoRunKickOffBuffer) * (overridableAttribute.values.runKickOffSpeed-autoRunKickOffBuffer))
            RunKickOff(ref currentVelocity, runDirection, false);
        // If automatic run kick off was triggered
        else if (sqrSpeed == 0 && Vector3.Dot(runDirection, -movement.upWorldOrientation) < 0 && Vector3.Angle(movement.groundNormal, -movement.upWorldOrientation) >= overridableAttribute.values.autoRunKickOffSlopeThreshold)
            RunKickOff(ref currentVelocity, runDirection, true);
        else
        {
            // If running against velocity
            if (Vector3.Dot(currentVelocity, runDirection) < 0)
            {
                // Brake via deceleration
                isBraking = true;
                currentVelocity += runDirection * overridableAttribute.values.brakeDecel * deltaTime;
            }
            else
            {
                // If possible run along ground via acceleration
                if (sqrSpeed < overridableAttribute.values.runMaxSpeed * overridableAttribute.values.runMaxSpeed * control.run * control.run)
                    currentVelocity += runDirection * overridableAttribute.values.runAccel * deltaTime;

                facingDirection = Mathf.Sign(control.run) * (overridableAttribute.values.invertRight > 0 ? -1 : +1);         
            }
        }
    }

    /// <summary>
    /// Activates run kick off
    /// </summary>
    /// <param name="currentVelocity"> Reference to the player's velocity </param>
    /// <param name="runDirection"> The direction to run kick off in</param>
    /// <param name="physicsNegations"> Determines overrides to player physics values </param>
    /// <param name="auto"> Was the run kick off automatically activated? </param>
    private void RunKickOff(ref Vector3 currentVelocity, Vector3 runDirection, bool auto)
    {
        if (auto)
            if (physics) physics.negations.gravityNegated = true;

        // Determine whether to use auto or manula run kick off speed
        float speed = (auto) ? overridableAttribute.values.autoRunKickOffSpeed : overridableAttribute.values.runKickOffSpeed;
        
        // Set velocity
        currentVelocity = speed * runDirection;
    }

    /// <summary>
    /// Adds appropriate air move acceleration to the player 
    /// </summary>
    /// <param name="currentVelocity"> Reference to the player's velocity </param>
    /// <param name="motor"> The player's kinematic motor </param>
    /// <param name="gravityDirection"> The direction of gravity </param>
    /// <param name="physicsNegations"> Determines overrides to player physics values </param>
    /// <param name="deltaTime"> Motor update time</param>
    private void AirMove(ref Vector3 currentVelocity, float deltaTime)
    {
        // The velocity perpendicular to gravity 
        Vector3 flattenedVelocity = Vector3.ProjectOnPlane(currentVelocity, -movement.upWorldOrientation);
        // The squared speed perpendicular to gravity 
        float flattenedSqrSpeed = flattenedVelocity.sqrMagnitude;
        // Direction attempting to move in
        Vector3 airMoveDirection = Vector3.Cross(movement.upWorldOrientation, movement.forward) * control.run * (overridableAttribute.values.invertRight > 0 ? -1 : +1);

        // Ensure drag does not activate
        if (physics) physics.negations.airDragNegated = true;

        // Determine which air move values to use
        float airMoveAccel = (flattenedSqrSpeed >= overridableAttribute.values.airSpeedThreshold * overridableAttribute.values.airSpeedThreshold) ? overridableAttribute.values.looseAirMoveAccel : overridableAttribute.values.preciseAirMoveAccel;
        float airMoveBrakeAccel = (flattenedSqrSpeed >= overridableAttribute.values.airSpeedThreshold * overridableAttribute.values.airSpeedThreshold) ? overridableAttribute.values.looseAirMoveBrakeDecel : overridableAttribute.values.preciseAirMoveBrakeDecel;

        Vector3 intoWallNormal = Vector3.zero;
        float intoWallDot = 0;
        float dotChecking;
        if (movement.wallHits.hitCeiling != Vector3.zero && (dotChecking = Vector3.Dot(movement.wallHits.hitCeiling, airMoveDirection)) < intoWallDot)
        {
            intoWallNormal = movement.wallHits.hitCeiling;
            intoWallDot = dotChecking;
        }
        if(movement.wallHits.hitRightWall != Vector3.zero && (dotChecking = Vector3.Dot(movement.wallHits.hitRightWall, airMoveDirection)) < intoWallDot)
        {
            intoWallNormal = movement.wallHits.hitRightWall;
            intoWallDot = dotChecking;
        }
        if(movement.wallHits.hitLeftWall != Vector3.zero && (dotChecking = Vector3.Dot(movement.wallHits.hitLeftWall, airMoveDirection)) < intoWallDot)
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
                currentVelocity += airMoveDirection * airMoveBrakeAccel * deltaTime;
            }
            // If possible move in air via acceleration
            else if (flattenedSqrSpeed < overridableAttribute.values.airMoveMaxSpeed * overridableAttribute.values.airMoveMaxSpeed * control.run * control.run)
            {
                currentVelocity += airMoveDirection * airMoveAccel * deltaTime;
            }
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

        isJumping = true;
    }

    private void HandleJumpRotation(ref Vector3 internalAngularVelocity)
    {
        internalAngularVelocity *= overridableAttribute.values.jumpRotationFactor;
    }

    /// <summary>
    /// Checks to see if the player is still moving against gravity due to an activate jump
    /// </summary>
    /// <param name="currentVelocity"> Reference to the player's velocity </param>
    /// <param name="motor"> The player's kinematic motor </param>
    /// <param name="gravityDirection"> The direction of gravity </param>
    /// <returns></returns>
    private bool CheckIsStillJumping(ref Vector3 currentVelocity)
    {
        // If the player is mid-air and not falling, return true 
        return !movement.isGroundedThisUpdate && Vector3.Dot(currentVelocity, -movement.upWorldOrientation) < 0;
    }

    /// <summary>
    /// Attempts to cancel a jump if activate, ensures attempt will be made again next motor update if unable to
    /// </summary>
    /// <param name="currentVelocity"> Reference to the player's velocity </param>
    /// <param name="motor"> The player's kinematic motor </param>
    /// <param name="gravityDirection"> The direction of gravity </param>
    private void JumpCancel(ref Vector3 currentVelocity)
    {
        // Get the velocity in the direction of gravity
        Vector3 velocityAlongGravity = Vector3.Project(currentVelocity, -movement.upWorldOrientation);

        if(velocityAlongGravity.sqrMagnitude < overridableAttribute.values.jumpCancelSpeed * overridableAttribute.values.jumpCancelSpeed)
            waitingToJumpCancel = false;
        // If jump cancel is valid
        else if (velocityAlongGravity.sqrMagnitude <= overridableAttribute.values.jumpCancelThreshold * overridableAttribute.values.jumpCancelThreshold)
        {
            // Perform jump cancel
            currentVelocity = (currentVelocity - velocityAlongGravity) + (movement.upWorldOrientation * overridableAttribute. values.jumpCancelSpeed);
            waitingToJumpCancel = false;
        }
        else // Remember to check next motor update
            waitingToJumpCancel = true;
    }

    /// <summary>
    /// Updates the reference rotation based on intended player actions
    /// </summary>
    /// <param name="currentRotation"> Reference to the player's rotation</param>
    /// <param name="currentAngularVelocity"> Reference to the player's angular velocity</param>
    /// <param name="movement"> The player's kinematic motor</param>
    /// <param name="deltaTime"> Motor update time</param>
    public override void UpdateRotation(ref Quaternion currentRotation, ref Vector3 currentAngularVelocity, float deltaTime)
    {
        if(movement.wasGroundedLastUpdate && isJumping && currentAngularVelocity != Vector3.zero)
            HandleJumpRotation(ref currentAngularVelocity);
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
        // If the player is jumping
        if(isJumping && CheckIsStillJumping(ref currentVelocity))
        {
            // If the player is trying to jump cancel
            if(control.jumpCancel || waitingToJumpCancel)
                JumpCancel(ref currentVelocity);
        }
        else 
        {   
            /*
            if(bufferingJump)
            {
                if (input.jumpCancel)
                    bufferingJumpCancel = true;
                if (motor.IsGroundedThisUpdate)
                    bufferingJump = false;
            }
            else if (!input.jumpCancel)
            {
                /*`bufferingJumpCancel = false;*/
                // Make sure player isn't still trying to jump cancel
                waitingToJumpCancel = false;
                isJumping = false;
            /*}*/
        }

        // if the player is trying to and able to jump
        if(control.jump)
        {
            if (movement.isGroundedThisUpdate)
                Jump(ref currentVelocity, movement.groundNormal);
            else if (bufferedUngroundedNormal != Vector3.zero)
            {
                Jump(ref currentVelocity, bufferedUngroundedNormal);
                bufferedUngroundedNormal = Vector3.zero;
            }
            /*
            else
            {
                GameManager.Instance.TimedConditionalCheckViaRealTime(groundingBufferTime, motor.GetIsGroundedThisUpdate, SetInputJumpTrue);
                bufferingJump = true;
            }
            */
        }
        isBraking = false;
        // if the player is trying to run
        if(control.run != 0)
        {
            if (movement.isGroundedThisUpdate)
                Run(ref currentVelocity, deltaTime);
            else
                AirMove(ref currentVelocity, deltaTime);
        }
    }

    public override void Flinch()
    {
        if (isJumping)
        {
            control.Reset();
            control.jumpCancel = true;
        }
        else
            control.Reset();
    }

}