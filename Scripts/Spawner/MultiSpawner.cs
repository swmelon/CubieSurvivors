using System;
using System.Collections.Generic;
using Local.Scripts.Extensions;
using UnityEngine;
using UnityEngine.Serialization;

public class MultiSpawner<T> : MultiPooling<T>, ISpawner where T : MonoBehaviour, IPoolable
{
    [SerializeField]
    private bool checkLifeSpan = false;
    
    [SerializeField]
    private bool checkReleaseEvent = false;
    
    [SerializeField][Range(1, 30)]
    private int spawnedObjectLifeSpan = 1;
    
    private Action<IPoolable> spawnEventListeners;
    
    public virtual T Spawn(T prefab)
    {
        T instance = Get(prefab);
        spawnEventListeners?.Invoke(instance);
        instance.RaiseSpawnEvent();
        return instance;
    }

    public T Spawn(T prefab, Vector3 spawnPos)
    {
        position = spawnPos;
        return Spawn(prefab);
    }

    public T Spawn(T prefab, Vector3 spawnPos, Quaternion spawnRot)
    {
        position = spawnPos;
        rotation = spawnRot;
        return Spawn(prefab);
    }

    public virtual T Spawn(T prefab, Transform parent)
    {
        T instance = Get(prefab);
        instance.transform.parent = parent;
        instance.RaiseSpawnEvent();
        spawnEventListeners?.Invoke(instance);
        return instance;
    }
    
    public virtual T SpawnRandom() => GetRandom();
    
    public void SubscribeSpawnEvent(Action<IPoolable> listener)
    {
        spawnEventListeners += listener;
    }
    
    public void UnsubscribeSpawnEvent(Action<IPoolable> listener)
    {
        spawnEventListeners -= listener;
    }
    
    public bool TryGetLifeSpan(out float lifeSpan)
    {
        lifeSpan = spawnedObjectLifeSpan;
        return checkLifeSpan;
    }
    
    public bool TryGetReleaseEventListener(out Action<IPoolable> listener)
    {
        listener = OnRelease;
        return checkReleaseEvent;
    }
    
    protected virtual void OnRelease(IPoolable poolable)
    {
    }
}


public class MultiSpawner<T, U> : MultiPooling<T, U>, ISpawner where T : MonoBehaviour, IPoolable where U : class
{
    [SerializeField]
    private bool checkLifeSpan = false;
    
    [SerializeField]
    private bool checkReleaseEvent = false;
    
    [SerializeField][Range(1, 30)]
    private int spawnedObjectLifeSpan = 1;
    
    private Action<IPoolable> spawnEventListeners;
    
    private T Spawn(U enumType)
    {
        T instance = Get(enumType);
        spawnEventListeners?.Invoke(instance);
        instance.RaiseSpawnEvent();
        return instance;
    }

    public T Spawn(U enumType, Vector3 spawnPos)
    {
        position = spawnPos;
        T instance = Spawn(enumType);
        return instance;
    }

    public T Spawn(U enumType, Vector3 spawnPos, Quaternion spawnRot)
    {
        position = spawnPos;
        rotation = spawnRot;
        T instance = Spawn(enumType);
        return instance;
    }
    
    public void SubscribeSpawnEvent(Action<IPoolable> listener)
    {
        spawnEventListeners += listener;
    }
    
    public void UnsubscribeSpawnEvent(Action<IPoolable> listener)
    {
        spawnEventListeners -= listener;
    }
    
    public bool TryGetLifeSpan(out float lifeSpan)
    {
        lifeSpan = spawnedObjectLifeSpan;
        return checkLifeSpan;
    }
    
    public bool TryGetReleaseEventListener(out Action<IPoolable> listener)
    {
        listener = OnRelease;
        return checkReleaseEvent;
    }
    
    protected virtual void OnRelease(IPoolable poolable)
    {
    }
}

public class MultiSpawnerWithData<T, TData> : MultiPoolingWithData<T, TData>, ISpawner where T : MonoBehaviour, IPoolable where TData : PrefabDataSO<T>
{
    [SerializeField]
    private bool checkLifeSpan = false;
    
    [SerializeField]
    private bool checkReleaseEvent = false;
    
    [SerializeField][Range(1, 10)]
    private int spawnedObjectLifeSpan = 1;

    private Action<IPoolable> spawnEventListeners;
    
    public virtual T Spawn(TData data)
    {
        T instance = Get(data.Prefab);
        spawnEventListeners?.Invoke(instance);
        instance.RaiseSpawnEvent();
        return instance;
    }

    public  virtual T Spawn(TData data, Vector3 spawnPos, Quaternion spawnRot)
    {
        position = spawnPos;
        rotation = spawnRot;
        return Spawn(data);
    }
    
    public void SubscribeSpawnEvent(Action<IPoolable> listener)
    {
        spawnEventListeners += listener;
    }
    
    public void UnsubscribeSpawnEvent(Action<IPoolable> listener)
    {
        spawnEventListeners -= listener;
    }
    
    public bool TryGetLifeSpan(out float lifeSpan)
    {
        lifeSpan = spawnedObjectLifeSpan;
        return checkLifeSpan;
    }
    
    public bool TryGetReleaseEventListener(out Action<IPoolable> listener)
    {
        listener = OnRelease;
        return checkReleaseEvent;
    }

    protected virtual void OnRelease(IPoolable poolable)
    {
    }
}

