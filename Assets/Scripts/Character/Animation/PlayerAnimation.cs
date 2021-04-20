using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimation : CharacterAnimation
{

    #region Sibling References
    MomentumMovementAction movementAction;
    #endregion

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();
        movementAction = GetComponent<MomentumMovementAction>();
    }

    ////public void ChangeCharacter(GameObject newRoot)
    ////{
    ////    newRoot.transform.rotation = root.transform.rotation;
    ////    
    ////    root.SetActive(false);
    ////    newRoot.SetActive(true);

    ////    root = newRoot;
    ////    modelRoot = root.transform.GetChild(0).gameObject;
    ////    animator = root.GetComponent<Animator>();
    ////}

    void Update()
    {
        animator.SetBool(animatorParameterNameToID["Braking"], movementAction.isBraking && movement.isGroundedThisUpdate);
        animator.SetBool(animatorParameterNameToID["Falling"], !movement.isGroundedThisUpdate);

        animator.SetFloat(animatorParameterNameToID["RunSpeed"], (movement.isGroundedThisUpdate) ? movement.velocity.magnitude : 0);
    }
}
