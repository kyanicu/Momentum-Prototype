using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KinematicCharacterController;
using PathCreation;

public interface IPlayerMovementInput
{
    void RegisterInput(PlayerController.PlayerActions controllerActions);
    void Reset();
}

[System.Serializable]
public abstract class PlayerMovementOverridableValues
{

    public abstract void SetDefaultValues(PlayerMovementOverrideType overrideType);

    protected static float DefaultFloat(PlayerMovementOverrideType type)
    {
        switch (type)
        {
            case PlayerMovementOverrideType.Addition :
                return 0;
            case PlayerMovementOverrideType.Multiplier :
                return 1;
            case PlayerMovementOverrideType.Set :
                return float.PositiveInfinity;
            default :
                return 0;
        }
    }
    protected static Vector3 DefaultVector3(PlayerMovementOverrideType type)
    {
        switch (type)
        {
            case PlayerMovementOverrideType.Addition :
                return Vector3.zero;
            case PlayerMovementOverrideType.Multiplier :
                return Vector3.one;
            case PlayerMovementOverrideType.Set :
                return Vector3.positiveInfinity;
            default :
                return Vector3.zero;
        }
    }

    protected static int DefaultInt(PlayerMovementOverrideType type)
    {
        switch (type)
        {
            case PlayerMovementOverrideType.Addition :
                return 0;
            case PlayerMovementOverrideType.Multiplier :
                return 1;
            case PlayerMovementOverrideType.Set :
                return int.MaxValue;
            default :
                return 0;
        }
    }

    protected static float Add(float v1, float v2)
    {
        return v1 + v2;
    }

    protected static float Subtract(float v1, float v2)
    {
        return v1 - v2;
    }
    
    protected static float Multiply(float v1, float v2)
    {
        return v1 * v2;
    }
    
    protected static float Divide(float v1, float v2)
    {
        return v1 / v2;
    }
    
    protected static float Or(float v1, float v2)
    {
        return (!float.IsInfinity(v2)) ? v2 : v1;
    }
    
    protected static float And(float v1, float v2)
    {
        return (!float.IsInfinity(v2)) ? float.PositiveInfinity : v1;
    }
    
    protected static int Add(int v1, int v2)
    {
        return v1 + v2;
    }

    protected static int Subtract(int v1, int v2)
    {
        return v1 - v2;
    }
    
    protected static int Multiply(int v1, int v2)
    {
        return v1 * v2;
    }
    
    protected static int Divide(int v1, int v2)
    {
        return v1 / v2;
    }
    
    protected static int Or(int v1, int v2)
    {
        return (v2 != int.MaxValue) ? v2 : v1;
    }
    
    protected static int And(int v1, int v2)
    {
        return (v2 != int.MaxValue) ? int.MaxValue : v1;
    }

    protected static Vector3 Add(Vector3 v1, Vector3 v2)
    {
        return v1 + v2;
    }
    
    protected static Vector3 Subtract(Vector3 v1, Vector3 v2)
    {
        return v1 + v2;
    }
    
    protected static Vector3 Multiply(Vector3 v1, Vector3 v2)
    {
        return Vector3.Scale(v1, v2);
    }
    
    protected static Vector3 Divide(Vector3 v1, Vector3 v2)
    {
        return Vector3.Scale(v1, new Vector3(1/v2.x, 1/v2.y, 1/v2.z));
    }
    
    protected static Vector3 Or(Vector3 v1, Vector3 v2)
    {
        return (!float.IsInfinity(v2.x)) ? v2 : v1;
    }
    
    protected static Vector3 And(Vector3 v1, Vector3 v2)
    {
        return (!float.IsInfinity(v2.x)) ? Vector3.positiveInfinity : v1;;
    }

    public abstract void AddBy(PlayerMovementOverridableValues v);
    public abstract void SubtractBy(PlayerMovementOverridableValues v);
    public abstract void MultiplyBy(PlayerMovementOverridableValues v);
    public abstract void DivideBy(PlayerMovementOverridableValues v);
    public abstract void OrBy(PlayerMovementOverridableValues v);
    public abstract void AndBy(PlayerMovementOverridableValues v);
}

[System.Serializable]
public abstract class PlayerMovementOverridableAttribute<Values> where Values : PlayerMovementOverridableValues, new()
{
    /// <summary>
    /// The default physics values for the player
    /// </summary>
    [SerializeField]
    protected Values baseValues;

    /// <summary>
    /// The added physics values for the player
    /// </summary>
    private Values addedValues;

    /// <summary>
    /// The multiplied physics values for the player
    /// </summary>
    private Values multipliedValues;

    /// <summary>
    /// The multiplied physics values for the player
    /// </summary>
    private List<Values> setValues;

    /// <summary>
    /// The current physics values for the player, including overrides
    /// </summary>
    //[SerializeField]
    protected Values values;

    public PlayerMovementOverridableAttribute()
    {
        baseValues = new Values();
        addedValues = new Values();
        multipliedValues = new Values();
        setValues = new List<Values>() { new Values() };
        values = new Values();

        baseValues.SetDefaultValues(PlayerMovementOverrideType.Addition);
        addedValues.SetDefaultValues(PlayerMovementOverrideType.Addition);
        multipliedValues.SetDefaultValues(PlayerMovementOverrideType.Multiplier);
        setValues[0].SetDefaultValues(PlayerMovementOverrideType.Set);
        values.SetDefaultValues(PlayerMovementOverrideType.Addition);

        SetDefaultBaseValues();

        CalculateValues();
    }

