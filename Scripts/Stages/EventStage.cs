
using StarterAssets;
using UnityEngine;
using System.Collections.Generic;
using FMODUnity;
using System.Runtime.CompilerServices;

public class EventStage : BaseStage<EventStage>
{
    public Vector3 initialCharacterPosition;
    public EventReference bgm;

    [SerializeField]
    private AmbientData ambientData;

    [SerializeField]
    private bool oneCamera = false;

    [SerializeField]
    private EventStageChannelSO channel;

    [SerializeField]
    private bool oneHorizontalCamera = true;

    [SerializeField]
    private float camSize = 10f;

    [SerializeField]
    private EventChannelSO enterEventStageEventChannel, exitEventStageEC;

    [SerializeField]
    private MainCameraChannelSO mainCameraChannel;

    [SerializeField]
    private TransformChannelSO playerTransformChannel;

    [SerializeField]
    private OnLiquidBehaviourChannel onLiquidBehaviourChannel;

    [SerializeField]
    private List<Vector3> cameraPositions;

    [SerializeField]
    private Bounds stageBounds;

    [SerializeField]
    private Vector3 upDirection, rightDirection;


    private Plane S;
    private Transform playerTransform;
    private CameraController mainCameraController;
    private Vector3 mainCameraForward;

    
    private int numberOfFixedCamPoints;
    private float overlapFactor = 0.7f;
    private int camIndex = -1;
    private bool playerInZoomInZone = false;
    private bool playerInFocusZone = false;
    private ZoomInTrigger[] zoomInTriggers;
    private FocusTrigger[] focusTriggers;
    private Dictionary<ZoomInTrigger, Vector3> zoomInCameraPositions = new Dictionary<ZoomInTrigger, Vector3>();
    private Vector3 prevCamForward;
    private OnLiquidBehaviour onLiquidBehaviour;

    public AmbientData AmbientData => ambientData;
    private void Awake()
    {
        S = new Plane(Vector3.Cross(upDirection, rightDirection), stageBounds.center);
        cameraPositions = CalculateCameraPositions();
        zoomInTriggers = GetComponentsInChildren<ZoomInTrigger>();
        zoomInCameraPositions = CalculateZoomInCameraPositions();

        for (int i = 0; i < zoomInTriggers.Length; i++)
        {
            zoomInTriggers[i].PlayerEnter += OnPlayerEnterZoomInZone;
            zoomInTriggers[i].PlayerExit += OnPlayerExitZoomInZone;
        }

        focusTriggers = GetComponentsInChildren<FocusTrigger>();

        for (int i = 0; i < focusTriggers.Length; i++)
        {
            focusTriggers[i].PlayerEnter += OnPlayerEnterFocusZone;
            focusTriggers[i].PlayerExit += OnPlayerExitFocusZone;
        }

        onLiquidBehaviour = GetComponent<OnLiquidBehaviour>();
    }
    private void OnDestroy()
    {
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        playerTransformChannel.Subscribe(SetPlayerTransform);
        camIndex = -1;

        if (!mainCameraChannel.TryGetVariable(out mainCameraController))
        {
            Debug.LogWarning("Main Camera is not set in the EventStage");
            return;
        }


        channel.Register(this);
        exitEventStageEC.Subscribe(OnExitEventStage);

        if (onLiquidBehaviourChannel == null)
        {
            return;
        }

        if (onLiquidBehaviour != null)
        {
            onLiquidBehaviourChannel.Register(onLiquidBehaviour);
        }
        else
        {
            onLiquidBehaviourChannel.Register(null);
        }

    }

    private void OnDisable()
    {
        playerTransformChannel.Unsubscribe(SetPlayerTransform);
        channel.Unregister(this);
        exitEventStageEC.Unsubscribe(OnExitEventStage);
    }

    private void Update()
    { 
        if (cameraPositions.Count == 0)
        {
            Debug.LogError("Camera positions are not set in the EventStage");
            return;
        }

        if (playerInZoomInZone || playerInFocusZone)
        {
            return;
        }

        int closestCamIndex = GetClosestCamIndex();
        camIndex = closestCamIndex;
        mainCameraController.MoveToFixedPoint(cameraPositions[camIndex], camSize);
    }

    private int GetClosestCamIndex()
    {
        mainCameraForward = CameraController.FIXED_CAM_MODE_FORWARD;
        int closestCamIndex = 0;
        float minAngle = 360f;

        for (int i = 0; i < cameraPositions.Count; i++)
        {
            Vector3 toPlayer = playerTransform.position - cameraPositions[i];
            float angle = Vector3.Angle(mainCameraForward, toPlayer);

            if (angle < minAngle)
            {
                minAngle = angle;
                closestCamIndex = i;                              
            }
        }

        return closestCamIndex;
    }

    public void OnPlayerFall()
    {
        Release();
    }

