using System.Collections;
using UnityEngine;


public class RocketLauncher : ExplosiveBulletGun<Rocket>
{
    [SerializeField]
    private float explosiveRangeUlitmate = 1.5f;

    private Upgradable<bool> UUltimateUpgrade;
    private WaitForSeconds skippingTime = new WaitForSeconds(0.12f);
    private const float deltaConstant = 0.5f;

    protected override void InitializeUpgradables(UpgradableStat upgradableStat)
    {
        base.InitializeUpgradables(upgradableStat);

        UUltimateUpgrade = new Upgradable<bool>(upgradableStat.Unlocked, optionText: CardText.ULTIMATE_UPGRADE_NAVBOMBER);

        IUpgradable[] otherUpgradables = { UDamage, URange, UFireWaitTime, UExplosiveDamage};
        UUltimateUpgrade.UnlockWhenComplete(otherUpgradables);
    }

    protected override void OnGetBullet(Rocket bullet)
    {
        base.OnGetBullet(bullet);
        bullet.SetTargetlessTime(0.5f);
    }


    protected override void OnBulletHit(Vector3 hitPoint)
    {
        base.OnBulletHit(hitPoint);

        if (UUltimateUpgrade.Value)
        {
            StartCoroutine(SkippingBombs(hitPoint));
        }
    }

    protected override void OnGetExplosive(Explosive explosive)
    {
        base.OnGetExplosive(explosive);

        if (UUltimateUpgrade.Value)
        {
            explosive.SetRange(explosiveRangeUlitmate);
        }
        else
        {
            explosive.SetRangeToInitialValue();
        }
    }

    private IEnumerator SkippingBombs(Vector3 firstHitPoint)
    {
        Vector3 direction = firstHitPoint - transform.position;
        direction.y = 0;
        float distance = direction.magnitude;
        direction.Normalize();

        // 2를 곱한 이유는 폭발 범위가 겹치지 않게 하려고
        // deltaConstant는 어느 정도로 멀어야 폭발 간의 간격을 일정하게 만들지,
        // 가까울 경우 얼마나 좁힐지 결정하는 상수
        Vector3 delta = Mathf.Min(deltaConstant *distance * explosiveRangeUlitmate, 2 * explosiveRangeUlitmate) * direction;

        yield return skippingTime;

        base.OnBulletHit(firstHitPoint + delta);

        yield return skippingTime;

        base.OnBulletHit(firstHitPoint + 2 * delta);
    }
}
