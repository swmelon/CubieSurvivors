using Local.Scripts.Extensions;
using System.Collections.Generic;
using UnityEngine;

public class FloorTrapBuilder : MonoBehaviour
{
    private List<TrapType> trapTypes;

    [SerializeField]
    private TrapSpawner trapSpawner;

    [SerializeField]
    [Range(1f, 2f)]
    private float numTrapMultiplier = 1.5f;

    [SerializeField]
    private WorldDirectionChannelSO worldDirectionChannel;


    private List<Trap> activeTraps, inactiveTraps; 
    private FloorLEDBuilder floorLEDBuilder;
    private float trapDensity;

    private void Awake()
    {
        floorLEDBuilder = transform.root.GetComponentInChildren<FloorLEDBuilder>();
        activeTraps = new List<Trap>();
        inactiveTraps = new List<Trap>();
    }

    private List<Trap> GetRandomTraps(int stageSize)
    {
        List<Trap> traps = new List<Trap>();
        int stageSizeFactor = (int)(2 * trapDensity  * Mathf.Pow(Mathf.RoundToInt((float)stageSize / 5) - 2, numTrapMultiplier));

        for (int i = 0; i < 2 * stageSizeFactor +
             RandomExtenstion.GetIntInRange(0, stageSizeFactor) - RandomExtenstion.GetIntInRange(0, stageSizeFactor); i++)
        {
            traps.Add(trapSpawner.SpawnRandom());
        }

        return traps;
    }

    public void BuildFloorTrap()
    {
        if (trapTypes == null || trapTypes.Count == 0)
        {
            return;
        }
        // buildLEDFloor first
        List<Trap> traps = GetRandomTraps(floorLEDBuilder.Size);

        for (int i = 0; i < traps.Count; i++)
        {
            Trap trap = traps[i];

            if (floorLEDBuilder.TryGetOnFloorObjectPosition(out Vector3 position))
            {
                trap.gameObject.SetActive(false);
                trap.transform.SetParent(floorLEDBuilder.transform);
                trap.transform.SetPositionAndRotation(position, worldDirectionChannel.RandomRotation());
                inactiveTraps.Add(trap);
            }
            else
            {
                trap.Release();
            }
        }
    }

    public void ActivateTraps()
    {
        for (int i = 0; i < inactiveTraps.Count; i++)
        {
            Trap trap = inactiveTraps[i];
            trap.gameObject.SetActive(true);
            activeTraps.Add(trap);
        }

        inactiveTraps.Clear();
    }

    public void ReleaseTraps()
    {
        for (int i = 0; i < activeTraps.Count; i++)
        {
            Trap trap = activeTraps[i];
            trap.Release();
        }

        activeTraps.Clear();
    }

    public void SetTrap(List<TrapType> trapTypes, float trapDensity)
    {
        this.trapTypes = trapTypes;
        this.trapDensity = trapDensity;
    }
}