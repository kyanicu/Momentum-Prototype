using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(SimpleMovementAction))]
public class EnemyLoxDirector : CharacterDirector
{

    [SerializeField]
    private float areaRadius;

    private Vector3 areaCenter;

    private Vector3 currentDirection;

    private SimpleMovementActionControl actionControl;

    protected override void Awake()
    {
        base.Awake();
        areaCenter = transform.position;

        currentDirection = transform.rotation * Random.insideUnitCircle.normalized;
    }

    void Start()
    {
        actionControl = GetComponent<SimpleMovementAction>().control;
    }

    void OnEnable()
    {
        EnableControl();
    }

    void OnDisable()
    {
        DisableControl();
    }

    void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying)
            areaCenter = transform.position;

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(areaCenter, areaRadius);
    }

    protected override void RegisterControl()
    {
        SetMovementActionControl();
        SetCombatControl();
    }

    private void SetCombatControl()
    {

    }

    private void SetMovementActionControl()
    {
        /*
        Vector3 direction = Vector3.zero;

        if (Keyboard.current.iKey.isPressed)
            direction += transform.up;

        if (Keyboard.current.kKey.isPressed)
            direction -= transform.up;

        if (Keyboard.current.lKey.isPressed)
            direction += transform.right;

        if (Keyboard.current.jKey.isPressed)
            direction -= transform.right;

        direction.Normalize();
        */

        if ((areaCenter - movement.position).sqrMagnitude >= areaRadius * areaRadius)
        {
            movement.position = areaCenter + (movement.position - areaCenter).normalized * areaRadius;
            currentDirection = Quaternion.AngleAxis(Random.Range(-45f, 45f), movement.forward) * (-currentDirection);
        }

        actionControl.rawMaxMove = currentDirection;
    }

}
