using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.Rendering.DebugUI;

public class CardAnimator : MonoBehaviour
{
    private Action animationFinished;
    private Action additionalAction;

    [SerializeField]
    private float animationDuration = 1f;

    [SerializeField]
    private float selectedCardScale = 1.2f;

    [SerializeField]
    private AnimationCurve moveCurve;

    private bool animateSelectedCard, animateCards;
    private float time;
    private float timeCards;
    private float targetX, targetY, startX, startY;
    private Stack<(float, float, float, float, Vector3, Vector3)> prevAnimation;
    private List<float> targetXList, targetYList, startXList, startYList;
    private List<VisualElement> selectedCards;

    private Vector3 startScale;
    private Vector3 targetScale;
    private VisualElement selectedCard;

    private bool newAnimationCalled;

    public bool IsAnimating => animateSelectedCard || animateCards;

    private void Awake()
    {
        targetXList = new List<float>();
        targetYList = new List<float>();
        startXList = new List<float>();
        startYList = new List<float>();
        selectedCards = new List<VisualElement>();
        prevAnimation = new Stack<(float, float, float, float, Vector3, Vector3)>();
    }

    private void Update()
    {
        UpdateCard();
        UpdateCards();
    }

    private void UpdateCard()
    {
        if (!animateSelectedCard)
        {
            return;
        }

        time += Time.deltaTime;

        if (time >= animationDuration)
        {
            animateSelectedCard = false;
            selectedCard.style.left = targetX;
            selectedCard.style.top = targetY;
            selectedCard.transform.scale = targetScale;

            newAnimationCalled = false;
            animationFinished?.Invoke();

            if (!newAnimationCalled)
            {
                animationFinished = null;
            }

            additionalAction?.Invoke();
            additionalAction = null;

            return;
        }

        float t = time / animationDuration;
        t = moveCurve.Evaluate(t);

        selectedCard.style.left = Mathf.Lerp(startX, targetX, t);
        selectedCard.style.top = Mathf.Lerp(startY, targetY, t);
        selectedCard.transform.scale = Vector3.Slerp(startScale, targetScale, t);
    }

    private void UpdateCards()
    {
        if (!animateCards)
        {
            return;
        }

        timeCards += Time.deltaTime;

        if (timeCards >= animationDuration)
        {
            animateCards = false;
            for (int i = 0; i < selectedCards.Count; i++)
            {
                selectedCards[i].style.left = targetXList[i];
                selectedCards[i].style.top = targetYList[i];
            }

            newAnimationCalled = false;
            animationFinished?.Invoke();

            if (!newAnimationCalled)
            {
                animationFinished = null;
            }

            additionalAction?.Invoke();
            additionalAction = null;
            return;
        }

        float t = timeCards / animationDuration;
        t = moveCurve.Evaluate(t);

        for (int i = 0; i < selectedCards.Count; i++)
        {
            selectedCards[i].style.left = Mathf.Lerp(startXList[i], targetXList[i], t);
            selectedCards[i].style.top = Mathf.Lerp(startYList[i], targetYList[i], t);
        }
    }

    public void AnimateCardZoomIn(VisualElement card, Vector3 start, Vector3 end)
    {
        if (IsAnimating)
        {
            return;
        }

        startX = start.x;
        startY = start.y;
        targetX = end.x;
        targetY = end.y;
        startScale = start.z * Vector3.one;
        targetScale = end.z * Vector3.one;
        selectedCard = card;
        time = 0;
        animateSelectedCard = true;
        newAnimationCalled = true;
    }

    public void AnimateCardZoomIn(VisualElement card, VisualElement screen)
    { 
        AnimateCardZoomIn(card, screen, selectedCardScale, null);
    }

    public void AnimateCardZoomIn(VisualElement card, VisualElement screen, float zoomScale, Action callback = null)
    {
        if (IsAnimating)
        {
            return;
        }

        IResolvedStyle screenStyle = screen.resolvedStyle;
        float screenWidth = screenStyle.width;
        float screenHeight = screenStyle.height;
        IResolvedStyle cardStyle = card.resolvedStyle;
        float cardWidth = cardStyle.width;
        float cardHeight = cardStyle.height;

        targetX = screenWidth * 0.5f - cardWidth * 0.5f;
        targetY = screenHeight * 0.5f - cardHeight * 0.5f;

        startX = cardStyle.left;
        startY = cardStyle.top;
        startScale = card.transform.scale;
        targetScale = card.transform.scale * zoomScale;
        selectedCard = card;
        time = 0;
        animateSelectedCard = true;
        newAnimationCalled = true;
        animationFinished = callback;
        PushAnimationInfo();
    }

    public void AnimateCardZoomIn(VisualElement card, IResolvedStyle referenceCardStyle, VisualElement screen, float zoomScale, Action callback = null)
    {
        if (IsAnimating)
        {
            return;
        }

        IResolvedStyle screenStyle = screen.resolvedStyle;
        float screenWidth = screenStyle.width;
        float screenHeight = screenStyle.height;

        targetX = screenWidth * 0.5f - referenceCardStyle.width * 0.5f;
        targetY = screenHeight * 0.5f - referenceCardStyle.height * 0.5f;

        startX = referenceCardStyle.left;
        startY = referenceCardStyle.top;
        startScale = card.transform.scale;
        targetScale = card.transform.scale * zoomScale;
        selectedCard = card;
        time = 0;
        animateSelectedCard = true;
        newAnimationCalled = true;
        animationFinished = callback;
        PushAnimationInfo();
    }

