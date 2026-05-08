using System.Collections;
using System;
using UnityEngine;
using UnityEngine.Serialization;
using StarterAssets;


public class APTossUpAndLockOnShooting : AttackPattern<ILockOnWeapon>
{
    [SerializeField]
    private int numShoot = 3;
    
    [FormerlySerializedAs("lockOnTime")]
    [Range(1f, 100f)]
    [SerializeField]
    private float maxDegreePerSec = 10f;

    [Range(5f, 15f)]
    [SerializeField]
    private float projectileSpeed = 10f;

    [SerializeField]
    private MainCameraChannelSO mainCameraChannel;

    private CameraController cameraController;


    private void Start()
    {
        mainCameraChannel.Subscribe(SetCameraController);
    }

    private void OnDestroy()
    {
        mainCameraChannel.Unsubscribe(SetCameraController);
    }



    protected override IEnumerator StartBehaviourRoutine()
    {
        yield return tickDelay;

        user.KeepRotatingWhileAttacking = true;

        enemyAnimationController.Jump();

        yield return unitDelay;

        cameraController.ShakeCamera();

        if (user.TryGetTarget(out Transform target) && target.TryGetComponent(out CustomThirdPersonController controller)) 
        {
            controller.HyperJump();
        }


        enemyAnimationController.GetAngry();
        
        
        weapon.SetLockOnMode(true, maxDegreePerSec, projectileSpeed);
        
        concurrentWeapons.ForEach(w => w.SetLockOnMode(true, maxDegreePerSec, projectileSpeed));

        yield return null;
        
        for (int i = 0; i < numShoot; i++)
        {
            ShootWeapon();
            yield return halfUnitDelay;
        }
        
        user.KeepRotatingWhileAttacking = false;
        
        OnFinishedBehaviour();
    }

    private void SetCameraController(CameraController cameraController)
    {
        this.cameraController = cameraController;
    }

}
