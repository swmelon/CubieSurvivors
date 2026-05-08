using Local.Scripts.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;


public enum WeaponType
{
    Melee,
    Ranged,
    Back,
    Top,
    Magic,
    Hammer,
    Shield,
    Special
}


[RequireComponent(typeof(ITargetHaves))]

public class WeaponManager : MonoBehaviour, IWeaponManager, IUpgradableContainer
{
    public event Action<Weapon> WeaponMounted;
    public event Action<Weapon> WeaponUnmounted;

    [Header("Listener")]
    [FormerlySerializedAs("restartGameEventChannel")]
    [SerializeField]
    private EventChannelSO playerFallEventChannel;

    [Header("")]
    [SerializeField]
    protected GameWeaponManagerSO gameWeaponManager;

    protected WeaponSlot[] weaponSlots;
    private Dictionary<WeaponType, Stack<WeaponSlot>> emptySlots;
    private Dictionary<WeaponType, Stack<WeaponSlot>> fullSlots;
    private List<WeaponSlot> fullSlotList = new List<WeaponSlot>();

    protected bool slotInitialized = false;
    private bool isQuickFireMode = false;

    [SerializeField]
    private GameObject quickFireEffect;

    private ITargetHaves user;
    private int count = 0;
    public int Count
    {
        get => count;
    }

    protected virtual void Awake()
    {
        user = GetComponent<ITargetHaves>();
    }

    private void Start()
    {
        // We have to wait WeaponSlots to be initialized that's why we use Start()

        if (!slotInitialized)
        {
            InitializeSlots();
        }
    }

    private void OnEnable()
    {
        playerFallEventChannel.Subscribe(UnmountAllAndParachute);
    }

    private void OnDisable()
    {
        playerFallEventChannel.Unsubscribe(UnmountAllAndParachute);
    }

    protected void InitializeSlots()
    {
        weaponSlots = GetComponentsInChildren<WeaponSlot>();
        emptySlots = new Dictionary<WeaponType, Stack<WeaponSlot>>();
        fullSlots = new Dictionary<WeaponType, Stack<WeaponSlot>>();

        foreach (WeaponType weaponType in Enum.GetValues(typeof(WeaponType)))
        {
            emptySlots.Add(weaponType, new Stack<WeaponSlot>());
            fullSlots.Add(weaponType, new Stack<WeaponSlot>());
        }

        foreach (WeaponSlot weaponSlot in weaponSlots)
        {
            if (!weaponSlot.IsFull())
            {
                weaponSlot.AddSelfToSlotCounter(emptySlots);
            }
            else
            {
                weaponSlot.Weapon.SetWeaponUser(user);
                weaponSlot.PreMount();
                fullSlotList.Add(weaponSlot);
                count++;
            }
        }

        slotInitialized = true;
    }

    // Need Fix
    public bool IsMountable(Weapon weapon)
    {
        WeaponType weaponType = weapon.Type;

        if (HasEmptySlot(weaponType))
        {
            return true;
        }

        foreach (WeaponSlot fullSlot in fullSlots[weaponType])
        {
            // 해당 무기타입을 장착하고 있는 슬롯의 무기 중 하나라도 교체할 수 있으면
            if (emptySlots[fullSlot.WeaponType].Count > 0)
            {
                return true;
            }
        }

        return false;
    }

    public virtual void Mount(Weapon weapon)
    {
        weapon.gameObject.SetActive(true);
        WeaponType weaponType = weapon.Type;

        Vector3 userScale = user.GetTransform().lossyScale;

        Debug.Assert(Mathf.Abs(userScale.x - userScale.y) < 0.001f &&
                     Mathf.Abs(userScale.x - userScale.z) < 0.001f,
           "Scale of root is not equal in all axis.");

        weapon.transform.localScale *= userScale.x;

        if (HasEmptySlot(weaponType))
        {
            WeaponSlot weaponSlot = emptySlots[weaponType].Peek();
            weaponSlot.Mount(weapon);
            CountSlot(weaponSlot, mount: true);
            WeaponMounted?.Invoke(weapon);
        }
        else
        {
            foreach (WeaponSlot fullSlot in fullSlots[weaponType])
            {
                // 해당 무기타입을 장착하고 있는 슬롯의 무기 중 하나라도 교체할 수 있으면
                if (emptySlots[fullSlot.WeaponType].Count > 0)
                {
                    Weapon unmounted = fullSlot.Unmount(true);
                    fullSlot.Mount(weapon);

                    WeaponSlot weaponSlot = emptySlots[unmounted.Type].Pop();
                    weaponSlot.Mount(unmounted);
                    unmounted.SetWeaponUser(user);

                    CountSlot(weaponSlot, mount: true);
                    WeaponMounted?.Invoke(weapon);

                    return;
                }
            }

            Debug.LogError("Mount() is called but not mountable");
            return;
        }

        weapon.SetWeaponUser(user);
    }

    public virtual Weapon UnmountRandom()
    {
        if (count == 0)
        {
            return default;
        }

        int index = RandomExtenstion.GetIntInRange(0, fullSlotList.Count - 1);

        WeaponSlot weaponSlot = fullSlotList.PickRandom();

        CountSlot(weaponSlot, mount: false);
        Weapon unmountedWeapon = weaponSlot.Unmount(true);

        WeaponUnmounted?.Invoke(unmountedWeapon);

        return unmountedWeapon;
    }

