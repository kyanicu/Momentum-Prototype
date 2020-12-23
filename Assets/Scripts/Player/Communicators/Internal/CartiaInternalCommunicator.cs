using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// The communication interface for Cartia's movement abilities
/// </summary>
public interface ICartiaMovementAbilityCommunication : IPlayerCommunication
{
    
}

/// <summary>
/// Handles internal communication specific to Cartia
/// </summary>
public class CartiaInternalCommunicator : PlayerInternalCommunicator
{
    /// <summary>
    /// The reference to the communication interface for Cartia's movement abilities
    /// </summary>
    ICartiaMovementAbilityCommunication ability;

    /// <summary>
    /// Sets the communication for Cartia's movement abilities
    /// </summary>
    /// <param name="communication">Communication interface for the communicator to reference</param>
    public void SetCommunication(ICartiaMovementAbilityCommunication communication)
    {
        ability = communication;
    }
}