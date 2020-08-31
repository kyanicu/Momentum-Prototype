using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KinematicCharacterController;

[System.Serializable]
public class PlayerMovementPhysicsValues : PlayerMovementOverridableValues
{
    /// <summary>
    /// The deceleration of kinetic friction on the player when they are not actively running along a ground
    /// </summary>
    public float kineticFriction;
    /// <summary>
    /// The max slope where static friction can be active
    /// </summary>
    public float staticFrictionMaxSlope;
    /// <summary>
    /// The minimum speed required to overcome static friction
    /// </summary>
    public float staticFrictionVelThreshold;
    /// <summary>
    /// The extra non-overridable deceleration of kinetic friction on the player when they pass a max speed
    /// </summary>
    public float extraKineticFriction;
    /// <summary>
    /// The minimum speed required for extraKineticFriction to activate
    /// </summary>
    public float extraKineticFrictionSpeedThreshold;
    /// <summary>
    /// The max extra non-overridable decceleration of kinetic friction to a player if they are running over a 90 degree angle along the plane of gravity.
    /// Can be considered the player's inability to maintain speed when running upside down
    /// </summary>
    public float upsideDownExtraKineticFriction;
    /// <summary>
    /// The max extra non-overridable decceleration of kinetic friction to a player if they are running not parallel to the plane of gravity.
    /// Can be considered the player's inability to maintain speed when running along but not up a slant
    /// </summary>
    public float sidewaysExtraKineticFriction;
    /// <summary>
    /// The factor of gravity when running downhill
    /// </summary>
    public float slopeConstantDown;
    /// <summary>
    /// The factor of gravity when running uphill
    /// </summary>
    public float slopeConstantUp;
    /// <summary>
    /// The deceleration of air drag on the player when they are not activily moving perpendicular to gravity 
    /// </summary>
    public float airDrag;
    /// <summary>
    /// The extra non-overridable deceleration of air drag on the player when they pass a max speed
    /// </summary>
    public float extraAirDrag;
    /// <summary>
    /// The minimum speed required for extraAirDrag to activate
    /// </summary>
    public float extraAirDragSpeedThreshold;
    /// <summary>
    /// The max velocity along gravity the player can have
    /// </summary>
    public float terminalVelocity;
    /// <summary>
    /// The acceleration at which the character decelerations when they pass terminal velocity
    /// </summary>
    public float terminalVelocityDeceleration;
    /// <summary>
    /// The accelerative strength of gravity on the player
    /// </summary>
    public float gravityAccel;
    /// <summary>
    /// The current direction of gravity
    /// </summary>
    //[SerializeField]
    public /*private*/ Vector3 gravityDirection;
    //public Vector3 gravityDirection { get { return _gravityDirection; } set { _gravityDirection = value;}}//(!float.IsInfinity(_gravityDirection.x)) ? value.normalized : value; } } 

    /// <summary>
    /// A constance acceleratice force on the player
    /// </summary>
    public Vector3 constantAcceleration;

    /// <summary>
    /// Shorthand for (gravityDirection * gravityAccel) to get the true value of gravity
    /// </summary>
    public Vector3 gravity { get { return gravityDirection * gravityAccel; } }

    public override void SetDefaultValues(PlayerMovementOverrideType overrideType)
    {
        kineticFriction = DefaultFloat(overrideType);
        staticFrictionMaxSlope = DefaultFloat(overrideType);
        staticFrictionVelThreshold = DefaultFloat(overrideType);
        extraKineticFriction = DefaultFloat(overrideType);
        extraKineticFrictionSpeedThreshold = DefaultFloat(overrideType);
        upsideDownExtraKineticFriction = DefaultFloat(overrideType);
        sidewaysExtraKineticFriction = DefaultFloat(overrideType);
        slopeConstantDown = DefaultFloat(overrideType);
        slopeConstantUp = DefaultFloat(overrideType);
        airDrag = DefaultFloat(overrideType);
        extraAirDrag = DefaultFloat(overrideType);
        extraAirDragSpeedThreshold = DefaultFloat(overrideType);
        terminalVelocity = DefaultFloat(overrideType);
        terminalVelocityDeceleration = DefaultFloat(overrideType);
        gravityAccel = DefaultFloat(overrideType);
        gravityDirection = DefaultVector3(overrideType);
        constantAcceleration = DefaultVector3(overrideType);
    }
    
