
using System;
using System.Collections.Generic;
using UnityEngine;

public class CardSelectionScreenController : MonoBehaviour
{
    [SerializeField]
    private CardSelectionScreen cardSelectionScreen;

    [SerializeField]
    private CardFXController cardFXController;

    [SerializeField]
    private BooleanEventChannelSO cardSelectionScreenControlChannel;

    [SerializeField]
    private CardEventChannel showCardEventChannel;




    private bool showSelectAnimation = false;

    private void OnEnable()
    {
        showCardEventChannel.Subscribe(ShowCard);
        cardSelectionScreenControlChannel.Subscribe(ShowCardSelectionScreen);
        CardSelectionScreen.CardClicked += OnCardClicked;
        CardSelectionScreen.CardSelectionScreenShown += TurnOnFx;
        CardSelectionScreen.CardSelectionScreenHidden += TurnOffFx;
    }

    private void OnDisable()
    {
        showCardEventChannel.Unsubscribe(ShowCard);
        cardSelectionScreenControlChannel.Unsubscribe(ShowCardSelectionScreen);
        CardSelectionScreen.CardClicked -= OnCardClicked;
        CardSelectionScreen.CardSelectionScreenShown -= TurnOnFx;
        CardSelectionScreen.CardSelectionScreenHidden -= TurnOffFx;

    }

    public void ShowCardSelectionScreen(bool value)
    {
        if (value)
        {
            cardSelectionScreen.ShowScreen();
        }
        else
        {
            cardSelectionScreen.HideScreen();
        }
    }

    public void ShowCardSelectionScreen(bool showSelectAnimation, string buttonText, Action callback = null)
    {
        ShowCardSelectionScreen(true);
        cardSelectionScreen.SetButton(buttonText, callback);
        
        this.showSelectAnimation = showSelectAnimation;
        
    }

    public void ShowCard(CardData cardData)
    {
        cardSelectionScreen.ShowCard(cardData);
    }

    public void ShowCardScrollable(CardData cardData)
    {
        cardSelectionScreen.ShowCardScrollable(cardData);
    }

    private void OnCardClicked()
    {
        

        if (!showSelectAnimation)
        {
            cardSelectionScreen.HideScreen();
            return;
        }

        cardSelectionScreen.StartCardSelectAnimation();

        // 애니메이션
    }

    public void TurnOnFx()
    {
       cardFXController.TurnOnFx();
    }
    
    public void TurnOffFx()
    {
        cardFXController.TurnOffFx();
    }

    public void HideScreen()
    {
        cardSelectionScreen.HideScreen();
    }
}
