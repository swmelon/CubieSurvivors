using System;
using System.Collections.Generic;
using System.Linq;
using Local.Scripts.Extensions;
using UnityEngine;


[CreateAssetMenu(fileName = "EnemySpawner", menuName = "ScriptableObjects/Spawner/EnemySpawner",
    order = SOAssetMenuIndex.Spawner)]
public class EnemySpawner : MultiSpawnerWithData<Enemy, EnemyData>
{
    [SerializeField]
    private Boss bossPrefab;
    
    [SerializeField]
    private EnemyManagerChannelSO enemyManagerChannel;

    [SerializeField]
    private DifficultyCurveEC difficultyCurveUpdateEC;

    [SerializeField]
    private EventChannelSO defeatBossEC, defeatFinalBossEC, nextStageEC;

    private DifficultyCurveManagerSO DCManager;
    private bool isBossDefeatedAndBeforeNextStage = false;


    protected override void OnEnable()
    {
        base.OnEnable();
        difficultyCurveUpdateEC.Subscribe(OnCurveUpdated, true);
        defeatBossEC.Subscribe(OnDefeatBoss);
        defeatFinalBossEC.Subscribe(OnDefeatBoss);
        nextStageEC.Subscribe(OnNextStage);
    }

    private void OnDisable()
    {
        difficultyCurveUpdateEC.Unsubscribe(OnCurveUpdated);
        defeatBossEC.Unsubscribe(OnDefeatBoss);
        defeatFinalBossEC.Unsubscribe(OnDefeatBoss);
        nextStageEC.Unsubscribe(OnNextStage);
    }

    private void OnCurveUpdated(DifficultyCurveManagerSO difficultyCurveManager)
    {
        DCManager = difficultyCurveManager;
        FlushPool();
    }

    public override Enemy Spawn(EnemyData data, Vector3 spawnPos, Quaternion spawnRot)
    {
        if (!enemyManagerChannel.CanSpawnEnemy())
        {
            return null;
        }

        Enemy enemy = base.Spawn(data, spawnPos, spawnRot);
        enemy.Data = data;
        return enemy;
    }

    public bool TrySpawn(EnemyData enemyData, Vector3 position, Quaternion rotation, out Enemy enemy)
    {
        if (isBossDefeatedAndBeforeNextStage)
        {
            enemy = null;
            return false;
        }

        enemy = Spawn(enemyData, position, rotation);
        return !ReferenceEquals(enemy, null);
    }
    
    public Boss SpawnBoss(EnemyData bossData)
    {
        Boss boss = Instantiate(bossData.Prefab).GetComponent<Boss>();
        // boss가 pool에서 나오는 것이 아니라 직접 생성하기 때문에 호출해야함.
        // 그렇지 않으면 보스의 SetAttackPattern이 호출되지 않음.
        boss.RaiseSpawnEvent();
        boss.Data = bossData;
        return boss;
    }
    
    private Enemy SpawnCurrentPowerEnemy(Vector3 position)
    {
        EnemyData enemyData = GetCurrentPowerEnemy();
        return Spawn(enemyData, position, Quaternion.identity);
    }

    public bool TrySpawnCurrentPowerEnemy(out Enemy enemy, Vector3 position)
    {
        if (isBossDefeatedAndBeforeNextStage)
        {
            enemy = null;
            return false;
        }

        enemy = SpawnCurrentPowerEnemy(position);
        return !ReferenceEquals(enemy, null);
    }

    private EnemyData GetCurrentPowerEnemy()
    {
        float desiredPower = DCManager.GetCurrentMeanEnemyPower();
        
        List<EnemyData> weightedList = new List<EnemyData>();

        foreach (var enemyData in prefabDataList)
        {
            // Calculate weight based on how close the enemy's Power is to the desired Power
            float weight = 1f / Mathf.Abs(enemyData.Power - desiredPower);

            // The number of times this EnemyType will appear in the weighted list
            int repeatCount = Mathf.RoundToInt(weight * 100);

            for (int i = 0; i < repeatCount; i++)
            {
                weightedList.Add(enemyData);
            }
        }

        // Select a random EnemyType based on the weights
        int randomIndex = RandomExtenstion.GetIntInRange(0, weightedList.Count - 1);
        return weightedList[randomIndex];
    }

    public void FlushPool()
    {
        float lowestPower = DCManager.GetCurrentLowestEnemyPower();

        foreach (var enemyData in prefabDataList)
        {
            if (enemyData.Power < lowestPower)
            {
                objectPools[enemyData.Prefab].Clear();
            }
        }
    }

    private void OnDefeatBoss()
    {
        isBossDefeatedAndBeforeNextStage = true;
    }

    private void OnNextStage()
    {
        isBossDefeatedAndBeforeNextStage = false;
    }
}
