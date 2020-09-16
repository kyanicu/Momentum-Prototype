using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KinematicCharacterController;

public class PlayerCamera : MonoBehaviour, IPlayerCameraCommunication
{


    [SerializeField]
    private float cameraDistance;

    Vector3 targetPoint;
    [SerializeField]
    float maxPlayerMoveToReset;
    [SerializeField]
    private float tiltThreshold;
    [SerializeField]
    private float tiltScale;
    private Vector3 tiltDampVel = Vector3.zero;
    [SerializeField]
    private float tiltDampTime;
    [SerializeField]
    private float tiltDampMaxSpeed;

    [SerializeField]
    private float zoomThreshold;
    [SerializeField]
    private float zoomScale;
    float zoomExtraDistance;
    private float zoomDampVel = 0;
    [SerializeField]
    private float zoomDampTime;
    [SerializeField]
    private float zoomDampMaxSpeed;
    
    private Vector3 cameraWorldForward;
    private Vector3 cameraWorldUp;
    private Vector3 orientForwardDampVel = Vector3.zero;
    private Vector3 orientUpDampVel = Vector3.zero;
    [SerializeField]
    private float orientDampTime;
    [SerializeField]
    private float orientDampMaxSpeed;

    private bool manualTilting;
    [SerializeField]
    private float manualTiltingSpeed;
    private Vector3 manualTiltDirection;

    /*
    [SerializeField]
    private float panThreshold;
    [SerializeField]
    private float panScale;
    [SerializeField]
    private float manualPanDistanceVertical;
    [SerializeField]
    private float manualPanDistanceHorizontal;

    private Vector2 manualPanDirection;
    [HideInInspector]
    public bool manualPanLock;
    */

    ReadOnlyTransform playerTransform;
    private KinematicCharacterMotorState playerMovementState = new KinematicCharacterMotorState();
    private Vector3 playerPlaneNormal;
    private Vector3 playerGravityDirection;

    // Start is called before the first frame update
    void Awake()
    {
        cameraWorldForward = transform.forward;
        playerPlaneNormal = transform.forward;
        
        cameraWorldUp = transform.up;
        playerGravityDirection = -transform.up;
    }

    void Start()
    {
        targetPoint = playerTransform.position;
    }

    void Reset()
    {
        cameraDistance = 15;

        /*
        panThreshold = 20;
        panScale = 0.2f;
        manualPanDistanceVertical = 4;
        manualPanDistanceHorizontal = 8;
        */

        maxPlayerMoveToReset = 50;

        tiltThreshold = 20;
        tiltScale = 0.5f;
        tiltDampTime = 0.1f;
        tiltDampMaxSpeed = 100;

        zoomThreshold = 30;
        zoomScale = 0.3f;
        zoomDampTime = 0.1f;
        zoomDampMaxSpeed = 100;

        orientDampTime = 0.1f;
        orientDampMaxSpeed = 100;

        manualTiltingSpeed = 30;
    }

    public void SetPlayerExternalCommunication(PlayerExternalCommunicator communicator, ReadOnlyTransform _playerTransform)
    {
        communicator.SetPlayerExternalCommunication(this);
        playerTransform = _playerTransform;
    }

    public void HandlePlayerPlaneChanged(PlaneChangeEventArgs planeChangeInfo)
    {
        if(!planeChangeInfo.planeBreaker)
            playerPlaneNormal = planeChangeInfo.planeNormal;
    }

    public void HandlePlayerGravityDirectionChanged(Vector3 gravityDirection)
    {
        playerGravityDirection = -gravityDirection;
    }

    public void HandlePlayerMovementStateUpdated(KinematicCharacterMotorState state)
    {
        if((state.Position - playerMovementState.Position).sqrMagnitude > maxPlayerMoveToReset * maxPlayerMoveToReset)
            targetPoint = state.Position;

        playerMovementState = state;
    }

