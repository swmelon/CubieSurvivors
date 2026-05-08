using Local.Scripts.Extensions;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

public class DamageOverTimeTrigger : MonoBehaviour
{
    [SerializeField] private bool startDamageOnStart = false;
    [SerializeField][Range(0.05f, 0.3f)] private float damageRate = 0.1f;
    [FormerlySerializedAs("rateOfFire")][SerializeField] private float damagePeriod = 1f;

    private WaitForSeconds wait;
    private Collider[] collidersInTrigger = new Collider[8];

    [SerializeField]
    private LayerMask targetLayer;

    private ColliderType colliderType = ColliderType.None;

    // Cached variables
    private Vector3 cachedBoxSize;
    private float cachedSphereRadius;
    private Vector3 cachedCapsulePoint1;
    private Vector3 cachedCapsulePoint2;
    private float cachedCapsuleRadius;
    private bool stopDamage = false;



    private void Awake()
    {
        wait = new WaitForSeconds(damagePeriod);
       
        if (targetLayer == 0)
        {
            targetLayer = LayerMaskCash.PlayerAndEnemy;
        }
        
        CacheColliderDetails();
    }
    
    private void Start()
    {
        if (startDamageOnStart)
        {
            StartDamage();
        }
    }

    private void CacheColliderDetails()
    {
        Collider collider = GetComponent<Collider>();
        if (collider is BoxCollider box)
        {
            colliderType = ColliderType.Box;
            cachedBoxSize = box.size / 2;  // Divided by 2 because OverlapBox uses half extents
        }
        else if (collider is SphereCollider sphere)
        {
            colliderType = ColliderType.Sphere;
            cachedSphereRadius = sphere.radius;
        }
        else if (collider is CapsuleCollider capsule)
        {
            colliderType = ColliderType.Capsule;
            cachedCapsuleRadius = capsule.radius;
            cachedCapsulePoint1 = transform.position + transform.up * (capsule.height / 2 - capsule.radius);
            cachedCapsulePoint2 = transform.position - transform.up * (capsule.height / 2 - capsule.radius);
        }
        else
        {
            Debug.Log("No recognized collider attached.");
            colliderType = ColliderType.None;
        }
    }

    public void StartDamage()
    {
        stopDamage = false;
        StartCoroutine(Damage());
    }

    public void SetTarget(LayerMask layerMask)
    {
        targetLayer = layerMask;
    }

    public void StopDamage()
    {
        stopDamage = true;
    }

    private IEnumerator Damage()
    {
        while (!stopDamage)
        {
            yield return wait;
            CheckDamagable();
        }
    }

    private void CheckDamagable()
    {
        int num = 0;
        switch (colliderType)
        {
            case ColliderType.Box:
                num = Physics.OverlapBoxNonAlloc(transform.position, cachedBoxSize, collidersInTrigger, transform.rotation, targetLayer);
                break;
            case ColliderType.Sphere:
                num = Physics.OverlapSphereNonAlloc(transform.position, cachedSphereRadius, collidersInTrigger, targetLayer);
                break;
            case ColliderType.Capsule:
                num = Physics.OverlapCapsuleNonAlloc(cachedCapsulePoint1, cachedCapsulePoint2, cachedCapsuleRadius, collidersInTrigger, targetLayer);
                break;
        }

        for (int i = 0; i < num; i++)
        {
            if (collidersInTrigger[i].TryGetComponent(out Damagable damagable))
            {
                damagable.HitRate(damageRate);
            }
        }
    }
}
