using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Communication interface for player animation
/// </summary>
public interface IPlayerAnimationCommunication
{

    /// <summary>
    /// Called when player changes facing direction
    /// </summary>
    void ChangeFacingDirection();

    void AnimateNeutralAttack();
    void AnimateDownAttack();
    void AnimateUpAttack();
    void AnimateRunningAttack();
    void AnimateBrakingAttack();
    void AnimateNeutralAerialAttack();
    void AnimateBackAerialAttack();
    void AnimateDownAerialAttack();
    void AnimateUpAerialAttack();

    void AnimateFlinch();

    void StartIFrames();
    void EndIFrames();

}

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

    #region Communications
    IPlayerMovementCommunication movementCommunication;
    IPlayerMovementActionCommunication movementActionCommunication;
    IPlayerCombatCommunication combatCommunication;
    #endregion

    void Awake()
    {
        root = transform.GetChild(0).gameObject;
        modelRoot = root.transform.GetChild(0).gameObject;
        rootAnimator = root.GetComponent<Animator>();
        animationEvents = root.GetComponent<PlayerAnimationEvents>();

        animationEvents.attackStateTransition += AttackStateTransition;
    }

    void Start()
    {
        combatCommunication = GetComponent<IPlayerCombatCommunication>();
        movementCommunication = GetComponent<IPlayerMovementCommunication>();
        movementActionCommunication = GetComponent<IPlayerMovementActionCommunication>();
    }

    public void AttackStateTransition(AttackAnimationState newState)
    {
       combatCommunication.AttackAnimationStateTransition(newState);
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
        rootAnimator.SetBool(animatorParameterNameToID["Braking"], movementActionCommunication.isBraking && movementCommunication.isGroundedThisUpdate);
        rootAnimator.SetBool(animatorParameterNameToID["Falling"], !movementCommunication.isGroundedThisUpdate);

        rootAnimator.SetFloat(animatorParameterNameToID["RunSpeed"], (movementCommunication.isGroundedThisUpdate) ? movementCommunication.velocity.magnitude : 0);
    }
}
