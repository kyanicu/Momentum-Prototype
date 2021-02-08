using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using KinematicCharacterController;

public enum PlayableCharacter { Alesta, Cartia, Nephui, Ilphine }

/// <summary>
/// Unity Component that uses input to control the player character
/// </summary>
public class PlayerCharacterDirector : CharacterDirector
{
    /// <summary>
    /// The input controller for the player
    /// </summary>
    PlayerController playerController;

#region ExternalReferences
    new PlayerCamera camera;
#endregion

#region Components 
    /// <summary>
    /// Handles character movement
    /// </summary>
    private PlayerMovementAction movementAction;
    /// <summary>
    /// Handles the player's combat
    /// </summary>
    private PlayerCombat combat;

    new private PlayerAnimation animation;

    private AlestaMovementAbility alestaAbility;
    ////private CartiaMovementAbility cartiaAbility;
    ////private NephuiMovementAbility nephuiAbility;
    ////private IlphineMovementAbility ilphineAbility;
#endregion

    ////PlayableCharacter currentCharacter;
    ////private GameObject currentCharacterModelRoot;

    ////private GameObject alestaModelRoot;
    ////private GameObject cartiaModelRoot;
    ////private GameObject nephuiModelRoot;
    ////private GameObject ilphineModelRoot; 

#region Unity MonoBehaviour Messages
    /// <summary>
    /// Handles class initialization
    /// </summary>
    void Awake()
    {
        playerController = new PlayerController();
        playerController.Enable();

        ////alestaModelRoot = transform.GetChild(0).gameObject;
        ////cartiaModelRoot = transform.GetChild(1).gameObject;
        ////nephuiModelRoot = transform.GetChild(2).gameObject;
        ////ilphineModelRoot = transform.GetChild(3).gameObject;

        combat = GetComponent<PlayerCombat>();
        movementAction = GetComponent<PlayerMovementAction>();
        animation = GetComponent<PlayerAnimation>();

        alestaAbility = GetComponent<AlestaMovementAbility>();
        ////cartiaAbility = GetComponent<CartiaMovementAbility>();
        ////nephuiAbility = GetComponent<NephuiMovementAbility>();
        ////ilphineAbility = GetComponent<IlphineMovementAbility>();
////
        ////if (cartiaModelRoot.activeSelf)
        ////{
        ////    currentCharacter = PlayableCharacter.Cartia;
        ////    currentCharacterModelRoot = cartiaModelRoot;
        ////}
        ////else if (nephuiModelRoot.activeSelf)
        ////{
        ////    currentCharacter = PlayableCharacter.Nephui;
        ////    currentCharacterModelRoot = nephuiModelRoot;
        ////}
        ////else if (ilphineModelRoot.activeSelf)
        ////{
        ////    currentCharacter = PlayableCharacter.Ilphine;
        ////    currentCharacterModelRoot = ilphineModelRoot;
        ////}
        ////else 
        ////{
        ////    alestaModelRoot.SetActive(true);
        ////    currentCharacter = PlayableCharacter.Alesta;
        ////    currentCharacterModelRoot = alestaModelRoot;
        ////}
    }

    void Start()
    {
        EnableControl();

        camera = Camera.main.transform.parent.GetComponent<PlayerCamera>();
        camera.SetReadOnlyReferences(new ReadOnlyTransform(transform), GetComponent<MomentumMovement>());

        ///animation.ChangeCharacter(currentCharacterModelRoot);
    }

    ////private void ChangeCharacter(PlayableCharacter changeTo)
    ////{
    ////    switch(changeTo)
    ////    {
    ////        case (PlayableCharacter.Alesta) :
    ////            currentCharacter = PlayableCharacter.Alesta;
    ////            currentCharacterModelRoot = alestaModelRoot;
    ////            break;

    ////        case (PlayableCharacter.Cartia) :
    ////            currentCharacter = PlayableCharacter.Cartia;
    ////            currentCharacterModelRoot = cartiaModelRoot;
    ////            break;

    ////        case (PlayableCharacter.Nephui) :
    ////            currentCharacter = PlayableCharacter.Nephui;
    ////            currentCharacterModelRoot = nephuiModelRoot;
    ////            break;

    ////        case (PlayableCharacter.Ilphine) :
    ////            currentCharacter = PlayableCharacter.Ilphine;
    ////            currentCharacterModelRoot = ilphineModelRoot;
    ////            break;
    ////    }
    ////    animation.ChangeCharacter(currentCharacterModelRoot);
    ////}
   
    /// <summary>
    /// Handles GameObject destruction
    /// </summary>
    void OnDestroy()
    {
        playerController.Disable();
    }
#endregion

    // TODO Change to SetInput(), which should be called only once on initialization, and set actions/handlers for input action being triggered
    /// <summary>
    /// Handle input via PlayerController
    /// </summary>
    /// <param name="controllerActions">The controller actions state</param>
    protected override void RegisterControl()
    {
        SetMovementActionControl();
        SetCombatControl();
        SetAbilityControl();

        camera.HandleInput(playerController.Player);

        // Debug
        // ? Should this be move into a specific class made to handle debug options?
        #region debug
        // Resets the the motor state (used as a makeshift "level restart")
        if (playerController.Player.Pause.triggered)
        {
            GetComponent<MomentumMovement>().ResetState();
        }
        #endregion
    }

    private void SetCombatControl()
    {
        combat.control.attack = playerController.Player.NeutralAttack.triggered;
        combat.control.direction = new Vector2(playerController.Player.Run.ReadValue<float>(), playerController.Player.VerticalDirection.ReadValue<float>());
    }

    private void SetMovementActionControl()
    {
        movementAction.control.jump = playerController.Player.Jump.triggered;
        movementAction.control.jumpCancel = playerController.Player.JumpCancel.triggered;
        movementAction.control.run = playerController.Player.Run.ReadValue<float>();
        movementAction.control.doubleTapRun = playerController.Player.RunKickOff.triggered;
    }

    private void SetAbilityControl()
    {
        SetAlestaAbilityControl();
        ////switch(currentCharacter)
        ////{
        ////    case (PlayableCharacter.Alesta) :
        ////        SetAlestaAbilityControl();
        ////        break;

        ////    case (PlayableCharacter.Cartia) :
        ////        SetCartiaAbilityControl();
        ////        break;

        ////    case (PlayableCharacter.Nephui) :
        ////        SetNephuiAbilityControl();
        ////        break;

        ////    case (PlayableCharacter.Ilphine) :
        ////        SetIlphineAbilityControl();
        ////        break;
        ////}
    }

    private void SetAlestaAbilityControl()
    {
        alestaAbility.control.permeation = playerController.Player.Permeation.ReadValue<float>() > 0;
    }

    ////private void SetCartiaAbilityControl()
    ////{

    ////}

    ////private void SetNephuiAbilityControl()
    ////{

    ////}

    ////private void SetIlphineAbilityControl()
    ////{

    ////}
}