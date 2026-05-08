
using System.Collections;
using System.Collections.Generic;
using Local.Scripts.Extensions;
using UnityEngine;
using UnityEngine.Pool;
using System;

public class Acidifier : QuickFirableWeapon<UWaitForSeconds>
{
    public event Action Unmounted;
    
    [SerializeField]
    private AcidMine bulletPrefab;

    [SerializeField]
    private Explosive explosivePrefab;

    [SerializeField]
    private Transform muzzleMiddle, muzzleLeft, muzzleRight;

    private Animator animator;
    private int shootId;

    private IObjectPool<AcidMine> poisonBulletPool;
    private ObjectPool<Explosive> explosivePool;
    private int playerLayerIndex, enemyLayerIndex;
    private Vector3 instancePosition;
    private Quaternion identity = Quaternion.identity;


    private struct UpgradableStat
    {
        public List<float> Damage, ExplosionRange;
        public List<float> RateOfFire;
        public List<bool> Unlocked;
    }

    private UInt UDamage;
    private UFloat UExplosionRange;
    private Upgradable<bool> UUltimateUpgrade;

    [SerializeField]
    private Color playerBulletColor, enemyBulletColor;

    private Color bulletColor;

    protected override void Awake()
    {
        base.Awake();

        poisonBulletPool = new CustomObjectPool<AcidMine>(CreatePoisonBullet, OnGetPoisonBullet, OnReleasePoisonBullet, OnDestroyPoisonBullet, maxSize: 40);
        explosivePool = new ObjectPool<Explosive>(CreateExplosive, OnGetExplosive, OnReleaseExplosive, OnDestroyExplosive, maxSize: 10);
        animator = GetComponent<Animator>();
        shootId = Animator.StringToHash("TrShoot");
        onMountedOnPlayer = OnMountedOnPlayer;
        onMountedOnEnemy = OnMountedOnEnemy;
        playerLayerIndex = LayerMask.NameToLayer("Player");
        enemyLayerIndex = LayerMask.NameToLayer("Enemy");
    }

    public override void OnUnmounted()
    {
        StopAllCoroutines();
        Unmounted?.Invoke();
    }

    private void OnMountedOnPlayer()
    {
        bulletColor = playerBulletColor;
        StartCoroutine(Fire());
    }

    private void OnMountedOnEnemy()
    {
        bulletColor = enemyBulletColor;
        poisonBulletPool.Clear();
        explosivePool.Clear();
        StopAllCoroutines();
    }

    protected override void SetupUpgradables()
    {
        UpgradableStat upgradableStat = UpgradeExtension.ReadUpgradableStat<UpgradableStat>(this);
        InitializeUpgradables(upgradableStat);
    }

    public override void Damage()
    {
        ShootMines();
    }

    private void InitializeUpgradables(UpgradableStat upgradableStat)
    {
        UDamage = new UInt(upgradableStat.Damage, symbol: symbolContainer.ExplosionDamage, optionText: CardText.EXPLOSION_DAMAGE);
        UExplosionRange = new UFloat(upgradableStat.ExplosionRange, symbol: symbolContainer.Scale, optionText: CardText.EXPLOSION_RADIUS);
        UUltimateUpgrade = new Upgradable<bool>(upgradableStat.Unlocked, optionText: CardText.THREE_X);
        UFireWaitTime = new UWaitForSeconds(upgradableStat.RateOfFire, symbol: symbolContainer.RateOfFire, optionText: CardText.RATE_OF_FIRE);

        IUpgradable[] otherUpgradables = { UDamage, UExplosionRange, UFireWaitTime };
        UUltimateUpgrade.UnlockWhenComplete(otherUpgradables);
        UUltimateUpgrade.UpgradeCompleted += Unlock;
        UUltimateUpgrade.UpgradeReset += Lock;
    }

    private void Unlock()
    {
        muzzleLeft.parent.gameObject.SetActive(true);
        muzzleRight.parent.gameObject.SetActive(true);
    }

    private void Lock()
    {
        muzzleLeft.parent.gameObject.SetActive(false);
        muzzleRight.parent.gameObject.SetActive(false);
    }

    private IEnumerator Fire()
    {
        while (true)
        {
            if (!ReferenceEquals(target, null))
            {
                ShootMines();
            }
            yield return UFireWaitTime.Value;
        }
    }

    private void ShootMines()
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

    public void Explode(Vector3 position, AcidMine mine)
    {
        instancePosition = position;
        Explosive explosive = explosivePool.Get();
        explosive.transform.position = position;
        explosive.Explode();
    }


    private void GetBulletAndSetup(Transform muzzleTransform)
    {
        instancePosition = muzzleTransform.position;
        AcidMine acidMine = poisonBulletPool.Get();

        if (mountedOnEnemy)
        {
            acidMine.gameObject.layer = enemyLayerIndex;
        }
        else
        {
            acidMine.gameObject.layer = playerLayerIndex;
        }

        acidMine.Shoot(muzzleTransform.forward, 1f);
    }

    private AcidMine CreatePoisonBullet()
    {
        AcidMine acidMine = Instantiate(bulletPrefab.gameObject, instancePosition, identity).GetComponent<AcidMine>();
        acidMine.SetManagedPool(poisonBulletPool);
        acidMine.SetMother(this);
        return acidMine;
    }

    private void OnGetPoisonBullet(AcidMine acidMine)
    {
        acidMine.transform.position = instancePosition;
        acidMine.SetTargetLayer(mountedOnEnemy ? LayerMaskCash.OnlyPlayer : LayerMaskCash.OnlyEnemy);
        acidMine.SetUser(mountedOnEnemy);
        acidMine.gameObject.SetActive(true);
    }

    private void OnReleasePoisonBullet(AcidMine acidMine)
    {
        acidMine.gameObject.SetActive(false);
    }

    private void OnDestroyPoisonBullet(AcidMine acidMine)
    {
        Destroy(acidMine.gameObject);
    }

    private Explosive CreateExplosive()
    {
        Explosive explosive = Instantiate(explosivePrefab, instancePosition, identity).GetComponent<Explosive>();
        explosive.SetManagedPool(explosivePool);
        explosive.SetWeapon(this);
        return explosive;
    }

    private void OnGetExplosive(Explosive explosive)
    {
        explosive.transform.position = instancePosition;
        explosive.SetDamage(UDamage.Value);
        explosive.SetRange(UExplosionRange.Value);
        explosive.SetTargetLayer(mountedOnEnemy ? LayerMaskCash.PlayerAndEnemy : LayerMaskCash.Enemy);
        explosive.gameObject.SetActive(true);
    }

    private void OnReleaseExplosive(Explosive explosive)
    {
        explosive.gameObject.SetActive(false);
    }

    private void OnDestroyExplosive(Explosive explosive)
    {
        Destroy(explosive.gameObject);
    }

    public override void BeItem()
    {
        transform.localPosition -= Vector3.up * 0.2f + Vector3.forward * 0.1f;
    }
}
