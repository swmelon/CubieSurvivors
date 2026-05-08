using Local.Scripts.Extensions;
using System;
using UnityEngine;



[RequireComponent(typeof(EnemyAnimationController))]

public class WeaponUsableEnemy : Enemy, IWeapon
{
    protected IEnemyWeaponManager weaponManager;
    protected IConditionalBehaviourPattern currentBP;

    private IAttackPattern[] nonWeaponAttackPatterns;
    private float timer = 0f;

    private bool CaptureInitalTransform = true;

    [SerializeField]
    private ItemPackerSO itemPacker;

    [SerializeField]
    private InitialTransformData transformData;

    [SerializeField]
    private bool useNonWeaponAttackPatterns = false;

    private const float dropItemYPosition = 0.5f;
    private bool dropWeapon = false;


    public bool UsedByPlayer() => false;

    protected override void Awake()
    {
        base.Awake();
        weaponManager = GetComponent<IEnemyWeaponManager>();
        weaponManager.WeaponMounted += (weapon) => SetAttackPattern();

        dropWeapon = weaponManager is EnemyWeaponManager;

        if (useNonWeaponAttackPatterns)
        {
            nonWeaponAttackPatterns = GetComponents<IAttackPattern>();

            if (nonWeaponAttackPatterns.Length == 0)
            {
                Debug.LogError("No non weapon attack patterns are found.");
            }

            for (int i = 0; i < nonWeaponAttackPatterns.Length; i++)
            {
                nonWeaponAttackPatterns[i].SetUser(this);
                nonWeaponAttackPatterns[i].SetAnimationController(animationController);
            }
        }
    }

    private void OnDisable()
    {
        transformData.ResetTransform(transform);
    }

    protected virtual void SetAttackTrigger(bool value)
    {
        if (value == isBehaving)
        {
            return;
        }
        
        if (value)
        {
            if (currentBP == null)
            {
                Debug.LogError("Attack Pattern is null but SetAttackTrigger(true) is called.");
            }
            
            currentBP?.StartAction(OnAttackFinished);
        }
        else
        {
            if (gameObject.activeSelf)
            {
                SetAttackPattern();
            } 
        }
        
        isBehaving = value;
    }

    public void Damage()
    {

    }

    protected override void OnSpawned()
    {
        base.OnSpawned();
        SetAttackPattern();
    }

    protected virtual void SetAttackPattern()
    {
        if (weaponManager.TryGetAttackPatternByDistanceToTarget(DistanceToTarget, out IAttackPattern attackPattern))
        {
            currentBP = attackPattern;
        }
        else if (useNonWeaponAttackPatterns)
            // 무기가 없는 상태에서 AP 실행을 위한 플래그
        {
            int randomIndex = RandomExtenstion.GetIntInRange(0, nonWeaponAttackPatterns.Length - 1);
            currentBP = nonWeaponAttackPatterns[randomIndex];
        }
        else
        {
            currentBP = null;
            Debug.Log("No attack pattern is chosen");
        }
    }
    
    public override void Attack()
    {
        SetAttackTrigger(true);
    }
    
    protected void OnAttackFinished()
    {
        SetAttackTrigger(false);
    }

    public override bool IsTargetWithInAttackRange()
    {
        if (!ReferenceEquals(currentBP, null))
        {
            return currentBP.IsTargetInRange(DistanceToTarget);
        }

        return false;
    }

    protected override void OnDead()
    {
        if (!ReferenceEquals(currentBP, null) && currentBP.IsActivated())
        {
            currentBP?.StopAction();
        }

        if (dropWeapon)
        {
            DropAllWeapons();
        }

        isBehaving = false;
        base.OnDead();
    }

    public override void ForceKill(bool spawnExp = false, bool ignore = false)
    {
        if (dropWeapon)
        {
            DropAllWeapons();
        }
        base.ForceKill(spawnExp);
    }

    protected void DropAllWeapons()
    {
        Vector3 dropItemPosition = transform.position;
        dropItemPosition.y = dropItemYPosition;

        itemPacker.WarpUp<Weapon, EnemyDropWeaponItem>(weaponManager.UnmountAll(), dropItemPosition, LayerMaskCash.Item);
    }
}
