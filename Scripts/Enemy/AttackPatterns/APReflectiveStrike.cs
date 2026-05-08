
using System;
using System.Collections;
using Local.Scripts.Extensions;
using UnityEngine;
using UnityEngine.Serialization;

public class APReflectiveStrike : AttackPattern<WingWingPlates>
{
    private const int minNumShoot = 3;

    [SerializeField]
    private int maxNumShoot = 8;
    private float numShoot;

    [Range(5f, 15f)]
    [SerializeField]
    private float projectileSpeed = 5f;

    protected override IEnumerator StartBehaviourRoutine()
    {
        numShoot = RandomExtenstion.GetIntInRange(minNumShoot, maxNumShoot);
        user.KeepRotatingWhileAttacking = true;
        weapon.SetLockOnMode(false, projectileSpeed: projectileSpeed);
        enemyAnimationController.GetAngry();
        
        for (int i = 0; i < numShoot; i++)
        {
            ShootWeapon();
            yield return unitDelay;
        }
        
        enemyAnimationController.Wink();
        yield return unitDelay;
        
        weapon.ReflectiveStrike();
        
        user.KeepRotatingWhileAttacking = false;
        
        OnFinishedBehaviour();
    }
}
