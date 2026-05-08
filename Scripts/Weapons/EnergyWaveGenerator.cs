using UnityEngine;
using System.Collections.Generic;
using Local.Scripts.Extensions;
using StarterAssets;


public class EnergyWaveGenerator : QuickFirableWeapon<UFloat>
{
    [SerializeField]
    private Transform novaWave;

    [SerializeField]
    private Transform moonWave;

    [SerializeField]
    private CircleRay novaCircleRay;

    [SerializeField]
    private CircleRay moonSemicircleRay;


    private struct UpgradableStat
    {
        public List<float> Damage;
        public List<float> RateOfFire, Range;
        public List<bool> Unlocked;
    }

    private UInt UDamage;
    private UFloat URange;
    private Upgradable<bool> UUltimateUpgrade;

    private AnimationCurve sizeCurve;
    private ICharacterController input;
    private Rigidbody novaWaveRb, moonWaveRb;
    private float time;
    private float moonTime;

    private GameObject dummyWave;
    private Quaternion initialWaveRotation;
    private Enemy enemy;

    [Header("Nova Wave")]
    [SerializeField]
    private float waveRadiusFactor;

    [SerializeField]
    private ParticleSystem novaParticle;


    [Header("Moon Wave")]
    [SerializeField]
    private float moonWaveRadius = 3f;

    [SerializeField]
    private float moonWaveRadiusBoss = 5f;

    [SerializeField]
    [Range(0f, 0.5f)]
    private float moonWaveRadiusRandomness = 0.2f;

    [SerializeField]
    private float moonWaveSpeed = 10f;

    [SerializeField]
    private float moonWaveScaleFactor = 0.75f;

    private float waveMaxRadius, waveScale;
    private float enemySpeed;
    private bool shootSunWaveBetweenNova;

    private float maxMoonWaveRadius;
    private float minMoonWaveRadius;

    protected override void Awake()
    {
        base.Awake();
        
        sizeCurve = novaParticle.sizeOverLifetime.size.curve;
        time = -1f;
        novaCircleRay.SetWeapon(this);
        moonSemicircleRay.SetWeapon(this);
        onMounted = OnMounted;
        onMountedOnPlayer = OnMountedOnPlayer;
        onMountedOnEnemy = OnMountedOnEnemy;
        dummyWave = transform.GetChild(0).gameObject;
        initialWaveRotation = novaWave.localRotation;
        novaWaveRb = novaWave.GetComponent<Rigidbody>();
        moonWaveRb = moonWave.GetComponent<Rigidbody>();

        maxMoonWaveRadius = moonWaveRadius + moonWaveRadiusRandomness;
        minMoonWaveRadius = moonWaveRadius - moonWaveRadiusRandomness;

    }

    private void OnMounted()
    {
        dummyWave.SetActive(false);
        novaWave.localRotation = initialWaveRotation;
        SetTarget();
    }

    private void OnMountedOnPlayer()
    {
        user.GetTransform().TryGetComponent(out input);
        time = 0f;
        moonTime = 0f;
    }

    private void OnMountedOnEnemy()
    {
        user.GetTransform().TryGetComponent(out enemy);
        enemySpeed = enemy.MoveSpeed;
    }
    
    protected override void SetupUpgradables()
    {
        UpgradableStat upgradableStat = UpgradeExtension.ReadUpgradableStat<UpgradableStat>(this);
        InitializeUpgradables(upgradableStat);
    }
    
    private void SetTarget()
    {
        novaCircleRay.SetTargetLayer(mountedOnEnemy ? LayerMaskCash.Player : LayerMaskCash.OnlyEnemy);
    }

    public override void Damage()
    {
        if (time < 0)
        {
            time = 0;
        }
    }
    
    private void InitializeUpgradables(UpgradableStat upgradableStat)
    {
        UDamage = new UInt(upgradableStat.Damage, symbol: symbolContainer.Damage,
            optionText: CardText.DAMAGE);
        URange = new UFloat(upgradableStat.Range, symbol: symbolContainer.Scale,
            optionText: CardText.ATTACK_RANGE);
        UFireWaitTime = new UFloat(upgradableStat.RateOfFire, symbol: symbolContainer.RateOfFire,
            optionText: CardText.RATE_OF_FIRE, useReciprocal: true);
        UUltimateUpgrade = new Upgradable<bool>(upgradableStat.Unlocked, optionText: CardText.ULTIMATE_UPGRADE_NOVAWAVE);

        IUpgradable[] otherUpgradables = {UDamage, URange, UFireWaitTime};
        UUltimateUpgrade.UnlockWhenComplete(otherUpgradables);
        UUltimateUpgrade.UpgradeCompleted += Unlock;
        UUltimateUpgrade.UpgradeReset += Lock;

        Lock();
    }

