
using System.Collections.Generic;
using UnityEngine;

public class UWaitForSeconds : PercentUpgradable<WaitForSeconds, float>
{
    public UWaitForSeconds(List<float> values, bool exposed = true, string name = "", Sprite icon = null, Sprite symbol = null, string optionText = "", bool useReciprocal = true) 
        : base(values, name, exposed, icon, symbol, optionText, useReciprocal)
    {
    }


    protected override float GetInitialValue(float initialValueInList)
    {
        return initialValueInList;
    }

    protected override float GetIncrement(float initialValue, float rateOfIncrease)
    {
        return initialValue * rateOfIncrease;
    }

    protected override WaitForSeconds ComputeBaseValue(int grade)
    {
        float value = reciprocal ? initialValue * Mathf.Pow(1 + rateOfIncrease, grade) : initialValue + increment * grade;
        
        if (reciprocal)
        {
            value = 1 / value;
        }

        return new WaitForSeconds(value);
    }
    
    protected override WaitForSeconds GetValueWithBonus(int grade)
    {
        float value = reciprocal ? initialValue * Mathf.Pow(1 + rateOfIncrease, grade) : initialValue + increment * grade;
        
        
        value += initialValue * bonusRate;
        
        if (reciprocal)
        {
            value = 1 / value;
        }

        return new WaitForSeconds(value);
    }
}
