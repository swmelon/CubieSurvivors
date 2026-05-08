using System;
using System.Collections;


public class BPJumpForward : BehaviourPattern
{
    protected override IEnumerator StartBehaviourRoutine()
    {
        // 스피드는 애니메이터가 거리에 따라 설정하게 한다.
        enemyAnimationController.JumpForwardAndDive();
        enemyAnimationController.GetAngry();

        while (enemyAnimationController.IsJumping)
        {
            yield return null;
        }

        OnFinishedBehaviour();
    }
}
