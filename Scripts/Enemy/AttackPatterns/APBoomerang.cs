using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class APBoomerang : AttackPattern<DoubleBeamSaber>
{

    protected override IEnumerator StartBehaviourRoutine()
    {
        user.KeepRotatingWhileAttacking = true;

        enemyAnimationController.GetAngry();
        weapon.On();

        yield return unitDelay;

        weapon.DisableAnimator();
        weapon.ThrowLeft();

        yield return unitDelay;

        weapon.ThrowRight();

        yield return unitDelay;
        yield return unitDelay;

        weapon.EnableAnimator();
        weapon.Off();
        yield return unitDelay;

        user.KeepRotatingWhileAttacking = false;
        OnFinishedBehaviour();
    }
}