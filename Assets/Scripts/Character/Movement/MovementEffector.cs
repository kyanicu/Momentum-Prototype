using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementEffector : MonoBehaviour
{

    [SerializeField]
    FullMovementOverride overrides;

    void OnTriggerEnter(Collider col)
    {
        col.GetComponent<ICharacterValueOverridabilityCommunication>()?.ApplyFullMovementOverride(overrides);
    }

    void OnTriggerExit(Collider col)
    {
        col.GetComponent<ICharacterValueOverridabilityCommunication>()?.RemoveFullMovementOverride(overrides);
    }
}