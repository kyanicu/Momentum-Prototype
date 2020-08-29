using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public interface IDeletheiMovementAbilityCommunication : IPlayerCommunication
{
    
}

public class DeletheiInternalCommunicator : PlayerInternalCommunicator
{

    IDeletheiMovementAbilityCommunication ability;

    public void SetCommunication(IDeletheiMovementAbilityCommunication communication)
    {
        ability = communication;
    }
}