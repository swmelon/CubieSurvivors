
using System;
using System.Collections;

public class APHammerDown : AttackPattern<Weapon>
{
    protected override IEnumerator StartBehaviourRoutine()
    {
        ShootWeapon();
        enemyAnimationController.HammerDown();
        enemyAnimationController.GetAngry();
        
        yield return unitDelay;
        OnFinishedBehaviour();
    }
}
