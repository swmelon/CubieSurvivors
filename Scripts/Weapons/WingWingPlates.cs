using Local.Scripts.Extensions;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class WingWingPlates : BulletGun<Plate>
{
    [SerializeField]
    private Transform muzzleLeft, muzzleRight;

    private HashSet<Plate> activePlates = new HashSet<Plate>();

    private bool left = true;



    public override void Damage()
    {
        bulletPool.Get();
        bulletPool.Get();
    }

    protected override void OnMountedOnPlayer()
    {
        StartCoroutine(Fire());
        SetLockOnMode(true, 1000f);
        lockOnModeBulletSpeed = bulletSpeed;
    }

    protected override void RotateTowardTarget()
    {
        // do nothing
    }

    protected override Plate CreateBullet()
    {
        Plate bullet = Instantiate(bulletPrefab);
        bullet.SetManagedPool(bulletPool);
        bullet.SetWeapon(this);
        bullet.BulletHit += OnBulletHit;
        return bullet;
    }

    protected override void OnGetBullet(Plate bullet)
    {
        bullet.SetDamage(UDamage.Value);

        if (!ReferenceEquals(target, null))
        {
            bullet.SetTarget(target);
        }

        bullet.SetTargetLayer(mountedOnEnemy ? LayerMaskCash.Player : LayerMaskCash.Enemy);
        bullet.SetMaxDegreePerSec(lockOnMode ? bulletMaxDegreePerSec : 0f);
        bullet.SetSpeed(lockOnMode ? lockOnModeBulletSpeed : bulletSpeed);
        bullet.SetScale(transform.lossyScale.x);
        bullet.SetUser(!mountedOnEnemy);

        if (left)
        {
            bullet.SetDirection(muzzleLeft.forward);
            bullet.transform.SetPositionAndRotation(muzzleLeft.position, muzzleLeft.rotation );
        }
        else
        {
            bullet.SetDirection(muzzleRight.forward);
            bullet.transform.SetPositionAndRotation(muzzleRight.position, muzzleRight.rotation);
        }


        // activate하기 전에 위치를 설정해야한다.
        bullet.gameObject.SetActive(true);
        left = !left;

        activePlates.Add(bullet);
    }

    protected override void OnReleaseBullet(Plate bullet)
    {
        activePlates.Remove(bullet);
        base.OnReleaseBullet(bullet);
    }

    protected override void OnDestroyBullet(Plate bullet)
    {
        activePlates.Remove(bullet);
        base.OnDestroyBullet(bullet);
    }

    public void ReflectiveStrike()
    {
        if (ReferenceEquals(target, null))
        {
            return;
        }

        foreach (var bullet in activePlates)
        {
            bullet.ReflectNow(target.position, 30f, UDamage.Value * activePlates.Count);
        }
    }
}