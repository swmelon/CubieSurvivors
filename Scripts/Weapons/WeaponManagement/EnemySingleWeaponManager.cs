using System.Collections.Generic;
using System;
using UnityEngine;
using Local.Scripts.Extensions;
using Unity.VisualScripting;

public class EnemySingleWeaponManager : MonoBehaviour, IEnemyWeaponManager
{
    public event Action<Weapon> WeaponMounted;
    public event Action<Weapon> WeaponUnmounted;

    protected WeaponUsableEnemy enemy;

    [SerializeField]
    protected WeaponSlot weaponSlot;

    protected virtual void Awake()
    {
        enemy = GetComponent<WeaponUsableEnemy>();
    }

    public virtual bool IsMountable(Weapon weapon)
    {
        return false;
    }

    public virtual void Mount(Weapon weapon)
    {
    }

    public void Unmount(Weapon weapon, bool deactivate = true)
    {
        if (ReferenceEquals(weaponSlot.Weapon, weapon))
        {
            weaponSlot.Unmount(deactivate);
        }
    }

    public List<Weapon> UnmountAll()
    {
        return default;
    }

    public Weapon Unmount(bool deactivate = true)
    {
        return weaponSlot.Unmount(deactivate);
    }

    public List<IWeapon> GetWeaponsContainAttackPattern(IConditionalBehaviourPattern attackPattern)
    {
        Debug.LogWarning("EnemyStaticWeaponManager.GetWeaponsContainAttackPattern() should not be called");
        return null;
    }

    public bool TryGetAttackPatternByDistanceToTarget(float distanceToTarget, out IAttackPattern attackPattern)
    {
        float healthRatio = (float)enemy.Health / enemy.MaxHealth;

        if (!weaponSlot.IsFull(out Weapon weapon))
        {
            attackPattern = null;
            return false;
        }

        List<IAttackPattern> attackPatterns = weapon.GetAvailableAttackPatterns(healthRatio);

        if (attackPatterns.Count == 0)
        {
            Debug.LogWarning("No attack pattern is available.");
            attackPattern = null;
            return false;
        }

        attackPattern = attackPatterns.PickRandom();
        return true;
    }

    protected void RaiseWeaponMounted(Weapon weapon)
    {
        WeaponMounted?.Invoke(weapon);
    }
}
