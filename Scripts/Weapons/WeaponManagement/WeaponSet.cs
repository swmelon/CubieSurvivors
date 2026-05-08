using System;
using System.Collections.Generic;
using System.Linq;
using Local.Scripts.Extensions;
using Unity.Mathematics;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using UnityEngine.UIElements;


[CreateAssetMenu(fileName = "WeaponSet", menuName = "ScriptableObjects/WeaponSet", order = int.MaxValue)]
public class WeaponSet : ScriptableObject, IDependentInitialization
{
    [SerializeField] 
    protected List<Weapon> weaponPrefabs;

    [SerializeField]
    private Vector3 warehousePosition;

    [SerializeField] 
    private WeaponEventChannelSO returnWeaponChannel;

    [SerializeField]
    private SaveLoadManagerSO saveLoadManager;

    [SerializeField]
    private string bossWeaponNameForDebug;
    
    private List<Weapon> weaponInstances = new List<Weapon>();
    
    private List<Weapon> weaponInstancesLocked = new List<Weapon>();
    private List<WeaponType> weaponTypes = new List<WeaponType>();
    private bool isInitialized;
    private List<int> indexUnlocked = new List<int>();

    public void Initialize()
    {
        returnWeaponChannel.Subscribe(ReturnWeaponInstance);
        InstantiateWeapons();
        
        Debug.Log("Pre-instantiate Weapons Complete");
    }
    
    /// <summary>
    /// Instantiate all weapon prefabs.
    /// </summary>
    private void InstantiateWeapons()
    {
        weaponInstances.Clear();
        weaponInstancesLocked.Clear();
        
        indexUnlocked = saveLoadManager.SaveFile.weaponUnlocked;

        for (int i = 0; i < weaponPrefabs.Count; i++)
        {
            GameObject weaponObject = Instantiate(weaponPrefabs[i].gameObject, warehousePosition, quaternion.identity);
            Weapon weaponInstance = weaponObject.GetComponent<Weapon>();
            weaponObject.SetActive(false);

            if (indexUnlocked.Contains(i))
            {
                weaponInstances.Add(weaponInstance);
            }
            else
            {
                weaponInstancesLocked.Add(weaponInstance);
            }
        }

        isInitialized = true;
    }
    
    /// <summary>
    /// Should SetActive(true) when using weapon from this method.
    /// Unused weapon instances should be returned by ReturnWeaponInstances().
    /// </summary>
    /// <param name="weapons"></param>
    /// <returns></returns>
    public bool TryGetAvailableWeaponInstances(out List<Weapon> weapons)
    {
        if (!isInitialized)
        {
            Debug.LogWarning("WeaponSet is not initialized but TryGetAvailableWeaponInstances is called. This can cause frame drop.");
            InstantiateWeapons();
        }
        
        weapons = new List<Weapon>(weaponInstances);
        weaponInstances.Clear();
        return weapons.Count > 0;
    }

    public bool TryGetAvailableWeaponInstances(out List<Weapon> weapons, WeaponManager weaponManager, int maxNumWeapons )
    {
        weapons = new List<Weapon>();

        foreach (var weapon in weaponInstances)
        {
            if (!weaponManager.IsMountable(weapon))
            {
                continue;
            }
            
            weapons.Add(weapon);
        }

        weapons.FisherShuffle();

        if (weapons.Count > maxNumWeapons)
        {
            weapons = RandomExtenstion.PickRandom(weapons, maxNumWeapons).ToList();
        }

        foreach (var weapon in weapons)
        {
            weaponInstances.Remove(weapon);    
        }
        
        return weapons.Count > 0;
    }

    public bool TryGetLockedWeaponInstances(out List<Weapon> weapons)
    {
        weapons = new List<Weapon>(weaponInstancesLocked);
        weaponInstancesLocked.Clear();
        return weapons.Count > 0;
    }

    
    public void ReturnWeaponInstance(Weapon weapon)
    {
        if (!TryGetWeaponIndex(weapon, out int index))
        {
            saveLoadManager.DataCorrupted();
            return;
        }

        weapon.enabled = false;
        weapon.gameObject.SetActive(false);
        weapon.transform.SetParent(null);
        weapon.ResetUpgrade();

        if (indexUnlocked.Contains(index))
        {
            if (weaponInstances.Contains(weapon))
            {
                Debug.LogError("Weapon instance is already in weaponInstances.");
                return;
            }

            weaponInstances.Add(weapon);
            weaponInstances.FisherShuffle();
        }
        else
        {
            if (weaponInstancesLocked.Contains(weapon))
            {
                Debug.LogError("Weapon instance is already in weaponInstances.");
                return;
            }

            weaponInstancesLocked.Add(weapon);
            weaponInstancesLocked.FisherShuffle();
        }
    }


