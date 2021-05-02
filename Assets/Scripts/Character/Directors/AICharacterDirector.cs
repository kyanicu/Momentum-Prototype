using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public interface IAIInternalPerceptionReactions
{
    void OnMovementHit(Collider hitCollider, Vector3 hitNormal);
    void OnLanding(Vector3 groundNormal);
    void OnUngrounding(Vector3 leftGroundNormal);
}

public interface IAIInternalWorldReactions
{

}

public interface IAIInternalEnemyReactions
{

}

public abstract class AICharacterDirector : CharacterDirector
{
    public virtual void OnMovementHit(Collider hitCollider, Vector3 hitNormal) { }

    public virtual void OnLanding(Vector3 groundNormal) { }

    public virtual void OnUngrounding(Vector3 leftGroundNormal) { }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
    }
#endif

}
