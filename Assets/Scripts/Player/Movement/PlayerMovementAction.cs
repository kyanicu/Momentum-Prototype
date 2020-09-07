using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KinematicCharacterController;
using System;

[System.Serializable]
public class PlayerMovementActionValues : PlayerMovementOverridableValues
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

    public override void SetDefaultValues(PlayerMovementOverrideType overrideType)
    {
        runAccel = DefaultFloat(overrideType);
        runKickOffSpeed = DefaultFloat(overrideType);
        autoRunKickOffSpeed = DefaultFloat(overrideType);
        autoRunKickOffSlopeThreshold = DefaultFloat(overrideType);
        runMaxSpeed = DefaultFloat(overrideType);
        brakeDecel = DefaultFloat(overrideType);
        looseAirMoveAccel = DefaultFloat(overrideType);
        looseAirMoveBrakeDecel = DefaultFloat(overrideType);
        preciseAirMoveAccel = DefaultFloat(overrideType);
        preciseAirMoveBrakeDecel = DefaultFloat(overrideType);
        airSpeedThreshold = DefaultFloat(overrideType);
        airMoveMaxSpeed = DefaultFloat(overrideType);
        jumpSpeed = DefaultFloat(overrideType);
        jumpRotationFactor = DefaultFloat(overrideType);
        jumpCancelSpeed = DefaultFloat(overrideType);
        jumpCancelThreshold = DefaultFloat(overrideType);
        invertRight = DefaultInt(overrideType);
    }
    
    public override void AddBy(PlayerMovementOverridableValues ov) 
    {
        PlayerMovementActionValues v = ov as PlayerMovementActionValues;

        runAccel = Add(runAccel, v.runAccel);
        runKickOffSpeed = Add(runKickOffSpeed, v.runKickOffSpeed);
        autoRunKickOffSpeed = Add(autoRunKickOffSpeed, v.autoRunKickOffSpeed);
        autoRunKickOffSlopeThreshold = Add(autoRunKickOffSlopeThreshold, v.autoRunKickOffSlopeThreshold);
        runMaxSpeed = Add(runMaxSpeed, v.runMaxSpeed);
        brakeDecel = Add(brakeDecel, v.brakeDecel);
        looseAirMoveAccel = Add(looseAirMoveAccel, v.looseAirMoveAccel);
        looseAirMoveBrakeDecel = Add(looseAirMoveBrakeDecel, v.looseAirMoveBrakeDecel);
        preciseAirMoveAccel = Add(preciseAirMoveAccel, v.preciseAirMoveAccel);
        preciseAirMoveBrakeDecel = Add(preciseAirMoveBrakeDecel, v.preciseAirMoveBrakeDecel);
        airSpeedThreshold = Add(airSpeedThreshold, v.airSpeedThreshold);
        airMoveMaxSpeed = Add(airMoveMaxSpeed, v.airMoveMaxSpeed);
        jumpSpeed = Add(jumpSpeed, v.jumpSpeed);
        jumpRotationFactor = Add(jumpRotationFactor, v.jumpRotationFactor);
        jumpCancelSpeed = Add(jumpCancelSpeed, v.jumpCancelSpeed);
        jumpCancelThreshold = Add(jumpCancelThreshold, v.jumpCancelThreshold);
        invertRight = Add(invertRight, v.invertRight);
    }

    public override void SubtractBy(PlayerMovementOverridableValues ov) 
    {
        PlayerMovementActionValues v = ov as PlayerMovementActionValues;

        runAccel = Subtract(runAccel, v.runAccel);
        runKickOffSpeed = Subtract(runKickOffSpeed, v.runKickOffSpeed);
        autoRunKickOffSpeed = Subtract(autoRunKickOffSpeed, v.autoRunKickOffSpeed);
        autoRunKickOffSlopeThreshold = Subtract(autoRunKickOffSlopeThreshold, v.autoRunKickOffSlopeThreshold);
        runMaxSpeed = Subtract(runMaxSpeed, v.runMaxSpeed);
        brakeDecel = Subtract(brakeDecel, v.brakeDecel);
        looseAirMoveAccel = Subtract(looseAirMoveAccel, v.looseAirMoveAccel);
        looseAirMoveBrakeDecel = Subtract(looseAirMoveBrakeDecel, v.looseAirMoveBrakeDecel);
        preciseAirMoveAccel = Subtract(preciseAirMoveAccel, v.preciseAirMoveAccel);
        preciseAirMoveBrakeDecel = Subtract(preciseAirMoveBrakeDecel, v.preciseAirMoveBrakeDecel);
        airSpeedThreshold = Subtract(airSpeedThreshold, v.airSpeedThreshold);
        airMoveMaxSpeed = Subtract(airMoveMaxSpeed, v.airMoveMaxSpeed);
        jumpSpeed = Subtract(jumpSpeed, v.jumpSpeed);
        jumpRotationFactor = Subtract(jumpRotationFactor, v.jumpRotationFactor);
        jumpCancelSpeed = Subtract(jumpCancelSpeed, v.jumpCancelSpeed);
        jumpCancelThreshold = Subtract(jumpCancelThreshold, v.jumpCancelThreshold);
        invertRight = Subtract(invertRight, v.invertRight);
    }

    public override void MultiplyBy(PlayerMovementOverridableValues ov) 
    {
        PlayerMovementActionValues v = ov as PlayerMovementActionValues;

        runAccel = Multiply(runAccel, v.runAccel);
        runKickOffSpeed = Multiply(runKickOffSpeed, v.runKickOffSpeed);
        autoRunKickOffSpeed = Multiply(autoRunKickOffSpeed, v.autoRunKickOffSpeed);
        autoRunKickOffSlopeThreshold = Multiply(autoRunKickOffSlopeThreshold, v.autoRunKickOffSlopeThreshold);
        runMaxSpeed = Multiply(runMaxSpeed, v.runMaxSpeed);
        brakeDecel = Multiply(brakeDecel, v.brakeDecel);
        looseAirMoveAccel = Multiply(looseAirMoveAccel, v.looseAirMoveAccel);
        looseAirMoveBrakeDecel = Multiply(looseAirMoveBrakeDecel, v.looseAirMoveBrakeDecel);
        preciseAirMoveAccel = Multiply(preciseAirMoveAccel, v.preciseAirMoveAccel);
        preciseAirMoveBrakeDecel = Multiply(preciseAirMoveBrakeDecel, v.preciseAirMoveBrakeDecel);
        airSpeedThreshold = Multiply(airSpeedThreshold, v.airSpeedThreshold);
        airMoveMaxSpeed = Multiply(airMoveMaxSpeed, v.airMoveMaxSpeed);
        jumpSpeed = Multiply(jumpSpeed, v.jumpSpeed);
        jumpRotationFactor = Multiply(jumpRotationFactor, v.jumpRotationFactor);
        jumpCancelSpeed = Multiply(jumpCancelSpeed, v.jumpCancelSpeed);
        jumpCancelThreshold = Multiply(jumpCancelThreshold, v.jumpCancelThreshold);
        invertRight = Multiply(invertRight, v.invertRight);
    }

    public override void DivideBy(PlayerMovementOverridableValues ov) 
    {
        PlayerMovementActionValues v = ov as PlayerMovementActionValues;

        runAccel = Divide(runAccel, v.runAccel);
        runKickOffSpeed = Divide(runKickOffSpeed, v.runKickOffSpeed);
        autoRunKickOffSpeed = Divide(autoRunKickOffSpeed, v.autoRunKickOffSpeed);
        autoRunKickOffSlopeThreshold = Divide(autoRunKickOffSlopeThreshold, v.autoRunKickOffSlopeThreshold);
        runMaxSpeed = Divide(runMaxSpeed, v.runMaxSpeed);
        brakeDecel = Divide(brakeDecel, v.brakeDecel);
        looseAirMoveAccel = Divide(looseAirMoveAccel, v.looseAirMoveAccel);
        looseAirMoveBrakeDecel = Divide(looseAirMoveBrakeDecel, v.looseAirMoveBrakeDecel);
        preciseAirMoveAccel = Divide(preciseAirMoveAccel, v.preciseAirMoveAccel);
        preciseAirMoveBrakeDecel = Divide(preciseAirMoveBrakeDecel, v.preciseAirMoveBrakeDecel);
        airSpeedThreshold = Divide(airSpeedThreshold, v.airSpeedThreshold);
        airMoveMaxSpeed = Divide(airMoveMaxSpeed, v.airMoveMaxSpeed);
        jumpSpeed = Divide(jumpSpeed, v.jumpSpeed);
        jumpRotationFactor = Divide(jumpRotationFactor, v.jumpRotationFactor);
        jumpCancelSpeed = Divide(jumpCancelSpeed, v.jumpCancelSpeed);
        jumpCancelThreshold = Divide(jumpCancelThreshold, v.jumpCancelThreshold);
        invertRight = Divide(invertRight, v.invertRight);
    }

    public override void OrBy(PlayerMovementOverridableValues ov) 
    {
        PlayerMovementActionValues v = ov as PlayerMovementActionValues;

        runAccel = Or(runAccel, v.runAccel);
        runKickOffSpeed = Or(runKickOffSpeed, v.runKickOffSpeed);
        autoRunKickOffSpeed = Or(autoRunKickOffSpeed, v.autoRunKickOffSpeed);
        autoRunKickOffSlopeThreshold = Or(autoRunKickOffSlopeThreshold, v.autoRunKickOffSlopeThreshold);
        runMaxSpeed = Or(runMaxSpeed, v.runMaxSpeed);
        brakeDecel = Or(brakeDecel, v.brakeDecel);
        looseAirMoveAccel = Or(looseAirMoveAccel, v.looseAirMoveAccel);
        looseAirMoveBrakeDecel = Or(looseAirMoveBrakeDecel, v.looseAirMoveBrakeDecel);
        preciseAirMoveAccel = Or(preciseAirMoveAccel, v.preciseAirMoveAccel);
        preciseAirMoveBrakeDecel = Or(preciseAirMoveBrakeDecel, v.preciseAirMoveBrakeDecel);
        airSpeedThreshold = Or(airSpeedThreshold, v.airSpeedThreshold);
        airMoveMaxSpeed = Or(airMoveMaxSpeed, v.airMoveMaxSpeed);
        jumpSpeed = Or(jumpSpeed, v.jumpSpeed);
        jumpRotationFactor = Or(jumpRotationFactor, v.jumpRotationFactor);
        jumpCancelSpeed = Or(jumpCancelSpeed, v.jumpCancelSpeed);
        jumpCancelThreshold = Or(jumpCancelThreshold, v.jumpCancelThreshold);
        invertRight = Or(invertRight, v.invertRight);
    }

    public override void AndBy(PlayerMovementOverridableValues ov) 
    {
        PlayerMovementActionValues v = ov as PlayerMovementActionValues;

        runAccel = And(runAccel, v.runAccel);
        runKickOffSpeed = And(runKickOffSpeed, v.runKickOffSpeed);
        autoRunKickOffSpeed = And(autoRunKickOffSpeed, v.autoRunKickOffSpeed);
        autoRunKickOffSlopeThreshold = And(autoRunKickOffSlopeThreshold, v.autoRunKickOffSlopeThreshold);
        runMaxSpeed = And(runMaxSpeed, v.runMaxSpeed);
        brakeDecel = And(brakeDecel, v.brakeDecel);
        looseAirMoveAccel = And(looseAirMoveAccel, v.looseAirMoveAccel);
        looseAirMoveBrakeDecel = And(looseAirMoveBrakeDecel, v.looseAirMoveBrakeDecel);
        preciseAirMoveAccel = And(preciseAirMoveAccel, v.preciseAirMoveAccel);
        preciseAirMoveBrakeDecel = And(preciseAirMoveBrakeDecel, v.preciseAirMoveBrakeDecel);
        airSpeedThreshold = And(airSpeedThreshold, v.airSpeedThreshold);
        airMoveMaxSpeed = And(airMoveMaxSpeed, v.airMoveMaxSpeed);
        jumpSpeed = And(jumpSpeed, v.jumpSpeed);
        jumpRotationFactor = And(jumpRotationFactor, v.jumpRotationFactor);
        jumpCancelSpeed = And(jumpCancelSpeed, v.jumpCancelSpeed);
        jumpCancelThreshold = And(jumpCancelThreshold, v.jumpCancelThreshold);
        invertRight = And(invertRight, v.invertRight);
    }
}

