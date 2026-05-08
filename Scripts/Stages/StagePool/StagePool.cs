

using UnityEngine;

[CreateAssetMenu(fileName = "StagePool", menuName = "ScriptableObjects/Stage/StagePool",
    order = SOAssetMenuIndex.Stage)]
public class StagePool : MultiPoolingWithData<Stage, StageData>
{
    [SerializeField]
    private WorldDirectionChannelSO worldDirectionChannelSo;
    public override Stage Get(StageData data)
    {
        Stage mainStage = base.Get(data);
        mainStage.transform.position = Vector3.zero;
        
        // Set rotation by WorldDirection
        return mainStage;
    }
}
