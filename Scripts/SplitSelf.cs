using System;
using UnityEngine;


[RequireComponent(typeof(Damagable))]
[RequireComponent(typeof(Enemy))]
public class SplitSelf : MonoBehaviour
{
    public Enemy Enemy => enemy;
    public int SplitCount => splitCount;

    [SerializeField]
    private EnemySpawner enemySpawner;

    [SerializeField]
    private EnemyManager enemyManager;
        
    [SerializeField]
    private float splitFactor = 0.7f;

    [SerializeField]
    private int maxSplitCount = 1;

    [SerializeField]
    private int splitPiece = 2;
    
    [SerializeReference]
    private int splitCount = 0;
    private Damagable damagable;
    private Enemy enemy;

    private void Awake()
    {
        damagable = GetComponent<Damagable>();
        damagable.OnDead.AddListener(() => Split());
        enemy = GetComponent<Enemy>();
    }

    private void OnEnable()
    {
        ResetSplitCount();
    }

    private void OnSplit(int currentSplitCount)
    {
        splitCount = currentSplitCount;
        transform.localScale = enemy.Data.Scale * Mathf.Pow(splitFactor, splitCount) * Vector3.one;
        damagable.MaxHealth = (int)(enemy.Data.MaxHealth * Mathf.Pow(splitFactor, splitCount));
        enemy.Weight = enemy.Data.Weight * Mathf.Pow(Mathf.Pow(splitFactor, 3), splitCount);
        enemy.MoveSpeed = enemy.Data.MoveSpeed * Mathf.Pow(1/splitFactor, splitCount);
    }
    
    public void Split()
    {
        splitCount += 1;
        
        if (splitCount > maxSplitCount)
        {
            return;
        }
        
        for(int i = 0; i < splitPiece; i++)
        {
            if (!enemySpawner.TrySpawn(enemy.Data, transform.position, transform.rotation, out Enemy clone))
            {
                return;
            }

            SplitSelf other = clone.GetComponent<SplitSelf>();
            other.enemy.SpawnExpOnDead = enemy.SpawnExpOnDead;
            other.OnSplit(splitCount);
        }
    }

    private void ResetSplitCount()
    {
        splitCount = 0;
    }
}
