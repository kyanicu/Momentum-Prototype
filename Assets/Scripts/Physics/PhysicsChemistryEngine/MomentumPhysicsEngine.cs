using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialForce
{
    private float _magnitude;
    public float magnitude { get { return _magnitude; } set { _magnitude = value; _force = value * direction; } }

    private Vector3 _direction;
    public Vector3 direction { get { return _direction; } set { _direction = value; _force = _magnitude * value; } }

    private Vector3 _force;
    public Vector3 force { get { return _force; } set { _force = value; _magnitude = value.magnitude; _direction = value.normalized; } }

    public MaterialForce complimentaryForce { get; private set; }

    public MaterialForce(float mag, Vector3 dir)
    {
        _magnitude = mag;
        _direction = dir;
        _force = mag * dir;
    }

    public MaterialForce(Vector3 f)
    {
        force = f;
    }

    public void SetForce(float mag, Vector3 dir)
    {
        _magnitude = mag;
        _direction = dir;
        _force = mag * dir;
    }

    public void SetComplimentaryForce(MaterialForce force)
    {
        force.complimentaryForce = this;
        complimentaryForce = force;
    }
}

public class MomentumPhysicsEngine : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
