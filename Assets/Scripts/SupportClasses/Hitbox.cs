using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The type of applicable knockback
/// STATIC: No knockback
/// KINEMATIC: Knockback at a set speed and time/distance
/// DYNAMIC: Knockback with impulse based physics (momentum-based knockback)
/// DYNAMIC_WITH_TORQUE: Knockback with full ragdoll physics including rotation
/// </summary>
public enum KnockbackType { STATIC, KINEMATIC, DYNAMIC, DYNAMIC_WITH_TORQUE }

/// <summary>
/// The way the direction of Knockback is calculated
/// GLOBAL: Directly uses the given direction
/// LOCAL_HITBOX: Uses the given direction, oriented to the local of the hitbox
/// LOCAL_HURTBOX: Uses the given direction, oriented to the local of the hurtbox
/// RADIAL: Uses the given direction, oriented to the direction of the hitbox center to the hurtbox center
/// </summary>
public enum KnockbackDirectionCalculation { GLOBAL, LOCAL_HITBOX, LOCAL_HURTBOX, RADIAL }

public struct StatusEffect
{

}

[System.Serializable]
public struct AttackInfo
{

    public static readonly AttackInfo normalAttack = new AttackInfo
    {
        baseDamage = 5,
        continousDamageRate = 0,
        flinch = true,
        halt = true,
        forceUnground = true,
        stunTime = 0.25f,
        activateIFrames = true,
        iFrameTimeOverride = 0,
        allowHitAfterIFrames = false,

        knockbackType = KnockbackType.KINEMATIC,
        baseKnockbackSpeed = 5,
        baseKnockbackDirection = Vector3.right,
        knockbackDirectionCalculation = KnockbackDirectionCalculation.LOCAL_HITBOX,
        kinematicKnockbackTime = 0.25f,
    };

    public static readonly AttackInfo chipDamage = new AttackInfo
    {
        baseDamage = 0.5f,
        continousDamageRate = 0.1f,
        flinch = false,
        halt = false,
        forceUnground = false,
        stunTime = 0,

        activateIFrames = false,
        iFrameTimeOverride = 0,
        allowHitAfterIFrames = false,

        knockbackType = KnockbackType.STATIC,
        baseKnockbackSpeed = 0,
        baseKnockbackDirection = Vector3.zero,
        knockbackDirectionCalculation = KnockbackDirectionCalculation.GLOBAL,
        kinematicKnockbackTime = 0,
    };

    public static readonly AttackInfo harmfulObject = new AttackInfo
    {
        baseDamage = 5f,
        continousDamageRate = 0,
        flinch = true,
        halt = false,
        forceUnground = true,
        stunTime = 0.25f,

        activateIFrames = false,
        iFrameTimeOverride = 0,
        allowHitAfterIFrames = true,

        knockbackType = KnockbackType.STATIC,
        baseKnockbackSpeed = 0,
        baseKnockbackDirection = Vector3.zero,
        knockbackDirectionCalculation = KnockbackDirectionCalculation.LOCAL_HURTBOX,
        kinematicKnockbackTime = 0,
    };

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
    /// Should the attacked object take another hit after iFrames? Only used if continousDamageRate is 0
    /// </summary>
    public bool allowHitAfterIFrames;

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
    /// The way the knockback direction is calculated
    /// </summary>
    public KnockbackDirectionCalculation knockbackDirectionCalculation;
    /// <summary>
    /// The time the attacked object is knocked back for kinematic knockback
    /// </summary>
    public float kinematicKnockbackTime;
}

public struct AttackerInfo
{
    
}

public interface IAttacker
{
    AttackerInfo GetAttackerInfo();

    IDamageable damageable { get; }
}

public class Hitbox : MonoBehaviour
{

    public static Dictionary<(string, string), bool> hitboxHurtboxCollision = new Dictionary<(string, string), bool>
    {
        [("PlayerHitbox", "PlayerHurtbox")] = false,
        [("PlayerHitbox", "EnemyHurtbox")] = true,
        [("PlayerHitbox", "EnemyImmuneHurtbox")] = true,

        [("EnemyHitbox", "EnemyHurtbox")] = false,
        [("EnemyHitbox", "PlayerHurtbox")] = true,
        [("EnemyHitbox", "EnemyImmuneHurtbox")] = false,

        [("FriendlyFireEnemyHitbox", "EnemyHurtbox")] = true,
        [("FriendlyFireEnemyHitbox", "PlayerHurtbox")] = true,
        [("FriendlyFireEnemyHitbox", "EnemyImmuneHurtbox")] = false,

        [("Untagged", "Untagged")] = true,

    };

