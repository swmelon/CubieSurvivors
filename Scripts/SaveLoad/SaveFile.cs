using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;


[Serializable]
public class SaveFile
{
    public int AdditionalHealthLevel;
    public int DamageMultiplierLevel;
    public int CriticalProbLevel;
    public int CriticalDamageMultiplierLevel;
    public int ExtraLifeLevel;
    public int NumARNMs;
    public int NumCoins;
    public int LastAppearanceCountOfPermanentUpgradeStage;
    public int LastSelectedCharIndex;
    // int : charID, bool : accInitialized
    public Dictionary<int, bool> charactersUnlocked = new Dictionary<int, bool>();
    public Dictionary<int, bool> eventStageUnlocked = new Dictionary<int, bool>();
    public Dictionary<int, int> scenarioUnlocked = new Dictionary<int, int>();

    // scenarioIndex * 10 + difficulty = id
    public Dictionary<int, long>scenarioMaxScore = new Dictionary<int, long>();
    public int LastSelectedScenarioIndex;

    public List<int> weaponUnlocked = new List<int>();
    public List<int> accessoriesOnShelf = new List<int>();
    public Dictionary<int, int> accessoriesEquipped = new Dictionary<int, int>();
    public bool RightJoystick = true;
    public bool Vibration = true;
    public float MusicVolume;
    public float SFXVolume;
    public int GraphicsQuality;
    public bool ShopExclamationMark = false;
    public bool ShelfExclamationMark = false;
    public bool UpgradeExclamationMark = false;
    public bool DiscoverExclamationMark = false;
    public bool ShowDiscoveredStageButton = false;
    public string LastAdShowTime = "0";
    public string LastAdDate = "0";
    public int AdsShownToday = 0;
    public int numCardsToSelect = 3;
    public int numUpgradeBooster = 0;
    public Dictionary<int, string> LastRewardStageAccGainTime = new Dictionary<int, string>();

    public List<int> pendingSelectionCards = new List<int>();
    public string Language = "";
    public bool AdsRemoved = false;

    public SaveFile()
    {
        // Initialize default values
        AdditionalHealthLevel = 0;
        DamageMultiplierLevel = 0;
        CriticalProbLevel = 0;
        CriticalDamageMultiplierLevel = 0;
        NumARNMs = 0;
        NumCoins = 0;
        LastAppearanceCountOfPermanentUpgradeStage = 0;
        LastSelectedCharIndex = 0;
        charactersUnlocked = new Dictionary<int, bool>
        {
            {0, false},
        };
        eventStageUnlocked = new Dictionary<int, bool>
        { 
        };

        // scenarioIndex, difficulty
        scenarioUnlocked = new Dictionary<int, int>
        {
            {0, 0},
        };
        LastSelectedScenarioIndex = 0;
        scenarioMaxScore = new Dictionary<int, long>
        {
        };
        accessoriesOnShelf = new List<int>();
        accessoriesEquipped = new Dictionary<int, int>
        { 
        };

        weaponUnlocked = new List<int>
        {
            0, 1, 2, 3, 4, 5, 6, 7, 8, 9
        };


        RightJoystick = false;
        Vibration = true;
        MusicVolume = 1f;
        SFXVolume = 1f;
        GraphicsQuality = 0;
        ShopExclamationMark = true;
        ShelfExclamationMark = true;
        UpgradeExclamationMark = true;
        DiscoverExclamationMark = true;
        numCardsToSelect = 3;
    }

    public SaveFile(bool unlock)
    {
        if (unlock)
        {
            // Unlock all characters
            AdditionalHealthLevel = 0;
            DamageMultiplierLevel = 0;
            CriticalProbLevel = 0;
            CriticalDamageMultiplierLevel = 0;
            NumARNMs = 0;
            NumCoins = 0;
            LastAppearanceCountOfPermanentUpgradeStage = 0;
            LastSelectedCharIndex = 0;
            charactersUnlocked = new Dictionary<int, bool>
            {
                {0, false},
                {1, false},
                {2, false},
                {3, false},
                {4, false},
                {5, false},
                {6, false},
                {7, false},
                {8, false},
                {9, false},
            };
            scenarioUnlocked = new Dictionary<int, int>
            {
                {0, 2},
                {1, 2},
                {2, 0},
            };

            LastSelectedScenarioIndex = 0;

            scenarioMaxScore = new Dictionary<int, long>
            {
            };

            eventStageUnlocked = new Dictionary<int, bool>
            {
                {0, true},
                {1, true},
            };
            accessoriesOnShelf = new List<int>
            {
            };
            accessoriesEquipped = new Dictionary<int, int>
            {
                // 모자는 0번부터, 안경은 100번부터 시작
            };

            weaponUnlocked = new List<int>
            {
                0, 1, 2, 3, 4, 5, 6, 7, 8, 9
            };

            RightJoystick = true;
            Vibration = true;
            MusicVolume = 1f;
            SFXVolume = 1f;
            GraphicsQuality = 0;
            ShopExclamationMark = true;
            ShelfExclamationMark = true;
            UpgradeExclamationMark = true;
            DiscoverExclamationMark = true;
            numCardsToSelect = 3;
        }
        else
        {
            AdditionalHealthLevel = 0;
            DamageMultiplierLevel = 0;
            CriticalProbLevel = 0;
            CriticalDamageMultiplierLevel = 0;
            NumARNMs = 0;
            NumCoins = 0;
            LastAppearanceCountOfPermanentUpgradeStage = 0;
            LastSelectedCharIndex = 0;
            charactersUnlocked = new Dictionary<int, bool>
            {
                {0, false},
            };
            scenarioUnlocked = new Dictionary<int, int>
            {
                {0, 0},
            };
            scenarioMaxScore = new Dictionary<int, long>
            {
            };
            eventStageUnlocked = new Dictionary<int, bool>
            {
            };

            accessoriesOnShelf = new List<int>
            // accID
            {
            };
            accessoriesEquipped = new Dictionary<int, int>
            // accID, slotID
            { 
            };
            weaponUnlocked = new List<int>
            {
                0, 1, 2, 3, 4, 5, 6, 7, 8, 9
            };

            RightJoystick = false;
            Vibration = true;
            MusicVolume = 1f;
            SFXVolume = 1f;
            GraphicsQuality = 0;
            ShopExclamationMark = true;
            ShelfExclamationMark = true;
            UpgradeExclamationMark = true;
            DiscoverExclamationMark = true;
            numCardsToSelect = 3;
        }
    }
}