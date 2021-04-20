using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MomentumRigidbody : Movement, IDamageable
{
    public override Vector3 position { get { return rigidbody.position; } set { rigidbody.MovePosition(value); } }
    public override Quaternion rotation { get { return rigidbody.rotation; } set { rigidbody.MoveRotation(value); } }
    public override Vector3 velocity { get { return rigidbody.velocity; } set { rigidbody.velocity = value; } }
    public override Vector3 angularVelocity { get { return rigidbody.angularVelocity; } set { rigidbody.angularVelocity = value; } }

    public override bool reaffirmCurrentPlane { get { return true; } }
    public override bool usePlaneBreakers { get { return true; } }
    [SerializeField]
    private float planeBreakThreshold = 5;

    private HashSet<Collider> contactedColliders = new HashSet<Collider>();

    // Start is called before the first frame update
    protected override void Awake()
    {
        base.Awake();
        rigidbody.isKinematic = false;
    }

    void OnCollisionEnter(Collision col)
    {
        contactedColliders.Add(col.collider);
    }

    void OnCollisionExit(Collision col)
    {
        contactedColliders.Remove(col.collider);
    }

    public override void AddImpulse(Vector3 impulse)
    {
        rigidbody.AddForce(impulse, ForceMode.Impulse);
    }

    public override void AddImpulseAtPoint(Vector3 impulse, Vector3 point)
    {
        rigidbody.AddForceAtPosition(impulse, point, ForceMode.Impulse);
    }

    public override bool PlaneBreakValid()
    {
        return contactedColliders.Count > 0 && velocity.sqrMagnitude >= planeBreakThreshold * planeBreakThreshold;
    }

    public void ActivateIFrames(float iFrameTimeOverride = 0) { }

    public void Flinch() { }

    public void ForceUnground() { }

    public void Halt()
    {
        ZeroVelocity();
    }

    public void Stun(float stunTime) { }

    public void TakeDamage(float damage) { }

    public void TakeDynamicKnockback(Vector3 knockback)
    {
        AddImpulse(knockback);
    }

    public void TakeDynamicKnockbackWithTorque(Vector3 knockback, Vector3 atPoint)
    {
        AddImpulseAtPoint(knockback, atPoint);
    }

    public void TakeKinematicKnockback(Vector3 knockback, float time)
    {
        AddImpulse(knockback);
    }

    public HitValidity ValidHit(Hitbox hitbox, Hurtbox hurtbox)
    {
        return HitValidity.VALID;
    }
}
