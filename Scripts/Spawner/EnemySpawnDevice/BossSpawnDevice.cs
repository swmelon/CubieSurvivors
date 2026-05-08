using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;


public class BossSpawnDevice : LocatableESD
{
    public Boss SpawnBoss(Transform parent, EnemyData data)
    {

        // enemyData 를 사용하지 않음. 속도 및 색상 조절이 필요함.
        Boss boss = enemySpawner.SpawnBoss(data);
        boss.transform.parent = parent;
        boss.transform.position = transform.position + new Vector3(0, enemyData.Scale / 2, 0);
        boss.Data = data;
        boss.SetColor(data.Color);
        Destroy(gameObject);
        return boss;
    }
}
