using System;
using UnityEngine;
using System.Collections.Generic;
using Local.Scripts.Extensions;


public abstract class Weapon : UpgradableContainer, IWeapon, IIconized, IItemizable
{
    public WeaponType Type => type;
    public string Name
    {
        get => weaponName;
    }

    [SerializeField]
    private WeaponType type;

    [SerializeField] 
    private string weaponName;

    [SerializeField]
    protected Sprite weaponIcon;

    private IAttackPattern[] attackPatterns;
    protected bool mountedOnEnemy;
    
    protected ITargetHaves user;
    protected Transform target;
    
    protected Action onMountedOnPlayer, onMountedOnEnemy, onMounted;
    private DamageCalculatorSO damageCalculator;


    public abstract void Damage();
    public bool UsedByPlayer() => !mountedOnEnemy;

    protected override void Awake()
    {
        base.Awake();
        SetUpgradableNameAndIcon();
        SetupAttackPatterns();
    }



    protected virtual void FixedUpdate()
    {
        if (ReferenceEquals(user, null))
        {
            return;
        }
        
        if (user.TryGetTarget(out Transform usersTarget, out float distance))
        {
            target = usersTarget;
        }
        else
        {
            target = null;
        }
    }
    
    private void SetUpgradableNameAndIcon()
    {
        foreach (var upgradable in upgradables)
        {
            upgradable.Name = weaponName;
            upgradable.Icon = weaponIcon;
            upgradable.Weapon = this;
        }
    }

    private void SetupAttackPatterns()
    {
        attackPatterns = GetComponents<IAttackPattern>();
    }

    public virtual void SetWeaponUser(ITargetHaves weaponUser)
    {
        if (weaponUser.GetTransform().TryGetComponent(out Player player))
        {
            mountedOnEnemy = false;
            damageCalculator = player.damageCalculator;
            user = player;
            onMountedOnPlayer?.Invoke();
        }
        else if(weaponUser.GetTransform().TryGetComponent(out WeaponUsableEnemy enemy))
        {
            mountedOnEnemy = true;
            user = enemy;
            onMountedOnEnemy?.Invoke();
            
            Debug.Assert(!ReferenceEquals(attackPatterns, null), name  + ": !ReferenceEquals(attackPatterns, null)");
            
            bool hasAttackAnimationController = enemy.TryGetComponent(out EnemyAnimationController controller);
            
            Debug.Assert(hasAttackAnimationController, name + ": !ReferenceEquals(controller, null)");
            
            foreach (IAttackPattern attackPattern in attackPatterns)
            {
                attackPattern.SetAnimationController(controller);   
                attackPattern.SetUser(enemy);
            }
        }
        else
        {
            Debug.LogError("Weapon should be mounted on Player or WeaponUsableEnemy. \n if weapon enabled by default, check the weapon's root object.");
        }
        
        onMounted?.Invoke();
    }
    
    public Sprite GetIcon()
    {
        return weaponIcon;
    }
    
    public bool TryGetRandomAttackPattern(out IAttackPattern attackPattern, float healthRatio)
    {
        if(attackPatterns.Length == 0)
        {
            attackPattern = null;
            return false;
        }
        
        List <IAttackPattern> availablePatterns = new List<IAttackPattern>();
        
        for(int i = 0; i < attackPatterns.Length; i++)
        {
            if(attackPatterns[i].IsAvailable(healthRatio))
            {
                availablePatterns.Add(attackPatterns[i]);
            }
        }
        
        if (availablePatterns.Count == 0)
        {
            attackPattern = null;
            return false;
        }
        
        attackPattern = availablePatterns.PickRandom();
        return true;
    }
    
    public List<IAttackPattern> GetAvailableAttackPatterns(float healthRatio)
    {
        List<IAttackPattern> availablePatterns = new List<IAttackPattern>();
        
        for(int i = 0; i < attackPatterns.Length; i++)
        {
            if(attackPatterns[i].IsAvailable(healthRatio))
            {
                availablePatterns.Add(attackPatterns[i]);
            }
        }

        return availablePatterns;
    }
    
    public bool ContainAttackPattern(IConditionalBehaviourPattern otherAttackPattern)
    {
        Type otherType = otherAttackPattern.GetType();
        foreach (IAttackPattern pattern in attackPatterns)
        {
            if (pattern.GetType() == otherType)
            {
                return true;
            }
        }
        
        return false;
    }
    
    public virtual int ComputeFinalDamage(int damage, out bool isCritical)
    {
        if (mountedOnEnemy)
        {
            isCritical = false;
            return damage;
        }
        
        return damageCalculator.CalcDamage(damage, out isCritical);
    }

    /// <summary>
    /// When weapon is converted to an item, this method is called.
    /// </summary>
    public virtual void BeItem() {}

    public abstract void OnUnmounted();
}
