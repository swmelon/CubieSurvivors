using System.Collections.Generic;
using Local.Scripts.Extensions;
using StarterAssets;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;


[RequireComponent(typeof(StageMover))]
public class StageManager : MonoBehaviour
{
    [Header("Listener")]
    [SerializeField]
    private DifficultyCurveEC curveUpdateEventChannel;

    [SerializeField] 
    private List<EventChannelSO> toNextStageEventChannel;

    [SerializeField]
    private EventChannelSO loadUndiscoveredEventStageEC;

    [SerializeField]
    private IntEventChannelSO loadDiscoveredEventStageEC;

    [SerializeField]
    private EventChannelSO scenarioChangedEC;

    [SerializeField]
    private EventChannelSO loadStageWithoutUpdatingCurveEC;
    
    [Header("Invoker")] 
    [SerializeField]
    private EventChannelSO enterBossStageEventChannel;

    [SerializeField]
    private Vector3EventChannelSO eventStagePositionChannel;
    
    [SerializeField]
    private EventChannelSO playerFallEventChannel;

    [SerializeField] 
    private EventChannelSO restartGameEventChannel;

    [SerializeField]
    private EventChannelSO defaultStageTransitionEC;

    [Header("Variable")]
    [SerializeField]
    private PlayerChannelSO currentPlayerChannel;
        
    [SerializeField]
    private WorldDirectionChannelSO worldDirectionChannel;
        
    [Header("Fall Event Management")]
    
    [Range(30f, 100f)]
    
    [SerializeField] 
    private float heightLimit = 30f;

    [SerializeField] 
    private Camera cubbyCam;
    
    [SerializeField]
    private float minPlayerYLoc = -5f;

    [Header("ETC")]

    [SerializeField]
    private Vector3 eventStagePosition;

    [SerializeField]
    private GameAmbientManager gameAmbientManager;

    [SerializeField]
    private BooleanEventChannelSO ingameUIControlChannel;

    private enum StageTransition
    {
        Default,
        EnterBossStage,
        ExitBossStage,
        EnterEventStage,
        ExitEventStage
    }

    // Stage Pooling
    private Queue<IPoolable> stages;
    private StageMover stageMover;
    private StageBuilder stageBuilder;

    private Stage prevStage, currentStage, nextStage;
    private EventStage eventStage;
    private bool isPlayerFallingDown;
    private bool noPlayer;
    private bool onFirstStageLoaded = true;
    private bool playerFallen = false;
    private Vector3 playerPositionBeforeEnterEventStage;
    private StageTransition stageTransition = StageTransition.Default;

    private GravityDelegator playerGravity;
    private CustomThirdPersonController playerController;
    private float defaultFloorHeight = -0.5f;
    private float defaultPlayerHeight = 0.5f;
    private bool sizeChaged = false;
    StageAdapter stageAdaptor;


    private void Awake()
    {
        stageMover = GetComponent<StageMover>();
        stageBuilder = GetComponent<StageBuilder>();
        stages = new Queue<IPoolable>();

        curveUpdateEventChannel.Subscribe(LoadStage, last: true);
        loadStageWithoutUpdatingCurveEC.Subscribe(LoadStageWithoutUpdatingCurve);

        for (int i = 0; i < toNextStageEventChannel.Count; i++)
        {
            toNextStageEventChannel[i].Subscribe(BeforeMoveOnToNextStage);
        }
        stageMover.OnFinishMove.AddListener(OnFinishStageMove);
        currentPlayerChannel.Subscribe(SetCurrentPlayer);
        loadUndiscoveredEventStageEC.SubscribeLast(LoadUndiscoveredEventStage);
        loadDiscoveredEventStageEC.Subscribe(LoadDiscovceredStage);
        scenarioChangedEC.Subscribe(OnScenarioChanged);
    }