    public void AnimateCardZoomIn(VisualElement card, VisualElement screen, VisualElement parent, float offset = 0f, Action callback = null)
    {
        IResolvedStyle screenStyle = screen.resolvedStyle;
        float screenWidth = screenStyle.width;
        float screenHeight = screenStyle.height;
        IResolvedStyle cardStyle = card.resolvedStyle;
        float cardWidth = cardStyle.width;
        float cardHeight = cardStyle.height;

        IResolvedStyle parentStyle = parent.resolvedStyle;

        targetX = screenWidth * 0.5f - cardWidth * 0.5f;
        targetY = screenHeight * 0.5f - cardHeight * 0.5f;

        startX = cardStyle.left + parentStyle.left;
        startY = cardStyle.top + parentStyle.top + offset;
        startScale = card.transform.scale;
        targetScale = card.transform.scale * selectedCardScale;
        selectedCard = card;
        time = 0;
        animateSelectedCard = true;
        newAnimationCalled = true;
        animationFinished = callback;
        PushAnimationInfo();
    }

    public void AnimateCardsDisplay(List<VisualElement> cards, VisualElement screen, List<VisualElement> parent, float offset = 0f, Action callback = null)
    {
        if (IsAnimating) 
        {
            return;
        }

        if (cards.Count == 0)
        {
            return;
        }

        if (cards.Count == 1)
        {
            AnimateCardZoomIn(cards[0], screen, parent[0], offset, callback);
        }

        targetXList.Clear();
        targetYList.Clear();
        startXList.Clear();
        startYList.Clear();
        selectedCards.Clear();

        IResolvedStyle screenStyle = screen.resolvedStyle;
        float screenWidth = screenStyle.width;
        float screenHeight = screenStyle.height;
      


        for (int i = 0; i < cards.Count; i++)
        {
            IResolvedStyle parentStyle = parent[i].resolvedStyle;
            IResolvedStyle cardStyle = cards[i].resolvedStyle;

            float cardWidth = cardStyle.width;
            float cardHeight = cardStyle.height;

            float targetX = screenWidth * (i + 1) / (1 + cards.Count) - cardWidth * 0.5f;
            float targetY = screenHeight * 0.5f - cardHeight * 0.5f;
            float startX = cardStyle.left + parentStyle.left;
            float startY = cardStyle.top + parentStyle.top + offset;

            targetXList.Add(targetX);
            targetYList.Add(targetY);
            startXList.Add(startX);
            startYList.Add(startY);
            selectedCards.Add(cards[i]);
        }

        timeCards = 0;
        animateCards = true;
        newAnimationCalled = true;
        animationFinished = callback;
    }

    public void ZoomOutSelectedCard(Action callback, VisualElement card = null)
    {
        startX = targetX;
        startY = targetY;
        startScale = targetScale;

        while (prevAnimation.Count > 0)
        {
            (float, float, float, float, Vector3, Vector3) prev = prevAnimation.Pop();
            targetX = prev.Item1;
            targetY = prev.Item3;
            targetScale = prev.Item5;
        }

        time = 0;
        animateSelectedCard = true;
        newAnimationCalled = true;
        animationFinished = callback;

        if (card != null)
        {
            selectedCard = card;
        }
    }

    public void ReleaseCardsDisplayed(Action callback)
    {
        List<float> tempXList = targetXList;
        List<float> tempYList = targetYList;

        targetXList = startXList;
        targetYList = startYList;

        startXList = tempXList;
        startYList = tempYList;

        timeCards = 0;
        animateCards = true;
        newAnimationCalled = true;
        animationFinished = callback;
    }

    public void FuseCardsDisplayed(VisualElement screen, Action callback)
    {
        startXList = targetXList;
        startYList = targetYList;

        List<float> newTargetXList = new List<float>();
        List<float> newTargetYList = new List<float>();

        IResolvedStyle screenStyle = screen.resolvedStyle;
        IResolvedStyle cardStyle = selectedCards[0].resolvedStyle;

        float cardWidth = cardStyle.width;

        float targetX = screenStyle.width * 0.5f - cardWidth * 0.5f;
        float targetY = screenStyle.height * 0.5f - cardStyle.height * 0.5f;

        for (int i = 0; i < selectedCards.Count; i++)
        {
            newTargetXList.Add(targetX);
            newTargetYList.Add(targetY);
        }

        targetXList = newTargetXList;
        targetYList = newTargetYList;

        timeCards = 0;
        animateCards = true;
        newAnimationCalled = true;
        animationFinished = callback;
    }

    public void AddCallback(Action callback)
    {
        additionalAction += callback;
    }

    public void PushAnimationInfo()
    {
        (float, float, float, float, Vector3, Vector3) prev = (startX, targetX, startY, targetY, startScale, targetScale);
        prevAnimation.Push(prev);
    }
}