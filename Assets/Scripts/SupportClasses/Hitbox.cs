using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The type of applicable knockback
/// STATIC: no knockback
/// KINEMATIC: Knockback at a set speed and time/distance
/// DYNAMIC: Knockback with impulse based physics (momentum-based knockback)
/// DYNAMIC_WITH_TORQUE: Knockback with full ragdoll physics including rotation
/// </summary>
public enum KnockbackType { STATIC, KINEMATIC, DYNAMIC, DYNAMIC_WITH_TORQUE }

public struct StatusEffect
{

}

[System.Serializable]
public struct AttackInfo
{

    /// <summary>
    /// The base damage of the attack
    /// </summary>
    public float baseDamage;
    /// <summary>
    /// How often damage is inflicted in seconds, 0 for single hit
    /// </summary>
    public float continousDamageRate;

    /// <summary>
    /// Does the attack cancel/reset the attacked object state and action
    /// </summary>
    public bool flinch;
    /// <summary>
    /// Does the attack cancel the attacked objects momentum
    /// </summary>
    public bool halt;
    /// <summary>
    /// Does the attack force the attacked object to unground if applicable and grounded
    /// </summary>
    public bool forceUnground;
    /// <summary>
    /// Time the attack prevents the attacked object from performing an action
    /// </summary>
    public float stunTime;
    
    /// <summary>
    /// Does the attack activate the attacked objects iFrames
    /// </summary>
    public bool activateIFrames;
    /// <summary>
    /// The overriden iFrame time for the attacked object, 0 for no override
    /// </summary>
    public float iFrameTimeOverride;

    /// <summary>
    /// The type of knockback applied
    /// </summary>
    public KnockbackType knockbackType;
    /// <summary>
    /// The speed at which the attacked object is knocked back
    /// </summary>
    public float baseKnockbackSpeed;
    /// <summary>
    /// The direction at which the attacked object is knocked back
    /// </summary>
    public Vector3 baseKnockbackDirection;
    /// <summary>
    /// The time the attacked object is knocked back for kinematic knockback
    /// </summary>
    public float kinematicKnockbacktime;

    /// <summary>
    /// The type of recoil applied
    /// </summary>
    public KnockbackType recoilType;
    /// <summary>
    /// The speed at which the attacker is recoiled back
    /// </summary>
    public float baseRecoilSpeed;
    /// <summary>
    /// The direction at which the attacker is recoiled back
    /// </summary>
    public Vector3 baseRecoilDirection;
    /// <summary>
    /// The time the attacker is recoiled back for kinematic recoil
    /// </summary>
    public float kinematiRecoiltime;
    
    public float momentumDamageFactor;
    public float momentumMinSpeed;
    public float momentumFactorMaxDamage;

    public StatusEffect statusEffect;

    public AttackerInfo attackerInfo;
}

public struct AttackerInfo
{

}

public interface IAttacker
{
    void HandleOutgoingAttack(AttackInfo attackInfo);
    AttackerInfo GetAttackerInfo();
}

public class Hitbox : MonoBehaviour
{

    [SerializeField]
    private AttackInfo attackInfo;
    private IAttacker attacker;

    public void SetAttacker(IAttacker d)
    {
        attacker = d;
    }

    public void SetAttackInfo(AttackInfo _attackInfo)
    {
        attackInfo = _attackInfo;
    }

    public void ResetAttackInfo()
    {
        attackInfo = new AttackInfo();
    }

    void OnTriggerEnter(Collider col)
    {
        Hurtbox hurtbox = col.GetComponent<Hurtbox>();
        
        attacker?.HandleOutgoingAttack(attackInfo);
        hurtbox?.HandleIncommingAttack(attackInfo);
    }

}
