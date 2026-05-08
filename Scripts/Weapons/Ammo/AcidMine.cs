using FMODUnity;
using System.Runtime.CompilerServices;
using UnityEngine;


public class AcidMine : Poolable<AcidMine>
{
    [SerializeField]
    private MonoBehaviour fmodEmitter;


    private LayerMask targetLayer;
    private Transform target;
    private Acidifier acidifier;
    private Rigidbody rb;
    private float detectionInterval = 0.5f;
    private float timer = 0f;
    private float detectionRadius = 4f;
    private float mineSpeed = 3.5f;
    private WaitForSeconds waitDetectionInterval;
    private Damagable damagable;
    private Collider[] colliders = new Collider[1];

    public void SetTargetLayer(LayerMask val) => targetLayer = val;
    public void SetUser(bool enemy) => fmodEmitter.enabled = enemy;
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        waitDetectionInterval = new WaitForSeconds(detectionInterval); 
        damagable = GetComponent<Damagable>();
        damagable.OnDead.AddListener(Explode);
    }
    
    protected override void OnEnable()
    {
        base.OnEnable();
        target = null;
        damagable.Revive();
    }

    private void Update()
    { 
        timer += Time.deltaTime;
        
        if (timer > detectionInterval)
        { 
            timer = 0f;

            if (target != null)
            {
                rb.linearVelocity = (target.position - transform.position).normalized * mineSpeed;
                return;
            }

            target = null;
            int collisionCount = Physics.OverlapSphereNonAlloc(transform.position, detectionRadius, colliders, targetLayer);

            if (collisionCount > 0)
            {
                target = colliders[0].transform;
            }
        }
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    private void OnDestroy()
    {
        acidifier.Unmounted -= Explode;
    }

    public void SetMother(Acidifier acidifier)
    {
        this.acidifier = acidifier;
        acidifier.Unmounted += Explode;
    }
    
    public void Shoot(Vector3 direction, float force)
    {
        rb.AddForce(direction * force, ForceMode.Impulse);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (Released)
        {
            return;
        }

        Explode();
    }

    private void Explode()
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        transform.rotation = Quaternion.identity;
        acidifier.Explode(transform.position, this);

        if (Released)
        {
            return;
        }

        Release();
    }
}
