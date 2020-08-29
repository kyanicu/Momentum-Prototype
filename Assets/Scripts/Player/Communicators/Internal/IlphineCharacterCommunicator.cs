using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public interface IIlphineMovementAbilityCommunication : IPlayerCommunication
{
    
}

public class IlphineInternalCommunicator : PlayerInternalCommunicator
{

    IIlphineMovementAbilityCommunication ability;

    public void SetCommunication(IIlphineMovementAbilityCommunication communication)
    {
        ability = communication;
    }

}