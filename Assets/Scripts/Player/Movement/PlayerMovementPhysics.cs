using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KinematicCharacterController;

/// <summary>
/// Handles application of physics mechanics on the player
/// </summary>
public class PlayerMovementPhysics : MonoBehaviour
{

    /// <summary>
    /// Contains information on potential update specific negations to physics values/mechanics
    /// </summary>
    public struct PhysicsNegations
    {    
        /// <summary>
        /// Are kinetic and static friction negated?
        /// </summary>
        public bool kineticAndStaticFrictionNegated;
        /// <summary>
        /// is air drag negated?
        /// </summary>
        public bool airDragNegated;
        /// <summary>
        /// Is gravity negated?
        /// </summary>
        public bool gravityNegated;
        
        /// <summary>
        /// The overridden value of gravity
        /// </summary>
        public Vector3 gravityOverride;

        /// <summary>
        /// Resets the struct to default values
        /// </summary>
        public void Reset()
        {
            kineticAndStaticFrictionNegated = false;
            airDragNegated = false;
            gravityNegated = false;
        }
    }

    /// <summary>
    /// The deceleration of kinetic friction on the player when they are not actively running along a ground
    /// </summary>
    [SerializeField] private float kineticFriction;
    /// <summary>
    /// The max slope where static friction can be active
    /// </summary>
    [SerializeField] private float staticFrictionMaxSlope;
    /// <summary>
    /// The minimum speed required to overcome static friction
    /// </summary>
    [SerializeField] private float staticFrictionVelThreshold;
    /// <summary>
    /// The extra non-overridable deceleration of kinetic friction on the player when they pass a max speed
    /// </summary>
    [SerializeField] private float extraKineticFriction;
    /// <summary>
    /// The minimum speed required for extraKineticFriction to activate
    /// </summary>
    [SerializeField] private float extraKineticFrictionSpeedThreshold;
    /// <summary>
    /// The max extra non-overridable decceleration of kinetic friction to a player if they are running over a 90 degree angle.
    /// Can be considered the player's inability to maintain speed when running upside down
    /// </summary>
    [SerializeField] private float upsideDownExtraKineticFriction;
    /// <summary>
    /// The factor of gravity when running downhill
    /// </summary>
    [SerializeField] private float slopeConstantDown;
    /// <summary>
    /// The factor of gravity when running uphill
    /// </summary>
    [SerializeField] private float slopeConstantUp;
    /// <summary>
    /// The deceleration of air drag on the player when they are not activily moving perpendicular to gravity 
    /// </summary>
    [SerializeField] private float airDrag;
    /// <summary>
    /// The extra non-overridable deceleration of air drag on the player when they pass a max speed
    /// </summary>
    [SerializeField] private float extraAirDrag;
    /// <summary>
    /// The minimum speed required for extraAirDrag to activate
    /// </summary>
    [SerializeField] private float extraAirDragSpeedThreshold;

    /// <summary>
    /// The acceleration of gravity on the player
    /// </summary>
    [SerializeField] private Vector3 gravity;

    /// <summary>
    /// The current direction of gravity
    /// </summary>
    public Vector3 gravityDirection { get { return gravity.normalized; } }

    /// <summary>
    /// The currently overriden physics values for the current motor update
    /// </summary>
    public PhysicsNegations negations;

    /// <summary>
    /// Initializes script
    /// </summary>
    void Awake()
    {
        negations = new PhysicsNegations();
    }

    /// <summary>
    /// Sets default values
    /// </summary>
    void Reset()
    {
        kineticFriction = 9;
        staticFrictionMaxSlope = 30;
        staticFrictionVelThreshold = 0.5f;
        extraKineticFriction = 2;
        extraKineticFrictionSpeedThreshold = 18;
        upsideDownExtraKineticFriction = 9;
        slopeConstantDown = 1.5f;
        slopeConstantUp = 0.5f;
        airDrag = 1;
        extraAirDrag = 1;
        extraAirDragSpeedThreshold = 18;
        gravity = Vector3.down * 36;
    }
    