    protected abstract void SetDefaultBaseValues();

    public void CalculateValues()
    {
        Values set = new Values();
        set.SetDefaultValues(PlayerMovementOverrideType.Set);
        foreach (Values s in setValues)
        {
            set.OrBy(s);
        }

        values.SetDefaultValues(PlayerMovementOverrideType.Addition);
        values.AddBy(baseValues);
        values.OrBy(set);
        values.MultiplyBy(multipliedValues);
        values.AddBy(addedValues);
    }

    public void AddOverride(Values overrideValues, PlayerMovementOverrideType overrideType)
    {
        switch (overrideType) 
        {
            case (PlayerMovementOverrideType.Addition) :
                addedValues.AddBy(overrideValues); 
                break;
            case (PlayerMovementOverrideType.Multiplier) :
                multipliedValues.MultiplyBy(overrideValues); 
                break;
            case (PlayerMovementOverrideType.Set) :
                setValues.Add(overrideValues); 
                break;
        }
        CalculateValues();
    }

    public void RemoveOverride(Values overrideValues, PlayerMovementOverrideType overrideType)
    {
        switch (overrideType) 
        {
            case (PlayerMovementOverrideType.Addition) :
               addedValues.SubtractBy(overrideValues); 
                break;
            case (PlayerMovementOverrideType.Multiplier) :
                multipliedValues.DivideBy(overrideValues); 
                break;
            case (PlayerMovementOverrideType.Set) :
                setValues.Remove(overrideValues); 
                break;
        }
        CalculateValues();
    }

    public void OnValidate()
    {
        ValidateBaseValues();
        CalculateValues();
    }

    protected abstract void ValidateBaseValues();

}

public enum PlayerMovementOverrideType { Set, Addition, Multiplier }

[System.Serializable]
public class PlayerMovementValues : PlayerMovementOverridableValues
{
    [SerializeField]
    public float attachThreshold;
    [SerializeField]
    public float pushOffGroundThreshold;
    [SerializeField]
    public float maxSlopeTrackTime;
    [SerializeField]
    public float ungroundRotationFactor;
    [SerializeField]
    public float ungroundRotationMinSpeed;
    [SerializeField]
    public float ungroundRotationMaxSpeed;

    public int negateAction;
    public int negatePhysics;

    public override void SetDefaultValues(PlayerMovementOverrideType overrideType)
    {
        attachThreshold = DefaultFloat(overrideType);
        pushOffGroundThreshold = DefaultFloat(overrideType);
        maxSlopeTrackTime = DefaultFloat(overrideType);
        ungroundRotationFactor = DefaultFloat(overrideType);
        ungroundRotationMinSpeed = DefaultFloat(overrideType);
        ungroundRotationMaxSpeed = DefaultFloat(overrideType);
        negateAction = DefaultInt(overrideType);
        negatePhysics = DefaultInt(overrideType);
    }
    
    public override void AddBy(PlayerMovementOverridableValues ov) 
    {
        PlayerMovementValues v = ov as PlayerMovementValues;


        attachThreshold = Add(attachThreshold, v.attachThreshold);
        pushOffGroundThreshold = Add(pushOffGroundThreshold, v.pushOffGroundThreshold);
        maxSlopeTrackTime = Add(maxSlopeTrackTime, v.maxSlopeTrackTime);
        ungroundRotationFactor = Add(ungroundRotationFactor, v.ungroundRotationFactor);
        ungroundRotationMinSpeed = Add(ungroundRotationMinSpeed, v.ungroundRotationMinSpeed);
        ungroundRotationMaxSpeed = Add(ungroundRotationMaxSpeed, v.ungroundRotationMaxSpeed);
        negateAction = Add(negateAction, v.negateAction);
        negatePhysics = Add(negatePhysics, v.negatePhysics);
    }

    public override void SubtractBy(PlayerMovementOverridableValues ov) 
    {
        PlayerMovementValues v = ov as PlayerMovementValues;

        attachThreshold = Subtract(attachThreshold, v.attachThreshold);
        pushOffGroundThreshold = Subtract(pushOffGroundThreshold, v.pushOffGroundThreshold);
        maxSlopeTrackTime = Subtract(maxSlopeTrackTime, v.maxSlopeTrackTime);
        ungroundRotationFactor = Subtract(ungroundRotationFactor, v.ungroundRotationFactor);
        ungroundRotationMinSpeed = Subtract(ungroundRotationMinSpeed, v.ungroundRotationMinSpeed);
        ungroundRotationMaxSpeed = Subtract(ungroundRotationMaxSpeed, v.ungroundRotationMaxSpeed);
        negateAction = Subtract(negateAction, v.negateAction);
        negatePhysics = Subtract(negatePhysics, v.negatePhysics);
    }

    public override void MultiplyBy(PlayerMovementOverridableValues ov) 
    {
        PlayerMovementValues v = ov as PlayerMovementValues;

        attachThreshold = Multiply(attachThreshold, v.attachThreshold);
        pushOffGroundThreshold = Multiply(pushOffGroundThreshold, v.pushOffGroundThreshold);
        maxSlopeTrackTime = Multiply(maxSlopeTrackTime, v.maxSlopeTrackTime);
        ungroundRotationFactor = Multiply(ungroundRotationFactor, v.ungroundRotationFactor);
        ungroundRotationMinSpeed = Multiply(ungroundRotationMinSpeed, v.ungroundRotationMinSpeed);
        ungroundRotationMaxSpeed = Multiply(ungroundRotationMaxSpeed, v.ungroundRotationMaxSpeed);
        negateAction = Multiply(negateAction, v.negateAction);
        negatePhysics = Multiply(negatePhysics, v.negatePhysics);
    }

