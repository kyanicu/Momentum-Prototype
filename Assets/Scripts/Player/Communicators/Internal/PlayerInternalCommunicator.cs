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
/// Communication interface for PlayerCharacter
/// </summary>
public interface IPlayerCharacterCommunication : IPlayerCommunication
{
    void LockInput(float time);
}

public interface IPlayerMovementOverrider
{
    event Action<FullMovementOverride> ApplyMovementOverride;
    event Action<FullMovementOverride> RemoveMovementOverride;
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

    ReadOnlyPlayerMovementAction GetReadOnlyAction();

    void ApplyMovementOverride(FullMovementOverride overrides);
    
    void RemoveMovementOverride(FullMovementOverride overrides);

    void ZeroVelocity(bool zeroAngularVelocity = false);
    void LockVelocity(float time, bool revertExtVelAfter);////, bool lockAngularVelocity = false);
    void Flinch();
    void ForceUnground();
    void AddImpulse(Vector3 Impulse);
    void AddImpulseAtPoint(Vector3 impulse, Vector3 point);

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

    void SetReadOnlyReferences(ReadOnlyKinematicMotor motor, ReadOnlyPlayerMovementAction actions);

    /// <summary>
    /// Called when player changes facing direction
    /// </summary>
    void ChangeFacingDirection();

    void AnimateNeutralAttack();
    void AnimateDownAttack();
    void AnimateUpAttack();
    void AnimateRunningAttack();
    void AnimateBrakingAttack();
    void AnimateNeutralAerialAttack();
    void AnimateBackAerialAttack();
    void AnimateDownAerialAttack();
    void AnimateUpAerialAttack();

    void AnimateFlinch();

    void StartIFrames();
    void EndIFrames();

    event Action<AttackAnimationState> attackStateTransition;
}

/// <summary>
/// Communication interface for player combat
/// </summary>
public interface IPlayerCombatCommunication : IPlayerCommunication, IPlayerMovementOverrider
{

    void SetReadOnlyReferences(ReadOnlyKinematicMotor motor, ReadOnlyPlayerMovementAction action);

    event Action neutralAttack;
    event Action downAttack;
    event Action upAttack;
    event Action runningAttack;
    event Action brakingAttack;
    event Action neutralAerialAttack;
    event Action backAerialAttack;
    event Action downAerialAttack;
    event Action upAerialAttack;

    event Action<Vector3,float> takeKinematicRecoil;
    event Action<Vector3> takeDynamicRecoil;
    event Action<Vector3,Vector3> takeDynamicRecoilWithTorque;

    void AttackAnimationStateTransition(AttackAnimationState newState);

    void Flinch();
} 

/// <summary>
/// Communication interface for player status
/// </summary>
public interface IPlayerStatusCommunication : IPlayerCommunication
{
    event Action flinch;
    event Action halt;
    event Action forceUnground;
    event Action<float> stun;
    event Action <Vector3, float> takeKinematicKnockback;
    event Action<Vector3> takeDynamicKnockback;
    event Action<Vector3, Vector3> takeDynamicKnockbackWithTorque;

