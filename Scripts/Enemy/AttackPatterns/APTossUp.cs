
using System;
using System.Collections;

public class APTossUp : AttackPattern<Weapon>
{
    protected override IEnumerator StartBehaviourRoutine()
    {
        enemyAnimationController.TossUp();
        enemyAnimationController.GetAngry();
        enemyAnimationController.OnTossUp += ShootWeapon;
        yield return unitDelay;
        OnFinishedBehaviour();
    }
}
