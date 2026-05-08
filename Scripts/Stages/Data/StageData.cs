
using System;
using UnityEngine;

[CreateAssetMenu(fileName = "MainStageData", menuName = "ScriptableObjects/Stage/MainStageData", order = SOAssetMenuIndex.Stage)]
public class StageData : PrefabDataSO<Stage>
{
    public float StageInterval = 18f;
}
