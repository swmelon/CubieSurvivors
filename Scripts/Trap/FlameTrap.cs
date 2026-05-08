using Local.Scripts.Extensions;
using UnityEngine;

public class FlameTrap : Trap
{
    [SerializeField] 
    private GameObject flames;

    [SerializeField]
    private int damage = 10;

    [SerializeField]
    private float damageInterval = 0.2f;

    private bool isOn;
    private float damageTimer;
    private RayPoint[] rayPoints;
    private RaycastHit[] raycastHits = new RaycastHit[32];

    protected override void Awake()
    {
        base.Awake();
        rayPoints = GetComponentsInChildren<RayPoint>();
    }

    protected override void On()
    {
        flames.SetActive(true);
        isOn = true;
    }
    protected override void Off()
    {
        flames.SetActive(false);
        isOn = false;
    }

    private void FixedUpdate()
    {
        damageTimer += Time.fixedDeltaTime; 

        if (isOn && damageTimer >= damageInterval)
        {
            damageTimer = 0f;
            foreach (RayPoint rayPoint in rayPoints)
            {
                int count = Physics.BoxCastNonAlloc(rayPoint.Origin, Vector3.one * 0.5f, rayPoint.transform.forward, 
                    raycastHits, Quaternion.identity, rayPoint.rayLength, LayerMaskCash.PlayerAndEnemy);
                
                for (int i = 0; i < count; i++)
                {
                    if (raycastHits[i].collider.TryGetComponent(out Damagable damageable))
                    {
                        damageable.Hit(damage);
                    }
                }
            }
        }
    }
}