    public override void DivideBy(PlayerMovementOverridableValues ov) 
    {
        PlayerMovementValues v = ov as PlayerMovementValues;

        attachThreshold = Divide(attachThreshold, v.attachThreshold);
        pushOffGroundThreshold = Divide(pushOffGroundThreshold, v.pushOffGroundThreshold);
        maxSlopeTrackTime = Divide(maxSlopeTrackTime, v.maxSlopeTrackTime);
        ungroundRotationFactor = Divide(ungroundRotationFactor, v.ungroundRotationFactor);
        ungroundRotationMinSpeed = Divide(ungroundRotationMinSpeed, v.ungroundRotationMinSpeed);
        ungroundRotationMaxSpeed = Divide(ungroundRotationMaxSpeed, v.ungroundRotationMaxSpeed);
        negateAction = Divide(negateAction, v.negateAction);
        negatePhysics = Divide(negatePhysics, v.negatePhysics);
    }

    public override void OrBy(PlayerMovementOverridableValues ov) 
    {
        PlayerMovementValues v = ov as PlayerMovementValues;

        attachThreshold = Or(attachThreshold, v.attachThreshold);
        pushOffGroundThreshold = Or(pushOffGroundThreshold, v.pushOffGroundThreshold);
        maxSlopeTrackTime = Or(maxSlopeTrackTime, v.maxSlopeTrackTime);
        ungroundRotationFactor = Or(ungroundRotationFactor, v.ungroundRotationFactor);
        ungroundRotationMinSpeed = Or(ungroundRotationMinSpeed, v.ungroundRotationMinSpeed);
        ungroundRotationMaxSpeed = Or(ungroundRotationMaxSpeed, v.ungroundRotationMaxSpeed);
        negateAction = Or(negateAction, v.negateAction);
        negatePhysics = Or(negatePhysics, v.negatePhysics);
    }

    public override void AndBy(PlayerMovementOverridableValues ov) 
    {
        PlayerMovementValues v = ov as PlayerMovementValues;

        attachThreshold = And(attachThreshold, v.attachThreshold);
        pushOffGroundThreshold = And(pushOffGroundThreshold, v.pushOffGroundThreshold);
        maxSlopeTrackTime = And(maxSlopeTrackTime, v.maxSlopeTrackTime);
        ungroundRotationFactor = And(ungroundRotationFactor, v.ungroundRotationFactor);
        ungroundRotationMinSpeed = And(ungroundRotationMinSpeed, v.ungroundRotationMinSpeed);
        ungroundRotationMaxSpeed = And(ungroundRotationMaxSpeed, v.ungroundRotationMaxSpeed);
        negateAction = And(negateAction, v.negateAction);
        negatePhysics = And(negatePhysics, v.negatePhysics);
    }
}

[System.Serializable]
public class PlayerMovement<Ability> : PlayerMovementOverridableAttribute<PlayerMovementValues>, ICharacterController, IPlayerMovementCommunication where Ability : IPlayerMovementAbility, new()
{

    #region MovementEvents
    public event EventHandler<PlaneChangeEventArgs> planeChanged;
    public event EventHandler<KinematicCharacterMotorState> stateUpdated;
    #endregion

    private struct SlopeList
    {
        private (Vector3, float)[] slopeList;
        private int capacity;
        private int count;

        private float slopeTimer;

        public SlopeList(int size)
        {
            slopeList = new (Vector3, float)[size];
            capacity = size;
            count = 0;
            slopeTimer = 0;
        }

        public void Reset()
        {
            this = new SlopeList(capacity);
        }

        public void IncrementTimer(float deltaTime, float maxTime)
        {
            slopeTimer += deltaTime;
        
            if (slopeTimer > maxTime && count != 0)
            {
                Reset();
            }
        }

        public void Add(Vector3 slopeNormal)
        {
            if (count < capacity)
            {
                slopeList[count] = (slopeNormal,slopeTimer);
                count++;
            }
            else 
            {
                for(int i = 0; i < count-1; i++)
                {
                    slopeList[i] = slopeList[i+1];
                }
                slopeList[count-1] = (slopeNormal, slopeTimer);
            }
            slopeTimer = 0;
        }

        public Vector3 GetAngularVelocity(Vector3 linearVelocity, Vector3 axis, float rotationFactor, float minRotationSpeed, float maxRotationSpeed)
        {
            if(count <= 1) 
                return Vector3.zero;

            Vector3 angularVelocity = Vector3.zero;

            float rotationSpeed = 0;

            for (int i = 0; i < count-1; i++)
            {
                rotationSpeed += Vector3.SignedAngle(slopeList[i].Item1, slopeList[i+1].Item1, axis) / (slopeList[i].Item2 + slopeList[i+1].Item2);
            }
            rotationSpeed /= count - 1;

            angularVelocity = rotationSpeed * axis * rotationFactor;

            if (Mathf.Abs(rotationSpeed) > maxRotationSpeed)
                rotationSpeed = maxRotationSpeed * Mathf.Sign(rotationSpeed);
            else if (Mathf.Abs(rotationSpeed) < minRotationSpeed)
                rotationSpeed = 0;
            
            return angularVelocity;
        }
    }

