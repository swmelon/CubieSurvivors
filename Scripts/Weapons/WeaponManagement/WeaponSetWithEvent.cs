
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "WeaponSetWithEvent", menuName = "ScriptableObjects/WeaponSetWithEvent", order = int.MaxValue)]
public class WeaponSetWithEvent : WeaponSet
{
    [SerializeField]
    private WeaponEventChannelSO weaponMountEventChannel;
    
    [SerializeField]
    private WeaponEventChannelSO weaponUnmountEventChannel;
    
    private List<Weapon> mountedWeapons = new List<Weapon>();

    private void OnEnable()
    {
        weaponMountEventChannel.Subscribe(OnMount);
        weaponUnmountEventChannel.Subscribe(OnUnmount);
        Reset();
    }

    private void OnMount(Weapon weapon)
    {
        mountedWeapons.Add(weapon);
    }
    
    private void OnUnmount(Weapon weapon)
    {
        mountedWeapons.Remove(weapon);
    }

    private void Reset()
    {
        mountedWeapons.Clear();
    }
}
