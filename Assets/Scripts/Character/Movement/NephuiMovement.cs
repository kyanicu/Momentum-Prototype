using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NephuiMovement : PlayerMovement
{
    [SerializeField]
    private NephuiMovementAbility ability;

    protected override void SetAbility(out IPlayerMovementAbility _ability)
    {
        ability = new NephuiMovementAbility();
        _ability = ability;
    }
}
