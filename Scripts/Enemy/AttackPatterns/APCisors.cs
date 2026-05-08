using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class APCisors : AttackPattern<DoubleBeamSaber>
{
    
    protected override IEnumerator StartBehaviourRoutine()
    {
        user.KeepRotatingWhileAttacking = true;
        
        enemyAnimationController.GetAngry();
        weapon.On();

        yield return unitDelay;
        weapon.SetAttackMode(1);
     
        yield return unitDelay;

        weapon.Off();
        weapon.SetAttackMode(0);

        yield return unitDelay;

        user.KeepRotatingWhileAttacking = false;
        OnFinishedBehaviour();
    }
}
