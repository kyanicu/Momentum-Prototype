using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using PathCreation;

public abstract class PlanePath : MonoBehaviour
{

    [HideInInspector]
    public PathCreator pathCreator;

    protected BezierPath bezierPath { get { return pathCreator.bezierPath; } }
    public VertexPath path { get { return pathCreator.path; } } 

    [SerializeField]
    private bool drawPointNormals;

    [SerializeField]
    private bool confirmNormals;

    protected bool forwardIsRight { get; private set; }
    protected float[] pointTimes { get; private set; }
    protected Plane[] pathPlanes { get; /*private*/ set; }
    /*
    private Plane[] pointForwardPlanes;
    private Plane[] pointBackwardPlanes;
    */

    [SerializeField]
    private bool _prioritize;
    public bool prioritize { get { return _prioritize; } private set { _prioritize = value; } }

    void Awake()
    {
        pathCreator = GetComponent<PathCreator>();
        DetermineForwardDirection();
        SetPointInfo();
    }
    
    void Reset()
    {

        pathCreator = GetComponent<PathCreator>();

        bezierPath.AddSegmentToStart(bezierPath.GetPoint(0) - path.GetDirection(0f, EndOfPathInstruction.Stop));
        bezierPath.SetPoint(1, bezierPath.GetPoint(3));
        bezierPath.SetPoint(2, bezierPath.GetPoint(0));
        bezierPath.AddSegmentToEnd(bezierPath.GetPoint(bezierPath.NumPoints-1) + path.GetDirection(1f, EndOfPathInstruction.Stop));
        bezierPath.SetPoint(bezierPath.NumPoints-2, bezierPath.GetPoint(bezierPath.NumPoints-4));
        bezierPath.SetPoint(bezierPath.NumPoints-3, bezierPath.GetPoint(bezierPath.NumPoints-1));
        
        DetermineForwardDirection();
        SetPointInfo();
    }

    void OnValidate()
    {
        bezierPath.SetPoint(1, bezierPath.GetPoint(3));
        bezierPath.SetPoint(2, bezierPath.GetPoint(0));
        bezierPath.SetPoint(bezierPath.NumPoints-2, bezierPath.GetPoint(bezierPath.NumPoints-4));
        bezierPath.SetPoint(bezierPath.NumPoints-3, bezierPath.GetPoint(bezierPath.NumPoints-1));
        
        if(confirmNormals)
            ConfirmNormals();
        
        DetermineForwardDirection();
        SetPointInfo();

        AdditionalValidate();
    }

    protected abstract void AdditionalValidate();

#if UnityEditor
    void OnDrawGizmosSelected()
    {
        if (Selection.Contains(gameObject))
            OnValidate();

        if(drawPointNormals)
        {
            for (int i = 0; i < path.NumPoints; i++)
            {
                Debug.DrawRay(path.GetPoint(i), path.GetNormal(i), Color.blue);
            }
        }

        AdditionalDrawGizmosSelected();
    }
#endif

    protected abstract void AdditionalDrawGizmosSelected();

    private void SetPointInfo()
    {
        pointTimes = new float[path.NumPoints];
        
        for (int i = 0; i < path.NumPoints; i++)
        {
            if(i == 0)
                pointTimes[i] = 0;
            else if ( i == path.NumPoints-1)
                pointTimes[i] = 1;
            else
                pointTimes[i] = path.GetClosestTimeOnPath(path.GetPoint(i));
        }

        pathPlanes = new Plane[path.NumPoints-1];
        for (int i = 0; i < path.NumPoints-1; i++)
        {
            Vector3 normal = (path.GetNormal(i) + path.GetNormal(i+1)) / 2;
            pathPlanes[i] = new Plane(normal, path.GetPoint(i));
        }

        /*
        pointForwardPlanes = new Plane[path.NumPoints];
        pointBackwardPlanes = new Plane[path.NumPoints];
        for (int i = 0; i < path.NumPoints; i++)
        {
            Vector3 forwardNormal;
            Vector3 backwardNormal;

            if (i == 0)
            {  
                backwardNormal = (path.GetPoint(i + 1) - path.GetPoint(i)).normalized;
                forwardNormal = -backwardNormal;
            }
            else if (i == path.NumPoints-1)
            {
                forwardNormal = (path.GetPoint(i - 1) - path.GetPoint(i)).normalized;
                backwardNormal = -forwardNormal;
            }
            else
            {
                forwardNormal = (path.GetPoint(i - 1) - path.GetPoint(i)).normalized;
                backwardNormal = (path.GetPoint(i + 1) - path.GetPoint(i)).normalized;
            }

            pointForwardPlanes[i] = new Plane(forwardNormal, path.GetPoint(i));
            pointBackwardPlanes[i] = new Plane(backwardNormal, path.GetPoint(i));
        }
        */

        AdditionalSetPointInfo();
    }