    [SerializeField]
    private PlayerMovementPhysics physics;
    [SerializeField]
    private PlayerMovementAction action;
    [SerializeField]
    private Ability ability;

    private SlopeList slopeList;
    private bool foundFloorToReorientTo;

    private Vector3 internalAngularVelocity = Vector3.zero;
    public Vector3 externalVelocity { private get; set; }

    private Plane currentPlane;
    private Plane brokenPlane;
    private DynamicPlane[] currentDynamicPlanes;
    private PlaneBreaker currentPlaneBreaker;

    // Debug
    #region debug
    KinematicCharacterMotorState startState;
    Plane startPlane;
    #endregion

    /// <summary>
    ///  Constructor
    /// </summary>
    public PlayerMovement()
    {
        physics = new PlayerMovementPhysics();
        action = new PlayerMovementAction();
        ability = new Ability();

        ability.addingMovementOverrides += AddAbilityOverride;
        ability.removingMovementOverrides += RemoveAbilityOverride;
    } 
    
    protected override void SetDefaultBaseValues()
    {
        // Set default field values
        baseValues.attachThreshold = 8;
        baseValues.pushOffGroundThreshold = 1;
        baseValues.maxSlopeTrackTime = 0.5f;
        baseValues.ungroundRotationFactor = 1.25f;
        baseValues.ungroundRotationMinSpeed = 15;
        baseValues.ungroundRotationMaxSpeed = 1000;
        baseValues.negateAction = 0;
        baseValues.negatePhysics = 0;
    }

    protected override void ValidateBaseValues()
    {
        action.OnValidate();
        physics.OnValidate();
        ability.OnValidate();
    }

    public void SetCommunication(PlayerInternalCommunicator communicator)
    {
        communicator.SetCommunication(this);

        action.SetCommunication(communicator);
        ability.SetCommunication(communicator);
    }

    /// <summary>
    /// Used to initialize motor's reference to this script
    /// Also initializes state info for debugging
    /// </summary>
    public void InitializeForPlay(KinematicCharacterMotor motor)
    {
        currentDynamicPlanes = new DynamicPlane[2];
        slopeList = new SlopeList(5);

        SetCurrentPlane(motor, new Plane(motor.CharacterForward, motor.Transform.position));
        motor.PlanarConstraintAxis = currentPlane.normal;
        
        // Debug
        #region debug
        startState = motor.GetState();
        startPlane = new Plane(motor.PlanarConstraintAxis, motor.Transform.position);
        #endregion
    }

    /// <summary>
    /// Handles Input when valid
    /// </summary>
    public void HandleInput(PlayerController.PlayerActions controllerActions)
    {
        if(values.negateAction == 0)
            action.RegisterInput(controllerActions);
            ability.RegisterInput(controllerActions);
    }

    public void HandleTriggerEnter(KinematicCharacterMotor motor, Collider col)
    {
        if (col.tag == "Plane")
            EnterDynamicPlane(motor, col.GetComponent<DynamicPlane>());
        else if (col.tag == "Plane Breaker")
            EnterPlaneBreaker(motor, col.GetComponent<PlaneBreaker>());
        else if (col.tag == "Movement Effector")
            EnterMovementEffector(col.GetComponent<MovementEffector>());
    } 

    public void HandleTriggerExit(Collider col)
    {
        if (col.tag == "Plane")
            ExitDynamicPlane(col);
        else if (col.tag == "Plane Breaker")
            ExitPlaneBreaker();
        else if (col.tag == "Movement Effector")
            ExitMovementEffector(col.GetComponent<MovementEffector>());
    } 

#region CharacterControllerInterface

