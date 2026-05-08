using UnityEngine;

public class ExplosivePercentDamage : Explosive
{
    public int Damage;

    private void Awake()
    {
        damage = Damage;
    }

    protected override void OnHitDamagable(Collider collider, Damagable damagable)
    { 
        damagable.HitRate(damage);
    }
}