using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KinematicObjectMover : MonoBehaviour
{

    public struct MoveInfo 
    {

        private static float moveCompletedOffset = totalCollisionOffset;

        public bool moveIsValid;
        public Vector3 moveInitial;
        public Vector3 moveDone;
        public Vector3 moveLeft;
        public bool moveCompleted;
        public Vector3 hitNormal;
        public RaycastHit[] hits;
        public bool[] wasDirectHit;
        public int hitCount;
        public Collider[] colliders;
        public int colliderCount;
        public bool stoppedAtIndirectHit;
        public bool stoppedAtTrigger;

        public MoveInfo(Vector3 init, Vector3 done,Vector3 _hitNormal = default(Vector3), RaycastHit[] _hits=null, bool[] _wasDirectHit=null, int _hitCount=0, Collider[] _colliders=null, int _colliderCount=0, bool _stoppedAtIndirectHit=false, bool _stoppedAtTrigger = false)
        {
            if (init == new Vector3() || init == Vector3.zero)
                moveIsValid = false;
            else
                moveIsValid = true;
            
            moveInitial = init;
            moveDone = done;
            hitNormal = _hitNormal;
            moveLeft = moveInitial - moveDone;
            moveCompleted = moveLeft.magnitude <= moveCompletedOffset;
            hits = _hits;
            wasDirectHit = _wasDirectHit;
            hitCount = _hitCount;
            colliders = _colliders;
            colliderCount = _colliderCount;
            stoppedAtIndirectHit = _stoppedAtIndirectHit;
            stoppedAtTrigger = _stoppedAtTrigger;

            if (hitNormal == default(Vector3))
                hitNormal = Vector3.zero;
        }

        public static MoveInfo invalidMove = new MoveInfo(Vector3.zero, Vector3.zero);

    }

#region StaticProperties

    bool staticInit = false;
    static int tempQueryLayer;
    static int tempQueryLayerMask;

    static float dotProductDelta = 0.022f;

    static float extraCollisionOffset = 0.0001f;
    
    static float totalCollisionOffset { get { return Physics.defaultContactOffset + extraCollisionOffset; } }

#endregion

#region CollisionInfo

    private enum CollisionType { NONE, DISCRETE, CONTINUOUS }

    [SerializeField]
    private CollisionType collisionType;

    
    [SerializeField]
    private bool discreteLimitFixDirection;
    [SerializeField]
    private bool continuousCheckIndirectHits;
    [SerializeField]
    private bool continuousCheckTriggers;
    //[SerializeField]
    //private bool supportProperConcaveMeshCollision;

    [SerializeField]
    private bool _stopOnCollision;
    public bool stopOnCollision {get {return _stopOnCollision; } private set { _stopOnCollision = value; } }

#endregion

#region InternalReferences

    Collider col;
    Rigidbody rb;

    int layerMask;

#endregion

#region Initialization

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        SetLayerMask();

        if (rb == null || col == null)
            collisionType = CollisionType.NONE;

        if (!staticInit)
        {
            tempQueryLayer = LayerMask.NameToLayer("Temp Query");
            tempQueryLayerMask = LayerMask.GetMask("Temp Query");
            staticInit = true;
        }
    }

    public void SetLayerMask()
    {
        int layer = gameObject.layer;

        for (int i = 0; i < 32; i++)
        {
            if (!Physics.GetIgnoreLayerCollision(layer, i))
            {
                layerMask = layerMask | 1 << i;
            }

        }
    }

#endregion

