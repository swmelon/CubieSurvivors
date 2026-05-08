using System;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;

public class NewWeaponCardData : CardData
{
    public event Action Activated;
    private static readonly Color CardColor = Color.red;
    private static readonly CardType CardType = CardType.NewWeapon;

    private Weapon weapon;   
    private WeaponManager weaponManager;
    
    
    
    public NewWeaponCardData(Weapon weapon, WeaponManager weaponManager)
    {
        this.weapon = weapon;
        this.weaponManager = weaponManager;
    }

    public override CardType GetCardType()
    {
        return CardType;
    }

    public override void SetCardVisualAndCallback(VisualElement card, Sprite backPlate, Action<VisualElement> callback, RenderTexture renderTexture = null)
    {

        CacheVisualElements(card);

        name.text = LocalizationSettings.StringDatabase.GetLocalizedString(LocalizationTableName.TABLE_CARD_TITLE, weapon.Name);
        icon.style.backgroundImage = new StyleBackground(weapon.GetIcon());

        SetBtnCallback(card, button, callback);

        button.style.backgroundImage = new StyleBackground(backPlate);
        option.text = LocalizationSettings.StringDatabase.GetLocalizedString(LocalizationTableName.TABLE_CARD_DESCRIPTION, CardText.NEW_WEAPON);
        bonus.text = CardText.NOTHING;
        symbol.style.backgroundImage = null;

        UserWeapon userWeapon = weapon as UserWeapon;
        bool isUserWeapon = ReferenceEquals(userWeapon, null);

        if (isUserWeapon && userWeapon.TryGetParticleIconCam(out var fxCam))
        {
            fxCam.TurnOnFx(); // �� ���� cardSelectionScreenController���� ��
            particleIcon.style.backgroundImage = new StyleBackground(Background.FromRenderTexture(fxCam.RenderTexture));
        }

        fx.style.unityBackgroundImageTintColor = CardColor;
    }
    
    public override void Activate()
    {
        weaponManager.Mount(weapon);
        Activated?.Invoke();
        activated = true;
    }
}
