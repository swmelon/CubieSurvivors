
using System.Collections.Generic;
using AYellowpaper;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemySpawnDevicePool", menuName = "ScriptableObjects/Pool/EnemySpawnDevicePool",
    order = SOAssetMenuIndex.Enemy)]
public class EnemySpawnDevicePool : MultiPooling<LocatableESD>
{
}
