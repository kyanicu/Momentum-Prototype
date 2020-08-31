using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;

[RequireComponent(typeof(PathCreator))]
public class PlaneBreaker : PlanePath
{

    [SerializeField]
    private bool drawPointUps;
    
    [SerializeField]
    private bool snapPointsToGround;

    private Vector3[] pointUps;

    protected override void AdditionalValidate()
    {
        if (snapPointsToGround)
            SnapPointsToGround();
    }

    protected override void AdditionalDrawGizmosSelected()
    {
        if(drawPointUps)
        {
            for (int i = 0; i < path.NumPoints; i++)
            {
                Debug.DrawRay(path.GetPoint(i), pointUps[i], Color.green);
            }
        }
    }

    protected override void AdditionalSetPointInfo()
    {
        pointUps = new Vector3[path.NumPoints];
        
        for (int i = 0; i < path.NumPoints; i++)
        {
            pointUps[i] = Vector3.Cross(path.GetNormal(i), path.GetDirection(pointTimes[i] + ((forwardIsRight) ? +0.001f : -0.001f) * ((forwardIsRight) ? +1 : -1), EndOfPathInstruction.Stop));
            /*RaycastHit hit;
            if (Physics.Raycast(path.GetPointAtTime(pointTimes[i]), -pointUps[i], out hit, 1, LayerMask.GetMask("Surface"), QueryTriggerInteraction.Ignore))
            {
                pointUps[i] = hit.normal;
                Debug.DrawRay(path.GetPointAtTime(pointTimes[i]), -pointUps[i], Color.red);
                Debug.DrawRay(hit.point, hit.normal, Color.black, 5);
            }*/
        }

        /*pathPlanes = new Plane[path.NumPoints-1];
        for (int i = 0; i < path.NumPoints-1; i++)
        {
            Vector3 normal = (Vector3.ProjectOnPlane(path.GetNormal(i), pointUps[i]).normalized + Vector3.ProjectOnPlane(path.GetNormal(i+1), pointUps[i+1])) / 2;

            pathPlanes[i] = new Plane(normal, path.GetPoint(i));
        }*/
    }

    private void SnapPointsToGround()
    {
        /*
        for (int i = 0; i < bezierPath.NumPoints; i++)
        {
            RaycastHit hit;
            Vector3 up = Vector3.Cross(bezierPath.normal(i), bezierPath + ((forwardIsRight) ? +0.001f : -0.001f) * ((forwardIsRight) ? +1 : -1), EndOfPathInstruction.Stop)).normalized;
            if (Physics.Raycast(bezierPath.GetPoint(i), -pointUps[i], 1))
            {
                
            }
        }
        */
        AdditionalSetPointInfo();
        snapPointsToGround = false;
    }
}
