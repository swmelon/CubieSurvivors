using System;
using System.Collections.Generic;
using Local.Scripts.Extensions;
using UnityEngine;

public class WeaponSlot : MonoBehaviour
{
    public WeaponType WeaponType
    {
        get => mountedWeapon.Type;
    }
    
    public Weapon Weapon => mountedWeapon;
    
    [SerializeField]
    [Tooltip("melee weapon mountable")]
    private bool melee;

    [SerializeField] 
    [Tooltip("ranged weapon mountable")]
    private bool ranged;
    
    [SerializeField]
    [Tooltip("back weapon mountable")]
    private bool back;
    
    [SerializeField] 
    [Tooltip("top weapon mountable")]
    private bool top;
    
    [SerializeField] 
    [Tooltip("hammer weapon mountable")]
    private bool hammer;
    
    [SerializeField] 
    [Tooltip("magic weapon mountable")]
    private bool magic;
    
    [SerializeField] 
    [Tooltip("shield weapon mountable")]
    private bool shield;
    
    [SerializeField] 
    [Tooltip("special weapon mountable")]
    private bool special;

    private HashSet<WeaponType> mountableTypes;

    [SerializeField]
    private Weapon mountedWeapon;
 
    
    [SerializeField]
    private WeaponSlotMeshManger meshManager = null;


    // ���⸦ ������ �� ������ ������
    // �� ���⸦ ������ �� �ִ� ������Ʈ�� ���⸦ ������ ��,
    // �������� ���� ������ �ȵȴ�.
    private Vector3 weaponLossyScale;

    private void Awake()
    {
        SetupMountableTypes();

        weaponLossyScale = transform.root.lossyScale;
    }

    private void SetupMountableTypes()
    {
        mountableTypes = new HashSet<WeaponType>();
        if (melee)
        {
            mountableTypes.Add(WeaponType.Melee);
        }
        if (ranged)
        {
            mountableTypes.Add(WeaponType.Ranged);
        }
        if (back)
        {
            mountableTypes.Add(WeaponType.Back);
        }
        if (top)
        {
            mountableTypes.Add(WeaponType.Top);
        }
        if (magic)
        {
            mountableTypes.Add(WeaponType.Magic);
        }
        if (hammer)
        {
            mountableTypes.Add(WeaponType.Hammer);
        }
        if (shield)
        {
            mountableTypes.Add(WeaponType.Shield);
        }
        if (special)
        {
            mountableTypes.Add(WeaponType.Special);
        }
    }

    public bool IsMountable(Weapon weapon)
    {
        return mountableTypes.Contains(weapon.Type);
    }

    public void Mount(Weapon weapon)
    {
        if (!IsMountable(weapon))
        {
            Debug.LogError("weapon " + weapon.gameObject.name + "is not mountable WeaponType.");
        }

        if (IsFull())
        {
            Debug.LogError("weapon " + weapon.gameObject.name + "is not mountable because the slot is full.");
        }
        
        if (!ReferenceEquals(weapon.transform.parent, null))
        {
            Debug.LogWarning("The weapon is already has parent " + weapon.transform.parent.name);
            weapon.transform.parent = null;
        }

        mountedWeapon = weapon;
        Transform weaponTransform = mountedWeapon.gameObject.transform;
        
       
        weaponTransform.SetPositionAndRotation(transform.position, transform.rotation);
        weaponTransform.localScale = weaponLossyScale;

        weaponTransform.SetParent(transform, worldPositionStays: true);
        weapon.enabled = true;

        if (!ReferenceEquals(meshManager, null))
        {
            meshManager.WeaponMounted();
        }
    }
    
    public void PreMount()
    {
        if (ReferenceEquals(mountedWeapon, null))
        {
            Debug.LogError("There is no weapon to pre-mount.");
            return;
        }
        
        mountedWeapon.enabled = true;

        if (!ReferenceEquals(meshManager, null))
        {
            meshManager.WeaponMounted();
        }
    }
    
    public Weapon Unmount(bool deactivate)
    {
        if (!IsFull())
        {
            Debug.LogError( "There is a no weapon mounted.");
        }
        
        Weapon temp = mountedWeapon;

        mountedWeapon.transform.parent = null;
        mountedWeapon = null;


        if (deactivate)
        {
            temp.enabled = false;
        }
        
        if (!ReferenceEquals(meshManager, null))
        {
            meshManager.WeaponUnmounted();
        }
        
        temp.OnUnmounted();
        return temp;
    }

    public void AddSelfToSlotCounter(Dictionary<WeaponType, Stack<WeaponSlot>> slotCounter)
    {
        foreach (WeaponType weaponType in mountableTypes)
        {
            slotCounter[weaponType].Push(this);
        }
    }
    
    public void RemoveSelfFromCounter(Dictionary<WeaponType, Stack<WeaponSlot>> slotCounter)
    {
        foreach (WeaponType weaponType in mountableTypes)
        {
            slotCounter[weaponType].RemoveElement(this);
        }
    }
    
    public bool IsFull()
    {
        if (ReferenceEquals(mountedWeapon, null))
        {
            return false;
        }

        return true;
    }
    
    public bool IsFull(out Weapon weapon)
    {
        weapon = mountedWeapon;
        
        if (ReferenceEquals(mountedWeapon, null))
        {
            return false;
        }
        return true;
    }



}