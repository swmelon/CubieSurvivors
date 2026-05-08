using UnityEngine;
using UnityEngine.Serialization;

public abstract class QuickFirableWeapon<T> : UserWeapon where T : IUpgradable
{
    protected T UFireWaitTime;

    public void TurnOnQuickFireMode()
    {
        if (UFireWaitTime.IsUpgradable())
        {
            UFireWaitTime.Buff();
        }
    }

    public void TurnOffQuickFireMode()
    {
        UFireWaitTime.FinishBuff();
    }
}
