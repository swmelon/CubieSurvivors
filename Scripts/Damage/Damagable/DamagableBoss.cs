using UnityEngine;

public class DamagableBoss : DamagableEnemy
{
    [SerializeField]
    private float perDamageDiscount = 0.3f;

    [SerializeField]
    private FloatChannelSO bossHealthRatioChannel;

    protected override void Awake()
    {
        base.Awake();
        OnHealthChange.AddListener(OnHealthChanged);
    }

    public override void HitRate(float percentage, bool ignoreInvincible = false)
    {
        percentage *= perDamageDiscount;

        base.HitRate(percentage, ignoreInvincible);
    }

    private void OnHealthChanged(float ratio)
    {
        bossHealthRatioChannel.Register(ratio);
    }
}