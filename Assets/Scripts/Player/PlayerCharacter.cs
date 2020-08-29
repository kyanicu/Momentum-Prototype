using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using KinematicCharacterController;

public enum Character { Alesta, Nephui, Delethei, Ilphine }

public class PlayerCharacter : MonoBehaviour
{

    Character character;

    PlayerController playerController;
    
    [SerializeField]
    private PlayerMovement movement;
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
        
        SetupClass();
    }

    void Reset()
    {
        SetupClass();
    }

    void SetupClass()
    {
        switch (gameObject.name)
        {
            case("Alesta") :
                character = Character.Alesta;
                break;
            case("Nephui") :
                character = Character.Nephui;
                break;
            case("Delethei") :
                character = Character.Delethei;
                break;
            case("Ilphine") :
                character = Character.Ilphine;
                break;
            default :
                character = Character.Alesta;
                break;
        }

        movement = new PlayerMovement(character);
        animation = new PlayerAnimation(transform.GetChild(0).gameObject);
        SetupCommunicators();
    }

    private void SetupCommunicators()
    {
        switch (character)
        {
            case(Character.Alesta) :
                internalCommunicator = new AlestaInternalCommunicator();
                externalCommunicator = new AlestaExternalCommunicator();
                break;
            case(Character.Nephui) :
                internalCommunicator = new NephuiInternalCommunicator();
                externalCommunicator = new NephuiExternalCommunicator();
                break;
            case(Character.Delethei) :
                internalCommunicator = new DeletheiInternalCommunicator();
                externalCommunicator = new DeletheiExternalCommunicator();
                break;
            case(Character.Ilphine) :
                internalCommunicator = new IlphineInternalCommunicator();
                externalCommunicator = new IlphineExternalCommunicator();
                break;
            default :
                internalCommunicator = new AlestaInternalCommunicator();
                externalCommunicator = new AlestaExternalCommunicator();
                break;
        }
        movement.SetCommunication(internalCommunicator);
        animation.SetCommunication(internalCommunicator);
        externalCommunicator.SetCommunication(internalCommunicator);
        
        Camera.main.transform.parent.GetComponent<PlayerCamera>().SetPlayerExternalCommunication(externalCommunicator);
        
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
    }
}
