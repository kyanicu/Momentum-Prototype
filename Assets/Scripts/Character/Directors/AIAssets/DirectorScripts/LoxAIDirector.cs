
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
        Wander();
        //TODO: MAKE ATTACK DUMDUM
        //combat.control.SetAttack("LoxSwoop");
    }

    private void Wander()
    {
        if((transform.position-center).sqrMagnitude > radius * radius)
        {
            transform.position -= (0.25f + (transform.position - center).magnitude - radius) * direction;
            direction = Quaternion.Euler(movement.forward * Random.Range(-maxReflectAngleOffset, +maxReflectAngleOffset)) * (Vector3.Reflect(direction, (center-transform.position).normalized));
        }
        (movementActionControl as SimpleMovementActionControl).rawMaxMove = direction;
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 gizmoCenter = Application.isPlaying ? center : transform.position;
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(gizmoCenter, radius);
    }
}
