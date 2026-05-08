
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "UserInputControlChannel", menuName = "ScriptableObjects/Channels/UserInputControlChannel", order = SOAssetMenuIndex.Channel)]
public class UserInputControlChannel : ScriptableObject
{
    [SerializeField] 
    private BooleanEventChannelSO inputCanvasControlChannel, playerInputControlChannel;

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

    public void Raise(bool val)
    {
        inputCanvasControlChannel.Raise(val);
        playerInputControlChannel.Raise(val);
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
