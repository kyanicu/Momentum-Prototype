using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KinematicCharacterController;

[System.Serializable]
public class PlayerMovementPhysicsValues : PlayerOverridableValues
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
    /// The extra non-overridable decceleration of kinetic friction to a player if they are running over a 90 degree angle along the plane of gravity.
    /// Works as a factor on the magnitude of gravity
    /// Can be considered the player's inability to maintain speed when running upside down
    /// </summary>
    public float upsideDownExtraKineticFrictionFactor;
    /// <summary>
    /// The time needed to be upside down before upside down physics activate
    /// </summary>
    public float upsideDownExtraKineticFrictionTimeThreshold;
    /// <summary>
    /// The speed needed to stay on the ground when upside down
    /// </summary>
    public float upsideDownAttachSpeed;
    /// <summary>
    /// The  extra non-overridable decceleration of kinetic friction to a player if they are running not parallel to the plane of gravity.
    /// Works as a factor on the magnitude of gravity
    /// Can be considered the player's inability to maintain speed when running along but not up a slant
    /// </summary>
    public float sidewaysExtraKineticFrictionFactor;
    /// <summary>
    /// The time needed to be sideways before sideways physics activate
    /// </summary>
    public float sidewaysExtraKineticFrictionTimeThreshold;
    /// <summary>
    /// The speed needed to stay on the ground when sideways
    /// </summary>
    public float sidewaysAttachSpeed;
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

    protected override void SetValueCounts()
    {
        floatValuesCount = 19;
        intValuesCount = 0;
        vector3ValuesCount = 2;
    }

    protected override float GetFloatValue(int i)
    {
        switch (i) 
        {
            case (0) :
                return kineticFriction;
            case (1) :
                return staticFrictionMaxSlope;
            case (2) :
                return staticFrictionVelThreshold;
            case (3) :
                return extraKineticFriction;
            case (4) :
                return extraKineticFrictionSpeedThreshold;
            case (5) :
                return upsideDownExtraKineticFrictionFactor;
            case (6) :
                return upsideDownExtraKineticFrictionTimeThreshold;
            case (7) :
                return upsideDownAttachSpeed;
            case (8) :
                return sidewaysExtraKineticFrictionFactor;
            case (9) :
                return sidewaysExtraKineticFrictionTimeThreshold;
            case (10) :
                return sidewaysAttachSpeed;
            case (11) :
                return slopeConstantDown;
            case (12) :
                return slopeConstantUp;
            case (13) :
                return airDrag;
            case (14) :
                return extraAirDrag;
            case (15) :
                return extraAirDragSpeedThreshold;
            case (16) :
                return terminalVelocity;
            case (17) :
                return terminalVelocityDeceleration;
            case (18) :
                return gravityAccel;
            default :
                return 0;
        }
    }
    protected override void SetFloatValue(int i, float value)
    {
        switch (i) 
        {
            case (0) :
                kineticFriction = value;
                break;
            case (1) :
                staticFrictionMaxSlope = value;
                break;
            case (2) :
                staticFrictionVelThreshold = value;
                break;
            case (3) :
                extraKineticFriction = value;
                break;
            case (4) :
                extraKineticFrictionSpeedThreshold = value;
                break;
            case (5) :
                upsideDownExtraKineticFrictionFactor = value;
                break;
            case (6) :
                upsideDownExtraKineticFrictionTimeThreshold = value;
                break;
            case (7) :
                upsideDownAttachSpeed = value;
                break;
            case (8) :
                sidewaysExtraKineticFrictionFactor = value;
                break;
            case (9) :
                sidewaysExtraKineticFrictionTimeThreshold = value;
                break;
            case (10) :
                sidewaysAttachSpeed = value;
                break;
            case (11) :
                slopeConstantDown = value;
                break;
            case (12) :
                slopeConstantUp = value;
                break;
            case (13) :
                airDrag = value;
                break;
            case (14) :
                extraAirDrag = value;
                break;
            case (15) :
                extraAirDragSpeedThreshold = value;
                break;
            case (16) :
                terminalVelocity = value;
                break;
            case (17) :
                terminalVelocityDeceleration = value;
                break;
            case (18) :
                gravityAccel = value;
                break;
            default :
                break;
        }
    }
    protected override int GetIntValue(int i)
    {
        return 0;
    }
    protected override void SetIntValue(int i, int value) {}
    protected override Vector3 GetVector3Value(int i)
    {
        switch (i) 
        {
            case (0) :
                return gravityDirection;
            case (1) :
                return constantAcceleration;
            default :
                return Vector3.zero;
        }
    }
    protected override void SetVector3Value(int i, Vector3 value)
    {
        switch (i) 
        {
            case (0) :
                gravityDirection = value;
                break;
            case (1) :
                constantAcceleration = value;
                break;
            default :
                break;
        }
    }

}

