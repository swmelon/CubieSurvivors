using UnityEngine;
using VolumetricLines;

public class Lancer : ColorableBulletGun<LaserBullet> 
{
    [SerializeField]
    private int maxReflection = 2;

    private Upgradable<bool> UUltimateUpgrade;

    protected override void InitializeUpgradables(UpgradableStat upgradableStat)
    {
        base.InitializeUpgradables(upgradableStat);

        UUltimateUpgrade = new Upgradable<bool>(upgradableStat.Unlocked, optionText: CardText.ULTIMATE_UPGRADE_LANCER);

        IUpgradable[] otherUpgradables = {UDamage, URange, UFireWaitTime};
        UUltimateUpgrade.UnlockWhenComplete(otherUpgradables);
        UUltimateUpgrade.UpgradeCompleted += Unlock;
        UUltimateUpgrade.UpgradeReset += Lock;
        Lock();
    }

    private void Unlock()
    {
        LaserBullet.SetMaxReflection(maxReflection);
    }

    private void Lock()
    {
        LaserBullet.SetMaxReflection(0);
    }
}
