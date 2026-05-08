using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.UIElements;
using System.Linq;
using Local.Scripts.Extensions;


[CreateAssetMenu(fileName = "GameAccessoryManager", menuName = "ScriptableObjects/GameAccessoryManager", order = SOAssetMenuIndex.Manager)]
public class GameAccessoryManager : ScriptableObject, IDependentInitialization
{
    public event Action DataChanged;
    public List<(Accessory, List<AccData>)> AccessoriesOnShelf => accessoriesOnShelf;
    public Dictionary<int, List<AccData>> AccessoriesEquipped => accessoriesEquipped;
    public List<Accessory> AccessoriesOnSale => accessoriesOnSale;

    [SerializeField]
    private List<Accessory> accessoryPrefabs;

    [SerializeField]
    private SaveLoadManagerSO saveLoadManager;

    [SerializeField]
    private EventChannelSO getNewAccEC;

    [SerializeField]
    private CharacterManagerSO characterManager;

    [SerializeField]
    private AccessoryStatsDatabase accStatsDatabase;

    private SaveFile saveFile;
    private List<Accessory> accessoriesOnSale, baseAccessories;
    // accessory prefab, list of statIDs
    private List<(Accessory, List<AccData>)> accessoriesOnShelf;
    private Dictionary<int, List<AccData>> accessoriesEquipped;

    private static int HAT_ID_OFFSET = 0, GLASSES_ID_OFFSET = 1000;


    public void Initialize()
    {
        saveFile = saveLoadManager.SaveFile;

        if (!CheckValidity())
        {
            saveLoadManager.DataCorrupted();
        }

        accessoriesOnShelf = new List<(Accessory, List<AccData>)>();
        accessoriesEquipped = new Dictionary<int, List<AccData>>();
        accessoriesOnSale = new List<Accessory>();
        baseAccessories = new List<Accessory>();

        GiveIDToUnassigned();

        AccData data;
        Accessory prevAcc = null;
        int index = -1;

        foreach (int savedID in saveFile.accessoriesOnShelf)
        {
            if (savedID == -1)
            {
                accessoriesOnShelf.Add((null, new List<AccData>()));
                index = accessoriesOnShelf.Count - 1;
                continue;
            }

            data = new AccData(savedID);

            if (!data.TryAssign(prevAcc))
            {
                Accessory currentAcc = accessoryPrefabs.Find(prefab => prefab.ID == data.accID);
                accessoriesOnShelf.Add((currentAcc, new List<AccData>()));
                index = accessoriesOnShelf.Count - 1;

                if (!data.TryAssign(currentAcc))
                {
                    Debug.LogError(currentAcc.name + " is not assignable.");
                    continue;
                }
                prevAcc = currentAcc;
            }

            accessoriesOnShelf[index].Item2.Add(data);
        }

        // ĳ���Ͱ� �Ǽ������� ������, �� ������Ʋ �߰����־���Ѵ�.

        foreach (var keyValuePair in saveFile.charactersUnlocked)
        {
            int charID = keyValuePair.Key;
            bool unlocked = keyValuePair.Value;

            if (unlocked)
            {
                accessoriesEquipped.Add(charID, new List<AccData>());
            }
        }


        foreach (KeyValuePair<int, int> accessory in saveFile.accessoriesEquipped)
        {
            int savedID = accessory.Key;
            int slotID = accessory.Value;

            data = new AccData(savedID);
            Accessory equippedAccessory = accessoryPrefabs.Find(prefab => prefab.ID == data.accID);

            if (!data.TryAssign(equippedAccessory))
            {
                Debug.LogError(equippedAccessory.name + " is not assignable.");
                continue;
            }

            if (!accessoriesEquipped.ContainsKey(slotID))
            {
                accessoriesEquipped.Add(slotID, new List<AccData>());
            }

            accessoriesEquipped[slotID].Add(data);
        }


        foreach (Accessory accessory in accessoryPrefabs)
        {
            // ���⼭�� �Ǹ��ϴ� �Ǽ������� ������.
            // prefab �߿��� �Ǹ����� �ʴ� �͵� �ִµ�, �⺻ �Ǽ������� 
            if (accessory is AccessoryNotForSale)
            {
                baseAccessories.Add(accessory);
            }
            else
            {
                accessoriesOnSale.Add(accessory);
            }
        }
    }


