using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "GameWeaponManagerSO", menuName = "ScriptableObjects/GameWeaponManagerSO",
    order = SOAssetMenuIndex.Manager)]
public class GameWeaponManagerSO : ScriptableObject, IDependentInitialization
{
    [SerializeField]
    private WeaponSet weaponSet;

    [SerializeField]
    private ItemPackerSO itemPacker;

    public WeaponSet WeaponSet => weaponSet;


    public void Initialize()
    {
        weaponSet.Initialize();
    }
    /// <summary>
    /// Be sure that each of weapon in weaponSet is mountable.
    /// </summary>
    /// <param name="position"></param>
    public void SpawnWeaponSpinner(Vector3 position)
    {
        if (!weaponSet.TryGetAvailableWeaponInstances(out List<Weapon> weapons))
        {
            Debug.LogError("No available weapons. Check the weapon set.");
            return;
        }

        List<Item> weaponItems = new List<Item>();

        foreach (Weapon weapon in weapons)
        {
            weaponItems.Add(PackAndPopUpWeapon(weapon));
        }

        if (weaponItems.Count == 0)
        {
            Debug.LogError("No available weapons. Check the weapon set.");
            return;
        }

        ItemSpinner itemSpinner = new GameObject().AddComponent<ItemSpinner>();
        itemSpinner.gameObject.transform.position = position;

        itemSpinner.Setup(weaponItems);
    }

    public Item PackAndParachuteWeapon(Weapon weapon)
    {
        Vector3 position = weapon.transform.position;
        weapon.gameObject.SetActive(true);
        InitialWeaponItem item = itemPacker.WarpUp<Weapon, InitialWeaponItem>(weapon, true);
        item.transform.position = position;
        return item;
    }

    public Item PackAndPopUpWeapon(Weapon weapon)
    {
        Vector3 position = weapon.transform.position;
        weapon.gameObject.SetActive(true);
        Item item = itemPacker.WarpUp<Weapon, InitialWeaponItem>(weapon, false);
        item.transform.position = position;
        return item;
    }
    
    public void ReturnWeaponInstance(Weapon weapon)
    {
        weaponSet.ReturnWeaponInstance(weapon);
    }

    public void ReturnAndUnlockWeaponInstance(Weapon weapon)
    {
        weaponSet.ReturnAndUnlockWeaponInstance(weapon);
    }
}
 