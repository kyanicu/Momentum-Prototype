using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class CharacterAnimationPlayable : PlayableAsset
{
    public CharacterAnimationPlayableBehaviour data;

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        ScriptPlayable<CharacterAnimationPlayableBehaviour> playable = ScriptPlayable<CharacterAnimationPlayableBehaviour>.Create(graph, data);

        CharacterAnimationPlayableBehaviour behaviour = playable.GetBehaviour();

        return playable;
    }
}

[TrackClipType(typeof(CharacterAnimationPlayable))]
//[TrackBindingType(typeof(CharacterCombat))]
public class CharacterAnimationPlayableTrack : TrackAsset { }

[System.Serializable]
public class HitboxReferenceWrapper
{
    [SerializeField]
    public ExposedReference<Hitbox> reference;
}

[System.Serializable]
public class CharacterAnimationPlayableBehaviour : PlayableBehaviour
{

    public ExposedReference<CharacterAnimation> characterAnimation;

    CharacterCombat combat = null;
    CharacterStatus status = null;
    CharacterMovement movement = null;
    CharacterAnimation animation = null;
    CharacterMovementAction movementAction = null;
    CharacterMovementPhysics movementPhysics = null;
    PlayerMovementAbility movementAbility = null;

    [Space]
    [Header("Hitbox/Attack Info")]
    public HitboxReferenceWrapper[] hitboxes;
    private Hitbox[] _hitboxes;
    public HitboxInfo[] hitboxInfo;


    [Space]
    [Header("Movement Mechanics Overriding")]
    public bool forceUnground = false;
    public bool lockFacingDirection = true;
    public bool lockMovementAction = true;
    public CharacterMovementPhysics.PhysicsNegations physicsNegations = default;
    public bool lockMovementPhysics = false;
    public bool lockMovementAbility;

    [Space]
    [Header("Character Armor")]
    public DamageArmor armor = DamageArmor.None;

    /*
    public bool deceleratingSlopedOrHorizontalVelocityToZero = false;
    public bool deceleratingFallingOrRisingVelocityToZero = false;
    public bool deceleratingAngularVelocityToZero = false;

    public int slopedOrHorizontalDecelerationFrameCount = 0;
    public int fallingOrRisingDecelerationFrameCount = 0;
    public int angularDecelerationFrameCount = 0;
    */

    [Space]
    [Header("Velocity Effects")]
    public bool settingSlopedOrHorizontalVelocity = false;
    public bool settingFallingOrRisingVelocity = false;
    public bool settingAngularVelocity = false;

    public ValueOverrideType velocitySetting = ValueOverrideType.Set;
    public ValueOverrideType angularVelocitySetting = ValueOverrideType.Set;
    public bool setVelocityWithFacingDirection = true;
    public bool setAngularVelocityWithFacingDirection = true;

    public Vector2 planarVelocity = Vector2.zero;
    public float angularVelocity = 0;

    private float slopedOrHorizontalDeceleration;
    private float FallingOrRisingDeceleration;
    private float angularDeceleration;
    
    [Space]
    public bool restorePreviousDataOnPlayableEnd;

    /*
    private HitboxInfo[] prevHitboxInfo;
    private bool prevMovementActionEnabled;
    private bool prevMovementPhysicsEnabled;
    private bool prevMovementAbilityEnabled;
    private DamageArmor prevArmor;
    private Vector3 initVelocity;
    private float initAngularVelocity;
    */
    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
            return;
#endif

        animation = characterAnimation.Resolve(playable.GetGraph().GetResolver());
        status = animation.GetComponent<CharacterStatus>();
        movement = animation.GetComponent<CharacterMovement>();
        movementAction = animation.GetComponent<CharacterMovementAction>();
        movementPhysics = animation.GetComponent<CharacterMovementPhysics>();
        movementAbility = animation.GetComponent<PlayerMovementAbility>();
        combat = animation.GetComponent<CharacterCombat>();

        int len = hitboxes.Length;
        _hitboxes = new Hitbox[len];
        for (int i = 0; i < len; i++)
        {
            _hitboxes[i] = hitboxes[i].reference.Resolve(playable.GetGraph().GetResolver());
        }

