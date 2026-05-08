using System.Collections;
using System;
using UnityEngine;


public class APMovingAndShooting: AttackPattern<Weapon>
{
    [SerializeField]
    private int numShots = 3;
    
    protected override IEnumerator StartBehaviourRoutine()
    {
        user.SetKeepChasingWhileAttacking(true);
        
        enemyAnimationController.GetAngry();
        
        for (int i = 0; i < numShots; i++)
        {
            yield return unitDelay;
            ShootWeapon();
        }
        
        user.SetKeepChasingWhileAttacking(false);
        
        OnFinishedBehaviour();
    }
}