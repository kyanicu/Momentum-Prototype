using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
public class FSMTransition
{
    public FSMState toState;

    public System.Func<bool> condition;

    public FSMTransition(FSMState to, System.Func<bool> cond)
    {
        toState = to;
        condition = cond;
    }
}

public class FSMState
{
    public List<FSMTransition> transitions = new List<FSMTransition>();

    public System.Action OnStateEnter;

    public System.Action OnStateUpdate;

    public System.Action OnStateExit;

    public System.Action OnStatePause;
}

public class FiniteStateMachine
{
    public bool isPaused;

    public bool isStopped;

    public FSMState currentState;

    public FSMState startState;

    public List<FSMTransition> anyStateTransitions = new List<FSMTransition>();


    public FiniteStateMachine(FSMState initialState)
    {
        startState = initialState;
        EnterState(startState);
    }

    public void Restart()
    {
        Stop();
        Play();
    }

    public void Stop()
    {
        currentState.OnStateExit?.Invoke();
        currentState = startState;
        isStopped = true;
    }

    public void Pause()
    {
        isPaused = true;
        currentState.OnStatePause?.Invoke();
    }

    public void Play()
    {
        if (isStopped)
        {
            EnterState(startState);
        }
    }

    public void TransitionTo(FSMState toState)
    {
        currentState.OnStateExit?.Invoke();
        EnterState(toState);
    }

    private void EnterState(FSMState toState)
    {
        currentState = toState;
        currentState.OnStateEnter?.Invoke();
    }

    public void Update()
    {
        foreach (FSMTransition transition in anyStateTransitions)
        {
            if (transition.toState == currentState)
                continue;

            if (transition.condition.Invoke())
            {
                TransitionTo(transition.toState);
                return;
            }
        }

        foreach (FSMTransition transition in currentState.transitions)
        {
            if (transition.condition.Invoke())
            {
                TransitionTo(transition.toState);
                return;
            }
        }

        current.OnStateUpdate?.Invoke();
    }
    */