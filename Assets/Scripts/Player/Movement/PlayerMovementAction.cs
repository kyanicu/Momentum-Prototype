using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KinematicCharacterController;

/// <summary>
/// Handles application of internally intended actions focused on moving the player
/// </summary>
public class PlayerMovementAction : MonoBehaviour
{

    /// <summary>
    /// Struct that holds information on player input
    /// Ensures any value found on Monobehavior.Update() will be handled when appropriate for the motor without being overwritten by a zeroed value
    /// </summary>
    private struct MovementInput 
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

        /// <summary>
        /// The direction of double tapping, overwritten if opposite direction is pressed
        /// </summary>
        public float doubleTapRunDirection;
        /// <summary>
        /// Time allowed for double tap to be registered
        /// </summary>
        public float doubleTapTime;
        /// <summary>
        /// Are we waiting for a double tap in the same direction?
        /// </summary>
        public bool doubleTapWaiting;
        /// <summary>
        /// The coroutine that handles the double tap values while waiting for a double tap input
        /// </summary>
        private Coroutine doubleTapTimerCoroutine;

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

        /// <summary>
        /// Called on first run input to start waiting for double tap
        /// </summary>
        /// <param name="s">The script that will handle the coroutine</param>
        public void SetDoubleTapDirection(PlayerMovementAction s)
        {
            // Set initial input
            doubleTapRunDirection = run;
            
            // Start waiting for new double tap
            if(doubleTapTimerCoroutine != null)
                s.StopCoroutine(doubleTapTimerCoroutine);
            doubleTapTimerCoroutine = s.StartCoroutine(s.DoubleTapWait());
        }

