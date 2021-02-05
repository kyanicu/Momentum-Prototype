using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using KinematicCharacterController;

/// <summary>
/// Communication interface for PlayerCharacter
/// </summary>
public interface IPlayerCharacterCommunication
{
    void LockInput(float time);
}

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
#endregion

#region ExternalReferences
    new PlayerCamera camera;
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
        // TODO move up to InputManager/PlayerInputController
        playerController = new PlayerController();
        playerController.Enable();
    }

    void Start()
    {

        animation = GetComponent<PlayerAnimation>();
        status = GetComponent<PlayerStatus>();
        combat = GetComponent<PlayerCombat>();
        movement = GetComponent<PlayerMovement>();

        camera = Camera.main.transform.parent.GetComponent<PlayerCamera>();
        camera.SetReadOnlyReferences(new ReadOnlyTransform(transform), movement);
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

#region Communication Methods
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
        camera.HandleInput(controllerActions);

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