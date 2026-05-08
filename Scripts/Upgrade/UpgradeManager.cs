using System;
using System.Collections.Generic;
using System.Linq;
using Local.Scripts.Extensions;
using UnityEngine;
using UnityEngine.Serialization;

public class UpgradeManager : MonoBehaviour
{
    [FormerlySerializedAs("CardSelectionScreenControlChannel")] [FormerlySerializedAs("upgradeCanvasControlChannel")] [SerializeField]
    private BooleanEventChannelSO cardSelectionScreenControlChannel;
    
    [SerializeField]
    private WeaponEventChannelSO returnWeaponChannel;
    
    [SerializeField]
    private BooleanEventChannelSO inputCanvasControlChannel;
    

    // Invoker
    [SerializeField]
    private EventChannelSO startUpgradeEventChannel, finishUpgradeEventChannel;
    
    [SerializeField]
    private CardEventChannel showCardEventChannel;
    
    // Invoker
    [SerializeField]
    private BooleanEventChannelSO pauseMenuUIControlChannel;
    
    // Subscriber
    [SerializeField]
    private WeaponEventChannelSO playerGetWeaponEventChannel;

    // Subscriber
    [SerializeField]
    private EventChannelSO startLevelUpEventChannel;
    
    [SerializeField]
    private GamePauser gamePauser;

    [SerializeField]
    private EventChannelSO gameStartEC;

    [SerializeField]
    private IntChannelSO upgradeCountChannel;

    [SerializeField]
    private CharacterUpgradableStat characterUpgradableStat;

    [SerializeField] 
    private WeaponSet weaponSet;

    [SerializeField]
    private SpecialCardsContainer specialCardsContainer;

    [SerializeField]
    private bool autoUpgrade = false;


    [Header("Option Card Icon & FX")]
    [SerializeField]
    private Sprite upgradeReculsivelyIcon;

    [SerializeField]
    private Sprite getOtherWeaponIcon, coinIcon;

    [SerializeField]
    private FXCameraChannelSO upgradeFXCamChannel, getOtherWeaponFXCamChannel;

    [SerializeField]
    private GemManagerSO gemManager;

    private FXCameraController upgradeFXCam, getOtherWeaponFXCam;
    private Action postUpgradeAction;

    private WeaponManager weaponManager;
    private List<CardData> deck;
    private List<Weapon> weapons;
    private Queue<Weapon> getWeaponQueue = new Queue<Weapon>();
    private int numUpgrades;

    private int upgradeCount = 0;
    private bool whileUpgrading = false;
    private int numUpgradeCards;
    private float specialOptionRate = 0.7f;

    private int UpgradeCount
    {
        set
        {
            upgradeCount = value;
            upgradeCountChannel.Register(upgradeCount);
        }
    }

    private void Awake()
    {
        deck = new List<CardData>();
        weaponManager = GetComponent<WeaponManager>();
        upgradeFXCamChannel.Subscribe(SetUpgradeFXCam);
        getOtherWeaponFXCamChannel.Subscribe(SetOtherWeaponFXCam);
    }

    private void OnEnable()
    {
        playerGetWeaponEventChannel.Subscribe(OnPlayerGetDroppedWeapon);
        startLevelUpEventChannel.Subscribe(UpgradeOnce);
        gameStartEC.Subscribe(ResetUpgradeCount);
        CharacterUpgradableStat.NumCardsChanged += SetNumUpgradeCards;
    }

    private void OnDisable()
    {
        playerGetWeaponEventChannel.Unsubscribe(OnPlayerGetDroppedWeapon);
        startLevelUpEventChannel.Unsubscribe(UpgradeOnce);
        gameStartEC.Unsubscribe(ResetUpgradeCount);
        CharacterUpgradableStat.NumCardsChanged -= SetNumUpgradeCards;
    }

    private void OnDestroy()
    {
        upgradeFXCamChannel.Unsubscribe(SetUpgradeFXCam);
        getOtherWeaponFXCamChannel.Unsubscribe(SetOtherWeaponFXCam);
    }

