using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(SimpleMovementAction))]
public class EnemyLuposDirector : CharacterDirector
{
    private enum PointMovement { Single, Loop, PingPong }

    [SerializeField]
    private Vector3[] points;
    [SerializeField]
    PointMovement pointMovement;
    [SerializeField]
    private float pointOffsetRadius = 0.5f;

    Vector3 initialPosition;
    private int currentPoint;
    private int pointCount;
    private int increment;

    private SimpleMovementActionControl actionControl;

    protected override void Awake()
    {
        base.Awake();

        currentPoint = 0;
        pointCount = points.Length;
        increment = +1;
        initialPosition = transform.position;
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

    protected override void RegisterControl()
    {
        SetMovementActionControl();
        SetCombatControl();
    }

    void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying)
            initialPosition = transform.position;

        Gizmos.color = Color.green - Color.black * 0.5f;
        foreach (Vector3 point in points)
        {
            Gizmos.DrawSphere(point + initialPosition, pointOffsetRadius);
        }
    }

    private void SetCombatControl()
    {

    }

    private void SetMovementActionControl()
    {
        if (increment != 0)
        {
            if ((points[currentPoint] + initialPosition - movement.position).sqrMagnitude < pointOffsetRadius * pointOffsetRadius)
                currentPoint += increment;

            if (currentPoint == pointCount || currentPoint == -1)
            {
                switch (pointMovement)
                {
                    case (PointMovement.PingPong):
                        increment *= -1;
                        currentPoint = pointCount - 2;
                        break;
                    case (PointMovement.Loop):
                        currentPoint = 0;
                        break;
                    case (PointMovement.Single):
                        increment = 0;
                        break;
                }
            }

            actionControl.maxMove = Mathf.Sign(Vector3.Dot(points[currentPoint] + initialPosition - movement.position, movement.right));
        }
    }
}
