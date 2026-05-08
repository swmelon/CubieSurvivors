public class EnemyDashState : EnemyState
{
    private EnemyState nextState;
    public EnemyDashState(EnemyStateManager stateManager) : base(stateManager)
    {
    }

    public override void EnterState(Enemy enemy)
    {
        // do jump or dash
        enemy.Dash();
        nextState = stateManager.ChaseState;
    }

    public override void ExitState(Enemy enemy)
    {

    }

    public override void UpdateState(Enemy enemy)
    {
        if (!enemy.IsBehaving)
        {
            stateManager.SwitchState(nextState);
        }
    }

    public override void OnKnockBackTransitionCalled()
    {
        // ignore
    }

    public override void OnSleepTransitionCalled()
    {
        if (stateManager.UseSleepState)
        {
            nextState = stateManager.SleepState;
        }
    }

    public override void OnDanceTransitionCalled()
    {
        nextState = stateManager.DanceState;
    }
}
