using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;

[RequireComponent(typeof(PathCreator))]
public class DynamicPlane : PlanePath
{

    private Collider col;

    [SerializeField]
    private bool drawPlaneExtents;

    [SerializeField]
    private bool _prioritize;
    public bool prioritize { get { return _prioritize; } private set { _prioritize = value; } }

    public Vector3 up { get { return transform.up; } }

    protected override void AdditionalValidate()
    {
        if (col == null && GetComponent<Collider>() != null)
            col = GetComponent<Collider>();

        bezierPath.GlobalNormalsAngle = 0;
        bezierPath.AlignOnPlane();
        for (int i = 0; i < bezierPath.NumAnchorPoints; i++)
        {
            bezierPath.SetAnchorNormalAngle(i, 0);
        }
    }

    protected override void AdditionalDrawGizmosSelected()
    {
        if (drawPlaneExtents)
        {
            for (int i = 0; i < pathCreator.path.NumPoints; i++)
            {
                if (col != null)
                {
                    Debug.DrawLine(path.GetPoint(i), col.ClosestPoint(path.GetPoint(i) + up * 10000), Color.green);
                    Debug.DrawLine(path.GetPoint(i), col.ClosestPoint(path.GetPoint(i) - up * 10000), Color.green);
                }
                else
                {
                    Debug.DrawLine(path.GetPoint(i), up, Color.green);
                    Debug.DrawLine(path.GetPoint(i), -up, Color.green);
                }
            }
        }
    }

    protected override void AdditionalSetPointInfo() { }
    
}
