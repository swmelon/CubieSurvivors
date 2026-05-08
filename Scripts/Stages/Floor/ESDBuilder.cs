using Local.Scripts.Extensions;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Floor Enemy Spawn Device Builder
/// </summary>
public class ESDBuilder : MonoBehaviour, IEasyListener
{
    [SerializeField]
    [Range(1, 5)]
    private int minSpawnPeriod, maxSpawnPeriod;

    [SerializeField]
    private EnemySpawnDevicePool esdPool;

    [SerializeField]
    private ESDLayoutContainer esdLayouts;

    [SerializeField]
    private IndicatorDisplay indicatorDisplay;

    [SerializeField]
    private bool testOneDevice = false;

    private FloorLEDBuilder floorLEDBuilder;
    private List<LocatableESD> activeESDs, inactiveESDs;
    private ESDContainer esdContainer;
    private EnemyDataContainer enemyDataContainer;

    private Vector3[] positions = new Vector3[64];
    private Quaternion[] rotations = new Quaternion[64];

    [SerializeReference]
    private ESDLayout currentLayout;

    private struct ESDData
    {
        public EnemyData EnemyData;
        public int Period;

        public ESDData(EnemyData enemyData, int period)
        {
            this.EnemyData = enemyData;
            this.Period = period;
        }
    }

    private List<ESDData> esdDatas = new List<ESDData>();
    private float[] esdPPRs;

    private ESDMover esdMover;

    private void Awake()
    {
        activeESDs = new List<LocatableESD>();
        inactiveESDs = new List<LocatableESD>();

        floorLEDBuilder = transform.root.GetComponentInChildren<FloorLEDBuilder>();
        esdMover = GetComponent<ESDMover>();
        esdPool.Initialize();
    }

    public void BuildEnemySpawnDevice(int ppr, int minEDS, int maxEDS)
    {
        List<ESDData> availableEnemySpawnDevices = GetAvailableEnemySpawnDevices(ppr, minEDS, maxEDS);

        int devicesCount = RandomExtenstion.GetIntInRange(minEDS, maxEDS);

        if (testOneDevice)
        {
            devicesCount = 1;
        }

        ESDLayout layout = esdLayouts.Datas.PickRandom();

        if (layout.directions.Count == 0)
        {
            BuildUndirectionalLayout(layout, devicesCount, availableEnemySpawnDevices);
        }
        else
        {
            BuildDirectionalLayout(layout, devicesCount, availableEnemySpawnDevices);
        }

        currentLayout = layout;
    }

    public void OnBeat(EasyEvent easyEvent)
    {
        for (int i = 0; i < activeESDs.Count; i++)
        {
            activeESDs[i].OnBeat(easyEvent);
        }
    }

    private void BuildUndirectionalLayout(ESDLayout layout, int devicesCount, List<ESDData> availableEnemySpawnDevices)
    {
        for (int i = 0; i < devicesCount; i++)
        {
            ESDData deviceInfo = availableEnemySpawnDevices.PickRandom();

            LocatableESD device = esdPool.Get(esdContainer.GetRandom());

            if (floorLEDBuilder.TryGetESDPositionAndRotation(out Vector3 position, out Quaternion rotation))
            {
                device.gameObject.SetActive(false);
                device.EnemyData = deviceInfo.EnemyData;
                device.SpawnPeriod = deviceInfo.Period;
                device.transform.SetParent(floorLEDBuilder.transform);
                device.transform.SetPositionAndRotation(position, rotation);
                inactiveESDs.Add(device);

                if (true)
                {
                    esdMover.AddESD(device);
                }
            }
            else
            {
                device.Release();
            }
        }
    }

