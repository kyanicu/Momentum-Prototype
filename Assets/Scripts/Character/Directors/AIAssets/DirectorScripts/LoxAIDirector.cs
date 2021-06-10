
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoxAIDirector : AICharacterDirector
{

    private Vector3 center;
    private Vector3 direction;
    [SerializeField]
    private float radius;
    [SerializeField]
    private float maxReflectAngleOffset;
    [SerializeField]
    private float searchRadius;

    private bool wandering;

    protected override void Awake()
    {
        base.Awake();
        center = transform.position;
    }

    protected override void Start()
    {
        base.Start();
        Vector2 randomPlanarDirection = Random.insideUnitCircle.normalized;
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
        if(combat.attackState == AttackState.FINISHED)
        {
            if((transform.position-center).sqrMagnitude <= radius * radius)
            {
                Collider[] hits = Physics.OverlapSphere(transform.position, searchRadius, LayerMask.GetMask("Player"), QueryTriggerInteraction.Ignore);
                if( hits.Length > 0)
                {
                    animation.animationRootRotation = Quaternion.FromToRotation(movement.right, Vector3.ProjectOnPlane(hits[0].transform.position - this.transform.position, movement.forward).normalized);
                    if ((animation.animationRootRotation * Vector3.forward).y < 0)
                        animation.animationRootRotation = Quaternion.FromToRotation(Vector3.down, Vector3.up) * animation.animationRootRotation;

                    combat.control.SetAttack("LoxSwoop");

                    wandering = false;
                }
                else
                {
                    Wander();
                }
            }
            else
            {
                GoHome();
            }
        }
    }

    private void Wander()
    {
        wandering = true;

        (movementActionControl as SimpleMovementActionControl).rawMaxMove = direction;
        animation.animationRootRotation = Quaternion.FromToRotation(movement.right, direction.x * movement.right);
    }

    private void GoHome()
    {
        Vector3 pointOffradius = center + (transform.position - center).normalized * radius;
        if (wandering && (transform.position - pointOffradius).sqrMagnitude > 1)
        {
            movement.position = pointOffradius;
            direction = Quaternion.Euler(movement.forward * Random.Range(-maxReflectAngleOffset, +maxReflectAngleOffset)) * ((center - transform.position).normalized);
        }
        else
            direction = (center - transform.position).normalized;

        wandering = false;
        
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
