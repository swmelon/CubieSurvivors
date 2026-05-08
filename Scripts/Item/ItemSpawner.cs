
using System.Collections.Generic;
using Local.Scripts.Extensions;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemSpawner", menuName = "ScriptableObjects/Spawner/ItemSpawner", order = SOAssetMenuIndex.Spawner)]
public class ItemSpawner : MultiSpawner<Item>
{
    [SerializeField]
    private OnePureEffectSpawner disappearingEffectSpawner;
    
    HashSet<Item> targetItems = new HashSet<Item>();
    List<Item> probabilityItems = new List<Item>();
    
    private List<float> cumulativeProbabilities = new List<float>();
    private float totalProbability = 0f;
    
    public override void Initialize()
    {
        ClassifyItems();
        UpdateItemProbabilities();
        base.Initialize();
    }
    private void ClassifyItems()
    {
        probabilityItems.Clear();

        foreach (var item in prefabList)
        {
            if (item.UseProbability)
            {
                probabilityItems.Add(item);
            }
            else
            {
                targetItems.Add(item);
            }
        }
    }
    
    public void UpdateItemProbabilities()
    {
        cumulativeProbabilities.Clear();
        totalProbability = 0f;

        for (int i = 0; i < probabilityItems.Count; i++)
        {
            totalProbability += probabilityItems[i].Probability;
            cumulativeProbabilities.Add(totalProbability);
        }
    }
    
    private void PreInstantiateTargetItems()
    {
        Item[] instances = new Item[preInstantiateCount];
        
        foreach (var prefab in targetItems)
        {
            for (int i = 0; i < preInstantiateCount; i++)
            {
                instances[i] = Get(prefab);
            }
            
            for (int i = 0; i < preInstantiateCount; i++)
            {
                instances[i].Release();
            }
        }
        
        Debug.Log("Pre-instantiate " + preInstantiateCount  + " target items Complete");
    }

    public override Item Spawn(Item prefab)
    {
        if (targetItems.Contains(prefab))
        {
            return base.Spawn(prefab);
        }
        
        Debug.LogError("ItemSpawner: " + prefab.name + " is not in the targetItems list.");
        return default;
    }
    
    public override Item SpawnRandom()
    {
        float randomPoint = RandomExtenstion.GetRandomProbability() * totalProbability;
        for (int i = 0; i < cumulativeProbabilities.Count; i++)
        {
            if (randomPoint <= cumulativeProbabilities[i])
            {
                return base.Spawn(probabilityItems[i]);
            }
        }
        
        Debug.LogError("ItemSpawner: Failed to spawn an item by probability.");
        return default;
    }

    public Item SpawnAdaptiveRandomItem()
    {
        UpdateItemProbabilities();
        return SpawnRandom();
    }
    
    protected override void OnRelease(IPoolable poolable)
    {
        PureEffect effect = disappearingEffectSpawner.Spawn();
        effect.transform.position = poolable.transform.position;
    }
}