    private void OnPlayerGetDroppedWeapon(Weapon weapon)
    {
        getWeaponQueue.Enqueue(weapon);

        if (!whileUpgrading)
        {
            UpgradeWhenGetWeapon(getWeaponQueue.Dequeue());
        }
    }

    private void UpgradeOnce()
    {
        whileUpgrading = true;
        OnStartUpgrade();
        SubscribeClickEvent(FinishUpgradeQuietly);
        
        PutUpgradeCardsInDeck();
        DrawCardsOnDeck(numUpgradeCards);
        ShowCardsOnScreen();
        UpgradeCount = upgradeCount + 1;
    }

    private void UpgradeWhenGetWeapon(Weapon weapon)
    {
        whileUpgrading = true;
        OnStartUpgrade();
        SubscribeClickEvent(FinishUpgradeQuietly);

        numUpgrades = weapon.GetNumOfUpgradableTimes();
        inputCanvasControlChannel.Raise(false);

        if (weaponManager.IsMountable(weapon))
        {
            NewWeaponCardData newWeaponCardData = new NewWeaponCardData(weapon, weaponManager);
            deck.Add(newWeaponCardData);
        }

        if (numUpgrades != 0 && weaponManager.HasWeapon())
        {
            IconizedAction recursiveUpgradeAction = new IconizedAction(() => UpgradeOnlyUpgradable(numUpgrades, weapon),
                icon: upgradeReculsivelyIcon,
                optionText: CardText.UPGRADE,
                optionTextNoTranslate: CardText.GetUpgradeNTime(numUpgrades));
            recursiveUpgradeAction.Name = CardText.OPTION;
            recursiveUpgradeAction.SetParticleIconCam(upgradeFXCam);
            deck.Add(new ActionCardData(recursiveUpgradeAction));
        }
      
        IconizedAction otherNewWeaponAction = new IconizedAction(() => ChooseNewWeapon(weapon),
            icon: getOtherWeaponIcon,
            optionText: CardText.OTHER_WEAPONS);
        otherNewWeaponAction.Name = CardText.OPTION;
        otherNewWeaponAction.SetParticleIconCam(getOtherWeaponFXCam);
        deck.Add(new ActionCardData(otherNewWeaponAction));
    
        ShowCardsOnScreen();
    }

    private void UpgradeOnlyUpgradable(int num, Weapon weaponLeft)
    {
        UnsubscribeClickEvent(FinishUpgradeQuietly);
        
        returnWeaponChannel.Raise(weaponLeft);
        
        if (num == 0)
        {
            FinishUpgradeQuietly();
            return;
        }
        
        if(num < 0)
        {
            throw new ArgumentException("num < 0");
        }
        
        numUpgrades = num;
        SubscribeClickEvent(UpgradeOnlyUpgradableRecursively);
        UpgradeOnlyUpgradableRecursively();
    }

    private void UpgradeOnlyUpgradableRecursively()
    {
        cardSelectionScreenControlChannel.Raise(true);
        
        if (numUpgrades == 1)
        {
            UnsubscribeClickEvent(UpgradeOnlyUpgradableRecursively);
            SubscribeClickEvent(FinishUpgradeQuietly);
        }
        else
        {
            numUpgrades--;
        }
        
        PutUpgradeCardsInDeck();
        DrawCardsOnDeck(numUpgradeCards);
        ShowCardsOnScreen();
        UpgradeCount = upgradeCount + 1;
    }