    protected abstract void AdditionalSetPointInfo();

    private void ConfirmNormals()
    {
        for (int i = 0; i < path.NumPoints; i++)
        {
            Vector3 normal = path.GetNormal(i);
            Plane plane = new Plane(normal, path.GetPoint(i));
            if (plane.normal == Vector3.zero)
            {
                Debug.Log("Faulty Normal at index: " + i);
                Debug.DrawRay(path.GetPoint(i), transform.forward * 2, Color.red, 5);
            }
        }
        confirmNormals = false;
    }

     /// <summary>
    /// Sets the proper forward direction along the Dynamic Plane
    /// </summary>
    private void DetermineForwardDirection()
    {
        VertexPath path = pathCreator.path;

        Vector3 startPathNormal = path.GetNormal(0);
        Vector3 startPathRight = Vector3.Cross(transform.up, startPathNormal);
        forwardIsRight = Vector3.Dot(startPathRight, path.GetDirectionAtDistance(0, EndOfPathInstruction.Stop)) > 0;
    }

    private int GetPathPlane(float pointTime, bool movingForward)
    {
        if (pointTime == 0)
        {
            return 0;
        }
        else if (pointTime == 1)
        {
            return path.NumPoints - 2;
        }
        else
        {
            for (int i = 0; i < path.NumPoints; i++)
            {
                if (pointTimes[i] == pointTime)
                    return (movingForward) ? i : i - 1 ;
                else if (pointTimes[i] > pointTime)
                    return i-1;
            }
            return 1;
        }
    }

    public Plane GetClosestPathPlane(Vector3 point, bool movingRight)
    {
        float closestPointTime = path.GetClosestTimeOnPath(point);
        return pathPlanes[GetPathPlane(path.GetClosestTimeOnPath(point), movingRight == forwardIsRight)];
    }

    /*
    public Plane GetApproachingPlaneTransition(Vector3 position, bool movingRight, out Plane traversalPlane)
    {   
        bool movingForward = (movingRight == forwardIsRight);
        int traversalPlaneIndex = GetPathPlane(path.GetClosestTimeOnPath(position), movingForward);
        traversalPlane = pathPlanes[traversalPlaneIndex];

        // If off start point
        if (traversalPlaneIndex == 0 && Vector3.Dot(position - path.GetPoint(0), pointForwardPlanes[0].normal) > 0)
        {
            return (movingForward) ? pointForwardPlanes[0] : new Plane(Vector3.zero, float.PositiveInfinity);
        }
        // If off end point
        else if (traversalPlaneIndex == path.NumPoints - 1 && Vector3.Dot(position - path.GetPoint(path.NumPoints - 1), pointBackwardPlanes[path.NumPoints - 1].normal) > 0)
        {
            return (!movingForward) ? pointBackwardPlanes[path.NumPoints - 1] : new Plane(Vector3.zero, float.PositiveInfinity);
        }

        return(movingForward) ? pointForwardPlanes[traversalPlaneIndex + 1] : pointBackwardPlanes[traversalPlaneIndex];
    } 
    */
}