        // store prev info
        /*
        for (int i = 0; i < len; i++)
        {
            prevHitboxInfo[i] = _hitboxes[i].hitboxInfo;
        }
        prevMovementActionEnabled = movementAction.enabled;
        prevMovementPhysicsEnabled = movementPhysics.enabled;
        prevMovementAbilityEnabled = movementAbility.enabled;

        prevArmor = status.armor;
        */
    }

    public override void OnBehaviourPause(Playable playable, FrameData info)
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
            return;
#endif
        if (!animation)
            return;
        
        if (restorePreviousDataOnPlayableEnd)
        {
            int len = _hitboxes.Length;
            for (int i = 0; i < len; i++)
            {
                _hitboxes[i].hitboxInfo = default; //prevHitboxInfo[i];
            }
        }

        if(movementAction)
            movementAction.enabled = true;
        if (movementPhysics)
            movementPhysics.enabled = true;
        if (movementAbility)
            movementAbility.enabled = true;

        animation.lockFacingDirection = false;

        if (status)
            status.armor = DamageArmor.None;
    }

    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
            return;
#endif

        int len = _hitboxes.Length;
        for (int i = 0; i < len; i++)
        {
            _hitboxes[i].hitboxInfo = hitboxInfo[i];
        }

        if (forceUnground)
            movement?.ForceUnground();

        animation.lockFacingDirection = lockFacingDirection;

        movementAction.enabled = !lockMovementAction;

        if (movementPhysics)
        {
            movementPhysics.enabled = !lockMovementPhysics;
            movementPhysics.negations = physicsNegations;
        }

        if (movementAbility)
            movementAbility.enabled = !lockMovementAbility;

        if (status)
            status.armor = armor;

        //slopedOrHorizontalDeceleration;
        //fallingOrRisingDeceleration;
        //angularDeceleration;

        /*
        Vector2 decelerationToSet = new Vector2(deceleratingSlopedOrHorizontalVelocityToZero ? slopedOrHorizontalDeceleration : 0, deceleratingFallingOrRisingVelocityToZero ? FallingOrRisingDeceleration : 0);

        if (decelerationToSet != Vector2.zero)
        {
            movement.AddImpulseAlongPlane(decelerationToSet);
            return;
        }
        else
        {
        */
            Vector2 velocityToSet = new Vector2(settingSlopedOrHorizontalVelocity ? planarVelocity.x : 0, settingFallingOrRisingVelocity ? planarVelocity.y : 0);

            if (velocityToSet != Vector2.zero)
            {
                switch (velocitySetting)
                {
                    case ValueOverrideType.Set:
                        movement.velocity = (setVelocityWithFacingDirection ? animation.GetFacingDirectionRotation() : Quaternion.identity) * movement.GetVelocityFromPlanarVelocity(velocityToSet);
                        break;
                    case ValueOverrideType.Addition:
                        movement.velocity += (setVelocityWithFacingDirection ? animation.GetFacingDirectionRotation() : Quaternion.identity) * movement.GetVelocityFromPlanarVelocity(velocityToSet);
                        break;
                    case ValueOverrideType.Multiplier:
                        movement.velocity = Vector3.Scale(movement.velocity, (setVelocityWithFacingDirection ? animation.GetFacingDirectionRotation() : Quaternion.identity) * movement.GetVelocityFromPlanarVelocity(velocityToSet));
                        break;
                }
            }
        //}

        /*
        if (deceleratingAngularVelocityToZero)
        {
            movement.angularVelocity += (angularDeceleration * movement.currentPlane.normal);
            return;
        }
        else
        {*/
            if (settingAngularVelocity)
            {
                switch (angularVelocitySetting)
                {
                    case ValueOverrideType.Set:
                        movement.angularVelocity = (setAngularVelocityWithFacingDirection ? animation.GetFacingDirectionRotation() : Quaternion.identity) * movement.currentPlane.normal * angularVelocity;
                        break;
                    case ValueOverrideType.Addition:
                        movement.angularVelocity += (setAngularVelocityWithFacingDirection ? animation.GetFacingDirectionRotation() : Quaternion.identity) * movement.currentPlane.normal * angularVelocity;
                        break;
                    case ValueOverrideType.Multiplier:
                        movement.angularVelocity = Vector3.Scale(movement.angularVelocity, (setAngularVelocityWithFacingDirection ? animation.GetFacingDirectionRotation() : Quaternion.identity) * movement.currentPlane.normal * angularVelocity);
                        break;
                }
            }
        //}
    }

}