    // for debugging
    public void SetDebugGravity(Vector3 down)
    {
        gravity = down * gravity.magnitude;
    }

    /// <summary>
    /// Adds gravitational acceleration onto the player
    /// </summary>
    /// <param name="currentVelocity"> Reference to the player's velocity</param>
    /// <param name="motor"> The player's kinematic motor</param>
    /// <param name="deltaTime"> Motor update time</param>
    private void AddGravity(ref Vector3 currentVelocity, KinematicCharacterMotor motor, float deltaTime)
    {
        if (motor.IsGroundedThisUpdate)
        {
            // Get appropriate gravity factor for either up hill or down hill slope
            float slopeConstant = 
                (Vector3.Dot(currentVelocity, gravity) >= 0)
                ? slopeConstantDown : slopeConstantUp;
            
            // Apply grounded gravity
            currentVelocity += Vector3.ProjectOnPlane(gravity * slopeConstant, motor.GetEffectiveGroundNormal()) * deltaTime;
        }
        else 
            // Apply aerial (standard) gravity
            currentVelocity += gravity * deltaTime;
    }

    /// <summary>
    /// Adds kinetic frictional deceleration onto the player
    /// </summary>
    /// <param name="currentVelocity"> Reference to the player's velocity</param>
    /// <param name="deltaTime"> Motor update time</param>
    private void AddKineticFriction(ref Vector3 currentVelocity, float deltaTime)
    {        
        // Apply kinetic friction ensuring velocity doesn't invert direction, stopping at 0
        if (currentVelocity.sqrMagnitude <= (kineticFriction * kineticFriction * deltaTime * deltaTime))
        {
            ActivateStaticFriction(ref currentVelocity);
        }
        else
            currentVelocity -= currentVelocity.normalized * kineticFriction * deltaTime;

    }

    /// <summary>
    /// Adds extra kinetic frictional deceleration to the player
    /// </summary>
    /// <param name="currentVelocity"> Reference to the player's velocity</param>
    /// <param name="deltaTime"> Motor update time</param>
    private void AddExtraKineticFriction(ref Vector3 currentVelocity, float deltaTime)
    {
        // Apply extra kinetic friction
        currentVelocity -= currentVelocity.normalized * extraKineticFriction * deltaTime;
    }

    /// <summary>
    /// Adds upside-down extra kinetic frictional deceleration onto the player
    /// </summary>
    /// <param name="currentVelocity"> Reference to the player's velocity</param>
    /// <param name="deltaTime"> Motor update time</param>
    private void AddUpsideDownExtraKineticFriction(ref Vector3 currentVelocity, float slopeAngle, float deltaTime)
    {
        // Used as a factor for strength of friction based on how "upside-down" the ground is
        float ratio = (slopeAngle - 90) / 90;

        // Apply upside-down extra kinetic friction
        currentVelocity -= currentVelocity.normalized * upsideDownExtraKineticFriction * ratio * deltaTime;
    }
    
    /// <summary>
    /// Adds a static frictional effect onto the player
    /// </summary>
    /// <param name="currentVelocity"> Reference to the player's velocity</param>
    private void ActivateStaticFriction(ref Vector3 currentVelocity)
    {
        // Apply Static Friction
        currentVelocity = Vector3.zero;

        // Ensure gravity does not effect velocity
        negations.gravityNegated = true;
    }

    /// <summary>
    /// Adds the deceleration of air drag onto the player
    /// </summary>
    /// <param name="currentVelocity"> Reference to the player's velocity</param>
    /// <param name="flattenedVelocity"> The velocity perpendicular to gravity</param>
    /// <param name="deltaTime"> Motor update time</param>
    private void AddAirDrag(ref Vector3 currentVelocity, Vector3 flattenedVelocity, float deltaTime)
    {
        // Apply air drag ensuring flattened velocity doesn't invert direction, stopping at 0
        if (flattenedVelocity.sqrMagnitude <= airDrag * airDrag * deltaTime * deltaTime)
            currentVelocity -= flattenedVelocity;
        else
            currentVelocity -= flattenedVelocity.normalized * airDrag * deltaTime;
    }

