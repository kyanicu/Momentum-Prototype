using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleMovement : Movement
{
    private Vector3 _velocity;
    public override Vector3 velocity { get { return _velocity; } set { if (kinematicPath == null) _velocity = value; } }

    public override bool usePlaneBreakers { get { return false;} }

    public override bool PlaneBreakValid()
    {
        return false;
    }

    public override void HandlePlaneSettingExtra(Plane plane) { }

    // Update is called once per frame
    void Update()
    {
        position += velocity * Time.deltaTime;
    }
}
