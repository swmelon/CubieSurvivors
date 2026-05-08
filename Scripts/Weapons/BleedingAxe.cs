public class BleedingAxe : Weapon
{
    private struct UpgradableInfo
    {
        // Define Damage, RateOfFire, or whatever you want.
    }
    
    // Override OnEnable() to initialize members.
    protected override void Awake()
    {
        base.Awake();
        // onMountedOnPlayer
        // onMountedOnEnemy
    }
    
    public override void Damage()
    {
        // This method will be call by AttackPattern.
    }

    protected override void SetupUpgradables()
    {
        // If you want to implement a upgradable weapon,
        // First : Define UpgradableStat and call ReadUpgradableInfo() to read a json file.
        // The json file name must be same with a weapon class name.
        // Second : Define Upgradables and initialize using returned UpgradableStat.
    }

    public override void OnUnmounted()
    {
    }

    // You can override SetWeaponUser() to implement more feature by user (ITargetHaves)
}