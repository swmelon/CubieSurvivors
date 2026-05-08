
using System.Collections.Generic;
using Local.Scripts.Extensions;
using UnityEngine;


[CreateAssetMenu(fileName = "AuraniumSpawner", menuName = "ScriptableObjects/Spawner/AuraniumSpawner", order = SOAssetMenuIndex.Spawner)]
public class AuraniumSpawner : MultiSpawner<Auranium>
{
    [SerializeField] 
    private List<float> probabilities;
    
    private float totalProbability = 0f;
    
    protected override void OnEnable()
    {
        totalProbability = 0f;
        base.OnEnable();
        
        foreach (float probability in probabilities)
        {
            totalProbability += probability;
        }
    }
    
    public override Auranium SpawnRandom()
    {
        float randomNumber = RandomExtenstion.GetFloatInRange(0, totalProbability);
        float cumulativeProbability = 0;
        
        for (int i = 0; i < probabilities.Count; i++)
        {
            cumulativeProbability += probabilities[i];
            if (randomNumber <= cumulativeProbability)
            {
                // Spawn the gem and return it
                return Get(prefabList[i]);
            }
        }

        Debug.LogError("AuraniumSpawner: Something went wrong while spawning a gem!");
        return default;
    }
    
}
