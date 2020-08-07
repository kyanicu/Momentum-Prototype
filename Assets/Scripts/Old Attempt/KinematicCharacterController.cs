using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
public class KinematicCharacterController : KinematicObjectMover
{

    private CapsuleCollider capCol;
    private Rigidbody rb;

    [SerializeField]
    private float floatattachThreshold;
    [SerializeField]
    private float maxSlope;
    [SerializeField]
    private float stepHeightThreshold;
    [SerializeField]
    private float pushOffGroundThreshold;
    [SerializeField]
    private float maxSpeed;

    [SerializeField]
    private bool rotateAlongGround;

    [SerializeField]
    private bool attachToGround;

    public bool isGrounded { get; private set; }
    public Vector3 currentSlopeNormal { get; private set; }
    private bool isAttachedToGround;

    private Vector3 down {
        get { return (isAttachedToGround) ? -transform.up : Physics.gravity.normalized; }
        set { transform.up = -value; }
    }

    public override MoveInfo MoveUntilCollision(Vector3 moveBy, bool informIfNoStop=true, bool returnColliders=false, bool returnHits=false)
    {
        
    }
    
    public int MoveFull(Vector3 moveBy, out MoveInfo[] moveInfo, bool informIfNoStop=true, bool returnColliders=false, bool returnHits=false)
    {
        
    }

    public int MoveAlongGround(Vector3 moveBy, out MoveInfo[] moveInfo, bool informIfNoStop=true, bool returnColliders=false, bool returnHits=false)
    {

        if (!isAttachedToGround)
            moveBy = Vector3.Project(moveBy, Vector2.right); 

        Vector3 newMoveBy = moveBy;
        MoveInfo moveInfo;
        int loops = 0;
        while (!moveCompleted(moveInfo = moveUntilCollision(newMoveBy)) && velocity != Vector2.zero)
        {

            if(!isGrounded)
            {
                moveMaxPossibleXYDistance(moveInfo.moveLeft);
                break;
            }

            newMoveBy = Mathf.Sign(Vector2.Dot(moveInfo.moveLeft, currentSlope))
                * moveInfo.moveLeft.magnitude * currentSlope;

            loops++;
            if (loops > 10)
            {
                //Debug.Break();
                Debug.LogError("moveAlongGround(): Possible Infinite Loop. Exiting");

                break;
            }
        }


    }

    public override void MoveSimple(Vector3 moveBy, bool forceUnground=false) 
    {

        MoveInfo[] moveInfo;

        if (isGrounded)
            MoveAlongGround(moveBy, out moveInfo);
        else
            MoveFull(moveBy, out moveInfo,);

    } 

}
*/