using UnityEngine;

public  abstract class  BPByBossHealth : ConditionalBehaviourPattern
{
    [Tooltip("decsending order")]
    [SerializeField]
    private float[] healthThresholds;

    [SerializeField]
    protected FloatChannelSO bossHealthRatioChannel;

    private int currentThresholdIndex = 0;
    private bool available = false;


    public override void StartAction(System.Action onActionFinished)
    {
        if (!available)
        {
            Debug.LogWarning("This behaviour pattern is not available.");
            onActionFinished();
            return;
        }

        available = false;
        base.StartAction(onActionFinished);
    }

    public override bool IsAvailable()
    {
        CheckBossHealthAndSetIndex();
        return available;
    }

    private void CheckBossHealthAndSetIndex()
    {
        if (available)
        {
            return;
        }

        float healthRatio = bossHealthRatioChannel.Value;

        if (currentThresholdIndex < healthThresholds.Length && healthRatio <= healthThresholds[currentThresholdIndex])
        {
            available = true;
            currentThresholdIndex++;
        }
        else
        {
            available = false;
        }
    }
}