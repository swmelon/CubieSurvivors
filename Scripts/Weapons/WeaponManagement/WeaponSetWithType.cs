using System.Collections.Generic;
using Local.Scripts.Extensions;
using UnityEngine;


[CreateAssetMenu(fileName = "WeaponSetWithType", menuName = "ScriptableObjects/WeaponSetWithType", order = int.MaxValue)]
public class WeaponSetWithType : WeaponSet
{
    private HashSet<WeaponType> weaponTypes = new HashSet<WeaponType>();

    private void OnEnable()
    {
        foreach (Weapon weapon in weaponPrefabs)
        {
            weaponTypes.Add(weapon.Type);
        }
    }
    
    public bool TryGetRandomTypeOfWeapons(out List<Weapon> weapons)
    {
        WeaponType type = weaponTypes.PickRandom();
        
        weapons = new List<Weapon>();
        
        foreach (Weapon weapon in weaponPrefabs)
        {
            if (weapon.Type == type)
            {
                weapons.Add(weapon);
            }
        }

        weapons.Shuffle();
        
        return weapons.Count > 0;
    }
}
