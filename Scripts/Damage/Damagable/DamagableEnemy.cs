using Unity.VisualScripting;
using UnityEngine;


[RequireComponent(typeof(Enemy))]
public class DamagableEnemy : Damagable
{
    private Enemy enemy;

    [SerializeField] 
    private GameObject hitEffect;

    protected virtual void Awake()
    {
        enemy = GetComponent<Enemy>();
    }

    public override void Hit(int damage, Vector3 hitForce, 
        bool isCritical = false,  bool ignoreInvincible = false, Transform hitman = null)
    {
        base.Hit(damage, hitForce, isCritical, ignoreInvincible);

        if (ReferenceEquals(hitman, null))
        {
            return;
        }

        enemy.SetTargetAndUpdate(hitman);
    }
}
