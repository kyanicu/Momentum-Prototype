using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAnimation : MonoBehaviour
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

    [SerializeField]
    protected GameObject root;
    [SerializeField]
    protected GameObject modelRoot;
    [SerializeField]
    protected Animator animator;

    private Coroutine iFrameCoroutine;
    [SerializeField]
    private float iFrameBlinkRate = 0.1f;


    [SerializeField]
    private float turnSpeed = 360;

    private Coroutine rotationCoroutine;

    [SerializeField]
    float facingYRotation;

    #region Sibling References
    protected CharacterMovement movement;
    protected CharacterCombat combat;
    #endregion

    protected virtual void Awake()
    {        
        //root = transform.GetChild(0).gameObject;
        //modelRoot = root.transform.GetChild(0).gameObject;
        //animator = root.GetComponent<Animator>();

        //facingRotation = root.transform.localRotation.eulerAngles.y;

       // if (gameObject.name == "Unity-Chan")
    }

    protected virtual void Start()
    {
        combat = GetComponent<CharacterCombat>();
        movement = GetComponent<CharacterMovement>();
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

    public void SetFacingDirection(float sign)
    {
        Vector3 euler = root.transform.localRotation.eulerAngles;
        root.transform.localRotation = Quaternion.Euler(euler.x, sign * facingYRotation, euler.z);
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

    public void AnimateAttack(string attackTriggerName)
    {
        animator.SetTrigger(animatorParameterNameToID[attackTriggerName]);
    }

    public void AttackStateTransition(AttackAnimationState newState)
    {
        combat.AttackAnimationStateTransition(newState);
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

}
