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
        movementAction = GetComponent<MomentumMovementAction>();
    }

    protected override void Start()
    {
        base.Start();
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

    /*
    protected override void FixedUpdate()
    {
        base.FixedUpdate();
    }
    */

    protected override void Update()
    {
        base.Update();

        modelAnimator.SetBool(animatorParameterNameToID["Braking"], movementAction.isBraking && movement.isGroundedThisUpdate);
        modelAnimator.SetBool(animatorParameterNameToID["Falling"], !movement.isGroundedThisUpdate);

        modelAnimator.SetFloat(animatorParameterNameToID["RunSpeed"], (movement.isGroundedThisUpdate) ? movement.velocity.magnitude : 0);
    }
}
