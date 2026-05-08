using UnityEngine;
using UnityEngine.UIElements;


[CreateAssetMenu(fileName = "AdsRemoverItemData", menuName = "ScriptableObjects/Shop/AdsRemoverItemData")]
public class AdsRemoverItemData : PremiumShopItemData
{
    [SerializeField]
    private SaveLoadManagerSO saveLoadManager;

    public override void OnProcessedPurchase(string productId)
    {
       saveLoadManager.RemoveAds();
    }

    public override void SetItemVisualAndCallback(VisualElement item, ShopPopupScreenController shopPopupScreenController, System.Action<ShopItemDataSO> callback)
    {
        base.SetItemVisualAndCallback(item, shopPopupScreenController, callback);

        if (saveLoadManager.AdsRemoved())


        {
            DeactivateItem();
        }
    }

    public override void ProductOwned()
    {
        if (!saveLoadManager.AdsRemoved())
        {
            Debug.Log("Recover Purchase Data : Ads Remover");
            saveLoadManager.RemoveAds();
        }
    }
}