    private void OnDestroy()
    {
        curveUpdateEventChannel.Unsubscribe(LoadStage);
        loadStageWithoutUpdatingCurveEC.Unsubscribe(LoadStageWithoutUpdatingCurve);
        
        for (int i = 0; i < toNextStageEventChannel.Count; i++)
        {
            toNextStageEventChannel[i].Unsubscribe(BeforeMoveOnToNextStage);
        }

        stageMover.OnFinishMove.RemoveListener(OnFinishStageMove);
        currentPlayerChannel.Unsubscribe(SetCurrentPlayer);
        loadUndiscoveredEventStageEC.Unsubscribe(LoadUndiscoveredEventStage);
        loadDiscoveredEventStageEC.Unsubscribe(LoadDiscovceredStage);
        scenarioChangedEC.Unsubscribe(OnScenarioChanged);
    }

    /// <summary>
    /// When the game Start, move up lobby stage.
    /// </summary>
    private void Start()
    {
        LoadFirstStage(10f);
        MoveUpStages(playerGravity);
    }

    private void SetCurrentPlayer(Player player)
    {
        if (ReferenceEquals(player, null))
        {
            noPlayer = true;
        }
        else
        {
            noPlayer = false;
            playerGravity = player.Gravity;
            playerController = playerGravity as CustomThirdPersonController   ;
        }
    }

    private void FixedUpdate()
    {
        if (!noPlayer && playerGravity.transform.position.y < minPlayerYLoc && !playerFallen)
        {
            if (stageTransition == StageTransition.ExitEventStage)
            {
                eventStage.MovePlayerToInitialPos(playerController);
                return;
            }

            if (!onFirstStageLoaded)
            {
                // dead
                playerFallEventChannel.Raise();
                playerFallen = true;
                return;
            }
            
            onFirstStageLoaded = false;
            playerGravity.transform.position = new Vector3(playerGravity.transform.position.x, minPlayerYLoc,
                playerGravity.transform.position.z);
            
            isPlayerFallingDown = true;
            currentStage.OnPlayerFall();
            stageMover.Delegate(playerGravity, currentStage.LowerHeight * (1f + RandomExtenstion.GetRandomProbability()));
        }
    }

    private void LoadFirstStage()
    {

        Manage(stageBuilder.BuildFirstStage());
    }

    private void LoadFirstStage(float height)
    {
        ResetStageQueue();
        Stage firstStage = stageBuilder.BuildFirstStage();
        Manage(firstStage, height);
    }

    private void LoadUndiscoveredEventStage()
    {
        eventStage = stageBuilder.BuildUndiscoveredEventStage();
        stageTransition = StageTransition.EnterEventStage;
        gameAmbientManager.SetAmbientData(eventStage.AmbientData);

    }

    private void LoadDiscovceredStage(int index = 0)
    {
        eventStage = stageBuilder.BuildDiscoveredEventStage(index);
        stageTransition = StageTransition.EnterEventStage;
        gameAmbientManager.SetAmbientData(eventStage.AmbientData);
    }

    public void LoadStatsUpgradeStage()
    {
        eventStage = stageBuilder.BuildStatsUpgradeStage();
        stageTransition = StageTransition.EnterEventStage;
    }

    public void LoadItemsUpgradeStage()
    {
        eventStage = stageBuilder.BuildItemsUpgradeStage();
        stageTransition = StageTransition.EnterEventStage;
    }


    private void LoadStage(DifficultyCurveManagerSO curveManager)
    {
        Stage stage = stageBuilder.BuildStage(curveManager);

        stageTransition =  StageTransition.Default;

        if (stage.StageType == StageType.BossStage)
        {
            stageTransition = StageTransition.EnterBossStage;
        }

        if (currentStage.StageType == StageType.BossStage)
        {
            stageTransition = StageTransition.ExitBossStage;
        }
        
        Manage(stage);
    }

    private void LoadStageWithoutUpdatingCurve()
    {
        Stage stage = stageBuilder.BuildStageWithESD();

        stageTransition = StageTransition.Default;
        
        Manage(stage);
        BeforeMoveOnToNextStage();
    }
    