    public void BeCurrentStage()
    {
        gameObject.SetActive(true);
        enterEventStageEventChannel.Raise();
        
        if (!bgm.IsNull)
        {
            FMODAudioManager.instance.SetMusicAndPlay(bgm);
        }
    }

    public void MovePlayerToInitialPos(CustomThirdPersonController playerController)
    {
        playerController.MoveOnlyCharacterTo(initialCharacterPosition);
        playerController.IgnoreInputUntillHitGround();
    }

    private void SetPlayerTransform(Transform curretPlayerTransfrom)
    {
        playerTransform = curretPlayerTransfrom;
    }

    public void SetBounds(Bounds stageBounds, Vector3 upDirection, Vector3 rightDirection)
    {
        this.stageBounds = stageBounds;
        this.upDirection = upDirection;
        this.rightDirection = rightDirection;
    }

    private List<Vector3> CalculateCameraPositions()
    {
        float W, H;
        int camerasHorizontal, camerasVertical;

        if (oneCamera)
        {
            (float width, float height) = CameraController.GetOrthogonalCamWidthHeight(stageBounds.size.x, stageBounds.size.y, out camSize);
            W = width;
            H = height;
            camerasHorizontal = 1;
            camerasVertical = 1;
        }
        else if (oneHorizontalCamera)
        {
            (float witdh, float height) = CameraController.GetOrthogonalCamWidthHeight(stageBounds.size.x, out camSize);
            W = witdh;
            H = height;

            camerasHorizontal = 1;
            camerasVertical = Mathf.CeilToInt((stageBounds.size.y / H) / overlapFactor); // Adjusted to use Z axis

        }
        else
        {
            (float witdh, float height) = CameraController.GetOrthogonalCamWidthHeight(camSize);
            W = witdh;
            H = height;

            camerasHorizontal = Mathf.CeilToInt(stageBounds.size.x / W);
            camerasVertical = Mathf.CeilToInt((stageBounds.size.y / H) / overlapFactor); // Adjusted to use Z axis

        }

        // Calculate the number of cameras/screens needed


        List<Vector3> positions = new List<Vector3>();

        float unitX = stageBounds.size.x / camerasHorizontal;
        float unitY = stageBounds.size.y / camerasVertical;

        for (int y = 0; y < camerasVertical; y++)
        {
            float boundY = -stageBounds.extents.y + unitY * y + unitY / 2;
            
            for (int x = 0; x < camerasHorizontal; x++)
            {
                float boundX = -stageBounds.extents.x + unitX * x + unitX / 2;
                // Calculate the 2D position on the plane
                Vector3 localPosition = boundX * rightDirection + boundY * upDirection;

                // Convert to 3D world coordinates
                Vector3 worldPosition = stageBounds.center + localPosition;

                positions.Add(worldPosition);

                Debug.DrawRay(worldPosition, -mainCameraForward, Color.blue, 10f);
            }
        }

        return positions;
    }

    private Dictionary<ZoomInTrigger, Vector3> CalculateZoomInCameraPositions()
    {
        Dictionary<ZoomInTrigger, Vector3> positions = new Dictionary<ZoomInTrigger, Vector3>();

        foreach (ZoomInTrigger trigger in zoomInTriggers)
        {
            Vector3 centerPos = trigger.center.position;
            Vector3 camPos = S.ClosestPointOnPlane(centerPos);

            positions.Add(trigger, camPos);
        }

        return positions;
    }

    public void OnPlayerEnterZoomInZone(ZoneTrigger zoneTrigger)
    {
        ZoomInTrigger trigger = (ZoomInTrigger)zoneTrigger;
        playerInZoomInZone = true;
        mainCameraController.MoveToFixedPoint(zoomInCameraPositions[trigger], trigger.camSize);
    }

    public void OnPlayerExitZoomInZone(ZoneTrigger zoneTrigger)
    {
        ZoomInTrigger trigger = (ZoomInTrigger)zoneTrigger;
        playerInZoomInZone = false;
    }

    public void OnPlayerEnterFocusZone(ZoneTrigger zoneTrigger)
    {
        FocusTrigger trigger = (FocusTrigger)zoneTrigger;
        mainCameraController.MovePointOnIdle(trigger.cameraPos, Quaternion.LookRotation(trigger.focusPos - trigger.cameraPos));
        playerInFocusZone = true;
    }

    public void OnPlayerExitFocusZone(ZoneTrigger zoneTrigger)
    {
        playerInFocusZone = false;
    }

    public void ExitFocusZone()
    {
        for (int i = 0; i < focusTriggers.Length; i++)
        {
            if (focusTriggers[i].isPlayerInZone)
            {
                focusTriggers[i].ExitZoneManually();
            }
        }
    }

    private void OnExitEventStage()
    {
        if (bgm.IsNull)
        {
            return;
        }
        FMODAudioManager instance = FMODAudioManager.instance;

        instance.StopMusic();
        instance.PlayMusicInPlayList();
    }
}