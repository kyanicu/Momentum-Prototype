
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoxAIDirector : AICharacterDirector
{
    private enum State {wander, goingHome, attacking };
    private State state;
    private Vector3 center;
    private Vector3 direction;
    [SerializeField]
    private float radius;
    [SerializeField]
    private float maxReflectAngleOffset;
    [SerializeField]
    private float searchRadius;


    protected override void Awake()
    {
        base.Awake();
        center = transform.position;
    }

    protected override void Start()
    {
        base.Start();
        Vector2 randomPlanarDirection = UnityEngine.Random.insideUnitCircle.normalized;
        direction = movement.GetVelocityFromPlanarVelocity(randomPlanarDirection);

        combat.AttackFinished += () =>
        {
            if (combat.currentAttack == "LoxSwoop")
                GetComponent<CharacterAnimation>().animationRootRotation = Quaternion.FromToRotation(transform.right, Vector3.ProjectOnPlane(direction, movement.upWorldOrientation).normalized);

        };
    }

    protected override void OnEnable()
    {
        base.OnEnable();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
    }
    
#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
    }
#endif

    protected override void RegisterControl()
    {
        switch (state)
        {
            case State.wander:
                Wander();
                break;
            case State.goingHome:
                GoHome();
                break;
            case State.attacking:
                Attacking();
                break;
        }

    }

    private void Attacking()
    {
        if (combat.attackState == AttackState.FINISHED)
        {
            GoHome();
            state = State.goingHome;
        }
    }

    private void Wander()
    {
        Vector3 pointOffradius = center + (transform.position - center).normalized * radius;
        if ((transform.position - center).sqrMagnitude >= radius*radius)
        {
            if ((transform.position - center).sqrMagnitude >= (radius+1)*(radius+1))
            {
                state = State.goingHome;
                Debug.Log("I'm gonna go home and say the N Word.");
                return;
            }

            movement.position = pointOffradius;
            direction = Quaternion.Euler(movement.forward * UnityEngine.Random.Range(-maxReflectAngleOffset, +maxReflectAngleOffset))
                * ((center - transform.position).normalized);
        }
        
        Collider[] hits = Physics.OverlapSphere(transform.position, searchRadius, LayerMask.GetMask("Player"), QueryTriggerInteraction.Ignore);
        if (hits.Length > 0)
        {
            animation.animationRootRotation = Quaternion.FromToRotation(movement.right, Vector3.ProjectOnPlane(hits[0].transform.position - this.transform.position, movement.forward).normalized);
            if ((animation.animationRootRotation * Vector3.forward).y < 0)
                animation.animationRootRotation = Quaternion.FromToRotation(Vector3.down, Vector3.up) * animation.animationRootRotation;

            combat.control.SetAttack("LoxSwoop");
            Attacking();
            state = State.attacking;
            return;
        }
        
        (movementActionControl as SimpleMovementActionControl).rawMaxMove = direction;
        animation.animationRootRotation = Quaternion.FromToRotation(movement.right, direction.x * movement.right);
    
    }

    private void GoHome()
    {
        if((transform.position - center).sqrMagnitude <= 1)
        {
            state = State.wander;
            return;
        }
        
        direction = (center - transform.position).normalized;
        
        (movementActionControl as SimpleMovementActionControl).rawMaxMove = direction;
        animation.animationRootRotation = Quaternion.FromToRotation(movement.right, direction.x * movement.right);
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 gizmoCenter = Application.isPlaying ? center : transform.position;
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(gizmoCenter, radius);
    }
}
