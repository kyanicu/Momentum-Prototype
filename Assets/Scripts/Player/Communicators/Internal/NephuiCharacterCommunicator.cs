using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public interface INephuiMovementAbilityCommunication : IPlayerCommunication
{
    
}

public class NephuiInternalCommunicator : PlayerInternalCommunicator
{

    INephuiMovementAbilityCommunication ability;

    public void SetCommunication(INephuiMovementAbilityCommunication communication)
    {
        ability = communication;
    }

}