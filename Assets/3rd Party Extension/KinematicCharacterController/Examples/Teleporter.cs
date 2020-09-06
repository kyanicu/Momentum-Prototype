using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using KinematicCharacterController.Examples;

namespace KinematicCharacterController.Examples
{
    public class Teleporter : MonoBehaviour
    {
        public Teleporter TeleportTo;

        public bool isBeingTeleportedTo { get; set; }

        private void OnTriggerEnter(Collider other)
        {
            if (!isBeingTeleportedTo)
            {
                KinematicCharacterMotor motor = other.GetComponent<KinematicCharacterMotor>();
                if (motor)
                {
                    motor.SetPositionAndRotation(TeleportTo.transform.position, TeleportTo.transform.rotation);
                    motor.BaseVelocity = Vector3.zero;
                    TeleportTo.isBeingTeleportedTo = true;
                }
            }
            isBeingTeleportedTo = false;
        }
    }
}