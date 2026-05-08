
using Local.Scripts.Extensions;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Pool;

public class MissileLauncher : ExplosiveBulletGun<Missile>
{
    [SerializeField]
    private GridSystemChannelSO gridSystemChannel;

    [SerializeField]
    private float miniumFireInterval = 0.3f;

    [SerializeField]
    private List<Transform> hardPoints;

    [SerializeField]
    private GameObject extraPods;

    [SerializeField]
    private float rotationSpeed = 10f;

    private const float missileSpawnHeight = 20f;
    private const float fallingMissileSpeed = 15f;
    private const float fallingMissileRotationX = 90f;
    private const float randomOffsetRange = 2f;
    private const float scaleFactor = 0.5f;
    private const float dummyTargetHeight = 1000f;

    private int hardPointIndex = 0;

    private Transform dummyTarget;
    private Queue<Missile> equippedMissiles = new Queue<Missile>();
    private Queue<Missile> launchedMissiles = new Queue<Missile>();
    private WaitForSeconds fireInterval, halfFireInterval;
    private Transform[] enemies = new Transform[32];

    private Upgradable<int> UNumProjectiles;
    private UFloat UExplosiveRange;
    private Upgradable<bool> UUltimateUpgrade;
    private Transform[] extraHardPoints;



    protected override void Awake()
    {
        base.Awake();
        dummyTarget = new GameObject().transform;
        dummyTarget.transform.position = Vector3.up * dummyTargetHeight;
        fireInterval = new WaitForSeconds(miniumFireInterval);
        halfFireInterval = new WaitForSeconds(miniumFireInterval / 2f);

        ResetHardPoint();
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        ResetHardPoint();
    }

    protected override void FixedUpdate()
    {
        if (mountedOnEnemy)
        {
            user.TryGetTarget(out target);
            return;
        }

        if (user.TryGetTarget(out Transform usersTarget, out float distance) &&
       (distance <= URange.Value))
        {
            target = usersTarget;

        }
        else
        {
            target = null;
        }
    }

    protected override void InitializeUpgradables(UpgradableStat upgradableStat)
    {
        base.InitializeUpgradables(upgradableStat);
        UNumProjectiles = new Upgradable<int>(upgradableStat.NumProjectiles, symbol: symbolContainer.Plus,
            optionText: CardText.NUM_OF_PROJECTILES);
        UExplosiveRange = new UFloat(upgradableStat.ExplosiveRange, symbol: symbolContainer.Scale,
            optionText: CardText.EXPLOSION_RADIUS);
        UFireWaitTime = new UWaitForSeconds(upgradableStat.RateOfFire, symbol: symbolContainer.Refresh,
            optionText: CardText.RELOAD_SPEED);
        UUltimateUpgrade = new Upgradable<bool>(upgradableStat.Unlocked, optionText: CardText.ULTIMATE_UPGRADE_QUADLAUNCHER);

        IUpgradable[] otherUpgradables = { UDamage, UNumProjectiles, UExplosiveRange, UFireWaitTime, UExplosiveDamage };
        UUltimateUpgrade.UnlockWhenComplete(otherUpgradables);
        UUltimateUpgrade.UpgradeCompleted += Unlock;
        UUltimateUpgrade.UpgradeReset += Lock;
    }

    private void Unlock()
    {
        extraHardPoints = extraPods.GetComponentsInChildren<Transform>();
        extraPods.SetActive(true);

        int count = hardPoints.Count;
        
        for (int i = 0; i < count; i++)
        {
            hardPoints.Add(extraHardPoints[i + 1]);
        }
    }

    private void Lock()
    {
        extraPods.SetActive(false);

        int count = hardPoints.Count / 2;

        for (int i = 0; i < count; i++)
        {
            hardPoints.RemoveAt(hardPoints.Count - 1);
        }
    }

    private void Update()
    {
        rotatingPart.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
    }

    public override void Damage()
    {
        if (ReferenceEquals(target, null))
        {
            return;
        }

        if (!FullyEquipped())
        {
            ReloadMissile();
        }

        

        LaunchMissile();

        if (lockOnMode)
        {
            FireFallingMissile(target);
        }
        else
        {
            FireFallingMissile(target.position);
        }
    }

    protected override void RotateTowardTarget()
    {
        return;
    }

    protected override void OnMountedOnPlayer()
    {
        SetLockOnMode(false, 10f, 10f);
        StartCoroutine(Fire());
    }

