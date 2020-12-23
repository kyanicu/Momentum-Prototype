using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// The communication interface for Alesta's movement abilities
/// </summary>
public interface IAlestaMovementAbilityCommunication : IPlayerCommunication
{
    
}

/// <summary>
/// Handles internal communication specific to Alesta
/// </summary>
public class AlestaInternalCommunicator : PlayerInternalCommunicator
{
    /// <summary>
    /// The reference to the communication interface for Alesta's movement abilities
    /// </summary>
    IAlestaMovementAbilityCommunication ability;

    /// <summary>
    /// Sets the communication for Alesta's movement abilities
    /// </summary>
    /// <param name="communication">Communication interface for the communicator to reference</param>
    public void SetCommunication(IAlestaMovementAbilityCommunication communication)
    {
        ability = communication;
    }
}