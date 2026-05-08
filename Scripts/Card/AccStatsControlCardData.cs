using System;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Threading.Tasks;

public class AccStatsControlCardData : AccessoryCardData
{
    private static readonly string atkPlusButton = "button-atk-plus";
    private static readonly string atkMinusButton = "button-atk-minus";
    private static readonly string defPlusButton = "button-def-plus";
    private static readonly string defMinusButton = "button-def-minus";
    private static readonly string agiPlusButton = "button-agi-plus";
    private static readonly string agiMinusButton = "button-agi-minus";
    private static readonly string lukPlusButton = "button-luk-plus";
    private static readonly string lukMinusButton = "button-luk-minus";
    private static readonly string pointsLeftElement = "element-points-left";
    private static readonly string pointsLeftLabel = "label-points-left";
    private static readonly string upgradeButton = "button-upgrade";

    private Button atkPlus, atkMinus, defPlus, defMinus, agiPlus, agiMinus, lukPlus, lukMinus;
    private Action<AccStatsControlCardData> onStatChanged, onRequestUpgrade;
    private Label pointsLeft;
    private VisualElement pointsLeftParent;
    private Button upgradeBtn;
    private AccStats initialStats;
    private bool statsApplied;

    // 한 번 업그레이드를 가정
    private int extraStatPoints = 1;
    private int maxExtraStatPoints = 2;

    private int sumDeltaAtk = 0;
    private int sumDeltaDef = 0;
    private int sumDeltaAgi = 0;
    private int sumDeltaLuk = 0;

    private List<SFXTags> composed;

    public AccStatsControlCardData(AccData accData, GameAccessoryManager gameAccessoryManager) : base(accData, gameAccessoryManager)
    {
        initialStats = accData.accessoryStats;
        composed = new List<SFXTags>();
    }

    public void SetCardVisualAndCallback(VisualElement card, Sprite backPlate, Color tintColor, Color textColor, 
        Action<AccStatsControlCardData> onStatChanged, Action<AccStatsControlCardData> onRequestUpgrade)
    {
        composed.Clear();
        atkPlus = card.Q<Button>(atkPlusButton);
        atkMinus = card.Q<Button>(atkMinusButton);
        defPlus = card.Q<Button>(defPlusButton);
        defMinus = card.Q<Button>(defMinusButton);
        agiPlus = card.Q<Button>(agiPlusButton);
        agiMinus = card.Q<Button>(agiMinusButton);
        lukPlus = card.Q<Button>(lukPlusButton);
        lukMinus = card.Q<Button>(lukMinusButton);

        pointsLeftParent = card.Q<VisualElement>(pointsLeftElement);
        pointsLeft = pointsLeftParent.Q<Label>(pointsLeftLabel);
        upgradeBtn = card.Q<Button>(upgradeButton);

        this.onStatChanged = onStatChanged;
        this.onRequestUpgrade = onRequestUpgrade;

        atkPlus.RegisterCallback<ClickEvent>(OnAtkPlusClicked);
        atkMinus.RegisterCallback<ClickEvent>(OnAtkMinusClicked);
        defPlus.RegisterCallback<ClickEvent>(OnDefPlusClicked);
        defMinus.RegisterCallback<ClickEvent>(OnDefMinusClicked);
        agiPlus.RegisterCallback<ClickEvent>(OnAgiPlusClicked);
        agiMinus.RegisterCallback<ClickEvent>(OnAgiMinusClicked);
        lukPlus.RegisterCallback<ClickEvent>(OnLukPlusClicked);
        lukMinus.RegisterCallback<ClickEvent>(OnLukMinusClicked);

        upgradeBtn.RegisterCallback<ClickEvent>(OnUpgradeBtnClicked);

        SetCardVisualOnly(card, backPlate, tintColor, textColor);
    }

    public void SetCardVisualOnly(VisualElement card, Sprite backPlate, Color tintColor, Color textColor)
    {
        base.SetCardVisualAndCallback(card, backPlate, null, tintColor, textColor);
        var cardImage = card.Q<VisualElement>(CardImage);
        cardImage.style.backgroundImage = new StyleBackground(backPlate);
        cardImage.style.unityBackgroundImageTintColor = tintColor;
        UpdatePointsLeft();
    }

    public void ApplyStats()
    {
        statsApplied = true;
    }

    private void OnAtkPlusClicked(ClickEvent evt)
    {
        if (ChangeStat(deltaAtk: 1))
        {
            onStatChanged?.Invoke(this);
            PlayCode(SFXTags.G);
        }
        else
        {
            PlayNegativeUISoundEffect();
        }

        evt.StopPropagation();
    }

    private void OnAtkMinusClicked(ClickEvent evt)
    {
        if (ChangeStat(deltaAtk: -1))
        {
            onStatChanged?.Invoke(this);
            PlayCode(SFXTags.C);
        }
        else
        {
            PlayNegativeUISoundEffect();
        }

        evt.StopPropagation();
    }

