using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.Playables;

public enum AttackAnimationState { FINISHED, STARTUP, CONTACT, RECOVERY, BUFFER }

public struct CharacterCombatControl
{
    public string attackName { get; private set; }
    public bool attackSet { get; private set; }

    public void SetAttack(string name)
    {
        attackName = name;
        attackSet = true;
    }

    public void Reset()
    {
        attackName = "";
        attackSet = false;
    }
}


[System.Serializable]
public struct AttackInitInfo
{
    public string attackName;

    public TimelineAsset attackTimeline;
}

public class CharacterCombat : MonoBehaviour, IAttacker
{
    [SerializeField]
    protected List<AttackInitInfo> attackInitInfo = new List<AttackInitInfo>();

    private Dictionary<string, AttackInitInfo> attackInitMap = new Dictionary<string, AttackInitInfo>();

    public CharacterCombatControl control = new CharacterCombatControl();

    private IDamageable _damageable;
    public IDamageable damageable { get { return _damageable; } private set { _damageable = value; } }

    private Hitbox[] hitboxes;

    AttackAnimationState attackAnimationState = AttackAnimationState.FINISHED;

    private bool attackBuffered;

    new protected CharacterAnimation animation;
    protected CharacterMovement movement;
    protected CharacterMovementAction movementAction;
    protected CharacterValueOverridability overridability;

    protected virtual void Awake()
    {
        hitboxes = GetComponentsInChildren<Hitbox>();
        foreach (Hitbox hb in hitboxes)
        {
            hb.SetAttacker(this);
        }
        
        foreach (AttackInitInfo initInfo in attackInitInfo)
        {
            attackInitMap.Add(initInfo.attackName, initInfo);
        }
    }

    protected virtual void Start()
    {
        animation = GetComponent<CharacterAnimation>();
        movement = GetComponent<CharacterMovement>();
        movementAction = GetComponent<CharacterMovementAction>();
        overridability = GetComponent<CharacterValueOverridability>();

        damageable = GetComponent<IDamageable>();
    }

    protected virtual void Update()
    {
        if (control.attackSet && !attackBuffered &&
    (attackAnimationState == AttackAnimationState.FINISHED || (attackBuffered = (attackAnimationState == AttackAnimationState.BUFFER))))
        {
            HandleAttack(control.attackName);
            control.Reset();
        }
    }

    private void HandleAttack(string name)
    {
        //animation.AnimateAttack(name);
        GetComponent<PlayableDirector>().Play(attackInitMap[name].attackTimeline);
    }

    public void AttackAnimationStateTransition(AttackAnimationState newState)
    {
        attackAnimationState = newState;

        if (newState == AttackAnimationState.STARTUP)
        {
            attackBuffered = false;
        }
        else if (newState == AttackAnimationState.FINISHED)
        {

        }
    }

    public void Flinch()
    {
        if (attackAnimationState != AttackAnimationState.FINISHED)
            AttackAnimationStateTransition(AttackAnimationState.FINISHED);
        attackBuffered = false;
        foreach (Hitbox hb in hitboxes)
            hb.enabled = false;
    }

    public AttackerInfo GetAttackerInfo()
    {
        AttackerInfo info = new AttackerInfo();

        return info;
    }
}
