using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionCharacter : Character
{
    /// <summary>
    /// Handles the player's status
    /// </summary>
    [SerializeField] private PlayerStatus status;
    /// <summary>
    /// Handles the player's combat
    /// </summary>
    [SerializeField] private PlayerCombat combat;
}
