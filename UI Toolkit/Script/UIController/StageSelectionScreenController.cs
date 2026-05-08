
using System;
using UnityEngine;

public class StageSelectionScreenController : MonoBehaviour
{
    [SerializeField]
    private StageSelectionScreen stageSelectionScreen;

    [SerializeField]
    private CharSelectionScreenController charSelectionScreenController;

    [SerializeField]
    private SaveLoadManagerSO saveLoadManager;

    [SerializeField]
    private GameScenarioManagerSO gameScenarioManager;

    [SerializeField]
    private EventStageManager rewardStageManager;

    [SerializeField]
    private IntEventChannelSO loadRewardStageEC;

    [Header("Confirm and Ads")]

    [SerializeField]
    private PopupScreenController popupScreenController;

    [SerializeField]
    private GameProcessManager gameStartupManager;

    [SerializeField]
    private InterstitialAd interstitialAd;

    [SerializeField]
    private LifeManager lifeManager;

    private enum Tab
    {
        Battle,
        Reward,
    }

    private SaveFile saveFile;

    private Tab currentTab = Tab.Battle;
    private Difficulty currentDifficulty = Difficulty.Normal;

    private ScenarioData currentScenarioData;
    private EventStageData currentRewardStageData;
    private int currentRewardStageIndex;

    private void OnEnable()
    {
        saveFile = saveLoadManager.SaveFile;
        stageSelectionScreen.BackButtonClicked += OnBackButtonClicked;
        stageSelectionScreen.BattleButtonClicked += OnBattleButtonClicked;
        stageSelectionScreen.RewardButtonClicked += OnRewardButtonClicked;
        stageSelectionScreen.LeftButtonClicked += OnLeftButtonClicked;
        stageSelectionScreen.RightButtonClicked += OnRightButtonClicked;
        stageSelectionScreen.NormalButtonClicked += OnNormalButtonClicked;
        stageSelectionScreen.HardButtonClicked += OnHardButtonClicked;
        stageSelectionScreen.HellButtonClicked += OnHellButtonClicked;
        stageSelectionScreen.StartButtonClicked += OnStartButtonClicked;
    }
    
    private void OnDisable()
    {
        stageSelectionScreen.BackButtonClicked -= OnBackButtonClicked;
        stageSelectionScreen.BattleButtonClicked -= OnBattleButtonClicked;
        stageSelectionScreen.RewardButtonClicked -= OnRewardButtonClicked;
        stageSelectionScreen.LeftButtonClicked -= OnLeftButtonClicked;
        stageSelectionScreen.RightButtonClicked -= OnRightButtonClicked;
        stageSelectionScreen.NormalButtonClicked -= OnNormalButtonClicked;
        stageSelectionScreen.HardButtonClicked -= OnHardButtonClicked;
        stageSelectionScreen.HellButtonClicked -= OnHellButtonClicked;
        stageSelectionScreen.StartButtonClicked -= OnStartButtonClicked;
    }

    public void ShowStageSelectionScreen()
    {
        stageSelectionScreen.ShowScreen();

        ScenarioData scenarioData = gameScenarioManager.GetScenarioData();
        stageSelectionScreen.ShowScenario(scenarioData);
        currentScenarioData = scenarioData;
        OnBattleButtonClicked();
    }

    private void OnBackButtonClicked()
    {
        stageSelectionScreen.HideScreen();
        charSelectionScreenController.ShowCharSelectionScreen();
    }

    private void OnBattleButtonClicked()
    {
        if (currentTab == Tab.Battle)
        {
            SetCurrentMaxDifficulty(currentScenarioData);
            return;
        }

        currentTab = Tab.Battle;
        stageSelectionScreen.ActivateBattleTab();
        SetCurrentMaxDifficulty(currentScenarioData);
        stageSelectionScreen.ShowScenario(currentScenarioData);
        stageSelectionScreen.HandleButtonClickPositiveSFX();
    }

    private void OnRewardButtonClicked()
    {
        if (currentTab == Tab.Reward)
        {
            stageSelectionScreen.HandleButtonClickPositiveSFX();
            return;
        }

        if (!rewardStageManager.AtLeastOneStageIsUndiscovered())
        {
            stageSelectionScreen.HandleButtonClickNegativeSFX();
            return;
        }

        if (currentRewardStageData == null)
        {
            currentRewardStageData = rewardStageManager.GetData(0);
        }

        currentTab = Tab.Reward;
        currentDifficulty = Difficulty.Normal;
        stageSelectionScreen.ActivateRewardTab();
        stageSelectionScreen.ShowRewardStage(currentRewardStageData);
        stageSelectionScreen.HandleButtonClickPositiveSFX();
    }

    private void OnLeftButtonClicked()
    {
        switch (currentTab)
        {
            case Tab.Battle:
                if (!gameScenarioManager.TryGetPrevScenarioData(out ScenarioData scenarioData))
                {
                    stageSelectionScreen.HandleButtonClickNegativeSFX();
                    return;
                }

                stageSelectionScreen.ShowScenario(scenarioData);
                currentScenarioData = scenarioData;
                SetCurrentMaxDifficulty(currentScenarioData);
                stageSelectionScreen.HandleButtonClickPositiveSFX();
                break;
            case Tab.Reward:

                if (!rewardStageManager.TryGetPrevData(currentRewardStageIndex, out EventStageData data))
                {
                    stageSelectionScreen.HandleButtonClickNegativeSFX();
                    return;
                }

                currentRewardStageData = data;
                currentRewardStageIndex--;
                stageSelectionScreen.ShowRewardStage(currentRewardStageData);
                stageSelectionScreen.HandleButtonClickPositiveSFX();
                break;
        }
    }

