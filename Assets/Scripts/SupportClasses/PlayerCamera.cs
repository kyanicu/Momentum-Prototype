using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KinematicCharacterController;

/// <summary>
/// Class that represents the Camera's behavior of following (Or being controlled directly by) the player
/// Implements IPlayerCamera to allow external  with the player
/// </summary>
public class PlayerCamera : MonoBehaviour
{

    /// <summary>
    /// The default distance from the player
    /// </summary>
    [SerializeField]
    private float cameraDistance;

#region Camera-World Orientation Calculation
    /// <summary>
    /// The current calculated world forward (during reorientation)
    /// </summary>
    private Vector3 cameraWorldForward;
    /// <summary>
    /// The current calculated world up (during reorientation)
    /// </summary>
    private Vector3 cameraWorldUp;
    /// <summary>
    /// Orient forward damp velocity reference
    /// </summary>
    private Vector3 orientForwardDampVel = Vector3.zero;
    /// <summary>
    /// Orient forward damp velocity reference
    /// </summary>
    private Vector3 orientUpDampVel = Vector3.zero;
    /// <summary>
    /// The shared forward/up orient damp time
    /// </summary>
    [SerializeField]
    private float orientDampTime;
    /// <summary>
    /// The shared forward/up orient damp max speed
    /// </summary>
    [SerializeField]
    private float orientDampMaxSpeed;
#endregion

#region Auto Camera Info
#region Tilt Calculation
    /// <summary>
    /// The current calculated point in space the camera is aiming towards
    /// </summary>
    Vector3 targetTiltPoint;
    [SerializeField]
    /// <summary>
    /// The max distance from the player before maxing out
    /// </summary>
    float maxPlayerSpeedToResetFollow;
    /// <summary>
    /// Required player velocity before the auto camera tilts behind 
    /// </summary>
    [SerializeField]
    private float tiltThreshold;
    /// <summary>
    /// Tilt strength factor
    /// </summary>
    [SerializeField]
    private float tiltScale;
    /// <summary>
    /// Tilt smooth damp velocity reference
    /// </summary>
    private Vector3 tiltDampVel = Vector3.zero;
    [SerializeField]
    /// <summary>
    /// Tilt smooth damp time 
    /// </summary>
    private float tiltDampTime;
    /// <summary>
    /// Tilt smooth damp max speed
    /// </summary>
    [SerializeField]
    private float tiltDampMaxSpeed;
#endregion

#region Zoom Calculation
    /// <summary>
    /// The current calculated extra camera distance
    /// </summary>
    float zoomExtraDistance;
    /// <summary>
    /// Required player velocity before the auto camera zooms out
    /// </summary>
    [SerializeField]
    private float zoomThreshold;
    /// <summary>
    /// Zoom strength factor
    /// </summary>
    [SerializeField]
    private float zoomScale;
    /// <summary>
    /// Zoom smooth damp velocity reference
    /// </summary>
    private float zoomDampVel = 0;
    /// <summary>
    /// Zoom smooth damp time
    /// </summary>
    [SerializeField]
    private float zoomDampTime;
    /// <summary>
    /// Zoom damp max speed
    /// </summary>
    [SerializeField]
    private float zoomDampMaxSpeed;
#endregion
#endregion

#region Manual Camera Info 
    /// <summary>
    /// Is camera on manual tilt mode?
    /// </summary>
    private bool manualTilting;
    /// <summary>
    /// Manual tilt speed
    /// </summary>
    [SerializeField]
    private float manualTiltingSpeed;
    /// <summary>
    /// Current manual tilting direction
    /// </summary>
    private Vector3 manualTiltDirection;
#endregion

#region Player Info References
    /// <summary>
    /// A readonly reference to tghe player's transform
    /// </summary>
    private ReadOnlyTransform playerTransform;
    /// <summary>
    /// The player's current plane normal
    /// </summary>
    private Vector3 playerPlaneNormal;
    /// <summary>
    /// The player's current gravity direction
    /// </summary>
    private Vector3 playerGravityDirection;
#endregion

#region External References
    private PlayerMovement movement;
#endregion

    #region Unity Monobehavior Messages
    /// <summary>
    /// Sets default values on component reset/initialization in inspector
    /// </summary>
    void Reset()
    {
        cameraDistance = 15;

        maxPlayerSpeedToResetFollow = 50;

        orientDampTime = 0.1f;
        orientDampMaxSpeed = 100;

        tiltThreshold = 20;
        tiltScale = 0.5f;
        tiltDampTime = 0.1f;
        tiltDampMaxSpeed = 100;

        zoomThreshold = 30;
        zoomScale = 0.3f;
        zoomDampTime = 0.1f;
        zoomDampMaxSpeed = 100;

        manualTiltingSpeed = 30;
    }
    
