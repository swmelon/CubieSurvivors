
using System;
using Local.Scripts.Extensions;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.UIElements;

public class CoinCardData : CardData
{
    private static readonly Color CardColor = Color.yellow;
    private static readonly CardType CardType = CardType.Coin;
    private static readonly float CoinAmountMultiplier = 10f;
        
    private GemManagerSO gemManager;
    private int coinAmount;
    private Sprite iconSprite;
    
    public CoinCardData(int coinAmount)
    {
        this.coinAmount = coinAmount;
    }
    
    public CoinCardData(GemManagerSO gemManager)
    {
        this.gemManager = gemManager;
        this.iconSprite = gemManager.CoinIcon;

        int coinAmount = Mathf.RoundToInt(CoinAmountMultiplier * Mathf.Abs((float)RandomExtenstion.SampleNormal()));
        this.coinAmount = Mathf.Clamp(coinAmount, 1, 100);
    }           
        
    public override CardType GetCardType()
    {
        return CardType;
    }
    
    public override void SetCardVisualAndCallback(VisualElement card, Sprite backPlate, Action<VisualElement> callback, RenderTexture renderTexture = null)
    {
        CacheVisualElements(card);

        name.text = LocalizationSettings.StringDatabase.GetLocalizedString(LocalizationTableName.TABLE_CARD_DESCRIPTION, CardText.COIN);

        // 나중에 {n} 을 대입하여 번역할 수 있게 만들자.
        option.text = CardText.GetNCoin(coinAmount);
        icon.style.backgroundImage = new StyleBackground(this.iconSprite);
        symbol.style.backgroundImage = null;

        SetBtnCallback(card, button, callback);

        button.style.backgroundImage = new StyleBackground(backPlate);
        fx.style.unityBackgroundImageTintColor = CardColor;
        bonus.text = "";
    }
    
    public override void Activate()
    {
        gemManager.GetCoin(coinAmount);
        activated = true;
        FMODAudioManager.instance.PlayOneShot(SFXTags.CoinCollected);
    }
}
