using System.Collections.Generic;
using UnityEngine;

public class UInt : PercentUpgradable<int, int>
{
    public UInt(List<float> values, bool exposed = true, string name = "", Sprite icon = null,  Sprite symbol = null, string optionText = "", bool useReciprocal = false, bool noBonus = false) : 
        base(values, name, exposed, icon, symbol, optionText, useReciprocal)
    {
        this.noBonus = noBonus;
    }

    protected override int GetInitialValue(float initialValueInList)
    {
        return Mathf.RoundToInt(initialValueInList);
    }
    
    protected override float GetIncrement(int initialValue, float rateOfIncrease)
    {
        return initialValue * rateOfIncrease;
    }
    
    protected override int ComputeBaseValue(int grade)
    {
        return Mathf.RoundToInt(initialValue + increment * grade);
    }

    protected override int GetValueWithBonus(int grade)
    {
        return Mathf.RoundToInt(initialValue * (1 + bonusRate) + increment * grade);
    }
}
