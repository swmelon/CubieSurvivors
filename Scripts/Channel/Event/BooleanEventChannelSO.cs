using System;
using System.Collections.Generic;
using UnityEngine;



[CreateAssetMenu(fileName = "BooleanEventChannel", menuName = "ScriptableObjects/Channels/BooleanEventChannel", order = SOAssetMenuIndex.Channel)]
public class BooleanEventChannelSO : TypeEventChannelSO<bool> 
{
    [SerializeField]
    private List<EventChannelSO> onEvents;

    [SerializeField]
    private List<EventChannelSO> offEvents;

    private void OnEnable()
    {
        foreach (EventChannelSO onEvent in onEvents)
        {
            onEvent.Subscribe(On);
        }

        foreach (EventChannelSO offEvent in offEvents)
        {
            offEvent.Subscribe(Off);
        }
    }

    private void On()
    {
        Raise(true);
    }

    private void Off()
    {
        Raise(false);
    }

    private void OnDisable()
    {
        foreach (EventChannelSO onEvent in onEvents)
        {
            onEvent.Unsubscribe(On);
        }

        foreach (EventChannelSO offEvent in offEvents)
        {
            offEvent.Unsubscribe(Off);
        }
    }
}
