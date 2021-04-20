using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CharacterMovementPhysics : MonoBehaviour
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
    /// The currently overriden physics values for the current motor update
    /// </summary>
    public PhysicsNegations negations;

    /// <summary>
    /// Getter for the current direction of gravity
    /// </summary>
    public virtual Vector3 gravityDirection { get { return Vector3.down; } }

    protected CharacterMovement movement;

    protected virtual void Awake()
    {
        movement = GetComponent<CharacterMovement>();
        negations = new PhysicsNegations();
    }

    /// <summary>
    /// Updates the reference rotation based on physics calculations
    /// </summary>
    /// <param name="currentRotation"> Reference to the character's rotation</param>
    /// <param name="currentAngularVelocity"> Reference to the character's angular velocity</param>
    /// <param name="movement"> The character's movement</param>
    /// <param name="deltaTime"> update time</param>
    public virtual void UpdateRotation(ref Quaternion currentRotation, ref Vector3 currentAngularVelocity, float deltaTime) { }

    /// <summary>
    /// Updates the reference velocity based on physics calculations
    /// </summary>
    /// <param name="currentVelocity"> Reference to the character's velocity</param>
    /// <param name="movement"> The character's movement</param>
    /// <param name="deltaTime"> update time</param>
    public abstract void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime);
}