    public void HandleInput(PlayerController.PlayerActions controllerActions)
    {
        if (controllerActions.CameraManualTilt.ReadValue<Vector2>() != Vector2.zero)
        {
            if(!manualTilting)
            {
                manualTilting = true;
                manualTiltDirection = cameraWorldForward;
            }

            Vector3 horizontalEuler = -playerGravityDirection * controllerActions.CameraManualTilt.ReadValue<Vector2>().x;
            Vector3 upEuler = Vector3.Cross(transform.forward, -playerGravityDirection) * controllerActions.CameraManualTilt.ReadValue<Vector2>().y;
            Vector3 euler = horizontalEuler + upEuler;
            if (euler.sqrMagnitude > 1)
                euler.Normalize();
            
            Vector3 newDirection = Quaternion.Euler(euler * Time.deltaTime * manualTiltingSpeed) * manualTiltDirection;

            if(Mathf.Abs(Vector3.Dot(newDirection, playerGravityDirection)) <= 95)
                manualTiltDirection = newDirection;
        }

        if(controllerActions.CameraChangeSetting.triggered)
        {
            manualTilting = !manualTilting;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (cameraWorldForward != playerPlaneNormal)
            cameraWorldForward = Vector3.SmoothDamp(cameraWorldForward, playerPlaneNormal, ref orientForwardDampVel, orientDampTime, orientDampMaxSpeed);
        if (cameraWorldUp != -playerGravityDirection)
            cameraWorldUp = Vector3.SmoothDamp(cameraWorldUp, playerGravityDirection, ref orientUpDampVel, orientDampTime, orientDampMaxSpeed);

        transform.position = playerTransform.position - cameraWorldForward * cameraDistance;

        if (!manualTilting)
        {
            transform.position = playerTransform.position - cameraWorldForward * cameraDistance;
            if (playerMovementState.BaseVelocity.sqrMagnitude > tiltThreshold * tiltThreshold)
                targetPoint = Vector3.SmoothDamp(targetPoint, playerTransform.position + (playerMovementState.BaseVelocity - playerMovementState.BaseVelocity.normalized * tiltThreshold) * tiltScale, ref tiltDampVel, tiltDampTime, tiltDampMaxSpeed);
            else
                targetPoint = Vector3.SmoothDamp(targetPoint, playerTransform.position, ref tiltDampVel, tiltDampTime/2, tiltDampMaxSpeed);
            
            transform.LookAt(targetPoint, cameraWorldUp);
            transform.position = playerTransform.position - transform.forward * cameraDistance;
        }
        else
        {
            targetPoint = playerTransform.position;
            tiltDampVel = Vector3.zero;
            transform.position = playerTransform.position - manualTiltDirection * cameraDistance;
            transform.LookAt(playerTransform.position, cameraWorldUp);
        }

        if (playerMovementState.BaseVelocity.sqrMagnitude > zoomThreshold * zoomThreshold)
            zoomExtraDistance = Mathf.SmoothDamp(zoomExtraDistance, (playerMovementState.BaseVelocity.magnitude - zoomThreshold) * zoomScale, ref zoomDampVel, zoomDampTime, zoomDampMaxSpeed);
        else
            zoomExtraDistance = Mathf.SmoothDamp(zoomExtraDistance, 0, ref zoomDampVel, zoomDampTime, zoomDampMaxSpeed);
        transform.position -= transform.forward * zoomExtraDistance;


        //transform.RotateAround(playerTransform.position, -playerGravityDirection, playerMovementState.BaseVelocity.magnitude * tiltScale);
        //targetPoint = playerTransform.position + playerMovementState.BaseVelocity * playerPositionPredictionTime;
        //transform.LookAt(targetPoint, -playerGravityDirection);

        /*
        Vector3 smoothedForward = Vector3.Slerp(transform.forward, playerPlaneNormal, 1 - Mathf.Exp(-10 * Time.deltaTime));
        smoothedForward = Vector3.ProjectOnPlane(smoothedForward, transform.up).normalized;
        transform.position = playerTransform.position + (-smoothedForward * cameraDistance);
        transform.forward = smoothedForward;
        transform.rotation = Quaternion.FromToRotation(transform.up, transform.up) * transform.rotation;
        */
        /*
        bool zoom = false;//playerMovementState.BaseVelocity.magnitude >= zoomThreshold;
        bool pan = false;//playerMovementState.BaseVelocity.magnitude >= panThreshold;
        bool tilt = playerMovementState.BaseVelocity.magnitude >= tiltThreshold;

        if (zoom || pan || tilt)
        {
            float addedZoomDistance = 0;
            float addedPanDistance = 0;

            if (zoom)
            {
                addedZoomDistance = (playerMovementState.BaseVelocity.magnitude - zoomThreshold) * zoomScale;

                transform.position += (Vector3.back * addedZoomDistance);
            }

            if (pan)
            {
                manualPanLock = false;

                addedPanDistance = (playerMovementState.BaseVelocity.magnitude - panThreshold) * panScale;

                transform.position += (playerMovementState.BaseVelocity.normalized * addedPanDistance);
            }

            if (tilt)
            {
                Vector3 center = playerTransform.position;//transform.position + Vector3.forward * (cameraDistance + addedZoomDistance);

                float tiltRotation = (playerMovementState.BaseVelocity.magnitude - tiltThreshold) * tiltScale;

                transform.RotateAround(playerTransform.position, , tiltRotation);

                //transform.RotateAround(center, Quaternion.Euler(playerPlaneNormal * 90) * playerMovementState.BaseVelocity.normalized, tiltRotation);
            }
        }
        */

        /*
        if (manualPanDirection != Vector2.zero && !pan)
        {
            transform.position += new Vector3(manualPanDirection.x * manualPanDistanceHorizontal, manualPanDirection.y * manualPanDistanceVertical);
        }
        */
    }
}
