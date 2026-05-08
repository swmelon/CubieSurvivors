using Local.Scripts.Extensions;
using System.Collections.Generic;
using UnityEngine;

public class StageBuilder : MonoBehaviour
{
    [SerializeField]
    private StagePool stagePool;

    [SerializeField]
    private List<EventStageData> randomEventStageData;

    [SerializeField]
    private SaveLoadManagerSO saveLoadManager;

    [SerializeField]
    private EventStageManager eventStageManager;

    [SerializeField]
    private StageAdapterPool stageAdapterPool;

    [SerializeField]
    private EventStageData statsUpgradeStageData, itemsUpgradeStageData;

    [SerializeField]
    private GameScenarioManagerSO gameScenarioManager;

    [SerializeField]
    private ThemeDataChannelSO themeDataChannel;

    [SerializeField]
    private EventChannelSO unloadScenarioEC;

    private ThemeData themeData;
    private FloorLEDBuilder floorLEDBuilder;
    private FloorLEDButtonBuilder floorLEDButtonBuilder;
    private FloorLiquidBuilder floorLiquidBuilder;
    private FloorTrapBuilder floorTrapBuilder;
    private FloorItemBoxBuilder floorItemBoxBuilder;
    private ESDBuilder ESDBuilder;
    private BossBuilder bossBuilder;
    private FloorPropBuilder floorPropLocator;
    private StageBlueprint blueprint;
    private List<FloorGridDataSO> localHeightMaps = new List<FloorGridDataSO>();
    private DifficultyCurveManagerSO curveManager;


    private void Awake()
    {
        floorLEDBuilder = GetComponentInChildren<FloorLEDBuilder>();
        floorLEDButtonBuilder = GetComponentInChildren<FloorLEDButtonBuilder>();
        floorLiquidBuilder = GetComponentInChildren<FloorLiquidBuilder>();
        floorTrapBuilder = GetComponentInChildren<FloorTrapBuilder>();
        floorItemBoxBuilder = GetComponentInChildren<FloorItemBoxBuilder>();
        floorPropLocator = GetComponentInChildren<FloorPropBuilder>();
        ESDBuilder = GetComponentInChildren<ESDBuilder>();
        bossBuilder = GetComponentInChildren<BossBuilder>();
    }

    private void OnEnable()
    {
        themeDataChannel.Subscribe(OnThemeChanged);
        unloadScenarioEC.Subscribe(OnScenrarioUnloaded);
        gameScenarioManager.OnScenarioLoaded += OnScenarioLoaded;
    }

    private void OnDisable()
    {
        themeDataChannel.Unsubscribe(OnThemeChanged);
        unloadScenarioEC.Unsubscribe(OnScenrarioUnloaded);
        gameScenarioManager.OnScenarioLoaded -= OnScenarioLoaded;
    }

    public Stage BuildFirstStage()
    {
        Stage stage = stagePool.Get(themeData.MainStageData);
        
        blueprint = new StageBlueprint();
        blueprint.Size = themeData.MainStageData.Prefab.Size;
        blueprint.StageType = StageType.FirstStage;
        
        stage.Size = blueprint.Size;
        stage.StageType = blueprint.StageType;
        floorLEDBuilder.BuildFlatFloor(blueprint.Size);
        floorLEDButtonBuilder.AddButtons();
        
        return stage;
    }

    public EventStage BuildUndiscoveredEventStage()
    {
        // manage

        return eventStageManager.GetRecentlyUnlocked();
    }

    public EventStage BuildDiscoveredEventStage(int index)
    {
        // manage

        return eventStageManager.Get(index);
    }

    public EventStage BuildStatsUpgradeStage()
    {
        return eventStageManager.Get(statsUpgradeStageData);
    }

    public EventStage BuildItemsUpgradeStage()
    {
        return eventStageManager.Get(itemsUpgradeStageData);
    }

    public Stage BuildStage(DifficultyCurveManagerSO curveManager)
    {
        int stageSize = themeData.MainStageData.Prefab.Size + curveManager.GetLevel() / 5;
        GameScenario currentScenario = gameScenarioManager.GetCurrentScenario();
        bool buildBossStage  = curveManager.GetLevel() % currentScenario.bossInterval == 0;
        bool finalBossStage = curveManager.GetLevel() == currentScenario.bossInterval * currentScenario.bossAppearances;

        // Make a blueprint
        blueprint = new StageBlueprint();
        blueprint.StageType = buildBossStage ? StageType.BossStage : StageType.MainStage;
        blueprint.Size = stageSize;
        blueprint.Padding = themeData.randomHeightMapPadding;
        blueprint.Threshold = themeData.randomHeightMapThreshold;
        blueprint.LiquidType = themeData.liquidType;

        Stage stage = BuildStage();

        if (buildBossStage)
        {
            if (finalBossStage)
            {
                if (currentScenario.TryGetFinalBossWeapon(out Weapon weapon))
                {
                    bossBuilder.BuildFinalBoss(curveManager.GetPPR(), curveManager.GetBossPowerMultiplier(), weapon);
                }
                else
                {
                    bossBuilder.BuildFinalBoss(curveManager.GetPPR(), curveManager.GetBossPowerMultiplier());
                }
            }
            else
            {
                bossBuilder.BuildBoss(curveManager.GetPPR(), curveManager.GetBossPowerMultiplier());
            }
        }
        else
        {
            ESDBuilder.BuildEnemySpawnDevice(curveManager.GetPPR(), curveManager.GetMinEnemySpawnDevices(), curveManager.GetMaxEnemySpawnDevices());
        }

        this.curveManager = curveManager;
        return stage;
    }

