using System;
using System.Collections.Generic;
using System.Dynamic;
using Local.Scripts.Extensions;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Serialization;


public class Pooling<T> : ScriptableObject, IDependentInitialization where T : class, IPoolable
{
    [SerializeField] 
    protected T poolablePrefab;

    [SerializeField] 
    private int poolSize;
    
    [SerializeField]
    private int preInstantiateCount = 0;
    
    protected IObjectPool<T> objectPool;
    protected Vector3 position;
    protected Quaternion rotation;
    
    public virtual void Initialize()
    { 
        ResetPool();

        T[] instances = new T[preInstantiateCount];
        
        for (int i = 0; i < preInstantiateCount; i++)
        {
            instances[i] = objectPool.Get();
        }
        
        for (int i = 0; i < preInstantiateCount; i++)
        {
            instances[i].Release();
        }

        Debug.Log("Pre-instantiate " + preInstantiateCount  + " " + typeof(T).Name + " Complete");

    }

    protected virtual void OnEnable()
    {
    }

    protected void ResetPool()
    {
        objectPool = new ObjectPool<T>(CreatePoolable, OnGetPoolable, OnReleasePoolable, OnDestroyPoolable, maxSize: poolSize);
    }

    protected virtual T CreatePoolable()
    {
        T poolable = Instantiate(poolablePrefab.gameObject, position, rotation).GetComponent<T>();
        poolable.SetManagedPool(objectPool);
        return poolable;
    }

    protected virtual void OnGetPoolable(T poolable)
    {
        poolable.transform.SetPositionAndRotation(position, rotation);
        poolable.gameObject.SetActive(true);
    }

    private void OnReleasePoolable(T poolable)
    {
        poolable.gameObject.SetActive(false);
    }

    private void OnDestroyPoolable(T poolable)
    {
        Destroy(poolable.gameObject);
    }
}

