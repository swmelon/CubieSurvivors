using UnityEngine;
using TMPro;

public class PriceDisplay : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI priceText;

    [SerializeField]
    private RectTransform currencyTypeIcon;

    [SerializeField]
    private float spacing = 30f;

    private int price;

    public int Price => price;

    public void SetPrice(int price)
    {
        this.price = price;
        priceText.text = price.ToString();
        UpdateCurrencyIconPosition();
    }

    private void UpdateCurrencyIconPosition()
    {
        // Calculate the total width of the text
        float textWidth = priceText.preferredWidth;

        // Position the coin icon to the left of the text
        currencyTypeIcon.anchoredPosition = new Vector2(-(textWidth * 0.5f + spacing), currencyTypeIcon.anchoredPosition.y);
    }
}