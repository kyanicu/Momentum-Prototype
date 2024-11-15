﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAIInternalPerceptionReactions
{
    void OnMovementHit(Collider hitCollider, Vector3 hitNormal);
    void OnLanding(Vector3 groundNormal);
    void OnUngrounding(Vector3 leftGroundNormal);
}

public interface IAIWorldPerceptionReactions
{

}

public interface IAIOpponentPerceptionReactions
{

}

public abstract class AICharacterDirector : CharacterDirector
{

    public CharacterMovement movement { get; private set; }
    public CharacterMovementActionControl movementActionControl { get; private set; }

    public CharacterCombat combat { get; private set; }
    public CharacterStatus status { get; private set; }
    public CharacterAnimation animation { get; private set; }

    //#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
    }
    //#endif

    protected virtual void Awake()
    {
        movement = GetComponent<CharacterMovement>();
        combat = GetComponent<CharacterCombat>();
        status = GetComponent<CharacterStatus>();
        animation = GetComponent<CharacterAnimation>();
    }

    protected virtual void Start()
    {
        movementActionControl = GetComponent<CharacterMovementAction>()?.control;
    }

    protected virtual void OnEnable()
    {
        EnableControl();
    }

    protected virtual void OnDisable()
    {
        DisableControl();
    }
}
