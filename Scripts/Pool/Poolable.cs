using UnityEngine;
using UnityEngine.Pool;
using System;

public abstract class Poolable<T> : MonoBehaviour, IPoolable where T : MonoBehaviour
{
    public bool Released => released;
    public void RaiseSpawnEvent() => onSpawned?.Invoke();
    
    private bool released = false; 
    private Action onSpawned;
    private IObjectPool<T> managedPool;
    
    protected virtual void OnEnable()
    {
        released = false;
    }
    
    public void SetManagedPool<U>(IObjectPool<U> pool) where U : class, IPoolable
    {
        IObjectPool<T> temp = pool as IObjectPool<T>;

        if (ReferenceEquals(temp, null))
        {
            Debug.LogError("Poolable object is not of type " + typeof(T).Name + ".");
        }
        
        managedPool = pool as IObjectPool<T>;
    }
    
    public void AddOnSpawnedEvent(Action action)
    {
        onSpawned += action;
    }
    
    public virtual void Release()
    {
        if (released)
        {
            Debug.LogError(name + " object has already been released.");
            return;
        }
        
        released = true;
        transform.SetParent(null);
        managedPool.Release(this as T);
    }
}