    /// <summary>
    /// This is called when the motor wants to know what its rotation should be right now
    /// </summary>
    /// <param name="currentRotation"> Reference to the player's </param>
    /// <param name="motor"> The player's kinematic motor</param>
    /// <param name="deltaTime"> Motor update time </param>
    public void UpdateRotation(ref Quaternion currentRotation, KinematicCharacterMotor motor, float deltaTime)
    {
        Vector3 initialCharacterBottomHemiCenter = motor.TransientPosition + (motor.CharacterUp * motor.Capsule.radius);

        float slerpFactor = 30;
        if (motor.CharacterForward != motor.PlanarConstraintAxis)
        {
            
            Vector3 smoothedForward;
                if(Vector3.Dot(motor.CharacterForward, -motor.PlanarConstraintAxis) > 0.95f)
                    smoothedForward = Vector3.Slerp((motor.CharacterUp - motor.CharacterRight * 0.05f).normalized, -physics.gravityDirection, 1 - Mathf.Exp(-10 * deltaTime));
                else 
                    smoothedForward = Vector3.Slerp(motor.CharacterForward, motor.PlanarConstraintAxis, 1 - Mathf.Exp(-slerpFactor * deltaTime));
            
            currentRotation = /*Quaternion.FromToRotation(motor.CharacterForward, motor.PlanarConstraintAxis) * currentRotation;*/Quaternion.FromToRotation(motor.CharacterForward, smoothedForward) * currentRotation;
        }

        bool reorient = false;
        if(foundFloorToReorientTo)
        {
            internalAngularVelocity = Vector3.zero;
            foundFloorToReorientTo = false;
            reorient = true;
        }
        
        if(motor.IsGroundedThisUpdate)
        {
            internalAngularVelocity = Vector3.zero;

            if(motor.BaseVelocity.magnitude > values.attachThreshold)
            {
                slerpFactor = 30;
                Vector3 smoothedGroundNormal = Vector3.Slerp(motor.CharacterUp, motor.GetEffectiveGroundNormal(), 1 - Mathf.Exp(-slerpFactor * deltaTime));
                currentRotation = /*Quaternion.FromToRotation(motor.CharacterUp, motor.GetEffectiveGroundNormal()) * currentRotation;*/Quaternion.FromToRotation(motor.CharacterUp, smoothedGroundNormal) * currentRotation;
            }
            else
            {
                slerpFactor = 3;
                Vector3 smoothedUp = Vector3.Slerp(motor.CharacterUp, -physics.gravityDirection, 1 - Mathf.Exp(-slerpFactor * deltaTime));
                
                currentRotation = Quaternion.FromToRotation(motor.CharacterUp, smoothedUp) * currentRotation;
            }
        }
        else if(internalAngularVelocity == Vector3.zero)
        {
            
            if(!reorient)
                slerpFactor = 3;

            Vector3 smoothedUp;
            if(Vector3.Dot(motor.CharacterUp, physics.gravityDirection) > 0.95f)
                smoothedUp = Vector3.Slerp((motor.CharacterUp - motor.CharacterRight * 0.05f).normalized, -physics.gravityDirection, 1 - Mathf.Exp(-slerpFactor * deltaTime));
            else
                smoothedUp = Vector3.Slerp(motor.CharacterUp, -physics.gravityDirection, 1 - Mathf.Exp(-slerpFactor * deltaTime));
            currentRotation = Quaternion.FromToRotation(motor.CharacterUp, smoothedUp) * currentRotation;
        }

        if(values.negateAction == 0)
            action.UpdateRotation(ref currentRotation, ref internalAngularVelocity, motor, deltaTime);
            ability.UpdateRotation(ref currentRotation, ref internalAngularVelocity, motor, deltaTime);
        if(values.negatePhysics == 0)
            physics.UpdateRotation(ref currentRotation, ref internalAngularVelocity, motor, deltaTime);

        if(internalAngularVelocity != Vector3.zero)
        {
            if (Mathf.Abs(Vector3.Dot(internalAngularVelocity, motor.PlanarConstraintAxis)) < 0.99) 
            {
                // Project rotational velocity
                internalAngularVelocity = Vector3.Project(internalAngularVelocity, motor.PlanarConstraintAxis);
                // Maintain rotational momentum but reorient
            }
            //internalAngularVelocity = Vector3.project(internalAngularVelocity, motor.PlanarConstraintAxis).normalized * internalAngularVelocity.magnitude

            currentRotation = Quaternion.Euler(internalAngularVelocity * deltaTime) * currentRotation;
        }

        if(motor.IsGroundedThisUpdate)
        // Move the position to create a rotation around the bottom hemi center instead of around the pivot
            motor.SetTransientPosition(initialCharacterBottomHemiCenter + (currentRotation * Vector3.down * motor.Capsule.radius));

    }
    
    /// <summary>
    /// This is called hen the motor wants to know what its velocity should be right now
    /// </summary>
    /// <param name="currentVelocity"> Reference to the player's velocity </param>
    /// <param name="maxMove"> The max distance the player can move this update</param>
    /// <param name="motor"> The player's kinematic motor</param>
    /// <param name="deltaTime"> Motor update time </param>
    public void UpdateVelocity(ref Vector3 currentVelocity, ref float maxMove, KinematicCharacterMotor motor, float deltaTime)
    {
        
        // Handle velocity projection if grounded
        if (motor.IsGroundedThisUpdate)
        {
            // The dot between the ground normal and the external velocity addition
            float dot = Vector3.Dot(externalVelocity, motor.GetEffectiveGroundNormal());
            // The velocity off the ground
            Vector3 projection = dot * motor.GetEffectiveGroundNormal();
            // If external velocity off ground is strong enough
            if(dot > 0  && projection.sqrMagnitude >= values.pushOffGroundThreshold * values.pushOffGroundThreshold)
                motor.ForceUnground();
            else
            {
                // if just landed or slope hasn't changed
                if (!motor.WasGroundedLastUpdate || motor.GetEffectiveGroundNormal() == motor.GetLastEffectiveGroundNormal())
                    // Project velocity onto ground
                    currentVelocity = Vector3.ProjectOnPlane(currentVelocity, motor.GetEffectiveGroundNormal());
                else
                    // Reorient without losing momentum
                    currentVelocity = Vector3.ProjectOnPlane(currentVelocity, motor.GetEffectiveGroundNormal()).normalized * currentVelocity.magnitude;

                // Snap external velocity to ground
                externalVelocity -= projection;
            }
        }

        // Add external velocity and reset back to zero
        currentVelocity += externalVelocity;
        externalVelocity = Vector3.zero;

        // Update velocity from components
        if(values.negateAction == 0)
            action.UpdateVelocity(ref currentVelocity, motor, physics.gravityDirection, ref physics.negations, deltaTime);
        ability.UpdateVelocity(ref currentVelocity, motor, physics.gravityDirection, ref physics.negations, deltaTime);
        if(values.negatePhysics == 0)
            physics.UpdateVelocity (ref currentVelocity, motor, deltaTime);

        currentVelocity = Vector3.ProjectOnPlane(currentVelocity, motor.PlanarConstraintAxis);

        
    }

