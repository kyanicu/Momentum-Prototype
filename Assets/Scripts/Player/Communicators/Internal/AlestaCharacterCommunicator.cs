using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public interface IAlestaMovementAbilityCommunication : IPlayerCommunication
{
    
}

public class AlestaInternalCommunicator : PlayerInternalCommunicator
{

    IAlestaMovementAbilityCommunication ability;

    public void SetCommunication(IAlestaMovementAbilityCommunication communication)
    {
        ability = communication;
    }
}