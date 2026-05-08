
using System;
using System.Runtime.CompilerServices;
using UnityEngine;


[CreateAssetMenu(fileName = "EnemyManagerChannel", menuName = "ScriptableObjects/Channels/EnemyManagerChannel", order = SOAssetMenuIndex.Channel)]
public class EnemyManagerChannelSO : ScriptableObject
{
    private Func<Enemy, Transform> OnEnemySubscribe;
    private Action<Transform> OnPlayerSubscribe, OnPlayerUnsubscribe;
    private Action<Enemy, Transform> OnUnsubscribe;
    private Action<Transform, Transform> OnSubstitute;
    private Action<Transform> OnRemoveSubstitute;
    private Func<bool> OnTrySpawnEnemy;

    
    public void SetUpChannel(Func<Enemy, Transform> OnEnemySubscribe, Action<Enemy, Transform> OnUnsubscribe, 
        Action<Transform> OnPlayerSubscribe, Action<Transform> OnPlayerUnsubscribe,
        Action<Transform, Transform> OnSubstitute, Action<Transform> OnRemoveSubstitute,
        Func<bool> OnTrySpawnEnemy)
    {
        this.OnEnemySubscribe = OnEnemySubscribe;
        this.OnUnsubscribe = OnUnsubscribe;
        this.OnPlayerSubscribe = OnPlayerSubscribe;
        this.OnPlayerUnsubscribe = OnPlayerUnsubscribe;
        this.OnSubstitute = OnSubstitute;
        this.OnRemoveSubstitute = OnRemoveSubstitute;
        this.OnTrySpawnEnemy = OnTrySpawnEnemy;
    }
    
    public Transform Subscribe(Enemy enemy)
    {
       return OnEnemySubscribe.Invoke(enemy);
    }

    public bool TrySubscribe(Enemy enemy, out Transform targetTransform)
    {
        targetTransform = OnEnemySubscribe.Invoke(enemy);
        return targetTransform != null;
    }

    public void Unsubscribe(Enemy enemy, Transform targetTransform)
    {
        OnUnsubscribe.Invoke(enemy, targetTransform);
    }
    
    public void SubScribe(Transform player)
    {
        OnPlayerSubscribe.Invoke(player);
    }
    
    public void Unsubscribe(Transform player)
    {
        OnPlayerUnsubscribe.Invoke(player);
    }

    public void Substitute(Transform substitute, Transform player)
    {
        OnSubstitute.Invoke(substitute, player);
    }

    public void RemoveSubstitute(Transform player)
    {
        OnRemoveSubstitute.Invoke(player);
    }

    public bool CanSpawnEnemy()
    {
        return OnTrySpawnEnemy.Invoke();
    }
}
