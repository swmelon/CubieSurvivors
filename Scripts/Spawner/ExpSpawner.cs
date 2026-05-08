using System;
using UnityEngine;
using UnityEngine.Serialization;


[CreateAssetMenu(fileName = "ExpSpawner", menuName = "ScriptableObjects/Spawner/ExpSpawner", order = SOAssetMenuIndex.Spawner)]
public class ExpSpawner : MultiSpawner<Exp, EnemyData>
{
    [SerializeField]
    private OnePureEffectSpawner disappearingEffectSpawner;
    
    protected override void OnRelease(IPoolable poolable)
    {
        PureEffect effect = disappearingEffectSpawner.Spawn();
        effect.transform.position = poolable.transform.position;
    }
}
