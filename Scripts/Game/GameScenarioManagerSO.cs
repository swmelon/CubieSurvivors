using Local.Scripts.Extensions;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


[CreateAssetMenu(fileName = "GameScenarioManager", menuName = "ScriptableObjects/Game/GameScenarioManager")]
public class GameScenarioManagerSO : ScriptableObject, IDependentInitialization
{
    public event Action<GameScenario> OnScenarioLoaded;

    [SerializeField]
    private GameScenario mainMenuScenario;

    [SerializeField]
    private SaveLoadManagerSO saveLoadManager;

    [SerializeField]
    private PermanentUpgradableStat permanentUpgradableStat;

    [SerializeField]
    private List<GameScenario> scenarios;

    [Header("Invoke")]
    [SerializeField]
    private EventChannelSO completeScenarioFirstTimeEC;

    [SerializeField]
    private EventChannelSO enterStatsUpgradeStageEC, enterItemsUpgradeStageEC;

    [SerializeField]
    private EventChannelSO defeatBossEC;

    [SerializeField]
    private EventChannelSO defeatFinalBossEC;

    [Header("Listen")]
    [SerializeField]
    private EventChannelSO endBossDefeatCutSceneEC;

    [SerializeField]
    private EventChannelSO loadScenraioEC, unloadScenarioEC;

    [SerializeField]
    private EventChannelSO exitEventStageEC;

    [SerializeField]
    private LongEventChannelSO updateTotalScoreEC;

    [SerializeField]
    [Range(0.5f, 1f)]
    private float goToItemsUpgradeStageProbability = 0.9f;

    private int index = 0;
    private int unlockedScenarioCount = 0;
    private bool completeScenarioFirstTime = false;
    private SaveFile saveFile;
    private GameScenario currentScenario => scenarios[index];

    public int AvailableScenarioCount => Mathf.Min(unlockedScenarioCount, scenarios.Count);
    public void Initialize()
    {
        mainMenuScenario.LoadRandomTheme();
        saveFile = saveLoadManager.SaveFile;
        unlockedScenarioCount = 0;
        completeScenarioFirstTime = false;

        foreach (var unlocked in saveFile.scenarioUnlocked)
        {
            int index = unlocked.Key;
            Difficulty MaxDifficultty = (Difficulty)unlocked.Value;

            if (index >= scenarios.Count)
            {
                Debug.LogWarning("Scenario is unlocked but No more scenario.");
                break;
            }

            scenarios[index].SetMaxDifficulty(MaxDifficultty);
            unlockedScenarioCount++;
        }

        long[] maxScore = new long[3];
        Dictionary<int, long> scenarioMaxScore = saveFile.scenarioMaxScore;

        for (int i = 0; i < scenarios.Count; i++)
        {
            int idNormal = GetScoreID(i, Difficulty.Normal);
            int idHard = GetScoreID(i, Difficulty.Hard);
            int idHell = GetScoreID(i, Difficulty.Hell);

            if (scenarioMaxScore.TryGetValue(idNormal, out long scoreNormal))
            {
                maxScore[0] = scoreNormal;
            }
            else
            {
                maxScore[0] = 0;
            }

            if (scenarioMaxScore.TryGetValue(idHard, out long scoreHard))
            {
                maxScore[1] = scoreHard;
            }
            else
            {
                maxScore[1] = 0;
            }

            if (scenarioMaxScore.TryGetValue(idHell, out long scoreHell))
            {
                maxScore[2] = scoreHell;
            }
            else
            {
                maxScore[2] = 0;
            }

            scenarios[i].SetMaxScore(maxScore);
        }

        if (saveFile.LastSelectedScenarioIndex >= scenarios.Count)
        {
            saveFile.LastSelectedScenarioIndex = scenarios.Count - 1;
        }

        index = saveFile.LastSelectedScenarioIndex;
    }

    public GameScenario GetCurrentScenario()
    {
        Debug.Log("Current Scenario index: " + index + " ");
        return scenarios[index];
    }


    private void OnEnable()
    {
        updateTotalScoreEC.Subscribe(OnTotalScoreUpdated);
        exitEventStageEC.Subscribe(OnExitEventStage);
        endBossDefeatCutSceneEC.SubscribeLast(OnBossDefeatCutSceneEnded);
    }

    private void OnDisable()
    {
        updateTotalScoreEC.Unsubscribe(OnTotalScoreUpdated);
        exitEventStageEC.Unsubscribe(OnExitEventStage);
        endBossDefeatCutSceneEC.Unsubscribe(OnBossDefeatCutSceneEnded);
    }


    public ScenarioData GetScenarioData()
    {
        return scenarios[index].Data;
    }



    public bool TryGetNextScenarioData(out ScenarioData scenarioData)
    {
        return TryGetScenarioData(1, out scenarioData);
    }

    public bool TryGetPrevScenarioData(out ScenarioData scenarioData)
    {
        return TryGetScenarioData(-1, out scenarioData);
    }

    private bool TryGetScenarioData(int move, out ScenarioData scenarioData)
    {
        index += move;
        bool result = false;

        if (index < 0)
        {
            index = 0;
        }
        else if (AvailableScenarioCount == 0)
        {
            Debug.LogError("At least one scenario must be available.");
            index = 0;
        }
        else if (index >= AvailableScenarioCount)
        {
            index = AvailableScenarioCount - 1;
        }
        else
        {
            result = true;
        }

        scenarioData = scenarios[index].Data;
        return result;
    }

