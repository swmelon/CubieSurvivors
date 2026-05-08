using UnityEngine.Serialization;
using UnityEngine;
using System;
using System.Collections;

public class APPrecisionBombStrike: AttackPattern<Mortar>
{
    [SerializeField]
    private int numShoot = 3;

    protected override IEnumerator StartBehaviourRoutine()
    {
        user.KeepRotatingWhileAttacking = true;

        enemyAnimationController.SpinEyeBalls();
        weapon.SetAlignMode();

        yield return new WaitForSeconds(weapon.SlerpTime);



        for (int i = 0; i < numShoot; i++)
        {
            weapon.ShootTarget();
            weapon.SetAlignMode();
            yield return unitDelay;
        }

        weapon.ResetRotationMode();
        yield return new WaitForSeconds(weapon.SlerpTime);

        user.KeepRotatingWhileAttacking = false;

        OnFinishedBehaviour();
    }
}