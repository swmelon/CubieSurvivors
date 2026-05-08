
using System;
using System.Diagnostics.Tracing;
using UnityEngine;

public class UpgradeScreenController : MonoBehaviour
{
    [SerializeField]
    private EventChannelSO enterStatsUpgradeStageChannel;

    [SerializeField]
    private EventChannelSO enterItemsUpgradeStageChannel;

    [SerializeField]
    private UpgradeScreen upgradeScreen;


    private void OnEnable()
    {
        upgradeScreen.StatsButtonClicked += OnClickStatsBtn;
        upgradeScreen.ItemsButtonClicked += OnClickItemsBtn;
    }
    
    private void OnDisable()
    {
        upgradeScreen.StatsButtonClicked -= OnClickStatsBtn;
        upgradeScreen.ItemsButtonClicked -= OnClickItemsBtn;
    }

    private void OnClickStatsBtn()
    {
        enterStatsUpgradeStageChannel.Raise();
    }

    private void OnClickItemsBtn()
    {
        enterItemsUpgradeStageChannel.Raise();
    }
}