#region PenetrationAndSweeping

    private bool CheckContact(Collider collider, bool handleOverlap, bool limitedDirection, Vector3 movedBy=new Vector3())
    {
        Vector3 otherPosition;
        Quaternion otherRotation;

        if (collider.attachedRigidbody != null)
        {
            otherPosition = collider.attachedRigidbody.transform.position;
            otherRotation = collider.attachedRigidbody.transform.rotation;
        }
        else
        {
            otherPosition = collider.gameObject.transform.position;
            otherRotation = collider.gameObject.transform.rotation;
        }

        Vector3 direction;
        float distance;

        bool overlapped = Physics.ComputePenetration(col, rb.position, rb.rotation, collider, otherPosition, otherRotation, out direction, out distance);
        if (handleOverlap && overlapped && distance != 0 && (!limitedDirection || Vector3.Dot(direction, movedBy) < -dotProductDelta))
        {
            if (limitedDirection)
            {
                int prevThisLayer = gameObject.layer;
                int prevOtherLayer = collider.gameObject.layer;
                collider.gameObject.layer = tempQueryLayer;
                gameObject.layer = tempQueryLayer;

                Vector3 movedByOffset = movedBy + movedBy.normalized * Physics.defaultContactOffset;

                Translate(-movedByOffset);
                Translate(Sweep(movedByOffset, QueryTriggerInteraction.Ignore, false, true).moveDone);

                collider.gameObject.layer = prevOtherLayer;
                gameObject.layer = prevThisLayer;
            }
            else
                Translate(direction * (distance));
        }

        return overlapped;
    }

    private void HandlePenetration(Vector3 movedBy=new Vector3())
    {
        bool limitedDirection = movedBy != new Vector3();

        Collider[] collidersInAABB = Physics.OverlapBox(col.bounds.center, col.bounds.extents + Vector3.one * totalCollisionOffset, Quaternion.identity, layerMask, QueryTriggerInteraction.Ignore);

        foreach (Collider collider in collidersInAABB)
        {
            if (collider == col)
                continue;

            CheckContact(collider, true, limitedDirection, movedBy);
        }
    }

    private int CheckPenetration(out Collider[] collidersHit, bool handle, Vector3 movedBy = new Vector3())
    {
        bool limitedDirection = !(movedBy == new Vector3());

        Collider[] collidersInAABB = Physics.OverlapBox(col.bounds.center, col.bounds.extents + Vector3.one * totalCollisionOffset, Quaternion.identity, layerMask, QueryTriggerInteraction.Ignore);

        collidersHit = new Collider[collidersInAABB.Length];
        int hitCount = 0;

        foreach (Collider collider in collidersInAABB)
        {
            if (CheckContact(collider, handle, limitedDirection, movedBy))
            {
                collidersHit[hitCount] = collider;
                hitCount++;
            }
        }

        System.Array.Resize(ref collidersHit, hitCount);
        return hitCount;
    }

    public int CheckPenetration(out Collider[] collidersHit)
    {
        return CheckPenetration(out collidersHit, false);
    }

    private void InformSurroundingColliders(Collider[] inform = null)
    {
        if (inform == null)
            inform = Physics.OverlapBox(col.bounds.center, col.bounds.extents + Vector3.one * totalCollisionOffset, Quaternion.identity, layerMask, QueryTriggerInteraction.Collide);
        
        int count = inform.Length;

        if(count > 0)
        {
            
            foreach(Collider collider in inform)
            {
                if (collider == col)
                    continue;

                KinematicObjectMover mover;
                if(collider.attachedRigidbody != null && collider.attachedRigidbody.isKinematic && (mover = collider.gameObject.GetComponent<KinematicObjectMover>()) != null)
                    if (mover.stopOnCollision)
                        mover.Inform();
            }
        }
    }

    public virtual void Inform()
    {
        HandlePenetration();
    }

    public MoveInfo Sweep(Vector3 sweepBy, QueryTriggerInteraction queryTriggerInteraction, bool checkIndirectHits, bool safeSweep, bool distanceZero = false)
    {
        float distance = sweepBy.magnitude;
        Vector3 direction = sweepBy.normalized;

        if (sweepBy == Vector3.zero)
            return MoveInfo.invalidMove;
        else if (distanceZero)
        {
            if (!safeSweep)
            {
                return MoveInfo.invalidMove;
            }
            else
                distance = 0;
        }

        if (safeSweep)
        {
            distance += Physics.defaultContactOffset;
            Translate(-direction * totalCollisionOffset);
        }

        RaycastHit[] sweepHits = rb.SweepTestAll(direction, distance, queryTriggerInteraction);
        int sweepHitLength = sweepHits.Length;
        RaycastHit[] hits = new RaycastHit[sweepHitLength];
        bool[] wasDirectHit = new bool[sweepHitLength];
        int hitCount = 0;
        Vector3 hitNormal = Vector3.zero;
        bool stoppedAtTrigger = false;
        bool stoppedAtIndirectHit = false;

        if (sweepHitLength > 0)
        {
            foreach (RaycastHit hit in sweepHits)
            {
                
                bool indirectHit = Vector3.Dot(direction, hit.normal) >= -dotProductDelta;

                if (hit.distance > distance || hit.distance <= extraCollisionOffset || (safeSweep && hit.distance <= totalCollisionOffset && (hit.collider.isTrigger || indirectHit)))
                    continue;
                else if (hit.distance < distance)
                {
                    distance = hit.distance;
                    hitCount = 0;
                    stoppedAtTrigger = false;
                    stoppedAtIndirectHit = false;
                }
                if (!(stoppedAtTrigger = hit.collider.isTrigger))
                {
                    hits[hitCount] = hit;
                    hitNormal += hit.normal;
                    if (checkIndirectHits && indirectHit) 
                    {
                        wasDirectHit[hitCount] = false;
                        stoppedAtIndirectHit = true;
                    }
                    else
                        wasDirectHit[hitCount] = true;

                    
                    hitCount++;
                }
            }
            hitNormal.Normalize();
        }

        System.Array.Resize(ref hits, hitCount);

        if (safeSweep)
        {
            distance -= Physics.defaultContactOffset;
            if (distance < 0)
                distance = 0;
            Translate(direction * totalCollisionOffset);
        }

        return new MoveInfo(sweepBy, sweepBy.normalized * distance, hitNormal, hits, wasDirectHit, hitCount, null, 0, stoppedAtIndirectHit ,stoppedAtTrigger);
    }

