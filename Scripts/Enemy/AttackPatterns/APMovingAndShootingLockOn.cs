using System.Collections;
using System;
using UnityEngine;
using UnityEngine.Serialization;


public class APMovingAndShootingLockOn: AttackPattern<ILockOnWeapon>
{
    [SerializeField]
    private int numShoot = 3;
    
    [FormerlySerializedAs("lockOnTime")]
    [Range(10f, 100f)]
    [SerializeField]
    private float maxDegreePerSec = 10f;
    
    [Range(5f, 15f)]
    [SerializeField]
    private float projectileSpeed = 5f;
    
    protected override IEnumerator StartBehaviourRoutine()
    {
        user.SetKeepChasingWhileAttacking(true);
        
        enemyAnimationController.GetAngry();
        
        weapon.SetLockOnMode(true, maxDegreePerSec);
        
        concurrentWeapons.ForEach(w => w.SetLockOnMode(true, maxDegreePerSec, projectileSpeed));
        
        for (int i = 0; i < numShoot; i++)
        {
            yield return unitDelay;
            ShootWeapon();
        }
        
        user.SetKeepChasingWhileAttacking(false);
        
        OnFinishedBehaviour();
    }
}