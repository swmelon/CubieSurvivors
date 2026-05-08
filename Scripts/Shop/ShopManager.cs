using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class ShopManager : MonoBehaviour
{
    [SerializeField]
    private GameAccessoryManager accessoryManager;

    [SerializeField]
    private GameObject CoinItemSlotsParent;

    [SerializeField]
    private EventChannelSO enterShopEC, exitShopEC;

    [SerializeField]
    private Camera accessoryCamera;

    private ShopSlot[] coinItemSlots;
    private List<Accessory> accessories;
    private List<Accessory> accessoriesToDisplay;

    public List<Accessory> AccessoriesDisplayed
    {
        get
        {
            PopulateSlots();
            return accessoriesToDisplay;
        }
    }

    private void Awake()
    {
        accessories = accessoryManager.AccessoriesOnSale;
        InitializeSlots();
        enterShopEC.Subscribe(OnEnterShop);
        exitShopEC.Subscribe(OnExitShop);
    }
 
    private void OnDestroy()
    {
        enterShopEC.Unsubscribe(OnEnterShop);
        exitShopEC.Unsubscribe(OnExitShop);
    }

    private void InitializeSlots()
    {
        coinItemSlots = CoinItemSlotsParent.GetComponentsInChildren<ShopSlot>(true); // Include inactive children
    }

    public void PopulateSlots()
    {
        // Sort accessories by price in ascending order and take up to 10
        var sortedAccessories = accessories.OrderBy(a => a.Price).Take(10).ToList();
        accessoriesToDisplay = sortedAccessories;


        // Populate coin item slots with sorted and limited number of accessories
        for (int i = 0; i < coinItemSlots.Length; i++)
        {
            if (i < sortedAccessories.Count)
            {
                coinItemSlots[i].SetContent(sortedAccessories[i], sortedAccessories[i].Price); // Assuming 'Price' is a field in 'Accessory'
                coinItemSlots[i].gameObject.SetActive(true); // Activate the slot as it is being used
            }
            else
            {
                coinItemSlots[i].gameObject.SetActive(false); // Deactivate the slot as it is not being used
            }
        }
    }

    private void OnEnterShop()
    {
        PopulateSlots();
        accessoryCamera.gameObject.SetActive(true);
    }

    private void OnExitShop()
    {
        accessoryCamera.gameObject.SetActive(false);
    }
}