    protected override IEnumerator Fire()
    {
        while (true)
        {
            int numProjectices = UUltimateUpgrade.Value ?  2*UNumProjectiles.Value : UNumProjectiles.Value;
            WaitForSeconds interval = UUltimateUpgrade.Value ? halfFireInterval : fireInterval;

            if (ReferenceEquals(target, null))
            {
                yield return interval;
                continue;
            }
            // 히히 미사일 발싸
            for (int i = 0; i < numProjectices; i++)
            {
                LaunchMissile();
                yield return interval;
            }

            for (int i = 0; i < numProjectices; i++)
            {
                ReloadMissile();
            }

            yield return UFireWaitTime.Value;

            if (ReferenceEquals(target, null))
            {
                continue;
            }

            // 적은 어디에 있는교?
            
            gridSystemChannel.GetEnemiesFromCrowdedPosition(numProjectices - 1, enemies);
            
            // 현재 타겟은 확정
            FireFallingMissile(target);
            

            yield return interval;

            //그리고 나머지는 밀집된 구역에 떨군다.
            for (int i = 0; i < numProjectices - 1; i++)
            {
                // 과거에 얻은 정보이므로 적이 죽었을 수도 있음
                if (enemies[i] != null)
                {
                    FireFallingMissile(enemies[i]);
                }
                yield return interval;
            }
        }
        // 미사일을 미사일을 일단 위로 날리고, 새 미사일을 Get하여 아래방향으로 떨어뜨림
    }

    private void LaunchMissile()
    {
        // queue가 비어있을 수도 있다
        
        if (equippedMissiles.Count == 0)
        {
            return;
        }

        Missile firedMissile = equippedMissiles.Dequeue();

        launchedMissiles.Enqueue(firedMissile);

        firedMissile.transform.SetParent(null, worldPositionStays: true);
        firedMissile.SetActive(true);
        firedMissile.SetDirection(hardPoints[0].forward);
    }

    private void ReloadMissile()
    {
        Missile missileToEquip = bulletPool.Get();
        missileToEquip.SetActive(false);
        missileToEquip.transform.position = hardPoints[hardPointIndex].position;
        missileToEquip.transform.rotation = hardPoints[hardPointIndex].rotation;
        missileToEquip.transform.SetParent(hardPoints[hardPointIndex], worldPositionStays:true);
        hardPointIndex = (hardPointIndex + 1) % hardPoints.Count;
        equippedMissiles.Enqueue(missileToEquip);
    }

    private void ResetHardPoint()
    {
        foreach (var missile in equippedMissiles)
        {
            missile.Release();
        }

        equippedMissiles.Clear();

        for (int i = 0; i < hardPoints.Count; i++)
        {
            ReloadMissile();
        }

        hardPointIndex = 0;
    }

    private void FireFallingMissile(Transform target)
    {
        Missile missile = bulletPool.Get();
        Vector3 position = new Vector3(target.position.x, missileSpawnHeight, target.position.z);

        // when player using this, lockOnMode is always false
        // this is awkward but fix it later
        missile.SetMaxDegreePerSec(lockOnMode ? bulletMaxDegreePerSec : float.MaxValue);
        missile.SetTarget(target);
        missile.transform.position = position;
        missile.transform.rotation = Quaternion.Euler(fallingMissileRotationX, 0f, 0f);
        missile.SetSpeed(fallingMissileSpeed);
    }

    private void FireFallingMissile(Vector3 fallPosition)
    {
        Missile missile = bulletPool.Get();

        // random position
        Vector3 randomOffset = RandomExtenstion.GetRandomXZVector3(-randomOffsetRange, randomOffsetRange);
        fallPosition += randomOffset;

        Vector3 position = new Vector3(fallPosition.x, missileSpawnHeight, fallPosition.z);
        Vector3 fallPoint = new Vector3(fallPosition.x, 0f, fallPosition.z);


        missile.SetPosAndMarkFallPoint(position);
        missile.transform.rotation = Quaternion.Euler(fallingMissileRotationX, 0f, 0f);
        missile.SetDirection(Vector3.down);
        missile.SetSpeed(fallingMissileSpeed);

    }

    private void FireFallingMissileInRandomPosition(Vector3 fallPosition)
    {
        float randomX = RandomExtenstion.GetFloatInRange(-randomOffsetRange, randomOffsetRange);
        float randomZ = RandomExtenstion.GetFloatInRange(-randomOffsetRange, randomOffsetRange);

        fallPosition.x += randomX;
        fallPosition.z += randomZ;

        FireFallingMissile(fallPosition);
    }

    protected override Missile CreateBullet()
    {
        Missile missile = base.CreateBullet();
        // target이 없는 상태로 targetLessTime을 초과하면 파괴됨
        missile.SetTargetlessTime(5f);
        return missile;
    }

    protected override void OnGetBullet(Missile bullet)
    {
        base.OnGetBullet(bullet);
        bullet.SetTarget(null);
        bullet.SetActive(true);

        if (!ReferenceEquals(user, null))
        {
            bullet.transform.localScale = Vector3.one * user.GetTransform().localScale.x * scaleFactor;
        }
    }

    protected override void OnGetExplosive(Explosive explosive)
    {
        base.OnGetExplosive(explosive);
        explosive.SetRange(UExplosiveRange.Value);
    }

    private bool FullyEquipped()
    {
        return equippedMissiles.Count == hardPoints.Count;
    }
}
