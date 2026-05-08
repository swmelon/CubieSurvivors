using UnityEngine;
using System.Collections.Generic;
using System;
using Local.Scripts.Extensions;
using FMODUnity;


[CreateAssetMenu(fileName = "GameScenario", menuName = "ScriptableObjects/Game/GameScenario")]
public class GameScenario : ScriptableObject
{
    public string scenarioName;
    public Texture2D icon;
    public bool Developing;
    
    public int bossAppearances;
    public int bossInterval;

    [SerializeField]
    private int scenarioNumber;

    [SerializeField]
    private DifficultyCurveManagerSO difficultyCurveManager;

    [SerializeField]
    private EnemyDataContainer enemyDataContainer;

    [SerializeField]
    private ESDContainer esdContainer;

    [SerializeField]
    private List<ThemeData> themeDatas = new List<ThemeData>();

    [SerializeField]
    private DifficultyCurveData[] difficultyCurveDatas = new DifficultyCurveData[3];

    [SerializeField]
    private ThemeDataChannelSO themeDataChannel;

    [SerializeField]
    private EventReference ambientSound;

    [SerializeField]
    private Material sideFrameMat, pillarMat;

    [SerializeField]
    private MaterialChannelSO sideFrameMatChannel, pillarMatChannel;

    private long maxScore, maxScoreHard, maxScoreHell;
    private Difficulty maxDifficulty;
    private ScenarioData scenarioData;


    private int index = 0;
    private int defeatedBossCount = 0;
    private bool defeatFinalBoss = false;
    private Difficulty currentDifficulty;

    public ThemeData ThemeData => themeDatas[index];
    public EnemyDataContainer EnemyDataContainer => enemyDataContainer;
    public ESDContainer ESDContainer => esdContainer;

    public Difficulty CurrentDifficulty => currentDifficulty;
    public Difficulty MaxDifficulty => maxDifficulty;
    public bool DefeatFinalBoss => defeatFinalBoss;
    public ScenarioData Data
    {
        get
        {
            if (scenarioData.name == null)
            {
                scenarioData.scenarioNumber = scenarioNumber;
                scenarioData.name = scenarioName;
                scenarioData.icon = icon;
                scenarioData.maxScore = maxScore;
                scenarioData.maxDifficulty = maxDifficulty;
                scenarioData.Developing = Developing;
            }

            return scenarioData;
        }
    }

    public Weapon finalBossWeapon;

    public void LoadRandomTheme()
    {
        index = RandomExtenstion.GetIntInRange(0, themeDatas.Count - 1);

        defeatedBossCount = 0;
        defeatFinalBoss = false;

        InitializeAndRaiseTheme();
    }

    public void Load(Difficulty difficulty)
    {
        index = 0;
        defeatedBossCount = 0;
        defeatFinalBoss = false;
        currentDifficulty = difficulty;

        sideFrameMatChannel.Register(sideFrameMat);
        pillarMatChannel.Register(pillarMat);

        if (DifficultySettingsScreen.HasChanges)
        {
            DifficultyCurveData data = difficultyCurveDatas[0];
            data.NumEnmiesFactor = DifficultySettingsScreen.NumEnemiesMultValue;
            data.EnmiesHealthFactor = DifficultySettingsScreen.EnemiesHealthMultValue;
            data.PPRMultiplier = DifficultySettingsScreen.PPRExpoValue;
            data.PPRInitial = DifficultySettingsScreen.PPRInitialValue;
            data.MaxExpInitial = DifficultySettingsScreen.InitialExpValue;
            data.MinEnemySpawnLocsInitial = DifficultySettingsScreen.MinESDValue;
            data.MaxEnemySpawnLocsInitial = DifficultySettingsScreen.MaxESDValue;
            data.EnemySpawnLocsMultiplier = DifficultySettingsScreen.NumEnemiesExpoValue;
            data.MaxExpMultiplier = DifficultySettingsScreen.NumEnemiesExpoValue;

            difficultyCurveManager.SetData(data);
            InitializeAndRaiseTheme();
            PlayAmbientSound();
            return;
        }

        switch (difficulty)
        {
            case Difficulty.Normal:
                difficultyCurveManager.SetData(difficultyCurveDatas[0]);
                break;
            case Difficulty.Hard:
                difficultyCurveManager.SetData(difficultyCurveDatas[1]);
                break;
            case Difficulty.Hell:
                difficultyCurveManager.SetData(difficultyCurveDatas[2]);
                break;
        }

        InitializeAndRaiseTheme();
        PlayAmbientSound();
    }

    private void PlayAmbientSound()
    {
        FMODAudioManager.instance.SetMusicAndPlay(ambientSound);
    }


    public void Unload()
    {
    }


    public void  OnDefeatBoss()
    {
        index++;
        defeatedBossCount++;
        
        if (index >= themeDatas.Count)
        {
            index = 0;
        }

        if (defeatedBossCount >= bossAppearances)
        {
            defeatFinalBoss = true;
        }

        // ���� �̺�Ʈ ���������� Ambient�� ���� �� �� �ְ� �ؾ���

        InitializeAndRaiseTheme();
    }

    public void SetMaxScore(long[] score)
    {
        maxScore = score[0];
        maxScoreHard = score[1];
        maxScoreHell = score[2];

        scenarioData = new ScenarioData
        {
            scenarioNumber = scenarioNumber,
            name = scenarioName,
            icon = icon,
            maxScore = maxScore,
            maxScoreHard = maxScoreHard,
            maxScoreHell = maxScoreHell,
            maxDifficulty = maxDifficulty,
            Developing = Developing
        };
    }

    public void SetMaxDifficulty(Difficulty difficulty)
    {
        maxDifficulty = difficulty;
        scenarioData = new ScenarioData
        {
            scenarioNumber = scenarioNumber,
            name = scenarioName,
            icon = icon,
            maxScore = maxScore,
            maxScoreHard = maxScoreHard,
            maxScoreHell = maxScoreHell,
            maxDifficulty = maxDifficulty,
            Developing = Developing
        };
    }

    private void InitializeAndRaiseTheme()
    {
        ThemeData.Initialize();
        themeDataChannel.Register(ThemeData);
    }

    public int GetEstimatedCompleteTime()
    {
        float idealCompleteTime = 0f;

        for (int i = 0; i < bossAppearances; i++)
        {
            int index = i % themeDatas.Count;
            idealCompleteTime += (themeDatas[index].MainStageData.StageInterval + 2) * bossInterval;
        }

        idealCompleteTime += bossAppearances * 100;

        return (int)idealCompleteTime;
    }

    public bool TryGetFinalBossWeapon(out Weapon weapon)
    {
        weapon = finalBossWeapon;
        return weapon != null;
    }
}
