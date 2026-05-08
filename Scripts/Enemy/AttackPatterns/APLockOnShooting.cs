using System.Collections;
using System;
using UnityEngine;
using UnityEngine.Serialization;


public class APLockOnShooting : AttackPattern<ILockOnWeapon>
{
    [SerializeField]
    private int numShoot = 3;
    
    [FormerlySerializedAs("lockOnTime")]
    [Range(1f, 1000f)]
    [SerializeField]
    private float maxDegreePerSec = 10f;
    
    [Range(5f, 15f)]
    [SerializeField]
    private float projectileSpeed = 5f;
    
    
    protected override IEnumerator StartBehaviourRoutine()
    {
        user.KeepRotatingWhileAttacking = true;
        
        enemyAnimationController.GetAngry();
        
        
        weapon.SetLockOnMode(true, maxDegreePerSec, projectileSpeed);
        
        concurrentWeapons.ForEach(w => w.SetLockOnMode(true, maxDegreePerSec, projectileSpeed));

        yield return null;
        
        for (int i = 0; i < numShoot; i++)
        {
            ShootWeapon();
            yield return unitDelay;
        }
        
        user.KeepRotatingWhileAttacking = false;
        
        OnFinishedBehaviour();
    }
}
