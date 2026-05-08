
using System;
using System.Collections;

public class APChargeAttack : AttackPattern<Weapon>
{
    protected override IEnumerator StartBehaviourRoutine()
    {
        ShootWeapon();
        enemyAnimationController.SetMotionSpeed(speedOfMotion);
        enemyAnimationController.Dash();
        enemyAnimationController.GetAngry();
        
        yield return unitDelay;
        
        enemyAnimationController.ResetMotionSpeed();
        OnFinishedBehaviour();
    }
}
