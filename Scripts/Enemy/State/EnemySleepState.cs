using UnityEngine;


public class EnemySleepState : EnemyState
{
    private const float defaultTransitionDelay = 1f;
    private const float wakeUpTransitionDelay = 2f;

    private bool isTransitionCalled = false;
    private float transitionDelay = defaultTransitionDelay;
    private const float minimumSleepTime = 3f;
    private float timeSlept = 0f;
    
    public EnemySleepState(EnemyStateManager stateManager) : base(stateManager)
    {
    }
    
    public override void EnterState(Enemy enemy)
    {
        enemy.Sleep();
        transitionDelay = defaultTransitionDelay;
        timeSlept = 0f;
    }

    public override void ExitState(Enemy enemy)
    {
        enemy.WakeUp();
    }

    public override void UpdateState(Enemy enemy)
    {
        timeSlept += Time.fixedDeltaTime;

        if (timeSlept < minimumSleepTime)
        {
            return;
        }

        if (enemy.IsTargetWithInWakeUpRange())
        {
            stateManager.SwitchState(stateManager.ChaseState, wakeUpTransitionDelay);
        }
    }
    
    public override void OnSleepTransitionCalled()
    {
    }
    
    public override void OnKnockBackTransitionCalled()
    {
        stateManager.SwitchState(stateManager.ChaseState, wakeUpTransitionDelay);
    }

    public override void OnDanceTransitionCalled()
    {
        stateManager.SwitchState(stateManager.DanceState, wakeUpTransitionDelay);
    }
}
