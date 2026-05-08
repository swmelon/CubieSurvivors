using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Local.Scripts.Extensions;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Pool;


public class PoisonReflector : QuickFirableWeapon<UWaitForSeconds>
{
    [SerializeField]
    private PoisonBullet bulletPrefab;

    [SerializeField] 
    private Transform muzzleMiddle, muzzleLeft, muzzleRight;
    
    private Animator animator;
    private int shootId;

    CustomObjectPool<PoisonBullet> poisonBulletPool;
    
    private struct UpgradableStat
    {
        public List<float> Damage, MaxBounces;
        public List<float> RateOfFire;
        public List<bool> Unlocked;
    }

    private UInt UMaxBounces;
    private UInt UDamage;
    private Upgradable<bool> UUltimateUpgrade;
    
    [SerializeField]
    private Color playerBulletColor, enemyBulletColor;
    
    private Color bulletColor;
    
    protected override void Awake()
    {
        base.Awake();
        
        poisonBulletPool = new CustomObjectPool<PoisonBullet>(CreatePoisonBullet, OnGetPoisonBullet, OnReleasePoisonBullet, OnDestroyPoisonBullet, maxSize:40);
        animator = GetComponent<Animator>();
        shootId = Animator.StringToHash("TrShoot");
        onMountedOnPlayer = OnMountedOnPlayer;
        onMountedOnEnemy = OnMountedOnEnemy;
    }

    private void OnMountedOnPlayer()
    {
        bulletColor = playerBulletColor;
        StartCoroutine(Fire());
    }
    
    private void OnMountedOnEnemy()
    {
        bulletColor = enemyBulletColor;
        StopAllCoroutines();
    }

    public override void OnUnmounted()
    {
    }

    protected override void SetupUpgradables()
    {
        UpgradableStat upgradableStat = UpgradeExtension.ReadUpgradableStat<UpgradableStat>(this);
        InitializeUpgradables(upgradableStat);
    }
    
    public override void Damage()
    {
        ShootBullets();
    }

    public void ReflectiveStrike()
    {
        HashSet<PoisonBullet> bullets = poisonBulletPool.GetActiveItems();
        
        foreach (var bullet in bullets)
        {
            bullet.ReflectNow(target.position, 30f, UDamage.Value * bullets.Count);
        }
    }
    
    private void InitializeUpgradables(UpgradableStat upgradableStat)
    {
        UDamage = new UInt(upgradableStat.Damage);
        UUltimateUpgrade = new Upgradable<bool>(upgradableStat.Unlocked);
        UMaxBounces = new UInt(upgradableStat.MaxBounces);
        UFireWaitTime = new UWaitForSeconds(upgradableStat.RateOfFire);
        
        UUltimateUpgrade.UnlockWhenComplete(UDamage);
        UUltimateUpgrade.UpgradeCompleted += Unlock;
    }
    
    private void Unlock()
    {
        muzzleLeft.gameObject.SetActive(true);
        muzzleRight.gameObject.SetActive(true);
    }

    private IEnumerator Fire()
    {
        while (true)
        {
            if (!ReferenceEquals(target, null))
            {
                ShootBullets();
            }
            yield return UFireWaitTime.Value;
        }
    }

    private void ShootBullets()
    {
        animator.ResetTrigger(shootId);
        animator.SetTrigger(shootId);
        
        GetBulletAndSetup(muzzleMiddle);

        if (UUltimateUpgrade.Value)
        {
            GetBulletAndSetup(muzzleLeft);
            GetBulletAndSetup(muzzleRight);
        }
    }

    private void GetBulletAndSetup(Transform muzzleTransform)
    {
        PoisonBullet poisonBullet = poisonBulletPool.Get();
        
        poisonBullet.SetDamage(UDamage.Value);
        poisonBullet.SetMaxBounces(UMaxBounces.Value);
        poisonBullet.SetDirection(muzzleTransform.forward);
        poisonBullet.SetSpeed(10f);
        poisonBullet.SetTargetLayer(mountedOnEnemy ? LayerMaskCash.Player : LayerMaskCash.Enemy); 
        poisonBullet.SetColor(bulletColor);
        poisonBullet.transform.position = muzzleTransform.position;
    }
    
    private PoisonBullet CreatePoisonBullet()
    {
        PoisonBullet poisonBullet = Instantiate(bulletPrefab.gameObject).GetComponent<PoisonBullet>();
        poisonBullet.SetManagedPool(poisonBulletPool);
        poisonBullet.SetWeapon(this);
        return poisonBullet;
    }

    private void OnGetPoisonBullet(PoisonBullet poisonBullet)
    {
        poisonBullet.gameObject.SetActive(true);
    }

    private void OnReleasePoisonBullet(PoisonBullet poisonBullet)
    {
        poisonBullet.gameObject.SetActive(false);
    }

    private void OnDestroyPoisonBullet(PoisonBullet poisonBullet)
    {
        Destroy(poisonBullet.gameObject);
    }

    public override void BeItem()
    {
        transform.localPosition -= Vector3.up * 0.2f + Vector3.forward * 0.1f;
    }
}