    event Action iFramesStarted;
    event Action iFramesEnded;
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
    /// Called when the players gravity direction changes
    /// </summary>
    /// <param name="gravityDirection">New direction of gravity</param>
    void HandlePlayerGravityDirectionChanged(Vector3 gravityDirection);
}
#endregion

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
    /// Communication with PlayerCharacter
    /// </summary>
    IPlayerCharacterCommunication character;
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
    /// Communication with player combat
    /// </summary>
    IPlayerCombatCommunication combat;
    /// <summary>
    /// Communication with player status
    /// </summary>
    IPlayerStatusCommunication status;
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
    public void SetCommunication(IPlayerCharacterCommunication communication)
    {
        character = communication;
    }

    /// <summary>
    /// Sets communication with PlayerMovement
    /// </summary>
    /// <param name="communication"> PlayerMovement communication interface</param>
    public void SetCommunication(IPlayerMovementCommunication communication)
    {
        movement = communication;
        movement.planeChanged += PlaneChangedHandler;
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
        animation.attackStateTransition += AttackAnimationStateTransitionHandler;
    }

    /// <summary>
    /// Sets communication with PlayerCombat
    /// </summary>
    /// <param name="communication"> PlayerCombat communication interface</param>
    public void SetCommunication(IPlayerCombatCommunication communication)
    {
        combat = communication;
        combat.neutralAttack += NuetralAttackHandler;
        combat.downAttack += DownAttackHandler;
        combat.upAttack += UpAttackHandler;
        combat.runningAttack += RunningAttackHandler;
        combat.brakingAttack += BrakingAttackHandler;
        combat.neutralAerialAttack += NeutralAerialAttackHandler;
        combat.backAerialAttack += BackAerialAttackHandler;
        combat.downAerialAttack += DownAerialAttackHandler;
        combat.upAerialAttack += UpAerialAttackHandler;

        combat.ApplyMovementOverride += movement.ApplyMovementOverride;
        combat.RemoveMovementOverride += movement.RemoveMovementOverride;

        combat.takeKinematicRecoil += TakeKinematicRecoilHandler;
        combat.takeDynamicRecoil += TakeDynamicRecoilHandler;
        combat.takeDynamicRecoilWithTorque += TakeDynamicRecoilWithTorqueHandler;
    }

    /// <summary>
    /// Sets communication with PlayerStatus
    /// </summary>
    /// <param name="communication"> PlayerStatus communication interface</param>
    public void SetCommunication(IPlayerStatusCommunication communication)
    {
        status = communication;

        status.flinch += FlinchHandler;
        status.halt += HaltHandler;
        status.forceUnground += ForceUngroundHandler;
        status.stun += StunHandler;
        status.takeKinematicKnockback += TakeKinematicKnockbackHandler;
        status.takeDynamicKnockback += TakeDynamicKnockbackHandler;
        status.takeDynamicKnockbackWithTorque += TakeDynamicKnockbackWithTorqueHandler;

        status.iFramesStarted += IFrameStartedHandler;
        status.iFramesEnded += IFrameEndedHandler;        
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

    private void NuetralAttackHandler()
    {
        animation.AnimateNeutralAttack();
    }
    void DownAttackHandler()
    {
        animation.AnimateDownAttack();
    }
    void UpAttackHandler()
    {
        animation.AnimateUpAttack();
    }
    void RunningAttackHandler()
    {
        animation.AnimateRunningAttack();
    }
    void BrakingAttackHandler()
    {
        animation.AnimateBrakingAttack();
    }
    void NeutralAerialAttackHandler()
    {
        animation.AnimateNeutralAerialAttack();
    }
    void BackAerialAttackHandler()
    {
        animation.AnimateBackAerialAttack();
    }
    void DownAerialAttackHandler()
    {
        animation.AnimateDownAerialAttack();
    }
    void UpAerialAttackHandler()
    {
        animation.AnimateUpAerialAttack();
    }
    #endregion

    #region External Communicator Notifiers

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

    #region Combat Notifiers
    private void AttackAnimationStateTransitionHandler(AttackAnimationState newState)
    {
        combat.AttackAnimationStateTransition(newState);
    }
    #endregion

    #region Status Events

    private void IFrameStartedHandler()
    {
        animation.StartIFrames();
    }

    private void IFrameEndedHandler()
    {
        animation.EndIFrames();
    }

    private void FlinchHandler()
    {
        movement.Flinch();
        combat.Flinch();
        animation.AnimateFlinch();
    }

    private void HaltHandler()
    {
        movement.ZeroVelocity(true);
    }
    private void ForceUngroundHandler()
    {
        movement.ForceUnground();
    }
    private void StunHandler(float time)
    {
        character.LockInput(time);
    }
    private void TakeKinematicKnockbackHandler(Vector3 knockback, float time)
    {
        movement.AddImpulse(knockback);
        movement.LockVelocity(time, true);
    }
    private void TakeDynamicKnockbackHandler(Vector3 knockback)
    {
        movement.AddImpulse(knockback);
    }
    private void TakeDynamicKnockbackWithTorqueHandler(Vector3 knockback, Vector3 atPoint)
    {
        movement.AddImpulseAtPoint(knockback, atPoint);
    }
    #endregion

    #region Combat Events
    private void TakeKinematicRecoilHandler(Vector3 recoil, float time)
    {
        
        movement.AddImpulse(recoil);
        movement.LockVelocity(time, true);
    }

    private void TakeDynamicRecoilHandler(Vector3 recoil)
    {
        movement.AddImpulse(recoil);
    }

    private void TakeDynamicRecoilWithTorqueHandler(Vector3 recoil, Vector3 atPoint)
    {
        movement.AddImpulseAtPoint(recoil, atPoint);
    }

    #endregion
}
