using System;
using UnityEngine;
using UnityEngine.Serialization;


// world z axis direction. Start from North, clockwise
public enum WorldDirection
{
    North,
    East,
    South,
    West
}


public class CameraController : MonoBehaviour
{
    [SerializeField]
    private HomeScreen homeScreen;

    [SerializeField]
    private Vector3 initialPosition, initialRotation;

    [SerializeField]
    private float initialFOV = 14f;

    [SerializeField]
    private CharSelectionScreen charSelectionScreen;

    [SerializeField]
    private Vector3 charSelectionPosition, charSelectionRotation;

    [SerializeField]
    private StageSelectionScreen stageSelectionScreen;

    [SerializeField]
    private Vector3 stageSelectionPosition, stageSelectionRotation;

    [Header("Listener")] 
    [SerializeField] 
    private EventChannelSO gameStartEventChannel;
        
    [SerializeField]
    private EventChannelSO playerFallEventChannel;

    [SerializeField] 
    private EventChannelSO enterBossStageEventChannel;
    
    [SerializeField]
    private EventChannelSO bossDeadEventChannel;

    [SerializeField]
    private EventChannelSO goBackToIdleModeEC;

    [SerializeField]
    private EventChannelSO defaultStageTransitionEC;

    [SerializeField]
    private Vector3EventChannelSO playerPositionMovedChannel;

    [Header("Variable Subscriber")] 
    [SerializeField]
    private TransformChannelSO playerTransformChannel;
    
    [SerializeField]
    private TransformChannelSO bossTransformChannel;

    [FormerlySerializedAs("mainCameraChannel")]
    [Header("Variable Publisher")]
    [SerializeField] 
    private MainCameraChannelSO mainMainCameraChannel;
    
    [SerializeField]
    private WorldDirectionChannelSO worldDirectionChannel;

    [Header("Top Down Settings")]
    [SerializeField]
    private float lookDownAngle;

    [SerializeField]
    private float baseLookDistance;

    [SerializeField]
    private float maxLookDistance;

    [Range(0.1f, 1f)]
    [SerializeField]
    private float deltaLookDistance;

    [SerializeField]
    private AnimationCurve smoothCurve;
    
    [SerializeField]
    private float smoothTime;
    
    // 카메라가 포커스하고 싶은 플레이어의 높이
    [SerializeField] 
    private float defaultCamHeight = 0.2f;

    [Header("Third Person Settings")] 
    [SerializeField]
    private float thirdPersonDistance;

    [SerializeField] 
    private float thirdPersonCamHeight;
    
    [SerializeField] 
    private float thirdPersonFocusHeight;

    [Header("Fixed Point Settings")]
    [SerializeField]
    private AnimationCurveContainer animationCurveContainer;

    [Header("Rotation Settings")]
    [SerializeField]
    private float rotationMaxSpeed = 50f; // The speed of rotation

    [SerializeField]
    private float rotationSmoothTime = 0.1f; // The smooth time

    [Header("Character Camera Settings")]
    [SerializeField]
    private Camera characterCamera;

    [Header("Character UI Settings")]
    [SerializeField]
    private FollowingCharacterUI followingCharacterUI;



    public Camera Camera => mainCamera;
    public static Vector3 FIXED_CAM_MODE_ROTATION = new Vector3(30, 45, 0);
    public static Vector3 FIXED_CAM_MODE_FORWARD = Quaternion.Euler(FIXED_CAM_MODE_ROTATION) * Vector3.forward;

    private float currentLookDistance;

    private Transform playerTransform, bossTransform;
    private Vector3 startPoint, endPoint;
    private Color startColor, endColor;
    private float colorChangeTime, colorTimeCount;
    private Vector3 offset;
    private Vector3 startSmoothPosition;
    private Quaternion startRotation;
    private Quaternion endRotation;
    private Vector3 playerHeightRegulatedPosition;
    private Vector3 playerPosition;
    private Camera mainCamera;
    private float time, smoothTimeCount;
    private bool smoothMotion;
    private float cameraPositionVelocity, camRotationVelocity = 0f;
    private float startSize, endSize;
    private float shakeDuration, shakeMagnitude, currentShakeDuration;
    private bool shakeCamera;
    
