using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeletheiCharacter : PlayerCharacter<DeletheiMovementAbility>
{
    protected override void SetupConcreteCommunicators(out PlayerInternalCommunicator internComm, out PlayerExternalCommunicator externComm)
    {
        internComm = new DeletheiInternalCommunicator();
        externComm = new DeletheiExternalCommunicator();
    }
}
