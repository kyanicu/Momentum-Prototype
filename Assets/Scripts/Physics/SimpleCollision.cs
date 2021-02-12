using System.Collections;
using System.Collections.Generic;
using KinematicCharacterController;
using UnityEngine;

public interface ISimpleCollidable : IMoverController
{

}

[RequireComponent(typeof(Rigidbody), typeof(ISimpleCollidable))]
public class SimpleCollision : PhysicsMover
{

    [SerializeField]
    private bool continousDetection = false;

    [SerializeField]
    private bool checkCollisionAlways = false;

    private ISimpleCollidable collidable;

    new private Collider collider;

    private LayerMask mask;


//#region CollisionHandling Values
    // Warning: Don't touch these constants unless you know exactly what you're doing!
//    public const int MaxHitsBudget = 16;
//    public const int MaxCollisionBudget = 16;
//    public const int MaxGroundingSweepIterations = 2;
//    public const int MaxMovementSweepIterations = 8;
//    public const int MaxSteppingSweepIterations = 3;
//    public const int MaxRigidbodyOverlapsCount = 16;
//    public const int MaxDiscreteCollisionIterations = 3;
//    public const float CollisionOffset = 0.001f;
//    public const float GroundProbeReboundDistance = 0.02f;
//    public const float MinimumGroundProbingDistance = 0.005f;
//    public const float GroundProbingBackstepDistance = 0.1f;
//    public const float SweepProbingBackstepDistance = 0.002f;
//    public const float SecondaryProbesVertical = 0.02f;
//    public const float SecondaryProbesHorizontal = 0.001f;
//    public const float MinVelocityMagnitude = 0.01f;
//    public const float SteppingForwardDistance = 0.03f;
//    public const float MinDistanceForLedge = 0.05f;
//    public const float CorrelationForVerticalObstruction = 0.01f;
//    public int OverlapsCount { get { return _overlapsCount; } }
//    private int _overlapsCount;
//    /// <summary>
//    /// The overlaps detected so far during character update
//    /// </summary>
//    public OverlapResult[] Overlaps { get { return _overlaps; } }
//    private OverlapResult[] _overlaps = new OverlapResult[MaxRigidbodyOverlapsCount];
//#endregion

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
        //MoverController = this;
        collider = GetComponent<Collider>();
    }

    void Start()
    {
        mask = GameManager.Instance.GetLayerMask(gameObject.layer);
    }

    public override void PreVelocityUpdate(float deltaTime)
    {

    }

    public override void PreSetPositionAndRotation() 
    {

    }

    public override void PostSetPositionAndRotation() 
    {
        if (!continousDetection)
            ResolveOverlaps();
    }

    private void ResolveOverlaps()
    {
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
                        if (Physics.ComputePenetration(
                                collider,
                                TransientPosition,
                                TransientRotation,
                                overlaps[i],
                                overlappedTransform.position,
                                overlappedTransform.rotation,
                                out resolutionDirection,
                                out resolutionDistance))
                        {
                            // Resolve along obstruction direction
                            Vector3 originalResolutionDirection = resolutionDirection;

                            // Solve overlap
                            Vector3 resolutionMovement = resolutionDirection * (resolutionDistance + 0.001f);
                            TransientPosition += resolutionMovement;

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
        if(iterationsMade > 0)
        {
            Transform.SetPositionAndRotation(TransientPosition, TransientRotation);
            Rigidbody.position = TransientPosition;
            Rigidbody.rotation = TransientRotation;
        }
    }
}