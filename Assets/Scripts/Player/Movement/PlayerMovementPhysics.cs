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
    /// Contains information on potential overrides to physics values/mechanics
    /// </summary>
    public struct PhysicsOverride
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
        /// Is the value of gravity overridden?
        /// </summary>
        public bool gravityOverridden;
        
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
            gravityOverridden = false;

            gravityOverride = Vector3.zero;
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
    /// The acceleration of gravity on the player
    /// </summary>
    [SerializeField] private Vector3 _gravity;
    /// <summary>
    /// Encapsulated gravity property to ensure active gravity (possibly overridden) is returned
    /// </summary>
    public Vector3 gravity { get { return ((overrides.gravityOverridden) ? overrides.gravityOverride : _gravity); } private set { _gravity = value; } }

    /// <summary>
    /// The currently overriden physics values for the current motor update
    /// </summary>
    public PhysicsOverride overrides;

    /// <summary>
    /// Initializes script
    /// </summary>
    void Awake()
    {
        overrides = new PhysicsOverride();
    }

    /// <summary>
    /// Sets default values
    /// </summary>
    void Reset()
    {
        extraKineticFriction = 2;
        extraKineticFrictionSpeedThreshold = 16;
        kineticFriction = 9;
        staticFrictionMaxSlope = 30;
        staticFrictionVelThreshold = 0.5f;
        upsideDownExtraKineticFriction = 9;
        slopeConstantDown = 1.5f;
        slopeConstantUp = 0.5f;
        airDrag = 1;
        gravity = Vector3.down * 36;
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
            // If slope is a flat floor with respect to gravity
            if (motor.GetEffectiveGroundNormal() == -gravity.normalized)
                return;

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
        if (Mathf.Abs(currentVelocity.magnitude) <= kineticFriction * deltaTime)
        {
            ActivateStaticFriction(ref currentVelocity);
        }
        else
            currentVelocity -= currentVelocity.normalized * kineticFriction * deltaTime;

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
        if (flattenedVelocity.magnitude <= airDrag * deltaTime)
            currentVelocity -= flattenedVelocity;
        else
            currentVelocity += -flattenedVelocity * airDrag * deltaTime;
        
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
        overrides.gravityNegated = true;
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

            if (!overrides.kineticAndStaticFrictionNegated) {

                // If there is static friction to apply
                if (slopeAngle < staticFrictionMaxSlope && currentVelocity.magnitude < staticFrictionVelThreshold)
                    ActivateStaticFriction(ref currentVelocity);

                // If there is kinetic friction to apply
                else if (currentVelocity != Vector3.zero)
                    AddKineticFriction(ref currentVelocity, deltaTime);
            }

            // If there is extra kinetic friction to apply  
            if (currentVelocity.magnitude > extraKineticFrictionSpeedThreshold)
                AddExtraKineticFriction(ref currentVelocity, deltaTime);

            // If there is upside-down extra kinetic friction to apply  
            if (Vector3.Dot(motor.GetEffectiveGroundNormal(), gravity) > 0)
                AddUpsideDownExtraKineticFriction(ref currentVelocity, slopeAngle, deltaTime);
        }
        else
        {
            // Velocity perpendicular to gravity
            Vector3 flattenedVelocity;
            
            // If there is air drag to apply
            if (!overrides.airDragNegated && (flattenedVelocity = Vector3.ProjectOnPlane(currentVelocity, gravity)) != Vector3.zero)
                AddAirDrag(ref currentVelocity, flattenedVelocity, deltaTime);
        }

        if(!overrides.gravityNegated)
            AddGravity(ref currentVelocity, motor, deltaTime);

        // Reset update based negations
        overrides.kineticAndStaticFrictionNegated = false;
        overrides.airDragNegated = false;
        overrides.gravityNegated = false;
    }
}