    public void LoadScenario(int index, Difficulty difficulty)
    {
        if (index >= scenarios.Count)
        {
            Debug.LogWarning("Scenario index is out of range.");
            return;
        }

        if (scenarios[index].MaxDifficulty < difficulty)
        {
            Debug.LogWarning("Difficulty " + (int)difficulty + " of " + scenarios[index].Data.name + "  is not unlocked yet. Lower difficulty");
            difficulty = scenarios[index].MaxDifficulty;
        }

        this.index = index;

        mainMenuScenario.Unload();
        unloadScenarioEC.Raise();
        currentScenario.Load(difficulty);
        saveFile.LastSelectedScenarioIndex = index;

        saveLoadManager.Save();

        OnScenarioLoaded?.Invoke(currentScenario);
        loadScenraioEC.Raise();
    }

    public void LoadScenario(ScenarioData data, Difficulty difficulty)
    {
        foreach (var scenario in scenarios)
        {
            if (data.name != default && scenario.Data.name == data.name)
            {
                LoadScenario(scenarios.IndexOf(scenario), difficulty);
                return;
            }
        }
    }

    public bool TryUnlockScenario(int scenarioIndex)
    {
        // �ó������� ĳ���Ϳ� �ٸ��� ������� �ر��� �����
        // ���� ���ο� �ó������� �ر��� ���ΰ�?
        // -> ���� �ó������� � ���̵��� �� �� Ŭ���� �ϸ�.
        // ���̵��� ��� �ر��� ���ΰ�?
        // -> ���� ���̵� Ŭ�����ϸ� ���� ���̵��� �رݵ�.

        // ���ο� �ó������� �رݵǸ�
        // �̺�Ʈ ���������� �ر�

        if (saveFile.scenarioUnlocked.ContainsKey(scenarioIndex))
        {
            Debug.Log("Scenario is already unlocked.");
            return false;
        }

        saveFile.scenarioUnlocked[scenarioIndex] = (int)Difficulty.Normal;
        saveFile.LastSelectedScenarioIndex = scenarioIndex;
        unlockedScenarioCount++;
        saveLoadManager.Save();
        return true;
    }

    public bool TryUnlockDifficulty(int scenarioIndex, Difficulty difficulty)
    {
        if (!saveFile.scenarioUnlocked.TryGetValue(scenarioIndex, out int maxDifficulty))
        {
            Debug.LogWarning("Scenario is not unlocked yet.");
            return false;
        }

        if (maxDifficulty < (int)difficulty)
        {
            saveFile.scenarioUnlocked[scenarioIndex] = (int)difficulty;
            scenarios[scenarioIndex].SetMaxDifficulty(difficulty);
            saveLoadManager.Save();
            return true;
        }

        return false;
    }

    public void DefeatBoss()
    {
        GameScenario scenario = scenarios[index];
        scenario.OnDefeatBoss();
        saveLoadManager.Save();

        if (scenario.DefeatFinalBoss)
        {
            OnDefeatFinalBoss();
            defeatFinalBossEC.Raise();
        }
        else
        {
            defeatBossEC.Raise();
        }
    }

    private void OnDefeatFinalBoss()
    {
        Difficulty clearedDifficulty = GetCurrentScenario().CurrentDifficulty;

        switch (clearedDifficulty)
        {
            case Difficulty.Normal:
                if (TryUnlockDifficulty(index, Difficulty.Hard))
                {
                    TryUnlockScenario(index + 1);
                    completeScenarioFirstTime = true;
                }
                break;
            case Difficulty.Hard:
                TryUnlockDifficulty(index, Difficulty.Hell);
                TryUnlockScenario(index + 1);
                break;
            case Difficulty.Hell:
                TryUnlockScenario(index + 1);
                // give sth special
                break;
        }
    }

    private void OnTotalScoreUpdated(long score)
    {
        int id = GetScoreID(index, GetCurrentScenario().CurrentDifficulty);

        if (saveFile.scenarioMaxScore.TryGetValue(id, out long value))
        {
            if (score <= value)
            {
                return;
            }
        }

        saveFile.scenarioMaxScore[id] = score;
        saveLoadManager.Save();
    }

    private void OnExitEventStage()
    {
        mainMenuScenario.ThemeData.RaiseAmbientData();
    }

    private int GetScoreID(int scenarioIndex, Difficulty difficulty)
    {
        return scenarioIndex * 10 + (int)difficulty;
    }

    private void OnBossDefeatCutSceneEnded()
    {
        if (completeScenarioFirstTime)
        {
            completeScenarioFirstTimeEC.Raise();
            completeScenarioFirstTime = false;
        }
        else
        {
            FMODAudioManager.instance.PlayMusicInPlayList();
            
            bool canUpgradeAny = permanentUpgradableStat.CanUpgradeAnything();

            if (canUpgradeAny && RandomExtenstion.IsHappen(1 - goToItemsUpgradeStageProbability))
            {
                enterStatsUpgradeStageEC.Raise();
            }
            else
            {
                enterItemsUpgradeStageEC.Raise();
            }
        }
    }
}