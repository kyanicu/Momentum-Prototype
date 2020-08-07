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
    /// The acceleration of the player when actively moving horizontally in the air under a max speed
    /// </summary>
    [SerializeField] private float slowAirMoveAccel;
    /// <summary>
    /// The decceleration of the player when actively moving horizontally in the air under a max speed against their velocity
    /// </summary>
    [SerializeField] private float slowAirMoveBrakeDecel;
    /// <summary>
    /// The acceleration of the player when actively moving horizontally in the air above a minimum speed
    /// </summary>
    [SerializeField] private float fastAirMoveAccel;
    /// <summary>
    /// The threshold of horizontal air speed to determine which acceleration to use (slow if under, fast if over)
    /// </summary>
    [SerializeField] private float airSpeedThreshold;
    /// <summary>
    /// The max horizontal speed a player can achieve by actively moving in the air alone
    /// </summary>
    [SerializeField] private float airMaxSpeed;
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
        root = transform.parent.Find("Root").gameObject;
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
        slowAirMoveAccel = 14;
        slowAirMoveBrakeDecel = 18;
        fastAirMoveAccel = 30;
        airSpeedThreshold = 18;
        airMaxSpeed = 4;
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
    /// Adds appropriate run ecceleration to the player 
    /// </summary>
    /// <param name="currentVelocity"> Reference to the player's velocity</param>
    /// <param name="motor"> The player's kinematic motor</param>
    /// <param name="gravityDirection"> The direction of gravity</param>
    /// <param name="physicsOverride"> Determines overrides to player physics values </param>
    /// <param name="deltaTime"> Motor update time</param>
    private void Run(ref Vector3 currentVelocity, KinematicCharacterMotor motor, Vector3 gravityDirection, ref PlayerMovementPhysics.PhysicsOverride physicsOverride, float deltaTime)
    {
        if (motor.IsGroundedThisUpdate)
        {
            // Ensure friction does not activate
            physicsOverride.kineticAndStaticFrictionNegated = true;

            // Calculate current speed and intended running direction
            float speed = currentVelocity.magnitude;
            Vector3 runDirection = input.run * Vector3.ProjectOnPlane(motor.CharacterRight, motor.GetEffectiveGroundNormal()).normalized;
            
            if(input.doubleTapRun && speed < runKickOffSpeed)
                RunKickOff(ref currentVelocity, runDirection, false);
            else if (speed < autoRunKickOffSpeed && Vector3.SignedAngle(runDirection, -gravityDirection, motor.PlanarConstraintAxis) >= autoRunKickOffSlopeThreshold)
                RunKickOff(ref currentVelocity, runDirection, true);
            else
            {
                if (Vector3.Dot(currentVelocity, runDirection) < 0)
                {
                    currentVelocity += runDirection * brakeDecel * deltaTime;
                }
                else
                {
                    if (speed >= runMaxSpeed)
                        return;
                    else
                        currentVelocity += runDirection * runAccel * deltaTime;
                    
                    //Temporary, see SetFacingDirection()
                    SetFacingDirection(Mathf.Sign(input.run));
                }
            }
        }
        else 
        {
            Vector3 flattenedVelocity = Vector3.ProjectOnPlane(currentVelocity, gravityDirection);
            float flattenedSpeed = flattenedVelocity.magnitude;
            Vector3 airMoveDirection = Quaternion.FromToRotation(Vector3.forward, motor.PlanarConstraintAxis) * Vector3.right * input.run;
                        
            if (flattenedSpeed >= airSpeedThreshold)
            {
                if (Vector3.Dot(airMoveDirection,flattenedVelocity) > 0)
                    return;
                else
                {
                    physicsOverride.airDragNegated = true;
                    currentVelocity += airMoveDirection * slowAirMoveAccel * deltaTime;
                }
            }
            else if (flattenedSpeed >= airMaxSpeed)
            {
                physicsOverride.airDragNegated = true;
                if (Vector3.Dot(airMoveDirection,flattenedVelocity) < 0)
                    currentVelocity += airMoveDirection * slowAirMoveBrakeDecel * deltaTime;
                else
                    currentVelocity += airMoveDirection * slowAirMoveAccel * deltaTime;
            }
            else
            {
                currentVelocity += airMoveDirection * fastAirMoveAccel * deltaTime;
            }
        }
    }

    
    private void RunKickOff(ref Vector3 currentVelocity, Vector3 runDirection, bool auto)
    {
        float speed;
        if (auto)
            speed = autoRunKickOffSpeed;
        else
            speed = runKickOffSpeed;

            currentVelocity = speed * runDirection;
    }

    private void Jump(ref Vector3 currentVelocity, KinematicCharacterMotor motor, Vector3 gravityDirection)
    {
        if (motor.GroundingStatus.IsStableOnGround && !motor.MustUnground())
        {
            currentVelocity += jumpSpeed * motor.GetEffectiveGroundNormal();
            motor.ForceUnground();
            isJumping = true;
        }
    }

    private bool CheckIsStillJumping(ref Vector3 currentVelocity, KinematicCharacterMotor motor, Vector3 gravityDirection)
    {
        return !(motor.GroundingStatus.IsStableOnGround || Vector3.Dot(currentVelocity, gravityDirection) > 0);
    }

    private void JumpCancel(ref Vector3 currentVelocity, KinematicCharacterMotor motor, Vector3 gravityDirection)
    {

        Vector3 velocityAlongGravity = Vector3.Project(currentVelocity, gravityDirection);

        if (velocityAlongGravity.magnitude <= jumpCancelSpeedThreshold)
        {
            currentVelocity = (currentVelocity - velocityAlongGravity) + (-gravityDirection * jumpCancelSpeed);
            waitingToJumpCancel = false;
        }
        else
            waitingToJumpCancel = true;

    }

    public void UpdateRotation(ref Quaternion currentRotation, KinematicCharacterMotor motor, float deltaTime)
    {
            
    }

    public void UpdateVelocity(ref Vector3 currentVelocity, KinematicCharacterMotor motor, Vector3 gravityDirection, ref PlayerMovementPhysics.PhysicsOverride physicsOverride, float deltaTime)
    {
        if(isJumping)
        {
            if(CheckIsStillJumping(ref currentVelocity, motor, gravityDirection))
            {
                if(waitingToJumpCancel)
                {
                    JumpCancel(ref currentVelocity, motor, gravityDirection);
                }
                else if (input.jumpCancel)
                    JumpCancel(ref currentVelocity, motor, gravityDirection);
            }
            else
                isJumping = false;
        }
        else
            waitingToJumpCancel = false;

        if(input.jump)
            Jump(ref currentVelocity, motor, gravityDirection);
        if(input.run != 0)
            Run(ref currentVelocity, motor, gravityDirection, ref physicsOverride, deltaTime);
    }

    public void ResetInput()
    {
        input.Reset();
    }

}