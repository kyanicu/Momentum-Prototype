using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPlayerCommunication
{
    void SetCommunication(PlayerInternalCommunicator communicator);
}

public class PlaneChangeEventArgs : EventArgs
{
    public Vector3 planeNormal;
    public bool planeBreaker;

    public PlaneChangeEventArgs(Vector3 normal, bool breaker)
    {
        planeNormal = normal;
        planeBreaker = breaker;
    }
}
public interface IPlayerMovementCommunication : IPlayerCommunication
{
    event EventHandler<PlaneChangeEventArgs> planeChanged;
    
    event EventHandler<KinematicCharacterController.KinematicCharacterMotorState> stateUpdated;
}

public interface IPlayerMovementActionCommunication : IPlayerCommunication
{
    event EventHandler facingDirectionChanged;
}

public interface IPlayerAnimationCommunication : IPlayerCommunication
{
    void ChangeFacingDirection();
}

public interface IPlayerExternalCommunicatorCommunication : IPlayerCommunication
{
    void HandlePlayerPlaneChanged(PlaneChangeEventArgs planeChangeInfo);
    void HandleMovementStateUpdated(KinematicCharacterController.KinematicCharacterMotorState state);
    void HandlePlayerGravityDirectionChanged(Vector3 gravityDirection);
}

public abstract class PlayerInternalCommunicator
{
    IPlayerMovementCommunication movement;
    IPlayerMovementActionCommunication action;
    IPlayerAnimationCommunication animation;
    IPlayerExternalCommunicatorCommunication externalCommunicator;

    public void SetCommunication(IPlayerMovementCommunication communication)
    {
        movement = communication;
        movement.planeChanged += PlaneChangedHandler;
        movement.stateUpdated += HandleMovementStateUpdate;
    }

    private void HandleMovementStateUpdate(object sender, KinematicCharacterController.KinematicCharacterMotorState state)
    {
        externalCommunicator.HandleMovementStateUpdated(state);
    }

    private void PlaneChangedHandler(object sender, PlaneChangeEventArgs planeChangeInfo)
    {
        externalCommunicator.HandlePlayerPlaneChanged(planeChangeInfo);
    }

    private void GravityDirectionChangedHandler(object sender, Vector3 gravityDirection)
    {
        externalCommunicator.HandlePlayerGravityDirectionChanged(gravityDirection);
    }

    public void SetCommunication(IPlayerMovementActionCommunication communication)
    {
        action = communication;
        action.facingDirectionChanged += FacingDirectionChangedHandler;
    }
    
    private void FacingDirectionChangedHandler(object sender, EventArgs args)
    {
        animation.ChangeFacingDirection();
    }

    public void SetCommunication(IPlayerAnimationCommunication communication)
    {
        animation = communication;
    }

    public void SetCommunication(IPlayerExternalCommunicatorCommunication communication)
    {
        externalCommunicator = communication;
    }

}
