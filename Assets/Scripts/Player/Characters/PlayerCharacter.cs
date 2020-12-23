using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using KinematicCharacterController;

#region Helper Classes
/// <summary>
/// A wrapper for the Transform class that allows transform state to only be read
/// </summary>
public class ReadOnlyTransform
{
    private Transform transform;

    public Vector3 position { get { return transform.position; } }
    public Quaternion rotation { get { return transform.rotation; } }
    public Vector3 localScale { get { return transform.localScale; } }
    public Vector3 lossyScale { get { return transform.lossyScale; } }
    public Matrix4x4 worldToLocalMatrix { get { return transform.worldToLocalMatrix; } }
    public Matrix4x4 localToWorldMatrix { get { return transform.localToWorldMatrix; } }

    public ReadOnlyTransform(Transform t)
    {  
        transform = t;
    }
}   
#endregion
/// <summary>
/// Unity Component that controls all Player Character mechanics and scripting
/// Abstract for specific character to derive from
/// </summary>
public abstract class PlayerCharacter : MonoBehaviour
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
    [SerializeField] private PlayerMovement movement;
    /// <summary>
    /// Handles character animation
    /// </summary>
    [SerializeField] private new PlayerAnimation animation;
    /// <summary>
    /// Handles the player's status
    /// </summary>
    [SerializeField] private PlayerStatus status;

    /// <summary>
    /// Handles communication between the components
    /// </summary>
    private PlayerInternalCommunicator internalCommunicator;
    /// <summary>
    /// Handles communication between the player and external game objects
    /// </summary>
    private PlayerExternalCommunicator externalCommunicator;
#endregion

#region Sibling Unity Component References 
    /// <summary>
    /// Unity Component that handles collision and kinematic movement
    /// Used by PlayerMovement to handle player movement mechanics
    /// </summary>
    private KinematicCharacterMotor motor;
#endregion

#region Unity MonoBehaviour Messages
    /// <summary>
    /// Handles class initialization
    /// </summary>
    void Awake()
    {
        SetupAbstractClass();

        // TODO move up to InputManager/PlayerInputController
        playerController = new PlayerController();
        playerController.Enable();

        motor = GetComponent<KinematicCharacterMotor>();

        SetupCommunicators();
    }

    /// <summary>
    /// Validates state on inspector change
    ///  TODO Make unnecessary with editor script
    /// </summary>
    void OnValidate()
    {
        ////if (movement == null)
        ////    movement = new PlayerMovement();
        
        ////if (animation == null)
        ////    animation = new PlayerAnimation(transform.GetChild(0).gameObject);
        
        ////if (status == null)
        ////    status = new PlayerStatus();
        /// 
        //  TODO Make unnecessary with editor script
        movement.OnValidate();
    }

    /// <summary>
    /// Initializes GameObject for play
    /// </summary>
    void Start()
    {
        // Set motor's Character Controller Interface reference to the movement field
        motor.CharacterController = (movement as ICharacterController);

        movement.InitializeForPlay(motor);
    }

    /// <summary>
    /// Handles when the kinematic character motor enters a trigger
    /// </summary>
    /// <param name="col">The trigger collider entered</param>
    void OnTriggerEnter(Collider col)
    {
        status.HandleTriggerEnter(col);
        movement.HandleTriggerEnter(motor, col);
    }

    /// <summary>
    /// Handles when the kinematic character motor leaves a trigger
    /// </summary>
    /// <param name="col">The trigger collider exited</param>
    void OnTriggerExit(Collider col)
    {
        movement.HandleTriggerExit(col);
    }

    /// <summary>
    /// Called on frame update
    /// </summary>
    void Update()
    {
        // TODO move up to InputManager/PlayerInputController
        HandleInput(playerController.Player);

        animation.FrameUpdate();
 
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
    /// Initializes class as a whole
    /// Sets up everything shared between all derived classes,
    ///   then calls abstract function to set up everything in derived class
    /// </summary>
    private void SetupAbstractClass()
    {
        animation = new PlayerAnimation(transform.GetChild(0).gameObject);
        status = new PlayerStatus();

        SetupConcreteClass(out movement);
    }

    /// <summary>
    /// Initializes concrete derived classes
    /// </summary>
    /// <param name="_movement">The movement field in PlayerCharacter to be set by concrete class</param>
    protected abstract void SetupConcreteClass(out PlayerMovement _movement);

    /// <summary>
    /// Initializes communicator fields
    /// Calls abstract function to set up communicators based on derived class
    ///   then initializes communication between classes
    /// </summary>
    private void SetupCommunicators()
    {
        SetupConcreteCommunicators(out internalCommunicator, out externalCommunicator);

        movement.SetCommunicationInterface(internalCommunicator);
        animation.SetCommunicationInterface(internalCommunicator);
        externalCommunicator.SetCommunicationInterface(internalCommunicator);
        
        // ? Should this be handled in external communicator?
        // TODO yes, yes it should
        Camera.main.transform.parent.GetComponent<PlayerCamera>().SetPlayerExternalCommunication(externalCommunicator, new ReadOnlyTransform(transform));
    }

    /// <summary>
    /// Initializes communicators for appropriate concrete class usage
    /// </summary>
    /// <param name="internComm">The internal communicator field in PlayerCharacter to be set by concrete class</param>
    /// <param name="externComm">The external communicator field in PlayerCharacter to be set by concrete class</param>
    protected abstract void SetupConcreteCommunicators(out PlayerInternalCommunicator internComm, out PlayerExternalCommunicator externComm);
#endregion

    // TODO Change to SetInput(), which should be called only once on initialization, and set actions/handlers for input action being triggered
    /// <summary>
    /// Handle input via PlayerController
    /// </summary>
    /// <param name="controllerActions">The controller actions state</param>
    public void HandleInput(PlayerController.PlayerActions controllerActions)
    {
        movement.HandleInput(controllerActions); 

        externalCommunicator.HandleInput(controllerActions); 

        // Debug
        // ? Should this be move into a specific class made to handle debug options?
        #region debug
        // Resets the the motor state (used as a makeshift "level restart")
        if (controllerActions.Pause.triggered)
        {
            movement.ResetState(motor);
        }
        #endregion
    }

}