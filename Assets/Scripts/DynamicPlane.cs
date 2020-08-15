using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;

public class DynamicPlane : PathCreator
{
    Vector3 prevPosition;
    Quaternion prevRotation;
    Vector3 prevScale;

    

    /// <summary>
    /// Initializes tri and plane info
    /// </summary>
    void Awake()
    {
        prevPosition = transform.position;
        prevRotation = transform.rotation;
        prevScale = transform.lossyScale;
    }

    public Plane GetClosestPlane(Vector3 point)
    {
        float shortestSquareDistance = float.PositiveInfinity;
        Plane closestPlane = new Plane();

        return closestPlane;
    }
}
