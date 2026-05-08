using UnityEngine;

public class AccData
{
    private const int saveIdLength = 9;
    private const int accIdSubstringStart = 1;
    private const int accIdSubstringLength = 4;
    private const int attackSubstringStart = 5;
    private const int defenseSubstringStart = 6;
    private const int agilitySubstringStart = 7;
    private const int luckSubstringStart = 8;
    private const int statSubstringLength = 1;
    private const int saveIdPrefix = 1;
    private const int saveIdPrefixMultiplier = 100000000;
    private const int saveIdAccIdMultiplier = 10000;
    private const int statSaveBaseOffset = 5;

    public Accessory accessory;
    public AccStats accessoryStats;
    public int accID;
    public int statID => accessoryStats.GetStatID();
    public int saveID => GetSaveID();

    public AccData(Accessory accessory, AccStats accessoryStats)
    {
        this.accessoryStats = accessoryStats;
        this.accessory = accessory;
        this.accID = accessory.ID;
    }

    public AccData(int saveID)
    {
        string idStr = saveID.ToString();

        if (idStr.Length != saveIdLength)
        {
            Debug.LogError($"Accessory ID {saveID} is not 8 digits long.");
        }

        accID = int.Parse(idStr.Substring(accIdSubstringStart, accIdSubstringLength));
        int attack = int.Parse(idStr.Substring(attackSubstringStart, statSubstringLength));
        int defense = int.Parse(idStr.Substring(defenseSubstringStart, statSubstringLength));
        int agility = int.Parse(idStr.Substring(agilitySubstringStart, statSubstringLength));
        int luck = int.Parse(idStr.Substring(luckSubstringStart, statSubstringLength));

        accessoryStats = new AccStats(attack, defense, agility, luck);
        accessory = null;
    }

    private int GetSaveID()
    {
        return saveIdPrefix * saveIdPrefixMultiplier + accessory.ID * saveIdAccIdMultiplier + statID;
    }

    public bool Assignable(Accessory accToAssign)
    {
        return ReferenceEquals(accessory, null) 
            && accToAssign != null 
            && accID == accToAssign.ID;
    }

    public bool TryAssign(Accessory accToAssign)
    {
        if (Assignable(accToAssign))
        {
            accessory = accToAssign;
            return true;
        }

        return false;
    }

    public void ChangeStats(AccStats newStats)
    {
        accessoryStats = newStats;
    }

    public int GetRank()
    {
        return accessoryStats.GetRank();
    }

    public static int GetSaveID(AccessoryNotForSale accNotForSale)
    {
        string idStr = "1" + accNotForSale.ID.ToString("D4")  // Ensure the accessory ID is three digits
                    + (accNotForSale.Attack + statSaveBaseOffset).ToString()
                    + (accNotForSale.Defense + statSaveBaseOffset).ToString()
                    + (accNotForSale.Agility + statSaveBaseOffset).ToString()
                    + (accNotForSale.Luck + statSaveBaseOffset).ToString();
        return int.Parse(idStr);
    }
}