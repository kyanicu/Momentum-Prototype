using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LuposAIDirector : AICharacterDirector
{
    protected override void Awake()
    {
        base.Awake();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
    }
#endif

    protected override void RegisterControl()
    {
        combat.control.SetAttack("LuposRawr");
    }
}
