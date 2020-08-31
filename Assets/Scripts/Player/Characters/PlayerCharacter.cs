using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using KinematicCharacterController;

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

public abstract class PlayerCharacter<Ability> : MonoBehaviour where Ability : IPlayerMovementAbility, new()
{

    PlayerController playerController;
    
    [SerializeField]
    private PlayerMovement<Ability> movement;
    [SerializeField]
    private new PlayerAnimation animation;

    private PlayerInternalCommunicator internalCommunicator;
    private PlayerExternalCommunicator externalCommunicator;

    private KinematicCharacterMotor motor;

    // Start is called before the first frame update
    void Awake()
    {
        // Temporarily here, will be moved up to InputManager/PlayerInputController
        playerController = new PlayerController();
        playerController.Enable();

        motor = GetComponent<KinematicCharacterMotor>();

        SetupCommunicators();
    }

    void Reset()
    {
        SetupAbstractClass();
    }

    void OnValidate()
    {
        movement.OnValidate();
    }

    void SetupAbstractClass()
    {
        movement = new PlayerMovement<Ability>();
        animation = new PlayerAnimation(transform.GetChild(0).gameObject);
    }

    protected abstract void SetupConcreteCommunicators(out PlayerInternalCommunicator internComm, out PlayerExternalCommunicator externComm);

    private void SetupCommunicators()
    {
        SetupConcreteCommunicators(out internalCommunicator, out externalCommunicator);
        movement.SetCommunication(internalCommunicator);
        animation.SetCommunication(internalCommunicator);
        externalCommunicator.SetCommunication(internalCommunicator);
        
        Camera.main.transform.parent.GetComponent<PlayerCamera>().SetPlayerExternalCommunication(externalCommunicator, new ReadOnlyTransform(transform));
    }

    void Start()
    {
        // Set motor's reference to this Character Controller Interface
        motor.CharacterController = (movement as ICharacterController);
        movement.InitializeForPlay(motor);
    }

    // Temporarily here, will be moved up to InputManager/PlayerInputController
    void OnDestroy()
    {
        playerController.Disable();
    }

    public void HandleInput(PlayerController.PlayerActions controllerActions)
    {
        movement.HandleInput(controllerActions); 

        // Debug
        #region debug
        // Resets the the motor state (used as a makeshift "level restart")
        if (controllerActions.Pause.triggered)
        {
            movement.ResetState(motor);
        }
        #endregion
    }

    void OnTriggerEnter(Collider col)
    {
        movement.HandleTriggerEnter(motor, col);
    }
    void OnTriggerExit(Collider col)
    {
        movement.HandleTriggerExit(col);
    }

    // Update is called once per frame
    void Update()
    {
        // Temporarily here, will be moved up to InputManager/PlayerInputController
        HandleInput(playerController.Player);

        animation.FrameUpdate();
    }
}
