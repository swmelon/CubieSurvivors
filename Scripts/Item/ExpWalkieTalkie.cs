using UnityEngine;

public class ExpWalkieTalkie : Item
{
    [SerializeField]
    private int maxProbabiltyCount = 100;

    [SerializeField]
    private EventChannelSO pullAllExpEC;

    [SerializeField]
    private IntChannelSO expCountChannel, coinCountChannel;

    [SerializeField]
    private AnimationCurve probMultiplierCurve;

    public override float Probability
    {
        get
        {
            int count = expCountChannel.Value + coinCountChannel.Value;
            return Mathf.Max(0f, probability * probMultiplierCurve.Evaluate((float)count / maxProbabiltyCount));
        }
    }

    public override void Activate(Player player)
    {
        pullAllExpEC.Raise();
        base.Activate(player);
    }

  
}