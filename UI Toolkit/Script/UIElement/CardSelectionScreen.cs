
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class CardSelectionScreen : MenuScreen
{
    [SerializeField]
    private CardSetupManager cardSetupManager;

    [SerializeField]
    private CardAnimator cardAnimator;

    public static event Action CardSelectionScreenShown;
    public static event Action CardSelectionScreenHidden;
    public static event Action CardClicked;
    private event Action buttonClickedAfterCardSelection;
    
    private const string buttonName = "button-bottom";
    private const string cardContainerName = "cards";
    private const string cardContainerScrollViewName = "scrollview-cards";
    private Button button;

    private VisualElement cardContainer;
    private VisualElement cardContainerScrollview;
    private List<VisualElement> cards = new List<VisualElement>();
    private VisualElement selectedCard;
    private bool animationPlayed;
  

    public void ShowCard(CardData cardData)
    {
        VisualElement card = cardSetupManager.SetupCard(cardData, OnSelectCard);
        cardContainer.Add(card);
        cards.Add(card);
    }

    public void ShowCardScrollable(CardData cardData)
    {
        VisualElement card = cardSetupManager.SetupCard(cardData, OnSelectCard);
        cardContainerScrollview.Add(card);
        cards.Add(card);
    }

    protected override void SetVisualElements()
    {
        base.SetVisualElements();
        cardContainer = screen.Q<VisualElement>(cardContainerName);
        cardContainerScrollview = screen.Q<VisualElement>(cardContainerScrollViewName);
        button = screen.Q<Button>(buttonName);
        button.text = "";
    }

    public override void ShowScreen()
    {
        button.style.display = DisplayStyle.None;
        ClearCards();
        base.ShowScreen();
        CardSelectionScreenShown?.Invoke();

        if (cards.Count != 3)
        {

        }
    }

    public override void HideScreen()
    {
        base.HideScreen();
        CardSelectionScreenHidden?.Invoke();

        if (cards.Count != 3)
        {
            
        }
    }

 

    private void ClearCards()
    {
        foreach (var card in cards)
        {
            if (animationPlayed && ReferenceEquals(card, selectedCard))
            {
                screen.Remove(selectedCard);
                selectedCard = null;
                animationPlayed = false;
                continue;
            }

            cardContainer.Remove(card);
        }
        
        cards.Clear();
        selectedCard = null;
    }

    private void OnSelectCard(VisualElement card)
    {
        if (selectedCard != null)
        {
            return;
        }
        selectedCard = card;
        CardClicked?.Invoke();
    }

    public void StartCardSelectAnimation()
    {
        selectedCard.focusable = false;

        foreach (var card in cards)
        {
            if (card != selectedCard)
            {
                card.style.display = DisplayStyle.None;
            }
        }

        selectedCard.style.position = Position.Absolute;
        selectedCard.RemoveFromHierarchy();
        screen.Add(selectedCard);

        cardAnimator.AnimateCardZoomIn(selectedCard, screen);
        cardAnimator.AddCallback(OnCardSelectAnimationFinished);
        animationPlayed = true;
    }

    private void OnCardSelectAnimationFinished()
    {
        // 그냥 버튼 만들어서 끝내는게 나을듯
        button.style.display = DisplayStyle.Flex;
        button.clicked += OnButtonClicked;
    }


    public void OnButtonClicked()
    {
        HideScreen();
        buttonClickedAfterCardSelection?.Invoke();
        buttonClickedAfterCardSelection = null;
        button.clicked -= OnButtonClicked;
    }

    public void SetButton(string text, Action callback)
    {
        button.text = GetLocalizedString(text);
        buttonClickedAfterCardSelection += callback;
    }
}
