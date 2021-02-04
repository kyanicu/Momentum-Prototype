using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour, IPlayerAnimationCommunication
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
        ["Falling"] = Animator.StringToHash("Falling"),
        ["Flinch"] = Animator.StringToHash("Flinch")
    };

    [SerializeField, HideInInspector]
    private GameObject root;
    [SerializeField, HideInInspector]
    private GameObject modelRoot;

    [SerializeField, HideInInspector]
    private Animator rootAnimator;

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

    private Coroutine iFrameCoroutine;
    [SerializeField]
    private float iFrameBlinkRate = 0.1f;

    void Awake()
    {
        root = transform.GetChild(0).gameObject;
        modelRoot = root.transform.GetChild(0).gameObject;
        rootAnimator = root.GetComponent<Animator>();
        animationEvents = root.GetComponent<PlayerAnimationEvents>();

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
        root.transform.localRotation *= flipUp;
    }

    public void AnimateNeutralAttack()
    {
        rootAnimator.SetTrigger(animatorParameterNameToID["NeutralAttack"]);
    }
    public void AnimateDownAttack()
    {
        rootAnimator.SetTrigger(animatorParameterNameToID["DownAttack"]);
    }
    public void AnimateUpAttack()
    {
        rootAnimator.SetTrigger(animatorParameterNameToID["UpAttack"]);
    }
    public void AnimateRunningAttack()
    {
        rootAnimator.SetTrigger(animatorParameterNameToID["RunningAttack"]);
    }
    public void AnimateBrakingAttack()
    {
        rootAnimator.SetTrigger(animatorParameterNameToID["BrakingAttack"]);
    }
    public void AnimateNeutralAerialAttack()
    {
        rootAnimator.SetTrigger(animatorParameterNameToID["NeutralAerialAttack"]);
    }
    public void AnimateBackAerialAttack()
    {
        rootAnimator.SetTrigger(animatorParameterNameToID["BackAerialAttack"]);
    }
    public void AnimateDownAerialAttack()
    {
        rootAnimator.SetTrigger(animatorParameterNameToID["DownAerialAttack"]);
    }
    public void AnimateUpAerialAttack()
    {
        rootAnimator.SetTrigger(animatorParameterNameToID["UpAerialAttack"]);
    }

    public void AnimateFlinch()
    {
        rootAnimator.SetTrigger(animatorParameterNameToID["Flinch"]);
    }

    public void StartIFrames()
    {
        if (iFrameCoroutine != null)
            GameManager.Instance.StopCoroutine(iFrameCoroutine);
        iFrameCoroutine = GameManager.Instance.StartCoroutine(IFrames());
        // modelRenderer.material.color = Color.red - (Color.clear/2);
    }

    public void EndIFrames()
    {
        if (iFrameCoroutine != null)
            GameManager.Instance.StopCoroutine(iFrameCoroutine);
        
        modelRoot.SetActive(true);
        // modelRenderer.material.color = Color.white
    }

    private IEnumerator IFrames()
    {
        while (true)
        {
            modelRoot.SetActive(!modelRoot.activeSelf);
            yield return new WaitForSeconds(iFrameBlinkRate);
        }
    }

    void Update()
    {
        rootAnimator.SetBool(animatorParameterNameToID["Braking"], playerActions.isBraking && playerMotor.isGroundedThisUpdate);
        rootAnimator.SetBool(animatorParameterNameToID["Falling"], !playerMotor.isGroundedThisUpdate);

        rootAnimator.SetFloat(animatorParameterNameToID["RunSpeed"], (playerMotor.isGroundedThisUpdate) ? playerMotor.velocity.magnitude : 0);
    }
}
