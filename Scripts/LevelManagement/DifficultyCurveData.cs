using UnityEngine;

[CreateAssetMenu(fileName = "DifficultyCurveData", menuName = "ScriptableObjects/DifficultyCurveData", order = SOAssetMenuIndex.Difficulty)]
public class DifficultyCurveData : ScriptableObject
{
    [SerializeField]
    private DifficultyLevel difficultyLevel;

    [SerializeField]
    private float pprInitial = 50f;

    [SerializeField][Range(1f, 1.2f)] 
    private float pprMultiplier = 1.08f;

    [SerializeField]
    private int maxExpInitial = 2000;

    [SerializeField][Range(1f, 1.2f)]
    private float maxExpMultiplier = 1.08f;

    [SerializeField]
    private float minEnemySpawnLocsInitial = 2f;
    
    [SerializeField]
    private float maxEnemySpawnLocsInitial = 4f;

    [SerializeField][Range(1f, 1.1f)]
    private float enemySpawnLocsMultiplier = 1.04f;

    [SerializeField]
    [Range(0.1f, 10f)]
    private float numEnmiesFactor = 1f;

    [SerializeField]
    [Range(0.1f, 10f)]
    private float enmieshealthFactor = 1f;

    public float PPRInitial
    {
        get => pprInitial;
        set => pprInitial = value;
    }
    public float PPRMultiplier
    {
        get => pprMultiplier;
        set => pprMultiplier = value;
    }
    public int MaxExpInitial
    {
        get => maxExpInitial;
        set => maxExpInitial = value;
    }
    public float MaxExpMultiplier
    {
        get => maxExpMultiplier;
        set => maxExpMultiplier = value;
    }
    public float MinEnemySpawnLocsInitial
    {
        get => minEnemySpawnLocsInitial;
        set => minEnemySpawnLocsInitial = value;
    }
    public float MaxEnemySpawnLocsInitial
    {
        get => maxEnemySpawnLocsInitial;
        set => maxEnemySpawnLocsInitial = value;
    }
    public float EnemySpawnLocsMultiplier
    {
        get => enemySpawnLocsMultiplier;
        set => enemySpawnLocsMultiplier = value;
    }
    public float NumEnmiesFactor
    {
        get => numEnmiesFactor;
        set => numEnmiesFactor = value;
    }
    public float EnmiesHealthFactor
    {
        get => enmieshealthFactor;
        set => enmieshealthFactor = value;
    }

    public DifficultyLevel DifficultyLevel
    {
        get => difficultyLevel;
        set => difficultyLevel = value;
    }
}
