using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Local.Scripts.Extensions;
using UnityEngine;

public class BPSpawnAlly : BPByBossHealth
{ 
    [SerializeField]
    private EnemyBoxSpawner enemyBoxSpawner;

    [SerializeField]
    private FloorGeoDataChannel floorGeoDataChannel;

    private const float boxSize = 1f;
    private const float activationDelay = 1f;
    private const int minAllyDrop = 1;
    private const int maxAllyDrop = 2;
    private const float minSpawnInterval = 0.1f;
    private const float maxSpawnInterval = 5f;
    private IEnumerator bossPattern;
    private bool stop = false;

    protected override IEnumerator StartBehaviourRoutine()
    {
        enemyAnimationController.Shake();
        enemyAnimationController.GetAngry();
        yield return new WaitForSeconds(activationDelay);
        OnActivated();
        OnFinishedBehaviour();
    }

    protected void OnActivated()
    {
        if (!user.TryGetTarget(out Transform target))
        {
            return;
        } 

        bossPattern = SpawnAlly(new[] { user.transform, target.transform});
        StartCoroutine(bossPattern);
    }

    private IEnumerator SpawnAlly(Transform[] playerTransforms)
    {
        Vector3[] playerPositions = new Vector3[playerTransforms.Length];

        while (true)
        {  

            // 왜 코루틴이 안멈추는지 모르겠지만, 임시방편으로 이렇게 해봄
            if (stop)
            {
                break;
            }

            for (int i = 0; i < playerTransforms.Length; i++)
            {
                playerPositions[i] = playerTransforms[i].position;
            }

            // numDrop : 한 번에 떨어뜨리는 박스의 개수
            DropEnemyBox(boxSize, RandomExtenstion.GetIntInRange(minAllyDrop, maxAllyDrop), playerPositions);

            yield return new WaitForSeconds(RandomExtenstion.GetFloatInRange(minSpawnInterval, maxSpawnInterval));
        }
    }

    public void DropEnemyBox(float boxSize, int numDrops, Vector3[] playerPositions)
    {
        if (stop == true)
        {
            return;
        }
        floorGeoDataChannel.LocateItemAround(playerPositions, boxSize, numDrops, out List<Vector3> locations);

        foreach (Vector3 location in locations)
        {
            enemyBoxSpawner.Spawn().Drop(location);
        }
    }

    public override void StopAction()
    {
        if (!ReferenceEquals(bossPattern, null))
        {
            StopCoroutine(bossPattern);
        }

        stop = true;
        base.StopAction();
    }
}
