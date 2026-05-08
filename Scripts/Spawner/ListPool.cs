
using System;
using System.Collections.Generic;
using AYellowpaper;
using UnityEngine;

public class ListPool : MonoBehaviour
{
    [SerializeField]
    private InterfaceReference<ISpawner> serializedSpawner;

    private ISpawner spawner;
    private List<IPoolable> poolables = new List<IPoolable>();
    private Action<IPoolable> releaseEventListener;
    
    protected virtual void Awake()
    {
        spawner = serializedSpawner.Value;
        spawner.SubscribeSpawnEvent(AddPoolable);
        spawner.TryGetReleaseEventListener(out releaseEventListener);
    }
    
    private void AddPoolable(IPoolable poolable)
    {
        poolables.Add(poolable);
    }
}
