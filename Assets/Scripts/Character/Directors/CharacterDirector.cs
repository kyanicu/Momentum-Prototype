using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using KinematicCharacterController;

#region Communication Structs
/// <summary>
/// A wrapper for the Transform class that allows transform state to only be read
/// </summary>
public struct ReadOnlyTransform
{
    private Transform transform;

    public Vector3 position { get { return transform.position; } }
    public Quaternion rotation { get { return transform.rotation; } }
    public Vector3 localScale { get { return transform.localScale; } }
    public Vector3 lossyScale { get { return transform.lossyScale; } }
    public Matrix4x4 worldToLocalMatrix { get { return transform.worldToLocalMatrix; } }
    public Matrix4x4 localToWorldMatrix { get { return transform.localToWorldMatrix; } }

    public ReadOnlyTransform(Transform t)
    {  
        transform = t;
    }
}

/////// <summary>
/////// A wrapper for the KinematicCharacterMotor class that allows a constant reference state to only be read
/////// </summary>
////public struct ReadOnlyKinematicMotor
////{
////    private KinematicCharacterMotor motor;
////
////    public Vector3 position { get { return motor.TransientPosition; } }
////    public Quaternion rotation { get { return motor.TransientRotation; } }
////    public Vector3 velocity { get { return motor.BaseVelocity; } }
////    public Vector3 groundNormal { get { return motor.GetEffectiveGroundNormal(); } }
////    public Vector3 lastGroundNormal { get { return motor.GetLastEffectiveGroundNormal(); } }
////    public bool isGroundedThisUpdate { get { return motor.IsGroundedThisUpdate; } }
////    public bool wasGroundedLastUpdate { get { return motor.WasGroundedLastUpdate; } }
////
////    public ReadOnlyKinematicMotor(KinematicCharacterMotor m)
////    {  
////        motor = m;
////    }
////}   

#endregion

/// <summary>
/// Unity Component that controls all Character mechanics and scripting
/// Abstract for specific charactertype to derive from
/// </summary>
[RequireComponent(typeof(CharacterMovement))]
public abstract class CharacterDirector : MonoBehaviour 
{

    #region fields
    bool controlLocked = false;
    Coroutine inputLockTimer;
    #endregion

    protected CharacterMovement movement;

    protected virtual void Awake()
    {
        movement = GetComponent<CharacterMovement>();
    }

    public void EnableControl()
    {
        GameManager.Instance.RegisterCharacterControl(this);
    }

    public void DisableControl()
    {
        GameManager.Instance?.DeregisterCharacterControl(this);
    }

    public void TempLockControl(float time)
    {
        if(inputLockTimer != null)
            StopCoroutine(inputLockTimer);
        
        inputLockTimer = StartCoroutine(LockControlTimer(time));
    }

    private IEnumerator LockControlTimer(float time)
    {
        controlLocked = true;
        yield return new WaitForSeconds(time);
        controlLocked = false;
    }

    public void HandleControl()
    {
        if(!controlLocked)
            RegisterControl();
    }

    protected abstract void RegisterControl();

}