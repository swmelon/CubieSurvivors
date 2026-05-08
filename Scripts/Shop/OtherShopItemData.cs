using UnityEngine;
using UnityEngine.UIElements;

[CreateAssetMenu(fileName = "OtherShopItemData", menuName = "ScriptableObjects/Shop/OtherShopItemData")]
public class OtherShopItemData : ShopItemDataSO
{
    public int price;
    public OtherShopItemType itemType;

    protected override void SetPrices(Label priceLabel)
    {
        priceText = price.ToString();
        priceLabel.text = priceText;
    }
}