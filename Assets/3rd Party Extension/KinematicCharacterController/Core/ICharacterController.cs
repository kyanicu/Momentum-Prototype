using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KinematicCharacterController
{
    public interface ICharacterController
    {

        /// <summary>
        /// This is called when the motor wants to know what its rotation should be right now
        /// </summary>
        /// <param name="currentRotation"> Reference to the player's rotation </param>
        /// <param name="deltaTime"> Motor update time </param>
        void UpdateRotation(ref Quaternion currentRotation, float deltaTime);
        
        /// <summary>
        /// This is called hen the motor wants to know what its velocity should be right now
        /// </summary>
        /// <param name="currentVelocity"> Reference to the player's velocity </param>
        /// <param name="maxMove"> The max distance the player can move this update</param>
        /// <param name="deltaTime"> Motor update time </param>
        void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime);

        /// <summary>
        /// This is called before the motor does anything
        /// </summary>
        /// <param name="motor"> The player's kinematic motor</param>
        /// <param name="deltaTime"> Motor update time </param>
        void BeforeCharacterUpdate(float deltaTime);

        /// <summary>
        /// This is called after the motor has finished its ground probing, but before PhysicsMover/Velocity/etc.... handling
        /// Primarily used currently to handle the slope tracking for the ungrounding angular momentum mechanic
        /// </summary>
        /// <param name="motor"> The player's kinematic motor</param>
        /// <param name="deltaTime"> Motor update time </param>
        void PostGroundingUpdate(float deltaTime);
        /// <summary>
        /// This is called after the motor has finished everything in its update
        /// </summary>
        /// <param name="motor"> The player's kinematic motor</param>
        /// <param name="deltaTime"> Motor update time </param>
        void AfterCharacterUpdate(float deltaTime);

        /// <summary>
        /// This is called after when the motor wants to know if the collider can be collided with (or if we just go through it)
        /// </summary>
        /// <param name="motor"> The player's kinematic motor</param>
        /// <param name="coll"> The collider being checked </param>
        bool IsColliderValidForCollisions(Collider coll);

        /// <summary>
        /// This is called when the motor's ground probing detects a ground hit
        /// </summary>
        /// <param name="motor"> The player's kinematic motor</param>
        /// <param name="hitCollider">The ground collider </param>
        /// <param name="hitNormal"> The ground normal </param>
        /// <param name="hitPoint"> The ground point </param>
        /// <param name="hitStabilityReport"> The ground stability </param>
        void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport);

        /// <summary>
        /// This is called when the motor's movement logic detects a hit
        /// </summary>
        /// <param name="motor"> The player's kinematic motor</param>
        /// <param name="hitCollider"> The hit collider </param>
        /// <param name="hitNormal"> The hit normal </param>
        /// <param name="hitPoint"> The hit point </param>
        /// <param name="hitStabilityReport"> The hit stability </param>
        void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport);

        /// <summary>
        /// This is called after every move hit, to give you an opportunity to modify the HitStabilityReport to your liking
        /// </summary>
        /// <param name="motor"> The player's kinematic motor</param>
        /// <param name="hitCollider"> The hit collider </param>
        /// <param name="hitNormal"> The hit normal </param>
        /// <param name="hitPoint"> The hit point </param>
        /// <param name="hitPoint"></param>
        /// <param name="atCharacterPosition"> The character position on hit </param>
        /// <param name="atCharacterRotation"> The character rotation on hit </param>
        /// <param name="hitStabilityReport"> The hit stability </param>
        void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport);

        /// <summary>
        /// This is called when the character detects discrete collisions (collisions that don't result from the motor's capsuleCasts when moving)
        /// </summary>
        /// <param name="motor"> The player's kinematic motor</param>
        /// <param name="hitCollider"> The detected collider </param>
        void OnDiscreteCollisionDetected(Collider hitCollider);
    }
}