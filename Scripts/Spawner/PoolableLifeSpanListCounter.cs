using System;
using System.Collections.Generic;
using AYellowpaper;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;


public class PoolableLifeSpanListCounter : MonoBehaviour
{
    [SerializeField]
    private InterfaceReference<ISpawner> serializedSpawner;

    private List<IPoolable> poolables = new List<IPoolable>();
    private List<float> lifeSpans = new List<float>();
    
    private ISpawner spawner;
    private float lifeSpan = 1f;
    private Action<IPoolable> releaseEventListener;
    
    protected virtual void Awake()
    {
        spawner = serializedSpawner.Value;
        spawner.SubscribeSpawnEvent(AddPoolable);
        
        if (!spawner.TryGetLifeSpan(out lifeSpan))
        {
            Debug.LogWarning("Spawner does not use life span. Check the spawner.");
            Destroy(this);
        }

        spawner.TryGetReleaseEventListener(out releaseEventListener);
    }

    private void AddPoolable(IPoolable poolable)
    {
        poolables.Add(poolable);
        lifeSpans.Add(lifeSpan);
    }

    private void Update()
    {
        for (int i = 0; i < lifeSpans.Count; i++)
        {
            lifeSpans[i] -= Time.deltaTime;
        }

        int deadline = -1;
        
        for (int i = lifeSpans.Count - 1; i >= 0; i--)
        {
            if (lifeSpans[i] <= 0f)
            {
                deadline = i;
                break;
            }
        }
        
        if (deadline == -1)
        {
            return;
        }
        
        for (int i = deadline; i >= 0; i--)
        {
            if (!poolables[i].Released)
            {
                poolables[i].Release();
                releaseEventListener?.Invoke(poolables[i]);
            }
        }
        
        poolables.RemoveRange(0, deadline + 1);
        lifeSpans.RemoveRange(0, deadline + 1);
    }
    
    protected virtual void OnDestroy()
    {
        spawner.UnsubscribeSpawnEvent(AddPoolable);
    }
}