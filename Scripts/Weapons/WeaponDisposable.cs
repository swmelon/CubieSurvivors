using UnityEngine;
using UnityEngine.Pool;
using System;
using UnityEngine.Rendering;



// 일단 Pooable을 상속받으려면 고쳐야할게 많으므로, Poolable을 복붙했다.
public abstract class WeaponDisposable : Weapon, IPoolable
{
    public bool Released => released;
    public void RaiseSpawnEvent() => onSpawned?.Invoke();

    private bool released = false;
    private Action onSpawned;
    private IObjectPool<WeaponDisposable> managedPool;
    protected IWeaponManager weaponManager;

    protected virtual void OnEnable()
    {
        released = false;
    }

    public void SetManagedPool<T>(IObjectPool<T> pool) where T : class, IPoolable
    {
        IObjectPool<WeaponDisposable> temp = pool as IObjectPool<WeaponDisposable>;

        if (ReferenceEquals(temp, null))
        {
            Debug.LogError("Poolable object is not of type " + typeof(T).Name + ".");
        }

        managedPool = pool as IObjectPool<WeaponDisposable>;
    }

    protected void AddOnSpawnedEvent(Action action)
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
        managedPool.Release(this);
    }

    public void SetWeaponManager(IWeaponManager weaponManager)
    {
        this.weaponManager = weaponManager;
    }
}