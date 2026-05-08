using System;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.UIElements;

public class AccessoryCardData : CardData
{
    public enum InteractionMode
    {
        None,
        Equip,
        GetNew,
        GetNewFromRewardStage,
        Fusion,
        Upgrade,
    }

    private static readonly Color CardColor = Color.blue;
    private static readonly CardType CardType = CardType.Accessory;

    protected static readonly string atkLabel = "label-atk";
    protected static readonly string defLabel = "label-def";
    protected static readonly string agiLabel = "label-agi";
    protected static readonly string lukLabel = "label-luk";

    protected static readonly string atkPlusLabel = "label-atk-plus";
    protected static readonly string defPlusLabel = "label-def-plus";
    protected static readonly string agiPlusLabel = "label-agi-plus";
    protected static readonly string lukPlusLabel = "label-luk-plus";

    protected static readonly string atkMinusLabel = "label-atk-minus";
    protected static readonly string defMinusLabel = "label-def-minus";
    protected static readonly string agiMinusLabel = "label-agi-minus";
    protected static readonly string lukMinusLabel = "label-luk-minus";

    protected static readonly string atkBonusLabel = "label-atk-bonus";
    protected static readonly string defBonusLabel = "label-def-bonus";
    protected static readonly string agiBonusLabel = "label-agi-bonus";
    protected static readonly string lukBonusLabel = "label-luk-bonus";

    protected static readonly string cardBorderName = "border-card";



    private Accessory accessory;
    private Sprite iconSprite;
    private GameAccessoryManager gameAccessoryManager;

    private AccData accData;
    private InteractionMode interactionMode;

    private VisualElement cardBorder;
    private bool tinted = false;
    private Color tintColor;

    public VisualElement Card => card;
    public AccData AccData => accData;

    public AccessoryCardData(AccData accData, GameAccessoryManager gameAccessoryManager)
    {
        this.accessory = accData.accessory;
        this.iconSprite = accessory.GetIcon();
        this.gameAccessoryManager = gameAccessoryManager;
        this.accData = accData;
        interactionMode = InteractionMode.None;
    }

    public AccessoryCardData(AccData accData, GameAccessoryManager gameAccessoryManager, InteractionMode interactionMode)
    {
        this.accessory = accData.accessory;
        this.iconSprite = accessory.GetIcon();
        this.gameAccessoryManager = gameAccessoryManager;
        this.accData = accData;
        this.interactionMode = interactionMode;
    }

    public override CardType GetCardType()
    {
        return CardType;
    }

    public AccessoryRank GetRank() => (AccessoryRank)(accData?.GetRank() ?? 0);


    public virtual void SetCardVisualAndCallback(VisualElement card, Sprite backPlate, Action<VisualElement> callback, Color tintColor, Color textColor)
    {
        SetCardVisualAndCallback(card, backPlate, callback);
        name.style.color = textColor;
        button.style.unityBackgroundImageTintColor = tintColor;
        fx.style.unityBackgroundImageTintColor = tintColor;

        tinted = true;
        this.tintColor = tintColor;
    }

    public override void SetCardVisualAndCallback(VisualElement card, Sprite backPlate, Action<VisualElement> callback, RenderTexture renderTexture = null)
    {
        CacheVisualElements(card);

        var atkBonusText = card.Q<Label>(atkBonusLabel);
        var defBonusText = card.Q<Label>(defBonusLabel);
        var agiBonusText = card.Q<Label>(agiBonusLabel);
        var lukBonusText = card.Q<Label>(lukBonusLabel);

        AccStats stats = accData.accessoryStats;
        bool atkUnder0, defUnder0, agiUnder0, lukUnder0;

        atkUnder0 = stats.Attack < 0;
        defUnder0 = stats.Defense < 0;
        agiUnder0 = stats.Agility < 0;
        lukUnder0 = stats.Luck < 0;

        Label atkPlusBar = card.Q<Label>(atkPlusLabel);
        Label atkMinusBar = card.Q<Label>(atkMinusLabel);
        Label defPlusBar = card.Q<Label>(defPlusLabel);
        Label defMinusBar = card.Q<Label>(defMinusLabel);
        Label agiPlusBar = card.Q<Label>(agiPlusLabel);
        Label agiMinusBar = card.Q<Label>(agiMinusLabel);
        Label lukPlusBar = card.Q<Label>(lukPlusLabel);
        Label lukMinusBar = card.Q<Label>(lukMinusLabel);


        
        SetStatBar(stats.Attack, atkMinusBar, atkPlusBar, atkBonusText);
        SetStatBar(stats.Defense, defMinusBar, defPlusBar, defBonusText);
        SetStatBar(stats.Agility, agiMinusBar, agiPlusBar, agiBonusText);
        SetStatBar(stats.Luck, lukMinusBar, lukPlusBar, lukBonusText);

        name.text = LocalizationSettings.StringDatabase.GetLocalizedString(LocalizationTableName.TABLE_ACCESSORY_NAMES, accessory.accessoryName);

        icon.style.backgroundImage = new StyleBackground(this.iconSprite);
        symbol.style.backgroundImage = null;
        
        SetBtnCallback(card, button, callback);

        button.style.backgroundImage = new StyleBackground(backPlate);
        fx.style.unityBackgroundImageTintColor = CardColor;

        cardBorder = card.Q<VisualElement>(cardBorderName);
        this.card = card;
    }

    public void ChangeInteractionMode(InteractionMode interactionMode)
    {
        this.interactionMode = interactionMode;
    }


    public override void Activate()
    {
        if (activated)
        {
            return;
        }

        switch (interactionMode)
        {
            case InteractionMode.None:
                break;
            case InteractionMode.Equip:
                activated = true;
                gameAccessoryManager.Equip(accData);
                break;
            case InteractionMode.GetNew:
                activated = true;
                gameAccessoryManager.GetNew(accData);
                break;
            case InteractionMode.GetNewFromRewardStage:
                activated = true;
                gameAccessoryManager.GetNewFromRewardStage(accData);
                break;
        }

    }

    private void SetStatBar(int statPoint, Label barMinus, Label barPlus, Label bonusText)
    {
        bool under0 = statPoint < 0;
        int abs =  Mathf.RoundToInt(Mathf.Abs(statPoint));
        barMinus.text = "";
        barPlus.text = "";

        Label bar = under0 ? barMinus : barPlus;

        if (under0)
        {
            for (int i = 0; i < abs; i++)
            {
                bar.text += "-";
            }

            bonusText.text = $"-{abs}";
        }
        else
        {
            for (int i = 0; i < abs; i++)
            {
                bar.text += "-";
            }

            bonusText.text = $"+{abs}";
        }
    }

    public void HighlightFX(bool val)
    {
        fx.style.unityBackgroundImageTintColor = val ? Color.red : tinted ? tintColor : CardColor;

        IStyle borderStyle = cardBorder.style;
        int fusionCardBorderWitdh = 5;

        borderStyle.borderRightWidth = val ? fusionCardBorderWitdh : 0;
        borderStyle.borderLeftWidth = val ? fusionCardBorderWitdh : 0;
        borderStyle.borderTopWidth = val ? fusionCardBorderWitdh : 0;
        borderStyle.borderBottomWidth = val ? fusionCardBorderWitdh : 0;
    }
}
