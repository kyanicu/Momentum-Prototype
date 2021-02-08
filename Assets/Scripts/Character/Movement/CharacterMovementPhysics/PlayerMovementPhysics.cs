using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KinematicCharacterController;

[System.Serializable]
public class PlayerMovementPhysicsValues : CharacterOverridableValues
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
    /// The extra non-overridable decceleration of kinetic friction to a player if they are running not parallel to the plane of gravity.
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

    protected override float[] floatValues 
    {
        get
        {
			return new float[]
            {
                kineticFriction,
        		staticFrictionMaxSlope,
        		staticFrictionVelThreshold,
        		extraKineticFriction,
        		extraKineticFrictionSpeedThreshold,
        		upsideDownExtraKineticFrictionFactor,
        		upsideDownExtraKineticFrictionTimeThreshold,
        		upsideDownAttachSpeed,
        		sidewaysExtraKineticFrictionFactor,
        		sidewaysExtraKineticFrictionTimeThreshold,
        		sidewaysAttachSpeed,
        		slopeConstantDown,
        		slopeConstantUp,
        		airDrag,
        		extraAirDrag,
        		extraAirDragSpeedThreshold,
        		terminalVelocity,
        		terminalVelocityDeceleration,
        		gravityAccel,
        	};
        }
        set 
        {
			kineticFriction = value[0];
        	staticFrictionMaxSlope = value[1];
    		staticFrictionVelThreshold = value[2];
        	extraKineticFriction = value[3];
        	extraKineticFrictionSpeedThreshold = value[4];
        	upsideDownExtraKineticFrictionFactor = value[5];
        	upsideDownExtraKineticFrictionTimeThreshold = value[6];
        	upsideDownAttachSpeed = value[7];
        	sidewaysExtraKineticFrictionFactor = value[8];
        	sidewaysExtraKineticFrictionTimeThreshold = value[9];
        	sidewaysAttachSpeed = value[10];
        	slopeConstantDown = value[11];
        	slopeConstantUp = value[12];
        	airDrag = value[13];
        	extraAirDrag = value[14];
        	extraAirDragSpeedThreshold = value[15];
        	terminalVelocity = value[16];
        	terminalVelocityDeceleration = value[17];
        	gravityAccel = value[18];
        }
    }

    protected override Vector3[] vector3Values
    {
        get
        {
            return new Vector3[] 
            {
                gravityDirection,
                constantAcceleration,
            };
        }
        set
        {
            gravityDirection = value[0];
            constantAcceleration = value[1];
        }
    }

}

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
    public Vector3 gravityDirection { get { return overridableAttribute.values.gravityDirection; } }

    /// <summary>
    /// The currently overriden physics values for the current motor update
    /// </summary>
    public PhysicsNegations negations;

    private float upsideDownTimer;
    private float sidewaysTimer;

    [SerializeField]
    public CharacterOverridableAttribute<PlayerMovementPhysicsValues> overridableAttribute = new CharacterOverridableAttribute<PlayerMovementPhysicsValues>();


    private void Reset()
    {
        // Set default values
        overridableAttribute.baseValues.kineticFriction = 15;
        overridableAttribute.baseValues.staticFrictionMaxSlope = 30;
        overridableAttribute.baseValues.staticFrictionVelThreshold = 0.5f;
        overridableAttribute.baseValues.extraKineticFriction = 3;
        overridableAttribute.baseValues.extraKineticFrictionSpeedThreshold = 18;
        overridableAttribute.baseValues.upsideDownExtraKineticFrictionFactor = 0.75f;
        overridableAttribute.baseValues.upsideDownExtraKineticFrictionTimeThreshold = 0.75f;
        overridableAttribute.baseValues.upsideDownAttachSpeed = 25;
        overridableAttribute.baseValues.sidewaysExtraKineticFrictionFactor = 0.75f;
        overridableAttribute.baseValues.sidewaysExtraKineticFrictionTimeThreshold = 0.75f;
        overridableAttribute.baseValues.sidewaysAttachSpeed = 25;
        overridableAttribute.baseValues.slopeConstantDown = 1.2f;
        overridableAttribute.baseValues.slopeConstantUp = 0.7f;
        overridableAttribute.baseValues.airDrag = 3;
        overridableAttribute.baseValues.extraAirDrag = 2;
        overridableAttribute.baseValues.extraAirDragSpeedThreshold = 18;
        overridableAttribute.baseValues.terminalVelocity = 125;
        overridableAttribute.baseValues.terminalVelocityDeceleration = 50;
        overridableAttribute.baseValues.gravityAccel = 50;
        overridableAttribute.baseValues.gravityDirection = Vector3.down;
        overridableAttribute.baseValues.constantAcceleration = Vector3.zero;
    }

    private void OnValidate()
    {
        overridableAttribute.baseValues.gravityDirection = overridableAttribute.baseValues.gravityDirection.normalized;
    }

    /// <summary>
    /// Constructor
    /// </summary>
    void Awake()
    {
        negations = new PhysicsNegations();
    }

    void Start()
    {
        GetComponent<ICharacterValueOverridabilityCommunication>()?.RegisterOverridability(overridableAttribute);
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
                ? overridableAttribute.values.slopeConstantDown : overridableAttribute.values.slopeConstantUp;
            
            // Apply grounded gravity
            currentVelocity += Vector3.ProjectOnPlane(overridableAttribute.values.gravity * slopeConstant, motor.GetEffectiveGroundNormal()) * deltaTime;
        }
        else if (Vector3.Dot(currentVelocity, gravityDirection)< 0 || Vector3.Project(currentVelocity, gravityDirection).sqrMagnitude < overridableAttribute.values.terminalVelocity * overridableAttribute.values.terminalVelocity)
            // Apply aerial (standard) gravity
            currentVelocity += overridableAttribute.values.gravity * deltaTime;
    }

    /// <summary>
    /// Adds kinetic frictional deceleration onto the player
    /// </summary>
    /// <param name="currentVelocity"> Reference to the player's velocity</param>
    /// <param name="deltaTime"> Motor update time</param>
    private void AddKineticFriction(ref Vector3 currentVelocity, float deltaTime)
    {        
        // Apply kinetic friction ensuring velocity doesn't invert direction, stopping at 0
        if (currentVelocity.sqrMagnitude <= (overridableAttribute.values.kineticFriction * overridableAttribute.values.kineticFriction * deltaTime * deltaTime))
        {
            ActivateStaticFriction(ref currentVelocity);
        }
        else
            currentVelocity -= currentVelocity.normalized * overridableAttribute.values.kineticFriction * deltaTime;
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
        currentVelocity -= currentVelocity.normalized * overridableAttribute.values.extraKineticFriction * ratio * deltaTime;
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
            currentVelocity -= currentVelDir * (overridableAttribute.values.upsideDownExtraKineticFrictionFactor * overridableAttribute.values.gravity.magnitude) * ratio * deltaTime;
        /*}
        else {

        // Get appropriate gravity factor for either up hill or down hill slope
        float slopeConstant = 
            (Vector3.Dot(currentVelocity, gravityDirection) >= 0)
            ? overridableAttribute.values.slopeConstantDown : -overridableAttribute.values.slopeConstantUp;
        
        // Apply grounded gravity
        currentVelocity += currentVelDir * overridableAttribute.values.gravity.magnitude * overridableAttribute.values.upsideDownExtraKineticFrictionFactor * slopeConstant * deltaTime * ratio;
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
            currentVelocity -= currentVelDir * (overridableAttribute.values.sidewaysExtraKineticFrictionFactor * overridableAttribute.values.gravity.magnitude) * planePerpGravPercent * deltaTime;
        /*}
        else {

        // Get appropriate gravity factor for either up hill or down hill slope
        float slopeConstant = 
            (Vector3.Dot(currentVelocity, gravityDirection) >= 0)
            ? overridableAttribute.values.slopeConstantDown : -overridableAttribute.values.slopeConstantUp;
        
        // Apply grounded gravity
        currentVelocity += currentVelDir * overridableAttribute.values.gravity.magnitude * overridableAttribute.values.sidewaysExtraKineticFrictionFactor * slopeConstant * deltaTime * planePerpGravPercent;
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
        if (flattenedVelocity.sqrMagnitude <= overridableAttribute.values.airDrag * overridableAttribute.values.airDrag * deltaTime * deltaTime)
            currentVelocity -= flattenedVelocity;
        else
            currentVelocity -= flattenedVelocity.normalized * overridableAttribute.values.airDrag * deltaTime;
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
        currentVelocity -= flattenedVelocity.normalized * overridableAttribute.values.extraAirDrag * deltaTime;
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
        currentVelocity -= sign * overridableAttribute.values.terminalVelocityDeceleration * gravityDirection * deltaTime;
        if ((velocityAlongGravity = Vector3.Project(currentVelocity, gravityDirection)).sqrMagnitude < overridableAttribute.values.terminalVelocity * overridableAttribute.values.terminalVelocity)
            currentVelocity = (currentVelocity - velocityAlongGravity) + (velocityAlongGravity.normalized * overridableAttribute.values.terminalVelocity);
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
                if (slopeAngle < overridableAttribute.values.staticFrictionMaxSlope && sqrSpeed < overridableAttribute.values.staticFrictionVelThreshold * overridableAttribute.values.staticFrictionVelThreshold)
                    ActivateStaticFriction(ref currentVelocity);

                // If there is kinetic friction to apply
                else if (sqrSpeed > 0)
                    AddKineticFriction(ref currentVelocity, deltaTime);
            }

            // If there is extra kinetic friction to apply  
            //if (sqrSpeed > overridableAttribute.values.extraKineticFrictionSpeedThreshold * overridableAttribute.values.extraKineticFrictionSpeedThreshold)
                //AddExtraKineticFriction(ref currentVelocity, slopeAngle, deltaTime);

            // If there is upside-down extra kinetic friction to apply  
            if (Vector3.Dot(motor.GetEffectiveGroundNormal(), gravityDirection) > 0)
            {
                if(upsideDownTimer >= overridableAttribute.values.upsideDownExtraKineticFrictionTimeThreshold)
                {
                    AddUpsideDownExtraKineticFriction(ref currentVelocity, slopeAngle, deltaTime);
                    
                    if (currentVelocity.sqrMagnitude < overridableAttribute.values.upsideDownAttachSpeed * overridableAttribute.values.upsideDownAttachSpeed)
                        motor.ForceUnground();
                }
                else
                    upsideDownTimer += deltaTime;
            }
            else
            {
                upsideDownTimer = 0;

                if (sqrSpeed > overridableAttribute.values.extraKineticFrictionSpeedThreshold * overridableAttribute.values.extraKineticFrictionSpeedThreshold)
                    AddExtraKineticFriction(ref currentVelocity, slopeAngle, deltaTime);
            }

            float planePerpGravPercent = Mathf.Abs(Vector3.Dot(motor.PlanarConstraintAxis, gravityDirection));
            if (planePerpGravPercent > motor.MaxStableSlopeAngle/90)
            {
                if(sidewaysTimer >= overridableAttribute.values.sidewaysExtraKineticFrictionTimeThreshold)
                {
                    AddSidewaysExtraKineticFrictionFactor(ref currentVelocity, planePerpGravPercent, deltaTime);

                    if (currentVelocity.sqrMagnitude < overridableAttribute.values.upsideDownAttachSpeed * overridableAttribute.values.upsideDownAttachSpeed)
                        motor.ForceUnground();
                }
                else
                    sidewaysTimer += deltaTime; 
            }
            else
            {
                sidewaysTimer = 0;
            }

            if(overridableAttribute.values.constantAcceleration != Vector3.zero)
                currentVelocity += Vector3.ProjectOnPlane(overridableAttribute.values.constantAcceleration, motor.GetEffectiveGroundNormal()) * deltaTime;
                
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
                if (flattenedVelocity.sqrMagnitude > overridableAttribute.values.extraAirDragSpeedThreshold * overridableAttribute.values.extraAirDragSpeedThreshold)
                    AddExtraAirDrag(ref currentVelocity, flattenedVelocity, deltaTime);
            }
            
            Vector3 velocityAlongGravity = currentVelocity - flattenedVelocity;
            // If past terminal velocity  
            if(velocityAlongGravity.sqrMagnitude > overridableAttribute.values.terminalVelocity * overridableAttribute.values.terminalVelocity)
                AddTerminalVelocityDeceleration(ref currentVelocity, velocityAlongGravity, deltaTime);

            currentVelocity += overridableAttribute.values.constantAcceleration * deltaTime;
        }

        if(!negations.gravityNegated)
            AddGravity(ref currentVelocity, motor, deltaTime);

        // Reset update based negations
        negations.Reset();

    }
}
