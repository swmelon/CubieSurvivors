using System;
using System.Collections;
using UnityEngine;


public class EnemyStateManager : MonoBehaviour
{
    public bool UseAttackState => useAttackState;
    public bool UseKnockBackState => useKnockBackState;
    public bool UseSleepState => useSleepState;
    public bool UseSleepStateOnlyWhenSpawned => useSleepStateOnlyWhenSpawned;
    
    public EnemyState CurrentState => currentState;
    
    public EnemyState ChaseState => chaseState;
    public EnemyState AttackState => attackState;
    public EnemyState KnockBackState => knockBackState;
    public EnemyState SleepState => sleepState;

    public EnemyState DashState => dashState;

    public EnemyState ShakeOffState => shakeOffState;

    public EnemyState DanceState => danceState;

    [SerializeField] 
    private bool useNoState;
    
    [SerializeField] 
    private bool useAttackState;
    
    [SerializeField]
    private bool useKnockBackState;
    
    [SerializeField]
    private bool useSleepState;

    [SerializeField]
    private bool useDashState;

    [SerializeField] 
    private bool useSleepStateOnlyWhenSpawned;
    
    private EnemyState startState, currentState;
    private EnemyState noState, chaseState, attackState, knockBackState, sleepState, dashState, danceState,
        shakeOffState;
    private Enemy enemy;
    private bool initialized = false;
    
    // Delayed transition is used to prevent state transition from being called multiple times.
    private bool isDelayedTransitionCalled = false;
    private int dashCount = 0;
    private float chaseTime = 0f;

    public int DashStateCount { get => dashCount; set => dashCount = value; }
    public float ChaseTime { get => chaseTime; set => chaseTime = value; }
    private void Initialize()
    {
        initialized = true;
        
        chaseState = new EnemyChaseState(this);
        danceState = new EnemyDanceState(this);
        noState = new EnemyNoState(this);


        if (useAttackState)
        {
            attackState = new EnemyAttackState(this);
        }
        
        if (useKnockBackState)
        {
            knockBackState = new EnemyKnockBackState(this);
        }
        
        if (useSleepState)
        {
            sleepState = new EnemySleepState(this);
        }

        if (useDashState)
        {
            dashState = new EnemyDashState(this);
            shakeOffState = new EnemyShakeOffState(this);
        }

        if (useSleepStateOnlyWhenSpawned)
        {
            sleepState ??= new EnemySleepState(this);
            useSleepState = false;
        }
    }

    public void SetStartState()
    {
        isDelayedTransitionCalled = false;

        if (useNoState)
        {
            startState = noState;
            return;
        }

        if (useSleepStateOnlyWhenSpawned)
        {
            startState = sleepState;
            useSleepState = false;
        }
        else
        {
            startState = chaseState;
        }
    }

    // Called when pooled enemy is spawned, or enemy unintentionally disabled and enabled. 
    private void OnEnable()
    {
        if (!initialized)
        {
            return;
        }
        
        startState.EnterState(enemy);
        currentState = startState;
        isDelayedTransitionCalled = false;
    }

    private void OnDisable()
    {
        if (!initialized)
        {
            return;
        }
        
        currentState.ExitState(enemy);
    }

    public void SetEnemyAndEnterState(Enemy enemy)
    {
        if (!initialized)
        {
            Initialize();
        }

        SetStartState();
        
        this.enemy = enemy;
        currentState = startState;    
        currentState.EnterState(enemy);
    }
    
    public void UpdateState()
    {
        currentState.UpdateState(enemy);
        enemy.State = currentState.ToString();
    }

    public void SwitchState(EnemyState state, float delay = 0f)
    {
        if (isDelayedTransitionCalled)
        {
            return;
        }
        
        currentState.ExitState(enemy);
        
        if (delay > 0f)
        {
            isDelayedTransitionCalled = true;
            StartCoroutine(SwitchStateWithDelay(state, delay));
        }
        else
        {
            currentState = state;
            currentState.EnterState(enemy);
        }
    }

    private void SwitchStateForced(EnemyState state)
    {
        currentState.ExitState(enemy);
        currentState = state;
        currentState.EnterState(enemy);
        StopAllCoroutines();
    }
    
    private IEnumerator SwitchStateWithDelay(EnemyState state, float delay)
    {
        yield return new WaitForSeconds(delay);
        currentState = state;
        currentState.EnterState(enemy);
        isDelayedTransitionCalled = false;
    }
    
    public void SetStateKnockBack()
    {
        currentState.OnKnockBackTransitionCalled();
    }
    
    public void SetStateSleep()
    {
        currentState.OnSleepTransitionCalled();
    }

    public void SetStateDance()
    {
        currentState.OnDanceTransitionCalled();
    }

    public void NoState()
    {
        SwitchStateForced(noState);  
    }
}
