using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPlayerCommunication
{
    void SetCommunication(PlayerInternalCommunicator communicator);
}

public interface IPlayerMovementCommunication : IPlayerCommunication
{
    event EventHandler ungrounded;
    event EventHandler grounded;
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
    void HandlePlayerUngrounded();
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
        movement.ungrounded += ungroundedHandler;
        movement.grounded += groundedHandler;
    }

    public void SetCommunication(IPlayerAnimationCommunication communication)
    {
        animation = communication;
    }

    public void SetCommunication(IPlayerMovementActionCommunication communication)
    {
        action = communication;
        action.facingDirectionChanged += facingDirectionChangedHandler;
    }

    public void SetCommunication(IPlayerExternalCommunicatorCommunication communication)
    {
        externalCommunicator = communication;
    }

    private void ungroundedHandler(object sender, EventArgs args)
    {
        externalCommunicator.HandlePlayerUngrounded();
    }

    private void groundedHandler(object sender, EventArgs arg)
    {
        
    }

    private void facingDirectionChangedHandler(object sender, EventArgs args)
    {
        animation.ChangeFacingDirection();
    }

    public PlayerInternalCommunicator()
    {
        
    }
    
}
