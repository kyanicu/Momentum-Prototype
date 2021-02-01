using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KinematicCharacterController;
using System;


#region Communication Structs
/// <summary>
/// A wrapper for the PlayerMovementAction class that allows it's state to referenced, but only be read
/// </summary>
public struct ReadOnlyPlayerMovementAction
{
    private PlayerMovementAction action;

    public float facingDirection { get { return action.facingDirection; } }
    public bool isBraking { get { return action.isBraking; } }

    public ReadOnlyPlayerMovementAction(PlayerMovementAction a)
    {  
        action = a;
    }
}   
#endregion

[System.Serializable]
public class PlayerMovementActionValues : PlayerOverridableValues
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

    protected override void SetValueCounts()
    {
        floatValuesCount = 16;
        intValuesCount = 1;
        vector3ValuesCount = 0;
    }

    protected override float GetFloatValue(int i)
    {
        switch (i) 
        {
            case (0) :
                return runAccel;
            case (1) :
                return runKickOffSpeed;
            case (2) :
                return autoRunKickOffSpeed;
            case (3) :
                return autoRunKickOffSlopeThreshold;
            case (4) :
                return runMaxSpeed;
            case (5) :
                return brakeDecel;
            case (6) :
                return looseAirMoveAccel;
            case (7) :
                return looseAirMoveBrakeDecel;
            case (8) :
                return preciseAirMoveAccel;
            case (9) :
                return preciseAirMoveBrakeDecel;
            case (10) :
                return airSpeedThreshold;
            case (11) :
                return airMoveMaxSpeed;
            case (12) :
                return jumpSpeed;
            case (13) :
                return jumpRotationFactor;
            case (14) :
                return jumpCancelSpeed;
            case (15) :
                return jumpCancelThreshold;
            default :
                return 0;
        }
    }
    protected override void SetFloatValue(int i, float value)
    {
        switch (i) 
        {
            case (0) :
                runAccel = value;
                break;
            case (1) :
                runKickOffSpeed = value;
                break;
            case (2) :
                autoRunKickOffSpeed = value;
                break;
            case (3) :
                autoRunKickOffSlopeThreshold = value;
                break;
            case (4) :
                runMaxSpeed = value;
                break;
            case (5) :
                brakeDecel = value;
                break;
            case (6) :
                looseAirMoveAccel = value;
                break;
            case (7) :
                looseAirMoveBrakeDecel = value;
                break;
            case (8) :
                preciseAirMoveAccel = value;
                break;
            case (9) :
                preciseAirMoveBrakeDecel = value;
                break;
            case (10) :
                airSpeedThreshold = value;
                break;
            case (11) :
                airMoveMaxSpeed = value;
                break;
            case (12) :
                jumpSpeed = value;
                break;
            case (13) :
                jumpRotationFactor = value;
                break;
            case (14) :
                jumpCancelSpeed = value;
                break;
            case (15) :
                jumpCancelThreshold = value;
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
                return invertRight;
            default :
                return 0;
        }
    }
    protected override void SetIntValue(int i, int value)
    {
        switch (i) 
        {
            case (0) :
                invertRight = value;
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

/// <summary>
/// Handles application of internally intended actions focused on moving the player
/// </summary>
[System.Serializable]
public class PlayerMovementAction : PlayerOverridableAttribute<PlayerMovementActionValues>, IPlayerMovementActionCommunication
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
        public float run { get { return _run; } set { if (resetRun || _run == 0) _run = value; } }

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

        public void RegisterInput(PlayerController.PlayerActions controllerActions)
    {
        jump = controllerActions.Jump.triggered;

        jumpCancel = controllerActions.JumpCancel.triggered;
        
        run = controllerActions.Run.ReadValue<float>();
        resetRun = false;

        doubleTapRun = controllerActions.RunKickOff.triggered;
    }

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

    #region Events
    public event Action facingDirectionChanged;
    #endregion

    /// <summary>
    /// Holds and maintains input info
    /// </summary>
    private MovementActionInput input;

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

    public float facingDirection { get; private set; }

    public bool isBraking { get; private set; }

    private bool stunned;

    /// <summary>
    /// Constructor
    /// </summary>
    public PlayerMovementAction()
    {
        // Set input values
        input = new MovementActionInput();
        facingDirection = +1;
    }

    public void SetCommunicationInterface(PlayerInternalCommunicator communicator)
    {
        communicator.SetCommunication(this);
    }

    protected override void SetDefaultBaseValues()
    {
        // Set default values
        baseValues.runAccel = 15;
        baseValues.runKickOffSpeed = 15;
        baseValues.autoRunKickOffSpeed = 2;
        baseValues.autoRunKickOffSlopeThreshold = 14;
        baseValues.runMaxSpeed = 18;
        baseValues.brakeDecel = 50;
        baseValues.looseAirMoveAccel = 10;
        baseValues.looseAirMoveBrakeDecel = 15;
        baseValues.preciseAirMoveAccel = 30;
        baseValues.preciseAirMoveBrakeDecel = 70; 
        baseValues.airSpeedThreshold = 12;
        baseValues.airMoveMaxSpeed = 18;
        baseValues.jumpSpeed = 22;
        baseValues.jumpRotationFactor = 2; 
        baseValues.jumpCancelSpeed = 4;
        baseValues.jumpCancelThreshold = 20;
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
                isBraking = true;
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
                    facingDirectionChanged?.Invoke();
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
    private void Jump(ref Vector3 currentVelocity, KinematicCharacterMotor motor, Vector3 groundNormal)
    {
        // Jump perpendicular to the current slope
        currentVelocity += Vector3.ProjectOnPlane(values.jumpSpeed * groundNormal, motor.PlanarConstraintAxis);

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

        if(velocityAlongGravity.sqrMagnitude < values.jumpCancelSpeed * values.jumpCancelSpeed)
            waitingToJumpCancel = false;
        // If jump cancel is valid
        else if (velocityAlongGravity.sqrMagnitude <= values.jumpCancelThreshold * values.jumpCancelThreshold)
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
    public void UpdateVelocity(ref Vector3 currentVelocity, KinematicCharacterMotor motor, Vector3 gravityDirection, WallHits wallHits, ref PlayerMovementPhysics.PhysicsNegations physicsNegations, float groundingBufferTime, Vector3 bufferedUngroundedNormal, float deltaTime)
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

        if(stunned)
            return;

        // if the player is trying to and able to jump
        if(input.jump)
        {
            if (motor.IsGroundedThisUpdate)
                Jump(ref currentVelocity, motor, motor.GetEffectiveGroundNormal());
            else if (bufferedUngroundedNormal != Vector3.zero)
                Jump(ref currentVelocity, motor, bufferedUngroundedNormal);
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
        if(input.run != 0)
        {
            if (motor.IsGroundedThisUpdate)
                Run(ref currentVelocity, motor, gravityDirection, ref physicsNegations, deltaTime);
            else
                AirMove(ref currentVelocity, motor, gravityDirection, wallHits, ref physicsNegations, deltaTime);
        }
    }

    public void StartStun()
    {
        stunned = true;
        Flinch();
        isBraking = false;
    }

    public void EndStun()
    {
        stunned = false;
    }

    public void Flinch()
    {
        if (isJumping)
        {
            input.Reset();
            input.jumpCancel = true;
        }
        else
            input.Reset();
    }
    /*
    private void SetInputJumpTrue()
    {
        input.jump = true;
        input.jumpCancel = bufferingJumpCancel;
    }
    */

    /// <summary>
    /// Gather input this Monobehavior.Update()
    /// </summary>
    public void RegisterInput(PlayerController.PlayerActions controllerActions)
    {
        if (!stunned)
            input.RegisterInput(controllerActions);
    }

    public void ResetInput()
    {
        input.Reset();
    }

}