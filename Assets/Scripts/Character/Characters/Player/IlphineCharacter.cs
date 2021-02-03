using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class representing the playable character of Ilphine
/// Overrides PlayerCharacter to handle specific character aspects
/// </summary>
public class IlphineCharacter : PlayerCharacter
{
    [SerializeField]
    private IlphineMovement movement;

    /// <summary>
    /// Initializes concrete class
    /// </summary>
    /// <param name="_movement">The movement field in PlayerCharacter to be set by concrete class</param>
    protected override void SetupConcreteClass(out PlayerMovement _movement)
    {
        movement = new IlphineMovement();
        _movement = movement;
    }

    /// <summary>
    /// Initializes communicators for appropriate concrete class usage
    /// </summary>
    /// <param name="internComm">The internal communicator field in PlayerCharacter to be set</param>
    /// <param name="externComm">The external communicator field in PlayerCharacter to be set</param>
    protected override void SetupConcreteCommunicators(out PlayerInternalCommunicator internComm, out PlayerExternalCommunicator externComm)
    {
        internComm = new IlphineInternalCommunicator();
        externComm = new IlphineExternalCommunicator();
    }
}