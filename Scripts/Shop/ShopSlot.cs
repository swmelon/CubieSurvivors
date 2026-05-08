using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopSlot : MonoBehaviour
{
    private Sprite icon;
    private IIconized content;
    private int price;

    [SerializeField]
    private AccessoryStatsSelectionScreenController accessorySelectionScreenController;

    [SerializeField]
    private PopupScreenController popupScreenController;

    [SerializeField]
    private ShopPopupScreenController shopPopupScreenController;

    [SerializeField]
    private GemManagerSO gemManager;

    [SerializeField]
    private GameAccessoryManager accessoryManager;

    [SerializeField]
    private ObjectActiveController popupUIActiveController;


    [SerializeField]
    private Image image;


    [SerializeField]
    private TemporalPlatform temporalPlatform;

    private LayerMask displayedAccLayer;
    private PriceDisplay priceDisplay;
    private Button button;
    private Accessory accDisplayed;
    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnButtonClicked);

        priceDisplay = GetComponent<PriceDisplay>();
        displayedAccLayer = LayerMask.NameToLayer("Prop");
    }

    public void SetContent(IIconized content, int price)
    {
        this.content = content;
        this.price = price;

        icon = content.GetIcon();

        priceDisplay.SetPrice(price);

        if (image != null)
        {
            image.sprite = icon;
        }
    }

    private void OnButtonClicked()
    {
        Accessory accessory = content as Accessory;
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

            shopPopupScreenController.ShowPopupScreen(accessory.name, 1, price, OnBuyButtonClicked, DestroyAccDisplayed);
            accDisplayed = instance;
        }
    }

    private void OnBuyButtonClicked()
    {
        DestroyAccDisplayed();
        Accessory accessory = content as Accessory;

        if (ReferenceEquals(accessory, null))
        {
            return; 
        }

        if (gemManager.PayCoin(priceDisplay.Price))
        {
            accessorySelectionScreenController.ShowAccessoryStatsSelectionScreen(accessory);
        }
        else
        {
            FMODAudioManager.instance.UIButtonClickedNegative();
            // message player that they don't have enough coins
            popupScreenController.ShowPopupScreen("Not enough coins", UIText.OK);
        }
    }

    private void DestroyAccDisplayed()
    {
        if (accDisplayed != null)
        {
            Destroy(accDisplayed.gameObject);
        }
    }
}
