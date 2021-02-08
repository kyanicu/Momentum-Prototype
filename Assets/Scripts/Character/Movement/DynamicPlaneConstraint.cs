using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

/// <summary>
/// Hold info on a recent player plane change
/// </summary>
public struct PlaneChangeArgs
{
    /// <summary>
    /// The new plane normal
    /// </summary>
    public Vector3 planeNormal;
    /// <summary>
    /// Was the plane changed via a plane breaker?
    /// </summary>
    public bool planeBreaker;

    /// <summary>
    /// Basic constructor
    /// </summary>
    /// <param name="normal">The normal of the new plane</param>
    /// <param name="breaker">Was the plane changed via a plane breaker</param>
    public PlaneChangeArgs(Vector3 normal, bool breaker)
    {
        planeNormal = normal;
        planeBreaker = breaker;
    }
}

public interface IDynamicPlaneConstrainable
{
    void HandlePlaneSettingExtra(Plane plane);
    bool PlaneBreakValid();

    Vector3 position { get; set; }
    Vector3 velocity { get; set; }

    bool usePlaneBreakers { get; }
}

public class DynamicPlaneConstraint : MonoBehaviour
{

    /// <summary>
    /// Triggered on Plane Change
    /// </summary>
    public event Action<PlaneChangeArgs> planeChanged;

#region Dynamic Plane Info
    /// <summary>
    /// The current plane that the player is on
    /// In regards to the 2.5D Dynamic Plane Shifting mechanic
    /// </summary>
    public Plane currentPlane {get; private set; }
    /// <summary>
    /// The plane a player broke off of when on a plane breaker
    /// Tracked so that if the player leaves a plane breaker without being in a dynamic plane, it can reorient itself back to this as it's last stable plane
    /// </summary>
    public Plane brokenPlane {get; private set; }
    /// <summary>
    /// The current dynamic plane the player is on
    /// Holds up to 2 incase of overlap
    /// </summary>
    private DynamicPlane[] currentDynamicPlanes = new DynamicPlane[2];
    public DynamicPlane effectiveDynamicPlane { get { return currentDynamicPlanes[0]; } }
    private PlaneBreaker[] currentPlaneBreakers = new PlaneBreaker[2];
    public PlaneBreaker effectivePlaneBreaker { get { return currentPlaneBreakers[0]; } }
    /// <summary>
    /// Current stored plane waiting to be set at the appropriate time in the update cycle
    /// </summary>
    private Plane settingPlane = new Plane(Vector3.zero, Vector3.zero);
#endregion
    
    IDynamicPlaneConstrainable constrainable;

    // Start is called before the first frame update
    void Awake()
    {
        constrainable = GetComponent<IDynamicPlaneConstrainable>();
    }

    /// <summary>
    /// Appropriately handle entering a trigger
    /// </summary>
    /// <param name="col"> The trigger collider</param>
    void OnTriggerEnter(Collider col)
    {
        switch (col.tag)
        {
            case ("Plane") :
                EnterDynamicPlane(col.GetComponent<DynamicPlane>());
                break;
            case ("Plane Breaker") :
                if (constrainable.usePlaneBreakers)
                EnterPlaneBreaker(col.GetComponent<PlaneBreaker>());
                break;
        }
    } 

    /// <summary>
    /// Sets the current plane and fixes the player and their movement on that plane
    /// </summary>
    /// <param name="motor"> The player's kinematic motor</param>
    /// <param name="plane"> The new plane to be set</param>
    /// <param name="breaker"> Is the plane change a result of a PlaneBreaker </param>
    public void SetCurrentPlane(Plane plane, bool breaker = false)
    {
        //If the plane is invalid or identical to the current plane
        if (plane.normal == Vector3.zero || (plane.normal == currentPlane.normal && plane.distance == currentPlane.distance))
            return; 

        // Set the currentPlane
        currentPlane = plane;
        // Move player to plane
        constrainable.position = plane.ClosestPointOnPlane(constrainable.position);

        
        // Rotate Velocity along new plane without losing momentum
        constrainable.velocity = Quaternion.FromToRotation(plane.normal, plane.normal) * constrainable.velocity;

        // Trigger communication event on plane change
        planeChanged?.Invoke(new PlaneChangeArgs(plane.normal, breaker));

        constrainable.HandlePlaneSettingExtra(currentPlane);
    }

    /// <summary>
    /// Begin tracking newly entered DynamicPlane
    /// </summary>
    /// <param name="motor"> The player's kinematic motor</param>
    /// <param name="dynamicPlane"> The DynamicPlane entered</param>
    private void EnterDynamicPlane(DynamicPlane dynamicPlane)
    {
        // If DynamicPlane overlaps with another the player is already in
        if (currentDynamicPlanes[0] != null)
        {
            // If the previous DynamicPlane is prioritized, keep new DynamicPlane tracked secondarily
            if (currentDynamicPlanes[0].prioritize)
                currentDynamicPlanes[1] = dynamicPlane;
            else
            {
                // Track previous DynamicPlane secondarily, and new one primarily, forgetting any other tracked DynamicPlane
                currentDynamicPlanes[1] = currentDynamicPlanes[0];
                currentDynamicPlanes[0] = dynamicPlane;
            }
        }
        else
            // Track new DynamicPlane primarily
            currentDynamicPlanes[0] = dynamicPlane;
    }

