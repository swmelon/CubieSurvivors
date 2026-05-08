using UnityEngine;

public class ShopComputer : MonoBehaviour
{
    [SerializeField]
    private GameObject exclamationMark;

    [SerializeField]
    private EventChannelSO enterShopEC, newItemAddedToShopEC;

    [SerializeField]
    private SaveLoadManagerSO saveLoadManager;

    private bool noticeShop = false;

    private void Awake()
    {
        newItemAddedToShopEC.Subscribe(OnNewItemAdded);
    }

    private void OnDestroy()
    {
        newItemAddedToShopEC.Unsubscribe(OnNewItemAdded);
    }

    private void OnEnable()
    {
        noticeShop = saveLoadManager.SaveFile.ShopExclamationMark;
        if (noticeShop)
        {
            exclamationMark.SetActive(true);
        }
        enterShopEC.Subscribe(DisableExMark);
        
    }

    private void OnDisable()
    {
        enterShopEC.Unsubscribe(DisableExMark);
    }

    private void DisableExMark()
    {
        exclamationMark.SetActive(false);
        noticeShop = false;
        saveLoadManager.SaveFile.ShopExclamationMark = false;
        saveLoadManager.Save();
    }

    private void OnNewItemAdded()
    {
        saveLoadManager.SaveFile.ShopExclamationMark = true;
        saveLoadManager.Save();
        exclamationMark.SetActive(true);
    }

}