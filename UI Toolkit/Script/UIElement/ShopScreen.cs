using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ShopScreen : MenuScreen
{
    private enum ShopTab
    {
        Others,
        Hats,
        Glassess,
        Coins,
    }

    [SerializeField]
    private BooleanEventChannelSO shopUIControlChannel;

    [SerializeField]
    private ShopPopupScreenController shopPopupScreenController;

    [SerializeField]
    private PopupScreenController popupScreenController;

    [SerializeField]
    private AccessoryStatsSelectionScreenController accessorySelectionScreenController;

    [SerializeField]
    private EventChannelSO exitShopEC;

    [SerializeField]
    private GameAccessoryManager accessoryManager;

    [SerializeField]
    private SaveLoadManagerSO saveLoadManager;

    [SerializeField]
    private GemManagerSO gemManager;

    [SerializeField]
    private VisualTreeAsset column, item, otherItem;

    [SerializeField]
    private List<OtherShopItemData> otherItemDatas;

    [SerializeField]
    private List<PremiumShopItemData> premiumShopItemDatas;

    [SerializeField]
    private Camera accessoryCamera;

    [SerializeField]
    private Color inactiveTabTextColor;

    [SerializeField]
    private Color inactiveTabImageTintColor;

    [SerializeField]
    private Sprite adsOn, adsOff;

    [SerializeField]
    private AnimationCurveContainer curveContainer;

    private Button backButton, hatsButton, glassesButton, othersButton, coinsButton;
    private ScrollView scrollView;
    private Button bannerButton;

    private Label hatsLabel, glassesLabel, othersLabel, premiumLabel;
    private Label numCoinsLabel, adsOnOffLabel, buyCoinsLabel;
    private VisualElement hatsIcon, glassesIcon, othersIcon, coinsIcon;
    private VisualElement tabBar;
    private VisualElement adsOnOffImage;
    private VisualElement numCoinsIcon;

    private const string backButtonName = "button-back";
    private const string scrollViewName = "scrollview";

    private const string hatsButtonName = "button-hats";
    private const string hatsLabelName = "label-hats";
    private const string hatsIconName = "image-hats";

    private const string glassesButtonName = "button-glasses";
    private const string glassesLabelName = "label-glasses";
    private const string glassesIconName = "image-glasses";

    private const string othersButtonName = "button-others";
    private const string othersLabelName = "label-others";
    private const string othersIconName = "image-others";

    private const string premiumButtonName = "button-premium";
    private const string premiumLabelName = "label-premium";
    private const string premiumIconName = "image-premium";

    private const string tabBarName = "bar-tab";

    private const string numCoinsLabelName = "label-num-coins";
    private const string numCoinsIconName = "image-num-coins";

    private const string adsOnOffLabelName = "label-ads-onoff";
    private const string adsOnOffIconName = "image-ads-onoff";

    private const string buyCoinsLabelName = "label-banner";
    private const string bannerButtonName = "button-banner";

    // other items

    private const string upgradeBoosterItemName = "item-upgrade-booster";
    private const string removeAdItemName = "item-remove-ad";


    private ShopTab shopTab;
    private List<Accessory> hatsOnSale;
    private List<Accessory> glassesOnSale;
    private List<VisualElement> hatColumns;
    private List<VisualElement> glassesColumns;
    private List<VisualElement> othersColumns;
    private List<VisualElement> coinsColumns;

    private LayerMask displayedAccLayer;
    private Accessory accInstanceDisplayed, accessory;
    private StyleBackground upgradeBoosterBackground;
    private VisualElementAnimator veAnimator;
    protected override void Awake()
    {
        base.Awake();

        hatsOnSale = new List<Accessory>();
        glassesOnSale = new List<Accessory>();

        hatColumns = new List<VisualElement>();
        glassesColumns = new List<VisualElement>();
        othersColumns = new List<VisualElement>();
        coinsColumns = new List<VisualElement>();

        displayedAccLayer = LayerMask.NameToLayer("Prop");


        List<Accessory> accDatas = accessoryManager.AccessoriesOnSale;

        foreach (Accessory acc in accDatas)
        {
            if (acc.AccessoryType == AccessoryType.Hat)
            {
                hatsOnSale.Add(acc);
            }
            else if (acc.AccessoryType == AccessoryType.Glasses)
            {
                glassesOnSale.Add(acc);
            }
            else
            {
                // 다른 악세서리 종류가 추가되면 탭을 추가하고 여기에
            }
        }
    }

    private void Start()
    {
        PopulateAccessories();
    }

    private void OnEnable()
    {
        shopUIControlChannel.Subscribe(ShowScreen);
    }

    private void OnDisable()
    {
        shopUIControlChannel.Unsubscribe(ShowScreen);
    }

    protected override void SetVisualElements()
    {
        base.SetVisualElements();
        backButton = screen.Q<Button>(backButtonName);
        scrollView = screen.Q<ScrollView>(scrollViewName);

        hatsButton = screen.Q<Button>(hatsButtonName);
        hatsLabel = screen.Q<Label>(hatsLabelName);
        hatsIcon = screen.Q<VisualElement>(hatsIconName);

        glassesButton = screen.Q<Button>(glassesButtonName);
        glassesLabel = screen.Q<Label>(glassesLabelName);
        glassesIcon = screen.Q<VisualElement>(glassesIconName);

        othersButton = screen.Q<Button>(othersButtonName);
        othersLabel = screen.Q<Label>(othersLabelName);
        othersIcon = screen.Q<VisualElement>(othersIconName);

        coinsButton = screen.Q<Button>(premiumButtonName);
        premiumLabel = screen.Q<Label>(premiumLabelName);
        coinsIcon = screen.Q<VisualElement>(premiumIconName);

        tabBar = screen.Q<VisualElement>(tabBarName);

        numCoinsLabel = screen.Q<Label>(numCoinsLabelName);
        numCoinsIcon = screen.Q<VisualElement>(numCoinsIconName);
        adsOnOffLabel = screen.Q<Label>(adsOnOffLabelName);
        buyCoinsLabel = screen.Q<Label>(buyCoinsLabelName);
        adsOnOffImage = screen.Q<VisualElement>(adsOnOffIconName);

        bannerButton = screen.Q<Button>(bannerButtonName);

        SetupAnimation();
    }

    protected override void RegisterButtonCallbacks()
    {
        base.RegisterButtonCallbacks();
        screen.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);

        backButton.RegisterCallback<ClickEvent>(OnClickBackBtn);
        backButton.RegisterCallback<NavigationSubmitEvent>(OnSubmitBackBtn);
        backButton.RegisterCallback<KeyDownEvent>(OnKeyDownBackBtn);

        hatsButton.RegisterCallback<ClickEvent>(OnClickHatsBtn);
        hatsButton.RegisterCallback<NavigationSubmitEvent>(OnSubmitHatsBtn);
        hatsButton.RegisterCallback<KeyDownEvent>(OnKeyDownHatsBtn);

        glassesButton.RegisterCallback<ClickEvent>(OnClickGlassesBtn);
        glassesButton.RegisterCallback<NavigationSubmitEvent>(OnSubmitGlassessBtn);
        glassesButton.RegisterCallback<KeyDownEvent>(OnKeyDownGlassesBtn);

        othersButton.RegisterCallback<ClickEvent>(OnClickOthersBtn);
        othersButton.RegisterCallback<NavigationSubmitEvent>(OnSubmitOthersBtn);
        othersButton.RegisterCallback<KeyDownEvent>(OnKeyDownOthersBtn);

        coinsButton.RegisterCallback<ClickEvent>(OnClickCoinsBtn);
        coinsButton.RegisterCallback<NavigationSubmitEvent>(OnSubmitCoinsBtn);
        coinsButton.RegisterCallback<KeyDownEvent>(OnKeyDownCoinsBtn);

        bannerButton.RegisterCallback<ClickEvent>(OnClickBannerBtn);
        bannerButton.RegisterCallback<NavigationSubmitEvent>(OnSubmitBannerBtn);
        bannerButton.RegisterCallback<KeyDownEvent>(OnKeyDownBannerBtn);
    }

    protected override void SetLocalizedText()
    {
        base.SetLocalizedText();
        hatsLabel.text = GetLocalizedString(UIText.BTN_HATS);
        glassesLabel.text = GetLocalizedString(UIText.BTN_GLASSES);
        othersLabel.text = GetLocalizedString(UIText.BTN_OTHERS);
        premiumLabel.text = GetLocalizedString(UIText.BTN_PREMIUM);

        bool adsRemoved = saveLoadManager.AdsRemoved();
        UpdateAdsState(adsRemoved);

        buyCoinsLabel.text = GetLocalizedString(UIText.LBL_BUY_COINS);
    }

    private void SetupAnimation()
    {
        if(TryGetComponent(out veAnimator))
        {
            veAnimator.SetVisualElement(numCoinsIcon);
        }
    }

    private void ShowScreen(bool val)
    {
        if (val)
        {
            ShowScreen();
        }
        else
        {
            HideScreen();
        }
    }

    public override void ShowScreen()
    {
        base.ShowScreen();
        accessoryCamera.gameObject.SetActive(true);

        gemManager.NumCoinsChanged += UpdateNumCoins;
        saveLoadManager.OnAdsRemoved += OnAdsRemoved;

        shopTab = ShopTab.Hats;

        ShowHats();
        HideGlasses();
        HideOthers();
        HideCoins();
        UpdateNumCoins(gemManager.Coins);
        PopulateOthers();
        PopulateCoins();
    }

    public override void HideScreen()
    {
        base.HideScreen();
        accessoryCamera.gameObject.SetActive(false);

        gemManager.NumCoinsChanged -= UpdateNumCoins;
        saveLoadManager.OnAdsRemoved -= OnAdsRemoved;
    }

    private void PopulateAccessories()
    {
        PopulateHats();
        PopulateGlasses();
    }

    private void PopulateHats()
    {
        for (int i = 0; i < hatsOnSale.Count; i += 2)
        {
            Accessory hatTop = hatsOnSale[i];
            Accessory hatBottom = i + 1 < hatsOnSale.Count ? hatsOnSale[i + 1] : null;

            VisualElement columnInstance = column.CloneTree();

            SetColumnInstanceStyle(columnInstance);

            VisualElement itemTop = item.CloneTree();

            ShopItemData hatTopData = new ShopItemData();
            hatTopData.SetItemVisualAndCallback(itemTop, hatTop.GetIcon(),
                hatTop, hatTop.Price, OnAccessoryItemClicked);

            columnInstance.Add(itemTop);

            if (!ReferenceEquals(hatBottom, null))
            {
                VisualElement itemBottom = item.CloneTree();
                ShopItemData hatBottomData = new ShopItemData();
                hatBottomData.SetItemVisualAndCallback(itemBottom, hatBottom.GetIcon(),
                    hatBottom, hatBottom.Price, OnAccessoryItemClicked);
                columnInstance.Add(itemBottom);
            }

            scrollView.Add(columnInstance);
            hatColumns.Add(columnInstance);
        }
    }

    private void PopulateGlasses()
    {
        for (int i = 0; i < glassesOnSale.Count; i += 2)
        {
            Accessory glassesTop = glassesOnSale[i];
            Accessory glassesBottom = i + 1 < glassesOnSale.Count ? glassesOnSale[i + 1] : null;

            VisualElement columnInstance = column.CloneTree();

            SetColumnInstanceStyle(columnInstance);

            VisualElement itemTop = item.CloneTree();

            ShopItemData glassesTopData = new ShopItemData();
            glassesTopData.SetItemVisualAndCallback(itemTop, glassesTop.GetIcon(),
                glassesTop, glassesTop.Price, OnAccessoryItemClicked);

            columnInstance.Add(itemTop);

            if (!ReferenceEquals(glassesBottom, null))
            {
                VisualElement itemBottom = item.CloneTree();
                ShopItemData glassesBottomData = new ShopItemData();
                glassesBottomData.SetItemVisualAndCallback(itemBottom, glassesBottom.GetIcon(),
                    glassesBottom, glassesBottom.Price, OnAccessoryItemClicked);

                columnInstance.Add(itemBottom);
            }

            scrollView.Add(columnInstance);
            glassesColumns.Add(columnInstance);
        }
    }

    private void PopulateOthers()
    {
        foreach (var othersColumn in othersColumns)
        {
            scrollView.Remove(othersColumn);
        }

        othersColumns.Clear();

        for (int i = 0; i < otherItemDatas.Count; i += 2)
        {
            OtherShopItemData topData = otherItemDatas[i];
            OtherShopItemData bottomData = i + 1 < otherItemDatas.Count ? otherItemDatas[i + 1] : null;

            VisualElement columnInstance = column.CloneTree();

            SetColumnInstanceStyle(columnInstance);

            VisualElement itemTop = otherItem.CloneTree();

            topData.SetItemVisualAndCallback(itemTop, shopPopupScreenController, OnBuyOtherItem);

            columnInstance.Add(itemTop);

            if (!ReferenceEquals(bottomData, null))
            {
                VisualElement itemBottom = otherItem.CloneTree();
                bottomData.SetItemVisualAndCallback(itemBottom, shopPopupScreenController, OnBuyOtherItem);
                columnInstance.Add(itemBottom);
            }

            scrollView.Add(columnInstance);
            othersColumns.Add(columnInstance);
        }
    }

    private void PopulateCoins()
    {
        //remove existing coins columns
        foreach (var coinColumn in coinsColumns)
        {
            scrollView.Remove(coinColumn);
        }

        coinsColumns.Clear();

        for (int i = 0; i < premiumShopItemDatas.Count; i += 2)
        {
            PremiumShopItemData topData = premiumShopItemDatas[i];
            PremiumShopItemData bottomData = i + 1 < premiumShopItemDatas.Count ? premiumShopItemDatas[i + 1] : null;

            VisualElement columnInstance = column.CloneTree();

            SetColumnInstanceStyle(columnInstance);

            VisualElement itemTop = otherItem.CloneTree();

            topData.SetItemVisualAndCallback(itemTop, shopPopupScreenController, OnBuyOtherItem);

            columnInstance.Add(itemTop);

            if (!ReferenceEquals(bottomData, null))
            {
                VisualElement itemBottom = otherItem.CloneTree();
                bottomData.SetItemVisualAndCallback(itemBottom, shopPopupScreenController, OnBuyOtherItem);
                columnInstance.Add(itemBottom);
            }

            scrollView.Add(columnInstance);
            coinsColumns.Add(columnInstance);
        }
    }

    private void OnBuyAccessoryItem()
    {
        DestroyAccDisplayed();

        if (ReferenceEquals(accInstanceDisplayed, null) || ReferenceEquals(accessory, null))
        {
            return;
        }

        if (gemManager.Payable(accessory.Price))
        //if (gemManager.Payable(0))
        {

            if (!accessorySelectionScreenController.ShowAccessoryStatsSelectionScreen(accessory))
            {
                popupScreenController.ShowPopupScreen(UIText.ALREADY_HAVE_ALL_CARDS, UIText.OK);
                HandleButtonClickNegativeSFX();
                return;
            }

            gemManager.PayCoin(accessory.Price);
            //gemManager.PayCoin(0);
            PlayBuySoundEffect();
            saveLoadManager.NoticeShelf();
        }
        else
        {
            FMODAudioManager.instance.UIButtonClickedNegative();
            // message player that they don't have enough coins
            popupScreenController.ShowPopupScreen(UIText.NOT_ENOUGH_COINS, UIText.OK);
        }
    }

    private void OnBuyOtherItem(ShopItemDataSO data)
    {
        OtherShopItemData otherData = data as OtherShopItemData;
        OtherShopItemType itemType = otherData.itemType;

        if (!gemManager.PayCoin(otherData.price))
        {
            HandleButtonClickNegativeSFX();
            popupScreenController.ShowPopupScreen(UIText.NOT_ENOUGH_COINS, UIText.OK);
            return;
        }

        PlayBuySoundEffect();
        saveLoadManager.NoticeShelf();

        switch (itemType)
        {
            case OtherShopItemType.UpgradeBooster:
                saveLoadManager.AddUpgradeBooster();
                break;
            case OtherShopItemType.RemoveAds:
                saveLoadManager.RemoveAds();
                break;
        }
    }

    private void SetColumnInstanceStyle(VisualElement columnInstance)
    {
        columnInstance.style.alignSelf = Align.Center;
        columnInstance.style.justifyContent = Justify.SpaceEvenly;
    }

    private void HideHats()
    {
        foreach (VisualElement column in hatColumns)
        {
            column.style.display = DisplayStyle.None;
        }

        hatsLabel.style.color = inactiveTabTextColor;
        hatsIcon.style.unityBackgroundImageTintColor = inactiveTabImageTintColor;
    }

    private void HideGlasses()
    {
        foreach (VisualElement column in glassesColumns)
        {
            column.style.display = DisplayStyle.None;
        }

        glassesLabel.style.color = inactiveTabTextColor;
        glassesIcon.style.unityBackgroundImageTintColor = inactiveTabImageTintColor;
    }

    private void HideOthers()
    {
        foreach (VisualElement column in othersColumns)
        {
            column.style.display = DisplayStyle.None;
        }

        othersLabel.style.color = inactiveTabTextColor;
        othersIcon.style.unityBackgroundImageTintColor = inactiveTabImageTintColor;
    }

    private void HideCoins()
    {
        foreach (VisualElement column in coinsColumns)
        {
            column.style.display = DisplayStyle.None;
        }

        premiumLabel.style.color = inactiveTabTextColor;
        coinsIcon.style.unityBackgroundImageTintColor = inactiveTabImageTintColor;
    }

    private void ShowHats()
    {
        foreach (VisualElement column in hatColumns)
        {
            column.style.display = DisplayStyle.Flex;
        }

        hatsLabel.style.color = Color.black;
        hatsIcon.style.unityBackgroundImageTintColor = Color.white;
    }

    private void ShowGlasses()
    {
        foreach (VisualElement column in glassesColumns)
        {
            column.style.display = DisplayStyle.Flex;
        }

        glassesLabel.style.color = Color.black;
        glassesIcon.style.unityBackgroundImageTintColor = Color.white;
    }

    private void ShowOthers()
    {
        foreach (VisualElement column in othersColumns)
        {
            column.style.display = DisplayStyle.Flex;
        }

        othersLabel.style.color = Color.black;
        othersIcon.style.unityBackgroundImageTintColor = Color.white;
    }

    private void ShowCoins()
    {
        foreach (VisualElement column in coinsColumns)
        {
            column.style.display = DisplayStyle.Flex;
        }

        premiumLabel.style.color = Color.black;
        coinsIcon.style.unityBackgroundImageTintColor = Color.white;
    }

    private void OnAccessoryItemClicked(Accessory accessory)
    {
        if (!ReferenceEquals(accessory, null))
        {
            Accessory instance = Instantiate(accessory, new Vector3(0f, 200f, 0f), Quaternion.identity);

            instance.gameObject.layer = displayedAccLayer;
            OffAxisStudios.SimpleRotateObject simpleRotate = instance.gameObject.AddComponent<OffAxisStudios.SimpleRotateObject>();
            simpleRotate.Y = true;
            simpleRotate.degreesPerSecond = 90f;


            for (int i = 0; i < instance.transform.childCount; i++)
            {
                instance.transform.GetChild(i).gameObject.layer = displayedAccLayer;
            }

            //popupUIManager.DisplayAccessory(content as Accessory);
            //popupUIActiveController.SetActive(true);

            shopPopupScreenController.ShowPopupScreen(accessory.accessoryName, 1, accessory.Price, OnBuyAccessoryItem, DestroyAccDisplayed);
            //shopPopupScreenController.ShowPopupScreen(accessory.accessoryName, 1, 0, OnBuyAccessoryItem, DestroyAccDisplayed);

            accInstanceDisplayed = instance;
            this.accessory = accessory;

            HandleButtonClickPositiveSFX();
        }
    }

    private void DestroyAccDisplayed()
    {
        if (accInstanceDisplayed != null)
        {
            Destroy(accInstanceDisplayed.gameObject);
        }
    }

    private void OnClickBackBtn(ClickEvent evt)
    {
        evt.StopPropagation();
        HandleButtonClickPositiveSFX();
        exitShopEC.Raise();
    }

    private void OnSubmitBackBtn(NavigationSubmitEvent evt)
    {
        evt.StopPropagation();
        HandleButtonClickPositiveSFX();
        exitShopEC.Raise();

    }

    private void OnKeyDownBackBtn(KeyDownEvent evt)
    {
        if (evt.keyCode == KeyCode.KeypadEnter || evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.Space)
        {
            evt.StopPropagation();
            HandleButtonClickPositiveSFX();
            exitShopEC.Raise();
        }
    }

    private void OnClickHatsBtn(ClickEvent evt)
    {
        evt.StopPropagation();
        HandleButtonClickPositiveSFX();
        SwitchTabHats();
    }

    private void OnSubmitHatsBtn(NavigationSubmitEvent evt)
    {
        evt.StopPropagation();
        HandleButtonClickPositiveSFX();
        SwitchTabHats();
    }

    private void OnKeyDownHatsBtn(KeyDownEvent evt)
    {
        if (evt.keyCode == KeyCode.KeypadEnter || evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.Space)
        {
            evt.StopPropagation();
            HandleButtonClickPositiveSFX();
            SwitchTabHats();
        }
    }

    private void OnClickGlassesBtn(ClickEvent evt)
    {
        evt.StopPropagation();
        HandleButtonClickPositiveSFX();
        SwitchTabGlasses();
    }

    private void OnSubmitGlassessBtn(NavigationSubmitEvent evt)
    {
        evt.StopPropagation();
        HandleButtonClickPositiveSFX();
        SwitchTabGlasses();
    }

    private void OnKeyDownGlassesBtn(KeyDownEvent evt)
    {
        if (evt.keyCode == KeyCode.KeypadEnter || evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.Space)
        {
            evt.StopPropagation();
            HandleButtonClickPositiveSFX();
            SwitchTabGlasses();
        }
    }

    private void OnClickOthersBtn(ClickEvent evt)
    {
        evt.StopPropagation();
        HandleButtonClickPositiveSFX();
        SwitchTabOthers();
    }

    private void OnSubmitOthersBtn(NavigationSubmitEvent evt)
    {
        evt.StopPropagation();
        HandleButtonClickPositiveSFX();
        SwitchTabOthers();
    }

    private void OnKeyDownOthersBtn(KeyDownEvent evt)
    {
        if (evt.keyCode == KeyCode.KeypadEnter || evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.Space)
        {
            evt.StopPropagation();
            HandleButtonClickPositiveSFX();
            SwitchTabOthers();
        }
    }

    private void OnClickCoinsBtn(ClickEvent evt)
    {
        evt.StopPropagation();
        HandleButtonClickPositiveSFX();
        SwitchTabCoins();
    }

    private void OnSubmitCoinsBtn(NavigationSubmitEvent evt)
    {
        evt.StopPropagation();
        HandleButtonClickPositiveSFX();
        SwitchTabCoins();
    }

    private void OnKeyDownCoinsBtn(KeyDownEvent evt)
    {
        if (evt.keyCode == KeyCode.KeypadEnter || evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.Space)
        {
            evt.StopPropagation();
            HandleButtonClickPositiveSFX();
            SwitchTabCoins();
        }
    }

    private void OnClickBannerBtn(ClickEvent evt)
    {
        evt.StopPropagation();
        HandleButtonClickPositiveSFX();
        
        // 임시로 설정
        SwitchTabCoins();
    }

    private void OnSubmitBannerBtn(NavigationSubmitEvent evt)
    {
        evt.StopPropagation();
        HandleButtonClickPositiveSFX();

        // 임시로 설정
        SwitchTabCoins();
    }

    private void OnKeyDownBannerBtn(KeyDownEvent evt)
    {
        if (evt.keyCode == KeyCode.KeypadEnter || evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.Space)
        {
            evt.StopPropagation();
            HandleButtonClickPositiveSFX();

            // 임시로 설정
            SwitchTabCoins();
        }
    }

    private void SwitchTabHats()
    {
        shopTab = ShopTab.Hats;

        ShowHats();
        HideGlasses();
        HideOthers();
        HideCoins();
        SetTabBarPos(shopTab);
    }

    private void SwitchTabGlasses()
    {
        shopTab = ShopTab.Glassess;

        ShowGlasses();
        HideHats();
        HideOthers();
        HideCoins();
        SetTabBarPos(shopTab);
    }

    private void SwitchTabOthers()
    {
        shopTab = ShopTab.Others;

        ShowOthers();
        HideHats();
        HideGlasses();
        HideCoins();
        SetTabBarPos(shopTab);
    }

    private void SwitchTabCoins()
    {
        shopTab = ShopTab.Coins;

        ShowCoins();
        HideHats();
        HideGlasses();
        HideOthers();
        SetTabBarPos(shopTab);
    }

    private void SetTabBarPos(ShopTab tab)
    {
        float barHeight = tabBar.resolvedStyle.height;
        float screenHeight = screen.resolvedStyle.height;
        float buttonHeight = hatsButton.resolvedStyle.height;

        float top = 0f;

        switch (tab)
        {
            case ShopTab.Hats:
                top += buttonHeight * 0.5f;
                break;
            case ShopTab.Glassess:
                top += buttonHeight * 1.5f;
                break;
            case ShopTab.Others:
                top += buttonHeight * 2.5f;
                break;
            case ShopTab.Coins:
                top += buttonHeight * 3.5f;
                break;
        }

        top -= barHeight * 0.5f;
        tabBar.style.top = top;
    }

    private void OnGeometryChanged(GeometryChangedEvent evt)
    {
        SetTabBarPos(shopTab);
    }

    private void PlayBuySoundEffect()
    {
        FMODAudioManager.instance.BuyItem();
    }

    private void UpdateNumCoins(int numCoins)
    {
        numCoinsLabel.text = numCoins.ToString();
        veAnimator?.StartAnimation();
    }

    private void UpdateAdsState(bool adsRemoved)
    {
        adsOnOffLabel.text = GetLocalizedString(adsRemoved ? UIText.LBL_AD_OFF : UIText.LBL_AD_ON);
        adsOnOffImage.style.backgroundImage = new StyleBackground(adsRemoved ? adsOff : adsOn);
    }

    private void OnAdsRemoved()
    {
        UpdateAdsState(true);
    }
}