    private bool CheckValidity()
    {
        // accessoryOnShelf�� �ִ� ID�� accessoryPrefabs�� �ִ��� Ȯ��
        // accessoryEquipped�� �ִ� ID�� accessoryPrefabs�� �ִ��� Ȯ��
        // accessoryEquipped�� accessoryOnShelf�� �ߺ��� ID�� �ִ��� Ȯ��

        bool isValid = true;

        for (int i = 0; i < saveFile.accessoriesOnShelf.Count; i++)
        {
            int saveID = saveFile.accessoriesOnShelf[i];

            //��ĭ�� �ǹ�
            if (saveID == -1)
            {
                continue;
            }

            AccData data = new AccData(saveID);

            if (!accessoryPrefabs.Exists(accessory => accessory.ID == data.accID))
            {
                Debug.LogError("Accessory ID " + saveID + " is not found in accessoryPrefabs.");
                isValid = false;
            }
        }

        foreach (KeyValuePair<int, int> accessory in saveFile.accessoriesEquipped)
        {
            int saveID = accessory.Key;
            AccData data = new AccData(saveID);

            if (!accessoryPrefabs.Exists(prefab => prefab.ID == data.accID))
            {
                Debug.LogError("Accessory ID " + saveID + " is not found in accessoryPrefabs.");
                isValid = false;
            }

            if (saveFile.accessoriesOnShelf.Contains(saveID))
            {
                Debug.LogError("Accessory ID " + saveID + " is found both shelf and slot.");
                isValid = false;
            }
        }

        return isValid;
    }

    public bool GetNew(AccData accData)
    {
        int id = accData.saveID;

        if (IDExist(id))
        {
            Debug.LogError("Already have same saveID.");
            return false;
        }

        PutAccDataOnShelf(accData);
        WriteSaveFile();
        getNewAccEC.Raise();
        return true;
    }

    public bool GetNewFromRewardStage(AccData accData)
    {
        if (!GetNew(accData))
        {
            return false;
        }

        saveLoadManager.SaveRewardStageAccGainTime();
        return true;
    }


    public void Equip(AccData accDataToEquip)
    {
        AccessoryManager accManager = characterManager.GetCurrentCharObject().GetComponent<AccessoryManager>();

        if (ReferenceEquals(accManager, null))
        {
            Debug.LogError("AccessoryManager is not found in the player.");
            return;
        }

        AccData accDataUnequipped = accManager.Equip(accDataToEquip);
        bool hasUnequipped = !ReferenceEquals(accDataUnequipped, null);

        if (hasUnequipped)
        {
            WriteSwap(accDataToEquip, accDataUnequipped, characterManager.GetCurrentCharIndex());
        }
        else
        {
            WriteEquip(accDataToEquip, characterManager.GetCurrentCharIndex());
        }

        WriteSaveFile();
    }
    public void RemoveAccessoryOnShelf(AccData data)
    {
        if (!TryRemoveAccDataOnShelf(data))
        {
            Debug.LogError("Accessory to equip is not found in onShelf.");
            return;
        }

        WriteSaveFile();
    }

    public bool Upgrade(AccData modifiedData)
    {
        int saveID = modifiedData.saveID;

        if (saveLoadManager.SaveFile.numUpgradeBooster == 0) 
        {
            saveLoadManager.DataCorrupted();
            return false;
        }

        if (IDExist(saveID))
        {
            saveLoadManager.DataCorrupted();
            return false;
        }

        saveLoadManager.SaveFile.numUpgradeBooster -= 1;

        WriteSaveFile();
        return true;
    }

    private void WriteSwap(AccData accDataToEquip, AccData accDataToUnequip, int charID)
    {
        if (!TryRemoveAccDataOnShelf(accDataToEquip, out int shelfIndex))
        {
            Debug.LogError("Accessory to equip is not found in onShelf.");
        }

        if (!TryRemoveAccDataEquipped(accDataToUnequip, charID))
        {
            Debug.LogError("Accessory to unequip is not found in equipped list.");
        }

        // ������ �Ǽ������� shelf���� ���� �ڸ��� �����Ѵٴ� ������ ����.
        // �� Ż���� �Ǽ������� �� �ڸ��� ���� ã�ƾ��Ѵ�.
        PutAccDataOnShelf(accDataToUnequip);
        FitAccIntoSlot(accDataToEquip, charID);
    }


