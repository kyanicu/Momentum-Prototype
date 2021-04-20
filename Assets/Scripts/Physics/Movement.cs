using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Movement : MonoBehaviour, IDynamicPlaneConstrainable
{
    public virtual Vector3 position { get { return transform.position; } set { transform.position = value; } }
    public virtual Quaternion rotation { get { return transform.rotation; } set { transform.rotation = value; } }
    public virtual Vector3 velocity { get; set; }
    public virtual Vector3 angularVelocity { get; set; }

    public virtual Vector3 forward { get { return transform.forward; } }

    public virtual bool usePlaneBreakers { get { return false; } }
    public virtual bool reaffirmCurrentPlane { get { return true; } }

    public virtual void HandlePlaneSettingExtra(Plane plane) { }
    public virtual bool PlaneBreakValid() { return false; }

    DynamicPlaneConstraint planeConstraint;

    public Plane currentPlane { get { return planeConstraint ? planeConstraint.currentPlane : new Plane(forward, position); } }
    
    new protected Rigidbody rigidbody;

    protected virtual void Awake()
    {
        SetComponentReferences();
    }

    protected virtual void Start()
    {
        planeConstraint?.SetCurrentPlane(new Plane(forward, position));
    }

    protected virtual void SetComponentReferences()
    {
        rigidbody = GetComponent<Rigidbody>();
        planeConstraint = GetComponent<DynamicPlaneConstraint>();
    }

    public virtual void ZeroVelocity()
    {
        velocity = Vector3.zero;
    }

    public virtual void AddImpulse(Vector3 impulse)
    {
        velocity += impulse;
    }

    public virtual void AddImpulseAtPoint(Vector3 impulse, Vector3 point)
    {
        velocity += impulse;
    }
}
