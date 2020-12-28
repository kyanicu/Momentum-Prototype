using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class representing the playable character of Cartia
/// Overrides PlayerCharacter to handle specific character aspects
/// </summary>
public class CartiaCharacter : PlayerCharacter
{
    /// <summary>
    /// Initializes concrete class
    /// </summary>
    /// <param name="_movement">The movement field in PlayerCharacter to be set by concrete class</param>
    protected override void SetupConcreteClass(out PlayerMovement _movement)
    {
        _movement = new PlayerMovement(new CartiaMovementAbility());
    }

    /// <summary>
    /// Initializes communicators for appropriate concrete class usage
    /// </summary>
    /// <param name="internComm">The internal communicator field in PlayerCharacter to be set</param>
    /// <param name="externComm">The external communicator field in PlayerCharacter to be set</param>
    protected override void SetupConcreteCommunicators(out PlayerInternalCommunicator internComm, out PlayerExternalCommunicator externComm)
    {
        internComm = new CartiaInternalCommunicator();
        externComm = new CartiaExternalCommunicator();
    }
}

