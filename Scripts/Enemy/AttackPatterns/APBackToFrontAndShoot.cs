
using System;
using System.Collections;
using UnityEngine;

public class APBackToFrontAndShoot : AttackPattern<Weapon>
{
    [SerializeField]
    private int numShoot = 3;

    [SerializeField]
    private float backToFrontSpeed = 2.5f;

    protected override IEnumerator StartBehaviourRoutine()
    {
        enemyAnimationController.SetMotionSpeed(backToFrontSpeed);
        enemyAnimationController.BackWeaponSlotToFront();

        yield return unitDelay;

        enemyAnimationController.SetMotionSpeed(speedOfMotion);
        user.KeepRotatingWhileAttacking = true;

        for (int i = 0; i < numShoot; i++)
        {
            ShootWeapon();
            yield return unitDelay;
        }

        user.KeepRotatingWhileAttacking = false;

        enemyAnimationController.SetMotionSpeed(backToFrontSpeed);
        enemyAnimationController.BackWeaponSlotToBack();

        yield return unitDelay;

        enemyAnimationController.ResetMotionSpeed();
        OnFinishedBehaviour();
    }
}
