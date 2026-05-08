
using Local.Scripts.Extensions;
using System;
using System.Collections;
using UnityEngine;

public class APSpinAndSlash : AttackPattern<DoubleBeamSaber>
{
    private const float spinStartDelay = 1f;

    [SerializeField][Range(1, 10)]
    private int numSlash = 3;
    protected override IEnumerator StartBehaviourRoutine()
    {
        enemyAnimationController.SetMotionSpeed(speedOfMotion);
        weapon.On();

        bool clockwise = RandomExtenstion.FiftyFifty();

        yield return new WaitForSeconds(spinStartDelay);
        
        for (int i = 0; i < numSlash; i++)
        {
            if (clockwise)
            {
                enemyAnimationController.Spin();
            }
            else
            {
                enemyAnimationController.SpinLeft();
            }

            enemyAnimationController.SpinEyeBalls();
            yield return unitDelay;
        }
        
        weapon.Off();
        enemyAnimationController.ResetMotionSpeed();
        
        OnFinishedBehaviour();
    }
}