    // Equip, Swap, Unequip�� ũ�ν�üũ��? onShelf�� ��������
    // accManager���� accEquipped�� ���������� ����
    // �׳� Ȯ�ο��̶� ġ��.

    private void WriteEquip(AccData accDataToEquip, int charID)
    {
        if (!TryRemoveAccDataOnShelf(accDataToEquip))
        {
            Debug.LogError("Accessory to equip is not found in onShelf.");
            return;
        }

        FitAccIntoSlot(accDataToEquip, charID);
    }

    public void Unequip(AccData accDataToUnequip, int slotID)
    {
        if (!TryRemoveAccDataEquipped(accDataToUnequip, slotID))
        {
            Debug.LogError("Accessory to unequip is not found in equipped list.");
        }

        PutAccDataOnShelf(accDataToUnequip);
    }

    private bool TryFindAccDataEquipped(AccData accData, int charID, out int equippedIndex)
    {
        equippedIndex = accessoriesEquipped[charID].FindIndex(data => ReferenceEquals(data, accData));
        return equippedIndex != -1;
    }

    private bool TryRemoveAccDataEquipped(AccData quippedData, int charID)
    {
        if (!TryFindAccDataEquipped(quippedData, charID, out int equippedIndex))
        {
            return false;
        }

        accessoriesEquipped[charID].RemoveAt(equippedIndex);
        return true;
    }

    private bool TrySwapAccDataEquipped(AccData quippedData, AccData toEquipData, int charID)
    {
        if (!TryFindAccDataEquipped(quippedData, charID, out int equippedIndex))
        {
            return false;
        }

        accessoriesEquipped[charID][equippedIndex] = toEquipData;
        return true;
    }

    private bool TryFindAccessoryOnShelf(AccData accData, out int shelfIndex)
    {
        shelfIndex = accessoriesOnShelf.FindIndex(kvp => kvp.Item1?.ID == accData.accID);
        return shelfIndex != -1;
    }

    private bool TryFindEmptySlotOnShelf(out int shelfIndex)
    {
        shelfIndex = accessoriesOnShelf.FindIndex(kvp => kvp.Item1 == null);
        return shelfIndex != -1;
    }

    private int FindShelfIndexToEquip(AccData accData)
    {
        int emptyIndex = -1;

        for (int i = 0; i < accessoriesOnShelf.Count; i++)
        {
            Accessory accessory = accessoriesOnShelf[i].Item1;
            List<AccData> accDatas = accessoriesOnShelf[i].Item2;

            // ���� ���� �� �ڸ��� ã�´�.
            if (accessory == null && emptyIndex == -1)
            {
                emptyIndex = i;
            }

            // ���� ���� �ִ� ������ �Ǽ����� �� ��
            else if (accessory == accData.accessory)
            {
                return i;
            }
        }

        if (emptyIndex == -1)
        {
            return accessoriesOnShelf.Count;
        }

        return emptyIndex;
    }

    private bool TryRemoveAccDataOnShelf(AccData accData)
    {
        if (!TryFindAccessoryOnShelf(accData, out int shelfIndex))
        {
            return false;
        }

        bool result = accessoriesOnShelf[shelfIndex].Item2.Remove(accData);

        if (accessoriesOnShelf[shelfIndex].Item2.Count == 0)
        {
            accessoriesOnShelf[shelfIndex] = (null, new List<AccData>());
        }

        return result;
    }

    private bool TryRemoveAccDataOnShelf(AccData accData, out int shelfIndex)
    {
        if (!TryFindAccessoryOnShelf(accData, out shelfIndex))
        {
            return false;
        }

        if (!accessoriesOnShelf[shelfIndex].Item2.Remove(accData))
        {
            return false;
        }

        if (accessoriesOnShelf[shelfIndex].Item2.Count == 0)
        {
            accessoriesOnShelf[shelfIndex] = (null, new List<AccData>());
        }

        return true;
    }

