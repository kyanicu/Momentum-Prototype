using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPlayerExternalCommunication
{
    void SetPlayerExternalCommunication(PlayerExternalCommunicator communicator);
}

public interface IPlayerCameraCommunication : IPlayerExternalCommunication
{
    void Foo();
}

public abstract class PlayerExternalCommunicator : IPlayerExternalCommunicatorCommunication
{

    private IPlayerCameraCommunication camera;

    public void SetCommunication(PlayerInternalCommunicator communicator)
    {
        communicator.SetCommunication(this);
    }

    public void SetPlayerExternalCommunication(IPlayerCameraCommunication communication)
    {
        camera = communication;
    }

    public void HandlePlayerUngrounded()
    {
        camera.Foo();
    }

}