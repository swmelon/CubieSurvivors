using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.UIElements;

public class UpgradeCardData : CardData
{
    private static readonly CardType CardType = CardType.Upgrade;
    private static readonly Color CardColor = Color.blue;

    private IUpgradable upgradable;
    
    public UpgradeCardData(IUpgradable upgradable)
    {
        this.upgradable = upgradable;
        this.upgradable.GenerateRandomBonusRate();
    }

    public override CardType GetCardType()
    {
        return CardType;
    }

    public override void SetCardVisualAndCallback(VisualElement card, Sprite backPlate, Action<VisualElement> callback, RenderTexture renderTexture = null)
    {

        CacheVisualElements(card);

        name.text = LocalizationSettings.StringDatabase.GetLocalizedString(LocalizationTableName.TABLE_CARD_TITLE, upgradable.Name);
        option.text = LocalizationSettings.StringDatabase.GetLocalizedString(LocalizationTableName.TABLE_CARD_DESCRIPTION, upgradable.GetOptionText()) + upgradable.GetPercentageText();
        bonus.text = upgradable.GetBonusText(out Color textColor);
        bonus.style.color = textColor;
        icon.style.backgroundImage = new StyleBackground(upgradable.GetIcon());
        symbol.style.backgroundImage = new StyleBackground(upgradable.GetUpgradeSymbol());

        
        bool hasWeapon = upgradable.TryGetWeapon(out Weapon weapon);
        UserWeapon userWeapon = weapon as UserWeapon;
        bool isUserWeapon = ReferenceEquals(userWeapon, null);

        if (hasWeapon && isUserWeapon && userWeapon.TryGetParticleIconCam(out var fxCam))
        {
            fxCam.TurnOnFx(); // �� ���� cardSelectionScreenController���� ��
            particleIcon.style.backgroundImage = new StyleBackground(Background.FromRenderTexture(fxCam.RenderTexture));
        }

        SetBtnCallback(card, button, callback);
        button.style.backgroundImage = new StyleBackground(backPlate);
        fx.style.unityBackgroundImageTintColor = CardColor;
    }
    
    public override void Activate()
    {
        upgradable.Upgrade();
        activated = true;
    }
}
