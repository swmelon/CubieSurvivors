using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Local.Scripts.Extensions;



[RequireComponent(typeof(WeaponUsableEnemy))]
public class EnemyWeaponManager : WeaponManager, IEnemyWeaponManager
{
    private WeaponUsableEnemy enemy;
    // this is used to modify AP's weight by distance
    private float weightConst = 2f;

    // 최근 2개의 AP를 기억하기 위한 배열
    private IAttackPattern[] memory = new IAttackPattern[2];

    protected override void Awake()
    {
        base.Awake();
        enemy = GetComponent<WeaponUsableEnemy>();
    }

    public bool TryGetAttackPatternRandomly(out IAttackPattern attackPattern)
    {
        if (!slotInitialized)
        {
            InitializeSlots();    
        }

        List<IAttackPattern> attackPatterns = new List<IAttackPattern>();
      
        foreach (WeaponSlot weaponSlot in weaponSlots)
        {
            if (weaponSlot.IsFull(out Weapon weapon) && weapon.TryGetRandomAttackPattern(
                    out IAttackPattern pattern, (float)enemy.Health/enemy.MaxHealth))
            {
                    attackPatterns.Add(pattern);
            }
        }

        if (attackPatterns.Count == 0)
        {
            attackPattern = null;
            return false;
        }

        attackPattern = attackPatterns.PickRandom();
        return true;
    }
    
    public bool TryGetAttackPatternByDistanceToTarget(float distanceToTarget, out IAttackPattern chosenPattern)
    {
        if (!slotInitialized)
        {
            InitializeSlots();
        }

        List<IAttackPattern> attackPatterns = new List<IAttackPattern>();

        float healthRatio = (float)enemy.Health/enemy.MaxHealth;
        
        foreach (var weaponSlot in weaponSlots)
        {
            if (weaponSlot.IsFull(out Weapon weapon))
            {
                attackPatterns = attackPatterns.Concat(
                    weapon.GetAvailableAttackPatterns(healthRatio)).ToList();
            }
        }


        // 없으면
        if (attackPatterns.Count == 0)
        {
            chosenPattern = null;
            return false;
        }
        
        // 하나만 있으면
        if (attackPatterns.Count == 1)
        {
            chosenPattern = attackPatterns[0];
            return true;
        }

        // 메모리 체크 : 최근 2번 연속 실행된 패턴이 있으면
        if (memory[0] != null && memory[1] != null && memory[0] == memory[1])
        {
            // 해당 패턴을 제거
            attackPatterns.Remove(memory[0]);
        }
        
        attackPatterns.FisherShuffle();
        
        // weighted method
        
        float totalWeight = 0f;
        float[] weights = new float[attackPatterns.Count];

        for (int i = 0; i < attackPatterns.Count; i++)
        {
            float weight = 1 / (Mathf.Abs(distanceToTarget - attackPatterns[i].GetAttackDistance()) +1 + weightConst );
            weights[i] = weight;
            totalWeight += weight;
        }    
        float randomValue = RandomExtenstion.GetFloatInRange(0, totalWeight);
        
        chosenPattern = null;

        for (int i = 0; i < attackPatterns.Count; i++)
        {
            randomValue -= weights[i];
            if (randomValue <= 0)
            {
                // 메모리 업데이트
                memory[1] = memory[0];
                memory[0] = attackPatterns[i];
                chosenPattern = attackPatterns[i];
                return true;
            }
        }
        
        return false;
    }

    public List<IWeapon> GetWeaponsContainAttackPattern(IConditionalBehaviourPattern attackPattern)
    {
        List<IWeapon> weapons = new List<IWeapon>();
        
        foreach (WeaponSlot slot in weaponSlots)
        {
            if (slot.IsFull(out Weapon weapon) && weapon.ContainAttackPattern(attackPattern))
            {
                weapons.Add(weapon);
            }
        }

        return weapons;
    }

    protected override void UnmountAllAndParachute()
    {
        foreach (Weapon weapon in UnmountAll())
        {
            gameWeaponManager.ReturnWeaponInstance(weapon);
        }
    }
}