/// <summary>
/// Handles application of physics mechanics on the player
/// </summary>
[System.Serializable]
public class PlayerMovementPhysics : PlayerOverridableAttribute<PlayerMovementPhysicsValues>
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

    private float upsideDownTimer;
    private float sidewaysTimer;

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
        baseValues.kineticFriction = 15;
        baseValues.staticFrictionMaxSlope = 30;
        baseValues.staticFrictionVelThreshold = 0.5f;
        baseValues.extraKineticFriction = 3;
        baseValues.extraKineticFrictionSpeedThreshold = 18;
        baseValues.upsideDownExtraKineticFrictionFactor = 0.75f;
        baseValues.upsideDownExtraKineticFrictionTimeThreshold = 0.75f;
        baseValues.upsideDownAttachSpeed = 25;
        baseValues.sidewaysExtraKineticFrictionFactor = 0.75f;
        baseValues.sidewaysExtraKineticFrictionTimeThreshold = 0.75f;
        baseValues.sidewaysAttachSpeed = 25;
        baseValues.slopeConstantDown = 1.2f;
        baseValues.slopeConstantUp = 0.7f;
        baseValues.airDrag = 3;
        baseValues.extraAirDrag = 2;
        baseValues.extraAirDragSpeedThreshold = 18;
        baseValues.terminalVelocity = 125;
        baseValues.terminalVelocityDeceleration = 50;
        baseValues.gravityAccel = 50;
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
    private void AddExtraKineticFriction(ref Vector3 currentVelocity, float slopeAngle, float deltaTime)
    {
        float ratio = -(slopeAngle - 90) / 90;
        
        // Apply extra kinetic friction
        currentVelocity -= currentVelocity.normalized * values.extraKineticFriction * ratio * deltaTime;
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
        Vector3 currentVelDir = currentVelocity.normalized;

        //if (ratio >= 0.90)
        //{ 
            // Apply upside-down extra kinetic friction as simple friction
            currentVelocity -= currentVelDir * (values.upsideDownExtraKineticFrictionFactor * values.gravity.magnitude) * ratio * deltaTime;
        /*}
        else {

        // Get appropriate gravity factor for either up hill or down hill slope
        float slopeConstant = 
            (Vector3.Dot(currentVelocity, gravityDirection) >= 0)
            ? values.slopeConstantDown : -values.slopeConstantUp;
        
        // Apply grounded gravity
        currentVelocity += currentVelDir * values.gravity.magnitude * values.upsideDownExtraKineticFrictionFactor * slopeConstant * deltaTime * ratio;
        }
        */
    }

    /// <summary>
    /// Adds upside-down extra kinetic frictional deceleration onto the player
    /// </summary>
    /// <param name="currentVelocity"> Reference to the player's velocity</param>
    /// <param name="deltaTime"> Motor update time</param>
    private void AddSidewaysExtraKineticFrictionFactor(ref Vector3 currentVelocity, float planePerpGravPercent, float deltaTime)
    {
        Vector3 currentVelDir = currentVelocity.normalized;

        //if (planePerpGravPercent >= 0.90)
        //{ 
            // Apply upside-down extra kinetic friction as simple frction
            currentVelocity -= currentVelDir * (values.sidewaysExtraKineticFrictionFactor * values.gravity.magnitude) * planePerpGravPercent * deltaTime;
        /*}
        else {

        // Get appropriate gravity factor for either up hill or down hill slope
        float slopeConstant = 
            (Vector3.Dot(currentVelocity, gravityDirection) >= 0)
            ? values.slopeConstantDown : -values.slopeConstantUp;
        
        // Apply grounded gravity
        currentVelocity += currentVelDir * values.gravity.magnitude * values.sidewaysExtraKineticFrictionFactor * slopeConstant * deltaTime * planePerpGravPercent;
        }
        */
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
                else if (sqrSpeed > 0)
                    AddKineticFriction(ref currentVelocity, deltaTime);
            }

            // If there is extra kinetic friction to apply  
            //if (sqrSpeed > values.extraKineticFrictionSpeedThreshold * values.extraKineticFrictionSpeedThreshold)
                //AddExtraKineticFriction(ref currentVelocity, slopeAngle, deltaTime);

            // If there is upside-down extra kinetic friction to apply  
            if (Vector3.Dot(motor.GetEffectiveGroundNormal(), gravityDirection) > 0)
            {
                if(upsideDownTimer >= values.upsideDownExtraKineticFrictionTimeThreshold)
                {
                    AddUpsideDownExtraKineticFriction(ref currentVelocity, slopeAngle, deltaTime);
                    
                    if (currentVelocity.sqrMagnitude < values.upsideDownAttachSpeed * values.upsideDownAttachSpeed)
                        motor.ForceUnground();
                }
                else
                    upsideDownTimer += deltaTime;
            }
            else
            {
                upsideDownTimer = 0;

                if (sqrSpeed > values.extraKineticFrictionSpeedThreshold * values.extraKineticFrictionSpeedThreshold)
                    AddExtraKineticFriction(ref currentVelocity, slopeAngle, deltaTime);
            }

            float planePerpGravPercent = Mathf.Abs(Vector3.Dot(motor.PlanarConstraintAxis, gravityDirection));
            if (planePerpGravPercent > motor.MaxStableSlopeAngle/90)
            {
                if(sidewaysTimer >= values.sidewaysExtraKineticFrictionTimeThreshold)
                {
                    AddSidewaysExtraKineticFrictionFactor(ref currentVelocity, planePerpGravPercent, deltaTime);

                    if (currentVelocity.sqrMagnitude < values.upsideDownAttachSpeed * values.upsideDownAttachSpeed)
                        motor.ForceUnground();
                }
                else
                    sidewaysTimer += deltaTime; 
            }
            else
            {
                sidewaysTimer = 0;
            }

            if(values.constantAcceleration != Vector3.zero)
                currentVelocity += Vector3.ProjectOnPlane(values.constantAcceleration, motor.GetEffectiveGroundNormal()) * deltaTime;
                
        }
        else
        {
            sidewaysTimer = 0;
            upsideDownTimer = 0;

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
