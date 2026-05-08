using System;
using System.Runtime.CompilerServices;
using Local.Scripts.Extensions;
using UnityEngine;


[CreateAssetMenu(fileName = "DeathManager", menuName = "ScriptableObjects/DeathManager", order = SOAssetMenuIndex.Manager)]
public class DeathManager : ScriptableObject
{
    public event Action<int> DeathCountChanged;

    [SerializeField]
    private ExpSpawner expSpawner;

    [SerializeField]
    private CoinSpawner coinSpawner;

    [SerializeField]
    private float defaultCoinSpawnProbability = 0.1f;

    [SerializeField]
    private AuraniumSpawner auraniumSpawner;
    
    [SerializeField]
    private float defaultAurainumSpawnProbability = 0.1f;
    
    [SerializeField]
    private OnePureEffectSpawner explosionEffectSpawner;
    
    [SerializeField]
    private OnePureEffectSpawner itemBoxCrashSpawner;

    [SerializeField]
    private GameScenarioManagerSO gameScenarioManager;

    [SerializeField]
    private IntChannelSO bossCountChannel;

    // Subscriber
    [SerializeField]
    private EventChannelSO restartGameEventChannel;


    private const float spawnOffsetDiagonal = 0.5f;
    private const float spawnOffsetCardinal = 0.7f;
    private const float luckProbabilityFactor = 0.01f;
    private const float itemBoxCrashHeightOffset = 0.5f;

    private int deathCount;
    private Vector3[] spawnPositionOffset = new Vector3[8]
    {
        new Vector3(spawnOffsetDiagonal, 0, spawnOffsetDiagonal),
        new Vector3(-spawnOffsetDiagonal, 0, spawnOffsetDiagonal),
        new Vector3(spawnOffsetDiagonal, 0, -spawnOffsetDiagonal),
        new Vector3(-spawnOffsetDiagonal, 0, -spawnOffsetDiagonal),
        new Vector3(spawnOffsetCardinal, 0, 0),
        new Vector3(-spawnOffsetCardinal, 0, 0),
        new Vector3(0, 0, spawnOffsetCardinal),
        new Vector3(0, 0, -spawnOffsetCardinal),
    };
    private int index = 0;
    private float coinSpawnProbability = 0.1f;
    private float aurainumSpawnProbability = 0.1f;

    private int bossCount = 0;

    private void OnEnable()
    {
        Debug.Log("EnemyDeathManager is enabled.");
        restartGameEventChannel.Subscribe(ResetDeathCount);
    }

    private void OnDisable()
    {
        restartGameEventChannel.Unsubscribe(ResetDeathCount);
    }

    public void OnEnemyDead(Enemy enemy, bool spawnExp = true)
    {
        if (spawnExp)
        {
            expSpawner.Spawn(enemy.Data, enemy.transform.position);
            
            if (RandomExtenstion.IsHappen(aurainumSpawnProbability))
            {
                auraniumSpawner.SpawnRandom().transform.position = enemy.transform.position;
            }

            if (RandomExtenstion.IsHappen(coinSpawnProbability))
            {
                coinSpawner.Spawn().transform.position = enemy.transform.position + spawnPositionOffset[index];
                index = (index + 1) % spawnPositionOffset.Length;
            }

            deathCount++;
            DeathCountChanged?.Invoke(deathCount);
        }
        
        SpawnExplosionEffect(enemy.transform);
    }
    
    public void OnBossDead(Boss boss)
    {
        SetBossCount(bossCount + 1);
        SpawnExplosionEffect(boss.transform);
        gameScenarioManager.DefeatBoss();
    }
    
    public void OnItemBoxCrashed(ItemBox itemBox)
    {
        Transform crashEffectTransform = itemBoxCrashSpawner.Spawn().transform;
        crashEffectTransform.localScale = itemBox.transform.localScale;
        crashEffectTransform.SetPositionAndRotation(itemBox.transform.position - Vector3.up * itemBoxCrashHeightOffset, itemBox.transform.rotation);
    }

    public void SpawnExplosionEffect(Transform deadTransform)
    {
        Transform effectTransform = explosionEffectSpawner.Spawn().transform;

        effectTransform.SetLocalPositionAndRotation(deadTransform.position, deadTransform.localRotation);
    }  

    public void OnAllyDead(Transform deadTrasnfrom)
    {
        SpawnExplosionEffect(deadTrasnfrom);
    }

    private void ResetDeathCount()
    {
        bossCount = 0;
        deathCount = 0;
        DeathCountChanged?.Invoke(deathCount);
    }
    
    public void SetLuckStat(int luck)
    {
        coinSpawnProbability = defaultCoinSpawnProbability + luckProbabilityFactor * luck;
        aurainumSpawnProbability = defaultAurainumSpawnProbability + luckProbabilityFactor * luck;
    }

    private void SetBossCount(int bossCount)
    {
        this.bossCount = bossCount;
        bossCountChannel.Register(bossCount);
    }
}