    /// <summary>
    /// End tracking previously entered DynamicPlane 
    /// </summary>
    /// <param name="col"> The collider of the DynamicPlane trigger exited</param>
    private void ExitDynamicPlane(Collider col)
    {
        // If exited DynamicPlane was tracked primarily
        if (currentDynamicPlanes[0].gameObject == col.gameObject)
        {
            // Shift over potential secondarily tracked DynamicPlane so that it is now tracked primarily
            // If there was no DynamicPlane, then there is no longer any tracked DynamicPlane
            currentDynamicPlanes[0] = currentDynamicPlanes[1];
            // Stop tracking secondary DynamicPlane
            currentDynamicPlanes[1] = null;
        }
        // If exited DynamicPlane was only tracked secondarily  
        else if (currentDynamicPlanes[1].gameObject == col.gameObject)
        {
            // Stop tracking it
            currentDynamicPlanes[1] = null;
        }
    }

    /// <summary>
    /// Begin tracking newly entered PlaneBreaker
    /// </summary>
    /// <param name="motor"> The player's kinematic motor</param>
    /// <param name="planeBreaker">The PlaneBreaker entered</param>
    private void EnterPlaneBreaker(PlaneBreaker planeBreaker)
    {
        if (!constrainable.usePlaneBreakers)
            return;

        // If PlaneBreaker overlaps with another the player is already in
        if (currentPlaneBreakers[0] != null)
        {
            // If the previous PlaneBreaker is prioritized, keep new PlaneBreaker tracked secondarily
            if (currentPlaneBreakers[0].prioritize)
                currentPlaneBreakers[1] = planeBreaker;
            else
            {
                // Track previous PlaneBreaker secondarily, and new one primarily, forgetting any other tracked PlaneBreaker
                currentPlaneBreakers[1] = currentPlaneBreakers[0];
                currentPlaneBreakers[0] = planeBreaker;

                // Remember the plane that was broken
                brokenPlane = currentPlane;
            }
        }
        else
        {
            // Track new PlaneBreaker primarily
            currentPlaneBreakers[0] = planeBreaker;

            // Remember the plane that was broken
            brokenPlane = currentPlane;
        }
    }

    /// <summary>
    /// End tracking previously entered PlaneBreaker 
    /// </summary>
    /// <param name="col"> The collider of the PlaneBreaker trigger exited</param>
    private void ExitPlaneBreaker(Collider col)
    {
        if (!constrainable.usePlaneBreakers)
            return;

        // If exited PlaneBreaker was tracked primarily
        if (currentPlaneBreakers[0].gameObject == col.gameObject)
        {
            // Shift over potential secondarily tracked PlaneBreaker so that it is now tracked primarily
            // If there was no PlaneBreaker, then there is no longer any tracked PlaneBreaker
            currentPlaneBreakers[0] = currentPlaneBreakers[1];
            // Stop tracking secondary PlaneBreaker
            currentPlaneBreakers[1] = null;

            // if There was no secondarily tracked PlaneBreaker
            if (currentPlaneBreakers[0] == null)
            {
                // Restore broken plane in the case that there is no current Dynamic Plane
                if (currentDynamicPlanes[0] == null)
                    settingPlane = brokenPlane; ////currentPlane = brokenPlane;
                
                // Stop Tracking the broken plane
                brokenPlane = new Plane(Vector3.zero, Vector3.zero);
            }
        }
        // If exited PlaneBreaker was only tracked secondarily  
        else if (currentPlaneBreakers[1].gameObject == col.gameObject)
        {
            // Stop tracking it
            currentPlaneBreakers[1] = null;
        }
    }

    /// <summary>
    /// Appropriately handle exiting a trigger
    /// </summary>
    /// <param name="col"> The trigger collider</param>
    void OnTriggerExit(Collider col)
    {
        switch (col.tag)
        {
            case ("Plane") :
                ExitDynamicPlane(col);
                break;
            case ("Plane Breaker") :
                if (constrainable.usePlaneBreakers)
                ExitPlaneBreaker(col);
                break;
        }
    } 

    // Update is called once per frame
    void FixedUpdate()
    {
        // Handle setting the current plane 
        // Set so that the update uses the proper plane that the player is currently on

        // If active on PlaneBreaker
        if (constrainable.usePlaneBreakers && currentPlaneBreakers[0] != null && constrainable.PlaneBreakValid()) 
        {
            Vector3 planeRight = Vector3.Cross(currentPlaneBreakers[0].transform.up, currentPlane.normal);
            bool movingRight = Vector3.Dot(constrainable.velocity, planeRight) > 0;            
            
            // Update current plane to match the player's position on the PlaneBreaker
            SetCurrentPlane(currentPlaneBreakers[0].GetClosestPathPlane(constrainable.position, movingRight), true);
        }
        // If on Dynamic Plane
        else if(currentDynamicPlanes[0] != null)
        {
            Vector3 planeRight = Vector3.Cross(currentDynamicPlanes[0].transform.up, currentPlane.normal);
            bool movingRight = Vector3.Dot(constrainable.velocity, planeRight) > 0;

            // Update current plane to match the player's position on the DynamicPlane
            SetCurrentPlane(currentDynamicPlanes[0].GetClosestPathPlane(constrainable.position, movingRight), false);
        }
        // Set stored plane, if waiting to be set
        else if (settingPlane.normal != Vector3.zero)
            SetCurrentPlane(settingPlane);

        // Reset stored plane
        settingPlane = new Plane(Vector3.zero, Vector3.zero);
    }
}