    public void Unmount(Weapon weapon, bool deactivate = true)
    {
        if (count == 0)
        {
            return;
        }

        for (int i = 0; i < weaponSlots.Length; i++)
        { 
            WeaponSlot weaponSlot = weaponSlots[i];

            if (weaponSlot.IsFull(out Weapon compare) && ReferenceEquals(compare, weapon))
            {
                CountSlot(weaponSlot, mount: false);
                weaponSlot.Unmount(deactivate);
                WeaponUnmounted?.Invoke(weapon);
                return;
            }
        }
    }

    public List<Weapon> UnmountAll()
    {
        List<Weapon> unmountedWeapons = new List<Weapon>();

        if (!slotInitialized)
        {
            InitializeSlots();
        }

        foreach (WeaponSlot weaponSlot in weaponSlots)
        {
            if (weaponSlot.IsFull(out Weapon weapon))
            {
                unmountedWeapons.Add(weaponSlot.Unmount(true));
                weapon.transform.localScale /= user.GetTransform().lossyScale.x;
                weapon.enabled = false;
                CountSlot(weaponSlot, mount: false);
            }
        }

        return unmountedWeapons;
    }

    public List<IUpgradable> Upgradables => GetUpgrabables();

    private List<IUpgradable> GetUpgrabables()
    {
        List<IUpgradable> allUpgradables = new List<IUpgradable>();

        foreach (WeaponSlot weaponSlot in weaponSlots)
        {
            if (weaponSlot.IsFull(out Weapon weapon))
            {
                allUpgradables.AddRange(weapon.Upgradables);
            }
        }

        return allUpgradables;
    }

    public IUpgradable[] GetUpgrabables(int num)
    {
        List<IUpgradable> allUpgradables = new List<IUpgradable>();

        foreach (WeaponSlot weaponSlot in weaponSlots)
        {
            if (weaponSlot.IsFull(out Weapon weapon))
            {
                allUpgradables.AddRange(weapon.Upgradables);
            }
        }

        allUpgradables = allUpgradables.Shuffle().ToList();
        return allUpgradables.Take(num).ToArray();
    }

    public void StartQuickFire(float time)
    {
        if (isQuickFireMode)
        {
            CancelInvoke("EndQuickFire");
        }

        isQuickFireMode = true;
        foreach (WeaponSlot weaponSlot in weaponSlots)
        {
            if (weaponSlot.IsFull(out Weapon weapon))
            {
                if (weapon is QuickFirableWeapon<UFloat>)
                {
                    QuickFirableWeapon<UFloat> quickFirableWeapon = (QuickFirableWeapon<UFloat>)weapon;
                    quickFirableWeapon.TurnOnQuickFireMode();
                }
                else if (weapon is QuickFirableWeapon<UWaitForSeconds>)
                {
                    QuickFirableWeapon<UWaitForSeconds> quickFirableWeapon = (QuickFirableWeapon<UWaitForSeconds>)weapon;
                    quickFirableWeapon.TurnOnQuickFireMode();
                }
            }
        }
        Invoke("EndQuickFire", time);
        quickFireEffect.SetActive(true);
    }

    private void EndQuickFire()
    {
        foreach (WeaponSlot weaponSlot in weaponSlots)
        {
            if (weaponSlot.IsFull(out Weapon weapon))
            {
                if (weapon is QuickFirableWeapon<UFloat>)
                {
                    QuickFirableWeapon<UFloat> quickFirableWeapon = (QuickFirableWeapon<UFloat>)weapon;
                    quickFirableWeapon.TurnOffQuickFireMode();
                }
                else if (weapon is QuickFirableWeapon<UWaitForSeconds>)
                {
                    QuickFirableWeapon<UWaitForSeconds> quickFirableWeapon = (QuickFirableWeapon<UWaitForSeconds>)weapon;
                    quickFirableWeapon.TurnOffQuickFireMode();
                }
            }
        }

        quickFireEffect.SetActive(false);
    }
    private bool HasEmptySlot(WeaponType weaponType)
    {
        if (!slotInitialized)
        {
            InitializeSlots();
        }

        return emptySlots[weaponType].Count != 0;
    }

    public bool HasWeapon()
    {
        return fullSlotList.Count != 0;
    }

    private void CountSlot(WeaponSlot weaponSlot, bool mount = true)
    {
        if (mount)
        {
            weaponSlot.RemoveSelfFromCounter(emptySlots);
            weaponSlot.AddSelfToSlotCounter(fullSlots);
            fullSlotList.Add(weaponSlot);
            count++;
        }
        else
        {
            weaponSlot.RemoveSelfFromCounter(fullSlots);
            weaponSlot.AddSelfToSlotCounter(emptySlots);
            fullSlotList.Remove(weaponSlot);
            count--;
        }
    }

    public void UnmountAndDestroyAll()
    {
        List<Weapon> unmountedWeapons = UnmountAll();

        unmountedWeapons.ForEach(w => Destroy(w.gameObject));
    }

    protected virtual void UnmountAllAndParachute()
    {
        foreach (Weapon weapon in UnmountAll())
        {
            gameWeaponManager.PackAndParachuteWeapon(weapon);
        }
    }
}
