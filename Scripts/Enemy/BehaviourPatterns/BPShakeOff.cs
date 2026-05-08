using System.Collections;
using StarterAssets;
using UnityEngine;

public class BPShakeOff :BehaviourPattern
{
    [SerializeField]
    private MainCameraChannelSO mainCameraChannel;

    [SerializeField]
    private float forceMagnitude = 70f;

    [SerializeField]
    private float noneAffectedDistance = 2.5f;

    private bool addForce = false;
    private CustomThirdPersonController userController;
    private Vector3 forceDirection;

    
    private float time;
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
        enemyAnimationController.SetMotionSpeed(speedOfMotion);
        enemyAnimationController.JumpAndDive();
        yield return unitDelay;

        cameraController.ShakeCamera();

        if (!user.TryGetTarget(out Transform target) ||!target.TryGetComponent(out userController))
        {
            OnFinishedBehaviour();
            yield break;
        }

        addForce = true;
        forceDirection = target.position - user.transform.position;
        forceDirection.y = 0f;

        if (forceDirection.magnitude > noneAffectedDistance)
        {
            OnFinishedBehaviour();
            yield break;
        }

        forceDirection.Normalize();
        time = 0f;
        userController?.IgnoreInput();

        yield return unitDelay;

        addForce = false;

        OnFinishedBehaviour();
    }

    private void Update()
    {
        if (!addForce)
        {
            return;
        }
        
        userController?.AddExtraForce(forceDirection, forceMagnitude* Mathf.Max(unitDelayTime- time, 0f));
        time += Time.deltaTime;
    }

    protected override void OnFinishedBehaviour()
    {
        addForce = false;
        userController?.IgnoreInputUntillHitGround();
        base.OnFinishedBehaviour();
    }

    private void SetCameraController(CameraController value)
    {
        cameraController = value;
    }
}
