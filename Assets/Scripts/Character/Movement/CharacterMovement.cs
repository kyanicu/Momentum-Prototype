using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class CharacterMovement : Movement
{

    protected struct KinematicPath
    {
        public Vector3 velocity;
        
        public Vector3 velocityAfter;

        public Coroutine timer;
                
    }

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

    protected KinematicPath? kinematicPath = null;

    protected override void Awake()
    {
        base.Awake();
        physics = GetComponent<PlayerMovementPhysics>();
        action = GetComponent<PlayerMovementAction>();
    }

    public virtual void SetKinematicPath(Vector3 vel, float time)
    {
        if (kinematicPath != null)
            EndKinematicPath();

        KinematicPath kp = new KinematicPath 
        { 
            velocity = vel,
            velocityAfter = velocity,
            timer = StartCoroutine(KinematicPathCoroutine(time)),
        };
        
        kinematicPath = kp;

        velocity = vel;
    }

    public virtual IEnumerator KinematicPathCoroutine(float time)
    {
        yield return new WaitForSeconds(time);
        EndKinematicPath();
    }

    public virtual void EndKinematicPath()
    {
        StopCoroutine(kinematicPath.Value.timer);
        velocity = kinematicPath.Value.velocityAfter;
        kinematicPath = null;
    }

    public virtual void Flinch()
    {
        action.Flinch();
    }

    public abstract void ForceUnground();

    
}
