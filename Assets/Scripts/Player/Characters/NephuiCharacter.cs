using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NephuiCharacter : PlayerCharacter<NephuiMovementAbility>
{
    protected override void SetupConcreteCommunicators(out PlayerInternalCommunicator internComm, out PlayerExternalCommunicator externComm)
    {
        internComm = new NephuiInternalCommunicator();
        externComm = new NephuiExternalCommunicator();
    }
}
