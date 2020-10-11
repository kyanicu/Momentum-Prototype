﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using KinematicCharacterController;


/// <summary>
/// A wrapper for the Transform class that allows only reading of main fields
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

/// <summary>
/// GameObject Component that represents and handles all aspects of a Player Character
/// </summary>
/// <typeparam name="Ability"> The ability class of the given character </typeparam>
public abstract class PlayerCharacter<Ability> : MonoBehaviour where Ability : IPlayerMovementAbility, new()
{

    PlayerController playerController;
    
    [SerializeField]
    private PlayerMovement<Ability> movement;
    [SerializeField]
    private new PlayerAnimation animation;
    [SerializeField]
    private PlayerStatus status;

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
        if (movement == null)
            movement = new PlayerMovement<Ability>();
        
        if (animation == null)
            animation = new PlayerAnimation(transform.GetChild(0).gameObject);

        if (status == null)
            status = new PlayerStatus();

        movement.OnValidate();
    }

    void SetupAbstractClass()
    {
        movement = new PlayerMovement<Ability>();
        animation = new PlayerAnimation(transform.GetChild(0).gameObject);
        status = new PlayerStatus();
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

        externalCommunicator.HandleInput(controllerActions); 

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
        //if (!trackedTriggerObjects.Contains(col))
        //{
            status.HandleTriggerEnter(col);
            movement.HandleTriggerEnter(motor, col);
        //}
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
