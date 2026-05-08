using System.Collections.Generic;
using System.Collections.Specialized;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using static UnityStandardAssets.Utility.TimedObjectActivator;

public class EnemyManager : MonoBehaviour
{
    [Header("Listener")]
    [SerializeField]
    private List<EventChannelSO> releaseAllEvents;

    [SerializeField]
    private List<EventChannelSO> releaseAllWithExpEvents;

    [SerializeField]
    private List<EventChannelSO> releaseAllNoExceptionEvents;

    [Header("Enemy Management")]
    [FormerlySerializedAs("channel")] [FormerlySerializedAs("channelSo")] [SerializeField]
    private EnemyManagerChannelSO enemyManagerChannel;

    [SerializeField]
    private GridSystemChannelSO gridSystemChannel;
    
    [SerializeField][Range(0, 500)]
    private int maxEnemyCount = 200;

    [Header("Update Settings")]
    [SerializeField]
    [Range(1, 50)]
    private int oneFullUpdateFrames = 10;

    [Header("Bucket Settings")]
    [Tooltip("Bucket size is the distance between each bucket.")]
    [SerializeField][Range(0.5f, 1f)]
    private float bucketSize;

    [Header("Grid Settings")]
    [Tooltip("Number of Grids in one side of the stage(square).")]
    [SerializeField]
    private int gridSize = 10;

    [SerializeField]
    private int cellSize = 3;

    [SerializeField]
    private int maxEnemyCountPerCell = 5;

    private GridSystem gridSystem;

    private Dictionary<Transform, DividedHashSet<Enemy>> enemiesWithTarget = new Dictionary<Transform, DividedHashSet<Enemy>>();
    private Dictionary<Transform, (Enemy, float)> nearestEnemies = new Dictionary<Transform, (Enemy, float)>();

    private const int maxDistance = 12;
    private int numBuckets;

    private Dictionary<Transform, HashSet<Enemy>[]> buckets = new Dictionary<Transform, HashSet<Enemy>[]>();
    private Dictionary<Enemy, (Transform, int)> enemyBucketIndexCash = new Dictionary<Enemy, (Transform, int)>();
    
    private List<Enemy> enemiesToRelease = new List<Enemy>();
    private DividedHashSet<Enemy> enemiesWithNoTarget = new DividedHashSet<Enemy>(30);
    private Dictionary<Transform, Transform> substituteWithDict  = new Dictionary<Transform, Transform>();
    private Dictionary<Transform, Transform> substituteForDict= new Dictionary<Transform, Transform>();

    [SerializeReference]
    private int enemyCount;
    private bool isOverflowing = false;
    
    public bool IsOverflowing => isOverflowing;

   
    private bool NoPlayer => enemiesWithTarget.Count == 0;
    private const float enemiesMinHeight = -1.5f;

    private void Awake()
    {
        foreach (var invoker in releaseAllEvents)
        {
            invoker.Subscribe(ReleaseAllEnemyNoExp);
        }

        foreach (var invoker in releaseAllWithExpEvents)
        {
            invoker.Subscribe(ReleaseAllEnemyWithExp);
        }

        foreach (var invoker in releaseAllNoExceptionEvents)
        {
            invoker.Subscribe(ReleaseAllNoException);
        }

        enemyManagerChannel.SetUpChannel(AddEnemy, RemoveEnemy, AddPlayer, RemovePlayer,
            SubstitutePlayerWith, RemoveSubstitute, IsEnemySpawnable);
        gridSystem = new GridSystem(gridSize, cellSize, maxEnemyCountPerCell);
        gridSystemChannel.SetUpChannel(gridSystem);
    }

    private void OnDestroy()
    {
        foreach (var invoker in releaseAllEvents)
        {
            invoker.Unsubscribe(ReleaseAllEnemyNoExp);
        }

        foreach (var invoker in releaseAllWithExpEvents)
        {
            invoker.Unsubscribe(ReleaseAllEnemyWithExp);
        }

        foreach (var invoker in releaseAllNoExceptionEvents)
        {
            invoker.Unsubscribe(ReleaseAllNoException);
        }
    }

