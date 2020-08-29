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

public class MovementEffector : MonoBehaviour
{

    [SerializeField]
    private List<MutableTuple<PlayerMovementValues, PlayerMovementOverrideType>> _movementOverrides = new List<MutableTuple<PlayerMovementValues, PlayerMovementOverrideType>>();
    public List<MutableTuple<PlayerMovementValues, PlayerMovementOverrideType>> movementOverrides { get { return _movementOverrides; } private set { _movementOverrides = value; } }

    [SerializeField]
    private List<MutableTuple<PlayerMovementPhysicsValues, PlayerMovementOverrideType>> _physicsOverrides = new List<MutableTuple<PlayerMovementPhysicsValues, PlayerMovementOverrideType>>();
    public List<MutableTuple<PlayerMovementPhysicsValues, PlayerMovementOverrideType>> physicsOverrides { get { return _physicsOverrides; } private set { _physicsOverrides = value; } }

    [SerializeField]
    private List<MutableTuple<PlayerMovementActionValues, PlayerMovementOverrideType>> _actionOverrides = new List<MutableTuple<PlayerMovementActionValues, PlayerMovementOverrideType>>();
    public List<MutableTuple<PlayerMovementActionValues, PlayerMovementOverrideType>> actionOverrides { get { return _actionOverrides; } private set { _actionOverrides = value; } }


    [SerializeField]
    private List<MutableTuple<AlestaMovementAbilityValues, PlayerMovementOverrideType>> _alestaAbilityOverrides = new List<MutableTuple<AlestaMovementAbilityValues, PlayerMovementOverrideType>>();
    public List<MutableTuple<AlestaMovementAbilityValues, PlayerMovementOverrideType>> alestaAbilityOverrides { get { return _alestaAbilityOverrides; } private set { _alestaAbilityOverrides = value; } }

    [SerializeField]
    private List<MutableTuple<NephuiMovementAbilityValues, PlayerMovementOverrideType>> _nephuiAbilityOverrides = new List<MutableTuple<NephuiMovementAbilityValues, PlayerMovementOverrideType>>();
    public List<MutableTuple<NephuiMovementAbilityValues, PlayerMovementOverrideType>> nephuiAbilityOverrides { get { return _nephuiAbilityOverrides; } private set { _nephuiAbilityOverrides = value; } }

    [SerializeField]
    private List<MutableTuple<DeletheiMovementAbilityValues, PlayerMovementOverrideType>> _deletheiAbilityOverrides = new List<MutableTuple<DeletheiMovementAbilityValues, PlayerMovementOverrideType>>();
    public List<MutableTuple<DeletheiMovementAbilityValues, PlayerMovementOverrideType>> deletheiAbilityOverrides { get { return _deletheiAbilityOverrides; } private set { _deletheiAbilityOverrides = value; } }

    [SerializeField]
    private List<MutableTuple<IlphineMovementAbilityValues, PlayerMovementOverrideType>> _ilphineAbilityOverrides = new List<MutableTuple<IlphineMovementAbilityValues, PlayerMovementOverrideType>>();
    public List<MutableTuple<IlphineMovementAbilityValues, PlayerMovementOverrideType>> ilphineAbilityOverrides { get { return _ilphineAbilityOverrides; } private set { _ilphineAbilityOverrides = value; } }

}
