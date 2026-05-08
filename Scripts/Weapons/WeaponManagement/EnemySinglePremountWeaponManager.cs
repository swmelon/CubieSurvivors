
using System;
using System.Collections.Generic;
using Local.Scripts.Extensions;
using UnityEngine;

public class EnemySinglePremountWeaponManager : EnemySingleWeaponManager
{
    private void Start()
    {
        weaponSlot.Weapon.SetWeaponUser(enemy);
        weaponSlot.PreMount();
    }
}