    public override void AddBy(PlayerMovementOverridableValues ov) 
    {
        PlayerMovementPhysicsValues v = ov as PlayerMovementPhysicsValues;


        kineticFriction = Add(kineticFriction, v.kineticFriction);
        staticFrictionMaxSlope = Add(staticFrictionMaxSlope, v.staticFrictionMaxSlope);
        staticFrictionVelThreshold = Add(staticFrictionVelThreshold, v.staticFrictionVelThreshold);
        extraKineticFriction = Add(extraKineticFriction, v.extraKineticFriction);
        extraKineticFrictionSpeedThreshold = Add(extraKineticFrictionSpeedThreshold, v.extraKineticFrictionSpeedThreshold);
        upsideDownExtraKineticFriction = Add(upsideDownExtraKineticFriction, v.upsideDownExtraKineticFriction);
        sidewaysExtraKineticFriction = Add(sidewaysExtraKineticFriction, v.sidewaysExtraKineticFriction);
        slopeConstantDown = Add(slopeConstantDown, v.slopeConstantDown);
        slopeConstantUp = Add(slopeConstantUp, v.slopeConstantUp);
        airDrag = Add(airDrag, v.airDrag);
        extraAirDrag = Add(extraAirDrag, v.extraAirDrag);
        extraAirDragSpeedThreshold = Add(extraAirDragSpeedThreshold, v.extraAirDragSpeedThreshold);
        terminalVelocity = Add(terminalVelocity, v.terminalVelocity);
        terminalVelocityDeceleration = Add(terminalVelocityDeceleration, v.terminalVelocityDeceleration);
        gravityAccel = Add(gravityAccel, v.gravityAccel);
        gravityDirection = Add(gravityDirection, v.gravityDirection);
        constantAcceleration = Add(constantAcceleration, v.constantAcceleration);
    }

    public override void SubtractBy(PlayerMovementOverridableValues ov) 
    {
        PlayerMovementPhysicsValues v = ov as PlayerMovementPhysicsValues;

        kineticFriction = Subtract(kineticFriction, v.kineticFriction);
        staticFrictionMaxSlope = Subtract(staticFrictionMaxSlope, v.staticFrictionMaxSlope);
        staticFrictionVelThreshold = Subtract(staticFrictionVelThreshold, v.staticFrictionVelThreshold);
        extraKineticFriction = Subtract(extraKineticFriction, v.extraKineticFriction);
        extraKineticFrictionSpeedThreshold = Subtract(extraKineticFrictionSpeedThreshold, v.extraKineticFrictionSpeedThreshold);
        upsideDownExtraKineticFriction = Subtract(upsideDownExtraKineticFriction, v.upsideDownExtraKineticFriction);
        sidewaysExtraKineticFriction = Subtract(sidewaysExtraKineticFriction, v.sidewaysExtraKineticFriction);
        slopeConstantDown = Subtract(slopeConstantDown, v.slopeConstantDown);
        slopeConstantUp = Subtract(slopeConstantUp, v.slopeConstantUp);
        airDrag = Subtract(airDrag, v.airDrag);
        extraAirDrag = Subtract(extraAirDrag, v.extraAirDrag);
        extraAirDragSpeedThreshold = Subtract(extraAirDragSpeedThreshold, v.extraAirDragSpeedThreshold);
        terminalVelocity = Subtract(terminalVelocity, v.terminalVelocity);
        terminalVelocityDeceleration = Subtract(terminalVelocityDeceleration, v.terminalVelocityDeceleration);
        gravityAccel = Subtract(gravityAccel, v.gravityAccel);
        gravityDirection = Subtract(gravityDirection, v.gravityDirection);
        constantAcceleration = Subtract(constantAcceleration, v.constantAcceleration);
    }