    /// <summary>
    /// This is called before the motor does anything
    /// </summary>
    /// <param name="motor"> The player's kinematic motor</param>
    /// <param name="deltaTime"> Motor update time </param>
    public void BeforeCharacterUpdate(KinematicCharacterMotor motor, float deltaTime)
    {

        // If active on PlaneBreaker
        if (currentPlaneBreaker != null && motor.IsGroundedThisUpdate && motor.BaseVelocity.magnitude > values.attachThreshold)
        {
            Vector3 planeRight = Vector3.Cross(currentPlaneBreaker.transform.up, currentPlane.normal);
            float dot = Vector3.Dot(motor.BaseVelocity, planeRight);
            bool movingRight = dot > 0;            
            /*
            Plane traversalPlane;
            Plane approachingPlaneTransition = currentPlaneBreaker.GetApproachingPlaneTransition(motor.Transform.position, currentVelocity.normalized, dot > 0, out traversalPlane);
            */    SetCurrentPlane(motor, currentPlaneBreaker.GetClosestPathPlane(/*motor.TransientPosition + (motor.CharacterUp * motor.Capsule.radius)*/motor.Transform.position, movingRight), true);//SetCurrentPlane(motor, currentPlaneBreaker.GetClosestPathPlane(motor.TransientPosition + (motor.CharacterUp * motor.Capsule.radius)/*motor.Transform.position*/, movingRight), true);
            //if (dot != 0 && approachingPlaneTransition.normal != Vector3.zero && approachingPlaneTransition.distance != float.PositiveInfinity)
            //{
            //    float dist;
                //if (approachingPlaneTransition.Raycast(new Ray(motor.Transform.position, currentVelocity), out dist))
                    //maxMove = dist;
            //}
            //Debug.Break();
        }
        // If on Dynamic Plane, handle movement  
        else if(currentDynamicPlanes[0] != null)
        {
            Vector3 planeRight = Vector3.Cross(currentDynamicPlanes[0].transform.up, currentPlane.normal);
            float dot = Vector3.Dot(motor.BaseVelocity, planeRight);
            bool movingRight = dot > 0;
            /*
            Plane traversalPlane;
            Plane approachingPlaneTransition = currentDynamicPlanes[0].GetApproachingPlaneTransition(motor.Transform.position, movingRight, out traversalPlane);
            */    SetCurrentPlane(motor, currentDynamicPlanes[0].GetClosestPathPlane(motor.Transform.position, movingRight));
            /*
            if (dot != 0 && approachingPlaneTransition.normal != Vector3.zero && approachingPlaneTransition.distance != float.PositiveInfinity)
            {
                float dist;
                if (approachingPlaneTransition.Raycast(new Ray(motor.Transform.position, currentVelocity), out dist))
                    maxMove = dist;
            }
            */
        }

        if (settingPlane.normal != Vector3.zero)
            SetCurrentPlane(motor, settingPlane, settingPlaneBreaker, true);

        ability.BeforeCharacterUpdate(motor, deltaTime);
    }

    /// <summary>
    /// This is called after the motor has finished its ground probing, but before PhysicsMover/Velocity/etc.... handling
    /// Primarily used currently to handle the slope tracking for the ungrounding angular momentum mechanic
    /// </summary>
    /// <param name="motor"> The player's kinematic motor</param>
    /// <param name="deltaTime"> Motor update time </param>
    public void PostGroundingUpdate(KinematicCharacterMotor motor, float deltaTime)
    {
        // kKep tracking time on current slope
        slopeList.IncrementTimer(deltaTime, values.maxSlopeTrackTime);
        
        // Used to see if angular velocity should be set for angular momentum when ungrounding
        bool setAngularVelocity = false;

        if(motor.IsGroundedThisUpdate)
        {
            // If speed drops enough, detatch from slope
            if(Vector3.ProjectOnPlane(motor.BaseVelocity, motor.GetEffectiveGroundNormal()).magnitude < values.attachThreshold && Vector3.Angle(-physics.gravityDirection, motor.GetEffectiveGroundNormal()) > motor.MaxStableSlopeAngle)
            {
                motor.ForceUnground();
                setAngularVelocity = true;
            }
            // If slope changed
            else if(motor.WasGroundedLastUpdate && motor.GetEffectiveGroundNormal() != motor.GetLastEffectiveGroundNormal())
                // Start tracking new slope
                slopeList.Add(motor.GetLastEffectiveGroundNormal());

            // If just grounded
            if(!motor.WasGroundedLastUpdate)
            {
                //grounded?.Invoke(this, EventArgs.Empty);
            }

        }
        // If just ungrounded
        else if(motor.WasGroundedLastUpdate)
        {
            // Log new slope that was just ungrounded from
            slopeList.Add(motor.GetLastEffectiveGroundNormal());
            setAngularVelocity = true;
            //ungrounded?.Invoke(this, EventArgs.Empty);
        }

        // If ungrounding angular momentum mechanic was triggered
        if(setAngularVelocity)
        {
            // Set angular velocity (if any) using slope tracking info and reset the tracker
            internalAngularVelocity = slopeList.GetAngularVelocity(motor.BaseVelocity, motor.PlanarConstraintAxis, values.ungroundRotationFactor, values.ungroundRotationMinSpeed, values.ungroundRotationMaxSpeed);
            slopeList.Reset();
        }
        ability.PostGroundingUpdate(motor, deltaTime);
    }

