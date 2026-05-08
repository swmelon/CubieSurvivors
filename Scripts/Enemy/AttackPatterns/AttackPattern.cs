using System;
using System.Collections.Generic;
using UnityEngine;

// 추후 일반 적도 패턴을 가질지 생각해본다.
// 패턴이 작동하는 방식
/// <summary>
/// 타겟이 거리 안에 들어왔을 때,
/// 가능한 패턴 중 하나를 실행한다.
/// 
/// 이는 적의 체력에 따라 달라질 수 있다. 다른 고려 요소는 없는가?
/// 적의 체력에 따라 적이 강해지거나 약해질 수 있다. -> 이것은 "Enemy"에서 구현
/// </summary>


public abstract class AttackPattern<T> : ConditionalBehaviourPattern, IAttackPattern where T : IWeapon 
{
    [SerializeField]
    protected FloatChannelSO bossHealthRatioChannel;

    [SerializeField][Range(0f, 10f)]
    private float attackDistance;
    
    [SerializeField][Range(0f, 1f)]
    private float unlockedHealthRatio = 1f;

    protected T weapon;
    protected List<T> concurrentWeapons = new List<T>();
    
    public float GetAttackDistance() => attackDistance;
    
    private const float unitDelayPeriod = 1f;
    private const float halfUnitDelayFactor = 0.5f;

     protected virtual void Awake()
    {
        unitDelay = new WaitForSeconds(unitDelayPeriod / speedOfMotion);
        halfUnitDelay = new WaitForSeconds((unitDelayPeriod / speedOfMotion) * halfUnitDelayFactor);
        weapon = GetComponent<T>();
    }

    public override bool IsAvailable()
    {
        return !activated && unlockedHealthRatio >= user.HealthRatio;
    }

    public bool IsAvailable(float healthRatio)
    {
        return !activated && unlockedHealthRatio >= healthRatio;
    }

    public override void StartAction(Action onActionFinished)
    {
        concurrentWeapons.Clear();
        base.StartAction(onActionFinished);
    }
    
    public void StartAction(Action onActionFinished, List<IWeapon> weapons)
    {
        concurrentWeapons.Clear();

        for (int i = 0; i < weapons.Count; i++)
        {
            if (weapons[i] is T weapon)
            {
                concurrentWeapons.Add(weapon);
            }
        }   

        base.StartAction(onActionFinished);
    }

    public override bool IsTargetInRange(float distance)
    {
        return attackDistance >= distance;
    }
    
    protected virtual void ShootWeapon()
    {
        if (concurrentWeapons.Count == 0)
        {
            weapon.Damage();
            return;
        }

        foreach (T concurrentWeapon in concurrentWeapons)
        {
            concurrentWeapon.Damage();
        }
    }

    protected float GetBossHealthRatio()
    {
        return Mathf.Clamp(bossHealthRatioChannel.Value, 0f, 1f);
    }
}
