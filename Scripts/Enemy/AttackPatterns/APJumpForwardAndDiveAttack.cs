using System;
using System.Collections;


public class APJumpForwardAndDiveAttack : AttackPattern<Weapon>
{

    protected override IEnumerator StartBehaviourRoutine()
    {
        yield return tickDelay;
        enemyAnimationController.SetMotionSpeed(speedOfMotion);
        enemyAnimationController.OnLanded += ShootWeapon;
        enemyAnimationController.JumpForwardAndDive();
        enemyAnimationController.GetAngry();
    }

    protected override void ShootWeapon()
    {
        base.ShootWeapon();
        OnFinishedBehaviour();
    }
}
