using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerAnimation : IPlayerAnimationCommunication
{
    Dictionary<string, int> animatorParameterNameToID = new Dictionary<string, int>()
    {
        ["NeutralAttack"] = Animator.StringToHash("NeutralAttack"),
        ["DownAttack"] = Animator.StringToHash("DownAttack"),
        ["UpAttack"] = Animator.StringToHash("UpAttack"),
        ["RunningAttack"] = Animator.StringToHash("RunningAttack"),
        ["BrakingAttack"] = Animator.StringToHash("BrakingAttack"),
        ["NeutralAerialAttack"] = Animator.StringToHash("NeutralAerialAttack"),
        ["BackAerialAttack"] = Animator.StringToHash("BackAerialAttack"),
        ["DownAerialAttack"] = Animator.StringToHash("DownAerialAttack"),
        ["UpAerialAttack"] = Animator.StringToHash("UpAerialAttack"),
        ["Braking"] = Animator.StringToHash("Braking"),
        ["RunSpeed"] = Animator.StringToHash("RunSpeed"),
        ["Falling"] = Animator.StringToHash("Falling")
    };

    [SerializeField, HideInInspector]
    private GameObject modelRoot;

    [SerializeField, HideInInspector]
    private Animator rootAnimator;
    
    ////[SerializeField, HideInInspector]
    ////private Animator modelAnimator;

    private PlayerAnimationEvents animationEvents;
    public event Action<AttackAnimationState> attackStateTransition;

    private ReadOnlyPlayerMovementAction playerActions;
    private ReadOnlyKinematicMotor playerMotor;

    private Vector3 positionInterpDampVel = Vector3.zero;
    [SerializeField]
    private float positionInterpDampTime;
    [SerializeField]
    private float positionInterpDampMaxSpeed;

    private Vector3 rotationInterpDampVel = Vector3.zero;
    [SerializeField]
    private float rotationInterpDampTime;
    [SerializeField]
    private float rotationInterpDampMaxSpeed;

    Vector3 prevParentPosition = Vector3.zero;
    Quaternion prevParentRotation = Quaternion.identity;

    static private readonly Quaternion flipUp = Quaternion.Euler(0,180,0);

    public PlayerAnimation(GameObject _modelRoot)
    {
        modelRoot = _modelRoot;
        rootAnimator = modelRoot.GetComponent<Animator>();
        ////modelAnimator = modelRoot.GetComponentInChildren<Animator>();
        animationEvents = modelRoot.GetComponent<PlayerAnimationEvents>();

        animationEvents.attackStateTransition += AttackStateTransition;
    }

    public void AttackStateTransition(AttackAnimationState newState)
    {
        attackStateTransition?.Invoke(newState);
    }

    public void SetCommunicationInterface(PlayerInternalCommunicator communicator)
    {
        communicator.SetCommunication(this);
    }

    public void SetReadOnlyReferences(ReadOnlyKinematicMotor motor, ReadOnlyPlayerMovementAction actions)
    {
        playerActions = actions;
        playerMotor = motor;
    }

    public void ChangeFacingDirection()
    {
        modelRoot.transform.localRotation *= flipUp;
    }

    public void AnimateNeutralAttack()
    {
        rootAnimator.SetTrigger(animatorParameterNameToID["NeutralAttack"]);
        ////modelAnimator.SetTrigger(animatorParameterNameToID["NeutralAttack"]);
    }
    public void AnimateDownAttack()
    {
        rootAnimator.SetTrigger(animatorParameterNameToID["DownAttack"]);
        ////modelAnimator.SetTrigger(animatorParameterNameToID["DownAttack"]);
    }
    public void AnimateUpAttack()
    {
        rootAnimator.SetTrigger(animatorParameterNameToID["UpAttack"]);
        ////modelAnimator.SetTrigger(animatorParameterNameToID["UpAttack"]);
    }
    public void AnimateRunningAttack()
    {
        rootAnimator.SetTrigger(animatorParameterNameToID["RunningAttack"]);
        ////modelAnimator.SetTrigger(animatorParameterNameToID["RunningAttack"]);
    }
    public void AnimateBrakingAttack()
    {
        rootAnimator.SetTrigger(animatorParameterNameToID["BrakingAttack"]);
        ////modelAnimator.SetTrigger(animatorParameterNameToID["BrakingAttack"]);
    }
    public void AnimateNeutralAerialAttack()
    {
        rootAnimator.SetTrigger(animatorParameterNameToID["NeutralAerialAttack"]);
        ////modelAnimator.SetTrigger(animatorParameterNameToID["NeutralAerialAttack"]);
    }
    public void AnimateBackAerialAttack()
    {
        rootAnimator.SetTrigger(animatorParameterNameToID["BackAerialAttack"]);
        ////modelAnimator.SetTrigger(animatorParameterNameToID["BackAerialAttack"]);
    }
    public void AnimateDownAerialAttack()
    {
        rootAnimator.SetTrigger(animatorParameterNameToID["DownAerialAttack"]);
        ////modelAnimator.SetTrigger(animatorParameterNameToID["DownAerialAttack"]);
    }
    public void AnimateUpAerialAttack()
    {
        rootAnimator.SetTrigger(animatorParameterNameToID["UpAerialAttack"]);
        ////modelAnimator.SetTrigger(animatorParameterNameToID["UpAerialAttack"]);
    }

    public void FrameUpdate()
    {
        rootAnimator.SetBool(animatorParameterNameToID["Braking"], playerActions.isBraking && playerMotor.isGroundedThisUpdate);
        rootAnimator.SetBool(animatorParameterNameToID["Falling"], !playerMotor.isGroundedThisUpdate);

        rootAnimator.SetFloat(animatorParameterNameToID["RunSpeed"], (playerMotor.isGroundedThisUpdate) ? playerMotor.velocity.magnitude : 0);
    }

}
