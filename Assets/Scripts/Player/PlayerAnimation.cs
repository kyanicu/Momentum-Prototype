using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerAnimation : IPlayerAnimationCommunication
{

    private GameObject modelRoot;

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

}
