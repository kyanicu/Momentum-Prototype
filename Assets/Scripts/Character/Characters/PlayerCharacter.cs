using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using KinematicCharacterController;

#region Communication Structs
/////// <summary>
/////// A wrapper for the Transform class that allows transform state to only be read
/////// </summary>
////public struct ReadOnlyTransform
////{
////    private Transform transform;
////
////    public Vector3 position { get { return transform.position; } }
////    public Quaternion rotation { get { return transform.rotation; } }
////    public Vector3 localScale { get { return transform.localScale; } }
////    public Vector3 lossyScale { get { return transform.lossyScale; } }
////    public Matrix4x4 worldToLocalMatrix { get { return transform.worldToLocalMatrix; } }
////    public Matrix4x4 localToWorldMatrix { get { return transform.localToWorldMatrix; } }
////
////    public ReadOnlyTransform(Transform t)
////    {  
////        transform = t;
////    }
////}   

#endregion

/// <summary>
/// Unity Component that controls all Player Character mechanics and scripting
/// Abstract for specific character to derive from
/// </summary>
public class PlayerCharacter : MonoBehaviour, IPlayerCharacterCommunication
{
    // TODO move up to InputManager/PlayerInputController
    /// <summary>
    /// The input controller for the player
    /// </summary>
    PlayerController playerController;

#region Components 
    /// <summary>
    /// Handles character movement
    /// </summary>
    private PlayerMovement movement;
    /// <summary>
    /// Handles character animation
    /// </summary>
    private new PlayerAnimation animation;
    /// <summary>
    /// Handles the player's status
    /// </summary>
    private PlayerStatus status;
    /// <summary>
    /// Handles the player's combat
    /// </summary>
    private PlayerCombat combat;

    /// <summary>
    /// Handles communication between the components
    /// </summary>
    private PlayerInternalCommunicator internalCommunicator;
    /// <summary>
    /// Handles communication between the player and external game objects
    /// </summary>
    private PlayerExternalCommunicator externalCommunicator;
#endregion

#region fields
    bool inputLocked = false;
    Coroutine inputLockTimer;
#endregion

#region Unity MonoBehaviour Messages
    /// <summary>
    /// Handles class initialization
    /// </summary>
    void Awake()
    {
        animation = GetComponent<PlayerAnimation>();
        status = GetComponent<PlayerStatus>();
        combat = GetComponent<PlayerCombat>();
        movement = GetComponent<PlayerMovement>();

        // TODO move up to InputManager/PlayerInputController
        playerController = new PlayerController();
        playerController.Enable();
    }

    void Start()
    {
        SetupCommunicators();
    }


    /// <summary>
    /// Called on frame update
    /// </summary>
    void Update()
    {
        // TODO move up to InputManager/PlayerInputController
        HandleInput(playerController.Player);
   }
   
    /// <summary>
    /// Handles GameObject destruction
    /// </summary>
    void OnDestroy()
    {
        // TODO move up to InputManager/PlayerInputController
        playerController.Disable();
    }
#endregion

#region Class Setup

    /// <summary>
    /// Initializes communicator fields
    /// Calls abstract function to set up communicators based on derived class
    ///   then initializes communication between classes
    /// </summary>
    private void SetupCommunicators()
    {
        internalCommunicator = new PlayerInternalCommunicator();
        externalCommunicator = new PlayerExternalCommunicator();
        
        KinematicCharacterMotor motor = GetComponent<KinematicCharacterMotor>();

        // Set internal Communications
        SetCommunicationInterface(internalCommunicator);
        movement.SetCommunicationInterface(internalCommunicator);
        animation.SetCommunicationInterface(internalCommunicator);
        animation.SetReadOnlyReferences(new ReadOnlyKinematicMotor(motor), movement.GetReadOnlyAction());
        combat.SetCommunicationInterface(internalCommunicator);
        combat.SetReadOnlyReferences(new ReadOnlyKinematicMotor(motor), movement.GetReadOnlyAction());
        status.SetCommunicationInterface(internalCommunicator);
        externalCommunicator.SetCommunicationInterface(internalCommunicator);
        
        // Set External Communications
        // TODO: Have a better way to reference the camera than Camera.main.transform.parent
        PlayerCamera camera = Camera.main.transform.parent.GetComponent<PlayerCamera>();
        camera.SetPlayerExternalCommunication(externalCommunicator);
        camera.SetReadOnlyReferences(new ReadOnlyTransform(transform), new ReadOnlyKinematicMotor(motor));
    }
#endregion

#region Communication Methods
    public void SetCommunicationInterface(PlayerInternalCommunicator communicator)
    {
        // Set the communication
        communicator.SetCommunication(this);
    }

    public void LockInput(float time)
    {
        if(inputLockTimer != null)
            StopCoroutine(inputLockTimer);
        
        inputLockTimer = StartCoroutine(LockInputTimer(time));
    }
#endregion

    private IEnumerator LockInputTimer(float time)
    {
        inputLocked = true;
        movement.StartStun();
        combat.StartStun();
        yield return new WaitForSeconds(time);
        inputLocked = false;
        movement.EndStun();
        combat.EndStun();
    }

    // TODO Change to SetInput(), which should be called only once on initialization, and set actions/handlers for input action being triggered
    /// <summary>
    /// Handle input via PlayerController
    /// </summary>
    /// <param name="controllerActions">The controller actions state</param>
    public void HandleInput(PlayerController.PlayerActions controllerActions)
    {

        movement.HandleInput(controllerActions); 
        combat.HandleInput(controllerActions);

        externalCommunicator.HandleInput(controllerActions); 

        // Debug
        // ? Should this be move into a specific class made to handle debug options?
        #region debug
        // Resets the the motor state (used as a makeshift "level restart")
        if (controllerActions.Pause.triggered)
        {
            movement.ResetState();
        }
        #endregion
    }

}