using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

public class AccessoryCollectionScreen : MenuScreen
{
    [SerializeField]
    private CardFXController cardFXController;

    [SerializeField]
    private CardAnimator cardAnimator;

    [SerializeField]
    private AnimationCurve sideBarAnimationCurve;

    [SerializeField]
    private GameAccessoryManager gameAccManager;

    [SerializeField]
    private PopupScreenController popupScreenController;

    [SerializeField]
    private AccessoryStatsSelectionScreenController accessorySelectionScreenController;

    [SerializeField]
    private BooleanEventChannelSO accInventoryScreenCC;

    [SerializeField]
    private CardSetupManager cardSetupManager;

    [SerializeField]
    private SaveLoadManagerSO saveLoadManager;

    [SerializeField]
    private VisualTreeAsset rowTemplate;

    [SerializeField]
    private Color inactiveTabTextColor;

    [SerializeField]
    private Color inactiveTabImageTintColor;

    private Button backButton, screenButton;
    private Button equipModeButton, fusionModeButton, upgradeModeButton;
    private Label equipModeLabel, fusionModeLabel, upgradeModeLabel, upgradeBoosterLabel;
    private VisualElement equipIcon, fusionIcon, upgradeIcon;
    private VisualElement interactionModeBar;

    private const string backButtonName = "button-back";
    private const string screenButtonName = "button-screen";
    private const string scrollViewName = "scrollview";
    private const string rowName = "row-collections";
    private const string interactionModeBarName = "bar-right";

    private const string equipModeButtonName = "button-equip";
    private const string fusionModeButtonName = "button-fusion";
    private const string upgradeModeButtonName = "button-upgrade";

    private const string upgradeModeLabelName = "label-upgrade";
    private const string upgradeBoosterLabelName = "label-upgrade-booster";
    private const string fusionModeLabelName = "label-fusion";
    private const string equipModeLabelName = "label-equip";

    private const string upgradeIconName = "image-upgrade";
    private const string fusionIconName = "image-fusion";
    private const string equipIconName = "image-equip";

    private const string tabBarName = "bar-tap";
        
    private int numCards;
    private AccessoryCardData.InteractionMode interactionMode;
    private int maxNumCardsPerRow = 4;
    private List<VisualElement> rows = new List<VisualElement>();
    private List<VisualElement> rowRoots = new List<VisualElement>();
    private ScrollView scrollView;
    private List<VisualElement> cards = new List<VisualElement>();
    private List<AccessoryCardData> cardDatas = new List<AccessoryCardData>();
    private VisualElement selectedCard;
    private AccStatsControlCardData accStatsControlCardData;
    private float accStatsControlCardZoomScale = 1.5f;
    private VisualElement selectedRow;
    private VisualElement tabBar;
    private int selectedCardIndex;
    private int rowIndex, colIndex;

    private List<int> rowIndexes = new List<int>();
    private List<int> colIndexes = new List<int>();

    private List<VisualElement> fusionCards = new List<VisualElement>();
    private List<AccessoryCardData> fusionCardDatas = new List<AccessoryCardData>();
    private int fusionCount = 3;

    private bool animateInteractionBar = false;
    private float startBarBottom;
    private float time, duration = 0.15f;
    private float targetBottom = 0;

    private VisualElementAnimator upgradeBoosterAnimator;

    protected override void SetVisualElements()
    {
        base.SetVisualElements();
        backButton = screen.Q<Button>(backButtonName);
        screenButton = screen.Q<Button>(screenButtonName);
        scrollView = screen.Q<ScrollView>(scrollViewName);
        tabBar = screen.Q<VisualElement>(tabBarName);

        equipModeButton = screen.Q<Button>(equipModeButtonName);
        fusionModeButton = screen.Q<Button>(fusionModeButtonName);
        upgradeModeButton = screen.Q<Button>(upgradeModeButtonName);

        equipModeLabel = screen.Q<Label>(equipModeLabelName);
        fusionModeLabel = screen.Q<Label>(fusionModeLabelName);
        upgradeModeLabel = screen.Q<Label>(upgradeModeLabelName);
        upgradeBoosterLabel = screen.Q<Label>(upgradeBoosterLabelName);

        if(TryGetComponent(out upgradeBoosterAnimator))
        {
            upgradeBoosterAnimator.SetVisualElement(upgradeBoosterLabel);
        }

        equipIcon = screen.Q<VisualElement>(equipIconName);
        fusionIcon = screen.Q<VisualElement>(fusionIconName);
        upgradeIcon = screen.Q<VisualElement>(upgradeIconName);
    }

