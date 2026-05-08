using StarterAssets;
using System.Collections;
using UnityEngine;


public class BPBreakFloor : BPByBossHealth, IConditionalBehaviourPattern
{
    [SerializeField]
    private float activationHealthRatio = 0.7f;

    [SerializeField]
    private MainCameraChannelSO mainCameraChannel;

    [SerializeField]
    private EventChannelSO loadStageWithoutUpdatingCurveEC;

    private CameraController cameraController;
    private Transform target;
    private CustomThirdPersonController controller;

    private const int breakJumpCount = 3;

    private bool landed = false;
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
        enemyAnimationController.SetMotionSpeed(speedOfMotion);
        user.KeepRotatingWhileAttacking = true;
        yield return tickDelay;

        bool hasTarget = user.TryGetTarget(out target);
        bool hasThirdPersonController = hasTarget && target.TryGetComponent(out controller);

        if (!hasTarget || !hasThirdPersonController)
        {
            user.KeepRotatingWhileAttacking = false;
            OnFinishedBehaviour();
            yield break;
        }

        bool hasDamagable = user.TryGetComponent(out Damagable damagable);

        if (hasDamagable)
        {
            damagable.Invincible();
        }

        for (int i = 0; i < breakJumpCount; i++)
        {
            landed = false;
            enemyAnimationController.RotateTowardTarget();
            enemyAnimationController.OnLanded += OnLand;
            enemyAnimationController.JumpAndDiveFixedSpeed(speedOfMotion);

            while (!landed)
            {
                yield return null;
            }
        }

        if (hasDamagable)
        {
            damagable.OffInvincible();
        }


        loadStageWithoutUpdatingCurveEC.Raise();
        user.KeepRotatingWhileAttacking = false;
        OnFinishedBehaviour();
    }

    private void SetCameraController(CameraController cameraController)
    {
        this.cameraController = cameraController;
    }

    private void OnLand()
    {
        cameraController?.ShakeCamera();
        controller?.JumpIfOnGround();
        landed = true;
    }
}