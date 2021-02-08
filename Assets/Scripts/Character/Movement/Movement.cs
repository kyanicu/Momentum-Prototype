using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Movement : MonoBehaviour, IDynamicPlaneConstrainable
{
    protected struct KinematicPath
    {
        public Vector3 velocity;
        
        public Vector3 velocityAfter;

        public Coroutine timer;
                
    }

    public virtual Vector3 position { get { return transform.position; } set { if (kinematicPath == null) transform.position = value; } }
    public virtual Quaternion rotation { get { return transform.rotation; } set { transform.rotation = value; } }
    public abstract Vector3 velocity { get; set; }

    public abstract bool usePlaneBreakers { get; }

    public abstract void HandlePlaneSettingExtra(Plane plane);
    public abstract bool PlaneBreakValid();

    protected KinematicPath? kinematicPath = null;

    public virtual void ZeroVelocity()
    {
        if (kinematicPath != null)
            return;
            
        velocity = Vector3.zero;
    }

    public virtual void SetKinematicPath(Vector3 vel, float time)
    {
        if (kinematicPath != null)
            EndKinematicPath();

        KinematicPath kp = new KinematicPath 
        { 
            velocity = vel,
            velocityAfter = velocity,
            timer = StartCoroutine(KinematicPathCoroutine(time)),
        };
        
        kinematicPath = kp;

        velocity = vel;
    }

    public virtual IEnumerator KinematicPathCoroutine(float time)
    {
        yield return new WaitForSeconds(time);
        EndKinematicPath();
    }

    public virtual void EndKinematicPath()
    {
        StopCoroutine(kinematicPath.Value.timer);
        velocity = kinematicPath.Value.velocityAfter;
        kinematicPath = null;
    }

    public virtual void AddImpulse(Vector3 impulse)
    {
        if (kinematicPath != null)
            return;

        velocity += impulse;
    }

    public virtual void AddImpulseAtPoint(Vector3 impulse, Vector3 point)
    {
        if (kinematicPath != null)
            return;

        velocity += impulse;
    }
}
