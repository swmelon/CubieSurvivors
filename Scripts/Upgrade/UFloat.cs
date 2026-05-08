
using System.Collections.Generic;
using UnityEngine;

public class UFloat : PercentUpgradable<float, float>
{
    public UFloat(List<float> values, bool exposed = true, string name= "", Sprite icon = null, Sprite symbol = null, string optionText = "", bool useReciprocal = false)
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
    
    protected override float ComputeBaseValue(int grade)
    {
        // if upgradable variable is rate of fire, then the value should be 1 / value
        // the input value of rate of fire means how many shoots per second.
        // the output value of rate of fire means how many seconds per shoot.
        // 연사속도는 곱연산 -> why? -> 데미지가 합연산이므로 연사속도까지 합연산 할 경우,
        // 무기의 dps에 비례하여 업그레이드 옵션들의 가치가 비슷하기 위해서 퍼센트가 같아지는 문제가 생긴다.
        // 모든 옵션이 10%씩 늘어난다면 재미가 없겠지?
        // 그래서 연사속도는 곱연산으로 한다.
        // 단 범위는 합연산을 해도, 면적으로 적용되기 때문에 제곱을 한 것과 같다.
        // 따라서 합연산해도 데미지보다 낮은 퍼센트로 증가시켜야한다.
        // 그리고 그러므로 모두 다른 퍼센트 증가치를 갖게 된다.
        
        float value = reciprocal ? initialValue * Mathf.Pow(1 + rateOfIncrease, grade) : initialValue + increment * grade;
        
        if (reciprocal)
        {
            value = 1 / value;
        }

        return value;
    }
    
    protected override float GetValueWithBonus(int grade)
    {
        float value = reciprocal ? initialValue * Mathf.Pow(1 + rateOfIncrease, grade) : initialValue + increment * grade;
        
        value += initialValue * bonusRate;
        
        if (reciprocal)
        {
            value = 1 / value;
        }

        return value;
    }
}