    public override void MultiplyBy(PlayerMovementOverridableValues ov) 
    {
        PlayerMovementPhysicsValues v = ov as PlayerMovementPhysicsValues;

        kineticFriction = Multiply(kineticFriction, v.kineticFriction);
        staticFrictionMaxSlope = Multiply(staticFrictionMaxSlope, v.staticFrictionMaxSlope);
        staticFrictionVelThreshold = Multiply(staticFrictionVelThreshold, v.staticFrictionVelThreshold);
        extraKineticFriction = Multiply(extraKineticFriction, v.extraKineticFriction);
        extraKineticFrictionSpeedThreshold = Multiply(extraKineticFrictionSpeedThreshold, v.extraKineticFrictionSpeedThreshold);
        upsideDownExtraKineticFriction = Multiply(upsideDownExtraKineticFriction, v.upsideDownExtraKineticFriction);
        sidewaysExtraKineticFriction = Multiply(sidewaysExtraKineticFriction, v.sidewaysExtraKineticFriction);
        slopeConstantDown = Multiply(slopeConstantDown, v.slopeConstantDown);
        slopeConstantUp = Multiply(slopeConstantUp, v.slopeConstantUp);
        airDrag = Multiply(airDrag, v.airDrag);
        extraAirDrag = Multiply(extraAirDrag, v.extraAirDrag);
        extraAirDragSpeedThreshold = Multiply(extraAirDragSpeedThreshold, v.extraAirDragSpeedThreshold);
        terminalVelocity = Multiply(terminalVelocity, v.terminalVelocity);
        terminalVelocityDeceleration = Multiply(terminalVelocityDeceleration, v.terminalVelocityDeceleration);
        gravityAccel = Multiply(gravityAccel, v.gravityAccel);
        gravityDirection = Multiply(gravityDirection, v.gravityDirection);
        constantAcceleration = Multiply(constantAcceleration, v.constantAcceleration);
    }

    public override void DivideBy(PlayerMovementOverridableValues ov) 
    {
        PlayerMovementPhysicsValues v = ov as PlayerMovementPhysicsValues;

        kineticFriction = Divide(kineticFriction, v.kineticFriction);
        staticFrictionMaxSlope = Divide(staticFrictionMaxSlope, v.staticFrictionMaxSlope);
        staticFrictionVelThreshold = Divide(staticFrictionVelThreshold, v.staticFrictionVelThreshold);
        extraKineticFriction = Divide(extraKineticFriction, v.extraKineticFriction);
        extraKineticFrictionSpeedThreshold = Divide(extraKineticFrictionSpeedThreshold, v.extraKineticFrictionSpeedThreshold);
        upsideDownExtraKineticFriction = Divide(upsideDownExtraKineticFriction, v.upsideDownExtraKineticFriction);
        sidewaysExtraKineticFriction = Divide(sidewaysExtraKineticFriction, v.sidewaysExtraKineticFriction);
        slopeConstantDown = Divide(slopeConstantDown, v.slopeConstantDown);
        slopeConstantUp = Divide(slopeConstantUp, v.slopeConstantUp);
        airDrag = Divide(airDrag, v.airDrag);
        extraAirDrag = Divide(extraAirDrag, v.extraAirDrag);
        extraAirDragSpeedThreshold = Divide(extraAirDragSpeedThreshold, v.extraAirDragSpeedThreshold);
        terminalVelocity = Divide(terminalVelocity, v.terminalVelocity);
        terminalVelocityDeceleration = Divide(terminalVelocityDeceleration, v.terminalVelocityDeceleration);
        gravityAccel = Divide(gravityAccel, v.gravityAccel);
        gravityDirection = Divide(gravityDirection, v.gravityDirection);
        constantAcceleration = Divide(constantAcceleration, v.constantAcceleration);
    }

