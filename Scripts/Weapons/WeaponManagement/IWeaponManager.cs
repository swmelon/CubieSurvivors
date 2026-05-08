
using System.Collections.Generic;
using System;
public interface IWeaponManager
{
    public event Action<Weapon> WeaponMounted;
    public event Action<Weapon> WeaponUnmounted;

    public bool IsMountable(Weapon weapon);
    public void Mount(Weapon weapon);
    public void Unmount(Weapon weapon, bool deactivate = true);
    public List<Weapon> UnmountAll();
}