/// <summary>
/// Handles application of internally intended actions focused on moving the player
/// </summary>
[System.Serializable]
public class PlayerMovementAction : PlayerMovementOverridableAttribute<PlayerMovementActionValues>, IPlayerMovementActionCommunication
{
    /// <summary>
    /// Struct that holds information on player input
    /// Ensures any value found on Monobehavior.Update() will be handled when appropriate for the motor without being overwritten by a zeroed value
    /// </summary>
    private struct MovementActionInput : IPlayerMovementInput
    {
        // Properties encapsulate their respective variable to prevent overwriting by a zeroed value

        /// <summary>
        /// Player run input using GetAxis()
        /// -1 for "left", +1 for "right" (with respect to current orientation of motor.CharacterRight)
        /// </summary>
        private float _run;
        public float run { get { return _run; } set {  if (_run == 0) _run = value; } }

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

        public void RegisterInput(PlayerController.PlayerActions controllerActions)
    {
        jump = controllerActions.Jump.triggered;

        jumpCancel = controllerActions.JumpCancel.triggered;
        
        run = controllerActions.Run.ReadValue<float>();

        doubleTapRun = controllerActions.RunKickOff.triggered;
    }

        /// <summary>
        /// Reset to default values
        /// </summary>
        public void Reset()
        {
            _run = 0;
            _jump = false;
            _jumpCancel = false;
            _doubleTapRun = false;
        }
    }

