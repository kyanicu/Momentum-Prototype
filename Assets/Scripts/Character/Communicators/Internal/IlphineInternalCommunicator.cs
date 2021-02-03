using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// The communication interface for Ilphine's movement abilities
/// </summary>
public interface IIlphineMovementAbilityCommunication : IPlayerCommunication
{
    
}

/// <summary>
/// Handles internal communication specific to Ilphine
/// </summary>
public class IlphineInternalCommunicator : PlayerInternalCommunicator
{
    /// <summary>
    /// The reference to the communication interface for Ilphine's movement abilities
    /// </summary>
    IIlphineMovementAbilityCommunication ability;

    /// <summary>
    /// Sets the communication for Ilphine's movement abilities
    /// </summary>
    /// <param name="communication">Communication interface for the communicator to reference</param>
    public void SetCommunication(IIlphineMovementAbilityCommunication communication)
    {
        ability = communication;
    }

}