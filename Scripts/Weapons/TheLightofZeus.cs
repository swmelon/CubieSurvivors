using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Local.Scripts.Extensions;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

public class TheLightofZeus : QuickFirableWeapon<UWaitForSeconds>
{
    private enum Mode
    {
        Tracking,
        Targeting,
        Predictive,
        Random
    }
    
    [SerializeField]
    private Explosive explosivePrefab, enemyExplosivePrefab;

    [SerializeField]
    private ObjectMover sparkEffect;

    [SerializeField]
    private DamageOverTimeTrigger sparkDOTTrigger;


    [FormerlySerializedAs("playerMoveInfoChannel")] [SerializeField]
    private PlayerMoveDirectionChannelSO playerMoveDirectionChannel;

    [SerializeField]
    private GridSystemChannelSO gridSystemChannel;

    [SerializeField]
    private OnePureEffectSpawner hitEffectSpawner;

    private CustomObjectPool<Explosive> explosivePool;

    private Mode mode;
    private Vector3 lastTrackingPos;
    
    struct UpgradableStat
    {
        public List<float> Damage, NumOfProjectiles;
        public List<float> RateOfFire, Range;
        public List<bool> Unlocked;
    }

    private UInt UDamage;
    private UInt UNumOfProjectiles;
    private UFloat UThunderRange;
    private Upgradable<bool> UUltimateUpgrade;

    private GameObject dummyLightning;
    private WaitForSeconds tickDelay;
    private float tickTime = 0.2f;
    private RaycastHit[] hits = new RaycastHit[32];
    private Vector3 lightningPos;


    protected override void Awake()
    {
        base.Awake();
        
        explosivePool = new CustomObjectPool<Explosive>(CreateExplosive, OnGetExplosive, OnReleaseExplosive, OnDestroyExplosive, maxSize:10);
        onMountedOnPlayer = OnMountedOnPlayer;
        onMounted = OnMounted;
        dummyLightning = transform.GetChild(0).gameObject;
        tickDelay = new WaitForSeconds(tickTime);
    }


    private void OnMounted()
    {
        dummyLightning.SetActive(false);
        sparkDOTTrigger.SetTarget(mountedOnEnemy ? LayerMaskCash.OnlyPlayer : LayerMaskCash.OnlyEnemy);
        sparkEffect.transform.position = transform.position;

        if (UUltimateUpgrade.Value)
        {
            Unlock();
        }
        else
        {
            Lock();
        }
    }
    
    private void OnMountedOnPlayer()
    {
        explosivePool.Clear();
        SetRandomMode();
        StartCoroutine(Fire());
    }

    public override void OnUnmounted()
    {
        explosivePool.Clear();
        StopAllCoroutines();
        sparkDOTTrigger.StopDamage();
        sparkEffect.gameObject.SetActive(false);
    }

    protected override void SetupUpgradables()
    {
        UpgradableStat upgradableStat = UpgradeExtension.ReadUpgradableStat<UpgradableStat>(this);
        UDamage = new UInt(upgradableStat.Damage, symbol: symbolContainer.ExplosionDamage, 
            optionText : CardText.EXPLOSION_DAMAGE);
        UNumOfProjectiles = new UInt(upgradableStat.NumOfProjectiles, symbol: symbolContainer.Plus, 
            optionText: CardText.NUM_OF_LIGHTNING, noBonus: true);
        UThunderRange = new UFloat(upgradableStat.Range, symbol: symbolContainer.Scale, 
            optionText : CardText.EXPLOSION_RADIUS);
        UFireWaitTime = new UWaitForSeconds(upgradableStat.RateOfFire, symbol: symbolContainer.RateOfFire,
            optionText : CardText.RATE_OF_FIRE);
        UUltimateUpgrade = new Upgradable<bool>(upgradableStat.Unlocked, optionText: CardText.ULTIMATE_UPGRADE_LIGHTOFZEUS);

        IUpgradable[] otherUpgradables = {UDamage, UNumOfProjectiles, UThunderRange, UFireWaitTime};
        UUltimateUpgrade.UnlockWhenComplete(otherUpgradables);
        UUltimateUpgrade.UpgradeCompleted += Unlock;
        UUltimateUpgrade.UpgradeReset += Lock;
        Lock();
    }
    
    private void Unlock()
    {
        sparkEffect.transform.SetParent(null);
        sparkEffect.gameObject.SetActive(true);
        sparkDOTTrigger.StartDamage();
    }

