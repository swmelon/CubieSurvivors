using UnityEngine;
using UnityEngine.Events;


public class PoolableLifeSpanCounterWithEvent : PoolableLifeSpanCounter
{
    [SerializeField]
    private EventChannelSO[] releaseEventChannel;

    protected override void Awake()
    {
        foreach (var channel in releaseEventChannel)
        {
            channel.Subscribe(ReleaseAll);
        }
        
        base.Awake();
    }
    
    private void ReleaseAll()
    {
        foreach(var kvp in poolables)
        {
            if (!kvp.Key.Released)
            {
                kvp.Key.Release();
                releaseEventListener?.Invoke(kvp.Key);
            }
        }
        
        poolables.Clear();
        count = 0;
    }

    protected override void OnDestroy()
    {
        foreach (var channel in releaseEventChannel)
        {
            channel.Unsubscribe(ReleaseAll);
        }
        
        base.OnDestroy();
    }
}