    public Stage BuildStage()
    {
        Stage stage = stagePool.Get(themeData.MainStageData);
        // Build the stage
        stage.Size = blueprint.Size;
        stage.StageType = blueprint.StageType;

        // Build the floor

        if (blueprint.StageType == StageType.BossStage && themeData.bossLocalHeightMaps.Count != 0)
        {
            floorLEDBuilder.BuildFloor(themeData.bossLocalHeightMaps.PickRandom().ToAugmentedArray(), blueprint.Size);
        }
        else if (localHeightMaps == null || localHeightMaps.Count == 0 || !RandomExtenstion.IsHappen(themeData.localHeightMapProbability))
        {
            floorLEDBuilder.BuildFloor(blueprint.Size, blueprint.Threshold, blueprint.Padding, false);
        }
        else
        {
            FloorGridDataSO floorGridData = localHeightMaps.PickRandom();
            floorLEDBuilder.BuildFloor(floorGridData.ToAugmentedArray(), blueprint.Size);
        }

        floorItemBoxBuilder.BuildFloorItemBox();

        return stage;
    }

    public Stage BuildStageWithESD(float esdDensity = 0.25f)
    {
        esdDensity = Mathf.Clamp01(esdDensity);
        Stage stage = BuildStage();

        int adjustedPPR = (int)(esdDensity * curveManager.GetPPR());
        int adjustedMinimumESDs = Mathf.RoundToInt(esdDensity * curveManager.GetMinEnemySpawnDevices());
        int adjustedMaximumESDs = Mathf.RoundToInt(esdDensity * curveManager.GetMaxEnemySpawnDevices());

        ESDBuilder.BuildEnemySpawnDevice(adjustedPPR, adjustedMinimumESDs, adjustedMaximumESDs);
        return stage;
    }


    public StageAdapter BuildStageAdapter(int currentStageSize, float heightAdapter, float heightNewStage)
    {
        StageAdapter adapter = stageAdapterPool.Get(themeData.StageAdapterData);
        adapter.Adapt(currentStageSize, blueprint.Size, heightAdapter);
        return adapter;
     }


    public void AnchorFloor(Vector3 anchorPos)
    {
        floorTrapBuilder.ReleaseTraps();
        ESDBuilder.ReleaseDevices();
        floorLEDButtonBuilder.DestoryLEDButtons();
        bossBuilder.DestroyBoss();

        floorTrapBuilder.ActivateTraps();
        floorItemBoxBuilder.ActivateBoxes();
        floorLEDButtonBuilder.ActivateLEDButtons();
        ESDBuilder.ActivateDevices();

        floorLEDBuilder.TransformFloor(anchorPos);
        floorPropLocator.ClearProps();
        floorPropLocator.BuildProps(themeData, blueprint);

        if (themeData.TextureProps.Count != 0)
        {
            floorPropLocator.BuildTextureProps(themeData, blueprint);
        }

        if (themeData.OnliquidProps.Count != 0)
        {
            floorPropLocator.BuildPropsOnLiquid(themeData, blueprint);
        }

        bool isFirstStage = blueprint.StageType == StageType.FirstStage;

        if (isFirstStage)
        {
            floorLiquidBuilder.DeactivateLiquid();
        }
        else
        {
            floorLEDBuilder.TriggerFloorExplosion();
            floorLiquidBuilder.BuildAndTransformFloor(blueprint.LiquidType, blueprint.Size, anchorPos);
        }
    }

    public void ClearFloor()
    {
        floorLEDBuilder.DeactivateLEDFloor();
        floorLiquidBuilder.DeactivateLiquid();
    }

    private void OnScenarioLoaded(GameScenario scenario)
    {
        ESDBuilder.ChangeScenario(scenario);
    }

    private void OnThemeChanged(ThemeData themeData)
    {
        this.themeData = themeData;
        floorLEDBuilder.CubeSet?.CornerLEDCubePool.ClearPool(); // unimplemented
        floorLEDBuilder.CubeSet = themeData.FloorBlockSet;
        floorLEDBuilder.ChangeFloorTheme();
        localHeightMaps = themeData.localHeightMaps;
        floorTrapBuilder.SetTrap(themeData.trapTypes, themeData.trapDensity);
    }

    private void OnScenrarioUnloaded()
    {
        floorLEDBuilder.RemoveFloor();
        floorPropLocator.ClearProps();
        floorLEDButtonBuilder.DestoryLEDButtons();
    }

    public void SetFloorActive(bool value)
    {
        floorLEDBuilder.gameObject.SetActive(value);
        floorLiquidBuilder.gameObject.SetActive(value);
    }

    private void OnDestroy()
    {
    }
}
