using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KinematicCharacterController;

public class PlayerCamera : MonoBehaviour
{

    private Player player;
    private KinematicCharacterMotor playerMotor;

    [SerializeField]
    private float
        cameraDistance,
        zoomThreshold,
        zoomScale,
        panThreshold,
        panScale,
        tiltThreshold,
        tiltScale,
        manualPanDistanceVertical,
        manualPanDistanceHorizontal;
        
    private Vector2 manualPanDirection;
    [HideInInspector]
    public bool manualPanLock;

    // Start is called before the first frame update
    void Start()
    {
        SetPlayerToFollow(GameObject.FindGameObjectWithTag("Player").GetComponent<Player>());
    }

    void Reset()
    {
        cameraDistance = 15;
        zoomThreshold = 30;
        zoomScale = 0.3f;
        panThreshold = 20;
        panScale = 0.2f;
        tiltThreshold = 40;
        tiltScale = 0.5f;
        manualPanDistanceVertical = 4;
        manualPanDistanceHorizontal = 8;

    }

    public void SetPlayerToFollow(Player p)
    {
        player = p;
        playerMotor = player.GetComponent<KinematicCharacterMotor>();
    }

    // Update is called once per frame
    // Need to fix to work with dynamic plane
    void Update()
    {
        transform.position = player.transform.position + (Vector3.back * cameraDistance);
        transform.LookAt(player.transform.position);

        bool zoom = playerMotor.BaseVelocity.magnitude >= zoomThreshold;
        bool pan = playerMotor.BaseVelocity.magnitude >= panThreshold;
        bool tilt = playerMotor.BaseVelocity.magnitude >= tiltThreshold;

        if (zoom || pan || tilt)
        {
            float addedZoomDistance = 0;
            float addedPanDistance = 0;

            if (zoom)
            {

                addedZoomDistance = (playerMotor.BaseVelocity.magnitude - zoomThreshold) * zoomScale;

                transform.position += (Vector3.back * addedZoomDistance);
            }

            if (pan)
            {
                manualPanLock = false;

                addedPanDistance = (playerMotor.BaseVelocity.magnitude - panThreshold) * panScale;

                transform.position += (playerMotor.BaseVelocity.normalized * addedPanDistance);
            }

            if (tilt)
            {
                Vector3 center = player.transform.position;//transform.position + Vector3.forward * (cameraDistance + addedZoomDistance);

                float tiltRotation = (playerMotor.BaseVelocity.magnitude - tiltThreshold) * tiltScale;

                transform.RotateAround(center, Quaternion.Euler(playerMotor.PlanarConstraintAxis * 90) * playerMotor.BaseVelocity.normalized, tiltRotation);
                
            }
        }

        if (manualPanDirection != Vector2.zero && !pan)
        {
            transform.position += new Vector3(manualPanDirection.x * manualPanDistanceHorizontal, manualPanDirection.y * manualPanDistanceVertical);
        }
    }
}
