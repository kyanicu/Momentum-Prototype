using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IlphineCharacter : PlayerCharacter<IlphineMovementAbility>
{
    protected override void SetupConcreteCommunicators(out PlayerInternalCommunicator internComm, out PlayerExternalCommunicator externComm)
    {
        internComm = new IlphineInternalCommunicator();
        externComm = new IlphineExternalCommunicator();
    }
}
