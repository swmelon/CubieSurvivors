using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class UnlockWeaponCardData : CardData
{
    public event Action Activated;
    private static readonly Color CardColor = Color.red;
    private static readonly CardType CardType = CardType.UnlockWeapon;

    private Weapon weapon;
    private GameWeaponManagerSO gameWeaponManager;

    bool isActivated = false;


    public UnlockWeaponCardData(Weapon weapon, GameWeaponManagerSO gameWeaponManager)
    {
        this.weapon = weapon;
        this.gameWeaponManager = gameWeaponManager;
    }

    public override CardType GetCardType()
    {
        return CardType;
    }

    public override void SetCardVisualAndCallback(VisualElement card, Sprite backPlate, Action<VisualElement> callback, RenderTexture renderTexture = null)
    {

        var name = card.Q<Label>(NameLabel);
        var option = card.Q<Label>(OptionLabel);
        var bonus = card.Q<Label>(BonusLabel);
        var image = card.Q<VisualElement>(IconImage);
        var symbol = card.Q<VisualElement>(SymbolImage);
        var button = card.Q<Button>(CardButton);
        var fx = card.Q<VisualElement>(CardFX);
        var particleIcon = card.Q<VisualElement>(ParticleImage);


        name.text = weapon.Name;
        image.style.backgroundImage = new StyleBackground(weapon.GetIcon());

        SetBtnCallback(card, button, callback);
        button.style.backgroundImage = new StyleBackground(backPlate);
        option.text = CardText.NEW_WEAPON;
        bonus.text = CardText.NOTHING;
        symbol.style.backgroundImage = null;

        UserWeapon userWeapon = weapon as UserWeapon;
        bool isUserWeapon = ReferenceEquals(userWeapon, null);

        if (isUserWeapon && userWeapon.TryGetParticleIconCam(out var fxCam))
        {
            fxCam.TurnOnFx(); // 끌 때는 cardSelectionScreenController에서 끔
            particleIcon.style.backgroundImage = new StyleBackground(Background.FromRenderTexture(fxCam.RenderTexture));
        }


        fx.style.unityBackgroundImageTintColor = CardColor;
    }

    public override void Activate()
    {
        if (isActivated)
        {
            return;
        }

        isActivated = true;
        // 만약 선택되지 않아서 instance를 반납하지 못해도 씬이 리로드 되는것은 확정이다.
        gameWeaponManager.ReturnAndUnlockWeaponInstance(weapon);
        Activated?.Invoke();
    }
}
