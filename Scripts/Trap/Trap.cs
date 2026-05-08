using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using Local.Scripts.Extensions;

public abstract class Trap : Poolable<Trap>, ILocatable
{ 
    [SerializeField]
    private int size;

    [SerializeField] 
    private float periodValue;

    
    [SerializeReference]
    private WaitForSeconds period;

    [SerializeField]
    private EventChannelSO playerDeadEC, playerReviveEC;

    public bool SelectLocation(Dictionary<Vector3, Vector3[]> locations, out List<Vector3> selected)
    {
        List<List<Vector3>> pieces = new List<List<Vector3>>();

        foreach (var array in locations.Values)
        {
            List<Vector3> space = new List<Vector3>();

            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] != Vector3.zero)
                {
                    space.Add(array[i]);
                }
                else
                {
                    space = new List<Vector3>();
                }

                if (space.Count == size)
                {
                    pieces.Add(space);
                    space = new List<Vector3>();
                }
            }
        }

        if (pieces.Count == 0)
        {
            selected = null;
            return false;
        }

        selected = pieces.PickRandom();
        return true;
    }

    protected abstract void On();
    protected abstract void Off();

    protected virtual void Awake()
    {
        period = new WaitForSeconds(periodValue);
    }
    
    protected override void OnEnable()
    {
        base.OnEnable();
        StartCoroutine(RepeatOnOff());
        playerDeadEC.Subscribe(Deactivate);
        playerReviveEC.Subscribe(Activate);
    }

    protected virtual void OnDisable()
    {
        StopCoroutine(RepeatOnOff());
        playerDeadEC.Unsubscribe(Deactivate);
        playerReviveEC.Unsubscribe(Activate);
    }

    private IEnumerator RepeatOnOff()
    {
        while (true)
        {
            yield return period;
            On();
            yield return period;
            Off();
        }
    }

    private void Deactivate()
    {
        StopAllCoroutines();
        Off();
    }

    private void Activate()
    {
        StartCoroutine(RepeatOnOff());
    }
}
