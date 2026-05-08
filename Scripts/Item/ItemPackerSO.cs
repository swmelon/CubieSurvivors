using Local.Scripts.Extensions;
using System;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "ItemPacker", menuName = "ScriptableObjects/ItemPacker", order = int.MaxValue)]
public class ItemPackerSO : ScriptableObject
{
    [SerializeField]
    private ItemSpawner itemSpawner;
    
    [SerializeField]
    private List<Item> itemPrefabs;

    [SerializeField]
    private FloorGeoDataChannel floorGeoDataChannel;

    private Dictionary<Type, Item> itemPrefabDict = new Dictionary<Type, Item>();
    
    private void OnEnable()
    {
        foreach (var item in itemPrefabs)
        {
            itemPrefabDict.Add(item.GetType(), item);
        }
    }
    
    public T WarpUp<U, T>(U itemizable, bool parachute = false) where T : CoveredItem<U> where U: MonoBehaviour, IItemizable
    {
        if (!itemPrefabDict.TryGetValue(typeof(T), out Item itemPrefab))
        {
            throw new Exception("No item prefab for type " + typeof(T));
        }

        T item = itemSpawner.Spawn(itemPrefab) as T;
        
        if (ReferenceEquals(item , null))
        {
            throw new Exception("No item prefab for type " + typeof(T));
        }
        
        item.SetContent(itemizable, parachute);
        return item;
    }

    public T[] WarpUp<U, T>(List<U> itemizable, Vector3 position, LayerMask layerToCheckOverlap, float radius = 5f) where T : CoveredItem<U> where U : MonoBehaviour, IItemizable
    {
        if (!itemPrefabDict.TryGetValue(typeof(T), out Item itemPrefab))
        {
            throw new Exception("No item prefab for type " + typeof(T));
        }

        int numItems = itemizable.Count;

        if (numItems == 0)
        {
            return null;
        }

        T[] items = new T[numItems];

        for (int i = 0; i < numItems; i++)
        {
            items[i] = itemSpawner.Spawn(itemPrefab) as T;

            if (ReferenceEquals(items[i], null))
            {
                throw new Exception("No item prefab for type " + typeof(T));
            }
        }

        for (int i = 0; i < numItems; i++)
        {
            items[i].SetContent(itemizable[i], parachute: false);
        }

        if(!floorGeoDataChannel.TryGetPopOutItemPosition(position, radius, 1f, numItems, 
            layerToCheckOverlap, out Vector3[] positions))
        {
            radius += 1f;

            int count = 0;

            while (!floorGeoDataChannel.TryGetPopOutItemPosition(position, radius, 1f, numItems, 
                               layerToCheckOverlap, out positions))
            {
                radius += 1f;
                count++;

                if (count > 5)
                {
                    throw new Exception("Cannot find pop out item position");
                }
            }
        }

        for (int i = 0; i < numItems; i++)
        {
            float yRotation = RandomExtenstion.GetIntInRange(0, 17) * 20f;
            items[i].SetPopOutPosition(position, positions[i] + Vector3.up);
            items[i].transform.rotation = Quaternion.Euler(0, yRotation, 0);
        }

        return items;
    }

    private GameObject OnlyMesh(GameObject gameObject)
    {
        foreach (Transform child in gameObject.transform)
        {
            if (child.gameObject.TryGetComponent(out MeshRenderer meshRenderer))
            {
                meshRenderer.enabled = true;
            }
            else
            {
                child.gameObject.SetActive(false);
            }
        }

        return gameObject;
    }
    
    private void OnlyMeshRecursively(GameObject gameObject)
    {
        foreach (Transform child in gameObject.transform)
        {
            if (child.gameObject.TryGetComponent(out MeshRenderer meshRenderer))
            {
                meshRenderer.enabled = true;
            }
            else
            {
                child.gameObject.SetActive(false);
            }
            
            OnlyMeshRecursively(child.gameObject);
        }
    }
}