    public override void OrBy(PlayerMovementOverridableValues ov) 
    {
        PlayerMovementPhysicsValues v = ov as PlayerMovementPhysicsValues;

        kineticFriction = Or(kineticFriction, v.kineticFriction);
        staticFrictionMaxSlope = Or(staticFrictionMaxSlope, v.staticFrictionMaxSlope);
        staticFrictionVelThreshold = Or(staticFrictionVelThreshold, v.staticFrictionVelThreshold);
        extraKineticFriction = Or(extraKineticFriction, v.extraKineticFriction);
        extraKineticFrictionSpeedThreshold = Or(extraKineticFrictionSpeedThreshold, v.extraKineticFrictionSpeedThreshold);
        upsideDownExtraKineticFriction = Or(upsideDownExtraKineticFriction, v.upsideDownExtraKineticFriction);
        sidewaysExtraKineticFriction = Or(sidewaysExtraKineticFriction, v.sidewaysExtraKineticFriction);
        slopeConstantDown = Or(slopeConstantDown, v.slopeConstantDown);
        slopeConstantUp = Or(slopeConstantUp, v.slopeConstantUp);
        airDrag = Or(airDrag, v.airDrag);
        extraAirDrag = Or(extraAirDrag, v.extraAirDrag);
        extraAirDragSpeedThreshold = Or(extraAirDragSpeedThreshold, v.extraAirDragSpeedThreshold);
        terminalVelocity = Or(terminalVelocity, v.terminalVelocity);
        terminalVelocityDeceleration = Or(terminalVelocityDeceleration, v.terminalVelocityDeceleration);
        gravityAccel = Or(gravityAccel, v.gravityAccel);
        gravityDirection = Or(gravityDirection, v.gravityDirection);
        constantAcceleration = Or(constantAcceleration, v.constantAcceleration);
    }

    public override void AndBy(PlayerMovementOverridableValues ov) 
    {
        PlayerMovementPhysicsValues v = ov as PlayerMovementPhysicsValues;

        kineticFriction = And(kineticFriction, v.kineticFriction);
        staticFrictionMaxSlope = And(staticFrictionMaxSlope, v.staticFrictionMaxSlope);
        staticFrictionVelThreshold = And(staticFrictionVelThreshold, v.staticFrictionVelThreshold);
        extraKineticFriction = And(extraKineticFriction, v.extraKineticFriction);
        extraKineticFrictionSpeedThreshold = And(extraKineticFrictionSpeedThreshold, v.extraKineticFrictionSpeedThreshold);
        upsideDownExtraKineticFriction = And(upsideDownExtraKineticFriction, v.upsideDownExtraKineticFriction);
        sidewaysExtraKineticFriction = And(sidewaysExtraKineticFriction, v.sidewaysExtraKineticFriction);
        slopeConstantDown = And(slopeConstantDown, v.slopeConstantDown);
        slopeConstantUp = And(slopeConstantUp, v.slopeConstantUp);
        airDrag = And(airDrag, v.airDrag);
        extraAirDrag = And(extraAirDrag, v.extraAirDrag);
        extraAirDragSpeedThreshold = And(extraAirDragSpeedThreshold, v.extraAirDragSpeedThreshold);
        terminalVelocity = And(terminalVelocity, v.terminalVelocity);
        terminalVelocityDeceleration = And(terminalVelocityDeceleration, v.terminalVelocityDeceleration);
        gravityAccel = And(gravityAccel, v.gravityAccel);
        gravityDirection = And(gravityDirection, v.gravityDirection);
        constantAcceleration = And(constantAcceleration, v.constantAcceleration);
    }

}

