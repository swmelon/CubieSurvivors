
using System;
using System.Collections.Generic;
using UnityEngine;

public class AccessoryCollectionScreenController : MonoBehaviour
{
    [SerializeField]
    private CardSelectionScreen cardSelectionScreen;

    [SerializeField]
    private BooleanEventChannelSO cardSelectionScreenControlChannel;

    [SerializeField]
    private CardEventChannel showCardEventChannel;

    [SerializeField]
    private List<FXCameraController> fxCamerasAlwaysOn;

    [SerializeField]
    private List<FXCameraController> fxCameras;

    private bool showSelectAnimation = false;

    
    

    public void ShowCardCollectionScreen(List<AccData> cardCollections)
    {
    }

    public void ShowCard(CardData cardData)
    {
        cardSelectionScreen.ShowCard(cardData);
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

    private void TurnOnFx()
    {
        foreach (var fx in fxCamerasAlwaysOn)
        {
            fx.TurnOnFx();
        }
    }
    
    private void TurnOffFx()
    {
        foreach (var fx in fxCameras)
        {
            fx.TurnOffFx();
        }
    }
}
