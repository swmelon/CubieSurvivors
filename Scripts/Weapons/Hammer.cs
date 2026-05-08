using System;
using UnityEngine;


public class Hammer : Weapon
{
    private Animator animator;
    private int foldHash, unfoldHash;
    private struct UpgradableInfo
    {
        // Define Damage, RateOfFire, or whatever you want.
    }
    
    // Override OnEnable() to initialize members.
    protected override void Awake()
    {
        base.Awake();
        animator = GetComponent<Animator>();
        foldHash = Animator.StringToHash("Fold");
        unfoldHash = Animator.StringToHash("Unfold");
        // onMountedOnPlayer
        // onMountedOnEnemy
    }

    private void OnEnable()
    {
        animator.ResetTrigger(unfoldHash);
        animator.SetTrigger(foldHash);
    }

    public override void Damage()
    {
        // This method will be call by AttackPattern.
        
        animator.SetTrigger(unfoldHash);
        Invoke(nameof(Fold), 1f);
    }

    private void Fold()
    {
        animator.SetTrigger(foldHash);
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
