using System.Collections;
using System.Collections.Generic;
using KinematicCharacterController;
using UnityEngine;

public class SimpleMovement : Movement, ISimpleCollidable
{
    SimpleCollision collision;
    PhysicsMover mover;

    private Vector3 internalPositionAddition;
    private Quaternion internalRotationMultiplication;

    public override Vector3 position 
    {
        set
        {
            if (mover && mover.isActiveAndEnabled)
                /*internalPositionAddition = value - position*/ mover.SetPosition(value);
            else 
                base.position = value;
        } 
    }
    public override Quaternion rotation 
    {
        set 
        { 
            if (mover && mover.isActiveAndEnabled)
                /*internalRotationMultiplication = value * Quaternion.Inverse(rotation)*/ mover.SetRotation(value);
            else 
                base.rotation = value; 
        } 
    }

    protected override void Awake()
    {
        base.Awake();
        collision = GetComponent<SimpleCollision>();
        mover = GetComponent<PhysicsMover>();
    }

    void HandleVelocity()
    {
        if (velocity != Vector3.zero)
            position += velocity * Time.deltaTime;
        if (angularVelocity != Vector3.zero)
            rotation = Quaternion.Euler(angularVelocity * Time.deltaTime) * rotation;
    }

    public void UpdateMoveBy(out Vector3 moveBy, out Quaternion rotateBy, float deltaTime)
    {
        moveBy = velocity * deltaTime; // + internalPositionAddition;
        rotateBy = Quaternion.Euler(angularVelocity * deltaTime); // * internalRotationMultiplication;

        //internalPositionAddition = Vector3.zero;
        //internalRotationMultiplication = Quaternion.identity;
    }

    public void HandleMovementCollision(Collider hitCollider, Vector3 hitNormal)
    {
        hitNormal = Vector3.ProjectOnPlane(hitNormal, forward).normalized;
        velocity = Vector3.ProjectOnPlane(velocity, hitNormal);
    }

    public void HandleStaticCollision(Collider hitCollider, Vector3 hitNormal) { }

    public void MoveDone(float deltaTime) { }

    // Update is called once per frame
    void Update()
    {
        if (!(collision && collision.isActiveAndEnabled) && rigidbody == null)
        {
            HandleVelocity();
        }
    }

    void FixedUpdate()
    {
        if (!(collision && collision.isActiveAndEnabled) && rigidbody != null)
        {
            HandleVelocity();
        }
    }
}