    private void FixedUpdate()
    {      
        enemyCount = 0;
        
        foreach (var item in enemiesWithTarget)
        {
            enemyCount += item.Value.Count;
            var target = item.Key;
            var needUpdateEnemies = item.Value;
            bool isSubstitute = false;
            float distance = 0f;
            Transform substitute = null;

            if (substituteForDict.ContainsKey(target))
            { 
                isSubstitute = true;
                substitute = substituteForDict[target];
            }
            
            // calculate distance, find nearest enemy, set enemy's move direction
            foreach (var enemy in needUpdateEnemies.GetNextPiece())
            {
                // check if enemy is valid. normally, it should be valid.
                if (enemy == null || !enemy.gameObject.activeSelf)
                {
                    Debug.LogError("Invalid Enemy. Destroyed or disabled.");
                    continue;
                }

                RemoveEnemyFromBucket(enemy);
                gridSystem.UpdateEnemyPosition(enemy.transform);

                if (isSubstitute)
                {
                    enemy.SetTargetAndUpdate(substitute);
                    distance = (enemy.transform.position - target.position).magnitude;
                }
                else
                {
                    enemy.SetTargetAndUpdate(target);
                    distance = enemy.DistanceToTarget;
                }

                int bucketIndex = GetBucketIndex(distance);
                enemyBucketIndexCash[enemy] = (target, bucketIndex);  
                buckets[target][bucketIndex].Add(enemy);
            }

            float minDistance = GetNearsetEnemyFromBucket(target, out Enemy nearestEnemy);

            if (!ReferenceEquals(nearestEnemy, null))
            {
                nearestEnemies[target] = (nearestEnemy, minDistance);
            }
        }

        isOverflowing = enemyCount >= maxEnemyCount;
        
        if (enemiesWithTarget.Count > 1)
        {
            Debug.Log("More than one player is registered.");
        }
    }
    
    private void InitializeBuckets(Transform newTarget)
    {
        numBuckets = Mathf.CeilToInt(maxDistance / bucketSize);

        // last bucket is for enemies that are out of range.
        buckets[newTarget] = new HashSet<Enemy>[numBuckets + 1];
        
        for (int i = 0; i < numBuckets + 1; i++)
        {
            buckets[newTarget][i] = new HashSet<Enemy>();
        }
    }

    private void RemoveEnemyFromBucket(Enemy enemy)
    {
        if (!enemyBucketIndexCash.ContainsKey(enemy))
        {
            return;
        }

        Transform target = enemyBucketIndexCash[enemy].Item1;
        int bucketIndex = enemyBucketIndexCash[enemy].Item2;
        buckets[target][bucketIndex].Remove(enemy);
        enemyBucketIndexCash.Remove(enemy);
    }

    private bool IsEnemyInTheBucket(Enemy enemy)
    {
        return enemyBucketIndexCash.ContainsKey(enemy);
    }

    private void ClearBuckets()
    {
        foreach (var item in buckets)
        {
            foreach (var bucket in item.Value)
            {
                bucket.Clear();
            }
        }
    }
    
    private int GetBucketIndex(float distance)
    {
        return Mathf.Clamp((int)((distance / maxDistance) * numBuckets), 0, numBuckets);
    }

    public float GetNearsetEnemyFromBucket(Transform target, out Enemy nearsetEnemy)
    {
        float distance = 0;
        nearsetEnemy = null;

        HashSet<Enemy>[] bucketToSearch = buckets[target];

        for (int i = 0; i < bucketToSearch.Length; i++)
        {
            foreach (var enemy in bucketToSearch[i])
            {
                nearsetEnemy = enemy;
                distance = i * bucketSize;
                return distance;
            }
        }

        return distance;
    }

    public int GetEnemiesFromBucketAscendingDistance(Transform target, int numEnemies, out Enemy[] enemies)
    {
        enemies = new Enemy[numEnemies];
        int count = 0;

        HashSet<Enemy>[] bucketToSearch = buckets[target];

        for (int i = 0; i < bucketToSearch.Length; i++)
        {
            foreach (var enemy in bucketToSearch[i])
            {
                enemies[count] = enemy;
                count++;

                if (count >= numEnemies)
                {
                    return count;
                }
            }
        }

        return count;
    }
    