    private void BeforeMoveOnToNextStage()
    {
        BoomCurrentStage();

        switch (stageTransition)         
        {
            case StageTransition.Default:
                {
                    if (sizeChaged && !ReferenceEquals(stageAdaptor, null))
                    {
                        stageAdaptor.DisablePillarBlockingView();
                    }

                    defaultStageTransitionEC.Raise();

                    break;
                }
            case StageTransition.EnterBossStage:
                {
                    enterBossStageEventChannel.Raise();
                    stageTransition = StageTransition.ExitBossStage;
                    break;
                }
            case StageTransition.ExitBossStage:
                {
                    if (sizeChaged && !ReferenceEquals(stageAdaptor, null))
                    {
                        stageAdaptor.transform.rotation = worldDirectionChannel.WorldRotation;
                        stageAdaptor.DisablePillarBlockingView();
                    }
                    else
                    {
                        prevStage.transform.rotation = worldDirectionChannel.WorldRotation;
                        prevStage.DisablePillarBlockingView();
                    }

                    defaultStageTransitionEC.Raise();
                    break;
                }
            case StageTransition.EnterEventStage:
                {
                    ResetStageQueue();
                    stageBuilder.SetFloorActive(false);
                    eventStage.BeCurrentStage();
                    eventStage.MovePlayerToInitialPos(playerController);
                    stageTransition = StageTransition.ExitEventStage;
                    ingameUIControlChannel.Raise(false);
                    return;
                }
            case StageTransition.ExitEventStage:
                {
                    eventStage.Release();
                    stageBuilder.SetFloorActive(true);
                    playerController.MoveOnlyCharacterTo(new Vector3(0f, 0.5f, 0f));
                    LoadFirstStage(10f);

                    ingameUIControlChannel.Raise(true);
                    stageTransition = StageTransition.Default;
                    break;
                }
        }

        MoveUpStages(playerGravity);
    }

    private void OnFinishStageMove()
    {
     

        if (isPlayerFallingDown)
        {
            currentStage = null;
            isPlayerFallingDown = false;
            ResetStageQueue();
            RestartGame();
            return;
        }

        if (!(stageTransition == StageTransition.EnterEventStage))
        {
            currentStage.OnFinishStageMove();
        }

    }

    private void RestartGame()
    {
        // Restart Logic
        restartGameEventChannel.Raise();
        LoadFirstStage(40f);
        MoveUpStagesWhenPlayerFallingDown(playerGravity);
    }


    private void Manage(Stage stage)
    {
        int heightAdapter = 0;
        stageAdaptor = null;

        if (!ReferenceEquals(currentStage, null) && currentStage.Size != stage.Size)
        {
            sizeChaged = true;
            heightAdapter = 20;
            stageAdaptor = PlugAdapter(heightAdapter, stage.Height);
        }
        else
        {
            sizeChaged = false;
        }

        Vector3 stageAnchorPosition = -(stage.Height + heightAdapter) * Vector3.up;
        Quaternion stageRoation = worldDirectionChannel.WorldRotation;

        stage.transform.SetPositionAndRotation(stageAnchorPosition, stageRoation);
        nextStage = stage;
        nextStage.transform.SetParent(transform, true);
        nextStage.gameObject.SetActive(false);

        if (!(nextStage.StageType == StageType.BossStage))
        {
            stageAdaptor?.DisablePillarBlockingView();
        }

        stages.Enqueue(nextStage);
    }
    
    private void Manage(Stage stage, float height)
    {
        stage.transform.position = - height * Vector3.up;
        nextStage = stage;
        nextStage.transform.SetParent(transform, true);
        nextStage.gameObject.SetActive(false);
        stages.Enqueue(nextStage);
    }

    private void Manage(EventStage stage, float height)
    {
        stage.transform.position = -height * Vector3.up;
        eventStage = stage;
        eventStage.transform.SetParent(transform, true);
        eventStage.gameObject.SetActive(false);
        stageTransition = StageTransition.EnterEventStage;
        stages.Enqueue(eventStage);
    }

