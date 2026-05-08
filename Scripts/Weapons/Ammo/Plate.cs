using Local.Scripts.Extensions;
using System;
using UnityEngine;


public class Plate : Bullet<Plate>
{
    [SerializeField]
    private AnimationCurve maxAngleCurve;

    [SerializeField]
    private AnimationCurve maxAngleCurveEnemy;

    [SerializeField]
    private float minimumDegreeDistance = 10f;

    [SerializeField]
    private GridSystemChannelSO gridSystemChannel;

    [SerializeField]
    private SFXTags laserSoundTag;
    
    private ParticleSystem.ColorOverLifetimeModule mainColorOverLifetimeModule, tailColorOverLifetimeModule;
    
    private Color color;
    private static int reflectionMax = 2;
    private int reflectionCount;
    private Transform prevHitTransform;
    private bool damaged = false;
    private Vector3 initialScale;
    private bool usedbyPlayer;

    public static void SetMaxReflection(int value) => reflectionMax = value;
    public void SetScale(float value) => transform.localScale = value * initialScale;

    public void SetUser(bool value) => usedbyPlayer = value;

    protected override void Awake()
    {
        base.Awake();
        initialScale = transform.localScale;
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        reflectionCount = reflectionMax;
        prevHitTransform = null;
        FMODAudioManager.instance.PlayOneShot(laserSoundTag, gameObject);
    }

    protected override void UpdateBulletDirection()
    {
        if (targetDead)
        {
            return;
        }

        if (target == null || target.gameObject.activeSelf == false)
        {
            OnTargetDead();
            return;
        }

        // Calculate the direction from the bullet's position to the target's position.
        Vector3 directionToTarget = target.position - transform.position;

        float angleToTarget = Vector3.Angle(transform.forward, directionToTarget);

        directionToTarget = directionToTarget.normalized;

        // Calculate the rotation to look at the target without banking.
        Quaternion lookRotation = Quaternion.LookRotation(directionToTarget, Vector3.up);

        // Calculate the bank angle (roll) based on the projectile's forward direction and the target direction.
        float bankAngle = Vector3.SignedAngle(transform.forward, directionToTarget, Vector3.up);

        // Apply the bank angle to the rotation, creating the new rotation with banking.
        Quaternion bankRotation = Quaternion.Euler(0f, 0f, -bankAngle);

        // Combine the look rotation and the bank rotation to get the final direction with banking.
        Quaternion finalRotation = lookRotation * bankRotation;


        

        float maxDegree = usedbyPlayer ? maxAngleCurve.Evaluate(angleToTarget / 180f) * maxDegreePerSecond : maxAngleCurveEnemy.Evaluate(angleToTarget / 180f) * maxDegreePerSecond;


        // Update the bullet's rotation to the finalRotation.
        transform.rotation = Quaternion.RotateTowards(transform.rotation, finalRotation, maxDegree * Time.fixedDeltaTime);
        direction = transform.forward;
    }

    protected override void FixedUpdate()
    {
        if (deathTimerStarted)
        {
            time -= Time.fixedDeltaTime;

            if (time <= 0)
            {
                Release();
                return;
            }
        }

        UpdateBulletDirection();

        RaycastHit hit;

        if (Physics.Raycast(transform.position, transform.forward, out hit, 0.5f,
                LayerMaskCash.Obstacle | targetLayer, QueryTriggerInteraction.Ignore))
        {
            if (ReferenceEquals(hit.transform, prevHitTransform))
            {
                return;
            }

            damaged = false;

            if (hit.collider.TryGetComponent(out Damagable damagable))
            {
                OnHitDamagable(hit, damagable);
                damaged = true;
                prevHitTransform = hit.transform;
                Vector3 reflection = Vector3.Reflect(direction, hit.normal);
                reflection.y = 0;
                direction = reflection.normalized;
            }
            else
            {
                OnBulletHit();
            }


        }

        Debug.DrawRay(transform.position, direction, Color.blue);
    }


    protected override void OnBulletHit()
    {
        if (target == null || !weapon.UsedByPlayer() || !damaged || reflectionCount <= 0 
            || !gridSystemChannel.TryGetEnemyNearby(target, out Transform enemy))
        {
            base.OnBulletHit();
            return;
        }

        target = enemy;
        reflectionCount--;
        return;
    }

    protected override void OnTargetDead()
    {
        if (!gridSystemChannel.TryGetEnemyNearby(transform.position, out Transform enemy))
        {
            target = enemy;
            reflectionCount--;
            return;
        }

        base.OnTargetDead();
    }

    public void ReflectNow(Vector3 targetPosition, float bulletSpeed, int bulletDamage)
    {
        damage = bulletDamage;
        speed = bulletSpeed;
        reflectionCount = reflectionMax;


        Vector3 targetDirection = (targetPosition - transform.position).normalized;

        // Directly set the bullet's direction towards the target
        direction = targetDirection;

        transform.rotation = Quaternion.LookRotation(direction);
        maxDegreePerSecond = 0f;
    }
}