    private void Lock()
    { 
        sparkEffect.gameObject.SetActive(false);
        sparkEffect.transform.SetParent(transform);
        sparkDOTTrigger.StopDamage();
    }


    public override void Damage()
    {
        if (target == null || !target.gameObject.activeSelf)
        {
            return;
        }
        
        switch (mode)
        {
            case Mode.Random:
                break;
            case Mode.Tracking:
            {
                Vector3 thunderPos = lastTrackingPos + ( target.position - lastTrackingPos).normalized;
                SpawnLightning(thunderPos);
                lastTrackingPos = thunderPos;
                break;
            }
            case Mode.Targeting:
                SpawnLightning(target.position);
                break;
            case Mode.Predictive:
            {
                if (!mountedOnEnemy)
                {
                    break;
                }
                
                KeyValuePair<Vector3, Vector3> playerMove = playerMoveDirectionChannel.GetDelayedMoveInfo();
                
                Vector3 thunderPos = playerMove.Key + UThunderRange.Value * playerMove.Value;
                SpawnLightning(thunderPos);
                break;
            }
        }
    }
    
    private IEnumerator Fire()
    {
        Vector3 targetPos = Vector3.zero;
        while (true)
        {
            // far to close
            for (int i = user.GetTargets(UNumOfProjectiles.Value, 10, out Transform[] targets) -1; i >= 0 ; i--)
            {
                if (targets[i] == null)
                {
                    continue;
                }

                targetPos = targets[i].position;
                MoveOrb(targetPos);
                yield return tickDelay;
                SpawnLightning(targetPos);
            }
            
            yield return UFireWaitTime.Value;
        }
    }

    public void SetTrackingMode()
    {
        mode = Mode.Tracking;
        lastTrackingPos = transform.position;
    }

    public void SetTargetingMode() => mode = Mode.Targeting;
    public void SetPredictiveMode() => mode = Mode.Predictive;
    public void SetRandomMode() => mode = Mode.Random;
    
    
    private void SpawnLightning(Vector3 position)
    {
        lightningPos = position;
        Explosive lightning = explosivePool.Get();
        lightning.Explode();
        MoveOrb(position);
    }

    private void MoveOrb(Vector3 position)
    {
        if (!UUltimateUpgrade.Value)
        {
            return;
        }

        sparkEffect.Move(position, tickTime);
        Cast(sparkEffect.transform.position, position);
    }

    private Explosive CreateExplosive()
    {
        Explosive explosive = Instantiate(explosivePrefab, lightningPos, Quaternion.identity).GetComponent<Explosive>();
        explosive.SetManagedPool(explosivePool);
        explosive.SetWeapon(this);
        return explosive;
    }

    protected virtual void OnGetExplosive(Explosive explosive)
    {
        explosive.SetDamage(UDamage.Value);
        explosive.SetRange(mountedOnEnemy ? 1f : UThunderRange.Value);
        
        if (mountedOnEnemy)
        {
            explosive.SetTargetLayer(LayerMaskCash.PlayerAndEnemy);
        }
        else
        {
            explosive.SetTargetLayer(LayerMaskCash.Enemy);
        }

        explosive.transform.position = lightningPos;
        explosive.gameObject.SetActive(true);
    }

    protected virtual void OnReleaseExplosive(Explosive explosive)
    {
        explosive.gameObject.SetActive(false);
    }

    protected virtual void OnDestroyExplosive(Explosive explosive)
    {
        Destroy(explosive.gameObject);
    }

    public override void BeItem()
    {
        dummyLightning.SetActive(true);
    }

    private void Cast(Vector3 start, Vector3 end)
    {
        Vector3 direction = end - start;
        float magnitude = direction.magnitude;
        direction.Normalize();
        int num = Physics.RaycastNonAlloc(start,  direction, hits, 
            magnitude, mountedOnEnemy ? LayerMaskCash.OnlyPlayer : LayerMaskCash.OnlyEnemy, QueryTriggerInteraction.Ignore);

        int damage = (int) (UDamage.Value * 0.5f);

        for (int i = 0; i < num; i++)
        {
            if (hits[i].collider.TryGetComponent(out Damagable damagable))
            {
                damagable.Hit(damage);
                hitEffectSpawner.Spawn(hits[i].point);
            }
        }
    }
}
