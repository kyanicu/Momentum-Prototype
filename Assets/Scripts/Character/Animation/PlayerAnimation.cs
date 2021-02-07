using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimation : CharacterAnimation
{
    private Coroutine iFrameCoroutine;
    [SerializeField]
    private float iFrameBlinkRate = 0.1f;
    
    #region Sibling References
    PlayerMovement movement;
    PlayerCombat combat;
    PlayerMovementAction movementAction;
    #endregion

    void Start()
    {
        combat = GetComponent<PlayerCombat>();
        movement = GetComponent<PlayerMovement>();
        movementAction = GetComponent<PlayerMovementAction>();
    }

    public void ChangeCharacter(GameObject newRoot)
    {
        newRoot.transform.rotation = root.transform.rotation;
        
        root.SetActive(false);
        newRoot.SetActive(true);

        root = newRoot;
        modelRoot = root.transform.GetChild(0).gameObject;
        animator = root.GetComponent<Animator>();
    }

    public void AttackStateTransition(AttackAnimationState newState)
    {
       combat.AttackAnimationStateTransition(newState);
    }

    public void AnimateNeutralAttack()
    {
        animator.SetTrigger(animatorParameterNameToID["NeutralAttack"]);
    }
    public void AnimateDownAttack()
    {
        animator.SetTrigger(animatorParameterNameToID["DownAttack"]);
    }
    public void AnimateUpAttack()
    {
        animator.SetTrigger(animatorParameterNameToID["UpAttack"]);
    }
    public void AnimateRunningAttack()
    {
        animator.SetTrigger(animatorParameterNameToID["RunningAttack"]);
    }
    public void AnimateBrakingAttack()
    {
        animator.SetTrigger(animatorParameterNameToID["BrakingAttack"]);
    }
    public void AnimateNeutralAerialAttack()
    {
        animator.SetTrigger(animatorParameterNameToID["NeutralAerialAttack"]);
    }
    public void AnimateBackAerialAttack()
    {
        animator.SetTrigger(animatorParameterNameToID["BackAerialAttack"]);
    }
    public void AnimateDownAerialAttack()
    {
        animator.SetTrigger(animatorParameterNameToID["DownAerialAttack"]);
    }
    public void AnimateUpAerialAttack()
    {
        animator.SetTrigger(animatorParameterNameToID["UpAerialAttack"]);
    }

    public void AnimateFlinch()
    {
        animator.SetTrigger(animatorParameterNameToID["Flinch"]);
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
        animator.SetBool(animatorParameterNameToID["Braking"], movementAction.isBraking && movement.isGroundedThisUpdate);
        animator.SetBool(animatorParameterNameToID["Falling"], !movement.isGroundedThisUpdate);

        animator.SetFloat(animatorParameterNameToID["RunSpeed"], (movement.isGroundedThisUpdate) ? movement.velocity.magnitude : 0);
    }
}
