using System.Collections;
using UnityEngine;
using UnityEngine.Events;


public class DamagableAlly : Damagable
{
    public UnityEvent OnRevive;

    [SerializeField]
    private float maxPercentDamage = 0.5f;

    [SerializeField]
    private float minimalHitDelay = 0.5f;

    [SerializeField]
    private HapticController hapticController;

    private WaitForSeconds minimalHitDelaySeconds;
    private WaitForSeconds durationDOT;
    private float invincibleTimeAfterRevive = 2f;

    private void Awake()
    {
        minimalHitDelaySeconds = new WaitForSeconds(minimalHitDelay);
        durationDOT = new WaitForSeconds(1);
        OnDead.AddListener(StopAllCoroutines);
    }

    public override void Hit(int damage, Vector3 hitForce = default, bool isCritical = false, bool ignoreInvincible = false,
    Transform hitman = null)
    {
        // prevent player from taking too much damage
        if (damage > maxHealth * maxPercentDamage)
        {
            damage = (int)(maxHealth * maxPercentDamage);
        }

        base.Hit(damage, hitForce, isCritical, ignoreInvincible);

        if (!invincible && !IsDead)
        {
            Invincible(minimalHitDelaySeconds);
            hapticController.HeavyFeedback();
        }
    }

    public override void HitRate(float percentage, bool ignoreInvincible = false)
    {
        if (percentage > maxPercentDamage)
        {
            percentage = maxPercentDamage;
        }

        base.HitRate(percentage, ignoreInvincible);
    }

    public void DOTHit(float time, float percentage, bool ignoreInvincible = false)
    { 
        StartCoroutine(DOT(time, percentage));
    } 

    public override void Revive()
    {
        base.Revive();
        OnRevive?.Invoke();
        Invincible(invincibleTimeAfterRevive);
    }

    private IEnumerator DOT(float time, float percentage)
    {
        int count = (int)time;

        for (int i = 0; i < count; i++)
        { 
            HitRate(percentage);
            yield return durationDOT;
        }
    }
}