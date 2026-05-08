public class EnemyDanceState : EnemyState
{
    private const float sleepTransitionDelay = 1f;

    public EnemyDanceState(EnemyStateManager stateManager) : base(stateManager)
    {
    }
    
    public override void EnterState(Enemy enemy)
    {
        enemy.AnimationController.Dance();
        enemy.IsKinematic = true;
    }
    
    public override void ExitState(Enemy enemy)
    {
       
    }

    public override void UpdateState(Enemy enemy)
    { 
        if (enemy.HasTarget())
        {
            stateManager.SwitchState(stateManager.ChaseState);
        }
    }

    public override void OnKnockBackTransitionCalled()
    {
    }

    public override void OnSleepTransitionCalled()
    {
        if (stateManager.UseSleepState)
        {
            stateManager.SwitchState(stateManager.SleepState, sleepTransitionDelay);
        }
    }

    public override void OnDanceTransitionCalled()
    {
         
    }
}