    private void FitAccIntoSlot(AccData accData, int slotID)
    {
        if (!accessoriesEquipped.ContainsKey(slotID))
        {
            accessoriesEquipped[slotID] = new List<AccData>();
        }

        accessoriesEquipped[slotID].Add(accData);
    }

    private void PutAccDataOnShelf(AccData accData)
    {
        int shelfIndex = FindShelfIndexToEquip(accData);

        if (shelfIndex >= accessoriesOnShelf.Count)
        {
            accessoriesOnShelf.Add((accData.accessory, new List<AccData> { accData }));
            return;
        }

        Accessory accessory = accessoriesOnShelf[shelfIndex].Item1;

        if (accessory == null)
        {
            accessoriesOnShelf[shelfIndex] = (accData.accessory, new List<AccData> { accData });
            return;
        }

        if (!ReferenceEquals(accData.accessory, accessory))
        {
            Debug.LogError("Wrong shelfIndex, Accessory to equip is not the same type with the accessory on the shelf.");
            return;
        }

        accessoriesOnShelf[shelfIndex].Item2.Add(accData);
        // �ߺ��� ������ �ִ��� Ȯ���ؾ���.

    }

    public void WriteSaveFile()
    {
        List<int> accessoriesOnShelfTemp = new List<int>();
        Dictionary<int, int> accessoriesEquippedTemp = new Dictionary<int, int>();

        Accessory accessory;
        List<AccData> datas;

        foreach (var pair in accessoriesOnShelf)
        {
            accessory = pair.Item1;
            datas = pair.Item2;

            if (ReferenceEquals(accessory, null))
            {
                accessoriesOnShelfTemp.Add(-1);
                continue;
            }

            foreach (var data in datas)
            {
                accessoriesOnShelfTemp.Add(data.saveID);
            }
        }

        int slotID;

        foreach (KeyValuePair<int, List<AccData>> kvp in accessoriesEquipped)
        {
            slotID = kvp.Key;
            datas = kvp.Value;
            foreach (AccData data in datas)
            {
                accessoriesEquippedTemp.Add(data.saveID, slotID);
            }
        }

        saveFile.accessoriesOnShelf = accessoriesOnShelfTemp;
        saveFile.accessoriesEquipped = accessoriesEquippedTemp;
        saveLoadManager.Save();
        DataChanged?.Invoke();
    }

    public bool TryGetBaseAccData(int charID, int accID, out AccData accData, bool saveData = true)
    {
        accData = null;
        AccessoryNotForSale accessory = baseAccessories.Find(acc => acc.ID == accID) as AccessoryNotForSale;

        if (ReferenceEquals(accessory, null))
        {
            saveLoadManager.DataCorrupted();
            return false;
        }

        if (accessoriesOnSale.Contains(accessory))
        {
            // Ư�� ĳ���Ϳ� ���� �Ǽ����� ���̺� ����� ���ư��� ���⿡ ������ �� ����.
            // Ư�� ĳ���Ͱ� �Ǽ������� ���� ��� ���� ������.

            // accessoriesLocked => accessoriesOnSale�� �ٲ�鼭 base acc�� �׳� ���⿡ ������ �ȵ�
            Debug.LogError("Base accessory is used by others but TryGetBaseAccessory is called.");
            return false;
        }

        if (accessoriesEquipped.ContainsKey(charID) && accessoriesEquipped[charID].Any(data => data.accID == accID))
        {
            Debug.LogError("Have equipped the accessory but TryGetBaseAccessory is called");
            return false;
        }

        // ������ �а�, accData �����

        accData = new AccData(accessory, new AccStats(accessory));

        if (saveData)
        {
            SaveEquippedAccessory(charID, accData);
        }

        return true;
    }

    public void SaveEquippedAccessory(int charID, AccData accData)
    {
        if (ReferenceEquals(accData, null))
        {
            saveLoadManager.DataCorrupted();
            return;
        }

        if (!accessoriesEquipped.ContainsKey(charID))
        {
            accessoriesEquipped.Add(charID, new List<AccData>());
        }

        accessoriesEquipped[charID].Add(accData);
        WriteSaveFile();
    }

