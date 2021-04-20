using System.Collections;
using System.Collections.Generic;
using KinematicCharacterController;
using UnityEngine;

public interface ISimpleCollidable
{
    void UpdateMoveBy(out Vector3 moveBy, out Quaternion rotateBy, float deltaTime);
    void HandleMovementCollision(Collider hitCollider, Vector3 hitNormal);
    void HandleStaticCollision(Collider hitCollider, Vector3 hitNormal);
    void MoveDone(float deltaTime);
}

[RequireComponent(typeof(Rigidbody), typeof(ISimpleCollidable))]
public class SimpleCollision : PhysicsMover, IMoverController
{

    //[SerializeField]
    //private bool continousDetection = false;

    public bool alwaysCheckCollision = false;

    private ISimpleCollidable collidable;

    new private Collider collider;

    private LayerMask mask;

    protected override void Reset()
    {
        base.Reset();
    }

    protected override void OnValidate()
    {
        base.OnValidate();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
    }

    protected override void Awake()
    {
        base.Awake();
        collidable = GetComponent<ISimpleCollidable>();
        MoverController = this;
        collider = GetComponent<Collider>();
    }

    void Start()
    {
        mask = GameManager.Instance.GetLayerMask(gameObject.layer);
    }

    public Dictionary<Collider, Vector3> ResolveOverlaps(ref Vector3 pos, Quaternion rot, bool movedInto, Vector3 alongMoveby = default)
    {
        Dictionary<Collider, Vector3> allOverlapsAndNormals = new Dictionary<Collider, Vector3>();
        Vector3 resolutionDirection = Vector3.up;
        float resolutionDistance = 0f;
        int iterationsMade = 0;
        bool overlapSolved = false;
        Collider[] overlaps = new Collider[16];
        while (iterationsMade < 3 && !overlapSolved)
        {
            int nbOverlaps = Physics.OverlapBoxNonAlloc(collider.bounds.center, collider.bounds.extents, overlaps, Quaternion.identity, mask, QueryTriggerInteraction.Ignore);
            if (nbOverlaps > 0)
            {
                // Solve overlaps that aren't against dynamic rigidbodies or physics movers
                for (int i = 0; i < nbOverlaps; i++)
                {
                    if (collider != overlaps[i])
                    {
                        // Process overlap
                        Transform overlappedTransform = overlaps[i].GetComponent<Transform>();
                        if (Physics.ComputePenetration(collider, pos, rot, overlaps[i], overlappedTransform.position, overlappedTransform.rotation,
                                out resolutionDirection, out resolutionDistance))
                        {
                            allOverlapsAndNormals[overlaps[i]] = resolutionDirection;
                            // Resolve along obstruction direction
                            Vector3 originalResolutionDirection = resolutionDirection;

                            // Solve overlap
                            Vector3 prevPos = pos - alongMoveby;
                            Vector3 resolutionMovement = resolutionDirection * (resolutionDistance + 0.001f);
                            pos += resolutionMovement;

                            if (alongMoveby != default)
                            {
                                Plane hitPlane = new Plane(resolutionDirection, pos);
                                float enter;
                                if (hitPlane.Raycast(new Ray(prevPos, alongMoveby.normalized), out enter))
                                {
                                    pos = prevPos + alongMoveby * enter;
                                }
                            }

                            if (movedInto)
                                collidable.HandleMovementCollision(overlaps[i], resolutionDirection);
                            else
                                collidable.HandleStaticCollision(overlaps[i], resolutionDirection);

                            break;
                        }
                    }
                }
            }
            else
            {
                overlapSolved = true;
            }

            iterationsMade++;
        }
        return allOverlapsAndNormals;
    }

    /// <summary>
    /// Processes movement projection upon detecting a hit
    /// </summary>
    private void InternalHandleVelocityProjection(Vector3 hitNormal, Vector3 obstructionNormal, Vector3 originalDirection,
        ref MovementSweepState sweepState, ref Vector3 previousObstructionNormal, ref Vector3 transientVelocity, ref float remainingMovementMagnitude, ref Vector3 remainingMovementDirection)
    {
        if (transientVelocity.sqrMagnitude <= 0f)
        {
            return;
        }

        Vector3 initialVelocity = transientVelocity;

        // Handle projection
        if (sweepState == MovementSweepState.Initial)
        {
            transientVelocity = Vector3.ProjectOnPlane(transientVelocity, obstructionNormal);
            sweepState = MovementSweepState.AfterFirstHit;
        }
        // Blocking crease handling
        else if (sweepState == MovementSweepState.AfterFirstHit)
        {
            Vector3 directionRight = Vector3.Cross(transform.up, originalDirection).normalized;
            bool dotPreviousIsPositive = Vector3.Dot(previousObstructionNormal, directionRight) >= 0f;
            bool dotCurrentIsPositive = Vector3.Dot(obstructionNormal, directionRight) >= 0f;

            if (dotPreviousIsPositive != dotCurrentIsPositive)
            {
                Vector3 cornerVector = Vector3.Cross(previousObstructionNormal, obstructionNormal).normalized;
                transientVelocity = Vector3.Project(transientVelocity, cornerVector);
                sweepState = MovementSweepState.FoundBlockingCrease;
            }
            else
            {
                transientVelocity = Vector3.ProjectOnPlane(transientVelocity, obstructionNormal);
            }
        }
        // Blocking corner handling
        else if (sweepState == MovementSweepState.FoundBlockingCrease)
        {
            transientVelocity = Vector3.zero;
            sweepState = MovementSweepState.FoundBlockingCorner;
        }

        previousObstructionNormal = obstructionNormal;

        float newVelocityFactor = transientVelocity.magnitude / initialVelocity.magnitude;
        remainingMovementMagnitude *= newVelocityFactor;
        remainingMovementDirection = transientVelocity.normalized;
    }

