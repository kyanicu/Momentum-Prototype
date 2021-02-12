﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KinematicCharacterController
{
    /// <summary>
    /// Represents the entire state of a PhysicsMover that is pertinent for simulation.
    /// Use this to save state or revert to past state
    /// </summary>
    [System.Serializable]
    public struct PhysicsMoverState
    {
        public Vector3 Position;
        public Quaternion Rotation;
        public Vector3 Velocity;
        public Vector3 AngularVelocity;
    }

    /// <summary>
    /// Component that manages the movement of moving kinematic rigidbodies for
    /// proper interaction with characters
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class PhysicsMover : MonoBehaviour
    {
        /// <summary>
        /// The mover's Rigidbody
        /// </summary>
        [ReadOnly]
        public Rigidbody Rigidbody;

        /// <summary>
        /// Determines if the platform moves with rigidbody.MovePosition (when true), or with rigidbody.position (when false)
        /// </summary>
        public bool MoveWithPhysics = true;

        /// <summary>
        /// Index of this motor in KinematicCharacterSystem arrays
        /// </summary>
        [NonSerialized]
        public IMoverController MoverController;
        /// <summary>
        /// Remembers latest position in interpolation
        /// </summary>
        [NonSerialized]
        public Vector3 LatestInterpolationPosition;
        /// <summary>
        /// Remembers latest rotation in interpolation
        /// </summary>
        [NonSerialized]
        public Quaternion LatestInterpolationRotation;
        /// <summary>
        /// The latest movement made by interpolation
        /// </summary>
        [NonSerialized]
        public Vector3 PositionDeltaFromInterpolation;
        /// <summary>
        /// The latest rotation made by interpolation
        /// </summary>
        [NonSerialized]
        public Quaternion RotationDeltaFromInterpolation;

        /// <summary>
        /// Index of this motor in KinematicCharacterSystem arrays
        /// </summary>
        public int IndexInCharacterSystem { get; set; }
        /// <summary>
        /// Remembers initial position before all simulation are done
        /// </summary>
        public Vector3 InitialTickPosition { get; set; }
        /// <summary>
        /// Remembers initial rotation before all simulation are done
        /// </summary>
        public Quaternion InitialTickRotation { get; set; }

        /// <summary>
        /// The mover's Transform
        /// </summary>
        public Transform Transform { get; protected set; }
        /// <summary>
        /// The character's position before the movement calculations began
        /// </summary>
        public Vector3 InitialSimulationPosition { get; protected set; }
        /// <summary>
        /// The character's rotation before the movement calculations began
        /// </summary>
        public Quaternion InitialSimulationRotation { get; protected set; }

        private Vector3 _internalTransientPosition;

        /// <summary>
        /// The mover's rotation (always up-to-date during the character update phase)
        /// </summary>
        public Vector3 TransientPosition
        {
            get
            {
                return _internalTransientPosition;
            }
            protected set
            {
                _internalTransientPosition = value;
            }
        }

        private Quaternion _internalTransientRotation;
        /// <summary>
        /// The mover's rotation (always up-to-date during the character update phase)
        /// </summary>
        public Quaternion TransientRotation
        {
            get
            {
                return _internalTransientRotation;
            }
            protected set
            {
                _internalTransientRotation = value;
            }
        }


        protected virtual void Reset()
        {
            ValidateData();
        }

        protected virtual void OnValidate()
        {
            ValidateData();
        }

        /// <summary>
        /// Handle validating all required values
        /// </summary>
        public void ValidateData()
        {
            Rigidbody = gameObject.GetComponent<Rigidbody>();

            Rigidbody.centerOfMass = Vector3.zero;
            Rigidbody.maxAngularVelocity = Mathf.Infinity;
            Rigidbody.maxDepenetrationVelocity = Mathf.Infinity;
            Rigidbody.isKinematic = true;
            Rigidbody.interpolation = RigidbodyInterpolation.None;
        }

        protected virtual void OnEnable()
        {
            KinematicCharacterSystem.EnsureCreation();
            KinematicCharacterSystem.RegisterPhysicsMover(this);
        }

        protected virtual void OnDisable()
        {
            KinematicCharacterSystem.UnregisterPhysicsMover(this);
        }

        protected virtual void Awake()
        {
            Transform = this.transform;
            ValidateData();

            TransientPosition = Rigidbody.position;
            TransientRotation = Rigidbody.rotation;
            InitialSimulationPosition = Rigidbody.position;
            InitialSimulationRotation = Rigidbody.rotation;
            LatestInterpolationPosition = Transform.position;
            LatestInterpolationRotation = Transform.rotation;
        }

        /// <summary>
        /// Sets the mover's position directly
        /// </summary>
        public void SetPosition(Vector3 position)
        {
            Transform.position = position;
            Rigidbody.position = position;
            InitialSimulationPosition = position;
            TransientPosition = position;
        }

        /// <summary>
        /// Sets the mover's rotation directly
        /// </summary>
        public void SetRotation(Quaternion rotation)
        {
            Transform.rotation = rotation;
            Rigidbody.rotation = rotation;
            InitialSimulationRotation = rotation;
            TransientRotation = rotation;
        }

        /// <summary>
        /// Sets the mover's position and rotation directly
        /// </summary>
        public void SetPositionAndRotation(Vector3 position, Quaternion rotation)
        {
            Transform.SetPositionAndRotation(position, rotation);
            Rigidbody.position = position;
            Rigidbody.rotation = rotation;
            InitialSimulationPosition = position;
            InitialSimulationRotation = rotation;
            TransientPosition = position;
            TransientRotation = rotation;
        }

        /// <summary>
        /// Returns all the state information of the mover that is pertinent for simulation
        /// </summary>
        public PhysicsMoverState GetState()
        {
            PhysicsMoverState state = new PhysicsMoverState();

            state.Position = TransientPosition;
            state.Rotation = TransientRotation;
            state.Velocity = Rigidbody.velocity;
            state.AngularVelocity = Rigidbody.velocity;

            return state;
        }

        /// <summary>
        /// Applies a mover state instantly
        /// </summary>
        public void ApplyState(PhysicsMoverState state)
        {
            SetPositionAndRotation(state.Position, state.Rotation);
            Rigidbody.velocity = state.Velocity;
            Rigidbody.angularVelocity = state.AngularVelocity;
        }

        // Self Added
        public virtual void PreVelocityUpdate(float deltaTime) { }

        /// <summary>
        /// Caches velocity values based on deltatime and target position/rotations
        /// </summary>
        public virtual void VelocityUpdate(float deltaTime)
        {
            InitialSimulationPosition = TransientPosition;
            InitialSimulationRotation = TransientRotation;

            MoverController.UpdateMovement(out _internalTransientPosition, out _internalTransientRotation, deltaTime);

            if (deltaTime > 0f)
            {
                Rigidbody.velocity = (TransientPosition - InitialSimulationPosition) / deltaTime;
                                
                Quaternion rotationFromCurrentToGoal = TransientRotation * (Quaternion.Inverse(InitialSimulationRotation));
                Rigidbody.angularVelocity = (Mathf.Deg2Rad * rotationFromCurrentToGoal.eulerAngles) / deltaTime;
            }
        }

        // Self Added
        public virtual void PreSetPositionAndRotation() { }

        // Self Added
        public virtual void PostSetPositionAndRotation() { }

    }
}