    public bool TryGetAccessoryInAccUnlockItem(int slotID, int accID, out AccData accData)
    {
        if (!saveLoadManager.IsRewardStageAccAvailable())
        {
            accData = default;
            return false;
        }

        AccessoryNotForSale accessory = baseAccessories.Find(acc => acc.ID == accID) as AccessoryNotForSale;

        if (accessory == default)
        {
            Debug.LogError("Accessory ID " + accID + " is not found in accessoryPrefabs or is not AccessoryNotForSale");
            accData = default;
            return false;
        }


        int saveID = AccData.GetSaveID(accessory);

        AccessoryRank rank = RandomExtenstion.GetRandomRank();
        List<AccStats> stats = accStatsDatabase.GetRandomAccessoryStats(accessory, rank, 1);

        if (stats.Count == 0)
        {
            accData = default;
            return false;
        }

        accData = new AccData(accessory, stats[0]);
        return true;
    }

    private void GiveIDToUnassigned()
    {
        int numHat = 0, numGlasses = 0;
        List<int> ids = new List<int>();

        foreach (Accessory accessory in accessoryPrefabs)
        {
            if (accessory.ID == -1)
            {
                continue;
            }

            if (accessory.AccessoryType == AccessoryType.Hat)
            {
                ids.Add(accessory.ID);
                numHat++;
            }
            else if (accessory.AccessoryType == AccessoryType.Glasses)
            {
                ids.Add(accessory.ID);
                numGlasses++;
            }

            // sth else to count
        }

        // id�� �������� Ȯ��

        if (HasRedundant(ids, out List<int> duplicants))
        {
            Debug.LogError("Duplicated ID");
            return;
        }

        // check if the IDs are not increase by 1

        foreach (Accessory accessory in accessoryPrefabs)
        {
            if (accessory.ID != -1)
            {
                if (accessory.AccessoryType == AccessoryType.Hat)
                {
                    if (accessory.ID >= numHat)
                    {
                        Debug.LogError("Hat ID is invalid.");
                        return;
                    }
                }
                else if (accessory.AccessoryType == AccessoryType.Glasses)
                {
                    if (accessory.ID >= GLASSES_ID_OFFSET + numGlasses)
                    {
                        Debug.LogError("Glasses ID is invalid.");
                        return;
                    }
                }

                // sth else to count
            }
        }






        foreach (Accessory accessory in accessoryPrefabs)
        {
            if (accessory.ID == -1)
            {
                if (accessory.AccessoryType == AccessoryType.Hat)
                {
                    accessory.SetID(numHat);
                    numHat++;
                }
                else if (accessory.AccessoryType == AccessoryType.Glasses)
                {
                    accessory.SetID(GLASSES_ID_OFFSET + numGlasses);
                    numGlasses++;
                }

                // sth else to count
            }
        }
    }

    private bool HasRedundant(List<int> ids, out List<int> duplicants)
    {
        duplicants = new List<int>();
        bool result = false;
        HashSet<int> seenIds = new HashSet<int>();
        foreach (int id in ids)
        {
            if (!seenIds.Add(id)) // If the add operation fails, it means the id is already in the set
            {
                Debug.LogError("Duplicated ID: " + id);
                duplicants.Add(id);
                result = true; // Duplicate found
            }
        }
        return result;
    }

    public bool TryGetAccDataBySaveID(int saveID, out AccData accData)
    {
        accData = new AccData(saveID);
        int accID = accData.accID;
        Accessory accessory = accessoryPrefabs.Find(prefab => prefab.ID == accID);
        
        if (ReferenceEquals(accessory, null))
        {
            return false;
        }

        return accData.TryAssign(accessory);
    }

    private bool IDExist(int saveID)
    {
        if (saveFile.accessoriesOnShelf.Contains(saveID))
        {
            Debug.Log("Already have this stats");
            return true;
        }

        foreach (int accID in saveFile.accessoriesEquipped.Keys)
        {
            if (accID == saveID)
            {
                Debug.Log("Already have this stats");
                return true;
            }
        }

        return false;
    }
}