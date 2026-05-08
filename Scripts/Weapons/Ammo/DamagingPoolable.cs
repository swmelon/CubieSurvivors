
using UnityEngine;

public class DamagingPoolable<T> : Poolable<T> where T : DamagingPoolable<T>
{
    public void SetDamage (int value) => damage = value;
    public void SetWeapon (IWeapon value) => weapon = value;
    public void SetTargetLayer (LayerMask value) => targetLayer = value;

    [SerializeField][Range(0f, 1f)]
    protected float hitForceMultiplier = 0.5f;
    
    protected int damage;
    protected IWeapon weapon;
    protected LayerMask targetLayer;
    
    protected Vector3 ComputeHitForce(Vector3 hitPoint)
    {
        Vector3 force = hitPoint - transform.position;
        force.y = 0;
        force.Normalize();
        return force * hitForceMultiplier;
    }
    
    protected Vector3 ComputeHitForce(Vector3 hitPoint, out float distance)
    {
        Vector3 force = hitPoint - transform.position;
        distance = force.magnitude;
        force.y = 0;
        force.Normalize();
        return force * hitForceMultiplier;
    }
}