/// <summary>
/// Handles application of physics mechanics on the player
/// </summary>
[System.Serializable]
public class PlayerMovementPhysics : PlayerMovementOverridableAttribute<PlayerMovementPhysicsValues>
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
    /// Getter for the current direction of gravity
    /// </summary>
    public Vector3 gravityDirection { get { return values.gravityDirection; } }


    /// <summary>
    /// The currently overriden physics values for the current motor update
    /// </summary>
    public PhysicsNegations negations;

    /// <summary>
    /// Constructor
    /// </summary>
    public PlayerMovementPhysics()
    {
        negations = new PhysicsNegations();
    }

    protected override void SetDefaultBaseValues()
    {
        // Set default values
        baseValues.kineticFriction = 9;
        baseValues.staticFrictionMaxSlope = 30;
        baseValues.staticFrictionVelThreshold = 0.5f;
        baseValues.extraKineticFriction = 2;
        baseValues.extraKineticFrictionSpeedThreshold = 18;
        baseValues.upsideDownExtraKineticFriction = 9;
        baseValues.sidewaysExtraKineticFriction = 9;
        baseValues.slopeConstantDown = 1.5f;
        baseValues.slopeConstantUp = 0.75f;
        baseValues.airDrag = 1;
        baseValues.extraAirDrag = 1;
        baseValues.extraAirDragSpeedThreshold = 18;
        baseValues.terminalVelocity = 70;
        baseValues.terminalVelocityDeceleration = 50;
        baseValues.gravityAccel = 36;
        baseValues.gravityDirection = Vector3.down;
        baseValues.constantAcceleration = Vector3.zero;
    }

    protected override void ValidateBaseValues()
    {
        baseValues.gravityDirection = gravityDirection.normalized;
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
                (Vector3.Dot(currentVelocity, gravityDirection) >= 0)
                ? values.slopeConstantDown : values.slopeConstantUp;
            
            // Apply grounded gravity
            currentVelocity += Vector3.ProjectOnPlane(values.gravity * slopeConstant, motor.GetEffectiveGroundNormal()) * deltaTime;
        }
        else if (Vector3.Dot(currentVelocity, gravityDirection)< 0 || Vector3.Project(currentVelocity, gravityDirection).sqrMagnitude < values.terminalVelocity * values.terminalVelocity)
            // Apply aerial (standard) gravity
            currentVelocity += values.gravity * deltaTime;
    }

    /// <summary>
    /// Adds kinetic frictional deceleration onto the player
    /// </summary>
    /// <param name="currentVelocity"> Reference to the player's velocity</param>
    /// <param name="deltaTime"> Motor update time</param>
    private void AddKineticFriction(ref Vector3 currentVelocity, float deltaTime)
    {        
        // Apply kinetic friction ensuring velocity doesn't invert direction, stopping at 0
        if (currentVelocity.sqrMagnitude <= (values.kineticFriction * values.kineticFriction * deltaTime * deltaTime))
        {
            ActivateStaticFriction(ref currentVelocity);
        }
        else
            currentVelocity -= currentVelocity.normalized * values.kineticFriction * deltaTime;
    }

    /// <summary>
    /// Adds extra kinetic frictional deceleration to the player
    /// </summary>
    /// <param name="currentVelocity"> Reference to the player's velocity</param>
    /// <param name="deltaTime"> Motor update time</param>
    private void AddExtraKineticFriction(ref Vector3 currentVelocity, float deltaTime)
    {
        // Apply extra kinetic friction
        currentVelocity -= currentVelocity.normalized * values.extraKineticFriction * deltaTime;
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
        currentVelocity -= currentVelocity.normalized * values.upsideDownExtraKineticFriction * ratio * deltaTime;
    }

    /// <summary>
    /// Adds upside-down extra kinetic frictional deceleration onto the player
    /// </summary>
    /// <param name="currentVelocity"> Reference to the player's velocity</param>
    /// <param name="deltaTime"> Motor update time</param>
    private void AddSidewaysExtraKineticFriction(ref Vector3 currentVelocity, float planePerpGravPercent, float deltaTime)
    {
        // Apply upside-down extra kinetic friction
        currentVelocity -= currentVelocity.normalized * values.upsideDownExtraKineticFriction * planePerpGravPercent * deltaTime;
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
        if (flattenedVelocity.sqrMagnitude <= values.airDrag * values.airDrag * deltaTime * deltaTime)
            currentVelocity -= flattenedVelocity;
        else
            currentVelocity -= flattenedVelocity.normalized * values.airDrag * deltaTime;
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
        currentVelocity -= flattenedVelocity.normalized * values.extraAirDrag * deltaTime;
    }

    /// <summary>
    /// Adds the deceleration of terminal velocity restrictions to the player
    /// </summary>
    /// <param name="currentVelocity"> Reference to the player's velocity</param>
    /// <param name="velocityAlongGravity"> The velocity along gravity</param>
    /// <param name="deltaTime"> Motor update time</param>
    private void AddTerminalVelocityDeceleration(ref Vector3 currentVelocity, Vector3 velocityAlongGravity, float deltaTime)
    {
        float sign = Mathf.Sign(Vector3.Dot(gravityDirection, velocityAlongGravity));
        currentVelocity -= sign * values.terminalVelocityDeceleration * gravityDirection * deltaTime;
        if ((velocityAlongGravity = Vector3.Project(currentVelocity, gravityDirection)).sqrMagnitude < values.terminalVelocity * values.terminalVelocity)
            currentVelocity = (currentVelocity - velocityAlongGravity) + (velocityAlongGravity.normalized * values.terminalVelocity);
    }

    /// <summary>
    /// Updates the reference rotation based on physics calculations
    /// </summary>
    /// <param name="currentRotation"> Reference to the player's rotation</param>
    /// <param name="currentAngularVelocity"> Reference to the player's angular velocity</param>
    /// <param name="motor"> The player's kinematic motor</param>
    /// <param name="deltaTime"> Motor update time</param>
    public void UpdateRotation(ref Quaternion currentRotation, ref Vector3 currentAngularVelocity, KinematicCharacterMotor motor, float deltaTime)
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
            float slopeAngle = Vector3.Angle(motor.GetEffectiveGroundNormal(), -gravityDirection);
            float sqrSpeed = currentVelocity.sqrMagnitude;

            if (!negations.kineticAndStaticFrictionNegated) {

                // If there is static friction to apply
                if (slopeAngle < values.staticFrictionMaxSlope && sqrSpeed < values.staticFrictionVelThreshold * values.staticFrictionVelThreshold)
                    ActivateStaticFriction(ref currentVelocity);

                // If there is kinetic friction to apply
                else if (sqrSpeed > 0 )
                    AddKineticFriction(ref currentVelocity, deltaTime);
            }

            // If there is extra kinetic friction to apply  
            if (sqrSpeed > values.extraKineticFrictionSpeedThreshold * values.extraKineticFrictionSpeedThreshold)
                AddExtraKineticFriction(ref currentVelocity, deltaTime);

            // If there is upside-down extra kinetic friction to apply  
            if (Vector3.Dot(motor.GetEffectiveGroundNormal(), gravityDirection) > 0)
                AddUpsideDownExtraKineticFriction(ref currentVelocity, slopeAngle, deltaTime);

            float planePerpGravPercent = Mathf.Abs(Vector3.Dot(motor.PlanarConstraintAxis, gravityDirection));

            if (planePerpGravPercent > 0.05)
                AddSidewaysExtraKineticFriction(ref currentVelocity, planePerpGravPercent, deltaTime);

            if(values.constantAcceleration != Vector3.zero)
                currentVelocity += Vector3.ProjectOnPlane(values.constantAcceleration, motor.GetEffectiveGroundNormal()) * deltaTime;
                
        }
        else
        {

            // Velocity perpendicular to gravity
            Vector3 flattenedVelocity;
            if ((flattenedVelocity = Vector3.ProjectOnPlane(currentVelocity, gravityDirection)) != Vector3.zero)
            {
                // If there is air drag to apply
                if (!negations.airDragNegated)
                    AddAirDrag(ref currentVelocity, flattenedVelocity, deltaTime);

                // If there is extra air drag to apply  
                if (flattenedVelocity.sqrMagnitude > values.extraAirDragSpeedThreshold * values.extraAirDragSpeedThreshold)
                    AddExtraAirDrag(ref currentVelocity, flattenedVelocity, deltaTime);
            }
            
            Vector3 velocityAlongGravity = currentVelocity - flattenedVelocity;
            // If past terminal velocity  
            if(velocityAlongGravity.sqrMagnitude > values.terminalVelocity * values.terminalVelocity)
                AddTerminalVelocityDeceleration(ref currentVelocity, velocityAlongGravity, deltaTime);

            currentVelocity += values.constantAcceleration * deltaTime;
        }

        if(!negations.gravityNegated)
            AddGravity(ref currentVelocity, motor, deltaTime);

        // Reset update based negations
        negations.Reset();

    }
}
