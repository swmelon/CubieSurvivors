using Local.Scripts.Extensions;
using UnityEngine;
using System;

public class Bullet<T> : DamagingPoolable<T> where T : Bullet<T>
{
    public Action<Vector3> BulletHit;
    public void SetSpeed(float value) => speed = value;
    public void SetMaxDegreePerSec(float value) => maxDegreePerSecond = value;
    public void SetTarget(Transform value)
    {
        targetDead = false;
        target = value;
        time = targetlessTime;
    }
    public void SetDirection (Vector3 value) => direction = value;
    public void SetTargetlessTime (float value) => targetlessTime = value;
    
    protected float speed;
    protected Vector3 direction;
    
    protected float maxDegreePerSecond;
    protected float distanceToTarget, lastDistanceToTarget;
    protected float targetlessTime = 1f;
    protected float time;
    protected bool targetDead, deathTimerStarted;
    protected Transform target;
    
    
    protected virtual void Awake()
    {
        direction = Vector3.zero;
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        targetDead = false;
        lastDistanceToTarget = float.MaxValue;
        
        deathTimerStarted = false;

        // need change?
        // never?
        // ever?

        time = targetlessTime;
    }

    protected virtual void Update()
    {
        transform.position += Time.deltaTime * speed * direction;
    }

    protected virtual void FixedUpdate()
    {
        if (deathTimerStarted)
        {
            time -= Time.fixedDeltaTime;
            
            if (time <= 0)
            {
                OnBulletHit();
                return;
            }
        }
        
        UpdateBulletDirection();
        
        RaycastHit hit;

        if (Physics.Raycast(transform.position, transform.forward, out hit, 0.5f,
                LayerMaskCash.Obstacle | targetLayer, QueryTriggerInteraction.Ignore))
        {

            if (hit.collider.TryGetComponent(out Damagable damagable))
            {
                OnHitDamagable(hit, damagable);
            }

            OnBulletHit();
        }        
    }

    protected void OnHitDamagable(RaycastHit hit, Damagable damagable)
    {
        damagable.Hit(weapon.ComputeFinalDamage(damage, out bool isCritical), 
            ComputeHitForce(hit.transform.position), isCritical: isCritical);
    }

    protected virtual void UpdateBulletDirection()
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
        
        // check bullet passed target
        distanceToTarget = directionToTarget.magnitude;
        
        if(distanceToTarget > lastDistanceToTarget)
        {
            StartDeathTimer();
        }
        
        lastDistanceToTarget = distanceToTarget;
        
        directionToTarget = directionToTarget.normalized;
        
        // Calculate the rotation to look at the target without banking.
        Quaternion lookRotation = Quaternion.LookRotation(directionToTarget, Vector3.up);

        // Calculate the bank angle (roll) based on the projectile's forward direction and the target direction.
        float bankAngle = Vector3.SignedAngle(transform.forward, directionToTarget, Vector3.up);
        
        // Apply the bank angle to the rotation, creating the new rotation with banking.
        Quaternion bankRotation = Quaternion.Euler(0f, 0f, -bankAngle);
        
        // Combine the look rotation and the bank rotation to get the final direction with banking.
        Quaternion finalRotation = lookRotation * bankRotation;
        
        // Update the bullet's rotation to the finalRotation.
        transform.rotation = Quaternion.RotateTowards(transform.rotation, finalRotation, maxDegreePerSecond * Time.fixedDeltaTime);
        direction = transform.forward;
    }
    
    private void StartDeathTimer()
    {
        if (deathTimerStarted)
        {
            return;
        }
        
        deathTimerStarted = true;
    }

    protected virtual void OnBulletHit()
    {
        BulletHit?.Invoke(transform.position);
        Release();
    }

    protected virtual void OnTargetDead()
    {
        targetDead = true;
        StartDeathTimer();
    }
}
