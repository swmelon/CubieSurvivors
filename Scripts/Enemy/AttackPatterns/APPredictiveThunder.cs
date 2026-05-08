
using System;
using System.Collections;
using UnityEngine;

public class APPredictiveThunder : AttackPattern<TheLightofZeus>
{
    [SerializeField]
    private int numShoot = 5;
    
    protected override IEnumerator StartBehaviourRoutine()
    {
        weapon.SetPredictiveMode();
        user.KeepRotatingWhileAttacking = true;
        
        for(int i = 0; i < numShoot; i++)
        {
            enemyAnimationController.GetAngry();
            yield return unitDelay;
            ShootWeapon();
        }
        
        user.KeepRotatingWhileAttacking = false;
        
        OnFinishedBehaviour();
    }
}