        /// <summary>
        /// Sets the double tap input if double tap is registered
        /// </summary>
        /// <param name="s">The script that will handle the coroutine</param>
        public void ActivateDoubleTap(PlayerMovementAction s)
        {
            // Trigger double tap input
            doubleTapRun = true;

            // Reset wait values
            doubleTapWaiting = false;
            doubleTapRunDirection = 0;

            // Stop waiting
            if(doubleTapTimerCoroutine != null)
                s.StopCoroutine(doubleTapTimerCoroutine);
        }
    }

    /// <summary>
    /// Holds and maintains input info
    /// </summary>
    private MovementInput input;

    /// <summary>
    /// The acceleration of the player when actively running on the ground
    /// </summary>
    [SerializeField] private float runAccel;
    /// <summary>
    /// The instantaneous speed the player achieves when manually performing a run kick off
    /// </summary>
    [SerializeField] private float runKickOffSpeed;
    /// <summary>
    /// The instantaneous speed the player achieves when automatically performing a run kick off
    /// </summary>
    [SerializeField] private float autoRunKickOffSpeed;
    /// <summary>
    /// The min slope required for auto run kick off to be triggered
    /// </summary>
    [SerializeField] private float autoRunKickOffSlopeThreshold;
    /// <summary>
    /// The max speed a player can achieve by actively running on the ground alone
    /// </summary>
    [SerializeField] private float runMaxSpeed;
    /// <summary>
    /// The deceleration of the player when actively running on the ground against their velocity
    /// </summary>
    [SerializeField] private float brakeDecel;
    /// <summary>
    /// The acceleration of the player when actively moving horizontally in the air over a minimum speed
    /// </summary>
    [SerializeField] private float looseAirMoveAccel;
    /// <summary>
    /// The deceleration of the player when actively moving horizontally in the air over a minimum speed against their velocity
    /// </summary>
    [SerializeField] private float looseAirMoveBrakeDecel;
    /// <summary>
    /// The acceleration of the player when actively moving horizontally in the air under a minimum speed
    /// </summary>
    [SerializeField] private float preciseAirMoveAccel;
    /// <summary>
    /// The deceleration of the player when actively moving horizontally in the air over a minimum speed against their velocity
    /// </summary>
    [SerializeField] private float preciseAirMoveBrakeDecel;
    /// <summary>
    /// The threshold of horizontal air speed to determine which acceleration to use (precise if under, loose if over)
    /// </summary>
    [SerializeField] private float airSpeedThreshold;
    /// <summary>
    /// The max horizontal speed a player can achieve by actively moving in the air alone
    /// </summary>
    [SerializeField] private float airMoveMaxSpeed;
    /// <summary>
    /// The "up" (perpendicular to the current slope) speed at which the player jumps off the ground
    /// </summary>
    [SerializeField] private float jumpSpeed;
    /// <summary>
    /// The "up" (opposite to the direction of gravity) speed the the player is set at when jump canceling
    /// </summary>
    [SerializeField] private float jumpCancelSpeed;
    /// <summary>
    /// The maximum "up" (opposite to the direction of gravity) speed allowed for jump canceling to activate 
    /// </summary>
    [SerializeField] private float jumpCancelSpeedThreshold;

    /// <summary>
    /// Is the player currently moving "up" due to an initiated jump?
    /// </summary>
    private bool isJumping;

    /// <summary>
    /// Is the player waiting for a jump cancel to activate ?
    /// </summary>
    private bool waitingToJumpCancel;

    // Temporary, see SetFacingDirection()
    private float facingDirection;
    private GameObject root;

    /// <summary>
    /// Initialize script
    /// </summary>
    void Awake()
    {
        // Set input values
        input = new MovementInput();
        input.doubleTapTime = 0.5f;
        
        //Temporary, see SetFacingDirection()
        #region Temporary
        root = transform.parent.Find("Model Root").gameObject;
        facingDirection = +1;
        #endregion
    }

    /// <summary>
    /// Set default values 
    /// </summary>
    void Reset()
    {
        runAccel = 10;
        runKickOffSpeed = 5;
        autoRunKickOffSpeed= 2;
        autoRunKickOffSlopeThreshold = 14;
        runMaxSpeed = 18;
        brakeDecel = 40;
        looseAirMoveAccel = 14;
        looseAirMoveBrakeDecel = 18;
        preciseAirMoveAccel = 30;
        preciseAirMoveBrakeDecel = 30;
        airMoveMaxSpeed = 18;
        airSpeedThreshold = 4;
        jumpSpeed = 15;
        jumpCancelSpeed = 4;
        jumpCancelSpeedThreshold = 15;
    }

    /// <summary>
    /// Gather input this Monobehavior.Update()
    /// </summary>
    public void RegisterInput()
    {
        input.jump = Input.GetKeyDown(KeyCode.Space);

        input.jumpCancel = Input.GetKeyUp(KeyCode.Space);
        
        float run = 0;
        if (Input.GetKey(KeyCode.A))
            run += -1;
        if (Input.GetKey(KeyCode.D))
            run += +1;
        input.run = run;

        // Handle double tap input
        
        // If a run input was just pressed
        if (run != 0 && (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.A)))
        {
            // If input was expected for a double tap
            if(input.doubleTapWaiting && input.doubleTapRunDirection == input.run)
                input.ActivateDoubleTap(this);
            else
            {
                input.SetDoubleTapDirection(this);
            }
        }
    }
    
    /// <summary>
    /// Coroutine that handles waiting for a double tap input 
    /// </summary>
    public IEnumerator DoubleTapWait()
    {
        // Start waiting
        input.doubleTapWaiting = true;
        yield return new WaitForSeconds(input.doubleTapTime);

        // End waiting
        input.doubleTapWaiting = false;
        input.doubleTapRunDirection = 0;
    }

    // Temporary until better design is decided
    #region Temporary
    private void SetFacingDirection(float direction)
    {
        if (direction != facingDirection)
        {
            facingDirection = direction;
            root.transform.Rotate(Vector3.up*180);
        }
    }
    #endregion

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
        Vector3 runDirection = input.run * Vector3.ProjectOnPlane(motor.CharacterRight, motor.GetEffectiveGroundNormal());
        
        float autoRunKickOffBuffer = 0.25f;
        // If manual run kick off was activated successfully
        if (input.doubleTapRun && sqrSpeed < (runKickOffSpeed-autoRunKickOffBuffer) * (runKickOffSpeed-autoRunKickOffBuffer))
            RunKickOff(ref currentVelocity, runDirection, physicsNegations, false);
        // If automatic run kick off was triggered
        else if (sqrSpeed == 0 && Vector3.Dot(runDirection, gravityDirection) < 0 && Vector3.Angle(motor.GetEffectiveGroundNormal(), -gravityDirection) >= autoRunKickOffSlopeThreshold)
            RunKickOff(ref currentVelocity, runDirection, physicsNegations, true);
        else
        {
            // If running against velocity
            if (Vector3.Dot(currentVelocity, runDirection) < 0)
            {
                // Brake via deceleration
                currentVelocity += runDirection * brakeDecel * deltaTime;
            }
            // If possible run along ground via acceleration
            else if (sqrSpeed < runMaxSpeed * runMaxSpeed)
            {
                currentVelocity += runDirection * runAccel * deltaTime;
            
                #region temporary
                //Temporary, see SetFacingDirection()
                SetFacingDirection(Mathf.Sign(input.run));
                #endregion
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
        float speed = (auto) ? autoRunKickOffSpeed : runKickOffSpeed;
        
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
    private void AirMove(ref Vector3 currentVelocity, KinematicCharacterMotor motor, Vector3 gravityDirection, ref PlayerMovementPhysics.PhysicsNegations physicsNegations, float deltaTime)
    {
        // The velocity perpendicular to gravity 
        Vector3 flattenedVelocity = Vector3.ProjectOnPlane(currentVelocity, gravityDirection);
        // The squared speed perpendicular to gravity 
        float flattenedSqrSpeed = flattenedVelocity.sqrMagnitude;
        // Direction attempting to move in
        Vector3 airMoveDirection = Vector3.Cross(-gravityDirection, motor.PlanarConstraintAxis) * input.run;

        // Ensure drag does not activate
        physicsNegations.airDragNegated = true;

        // Determine which air move values to use
        float airMoveAccel = (flattenedSqrSpeed >= airSpeedThreshold * airSpeedThreshold) ? looseAirMoveAccel : preciseAirMoveAccel;
        float airMoveBrakeAccel = (flattenedSqrSpeed >= airSpeedThreshold * airSpeedThreshold) ? looseAirMoveBrakeDecel : preciseAirMoveBrakeDecel;

        // If moving against horizontal velocity
        if (Vector3.Dot(flattenedVelocity, airMoveDirection) < 0)
        {
            // Brake via deceleration
            currentVelocity += airMoveDirection * airMoveBrakeAccel * deltaTime;
        }
        // If possible move in air via acceleration
        else if (flattenedSqrSpeed < airMoveMaxSpeed * airMoveMaxSpeed)
        {
            currentVelocity += airMoveDirection * airMoveAccel * deltaTime;
        }
    }

    /// <summary>
    /// Activates jump
    /// </summary>
    /// <param name="currentVelocity"> Reference to the player's velocity </param>
    /// <param name="motor"> The player's kinematic motor </param>
    private void Jump(ref Vector3 currentVelocity, KinematicCharacterMotor motor)
    {
        // Jump perpendicular to the current slope
        currentVelocity += jumpSpeed * motor.GetEffectiveGroundNormal();

        // Unground the motor
        motor.ForceUnground();

        isJumping = true;
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
        if (velocityAlongGravity.sqrMagnitude <= jumpCancelSpeedThreshold * jumpCancelSpeedThreshold)
        {
            // Perform jump cancel
            currentVelocity = (currentVelocity - velocityAlongGravity) + (-gravityDirection * jumpCancelSpeed);
            waitingToJumpCancel = false;
        }
        else // Remember to check next motor update
            waitingToJumpCancel = true;
    }

    /// <summary>
    /// Updates the reference rotation based on intended player actions
    /// </summary>
    /// <param name="currentVelocity"> Reference to the player's velocity</param>
    /// <param name="motor"> The player's kinematic motor</param>
    /// <param name="deltaTime"> Motor update time</param>
    public void UpdateRotation(ref Quaternion currentRotation, KinematicCharacterMotor motor, float deltaTime)
    {
            
    }

    /// <summary>
    /// Updates the reference velocity based on intended player actions
    /// </summary>
    /// <param name="currentVelocity"> Reference to the player's velocity</param>
    /// <param name="motor"> The player's kinematic motor</param>
    /// <param name="gravityDirection"> The direction of gravity </param>
    /// <param name="physicsOverride"> Determines overrides to player physics values </param>
    /// <param name="deltaTime"> Motor update time</param>
    public void UpdateVelocity(ref Vector3 currentVelocity, KinematicCharacterMotor motor, Vector3 gravityDirection, ref PlayerMovementPhysics.PhysicsNegations physicsNegations, float deltaTime)
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
                AirMove(ref currentVelocity, motor, gravityDirection, ref physicsNegations, deltaTime);
        }
    }

    /// <summary>
    /// Resets the input state
    /// </summary>
    public void ResetInput()
    {
        input.Reset();
    }

}