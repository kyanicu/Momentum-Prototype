using System.Collections;
using System.Collections.Generic;
using KinematicCharacterController;
using UnityEngine;

public class SimpleMovement : Movement, ISimpleCollidable
{
    SimpleCollision collision;
    PhysicsMover mover;

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
            rotation = rotation = Quaternion.Euler(angularVelocity * Time.deltaTime) * rotation;
    }

    public void UpdateMovement(out Vector3 goalPosition, out Quaternion goalRotation, float deltaTime)
    {
        throw new System.NotImplementedException();
    }

    // Update is called once per frame
    void Update()
    {
        if(rigidbody == null)
        {
            HandleVelocity();
        }
    }

    void FixedUpdate()
    {
        if(rigidbody != null)
        {
            HandleVelocity();
        }
    }
}