    #region Events
    public event EventHandler facingDirectionChanged;
    #endregion

    /// <summary>
    /// Holds and maintains input info
    /// </summary>
    private MovementActionInput input;

    /// <summary>
    /// Is the player currently moving "up" due to an initiated jump?
    /// </summary>
    private bool isJumping;

    /// <summary>
    /// Is the player waiting for a jump cancel to activate ?
    /// </summary>
    private bool waitingToJumpCancel;

    private float facingDirection;

    /// <summary>
    /// Constructor
    /// </summary>
    public PlayerMovementAction()
    {
        // Set input values
        input = new MovementActionInput();
        facingDirection = +1;
    }

    public void SetCommunication(PlayerInternalCommunicator communicator)
    {
        communicator.SetCommunication(this);
    }

    protected override void SetDefaultBaseValues()
    {
        // Set default values
        baseValues.runAccel = 10;
        baseValues.runKickOffSpeed = 5;
        baseValues.autoRunKickOffSpeed = 2;
        baseValues.autoRunKickOffSlopeThreshold = 14;
        baseValues.runMaxSpeed = 18;
        baseValues.brakeDecel = 40;
        baseValues.looseAirMoveAccel = 14;
        baseValues.looseAirMoveBrakeDecel = 18;
        baseValues.preciseAirMoveAccel = 30;
        baseValues.preciseAirMoveBrakeDecel = 30; 
        baseValues.airMoveMaxSpeed = 18;
        baseValues.airSpeedThreshold = 4;
        baseValues.jumpSpeed = 15;
        baseValues.jumpRotationFactor = 1.25f; 
        baseValues.jumpCancelSpeed = 4;
        baseValues.jumpCancelThreshold = 15;
        baseValues.invertRight = 0;
    }

