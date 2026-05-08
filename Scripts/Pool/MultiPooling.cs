using UnityEngine;
using UnityEngine.Pool;
using System.Collections.Generic;
using Local.Scripts.Extensions;
using System.Linq;


public abstract class MultiPooling<T> : ScriptableObject, IDependentInitialization where T : MonoBehaviour, IPoolable
{
    [SerializeField]
    protected List<T> prefabList;
    
    [SerializeField] 
    private int poolSize;

    private Dictionary<T, IObjectPool<T>> objectPools = new Dictionary<T, IObjectPool<T>>();
    private T prefabToInstantiate;
     
    [SerializeField] 
    protected int preInstantiateCount = 0;

    [SerializeField]
    private bool setPoolSeparately = false;

    [SerializeField]
    private List<int> poolSizes;

    [SerializeField]
    private List<int> preInstantiateCounts;
    
    public List<T> PrefabList => prefabList;    


    protected Vector3 position;
    protected Quaternion rotation;

    public virtual void Initialize()
    {
        ResetPool();

        T[] instances;

        if (setPoolSeparately && preInstantiateCounts.Count > 0)
        {
            instances = new T[preInstantiateCounts.Max()];
        }
        else
        {
            instances = new T[preInstantiateCount];
        }

        for (int i = 0; i < prefabList.Count; i++)
        {
            int count = setPoolSeparately ? preInstantiateCounts[i] : preInstantiateCount;
            
            for (int j = 0; j < count; j++)
            {
                instances[j] = Get(prefabList[i]);
            }
            
            for (int j = 0; j < count; j++)
            {
                instances[j].Release();
            }
        }

        if (!setPoolSeparately)
        {
            Debug.Log("Pre-instantiate " + preInstantiateCount  + " " + typeof(T).Name + " Complete");
        }
    }

    private void ResetPool()
    {
        objectPools.Clear();

        for (int i = 0; i < prefabList.Count; i++)
        {
            objectPools.Add(prefabList[i], new ObjectPool<T>(CreatePoolable, OnGetPoolable, OnReleasePoolable,
                               OnDestroyPoolable, maxSize: setPoolSeparately ? poolSizes[i] : poolSize));
        }
    }
    
    protected virtual void OnEnable()
    {
 
    }

    public virtual T Get(T prefab)
    {
        // in case CreatePoolable() Called, enumType will be used to decide what gameObject to Instantiate().
        prefabToInstantiate = prefab;
        return objectPools[prefab].Get();
    }

    public void ClearPool()
    {
        foreach (var pool in objectPools)
        {
            pool.Value.Clear();
        }
    }

    protected T GetRandom()
    {
        T prefab = objectPools.Keys.PickRandom();
        return Get(prefab);
    }
    
    private T CreatePoolable()
    {
        T poolable = Instantiate(prefabToInstantiate.gameObject, position, rotation).GetComponent<T>();
        poolable.SetManagedPool(objectPools[prefabToInstantiate]);
        return poolable;
    }

    private void OnGetPoolable(T poolable)
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

public abstract class MultiPooling<T, U> : ScriptableObject, IDependentInitialization where T : MonoBehaviour, IPoolable where U : class
{
    [SerializeField] 
    private SerializableDictionary<U, T> serializableDictionary;
    
    [SerializeField] 
    private int poolSize;

    private Dictionary<U, T> identifier = new Dictionary<U, T>();
    private Dictionary<U, IObjectPool<T>> objectPools = new Dictionary<U, IObjectPool<T>>();
    private U keyToInstantiate;

    protected Vector3 position;
    protected Quaternion rotation;

    public void Initialize()
    {
        ResetPool();
    }
    
    protected virtual void OnEnable()
    {
        identifier = serializableDictionary.ToDictionary();
    }

    private void ResetPool()
    {
        objectPools.Clear();

        foreach (U prefabType in identifier.Keys)
        {
            objectPools.Add(prefabType, new ObjectPool<T>(CreatePoolable, OnGetPoolable, OnReleasePoolable,
                OnDestroyPoolable, maxSize: poolSize));
        }
    }
    
    protected T Get(U key)
    {
        // in case CreatePoolable() Called, enumType will be used to decide what gameObject to Instantiate().
        keyToInstantiate = key;
        return objectPools[key].Get();
    }
    
    private T CreatePoolable()
    {
        T poolable = Instantiate(identifier[keyToInstantiate].gameObject, position, rotation).GetComponent<T>();
        poolable.SetManagedPool(objectPools[keyToInstantiate]);
        return poolable;
    }

    private void OnGetPoolable(T poolable)
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

public abstract class MultiPoolingWithData<T, TData> : ScriptableObject, IDependentInitialization where T : MonoBehaviour, IPoolable where TData : PrefabDataSO<T>
{
    [SerializeField]
    protected List<TData> prefabDataList;

