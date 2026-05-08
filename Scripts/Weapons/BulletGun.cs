using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using Local.Scripts.Extensions;
using UnityEngine.Pool;
using UnityEngine.Serialization;
using UnityEngine.UIElements;


public class BulletGun<T> : QuickFirableWeapon<UWaitForSeconds>, ILockOnWeapon where T : Bullet<T>
{
    [SerializeField]
    protected float bulletSpeed = 100f;

    [SerializeField] 
    protected Transform rotatingPart;

    [SerializeField] 
    protected T bulletPrefab;

    [SerializeField] 
    private Sprite rangeUpgradeIcon;

    [SerializeField]
    private OnePureEffectSpawner bulletHitEffect;
    
    protected struct UpgradableStat
    {
        public List<float> Damage, ExplosiveDamage, ExplosiveRange;
        public List<float> RateOfFire, Range;
        public List<int> Color, NumProjectiles;
        public List<bool> Explosion;
        public List<bool> Unlocked;
    }
    
    /// <summary>
    /// Upgradable Options
    /// </summary>
    protected UInt UDamage;
    protected UFloat URange;
    
    
    /// <summary>
    /// Dynamic Mesh
    /// </summary>
    [SerializeField]
    private bool useDynamicMesh = false;
    private DynamicMesh[] dynamicMeshes;
    
    /// <summary>
    /// Object Pooling
    /// </summary>
    protected IObjectPool<T> bulletPool;
    
    //LockOnWeapon
    protected float bulletMaxDegreePerSec;

    protected bool lockOnMode;
    protected float lockOnModeBulletSpeed;

    protected override void Awake()
    {
        base.Awake();
        bulletPool = new ObjectPool<T>(CreateBullet, OnGetBullet, OnReleaseBullet, OnDestroyBullet, maxSize:10);
        target = null;
        onMountedOnPlayer = OnMountedOnPlayer;
    }
    
    protected virtual void OnMountedOnPlayer()
    {
        StartCoroutine(Fire());
        SetLockOnMode(true);
        
        // When mounted on player, bulletSpeed is same whenever lockOnMode is true or false.
        lockOnModeBulletSpeed = bulletSpeed;
    }

    protected override void SetupUpgradables()
    {
        UpgradableStat upgradableStat = UpgradeExtension.ReadUpgradableStat<UpgradableStat>(this);
        InitializeUpgradables(upgradableStat);
    }

    public override void Damage()
    {
        bulletPool.Get();
    }

    protected virtual void InitializeUpgradables(UpgradableStat upgradableStat)
    {
        UDamage = new UInt(upgradableStat.Damage, symbol: symbolContainer.Damage,
            optionText: CardText.DAMAGE);
        URange = new UFloat(upgradableStat.Range, symbol: symbolContainer.Range,
            optionText: CardText.RANGE);
        UFireWaitTime = new UWaitForSeconds(upgradableStat.RateOfFire, symbol: symbolContainer.RateOfFire,
            optionText: CardText.RATE_OF_FIRE);

        if (!useDynamicMesh)
        {
            return;
        }

        dynamicMeshes = GetComponentsInChildren<DynamicMesh>();
        
        foreach(DynamicMesh dynamicMesh in dynamicMeshes)
        {
            dynamicMesh.LinkTo(UDamage);
        }
    }
    
    protected override void FixedUpdate()
    {
        if (mountedOnEnemy)
        {
            user.TryGetTarget(out target);
        }
        else if (user.TryGetTarget(out Transform usersTarget, out float distance) && 
            (distance <= URange.Value))
        {
            target = usersTarget;
            
        }
        else
        {
            target = null;
        }
        
        RotateTowardTarget();
    }

    public override void OnUnmounted()
    {
        StopAllCoroutines();
    }

    protected virtual void RotateTowardTarget()
    {
        if (!lockOnMode)
        {
            rotatingPart.localRotation = Quaternion.identity;
            return;
        }
        
        if (target != null)
        {
            rotatingPart.rotation = Quaternion.LookRotation(target.transform.position - rotatingPart.position) *
                                    transform.parent.localRotation;
            Debug.DrawRay(rotatingPart.position, rotatingPart.forward*10, Color.yellow);
            return;
        }
    }

    protected virtual IEnumerator Fire()
    {
        while(true)
        {
            if (!ReferenceEquals(target, null))
            { 
                bulletPool.Get();
            }

            yield return UFireWaitTime.Value;
        }
    }
    