    /// <summary>
    /// Handles class initialization
    /// </summary>
    void Awake()
    {
        cameraWorldForward = transform.forward;
        playerPlaneNormal = transform.forward;
        
        cameraWorldUp = transform.up;
        playerGravityDirection = -transform.up;
    }

    /// <summary>
    /// Initializes GameObject for player
    /// </summary>
    void Start()
    {
        targetTiltPoint = playerTransform.position;
    }

    /// <summary>
    /// Handles frame updates
    /// Handles the camera's following of the player as well as auto camera tilting and zooming
    /// ? Should this be cleaned up/restructured? It looks a bit messy for what it's trying to do, especially with it's constant positioning  changes of the camera instead of just changing it once at the end
    /// </summary>
    void Update()
    {
        // Handle reorientation if world up or plane forward doesn't match the camera's up/forward
        if (cameraWorldForward != playerPlaneNormal)
            cameraWorldForward = Vector3.SmoothDamp(cameraWorldForward, playerPlaneNormal, ref orientForwardDampVel, orientDampTime, orientDampMaxSpeed);
        if (cameraWorldUp != -playerGravityDirection)
            cameraWorldUp = Vector3.SmoothDamp(cameraWorldUp, playerGravityDirection, ref orientUpDampVel, orientDampTime, orientDampMaxSpeed);
        
        // Set the default position of the camera from the player without considering added tilt/zoom
        transform.position = playerTransform.position - cameraWorldForward * cameraDistance;

        if (!manualTilting)
        {
            // Handle auto tilting
            transform.position = playerTransform.position - cameraWorldForward * cameraDistance;
            if (movement.velocity.sqrMagnitude > tiltThreshold * tiltThreshold)
                targetTiltPoint = Vector3.SmoothDamp(targetTiltPoint, playerTransform.position + (movement.velocity - movement.velocity.normalized * tiltThreshold) * tiltScale, ref tiltDampVel, tiltDampTime, tiltDampMaxSpeed);
            else
                targetTiltPoint = Vector3.SmoothDamp(targetTiltPoint, playerTransform.position, ref tiltDampVel, tiltDampTime/2, tiltDampMaxSpeed);
            
            transform.LookAt(targetTiltPoint, cameraWorldUp);
            transform.position = playerTransform.position - transform.forward * cameraDistance;
        }
        else
        {
            // Handle manual tilting
            targetTiltPoint = playerTransform.position;
            tiltDampVel = Vector3.zero;
            transform.position = playerTransform.position - manualTiltDirection * cameraDistance;
            transform.LookAt(playerTransform.position, cameraWorldUp);
        }

        // Handle auto zoom
        if (movement.velocity.sqrMagnitude > zoomThreshold * zoomThreshold)
            zoomExtraDistance = Mathf.SmoothDamp(zoomExtraDistance, (movement.velocity.magnitude - zoomThreshold) * zoomScale, ref zoomDampVel, zoomDampTime, zoomDampMaxSpeed);
        else
            zoomExtraDistance = Mathf.SmoothDamp(zoomExtraDistance, 0, ref zoomDampVel, zoomDampTime, zoomDampMaxSpeed);
        transform.position -= transform.forward * zoomExtraDistance;
    }
#endregion

#region PlayerExternal

    /// <summary>
    /// Handles ReadOnly reference setup
    /// </summary>
    /// <param name="_playerTransform">The player's readonly transform reference</param>
    /// <param name="_playerKinematicMotor">The player's readonly Motor reference</param>
    public void SetReadOnlyReferences(ReadOnlyTransform _playerTransform, PlayerMovement _movement)
    {
        playerTransform = _playerTransform;
        movement = _movement;
        movement.planeChanged += HandlePlayerPlaneChanged;
    }

    /// <summary>
    /// Handles the player's change in plane
    /// </summary>
    /// <param name="planeChangeInfo">Info on the plane change</param>
    public void HandlePlayerPlaneChanged(PlaneChangeArgs planeChangeInfo)
    {
        if(!planeChangeInfo.planeBreaker)
            playerPlaneNormal = planeChangeInfo.planeNormal;
    }

    /// <summary>
    /// Handles the player's change of gravity
    /// </summary>
    /// <param name="gravityDirection">The player's new gravity direction</param>
    public void HandlePlayerGravityDirectionChanged(Vector3 gravityDirection)
    {
        playerGravityDirection = -gravityDirection;
    }
#endregion

    /// <summary>
    /// Handles the player's controller input for manual camera tilting
    /// </summary>
    /// <param name="controllerActions">The controller's actions</param>
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
}
