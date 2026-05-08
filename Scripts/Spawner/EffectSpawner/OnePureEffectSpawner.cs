using UnityEngine;

[CreateAssetMenu(fileName = "OnePureEffectSpawner", menuName = "ScriptableObjects/Spawner/OnePureEffectSpawner", order = SOAssetMenuIndex.Spawner)]
public class OnePureEffectSpawner : Spawner<PureEffect> 
{

    public override void Initialize()
    {
        base.Initialize();
        rotation = poolablePrefab.transform.rotation;
    }
}
