using System;
using TMPro;
using UnityEngine;

public class TextInfoController : MonoBehaviour
{ 
    [SerializeField] 
    private DifficultyCurveEC curveUpdateEventChannel;
    
    [SerializeField]
    private EventChannelSO playerDeathEventChannel, finishLevelUpEventChannel;

    [SerializeField]
    private StringEventChannelSO drawStageNumberEventChannel;
    
    private TextMeshProUGUI levelText;
    private int playerDeathCount = 0;
    private int level = 0;
    
    private void Awake()
    {
        levelText = GetComponent<TextMeshProUGUI>();
        curveUpdateEventChannel.Subscribe(OnUpdateCurve);
        levelText.text = "";
        playerDeathEventChannel.Subscribe(() => playerDeathCount++);
        finishLevelUpEventChannel.SubscribeLast(DrawInfo);
    }
    
    private void OnUpdateCurve(DifficultyCurveManagerSO difficultyCurveManager)
    {
        level = difficultyCurveManager.GetLevel();
        
        levelText.text = "PPR : " + difficultyCurveManager.GetPPR() + " GetGrade : " + level
                         + " MaxExp : " + difficultyCurveManager.GetMaxExp() + " MinDevices : " + difficultyCurveManager.GetMinEnemySpawnDevices()
                         + " MaxDevices : " + difficultyCurveManager.GetMaxEnemySpawnDevices() + " playerDeathCount : " + playerDeathCount;
    }

    private void DrawInfo()
    {
        drawStageNumberEventChannel.Raise(level.ToString());
    }
}
