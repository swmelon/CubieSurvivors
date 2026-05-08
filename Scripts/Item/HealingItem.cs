using UnityEngine;


public class HealingItem : Item
{
    [SerializeField] 
    private int healAmount;

    [SerializeField]
    private FloatChannelSO playerHealthChannel;

    [SerializeField]
    private AnimationCurve probMultiplierCurve;

    public override float Probability
    {
        get
        {
            return Mathf.Max(0f, probability * probMultiplierCurve.Evaluate(1 - playerHealthChannel.Value));
        }
    }

    public override void Activate(Player player)
    {
        player.Damagable.Heal(healAmount);
        base.Activate(player);
    }
}
