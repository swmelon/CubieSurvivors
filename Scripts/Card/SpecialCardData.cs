using System;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.UIElements;


// Special card data class
public class SpecialCardData : ActionCardData
{
    public SpecialCardData(IconizedAction iconizedAction) : base(iconizedAction)
    {
        CardType = CardType.Special;
        CardColor = Color.green;
    }

    public override void SetCardVisualAndCallback(VisualElement card, Sprite backPlate, Action<VisualElement> callback, RenderTexture renderTexture = null)
    {
        base.SetCardVisualAndCallback(card, backPlate, callback, renderTexture);
        this.callback = callback;
    }

    protected override void SetLocalizedTexts()
    {
        name.text = LocalizationSettings.StringDatabase.GetLocalizedString(LocalizationTableName.TABLE_CARD_DESCRIPTION, CardText.OPTION);
        option.text = LocalizationSettings.StringDatabase.GetLocalizedString(LocalizationTableName.TABLE_CARD_DESCRIPTION, iconizedAction.GetOptionText())
            + iconizedAction.GetOptionTextNoTranslate();
        bonus.text = "";
    }
}