    protected override void ValidateBaseValues()
    {
        
    }

    /// <summary>
    /// Adds appropriate run acceleration to the player 
    /// </summary>
    /// <param name="currentVelocity"> Reference to the player's velocity</param>
    /// <param name="motor"> The player's kinematic motor</param>
    /// <param name="gravityDirection"> The direction of gravity</param>
    /// <param name="physicsOverride"> Determines overrides to player physics values </param>
    /// <param name="deltaTime"> Motor update time</param>
    private void Run(ref Vector3 currentVelocity, KinematicCharacterMotor motor, Vector3 gravityDirection, ref PlayerMovementPhysics.PhysicsNegations physicsNegations, float deltaTime)
    {
        // Ensure friction does not activate
        physicsNegations.kineticAndStaticFrictionNegated = true;

        // Calculate current square speed
        float sqrSpeed = currentVelocity.sqrMagnitude;
        // Direction attempting to run in
        Vector3 runDirection = input.run * Vector3.ProjectOnPlane(motor.CharacterRight, motor.GetEffectiveGroundNormal()).normalized * (values.invertRight > 0 ? -1 : +1);
        
        float autoRunKickOffBuffer = 0.25f;
        // If manual run kick off was activated successfully
        if (input.doubleTapRun && sqrSpeed < (values.runKickOffSpeed-autoRunKickOffBuffer) * (values.runKickOffSpeed-autoRunKickOffBuffer))
            RunKickOff(ref currentVelocity, runDirection, physicsNegations, false);
        // If automatic run kick off was triggered
        else if (sqrSpeed == 0 && Vector3.Dot(runDirection, gravityDirection) < 0 && Vector3.Angle(motor.GetEffectiveGroundNormal(), -gravityDirection) >= values.autoRunKickOffSlopeThreshold)
            RunKickOff(ref currentVelocity, runDirection, physicsNegations, true);
        else
        {
            // If running against velocity
            if (Vector3.Dot(currentVelocity, runDirection) < 0)
            {
                // Brake via deceleration
                currentVelocity += runDirection * values.brakeDecel * deltaTime;
            }
            else
            {
                // If possible run along ground via acceleration
                if (sqrSpeed < values.runMaxSpeed * values.runMaxSpeed)
                    currentVelocity += runDirection * values.runAccel * deltaTime;

                float faceDir = Mathf.Sign(input.run) * (values.invertRight > 0 ? -1 : +1);
                if (faceDir != facingDirection)
                {
                    facingDirection = faceDir;
                    facingDirectionChanged?.Invoke(this, EventArgs.Empty);
                }                
            }
        }
    }
    
