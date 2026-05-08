using System;
using System.Collections.Generic;
using AYellowpaper;
using UnityEngine;


public class PoolableLifeSpanCounter : MonoBehaviour
{
    [SerializeField]
    private InterfaceReference<ISpawner> serializedSpawner;
 
    protected Dictionary<IPoolable, float> poolables = new Dictionary<IPoolable, float>();

    private static Dictionary<IPoolable, float> poolablesCopy = new Dictionary<IPoolable, float>(); 
    private static List<IPoolable> toRemove = new List<IPoolable>();

    private ISpawner spawner;
    private float lifeSpan = 1f;
    private bool checkLifeSpan = true;
    
    protected Action<IPoolable> releaseEventListener;

    protected int count = 0;
    
    IPoolable poolable = default;
    float life = 0f;

    public int Count => count;

    protected virtual void Awake()
    {
        spawner = serializedSpawner.Value;
        spawner.SubscribeSpawnEvent(AddPoolable);
        
        if (!spawner.TryGetLifeSpan(out lifeSpan))
        {
            checkLifeSpan = false;
        }

        spawner.TryGetReleaseEventListener(out releaseEventListener);
    }

    private void AddPoolable(IPoolable poolable)
    {
        if (poolables.ContainsKey(poolable))
        {
            Debug.Log(poolable.transform.name +" already exists in the life span counter.");
            poolables[poolable] = lifeSpan;
            return;
        }
        
        poolables.Add(poolable, lifeSpan);
        count += 1;
    }

    protected virtual void Update()
    {

        toRemove.Clear();
        poolablesCopy.Clear();


        foreach (var kvp in poolables)
        {
            poolable = kvp.Key;
            life = kvp.Value;
            
            if(poolable.Released)
            {
                toRemove.Add(poolable);
                continue;
            } 
            
            if(checkLifeSpan && life < 0f)
            {
                poolable.Release();
                releaseEventListener?.Invoke(poolable);
                toRemove.Add(poolable);
            }
            else
            {
                poolablesCopy[poolable] = life - Time.deltaTime;

            }
        }

        RemoveToRemove();
        count = poolables.Count;

        if (checkLifeSpan)
        {
            CopyLifespan();
        }

    }

    private void RemoveToRemove()
    {
        for (int i = 0; i < toRemove.Count; i++)
        {
            poolables.Remove(toRemove[i]);
        }
    }

    private void CopyLifespan()
    {
        foreach (var kvp in poolablesCopy)
        {
            poolables[kvp.Key] = kvp.Value;
        }
    }

    
    protected virtual void OnDestroy()
    {
        spawner.UnsubscribeSpawnEvent(AddPoolable);
    }
}