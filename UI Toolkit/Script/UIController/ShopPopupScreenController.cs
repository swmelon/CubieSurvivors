
using System;
using UnityEngine;
using UnityEngine.UIElements;

public class ShopPopupScreenController : MonoBehaviour
{
    [SerializeField]
    private ShopPopupScreen popupScreen;

    [SerializeField]
    private MainMenuUIManager mainMenuUIManager;

    private Action buyAction, backAction;

    private void OnEnable()
    {
        popupScreen.BuyButtonClicked += OnBuyButtonClicked;
        popupScreen.BackButtonClicked += OnBackButtonClicked;
    }
    
    private void OnDisable()
    {
        popupScreen.BuyButtonClicked -= OnBuyButtonClicked;
        popupScreen.BackButtonClicked -= OnBackButtonClicked;
    }

    private void OnBuyButtonClicked()
    {
        buyAction?.Invoke();
        popupScreen.HideScreen();
    }

    private void OnBackButtonClicked()
    {
        backAction?.Invoke();
        popupScreen.HideScreen();
    }
    
    public void ShowPopupScreen(string accName, int accRank, int accPrice, Action BuyAction = null, Action backAction = null, bool isCoin = true)
    {
        popupScreen.SetupScreen(accName, accRank, accPrice, isCoin);
        this.buyAction = BuyAction;
        this.backAction = backAction;

        popupScreen.ShowScreen();
    }

    public void ShopPopupScreen(string itemName, string itemDescription, string price, Sprite itemIcon, Action BuyAction = null, Action backAction = null, bool isCoin = true)
    {
        popupScreen.SetupScreen(itemName, itemDescription, price, isCoin: isCoin, itemIcon);
        this.buyAction = BuyAction;
        this.backAction = backAction;

        popupScreen.ShowScreen();
    }
}
