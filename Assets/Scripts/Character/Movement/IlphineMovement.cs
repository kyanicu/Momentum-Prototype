using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IlphineMovement : PlayerMovement
{
    [SerializeField]
    private IlphineMovementAbility ability;

    protected override void SetAbility(out IPlayerMovementAbility _ability)
    {
        ability = new IlphineMovementAbility();
        _ability = ability;
    }
}