    public void ReturnAndUnlockWeaponInstance(Weapon weapon)
    {
        if (!TryGetWeaponIndex(weapon, out int index))
        {
            saveLoadManager.DataCorrupted();
            return;
        }

        indexUnlocked.Add(index);
        saveLoadManager.Save();
        
        if (weaponInstancesLocked.Contains(weapon))
        {
            weaponInstancesLocked.Remove(weapon);
        }

        ReturnWeaponInstance(weapon);
    }
    
    public void ReturnWeaponInstances(List<Weapon> weapons)
    {
        weaponInstances.AddRange(weapons);
    }


    /// <summary>
    /// Get weapon instances of the same type.
    /// Usable for Boss.
    /// </summary>
    /// <param name="weapons"></param>
    /// <returns></returns>
    public bool TryGetRandomTypeOfWeaponInstances(out List<Weapon> weapons, WeaponManager weaponManager, 
        List<WeaponType> excludedTypes = null)
    {
        if (weaponInstances.Count == 0)
        {
            Debug.LogWarning("No available weapon instances. Call TryGetRandomTypeOfWeapons() instead.");
            weapons = new List<Weapon>();
            return false;
        }
        
        // Insure that each WeaponType has at least one available weapon in weaponInstances.
        
        weaponTypes.Clear();
        
        foreach (Weapon weapon in weaponInstances)
        {
            if(!weaponManager.IsMountable(weapon))
            {
                continue;
            }
            
            weaponTypes.Add(weapon.Type);
        }

        if (!ReferenceEquals(excludedTypes, null))
        {
            excludedTypes.ForEach(weaponType => weaponTypes.Remove(weaponType));
        }

        if (weaponTypes.Count == 0)
        {
            Debug.LogWarning("No available weapon instances. Call TryGetRandomTypeOfWeapons() instead.");
            weapons = new List<Weapon>();
            return false;
        }

        weaponTypes.FisherShuffle();
        WeaponType type = weaponTypes.PickRandom();

        weapons = new List<Weapon>();

        foreach (Weapon weapon in weaponInstances)
        {
            if (weapon.Type == type && weaponManager.IsMountable(weapon))
            {
                weapons.Add(weapon);
            }
        }

        foreach (var weapon in weapons)
        {
            weaponInstances.Remove(weapon);
        }

        weapons.Shuffle();

        return weapons.Count > 0;
    }

    public bool TryGetSpecificWeaponInstance(out Weapon weapon)
    {
        weapon = null;
        for (int i = 0; i < weaponInstances.Count; i++)
        {
            if (weaponInstances[i].Name == bossWeaponNameForDebug)
            {
                weapon = weaponInstances[i];
                break;
            }
        }

        if (weapon != null)
        {
            weaponInstances.Remove(weapon);
            return true;
        }

        return false;
    }

    public bool TryGetLockedWeaponInstance(Weapon weapon, out Weapon weaponInstance)
    {
        weaponInstance = null;

        if (!TryGetWeaponIndex(weapon, out int index))
        {
            return false;
        }

        index = -1;

        for (int i = 0; i < weaponInstancesLocked.Count; i++)
        {
            if (weaponInstancesLocked[i].Name == weapon.Name)
            {
                weaponInstance = weaponInstancesLocked[i];
                index = i;
                break;
            }
        }

        if (index != -1)
        {
            weaponInstancesLocked.RemoveAt(index);
            return true;
        }

        return false;
    }

    private void OnDestroy()
    {
        returnWeaponChannel.Unsubscribe(ReturnWeaponInstance);
        weaponInstances.ForEach(weapon => Destroy(weapon.gameObject));
    }

    private bool TryGetWeaponIndex(Weapon weapon, out int index)
    {
        index = -1;
        for (int i = 0; i < weaponPrefabs.Count; i++)
        {
            if (weaponPrefabs[i].Name == weapon.Name)
            {
                index = i;
                return true; 
            }
        }

        return false;
    }
}
