using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.Playables;

public class TimelineTransition
{
    public TimelineState toState;

    public System.Func<bool> condition;

    public TimelineTransition(TimelineState to, System.Func<bool> cond)
    {
        toState = to;
        condition = cond;
    }
}

[System.Serializable]
public struct TimelineStatePlayable
{
    public TimelineAsset timelinePlayable;

    public AnimationClip modelDefaultAnimation;

    public TimelineStatePlayable(TimelineAsset playable, AnimationClip modelDefaultAnim)
    {
        timelinePlayable = playable;
        modelDefaultAnimation = modelDefaultAnim;
    }
}

public class TimelineState
{
    public List<TimelineTransition> transitions = new List<TimelineTransition>();

    public TimelineStatePlayable statePlayable;

    public System.Action OnStateEnter;

    public System.Action OnStateUpdate;

    public System.Action OnStateExit;

    public System.Action OnStatePause;

    public TimelineState(TimelineStatePlayable playable)
    {
        statePlayable = playable;
    }
}

public class TimelineStateMachine
{

    public bool oneOffPlaying;
    public bool oneOffInterrubtable;

    public bool isStopped;

    public TimelineState currentState;

    public TimelineState idleState;

    public List<TimelineTransition> anyStateTransitions = new List<TimelineTransition>();

    public PlayableDirector playableDirector;
    public Animator modelAnimator;

    public TimelineStateMachine(PlayableDirector playDir, TimelineState initialState, Animator modelAnim)
    {
        playableDirector = playDir;
        idleState = initialState;
        modelAnimator = modelAnim;
        EnterState(idleState);
    }

    public void Restart()
    {
        Stop();
        Play();
    }

    public void Stop()
    {
        currentState.OnStateExit?.Invoke();
        currentState = idleState;
        oneOffPlaying = true;
        isStopped = true;
    }

    public void Pause()
    {
        oneOffPlaying = true;
        currentState.OnStatePause?.Invoke();
    }

    public void FinishInterruptedPlayable(PlayableDirector obj)
    {
        if(!isStopped)
            Play();
    }

    public void Play()
    {
        oneOffPlaying = false;
        oneOffInterrubtable = false;
        if (isStopped)
        {
            EnterState(idleState);
        }
        else
        {
            ContinueState();
        }
    }

    public void TransitionTo(TimelineState toState)
    {
        currentState.OnStateExit?.Invoke();
        EnterState(toState);
    }

    private void EnterState(TimelineState toState)
    {
        currentState = toState;
        currentState.OnStateEnter?.Invoke();
        
        AnimatorOverrideController aoc = new AnimatorOverrideController(modelAnimator.runtimeAnimatorController);
        aoc.ApplyOverrides(new List<KeyValuePair<AnimationClip, AnimationClip>> { new KeyValuePair<AnimationClip, AnimationClip>(aoc.animationClips[0], currentState.statePlayable.modelDefaultAnimation) });
        modelAnimator.runtimeAnimatorController = aoc;

        if (!oneOffPlaying || oneOffInterrubtable)
            ContinueState();
    }

    private void ContinueState()
    {

        playableDirector.Play(currentState.statePlayable.timelinePlayable, DirectorWrapMode.Loop);
        playableDirector.playableGraph.GetRootPlayable(0).SetSpeed(1);
    }

    public void Update()
    {
        foreach (TimelineTransition transition in anyStateTransitions)
        {
            if (transition.toState == currentState)
                continue;

            if (transition.condition.Invoke())
            {
                TransitionTo(transition.toState);
                return;
            }
        }

        foreach (TimelineTransition transition in currentState.transitions)
        {
            if (transition.condition.Invoke())
            {
                TransitionTo(transition.toState);
                return;
            }
        }

        if (!oneOffPlaying)
            currentState.OnStateUpdate?.Invoke();
    }
}

public class CharacterAnimation : MonoBehaviour
{

    protected Animator animator;
    protected PlayableDirector playableDirector;

    public GameObject animationRoot;

