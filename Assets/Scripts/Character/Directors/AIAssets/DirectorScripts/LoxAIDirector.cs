
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
                    GetComponent<CharacterAnimation>().animationRootRotation = Quaternion.FromToRotation(movement.right, Vector3.ProjectOnPlane(hits[0].transform.position - this.transform.position, movement.forward).normalized);
                    combat.control.SetAttack("LoxSwoop");
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
        (movementActionControl as SimpleMovementActionControl).rawMaxMove = direction;   
    }

    private void GoHome()
    {
        (movementActionControl as SimpleMovementActionControl).rawMaxMove = (center - transform.position).normalized;
        direction = Quaternion.Euler(movement.forward * Random.Range(-maxReflectAngleOffset, +maxReflectAngleOffset)) * ((center - transform.position).normalized);
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 gizmoCenter = Application.isPlaying ? center : transform.position;
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(gizmoCenter, radius);
    }
}
