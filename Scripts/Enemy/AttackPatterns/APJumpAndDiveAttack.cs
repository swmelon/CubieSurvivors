using System;
using System.Collections;


public class APJumpAndDiveAttack : AttackPattern<Weapon>
{
    protected override IEnumerator StartBehaviourRoutine()
    {
        yield return tickDelay;
        enemyAnimationController.SetMotionSpeed(speedOfMotion);
        enemyAnimationController.OnLanded += ShootWeapon;
        enemyAnimationController.JumpAndDive();
        enemyAnimationController.GetAngry();
        yield return unitDelay;
    }

    protected override void ShootWeapon()
    {
        base.ShootWeapon();
        OnFinishedBehaviour();
    }
}
