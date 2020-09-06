using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerAnimation : IPlayerAnimationCommunication
{

    [SerializeField, HideInInspector]
    private GameObject modelRoot;

    private Vector3 positionInterpDampVel = Vector3.zero;
    [SerializeField]
    private float positionInterpDampTime;
    [SerializeField]
    private float positionInterpDampMaxSpeed;

    private Vector3 rotationInterpDampVel = Vector3.zero;
    [SerializeField]
    private float rotationInterpDampTime;
    [SerializeField]
    private float rotationInterpDampMaxSpeed;

    Vector3 prevParentPosition = Vector3.zero;
    Quaternion prevParentRotation = Quaternion.identity;

    static private readonly Quaternion flipUp = Quaternion.Euler(0,180,0);

    public PlayerAnimation(GameObject _modelRoot)
    {
        modelRoot = _modelRoot;
    }

    public void SetCommunication(PlayerInternalCommunicator communicator)
    {
        communicator.SetCommunication(this);
    }

    public void ChangeFacingDirection()
    {
        modelRoot.transform.localRotation *= flipUp;
    }

    public void FrameUpdate()
    {
        
    }

}
