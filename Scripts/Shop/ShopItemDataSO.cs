using UnityEngine;
using UnityEngine.UIElements;
using System;
using UnityEngine.InputSystem;
using UnityEngine.Localization.Settings;

public abstract class ShopItemDataSO : ScriptableObject
{
    protected static readonly string NameLabel = "label-name";
    protected static readonly string PriceLabel = "label-price";
    protected static readonly string IconImage = "image";
    protected static readonly string ItemButton = "button-item";
    protected static readonly string ParticleImage = "image-particle";
    protected static readonly string descriptionLabel = "label-description";
    protected static readonly string underCoinIconName = "image-coin-under";

    public string localizationTableKey;
    public Sprite itemIcon;
    private VisualElement item;

    private Label nameLabel, priceLabel;
    private VisualElement iconImage, coinIcon;
    private Button itemButton;
    protected string priceText;
    protected string localizedName;
    protected string localizedDescription;


    // set priceText.

    protected abstract void SetPrices(Label priceLabel);

    public virtual void SetItemVisualAndCallback(VisualElement item, ShopPopupScreenController shopPopupScreenController, Action<ShopItemDataSO> callback)
    {
        CacheVisualElements(item);
        SetPrices(priceLabel);
        (localizedName, localizedDescription) = LocalizeStrings(localizationTableKey);
        nameLabel.text = localizedName;
        iconImage.style.width = itemIcon.texture.width;
        iconImage.style.height = itemIcon.texture.height;
        iconImage.style.backgroundImage = new StyleBackground(itemIcon);
        itemButton.clicked += () => OnButtonClicked(shopPopupScreenController, callback);
    }

    protected virtual void OnButtonClicked(ShopPopupScreenController shopPopupScreenController, Action<ShopItemDataSO> callback)
    {
        shopPopupScreenController.ShopPopupScreen(localizedName, localizedDescription, priceText, itemIcon, () => callback(this));
    }

    private void CacheVisualElements(VisualElement item)
    {
        nameLabel = item.Q<Label>(NameLabel);
        priceLabel = item.Q<Label>(PriceLabel);
        iconImage = item.Q<VisualElement>(IconImage);
        itemButton = item.Q<Button>(ItemButton);
        coinIcon = item.Q<VisualElement>(underCoinIconName);
        this.item = item;
    }

    protected void DeactivateItem()
    {
        item.SetEnabled(false);
    }

    protected void HideCoinIcon()
    {
        coinIcon.style.display = DisplayStyle.None;
    }

    protected string GetLocalizedName(string shopItemName)
    {
        return LocalizationSettings.StringDatabase.GetLocalizedString(LocalizationTableName.TABLE_SHOP_ITEM_NAMES, shopItemName);
    }

    protected string GetLocalizedDescription(string shopItemDescription)
    {
        return LocalizationSettings.StringDatabase.GetLocalizedString(LocalizationTableName.TABLE_SHOP_ITEM_DESCRIPTIONS, shopItemDescription);
    }

    private (string, string) LocalizeStrings(string key)
    {
        string localizedName = GetLocalizedName(key);
        string localizedDescription = GetLocalizedDescription(key);
        return (localizedName, localizedDescription);
    }
}