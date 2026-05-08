
using System;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UIElements;

public abstract class CardData
{
    protected static readonly string NameLabel = "label-name";
    protected static readonly string OptionLabel = "label-option";
    protected static readonly string PercentageLabel = "label-percentage";
    protected static readonly string BonusLabel = "label-bonus";
    protected static readonly string IconImage = "image";
    protected static readonly string SymbolImage = "image-symbol";
    protected static readonly string CardButton = "button-card";
    protected static readonly string CardImage = "image-card";
    protected static readonly string CardFX = "fx-card";
    protected static readonly string ParticleImage = "image-particle";
    protected Action<VisualElement> callback;
    protected VisualElement card;
    protected Button button;
    protected bool activated;

    protected VisualElement icon, symbol, particleIcon, fx;
    protected Label name, option, bonus;


    public abstract CardType GetCardType();
    public abstract void Activate();
    public abstract void SetCardVisualAndCallback(VisualElement card, Sprite backPlate, Action<VisualElement> callback, RenderTexture renderTexture = null);
    public virtual void SetFont(StyleFontDefinition cardFont)
    {
        name.style.unityFontDefinition = cardFont;
        option.style.unityFontDefinition = cardFont;
        bonus.style.unityFontDefinition = cardFont;
    }

    public void Refresh()
    {
        activated = false;
    }

    protected void CacheVisualElements(VisualElement card)
    {
        name = card.Q<Label>(NameLabel);
        option = card.Q<Label>(OptionLabel);
        bonus = card.Q<Label>(BonusLabel);
        icon = card.Q<VisualElement>(IconImage);
        symbol = card.Q<VisualElement>(SymbolImage);
        button = card.Q<Button>(CardButton);
        fx = card.Q<VisualElement>(CardFX);
        particleIcon = card.Q<VisualElement>(ParticleImage);
    }

    protected void PlayPositiveUISoundEffect()
    {
        FMODAudioManager.instance.PlayOneShot(SFXTags.UISelectPositive);
    }

    protected void PlayNegativeUISoundEffect()
    {
        FMODAudioManager.instance.PlayOneShot(SFXTags.UISelectNegative);
    }

    protected void SetBtnCallback(VisualElement card, Button button, Action<VisualElement> callback)
    {
        button.RegisterCallback<ClickEvent>(OnClickBtn);
        button.RegisterCallback<KeyDownEvent>(OnKeyDownBtn);
        button.RegisterCallback<NavigationSubmitEvent>(OnSubmitBtn);

        this.card = card;
        this.button = button;
        this.callback = callback;
    }

    private void OnClickBtn(ClickEvent evt)
    {
        evt.StopPropagation();
        ActivateAndCallback();
    }

    private void OnKeyDownBtn(KeyDownEvent evt)
    {
        if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.Space)
        {
            evt.StopPropagation();
            ActivateAndCallback();
        }
    }

    private void OnSubmitBtn(NavigationSubmitEvent evt)
    {
        evt.StopPropagation();
        ActivateAndCallback();
    }

    private void ActivateAndCallback()
    {
        if (activated)
        {
            return;
        }

        Activate();
        callback?.Invoke(card);
        FMODAudioManager.instance.PlayOneShot(SFXTags.UISelectPositive);
    }
}
