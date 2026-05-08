using System.Collections.Generic;
using Local.Scripts.Extensions;
using UnityEngine;

public abstract class PercentUpgradable<T, U> : Upgradable<T>
{
    protected U initialValue;
    protected T currentValue;
    protected int upgradableTime;
    protected float rateOfIncrease;
    protected float increment;
    protected float bonusRate;
    protected bool reciprocal;
    protected bool noBonus;

    private static readonly float maxBonusRate = 0.05f;
    private static Dictionary<int, Color> bonusToColor = new Dictionary<int, Color>()
    {
        {0, Color.white},
        {1, Color.green},
        {2, Color.blue},
        {3, Color.magenta},
        {4, Color.yellow},
        {5, Color.red},
        {6, Color.cyan},
        {7, Color.gray},
        {8, Color.black},
    };

    protected  PercentUpgradable(List<float> values, string name, bool exposed = true, Sprite icon = null, Sprite symbol = null, string optionText = "", 
        bool useReciprocal = false) : base( exposed, icon, symbol, name, optionText)
    {
        initialValue = GetInitialValue(values[0]);
        reciprocal = useReciprocal;
        rateOfIncrease = values[1]; // between 0 and 1
        increment = GetIncrement(initialValue, rateOfIncrease);
        upgradableTime = Mathf.RoundToInt(values[2]);
        
        for (int grade = 0; grade <= upgradableTime; grade++)
        {
            this.values.Add(ComputeBaseValue(grade));
        }

        currentValue = this.values[0];
    }
    
    protected abstract U GetInitialValue(float initialValueInList);
    protected abstract float GetIncrement(U initialValue, float rateOfIncrease);
    protected abstract T ComputeBaseValue(int grade);
    protected abstract T GetValueWithBonus(int grade);

    public override T Value => buff ? base.Value : currentValue;

    public override void Upgrade()
    {
        base.Upgrade();
        currentValue = GetValueWithBonus(GetGrade());
    }

    public override void GenerateRandomBonusRate()
    {
        if (noBonus)
        {
            bonusRate = 0;
            return;
        }

        bonusRate = RandomExtenstion.GetFloatInRange(0, maxBonusRate);
    }

    public override string GetPercentageText()
    {
        return " +" + Mathf.RoundToInt((rateOfIncrease + bonusRate) * 100) + "%";
    }

    public override string GetBonusText(out Color color)
    {
        string bonusText = base.GetBonusText(out color);
        int bonusPercent = Mathf.RoundToInt(bonusRate * 100);
        color = bonusToColor[bonusPercent];
        bonusText += "(+" + Mathf.RoundToInt(bonusRate * 100) + "%)";
        return bonusText;
    }
}