    /// <summary>
    /// Activates run kick off
    /// </summary>
    /// <param name="currentVelocity"> Reference to the player's velocity </param>
    /// <param name="runDirection"> The direction to run kick off in</param>
    /// <param name="physicsOverride"> Determines overrides to player physics values </param>
    /// <param name="auto"> Was the run kick off automatically activated? </param>
    private void RunKickOff(ref Vector3 currentVelocity, Vector3 runDirection, PlayerMovementPhysics.PhysicsNegations physicsNegations, bool auto)
    {
        if (auto)
            physicsNegations.gravityNegated = true;

        // Determine whether to use auto or manula run kick off speed
        float speed = (auto) ? values.autoRunKickOffSpeed : values.runKickOffSpeed;
        
        // Set velocity
        currentVelocity = speed * runDirection;
    }

    /// <summary>
    /// Adds appropriate air move acceleration to the player 
    /// </summary>
    /// <param name="currentVelocity"> Reference to the player's velocity </param>
    /// <param name="motor"> The player's kinematic motor </param>
    /// <param name="gravityDirection"> The direction of gravity </param>
    /// <param name="physicsOverride"> Determines overrides to player physics values </param>
    /// <param name="deltaTime"> Motor update time</param>
    private void AirMove(ref Vector3 currentVelocity, KinematicCharacterMotor motor, Vector3 gravityDirection, WallHits wallHits, ref PlayerMovementPhysics.PhysicsNegations physicsNegations, float deltaTime)
    {
        // The velocity perpendicular to gravity 
        Vector3 flattenedVelocity = Vector3.ProjectOnPlane(currentVelocity, gravityDirection);
        // The squared speed perpendicular to gravity 
        float flattenedSqrSpeed = flattenedVelocity.sqrMagnitude;
        // Direction attempting to move in
        Vector3 airMoveDirection = Vector3.Cross(-gravityDirection, motor.PlanarConstraintAxis) * input.run * (values.invertRight > 0 ? -1 : +1);

        // Ensure drag does not activate
        physicsNegations.airDragNegated = true;

        // Determine which air move values to use
        float airMoveAccel = (flattenedSqrSpeed >= values.airSpeedThreshold * values.airSpeedThreshold) ? values.looseAirMoveAccel : values.preciseAirMoveAccel;
        float airMoveBrakeAccel = (flattenedSqrSpeed >= values.airSpeedThreshold * values.airSpeedThreshold) ? values.looseAirMoveBrakeDecel : values.preciseAirMoveBrakeDecel;

        Vector3 intoWallNormal = Vector3.zero;
        float intoWallDot = 0;
        float dotChecking;
        if (wallHits.hitCeiling != Vector3.zero && (dotChecking = Vector3.Dot(wallHits.hitCeiling, airMoveDirection)) < intoWallDot)
        {
            intoWallNormal = wallHits.hitCeiling;
            intoWallDot = dotChecking;
        }
        if(wallHits.hitRightWall != Vector3.zero && (dotChecking = Vector3.Dot(wallHits.hitRightWall, airMoveDirection)) < intoWallDot)
        {
            intoWallNormal = wallHits.hitRightWall;
            intoWallDot = dotChecking;
        }
        if(wallHits.hitLeftWall != Vector3.zero && (dotChecking = Vector3.Dot(wallHits.hitLeftWall, airMoveDirection)) < intoWallDot)
        {
            intoWallNormal = wallHits.hitLeftWall;
            intoWallDot = dotChecking;
        }
        
        if (intoWallDot == 0 || Vector3.Dot(intoWallNormal, gravityDirection) >= 0) 
        {
            // If moving against horizontal velocity
            if (Vector3.Dot(flattenedVelocity, airMoveDirection) < 0)
            {
                // Brake via deceleration
                currentVelocity += airMoveDirection * airMoveBrakeAccel * deltaTime;
            }
            // If possible move in air via acceleration
            else if (flattenedSqrSpeed < values.airMoveMaxSpeed * values.airMoveMaxSpeed)
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
    private void Jump(ref Vector3 currentVelocity, KinematicCharacterMotor motor)
    {
        // Jump perpendicular to the current slope
        currentVelocity += Vector3.ProjectOnPlane(values.jumpSpeed * motor.GetEffectiveGroundNormal(), motor.PlanarConstraintAxis);

        // Unground the motor
        motor.ForceUnground();

        isJumping = true;
    }

    private void HandleJumpRotation(ref Vector3 internalAngularVelocity)
    {
        internalAngularVelocity *= values.jumpRotationFactor;
    }

    /// <summary>
    /// Checks to see if the player is still moving against gravity due to an activate jump
    /// </summary>
    /// <param name="currentVelocity"> Reference to the player's velocity </param>
    /// <param name="motor"> The player's kinematic motor </param>
    /// <param name="gravityDirection"> The direction of gravity </param>
    /// <returns></returns>
    private bool CheckIsStillJumping(ref Vector3 currentVelocity, KinematicCharacterMotor motor, Vector3 gravityDirection)
    {
        // If the player is mid-air and not falling, return true 
        return !motor.GroundingStatus.IsStableOnGround && Vector3.Dot(currentVelocity, gravityDirection) < 0;
    }

    /// <summary>
    /// Attempts to cancel a jump if activate, ensures attempt will be made again next motor update if unable to
    /// </summary>
    /// <param name="currentVelocity"> Reference to the player's velocity </param>
    /// <param name="motor"> The player's kinematic motor </param>
    /// <param name="gravityDirection"> The direction of gravity </param>
    private void JumpCancel(ref Vector3 currentVelocity, KinematicCharacterMotor motor, Vector3 gravityDirection)
    {
        // Get the velocity in the direction of gravity
        Vector3 velocityAlongGravity = Vector3.Project(currentVelocity, gravityDirection);

        // If jump cancel is valid
        if (velocityAlongGravity.sqrMagnitude <= values.jumpCancelThreshold * values.jumpCancelThreshold)
        {
            // Perform jump cancel
            currentVelocity = (currentVelocity - velocityAlongGravity) + (-gravityDirection * values.jumpCancelSpeed);
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
    /// <param name="motor"> The player's kinematic motor</param>
    /// <param name="deltaTime"> Motor update time</param>
    public void UpdateRotation(ref Quaternion currentRotation, ref Vector3 currentAngularVelocity, KinematicCharacterMotor motor, float deltaTime)
    {
        if(motor.WasGroundedLastUpdate && isJumping && currentAngularVelocity != Vector3.zero)
            HandleJumpRotation(ref currentAngularVelocity);

    }

    /// <summary>
    /// Updates the reference velocity based on intended player actions
    /// </summary>
    /// <param name="currentVelocity"> Reference to the player's velocity</param>
    /// <param name="motor"> The player's kinematic motor</param>
    /// <param name="gravityDirection"> The direction of gravity </param>
    /// <param name="physicsOverride"> Determines overrides to player physics values </param>
    /// <param name="deltaTime"> Motor update time</param>
    public void UpdateVelocity(ref Vector3 currentVelocity, KinematicCharacterMotor motor, Vector3 gravityDirection, WallHits wallHits, ref PlayerMovementPhysics.PhysicsNegations physicsNegations, float deltaTime)
    {
        // If the player is jumping
        if(isJumping && CheckIsStillJumping(ref currentVelocity, motor, gravityDirection))
        {
            // If the player is trying to jump cancel
            if(input.jumpCancel || waitingToJumpCancel)
                JumpCancel(ref currentVelocity, motor, gravityDirection);
        }
        else 
        {   
            // Make sure player isn't still trying to jump cancel
            waitingToJumpCancel = false;
            isJumping = false;
        }

        // if the player is trying to and able to jump
        if(input.jump && motor.IsGroundedThisUpdate)
            Jump(ref currentVelocity, motor);

        // if the player is trying to run
        if(input.run != 0)
        {
            if (motor.IsGroundedThisUpdate)
                Run(ref currentVelocity, motor, gravityDirection, ref physicsNegations, deltaTime);
            else
                AirMove(ref currentVelocity, motor, gravityDirection, wallHits, ref physicsNegations, deltaTime);
        }
    }

    /// <summary>
    /// Gather input this Monobehavior.Update()
    /// </summary>
    public void RegisterInput(PlayerController.PlayerActions controllerActions)
    {
        input.RegisterInput(controllerActions);
    }

    /// <summary>
    /// Resets the input state
    /// </summary>
    public void ResetInput()
    {
        input.Reset();
    }

}