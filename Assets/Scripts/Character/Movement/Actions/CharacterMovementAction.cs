using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Struct that holds information on player input
/// Ensures any value found on Monobehavior.Update() will be handled when appropriate for the motor without being overwritten by a zeroed value
/// </summary>
public interface MovementActionControl
{
    /// <summary>
    /// Reset to default values
    /// </summary>
    void Reset();
}

public abstract class CharacterMovementAction : MonoBehaviour
{

    /// <summary>
    /// Holds and maintains input info
    /// </summary>
    public MovementActionControl control;

    private float _facingDirection;
    public float facingDirection
    { 
        get
        { 
            return _facingDirection; 
        } 
        protected set
        {
            if (value != _facingDirection)
            {
                _facingDirection = value;
                animation?.SetFacingDirection(_facingDirection);
            };
        } 
    }

    protected Vector3 bufferedUngroundedNormal;

    new protected CharacterAnimation animation;

    protected CharacterMovement movement;

    protected CharacterMovementPhysics physics;

    protected virtual void Awake()
    {
        movement = GetComponent<CharacterMovement>();
        physics = GetComponent<CharacterMovementPhysics>();
        animation = GetComponent<CharacterAnimation>();

        facingDirection = +1;
    }

    protected virtual void Start()
    {

    }

    /// <summary>
    /// Updates the reference rotation based on intended character actions
    /// </summary>
    /// <param name="currentRotation"> Reference to the character's rotation</param>
    /// <param name="currentAngularVelocity"> Reference to the character's angular velocity</param>
    /// <param name="movement"> The character's movement</param>
    /// <param name="deltaTime"> Motor update time</param>
    public virtual void UpdateRotation(ref Quaternion currentRotation, ref Vector3 currentAngularVelocity, float deltaTime) { }

    /// <summary>
    /// Updates the reference velocity based on intended player actions
    /// </summary>
    /// <param name="currentVelocity"> Reference to the character's velocity</param>
    /// <param name="movement"> The character's movement </param>
    /// <param name="gravityDirection"> The direction of gravity </param>
    /// <param name="physicsOverride"> Determines overrides to player physics values </param>
    /// <param name="deltaTime"> Motor update time</param>
    public abstract void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime);

    public virtual void Flinch()
    {
        control.Reset();
    }

    public void BufferUngroundedNormal(Vector3 ungroundedNormal)
    {
        bufferedUngroundedNormal = ungroundedNormal;
    }

    public void ClearUngroundedBuffer()
    {
        bufferedUngroundedNormal = Vector3.zero;
    }

}
