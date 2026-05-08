
using System;
using System.Collections;

public class APStabRight : AttackPattern<Weapon>
{
    protected override IEnumerator StartBehaviourRoutine()
    {
        ShootWeapon();
        enemyAnimationController.SetMotionSpeed(speedOfMotion);
        enemyAnimationController.StabRight();
        enemyAnimationController.GetAngry();
        
        yield return unitDelay;
        
        enemyAnimationController.ResetMotionSpeed();
        
        
        OnFinishedBehaviour();
    }
}
