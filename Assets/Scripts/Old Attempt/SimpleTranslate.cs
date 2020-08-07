using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public struct PlaneInfo 
    {
        public Plane plane { get; private set; }
        public Vector3 planeUp{ get; private set; }
        public Vector3 planeRight{ get; private set; }
        private Vector3 totalUp;
        private Vector3 totalRight;
        private Vector3 totalNormal;
        private float totalDistance;
        private int planeCount;

        private Rigidbody rb;

        public static int planeTriggerLayerMask = LayerMask.GetMask("Plane Trigger");
        public static int planeTriggerLayer = LayerMask.NameToLayer("Plane Trigger");
    
        public PlaneInfo(Transform t, ref Rigidbody _rb)
        {
            plane = new Plane();
            planeUp = Vector3.zero;
            planeRight = Vector3.zero;
            totalUp = Vector3.zero;
            totalRight = Vector3.zero;
            totalNormal = Vector3.zero;
            totalDistance = 0;
            planeCount = 0;
            rb = _rb;

            SetCurrentPlane(t);
        }

        public void ResetPlaneInfo(Transform t)
        {
            this = new PlaneInfo(t, ref rb);
        }

        private void SetCurrentPlane(Transform t)
        {
            plane = new Plane(t.forward, t.position);
            
            planeUp = t.up;
            planeRight = t.right;            

            AlignToPlane();
        }

        private void ResetCurrentPlane()
        {
            plane = new Plane(totalNormal/planeCount, totalDistance/planeCount);
            
            planeUp = totalUp/planeCount;
            planeRight = totalRight/planeCount;            

            AlignToPlane();
        }
        
        private void AlignToPlane()
        {
            rb.position = plane.ClosestPointOnPlane(rb.position);
            rb.transform.forward = plane.normal;
            //Debug.Log(ToString());
        }

        public void AddPlane(Transform t)
        {

            totalNormal += t.forward;
            totalUp += t.up;
            totalRight += t.right;
            totalDistance += new Plane(t.forward, t.position).distance;
            planeCount++;
            ResetCurrentPlane();
        }

        public void SubtractPlane(Transform t)
        {
            
            totalNormal -= t.forward;
            totalUp -= t.up;
            totalRight -= t.right;
            totalDistance -= new Plane(t.forward, t.position).distance;
            planeCount--;

                if(planeCount != 0)
                {
                    ResetCurrentPlane();
                    //Debug.Log(ToString());
                }
        }

        public Vector3 Vector2To3OnPlane(Vector2 v)
        {
            return (planeRight * v.x) + (planeUp * v.y);
        } 

        public override string ToString()
        {
            return
                  "Plane Count: " + planeCount + "\n"  
                + "Plane Normal(Forward): " + plane.normal + "\n"
                + "Plane Up: " + planeUp + "\n"
                + "Plane Right: " + planeRight + "\n"
                + "Plane Distance: " + plane.distance;
                
        }
    
    }

public class SimpleTranslate : MonoBehaviour
{

    public float speed;

    private Rigidbody rb;

    public PlaneInfo currentPlaneInfo;
    public Vector3 currentSlopeNormal;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        currentPlaneInfo = new PlaneInfo(gameObject.transform, ref rb);
        currentSlopeNormal = Vector3.up;
    }

    public void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.layer == PlaneInfo.planeTriggerLayer) 
        {
            currentPlaneInfo.AddPlane(col.transform);
        }
    }

    public void OnTriggerExit(Collider col)
    {
        if (col.gameObject.layer == PlaneInfo.planeTriggerLayer) 
        {
            currentPlaneInfo.SubtractPlane(col.transform);
        }
    }

    private void SetVelocity(Vector2 rawVelocity) 
    {
        if (rawVelocity == Vector2.zero)
            return;

        Vector3 planeRawVelocity = currentPlaneInfo.Vector2To3OnPlane(rawVelocity);
        Vector3 velocity = planeRawVelocity;//Vector3.ProjectOnPlane(planeRawVelocity, currentSlopeNormal).normalized * rawVelocity.magnitude;

        rb.velocity = velocity;
    }

    // Update is called once per frame
    public void FixedUpdate()
    {

        rb.velocity = Vector3.zero;

        Vector2 direction = Vector2.zero;
        if (Input.GetKey(KeyCode.W))
            direction += Vector2.up;
        if (Input.GetKey(KeyCode.S))
            direction += Vector2.down;
        if (Input.GetKey(KeyCode.A))
            direction += Vector2.left;
        if (Input.GetKey(KeyCode.D))
            direction += Vector2.right;
        direction.Normalize();

        SetVelocity(direction * speed);

        Debug.DrawRay(transform.position,currentPlaneInfo.plane.normal*3, Color.blue);
        Debug.DrawRay(transform.position,currentPlaneInfo.planeRight*3, Color.red);
        Debug.DrawRay(transform.position,currentPlaneInfo.planeUp*3.5f, Color.green);

    }
}
