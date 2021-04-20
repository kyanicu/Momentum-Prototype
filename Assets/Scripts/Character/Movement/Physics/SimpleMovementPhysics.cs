using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KinematicCharacterController;

[System.Serializable]
public class SimpleMovementPhysicsValues : CharacterOverridableValues
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
                slopeConstantDown,
                slopeConstantUp,
                airDrag,
                gravityAccel
            };
        }
        set
        {
            kineticFriction = value[0];
            staticFrictionMaxSlope = value[1];
            staticFrictionVelThreshold = value[2];
            slopeConstantDown = value[3];
            slopeConstantUp = value[4];
            airDrag = value[5];
            gravityAccel = value[6];
        }
    }

    protected override Vector3[] vector3Values
    {
        get
        {
            return new Vector3[]
            {
                gravityDirection
            };
        }
        set
        {
            gravityDirection = value[0];
        }
    }

}

/// <summary>
/// Handles application of physics mechanics on the player
/// </summary>
public class SimpleMovementPhysics : CharacterMovementPhysics
{

    /// <summary>
    /// Getter for the current direction of gravity
    /// </summary>
    public override Vector3 gravityDirection { get { return overridableAttribute.values.gravityDirection; } }

    [SerializeField]
    public CharacterOverridableAttribute<SimpleMovementPhysicsValues> overridableAttribute = new CharacterOverridableAttribute<SimpleMovementPhysicsValues>();

    private void Reset()
    {
        // Set default values
        overridableAttribute.baseValues.kineticFriction = 15;
        overridableAttribute.baseValues.staticFrictionMaxSlope = 30;
        overridableAttribute.baseValues.staticFrictionVelThreshold = 0.5f;
        overridableAttribute.baseValues.slopeConstantDown = 1.2f;
        overridableAttribute.baseValues.slopeConstantUp = 0.7f;
        overridableAttribute.baseValues.airDrag = 3;
        overridableAttribute.baseValues.gravityAccel = Physics.gravity.magnitude;
        overridableAttribute.baseValues.gravityDirection = Physics.gravity.normalized;
    }

    private void OnValidate()
    {
        overridableAttribute.baseValues.gravityDirection = overridableAttribute.baseValues.gravityDirection.normalized;
    }

    /// <summary>
    /// Constructor
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
    }

    void Start()
    {
        GetComponent<ICharacterValueOverridabilityCommunication>()?.RegisterOverridability(overridableAttribute);
    }

    /// <summary>
    /// Adds gravitational acceleration onto the player
    /// </summary>
    /// <param name="currentVelocity"> Reference to the player's velocity</param>
    /// <param name="deltaTime"> Motor update time</param>
    private void AddGravity(ref Vector3 currentVelocity, bool grounded, Vector3 groundNormal, float deltaTime)
    {
        
        if (grounded)
        {
            // Get appropriate gravity factor for either up hill or down hill slope
            float slopeConstant =
                (Vector3.Dot(currentVelocity, gravityDirection) >= 0)
                ? overridableAttribute.values.slopeConstantDown : overridableAttribute.values.slopeConstantUp;

            // Apply grounded gravity
            if (slopeConstant != 0)
                currentVelocity += Vector3.ProjectOnPlane(overridableAttribute.values.gravity * slopeConstant, groundNormal) * deltaTime;
        }
        else
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
    /// Updates the reference velocity based on physics calculations
    /// </summary>
    /// <param name="currentVelocity"> Reference to the player's velocity</param>
    /// <param name="motor"> The player's kinematic motor</param>
    /// <param name="deltaTime"> Motor update time</param>
    public override void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
    {
        if (movement.isGroundedThisUpdate)
        {
            // Get angle of slope in relation to gravity
            float slopeAngle = Vector3.Angle(movement.groundNormal, -gravityDirection);
            float sqrSpeed = currentVelocity.sqrMagnitude;

            if (!negations.kineticAndStaticFrictionNegated)
            {
                // If there is static friction to apply
                if (overridableAttribute.values.staticFrictionMaxSlope != 0 && slopeAngle < overridableAttribute.values.staticFrictionMaxSlope && sqrSpeed < overridableAttribute.values.staticFrictionVelThreshold * overridableAttribute.values.staticFrictionVelThreshold)
                    ActivateStaticFriction(ref currentVelocity);

                // If there is kinetic friction to apply
                else if (overridableAttribute.values.kineticFriction != 0 && sqrSpeed > 0)
                    AddKineticFriction(ref currentVelocity, deltaTime);
            }
        }
        else
        {
            // Velocity perpendicular to gravity
            Vector3 flattenedVelocity;
            if ((flattenedVelocity = Vector3.ProjectOnPlane(currentVelocity, gravityDirection)) != Vector3.zero)
            {
                // If there is air drag to apply
                if (overridableAttribute.values.airDrag != 0 && !negations.airDragNegated)
                    AddAirDrag(ref currentVelocity, flattenedVelocity, deltaTime);
            }
        }

        if (overridableAttribute.values.gravityAccel != 0 && !negations.gravityNegated)
            AddGravity(ref currentVelocity, movement.isGroundedThisUpdate, movement.groundNormal, deltaTime);

        // Reset update based negations
        negations.Reset();

    }
}
