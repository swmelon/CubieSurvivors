using UnityEngine;

[CreateAssetMenu(fileName = "StageAdapterPool", menuName = "ScriptableObjects/Stage/StageAdapterPool", order = SOAssetMenuIndex.Stage)]
public class StageAdapterPool : MultiPoolingWithData<StageAdapter, StageAdapterData>{}