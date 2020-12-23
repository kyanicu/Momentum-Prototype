using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// The communication interface for Nephui's movement abilities
/// </summary>
public interface INephuiMovementAbilityCommunication : IPlayerCommunication
{
    
}

/// <summary>
/// Handles internal communication specific to Nephui
/// </summary>
public class NephuiInternalCommunicator : PlayerInternalCommunicator
{
    /// <summary>
    /// The reference to the communication interface for Nephui's movement abilities
    /// </summary>
    INephuiMovementAbilityCommunication ability;

    /// <summary>
    /// Sets the communication for Nephui's movement abilities
    /// </summary>
    /// <param name="communication">Communication interface for the communicator to reference</param>
    public void SetCommunication(INephuiMovementAbilityCommunication communication)
    {
        ability = communication;
    }

}