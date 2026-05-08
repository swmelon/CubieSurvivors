using UnityEngine;


[RequireComponent(typeof(Damagable))]
[RequireComponent(typeof(EnemyStateManager))]
public class Enemy : Poolable<Enemy>, ITargetHaves
{
    public EnemyData Data
    {
        get => enemyData;
        set => SetEnemyData(value);
    }

    public Rigidbody Rigidbody => rb;

    public int MaxHealth
    {
        get => damagable.MaxHealth;
        set => damagable.MaxHealth = value;
    }

    public int Health
    {
        get => damagable.Health;
        set => damagable.Health = value;
    }

    public float HealthRatio => Health / (float)MaxHealth;
    
    public float MoveSpeed
    {
        set
        {
            moveSpeed = value;
            animationController.SetWalkSpeed(moveSpeed / transform.localScale.x);
        }    
        get => moveSpeed;
    }

    public virtual bool UseGravity
    {
        set
        {
            if (value)
            {
                yHeightDifference = 0;
                useGravity = true;
                return;
            }

            useGravity = false;

            float yHeight = transform.position.y;
            
            const float levitationTargetHeight = 0.5f;
            yHeightDifference = levitationTargetHeight - yHeight;

            if (yHeightDifference < 0)
            {
                yHeightDifference = 0;
            }
        }

        get => useGravity;
    }
    
    public float Weight
    {
        set
        {
            weight = value;
            knockBackForce = knockBackForceConst / weight;
        }
        
        get => weight;
    }
    
    public bool IsKinematic
    {
        set => rb.isKinematic = value;
    }
    
    public bool IsBehaving { get => isBehaving; set => isBehaving = value; }
    public bool IsSleeping => isSleeping;
    
    public bool KeepRotatingWhileAttacking
    {
        get => keepRotatingWhileAttacking;
        set => keepRotatingWhileAttacking = value;
    }
    
    public bool KeepChasingWhileAttacking
    {
        get => keepChasingWhileAttacking;
    }

    public bool IsDead => damagable.IsDead;

    public float DistanceToTarget => distanceToTarget;
    public Vector3 DirectionToTarget => directionToTarget;
    public EnemyAnimationController AnimationController => animationController;
    public bool SpawnExpOnDead
    {
        get => spawnExpOnDead;
        set => spawnExpOnDead = value;
    }

    public string State;
    
    
    [Header("Enemy Management")]
    [SerializeField] 
    protected EnemyManagerChannelSO managerChannel;
    
    [SerializeField]
    protected DeathManager deathManager;
    
    protected Transform target;

    protected EnemyAnimationController animationController;
    protected EnemyStateManager stateManager;
    protected Damagable damagable;
    protected Rigidbody rb;
    protected Transform enemyBody;

    protected bool isBehaving = false;
    protected bool isSleeping = false;
    private bool keepRotatingWhileAttacking = false;
    private bool keepChasingWhileAttacking = false;
    private float keepChasingMinDistance;

    private float moveSpeed = 3f;
    private float weight = 10f;

    private const float knockBackForceConst = 150f;
    private const float gravityScale = 3f;
    private const float levitationCorrectionScale = 10f;

    private int walkSpeedHash;
    private float knockBackForce;

    private Vector3 directionToTarget = Vector3.zero;
    private Vector3 knockBackDirection = Vector3.zero;
    private float distanceToTarget = float.MaxValue;

    private bool spawnExpOnDead = true;
    private bool useGravity = true;

    [SerializeField]
    private EnemyData enemyData;

    private Collider collider;
    private float yHeightDifference;


