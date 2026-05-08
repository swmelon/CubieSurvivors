public interface IDamagable
{
    void Hit(int damage, bool ignoreInvincible = false);
    void HitPercentage(float percentage, bool ignoreInvincible = false);
    void Heal(int healthBoost);
    void Revive();
    void Invincible(float time);
    void Invincible();
    void Kill();
}