    private void ChooseNewWeapon(Weapon weaponLeft)
    {
        
        // this is used to remove old cards
        cardSelectionScreenControlChannel.Raise(true);
        
        if (!weaponSet.TryGetAvailableWeaponInstances(out List<Weapon> weapons, weaponManager, numUpgradeCards))
        {
            return;
        }

        List<Weapon> weaponToDisplay = weapons;
        
        foreach (var weapon in weaponToDisplay)
        {
            // need to check if it is mountable.
            
            NewWeaponCardData newWeaponCardData = new NewWeaponCardData(weapon, weaponManager);

            
            // return unselected weapons
            newWeaponCardData.Activated += () =>
            {
                foreach (var weaponToReturn in weaponToDisplay)
                {
                    if (!ReferenceEquals(weaponToReturn, weapon))
                    {
                        weaponSet.ReturnWeaponInstance(weaponToReturn);
                    }
                    else
                    {
                        for (int i = 0; i < weaponLeft.GetNumOfUpgradableTimes(); i++)
                        {
                            if (!weaponToReturn.UpgradeRandom())
                            {
                                break;
                            }
                        }
                    }
                }
                
                // return the weapon boss equipped
                weaponSet.ReturnWeaponInstance(weaponLeft);
            };
            
            deck.Add(newWeaponCardData);
        }
        
        ShowCardsOnScreen();
    }

    private void PutUpgradeCardsInDeck()
    {
        foreach (IUpgradable upgradable in weaponManager.Upgradables)
        {
            upgradable.GenerateRandomBonusRate();
            UpgradeCardData card = new UpgradeCardData(upgradable);
            deck.Add(card);
        }

        foreach (IUpgradable upgradable in  characterUpgradableStat.Upgradables)
        {
            upgradable.GenerateRandomBonusRate();
            deck.Add(new UpgradeCardData(upgradable));
        }

        int numCoinCard = numUpgradeCards - deck.Count;
        bool specialCardSelected = false;

        for (int i = 0; i < numCoinCard; i++)
        {
            if (!specialCardSelected && RandomExtenstion.IsHappen(specialOptionRate) 
                && specialCardsContainer.TryGetRandomSpecialCardData(out SpecialCardData specialCardData))
            {
                specialCardSelected = true;
                deck.Add(specialCardData);
                continue;
            }

            deck.Add(new CoinCardData(gemManager));
        }
    }
    
    private void DrawCardsOnDeck(int numCards)
    {
        deck.FisherShuffle();
        List<CardData> selectedCards = deck.Shuffle().Take(numCards).ToList();
        deck = selectedCards;
        
        Debug.Assert(numUpgradeCards == deck.Count, "numUpgradeCards != deck.Count");
    }
    
    private void ShowCardsOnScreen()
    {
        if (autoUpgrade)
        {
            deck[0].Activate();
            deck.Clear();
            postUpgradeAction?.Invoke();
            return;
        }

        if (deck.Count == 0)
        {
            Debug.LogError("No cards in the deck.");
            postUpgradeAction?.Invoke();
        }

        foreach (CardData card in deck)
        {
            showCardEventChannel.Raise(card);
        }
        
        deck.Clear();
    }
    
    private void SubscribeClickEvent(Action action)
    {
        CardSelectionScreen.CardClicked += action;
        postUpgradeAction = action;
    }
    
    private void UnsubscribeClickEvent(Action action)
    {
        CardSelectionScreen.CardClicked -= action;
        postUpgradeAction = null;
    }
    
    private void FinishUpgradeQuietly()
    {
        gamePauser.Pause = false;
        cardSelectionScreenControlChannel.Raise(false);
        pauseMenuUIControlChannel.Raise(true);
        inputCanvasControlChannel.Raise(true);
        whileUpgrading = false;
        UnsubscribeClickEvent(FinishUpgradeQuietly);

        if (getWeaponQueue.Count != 0)
        {
            UpgradeWhenGetWeapon(getWeaponQueue.Dequeue());
        }
    }

    private void OnStartUpgrade()
    {
        startUpgradeEventChannel.Raise();
        pauseMenuUIControlChannel.Raise(false);
        cardSelectionScreenControlChannel.Raise(true);
        gamePauser.Pause = true;

        inputCanvasControlChannel.Raise(false);
    }

    private void SetUpgradeFXCam(FXCameraController fxCam)
    {
        upgradeFXCam = fxCam;
    }

    private void SetOtherWeaponFXCam(FXCameraController fxCam)
    {
        getOtherWeaponFXCam = fxCam;
    }

    private void ResetUpgradeCount()
    {
        UpgradeCount = 0;
    }
    
    private void SetNumUpgradeCards(int num)
    {
        numUpgradeCards = num;
    }

}
