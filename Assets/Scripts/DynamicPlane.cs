using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;

public class DynamicPlane : PathCreator
{
    private float forwardDirection;

    /// <summary>
    /// Initializes tri and plane info
    /// </summary>
    void Awake()
    {
        DetermineForwardDirection();
    }

    void OnValidate()
    {
        bezierPath.GlobalNormalsAngle = 0;
        bezierPath.AlignOnPlane();
        for (int i = 0; i < bezierPath.NumAnchorPoints; i++)
        {
            bezierPath.SetAnchorNormalAngle(i, 0);
        }

    }

    private void DetermineForwardDirection()
    {
        Vector3 startNormal = path.GetNormal(0);
    }

}
