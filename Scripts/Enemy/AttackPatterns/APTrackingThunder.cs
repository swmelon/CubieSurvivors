
using System;
using System.Collections;
using UnityEngine;

public class APTrackingThunder : AttackPattern<TheLightofZeus>
{
    [SerializeField]
    private int numShoot = 5;
    
    protected override IEnumerator StartBehaviourRoutine()
    {
        weapon.SetTrackingMode();
        
        for(int i = 0; i < numShoot; i++)
        {
            ShootWeapon();
            yield return unitDelay;
        }
        
        OnFinishedBehaviour();
    }
}