    private Vector3[] camDirection = new Vector3[4];
    private Vector3[] camPosition = new Vector3[4];
    private int directionIndex;
    private CamState camState;
    private float minCamYHeight = 1.7f;

    // prevent clipping
    private float fixedOrthogonalSafeDistance = 20f;

    private AnimationCurve stopSmootlyCurve;

    public CamState State => camState;

    public enum CamState {ThirdPerson, TopDown, Transition, Idle, MovePoint, FixedOthograpicMode, FixedTrackingMode}
    
    private void Awake()
    {
        mainCamera = GetComponent<Camera>();
        camState = CamState.Idle;
        const float camDirAngleNe = 45f;
        const float camDirAngleSe = 135f;
        const float camDirAngleSw = 225f;
        const float camDirAngleNw = 315f;
        // initialize camDirection
        camDirection[2] = Quaternion.Euler(-lookDownAngle, camDirAngleNe, 0) * Vector3.forward;
        camDirection[3] = Quaternion.Euler(-lookDownAngle, camDirAngleSe, 0) * Vector3.forward;
        camDirection[0] = Quaternion.Euler(-lookDownAngle, camDirAngleSw, 0) * Vector3.forward;
        camDirection[1] = Quaternion.Euler(-lookDownAngle, camDirAngleNw, 0) * Vector3.forward;

        transform.position = initialPosition;
        transform.rotation = Quaternion.Euler(initialRotation);
        mainCamera.fieldOfView = initialFOV;
        characterCamera.gameObject.SetActive(false);
        currentLookDistance = baseLookDistance;
        stopSmootlyCurve = animationCurveContainer.StopSmoothly;
    }

    private void OnEnable()
    {
        mainMainCameraChannel.Register(this);
        gameStartEventChannel.Subscribe(GameStart);
        enterBossStageEventChannel.Subscribe(ThirdPersonMode);
        bossDeadEventChannel.Subscribe(OffThirdPersonMode);
        playerTransformChannel.Subscribe(SetPlayerTransform);
        playerPositionMovedChannel.Subscribe(OnPlayerMoved);
        defaultStageTransitionEC.Subscribe(ZoomOutAndLookDownPlayer);

        homeScreen.ScreenStarted += GoBackToIdleMode;
        charSelectionScreen.ScreenStarted += OnCharSelectionScreenShown;
        stageSelectionScreen.ScreenStarted += OnStageSelectionScreenShown;

    }

    private void OnDisable()
    {
        mainMainCameraChannel.Unregister(this);
        gameStartEventChannel.Unsubscribe(GameStart);
        enterBossStageEventChannel.Unsubscribe(ThirdPersonMode);
        bossDeadEventChannel.Unsubscribe(OffThirdPersonMode);
        playerTransformChannel.Unsubscribe(SetPlayerTransform);
        playerPositionMovedChannel.Unsubscribe(OnPlayerMoved);
        defaultStageTransitionEC.Unsubscribe(ZoomOutAndLookDownPlayer);

        homeScreen.ScreenStarted -= GoBackToIdleMode;
        charSelectionScreen.ScreenStarted -= OnCharSelectionScreenShown;
        stageSelectionScreen.ScreenStarted -= OnStageSelectionScreenShown;
    }

    private const int targetFrameRate = 60;

    private void Start()
    {
        Application.targetFrameRate = targetFrameRate;
    }