    /// <summary>
    /// Sweeps the capsule's volume to detect collision hits
    /// </summary>
    /// <returns> Returns the number of hits </returns>
    public int Sweep(Vector3 position, Quaternion rotation, Vector3 direction, float distance, out RaycastHit closestHit, RaycastHit[] hits)
    {
        int queryLayers = GameManager.Instance.GetLayerMask(gameObject.layer);

        // Capsule cast
        int nbHits = 0;
        hits = (Rigidbody.SweepTestAll(direction, distance + 0.002f, QueryTriggerInteraction.Ignore));
        int nbUnfilteredHits = hits.Length;

        // Hits filter
        closestHit = new RaycastHit();
        float closestDistance = Mathf.Infinity;
        nbHits = nbUnfilteredHits;
        for (int i = nbUnfilteredHits - 1; i >= 0; i--)
        {
            hits[i].distance -= 0.002f;

            RaycastHit hit = hits[i];
            float hitDistance = hit.distance;

            // Filter out the invalid hits
            if (hitDistance <= 0f ||
                collider == hit.collider)
            {
                nbHits--;
                if (i < nbHits)
                {
                    hits[i] = hits[nbHits];
                }
            }
            else
            {
                // Remember closest valid hit
                if (hitDistance < closestDistance)
                {
                    closestHit = hit;
                    closestDistance = hitDistance;
                }
            }
        }

        return nbHits;
    }

    /// <summary>
    /// Moves the character's position by given movement while taking into account all physics simulation, step-handling and 
    /// velocity projection rules that affect the character motor
    /// </summary>
    /// <returns> Returns false if movement could not be solved until the end </returns>
    private bool ContinuousMovement(Vector3 moveBy, ref Vector3 position, ref Quaternion rotation)
    {
        bool wasCompleted = true;
        Vector3 remainingMovementDirection = moveBy.normalized;
        float remainingMovementMagnitude = moveBy.magnitude;
        Vector3 originalVelocityDirection = remainingMovementDirection;
        int sweepsMade = 0;
        bool hitSomethingThisSweepIteration = true;
        Vector3 tmpMovedPosition = position;
        Vector3 previousMovementHitNormal = Vector3.zero;
        MovementSweepState sweepState = MovementSweepState.Initial;
        RaycastHit[] _internalCharacterHits = new RaycastHit[16];

        // Sweep the desired movement to detect collisions
        while (remainingMovementMagnitude > 0f &&
            (sweepsMade <= 8) &&
            hitSomethingThisSweepIteration)
        {
            if (Sweep(tmpMovedPosition, rotation, remainingMovementDirection, remainingMovementMagnitude + 0.001f,
                out RaycastHit closestSweepHit, _internalCharacterHits) > 0)
            {
                // Calculate movement from this iteration
                Vector3 sweepMovement = (remainingMovementDirection * closestSweepHit.distance) + (closestSweepHit.normal * 0.001f);
                tmpMovedPosition += sweepMovement;
                remainingMovementMagnitude -= sweepMovement.magnitude;

                Collider closestSweepCollider = closestSweepHit.collider;
                Vector3 closestSweepPoint = closestSweepHit.point;
                Vector3 closestSweepNormal = closestSweepHit.normal;
                Vector3 obstructionNormal = closestSweepNormal;

                Vector3 vel = moveBy / Time.fixedDeltaTime;

                // Project velocity for next iteration
                InternalHandleVelocityProjection(closestSweepNormal, obstructionNormal, originalVelocityDirection,
                    ref sweepState, ref previousMovementHitNormal, ref vel, ref remainingMovementMagnitude, ref remainingMovementDirection);
            }
            // If we hit nothing...
            else
            {
                hitSomethingThisSweepIteration = false;
            }

            // Safety for exceeding max sweeps allowed
            sweepsMade++;
            if (sweepsMade > 8)
            {
                wasCompleted = false;
            }
        }

        // Move position for the remainder of the movement
        tmpMovedPosition += (remainingMovementDirection * remainingMovementMagnitude);
        position = tmpMovedPosition;

        return wasCompleted;
    }

    public void UpdateMovement(out Vector3 goalPosition, out Quaternion goalRotation, float deltaTime)
    {
        Vector3 moveBy;
        Quaternion rotateBy;
        collidable.UpdateMoveBy(out moveBy, out rotateBy, deltaTime);
        
        if (alwaysCheckCollision && moveBy == Vector3.zero && rotateBy == Quaternion.identity)
        {
            goalPosition = TransientPosition;
            goalRotation = TransientRotation;
            ResolveOverlaps(ref goalPosition, goalRotation, false, moveBy);
        }
        else 
        {
            /*
            if (Rigidbody.collisionDetectionMode == CollisionDetectionMode.Discrete)
            {
            */
                goalPosition = TransientPosition += moveBy;
                goalRotation = rotateBy * TransientRotation;
                ResolveOverlaps(ref goalPosition, goalRotation, true, moveBy);
            /*
            }
            else
            {
                goalPosition = TransientPosition;
                goalRotation = TransientRotation;
                ResolveOverlaps(ref goalPosition, ref goalRotation);

                ContinuousMovement(moveBy, ref goalPosition, ref goalRotation);

                goalPosition += moveBy;
                goalRotation = rotateBy * goalRotation;
            }
            */
        }
    }

    public override void MoveDone(float deltaTime)
    {
        collidable.MoveDone(deltaTime);
    }
}