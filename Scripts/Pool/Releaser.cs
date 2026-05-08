
using System;
using System.Collections.Generic;
using UnityEngine;

public class Releaser : MonoBehaviour
{
    private List<IPoolable> poolables = new List<IPoolable>();
    
    public void AddPoolable(IPoolable poolable)
    {
        poolables.Add(poolable);
    }
    
    public void Release()
    {
        foreach (IPoolable poolable in poolables)
        {
            if (poolable.gameObject.activeSelf == false)
            {
                Debug.Log(poolable.transform.name + " is already inactive.");
                continue;
            }
            if (!poolable.Released)
            {
                poolable.Release();
            }
            else
            {
                Debug.Log(poolable.transform.name + "Already released before Releaser release it.");
            }
        }
        
        poolables.Clear();
    }
}