    public void LateUpdate()
    {
        if (colorTimeCount < colorChangeTime)
        {
            mainCamera.backgroundColor = Color.Lerp(startColor, endColor, colorTimeCount / colorChangeTime);
            colorTimeCount += Time.deltaTime;
        }

        switch (camState)
        {
            case CamState.MovePoint:
            {
                time += Time.deltaTime;
                transform.position = Vector3.Lerp(startPoint, endPoint, smoothCurve.Evaluate(time));
                transform.rotation = Quaternion.Slerp(startRotation, endRotation, time);

                if (time > 1f)
                {
                    camState = CamState.Idle;
                    transform.position = endPoint;
                    transform.rotation = endRotation;
                }

                return;
            }
            case CamState.Idle:
                return;
        }
        
        if (smoothMotion)
        {
            playerPosition = Vector3.Lerp(startSmoothPosition, playerTransform.position,
            smoothCurve.Evaluate(smoothTimeCount / smoothTime));
            smoothTimeCount += Time.deltaTime;

            if (smoothTimeCount > smoothTime)
            {
                smoothMotion = false;
            }
        }
        else
        {
            playerPosition = playerTransform.position;
        }

        playerHeightRegulatedPosition = playerPosition;
        playerHeightRegulatedPosition.y = defaultCamHeight + Mathf.Min(0.5f, playerPosition.y);


        switch (camState)
        {
            case CamState.TopDown:
            {
                transform.position = playerHeightRegulatedPosition + offset;
                CheckMinumumCamHeight();
                break;
            }
            case CamState.ThirdPerson:
            {
                if (bossTransform == null)
                {
                    return;
                }

                time += Time.deltaTime;
                Vector3 bossPosition = bossTransform.position;
                Vector3 bossDirection = bossPosition - playerPosition;
                bossDirection.y = 0f;
                bossDirection.Normalize();

                Vector3 camForward = transform.forward;
                camForward.y = 0f;
                camForward.Normalize();

                float angle = Vector3.SignedAngle(camForward, bossDirection, Vector3.up);

                angle = Mathf.SmoothDampAngle(0, angle, ref cameraPositionVelocity, rotationSmoothTime,
                    rotationMaxSpeed);

                bossDirection = Quaternion.Euler(0, angle, 0) * camForward;
                    
                const float thirdPersonHeightBuffer = 1f;
                transform.position = playerPosition - bossDirection * thirdPersonDistance +
                                     Vector3.up * (thirdPersonCamHeight - (bossPosition.y - playerPosition.y) + thirdPersonHeightBuffer);

                Vector3 targetLookAtPoint = playerPosition + thirdPersonFocusHeight * Vector3.up;

                CheckMinumumCamHeight();
                transform.LookAt(targetLookAtPoint);
                break;
            }
            case CamState.Transition:
            {
                time += Time.deltaTime;
                endPoint = playerHeightRegulatedPosition + offset;
                transform.position = Vector3.Lerp(startPoint, endPoint, time);
                transform.rotation = Quaternion.Slerp(startRotation, endRotation, time);
                const float transitionOrthoStart = 1f;
                const float transitionOrthoEnd = 5f;
                mainCamera.orthographicSize = Mathf.Lerp(transitionOrthoStart, transitionOrthoEnd, time);

                if (time > 1f)
                {
                    camState = CamState.TopDown;

                    // i dont think this is necessary.
                    transform.position = endPoint;
                    transform.rotation = endRotation;
                }

                break;
            }
            case CamState.FixedOthograpicMode:
            {
                time += Time.deltaTime;
                transform.position = Vector3.Lerp(startPoint, endPoint, stopSmootlyCurve.Evaluate(time));
                transform.rotation = Quaternion.Slerp(startRotation, endRotation, time);
                mainCamera.orthographicSize = Mathf.Lerp(startSize, endSize, time);

                break;
            }
            case CamState.FixedTrackingMode:
            {
                transform.LookAt(playerHeightRegulatedPosition);
                CheckMinumumCamHeight();
                break;
            }
        }



        if (shakeCamera)
        {
            if (currentShakeDuration > 0)
            {
                transform.localPosition += UnityEngine.Random.insideUnitSphere * shakeMagnitude;
                currentShakeDuration -= Time.deltaTime;
            }
            else
            {
                shakeCamera = false;
            }
        }

        followingCharacterUI.UpdateUIPos();
    }

    private void GameStart()
    {
        LookDownPlayer();
    }