    private void OnDefPlusClicked(ClickEvent evt)
    {
        if (ChangeStat(deltaDef: 1))
        {
            onStatChanged?.Invoke(this);
            PlayCode(SFXTags.F);
        }
        else
        {
            PlayNegativeUISoundEffect();
        }

        evt.StopPropagation();
    }

    private void OnDefMinusClicked(ClickEvent evt)
    {
        if (ChangeStat(deltaDef: -1))
        {
            onStatChanged?.Invoke(this);
            PlayCode(SFXTags.D);
        }
        else
        {
            PlayNegativeUISoundEffect();
        }

        evt.StopPropagation();
    }

    private void OnAgiPlusClicked(ClickEvent evt)
    {
        if (ChangeStat(deltaAgi: 1))
        {
            onStatChanged?.Invoke(this);
            PlayCode(SFXTags.B);
        }
        else
        {
            PlayNegativeUISoundEffect();
        }

        evt.StopPropagation();
    }

    private void OnAgiMinusClicked(ClickEvent evt)
    {
        if (ChangeStat(deltaAgi: -1))
        {
            onStatChanged?.Invoke(this);
            PlayCode(SFXTags.E);
        }
        else
        {
            PlayNegativeUISoundEffect();
        }

        evt.StopPropagation();
    }

    private void OnLukPlusClicked(ClickEvent evt)
    {
        if (ChangeStat(deltaLuk: 1))
        {
            onStatChanged?.Invoke(this);
            PlayCode(SFXTags.C_H);
        }
        else
        {
            PlayNegativeUISoundEffect();
        }

        evt.StopPropagation();
    }

    private void OnLukMinusClicked(ClickEvent evt)
    {
        if (ChangeStat(deltaLuk: -1))
        {
            onStatChanged?.Invoke(this);
            PlayCode(SFXTags.F);
        }
        else
        {
            PlayNegativeUISoundEffect();
        }

        evt.StopPropagation();
    }

    private void OnUpgradeBtnClicked(ClickEvent evt)
    {
        if (extraStatPoints == 0)
        {
            onRequestUpgrade?.Invoke(this);
        }
        else
        {
            PlayNegativeUISoundEffect();
        }

        evt.StopPropagation();
    }

    private bool ChangeStat(int deltaAtk = 0, int deltaDef = 0, int deltaAgi = 0, int deltaLuk = 0)
    {
        AccData accData = AccData;

        if (accData == null)
        {
            return false;
        }

        bool result = CheckSumDelta(sumDeltaAtk, deltaAtk) &&
                      CheckSumDelta(sumDeltaDef, deltaDef) &&
                      CheckSumDelta(sumDeltaAgi, deltaAgi) &&
                      CheckSumDelta(sumDeltaLuk, deltaLuk);

        if (!result)
        {
            return false;
        }

        int totalDelta = deltaAtk + deltaDef + deltaAgi + deltaLuk;
        int pointsLeft = extraStatPoints - totalDelta;

        if (totalDelta < 0)
        {
            if (pointsLeft > maxExtraStatPoints)
            {
                return false;
            }
        }
        else
        {
            if (pointsLeft < 0)
            {
                return false;
            }
        }

        result = accData.accessoryStats.ChangeStats(deltaAtk, deltaDef, deltaAgi, deltaLuk);

        if (!result)
        {
            return false;
        }

        sumDeltaAtk += deltaAtk;
        sumDeltaDef += deltaDef;
        sumDeltaAgi += deltaAgi;
        sumDeltaLuk += deltaLuk;

        extraStatPoints = pointsLeft;

        return true;
    }

    private bool CheckSumDelta(int sumDelta, int delta)
    {
        int newSumDelta = sumDelta + delta;

        if (newSumDelta > 2 || newSumDelta < -1)
        {
            return false;
        }

        return true;
    }

    private void UpdatePointsLeft()
    {
        pointsLeft.text = "";

        for (int i = 0; i < extraStatPoints; i++)
        {
            pointsLeft.text += "-";
        }

        if (extraStatPoints == 0)
        {
            pointsLeftParent.style.display = DisplayStyle.None;
            upgradeBtn.style.display = DisplayStyle.Flex;

        }
        else
        {
            pointsLeftParent.style.display = DisplayStyle.Flex;
            upgradeBtn.style.display = DisplayStyle.None;
        }
    }

    public void ResetStatChanges()
    {
        if (statsApplied)
        {
            return;
        }

        sumDeltaAtk = 0;
        sumDeltaDef = 0;
        sumDeltaAgi = 0;
        sumDeltaLuk = 0;
        extraStatPoints = 1;

        AccData.accessoryStats = initialStats;
    }

    private void PlayCode(SFXTags code)
    {
        FMODAudioManager.instance.PlayOneShot(code);
        composed.Add(code);
    }

    public async void PlayComposed()
    {
        await Task.Delay(300);

        for (int i = 0; i < composed.Count; i++)
        {
            FMODAudioManager.instance.PlayOneShot(composed[i]);
            await Task.Delay(100);
        }

        composed.Clear();
    }
}
