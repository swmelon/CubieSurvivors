using Local.Scripts.Extensions;
using System;
using System.Collections;
using UnityEngine;

public class APJumpAndMoonWaveAttack : AttackPattern<EnergyWaveGenerator>
{

    private const int minShootCount = 1;
    private const int maxShootCount = 4;
    private const float healthRatioBase = 2f;

    protected override IEnumerator StartBehaviourRoutine()
    {
        yield return tickDelay;
        user.KeepRotatingWhileAttacking = true;
        enemyAnimationController.SetMotionSpeed(speedOfMotion);
        int numShoot = (int)(RandomExtenstion.GetIntInRange(minShootCount, maxShootCount) * (healthRatioBase - GetBossHealthRatio()));

        for (int i = 0; i < numShoot; i++)
        {
            enemyAnimationController.OnLanded += ShootMoonWave;
            enemyAnimationController.JumpAndDive();
            enemyAnimationController.GetAngry();
            yield return unitDelay;
        }

        user.KeepRotatingWhileAttacking = false;
        yield return tickDelay;

        OnFinishedBehaviour();
    }

    private void ShootMoonWave()
    {
        weapon.ShootMoonWave();
    }
}