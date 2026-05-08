using Local.Scripts.Extensions;
using System;
using UnityEngine;


public class LaserBullet : Bullet<LaserBullet>, IColorable
{
    [SerializeField]
    private GridSystemChannelSO gridSystemChannel;

    [SerializeField]
    private SFXTags laserSoundTag;
    
    [SerializeField]
    private ParticleSystem mainPS, tailPS;
    
    private ParticleSystem.ColorOverLifetimeModule mainColorOverLifetimeModule, tailColorOverLifetimeModule;
    
    private Color color;
    private static int reflectionMax = 2;
    private int reflectionCount;
    private Transform prevHitTransform;
    private bool damaged = false;

    public static void SetMaxReflection(int value) => reflectionMax = value;


    protected override void Awake()
    {
        base.Awake();
        mainColorOverLifetimeModule = mainPS.colorOverLifetime;
        tailColorOverLifetimeModule = tailPS.colorOverLifetime;
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        reflectionCount = reflectionMax;
        prevHitTransform = null;
        FMODAudioManager.instance.PlayOneShot(laserSoundTag, gameObject);
    }

    public void SetColor(Color color)
    {
        if (this.color != color)
        {
            this.color = color;
            Gradient mainGrad = new Gradient();
            mainGrad.SetKeys(
                new GradientColorKey[]
                {
                    new GradientColorKey(Color.white, 0.0f), 
                    new GradientColorKey(this.color, 1.0f)
                }, 
                new GradientAlphaKey[]
                {
                    new GradientAlphaKey(1.0f, 0.0f), 
                    new GradientAlphaKey(0.0f, 1.0f)
                });
            Gradient tailGrad = new Gradient();
            tailGrad.SetKeys(
                new GradientColorKey[]
                {
                    new GradientColorKey(this.color, 0.0f),
                    new GradientColorKey(Color.white, 1.0f) 
                }, 
                new GradientAlphaKey[]
                {
                    new GradientAlphaKey(1.0f, 0.0f), 
                    new GradientAlphaKey(0.0f, 1.0f)
                });
            mainColorOverLifetimeModule.color = mainGrad;
            tailColorOverLifetimeModule.color = tailGrad;
        }
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
            }

            OnBulletHit();

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
        if (reflectionCount <= 0)
        {
            base.OnTargetDead();
            return;
        }

        if (!gridSystemChannel.TryGetEnemyNearby(transform.position, out Transform enemy))
        {
            target = enemy;
            reflectionCount--;
            return;
        }
    }
}
