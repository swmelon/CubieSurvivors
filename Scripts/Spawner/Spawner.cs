
using System;
using UnityEngine;
using UnityEngine.Serialization;


public class Spawner<T> : Pooling<T>, ISpawner where T : class, IPoolable
{ 
    [SerializeField]
    private bool checkLifeSpan = true;
    
    [SerializeField]
    private bool checkReleaseEvent = false;
    
    [SerializeField][Range(1, 30)]
    private int spawnedObjectLifeSpan = 1;
    
    protected Action<IPoolable> spawnEventListeners;
    
    private Vector3 spawnPosition;
    private Quaternion spawnRotation;

    public virtual T Spawn()
    {
        T instance = objectPool.Get();
        spawnEventListeners?.Invoke(instance);
        return instance;
    }

    public virtual T Spawn(Vector3 spawnPos)
    {
        position = spawnPos;
        return Spawn();
    }

    public virtual T Spawn(Vector3 spawnPos, Quaternion spawnRot)
    {
        position = spawnPos;
        rotation = spawnRot;
        return Spawn();
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
        // 상속받은 클래스에서 구현
    }
}


// Use this class when you want to spawn a derived class of Poolable<T> from a pool.
public class Spawner<T, U> : Spawner<U> where U : Poolable<U> where T : U 
{
    // Intended to hide the base class's Spawn() method.
    public new virtual T Spawn()
    {
        T instance = objectPool.Get() as T;
        spawnEventListeners?.Invoke(instance);
        return instance;
    }
}