using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.UIElements;
using System;


public abstract class PremiumShopItemData : ShopItemDataSO
{
    [SerializeField]
    private string productId;

    public string ProductId => productId;
    public abstract void OnProcessedPurchase(string productId);

    public virtual void ProductOwned()
    {
        // non-consumable items should override this method
    }
    public override void SetItemVisualAndCallback(VisualElement item, ShopPopupScreenController shopPopupScreenController, Action<ShopItemDataSO> callback)
    {
        base.SetItemVisualAndCallback(item, shopPopupScreenController, callback);
        HideCoinIcon();
    }

     
    protected override void SetPrices(Label priceLabel)
    {
        if (!IAPManager.Instance.TryGetProduct(productId, out Product product))
        {
            priceText = UIText.LBL_WRONG_PRODUCT_ID;
            DeactivateItem();
        }
        else
        {
            priceText = product.metadata.localizedPriceString;
            priceLabel.text = priceText;
        }
    }

    protected override void OnButtonClicked(ShopPopupScreenController shopPopupScreenController, Action<ShopItemDataSO> callback)
    {
        shopPopupScreenController.ShopPopupScreen(localizedName, localizedDescription, priceText, itemIcon, () => IAPManager.Instance.BuyProductID(productId), isCoin: false);
    }
}