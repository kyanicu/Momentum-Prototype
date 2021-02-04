using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Base interface for all external Communication interfaces
/// Derived interfaces must have actions for things the class wishes to tell, and methods for things it wishes to be informed of
/// </summary>
public interface IPlayerExternalCommunication
{
    /// <summary>
    /// Initializes the communication interface
    /// Must call communicator.SetCommunication(this);
    /// </summary>
    /// <param name="communicator">The passed in communicator to use to set up communication</param>
    void SetPlayerExternalCommunication(PlayerExternalCommunicator communicator);
}

/// <summary>
/// Communication interface for allowing communication with the player camera
/// </summary>
public interface IPlayerCameraCommunication : IPlayerExternalCommunication
{
    /// <summary>
    /// Handles ReadOnly reference setup
    /// </summary>
    /// <param name="_playerTransform">The player's readonly transform reference</param>
    /// <param name="_playerKinematicMotor">The player's readonly Motor reference</param>
    void SetReadOnlyReferences(ReadOnlyTransform _playerTransform, ReadOnlyKinematicMotor _playerKinematicMotor);

    /// <summary>
    /// Allows the camera to gather player input
    /// TODO Should have it's own controller/action mapping instead of needing the PlayerCharacter's
    /// </summary>
    /// <param name="controllerActions">The controller's action mapping</param>
    void HandleInput(PlayerController.PlayerActions controllerActions);
    /// <summary>
    /// Used to inform the camera of when the player changes planes 
    /// </summary>
    /// <param name="planeChangeInfo">Info about the plane change</param>
    void HandlePlayerPlaneChanged(PlaneChangeArgs planeChangeInfo);
    /// <summary>
    /// Informs the camera of when the player's gravity direction is changed
    /// </summary>
    /// <param name="gravityDirection">The new direction of gravity</param>
    void HandlePlayerGravityDirectionChanged(Vector3 gravityDirection);
}

/// <summary>
/// Handles communication between the PlayerCharacter and other GameObjects
/// Works in the same way as InternalCommunicator, but applied to classes outside of PlayerCharacter 
/// </summary>
public class PlayerExternalCommunicator : IPlayerExternalCommunicatorCommunication
{

    #region Communications
    /// <summary>
    /// Communication with the PlayerCamera
    /// </summary>
    private IPlayerCameraCommunication camera;
    #endregion
    
    #region InternalCommunicationSetup
    /// <summary>
    /// Initializes the communication interface
    /// </summary>
    /// <param name="communicator">The passed in communicator to use to set up communication</param>
    public void SetCommunicationInterface(PlayerInternalCommunicator communicator)
    {
        communicator.SetCommunication(this);
    }
    #endregion

    #region SetPlayerExternalCommunication() Methods
    /// <summary>
    /// Sets communication with PlayerCamera
    /// </summary>
    /// <param name="communication"> PlayerCamera communication interface</param>
    public void SetPlayerExternalCommunication(IPlayerCameraCommunication communication)
    {
        camera = communication;
    }
    
    #endregion

    #region PlayerCamera Notifiers
    
    /// <summary>
    /// Informs the PlayerCamera of the current frame's input
    /// TODO Should have it's own controller/action mapping instead of needing the PlayerCharacter's
    /// </summary>
    /// <param name="controllerActions">The controller's action mapping</param>
    public void HandleInput(PlayerController.PlayerActions controllerActions)
    {
        camera.HandleInput(controllerActions);
    }

    /// <summary>
    /// Informs the PlayerCamera when the Player's plane changes
    /// </summary>
    /// <param name="planeChangeInfo">Info about the plane change</param>
    public void HandlePlayerPlaneChanged(PlaneChangeArgs planeChangeInfo)
    {
        camera.HandlePlayerPlaneChanged(planeChangeInfo);
    }

    /// <summary>
    /// Informs the PlayerCamera of when the player's gravity direction changes
    /// </summary>
    /// <param name="gravityDirection">THe new direction of gravity</param>
    public void HandlePlayerGravityDirectionChanged(Vector3 gravityDirection)
    {
        camera.HandlePlayerGravityDirectionChanged(gravityDirection);
    }
    #endregion

}