    public AttackInfo attackInfo;
    public bool hasRecoilEffect = false;
    [SerializeField]
    private AttackInfo recoilInfo;
    private IAttacker _attacker;
    public IAttacker attacker { get { return _attacker; } private set { _attacker = value; } }

    private HitboxManager manager;

    /// <summary>
    /// Are multiple entries allowed as extra hits, if not, new hits are only allowed after hitbox is disabled (end of attack)
    /// </summary>
    [SerializeField]
    private bool deregisterOnExit = false;
    /// <summary>
    /// Group the hitbox belongs to, used to allow different hitboxes to register their own separate hits
    /// Use only one group for all hitboxes to only allow one overlap at a time
    /// Use multiple to allow separate hitboxes to be open at the same time with separate registrations
    /// </summary>
    [SerializeField]
    private int _hitboxGroup = 0;
    public int hitboxGroup { get { return _hitboxGroup; } private set { _hitboxGroup = value; } }

    public void Awake()
    {
        manager = transform.parent.GetComponent<HitboxManager>();
        if (manager == null)
            manager = gameObject.AddComponent<HitboxManager>();
    }

    public void SetAttacker(IAttacker d)
    {
        attacker = d;
    }

    public void SetAttackInfo(AttackInfo _attackInfo)
    {
        attackInfo = _attackInfo;
    }

    public void HandleOutgoingAttack(Hurtbox hurtbox)
    {
        if(hasRecoilEffect && attacker != null && attacker.damageable != null)
        {
            HandleAttackInfo(recoilInfo, attacker.damageable, this.transform, hurtbox.transform);
        }
    }

    public static void HandleAttackInfo(AttackInfo info, IDamageable damageable, Transform hit, Transform hurt, bool recoil = false)
    {
        if (info.baseDamage != 0)
            damageable.TakeDamage(info.baseDamage);

        if (info.activateIFrames)
            damageable.ActivateIFrames(info.iFrameTimeOverride);

        if (info.flinch)
            damageable.Flinch();
        if (info.halt)
            damageable.Halt();
        if (info.forceUnground)
            damageable.ForceUnground();
        if (info.stunTime != 0)
            damageable.Stun(info.stunTime);

        if (info.knockbackType != KnockbackType.STATIC)
        {
            Vector3 calculatedKnockback = info.baseKnockbackDirection * info.baseKnockbackSpeed;
            switch (info.knockbackDirectionCalculation)
            {
                case (KnockbackDirectionCalculation.LOCAL_HITBOX):
                    calculatedKnockback = hit.TransformDirection(calculatedKnockback);
                    break;
                case (KnockbackDirectionCalculation.LOCAL_HURTBOX):
                    calculatedKnockback = hurt.TransformDirection(calculatedKnockback);
                    break;
                case (KnockbackDirectionCalculation.RADIAL):
                    calculatedKnockback = Quaternion.FromToRotation(Vector3.right, (hurt.position - hit.transform.position).normalized) * calculatedKnockback;
                    break;
            }

            switch (info.knockbackType)
            {
                case (KnockbackType.KINEMATIC):
                    damageable.TakeKinematicKnockback(calculatedKnockback, info.kinematicKnockbackTime);
                    break;
                case (KnockbackType.DYNAMIC):
                    damageable.TakeDynamicKnockback(calculatedKnockback);
                    break;
                case (KnockbackType.DYNAMIC_WITH_TORQUE):
                    damageable.TakeDynamicKnockbackWithTorque(calculatedKnockback, hurt.position);
                    break;
            }
        }
    }

    void OnTriggerEnter(Collider col)
    {
        if (hitboxHurtboxCollision[(this.tag, col.tag)])
        {
            Hurtbox hurtbox = col.GetComponent<Hurtbox>();
            if (hurtbox != null)
            {
                manager.RegisterOverlap(this, hurtbox);
            }
        }
    }
    void OnTriggerExit(Collider col)
    {
        if (deregisterOnExit && hitboxHurtboxCollision[(this.tag, col.tag)])
        {
            Hurtbox hurtbox = col.GetComponent<Hurtbox>();
            if (hurtbox != null)
            {
                manager.DeregisterOverlap(this, hurtbox);
            }
        }
    }

    void OnDisable()
    {
        if (!deregisterOnExit)
            manager.DeregisterAllOverlaps(this);
        attackInfo = new AttackInfo();
        recoilInfo = new AttackInfo();
        hasRecoilEffect = false;
    }
}

