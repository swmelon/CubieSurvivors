
using UnityEngine;

public class DamagableNoText : Damagable
{
    public override void Hit(int damage, Vector3 hitForce = default, 
        bool isCritical = false, bool ignoreInvincible = false, Transform hitman = null)
    {
        if(damage < 0)
        {
            Debug.LogError("Damage cannot be negative.");
            return;
        }

        if (!Hitable())
        {
            return;
        }

        OnHit?.Invoke(hitForce);
        CheckIfDead(damage, ignoreInvincible);
    }
}