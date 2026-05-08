using System;
using UnityEngine;
using System.Collections.Generic;
using Local.Scripts.Extensions;
using UnityEngine.Serialization;
using System.Runtime.CompilerServices;


public class Boss : WeaponUsableEnemy, IColorable
{
    [SerializeField]
    private Renderer rollerRenderer;

    [SerializeField]
    private float wakeUpDistance = 10f;

    [Header("Boss BehaviourPatterns")]
    [SerializeField]
    private BehaviourPattern dashPattern;

    [SerializeField]
    private float dashDistance = 10f;

    [Range(4f, 10f)]
    [SerializeField]
    private float maxChaseStateTime = 4f;

    [SerializeField]
    private BehaviourPattern shakeOffPattern;

    [Header("Channels")]
    [SerializeField]
    private PlayerChannelSO currentPlayerChannel;

    [SerializeField]
    private WeaponEventChannelSO returnWeaponChannel;

    [SerializeField]
    private EventChannelSO playerDeadEC, playerReviveEC;

    [SerializeField]
    private BossChannelSO bossChannel;

    [SerializeField]
    private EventChannelSO playerFallEventChannel;

    [SerializeField]
    private EventChannelSO finishStageMoveEC;

    [SerializeField]
    private TransformChannelSO bossTransformChannel;

    [SerializeField]
    private StringEventChannelSO drawTextOnFloorEC;

    private IConditionalBehaviourPattern[] bossPatterns;
    private HealthBarManager healthBarManager;

    public WeaponManager WeaponManager => weaponManager as WeaponManager;

    private const float healRatioOnPlayerRevive = 0.1f;
    private const float onHeadXzDistanceThreshold = 2f;
    private const float onHeadMinYDifference = 1f;
    private const float onHeadMaxYDifference = 4f;
    private const float rotateSlowlySpeedFactor = 0.25f;
    private Player player;


    protected override void Awake()
    {
        base.Awake();

        BehaviourPattern[] behaviourPatterns = GetComponents<BehaviourPattern>();

        for (int i = 0; i < behaviourPatterns.Length; i++)
        {
            behaviourPatterns[i].SetAnimationController(animationController);
            behaviourPatterns[i].SetUser(this);
        }

        healthBarManager = GetComponent<HealthBarManager>();
        
        // This is for public access to WeaponManager from other classes.
        weaponManager = GetComponent<EnemyWeaponManager>();
        bossPatterns = GetComponents<ConditionalBehaviourPattern>();        
       
        bossTransformChannel.Register(transform);
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        playerDeadEC.Subscribe(OnPlayerDead);
        playerReviveEC.Subscribe(OnPlayerRevive);
        bossChannel.Register (this);
        finishStageMoveEC.Subscribe(OnFinishStageMove);
        currentPlayerChannel.Subscribe(SetPlayer);
    }
    
    protected void OnDisable()
    {
        playerDeadEC.Unsubscribe(OnPlayerDead);
        playerReviveEC.Unsubscribe(OnPlayerRevive);
        bossChannel.Unregister(this);
        finishStageMoveEC.Unsubscribe(OnFinishStageMove);
        currentPlayerChannel.Unsubscribe(SetPlayer);
    }

    private void SetPlayer(Player player)
    {
        this.player = player;
    }

    protected override void SetAttackPattern()
    {
        bool activeBossPattern = false;
        
        // 일반적인 공격 패턴을 정하기 전에, 조건을 만족하는 보스 패턴이 있는지 확인함.
        foreach (var pattern in bossPatterns)
        {
            // 패턴이 여러개 동시에 실행될 수 있음. 수정함
            if (pattern.IsAvailable() && !pattern.IsActivated())
            {
                activeBossPattern = true;
                currentBP = pattern;
                break;
            }
        }
        
        if (!activeBossPattern)
        {
            base.SetAttackPattern();
        }
    }

    protected override void SetAttackTrigger(bool value)
    {
        if (value == isBehaving)
        {
            return;
        }

        IsBehaving = value;

        if (IsDead)
        {
            return;
        }

        if (value)
        {
            if (currentBP == null)
            {
                Debug.LogError("Attack Pattern is null but SetAttackTrigger(true) is called.");
            }

            IAttackPattern attackPattern = currentBP as IAttackPattern;

            if (attackPattern != null)
            {
                attackPattern.StartAction(OnAttackFinished, weaponManager.GetWeaponsContainAttackPattern(attackPattern));
            }
            else
            {
                currentBP?.StartAction(OnAttackFinished);
            }

        }
        else
        {
            SetAttackPattern();
        }
    }

    protected void SetDashTrigger(bool value)
    {
        if (value == IsBehaving)
        {
            return;
        }

        if (value)
        {
            dashPattern?.StartAction(OnDashFinished);
        }
     

        IsBehaving = value;
    }

    protected void OnDashFinished()
    {
        SetDashTrigger(false);
    }