    private void Unlock()
    {
        moonWave.gameObject.SetActive(true);
    }

    private void Lock()
    {
        moonWave.gameObject.SetActive(false);
    }


    // 추후 ultimate upgrade된 보스가 클리어 어려우면 제한하자
    protected override void FixedUpdate()
    {
        if (time < 0f)
        {
            if (mountedOnEnemy)
            {
                return;
            }

            if (user.TryGetTarget(out target))
            {
                time = 0f;
            }
        }

        if (time == 0f)
        {
            ShootNovaWave();

            // 동기화하기 위해 같이 발사
            if (!mountedOnEnemy && UUltimateUpgrade.Value)
            {
                ShootMoonWave();
                shootSunWaveBetweenNova = false;
                moonTime = 0f;
            }
        }

        novaWave.localScale = URange.Value * Vector3.one;
        waveScale = novaWave.lossyScale.x;

        // 절대 스케일보다 파티클의 radius가 커지거나 작아지는지 확인
        waveMaxRadius = waveScale * waveRadiusFactor;

        // 여기서 waveSpeed는 실제 애니메이션 커브의 사이즈와 파티클의 사이즈가 눈에 보기에 일치하지 않을 때 조절
        float radius = waveRadiusFactor * sizeCurve.Evaluate(time/UFireWaitTime.Value) * waveMaxRadius;

        if (radius > waveMaxRadius)
        {
            radius = 0;
        }

        novaCircleRay.Radius = radius;
        
        time += Time.fixedDeltaTime;
        moonTime += Time.fixedDeltaTime;

        if (!mountedOnEnemy && moonTime >= 0.5f * UFireWaitTime.Value && !shootSunWaveBetweenNova && UUltimateUpgrade.Value)
        {
            ShootMoonWave();
            shootSunWaveBetweenNova = true;
            moonTime = 0f;
        }
        
        if (time >= UFireWaitTime.Value)
        {
            user.TryGetTarget(out Transform target);

            time = ReferenceEquals(target,  null) ? -1f : 0f;

            if (mountedOnEnemy)
            {
                time = -1f;
            }

            novaCircleRay.transform.SetPositionAndRotation(transform.position, initialWaveRotation);
            novaCircleRay.transform.SetParent(transform);
            novaWave.gameObject.SetActive(false);
        }
    }

    private void ShootNovaWave()
    {
        novaWave.gameObject.SetActive(true);

        if (!mountedOnEnemy)
        {
            novaWaveRb.linearVelocity = input.GetDirection() * input.GetSpeed();
        }
        else
        {
            novaWaveRb.linearVelocity = transform.forward * enemySpeed;
        }

        novaCircleRay.SetDamage(UDamage.Value);
        novaWave.parent = null;
        novaWave.rotation = initialWaveRotation;

        FMODAudioManager.instance.PlayOneShot(SFXTags.NovaWave, transform.position);
    }

    public void ShootMoonWave()
    {
        if (!user.TryGetTarget(out target))
        {
            return;
        }

        moonWave.gameObject.SetActive(false);
        moonWave.gameObject.SetActive(true);

        moonSemicircleRay.SetDamage((int)(0.5f * UDamage.Value));
        moonSemicircleRay.SetTargetLayer(mountedOnEnemy ? LayerMaskCash.Player : LayerMaskCash.OnlyEnemy);
        moonSemicircleRay.SetExpandHeight(mountedOnEnemy ? 0.2f : 0.5f);

        moonWave.parent = null;
        moonWave.position = transform.position;

        float randomizedRadius = GetMoonWaveRadius();

        moonWave.localScale = randomizedRadius * moonWaveScaleFactor * Vector3.one;


        Vector3 directionToTarget = target.position - transform.position;
        directionToTarget.y = 0f;
        directionToTarget.Normalize();

        float yRotationToTarget = Quaternion.LookRotation(directionToTarget).eulerAngles.y;

        Quaternion rotation = Quaternion.Euler(0f, yRotationToTarget, 0f);


        moonWave.rotation = rotation;
        moonSemicircleRay.SetYRotationAndRadius(rotation, randomizedRadius);

        moonWaveRb.linearVelocity = directionToTarget * moonWaveSpeed;

        FMODAudioManager.instance.PlayOneShot(SFXTags.MoonWave, transform.position);


    }

    public override void BeItem()
    {
        dummyWave.SetActive(true);
    }

    public override void OnUnmounted()
    {
        time = -1f;
        novaCircleRay.transform.SetParent(transform);
        novaCircleRay.transform.position = transform.position;
        novaWave.gameObject.SetActive(false);
        novaWave.localRotation = initialWaveRotation;
    }

    private float GetMoonWaveRadius()
    {
        return mountedOnEnemy ? moonWaveRadiusBoss : Random.Range(minMoonWaveRadius, maxMoonWaveRadius);
    }

}