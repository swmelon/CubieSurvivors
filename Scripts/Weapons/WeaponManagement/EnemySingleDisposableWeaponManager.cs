
using System;
using System.Collections.Generic;
using Local.Scripts.Extensions;
using UnityEngine;

public class EnemySingleDisposableWeaponManager : EnemySingleWeaponManager
{
    [SerializeField]
    private DisposableWeaponSpawner disposableWeaponSpawner;

    protected override void Awake()
    {
        base.Awake();
        enemy.AddOnSpawnedEvent(CheckAndMountWeapon);
    }

    private void CheckAndMountWeapon()
    {
        if (weaponSlot.IsFull())
        {
            return;
        }


        Transform weaponSlotTrans = weaponSlot.transform; 
        WeaponDisposable weapon = disposableWeaponSpawner.Spawn(weaponSlotTrans.position, weaponSlotTrans.rotation);
        weapon.SetWeaponUser(enemy);
        weapon.SetWeaponManager(this);
        Mount(weapon);

    }

    public override void Mount(Weapon weapon)
    {
        if (weaponSlot.IsMountable(weapon))
        {
            weaponSlot.Mount(weapon);
            RaiseWeaponMounted(weapon);
        }
    }
}