using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDynamicPlaneConstrainable
{

}

public class DynamicPlaneConstraint : MonoBehaviour
{
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
                EnterPlaneBreaker(col.GetComponent<PlaneBreaker>());
                break;
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
                ExitPlaneBreaker(col);
                break;
        }
    } 

    // Update is called once per frame
    void Update()
    {
        
    }
}