    public int GetEnemiesFromBucketAscendingDistance(Transform target, int numEnemies, float maxRange, out Transform[] enemies)
    {
        enemies = new Transform[numEnemies];
        int count = 0;
        
        foreach(var bucket in buckets[target])
        {
            foreach (var enemy in bucket)
            {
                if (enemy.DistanceToTarget > maxRange)
                {
                    return count;
                }

                enemies[count] = enemy.transform;
                count++;

                if (count >= numEnemies)
                {
                    return count;
                }
            }

        }

        return count;
    }


    private void RecalculateNearestEnemy()
    {
        // if player is more than one, then we need to check if there is any enemy that is closer to another player.
        Dictionary<Transform, (Enemy, float)> newNearestEnemies = new Dictionary<Transform, (Enemy, float)>();

        // Cross check nearest enemy (time complexity : O(n^2))
        foreach (var item1 in nearestEnemies)
        {
            float minDistance = item1.Value.Item2;
            Enemy nearestEnemy = item1.Value.Item1;

            foreach (var item2 in nearestEnemies)
            {
                if (item1.Key == item2.Key || ReferenceEquals(item2.Value.Item1, null))
                {
                    continue;
                }

                float distance = (item1.Key.position - (item2.Value.Item1).transform.position).magnitude;

                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestEnemy = item2.Value.Item1;
                }
            }

            newNearestEnemies[item1.Key] = (nearestEnemy, minDistance);
        }

