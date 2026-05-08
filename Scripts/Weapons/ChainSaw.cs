
using System;
using UnityEngine;

public class ChainSaw : Weapon
{
    [SerializeField]
    private AudioClip attackSound;

    private Collider collider;
    
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
        collider = GetComponent<Collider>();
    }
    
    public override void Damage()
    {
        collider.enabled = true;
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