    // Must be called when user select a card.
    private void MoveUpStages(GravityDelegator delegator)
    {
        ResetPosition();
        prevStage = currentStage;
        currentStage = nextStage;
        
        currentStage.BeCurrentStage();

        if (currentStage.StageType == StageType.EventStage)
        {
            //this should not happen
            Debug.LogError("No current stage type is EventStage on MoveUpStages()");
        }

        Vector3 floorAnchorPos = currentStage.transform.position + defaultFloorHeight * Vector3.up;

        stageBuilder.AnchorFloor(floorAnchorPos);

        // ���� y ���̰� �׻� �������� �����Ƿ� ���� ����ñ��� ������ ���̷� �ٲٱ� ����
        float playerHeightAdjustment = defaultPlayerHeight - playerGravity.transform.position.y;
        bool moveFog = currentStage.StageType != StageType.FirstStage;

        stageMover.Delegate(delegator, - currentStage.transform.position.y, playerHeightAdjustment, moveFog);

        ReleaseStageOverHeightLimit();
    }

    private void MoveUpStagesWhenPlayerFallingDown(GravityDelegator delegator)
    {
        ResetPosition();

        float moveDistance = -nextStage.transform.position.y ;
        
        // assume that the player (actually StageMover) reached terminal velocity.
        stageMover.Delegate(delegator, moveDistance, -minPlayerYLoc + defaultPlayerHeight, false);
        
        // Don't need it because currentStage were already Release(), but just in case.
        if (!ReferenceEquals(currentStage, null) && !currentStage.Released)
        {
            currentStage.OnPlayerFall();
        }
        
        currentStage = nextStage;
        currentStage.BeCurrentStage();

        Vector3 floorAnchorPos = currentStage.transform.position + defaultFloorHeight * Vector3.up;

        stageBuilder.AnchorFloor(floorAnchorPos);
        ReleaseStageOverHeightLimit();
    }
    
    private void ActivateBoomEffectCoveringScreen()
    {
    }

    private void BoomCurrentStage()
    {
        if (currentStage.Released)
        {
            return;
        }
        
        currentStage.SetPillarHeight(0f, nextStage.Height);
        currentStage.transform.rotation = worldDirectionChannel.WorldRotation;
        currentStage.DisableEdgeCollider();

        if (!(nextStage.StageType == StageType.BossStage))
        {
            currentStage.DisablePillarBlockingView();
        }

    }

    private void DisableBoomEffect()
    {
        cubbyCam.gameObject.SetActive(false);
    }
    
    /// <summary>
    /// Makes localPosition equal to worldPosition.
    /// </summary>
    private void ResetPosition()
    {
        foreach (IPoolable stage in stages)
        {
            stage.transform.parent = null;
        }

        transform.position = Vector3.zero;
        
        foreach (IPoolable stage in stages)
        {
            stage.transform.SetParent(transform, false);
        }
    }

    private void ReleaseStageOverHeightLimit()
    {
        while(stages.Peek().transform.position.y > heightLimit)
        {
            stages.Dequeue().Release();
        }
    }
    
    private void ResetStageQueue()
    { 
        while (stages.Count > 0)
        {
            stages.Dequeue().Release();
        }
    }

    private StageAdapter PlugAdapter(float heightAdapter, float heightNewStage)
    {
        StageAdapter adapter = stageBuilder.BuildStageAdapter(currentStage.Size, heightAdapter, heightNewStage);
        adapter.transform.SetParent(transform, true);

        Vector3 adaptorPos = - heightNewStage * Vector3.up;
        Quaternion adaptorRot = worldDirectionChannel.WorldRotation;

        adapter.transform.SetPositionAndRotation(adaptorPos, adaptorRot);
        stages.Enqueue(adapter);
        return adapter;
    }

    private void OnScenarioChanged()
    {
        ResetStageQueue();
    }
}
