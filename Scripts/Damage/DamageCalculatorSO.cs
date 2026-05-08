
using System;
using Local.Scripts.Extensions;
using UnityEngine;
using UnityEngine.Serialization;


[CreateAssetMenu(fileName = "damageCalculator", menuName = "ScriptableObjects/Damage/damageCalculator", order = SOAssetMenuIndex.Damage)]
public class DamageCalculatorSO : ScriptableObject, IDependentInitialization
{
    [SerializeField]
    private PermanentUpgradableStat upgradables;

    [SerializeField]
    private IntChannelSO totalDamageCountChannel;

    [SerializeField]
    private EventChannelSO gameStartEC;
    private float criticalProb => upgradables.UCriticalProb.Value;
    private float criticalDamageMultiplier => upgradables.UCriticalDamageMultiplier.Value;
    private float damageMultiplier => upgradables.UDamageMultiplier.Value;

    private float damageBonus = 1f, criticalProbBonus = 1f;

    // -5 ~ 5
    private int attack = 0;
    private int defense = 0;
    private int totalDamage = 0;

    private int damageCount = 0;
    private Action<int> onDamage;
    private bool useCallback = false;
    private float defenseDamageMultiplier;

    public void Initialize()
    {
        damageBonus = 1f;
        criticalProbBonus = 1f;
        totalDamage = 0;
        gameStartEC.Subscribe(ResetTotalDamage);
        useCallback = false;
    }

    private void OnDisable()
    {
        gameStartEC.Unsubscribe(ResetTotalDamage);
    }
    public void SetAttackAndDefenseStats(int attack, int defense)
    {
        this.attack = attack;
        this.defense = defense;
        
        CalculateDefenseDamageMult(defense);
    }

    public int CalcDamage(int damage, out bool isCritical)
    {
        damage = (int)((damage +attack)* damageMultiplier * damageBonus);
        isCritical = RandomExtenstion.IsHappen(criticalProb * criticalProbBonus);
        
        if (isCritical)
        {
            damage = (int) (damage * criticalDamageMultiplier);
        }
        
        SetTotalDamage(totalDamage + damage);

        if (useCallback)
        {
            onDamage.Invoke(damage);
        }

        return damage;
    }

    public int TakeDamage(int damage)
    {
        damage = Mathf.Max(0, (int)(damage * defenseDamageMultiplier));
        return damage;
    }

    public void SetDamageBonus(float bonus)
    {
        bonus = Mathf.Clamp(bonus, 1f, 1.5f);
        damageBonus = bonus;
    }

    public void SetCriticalProbBonus(float bonus)
    {
        bonus = Mathf.Clamp(bonus, 1f, 1.5f);
        criticalProbBonus = bonus;
    }

    public void ResetDamageBonus()
    {
        damageBonus = 1f;
    }

    public void ResetCriticalProbBonus()
    {
        criticalProbBonus = 1f;
    }

    public void CallbackDamage(Action<int> callback)
    {
        if (callback == null)
        {
            return;
        }

        onDamage += callback;
        useCallback = true;
    }

    public void RemoveCallback(Action<int> callback)
    {
        onDamage -= callback;

        if (onDamage == null)
        {
            useCallback = false;
        }
    }

    private void ResetTotalDamage()
    {
        totalDamage = 0;
    }

    private void SetTotalDamage(int total)
    {
        totalDamage = total;
        totalDamageCountChannel.Register(totalDamage);
    }

    private const float defenseBase = 10f;
    private const float defenseNegativeFactor = 0.6f;

    private void CalculateDefenseDamageMult(int defense)
    {
        if (defense > 0)
        {
            defenseDamageMultiplier = defenseBase / (defenseBase + defense);
        }
        else
        {
            defenseDamageMultiplier = defenseBase / (defenseBase + defenseNegativeFactor * defense);
        }
    }
}
