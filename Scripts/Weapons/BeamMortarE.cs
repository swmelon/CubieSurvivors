using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;

public class BeamMortarE : EnemyWeapon
{
    [SerializeField] 
    private Laser LaserPrefab;
    
    [SerializeField]
    private Explosive explosivePrefab;

    [SerializeField] 
    private Transform barrel;
    private struct UpgradableInfo
    {
        public List<int> ExplosiveDamage;
        public List<float> ExplosiveRange, RateOfFire;
    }

    private Upgradable<int> UExplosiveDamage;
    private Upgradable<float> UExplosiveRange;

    private static IObjectPool<Laser> LaserPool;
    private static IObjectPool<Explosive> explosivePool;
    private static bool initialized = false;

    private void Awake()
    {
        if (!initialized)
        {
            LaserPool = new ObjectPool<Laser>(CreateLaser, OnGetLaser, OnReleaseLaser, OnDestroyLaser, maxSize: 10);
            explosivePool = new ObjectPool<Explosive>(CreateExplosive, OnGetExplosive, OnReleaseExplosive,
                OnDestroyExplosive, maxSize: 10);
            initialized = true;
        }
    }
    
    public override void Shoot()
    {
        Laser laser = LaserPool.Get();
        laser.transform.position = barrel.position;
        laser.transform.rotation = Quaternion.Euler(0, barrel.rotation.y, barrel.rotation.z);
    }
    
    private Laser CreateLaser()
    {
        Laser laser = Instantiate(LaserPrefab.gameObject).GetComponent<Laser>();
        laser.SetManagedPool(LaserPool);
        return laser;
    }

    private void OnGetLaser(Laser laser)
    {
        laser.gameObject.SetActive(true);
    }

    private void OnReleaseLaser(Laser laser)
    {
        laser.gameObject.SetActive(false);
    }

    private void OnDestroyLaser(Laser laser)
    {
        Destroy(laser.gameObject);
    }
    
    private Explosive CreateExplosive()
    {
        Explosive explosive = Instantiate(explosivePrefab).GetComponent<Explosive>();
        explosive.SetManagedPool(explosivePool);
        return explosive;
    }
    
    private void OnGetExplosive(Explosive explosive)
    {
        explosive.SetDamage(UExplosiveDamage.Value);
        explosive.SetRange(UExplosiveRange.Value);
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
}