    /// <summary>
    /// This is called after the motor has finished everything in its update
    /// </summary>
    /// <param name="motor"> The player's kinematic motor</param>
    /// <param name="deltaTime"> Motor update time </param>
    public void AfterCharacterUpdate(KinematicCharacterMotor motor, float deltaTime)
    {
        ability.AfterCharacterUpdate(motor, deltaTime);
        stateUpdated?.Invoke(this, motor.GetState());

        // Reset Ability and Action Input
        ability.ResetInput();
        action.ResetInput();

        //if(motor.WasGroundedLastUpdate != motor.IsGroundedThisUpdate)
            //Debug.Break();
    }

    /// <summary>
    /// This is called after when the motor wants to know if the collider can be collided with (or if we just go through it)
    /// </summary>
    /// <param name="motor"> The player's kinematic motor</param>
    /// <param name="coll"> The collider being checked </param>
    public bool IsColliderValidForCollisions(KinematicCharacterMotor motor, Collider coll)
    {
        // As of now all colliders are valid
        return ability.IsColliderValidForCollisions(motor, coll);
    }

    /// <summary>
    /// This is called when the motor's ground probing detects a ground hit
    /// </summary>
    /// <param name="motor"> The player's kinematic motor</param>
    /// <param name="hitCollider">The ground collider </param>
    /// <param name="hitNormal"> The ground normal </param>
    /// <param name="hitPoint"> The ground point </param>
    /// <param name="hitStabilityReport"> The ground stability </param>
    public void OnGroundHit(KinematicCharacterMotor motor, Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
    {
        ability.OnGroundHit(motor, hitCollider, hitNormal, hitPoint, ref hitStabilityReport);
    }

    /// <summary>
    /// This is called when the motor's movement logic detects a hit
    /// </summary>
    /// <param name="motor"> The player's kinematic motor</param>
    /// <param name="hitCollider"> The hit collider </param>
    /// <param name="hitNormal"> The hit normal </param>
    /// <param name="hitPoint"> The hit point </param>
    /// <param name="hitStabilityReport"> The hit stability </param>
    public void OnMovementHit(KinematicCharacterMotor motor, Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
    {   
        if(motor.IsGroundedThisUpdate && hitStabilityReport.IsStable && motor.BaseVelocity.magnitude > values.attachThreshold)
        {
            Vector3 initialCharacterBottomHemiCenter = motor.TransientPosition + (motor.CharacterUp * motor.Capsule.radius);
            /*
            slerpFactor = 300;
            Vector3 smoothedGroundNormal = Vector3.Slerp(motor.CharacterUp, motor.GetEffectiveGroundNormal(), 1 - Mathf.Exp(-slerpFactor * deltaTime));
            */
            motor.SetRotation(Quaternion.FromToRotation(motor.CharacterUp, hitNormal) * motor.TransientRotation);//Quaternion.FromToRotation(motor.CharacterUp, smoothedGroundNormal) * currentRotation;
             motor.SetTransientPosition(initialCharacterBottomHemiCenter + (motor.TransientRotation * Vector3.down * motor.Capsule.radius));           
        }
        // If floor it hit when mid air
        else if (!motor.IsGroundedThisUpdate
        && motor.StableGroundLayers.value == (motor.StableGroundLayers.value | (1 << hitCollider.gameObject.layer))
        && Vector3.Angle(-physics.gravityDirection, hitNormal) <= motor.MaxStableSlopeAngle)
        { 
            foundFloorToReorientTo = true;
        }

        ability.OnMovementHit(motor, hitCollider, hitNormal, hitPoint, ref hitStabilityReport);
    }

    /// <summary>
    /// This is called after every move hit, to give you an opportunity to modify the HitStabilityReport to your liking
    /// </summary>
    /// <param name="motor"> The player's kinematic motor</param>
    /// <param name="hitCollider"> The hit collider </param>
    /// <param name="hitNormal"> The hit normal </param>
    /// <param name="hitPoint"> The hit point </param>
    /// <param name="hitPoint"></param>
    /// <param name="atCharacterPosition"> The character position on hit </param>
    /// <param name="atCharacterRotation"> The character rotation on hit </param>
    /// <param name="hitStabilityReport"> The hit stability </param>
    public void ProcessHitStabilityReport(KinematicCharacterMotor motor, Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport)
    {
        ability.ProcessHitStabilityReport(motor, hitCollider, hitNormal, hitPoint, atCharacterPosition, atCharacterRotation, ref hitStabilityReport);
    }

    /// <summary>
    /// This is called when the character detects discrete collisions (collisions that don't result from the motor's capsuleCasts when moving)
    /// </summary>
    /// <param name="motor"> The player's kinematic motor</param>
    /// <param name="hitCollider"> The detected collider </param>
    public void OnDiscreteCollisionDetected(KinematicCharacterMotor motor, Collider hitCollider)
    {
        ability.OnDiscreteCollisionDetected(motor, hitCollider);
    }

#endregion
    Plane settingPlane = new Plane(Vector3.zero, Vector3.zero);
    bool settingPlaneBreaker = false;
    private void SetCurrentPlane(KinematicCharacterMotor motor, Plane plane, bool breaker = false, bool immediate = false)
    {
        
        if (plane.normal == Vector3.zero || (plane.normal == currentPlane.normal && plane.distance == currentPlane.distance))
            return; 
    
        if (immediate)
        {
            currentPlane = plane;
            motor.BaseVelocity = Vector3.ProjectOnPlane(motor.BaseVelocity, currentPlane.normal).normalized * motor.BaseVelocity.magnitude; 
            motor.PlanarConstraintAxis = plane.normal;
            motor.SetPosition(plane.ClosestPointOnPlane(motor.Transform.position));
            planeChanged?.Invoke(this, new PlaneChangeEventArgs(plane.normal, breaker));
            settingPlane = new Plane(Vector3.zero, Vector3.zero);
            settingPlaneBreaker = false;
        }
        else
        {
            settingPlane = plane;
            settingPlaneBreaker = breaker;
        }
    }

    private void EnterDynamicPlane(KinematicCharacterMotor motor, DynamicPlane dynamicPlane)
    {
        if (currentDynamicPlanes[0] != null && currentDynamicPlanes[0].prioritize)
        {
            currentDynamicPlanes[1] = dynamicPlane;
        }
        else
        {    
            if (currentDynamicPlanes[0] != null)
                currentDynamicPlanes[1] = currentDynamicPlanes[0];

            currentDynamicPlanes[0] = dynamicPlane;

            if (currentPlaneBreaker == null || !motor.IsGroundedThisUpdate)
            {
                Vector3 planeRight = Vector3.Cross(currentDynamicPlanes[0].transform.up, currentPlane.normal);
                float dot = Vector3.Dot(motor.BaseVelocity, planeRight);
                bool movingRight = dot > 0;
                SetCurrentPlane(motor, dynamicPlane.GetClosestPathPlane(motor.Transform.position, movingRight));
            }
        }
    }

    private void ExitDynamicPlane(Collider col)
    {
        if (currentDynamicPlanes[0].gameObject == col.gameObject)
        {
            currentDynamicPlanes[0] = currentDynamicPlanes[1];
            currentDynamicPlanes[1] = null;
        }
        else if (currentDynamicPlanes[1].gameObject == col.gameObject)
        {
            currentDynamicPlanes[1] = null;
        }
    }

    private void EnterPlaneBreaker(KinematicCharacterMotor motor, PlaneBreaker planeBreaker)
    {
        currentPlaneBreaker = planeBreaker;
        brokenPlane = currentPlane;
        Vector3 planeRight = Vector3.Cross(currentDynamicPlanes[0].transform.up, currentPlane.normal);
        float dot = Vector3.Dot(motor.BaseVelocity, planeRight);
        bool movingRight = dot > 0;
        if(motor.IsGroundedThisUpdate)
            SetCurrentPlane(motor, planeBreaker.GetClosestPathPlane(motor.Transform.position, movingRight), true);
        
    }

    private void ExitPlaneBreaker()
    {
        currentPlaneBreaker = null;
        if (currentDynamicPlanes[0] == null)
            currentPlane = brokenPlane;
    }

    private void EnterMovementEffector(MovementEffector effector)
    {
        for (int i = 0; i < effector.movementOverrides.Count; i++)
        {
            AddOverride(effector.movementOverrides[i].item1, effector.movementOverrides[i].item2);
        }

        for (int i = 0; i < effector.physicsOverrides.Count; i++)
        {
            physics.AddOverride(effector.physicsOverrides[i].item1, effector.physicsOverrides[i].item2);
        }

        for (int i = 0; i < effector.actionOverrides.Count; i++)
        {
            action.AddOverride(effector.actionOverrides[i].item1, effector.actionOverrides[i].item2);
        }

        ability.EnterMovementEffector(effector);
    }

    private void ExitMovementEffector(MovementEffector effector)
    {
        for (int i = 0; i < effector.movementOverrides.Count; i++)
        {
            RemoveOverride(effector.movementOverrides[i].item1, effector.movementOverrides[i].item2);
        }

        for (int i = 0; i < effector.physicsOverrides.Count; i++)
        {
            physics.RemoveOverride(effector.physicsOverrides[i].item1, effector.physicsOverrides[i].item2);
        }

        for (int i = 0; i < effector.actionOverrides.Count; i++)
        {
            action.RemoveOverride(effector.actionOverrides[i].item1, effector.actionOverrides[i].item2);
        }

        ability.ExitMovementEffector(effector);
    }

    private void AddAbilityOverride(object sender, AbilityOverrideArgs args)
    {
        for (int i = 0; i < args.movementOverrides.Count; i++)
        {
            AddOverride(args.movementOverrides[i].item1, args.movementOverrides[i].item2);
        }

        for (int i = 0; i < args.physicsOverrides.Count; i++)
        {
            physics.AddOverride(args.physicsOverrides[i].item1, args.physicsOverrides[i].item2);
        }

        for (int i = 0; i < args.actionOverrides.Count; i++)
        {
            action.AddOverride(args.actionOverrides[i].item1, args.actionOverrides[i].item2);
        }
    }

    private void RemoveAbilityOverride(object sender, AbilityOverrideArgs args)
    {
        for (int i = 0; i < args.movementOverrides.Count; i++)
        {
            RemoveOverride(args.movementOverrides[i].item1, args.movementOverrides[i].item2);
        }

        for (int i = 0; i < args.physicsOverrides.Count; i++)
        {
            physics.RemoveOverride(args.physicsOverrides[i].item1, args.physicsOverrides[i].item2);
        }

        for (int i = 0; i < args.actionOverrides.Count; i++)
        {
            action.RemoveOverride(args.actionOverrides[i].item1, args.actionOverrides[i].item2);
        }
    }

    #region debug        
    /// <summary>
    /// Currently used for debugging inputs
    /// </summary>
    public void ResetState(KinematicCharacterMotor motor)
    {
        // Resets the the motor state (used as a makeshift "level restart")
        motor.ApplyState(startState);
        SetCurrentPlane(motor, startPlane);
    }
    #endregion
}
