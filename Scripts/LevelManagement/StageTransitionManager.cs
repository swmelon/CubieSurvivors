using System;
using UnityEngine;


public class StageTransitionManager : MonoBehaviour
{
    [SerializeField]
    private StageManager stageManager;

    [SerializeField]
    private CutSceneDirector cutSceneDirector;

    // Invoker
    [SerializeField]
    private EventChannelSO startLevelUpEventChannel, finishLevelUpEventChannel;
    
    // Subscriber
    [SerializeField]
    private EventChannelSO maxExpEventChannel, getInitialWeaponEventChannel, runOutFloorTimerEventChannel, 
        runOutOfTimerEventChannel, portalActivatedEventChannel, toEventStageEC;

    [SerializeField]
    private IntEventChannelSO toDiscoveredEventStageEC;

    [SerializeField]
    private EventChannelSO enterStatsUpgradeStageEC, enterItemsUpgradeStageEC;

    [SerializeField]
    private DifficultyCurveManagerSO difficultyCurveManager;
    
    private void OnEnable()
    {
        maxExpEventChannel.Subscribe(LevelUp);
        getInitialWeaponEventChannel.Subscribe(ToNextStage);
        portalActivatedEventChannel.Subscribe(ToNextStage);
        runOutOfTimerEventChannel.Subscribe(ToNextStage);
        runOutFloorTimerEventChannel.Subscribe(ToNextStage);
        toEventStageEC.SubscribeLast(ToEventStage);
        toDiscoveredEventStageEC.SubscribeLast(ToEventStage);
        enterStatsUpgradeStageEC.Subscribe(ToStatsUpgradeStage);
        enterItemsUpgradeStageEC.Subscribe(ToItemsUpgradeStage);
    }

    private void OnDisable()
    {
        maxExpEventChannel.Unsubscribe(LevelUp);
        getInitialWeaponEventChannel.Unsubscribe(ToNextStage);
        portalActivatedEventChannel.Unsubscribe(ToNextStage);
        runOutOfTimerEventChannel.Unsubscribe(ToNextStage);
        runOutFloorTimerEventChannel.Unsubscribe(ToNextStage);
        toEventStageEC.Unsubscribe(ToEventStage);
        toDiscoveredEventStageEC.Unsubscribe(ToEventStage);
        enterStatsUpgradeStageEC.Unsubscribe(ToStatsUpgradeStage);
        enterItemsUpgradeStageEC.Unsubscribe(ToItemsUpgradeStage);
    }
    
    private void LevelUp()
    {
        startLevelUpEventChannel.Raise();
    }

    private void ToNextStage()
    {
        difficultyCurveManager.UpdateCurves();
        finishLevelUpEventChannel.Raise();
    }

    private void ToEventStage()
    {
        finishLevelUpEventChannel.Raise();
    }

    private void ToEventStage(int index)
    {
        finishLevelUpEventChannel.Raise();
    }

    private void ToStatsUpgradeStage()
    {
        stageManager.LoadStatsUpgradeStage();
        ToEventStage();
    }

    private void ToItemsUpgradeStage()
    {
        stageManager.LoadItemsUpgradeStage();
        ToEventStage();
    }
}