    protected override void RegisterButtonCallbacks()
    {
        base.RegisterButtonCallbacks();

        screen.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);

        backButton.RegisterCallback<ClickEvent>(OnClickBackBtn);
        backButton.RegisterCallback<NavigationSubmitEvent>(OnSubmitBackBtn);
        backButton.RegisterCallback<KeyDownEvent>(OnKeyDownBackBtn);

        screenButton.RegisterCallback<ClickEvent>(OnClickScreenButton);
        screenButton.RegisterCallback<NavigationSubmitEvent>(OnSubmitScreenButton);
        screenButton.RegisterCallback<KeyDownEvent>(OnKeyDownScreenButton);

        equipModeButton.RegisterCallback<ClickEvent>(OnClickEquipBtn);
        equipModeButton.RegisterCallback<NavigationSubmitEvent>(OnSubmitEquipBtn);
        equipModeButton.RegisterCallback<KeyDownEvent>(OnKeyDownEquipBtn);

        fusionModeButton.RegisterCallback<ClickEvent>(OnClickFusionBtn);
        fusionModeButton.RegisterCallback<NavigationSubmitEvent>(OnSubmitFusionBtn);
        fusionModeButton.RegisterCallback<KeyDownEvent>(OnKeyDownFusionBtn);

        upgradeModeButton.RegisterCallback<ClickEvent>(OnClickUpgradeBtn);
        upgradeModeButton.RegisterCallback<NavigationSubmitEvent>(OnSubmitUpgradeBtn);
        upgradeModeButton.RegisterCallback<KeyDownEvent>(OnKeyDownUpgradeBtn);

