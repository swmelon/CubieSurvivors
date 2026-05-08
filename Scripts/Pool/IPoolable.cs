
using UnityEngine;
using UnityEngine.Pool;

public interface IPoolable
{
    public void SetManagedPool<T>(IObjectPool<T> pool) where T : class, IPoolable;
    public bool Released { get; }
    public void Release();
    public void RaiseSpawnEvent();
    public Transform transform { get; }
    public GameObject gameObject{ get; }

}
