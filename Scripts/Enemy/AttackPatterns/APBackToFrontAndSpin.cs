
using System;
using System.Collections;
using UnityEngine;

public class APBackToFrontAndSpin : AttackPattern<Weapon>
{
    private const float slotTransitionDelay = 1f;

    protected override IEnumerator StartBehaviourRoutine()
    {
        enemyAnimationController.BackWeaponSlotToFront();
        yield return new WaitForSeconds(slotTransitionDelay);
        
        enemyAnimationController.SetMotionSpeed(speedOfMotion);
        
        enemyAnimationController.Spin();
        yield return unitDelay;
        
        enemyAnimationController.ResetMotionSpeed();
        
        enemyAnimationController.BackWeaponSlotToBack();

        yield return new WaitForSeconds(slotTransitionDelay);
        OnFinishedBehaviour();
    }
}
