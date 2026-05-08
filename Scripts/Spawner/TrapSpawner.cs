using UnityEngine;

[CreateAssetMenu(fileName = "TrapSpawner", menuName = "ScriptableObjects/Spawner/TrapSpawner",
    order = SOAssetMenuIndex.Spawner)]
public class TrapSpawner : MultiSpawner<Trap> {}