    protected void SetShakeOffTrigger(bool value)
    {
        if (value == isBehaving)
        {
            return;
        }

        if (value)
        {
            shakeOffPattern?.StartAction(OnShakeOffFinished);
        }

        IsBehaving = value;
    }

    protected void OnShakeOffFinished()
    {
        SetShakeOffTrigger(false);
    }

    public override bool IsTargetFarAway()
    {
        return DistanceToTarget > dashDistance || stateManager.ChaseTime > maxChaseStateTime;
    }

    public override bool IsTargetOnMyHead()
    {
        Vector3 diff = target.transform.position - transform.position;

        float xzDistance = new Vector3(diff.x, 0f, diff.z).magnitude;

        float yDifferece = target.transform.position.y - transform.position.y;

        bool isTargetCloseXZ = xzDistance < onHeadXzDistanceThreshold;
        bool isTargetHigher = yDifferece > onHeadMinYDifference;
        bool isTargetOnMyHead = isTargetCloseXZ && isTargetHigher && yDifferece < onHeadMaxYDifference;

        return isTargetOnMyHead;
    }


    private void SetSleep(bool value)
    {
        if (isSleeping == value)
        {
            return;
        }
        
        if (value)
        {
            animationController.Sleep();
        }
        else
        {
            animationController.WakeUp();
        }
        
        isSleeping = value;
    }

    public override void Sleep()
    {
        SetSleep(true);
    }
    
    public override void WakeUp()
    {
        SetSleep(false);
        drawTextOnFloorEC.Raise("!");
    }

    public override void Attack()
    {
        base.Attack();
        drawTextOnFloorEC.Raise("Heart");
    }

    public override void Dash()
    {
        SetDashTrigger(true);
    }

    public override void ShakeOff()
    {
        SetShakeOffTrigger(true);
    }



    public bool MountWeapon(Weapon weapon)
    {
        bool result = weaponManager.IsMountable(weapon);
        
        if (result)
        {
            weaponManager.Mount(weapon);
        }

        return result;
    }
    
    public override bool IsTargetWithInWakeUpRange()
    {
        return DistanceToTarget < wakeUpDistance;
    }

    public override void RotateSlowly()
    {
        float playerSpeed = player.MoveSpeed;

        Quaternion lookRotation = Quaternion.LookRotation(DirectionToTarget);

        Vector3 newDirection = Vector3.RotateTowards(transform.forward, DirectionToTarget, Time.deltaTime * playerSpeed * rotateSlowlySpeedFactor, 0.0f);

        Quaternion rotation = Quaternion.LookRotation(newDirection);

        // Smoothly interpolate the enemy's rotation only around the Y-axis
        transform.rotation = rotation;
    }

    public void SetColor(Color color)
    {
        rollerRenderer.material.color = color;
    }

  

    protected override void OnDead()
    {
        deathManager.OnBossDead(this);
        managerChannel.Unsubscribe(this, target);
        bossTransformChannel.Unregister(transform);
        
        //Drop
        // This because of the boss is child of the bossStage.
        transform.parent = null;
        currentBP?.StopAction();
        dashPattern.StopAction();

        DropAllWeapons();
        animationController.Die();
        rb.isKinematic = true;
        rb.linearVelocity = Vector3.zero;
        stateManager.NoState();
        UseGravity = true;
        
        for (int i = 0; i < bossPatterns.Length; i++)
        {
            bossPatterns[i].StopAction();
        }

    }


    private void OnDestroy()
    {
        if (!damagable.IsDead)
        {
            // in this case, the boss is destroyed with the stage.
            // so, OnDead() wasnt called.
            
            if (target == null)
            {
                playerFallEventChannel.Unsubscribe(OnPlayerFall);

                // if the scene reloads, new weapon will be created.
                // so there is no need to return weapon to WeaponSet.
                // target dead, game over.
                return;
            }
            else
            {
                managerChannel.Unsubscribe(this, target);
            }

            

            // need to return all weapons to WeaponSet
            List<Weapon> unmountedWeapons = weaponManager.UnmountAll();

            for (int i = 0; i < unmountedWeapons.Count; i++)
            {
                returnWeaponChannel.Raise(unmountedWeapons[i]);
            }
        }

        playerFallEventChannel.Unsubscribe(OnPlayerFall);
    }

    public override void ForceKill(bool spawnExp = false, bool ignore = false)
    {
        // ignore
    }

    private void OnPlayerFall()
    {
        if (!damagable.IsDead)
        {
            bossTransformChannel.Unregister(transform);
        }

        playerFallEventChannel.Unsubscribe(OnPlayerFall);
        currentBP.StopAction();

        Destroy(gameObject);
    }

    private void OnPlayerDead()
    {
        damagable.Invincible();
    }

    private void OnPlayerRevive()
    {
        if (!damagable.IsDead)
        {
            damagable.HealRate(healRatioOnPlayerRevive);
            damagable.OffInvincible();
        }
    }

    private void OnFinishStageMove()
    {
        transform.parent = null;
    }
}
