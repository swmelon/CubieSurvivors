using System.Collections.Generic;
using UnityEngine.Pool;



public class CustomObjectPool<T> : IObjectPool<T> where T : class
{
    public int CountInactive => pool.Count;
    public delegate T FactoryMethod();
    public delegate void OnGetAction(T item);
    public delegate void OnReleaseAction(T item);
    public delegate void OnDestroyAction(T item);

    private Queue<T> pool;
    private HashSet<T> activeItems;
    private FactoryMethod createMethod;
    private OnGetAction onGet;
    private OnReleaseAction onRelease;
    private OnDestroyAction onDestroy;

    public CustomObjectPool(FactoryMethod createMethod, OnGetAction onGet = null, OnReleaseAction onRelease = null, OnDestroyAction onDestroy = null, int maxSize = 10)
    {
        pool = new Queue<T>();
        activeItems = new HashSet<T>();
        this.createMethod = createMethod;
        this.onGet = onGet;
        this.onRelease = onRelease;
        this.onDestroy = onDestroy;

    }

    public T Get()
    {
        T instance;

        if (pool.Count > 0)
        {
            instance = pool.Dequeue();
        }
        else
        {
            instance = createMethod();
        }

        activeItems.Add(instance);
        onGet?.Invoke(instance);

        return instance;
    }

    /// <summary>
    ///  Do not use the return value of this method. It seems like can't access to PooledObject<T> constructor.
    /// </summary>
    /// <param name="v"></param>
    /// <returns></returns>
    public PooledObject<T> Get(out T v)
    {
        v = Get();
        return new PooledObject<T>();
    }
    
    public HashSet<T> GetActiveItems()
    {
        return activeItems;
    }

    public void Release(T item)
    {
        onRelease?.Invoke(item);
        activeItems.Remove(item);
        pool.Enqueue(item);
    }
    
    public void Clear()
    {
        foreach (T item in activeItems)
        {
            onDestroy?.Invoke(item);
        }
        
        activeItems.Clear();
        pool.Clear();
    }
}
