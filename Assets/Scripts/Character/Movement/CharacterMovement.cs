using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CharacterMovement : Movement
{
    public abstract Vector3 groundNormal { get; }
    public abstract Vector3 lastGroundNormal { get; }
    public abstract bool isGroundedThisUpdate { get; }
    public abstract bool wasGroundedLastUpdate { get; }

    /// <summary>
    /// The component for handling player physics that occur as a result of environment
    /// Adds acceleration to velocity each physics tick
    /// </summary>
    protected PlayerMovementPhysics physics;
    /// <summary>
    /// The component for handling player actions directly related to movement as a result of player input
    /// Adds acceleration to velocity each physics tick
    /// </summary>
    protected PlayerMovementAction action;

    protected virtual void Awake()
    {
        physics = GetComponent<PlayerMovementPhysics>();
        action = GetComponent<PlayerMovementAction>();
    }

    public virtual void Flinch()
    {
        action.Flinch();
    }

    public abstract void ForceUnground();

    
}