        nearestEnemies = newNearestEnemies;
    }

    public bool GetNearestEnemy(Transform transform, out Enemy enemy)
    {
        enemy = nearestEnemies[transform].Item1;

        if (ReferenceEquals(enemy, null))
        {
            return false;
        }

        return true;
    }

    public bool GetNearestEnemy(Transform transform, out Enemy enemy, out float distance)
    {
        enemy = nearestEnemies[transform].Item1;
        distance = nearestEnemies[transform].Item2;

        if (ReferenceEquals(enemy, null))
        {
            return false;
        }

        return true;
    }

    public void AddPlayer(Transform playerTransform)
    {
        if (enemiesWithTarget.Count == 0)
        {
            enemiesWithTarget.Add(playerTransform, enemiesWithNoTarget);

            foreach (var enemy in enemiesWithNoTarget)
            {
                if (enemy != null)
                {
                    enemy.SetTargetAndUpdate(playerTransform);
                }
            }

            enemiesWithNoTarget = new DividedHashSet<Enemy>(oneFullUpdateFrames);
        }
        else
        {
            enemiesWithTarget.Add(playerTransform, new DividedHashSet<Enemy>(oneFullUpdateFrames));
        }

        nearestEnemies.Add(playerTransform, (null, float.MaxValue));
        InitializeBuckets(playerTransform);
    }

    public void RemovePlayer(Transform playerTransform)
    {
        if (!enemiesWithTarget.ContainsKey(playerTransform))
        {
            return;
        }

        DividedHashSet<Enemy> enemies = enemiesWithTarget[playerTransform];

        enemiesWithTarget.Remove(playerTransform);
        nearestEnemies.Remove(playerTransform);

        foreach (var enemy in enemies)
        {
            if (enemy != null)
            {
                enemy.Dance();
                enemy.SetTargetAndUpdate(null);
            }

            enemiesWithNoTarget.Add(enemy);
            enemyBucketIndexCash.Remove(enemy);
        }

        buckets.Remove(playerTransform);
    }

    public void SubstitutePlayerWith(Transform substitute, Transform player)
    {
        if (!enemiesWithTarget.ContainsKey(player))
        {
            Debug.LogError("Try to substitute unregistered player");
            return;
        }

        substituteForDict[player] = substitute;
        substituteWithDict[substitute] = player;
    }

    public void RemoveSubstitute(Transform player)
    {
        if (!substituteForDict.ContainsKey(player))
        {
            Debug.LogError("Try to remove not registered substitute");
            return;
        }

        substituteForDict.Remove(player);
    }

    public Transform AddEnemy(Enemy enemy)
    {
        Vector3 enemyPosition = enemy.gameObject.transform.position;

        Transform target = GetNearestTarget(enemyPosition);

        if (ReferenceEquals(target, null))
        {
            return null;
        }

        enemiesWithTarget[target].Add(enemy);
        return target;
    }

    public void RemoveEnemy(Enemy enemy, Transform targetTransform)
    {
        if (ReferenceEquals(targetTransform, null))
        {
            enemiesWithNoTarget.Remove(enemy);
            return;
        }

        // �� Ÿ���� ��ü���̸�
        if (substituteWithDict.ContainsKey(targetTransform))
        {
            // ����
            targetTransform = substituteWithDict[targetTransform];
        }

        gridSystem.RemoveEnemy(enemy.transform);

        if (!enemiesWithTarget.ContainsKey(targetTransform))
        {
            enemiesWithNoTarget.Remove(enemy);
            return;
        }
 
        enemiesWithTarget[targetTransform].Remove(enemy);
        RemoveEnemyFromBucket(enemy);

        if (ReferenceEquals(nearestEnemies[targetTransform].Item1, enemy))
        {
            nearestEnemies[targetTransform] = (null, float.MaxValue);
        }
    }

    public void EnableGravity()
    {
        foreach (var item in enemiesWithTarget)
        {
            foreach (Enemy enemy in item.Value)
            {
                enemy.UseGravity = true;
            }
        }

        CheckAndDestroyInvalidEnemy();
    }

    public void DisableGravity()
    {
        foreach (var item in enemiesWithTarget)
        {
            foreach (Enemy enemy in item.Value)
            {
                enemy.UseGravity = false;
            }
        }
    }
    
    public Transform GetNearestTarget(Vector3 position)
    {
        float minDistance = float.MaxValue;
        Transform target = null;

        foreach (var key in enemiesWithTarget.Keys)
        {
            float distance = (key.position - position).magnitude;

            if (distance < minDistance)
            {
                minDistance = distance;
                target = key;
            }
        }

        return target;
    }

    private void CheckAndDestroyInvalidEnemy()
    {
        foreach (var item in enemiesWithTarget)
        {
            foreach (Enemy enemy in item.Value)
            {
                if (enemy.transform.position.y < enemiesMinHeight)
                {
                    enemiesToRelease.Add(enemy);
                }
            }
        }

        foreach (var enemy in enemiesWithNoTarget)
        {
            if(enemy.transform.position.y < enemiesMinHeight)
            {
                enemiesToRelease.Add(enemy);
            }   
        }

        foreach (Enemy enemy in enemiesToRelease)
        {
            enemy.ForceKill(ignore: false);
        }
        
        enemiesToRelease.Clear();
    }

    private void ReleaseAllEnemyNoExp()
    {
        ReleaseAllEnemy(false);
    }

    private void ReleaseAllEnemyWithExp()
    {
        ReleaseAllEnemy(true);
    }

    private void ReleaseAllNoException()
    {
        ReleaseAllEnemy(false, false);
    }

    private void ReleaseAllEnemy(bool spawnExp, bool ignore=true)
    {
        foreach (var item in enemiesWithTarget)
        {
            foreach (Enemy enemy in item.Value)
            {
                enemiesToRelease.Add(enemy);
            }
        }
        
        foreach (Enemy enemy in enemiesToRelease)
        {
            if (!enemy.HasTarget())
            {

            }
            enemy.ForceKill(spawnExp, ignore);
        }
        
        enemiesToRelease.Clear();

        foreach (var item in buckets)
        {
            foreach (var bucket in item.Value)
            {
                bucket.Clear();
            }
        }

        gridSystem.RemoveAllEnemies();
        
        foreach (var target in enemiesWithTarget.Keys)
        {
            nearestEnemies[target] = (null, float.MaxValue);
        }
    }
    public bool IsEnemySpawnable() => !(isOverflowing || NoPlayer);

}
