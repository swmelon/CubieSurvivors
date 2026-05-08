
using System;
using UnityEngine;
using UnityEngine.Serialization;


[CreateAssetMenu(fileName = "GemManager", menuName = "ScriptableObjects/GemManager", order = 1)]
public class GemManagerSO : ScriptableObject, IDependentInitialization
{
    public event Action<int> NumARNMsChanged, NumCoinsChanged;
    public int AUs => saveFile.NumARNMs;
    public int Coins => saveFile.NumCoins;
    
    [SerializeField]
    private SaveLoadManagerSO saveLoadManager;

    [SerializeField]
    private Sprite coinIcon;
    
    private SaveFile saveFile;

    public Sprite CoinIcon => coinIcon;

    public void Initialize()
    {
        saveFile = saveLoadManager.SaveFile;
    }
    
    public void GetAuranium(int amount = 1)
    {
        saveFile.NumARNMs += amount;
        NumARNMsChanged?.Invoke(saveFile.NumARNMs);
    }
    
    public bool PayAuranium(int payAmount)
    {
        if (saveFile.NumARNMs < payAmount)
        {
            return false;
        }
        
        saveFile.NumARNMs -= payAmount;
        saveLoadManager.Save();
        NumARNMsChanged?.Invoke(saveFile.NumARNMs);
        return true;
    }

    public void GetCoin(int amount = 1)
    {
        saveFile.NumCoins += amount;
        NumCoinsChanged?.Invoke(saveFile.NumCoins);
    }

    public void PurchaseCoin(int amount)
    {
        saveLoadManager.GetCoins(amount);
        NumCoinsChanged?.Invoke(saveFile.NumCoins);
    }

    public bool PayCoin(int payAmount)
    {
        if (saveFile.NumCoins < payAmount)
        {
            return false;
        }
        
        saveFile.NumCoins -= payAmount;
        saveLoadManager.Save();
        NumCoinsChanged?.Invoke(saveFile.NumCoins);
        return true;
    }

    public bool Payable(int payAmount)
    {
        return saveFile.NumCoins >= payAmount;
    }
    
}
