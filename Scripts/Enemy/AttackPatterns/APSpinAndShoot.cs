
using Local.Scripts.Extensions;
using System;
using System.Collections;
using UnityEngine;

public class APSpinAndShoot : AttackPattern<Weapon>
{
    protected override IEnumerator StartBehaviourRoutine()
    {
        enemyAnimationController.Spin();

        float startShootingRandomOffset = RandomExtenstion.GetFloatInRange(0.1f, unitDelayTime);
        // no need to use Mathf.Max because max motionSpeed is 4
        
        yield return new WaitForSeconds(startShootingRandomOffset);

        for (int i = 0; i < speedOfMotion; i++)
        {
            ShootWeapon();

            if (i == speedOfMotion - 1)
            {
                yield return new WaitForSeconds(unitDelayTime - startShootingRandomOffset);
            }
            else
            {
                yield return unitDelay;
            }
        }

        enemyAnimationController.ResetMotionSpeed();
        OnFinishedBehaviour();
    }
}
