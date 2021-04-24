using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.Playables;

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

    protected Animator animator;

    public GameObject animationRoot;

    [SerializeField]
    protected GameObject modelRoot;
    protected Animator modelAnimator;

    private Coroutine iFrameCoroutine;
    [SerializeField]
    private float iFrameBlinkRate = 0.1f;


    [SerializeField]
    private float turnSpeed = 360;

    private Coroutine rotationCoroutine;

    [SerializeField]
    Quaternion initialFacingRotation;
    Vector3 initialAnimationRootPosition;
    Quaternion animationRootRotation = Quaternion.identity;
    public bool lockFacingDirection;

    Quaternion prevRotation;
    Vector3 prevPosition;

    #region Sibling References
    protected CharacterMovement movement;
    #endregion

    protected virtual void Awake()
    {
        movement = GetComponent<CharacterMovement>();

        animator = GetComponent<Animator>();
        modelAnimator = modelRoot.GetComponent<Animator>();
        initialAnimationRootPosition = animationRoot.transform.localPosition;
        //initialFacingRotation = animationRoot.transform.localRotation;
    }

    protected virtual void Start()
    {

    }

    //Vector3 rotateDampVelocity;
    //[SerializeField]
    //float orientDampTime;
    //[SerializeField]
    //float orientDampMaxSpeed;
    [SerializeField]
    float rotationDifferenceScale;
    [SerializeField]
    float planeChangePositionDifferenceScale;
    //[SerializeField]
    //float angleFactor;
    protected virtual void Update()
    {
        if (prevRotation != transform.rotation * initialFacingRotation * animationRootRotation)
        {
            //animationRoot.transform.rotation = prevRotation;
            Quaternion toRotateTo = transform.rotation * initialFacingRotation * animationRootRotation;

            float angle = Quaternion.Angle(prevRotation, toRotateTo);

            //orientDampTime = angle / rotationDifferenceScale;
            //orientDampMaxSpeed = angle * rotationDifferenceScale;

            animationRoot.transform.rotation = Quaternion.Slerp(prevRotation, toRotateTo, angle / 360 * rotationDifferenceScale);//Quaternion.Euler(Vector3.SmoothDamp(prevRotation.eulerAngles, toRotateTo.eulerAngles, ref rotateDampVelocity, orientDampTime, orientDampMaxSpeed));

        }
        else if (animationRoot.transform.localPosition != initialAnimationRootPosition)
        {
            animationRoot.transform.position = Vector3.Lerp(prevPosition, transform.position + initialAnimationRootPosition, (prevPosition - (transform.position + initialAnimationRootPosition)).magnitude * planeChangePositionDifferenceScale);
        }

        prevRotation = animationRoot.transform.rotation;
        prevPosition = animationRoot.transform.position;
    }

        /*s
        #if UNITY_EDITOR
            void OnDrawGizmosSelected()
            {
                if (!animator)
                    animator = GetComponent<Animator>();

                if (!Application.isPlaying && animator.deltaPosition != Vector3.zero && animator.deltaRotation != Quaternion.identity)
                {
                    Debug.Log(animationRoot.transform.rotation * animator.deltaPosition);
                    animationRoot.transform.position += animator.deltaPosition;
                    animationRoot.transform.rotation = transform.rotation * animationRoot.transform.rotation;
                    animator = GetComponent<Animator>();
                } 
            }
        #endif
        */
    void OnAnimatorMove()
    {
        if (animator.deltaPosition != Vector3.zero)
        {   
            //Debug.Log(animationRootRotation * animator.deltaPosition);
            movement.MoveBy(animationRootRotation * animator.deltaPosition);
        }
        if (animator.deltaRotation != Quaternion.identity)
        {
            movement.RotateBy(animationRootRotation * animator.deltaRotation);
        }
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

    public Quaternion GetFacingDirectionRotation()
    {
        return animationRootRotation;
    }

    public void SetFacingDirection(float sign)
    {
        if (lockFacingDirection)
            return;

        animationRootRotation = (sign > 0) ? Quaternion.identity : flipUp;
        animationRoot.transform.localRotation = initialFacingRotation * ((sign > 0) ? Quaternion.identity : flipUp);
    }
    
    public void ChangeFacingDirection(float speed)
    {
        if (lockFacingDirection)
            return;

        animationRoot.transform.rotation *= flipUp;
    }
    
    public void FaceTowards(Vector3 lookTo)
    {
        if (lockFacingDirection)
            return;

        RotateSmoothly(Quaternion.FromToRotation(animationRoot.transform.right, transform.worldToLocalMatrix * lookTo), turnSpeed);
    }
    
    public void FaceTowards(Vector3 lookTo, float speed)
    {
        if (lockFacingDirection)
            return;

        RotateSmoothly(Quaternion.FromToRotation(animationRoot.transform.right, transform.worldToLocalMatrix * lookTo), speed);
    }

    public void AnimateFlinch()
    {
        modelAnimator.SetTrigger(animatorParameterNameToID["Flinch"]);
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
