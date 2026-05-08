
public interface IWeapon
{
    public void Damage();
    public int ComputeFinalDamage(int Damage, out bool isCritical)
    {
        isCritical = false;
        return Damage;
    }

    public bool UsedByPlayer();
}