#endregion

#region MoveViaCollisionType

    private void RotateDiscrete(Quaternion rotateBy, bool informIfNoStop)
{
    Rotate(rotateBy);

    if (stopOnCollision)
        HandlePenetration();
    else if(informIfNoStop)
        InformSurroundingColliders();
}

    private MoveInfo MoveDiscrete(Vector3 moveBy, bool informIfNoStop, bool limitedDirection,  bool returnColliders=false, bool returnHits=false) 
    {

        Vector3 prevPos = rb.position;

        Translate(moveBy);

        int colliderCount = 0;
        Collider[] colliders = null;

        if (!returnColliders){
            if (stopOnCollision)
            {
                if (!limitedDirection)
                    HandlePenetration();
                else
                    HandlePenetration(moveBy);
            
            }
            else if (informIfNoStop)
            {
                InformSurroundingColliders();
            }
        }
        else 
        {
            if (stopOnCollision)
            {
                colliderCount = CheckPenetration(out colliders, true, moveBy);
            }
            else
            {
                colliderCount = CheckPenetration(out colliders, false);

                if(informIfNoStop)
                    InformSurroundingColliders(colliders);
            }
        }
        
        MoveInfo moveInfo = new MoveInfo(moveBy, Vector3.Project(rb.position-prevPos, moveBy));

        if (!limitedDirection)
            moveInfo.moveCompleted = true;

        if (returnHits)
        {
            MoveInfo sweepInfo = Sweep(moveBy.normalized, QueryTriggerInteraction.Ignore, continuousCheckIndirectHits, true, true);

            moveInfo.hits = sweepInfo.hits;
            moveInfo.hitCount = sweepInfo.hitCount;
            moveInfo.hitNormal = sweepInfo.hitNormal;
        }
        return moveInfo;
    }

    private MoveInfo MoveContinuous(Vector3 moveBy,  bool informIfNoStop,  bool returnColliders=false)
    {

        QueryTriggerInteraction queryTriggerInteraction = (continuousCheckTriggers) ? QueryTriggerInteraction.Collide : QueryTriggerInteraction.Ignore;

        MoveInfo moveInfo = Sweep(moveBy, queryTriggerInteraction, continuousCheckIndirectHits, stopOnCollision);

        Translate(moveInfo.moveDone);

        if (returnColliders)
        {
            moveInfo.colliderCount = CheckPenetration(out moveInfo.colliders, false);

            if (!stopOnCollision && informIfNoStop)
                InformSurroundingColliders(moveInfo.colliders);
        }
        else if (!stopOnCollision && informIfNoStop)
        {
            InformSurroundingColliders();
        }

        return moveInfo;

    }

#endregion

