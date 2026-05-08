using Local.Scripts.Extensions;
using StarterAssets;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using VolumetricLines;

public class DoubleBeamSaber : UserWeapon
{

    [SerializeField]
    private BeamSaber left, right;

    private Animator animator;
    private LayerMask targetLayer;

    private int attackModeHash;

    private struct UpgradableStat
    {
        public List<float> Damage, BeamLength; 
        public List<int>  AttackMode, Color;
        public List<bool> Unlocked;
    } 

    private UInt UDamage;
    private UFloat UBeamLength;
    private Upgradable<int> UAttackMode;
    private Upgradable<bool> UUltimateUpgrade;
    private Upgradable<Color> UBeamColor;
    private int currentAttackMode;
    private float hitMemoryFlushTime = 1f;
    private float hitMemoryFlushTimeCount = 0;
    private float noTargetTime = 1f;
    private float noTargetTimeCount = 0;

    private IAnimationController userAnimationController;
    private bool clockwise = true;

    protected override void Awake()
    {
        base.Awake();
        
        animator = GetComponent<Animator>();
        SetAttackMode(0);
        onMountedOnPlayer += OnMountedOnPlayer;
        onMountedOnEnemy += OnMountedOnEnemy;
        onMounted += OnMounted;
        attackModeHash = Animator.StringToHash("attackMode");

        left.SetParentWeapon(this);
        right.SetParentWeapon(this);

        left.SetThrowRotation(clockwise : true);
        right.SetThrowRotation(clockwise: false);
    }

    private void Start()
    {
        UpdateBeamLength();
        SetDamage();
    }

    private void OnMounted()
    {
        user.GetTransform().TryGetComponent(out userAnimationController);
        SetBeamColor();
        UpdateBeamLength();
        EnableAnimator();
    }

    private void OnMountedOnPlayer()
    {
        SetAttackMode(UAttackMode.Value);
        targetLayer = LayerMaskCash.Enemy;

        left.SetTargetLayer(targetLayer);
        right.SetTargetLayer(targetLayer);   

        left.SetWeaponUser(user.GetTransform(), WeaponUser.Player);
        right.SetWeaponUser(user.GetTransform(), WeaponUser.Player);
        
        if(user.GetTransform().TryGetComponent(out CustomThirdPersonController customThirdPersonController))
        {
            customThirdPersonController.SetFastRotation(true);
        }

        On();
    }

    private void OnMountedOnEnemy() 
    {
        SetAttackMode(0);
        targetLayer = LayerMaskCash.PlayerAndEnemy;
        left.SetTargetLayer(targetLayer);
        right.SetTargetLayer(targetLayer);

        left.SetWeaponUser(user.GetTransform(), WeaponUser.Enemy);
        right.SetWeaponUser(user.GetTransform(), WeaponUser.Enemy);
    }

    public override void OnUnmounted()
    {
        Off();
        SetAttackMode(0);
        left.ThrownBack();
        right.ThrownBack();

        if (user.GetTransform().TryGetComponent(out CustomThirdPersonController customThirdPersonController))
        {
            customThirdPersonController.SetFastRotation(false);
        }
    }

    protected override void SetupUpgradables()
    {
        UpgradableStat upgradableStat = UpgradeExtension.ReadUpgradableStat<UpgradableStat>(this);
        InitializeUpgradables(upgradableStat);
    }

    private void InitializeUpgradables(UpgradableStat upgradableStat)
    {
        UDamage = new UInt(upgradableStat.Damage, symbol: symbolContainer.Damage,
            optionText: CardText.DAMAGE);
        UDamage.Upgraded += SetDamage;
        UBeamLength = new UFloat(upgradableStat.BeamLength, symbol: symbolContainer.Scale,
            optionText: CardText.BEAM_LENGTH);
        UBeamLength.Upgraded += UpdateBeamLength;
        UAttackMode = new Upgradable<int>(upgradableStat.AttackMode, symbol: symbolContainer.Sword,
            optionText: CardText.CHANGE_MODE);
        UAttackMode.Upgraded += () => SetAttackMode(UAttackMode.Value);
        UUltimateUpgrade = new Upgradable<bool>(upgradableStat.Unlocked, optionText: CardText.ULTIMATE_UPGRADE_DUALSABER);
        UBeamColor = new Upgradable<Color>(upgradableStat.Color.ToColors());
        UBeamColor.LinkTo(UDamage);
        UBeamColor.Upgraded += SetBeamColor;


        IUpgradable[] otherUpgradables = { UDamage, UBeamLength, UAttackMode};
        UUltimateUpgrade.UnlockWhenComplete(otherUpgradables);
    }



    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        if (mountedOnEnemy)
        {
            hitMemoryFlushTimeCount += Time.fixedDeltaTime;
            
            if (hitMemoryFlushTimeCount >= hitMemoryFlushTime)
            {
                ClearHitMemory();
                hitMemoryFlushTimeCount = 0;
            }

            return;
        }

        bool hasTarget = !ReferenceEquals(target, null);

        if (hasTarget && currentAttackMode == 0)
        {
            On();
            SetAttackMode(UAttackMode.Value);
            noTargetTimeCount = 0;
            return;
        }

        if (!hasTarget && currentAttackMode != 0)
        {
            noTargetTimeCount += Time.fixedDeltaTime;

            if (noTargetTimeCount >= noTargetTime)
            {
                Off();
                SetAttackMode(0);
            }

            return;
        }
    }

    public override void Damage()
    {
        throw new NotImplementedException();
    }
    
    public void SetAttackMode(int mode)
    {
        currentAttackMode = mode;
        animator.SetInteger(attackModeHash, mode);
    }

    private void SetDamage() 
    {
        left.SetDamage(UDamage.Value);
        right.SetDamage(UDamage.Value);
    }

    private void UpdateBeamLength()
    {
        left.SetMaxBeamLength(UBeamLength.Value);
        right.SetMaxBeamLength(UBeamLength.Value);
    }

    public void On()
    {
        left.TurnOnBeam();
        right.TurnOnBeam();
    }
    
    public void Off()
    {
        left.TurnOffBeam();
        right.TurnOffBeam();
    }

    public void ClearHitMemory()
    {
        left.ClearHitMemory();
        right.ClearHitMemory();
    }

    public void ThrowLeft(float interval = 1.5f)
    {
        if (!mountedOnEnemy && !UUltimateUpgrade.Value)
        {
            return;
        }

        left.Throw(interval);
    }

    public void ThrowRight(float interval = 1.5f)
    {
        if (!mountedOnEnemy && !UUltimateUpgrade.Value)
        {
            return;
        }

        right.Throw(interval);
    }

    public void EnableAnimator()
    {
        animator.enabled = true;
    }

    public void DisableAnimator()
    {
        animator.enabled = false;
    }

    public void SpinLeftUser()
    {
        if (mountedOnEnemy)
        {
            return;
        }
        userAnimationController.SpinLeft();
    }

    public void SpinRightUser()
    {
        if (mountedOnEnemy)
        {
            return;
        }
        userAnimationController.Spin();
    }

    public void SpinUser()
    {
        if (clockwise)
        {
            SpinRightUser();
        }
        else
        {
            SpinLeftUser();
        }

        clockwise = !clockwise;
    }

    public override void BeItem()
    {
        transform.localScale = 0.5f * Vector3.one;
    }

    
    private void SetBeamColor()
    {
        left.SetBeamColor(UBeamColor.Value);
        right.SetBeamColor(UBeamColor.Value);
    }
}
