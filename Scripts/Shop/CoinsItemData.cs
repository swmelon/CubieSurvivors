using System;
using UnityEngine.UIElements;
using UnityEngine;

[CreateAssetMenu(fileName = "CoinsItemData", menuName = "ScriptableObjects/Shop/CoinsItemData")]
public class CoinsItemData : PremiumShopItemData
{
    [SerializeField]
    private GemManagerSO gemManager;

    private const string upperCoinIconName = "image-coin-upper";

    public override void OnProcessedPurchase(string productId)
    {
        if (productId.StartsWith("coins_"))
        {
            int coinAmounts = int.Parse(productId.Split('_')[1]);
            gemManager.PurchaseCoin(coinAmounts);
        }
        else
        {
            Debug.LogError("Product ID does not match the expected format: " + productId);
        }
    }

    public override void SetItemVisualAndCallback(VisualElement item, ShopPopupScreenController shopPopupScreenController, Action<ShopItemDataSO> callback)
    {
        base.SetItemVisualAndCallback(item, shopPopupScreenController, callback);

        VisualElement upperCoinIcon = item.Q<VisualElement>(upperCoinIconName);
        upperCoinIcon.style.display = DisplayStyle.Flex;
    }
}