#region Move

    private void Translate(Vector3 moveBy)
    {
        rb.position += moveBy;
        //transform.position = rb.position;
    }

    private void SetPosition(Vector3 position)
    {
        rb.position = position;
    }

    private void Rotate(Quaternion rotateBy)
    {
        rb.rotation *= rotateBy;
    }

    private void SetRotation(Quaternion rotation)
    {
        rb.rotation = rotation;
    }

    public Vector3 GetSlideMoveBy(MoveInfo previousMoveInfo, Vector3 initialMoveBy)
    {
        if (previousMoveInfo.moveCompleted || previousMoveInfo.hitNormal == Vector3.zero || !previousMoveInfo.moveIsValid || previousMoveInfo.stoppedAtIndirectHit || previousMoveInfo.stoppedAtTrigger)
            return Vector3.zero;

        Vector3 slideMoveBy = Vector3.ProjectOnPlane(Vector3.Project(previousMoveInfo.moveLeft, initialMoveBy), previousMoveInfo.hitNormal);

        if (Vector3.Dot(slideMoveBy, initialMoveBy) <= 0) 
            return Vector3.zero;

        return slideMoveBy;

    }

    public virtual MoveInfo MoveUntilCollision(Vector3 moveBy, bool informIfNoStop=true, bool returnColliders=false, bool returnHits=false)
    {   
        if (moveBy == Vector3.zero)
            return MoveInfo.invalidMove;

        switch (collisionType)
        {
            case CollisionType.DISCRETE :
                return MoveDiscrete(moveBy, informIfNoStop, discreteLimitFixDirection, returnColliders, returnHits);
            case CollisionType.CONTINUOUS :
                return MoveContinuous(moveBy, informIfNoStop, returnColliders);
            default:
                Translate(moveBy);
                return(new MoveInfo(moveBy, moveBy));
        }
    }

    public virtual int MoveFull(Vector3 moveBy, out MoveInfo[] moveInfo, bool informIfNoStop=true,  bool returnColliders=false, bool returnHits=false)
    { 
        if (moveBy == Vector3.zero)
        {
            moveInfo = new MoveInfo[] { MoveInfo.invalidMove };
            return 1;
        }

        if (!stopOnCollision)
        {
            moveInfo = new MoveInfo[] { MoveUntilCollision(moveBy, informIfNoStop, returnColliders, returnHits) };
            return 1;
        }
        else if (!discreteLimitFixDirection && collisionType == CollisionType.DISCRETE)
        {
            moveInfo = new MoveInfo[] { MoveDiscrete(moveBy, informIfNoStop, false, returnColliders, returnHits) };
            return 1;
        }
        else 
        {
            int maxLoops = 3;
            Vector3 newMoveBy = moveBy;
            moveInfo = new MoveInfo[maxLoops];

            int loops = 0;

            moveInfo[0] = MoveUntilCollision(newMoveBy, false, returnColliders, true);
            
            while ((newMoveBy = GetSlideMoveBy(moveInfo[loops], moveBy)) != Vector3.zero && loops+1 < maxLoops)
            {
                loops++;
                moveInfo[loops] = MoveUntilCollision(newMoveBy);
            }
            
            if ((!stopOnCollision && informIfNoStop))
            {
                if (returnColliders)
                    InformSurroundingColliders(moveInfo[loops].colliders);
                else
                    InformSurroundingColliders();
            }
                

            System.Array.Resize(ref moveInfo, loops+1);
            return loops+1;
        }
    }

    public virtual void MoveSimple(Vector3 moveBy)
    {
        MoveInfo[] moveInfo;
        MoveFull(moveBy, out moveInfo);
    }

    public virtual void RotateFull(Quaternion rotateBy, bool informIfNoStop)
    {
        RotateDiscrete(rotateBy, informIfNoStop);
    }

#endregion

#region DebugAndTesting

    [SerializeField]
    float moveSpeed = 0;
    [SerializeField]
    float rotateSpeed = 0;

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector3 moveDirection = Vector3.zero;

        if (Input.GetKey(KeyCode.W))
            moveDirection += Vector3.up;
        if (Input.GetKey(KeyCode.S))
            moveDirection += Vector3.down;
        if (Input.GetKey(KeyCode.A)) 
            moveDirection += Vector3.left;
        if (Input.GetKey(KeyCode.D))
            moveDirection += Vector3.right;
        if (Input.GetKey(KeyCode.E))
            moveDirection += Vector3.forward;
        if (Input.GetKey(KeyCode.Q))
            moveDirection += Vector3.back;

        moveDirection = Quaternion.Euler(-30,0,0) * Quaternion.FromToRotation(Vector3.forward, Camera.main.transform.forward) * moveDirection;

        if (moveDirection != Vector3.zero)
            MoveSimple(moveDirection.normalized * moveSpeed * Time.fixedDeltaTime);

        Vector3 rotateDirection = Vector3.zero;

        if (Input.GetKey(KeyCode.I))
            rotateDirection += Vector3.right;
        if (Input.GetKey(KeyCode.K))
            rotateDirection += Vector3.left;
        if (Input.GetKey(KeyCode.J)) 
            rotateDirection += Vector3.forward;
        if (Input.GetKey(KeyCode.L))
            rotateDirection += Vector3.back;
        if (Input.GetKey(KeyCode.U))
            rotateDirection += Vector3.up;
        if (Input.GetKey(KeyCode.O))
            rotateDirection += Vector3.down;

        if (rotateDirection != Vector3.zero)
            RotateFull(Quaternion.Euler(rotateDirection.normalized * rotateSpeed * Time.fixedDeltaTime), true);

    }

#endregion

} 