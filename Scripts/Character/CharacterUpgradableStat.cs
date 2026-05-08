using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using Local.Scripts.Extensions;


[CreateAssetMenu(fileName = "CharacterUpgradableStat", menuName = "ScriptableObjects/CharacterAbillity/CharacterUpgradableStat", order = SOAssetMenuIndex.Character)]
public class CharacterUpgradableStat : ScriptableObject, IUpgradableContainer, IDependentInitialization
{
    [SerializeField]
    private SymbolIconContainer symbolContainer;

    public static event Action<float> MagnetRangeChanged;
    public static event Action<float> SpeedChanged;
    public static event Action<int> MaxHealthChanged;
    public static event Action<int> NumCardsChanged;

    public List<IUpgradable> Upgradables
    {
        get
        {
            List<IUpgradable> exposableUpgradables = new List<IUpgradable>();
            foreach (IUpgradable upgradable in upgradables)
            {
                if (ReferenceEquals(upgradable, UNumCards) && RandomExtenstion.FiftyFifty())
                {
                    continue;
                }

                if (upgradable.IsExposed() && upgradable.IsUpgradable())
                {
                    exposableUpgradables.Add(upgradable);
                }
            }
            return exposableUpgradables;
        }
    }
    
    [SerializeField]
    private List<EventChannelSO> resetEvents;
    
    [SerializeField]
    private Sprite speedUpgradeIcon, maxHealthUpgradeIcon, healPeriodUpgradeIcon, magnetRangeUpgradeIcon, addCardUpgradeIcon;
    private struct UpgradableStat
    {
        public List<float> Speed, HealPeriod, MagnetRange;//, UCriticalProb, UCriticalDamage;
        public List<int> MaxHealth, NumCards;
    }

    private Upgradable<float> USpeed; //, UCriticalProb, UCriticalDamage;
    private Upgradable<float> UMagnetRange;//, UHealPeriod;
    private Upgradable<int> UMaxHealth;
    private Upgradable<int> UNumCards;
    
    private List<IUpgradable> upgradables;
    
    public void Initialize()
    {
        SetupUpgradables();
        RegisterUpgradables();

        for (int i = 0; i < resetEvents.Count; i++)
        {
            resetEvents[i].SubscribeLast(ResetUpgrade) ;
        }
    }
    
    private void SetupUpgradables()
    {   
        UpgradableStat upgradableStat = UpgradeExtension.ReadUpgradableStat<UpgradableStat>(GetType().Name);
        InitializeUpgradables(upgradableStat);
        
        USpeed.Upgraded += () => SpeedChanged?.Invoke(USpeed.Value);
        UMaxHealth.Upgraded += () => MaxHealthChanged?.Invoke(UMaxHealth.Value);
        UMagnetRange.Upgraded += () => MagnetRangeChanged?.Invoke(UMagnetRange.Value);
        UNumCards.Upgraded += () => NumCardsChanged?.Invoke(UNumCards.Value);

        USpeed.UpgradeReset += () => SpeedChanged?.Invoke(USpeed.Value);
        UMaxHealth.UpgradeReset += () => MaxHealthChanged?.Invoke(UMaxHealth.Value);
        UMagnetRange.UpgradeReset += () => MagnetRangeChanged?.Invoke(UMagnetRange.Value);
        UNumCards.UpgradeReset += () => NumCardsChanged?.Invoke(UNumCards.Value);
    }
    
    private void RegisterUpgradables()
    {
        upgradables = new List<IUpgradable>();
        
        FieldInfo[] fieldInfos;
        fieldInfos = GetType().GetFields(BindingFlags.NonPublic |
                                         BindingFlags.Instance);

        foreach (var item in fieldInfos)
        {
            if (typeof(IUpgradable).IsAssignableFrom(item.FieldType))
            {
                upgradables.Add((IUpgradable)item.GetValue(this));
            }
        }
    }
    
    private void InitializeUpgradables(UpgradableStat upgradableStat)
    {
        USpeed = new Upgradable<float>(upgradableStat.Speed, CardText.UPGRADE, CardText.SPEED,
            icon: speedUpgradeIcon, symbol: symbolContainer.MoveSpeed);
        UMaxHealth = new Upgradable<int>(upgradableStat.MaxHealth, CardText.UPGRADE, CardText.MAX_HEALTH,
            icon: maxHealthUpgradeIcon, symbol: symbolContainer.Health);
        UMagnetRange = new Upgradable<float>(upgradableStat.MagnetRange, CardText.UPGRADE, CardText.MAGNET_RANGE,
            icon: magnetRangeUpgradeIcon, symbol: symbolContainer.Magenet);
        UNumCards = new Upgradable<int>(upgradableStat.NumCards, CardText.UPGRADE, CardText.ADD_CARD, 
            icon: addCardUpgradeIcon, symbol:symbolContainer.Plus);
    }
    
    private void ResetUpgrade()
    {
        upgradables.ForEach(upgradable => upgradable.Reset());
    }
}
