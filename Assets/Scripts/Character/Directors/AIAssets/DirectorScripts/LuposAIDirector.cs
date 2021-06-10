
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LuposAIDirector : AICharacterDirector
{
    private enum State { wander, goingHome, attacking };
    private State state;
    private Vector3 center;
    private float direction;
    [SerializeField]
    private float radius;
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
        direction = +1;

        
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
            Debug.Log("Go home");
            GoHome();
            state = State.goingHome;
        }
    }

    private void Wander()
    {
        Vector3 pointOffradius = center + (transform.position - center).normalized * radius;
        if ((transform.position - center).sqrMagnitude >= radius * radius)
        {
            if ((transform.position - center).sqrMagnitude >= (radius + 1) * (radius + 1))
            {
                state = State.goingHome;
                Debug.Log("I'm gonna go home and say the N Word.");
                return;
            }

            movement.position = pointOffradius;
            direction = -direction;
            GetComponent<CharacterMovementAction>().facingDirection = direction;
        }
        bool hit = Physics.Raycast(transform.position, movement.right*GetComponent<CharacterMovementAction>().facingDirection, searchRadius,LayerMask.GetMask("Player"), QueryTriggerInteraction.Ignore);
        if (hit)
        {
                    
            combat.control.SetAttack("LuposRawr");
            Attacking();
            state = State.attacking;
            return;
        }
        
        (movementActionControl as SimpleMovementActionControl).maxMove= direction;
        
    }

    private void GoHome()
    {
        if ((transform.position - center).sqrMagnitude <= 1)
        {
            state = State.wander;
   
            return;
        }

        direction = Mathf.Sign(Vector3.Dot(movement.right,center-transform.position));
        GetComponent<CharacterMovementAction>().facingDirection = direction;

        (movementActionControl as SimpleMovementActionControl).maxMove = direction;
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 gizmoCenter = Application.isPlaying ? center : transform.position;
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(gizmoCenter, radius);
    }
}