    [SerializeField]
    protected GameObject modelRoot;
    protected Animator modelAnimator;

    private Coroutine iFrameCoroutine;
    [SerializeField]
    private float iFrameBlinkRate = 0.1f;
    /*
    [SerializeField]
    private float turnSpeed = 360;

    private Coroutine rotationCoroutine;
    */

    Quaternion animationRootRotation = Quaternion.identity;
    public bool lockFacingDirection;

    Quaternion prevRotation;
    Vector3 prevPosition;

    [SerializeField]
    float rotationDifferenceScale;
    [SerializeField]
    float planeChangePositionDifferenceScale;

    static private readonly Quaternion flipUp = Quaternion.Euler(0, 180, 0);

    public TimelineStateMachine stateMachine;

    [SerializeField]
    TimelineStatePlayable idlePlayable;

    [SerializeField]
    TimelineAsset[] boredPlayables;
    [SerializeField]
    private float boredRate;
    private float boredTimer;

    #region Sibling References
    protected CharacterMovement movement;
    #endregion

    protected virtual void Awake()
    {
        movement = GetComponent<CharacterMovement>();

        animator = GetComponent<Animator>();
        modelAnimator = modelRoot.GetComponent<Animator>();
        //initialFacingRotation = animationRoot.transform.localRotation;

        playableDirector = GetComponent<PlayableDirector>();

        stateMachine = new TimelineStateMachine(playableDirector, new TimelineState(idlePlayable), modelAnimator);

        stateMachine.idleState.OnStateEnter += () => boredTimer = 0;
        stateMachine.idleState.OnStatePause += () => boredTimer = 0;
        stateMachine.idleState.OnStateExit += () => boredTimer = 0;

        stateMachine.idleState.OnStateUpdate += () => { boredTimer += Time.deltaTime; if (boredTimer >= boredRate) { boredTimer = 0; PlayBoredFidget(); } };

        playableDirector.stopped += stateMachine.FinishInterruptedPlayable;
    }

    protected virtual void Start()
    {

    }

    protected virtual void Update()
    {
        stateMachine.Update();

        if (prevRotation != transform.rotation * animationRootRotation)
        {
            Quaternion toRotateTo = transform.rotation * animationRootRotation;

            float angle = Quaternion.Angle(prevRotation, toRotateTo);

            animationRoot.transform.rotation = Quaternion.Slerp(prevRotation, toRotateTo, angle / 360 * rotationDifferenceScale);//Quaternion.Euler(Vector3.SmoothDamp(prevRotation.eulerAngles, toRotateTo.eulerAngles, ref rotateDampVelocity, orientDampTime, orientDampMaxSpeed));

        }
        else if (animationRoot.transform.localPosition != Vector3.zero)
        {
            animationRoot.transform.position = Vector3.Lerp(prevPosition, transform.position, (prevPosition - transform.position).magnitude * planeChangePositionDifferenceScale);
        }

        prevRotation = animationRoot.transform.rotation;
        prevPosition = animationRoot.transform.position;
    }

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

    private void PlayBoredFidget()
    {
        if(boredPlayables.Length != 0)
            PlayTimelinePlayable(boredPlayables[Random.Range(0, boredPlayables.Length)], true);

        boredTimer = 0;
    }

    public void PlayTimelinePlayable(TimelineAsset playable, bool interruptableByStateTransition = false)
    {
        if (interruptableByStateTransition)
            stateMachine.oneOffInterrubtable = true;

        stateMachine.Pause();

        playableDirector.Play(playable, DirectorWrapMode.None);
    }

    public Quaternion GetFacingDirectionRotation()
    {
        return animationRootRotation;
    }

    public void SetFacingDirection(float sign)
    {
        if (lockFacingDirection)
            return;

        animationRootRotation = (sign > 0) ? Quaternion.identity : flipUp;
        animationRoot.transform.localRotation = (sign > 0) ? Quaternion.identity : flipUp;
    }

    /*
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
    */

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