    private MaterialPropertyBlock propertyBlock;
    public Transform GetTransform() => transform;
    
    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody>();
        collider = GetComponentInChildren<Collider>();
        damagable = GetComponent<Damagable>(); 
        stateManager = GetComponent<EnemyStateManager>();
        animationController = GetComponent<EnemyAnimationController>();
        AddOnSpawnedEvent(OnSpawned);
        propertyBlock = new MaterialPropertyBlock();

    }

    // OnDead() must be add last in the list of OnDead. So use Start() instead of OnEnable()
    // Other Listeners must be added in OnEnable(). Because other listeners must be added before OnDead().
    protected virtual void Start()
    {
        damagable.OnHit.AddListener((direction) => OnHit(direction));
        damagable.OnDead.AddListener(OnDead);
        
        stateManager.SetEnemyAndEnterState(this);
    }
    
    protected virtual void OnSpawned()
    {
        if (!TrySubscribeManager())
        {
            return;
        }

        SpawnExpOnDead = true;
        stateManager.SetEnemyAndEnterState(this);
        damagable.Revive();
        collider.enabled = true;
        UpdateTarget();
    }

    private void FixedUpdate()
    {
        stateManager.UpdateState();
    }

    private void SetEnemyData(EnemyData data)
    {
        enemyData = data;
        MaxHealth = enemyData.MaxHealth;
        Health = MaxHealth;
        MoveSpeed = enemyData.MoveSpeed;
        Weight = enemyData.Weight;
        transform.localScale = Vector3.one * enemyData.Scale;

    }
    
    public void SetKeepChasingWhileAttacking(bool value, float minDistance = 3f)
    {
        rb.isKinematic = !value;
        keepChasingWhileAttacking = value;
        animationController.Walk(value);
        keepChasingMinDistance = minDistance;
    }
    
    protected bool TrySubscribeManager()
    {
        if (managerChannel.TrySubscribe(this, out target))
        {
            SetTargetAndUpdate(target);
            return true;
        }
        else
        {
            Release();
            return false;
        }
    }

    public void UpdateTarget()
    {
        // check if target is valid. normally, it should be valid.
        if (target == null || !target.gameObject.activeSelf)
        {
            return;
        }
        
        directionToTarget = target.position - transform.position;
        distanceToTarget = directionToTarget.magnitude;
        directionToTarget.y = 0;
        directionToTarget = directionToTarget.normalized;
    }
    
    public void SetTargetAndUpdate(Transform target)
    {
        this.target = target;
        UpdateTarget();
    }

    public void SetTemporoalTarget(Transform target)
    {
    }

    private void SwitchBackToTargetOrigin()
    {

    }

    public bool HasTarget()
    {
        return !ReferenceEquals(target, null);
    }
    public bool TryGetTarget(out Transform target)
    {
        target = this.target;
        return HasTarget();
    }
    
    public bool TryGetTarget(out Transform target, out float distance)
    {
        target = this.target;
        bool result = HasTarget();
        
        if (result)
        {
            distance = Vector3.Distance(transform.position, target.position);
        }
        else
        {
            distance = float.MaxValue;
        }

        return result;
    }

    public int GetTargets(int maxTargets, int maxRange, out Transform[] targets)
    {
        targets = new Transform[1];
        
        if (ReferenceEquals(target, null) || distanceToTarget > maxRange)
        {
            return 0;
        }
        
        targets[0] = target;
        return 1;
    }

    public void AdjustYScale(float yFactor)
    {
        Vector3 newScale = transform.localScale;
        newScale.y *= yFactor;
        transform.localScale = newScale;
    }
    
    
    #region Unimplemented State Functions
    public virtual void Attack() {}
    public virtual void Sleep() {}
    public virtual void WakeUp() {}

    public virtual void Dash() {}

    public virtual void ShakeOff() {}
    public virtual bool IsTargetWithInAttackRange() { return false; }

    public virtual bool IsTargetOnTheTop() { return false; }
    public virtual bool IsTargetFarAway() { return false; }

    public virtual bool IsTargetOnMyHead() { return false; }
    public virtual bool IsTargetWithInWakeUpRange() { return false; }

    #endregion
    
    #region Imlpemented State Functions

    public void ChaseTarget(bool rotateSmoothly = false)
    {
        if (rotateSmoothly)
        {
            RotateSmoothly();
        }
        else
        {
            Rotate(directionToTarget);
        }
        
        Move(directionToTarget);
    }
    
    public void ChaseTargetWhileAttacking()
    {
        Rotate(directionToTarget);
        
        if (distanceToTarget > keepChasingMinDistance)
        {
            animationController.Walk(true);
            Move(directionToTarget);
        }
        else
        {
            animationController.Walk(false);
        }
    }
    
    private void Move(Vector3 direction)
    {
        if (rb == null)
        {
            Debug.LogError("Rigidbody is null.");
            return;
        }
        
        rb.linearVelocity = moveSpeed * direction;
        
        if (useGravity)
        {
            rb.linearVelocity += gravityScale * Time.fixedDeltaTime * Physics.gravity;
        }
        else
        {
            rb.linearVelocity += levitationCorrectionScale * yHeightDifference * Time.fixedDeltaTime * Vector3.up;
        }
    }

    public void Rotate(Vector3 direction)
    {
        float angle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, angle, 0);
    }
    
    // 공격이 끝나고 나서 타겟을 바라보게 함
    public void RotateSmoothly()
    {
        Quaternion lookRotation = Quaternion.LookRotation(directionToTarget);

        // Smoothly interpolate the enemy's rotation only around the Y-axis
        const float smoothRotationSpeed = 2f;
        transform.rotation = Quaternion.Lerp(transform.rotation, lookRotation, Time.fixedDeltaTime * smoothRotationSpeed);
    }

    public void RotateTowardTarget()
    {
        Rotate(directionToTarget);
    }
    
    public virtual void RotateSlowly() {}
    
    public void KnockBack()
    {
        rb.AddForce(knockBackForce * knockBackDirection, ForceMode.Impulse);
        knockBackDirection = Vector3.zero;
    }
        // This method will be call by AttackPattern.

   
    #endregion
    
    #region Event Functions
    protected virtual void OnDead()
    {
        deathManager.OnEnemyDead(this, spawnExpOnDead);
        managerChannel.Unsubscribe(this, target);
        Release();
    }
    
    private void OnHit(Vector3 direction)
    {
        SetKnockBackDirection(direction);
        stateManager.SetStateKnockBack();
    }

    private void SetKnockBackDirection(Vector3 direction)
    {
        knockBackDirection = direction;
    }

    
    #endregion
    
    public virtual void ForceKill(bool spawnExp= false, bool ignore=true)
    {   
        managerChannel.Unsubscribe(this, target);
        deathManager.OnEnemyDead(this, spawnExp: spawnExp);
        damagable.ForeceSetDead();
        Release();
    }

    public override void Release()
    {
        collider.enabled = false;
        base.Release();
    }

    public void Dance()
    {
        stateManager.SetStateDance();
    }

    public void ManuallyAdjustHeightWhenFalling()
    {
        transform.position += yHeightDifference * Time.fixedDeltaTime* Vector3.up;
    }
}
