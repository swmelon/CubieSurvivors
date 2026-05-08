
using System;
using System.Collections;
using UnityEngine;

public abstract class APBossPattern : AttackPattern<Boss>
{
    private const float activationDelay = 1f;

    protected override IEnumerator StartBehaviourRoutine()
    {
        enemyAnimationController.Shake();
        enemyAnimationController.GetAngry();
        yield return new WaitForSeconds(activationDelay);
        OnActivated();
        OnFinishedBehaviour();
    }

    protected abstract void OnActivated();
}
