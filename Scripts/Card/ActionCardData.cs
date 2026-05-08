using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.UIElements;


// ī�� ���� �ɼǿ� ���� ��Ÿ �׼� ī�� ������ Ŭ����
public class ActionCardData : CardData
{
    protected readonly IconizedAction iconizedAction;
    protected Color CardColor = Color.white;
    protected CardType CardType = CardType.Action;
    
    public ActionCardData(IconizedAction iconizedAction)
    {
        this.iconizedAction = iconizedAction;
    }
    
    public override CardType GetCardType()
    {
        return CardType;
    }

    public override void SetCardVisualAndCallback(VisualElement card, Sprite backPlate, Action<VisualElement> callback, RenderTexture renderTexture = null)
    {
        CacheVisualElements(card);
        SetLocalizedTexts();

        icon.style.backgroundImage = new StyleBackground(iconizedAction.GetIcon());
        symbol.style.backgroundImage = null;
        SetBtnCallback(card, button, null);

        button.style.backgroundImage = new StyleBackground(backPlate);

        if (iconizedAction.TryGetParticleIconCam(out FXCameraController fxCam))
        {
            fxCam.TurnOnFx();
            particleIcon.style.backgroundImage = new StyleBackground(Background.FromRenderTexture(fxCam.RenderTexture));
        }
        
        fx.style.unityBackgroundImageTintColor = CardColor;
    }

    protected virtual void SetLocalizedTexts()
    {
        name.text = LocalizationSettings.StringDatabase.GetLocalizedString(LocalizationTableName.TABLE_CARD_DESCRIPTION, CardText.OPTION);
        option.text = LocalizationSettings.StringDatabase.GetLocalizedString(LocalizationTableName.TABLE_CARD_DESCRIPTION, iconizedAction.GetOptionText())
            + iconizedAction.GetOptionTextNoTranslate();
        bonus.text = "";
    }

    public override void Activate()
    {
        iconizedAction.GetContent().Invoke();
        activated = true;
        callback?.Invoke(card);
    }
}
