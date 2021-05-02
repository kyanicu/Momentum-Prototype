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

#if UNITY_EDITOR
    public enum InspectorMode { All, Director, Movement, Animation, Combat }

    [SerializeField]
    private InspectorMode inspectorMode;

    protected virtual void OnValidate()
    {
        Component[] components = GetComponents<Component>();
        
        if(inspectorMode != InspectorMode.All)
        {
            foreach (Component component in components)
            {
                if (component == this)
                    continue;

                component.hideFlags = HideFlags.HideInInspector;
            }
        }
        else
        {
            foreach (Component component in components)
            {
                if (component == this)
                    continue;

                component.hideFlags = HideFlags.None;
                component.SendMessage("OnValidate");
            }
            return;
        }

        List<Component> shownComponents = new List<Component>();
        switch (inspectorMode)
        {
            case InspectorMode.Director:
                shownComponents.Add(GetComponent<Bolt.Variables>());
                break;
            case InspectorMode.Movement:
                shownComponents.Add(GetComponent<CharacterMovement>());
                shownComponents.Add(GetComponent<CharacterMovementAction>());
                shownComponents.Add(GetComponent<CharacterMovementPhysics>());
                shownComponents.Add(GetComponent<DynamicPlaneConstraint>());
                shownComponents.Add(GetComponent<CharacterValueOverridability>());
                shownComponents.Add(GetComponent<PlayerMovementAbility>());
                break;
            case InspectorMode.Animation:
                shownComponents.Add(GetComponent<CharacterAnimation>());
                shownComponents.Add(GetComponent<UnityEngine.Playables.PlayableDirector>());
                shownComponents.Add(GetComponent<Animator>());
                shownComponents.Add(GetComponent<AudioSource>());
                break;
            case InspectorMode.Combat:
                shownComponents.Add(GetComponent<CharacterCombat>());
                shownComponents.Add(GetComponent<CharacterStatus>());
                break;
        }

        foreach (Component component in shownComponents)
        {
            if (component == null)
                continue;

            component.hideFlags = HideFlags.None;
            component.SendMessage("OnValidate");
        }
    }
#endif

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