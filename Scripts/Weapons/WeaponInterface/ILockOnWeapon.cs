public interface ILockOnWeapon : IWeapon
{
    public void SetLockOnMode(bool useLockOnMode, float maxDegreePerSec = float.MaxValue, float projectileSpeed = 5f);
}
 