    private void BuildDirectionalLayout(ESDLayout layout, int devicesCount, List<ESDData> availableEnemySpawnDevices)
    {
        int neighborCount = layout.moveESD ? 2 : 1;
        int availableDevicesCount = floorLEDBuilder.GetESDLocations(devicesCount, neighborCount, layout.directions, positions, rotations);

        if (availableDevicesCount < devicesCount)
        {
            Debug.LogWarning("Directional Layout : Not enough space for ESDs");
            devicesCount = availableDevicesCount;
        }

        for (int i = 0; i < devicesCount; i++)
        {
            ESDData deviceInfo = availableEnemySpawnDevices.PickRandom();

            LocatableESD device = esdPool.Get(esdContainer.GetRandom());

            device.gameObject.SetActive(false);
            device.EnemyData = deviceInfo.EnemyData;
            device.SpawnPeriod = deviceInfo.Period;
            device.transform.SetParent(floorLEDBuilder.transform);
            device.transform.SetPositionAndRotation(positions[i], rotations[i]);
            inactiveESDs.Add(device);

            if (true)
            {
                esdMover.AddESD(device);
            }
        }
    }

    private List<ESDData> GetAvailableEnemySpawnDevices(int ppr, int minESD, int maxESD)
    {
        if (maxESD == 0)
        {
            return new List<ESDData>();
        }

        int from = -1, to = -1;

        for (int i = 0; i < esdDatas.Count; i++)
        {
            if (esdPPRs[i] * maxESD >= ppr)
            {
                from = i;
                break;
            }
        }

        for (int i = esdDatas.Count - 1; i >= 0; i--)
        {
            if (esdPPRs[i] * minESD <= ppr)
            {
                to = i;
                break;
            }
        }

        if (from == -1 && to == -1)
        {
            Debug.LogError("No EnemySpawnDevice is matched with condition.");
        }
        else if (from == -1)
        {
            Debug.LogWarning("Existing PPR Combination of EDS could not reach targetPPR");
            from = esdDatas.Count - 1;
        }
        else if (to == -1)
        {
            Debug.LogWarning("Existing PPR Combination of EDS could not smaller then targetPPR");
            to = 0;
        }
        else if (from > to)
        {
            return GetAvailableEnemySpawnDevices(ppr, minESD, maxESD + 1);
        }

        return esdDatas.GetRange(from, to - from + 1);
    }

    public void ActivateDevices()
    {
        bool hasLayout = !ReferenceEquals(currentLayout, null);

        esdMover.StartMoveESD(hasLayout? currentLayout.CycleMode() : false);

        for (int i = 0; i < inactiveESDs.Count; i++)
        {
            LocatableESD esd = inactiveESDs[i];
            esd.gameObject.SetActive(true);
            esd.Activate();
            activeESDs.Add(esd);
        }

        if (inactiveESDs.Count != 0 && hasLayout && currentLayout.TryGetIndicator(out Sprite indicator))
        {
            indicatorDisplay.SetIndicator(indicator);
        }

        inactiveESDs.Clear();
    }

    public void ReleaseDevices()
    {
        esdMover.StopMoveESD();

        for (int i = 0; i < activeESDs.Count; i++)
        {
            LocatableESD esd = activeESDs[i];
            esd.Release();
        }

        activeESDs.Clear();
    }

    public void ChangeScenario(GameScenario gameScenario)
    {
        enemyDataContainer = gameScenario.EnemyDataContainer;
        esdContainer = gameScenario.ESDContainer;

        InitializeESDs();
    }


    private void InitializeESDs()
    {
        esdDatas.Clear();

        EnemyData[] enemyDatas = enemyDataContainer.Datas;

        for (int i = 0; i < enemyDatas.Length; i++)
        {
            for (int j = minSpawnPeriod; j <= maxSpawnPeriod; j++)
            {
                esdDatas.Add(new ESDData(enemyDatas[i], j));
            }
        }

        esdDatas = esdDatas.OrderBy(item => item.EnemyData.Power / item.Period).ToList();

        esdPPRs = new float[esdDatas.Count];

        for (int i = 0; i < esdDatas.Count; i++)
        {
            esdPPRs[i] = esdDatas[i].EnemyData.Power / esdDatas[i].Period;
        }
    }
}