    private void OnRightButtonClicked()
    {
        switch (currentTab)
        {
            case Tab.Battle:
                if (!gameScenarioManager.TryGetNextScenarioData(out ScenarioData scenarioData))
                {
                    stageSelectionScreen.HandleButtonClickNegativeSFX();
                    return;
                }

                stageSelectionScreen.ShowScenario(scenarioData);
                currentScenarioData = scenarioData;
                SetCurrentMaxDifficulty(currentScenarioData);
                stageSelectionScreen.HandleButtonClickPositiveSFX();
                break;
            case Tab.Reward:

                if (!rewardStageManager.TryGetNextData(currentRewardStageIndex, out EventStageData data))
                {
                    stageSelectionScreen.HandleButtonClickNegativeSFX();
                    return;
                }

                currentRewardStageData = data;
                currentRewardStageIndex++;
                stageSelectionScreen.ShowRewardStage(currentRewardStageData);
                stageSelectionScreen.HandleButtonClickPositiveSFX();
                break;
        }
    }

    private void OnNormalButtonClicked()
    {
        if (currentScenarioData.maxDifficulty < Difficulty.Normal)
        {
            stageSelectionScreen.HandleButtonClickNegativeSFX();
            return;
        }

        stageSelectionScreen.ShowHighestScore(currentScenarioData.maxScore);


        if (currentDifficulty == Difficulty.Normal)
        {
            stageSelectionScreen.HandleButtonClickPositiveSFX();
            return;
        }

        currentDifficulty = Difficulty.Normal;
        stageSelectionScreen.SelectNormalButton();
        stageSelectionScreen.HandleButtonClickPositiveSFX();
    }

    private void OnHardButtonClicked()
    {
        if (currentScenarioData.maxDifficulty < Difficulty.Hard)
        {
            stageSelectionScreen.HandleButtonClickNegativeSFX();
            return;
        }

        stageSelectionScreen.ShowHighestScore(currentScenarioData.maxScoreHard);

        if (currentDifficulty == Difficulty.Hard)
        {
            stageSelectionScreen.HandleButtonClickPositiveSFX();
            return;
        }

        
        currentDifficulty = Difficulty.Hard;
        stageSelectionScreen.SelectHardButton();
        stageSelectionScreen.HandleButtonClickPositiveSFX();
    }

    private void OnHellButtonClicked()
    {
        if (currentScenarioData.maxDifficulty < Difficulty.Hell)
        {
            stageSelectionScreen.HandleButtonClickNegativeSFX();
            return;
        }

        stageSelectionScreen.ShowHighestScore(currentScenarioData.maxScoreHell);


        if (currentDifficulty == Difficulty.Hell)
        {
            stageSelectionScreen.HandleButtonClickPositiveSFX();
            return;
        }

        currentDifficulty = Difficulty.Hell;
        stageSelectionScreen.SelectHellButton();
        stageSelectionScreen.HandleButtonClickPositiveSFX();
    }

    private void OnStartButtonClicked()
    {
        StartGame();
    }

    /// <summary>
    /// 
    /// 일단 게임 시작 전에 광고를 보지 않는 것으로.
    /// </summary>

    private void OnConfirmButtonClicked()
    {
        if (interstitialAd.Loadable())
        {
            popupScreenController.ShowPopupScreen(UIText.WATCH_ADS_FOR_FREE_LIFE,
                UIText.YES, UIText.NO, OnWatchAdsForFreeLife, StartGame);
        }
        else
        {
            StartGame();
        }

        stageSelectionScreen.HandleButtonClickPositiveSFX();
    }

    private void OnWatchAdsForFreeLife()
    {
        interstitialAd.TryLoadAndShow(StartGameAfterWatchAD, StartGame);
    }

    private void StartGameAfterWatchAD()
    {
        interstitialAd.AdsShowCompleted -= StartGameAfterWatchAD;
        lifeManager.AddLife();
        StartGame();
    }

    private void StartGame()
    {
        stageSelectionScreen.HideScreen();

        switch (currentTab)
        {
            case Tab.Battle:
                gameScenarioManager.LoadScenario(currentScenarioData, currentDifficulty);
                gameStartupManager.StartGame();
                break;
            case Tab.Reward:
                loadRewardStageEC.Raise(currentRewardStageIndex);
                break;
        }
    }

    private void SetCurrentMaxDifficulty(ScenarioData data)
    {
        currentDifficulty = data.maxDifficulty;

        switch (currentDifficulty)
        {
            case Difficulty.Normal:
                stageSelectionScreen.SelectNormalButton();
                stageSelectionScreen.ShowHighestScore(data.maxScore);
                break;
            case Difficulty.Hard:
                stageSelectionScreen.SelectHardButton();
                stageSelectionScreen.ShowHighestScore(data.maxScoreHard);
                break;
            case Difficulty.Hell:
                stageSelectionScreen.SelectHellButton();
                stageSelectionScreen.ShowHighestScore(data.maxScoreHell);
                break;
        }
    }
}