    private void SetPlayerTransform(Transform newTransform)
    {
        playerTransform = newTransform;
    }

    private void ThirdPersonMode()
    {
        if (!bossTransformChannel.TryGetVariable(out bossTransform))
        {
            Debug.LogError("Boss transform not found!");
            return;
        }

        camState = CamState.ThirdPerson;
        time = 0f;
    }

    public void FixedOthograpicMode()
    {
        camState = CamState.FixedOthograpicMode;
        time = 0f;
        const float orthoStartSize = 1f;
        const float orthoEndSize = 8f;
        mainCamera.orthographic = true;

        startRotation = transform.rotation;
        startSize = orthoStartSize;
        endSize = orthoEndSize;

        endPoint = new Vector3(-10, 10, -10);
        endRotation = Quaternion.Euler(FIXED_CAM_MODE_ROTATION);
        
        startPoint = transform.position - endRotation * Vector3.forward * fixedOrthogonalSafeDistance;
    }

    public void FixedTrackingMode()
    {
        camState = CamState.FixedTrackingMode;
    }
    
    public void OffThirdPersonMode()
    {
        if (camState == CamState.ThirdPerson)
        {
            LookDownPlayer();
            mainCamera.fieldOfView = initialFOV;
        }
    }

    public void ZoomOutAndLookDownPlayer()
    {
        if (camState == CamState.ThirdPerson)
        {
            return;
        }


        currentLookDistance += deltaLookDistance;
        currentLookDistance = Mathf.Min(currentLookDistance, maxLookDistance);
        LookDownPlayer();
    }

    private void LookDownPlayer()
    {
        Vector3 playerPosition = playerTransform.position;
        playerPosition.y = 0f;
        
        for(int i = 0; i < 4; i++)
        {
            camPosition[i] = playerPosition + camDirection[i] * currentLookDistance;
        }
        
        // find closet point
        float minDistance = float.MaxValue;
        
        for(int i = 0; i < 4; i++)
        {
            float distance = Vector3.Distance(camPosition[i], transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                directionIndex = i;
                worldDirectionChannel.WorldDirection = (WorldDirection) i;
            }
        }
        
        time = 0f;
        camState = CamState.Transition;
        offset = camDirection[directionIndex] * currentLookDistance;
        startPoint = transform.position;
        startRotation = transform.rotation;
        const float camBaseRotationY = 45f;
        const float camRotationStepY = 90f;
        endRotation = Quaternion.Euler(lookDownAngle, camBaseRotationY + camRotationStepY * directionIndex, 0);
    }

    private void GoBackToIdleMode()
    {
        camState = CamState.MovePoint;
        mainCamera.orthographic = false;
        mainCamera.fieldOfView = initialFOV;
        time = 0f;
        startPoint = transform.position;
        startRotation = transform.rotation;
        endPoint = initialPosition;
        endRotation = Quaternion.Euler(initialRotation);
    }

    public void TargetMove(Vector3 movement)
    {
        smoothMotion = true;
        smoothTimeCount = 0f;
        startSmoothPosition = playerTransform.position - movement;
    }

    public void OnPlayerMoved(Vector3 movement)
    {
        transform.position += movement;
    }

    public void MoveToFixedPoint(Vector3 position, float size)
    {
        if (camState != CamState.FixedOthograpicMode)
        {
            FixedOthograpicMode();
            return;
        }

        startPoint = transform.position;
        startRotation = transform.rotation;
        startSize = mainCamera.orthographicSize;

        endPoint = position;
        endSize = size;
        time = 0f;
    }

    public void SetCamBackgroundColor(Color color)
    {
        mainCamera.backgroundColor = color;
    }

    public void SetCamBackgroundColor(Color color, float changeTime)
    {
        startColor = mainCamera.backgroundColor;
        endColor = color;
        colorChangeTime = changeTime;
        colorTimeCount = 0f;
    }

