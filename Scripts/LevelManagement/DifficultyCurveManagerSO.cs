using System;
using UnityEngine;
using System.Collections.Generic;


[CreateAssetMenu(fileName = "DifficultyCurveManager", menuName = "ScriptableObjects/DifficultyCurveManager", order = SOAssetMenuIndex.Difficulty)]
public class DifficultyCurveManagerSO : ScriptableObject, IDependentInitialization
{
    public int GetLevel() => level;
    public int GetPPR() => (int)ppr;
    public int GetMaxExp() => (int)maxExp;
    public int GetMinEnemySpawnDevices() => Mathf.RoundToInt(minEnemySpawnDevices);
    public int GetMaxEnemySpawnDevices() => Mathf.RoundToInt(maxEnemySpawnDevices);
    public float GetBossPowerMultiplier() => Mathf.Pow(data.PPRMultiplier, level);
    public float GetStageSizeMultiplier() => Mathf.Pow(data.EnemySpawnLocsMultiplier, level);

    [SerializeField]
    private EventChannelSO restartGameEventChannel;
    
    [SerializeField] 
    private DifficultyCurveData data;

    [SerializeField]
    private DifficultyCurveEC difficultyCurveUpdateEventChannel;

    [SerializeField]
    private DifficultyCurveEC difficultyCurveResetEC;

  

    [SerializeField]
    private EnemyDataContainer enemyDataContainer;

    [Space(10)]
    [SerializeReference]
    private int level = 0;

    [SerializeReference]
    private float ppr, maxExp;

    [SerializeReference]
    private float minEnemySpawnDevices;

    [SerializeReference]
    private float maxEnemySpawnDevices;

    private LevelCurve pprCurve, maxExpCurve;
    private LevelCurve minEnemySpawnLocsCurve, maxEnemySpawnLocsCurve;

    public DifficultyLevel CurrentDifficultyLevel => data.DifficultyLevel;
    public void Initialize()
    {
        CalulateEnemyPower();
    }

    private void OnDisable()
    {
    }

    private void LoadDataAndInitialize()
    {
        pprCurve = new LevelCurve(data.PPRInitial, data.PPRMultiplier);
        maxExpCurve = new LevelCurve(data.MaxExpInitial, data.MaxExpMultiplier);

        int initialMinEnemySpawnLocs = Mathf.RoundToInt(data.MinEnemySpawnLocsInitial * data.NumEnmiesFactor);
        int initialMaxEnemySpawnLocs = Mathf.RoundToInt(data.MaxEnemySpawnLocsInitial * data.NumEnmiesFactor);

        minEnemySpawnLocsCurve = new LevelCurve(initialMinEnemySpawnLocs, data.EnemySpawnLocsMultiplier);
        maxEnemySpawnLocsCurve = new LevelCurve(initialMaxEnemySpawnLocs, data.EnemySpawnLocsMultiplier);
    }

    public void SetData(DifficultyCurveData data)
    {
        this.data = data;
        LoadDataAndInitialize();
        ApplyHealthFactorForTest();
        ResetCurve();
    }
    public void UpdateCurves(bool noUpdate = false)
    {
        level++;
        
        if (level == 1)
        {
            difficultyCurveUpdateEventChannel.Raise(this);
            return;
        }
        
        pprCurve.UpdatePoint();
        minEnemySpawnLocsCurve.UpdatePoint();
        maxEnemySpawnLocsCurve.UpdatePoint();

        SetDifficultyValues();
        
        difficultyCurveUpdateEventChannel.Raise(this);
    }

    public int UpdateExp()
    {
        maxExpCurve.UpdatePoint();
        maxExp = maxExpCurve.GetPoint();
        return (int)maxExp;
    }

    public float GetCurrentMeanEnemyPower()
    {
        float meanDeviceCount = (minEnemySpawnDevices + maxEnemySpawnDevices) / 2f;

        if (meanDeviceCount == 0)
        {
            Debug.LogError("MeanDeviceCount is 0. Set DifficultyCurveData properly.");
            return 0f;
        }

        float meanSpawnPeriod = 3f; // 가정
        // 이 값은 목표 PPR을 맞추지 못하면 실제보다 작아진다. (평균적으로)
        return GetPPR() * meanSpawnPeriod / meanDeviceCount;
    }

    public float GetCurrentLowestEnemyPower()
    {
        return GetPPR() * 1f / maxEnemySpawnDevices;
    }

    public void ResetCurve()
    {
        level = 0;
        ppr = data.PPRInitial;
        maxExp = data.MaxExpInitial;
        minEnemySpawnDevices = minEnemySpawnLocsCurve.GetInitialPoint();
        maxEnemySpawnDevices = maxEnemySpawnLocsCurve.GetInitialPoint();
        difficultyCurveResetEC.Raise(this);
    }

    private void ApplyHealthFactorForTest()
    {
        foreach (EnemyData data in enemyDataContainer.Datas)
        {
            data.healtFactor = this.data.EnmiesHealthFactor;
        }
    }

    private void CalulateEnemyPower()
    {
        foreach (EnemyData data in enemyDataContainer.Datas)
        {
            data.InitializePower();
        }
    }

    private void SetDifficultyValues()
    {
        ppr = pprCurve.GetPoint();
        maxExp = maxExpCurve.GetPoint();
        minEnemySpawnDevices = minEnemySpawnLocsCurve.GetPoint();
        maxEnemySpawnDevices = maxEnemySpawnLocsCurve.GetPoint();
    }
}
