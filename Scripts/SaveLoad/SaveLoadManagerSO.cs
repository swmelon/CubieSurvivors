using UnityEngine;
using ES3Types;
using System;
using System.Collections.Generic; // Ensure ES3 Types are available for saving custom classes


[CreateAssetMenu(fileName = "SaveLoadManager", menuName = "ScriptableObjects/SaveLoadManager", order = SOAssetMenuIndex.SaveLoad)]
public class SaveLoadManagerSO : ScriptableObject, IDependentInitialization
{
    public event Action OnAdsRemoved;

    [SerializeField]
    private bool resetSaveData = false;

    [SerializeField]
    private bool resetFullUnlock = false;

    [SerializeField]
    private IntChannelSO rewardStageIndexChannel;

    [SerializeField]
    private PopupScreenController popupScreenController;

    [SerializeField]
    private EventChannelSO getNewAccEC;

    public SaveFile SaveFile => saveFile;
    private SaveFile saveFile;

    private string saveFileKey = "PlayerStats"; // Unique key for the save file
    private bool loaded = false;
    private int dayInSeconds = 86400;
    public void Initialize()
    {
        ReadSaveFile();
        SetInitialSettings();
    }

    private void OnEnable()
    {
        getNewAccEC.Subscribe(NoticeShelf);
    }

    private void OnDisable()
    {
        getNewAccEC.Unsubscribe(NoticeShelf);
    }

    public void ReadSaveFile()
    {
        if (ES3.KeyExists(saveFileKey) && !resetSaveData)
        {
            // Key exists, load existing stats
            saveFile = ES3.Load<SaveFile>(saveFileKey);
            Debug.Log("Loaded Player Stats from storage.");
        }
        else
        {
            // Key does not exist, initialize with defaults
            saveFile = new SaveFile(resetFullUnlock);
            Save();
            Debug.Log("Initialized Player Stats with default values.");
        }

        Debug.Log(saveFile.NumARNMs + " coins");

        loaded = true;
    }

    public SaveFile Save()
    {
        ES3.Save(saveFileKey, saveFile);
        Debug.Log("Saved Player Stats.");
        return saveFile;
    }

    public void SetInitialSettings()
    {
        string localeCode = saveFile.Language;

        if (localeCode != "")
        {
            LanguageManager.TryChangeLanguage(saveFile.Language);
        }
    }

    public void ChangeLanguage(string localeCode)
    {
        if (!LanguageManager.TryChangeLanguage(localeCode))
        {
            return;
        }

        saveFile.Language = localeCode;
        Save();
    }

    public void DataCorrupted()
    {
        Debug.LogError("Data corrupted.");
    }

    private void ResetSaveDataAndReload()
    {
        Debug.Log("Resetting save data");
        saveFile = new SaveFile();
        Save();
        Application.Quit();
    }

    public bool HasPendingCardSelection()
    {
        return saveFile.pendingSelectionCards.Count > 0;
    }

    public void AddUpgradeBooster()
    {
        saveFile.numUpgradeBooster++;
        Save();
    }

    public void RemoveAds()
    {
        saveFile.AdsRemoved = true;
        Save();
        OnAdsRemoved?.Invoke();
    }

    public bool AdsRemoved()
    {
        return saveFile.AdsRemoved;
    }

    public bool IsRewardStageAccAvailable()
    {
        int rewardStageIndex = rewardStageIndexChannel.Value;
        var accGainTimes = saveFile.LastRewardStageAccGainTime;
        DateTime now = DateTime.UtcNow;

        if (!accGainTimes.ContainsKey(rewardStageIndex))
        {
            // no save data
            return true;
        }

        if (DateTime.TryParse(saveFile.LastRewardStageAccGainTime[rewardStageIndex], out  DateTime lastAccGainTime))
        {
            Debug.Log("Last accessory gain time loaded successfully.");

            lastAccGainTime = lastAccGainTime.ToUniversalTime();

            if ((now - lastAccGainTime).TotalSeconds >= dayInSeconds)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            Debug.Log("Failed to parse last accessory gain time, setting to current time.");
            return false;
        }
    }

    public void SaveRewardStageAccGainTime()
    {
        var accGainTimes = saveFile.LastRewardStageAccGainTime;
        int rewardStageIndex = rewardStageIndexChannel.Value;
        DateTime now = DateTime.UtcNow;

        accGainTimes[rewardStageIndex] = now.ToString("o");
        Save();
    }

    public bool TryGetLocale(out string locale)
    {
        locale = saveFile.Language;
        return !string.IsNullOrEmpty(locale);
    }

    public void GetCoins(int amount)
    {
        saveFile.NumCoins += amount;
        Save();
    }

    public void NoticeShelf()
    {
        saveFile.ShelfExclamationMark = true;
        Save();
    }
}