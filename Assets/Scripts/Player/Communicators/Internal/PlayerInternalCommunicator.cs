using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#region Communication Interfaces

/// <summary>
/// Base interface for all internal Communication interfaces
/// Derived interfaces must have actions for things the component wishes to tell, and methods for things it wishes to be informed of
/// </summary>
public interface IPlayerCommunication
{
    /// <summary>
    /// Initializes the communication interface
    /// Must call communicator.SetCommunication(this);
    /// </summary>
    /// <param name="communicator">The passed in communicator to use to set up communication</param>
    void SetCommunicationInterface(PlayerInternalCommunicator communicator);
}

/// <summary>
/// Communication interface for PlayerMovement
/// </summary>
public interface IPlayerMovementCommunication : IPlayerCommunication
{
    /// <summary>
    /// Event triggered when the player changes plane
    /// </summary>
    event Action<PlaneChangeArgs> planeChanged;
    
    /// <summary>
    /// Event triggered when the kinematic motor state changes
    /// TODO Really shouldn't be an event since it's gonna be triggered every physics tick
    /// TODO find a way to wrap it as a a read only reference that updates every physics tick
    /// </summary>
    event Action<KinematicCharacterController.KinematicCharacterMotorState> stateUpdated;
}

/// <summary>
/// Communication interface for PlayerMovementAction
/// </summary>
public interface IPlayerMovementActionCommunication : IPlayerCommunication
{
    /// <summary>
    /// Event triggered when the player changes facing direction
    /// </summary>
    event Action facingDirectionChanged;
}

/// <summary>
/// Communication interface for player animation
/// </summary>
public interface IPlayerAnimationCommunication : IPlayerCommunication
{
    /// <summary>
    /// Called when player changes facing direction
    /// </summary>
    void ChangeFacingDirection();
}

/// <summary>
/// Communication for the external communicator
/// </summary>
public interface IPlayerExternalCommunicatorCommunication : IPlayerCommunication
{

    /// <summary>
    /// Called when the player changes planes
    /// </summary>
    /// <param name="planeChangeInfo">Info about the plane change</param>
    void HandlePlayerPlaneChanged(PlaneChangeArgs planeChangeInfo);
    /// <summary>
    /// Called when the player's kinematic motor state changes
    /// </summary>
    /// <param name="state">The state of the player</param>
    void HandleMovementStateUpdated(KinematicCharacterController.KinematicCharacterMotorState state);
    /// <summary>
    /// Called when the players gravity direction changes
    /// </summary>
    /// <param name="gravityDirection">New direction of gravity</param>
    void HandlePlayerGravityDirectionChanged(Vector3 gravityDirection);
}
#endregion`````

#region Action Event Handler Argument Structs
/// <summary>
/// Hold info on a recent player plane change
/// </summary>
public struct PlaneChangeArgs
{
    /// <summary>
    /// The new plane normal
    /// </summary>
    public Vector3 planeNormal;
    /// <summary>
    /// Was the plane changed via a plane breaker?
    /// </summary>
    public bool planeBreaker;

    /// <summary>
    /// Basic constructor
    /// </summary>
    /// <param name="normal">The normal of the new plane</param>
    /// <param name="breaker">Was the plane changed via a plane breaker</param>
    public PlaneChangeArgs(Vector3 normal, bool breaker)
    {
        planeNormal = normal;
        planeBreaker = breaker;
    }
}
#endregion

/// <summary>
/// Class that handles communication between internal player components
/// Every component that requires comunication with another has a communication interface 
/// Upon that component's initialization in PlayerCharacter, PlayerCharacter calls the interface's SetCommunicationInterface() function, which calls the appropriately overloaded SetCommunication() function to initialize communication
/// An overloaded SetCommunication() function is required in this class for each communication interface
/// This function takes in the appropriate communication interface and sets the reference in this class to it, and subscribes the appropriate event handlers here for the actions triggered in the communication
/// </summary>
public abstract class PlayerInternalCommunicator
{
    
    #region Communications
    /// <summary>
    /// Communication with PlayerMovement
    /// </summary>
    IPlayerMovementCommunication movement;
    /// <summary>
    /// Communication with PlayerMovementAction
    /// </summary>
    IPlayerMovementActionCommunication movementAction;
    /// <summary>
    /// Communication with player animation
    /// </summary>
    IPlayerAnimationCommunication animation;
    /// <summary>
    /// Communication with the external communicator
    /// </summary>
    IPlayerExternalCommunicatorCommunication externalCommunicator;
    #endregion

    #region SetCommunication() Methods
    /// <summary>
    /// Sets communication with PlayerMovement
    /// </summary>
    /// <param name="communication"> PlayerMovement communication interface</param>
    public void SetCommunication(IPlayerMovementCommunication communication)
    {
        movement = communication;
        movement.planeChanged += PlaneChangedHandler;
        movement.stateUpdated += HandleMovementStateUpdate;
    }

    /// <summary>
    /// Sets communication with PlayerMovementAction
    /// </summary>
    /// <param name="communication"> PlayerMovementAction communication interface</param>
    public void SetCommunication(IPlayerMovementActionCommunication communication)
    {
        movementAction = communication;
        movementAction.facingDirectionChanged += FacingDirectionChangedHandler;
    }

    /// <summary>
    /// Sets communication with PlayerAnimation
    /// </summary>
    /// <param name="communication"> PlayerAnimation communication interface</param>
    public void SetCommunication(IPlayerAnimationCommunication communication)
    {
        animation = communication;
    }

    /// <summary>
    /// Sets communication with ExternalCommunicator
    /// </summary>
    /// <param name="communication">ExternalCommunicator communication interface</param>
    public void SetCommunication(IPlayerExternalCommunicatorCommunication communication)
    {
        externalCommunicator = communication;
    }
    #endregion
    
    #region Animation Notifiers
    /// <summary>
    /// Event Handler for when the player changes facing direction
    /// </summary>
    private void FacingDirectionChangedHandler()
    {
        animation.ChangeFacingDirection();
    }
    #endregion

    #region External Communicator Notifiers
    /// <summary>
    /// Event Handler for when the kinematic motor state changes
    /// </summary>
    /// <param name="state">The KinematicMotor state</param>
    private void HandleMovementStateUpdate(KinematicCharacterController.KinematicCharacterMotorState state)
    {
        externalCommunicator.HandleMovementStateUpdated(state);
    }

    /// <summary>
    /// Event Handler for when the player's plane changes
    /// </summary>
    /// <param name="planeChangeInfo">Info about the plane change </param>
    private void PlaneChangedHandler(PlaneChangeArgs planeChangeInfo)
    {
        externalCommunicator.HandlePlayerPlaneChanged(planeChangeInfo);
    }

    /// <summary>
    /// Event Handler for when the player's gravity direction changes
    /// </summary>
    /// <param name="gravityDirection">The new direction of gravity</param>
    private void GravityDirectionChangedHandler(Vector3 gravityDirection)
    {
        externalCommunicator.HandlePlayerGravityDirectionChanged(gravityDirection);
    }
    #endregion
}
