using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CartiaMovement : PlayerMovement
{
    [SerializeField]
    private CartiaMovementAbility ability;

    protected override void SetAbility(out IPlayerMovementAbility _ability)
    {
        ability = new CartiaMovementAbility();
        _ability = ability;
    }
}
