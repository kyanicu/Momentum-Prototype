using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using KinematicCharacterController;

#region Communication Structs
/// <summary>
/// A wrapper for the Transform class that allows transform state to only be read
/// </summary>
public struct ReadOnlyTransform
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

/// <summary>
/// A wrapper for the KinematicCharacterMotor class that allows a constant reference state to only be read
/// </summary>
public struct ReadOnlyKinematicMotor
{
    private KinematicCharacterMotor motor;

    public Vector3 position { get { return motor.TransientPosition; } }
    public Quaternion rotation { get { return motor.TransientRotation; } }
    public Vector3 velocity { get { return motor.BaseVelocity; } }
    public Vector3 groundNormal { get { return motor.GetEffectiveGroundNormal(); } }
    public Vector3 lastGroundNormal { get { return motor.GetLastEffectiveGroundNormal(); } }
    public bool isGroundedThisUpdate { get { return motor.IsGroundedThisUpdate; } }
    public bool wasGroundedLastUpdate { get { return motor.WasGroundedLastUpdate; } }

    public ReadOnlyKinematicMotor(KinematicCharacterMotor m)
    {  
        motor = m;
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
    /// Handles the player's combat
    /// </summary>
    [SerializeField] private PlayerCombat combat;

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

        Debug.Log("Player hit: " + col.gameObject.name);
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
        status = new PlayerStatus(transform.GetChild(0).GetChild(2).gameObject);
        combat = new PlayerCombat(transform.GetChild(0).GetChild(1).gameObject);

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

        // Set internal Communications
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
        combat.HandleInput(controllerActions);

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