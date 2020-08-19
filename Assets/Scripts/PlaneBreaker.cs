using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using PathCreation;

[RequireComponent(typeof(PathCreator))]
public class PlaneBreaker : MonoBehaviour
{

    [HideInInspector]
    public PathCreator pathCreator;

    private BezierPath bezierPath { get { return pathCreator.bezierPath; } }
    public VertexPath path { get { return pathCreator.path; } }

    [SerializeField]
    private bool drawPointUps;
    [SerializeField]
    private bool drawPointNormals;

    
    [SerializeField]
    private bool snapPointsToFloor;

    private bool forwardIsRight;
    private float[] pointTimes;
    private Vector3[] pointUps;

    /// <summary>
    /// Initializes tri and plane info
    /// </summary>
    void Awake()
    {
        pathCreator = GetComponent<PathCreator>();
        DetermineForwardDirection();
        SetPointInfo();
    }

    void Reset()
    {
        pathCreator = GetComponent<PathCreator>();
        DetermineForwardDirection();
        SetPointInfo();

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
        
        if (snapPointsToFloor)
            SnapPointsToFloor();

        bezierPath.SetPoint(1, bezierPath.GetPoint(3));
        bezierPath.SetPoint(2, bezierPath.GetPoint(0));
        bezierPath.SetPoint(bezierPath.NumPoints-2, bezierPath.GetPoint(bezierPath.NumPoints-4));
        bezierPath.SetPoint(bezierPath.NumPoints-3, bezierPath.GetPoint(bezierPath.NumPoints-1));
    }

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
        if(drawPointUps)
        {
            for (int i = 0; i < path.NumPoints; i++)
            {
                Debug.DrawRay(path.GetPoint(i), pointUps[i], Color.green);
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
    }

    private void SetPointInfo()
    {
        pointTimes = new float[path.NumPoints];
        pointUps = new Vector3[path.NumPoints];
        
        for (int i = 0; i < path.NumPoints; i++)
        {
            if(i == 0)
                pointTimes[i] = 0;
            else if ( i == path.NumPoints-1)
                pointTimes[i] = 1;
            else
                pointTimes[i] = path.GetClosestTimeOnPath(path.GetPoint(i));
            pointUps[i] = Vector3.Cross(path.GetNormal(i), path.GetDirection(pointTimes[i] + ((forwardIsRight) ? +0.001f : -0.001f) * ((forwardIsRight) ? +1 : -1), EndOfPathInstruction.Stop));
        }
    }

    private void SnapPointsToFloor()
    {
        /*
        for (int i = 0; i < bezierPath.NumPoints; i++)
        {
            RaycastHit hit;
            Vector3 up = Vector3.Cross(path.GetNormal(i), bezierPath.get(pointTimes[i] + ((forwardIsRight) ? +0.001f : -0.001f) * ((forwardIsRight) ? +1 : -1), EndOfPathInstruction.Stop));;
            if (Physics.Raycast(bezierPath.GetPoint(i), -pointUps[i], 1))
            {
                pat
            }
        }
        */
        SetPointInfo();
        snapPointsToFloor = false;
    }

    public Plane GetClosestPlane(Vector3 flooredPoint)
    {
        float closestPointTime = path.GetClosestTimeOnPath(flooredPoint);
        Vector3 closestPoint = path.GetPointAtTime(closestPointTime, EndOfPathInstruction.Stop);
        Vector3 closestPointNormal = path.GetNormal(closestPointTime, EndOfPathInstruction.Stop);

        return new Plane(closestPointNormal, closestPoint);
    }

    public Plane GetApproachingPlaneTransition(Vector3 flooredPosition, Vector3 moveDirection, bool movingRight, out Plane traversalPlane)
    {
        float closestPointTime = path.GetClosestTimeOnPath(flooredPosition);
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

        Vector3 pointApproaching = (movingForward) ? path.GetPoint(pointAhead) : path.GetPoint(pointBehind);

        Vector3 normal;
        if (!onPoint)
            normal = path.GetNormal(closestPointTime);
        else
            normal = path.GetNormal(closestPointTime + ((movingForward) ? +0.001f : -0.001f), EndOfPathInstruction.Stop);

        traversalPlane = new Plane(normal, closestPoint);
        return new Plane((closestPoint - pointApproaching), pointApproaching);

    } 

}
