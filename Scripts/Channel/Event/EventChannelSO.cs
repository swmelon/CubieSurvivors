using System;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "EventChannel", menuName = "ScriptableObjects/Channels/EventChannel", order = SOAssetMenuIndex.Channel)]
public class EventChannelSO : ScriptableObject
{
    private Action listeners, lastListeners;
    
    public void Subscribe(Action listener)
    {
        listeners -= listener;  // 중복 구독 방지
        listeners += listener;
    }
    
    public void SubscribeLast(Action listener)
    {
        lastListeners += listener;
    }
    
    public void Unsubscribe(Action listener)
    {
        listeners -= listener;
        lastListeners -= listener;
    }
    
    public void Raise()
    {
        listeners?.Invoke();
        lastListeners?.Invoke();
    }
}
