using System;
using UnityEngine;

public class LaserWaveGenTrap : Trap
{
    [SerializeField]
    private LaserWaveGenerator laserWaveGenerator;
    protected override void On()
    {
        laserWaveGenerator.Activate();
    }

    protected override void Off()
    {
        laserWaveGenerator.Deactivate();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        laserWaveGenerator.Stop();
    }

    private void OnDestroy()
    {
        
    }
}
