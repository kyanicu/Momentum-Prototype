using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct MutableTuple<i,j>
{
    public i item1;
    public j item2;

    public MutableTuple(i i1,j i2)
    {
        item1 = i1;
        item2 = i2;
    }
}

[System.Serializable]
public struct FullMovementOverride
{
    [SerializeField]
    public List<MutableTuple<PlayerMovementValues, PlayerOverrideType>> movementOverrides;

    [SerializeField]
    public List<MutableTuple<PlayerMovementPhysicsValues, PlayerOverrideType>> physicsOverrides;

    [SerializeField]
    public List<MutableTuple<PlayerMovementActionValues, PlayerOverrideType>> actionOverrides;


    [SerializeField]
    public List<MutableTuple<AlestaMovementAbilityValues, PlayerOverrideType>> alestaAbilityOverrides;

    [SerializeField]
    public List<MutableTuple<NephuiMovementAbilityValues, PlayerOverrideType>> nephuiAbilityOverrides;

    [SerializeField]
    public List<MutableTuple<CartiaMovementAbilityValues, PlayerOverrideType>> cartiaAbilityOverrides;

    [SerializeField]
    public List<MutableTuple<IlphineMovementAbilityValues, PlayerOverrideType>> ilphineAbilityOverrides;
}

public class MovementEffector : MonoBehaviour
{

    [SerializeField]
    private List<MutableTuple<PlayerMovementValues, PlayerOverrideType>> _movementOverrides = new List<MutableTuple<PlayerMovementValues, PlayerOverrideType>>();
    public List<MutableTuple<PlayerMovementValues, PlayerOverrideType>> movementOverrides { get { return _movementOverrides; } private set { _movementOverrides = value; } }

    [SerializeField]
    private List<MutableTuple<PlayerMovementPhysicsValues, PlayerOverrideType>> _physicsOverrides = new List<MutableTuple<PlayerMovementPhysicsValues, PlayerOverrideType>>();
    public List<MutableTuple<PlayerMovementPhysicsValues, PlayerOverrideType>> physicsOverrides { get { return _physicsOverrides; } private set { _physicsOverrides = value; } }

    [SerializeField]
    private List<MutableTuple<PlayerMovementActionValues, PlayerOverrideType>> _actionOverrides = new List<MutableTuple<PlayerMovementActionValues, PlayerOverrideType>>();
    public List<MutableTuple<PlayerMovementActionValues, PlayerOverrideType>> actionOverrides { get { return _actionOverrides; } private set { _actionOverrides = value; } }


    [SerializeField]
    private List<MutableTuple<AlestaMovementAbilityValues, PlayerOverrideType>> _alestaAbilityOverrides = new List<MutableTuple<AlestaMovementAbilityValues, PlayerOverrideType>>();
    public List<MutableTuple<AlestaMovementAbilityValues, PlayerOverrideType>> alestaAbilityOverrides { get { return _alestaAbilityOverrides; } private set { _alestaAbilityOverrides = value; } }

    [SerializeField]
    private List<MutableTuple<NephuiMovementAbilityValues, PlayerOverrideType>> _nephuiAbilityOverrides = new List<MutableTuple<NephuiMovementAbilityValues, PlayerOverrideType>>();
    public List<MutableTuple<NephuiMovementAbilityValues, PlayerOverrideType>> nephuiAbilityOverrides { get { return _nephuiAbilityOverrides; } private set { _nephuiAbilityOverrides = value; } }

    [SerializeField]
    private List<MutableTuple<CartiaMovementAbilityValues, PlayerOverrideType>> _cartiaAbilityOverrides = new List<MutableTuple<CartiaMovementAbilityValues, PlayerOverrideType>>();
    public List<MutableTuple<CartiaMovementAbilityValues, PlayerOverrideType>> cartiaAbilityOverrides { get { return _cartiaAbilityOverrides; } private set { _cartiaAbilityOverrides = value; } }

    [SerializeField]
    private List<MutableTuple<IlphineMovementAbilityValues, PlayerOverrideType>> _ilphineAbilityOverrides = new List<MutableTuple<IlphineMovementAbilityValues, PlayerOverrideType>>();
    public List<MutableTuple<IlphineMovementAbilityValues, PlayerOverrideType>> ilphineAbilityOverrides { get { return _ilphineAbilityOverrides; } private set { _ilphineAbilityOverrides = value; } }

}
