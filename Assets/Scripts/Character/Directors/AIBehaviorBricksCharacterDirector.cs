using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pada1.BBCore.Framework;
using Pada1.BBCore.Tasks;
using Pada1.BBCore;
using BBUnity;

namespace BehaviorBricksAI
{

    namespace WorldPerception
    {
        namespace Conditionals
        {
            [Condition("AICharacter/WorldPerception/Conditionals/IsNearOpponent")]
            [Help("Checks if an opposing character is near")]
            public class IsNearOpponent : BBUnity.Conditions.GOCondition
            {
                [OutParam("OpponentNear")]
                public GameObject OpponentNear;

                [OutParam("OpponentPosition")]
                public Vector3 OpponentPosition;

                //[InParam("Radius")]
                public float Radius = 10;

                public override bool Check()
                {
                    LayerMask opponentLayerMask = (gameObject.layer == LayerMask.NameToLayer("Enemy")) ? LayerMask.GetMask("Player") : LayerMask.GetMask("Enemy");
                    Collider[] opponents = Physics.OverlapSphere(gameObject.transform.position, Radius, opponentLayerMask, QueryTriggerInteraction.Ignore);
                    if (opponents.Length > 0)
                    {
                        OpponentNear = opponents[0].gameObject;
                        OpponentPosition = OpponentNear.transform.position;
                        return true;
                    }
                    else
                        return false;
                }
            }
        }
    }

    namespace CharacterActions
    {
        [Action("AICharacter/WorldPerception/Actions/GetDirectionToPosition/GetSlopedDirectionToPosition")]
        [Help("Gets direction to move across either the horizon or ground to reach a position")]
        public class GetSlopedDirectionToPosition : BBUnity.Actions.GOAction
        {
            //[InParam("Position")]
            public Vector3 Position = Vector3.zero;

            [OutParam("DirectionTo")]
            public float DirectionTo;

            private AICharacterDirector director;

            public override void OnStart()
            {
                director = gameObject.GetComponent<AICharacterDirector>();
            }

            public override TaskStatus OnUpdate()
            {
                Vector3 forward = director.movement.forward;
                Vector3 up = (director.movement.isGroundedThisUpdate) ? director.movement.groundNormal : director.movement.upWorldOrientation;

                Vector3 right = Vector3.Cross(up, forward);
                DirectionTo = Mathf.Sign(Vector3.Dot(right, gameObject.transform.position));
                return TaskStatus.COMPLETED;
            }
        }

        [Action("AICharacter/CharacterActions/SetSimpleCharacterMovementActionControl/SetSimpleActionAcceleration")]
        [Help("Sets the current control for the character's Simple Movement Action component")]
        public class SetSimpleActionAcceleration : BBUnity.Actions.GOAction
        {
            //[InParam("Direction")]
            public float Direction = -1;

            private AICharacterDirector director;

            public override void OnStart()
            {
                director = gameObject.GetComponent<AICharacterDirector>();
            }

            public override TaskStatus OnUpdate()
            {
                (director.movementActionControl as SimpleMovementActionControl).accelerate = Direction;
                return TaskStatus.COMPLETED;
            }
        }
    }
}

public class AIBehaviorBricksCharacterDirector : AICharacterDirector
{

    [Space]
    [Header("Behaviour Tree")]
    [SerializeField]
    private InternalBrickAsset behaviourTree;
    [SerializeField]
    [Range(1, 1000)]
    private int behaviourTreeMaxTasksPerTick = 500;
    [SerializeField]
    private bool behaviourTreePaused;
    [SerializeField]
    private bool behaviourTreeRestartWhenFinished;
    private BrickExecutor behaviourTreeExecutor;

    // Start is called before the first frame update
    protected override void Awake()
    {
        base.Awake();

        behaviourTreeExecutor = new BrickExecutor(gameObject);
        behaviourTreeExecutor.SetBrickAsset(behaviourTree);
    }
    protected override void OnEnable()
    {
        base.OnEnable();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
    }
#endif

    protected override void RegisterControl()
    {
        if (behaviourTreePaused)
            return;

        if (behaviourTreeExecutor.Finished)
        {
            if (behaviourTreeRestartWhenFinished)
                behaviourTreeExecutor.Relaunch(null);
        }
        else
        {
            behaviourTreeExecutor.Tick(behaviourTreeMaxTasksPerTick);
        }
    }
}
