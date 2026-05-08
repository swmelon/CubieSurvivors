using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.UIElements;

public class ShopPopupScreen : MenuScreen
{
    public event Action BuyButtonClicked;
    public event Action BackButtonClicked;

    [SerializeField]
    private CardSetupManager cardSetupManager;

    private const string priceLabelName = "label-price";
    private const string nameLabelName = "label-name";
    private const string rankLabelName = "label-rank";
    private const string accessoryImageName = "image-accessory";
    private const string othersImageName = "image-others";
    private const string coinImageName = "image-coin";

    private const string buyButtonName = "button-buy";
    private const string buyLabelName = "label-buy";
    private const string backButtonName = "button-back";
    private const string containerName = "container";

    private Label priceLabel, nameLabel, rankLabel, buyLabel;
    private Button buyButton;
    private Button backButton;
    private VisualElement coinImage;
    private VisualElement container;

    private VisualElement accessoryImage, othersImage;
    
    protected override void SetVisualElements()
    {
        base.SetVisualElements();
        priceLabel = screen.Q<Label>(priceLabelName);
        nameLabel = screen.Q<Label>(nameLabelName);
        rankLabel = screen.Q<Label>(rankLabelName);
        buyLabel = screen.Q<Label>(buyLabelName);

        buyButton = screen.Q<Button>(buyButtonName);
        backButton = screen.Q<Button>(backButtonName);

        accessoryImage = screen.Q<VisualElement>(accessoryImageName);
        othersImage = screen.Q<VisualElement>(othersImageName);
        coinImage = screen.Q<VisualElement>(coinImageName);

        container = screen.Q<VisualElement>(containerName);
    }

    public void SetupScreen(string accName, int accRank, int price, bool isCoin)
    {
        nameLabel.text = LocalizationSettings.StringDatabase.GetLocalizedString(LocalizationTableName.TABLE_ACCESSORY_NAMES, accName);
        rankLabel.text = "+" + accRank.ToString();
        rankLabel.style.color = cardSetupManager.GetRankThemeColor(accRank);
        buyLabel.text = GetLocalizedString(UIText.LBL_BUY);
        priceLabel.text = price.ToString();

        accessoryImage.style.display = DisplayStyle.Flex;
        othersImage.style.display = DisplayStyle.None;

        if (isCoin)
        { 
            coinImage.style.display = DisplayStyle.Flex;
        }
        else
        {
            coinImage.style.display = DisplayStyle.None;
        }
    }

    public void SetupScreen(string itemName, string description , string price, bool isCoin, Sprite itemIcon)
    {

        nameLabel.text = itemName;
        rankLabel.text = description;
        rankLabel.style.color = Color.white;
        priceLabel.text = price;

        if (isCoin)
        {
            coinImage.style.display = DisplayStyle.Flex;
        }
        else
        {
            coinImage.style.display = DisplayStyle.None;
        }

        othersImage.style.width = itemIcon.texture.width;
        othersImage.style.height = itemIcon.texture.height;

        othersImage.style.backgroundImage = new StyleBackground(itemIcon);

        accessoryImage.style.display = DisplayStyle.None;
        othersImage.style.display = DisplayStyle.Flex;
    }


    //public void SetupScreen(string noticeText, string leftButtonText, float alpha)
    //{
    //    priceLabel.text = noticeText;
    //    buyButton.text = leftButtonText;
    //    backButton.style.display = DisplayStyle.None;
    //    StyleColor color = container.resolvedStyle.backgroundColor;
    //    color.value = new Color(color.value.r, color.value.g, color.value.b, alpha);
    //    container.style.backgroundColor = color;

    
    //}


    protected override void RegisterButtonCallbacks()
    {
        base.RegisterButtonCallbacks();
        
        buyButton.RegisterCallback<ClickEvent>(OnClickBuyBtn);
        buyButton.RegisterCallback<NavigationSubmitEvent>(OnSubmitBuyBtn);
        buyButton.RegisterCallback<KeyDownEvent>(OnKeyDownBuyBtn);

        backButton.RegisterCallback<ClickEvent>(OnClickBackBtn);
        backButton.RegisterCallback<NavigationSubmitEvent>(OnSubmitBackBtn);
        backButton.RegisterCallback<KeyDownEvent>(OnKeyDownBackBtn);
    }
    
    private void OnClickBuyBtn(ClickEvent evt)
    {
        //ActivateButton(m_InfoScreenMenuButton);
        BuyButtonClicked?.Invoke();
        evt.StopPropagation();
        //ClickMarker(evt);
        HandleButtonClickPositiveSFX();

    }

    private void OnSubmitBuyBtn(NavigationSubmitEvent evt)
    {
        BuyButtonClicked?.Invoke();
        evt.StopPropagation();
        HandleButtonClickPositiveSFX();

    }

    private void OnKeyDownBuyBtn(KeyDownEvent evt)
    {
        if (evt.keyCode == KeyCode.KeypadEnter || evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.Space)
        {
            BuyButtonClicked?.Invoke();
            evt.StopPropagation();
            HandleButtonClickPositiveSFX();

        }
    }

    private void OnClickBackBtn(ClickEvent evt)
    {
        BackButtonClicked?.Invoke();
        evt.StopPropagation();
        HandleButtonClickPositiveSFX();

    }

    private void OnSubmitBackBtn(NavigationSubmitEvent evt)
    {
        BackButtonClicked?.Invoke();
        evt.StopPropagation();
        HandleButtonClickPositiveSFX();

    }

    private void OnKeyDownBackBtn(KeyDownEvent evt)
    {
        if (evt.keyCode == KeyCode.KeypadEnter || evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.Space)
        {
            BackButtonClicked?.Invoke();
            evt.StopPropagation();
            HandleButtonClickPositiveSFX();

        }
    }
}
