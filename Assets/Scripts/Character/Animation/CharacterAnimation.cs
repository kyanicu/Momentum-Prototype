using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CharacterAnimation : MonoBehaviour
{
    static protected Dictionary<string, int> animatorParameterNameToID = new Dictionary<string, int>()
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
        ["Flinch"] = Animator.StringToHash("Flinch"),

        ["Walking"] = Animator.StringToHash("Walking"),
        ["Talking"] = Animator.StringToHash("Talking"),

    };

    [SerializeField, HideInInspector]
    protected GameObject root;
    [SerializeField, HideInInspector]
    protected GameObject modelRoot;

    [SerializeField, HideInInspector]
    protected Animator animator;

    [SerializeField]
    private float turnSpeed = 360;

    private Coroutine rotationCoroutine;

    protected virtual void Awake()
    {
        root = transform.GetChild(0).gameObject;
        modelRoot = root.transform.GetChild(0).gameObject;
        animator = root.GetComponent<Animator>();
    }

    private IEnumerator RotateSmoothlyCoroutine(Quaternion rotateBy, float speed)
    {
        Quaternion endRotation = transform.rotation * rotateBy;
        float time = Quaternion.Angle(transform.rotation, endRotation) / speed;
        Quaternion increment = Quaternion.Lerp(transform.rotation, endRotation, Time.deltaTime);
        for (float t = 0; t < time; t += Time.deltaTime)
         {
            transform.rotation *= increment;
            yield return new WaitForEndOfFrame();
        }
    }

    public void RotateSmoothly(Quaternion rotateBy, float speed)
    {
        if(rotationCoroutine != null)
            StopCoroutine(rotationCoroutine);

        rotationCoroutine = StartCoroutine(RotateSmoothlyCoroutine(rotateBy, speed));

    }

    static private readonly Quaternion flipUp = Quaternion.Euler(0,180,0); 

    public void ChangeFacingDirection()
    {
        root.transform.rotation *= flipUp;
    }
    
    public void ChangeFacingDirection(float speed)
    {
        root.transform.rotation *= flipUp;
    }
    
    public void FaceTowards(Vector3 lookTo)
    {
        RotateSmoothly(Quaternion.FromToRotation(root.transform.right, transform.worldToLocalMatrix * lookTo), turnSpeed);
    }
    
    public void FaceTowards(Vector3 lookTo, float speed)
    {
        RotateSmoothly(Quaternion.FromToRotation(root.transform.right, transform.worldToLocalMatrix * lookTo), speed);
    }

}