        interactionModeBar = screen.Q<VisualElement>(interactionModeBarName);

    }

    protected override void SetLocalizedText()
    {
        base.SetLocalizedText();

        equipModeLabel.text = GetLocalizedString(UIText.BTN_EQUIP);
        fusionModeLabel.text = GetLocalizedString(UIText.BTN_FUSION);
        upgradeModeLabel.text = GetLocalizedString(UIText.BTN_UPGRADE);
    }

    public override void ShowScreen()
    {
        base.ShowScreen();

        cardFXController.TurnOnFx();
        screenButton.style.display = DisplayStyle.None;
        SlideUpInteractionBarAnimation();
        UpdateUpgradeBoosterText();
        SwitchTabEquip();
    }

    public override void HideScreen()
    {
        base.HideScreen();

        for (int i = 0; i < rows.Count; i += 1)
        {
            for (int j = 0; j < maxNumCardsPerRow; j++)
            {
                int cardIndex = i * maxNumCardsPerRow + j;

                if (cards.Count <= cardIndex)
                {
                    break;
                }

                if (ReferenceEquals(cards[cardIndex], selectedCard))
                {
                    screenButton.Remove(selectedCard);
                    selectedCard = null;
                    continue;
                }

                rows[i].Remove(cards[cardIndex]);
            }
        }

        for (int i = 0; i < rowRoots.Count; i++)
        {
            screen.Remove(rowRoots[i]);
        }

        numCards = 0;
        cards.Clear();
        rows.Clear();
        rowRoots.Clear();
        cardDatas.Clear();
        cardFXController.TurnOffFx();
        accInventoryScreenCC.Raise(true);
    }

    private void Update()
    {
        if (!animateInteractionBar)
        {
            return;
        }

        time += Time.deltaTime;
        if (time > 0)
        {
            float t = time / duration;
            t = sideBarAnimationCurve.Evaluate(Mathf.Clamp01(t));
            float bottom = Mathf.Lerp(startBarBottom, targetBottom, t);
            interactionModeBar.style.right = bottom;
        }

        if (time >= duration)
        {
            animateInteractionBar = false;
            interactionModeBar.style.right = targetBottom;
        }

    }

    public void ShowAccessoryCollectionScreen(List<AccData> accDatas)
    {
        ShowScreen();

        numCards = 0;
        
        // 버튼을 통해 이것을 변경하도록
        interactionMode = AccessoryCardData.InteractionMode.Equip;

        for (int i = 0; i < accDatas.Count; i++)
        {
            AddCard(accDatas[i]);
        }


        //if (cards.Count == 1)
        //{
        //    OnSelectCard(cards[0]);
        //}
    }

    private void AddCard(AccData data)
    {
        AccessoryCardData cardData = new AccessoryCardData(data, gameAccManager, AccessoryCardData.InteractionMode.None);
        cardDatas.Add(cardData);
        VisualElement card = cardSetupManager.SetupCard(cardData, OnSelectCard);

        if (numCards % maxNumCardsPerRow == 0)
        {
            VisualElement rowRoot = rowTemplate.CloneTree();
            VisualElement row = rowRoot.Q<VisualElement>(rowName);
            screen.Add(rowRoot);
            row.Add(card);  // 카드를 자식으로
            rows.Add(row);  // 행을 리스트에
            scrollView.Add(row);    // 행을 화면에
            cards.Add(card);
        }
        else
        {
            rows[rows.Count - 1].Add(card);
            cards.Add(card);
        }

        numCards += 1;
    }



    private void OnClickBackBtn(ClickEvent evt)
    {
        evt.StopPropagation();
        HandleButtonClickPositiveSFX();
        HideScreen();
    }

    private void OnSubmitBackBtn(NavigationSubmitEvent evt)
    {
        evt.StopPropagation();
        HandleButtonClickPositiveSFX();
        HideScreen();
    }

    private void OnKeyDownBackBtn(KeyDownEvent evt)
    {
        if (evt.keyCode == KeyCode.KeypadEnter || evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.Space)
        {
            evt.StopPropagation();
            HideScreen();
        }
    }

    private void OnClickScreenButton(ClickEvent evt)
    {
        evt.StopPropagation();
        ExitViewMode();
    }

    private void OnSubmitScreenButton(NavigationSubmitEvent evt)
    {
        evt.StopPropagation();
        ExitViewMode();
    }

    private void OnKeyDownScreenButton(KeyDownEvent evt)
    {
        if (evt.keyCode == KeyCode.KeypadEnter || evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.Space)
        {
            evt.StopPropagation();
            ExitViewMode();
        }
    }

    private void OnClickEquipBtn(ClickEvent evt) 
    {
        evt.StopPropagation();
        SwitchTabEquip();
    }

    private void OnSubmitEquipBtn(NavigationSubmitEvent evt)
    {
        evt.StopPropagation();
        SwitchTabEquip();
    }

    private void OnKeyDownEquipBtn(KeyDownEvent evt)
    {
        if (evt.keyCode == KeyCode.KeypadEnter || evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.Space)
        {
            evt.StopPropagation();
            SwitchTabEquip();
        }
    }

    private void OnClickFusionBtn(ClickEvent evt)
    {
        evt.StopPropagation();
        SwitchTabFusion();
    }

    private void OnSubmitFusionBtn(NavigationSubmitEvent evt)
    {
        evt.StopPropagation();
        SwitchTabFusion();
    }

    private void OnKeyDownFusionBtn(KeyDownEvent evt)
    {
        if (evt.keyCode == KeyCode.KeypadEnter || evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.Space)
        {
            evt.StopPropagation();
            HandleButtonClickPositiveSFX();
            SwitchTabFusion();
        }
    }

    private void OnClickUpgradeBtn(ClickEvent evt)
    {
        evt.StopPropagation();
        SwitchTabUpgrade();
    }

    private void OnSubmitUpgradeBtn(NavigationSubmitEvent evt)
    {
        evt.StopPropagation();
        SwitchTabUpgrade();
    }

    private void OnKeyDownUpgradeBtn(KeyDownEvent evt)
    {
        if (evt.keyCode == KeyCode.KeypadEnter || evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.Space)
        {
            evt.StopPropagation();
            SwitchTabUpgrade();
        }
    }



    private void OnSelectCard(VisualElement card)
    {
        if (cardAnimator.IsAnimating)
        {
            return;
        }

        // 이미 선택된 카드를 다시 선택하면
        if (ReferenceEquals(card, selectedCard))
        {
            // 모드에 따라 다르게 동작
            switch (interactionMode)
            {
                case AccessoryCardData.InteractionMode.Fusion:
                    CardSelectedFusionMode(card);
                    break;
                case AccessoryCardData.InteractionMode.Upgrade:
                    CardSelectedUpgradeMode(card);
                    break;
                case AccessoryCardData.InteractionMode.Equip:
                    HideScreen();
                    break;
                default:
                    HideScreen();
                    break;
            }

            return;
        }

        // 첫번째 선택시 뷰모드  

        ReadyToAnimateCard(card);
        DisableCards();

        float offset = scrollView.scrollOffset.y;


        cardAnimator.AnimateCardZoomIn(selectedCard, screen, selectedRow, -offset, ChangeSelectedCardInteractionMode);
    }

    private void ReadyToAnimateCard(VisualElement card)
    {
        selectedCardIndex = cards.IndexOf(card);
        rowIndex = selectedCardIndex / maxNumCardsPerRow;
        colIndex = selectedCardIndex % maxNumCardsPerRow;
        selectedCard = card;
        selectedRow = rows[rowIndex];
        selectedCard.style.position = Position.Absolute;
        selectedCard.RemoveFromHierarchy();
        screenButton.style.display = DisplayStyle.Flex;
        screenButton.Add(selectedCard);
    }

    private void ReadyToAnimateCards(List<VisualElement> cardsToAnimate)
    {
        rowIndexes.Clear();
        colIndexes.Clear();
        for (int i = 0; i < cardsToAnimate.Count; i++)
        {
            VisualElement selectedCard = cardsToAnimate[i];
            int cardIndex = cards.IndexOf(selectedCard);
            int rowIndex = cardIndex / maxNumCardsPerRow;
            int colIndex = cardIndex % maxNumCardsPerRow;
            selectedRow = rows[rowIndex];
            selectedCard.style.position = Position.Absolute;
            selectedCard.RemoveFromHierarchy();
            screenButton.style.display = DisplayStyle.Flex;
            screenButton.Add(selectedCard);
            rowIndexes.Add(rowIndex);
            colIndexes.Add(colIndex);
        }

        this.selectedCard = null;
    }


    private void CardSelectedFusionMode(VisualElement card)
    {
        if (fusionCardDatas.Count != fusionCardDatas.Count)
        {
            Debug.LogError("Fusion card data count mismatch.");
            return;
        }

        int cardIndex = cards.IndexOf(card);
        AccessoryCardData cardData = cardDatas[cardIndex];

        if (fusionCards.Contains(card))
        {
            // 이 경우는 선택 해제
            SetSelectedCardVisualFusionMode(cardData, false);
            fusionCards.Remove(card);
            fusionCardDatas.Remove(cardData);
            return;
        }

        // 선택한 카드가 이미 선택된 카드와 같은 랭크가 아니면
        if (fusionCards.Count != 0 && fusionCardDatas[0].GetRank() != cardData.GetRank())
        {
            popupScreenController.ShowPopupScreen(UIText.NOT_SAME_RANK, UIText.OK);
            return;
        }
        else if(cardData.GetRank() == AccessoryRank.Legendary)
        {
            popupScreenController.ShowPopupScreen(UIText.CANNOT_FUSE_LEGENDARY, UIText.OK);
            return;
        }

        SetSelectedCardVisualFusionMode(cardData, true);
        fusionCards.Add(card);
        fusionCardDatas.Add(cardData);

        if (fusionCardDatas.Count == fusionCount)
        {
            // 이것을 골랐으니 카드가 다 모였다.
            // 팝업을 띄우고 수락시 융합
            cardAnimator.AddCallback(AllFusionCardsSelected);
            return;
        }
    }

    private void CardSelectedUpgradeMode(VisualElement card)
    {
        // 업그레이드 부스터 갯수가 부족하면

        if (saveLoadManager.SaveFile.numUpgradeBooster < 1)
        {
            popupScreenController.ShowPopupScreen(UIText.NO_UPGRADE_BOOSTER, UIText.OK);
            ExitViewMode();
            return;
        }

        int cardIndex = cards.IndexOf(card);
        AccessoryCardData cardData = cardDatas[cardIndex];

        if (cardData.GetRank() == AccessoryRank.Legendary)
        {
            popupScreenController.ShowPopupScreen(UIText.CANNOT_UPGRADE_LEGENDARY, UIText.OK);
            return;
        }

        accStatsControlCardData = new AccStatsControlCardData(cardData.AccData, gameAccManager);
        VisualElement accStatsControlCard = cardSetupManager.SetupAccStatsControlCard(accStatsControlCardData, OnRequestUpgrade);

        IStyle statsControlCardStyle = accStatsControlCard.style;
        IResolvedStyle cardStyle = card.resolvedStyle;

        statsControlCardStyle.position = Position.Absolute;
        statsControlCardStyle.top = cardStyle.top;
        statsControlCardStyle.left = cardStyle.left;
        statsControlCardStyle.right = cardStyle.right;
        statsControlCardStyle.bottom = cardStyle.bottom;
        statsControlCardStyle.scale = cardStyle.scale;


        // 안보이게 만들면 추후에 이 스타일로 새로운 카드를 생성할 수 없음.
        //card.style.display = DisplayStyle.None;

        screenButton.Add(accStatsControlCard);

        cardAnimator.AnimateCardZoomIn(accStatsControlCard, cardStyle, screen, accStatsControlCardZoomScale);

        //cardAnimator.AnimateCardZoomIn(card, screen, 1.5f);
    }

    private void OnRequestUpgrade(AccStatsControlCardData data)
    {
        //if (saveLoadManager.SaveFile.numUpgradeBooster < 1)
        //{
        //    popupScreenController.ShowPopupScreen(UIText.NO_UPGRADE_BOOSTER, UIText.OK);
        //    return;
        //}

        if (gameAccManager.Upgrade(data.AccData))
        {
            // upgrade finish
            data.ApplyStats();

            // TODO : 이스터에그, 네거티브 사운드에 비트 넣기
            //data.PlayComposed();

            // 해당 데이터 다시 그리기
            int cardIndex = cards.IndexOf(selectedCard);
            CardData cardData = cardDatas[cardIndex];
            VisualElement cardUpgraded = cardSetupManager.SetupCard(cardData, OnSelectCard);

            // 카드 교체하기
            IStyle newCardStyle = cardUpgraded.style;
            IResolvedStyle oldCardStyle = selectedCard.resolvedStyle;

            MakesCardVisualIdentical(newCardStyle, oldCardStyle);

            screenButton.Remove(selectedCard);
            screenButton.Add(cardUpgraded);

            selectedCard = cardUpgraded;
            cards[cardIndex] = cardUpgraded;

            FMODAudioManager.instance.PlayOneShot(SFXTags.UpgradeAccessory);
            UpdateUpgradeBoosterText();
            ExitViewMode();
        }
        else
        {
            popupScreenController.ShowPopupScreen(UIText.ALREADY_HAVE_CARD, UIText.OK, ExitViewMode);
            HandleButtonClickNegativeSFX();
        }
    }

    private async void AllFusionCardsSelected()
    {
        DisableCards();

        List<VisualElement> rowsWhereCardExist = new List<VisualElement>();


        for (int i = 0; i < fusionCardDatas.Count; i++)
        {
            int index = cardDatas.IndexOf(fusionCardDatas[i]);
            int rowIndex = index / maxNumCardsPerRow;
            rowsWhereCardExist.Add(rows[rowIndex]);
        }
        
        float offset = scrollView.scrollOffset.y;


        // 딜레이를 주어야 마지막 카드의 위치가 제대로 계산됨
        await Task.Yield();
        await Task.Yield();


        for (int i = 0; i < fusionCards.Count; i++)
        {
            fusionCards[i].SetEnabled(true);
            //fusionCards[i].style.rotate = new StyleRotate(new Rotate(new Angle(180f, AngleUnit.Degree)));
        }

        ReadyToAnimateCards(fusionCards);
        ResetFusionCardsVisual();
        cardAnimator.AnimateCardsDisplay(fusionCards, screen, rowsWhereCardExist, -offset);
        popupScreenController.ShowPopupScreen(UIText.FUSE_CARDS, UIText.YES, UIText.CANCEL, OnAcceptFusion, OnDeclineFusion, alpha: 0.5f);
    }

    private void OnAcceptFusion()
    {
        cardAnimator.FuseCardsDisplayed(screen, OnFinishFusion);
    }

    private void OnDeclineFusion()
    {
        cardAnimator.ReleaseCardsDisplayed(OnFinishRelease);
    }

    private void SetSelectedCardVisualFusionMode(AccessoryCardData data, bool mark)
    {
        data.HighlightFX(mark);
    }

    private void ChangeSelectedCardInteractionMode()
    {
        cardDatas[selectedCardIndex].ChangeInteractionMode(interactionMode);
    }

    private void SetSelectedCardInteractionModeNone()
    {
        if (ReferenceEquals(selectedCard, null))
        {
            return;
        }

        cardDatas[selectedCardIndex].ChangeInteractionMode(AccessoryCardData.InteractionMode.None);
    }


    private void DisableCards()
    {
        for (int i = 0; i < cards.Count; i++)
        {
            if (cards[i] != selectedCard)
            {
                cards[i].SetEnabled(false);
            }
        }
    }

    private void EnableCards()
    {
        for (int i = 0; i < cards.Count; i++)
        {
            cards[i].SetEnabled(true);
        }
    }

    private void ExitViewMode()
    {
        if (cardAnimator.IsAnimating)
        {
            return;
        }

        cardAnimator.ZoomOutSelectedCard(OnFinishZoomOut, selectedCard);
        SetSelectedCardInteractionModeNone();
        HandleButtonClickPositiveSFX();

        switch (interactionMode)
        {
            case AccessoryCardData.InteractionMode.Fusion:
                break;
            case AccessoryCardData.InteractionMode.Upgrade:
                if (!ReferenceEquals(selectedCard, null))
                {
                    selectedCard.style.display = DisplayStyle.Flex;
                }
                if (!ReferenceEquals(accStatsControlCardData, null))
                {
                    screenButton?.Remove(accStatsControlCardData.Card);
                    accStatsControlCardData.ResetStatChanges();
                    accStatsControlCardData = null;
                }
                break;
        }
    }

    private void OnFinishZoomOut()
    {
        if (ReferenceEquals(selectedCard, null))
        {
            return;
        }

        selectedCard.RemoveFromHierarchy();
        IStyle style = selectedCard.style;
        style.position = Position.Relative;
        style.top = 0;
        style.left = 0;

        rows[rowIndex].Insert(colIndex, selectedCard);
        screenButton.style.display = DisplayStyle.None;
        selectedCard = null;
        EnableCards();
    }

    private void OnFinishFusion()
    {
        AccData data = fusionCardDatas[0].AccData;
        Accessory accessory = data.accessory;
        int rank = data.GetRank();

        if ((AccessoryRank)rank > AccessoryRank.Epic)
        {
            Debug.LogError("Cannot fuse legendary or higher rank accessories.");
            popupScreenController.ShowPopupScreen(UIText.CANNOT_FUSE_LEGENDARY, UIText.OK);
            return;
        }

        rank += 1;

        bool haveCombination = accessorySelectionScreenController.ShowAccessoryStatsSelectionScreen(accessory, (AccessoryRank)rank);

        if (!haveCombination)
        {
            popupScreenController.ShowPopupScreen(UIText.ALREADY_HAVE_ALL_CARDS, UIText.OK, OnDeclineFusion);
            HandleButtonClickNegativeSFX();
            return;
        }

        // need to remove fusion cards
        for (int i = 0; i < fusionCards.Count; i++)
        {
            AccData accData = fusionCardDatas[i].AccData;
            gameAccManager.RemoveAccessoryOnShelf(accData);
        }

        ReparentFusionCards();

        for (int i = 0; i < fusionCards.Count; i++)
        {
            fusionCards[i].style.display = DisplayStyle.None;
        }

        ResetFusion();
        accessorySelectionScreenController.AccessoryStatsSelected += HideScreen;
    }

    private void OnFinishRelease()
    {
        ReparentFusionCards();
        ResetFusion();
        EnableCards();
    }

    private void ReparentFusionCards()
    {
        for (int i = 0; i < fusionCards.Count; i++)
        {
            fusionCards[i].RemoveFromHierarchy();
            IStyle style = fusionCards[i].style;
            style.position = Position.Relative;
            style.top = 0;
            style.left = 0;

            int row = rowIndexes[i];
            int col = colIndexes[i];
            int colAdjustment = 0;

            for (int j = i + 1; j < fusionCards.Count; j++)
            {
                int rowOthers = rowIndexes[j];
                int colOthers = colIndexes[j];

                // 같은 행에 내 앞에 들어와야할 카드가 있는데, 아직 안들어온 경우
                if (rowOthers == row && colOthers < col)
                {
                    colAdjustment -= 1;
                }
            }

            int adjustedCol = Mathf.Max(col + colAdjustment, 0);

            rows[row].Insert(adjustedCol, fusionCards[i]);
        }

        screenButton.style.display = DisplayStyle.None;
    }

    //private void OnAccessoryStatsSelected()
    //{
    //    HideScreen();
    //}

    private void SlideUpInteractionBarAnimation()
    {
        time = 0;
        targetBottom = 0;
        startBarBottom = -320;
        animateInteractionBar = true;
    }

    private void SlideDownInteractionBarAnimation()
    {
        time = 0;
        targetBottom = -320;
        startBarBottom = 0;
        animateInteractionBar = true;
    }

    private void SwitchTabEquip()
    {
        interactionMode = AccessoryCardData.InteractionMode.Equip;
        SetTabBarPos(interactionMode);
        SetTabVisual(interactionMode);
        ResetFusion();
        HandleButtonClickPositiveSFX();
    }

    private void SwitchTabFusion()
    {
        interactionMode = AccessoryCardData.InteractionMode.Fusion;
        SetTabBarPos(interactionMode);
        SetTabVisual(interactionMode);
        HandleButtonClickPositiveSFX();
    }

    private void SwitchTabUpgrade()
    {
        //if (saveLoadManager.SaveFile.numUpgradeBooster < 1)
        //{
        //    HandleButtonClickNegative();
        //    return;
        //}

        interactionMode = AccessoryCardData.InteractionMode.Upgrade;
        SetTabBarPos(interactionMode);
        SetTabVisual(interactionMode);
        ResetFusion();
        HandleButtonClickPositiveSFX();
    }

    private void SetTabVisual(AccessoryCardData.InteractionMode mode)
    {
        equipModeLabel.style.color = inactiveTabTextColor;
        fusionModeLabel.style.color = inactiveTabTextColor;
        upgradeModeLabel.style.color = inactiveTabTextColor;
        upgradeBoosterLabel.style.color = inactiveTabTextColor;

        equipIcon.style.unityBackgroundImageTintColor = inactiveTabImageTintColor;
        fusionIcon.style.unityBackgroundImageTintColor = inactiveTabImageTintColor;
        upgradeIcon.style.unityBackgroundImageTintColor = inactiveTabImageTintColor;

        switch (mode)
        {
            case AccessoryCardData.InteractionMode.Equip:
                equipModeLabel.style.color = Color.black;
                equipIcon.style.unityBackgroundImageTintColor = Color.white;
                break;
            case AccessoryCardData.InteractionMode.Fusion:
                fusionModeLabel.style.color = Color.black;
                fusionIcon.style.unityBackgroundImageTintColor = Color.white;
                break;
            case AccessoryCardData.InteractionMode.Upgrade:
                upgradeModeLabel.style.color = Color.black;
                upgradeBoosterLabel.style.color = Color.black;
                upgradeIcon.style.unityBackgroundImageTintColor = Color.white;
                break;
        }
    }

    private void SetTabBarPos(AccessoryCardData.InteractionMode mode)
    {
        float barHeight = tabBar.resolvedStyle.height;
        float screenHeight = screen.resolvedStyle.height;
        float buttonHeight = equipModeButton.resolvedStyle.height;

        float top = 0f;

        switch (mode)
        {
            case AccessoryCardData.InteractionMode.Equip:
                top += buttonHeight * 0.5f;
                break;
            case AccessoryCardData.InteractionMode.Fusion:
                top += buttonHeight * 1.5f;
                break;
            case AccessoryCardData.InteractionMode.Upgrade:
                top += buttonHeight * 2.5f;
                break;
        }

        top -= barHeight * 0.5f;
        tabBar.style.top = top;
    }

    private void ResetFusion()
    {
        ResetFusionCardsVisual();
        fusionCards.Clear();
        fusionCardDatas.Clear();
    }

    private void ResetFusionCardsVisual()
    {
        for (int i = 0; i < fusionCards.Count; i++)
        {
            SetSelectedCardVisualFusionMode(fusionCardDatas[i], false);
        }
    }

    private void UpdateUpgradeBoosterText()
    {
        upgradeBoosterLabel.text =  "x" + saveLoadManager.SaveFile.numUpgradeBooster.ToString();
        upgradeBoosterAnimator?.StartAnimation();
    }

    private void OnGeometryChanged(GeometryChangedEvent evt)
    {
        SetTabBarPos(interactionMode);
    }

    private static void MakesCardVisualIdentical(IStyle style, IResolvedStyle resolvedStyle)
    {
        style.position = resolvedStyle.position;
        style.top = resolvedStyle.top;
        style.left = resolvedStyle.left;
        style.right = resolvedStyle.right;
        style.bottom = resolvedStyle.bottom;
        style.scale = resolvedStyle.scale;
        style.width = resolvedStyle.width;
        style.height = resolvedStyle.height;
    }
}
