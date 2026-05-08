using System.Collections;
using System;
using UnityEngine;


public class APRampageShooting : AttackPattern<ILockOnWeapon>
{
    [SerializeField]
    private int numShoot = 3;
    
    protected override IEnumerator StartBehaviourRoutine()
    {
        yield return tickDelay;

        user.KeepRotatingWhileAttacking = true;
        
        enemyAnimationController.SetMotionSpeed(speedOfMotion);
        enemyAnimationController.GetAngry();
        
        weapon.SetLockOnMode(false);
        
        concurrentWeapons.ForEach(w => w.SetLockOnMode(false));

        yield return null;
        
        for (int i = 0; i < numShoot; i++)
        {
            enemyAnimationController.SwingLeftAndRight();
            ShootWeapon();
            yield return unitDelay;
        }
        
        user.KeepRotatingWhileAttacking = false;

        yield return tickDelay ;
        
        OnFinishedBehaviour();
    }
}
