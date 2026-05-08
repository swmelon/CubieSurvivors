using System.Collections.Generic;
using UnityEngine;

public enum PermanentUpgradeOption
{
    Damage,
    CriticalProb,
    CriticalDamage,
    Health,
    ExtraLife,
}


[CreateAssetMenu(fileName = "PermanentUpgradableStat", menuName = "ScriptableObjects/CharacterAbillity/PermanentUpgradableStat", order = SOAssetMenuIndex.Character)]
public class PermanentUpgradableStat : ScriptableObject, IDependentInitialization
{
    [SerializeField]
    private SaveLoadManagerSO saveLoadManager;
    
    [SerializeField]
    private GemManagerSO coinManager;
    
    public Upgradable<int> UAdditionalHealth;
    public Upgradable<float> UDamageMultiplier;
    public Upgradable<float> UCriticalProb;
    public Upgradable<float> UCriticalDamageMultiplier;
    public Upgradable<int> UExtraLife;
    
    private Upgradable<int> UAdditionalHealthCost;
    private Upgradable<int> UDamageMultiplierCost;
    private Upgradable<int> UCriticalProbCost;
    private Upgradable<int> UCriticalDamageMultiplierCost;
    private Upgradable<int> UExtraLifeCost;

    private Dictionary<PermanentUpgradeOption, Upgradable<int>> permanentUpgradeCosts = 
        new Dictionary<PermanentUpgradeOption, Upgradable<int>>();

    
    private SaveFile saveFile;
    
    private struct UpgradableStat
    {
        public List<int> AdditionalHealth;
        public List<float> DamageMultiplier;
        public List<float> CriticalProb;
        public List<float> CriticalDamageMultiplier;
        public List<int> ExtraLife;

        public List<int> AdditionalHealthCost;
        public List<int> DamageMultiplierCost;
        public List<int> CriticalProbCost;
        public List<int> CriticalDamageMultiplierCost;
        public List<int> ExtraLifeCost;
    }
    
    public void Initialize()
    {
        UpgradableStat upgradableStat = UpgradeExtension.ReadUpgradableStat<UpgradableStat>(
            nameof(PermanentUpgradableStat));

        saveFile = saveLoadManager.SaveFile;
        permanentUpgradeCosts.Clear();

        UAdditionalHealth = new Upgradable<int>(upgradableStat.AdditionalHealth);
        UDamageMultiplier = new Upgradable<float>(upgradableStat.DamageMultiplier);
        UCriticalProb = new Upgradable<float>(upgradableStat.CriticalProb);
        UCriticalDamageMultiplier = new Upgradable<float>(upgradableStat.CriticalDamageMultiplier);
        UExtraLife = new Upgradable<int>(upgradableStat.ExtraLife);

        UAdditionalHealthCost = new Upgradable<int>(upgradableStat.AdditionalHealthCost);
        UDamageMultiplierCost = new Upgradable<int>(upgradableStat.DamageMultiplierCost);
        UCriticalProbCost = new Upgradable<int>(upgradableStat.CriticalProbCost);
        UCriticalDamageMultiplierCost = new Upgradable<int>(upgradableStat.CriticalDamageMultiplierCost);
        UExtraLifeCost = new Upgradable<int>(upgradableStat.ExtraLifeCost);

        UDamageMultiplier.SetGrade(saveFile.DamageMultiplierLevel);
        UCriticalProb.SetGrade(saveFile.CriticalProbLevel);
        UCriticalDamageMultiplier.SetGrade(saveFile.CriticalDamageMultiplierLevel);
        UAdditionalHealth.SetGrade(saveFile.AdditionalHealthLevel);
        UExtraLife.SetGrade(saveFile.ExtraLifeLevel);

        SetupUpgradables();
        InitializeCosts();
    }
    

