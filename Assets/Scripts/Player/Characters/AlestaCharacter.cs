using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlestaCharacter : PlayerCharacter<AlestaMovementAbility>
{
    protected override void SetupConcreteCommunicators(out PlayerInternalCommunicator internComm, out PlayerExternalCommunicator externComm)
    {
        internComm = new AlestaInternalCommunicator();
        externComm = new AlestaExternalCommunicator();
    }
}
