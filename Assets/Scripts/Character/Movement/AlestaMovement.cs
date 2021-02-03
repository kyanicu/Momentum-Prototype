using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AlestaMovement : PlayerMovement
{
    [SerializeField]
    private AlestaMovementAbility ability;

    protected override void SetAbility(out IPlayerMovementAbility _ability)
    {
        ability = new AlestaMovementAbility();
        _ability = ability;
    }
}