    public static Plane DefineFixedOrthogonalPlane(out Vector3 planeOrigin, float distanceToOrigin)
    {
        float angle_a = CameraController.FIXED_CAM_MODE_ROTATION.x;
        float y = distanceToOrigin * Mathf.Sin(angle_a * Mathf.Deg2Rad);
        float xz = - distanceToOrigin * Mathf.Cos(angle_a * Mathf.Deg2Rad) / Mathf.Sqrt(2);
        Vector3 P = new Vector3(xz, y, xz);
        planeOrigin = P;
        Vector3 N = (Vector3.zero - P).normalized;
        return new Plane(N, P);
    }

    public static (float, float) GetOrthogonalCamWidthHeight(float camSize)
    {
        float aspectRatio = (float)Screen.width / Screen.height;

        float H = 2 * camSize; // Vertical length
        float W = aspectRatio * H; // Horizontal length

        return (W, H);
    }

    /// <summary>
    /// 투영된 물체의 너비에 맞춰 카메라의 사이즈를 정하고 싶은 경우 사용. 가정 : 이 게임에서 세로 모드는 없음
    /// </summary>
    /// <param name="projectedObjectWidth"></param>
    /// <param name="camSize"></param>
    /// <returns></returns>
    public static (float, float) GetOrthogonalCamWidthHeight(float projectedObjectWidth, out float camSize)
    {
        float aspectRatio = (float)Screen.width / Screen.height;
        float W = projectedObjectWidth + 1f;
        float H = W / aspectRatio;
        camSize = H / 2;
        return (W, H);
    }

    public static (float, float)GetOrthogonalCamWidthHeight(float projectedObjectWidth, float projectedObjectHeight, out float camSize)
    {
        float aspectRatio = (float)Screen.width / Screen.height;
        float H = projectedObjectHeight + 1f;
        float W = aspectRatio * H;

        if (W < projectedObjectWidth)
        {
            W = projectedObjectWidth + 1f;
            H = W / aspectRatio;
        }

        camSize = H / 2;

        return (W, H);
    }

    private void OnCharSelectionScreenShown()
    {
        camState = CamState.MovePoint;
        startPoint = transform.position;
        startRotation = transform.rotation;
        endPoint = charSelectionPosition;
        endRotation = Quaternion.Euler(charSelectionRotation);
        time = 0f;
    }

    private void OnStageSelectionScreenShown()
    {
        camState = CamState.MovePoint;
        startPoint = transform.position;
        startRotation = transform.rotation;
        endPoint = stageSelectionPosition;
        endRotation = Quaternion.Euler(stageSelectionRotation);
        time = 0f;
    }

    public void MovePointOnIdle(Vector3 position, Quaternion rotation)
    {
        GoBackToIdleMode();
        camState = CamState.MovePoint;
        startPoint = transform.position;
        startRotation = transform.rotation;
        endPoint = position;
        endRotation = rotation;
        time = 0f;
    }

    private const float characterCameraWidth = 0.3f;
    private const float mainCameraWidth = 0.7f;

    public void SplitScreen()
    {
        characterCamera.rect = new Rect(0, 0, characterCameraWidth, 1);
        characterCamera.gameObject.SetActive(true);
        mainCamera.rect = new Rect(characterCameraWidth, 0, mainCameraWidth, 1);
    }

    public void MergeScreen()
    {
        characterCamera.gameObject.SetActive(false);
        mainCamera.rect = new Rect(0, 0, 1, 1);
    }

    private void CheckMinumumCamHeight()
    {
        Vector3 camPos = transform.position;

        if (camPos.y < minCamYHeight)
        {
            transform.position = new Vector3(camPos.x, minCamYHeight, camPos.z);
        }
    }

    public bool IsThirdPersonMode()
    {
        return camState == CamState.ThirdPerson;
    }

    public void ShakeCamera(float shakeDuration = 0.5f, float shakeMagnitude = 0.2f)
    {
        this.shakeDuration = shakeDuration;
        this.shakeMagnitude = shakeMagnitude;
        shakeCamera = true;
        currentShakeDuration = shakeDuration;
    }

    public void SetBackgroundColor(Color color)
    {
        mainCamera.backgroundColor = color;
    }
}