    public void SetLockOnMode(bool useLockOnMode, float maxDegreePerSec = float.MaxValue, float projectileSpeed = 5f)
    {
        lockOnMode = useLockOnMode;
        bulletMaxDegreePerSec = maxDegreePerSec;
        lockOnModeBulletSpeed = projectileSpeed;
    }
    
    protected virtual T CreateBullet()
    {
        T bullet = Instantiate(bulletPrefab.gameObject, rotatingPart.position , rotatingPart.rotation).GetComponent<T>();
        bullet.SetManagedPool(bulletPool);
        bullet.SetWeapon(this);
        bullet.BulletHit += OnBulletHit;
        return bullet;
    }

    protected virtual void OnGetBullet(T bullet)
    {
        bullet.SetDamage(UDamage.Value);
        
        if(!ReferenceEquals(target, null))
        {
            bullet.SetTarget(target);
        }
        
        bullet.SetTargetLayer(mountedOnEnemy ? LayerMaskCash.Player : LayerMaskCash.Enemy);
        bullet.SetMaxDegreePerSec(lockOnMode ? bulletMaxDegreePerSec : 0f);
        bullet.SetSpeed(lockOnMode ? lockOnModeBulletSpeed : bulletSpeed);
        bullet.SetDirection(rotatingPart.forward);
        bullet.transform.SetPositionAndRotation(rotatingPart.position, rotatingPart.rotation);
        
        // activate�ϱ� ���� ��ġ�� �����ؾ��Ѵ�.
        bullet.gameObject.SetActive(true);
    }

    protected virtual void OnReleaseBullet(T bullet)
    {
        bullet.gameObject.SetActive(false);
    }

    protected virtual void OnDestroyBullet(T bullet)
    {
        bullet.BulletHit -= OnBulletHit;
        Destroy(bullet.gameObject);
    }

    [SerializeField]
    private float _itemDropOffset = 0.2f;

    public override void BeItem()
    {
        transform.localPosition -= _itemDropOffset * Vector3.up;
    }

    protected virtual void OnBulletHit(Vector3 hitPoint)
    {
        bulletHitEffect.Spawn().transform.position = hitPoint;
    }
}

public class ColorableBulletGun<T> :  BulletGun<T> where T : Bullet<T>, IColorable
{
    private Upgradable<Color> UColor;
    
    protected override void InitializeUpgradables(UpgradableStat upgradableStat)
    {
        base.InitializeUpgradables(upgradableStat);
        UColor = new Upgradable<Color>(upgradableStat.Color.ToColors());
        UColor.LinkTo(UDamage);
    }

    protected override void OnGetBullet(T bullet)
    {
        base.OnGetBullet(bullet);
        bullet.SetColor(UColor.Value);
    }
}

public class ExplosiveBulletGun<T> : BulletGun<T> where T : Bullet<T>
{
    [SerializeField]
    private Explosive explosivePrefab;
    
    protected Upgradable<int> UExplosiveDamage;
    private IObjectPool<Explosive> explosivePool;


    protected override void Awake()
    {
        base.Awake();
        explosivePool = new ObjectPool<Explosive>(CreateExplosive, OnGetExplosive, OnReleaseExplosive, OnDestroyExplosive, maxSize:10);
    }
    
    protected override void InitializeUpgradables(UpgradableStat upgradableStat)
    {
        base.InitializeUpgradables(upgradableStat);
        UExplosiveDamage = new UInt(upgradableStat.ExplosiveDamage, symbol: symbolContainer.ExplosionDamage , 
            optionText: CardText.EXPLOSION_DAMAGE);
    }

    protected override void OnBulletHit(Vector3 hitPoint)
    {
        Explosive explosive = explosivePool.Get();
        explosive.gameObject.transform.position = hitPoint;
        explosive.Explode();
    }

    private Explosive CreateExplosive()
    {
        Explosive explosive = Instantiate(explosivePrefab.gameObject).GetComponent<Explosive>();
        explosive.SetManagedPool(explosivePool);
        explosive.SetWeapon(this);
        return explosive;
    }

    protected virtual void OnGetExplosive(Explosive explosive)
    {
        explosive.gameObject.SetActive(true);
        explosive.SetDamage(UExplosiveDamage.Value);
        explosive.SetTargetLayer(mountedOnEnemy ? LayerMaskCash.PlayerAndEnemy : LayerMaskCash.Enemy);
    }

    private void OnReleaseExplosive(Explosive explosive)
    {
        explosive.gameObject.SetActive(false);
    }

    private void OnDestroyExplosive(Explosive explosive)
    {
        Destroy(explosive.gameObject);
    }
}

