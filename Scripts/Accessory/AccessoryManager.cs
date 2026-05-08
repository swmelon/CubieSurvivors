using System.Collections.Generic;
using UnityEngine;
using System;
using StarterAssets;
/// <summary>
/// �Ǽ����� ����
/// ��Ÿ�� �Ǽ����� �߰� , ����,  ��ü, �ҷ����� ��
/// </summary>
public class AccessoryManager : MonoBehaviour
{
    public Action<AccData> OnAccessoryEquipped;
    public Action<List<AccData>> OnRemoveAllAccessories;

    [SerializeField]
    private GameAccessoryManager accessoryManager;

    [SerializeField]
    private DamageCalculatorSO damageCalculator;

    [SerializeField]
    private DeathManager deathManager;

    [SerializeField]
    private List<int> baseAccessoryIDs;


    private CustomThirdPersonController playerController;
    private bool hasThirdPersonController = false;
    private Dictionary<AccessoryType, AccessorySlot> accessorySlots;

    private Renderer rollerRenderer;
    private bool initialized = false;

    public List<int> BaseAccessoryIDs => baseAccessoryIDs;

    private void Awake()
    {
        AccessorySlot[] slots = GetComponentsInChildren<AccessorySlot>();

        accessorySlots = new Dictionary<AccessoryType, AccessorySlot>();

        foreach (var slot in slots)
        {
            if (accessorySlots.ContainsKey(slot.accessoryType))
            {
                Debug.LogError("Duplicate accessory slot found for " + slot.accessoryType);
                continue;
            }

            accessorySlots.Add(slot.accessoryType, slot);
        }

        rollerRenderer = transform.GetChild(0).Find("Roller").GetComponent<MeshRenderer>();

        if (TryGetComponent(out CustomThirdPersonController controller))
        {
            playerController = controller;
            hasThirdPersonController = true;
        }
    }

    private void OnEnable()
    {
        ApplyAccessoryStat();
    }

    public void Copy(AccessoryManager otherManager)
    {
        foreach (var slot in accessorySlots.Values)
        { 
            foreach (var otherSlot in otherManager.accessorySlots.Values)
            {
                if (slot.accessoryType== otherSlot.accessoryType)
                {
                    otherSlot.Copy(slot);
                    break;
                }
            }
         }
        
        otherManager.rollerRenderer.material = rollerRenderer.material;
    }

    public void SetInitialDecoration(DecoData data)
    {
        initialized = true;

        if (data.Hat != null && accessorySlots.ContainsKey(AccessoryType.Hat))
        {
            accessorySlots[AccessoryType.Hat].EquipAccessory(data.Hat);
        }

        if (data.Glasses != null && accessorySlots.ContainsKey(AccessoryType.Glasses))
        {
            accessorySlots[AccessoryType.Glasses].EquipAccessory(data.Glasses);
        }

        if (data.Facial != null && accessorySlots.ContainsKey(AccessoryType.Facial))
        {
            accessorySlots[AccessoryType.Facial].EquipAccessory(data.Facial);
        }

        if (data.Particles != null && accessorySlots.ContainsKey(AccessoryType.Particles))
        {
            accessorySlots[AccessoryType.Particles].EquipAccessory(data.Particles);
        }

        if (data.RollerMaterial != null)
        {
            rollerRenderer.material = data.RollerMaterial;
        }
    }

    public AccData Equip(AccData accData)
    {
        if (!accessorySlots.TryGetValue(accData.accessory.AccessoryType, out AccessorySlot slot))
        {
            Debug.Log("No type matching accessory slot.");
            return null;
        }

        AccData accDataToReturn = slot.UnequipAccessory();
        slot.EquipAccessory(accData);
        OnAccessoryEquipped?.Invoke(accData);
        ApplyAccessoryStat();
        return accDataToReturn;
    }

    public (int, int, int ,int) GetAccessoryBonusStats()
    {
        int attack = 0;
        int defense = 0;
        int agility = 0;
        int luck = 0;

        foreach (var slot in accessorySlots.Values)
        {
            if (!slot.IsFull())
            {
                continue;
            }

            AccStats stats = slot.EquippedAccData.accessoryStats;
                
            attack += stats.Attack;
            defense += stats.Defense;
            agility += stats.Agility;
            luck += stats.Luck;

        }

        return (attack, defense, agility, luck);
    }

    public void ApplyAccessoryStat()
    {
        (int, int, int, int) bonusStats = GetAccessoryBonusStats();

        damageCalculator.SetAttackAndDefenseStats(bonusStats.Item1, bonusStats.Item2);
        deathManager.SetLuckStat(bonusStats.Item4);

        if (hasThirdPersonController)
        {
            playerController.SetAgilityStat(bonusStats.Item3);
        }
    }

    public List<AccData> UnequipAll()
    {
        List<AccData> unequippedAccessories = new List<AccData>();

        foreach (var slot in accessorySlots.Values)
        {
            AccData accessory = slot.UnequipAccessory();
            if (accessory != null)
            {
                unequippedAccessories.Add(accessory);
            }
        }

        ApplyAccessoryStat();

        OnRemoveAllAccessories?.Invoke(unequippedAccessories);
        return unequippedAccessories;
    }

    public List<AccData> GetEquippedAccessories()
    {
        List<AccData> equippedAccessories = new List<AccData>();

        foreach (var slot in accessorySlots.Values)
        {
            if (slot.IsFull())
            {
                equippedAccessories.Add(slot.EquippedAccData);
            }
        }

        return equippedAccessories;
    }
}