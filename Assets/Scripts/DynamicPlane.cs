using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using PathCreation;

[RequireComponent(typeof(PathCreator))]
public class DynamicPlane : MonoBehaviour
{

    [HideInInspector]
    public PathCreator pathCreator;
    public Collider col;

    [SerializeField]
    private bool drawPlaneExtents;

    [SerializeField]
    private bool _prioritize;
    public bool prioritize { get { return _prioritize; } private set { _prioritize = value; } }

    private BezierPath bezierPath { get { return pathCreator.bezierPath; } }
    public VertexPath path { get { return pathCreator.path; } }
    public Vector3 up { get { return transform.up; } }

    Plane pathFloor;

    private bool forwardIsRight;
    private float[] pointTimes;

    /// <summary>
    /// Initializes tri and plane info
    /// </summary>
    void Awake()
    {
        DetermineForwardDirection();
        SetPointTimes(); 
    }

    void Reset()
    {
        pathCreator = GetComponent<PathCreator>();
        col = GetComponent<Collider>();

        bezierPath.AddSegmentToStart(bezierPath.GetPoint(0) - path.GetDirection(0f, EndOfPathInstruction.Stop));
        bezierPath.SetPoint(1, bezierPath.GetPoint(3));
        bezierPath.SetPoint(2, bezierPath.GetPoint(0));
        bezierPath.AddSegmentToEnd(bezierPath.GetPoint(bezierPath.NumPoints-1) + path.GetDirection(1f, EndOfPathInstruction.Stop));
        bezierPath.SetPoint(bezierPath.NumPoints-2, bezierPath.GetPoint(bezierPath.NumPoints-4));
        bezierPath.SetPoint(bezierPath.NumPoints-3, bezierPath.GetPoint(bezierPath.NumPoints-1));
    }

    /// <summary>
    /// Ensures the path is appropriate as a Dynamic Plane
    /// </summary>
    void OnValidate()
    {
        if (pathCreator == null)
            pathCreator = gameObject.AddComponent<PathCreator>();
        if (col == null && GetComponent<Collider>() != null)
            col = GetComponent<Collider>();

        bezierPath.GlobalNormalsAngle = 0;
        bezierPath.AlignOnPlane();
        for (int i = 0; i < bezierPath.NumAnchorPoints; i++)
        {
            bezierPath.SetAnchorNormalAngle(i, 0);
        }

        pathFloor = new Plane(up, transform.position);

        bezierPath.SetPoint(1, bezierPath.GetPoint(3));
        bezierPath.SetPoint(2, bezierPath.GetPoint(0));
        bezierPath.SetPoint(bezierPath.NumPoints-2, bezierPath.GetPoint(bezierPath.NumPoints-4));
        bezierPath.SetPoint(bezierPath.NumPoints-3, bezierPath.GetPoint(bezierPath.NumPoints-1));
    }

    void OnDrawGizmosSelected()
    {
        if (Selection.Contains(gameObject))
            OnValidate();

        if (drawPlaneExtents)
        {
            for (int i = 0; i < pathCreator.path.NumPoints; i++)
            {
                if (col != null)
                {
                    Debug.DrawLine(path.GetPoint(i), col.ClosestPoint(path.GetPoint(i) + transform.up * 10000), Color.green);
                    Debug.DrawLine(path.GetPoint(i), col.ClosestPoint(path.GetPoint(i) - transform.up * 10000), Color.green);
                }
                else
                {
                    Debug.DrawLine(path.GetPoint(i), transform.up, Color.green);
                    Debug.DrawLine(path.GetPoint(i), -transform.up, Color.green);
                }
            }
        }
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
        
        /*
        Debug.DrawRay(path.GetPoint(0), startPathNormal, Color.blue, 5);
        Debug.DrawRay(path.GetPoint(0), transform.up, Color.green, 5);
        Debug.DrawRay(path.GetPoint(0) + transform.up * 0.5f, startPathRight, Color.red, 5);
        Debug.DrawRay(path.GetPoint(0) + transform.up, path.GetDirectionAtDistance(0.01f, EndOfPathInstruction.Stop), Color.yellow, 5);
        Debug.Log(path.GetNormal(0));
        Debug.Log(forwardIsRight);
        Debug.Break();
        */
    }

    private void SetPointTimes()
    {
        pointTimes = new float[path.NumPoints];
        
        pointTimes[0] = 0;
        for (int i = 1; i < path.NumPoints-1; i++)
        {
            pointTimes[i] = path.GetClosestTimeOnPath(path.GetPoint(i));
        }
        pointTimes[path.NumPoints-1] = 1;
    }

    public Plane GetClosestPlane(Vector3 point)
    {
        float closestPointTime = path.GetClosestTimeOnPath(pathFloor.ClosestPointOnPlane(point));
        Vector3 closestPoint = path.GetPointAtTime(closestPointTime, EndOfPathInstruction.Stop);
        Vector3 closestPointNormal = path.GetNormal(closestPointTime, EndOfPathInstruction.Stop);

        //Debug.DrawLine(point, closestPoint, Color.yellow);
        //Debug.DrawRay(closestPoint, closestPointNormal, Color.blue);
        //Debug.DrawLine(closestPoint, col.ClosestPoint(closestPoint + transform.up * 10000), Color.green);
        //Debug.DrawLine(closestPoint, col.ClosestPoint(closestPoint - transform.up * 10000), Color.green);

        return new Plane(closestPointNormal, closestPoint);
    }

    public Plane GetApproachingPlaneTransition(Vector3 position, Vector3 moveDirection, bool movingRight, out Plane traversalPlane)
    {
        Plane pathFloor = new Plane(up, transform.position);
        float closestPointTime = path.GetClosestTimeOnPath(pathFloor.ClosestPointOnPlane(position));
        Vector3 closestPoint = path.GetPointAtTime(closestPointTime, EndOfPathInstruction.Stop);
        bool movingForward = movingRight == forwardIsRight;

        // If at or behind/ahead start/end and moving back/forward
        if ((closestPointTime == 0 && !movingForward) || (closestPointTime == 1 && movingForward))
        {
            traversalPlane = new Plane(path.GetNormal(closestPointTime, EndOfPathInstruction.Stop), closestPoint);
            return new Plane(Vector3.zero, float.PositiveInfinity);
        }

        bool onPoint = false;
        int pointBehind = -1;
        int pointAhead = 2;
        for (int i = 0; i < path.NumPoints; i++)
        {
            if (pointTimes[i] == closestPointTime)
                onPoint = true;
            else if (pointTimes[i] < closestPointTime && (pointBehind == -1 || pointTimes[pointBehind] < pointTimes[i]))
                pointBehind = i;
            else if (pointTimes[i] > closestPointTime)// && pointTimes[pointAhead] > pointTimes[i])
            { 
                pointAhead = i;
                break;
            }
        }

        Vector3 pointApproaching = (movingForward) ? path.GetPoint(pointAhead) : path.GetPoint(pointBehind) ;

        Vector3 normal;
        if (!onPoint)
            normal = path.GetNormal(closestPointTime);
        else
            normal = path.GetNormal(closestPointTime + ((movingForward) ? +0.001f : -0.001f), EndOfPathInstruction.Stop);

        traversalPlane = new Plane(normal, closestPoint);
        return new Plane((closestPoint - pointApproaching), pointApproaching);

    } 

}
