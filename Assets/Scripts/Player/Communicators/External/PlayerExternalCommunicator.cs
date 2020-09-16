using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPlayerExternalCommunication
{
    void SetPlayerExternalCommunication(PlayerExternalCommunicator communicator, ReadOnlyTransform playerTransform);
}

public interface IPlayerCameraCommunication : IPlayerExternalCommunication
{
    void HandleInput(PlayerController.PlayerActions controllerActions);
    void HandlePlayerPlaneChanged(PlaneChangeEventArgs planeChangeInfo);
    void HandlePlayerMovementStateUpdated(KinematicCharacterController.KinematicCharacterMotorState state);
    void HandlePlayerGravityDirectionChanged(Vector3 gravityDirection);
}

public abstract class PlayerExternalCommunicator : IPlayerExternalCommunicatorCommunication
{

    private IPlayerCameraCommunication camera;

    public void SetCommunication(PlayerInternalCommunicator communicator)
    {
        communicator.SetCommunication(this);
    }

    public void HandleInput(PlayerController.PlayerActions controllerActions)
    {
        camera.HandleInput(controllerActions);
    }

    public void SetPlayerExternalCommunication(IPlayerCameraCommunication communication)
    {
        camera = communication;
    }

    public void HandlePlayerPlaneChanged(PlaneChangeEventArgs planeChangeInfo)
    {
        camera.HandlePlayerPlaneChanged(planeChangeInfo);
    }

    public void HandleMovementStateUpdated(KinematicCharacterController.KinematicCharacterMotorState state)
    {
        camera.HandlePlayerMovementStateUpdated(state);
    }

    public void HandlePlayerGravityDirectionChanged(Vector3 gravityDirection)
    {
        camera.HandlePlayerGravityDirectionChanged(gravityDirection);
    }

}