    /// <summary>
    /// Adds the deceleration of extra air drag to the player
    /// </summary>
    /// <param name="currentVelocity"> Reference to the player's velocity</param>
    /// <param name="flattenedVelocity"> The velocity perpendicular to gravity</param>
    /// <param name="deltaTime"> Motor update time</param>
    private void AddExtraAirDrag(ref Vector3 currentVelocity, Vector3 flattenedVelocity, float deltaTime)
    {
        // Apply extra air drag
        currentVelocity -= flattenedVelocity.normalized * extraAirDrag * deltaTime;
    }

    /// <summary>
    /// Updates the reference rotation based on physics calculations
    /// </summary>
    /// <param name="currentVelocity"> Reference to the player's velocity</param>
    /// <param name="motor"> The player's kinematic motor</param>
    /// <param name="deltaTime"> Motor update time</param>
    public void UpdateRotation(ref Quaternion currentRotation, KinematicCharacterMotor motor, float deltaTime)
    {
        // No necessary calculations yet
    }

    /// <summary>
    /// Updates the reference velocity based on physics calculations
    /// </summary>
    /// <param name="currentVelocity"> Reference to the player's velocity</param>
    /// <param name="motor"> The player's kinematic motor</param>
    /// <param name="deltaTime"> Motor update time</param>
    public void UpdateVelocity(ref Vector3 currentVelocity, KinematicCharacterMotor motor, float deltaTime)
    {
        if (motor.IsGroundedThisUpdate) {
            
            // Get angle of slope in relation to gravity
            float slopeAngle = Vector3.Angle(motor.GetEffectiveGroundNormal(), -gravity);
            float sqrSpeed = currentVelocity.sqrMagnitude;

            if (!negations.kineticAndStaticFrictionNegated) {

                // If there is static friction to apply
                if (slopeAngle < staticFrictionMaxSlope && sqrSpeed < staticFrictionVelThreshold * staticFrictionVelThreshold)
                    ActivateStaticFriction(ref currentVelocity);

                // If there is kinetic friction to apply
                else if (sqrSpeed > 0 )
                    AddKineticFriction(ref currentVelocity, deltaTime);
            }

            // If there is extra kinetic friction to apply  
            if (sqrSpeed > extraKineticFrictionSpeedThreshold * extraKineticFrictionSpeedThreshold)
                AddExtraKineticFriction(ref currentVelocity, deltaTime);

            // If there is upside-down extra kinetic friction to apply  
            if (Vector3.Dot(motor.GetEffectiveGroundNormal(), gravity) > 0)
                AddUpsideDownExtraKineticFriction(ref currentVelocity, slopeAngle, deltaTime);
        }
        else
        {
            // Velocity perpendicular to gravity
            Vector3 flattenedVelocity;
            if ((flattenedVelocity = Vector3.ProjectOnPlane(currentVelocity, gravity)) != Vector3.zero)
            {
                // If there is air drag to apply
                if (!negations.airDragNegated)
                    AddAirDrag(ref currentVelocity, flattenedVelocity, deltaTime);

                // If there is extra air drag to apply  
                if (flattenedVelocity.sqrMagnitude > extraAirDragSpeedThreshold * extraAirDragSpeedThreshold)
                    AddExtraKineticFriction(ref currentVelocity, deltaTime);
            }
        }

        if(!negations.gravityNegated)
            AddGravity(ref currentVelocity, motor, deltaTime);

        // Reset update based negations
        negations.kineticAndStaticFrictionNegated = false;
        negations.airDragNegated = false;
        negations.gravityNegated = false;
    }
}