    [SerializeField]
    private bool usePreinstantiatedPrefabs = false;

    [SerializeField]
    private SerializableDictionary<T, TData> preinstantiatedPrefabs;
    
    [SerializeField] 
    private int poolSize;

    protected Dictionary<T, IObjectPool<T>> objectPools = new Dictionary<T, IObjectPool<T>>();
    private T prefabToInstantiate;
    
    [SerializeField] 
    private int preinstantiateCount = 0;
    
    protected Vector3 position;
    protected Quaternion rotation;

    public virtual void Initialize()
    {
        ResetPool();

        if (usePreinstantiatedPrefabs)
        {
            Dictionary<T, TData> preinstantiatedPrefabsDictionary = preinstantiatedPrefabs.ToDictionary();
            foreach (var item in preinstantiatedPrefabsDictionary)
            {
                T instance = item.Key;
                TData data = item.Value;

                instance.SetManagedPool(objectPools[data.Prefab]);
                instance.Release();
            }
            
            Debug.Log("Pre-instantiate " + preinstantiateCount  + " " + typeof(T).Name + " Complete");
            return;
        }

        T[] instances = new T[preinstantiateCount];
        
        foreach (var data in prefabDataList)
        {
            for (int i = 0; i < preinstantiateCount; i++)
            {
                instances[i] = Get(data);
            }
            
            for (int i = 0; i < preinstantiateCount; i++)
            {
                instances[i].Release();
            }
        }
        
        Debug.Log("Pre-instantiate " + preinstantiateCount  + " " + typeof(T).Name + " Complete");
    }
    
    protected virtual void OnEnable()
    {
    }

    protected  void ResetPool()
    {
        objectPools.Clear();

        foreach (TData prefabData in prefabDataList)
        {
            objectPools.Add(prefabData.Prefab, new ObjectPool<T>(CreatePoolable, OnGetPoolable, OnReleasePoolable,
                               OnDestroyPoolable, maxSize: poolSize));
        }
    }
    
    public virtual T Get(TData data)
    {
        return Get(data.Prefab);
    }

    public virtual T Get(T prefab)
    {
        prefabToInstantiate = prefab;
        return objectPools[prefabToInstantiate].Get();
    }
    
    private T CreatePoolable()
    {
        T poolable = Instantiate(prefabToInstantiate, position, rotation).GetComponent<T>();
        poolable.SetManagedPool(objectPools[prefabToInstantiate]);
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


/// <summary>
/// This class is used to pool objects in the scene.
/// Single instance of each type of object is pooled.
/// </summary>
/// <typeparam name="T"></typeparam>
/// <typeparam name="TData"></typeparam>
public abstract class SingleInstancePoolingWithDataMB<T, TData> : MonoBehaviour where T : MonoBehaviour, IPoolable where TData : PrefabDataSO<T>
{
    [SerializeField]
    private SerializableDictionary<T, TData> preinstantiatedPrefabs;

    protected Dictionary<T, IObjectPool<T>> objectPools = new Dictionary<T, IObjectPool<T>>();
    protected List<TData> prefabDataList;
    private T prefabToInstantiate;

    private int poolSize = 1;

    private void Awake()
    {
        Initialize();
    }

    public virtual void Initialize()
    {

        Dictionary<T, TData> preinstantiatedPrefabsDictionary = preinstantiatedPrefabs.ToDictionary();
        prefabDataList = new List<TData>(preinstantiatedPrefabsDictionary.Values);

        InitializePool();

        foreach (var item in preinstantiatedPrefabsDictionary)
        {
            T instance = item.Key;
            TData data = item.Value;



            instance.SetManagedPool(objectPools[data.Prefab]);
            instance.Release();
        }

        Debug.Log("SetManagedPool() " + typeof(T).Name + " Complete");
        return;
    }

    protected virtual void OnEnable()
    {
    }

    protected void InitializePool()
    {
        objectPools.Clear();

        foreach (TData prefabData in prefabDataList)
        {
            objectPools.Add(prefabData.Prefab, new ObjectPool<T>(CreatePoolable, OnGetPoolable, OnReleasePoolable,
                               OnDestroyPoolable, maxSize: poolSize));
        }
    }

    public virtual T Get(TData data)
    {
        prefabToInstantiate = data.Prefab;
        return objectPools[prefabToInstantiate].Get();
    }

    private T CreatePoolable()
    {
        T poolable = Instantiate(prefabToInstantiate).GetComponent<T>();
        poolable.SetManagedPool(objectPools[prefabToInstantiate]);
        return poolable;
    }

    private void OnGetPoolable(T poolable)
    {
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