    private void SetupUpgradables()
    {
        UAdditionalHealthCost.Upgraded += () =>
        {
            UAdditionalHealth.Upgrade();
            saveFile.AdditionalHealthLevel = UAdditionalHealth.GetGrade();
            saveLoadManager.Save();
        };
        UDamageMultiplierCost.Upgraded += () =>
        {
            UDamageMultiplier.Upgrade();
            saveFile.DamageMultiplierLevel = UDamageMultiplier.GetGrade();
            saveLoadManager.Save();
        };
        UCriticalProbCost.Upgraded += () =>
        {
            UCriticalProb.Upgrade();
            saveFile.CriticalProbLevel = UCriticalProb.GetGrade();
            saveLoadManager.Save();
        };
        UCriticalDamageMultiplierCost.Upgraded += () =>
        {
            UCriticalDamageMultiplier.Upgrade();
            saveFile.CriticalDamageMultiplierLevel = UCriticalDamageMultiplier.GetGrade();
            saveLoadManager.Save();
        };
        UExtraLifeCost.Upgraded += () =>
        {
            UExtraLife.Upgrade();
            saveFile.ExtraLifeLevel = UExtraLife.GetGrade();
            saveLoadManager.Save();
        };
    }
    
    private void InitializeCosts()
    {
        UAdditionalHealthCost.SetGrade(saveFile.AdditionalHealthLevel);
        UDamageMultiplierCost.SetGrade(saveFile.DamageMultiplierLevel);
        UCriticalProbCost.SetGrade(saveFile.CriticalProbLevel);
        UCriticalDamageMultiplierCost.SetGrade(saveFile.CriticalDamageMultiplierLevel);
        UExtraLifeCost.SetGrade(saveFile.ExtraLifeLevel);

        if (!CheckValid())
        {
            Debug.LogWarning("Invalid UpgradableStat");
        }

        permanentUpgradeCosts.Add(PermanentUpgradeOption.Damage, UDamageMultiplierCost);
        permanentUpgradeCosts.Add(PermanentUpgradeOption.CriticalProb, UCriticalProbCost);
        permanentUpgradeCosts.Add(PermanentUpgradeOption.CriticalDamage, UCriticalDamageMultiplierCost);
        permanentUpgradeCosts.Add(PermanentUpgradeOption.Health, UAdditionalHealthCost);
        permanentUpgradeCosts.Add(PermanentUpgradeOption.ExtraLife, UExtraLifeCost);
    }
    
    private void Validate(UpgradableStat upgradableStat)
    {
    }
    
    public bool IsUpgradable(PermanentUpgradeOption option)
    {
        return permanentUpgradeCosts[option].Value < coinManager.AUs;
    }
    
    public void Upgrade(PermanentUpgradeOption option)
    {
        if (coinManager.PayAuranium(permanentUpgradeCosts[option].Value))
        {
            permanentUpgradeCosts[option].Upgrade();
        }
        else
        {
            Debug.LogWarning("Upgrade() called when unable to upgrade.");
        }
    }

    
    public bool TryGetCost(PermanentUpgradeOption option, out Upgradable<int> UCost)
    {
        return permanentUpgradeCosts.TryGetValue(option, out UCost);
    }
    
    public bool IsUpgradableAtLeastOne()
    {
        foreach (var pair in permanentUpgradeCosts)
        {
            if (pair.Value.Value < saveFile.NumARNMs)
            {
                return true;
            }
        }

        return false;
    }
    
    
    public void ResetAppearanceCount()
    {
        saveFile.LastAppearanceCountOfPermanentUpgradeStage = 0;
    }

    public bool CanUpgradeAnything()
    {
        int numARNMs = saveFile.NumARNMs;

        bool upgradable = UAdditionalHealthCost.IsUpgradable() && UAdditionalHealthCost.Value <= numARNMs;
        upgradable |= UDamageMultiplierCost.IsUpgradable() && UDamageMultiplierCost.Value <= numARNMs;
        upgradable |= UCriticalProbCost.IsUpgradable() && UCriticalProbCost.Value <= numARNMs;
        upgradable |= UCriticalDamageMultiplierCost.IsUpgradable() && UCriticalDamageMultiplierCost.Value <= numARNMs;
        upgradable |= UExtraLifeCost.IsUpgradable() && UExtraLifeCost.Value <= numARNMs;

        return upgradable;
    }

    private bool CheckValid()
    {
        return UAdditionalHealth.GetGrade() == UAdditionalHealthCost.GetGrade() &&
            UDamageMultiplier.GetGrade() == UDamageMultiplierCost.GetGrade() &&
            UCriticalProb.GetGrade() == UCriticalProbCost.GetGrade() &&
            UCriticalDamageMultiplier.GetGrade() == UCriticalDamageMultiplierCost.GetGrade() &&
            UExtraLife.GetGrade() == UExtraLifeCost.GetGrade();
    }
}
